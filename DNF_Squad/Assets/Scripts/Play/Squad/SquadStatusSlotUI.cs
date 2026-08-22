using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 좌상단 스쿼드 상태 패널의 슬롯 1칸 (표시 전용, 상호작용 없음).
    /// 색상 태그 이미지는 슬롯마다 이미 정해진 색이므로 인스펙터에서 고정 스프라이트로 세팅해두면 됨
    /// (이 스크립트는 색을 바꾸지 않음).
    /// </summary>
    public class SquadStatusSlotUI : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text nameText;

        public void Display(AdventurerCharacterData data)
        {
            if (data == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);
            portraitImage.sprite = Resources.Load<Sprite>($"Image/Portrait/{data.characterId}_portrait");
            nameText.text = data.characterName;
        }
    }
}
