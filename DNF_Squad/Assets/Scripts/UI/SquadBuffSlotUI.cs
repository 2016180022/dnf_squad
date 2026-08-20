using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// 버프 아이콘 1칸 (BuffSkill 프리팹에 부착).
    /// IconArea의 LayoutGroup이 정렬을 담당하므로 위치는 신경쓰지 않는다.
    /// </summary>
    public class SquadBuffSlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image frameImage;   // 선택 여부에 따라 색이 바뀌는 테두리
        [SerializeField] private Button button;

        [Header("테두리 색")]
        [SerializeField] private Color normalColor = Color.red;
        [SerializeField] private Color selectedColor = Color.green;

        public SquadBuffData BuffData { get; private set; }

        private SquadBuffPanelController controller;

        public void Init(SquadBuffPanelController owner, SquadBuffData data)
        {
            controller = owner;
            BuffData = data;

            if (iconImage != null)
            {
                iconImage.sprite = Resources.Load<Sprite>($"Image/Skill/{data.buffId}");
                iconImage.enabled = iconImage.sprite != null;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => controller.OnBuffSlotClicked(this));
        }

        /// <summary>현재 레벨 표시와 선택 상태 갱신</summary>
        public void Refresh(int currentLevel, bool isSelected)
        {
            if (levelText != null) levelText.text = $"Lv. {currentLevel}";
            if (frameImage != null) frameImage.color = isSelected ? selectedColor : normalColor;
        }
    }
}
