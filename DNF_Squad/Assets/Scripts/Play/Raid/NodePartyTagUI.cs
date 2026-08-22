using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 노드 프리팹 내부 "PartyTag" — 파견/계율의 사슬 명령 버튼 6개를 캡슐화한다 (순수 뷰, 로직 없음).
    /// 어떤 색을 눌렀을 때 무슨 일이 일어나는지는 전혀 모르고, 버튼 참조와 표시/활성화 여부만 다룬다.
    /// 실제 클릭 동작 연결과 상태 판단은 SquadController(Play.Squad)가 스폰 시점에 주입한다
    /// (Play.Raid가 Play.Squad를 몰라도 되도록 — 단방향 의존 유지).
    /// </summary>
    public class NodePartyTagUI : MonoBehaviour
    {
        [Header("윗줄 — 계율의 사슬")]
        [SerializeField] private Button chainRButton;
        [SerializeField] private Button chainYButton;
        [SerializeField] private Button chainGButton;

        [Header("아랫줄 — 파견 (Y는 파견이 아니라 플레이어 직접 이동)")]
        [SerializeField] private Button dispatchRButton;
        [SerializeField] private Button dispatchYButton;
        [SerializeField] private Button dispatchGButton;

        public Button ChainRButton => chainRButton;
        public Button ChainYButton => chainYButton;
        public Button ChainGButton => chainGButton;
        public Button DispatchRButton => dispatchRButton;
        public Button DispatchYButton => dispatchYButton;
        public Button DispatchGButton => dispatchGButton;

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void SetInteractable(bool chainR, bool chainY, bool chainG, bool dispatchR, bool dispatchY, bool dispatchG)
        {
            chainRButton.interactable = chainR;
            chainYButton.interactable = chainY;
            chainGButton.interactable = chainG;
            dispatchRButton.interactable = dispatchR;
            dispatchYButton.interactable = dispatchY;
            dispatchGButton.interactable = dispatchG;
        }
    }
}
