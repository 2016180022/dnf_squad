using System.Collections.Generic;
using UnityEngine;

namespace DnfSquad.Data
{
    // ========== 마스터 데이터 (레이드 스크립트 설정, 읽기 전용) ==========

    /// <summary>몬스터의 등급 — 네임드 / 보스</summary>
    public enum MonsterTier { Named, Boss }

    /// <summary>현황판의 발판 1개 = 맵 1개에 대한 마스터 정보. 위치만 가지며,
    /// 지금 이 노드에 어떤 몬스터가 있는지는 몬스터 쪽 런타임 상태(currentNodeId)가 결정한다.</summary>
    [System.Serializable]
    public class RaidNodeData
    {
        public string nodeId;                 // 고유 ID
        public Vector2 boardPosition;         // 현황판 UI 상의 좌표
    }

    /// <summary>몬스터 마스터 데이터</summary>
    [System.Serializable]
    public class MonsterData
    {
        public string monsterId;
        public string monsterName;
        public MonsterTier tier;              // Named / Boss
        public int maxHp;
        [Tooltip("Resources/Image/Map/ 아래 파일명 (확장자 제외) — 이 몬스터가 있는 노드에 표시할 배경")]
        public string mapBackgroundImageId;
        [Tooltip("Resources/Prefab/Monster/ 아래 프리팹 파일명 (확장자 제외) — 애니메이터/스프라이트 렌더러가 포함된 몬스터 프리팹")]
        public string monsterPrefabId;
        [Tooltip("레이드 시작 시 이 몬스터가 배치될 노드 ID")]
        public string startingNodeId;
        [Tooltip("패턴 스크립트 연동 예정 — 지금은 자리만 확보")]
        public string patternScriptId;
    }

    // ========== 런타임 상태 (플레이 중 값이 채워짐) ==========

    /// <summary>발판에 입장해 있는 스쿼드원 1명</summary>
    [System.Serializable]
    public class RaidNodeOccupant
    {
        public string characterId;
        public SlotRole role; // Leader / Buffer / Member (기존 SlotRole 재사용)
    }

    /// <summary>발판 1개의 현재 진행 상태 — 누가 입장해 있는지만 관리 (몬스터 정보는 MonsterRuntimeState 쪽)</summary>
    [System.Serializable]
    public class RaidNodeRuntimeState
    {
        public string nodeId;
        public List<RaidNodeOccupant> occupants = new List<RaidNodeOccupant>();
    }

    /// <summary>몬스터 1마리의 현재 진행 상태. 계율의 사슬로 위치가 바뀌면 currentNodeId만 갱신하면 된다.</summary>
    [System.Serializable]
    public class MonsterRuntimeState
    {
        public string monsterId;
        public string currentNodeId;
        public int currentHp;
        public bool isDead;
    }

    /// <summary>현황판 전체의 런타임 상태</summary>
    [System.Serializable]
    public class RaidBoardRuntimeState
    {
        public List<RaidNodeRuntimeState> nodeStates = new List<RaidNodeRuntimeState>();
        public List<MonsterRuntimeState> monsterStates = new List<MonsterRuntimeState>();
    }
}
