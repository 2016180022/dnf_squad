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

        // ===== 초기화 =====

        /// <summary>새 세션 시작 시 런타임 상태를 비움 (마스터 데이터는 유지)</summary>
        public void ResetRuntimeState()
        {
            runtimeState = new SquadRuntimeState();
        }
    }
}
