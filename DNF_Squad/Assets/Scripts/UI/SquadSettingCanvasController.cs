using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class SquadSettingCanvasController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadData;

        [Header("스킬 정보 목록 (마스터 데이터 순서대로 연결, 표시 전용)")]
        [SerializeField] private SquadSkillInfoRowUI[] skillInfoRows = new SquadSkillInfoRowUI[6];

        [Header("퀵슬롯 바 (1~6번 위치 순서대로 연결)")]
        [SerializeField] private SquadSkillSlotUI[] quickSlots = new SquadSkillSlotUI[6];
        [SerializeField] private Transform dragLayer;

        [Header("스킬 체인")]
        [SerializeField] private GameObject settingCanvasRoot;       // 이 캔버스의 루트 오브젝트
        [SerializeField] private GameObject skillChainCanvas;        // 체인 설정 UI 루트
        [SerializeField] private GameObject skillChainViewPopup;     // 체인 확인용 작은 팝업
        [SerializeField] private SkillChainListViewUI skillChainViewList; // 팝업 내부 목록 뷰
        [SerializeField] private TMP_Text skillChainTotalDamageText;
        [SerializeField] private Button skillChainViewButton;        // "스킬 체인 확인" — 데이터 있을 때만 활성

        [Header("씬 흐름")]
        [SerializeField] private DnfSquad.Scene.SettingSceneFlowController flowController;

        private void OnEnable()
        {
            EnsureDefaultQuickSlotOrder();

            for (int i = 0; i < quickSlots.Length; i++)
            {
                quickSlots[i].Init(this, i, dragLayer);
            }

            RefreshSkillInfoRows();
            RefreshQuickSlots();
            RefreshSkillChainSummary();
        }

        /// <summary>런타임 상태에 퀵슬롯 배치가 없으면(최초 진입) squadSkills 순서 그대로 채움</summary>
        private void EnsureDefaultQuickSlotOrder()
        {
            if (squadData.runtimeState.quickSlots.Count > 0) return;

            for (int i = 0; i < squadData.squadSkills.Count && i < quickSlots.Length; i++)
            {
                squadData.runtimeState.quickSlots.Add(new QuickSlotAssignment
                {
                    slotIndex = i,
                    skillId = squadData.squadSkills[i].skillId
                });
            }
        }

        /// <summary>
        /// 왼쪽 위 정보 목록 갱신. 행 순서는 마스터 데이터 순서로 고정이며,
        /// 퀵슬롯 배치 변경과는 무관하다.
        /// </summary>
        private void RefreshSkillInfoRows()
        {
            for (int i = 0; i < skillInfoRows.Length && i < squadData.squadSkills.Count; i++)
            {
                var skill = squadData.squadSkills[i];

                // TODO: 버프 강화 시스템 구현 후 buffProgress 기반 레벨 계산으로 교체 (현재는 1 고정)
                skillInfoRows[i].Display(skill, 1);
            }
        }

        private void RefreshQuickSlots()
        {
            foreach (var assignment in squadData.runtimeState.quickSlots)
            {
                if (assignment.slotIndex < 0 || assignment.slotIndex >= quickSlots.Length) continue;

                var skill = squadData.GetSkill(assignment.skillId);
                quickSlots[assignment.slotIndex].Display(skill);
            }
        }

        /// <summary>퀵슬롯 두 칸의 스킬을 서로 교환 (조건 검사 없이 항상 허용)</summary>
        public void SwapSlots(int indexA, int indexB)
        {
            if (indexA == indexB) return;

            var list = squadData.runtimeState.quickSlots;
            var assignmentA = list.Find(a => a.slotIndex == indexA);
            var assignmentB = list.Find(a => a.slotIndex == indexB);
            if (assignmentA == null || assignmentB == null) return;

            (assignmentA.skillId, assignmentB.skillId) = (assignmentB.skillId, assignmentA.skillId);

            RefreshQuickSlots(); // 위 정보 목록은 배치와 무관하게 고정이므로 갱신 불필요
        }

        /// <summary>스킬 체인에서 적용한 총합 데미지를 스쿼드 세팅 화면에 동기화</summary>
        public void RefreshSkillChainSummary()
        {
            long damage = squadData.runtimeState.leaderSkillChainTotalDamage;
            bool hasChain = squadData.runtimeState.leaderSkillChain.Count > 0 && damage > 0;

            skillChainTotalDamageText.text = hasChain
                ? $"리더 스킬 체인 총합 데미지\n({damage:N0})"
                : "리더 스킬 체인 총합 데미지\n(미설정)";

            skillChainViewButton.interactable = hasChain;
        }

        /// <summary>"스쿼드 리더 스킬 체인 설정" 버튼 — 체인 설정 UI로 전환</summary>
        public void OnOpenSkillChainClicked()
        {
            skillChainCanvas.SetActive(true);
            settingCanvasRoot.SetActive(false);
        }

        /// <summary>체인 설정 UI에서 복귀할 때 호출 — 이 캔버스를 다시 켜고 요약 갱신</summary>
        public void ShowCanvas()
        {
            settingCanvasRoot.SetActive(true);
            RefreshSkillChainSummary();
        }

        /// <summary>"스쿼드 리더 스킬 체인 확인" 버튼 — 토글 방식 (한 번 더 누르면 닫힘)</summary>
        public void OnViewSkillChainClicked()
        {
            if (skillChainViewPopup.activeSelf)
            {
                skillChainViewPopup.SetActive(false);
                return;
            }

            skillChainViewList.Show(squadData.runtimeState.leaderSkillChain);
            skillChainViewPopup.SetActive(true);
        }

        public void OnCloseSkillChainViewClicked()
        {
            skillChainViewPopup.SetActive(false);
        }

        /// <summary>하단 확인 버튼 — 레이드 시작</summary>
        public void OnConfirmButtonClicked()
        {
            flowController.OnStartRaidClicked();
        }
    }
}
