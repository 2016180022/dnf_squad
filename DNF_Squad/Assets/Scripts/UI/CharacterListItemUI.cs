using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class CharacterListItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text jobNameText;
        [SerializeField] private TMP_Text gearScoreText;
        [SerializeField] private TMP_Text fameText;
        [SerializeField] private TMP_Text entryCount;
        [SerializeField] private CanvasGroup canvasGroup;

        public AdventurerCharacterData CharacterData { get; private set; }

        private Transform dragLayer;
        private RectTransform dragGhost;

        public void Setup(AdventurerCharacterData data, Transform dragLayerRoot)
        {
            CharacterData = data;
            dragLayer = dragLayerRoot;

            portraitImage.sprite = Resources.Load<Sprite>($"Image/Portrait/{data.characterId}_portrait");
            nameText.text = data.characterName;
            jobNameText.text = data.jobName;
            gearScoreText.text = data.gearScore.ToString();
            fameText.text = data.fame.ToString();
            entryCount.text = $"{data.remainingEntryCount} / 1";

            bool isLocked = data.remainingEntryCount <= 0;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = isLocked ? 0.4f : 1f;
            }
            enabled = !isLocked; // 입장 횟수 소모 시 드래그 이벤트 자체가 발생하지 않도록 컴포넌트 비활성화
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            dragGhost = new GameObject("DragGhost", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            dragGhost.SetParent(dragLayer, false);
            dragGhost.sizeDelta = ((RectTransform)portraitImage.transform).rect.size;

            var dragGhostImage = dragGhost.GetComponent<Image>();
            dragGhostImage.sprite = portraitImage.sprite;
            dragGhostImage.raycastTarget = false; // 고스트가 드롭 이벤트를 가로채지 않도록

            if (canvasGroup != null) canvasGroup.alpha = 0.5f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragGhost != null) dragGhost.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragGhost != null) Destroy(dragGhost.gameObject);
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }
    }
}