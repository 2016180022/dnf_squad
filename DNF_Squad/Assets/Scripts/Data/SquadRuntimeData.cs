using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DnfSquad.Data
{
    [CreateAssetMenu(fileName = "SquadRuntimeData", menuName = "DnfSquad/Squad Runtime Data")]
    public class SquadRuntimeData : ScriptableObject
    {
        [Header("마스터 데이터 (XML 로더가 채움, 읽기 전용)")]
        public List<AdventurerCharacterData> adventurerCharacters = new List<AdventurerCharacterData>();
        public List<SquadSkillData> squadSkills = new List<SquadSkillData>();
        public List<SquadBuffData> squadBuffs = new List<SquadBuffData>();

        [Header("스쿼드 버프 공용 레벨 설정 (모든 버프가 동일하게 사용)")]
        public SquadBuffLevelConfig buffLevelConfig = new SquadBuffLevelConfig();

        [Header("아이템 리소스")]
        [Tooltip("Resources/Image/Item/ 아래의 파일명 (확장자 제외)")]
        public string raidIngredientImageId = "RaidIngredient";

        [Header("런타임 상태 (플레이어가 세팅, 저장/복원 대상)")]
        public SquadRuntimeState runtimeState = new SquadRuntimeState();

        // ===== 조회 헬퍼 =====

        public AdventurerCharacterData GetCharacter(string characterId)
        {
            return adventurerCharacters.FirstOrDefault(c => c.characterId == characterId);
        }

        public SquadSkillData GetSkill(string skillId)
        {
            return squadSkills.FirstOrDefault(s => s.skillId == skillId);
        }

        public SquadBuffData GetBuff(string buffId)
        {
            return squadBuffs.FirstOrDefault(b => b.buffId == buffId);
        }

        // ===== 버프 진행 상태 헬퍼 =====

        /// <summary>해당 버프의 현재 레벨 (0 = 미습득)</summary>
        public int GetBuffLevel(string buffId)
        {
            var progress = runtimeState.buffProgress.FirstOrDefault(p => p.buffId == buffId);
            return progress?.currentLevel ?? 0;
        }

        /// <summary>해당 버프의 레벨을 설정. 항목이 없으면 새로 추가한다.</summary>
        public void SetBuffLevel(string buffId, int level)
        {
            var progress = runtimeState.buffProgress.FirstOrDefault(p => p.buffId == buffId);
            if (progress == null)
            {
                progress = new SquadBuffProgress { buffId = buffId };
                runtimeState.buffProgress.Add(progress);
            }
            progress.currentLevel = level;
        }

        // ===== 초기화 =====

        /// <summary>새 세션 시작 시 런타임 상태를 비움 (마스터 데이터는 유지)</summary>
        public void ResetRuntimeState()
        {
            runtimeState = new SquadRuntimeState();
        }
    }
}
