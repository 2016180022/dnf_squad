using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DnfSquad.Data;
using DnfSquad.Play.Core;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 성광 유지율(레이드 제한시간) 감소를 전담하는 레이드 코어 컨트롤러.
    /// HealthController와 동일한 원칙 — 값 변경은 전부 이 컨트롤러를 거치고,
    /// 외부(스쿼드 스킬 등)는 DecayMultiplier만 조정해서 감소량에 개입한다.
    /// </summary>
    public class LuminousGaugeController : MonoBehaviour
    {
        private const float BaseDecayInterval = 6f;      // 규칙 2: 6초에 1씩 감소
        private const int BaseDecayAmount = 1;

        // 규칙 3: 보스(우리엘/라파엘) 비점유 — 각자 독립적으로 60초마다 10 감소
        private const float BossUnoccupiedDecayInterval = 60f;
        private const int BossUnoccupiedDecayAmount = 10;

        private const float FixedPositionDecayInterval = 30f; // 규칙 4: 미카엘라 위치 고정 30초마다 30 감소
        private const int FixedPositionDecayAmount = 30;

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private MapTransitionController mapTransitionController;
        [Tooltip("보스 비점유/미카엘라 위치 고정으로 성광 유지율이 감소될 때 안내 문구를 띄우는 데 사용")]
        [SerializeField] private GlobalWarningUI globalWarningUI;

        /// <summary>감소량에 곱해지는 배율. 감소량을 낮추는 스쿼드 스킬 등이 외부에서 조정 (기본 1 = 원래 그대로).</summary>
        public float DecayMultiplier { get; set; } = 1f;

        public int MaxLuminousGauge => raidRuntimeData.maxLuminousGauge;
        public int CurrentLuminousGauge => raidRuntimeData.runtimeState.luminousGauge;

        // 미카엘라 위치 고정 카운트다운 게이지용 (30에서 시작해 1초에 1씩 줄어드는 형태로 노출)
        public float FixedPositionCountdownMax => FixedPositionDecayInterval;
        public float FixedPositionCountdownRemaining => Mathf.Max(0f, FixedPositionDecayInterval - fixedPositionTimer);

        // 보스(우리엘/라파엘) 비점유 카운트다운 게이지용. monsterId별로 독립된 값.
        public float BossUnoccupiedCountdownMax => BossUnoccupiedDecayInterval;
        public float GetBossUnoccupiedCountdownRemaining(string monsterId) =>
            Mathf.Max(0f, BossUnoccupiedDecayInterval - (bossUnoccupiedTimers.TryGetValue(monsterId, out var t) ? t : 0f));

        private float baseDecayTimer;

        // 규칙 3: 우리엘/라파엘 각각 독립적인 비점유 타이머 (monsterId 기준)
        private readonly Dictionary<string, float> bossUnoccupiedTimers = new Dictionary<string, float>();

        private float fixedPositionTimer;
        private string lastMichaelaNodeId;
        private bool hasLastMichaelaNodeId;

        private void Update()
        {
            float dt = Time.deltaTime;

            TickBaseDecay(dt);
            TickBossUnoccupied(dt);
            TickMichaelaPositionFixed(dt, GetMichaela());
        }

        private void TickBaseDecay(float dt)
        {
            baseDecayTimer += dt;
            while (baseDecayTimer >= BaseDecayInterval)
            {
                baseDecayTimer -= BaseDecayInterval;
                DecreaseGauge(BaseDecayAmount);
            }
        }

        /// <summary>규칙 3: 우리엘/라파엘 각각 — 플레이어가 그 몬스터 노드에 없는 상태(비점유)가 유지되는 동안 60초마다 10 감소</summary>
        private void TickBossUnoccupied(float dt)
        {
            var bosses = raidRuntimeData.monsters.Where(m => m.tier == MonsterTier.Boss).ToList();

            // 더 이상 존재하지 않는(죽었거나 스크립트에서 빠진) 몬스터의 타이머는 정리
            var activeIds = new HashSet<string>(bosses.Select(b => b.monsterId));
            foreach (var staleId in bossUnoccupiedTimers.Keys.Where(id => !activeIds.Contains(id)).ToList())
                bossUnoccupiedTimers.Remove(staleId);

            foreach (var boss in bosses)
            {
                string bossNodeId = raidRuntimeData.GetMonsterState(boss.monsterId)?.currentNodeId;
                bool occupied = bossNodeId != null && bossNodeId == mapTransitionController.CurrentNodeId;

                if (occupied)
                {
                    bossUnoccupiedTimers[boss.monsterId] = 0f;
                    continue;
                }

                float timer = bossUnoccupiedTimers.TryGetValue(boss.monsterId, out var t) ? t : 0f;
                timer += dt;
                while (timer >= BossUnoccupiedDecayInterval)
                {
                    timer -= BossUnoccupiedDecayInterval;
                    DecreaseGauge(BossUnoccupiedDecayAmount);
                    // 2026-08-22: 보스 비점유로 성광 유지율이 감소하는 순간 안내 문구 표시
                    globalWarningUI.ShowWarning("보스 몬스터 60초 비점유 시,\\n 성광 유지율이 감소됩니다");
                }
                bossUnoccupiedTimers[boss.monsterId] = timer;
            }
        }

        /// <summary>규칙 4: 미카엘라 위치가 바뀌지 않는 상태가 유지되는 동안 30초마다 30 감소</summary>
        private void TickMichaelaPositionFixed(float dt, MonsterData michaela)
        {
            if (michaela == null) { fixedPositionTimer = 0f; hasLastMichaelaNodeId = false; return; }

            string michaelaNodeId = raidRuntimeData.GetMonsterState(michaela.monsterId)?.currentNodeId;

            if (!hasLastMichaelaNodeId || michaelaNodeId != lastMichaelaNodeId)
            {
                lastMichaelaNodeId = michaelaNodeId;
                hasLastMichaelaNodeId = true;
                fixedPositionTimer = 0f;
                return;
            }

            fixedPositionTimer += dt;
            while (fixedPositionTimer >= FixedPositionDecayInterval)
            {
                fixedPositionTimer -= FixedPositionDecayInterval;
                DecreaseGauge(FixedPositionDecayAmount);
                // 2026-08-22: 미카엘라 위치 고정으로 성광 유지율이 감소하는 순간 안내 문구 표시
                globalWarningUI.ShowWarning("미카엘라의 위치가 30초간 고정되어 있을 경우,\\n 성광 유지율이 감소됩니다");
            }
        }

        private MonsterData GetMichaela() =>
            raidRuntimeData.monsters.FirstOrDefault(m => m.tier == MonsterTier.Michaela);

        /// <summary>DecayMultiplier를 적용해 성광 유지율을 감소시킨다 (0 미만으로 내려가지 않음).</summary>
        private void DecreaseGauge(int amount)
        {
            var state = raidRuntimeData.runtimeState;
            int reduced = Mathf.RoundToInt(amount * DecayMultiplier);
            state.luminousGauge = Mathf.Max(0, state.luminousGauge - reduced);
        }
    }
}
