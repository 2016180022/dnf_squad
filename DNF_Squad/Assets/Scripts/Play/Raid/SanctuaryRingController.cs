using UnityEngine;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 성천의 고리 — 확정된 궤도 반지름/회전 속도로 원을 그리며 도는 시각 오브젝트 전용 (레이드 기능).
    /// 노드 활성 여부 판정은 SanctuaryController가 별도로 담당하고, 이 컴포넌트는 순수 비주얼(위치 갱신)만 한다.
    /// LightHalo처럼 RectTransform을 가진 UI 오브젝트에 그대로 붙여서 쓴다.
    /// </summary>
    public class SanctuaryRingController : MonoBehaviour
    {
        // 확정된 고리 파라미터 (성역 타이밍 시뮬레이터에서 확정된 값 그대로 — 더 이상 조정 안 함)
        private const float PeriodSeconds = 300f;  // 회전 주기(1바퀴)
        private const float OrbitRadius = 205f;    // 궤도 반지름(px)
        private const float StartAngleDeg = 90f;   // 시작각(t=0일 때 고리 방향)
        private const int Direction = 1;           // 1 = 반시계방향(CCW)

        [SerializeField] private RaidClockController raidClockController;
        [Tooltip("비워두면 이 오브젝트 자신의 RectTransform을 사용")]
        [SerializeField] private RectTransform ring;

        // 씬에 배치해둔 최초 위치를 궤도의 중심(=BossNode 위치)으로 그대로 사용
        private Vector2 centerPosition;

        private void Awake()
        {
            if (ring == null) ring = GetComponent<RectTransform>();
            centerPosition = ring.anchoredPosition;
        }

        private void Update()
        {
            float elapsed = raidClockController.ElapsedSecondsFloat;
            float angleDeg = StartAngleDeg + Direction * (360f / PeriodSeconds) * elapsed;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * OrbitRadius;
            ring.anchoredPosition = centerPosition + offset;
        }
    }
}
