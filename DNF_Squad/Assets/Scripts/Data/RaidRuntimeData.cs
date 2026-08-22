using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DnfSquad.Data
{
    [CreateAssetMenu(fileName = "RaidRuntimeData", menuName = "DnfSquad/Raid Runtime Data")]
    public class RaidRuntimeData : ScriptableObject
    {
        [Header("마스터 데이터 (레이드 스크립트 설정, 읽기 전용)")]
        public List<MonsterData> monsters = new List<MonsterData>();

        [Header("몬스터가 없는 노드에 표시할 기본 배경")]
        [Tooltip("Resources/Image/Map/ 아래 파일명 (확장자 제외)")]
        public string defaultNodeBackgroundImageId;

        [Header("플레이어 스탯 (데모용 고정값 — 캐릭터별 수치 대신 레이드에서 통일 관리)")]
        public int playerMaxHp = 100;
        public int playerMaxMp = 100;

        [Header("성광 유지율 (레이드 제한시간, 데모용 고정값)")]
        public int maxLuminousGauge = 100;

        [Header("성역(빛의 고리) 시스템 — 노드별 활성 구간 (초 단위 확정값)")]
        [Tooltip("좌표/고리 파라미터로 매번 역산하지 않고, 이미 확정된 열림~닫힘 초 값을 그대로 저장한다. " +
            "고리가 레이드 중 여러 바퀴 돌 수 있어서 같은 nodeId가 여러 항목(구간)을 가질 수 있다.")]
        public List<SanctuaryNodeWindow> sanctuaryWindows = new List<SanctuaryNodeWindow>();
        [Tooltip("성역 타이머와 무관하게 항상 활성으로 취급할 노드 (BossNode, StandbyNode 등)")]
        public List<string> alwaysActiveNodeIds = new List<string>();

        [Header("정원(동시 입장 인원 제한) — 기본 1명, 아래 목록에 있는 노드만 3명")]
        [Tooltip("스탠바이 노드, 보스 노드처럼 스쿼드원이 여러 명 동시에 들어갈 수 있는 노드")]
        public List<string> highCapacityNodeIds = new List<string> { "StandbyNode", "BossNode" };

        [Header("런타임 상태 (플레이 중 갱신, 저장 대상)")]
        public RaidBoardRuntimeState runtimeState = new RaidBoardRuntimeState();

        // ===== 조회 헬퍼 =====

        public MonsterData GetMonster(string monsterId) =>
            monsters.FirstOrDefault(m => m.monsterId == monsterId);

        public RaidNodeRuntimeState GetNodeState(string nodeId) =>
            runtimeState.nodeStates.FirstOrDefault(s => s.nodeId == nodeId);

        /// <summary>노드 런타임 상태를 조회하고, 없으면 새로 만들어 반환한다.
        /// 노드 목록을 마스터 데이터로 미리 들고 있지 않으므로, 필요한 시점에 생성한다.</summary>
        public RaidNodeRuntimeState GetOrCreateNodeState(string nodeId)
        {
            var state = GetNodeState(nodeId);
            if (state == null)
            {
                state = new RaidNodeRuntimeState { nodeId = nodeId };
                runtimeState.nodeStates.Add(state);
            }
            return state;
        }

        public MonsterRuntimeState GetMonsterState(string monsterId) =>
            runtimeState.monsterStates.FirstOrDefault(s => s.monsterId == monsterId);

        /// <summary>이 노드에 현재 위치한 몬스터(마스터 데이터) 조회. 없으면 null</summary>
        public MonsterData GetMonsterAtNode(string nodeId)
        {
            var monsterState = runtimeState.monsterStates.FirstOrDefault(s => s.currentNodeId == nodeId && !s.isDead);
            return monsterState != null ? GetMonster(monsterState.monsterId) : null;
        }

        /// <summary>이 노드에 표시할 배경 이미지 ID. 몬스터가 있으면 몬스터 배경, 없으면 기본 배경</summary>
        public string GetNodeBackgroundImageId(string nodeId)
        {
            var monster = GetMonsterAtNode(nodeId);
            return monster != null ? monster.mapBackgroundImageId : defaultNodeBackgroundImageId;
        }

        /// <summary>성역 시스템 — 이 노드가 주어진 경과시간(초)에 활성 상태인지.
        /// alwaysActiveNodeIds에 있으면 무조건 활성, 아니면 sanctuaryWindows 중 해당 구간에 걸리는 게 있는지로 판정.</summary>
        public bool IsNodeActive(string nodeId, int elapsedSec)
        {
            if (alwaysActiveNodeIds.Contains(nodeId)) return true;

            foreach (var window in sanctuaryWindows)
            {
                if (window.nodeId == nodeId && elapsedSec >= window.openSec && elapsedSec <= window.closeSec)
                    return true;
            }
            return false;
        }

        /// <summary>성역 시스템 — 이 노드의 다음 활성화/비활성화 전환까지 남은 초. 상시 활성 노드거나 예정된 전환이
        /// 없으면 -1 (타이머 UI를 끄라는 뜻). 강제 퇴장 규칙(닫힘 초+1에 전환)과 동일한 기준으로 계산한다.</summary>
        public int GetSecondsUntilSanctuaryTransition(string nodeId, int elapsedSec)
        {
            if (alwaysActiveNodeIds.Contains(nodeId)) return -1;

            // 현재 활성 구간 안이면 → 그 구간이 닫히는 시점까지 남은 시간
            foreach (var window in sanctuaryWindows)
            {
                if (window.nodeId == nodeId && elapsedSec >= window.openSec && elapsedSec <= window.closeSec)
                    return (window.closeSec + 1) - elapsedSec;
            }

            // 비활성 상태면 → 앞으로 열릴 가장 가까운 구간의 시작 시각까지 남은 시간
            int nextOpenSec = int.MaxValue;
            foreach (var window in sanctuaryWindows)
            {
                if (window.nodeId == nodeId && window.openSec > elapsedSec && window.openSec < nextOpenSec)
                    nextOpenSec = window.openSec;
            }

            return nextOpenSec == int.MaxValue ? -1 : nextOpenSec - elapsedSec;
        }

        // ===== 초기화 =====

        /// <summary>플레이 씬 시작 시 마스터 데이터 기준으로 런타임 상태를 새로 채운다</summary>
        public void InitializeRuntimeState()
        {
            runtimeState = new RaidBoardRuntimeState
            {
                playerCurrentHp = playerMaxHp,
                playerCurrentMp = playerMaxMp,
                luminousGauge = maxLuminousGauge // 레이드 시작 시 100(=maxLuminousGauge) 부여
            };

            foreach (var monster in monsters)
            {
                runtimeState.monsterStates.Add(new MonsterRuntimeState
                {
                    monsterId = monster.monsterId,
                    currentNodeId = monster.startingNodeId,
                    currentHp = monster.maxHp,
                    isDead = false
                });
            }
        }

        // ===== 상태 갱신 =====

        /// <summary>계율의 사슬 등으로 몬스터 위치를 옮길 때 사용</summary>
        public void MoveMonsterToNode(string monsterId, string targetNodeId)
        {
            var state = GetMonsterState(monsterId);
            if (state != null) state.currentNodeId = targetNodeId;
        }

        // ===== 스쿼드 파견 — 노드 정원/occupant 관리 =====

        private const int DefaultNodeCapacity = 1;
        private const int HighNodeCapacity = 3;

        /// <summary>이 노드의 최대 동시 입장 인원. highCapacityNodeIds에 있으면 3, 아니면 1</summary>
        public int GetNodeCapacity(string nodeId) =>
            highCapacityNodeIds.Contains(nodeId) ? HighNodeCapacity : DefaultNodeCapacity;

        /// <summary>이 노드에 지금 한 명 더 들어갈 수 있는지 (정원 체크)</summary>
        public bool CanAddOccupant(string nodeId) =>
            GetOrCreateNodeState(nodeId).occupants.Count < GetNodeCapacity(nodeId);

        /// <summary>정원 체크 없이 그대로 추가 — 호출부가 CanAddOccupant로 미리 확인했거나,
        /// 레이드 시작 시 최초 배치처럼 체크가 필요 없는 경우에 사용</summary>
        public void AddOccupant(string nodeId, string characterId, SlotRole role = SlotRole.Member)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            GetOrCreateNodeState(nodeId).occupants.Add(new RaidNodeOccupant { characterId = characterId, role = role });
        }

        public void RemoveOccupant(string nodeId, string characterId)
        {
            GetNodeState(nodeId)?.occupants.RemoveAll(o => o.characterId == characterId);
        }

        /// <summary>occupant를 한 노드에서 다른 노드로 옮긴다. 대상 노드 정원 체크는 호출부 책임.</summary>
        public void MoveOccupant(string characterId, string fromNodeId, string toNodeId)
        {
            var fromRole = GetNodeState(fromNodeId)?.occupants.FirstOrDefault(o => o.characterId == characterId)?.role ?? SlotRole.Member;
            RemoveOccupant(fromNodeId, characterId);
            AddOccupant(toNodeId, characterId, fromRole);
        }

        /// <summary>이 캐릭터가 현재 어느 노드에 있는지. 어디에도 없으면 null</summary>
        public string FindOccupantNode(string characterId) =>
            runtimeState.nodeStates.FirstOrDefault(s => s.occupants.Any(o => o.characterId == characterId))?.nodeId;

        /// <summary>성역 노드가 닫힐 때 그 노드에 있던 스쿼드원을 전부 대기 노드로 강제 이동</summary>
        public void EvacuateOccupants(string nodeId, string standbyNodeId)
        {
            var state = GetNodeState(nodeId);
            if (state == null || state.occupants.Count == 0) return;

            var toMove = new List<RaidNodeOccupant>(state.occupants);
            state.occupants.Clear();
            foreach (var occupant in toMove)
                AddOccupant(standbyNodeId, occupant.characterId, occupant.role);
        }
    }
}
