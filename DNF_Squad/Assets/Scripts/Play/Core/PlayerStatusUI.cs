using UnityEngine;

namespace DnfSquad.Play.Core
{
    /// <summary>플레이어 HP/MP 게이지를 HealthController 값에 맞춰 매 프레임 갱신한다.</summary>
    public class PlayerStatusUI : MonoBehaviour
    {
        [SerializeField] private HealthController healthController;
        [SerializeField] private ValueGaugeUI hpGauge;
        [SerializeField] private ValueGaugeUI mpGauge;

        private void Update()
        {
            hpGauge.SetRatio(healthController.PlayerCurrentHp, healthController.PlayerMaxHp);
            mpGauge.SetRatio(healthController.PlayerCurrentMp, healthController.PlayerMaxMp);
        }
    }
}
