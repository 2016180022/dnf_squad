using UnityEngine;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 노드 프리팹 내부 "NowTag" — 지금 이 노드에 있는 스쿼드원의 R/Y/G 표시를 캡슐화한다 (순수 뷰).
    /// LayoutGroup이 이미 세팅되어 있으므로 Active 여부만 켜고 끄면 자동 정렬된다.
    /// 정원과 무관하게 모든 노드 프리팹에 R/Y/G 3개를 다 배치해두면 됨(스탠바이/보스 노드도 동일).
    /// </summary>
    public class NodeOccupantTagUI : MonoBehaviour
    {
        [SerializeField] private GameObject redTag;
        [SerializeField] private GameObject yellowTag;
        [SerializeField] private GameObject greenTag;

        public void SetActiveColors(bool r, bool y, bool g)
        {
            redTag.SetActive(r);
            yellowTag.SetActive(y);
            greenTag.SetActive(g);
        }
    }
}
