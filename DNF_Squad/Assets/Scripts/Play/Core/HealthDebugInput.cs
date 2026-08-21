using UnityEngine;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// HP/MP 증감 테스트용 임시 기능. 버튼 OnClick에서 아래 함수들을 직접 호출해서 사용한다.
    /// 실제 공격/자동 딜링 로직이 생기면 이 스크립트는 제거하거나 비활성화한다.
    /// </summary>
    public class HealthDebugInput : MonoBehaviour
    {
        [SerializeField] private HealthController healthController;
        [SerializeField] private MonsterSpawnController monsterSpawnController;
        [SerializeField] private int testAmount = 10;
        [SerializeField] private string testNodeId;

        /// <summary>현재 맵에 스폰된 몬스터에게 데미지</summary>
        public void DamageCurrentMapMonster()
        {
            if (monsterSpawnController.CurrentMonsterId != null)
                healthController.DamageMonster(monsterSpawnController.CurrentMonsterId, testAmount);
        }

        /// <summary>testNodeId로 지정한 노드의 몬스터에게 데미지</summary>
        public void DamageNodeMonster()
        {
            healthController.DamageMonsterAtNode(testNodeId, testAmount);
        }

        public void DamagePlayerHp()
        {
            healthController.DamagePlayer(testAmount);
        }

        public void DamagePlayerMp()
        {
            healthController.ChangePlayerMp(-testAmount);
        }

        public void HealPlayerHp()
        {
            healthController.HealPlayer(testAmount);
        }

        public void HealPlayerMp()
        {
            healthController.ChangePlayerMp(testAmount);
        }
    }
}
