using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DnfSquad.Data;
using DnfSquad.Play.Core;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 성역(빛의 고리) 시스템 — 노드별 활성 여부 조회 + 노드가 닫히는 순간 강제 퇴장 처리 (레이드 기능).
    /// 활성 구간 데이터 자체는 RaidRuntimeData(sanctuaryWindows/alwaysActiveNodeIds)가 갖고 있고,
    /// 이 컨트롤러는 RaidClockController의 경과시간과 대조해서 "지금 이 순간" 판정 + 전환 감지만 담당한다.
    /// </summary>
    public class SanctuaryController : MonoBehaviour
    {
        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private RaidClockController raidClockController;
        [SerializeField] private MapTransitionController mapTransitionController;
        [Tooltip("노드가 닫힐 때 강제로 이동시킬 대기 노드의 nodeId")]
        [SerializeField] private string standbyNodeId = "StandbyNode";

        // 노드별 "직전 프레임에 활성이었는지" 캐시 — 열림→닫힘으로 바뀌는 순간에만 강제 퇴장을 1회 트리거하기 위함
        private readonly Dictionary<string, bool> lastActiveState = new Dictionary<string, bool>();
        private List<string> timedNodeIds;

        private void Start()
        {
            // sanctuaryWindows에 등장하는 nodeId만 추린다 — BossNode/StandbyNode처럼 상시 활성인 노드는
            // 여기서 감지할 "닫힘 전환" 자체가 없으므로 대상에서 제외.
            timedNodeIds = raidRuntimeData.sanctuaryWindows.Select(w => w.nodeId).Distinct().ToList();
        }

        /// <summary>이 노드가 지금(현재 경과시간 기준) 활성 상태인지. RaidBoardController가 파견/진입 가능 여부에 사용.
        /// 대기 노드는 성역 타이머와 무관하게 항상 활성이어야 하는 스펙 확정 사항이라, `alwaysActiveNodeIds`
        /// 데이터 설정에만 기대지 않고 코드에서도 예외로 보장한다(2026-08-22, 17차).</summary>
        public bool IsNodeActive(string nodeId)
        {
            if (nodeId == standbyNodeId) return true;
            return raidRuntimeData.IsNodeActive(nodeId, raidClockController.ElapsedSeconds);
        }

        private void Update()
        {
            DetectClosures();
        }

        /// <summary>노드별로 활성→비활성 전환 순간을 찾아서 그 순간에만 강제 퇴장을 트리거한다.</summary>
        private void DetectClosures()
        {
            foreach (var nodeId in timedNodeIds)
            {
                bool active = IsNodeActive(nodeId);
                bool wasActive = lastActiveState.TryGetValue(nodeId, out var prev) && prev;

                if (wasActive && !active) OnNodeClosed(nodeId);

                lastActiveState[nodeId] = active;
            }
        }

        /// <summary>노드가 방금 비활성화됐을 때: 그 노드에 있던 플레이어를 대기 노드로 강제 이동 + 필드 리프레시.</summary>
        private void OnNodeClosed(string nodeId)
        {
            if (mapTransitionController.CurrentNodeId == nodeId)
            {
                mapTransitionController.EnterNode(standbyNodeId);
            }

            // 2026-08-22: 스쿼드 파견 시스템(occupants) 도입에 맞춰 강제 퇴장 처리 반영.
            // 플레이어(Y)를 포함해 이 노드에 있던 occupant 전원을 대기 노드로 옮긴다.
            raidRuntimeData.EvacuateOccupants(nodeId, standbyNodeId);
        }
    }
}
