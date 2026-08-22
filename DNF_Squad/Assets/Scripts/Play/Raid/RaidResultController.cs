using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using DnfSquad.Data;
using DnfSquad.Play.Squad; // 신규(25차) — 보상 슬롯(SquadStatusSlotUI) 재사용을 위해 추가

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 미카엘라 사망을 감지해 레이드 승리를 처리한다 (레이드 기능).
    /// 승리 판정 시 지정된 시스템들을 전부 정지시키고 보상 캔버스를 활성화한다.
    /// 보상 캔버스 내부(승리 팝업 → 보상 받기 버튼 → 보상 화면 전환)는 리소스/텍스트가 고정이라
    /// 인스펙터의 SetActive 바인딩만으로 처리하고, 별도 코드는 필요해질 때 추가한다(사용자 확정).
    /// </summary>
    public class RaidResultController : MonoBehaviour
    {
        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [Tooltip("승리 판정 시 정지(비활성화)할 컴포넌트들 — 조우 데미지 틱, 성광 유지율 감소, 스쿼드 자동 딜링/스킬, 성역 강제 퇴장, 현황판 갱신 등 원하는 것을 자유롭게 추가")]
        [SerializeField] private List<Behaviour> systemsToStop = new List<Behaviour>();
        [Tooltip("승리 판정 시 활성화할 보상 캔버스(승리 팝업 포함, 기본 비활성 상태로 시작)")]
        [SerializeField] private GameObject rewardCanvas;
        [Tooltip("보상 화면의 '세팅 씬으로 돌아가기' 버튼이 이동할 씬 이름")]
        [SerializeField] private string settingSceneName = "SettingScene";

        // ===== 신규(25차) — 보상 캔버스 슬롯에 세팅 씬에서 편성한 스쿼드원 정보(초상화+이름) 표시 =====
        [Header("보상 슬롯 — 세팅 씬에서 편성한 스쿼드원 정보 표시")]
        [SerializeField] private SquadRuntimeData squadRuntimeData;
        [Tooltip("RewardPanel/LeaderReward에 붙은 SquadStatusSlotUI")]
        [SerializeField] private SquadStatusSlotUI leaderRewardSlot;
        [Tooltip("RewardPanel/BufferReward에 붙은 SquadStatusSlotUI")]
        [SerializeField] private SquadStatusSlotUI bufferRewardSlot;
        [Tooltip("RewardPanel/MemberReward(1번째)에 붙은 SquadStatusSlotUI")]
        [SerializeField] private SquadStatusSlotUI member0RewardSlot;
        [Tooltip("RewardPanel/MemberReward(2번째)에 붙은 SquadStatusSlotUI")]
        [SerializeField] private SquadStatusSlotUI member1RewardSlot;

        private bool raidCleared;

        private void Update()
        {
            if (raidCleared) return;

            var michaela = raidRuntimeData.monsters.FirstOrDefault(m => m.tier == MonsterTier.Michaela);
            if (michaela == null) return;

            var state = raidRuntimeData.GetMonsterState(michaela.monsterId);
            if (state != null && state.isDead)
            {
                HandleRaidClear();
            }
        }

        private void HandleRaidClear()
        {
            raidCleared = true;

            foreach (var system in systemsToStop)
            {
                if (system != null) system.enabled = false;
            }

            if (rewardCanvas != null) rewardCanvas.SetActive(true);

            DisplayRewardSquad(); // 신규(25차)
        }

        /// <summary>
        /// 신규(25차) — 세팅 씬에서 편성한 4개 역할(리더/버퍼/멤버1/멤버2)의 캐릭터 정보를
        /// 보상 패널의 각 슬롯(SquadStatusSlotUI)에 그대로 표시한다.
        /// 슬롯은 SquadStatusSlotUI.Display(null)일 때 자동으로 자기 자신을 비활성화하므로
        /// 편성이 비어있는 역할은 별도 처리 없이 자연스럽게 숨겨진다.
        /// </summary>
        private void DisplayRewardSquad()
        {
            if (squadRuntimeData == null) return;

            var composition = squadRuntimeData.runtimeState.composition;

            leaderRewardSlot?.Display(squadRuntimeData.GetCharacter(composition.leaderCharacterId));
            bufferRewardSlot?.Display(squadRuntimeData.GetCharacter(composition.bufferCharacterId));
            member0RewardSlot?.Display(squadRuntimeData.GetCharacter(composition.memberCharacterIds[0]));
            member1RewardSlot?.Display(squadRuntimeData.GetCharacter(composition.memberCharacterIds[1]));
        }

        /// <summary>보상 화면의 "세팅 씬으로 돌아가기" 버튼 OnClick에 연결.</summary>
        public void ReturnToSettingScene()
        {
            SceneManager.LoadScene(settingSceneName);
        }
    }
}
