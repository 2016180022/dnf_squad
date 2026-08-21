using TMPro;
using UnityEngine;

namespace DnfSquad.Play.Core
{
    /// <summary>현재 맵에 스폰된 몬스터의 체력 게이지와 이름을 갱신한다. 몬스터가 없으면 숨긴다.</summary>
    public class MapMonsterHealthUI : MonoBehaviour
    {
        [SerializeField] private HealthController healthController;
        [SerializeField] private MonsterSpawnController monsterSpawnController;
        [SerializeField] private GameObject gaugeRoot;
        [SerializeField] private ValueGaugeUI hpGauge;
        [SerializeField] private TMP_Text monsterNameText;

        private void Update()
        {
            string monsterId = monsterSpawnController.CurrentMonsterId;
            bool hasMonster = !string.IsNullOrEmpty(monsterId);

            if (gaugeRoot != null) gaugeRoot.SetActive(hasMonster);
            if (!hasMonster) return;

            hpGauge.SetRatio(healthController.GetMonsterCurrentHp(monsterId), healthController.GetMonsterMaxHp(monsterId));
            if (monsterNameText != null) monsterNameText.text = monsterSpawnController.CurrentMonsterName;
        }
    }
}
