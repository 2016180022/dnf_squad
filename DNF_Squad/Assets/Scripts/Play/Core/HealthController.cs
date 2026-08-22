using UnityEngine;
using DnfSquad.Data;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// 플레이어 HP/MP와 몬스터 체력 증감을 전담하는 코어 컨트롤러.
    /// 데미지/회복이 발생하는 모든 경로(플레이어 공격, 스쿼드원 자동 딜링 등)는
    /// 값을 직접 바꾸지 않고 이 컨트롤러의 함수를 통해서만 처리한다.
    /// 나중에 데미지 증감 효과(버프 등)를 추가할 때도 이 컨트롤러 내부만 손보면 된다.
    /// </summary>
    public class HealthController : MonoBehaviour
    {
        // 신규: 보스(우리엘/라파엘) 생존 중 미카엘라 체력 하한 비율(10%)
        private const float MichaelaHpFloorRatio = 0.1f;

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        // 신규: 미카엘라 10% 하한에 걸렸을 때 안내 문구 표시용
        [SerializeField] private GlobalWarningUI globalWarningUI;

        public int PlayerMaxHp => raidRuntimeData.playerMaxHp;
        public int PlayerMaxMp => raidRuntimeData.playerMaxMp;
        public int PlayerCurrentHp => raidRuntimeData.runtimeState.playerCurrentHp;
        public int PlayerCurrentMp => raidRuntimeData.runtimeState.playerCurrentMp;

        public void DamagePlayer(int amount)
        {
            var state = raidRuntimeData.runtimeState;
            state.playerCurrentHp = Mathf.Max(0, state.playerCurrentHp - amount);
        }

        public void HealPlayer(int amount)
        {
            var state = raidRuntimeData.runtimeState;
            state.playerCurrentHp = Mathf.Min(raidRuntimeData.playerMaxHp, state.playerCurrentHp + amount);
        }

        public void ChangePlayerMp(int amount)
        {
            var state = raidRuntimeData.runtimeState;
            state.playerCurrentMp = Mathf.Clamp(state.playerCurrentMp + amount, 0, raidRuntimeData.playerMaxMp);
        }

        public int GetMonsterCurrentHp(string monsterId) =>
            raidRuntimeData.GetMonsterState(monsterId)?.currentHp ?? 0;

        public int GetMonsterMaxHp(string monsterId) =>
            raidRuntimeData.GetMonster(monsterId)?.maxHp ?? 0;

        public void DamageMonster(string monsterId, int amount)
        {
            var state = raidRuntimeData.GetMonsterState(monsterId);
            if (state == null || state.isDead) return;

            int newHp = Mathf.Max(0, state.currentHp - amount);

            // 신규(규칙 2): 우리엘/라파엘 중 하나라도 살아있으면 미카엘라 체력은
            // 최대체력의 10% 밑으로 내려가지 않음(=처치 불가)
            var monster = raidRuntimeData.GetMonster(monsterId);
            if (monster != null && monster.tier == MonsterTier.Michaela && IsAnyBossAlive())
            {
                int hpFloor = Mathf.CeilToInt(monster.maxHp * MichaelaHpFloorRatio);
                if (newHp < hpFloor)
                {
                    newHp = hpFloor;
                    // 신규(버그 진단 보조): globalWarningUI 인스펙터 연결이 누락된 경우 조용히 무시되지 않고
                    // 콘솔에 남도록 함(연결이 안 돼 있어도 체력 클램프 자체는 계속 정상 동작해야 하므로 예외를 던지진 않음).
                    if (globalWarningUI != null)
                        globalWarningUI.ShowWarning("우리엘 또는 라파엘이 살아있을 경우에는\n미카엘라를 처치할 수 없습니다");
                    else
                        Debug.LogWarning("[HealthController] globalWarningUI가 연결되어 있지 않아 미카엘라 하한 경고를 표시할 수 없습니다. 인스펙터에서 연결해주세요.");
                }
            }

            state.currentHp = newHp;
            if (state.currentHp <= 0) state.isDead = true;
        }

        /// <summary>신규: 우리엘/라파엘(Boss 등급) 중 하나라도 살아있는지</summary>
        private bool IsAnyBossAlive()
        {
            foreach (var monster in raidRuntimeData.monsters)
            {
                if (monster.tier != MonsterTier.Boss) continue;
                var state = raidRuntimeData.GetMonsterState(monster.monsterId);
                if (state != null && !state.isDead) return true;
            }
            return false;
        }

        /// <summary>지정한 노드에 현재 있는 몬스터에게 데미지를 준다. 몬스터가 없으면 아무 일도 안 함.</summary>
        public void DamageMonsterAtNode(string nodeId, int amount)
        {
            var monster = raidRuntimeData.GetMonsterAtNode(nodeId);
            if (monster != null) DamageMonster(monster.monsterId, amount);
        }

        public void HealMonster(string monsterId, int amount)
        {
            var state = raidRuntimeData.GetMonsterState(monsterId);
            if (state == null || state.isDead) return;

            int maxHp = raidRuntimeData.GetMonster(monsterId)?.maxHp ?? state.currentHp;
            state.currentHp = Mathf.Min(maxHp, state.currentHp + amount);
        }
    }
}
