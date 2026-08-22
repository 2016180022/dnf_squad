using UnityEngine;
using DnfSquad.Play.Core;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 규칙 1(2026-08-22 추가): 플레이어가 몬스터와 조우 중일 때(현재 화면에 몬스터가 있을 때)
    /// 1초마다 몬스터 체력 1, 플레이어 체력 1, 마나 1을 자동으로 깎는다.
    /// 실제 공격/몬스터 패턴 시스템이 들어오기 전까지의 임시 처리.
    /// </summary>
    public class EncounterTickController : MonoBehaviour
    {
        private const float TickInterval = 1f;
        private const int MonsterDamagePerTick = 1;
        private const int PlayerHpLossPerTick = 1;
        private const int PlayerMpLossPerTick = 1;

        [SerializeField] private HealthController healthController;
        [SerializeField] private MonsterSpawnController monsterSpawnController;

        private float tickTimer;

        private void Update()
        {
            string monsterId = monsterSpawnController.CurrentMonsterId;
            if (string.IsNullOrEmpty(monsterId))
            {
                tickTimer = 0f; // 조우 중이 아니면 타이머 리셋
                return;
            }

            tickTimer += Time.deltaTime;
            while (tickTimer >= TickInterval)
            {
                tickTimer -= TickInterval;
                healthController.DamageMonster(monsterId, MonsterDamagePerTick);
                healthController.DamagePlayer(PlayerHpLossPerTick);
                healthController.ChangePlayerMp(-PlayerMpLossPerTick);
            }
        }
    }
}
