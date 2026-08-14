using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class CharacterSlotUI : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text fameText;
        [SerializeField] private GameObject emptyStatePlaceholder;
        [SerializeField] private TMP_Text warningText;

        public SlotRole Role { get; private set; }
        public int MemberIndex { get; private set; } = -1;
        public string AssignedCharacterId { get; private set; }

        private SquadConfigCanvasController controller;

        public void Init(SquadConfigCanvasController owner, SlotRole role, int memberIndex = -1)
        {
            controller = owner;
            Role = role;
            MemberIndex = memberIndex;
            Clear();
        }

        public void AssignCharacter(AdventurerCharacterData data)
        {
            AssignedCharacterId = data.characterId;
            portraitImage.sprite = Resources.Load<Sprite>(data.portraitImageId); // TODO: 프로젝트 리소스 로드 방식 확정 후 교체
            portraitImage.enabled = true;
            nameText.text = data.characterName;
            fameText.text = data.fame.ToString();
            emptyStatePlaceholder.SetActive(false);
            SetWarning(null);
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

        public void SetWarning(string message)
        {
            if (warningText == null) return;
            warningText.gameObject.SetActive(!string.IsNullOrEmpty(message));
            warningText.text = message;
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
