using DnfSquad.Data;
using DnfSquad.Logic;
using DnfSquad.Play.Core;
using UnityEngine;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 스쿼드 스킬 바 — 화면 가운데 UI에 배치된 스킬 버튼 클릭 시,
    /// 리더/버퍼 프리팹을 등장시키고(연출은 SquadSkillActorLifetime이 애니메이션 종료 시 정리) 효과를 즉시 적용한다.
    /// 버튼 배치 순서는 세팅씬에서 정한 quickSlots(슬롯 0~5) 그대로 따라간다.
    /// effectType이 None인 스킬(아직 구현 안 된 4종)은 버튼이 항상 비활성 상태로, 발동 자체가 일어나지 않는다.
    /// </summary>
    public class SquadSkillBarController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadRuntimeData;

        [Header("연동 컨트롤러")]
        [SerializeField] private HealthController healthController;
        [SerializeField] private MonsterSpawnController monsterSpawnController;
        [SerializeField] private Transform playerTransform;

        [Header("스폰 위치 오프셋 (앵커 기준 상대 좌표)")]
        [SerializeField] private Vector2 bossAnchorOffset = new Vector2(-1.2f, 0f);
        [SerializeField] private Vector2 playerAnchorOffset = new Vector2(1f, 0f);

        [Header("UI (퀵슬롯 0~5 순서대로 연결 — 버튼 오브젝트/배치는 직접 진행)")]
        [SerializeField] private SquadSkillButtonUI[] skillButtons = new SquadSkillButtonUI[6];

        private void Start()
        {
            squadRuntimeData.ResetSkillCooldowns(); // 레이드 시작 시 이전 쿨타임 초기화
            InitButtons();
        }

        /// <summary>quickSlots 배치대로 각 버튼에 아이콘/클릭 리스너를 연결</summary>
        private void InitButtons()
        {
            foreach (var assignment in squadRuntimeData.runtimeState.quickSlots)
            {
                if (assignment.slotIndex < 0 || assignment.slotIndex >= skillButtons.Length) continue;

                var button = skillButtons[assignment.slotIndex];
                if (button == null) continue;

                var skill = squadRuntimeData.GetSkill(assignment.skillId);
                if (skill == null) continue;

                Sprite icon = Resources.Load<Sprite>($"Image/Skill/{skill.skillId}");
                string skillId = skill.skillId;
                button.Init(skillId, icon, () => TryUseSkill(skillId));
            }
        }

        private void Update()
        {
            squadRuntimeData.TickCooldowns(Time.deltaTime);
            RefreshButtonStates();
        }

        /// <summary>쿨타임/대상 유무에 따라 버튼 상호작용 가능 여부와 쿨타임 표시를 갱신</summary>
        private void RefreshButtonStates()
        {
            foreach (var assignment in squadRuntimeData.runtimeState.quickSlots)
            {
                if (assignment.slotIndex < 0 || assignment.slotIndex >= skillButtons.Length) continue;

                var button = skillButtons[assignment.slotIndex];
                if (button == null) continue;

                var skill = squadRuntimeData.GetSkill(assignment.skillId);
                if (skill == null) continue;

                float remaining = squadRuntimeData.GetRemainingCooldown(skill.skillId);
                float finalCooldown = SquadSkillStatService.GetFinalCooldown(squadRuntimeData, skill.skillId);
                button.SetCooldown(remaining, finalCooldown);

                bool hasEffect = skill.effectType != SquadSkillEffectType.None;
                bool offCooldown = remaining <= 0f;
                // 공격 스킬은 현재 화면에 보스가 있을 때만 사용 가능 (지원 스킬은 조건 없음 — 확장 기능에서 다룰 예정)
                bool hasTarget = skill.skillType != SquadSkillType.Attack || monsterSpawnController.CurrentMonsterId != null;

                button.SetInteractable(hasEffect && offCooldown && hasTarget);
            }
        }

        // ===== 스킬 사용 =====

        private void TryUseSkill(string skillId)
        {
            var skill = squadRuntimeData.GetSkill(skillId);
            if (skill == null || skill.effectType == SquadSkillEffectType.None) return;

            if (squadRuntimeData.GetRemainingCooldown(skillId) > 0f) return;

            // 버튼이 정상적으로 비활성화돼 있었다면 호출될 일이 없지만, 실행 시점에도 한 번 더 방어적으로 체크한다
            // (계율의 사슬 등 기존 스쿼드 로직과 동일한 패턴).
            if (skill.skillType == SquadSkillType.Attack && monsterSpawnController.CurrentMonsterId == null) return;

            ApplyEffect(skill);
            SpawnActor(skill);

            float finalCooldown = SquadSkillStatService.GetFinalCooldown(squadRuntimeData, skillId);
            if (finalCooldown > 0f) squadRuntimeData.StartCooldown(skillId, finalCooldown);
        }

        /// <summary>스킬의 실제 효과 적용 (연출과 무관하게 즉시 적용). effectType 추가될 때마다 case만 늘리면 됨.</summary>
        private void ApplyEffect(SquadSkillData skill)
        {
            switch (skill.effectType)
            {
                case SquadSkillEffectType.LeaderChainDamage:
                    ApplyLeaderChainDamage();
                    break;
                case SquadSkillEffectType.HealPlayerPercent:
                    ApplyHealPlayerPercent(skill);
                    break;
            }
        }

        /// <summary>26001 전투 조력 — 세팅씬에서 측정해둔 리더 스킬 체인 총합 데미지를 보스에게 적용</summary>
        private void ApplyLeaderChainDamage()
        {
            string monsterId = monsterSpawnController.CurrentMonsterId;
            if (string.IsNullOrEmpty(monsterId)) return;

            long damage = squadRuntimeData.runtimeState.leaderSkillChainTotalDamage;
            if (damage <= 0) return;

            healthController.DamageMonster(monsterId, (int)System.Math.Min(damage, int.MaxValue));
        }

        /// <summary>26004 버퍼 회복 — 버프 반영된 최종 비율(%)만큼 플레이어 체력/마나 회복</summary>
        private void ApplyHealPlayerPercent(SquadSkillData skill)
        {
            var finalValues = SquadSkillStatService.GetFinalValues(squadRuntimeData, skill.skillId);
            float percent = finalValues.Length > 0 ? finalValues[0] : 0f;

            int healHp = Mathf.RoundToInt(healthController.PlayerMaxHp * percent / 100f);
            int healMp = Mathf.RoundToInt(healthController.PlayerMaxMp * percent / 100f);

            healthController.HealPlayer(healHp);
            healthController.ChangePlayerMp(healMp);
        }

        // ===== 연출 스폰 =====

        /// <summary>공격 스킬 → 리더, 지원 스킬 → 버퍼 프리팹을 spawnAnchor 위치에 스폰. 정리는 SquadSkillActorLifetime이 담당.</summary>
        private void SpawnActor(SquadSkillData skill)
        {
            string prefabPath = skill.skillType == SquadSkillType.Attack ? "Prefab/Squad/Leader" : "Prefab/Squad/Buffer";
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[SquadSkillBarController] 프리팹을 찾을 수 없음: Resources/{prefabPath}");
                return;
            }

            Instantiate(prefab, GetAnchorPosition(skill.spawnAnchor), Quaternion.identity);
        }

        private Vector3 GetAnchorPosition(SquadSkillSpawnAnchor anchor)
        {
            if (anchor == SquadSkillSpawnAnchor.NearBoss)
            {
                var bossTransform = monsterSpawnController.CurrentMonsterTransform;
                return bossTransform != null ? bossTransform.position + (Vector3)bossAnchorOffset : transform.position;
            }

            return playerTransform != null ? playerTransform.position + (Vector3)playerAnchorOffset : transform.position;
        }
    }
}
