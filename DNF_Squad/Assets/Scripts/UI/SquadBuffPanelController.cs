using System.Collections.Generic;
using System.Linq;
using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// 스쿼드 버프 세팅 패널 전체를 관리.
    /// 강화 요소 / 보조 요소 IconArea에 버프 슬롯을 생성하고,
    /// 선택된 버프의 설명과 레벨업/초기화, 포인트 및 재료 표시를 담당한다.
    /// </summary>
    public class SquadBuffPanelController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadData;

        [Header("아이콘 영역 (LayoutGroup이 붙은 Content)")]
        [SerializeField] private Transform enhanceIconArea;   // 강화 요소
        [SerializeField] private Transform assistIconArea;    // 보조 요소
        [SerializeField] private SquadBuffSlotUI buffSlotPrefab;

        [Header("스킬 설명")]
        [SerializeField] private TMP_Text buffNameText;
        [SerializeField] private TMP_Text buffDescriptionText;

        [Header("레벨 / 포인트")]
        [SerializeField] private TMP_Text currentLevelText;      // "현재 스킬 레벨 : Lv. 1"
        [SerializeField] private TMP_Text requiredPointText;     // "레벨업 시 필요한 포인트 : 120 pt"
        [SerializeField] private TMP_Text ownedPointText;        // 하단 보유 포인트
        [SerializeField] private Button levelUpButton;
        [SerializeField] private Button resetButton;

        [Header("재료 표시")]
        [SerializeField] private Image ingredientIconImage;
        [SerializeField] private TMP_Text ingredientCountText;

        [Header("연동")]
        [Tooltip("버프 레벨 변경 시 왼쪽 스킬 정보 목록을 다시 그리기 위해 참조")]
        [SerializeField] private SquadSettingCanvasController settingCanvasController;

        private readonly List<SquadBuffSlotUI> spawnedSlots = new List<SquadBuffSlotUI>();
        private SquadBuffSlotUI selectedSlot;

        private void OnEnable()
        {
            BuildBuffSlots();
            RefreshIngredientDisplay();

            // 최초 진입 시 첫 번째 버프를 자동 선택해 설명 패널을 채운다
            if (selectedSlot == null && spawnedSlots.Count > 0) selectedSlot = spawnedSlots[0];

            RefreshAll();
        }

        /// <summary>
        /// 마스터 데이터를 읽어 IconArea에 슬롯을 생성.
        /// 개수는 고정하지 않고 squadBuffs에 들어온 만큼 만든다.
        /// </summary>
        private void BuildBuffSlots()
        {
            foreach (var slot in spawnedSlots) Destroy(slot.gameObject);
            spawnedSlots.Clear();
            selectedSlot = null;

            foreach (var buff in squadData.squadBuffs)
            {
                var parent = buff.category == SquadBuffCategory.Enhance ? enhanceIconArea : assistIconArea;
                var slot = Instantiate(buffSlotPrefab, parent);
                slot.Init(this, buff);
                spawnedSlots.Add(slot);
            }
        }

        public void OnBuffSlotClicked(SquadBuffSlotUI slot)
        {
            selectedSlot = slot;
            RefreshAll();
        }

        /// <summary>레벨업 버튼 — 다음 레벨 비용만큼 포인트를 차감하고 레벨을 1 올린다</summary>
        public void OnLevelUpClicked()
        {
            if (selectedSlot == null) return;

            var buff = selectedSlot.BuffData;
            int currentLevel = squadData.GetBuffLevel(buff.buffId);
            if (currentLevel >= squadData.buffLevelConfig.maxLevel) return;

            int cost = squadData.buffLevelConfig.GetLevelUpCost(currentLevel);
            if (squadData.runtimeState.squadPoints < cost) return;

            squadData.runtimeState.squadPoints -= cost;
            squadData.SetBuffLevel(buff.buffId, currentLevel + 1);

            RefreshAll();
        }

        /// <summary>초기화 버튼 — 습득 전(Lv.0)으로 되돌리고 투입한 포인트를 전액 반환</summary>
        public void OnResetClicked()
        {
            if (selectedSlot == null) return;

            var buff = selectedSlot.BuffData;
            int currentLevel = squadData.GetBuffLevel(buff.buffId);
            if (currentLevel <= 0) return;

            squadData.runtimeState.squadPoints += squadData.buffLevelConfig.GetSpentPoints(currentLevel);
            squadData.SetBuffLevel(buff.buffId, 0);

            RefreshAll();
        }

        // GetLevelUpCost / GetSpentPoints 삭제 — SquadBuffLevelConfig가 공용으로 담당

        private void RefreshAll()
        {
            RefreshSlots();
            RefreshDescription();
            RefreshLevelAndPoints();

            // 버프 레벨이 바뀌면 왼쪽 스킬 정보의 수치/레벨/부가설명도 함께 갱신
            if (settingCanvasController != null) settingCanvasController.RefreshSkillInfoRows();
        }

        private void RefreshSlots()
        {
            foreach (var slot in spawnedSlots)
            {
                int level = squadData.GetBuffLevel(slot.BuffData.buffId);
                slot.Refresh(level, slot == selectedSlot);
            }
        }

        private void RefreshDescription()
        {
            if (selectedSlot == null)
            {
                buffNameText.text = string.Empty;
                buffDescriptionText.text = string.Empty;
                return;
            }

            var buff = selectedSlot.BuffData;
            buffNameText.text = buff.buffName;
            buffDescriptionText.text = BuildDescription(buff);
        }

        /// <summary>
        /// 설명문 조립. 레벨과 무관하게 항상 동일한 고정 설명을 보여준다.
        /// 템플릿의 {0}, {1}... 자리에는 각 레벨의 수치를 "3/4/5" 형태로 이어붙인다.
        /// 부가 설명은 "N레벨 달성 시, " 접두어를 코드에서 붙여 표시한다.
        /// </summary>
        private string BuildDescription(SquadBuffData buff)
        {
            var text = FormatTemplate(buff.descriptionTemplate, buff);

            if (buff.levels == null) return text;

            foreach (var levelData in buff.levels)
            {
                if (string.IsNullOrEmpty(levelData.bonusDescriptionTemplate)) continue;

                var bonus = FormatTemplate(levelData.bonusDescriptionTemplate, buff);
                text += $"\n\n{levelData.level}레벨 달성 시, {bonus}";
            }

            return text;
        }

        /// <summary>{0}, {1}... 자리를 레벨별 수치를 "/"로 이은 문자열로 치환</summary>
        private string FormatTemplate(string template, SquadBuffData buff)
        {
            if (string.IsNullOrEmpty(template) || buff.levels == null) return template;

            // 템플릿이 참조할 수 있는 최대 수치 개수 = 레벨 데이터 중 가장 긴 effectValues 길이
            int valueCount = buff.levels.Max(l => l.effectValues?.Length ?? 0);
            var args = new object[valueCount];

            for (int i = 0; i < valueCount; i++)
            {
                var perLevel = buff.levels
                    .Where(l => l.effectValues != null && i < l.effectValues.Length)
                    .Select(l => l.effectValues[i].ToString("0.##"));

                args[i] = string.Join("/", perLevel);
            }

            return string.Format(template, args);
        }

        private void RefreshLevelAndPoints()
        {
            int ownedPoints = squadData.runtimeState.squadPoints;
            ownedPointText.text = $"{ownedPoints:N0} pt";

            if (selectedSlot == null)
            {
                levelUpButton.interactable = false;
                resetButton.interactable = false;
                return;
            }

            var buff = selectedSlot.BuffData;
            int currentLevel = squadData.GetBuffLevel(buff.buffId);
            bool isMaxLevel = currentLevel >= squadData.buffLevelConfig.maxLevel;

            currentLevelText.text = $"현재 스킬 레벨 : Lv. {currentLevel}";

            int cost = squadData.buffLevelConfig.GetLevelUpCost(currentLevel);
            requiredPointText.text = isMaxLevel
                ? "레벨업 시 필요한 포인트 : -"
                : $"레벨업 시 필요한 포인트 : {cost:N0} pt";

            levelUpButton.interactable = !isMaxLevel && ownedPoints >= cost;
            resetButton.interactable = currentLevel > 0;
        }

        /// <summary>
        /// 재료 보유 개수 표시.
        /// 프로토타입에서는 "현재 접속 중인 캐릭터" 개념이 없으므로
        /// 멤버 1번 슬롯에 배치된 캐릭터의 보유량을 대신 동기화한다.
        /// </summary>
        public void RefreshIngredientDisplay()
        {
            if (ingredientIconImage != null)
            {
                ingredientIconImage.sprite = Resources.Load<Sprite>($"Image/Item/{squadData.raidIngredientImageId}");
                ingredientIconImage.enabled = ingredientIconImage.sprite != null;
            }

            string memberId = squadData.runtimeState.composition.memberCharacterIds[0];
            var character = string.IsNullOrEmpty(memberId) ? null : squadData.GetCharacter(memberId);

            ingredientCountText.text = character != null ? $"{character.ingredientCount:N0} 개" : "0 개";
        }
    }
}
