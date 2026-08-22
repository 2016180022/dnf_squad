using UnityEngine;
using UnityEngine.InputSystem;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 성역 타이밍 수동 측정용 디버그 도구 (레이드 기능 — 측정 끝나면 씬에서 빼도 되는 임시 도구).
    /// 고리 회전(SanctuaryRingController)과 경과시간(RaidClockController)은 둘 다 Time.timeSinceLevelLoad를
    /// 그대로 쓰고 있고, 이 값은 Time.timeScale을 그대로 따라간다. 그래서 여기서는 다른 코드를 건드리지 않고
    /// Time.timeScale만 조절해서 "일시정지"와 "배속"을 동시에 구현한다.
    ///
    /// 조작법:
    ///  - P 키: 일시정지 / 재생 토글
    ///  - [ 키: 배속 한 단계 낮추기
    ///  - ] 키: 배속 한 단계 올리기
    /// (Update()는 timeScale과 무관하게 항상 호출되므로, 일시정지 중에도 키 입력은 계속 먹는다)
    /// </summary>
    public class SanctuaryDebugTimeController : MonoBehaviour
    {
        private static readonly float[] SpeedSteps = { 0.25f, 0.5f, 1f, 2f, 5f, 10f, 20f };

        private int speedIndex = 2; // SpeedSteps[2] = 1x 부터 시작
        private bool paused = false;

        private void Update()
        {
            if (Keyboard.current == null) return;

            if (Keyboard.current.pKey.wasPressedThisFrame)
                TogglePause();

            if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
                ChangeSpeed(-1);

            if (Keyboard.current.rightBracketKey.wasPressedThisFrame)
                ChangeSpeed(1);
        }

        private void TogglePause()
        {
            paused = !paused;
            ApplyTimeScale();
            Debug.Log($"[성역 디버그] {(paused ? "일시정지" : "재생")} (배속 x{SpeedSteps[speedIndex]})");
        }

        private void ChangeSpeed(int direction)
        {
            speedIndex = Mathf.Clamp(speedIndex + direction, 0, SpeedSteps.Length - 1);
            if (!paused) ApplyTimeScale();
            Debug.Log($"[성역 디버그] 배속 x{SpeedSteps[speedIndex]}");
        }

        private void ApplyTimeScale()
        {
            Time.timeScale = paused ? 0f : SpeedSteps[speedIndex];
        }

        // 혹시 이 오브젝트가 씬에서 빠지거나 플레이가 멈출 때 timeScale이 0/배속인 채로 남지 않도록 원복
        private void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
