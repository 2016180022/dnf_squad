using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class CharacterSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text fameText;
        [SerializeField] private GameObject emptyStatePlaceholder;
        // warningText 삭제 — 경고 표시는 SquadConfigCanvasController의 공용 TextBox가 담당

        public SlotRole Role { get; private set; }
        public int MemberIndex { get; private set; } = -1;
        public string AssignedCharacterId { get; private set; }

        private SquadConfigCanvasController controller;
        private Transform dragLayer;
        private RectTransform dragGhost;

        public void Init(SquadConfigCanvasController owner, SlotRole role, Transform dragLayerRoot, int memberIndex = -1)
        {
            controller = owner;
            Role = role;
            dragLayer = dragLayerRoot;
            MemberIndex = memberIndex;
            Clear();
        }

        public void AssignCharacter(AdventurerCharacterData data)
        {
            AssignedCharacterId = data.characterId;
            portraitImage.sprite = Resources.Load<Sprite>($"Image/Portrait/{data.characterId}");
            portraitImage.enabled = true;
            nameText.text = data.characterName;
            fameText.text = data.fame.ToString();
            emptyStatePlaceholder.SetActive(false);
        }

        public void Clear()
        {
            AssignedCharacterId = null;
            portraitImage.sprite = null;
            portraitImage.enabled = false;
            nameText.text = string.Empty;
            fameText.text = string.Empty;
            emptyStatePlaceholder.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (string.IsNullOrEmpty(AssignedCharacterId)) return; // 빈 슬롯은 드래그 불가

            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            dragGhost.SetParent(dragLayer, false);
            dragGhost.sizeDelta = ((RectTransform)portraitImage.transform).sizeDelta;

            var dragGhostImage = dragGhost.GetComponent<Image>();
            dragGhostImage.sprite = portraitImage.sprite;
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

            if (string.IsNullOrEmpty(AssignedCharacterId)) return; // OnDrop에서 이미 이동/스왑 처리되어 비워진 경우

            var droppedOnSlot = eventData.pointerEnter != null
                ? eventData.pointerEnter.GetComponentInParent<CharacterSlotUI>()
                : null;

            if (droppedOnSlot == null)
            {
                controller.TryUnassignCharacter(this); // 슬롯 위가 아닌 곳에 놓으면 배치 해제
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) return;

            var draggedItem = eventData.pointerDrag.GetComponent<CharacterListItemUI>();
            if (draggedItem != null && draggedItem.CharacterData != null)
            {
                controller.TryAssignCharacter(this, draggedItem.CharacterData);
                return;
            }

            var draggedSlot = eventData.pointerDrag.GetComponent<CharacterSlotUI>();
            if (draggedSlot != null && draggedSlot != this)
            {
                controller.TrySwapOrMove(draggedSlot, this);
            }
        }
    }
}
