using TMPro;
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
        [Tooltip("미카엘라만 보유. 위치 고정 카운트다운 게이지")]
        [SerializeField] private Core.ValueGaugeUI luminousGauge;
        [Tooltip("Named/Boss만 보유. 이 노드에 있는 몬스터 아이콘")]
        [SerializeField] private Image bossIcon;
        [Tooltip("성역 카운트다운 표시 오브젝트 (평소엔 꺼둠). 상시 활성 노드(BossNode/StandbyNode)는 " +
            "RaidBoardController가 항상 -1을 넘겨서 꺼둔다")]
        [SerializeField] private GameObject sanctuaryTimerDisplay;
        [SerializeField] private TMP_Text sanctuaryTimerText;
        [Tooltip("파견/계율의 사슬 명령 버튼 6개 — 모든 노드 프리팹(Empty/Named/Boss)에 배치, 선택 시에만 표시")]
        [SerializeField] private NodePartyTagUI partyTag;
        [Tooltip("현재 이 노드에 있는 스쿼드원 R/Y/G 표시 — 모든 노드 프리팹에 3개 다 배치")]
        [SerializeField] private NodeOccupantTagUI nowTag;

        private const int SanctuaryTimerShowThresholdSec = 20;

        public Core.ValueGaugeUI HpGauge => hpGauge;
        public Core.ValueGaugeUI LuminousGauge => luminousGauge;
        public NodePartyTagUI PartyTag => partyTag;
        public NodeOccupantTagUI NowTag => nowTag;

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

        /// <summary>성역 카운트다운 표시. remainingSec가 0 이하이거나 20초 초과면 꺼진다.</summary>
        public void SetSanctuaryTimer(int remainingSec)
        {
            bool show = remainingSec > 0 && remainingSec <= SanctuaryTimerShowThresholdSec;
            if (sanctuaryTimerDisplay != null) sanctuaryTimerDisplay.SetActive(show);
            if (show && sanctuaryTimerText != null) sanctuaryTimerText.text = remainingSec.ToString();
        }
    }
}
