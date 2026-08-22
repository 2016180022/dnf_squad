using UnityEngine;
using DnfSquad.Play.Core;

namespace DnfSquad.Play.Raid
{
    /// <summary>성광 유지율 게이지를 LuminousGaugeController 값에 맞춰 매 프레임 갱신한다.</summary>
    public class LuminousGaugeUI : MonoBehaviour
    {
        [SerializeField] private LuminousGaugeController luminousGaugeController;
        [SerializeField] private ValueGaugeUI gauge;

        private void Update()
        {
            gauge.SetRatio(luminousGaugeController.CurrentLuminousGauge, luminousGaugeController.MaxLuminousGauge);
        }
    }
}
