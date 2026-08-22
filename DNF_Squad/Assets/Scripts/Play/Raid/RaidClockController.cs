using UnityEngine;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 레이드 경과 시간(초)을 재는 공용 컨트롤러 (레이드 기능).
    /// 성역 시스템 등 "레이드 시작 후 몇 초"가 필요한 레이드 기능들이 공통으로 참조해서 쓴다.
    /// 별도 시작 처리 없이 플레이 씬이 로드된 시점을 곧 레이드 시작 시점으로 취급한다
    /// (Time.timeSinceLevelLoad 그대로 사용 — 프레임 누적 오차 없음).
    /// </summary>
    public class RaidClockController : MonoBehaviour
    {
        /// <summary>레이드(=씬) 시작 후 경과 시간(초, 실수) — 성역 고리 회전처럼 부드러운 애니메이션에 사용.</summary>
        public float ElapsedSecondsFloat => Time.timeSinceLevelLoad;

        /// <summary>레이드(=씬) 시작 후 경과한 정수 초 — 노드 활성 여부 등 정수초 판정에 사용.</summary>
        public int ElapsedSeconds => Mathf.FloorToInt(ElapsedSecondsFloat);
    }
}
