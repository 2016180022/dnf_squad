using DnfSquad.Data;

namespace DnfSquad.Logic
{
    /// <summary>
    /// 스쿼드 파견 시스템 공용 로직 — R/Y/G 색상과 실제 캐릭터ID 매핑, 자동 딜링 수치 계산.
    /// 버퍼(bufferCharacterId)는 이 매핑에 포함하지 않는다 (파견 대상 아님, 스쿼드 스킬 쪽에서 별도 처리 예정).
    /// </summary>
    public static class SquadDispatchService
    {
        /// <summary>이 캐릭터가 R/Y/G 중 어디에 해당하는지. 버퍼거나 미배치면 null.</summary>
        public static SquadColor? GetColor(SquadComposition composition, string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return null;
            if (composition.leaderCharacterId == characterId) return SquadColor.R;
            if (composition.memberCharacterIds.Length > 0 && composition.memberCharacterIds[0] == characterId) return SquadColor.Y;
            if (composition.memberCharacterIds.Length > 1 && composition.memberCharacterIds[1] == characterId) return SquadColor.G;
            return null;
        }

        /// <summary>이 색상에 배치된 캐릭터ID. 미배치면 null.</summary>
        public static string GetCharacterId(SquadComposition composition, SquadColor color)
        {
            switch (color)
            {
                case SquadColor.R: return composition.leaderCharacterId;
                case SquadColor.Y: return composition.memberCharacterIds.Length > 0 ? composition.memberCharacterIds[0] : null;
                case SquadColor.G: return composition.memberCharacterIds.Length > 1 ? composition.memberCharacterIds[1] : null;
                default: return null;
            }
        }

        /// <summary>자동 딜링 — 1초당 데미지(장비점수 기반). 테스트 후 계수(1/100000) 조정 예정, 1초 주기 자체는 고정.</summary>
        public static float GetAutoDamagePerSecond(int gearScore) => gearScore / 100000f;
    }
}
