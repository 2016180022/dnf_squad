using System.Collections.Generic;
using UnityEngine;

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
        public int ingredientCount;     // 이 캐릭터가 보유 중인 레이드 재료 개수
        // portraitImageId 삭제 — Resources/Image/Portrait/{characterId} 경로로 characterId를 그대로 사용해 로드
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
        // description → descriptionTemplate + baseValues 로 분리 (버프로 수치가 변경되므로)
        public string descriptionTemplate; // 예: "스쿼드 리더가 등장하여 스킬 체인대로 공격한다\n쿨타임 {0}초"
        public float[] baseValues;         // 템플릿의 {0}, {1}... 자리에 들어갈 버프 미적용 기본 수치

        // ===== PlayScene 구현용 (표시와 무관, 스킬 사용 시 실제 동작을 결정) =====
        public SquadSkillSpawnAnchor spawnAnchor; // 스킬 사용 시 리더/버퍼가 등장할 위치
        public SquadSkillEffectType effectType;   // 실제로 어떤 효과를 낼지 (None = 아직 미구현, 발동 자체를 하지 않음)
    }

    /// <summary>스쿼드 버프(보조/강화 요소) 정의</summary>
    [System.Serializable]
    public class SquadBuffData
    {
        public string buffId;
        public string buffName;
        public SquadBuffCategory category;   // Assist(보조) / Enhance(강화)
        public SquadBuffSlotType slotType;   // Active(액티브) / Passive(패시브)
        public string descriptionTemplate;   // 예: "모든 겁화 증가량을 {0}% 감소시킨다" — {0} 자리를 UI에서 강조색 처리
        public SquadBuffLevelData[] levels;  // 레벨별 수치 및 부가 효과
        public SquadBuffEffect[] effects;    // 이 버프가 어떤 스킬의 무엇을 바꾸는지
        // maxLevel 삭제 — 모든 버프가 동일하므로 SquadRuntimeData.buffLevelConfig에서 공용 관리
    }

    /// <summary>버프가 스쿼드 스킬에 적용하는 효과 1건</summary>
    [System.Serializable]
    public class SquadBuffEffect
    {
        public string targetSkillId;        // 영향을 줄 스킬
        public SkillTargetType targetType;  // 무엇을 바꾸는지
        [Tooltip("DescriptionValue일 때만 사용 — 스킬 baseValues의 인덱스")]
        public int targetValueIndex;
        [Tooltip("이 버프 effectValues의 몇 번째 수치를 쓸지")]
        public int effectValueIndex;
        public BuffOperation operation;
    }

    /// <summary>
    /// 모든 스쿼드 버프가 공유하는 레벨 설정.
    /// 최대 레벨과 레벨업 비용은 버프별로 다르지 않으므로 한 곳에서 관리한다.
    /// </summary>
    [System.Serializable]
    public class SquadBuffLevelConfig
    {
        public int maxLevel = 3;
        [Tooltip("인덱스 0 = 1레벨 도달 비용, 1 = 2레벨 도달 비용, ...")]
        public int[] pointCosts = new int[3];

        /// <summary>currentLevel에서 다음 레벨로 올릴 때 드는 비용</summary>
        public int GetLevelUpCost(int currentLevel)
        {
            if (pointCosts == null || currentLevel < 0 || currentLevel >= pointCosts.Length) return 0;
            return pointCosts[currentLevel];
        }

        /// <summary>currentLevel까지 투입한 포인트 총합 (초기화 시 반환량)</summary>
        public int GetSpentPoints(int currentLevel)
        {
            if (pointCosts == null) return 0;

            int sum = 0;
            for (int i = 0; i < currentLevel && i < pointCosts.Length; i++) sum += pointCosts[i];
            return sum;
        }
    }

    /// <summary>스쿼드 버프의 레벨별 수치/부가 효과</summary>
    [System.Serializable]
    public class SquadBuffLevelData
    {
        public int level;            // 1~3
        public float[] effectValues; // 템플릿의 {0}, {1}... 자리에 들어갈 수치 (예: 10 / 20 / 30)
        // TODO: 버프→스킬 연동 작업 시 스킬 Desc 데이터로 이관 예정.
        // 현재는 버프 Desc에 레벨 무관 항상 표시되는 안내 문구로 사용 중.
        public string bonusDescriptionTemplate;
        // pointCost 삭제 — SquadRuntimeData.buffLevelConfig.pointCosts에서 공용 관리
    }

    public enum SquadSkillType { Attack, Support }
    public enum SquadSkillSpawnAnchor { NearBoss, NearPlayer } // 스킬 사용 시 리더/버퍼가 등장할 위치
    public enum SquadSkillEffectType { None, LeaderChainDamage, HealPlayerPercent } // None = 아직 미구현
    public enum SquadBuffCategory { Assist, Enhance }
    public enum SquadBuffSlotType { Active, Passive }

    /// <summary>버프가 스킬의 무엇을 대상으로 하는지</summary>
    public enum SkillTargetType
    {
        DescriptionValue, // 설명문 수치 (baseValues[targetValueIndex])
        Cooldown,         // 쿨타임 초
        MaxUses,          // 레이드 내 최대 사용 횟수
        SkillLevel        // 스킬 레벨 표기 (기본 1)
    }

    public enum BuffOperation { Add, Subtract, Multiply, Override }

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

    /// <summary>
    /// 프로토타입 전용 — 리더 캐릭터 스킬의 키 바인딩.
    /// 실제 인게임에서는 리더 캐릭터 접속 시 그 캐릭터의 스킬 배치를 그대로 가져오지만,
    /// 프로토타입에서는 A/S/D/F 4키에 임의 매핑해서 사용한다.
    /// </summary>
    [System.Serializable]
    public class LeaderSkillBinding
    {
        public KeyCode key;         // A / S / D / F
        public string skillId;      // 고유 ID
        public string skillName;    // 표시용 이름
    }

    /// <summary>보유 중인 스쿼드 버프의 레벨 진행 상태</summary>
    [System.Serializable]
    public class SquadBuffProgress
    {
        public string buffId;
        public int currentLevel; // 0 = 미보유, 1~3 = 레벨
    }

    /// <summary>
    /// 스쿼드 스킬 1개의 남은 쿨타임. PlayScene 전용 — 세이브 대상인 SquadRuntimeState와는 별개로
    /// SquadRuntimeData에 직접 둔다 (레이드 중에만 의미 있는 값이라 저장/복원 대상이 아님).
    /// </summary>
    [System.Serializable]
    public class SquadSkillCooldownState
    {
        public string skillId;
        public float remainingSeconds;
    }

    /// <summary>SettingScene 전체가 다루는 런타임 데이터 묶음</summary>
    [System.Serializable]
    public class SquadRuntimeState
    {
        public SquadComposition composition = new SquadComposition();
        public List<QuickSlotAssignment> quickSlots = new List<QuickSlotAssignment>(6);
        public List<SkillChainStep> leaderSkillChain = new List<SkillChainStep>();
        public long leaderSkillChainTotalDamage; // 스킬 체인 측정 결과 총합 데미지 (0 = 미설정)
        public List<SquadBuffProgress> buffProgress = new List<SquadBuffProgress>();
        public int squadPoints; // 보유 스쿼드 포인트
    }
}
