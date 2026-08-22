using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 성역 타이밍 수동 측정용 디버그 도구 (레이드 기능 — 측정 끝나면 씬에서 빼도 되는 임시 도구).
    /// 노드 버튼에 이 컴포넌트가 리스너를 추가로 하나 더 얹어서, 버튼을 누른 시점의 경과시간을 콘솔에 기록한다.
    /// 현황판의 기존 선택 동작(RaidBoardController)은 건드리지 않는다 — 같은 버튼에 리스너 두 개가 같이 붙는 것뿐.
    /// </summary>
    public class SanctuaryTimingLoggerController : MonoBehaviour
    {
        [System.Serializable]
        public class NodeLogButton
        {
            public string nodeId;
            public Button button;
        }

        [SerializeField] private RaidClockController raidClockController;
        [SerializeField] private List<NodeLogButton> nodeButtons = new List<NodeLogButton>();

        private void Start()
        {
            foreach (var entry in nodeButtons)
            {
                string nodeId = entry.nodeId; // 클로저 캡처용 로컬 변수
                if (entry.button != null) entry.button.onClick.AddListener(() => LogTimestamp(nodeId));
            }
        }

        /// <summary>버튼을 누른 시점의 경과시간을 "초:센티초" 형식으로 콘솔에 기록한다 (예: 137.42초 → "137:42")</summary>
        private void LogTimestamp(string nodeId)
        {
            float elapsed = raidClockController.ElapsedSecondsFloat;
            int sec = Mathf.FloorToInt(elapsed);
            int centi = Mathf.FloorToInt((elapsed - sec) * 100f);
            Debug.Log($"[성역 타이밍 기록] {nodeId} : {sec}:{centi:00}");
        }
    }
}
