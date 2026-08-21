using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 현황판 노드 외형 프리팹(EmptyNode/NamedNode/BossNode) 루트에 붙는 컴포넌트.
    /// 프리팹 내부 구조(하이라이트, 체력 게이지)를 스스로 캡슐화한다.
    /// RaidBoardController는 이 컴포넌트를 통해서만 상태를 전달하고, 내부 오브젝트를 직접 뒤지지 않는다.
    /// </summary>
    public class NodeVisualPrefab : MonoBehaviour
    {
        [Tooltip("선택됐을 때 켤 하이라이트 오브젝트")]
        [SerializeField] private GameObject highlightObject;
        [Tooltip("Named/Boss만 보유. Empty는 비워둬도 됨")]
        [SerializeField] private Core.ValueGaugeUI hpGauge;
        [Tooltip("Named/Boss만 보유. 이 노드에 있는 몬스터 아이콘")]
        [SerializeField] private Image bossIcon;

        public Core.ValueGaugeUI HpGauge => hpGauge;

        public void SetHighlighted(bool highlighted)
        {
            if (highlightObject != null) highlightObject.SetActive(highlighted);
        }

        /// <summary>이 노드에 있는 몬스터 id에 맞는 아이콘을 Resources/Image/MonsterIcon/icon_{monsterId}에서 로드해 표시한다</summary>
        public void SetMonsterIcon(string monsterId)
        {
            if (bossIcon == null) return;

            Sprite icon = Resources.Load<Sprite>($"Image/MonsterIcon/icon_{monsterId}");
            if (icon == null)
            {
                Debug.LogWarning($"[NodeVisualPrefab] 몬스터 아이콘을 찾을 수 없음: Image/MonsterIcon/icon_{monsterId}");
                return;
            }
            bossIcon.sprite = icon;
        }
    }
}
