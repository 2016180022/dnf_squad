using UnityEngine;
using DnfSquad.Data;

namespace DnfSquad.Logic
{
    public static class SquadValidationService
    {
        public static bool CanAssign(SquadComposition composition, SlotRole role, AdventurerCharacterData character, int requiredFame, out string errorMessage)
        {
            errorMessage = null;

            if (character.remainingEntryCount <= 0)
            {
                errorMessage = "입장 횟수가 소모된 캐릭터입니다";
                return false;
            }

            if (character.fame < requiredFame)
            {
                errorMessage = role == SlotRole.Leader
                    ? "스쿼드 리더 명성 조건을 만족하지 않습니다"
                    : "입장 명성 조건을 만족하지 않습니다";
                return false;
            }

            if (IsAlreadyAssigned(composition, character.characterId))
            {
                errorMessage = "이미 스쿼드에 배치된 캐릭터입니다";
                return false;
            }

            return true;
        }

        private static bool IsAlreadyAssigned(SquadComposition comp, string characterId)
        {
            if (comp.leaderCharacterId == characterId) return true;
            if (comp.bufferCharacterId == characterId) return true;
            foreach (var memberId in comp.memberCharacterIds)
                if (memberId == characterId) return true;
            return false;
        }
    }
}