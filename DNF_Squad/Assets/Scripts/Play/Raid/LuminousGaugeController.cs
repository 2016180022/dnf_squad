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
        private const float BaseDecayInterval = 6f;      // 규칙 2: 6초에 1씩 감소 (그대로 유지, 인스펙터 노출 안 함)
        private const int BaseDecayAmount = 1;

        [Header("규칙 3: 보스(우리엘/라파엘) 비점유 페널티 — 인스펙터에서 조정")]
        [SerializeField] private float bossUnoccupiedDecayInterval = 60f; // 각자 독립적으로 이 초마다
        [SerializeField] private int bossUnoccupiedDecayAmount = 10;      // 이만큼 감소

        [Header("규칙 4: 미카엘라 위치 고정 페널티 — 인스펙터에서 조정")]
        [SerializeField] private float fixedPositionDecayInterval = 30f; // 이 초마다
        [SerializeField] private int fixedPositionDecayAmount = 30;      // 이만큼 감소

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private MapTransitionController mapTransitionController;
        [Tooltip("보스 비점유/미카엘라 위치 고정으로 성광 유지율이 감소될 때 안내 문구를 띄우는 데 사용")]
        [SerializeField] private GlobalWarningUI globalWarningUI;

        /// <summary>감소량에 곱해지는 배율. 감소량을 낮추는 스쿼드 스킬 등이 외부에서 조정 (기본 1 = 원래 그대로).</summary>
        public float DecayMultiplier { get; set; } = 1f;

        public int MaxLuminousGauge => raidRuntimeData.maxLuminousGauge;
        public int CurrentLuminousGauge => raidRuntimeData.runtimeState.luminousGauge;

        // 미카엘라 위치 고정 카운트다운 게이지용 (30에서 시작해 1초에 1씩 줄어드는 형태로 노출)
        public float FixedPositionCountdownMax => fixedPositionDecayInterval;
        public float FixedPositionCountdownRemaining => Mathf.Max(0f, fixedPositionDecayInterval - fixedPositionTimer);

        // 보스(우리엘/라파엘) 비점유 카운트다운 게이지용. monsterId별로 독립된 값.
        public float BossUnoccupiedCountdownMax => bossUnoccupiedDecayInterval;
        public float GetBossUnoccupiedCountdownRemaining(string monsterId) =>
            Mathf.Max(0f, bossUnoccupiedDecayInterval - (bossUnoccupiedTimers.TryGetValue(monsterId, out var t) ? t : 0f));

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

        /// <summary>규칙 3: 우리엘/라파엘 각각 — 플레이어가 그 몬스터 노드에 없는 상태(비점유)가 유지되는 동안
        /// bossUnoccupiedDecayInterval초마다 bossUnoccupiedDecayAmount만큼 감소 (인스펙터에서 조정 가능)</summary>
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
                while (timer >= bossUnoccupiedDecayInterval)
                {
                    timer -= bossUnoccupiedDecayInterval;
                    DecreaseGauge(bossUnoccupiedDecayAmount);
                    // 안내 문구를 하드코딩하지 않고 인스펙터 값(bossUnoccupiedDecayInterval)에 자동 동기화
                    globalWarningUI.ShowWarning($"보스 몬스터 {bossUnoccupiedDecayInterval:0.#}초 비점유 시,\\n 성광 유지율이 감소됩니다");
                }
                bossUnoccupiedTimers[boss.monsterId] = timer;
            }
        }

        /// <summary>규칙 4: 미카엘라 위치가 바뀌지 않는 상태가 유지되는 동안 fixedPositionDecayInterval초마다
        /// fixedPositionDecayAmount만큼 감소 (인스펙터에서 조정 가능)</summary>
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
            while (fixedPositionTimer >= fixedPositionDecayInterval)
            {
                fixedPositionTimer -= fixedPositionDecayInterval;
                DecreaseGauge(fixedPositionDecayAmount);
                // 안내 문구를 하드코딩하지 않고 인스펙터 값(fixedPositionDecayInterval)에 자동 동기화
                globalWarningUI.ShowWarning($"미카엘라의 위치가 {fixedPositionDecayInterval:0.#}초간 고정되어 있을 경우,\\n 성광 유지율이 감소됩니다");
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
