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

        private const float UnoccupiedDecayInterval = 30f; // 규칙 3: 보스 비점유 30초마다 20 감소
        private const int UnoccupiedDecayAmount = 20;

        private const float FixedPositionDecayInterval = 30f; // 규칙 4: 위치 고정 30초마다 30 감소
        private const int FixedPositionDecayAmount = 30;

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private MapTransitionController mapTransitionController;

        /// <summary>감소량에 곱해지는 배율. 감소량을 낮추는 스쿼드 스킬 등이 외부에서 조정 (기본 1 = 원래 그대로).</summary>
        public float DecayMultiplier { get; set; } = 1f;

        public int MaxLuminousGauge => raidRuntimeData.maxLuminousGauge;
        public int CurrentLuminousGauge => raidRuntimeData.runtimeState.luminousGauge;

        private float baseDecayTimer;
        private float unoccupiedTimer;
        private float fixedPositionTimer;
        private string lastBossNodeId;
        private bool hasLastBossNodeId;

        private void Update()
        {
            float dt = Time.deltaTime;
            MonsterData boss = GetBoss();

            TickBaseDecay(dt);
            TickBossUnoccupied(dt, boss);
            TickBossPositionFixed(dt, boss);
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

        /// <summary>규칙 3: 플레이어가 보스 노드에 없는 상태(비점유)가 유지되는 동안 30초마다 20 감소</summary>
        private void TickBossUnoccupied(float dt, MonsterData boss)
        {
            if (boss == null) { unoccupiedTimer = 0f; return; }

            string bossNodeId = raidRuntimeData.GetMonsterState(boss.monsterId)?.currentNodeId;
            bool occupied = bossNodeId != null && bossNodeId == mapTransitionController.CurrentNodeId;

            if (occupied)
            {
                unoccupiedTimer = 0f;
                return;
            }

            unoccupiedTimer += dt;
            while (unoccupiedTimer >= UnoccupiedDecayInterval)
            {
                unoccupiedTimer -= UnoccupiedDecayInterval;
                DecreaseGauge(UnoccupiedDecayAmount);
            }
        }

        /// <summary>규칙 4: 보스(미카엘라) 위치가 바뀌지 않는 상태가 유지되는 동안 30초마다 30 감소</summary>
        private void TickBossPositionFixed(float dt, MonsterData boss)
        {
            if (boss == null) { fixedPositionTimer = 0f; hasLastBossNodeId = false; return; }

            string bossNodeId = raidRuntimeData.GetMonsterState(boss.monsterId)?.currentNodeId;

            if (!hasLastBossNodeId || bossNodeId != lastBossNodeId)
            {
                lastBossNodeId = bossNodeId;
                hasLastBossNodeId = true;
                fixedPositionTimer = 0f;
                return;
            }

            fixedPositionTimer += dt;
            while (fixedPositionTimer >= FixedPositionDecayInterval)
            {
                fixedPositionTimer -= FixedPositionDecayInterval;
                DecreaseGauge(FixedPositionDecayAmount);
            }
        }

        private MonsterData GetBoss() =>
            raidRuntimeData.monsters.FirstOrDefault(m => m.tier == MonsterTier.Boss);

        /// <summary>DecayMultiplier를 적용해 성광 유지율을 감소시킨다 (0 미만으로 내려가지 않음).</summary>
        private void DecreaseGauge(int amount)
        {
            var state = raidRuntimeData.runtimeState;
            int reduced = Mathf.RoundToInt(amount * DecayMultiplier);
            state.luminousGauge = Mathf.Max(0, state.luminousGauge - reduced);
        }
    }
}
