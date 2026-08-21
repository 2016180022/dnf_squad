using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Core
{
    /// <summary>현재 맵에 스폰된 몬스터의 체력 게이지와 이름, 아이콘을 갱신한다. 몬스터가 없으면 숨긴다.</summary>
    public class MapMonsterHealthUI : MonoBehaviour
    {
        [SerializeField] private HealthController healthController;
        [SerializeField] private MonsterSpawnController monsterSpawnController;
        [SerializeField] private GameObject gaugeRoot;
        [SerializeField] private ValueGaugeUI hpGauge;
        [SerializeField] private TMP_Text monsterNameText;
        [Tooltip("현황판 아이콘(icon_{id})과 별도로 _box가 붙은 사각형 버전을 사용")]
        [SerializeField] private Image monsterIcon;

        // 몬스터가 안 바뀌었으면 매 프레임 Resources.Load 하지 않기 위한 캐시
        private string lastIconMonsterId;

        private void Update()
        {
            string monsterId = monsterSpawnController.CurrentMonsterId;
            bool hasMonster = !string.IsNullOrEmpty(monsterId);

            if (gaugeRoot != null) gaugeRoot.SetActive(hasMonster);
            if (!hasMonster)
            {
                lastIconMonsterId = null;
                return;
            }

            hpGauge.SetRatio(healthController.GetMonsterCurrentHp(monsterId), healthController.GetMonsterMaxHp(monsterId));
            if (monsterNameText != null) monsterNameText.text = monsterSpawnController.CurrentMonsterName;

            if (monsterId != lastIconMonsterId)
            {
                lastIconMonsterId = monsterId;
                SetMonsterIcon(monsterId);
            }
        }

        /// <summary>Resources/Image/MonsterIcon/icon_{monsterId}_box 스프라이트를 로드해 표시한다 (현황판용 icon_{id}와는 별개 파일)</summary>
        private void SetMonsterIcon(string monsterId)
        {
            if (monsterIcon == null) return;

            Sprite icon = Resources.Load<Sprite>($"Image/MonsterIcon/icon_{monsterId}_box");
            if (icon == null)
            {
                Debug.LogWarning($"[MapMonsterHealthUI] 몬스터 아이콘을 찾을 수 없음: Image/MonsterIcon/icon_{monsterId}_box");
                return;
            }
            monsterIcon.sprite = icon;
        }
    }
}
