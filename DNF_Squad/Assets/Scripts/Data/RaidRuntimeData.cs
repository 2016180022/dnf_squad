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
    }
}
