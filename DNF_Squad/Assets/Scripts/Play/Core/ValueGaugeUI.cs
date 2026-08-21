using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// Fill Amount 방식 이미지로 비율을 표시하는 범용 게이지.
    /// Fill 이미지의 Image Type을 Filled로 설정해서 사용 (세로 게이지는 Fill Method: Vertical,
    /// 가로 게이지는 Horizontal). Back 이미지는 별도 오브젝트로 뒤에 깔면 되고 이 스크립트는 안 건드림.
    /// </summary>
    public class ValueGaugeUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [Tooltip("게이지 가운데 표시할 'n%' 텍스트 (없으면 비워둬도 됨)")]
        [SerializeField] private TMP_Text percentText;

        public void SetRatio(float current, float max)
        {
            float ratio = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            fillImage.fillAmount = ratio;

            if (percentText != null)
                percentText.text = $"{Mathf.RoundToInt(ratio * 100f)}%";
        }
    }
}
