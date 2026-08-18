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
        [SerializeField] private TMP_Text warningText; // 슬롯 내부 경고문 — 비어있을 때만 표시

        public SlotRole Role { get; private set; }
        public int MemberIndex { get; private set; } = -1;
        public string AssignedCharacterId { get; private set; }

        private SquadConfigCanvasController controller;
        private Transform dragLayer;
        private RectTransform dragGhost;
        private string defaultWarningMessage;

        public void Init(SquadConfigCanvasController owner, SlotRole role, Transform dragLayerRoot, int memberIndex = -1)
        {
            controller = owner;
            Role = role;
            dragLayer = dragLayerRoot;
            MemberIndex = memberIndex;

            defaultWarningMessage = role switch
            {
                SlotRole.Leader => "스쿼드 리더를 편성해주세요",
                SlotRole.Buffer => "버퍼를 편성해주세요",
                _ => "멤버를 편성해주세요"
            };

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
            warningText.gameObject.SetActive(false);
        }

        public void Clear()
        {
            AssignedCharacterId = null;
            portraitImage.sprite = null;
            portraitImage.enabled = false;
            nameText.text = string.Empty;
            fameText.text = string.Empty;
            emptyStatePlaceholder.SetActive(true);
            warningText.text = defaultWarningMessage;
            warningText.gameObject.SetActive(true);
        }

        /// <summary>멤버 슬롯 동기화용 — 비어있는 상태에서 문구만 교체 (활성/비활성은 그대로 유지)</summary>
        public void SetWarningMessage(string message)
        {
            warningText.text = message;
        }

        /// <summary>기본 문구("~를 편성해주세요")로 복귀</summary>
        public void ResetWarningMessage()
        {
            warningText.text = defaultWarningMessage;
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
