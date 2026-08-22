using System.Collections;
using TMPro;
using UnityEngine;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// 플레이 씬 공용 경고 텍스트 팝업 (기본 기능 — 코어).
    /// 정원 초과 등 즉시성 경고에 사용. 메시지를 띄우고 일정 시간 뒤 자동으로 닫힌다.
    /// </summary>
    public class GlobalWarningUI : MonoBehaviour
    {
        [SerializeField] private GameObject warningRoot;
        [SerializeField] private TMP_Text warningText;
        [SerializeField] private float displaySeconds = 3f;

        private Coroutine hideRoutine;

        public void ShowWarning(string message)
        {
            // 현황판이 닫혀 있는 등 이 오브젝트 자체가 비활성 상태면 StartCoroutine이 예외를 던지므로,
            // 그런 경우는 그냥 무시한다 (호출부 — LuminousGaugeController 등 — 는 신경 쓰지 않아도 됨).
            if (!isActiveAndEnabled) return;

            warningText.text = message;
            warningRoot.SetActive(true);

            if (hideRoutine != null) StopCoroutine(hideRoutine);
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displaySeconds);
            warningRoot.SetActive(false);
            hideRoutine = null;
        }
    }
}
