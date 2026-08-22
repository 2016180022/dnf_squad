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
            // 신규(23차): 화면을 가리기 위해 이 오브젝트 자체를 하이어라키에서 비활성 상태로 시작하는
            // 세팅이 있을 수 있음 — 기존엔 이 경우 그냥 무시했는데, 그러면 영원히 활성화될 기회가 없어
            // 경고가 한 번도 안 뜨는 문제가 있었다. ShowWarning이 호출되는 시점엔 "지금 당장 보여달라"는
            // 뜻이므로, 비활성 상태면 스스로 먼저 활성화한다.
            if (!gameObject.activeSelf) gameObject.SetActive(true);

            // 그래도(상위 부모가 비활성인 경우 등) 여전히 활성화가 안 됐다면 StartCoroutine이 예외를
            // 던지므로 방어적으로 무시하되, 원인을 알 수 있도록 콘솔에는 남긴다.
            if (!isActiveAndEnabled)
            {
                Debug.LogWarning($"[GlobalWarningUI] 상위 오브젝트가 비활성 상태라 경고를 표시할 수 없습니다: {message}");
                return;
            }

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
