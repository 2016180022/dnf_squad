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

        public void OnDrop(PointerEventData eventData)
        {
            var draggedItem = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<CharacterListItemUI>()
                : null;

            if (draggedItem == null || draggedItem.CharacterData == null) return;

            controller.TryAssignCharacter(this, draggedItem.CharacterData);
        }
    }
}
