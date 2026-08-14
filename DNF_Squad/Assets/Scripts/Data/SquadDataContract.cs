using System.Collections.Generic;

namespace DnfSquad.Data
{
    // ========== 마스터 데이터 (엑셀 → XML → 이 클래스로 변환, 읽기 전용) ==========

    /// <summary>모험단 캐릭터 1명의 정보</summary>
    [System.Serializable]
    public class AdventurerCharacterData
    {
        public string characterId;      // 고유 ID (엑셀 export 시 고정 키로 사용)
        public string characterName;    // 캐릭터명
        public string jobName;          // 직업명
        public int gearScore;           // 장비점수
        public int fame;                // 명성
        public int remainingEntryCount; // 레이드 잔여 입장 횟수
        public string portraitImageId;  // 캐릭터 전체 이미지 리소스 키
    }

    /// <summary>스쿼드 스킬(공격 3종 + 지원 3종) 정의</summary>
    [System.Serializable]
    public class SquadSkillData
    {
        public string skillId;
        public string skillName;
        public SquadSkillType skillType;      // Attack / Support
        public float cooldownSeconds;
        public int maxUsesPerRaid;            // 레이드 내 최대 사용 횟수 (0 = 무제한)
        public string usableConditionNote;    // 사용 조건 설명 (임시 텍스트)
        // TODO: PlayScene 개발 시 환경 변수 기반 Flag/임계값 비교 로직으로 대체 예정.
        // 예: 특정 변수가 기준치 미만일 때만 사용 가능 등 — 지금은 상세 조건 미확정, 텍스트로만 기록.
        public string description;
    }

    /// <summary>스쿼드 버프(보조/강화 요소) 정의</summary>
    [System.Serializable]
    public class SquadBuffData
    {
        public string buffId;
        public string buffName;
        public SquadBuffCategory category;   // Assist(보조) / Enhance(강화)
        public int maxLevel;                 // 최대 3레벨
        public string descriptionTemplate;   // 예: "모든 겁화 증가량을 {0}% 감소시킨다" — {0} 자리를 UI에서 강조색 처리
        public SquadBuffLevelData[] levels;  // 레벨별 수치 및 포인트
    }

    /// <summary>스쿼드 버프의 레벨별 수치/비용/부가 효과</summary>
    [System.Serializable]
    public class SquadBuffLevelData
    {
        public int level;            // 1~3
        public float[] effectValues; // 템플릿의 {0}, {1}... 자리에 들어갈 수치 (예: 10 / 20 / 30)
        public int pointCost;        // 이 레벨 도달에 필요한 스쿼드 포인트
        public string bonusDescriptionTemplate; // 값이 있으면 해당 레벨 달성 시 Description에 추가 노출 (예: 3레벨 전용 부가 효과)
    }

    public enum SquadSkillType { Attack, Support }
    public enum SquadBuffCategory { Assist, Enhance }

    // ========== 런타임 상태 (SO가 감쌀 대상, 플레이어별로 값이 채워짐) ==========

    /// <summary>스쿼드 구성 - 리더/버퍼/멤버 배치 상태</summary>
    [System.Serializable]
    public class SquadComposition
    {
        public string leaderCharacterId;                     // 스쿼드 리더로 등록된 캐릭터 ID
        public string bufferCharacterId;                     // 스쿼드 버퍼로 등록된 캐릭터 ID
        public string[] memberCharacterIds = new string[2];  // 스쿼드 멤버 2명
    }

    /// <summary>퀵슬롯 1칸의 배치 정보 (총 6칸, 전부 자유 배치)</summary>
    [System.Serializable]
    public class QuickSlotAssignment
    {
        public int slotIndex;   // 0~5
        public string skillId;  // 배치된 스킬 ID
    }

    /// <summary>리더 스킬 체인의 한 스텝</summary>
    [System.Serializable]
    public class SkillChainStep
    {
        public int order;       // 몇 번째로 실행되는지 (0부터 시작)
        public string skillId;
    }
    // TODO: 스킬 체인 설정 기능 상세 구현 시 구조 재정리 예정 (사용자 확인 대기)

    /// <summary>보유 중인 스쿼드 버프의 레벨 진행 상태</summary>
    [System.Serializable]
    public class SquadBuffProgress
    {
        public string buffId;
        public int currentLevel; // 0 = 미보유, 1~3 = 레벨
    }

    /// <summary>SettingScene 전체가 다루는 런타임 데이터 묶음</summary>
    [System.Serializable]
    public class SquadRuntimeState
    {
        public SquadComposition composition = new SquadComposition();
        public List<QuickSlotAssignment> quickSlots = new List<QuickSlotAssignment>(6);
        public List<SkillChainStep> leaderSkillChain = new List<SkillChainStep>();
        public List<SquadBuffProgress> buffProgress = new List<SquadBuffProgress>();
        public int squadPoints; // 보유 스쿼드 포인트
    }
}
