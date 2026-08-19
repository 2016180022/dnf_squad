using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>"스킬 사용 순서" 목록의 한 줄</summary>
    public class SkillChainStepItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text orderText;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text skillNameText;

        public void Display(int orderNumber, string skillId, string skillName)
        {
            orderText.text = orderNumber.ToString();
            skillNameText.text = skillName;

            if (iconImage != null)
            {
                iconImage.sprite = Resources.Load<Sprite>($"Image/Skill/{skillId}");
                iconImage.enabled = iconImage.sprite != null;
            }
        }
    }
}
