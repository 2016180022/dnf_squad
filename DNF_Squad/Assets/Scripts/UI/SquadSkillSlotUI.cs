using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// 왼쪽 아래 퀵슬롯 바의 1칸 — 드래그 앤 드롭으로 배치 순서 변경.
    /// 칸의 위치 번호(1~6)는 고정이고, 그 안에 담기는 스킬이 서로 교환된다.
    /// </summary>
    public class SquadSkillSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private TMP_Text slotNumberText; // 고정 위치 번호 (1~6)
        [SerializeField] private Image iconImage;         // 배치된 스킬 아이콘
        [SerializeField] private TMP_Text skillNameText;  // 배치된 스킬명 (아이콘만 쓸 경우 비워둬도 무방)

        public int SlotIndex { get; private set; }

        private SquadSettingCanvasController controller;
        private Transform dragLayer;
        private RectTransform dragGhost;

        public void Init(SquadSettingCanvasController owner, int slotIndex, Transform dragLayerRoot)
        {
            controller = owner;
            SlotIndex = slotIndex;
            dragLayer = dragLayerRoot;

            if (slotNumberText != null) slotNumberText.text = (slotIndex + 1).ToString();
        }

        /// <summary>이 칸에 배치된 스킬을 표시</summary>
        public void Display(SquadSkillData skill)
        {
            if (skill == null) return;

            if (iconImage != null)
            {
                iconImage.sprite = Resources.Load<Sprite>($"Image/Skill/{skill.skillId}");
                iconImage.enabled = iconImage.sprite != null;
            }
            if (skillNameText != null) skillNameText.text = skill.skillName;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragLayer.SetAsLastSibling(); // 하이어라키 순서가 틀어져도 고스트가 항상 최상단에 그려지도록 보장

            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            dragGhost.SetParent(dragLayer, false);
            dragGhost.sizeDelta = ((RectTransform)transform).sizeDelta;

            var dragGhostImage = dragGhost.GetComponent<Image>();
            dragGhostImage.sprite = iconImage != null ? iconImage.sprite : null;
            dragGhostImage.raycastTarget = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null) dragGhost.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null) Destroy(dragGhost.gameObject);
            dragGhost = null;
            // 뗀 위치가 퀵슬롯 위가 아니면 OnDrop이 호출되지 않으므로 입력이 자연히 무시된다
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;

            var draggedSlot = eventData.pointerDrag.GetComponent<SquadSkillSlotUI>();
            if (draggedSlot == null || draggedSlot == this) return;

            controller.SwapSlots(draggedSlot.SlotIndex, SlotIndex);
        }
    }
}
