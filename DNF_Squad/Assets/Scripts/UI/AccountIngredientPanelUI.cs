using DnfSquad.Data;
using UnityEngine;

namespace DnfSquad.UI
{
    /// <summary>
    /// 모험단 재료 현황 팝업 (AccountIngredientPanel에 부착).
    /// IngredientListView 스크롤 뷰의 Content 아래에 캐릭터 수만큼 행을 생성한다.
    /// </summary>
    public class AccountIngredientPanelUI : MonoBehaviour
    {
        [SerializeField] private SquadRuntimeData squadData;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Transform listContent;      // IngredientListView > Viewport > Content
        [SerializeField] private IngredientListRowUI rowPrefab;

        /// <summary>버튼에 연결 — 열려 있으면 닫고, 닫혀 있으면 목록을 채워 연다</summary>
        public void Toggle()
        {
            if (panelRoot.activeSelf)
            {
                panelRoot.SetActive(false);
                return;
            }

            Populate();
            panelRoot.SetActive(true);
        }

        public void Close()
        {
            panelRoot.SetActive(false);
        }

        private void Populate()
        {
            foreach (Transform child in listContent) Destroy(child.gameObject);

            foreach (var character in squadData.adventurerCharacters)
            {
                var row = Instantiate(rowPrefab, listContent);
                row.Display(character, squadData.raidIngredientImageId);
            }
        }
    }
}
