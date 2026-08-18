using UnityEngine;
using DnfSquad.Data;

namespace DnfSquad.Logic
{
    public static class SquadValidationService
    {
        // 버퍼 슬롯에만 배치 가능하고, 반대로 버퍼 슬롯에는 이 직업만 배치 가능한 전용 직업군
        private static readonly string[] BufferOnlyJobNames =
        {
            "진 뮤즈", "진 크루세이더", "진 인챈트리스", "진 패러메딕"
        };

        public static bool CanAssign(SquadComposition composition, SlotRole role, AdventurerCharacterData character, int requiredFame, out string errorMessage)
        {
            errorMessage = null;

            if (character.remainingEntryCount <= 0)
            {
                errorMessage = "입장 횟수가 소모된 캐릭터입니다";
                return false;
            }

            bool isBufferOnlyJob = System.Array.IndexOf(BufferOnlyJobNames, character.jobName) >= 0;
            if (role == SlotRole.Buffer && !isBufferOnlyJob)
            {
                errorMessage = "해당 직업은 버퍼 슬롯에 배치할 수 없습니다";
                return false;
            }
            if (role != SlotRole.Buffer && isBufferOnlyJob)
            {
                errorMessage = "해당 직업은 버퍼 슬롯에만 배치할 수 있습니다";
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