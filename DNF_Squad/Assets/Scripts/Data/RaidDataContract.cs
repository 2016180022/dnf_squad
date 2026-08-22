using System.Collections.Generic;
using UnityEngine;

namespace DnfSquad.Data
{
    // ========== 마스터 데이터 (레이드 스크립트 설정, 읽기 전용) ==========

    /// <summary>몬스터의 등급 — 네임드 / 보스(우리엘·라파엘) / 미카엘라</summary>
    public enum MonsterTier { Named, Boss, Michaela }

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
        [Tooltip("이 몬스터가 소환될 때 배치될 월드 좌표")]
        public Vector3 spawnPosition;
        [Tooltip("패턴 스크립트 연동 예정 — 지금은 자리만 확보")]
        public string patternScriptId;
    }

    /// <summary>성역(빛의 고리) 시스템 — 노드 하나의 활성 구간 하나(초 단위, 확정값을 그대로 저장).
    /// 좌표/고리 파라미터로 매번 역산하지 않고, 이미 확정된 값을 그대로 데이터로 들고 있는 방식.
    /// 고리가 레이드 중 여러 바퀴 돌 수 있어서, 같은 nodeId가 여러 구간(항목)을 가질 수 있다.</summary>
    [System.Serializable]
    public class SanctuaryNodeWindow
    {
        public string nodeId;
        public int openSec;   // 이 초부터 활성
        public int closeSec;  // 이 초까지 활성 (경과시간이 closeSec+1이 되는 순간 강제 퇴장 판정)
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
        // TODO(스쿼드 멤버 강제 퇴장): 성역 노드가 닫힐 때 occupants를 전부 대기 노드로 옮기는 처리가
        // 필요함 (SanctuaryController.OnNodeClosed 참고). occupant 입장 UI를 만들 때 함께 구현 예정 —
        // 예: RaidRuntimeData.EvacuateOccupants(nodeId, standbyNodeId) 같은 메서드 추가.
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
        public int playerCurrentHp;
        public int playerCurrentMp;
        public int luminousGauge; // 성광 유지율(레이드 제한시간) 런타임 값
    }
}
