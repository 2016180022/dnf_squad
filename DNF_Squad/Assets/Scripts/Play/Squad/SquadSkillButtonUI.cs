using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 플레이씬 스쿼드 스킬 바의 버튼 1개.
    /// 세팅씬 퀵슬롯(SquadSkillSlotUI)과 달리 드래그는 없고, 클릭 시 스킬을 실행한다.
    /// 표시 순서는 SquadSkillBarController가 quickSlots 배치를 그대로 읽어서 정해준다.
    /// </summary>
    public class SquadSkillButtonUI : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [Tooltip("Image Type=Filled 권장. 비워두면 쿨타임 표시를 생략한다.")]
        [SerializeField] private Image cooldownFillImage;
        [Tooltip("선택 사항. 비워두면 숫자 표시를 생략한다.")]
        [SerializeField] private TMP_Text cooldownText;

        public string SkillId { get; private set; }

        public void Init(string skillId, Sprite icon, UnityAction onClick)
        {
            SkillId = skillId;

            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
            }

            // cooldownFillImage도 Source Image가 있어야 Fill 타입(Radial 등)이 정상 동작하므로 아이콘과 동일한
            // 스프라이트를 넣어준다. 색상/투명도는 에디터에서 오브젝트 자체에 설정해둔 값이 그대로 유지된다.
            // 퀵슬롯 배치는 세팅씬에서 바뀌므로(슬롯마다 뜨는 스킬이 고정이 아님) 에디터에서 미리 넣어둘 수 없어 여기서 갱신.
            if (cooldownFillImage != null)
            {
                cooldownFillImage.sprite = icon;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClick);
        }

        public void SetInteractable(bool interactable)
        {
            button.interactable = interactable;
        }

        /// <summary>
        /// 남은 쿨타임을 표시.
        /// 주의: cooldownFillImage/cooldownText는 iconImage/button과 겹치지 않는 "전용 자식 오브젝트"여야 한다.
        /// (아이콘과 같은 오브젝트를 재사용하면 SetActive(false) 호출로 슬롯 전체가 꺼지는 버그가 재발함 — 이전에 겪은 문제.)
        /// </summary>
        public void SetCooldown(float remainingSeconds, float totalSeconds)
        {
            bool onCooldown = remainingSeconds > 0f;

            if (cooldownFillImage != null)
            {
                cooldownFillImage.gameObject.SetActive(onCooldown);
                cooldownFillImage.fillAmount = totalSeconds > 0f ? remainingSeconds / totalSeconds : 0f;
            }

            if (cooldownText != null)
            {
                cooldownText.gameObject.SetActive(onCooldown);
                cooldownText.text = onCooldown ? Mathf.CeilToInt(remainingSeconds).ToString() : string.Empty;
            }
        }
    }
}
