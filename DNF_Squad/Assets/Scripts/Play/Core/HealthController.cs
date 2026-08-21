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
        [SerializeField] private RaidRuntimeData raidRuntimeData;

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

            state.currentHp = Mathf.Max(0, state.currentHp - amount);
            if (state.currentHp <= 0) state.isDead = true;
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
