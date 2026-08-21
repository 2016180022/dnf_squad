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

        // ===== 초기화 =====

        /// <summary>플레이 씬 시작 시 마스터 데이터 기준으로 런타임 상태를 새로 채운다</summary>
        public void InitializeRuntimeState()
        {
            runtimeState = new RaidBoardRuntimeState
            {
                playerCurrentHp = playerMaxHp,
                playerCurrentMp = playerMaxMp
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
