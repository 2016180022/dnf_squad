using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>모험단 재료 현황 목록의 한 줄 (IngredientListRow 프리팹에 부착)</summary>
    public class IngredientListRowUI : MonoBehaviour
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text characterNameText;
        [SerializeField] private Image ingredientIconImage;
        [SerializeField] private TMP_Text countText;

        public void Display(AdventurerCharacterData character, string ingredientImageId)
        {
            if (portraitImage != null)
            {
                portraitImage.sprite = Resources.Load<Sprite>($"Image/Portrait/{character.characterId}_portrait");
                portraitImage.enabled = portraitImage.sprite != null;
            }

            characterNameText.text = character.characterName;
            countText.text = $"{character.ingredientCount:N0} 개";

            if (ingredientIconImage != null)
            {
                ingredientIconImage.sprite = Resources.Load<Sprite>($"Image/Item/{ingredientImageId}");
                ingredientIconImage.enabled = ingredientIconImage.sprite != null;
            }
        }
    }
}
