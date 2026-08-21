using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DnfSquad.Data;
using DnfSquad.Play.Core;

namespace DnfSquad.Play.Raid
{
    /// <summary>
    /// 현황판 UI — 발판 선택과 '진입' 처리를 담당한다 (레이드 기능).
    /// 1차 테스트 버전: 노드 4개를 인스펙터에서 직접 연결해서 사용.
    /// </summary>
    public class RaidBoardController : MonoBehaviour
    {
        [System.Serializable]
        public class NodeButtonBinding
        {
            public string nodeId;
            public Button button;
        }

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private GameObject boardCanvas;
        [SerializeField] private List<NodeButtonBinding> nodeButtons = new List<NodeButtonBinding>();
        [SerializeField] private Button enterButton;
        [SerializeField] private MapTransitionController mapTransitionController;

        [Header("투명도 조절")]
        [SerializeField] private CanvasGroup boardCanvasGroup;
        [SerializeField] private Slider opacitySlider;

        private string selectedNodeId;

        private void Start()
        {
            raidRuntimeData.InitializeRuntimeState();

            foreach (var binding in nodeButtons)
            {
                string nodeId = binding.nodeId; // 클로저 캡처용 로컬 변수
                binding.button.onClick.AddListener(() => SelectNode(nodeId));
            }

            enterButton.onClick.AddListener(EnterSelectedNode);

            if (opacitySlider != null && boardCanvasGroup != null)
            {
                opacitySlider.value = boardCanvasGroup.alpha;
                opacitySlider.onValueChanged.AddListener(SetBoardOpacity);
            }

            OpenBoard();
        }

        private void SetBoardOpacity(float value)
        {
            boardCanvasGroup.alpha = value;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.insertKey.wasPressedThisFrame)
            {
                ToggleBoard();
            }
        }

        public void OpenBoard()
        {
            selectedNodeId = null;
            EventSystem.current.SetSelectedGameObject(null);
            boardCanvas.SetActive(true);
        }

        /// <summary>Insert 키로 현황판을 열고 닫는다.</summary>
        public void ToggleBoard()
        {
            if (boardCanvas.activeSelf) boardCanvas.SetActive(false);
            else OpenBoard();
        }

        /// <summary>발판을 선택 상태로 표시한다. 버튼의 Selectable "Selected Color"를
        /// 그대로 활용하므로, 인스펙터에서 각 버튼의 Selected 색상을 원하는 하이라이트 색으로
        /// 지정해두면 별도 하이라이트 오브젝트 없이도 선택 표시가 된다.</summary>
        private void SelectNode(string nodeId)
        {
            selectedNodeId = nodeId;

            var binding = nodeButtons.FirstOrDefault(b => b.nodeId == nodeId);
            if (binding != null) EventSystem.current.SetSelectedGameObject(binding.button.gameObject);
        }

        private void EnterSelectedNode()
        {
            if (string.IsNullOrEmpty(selectedNodeId)) return;

            boardCanvas.SetActive(false);
            mapTransitionController.EnterNode(selectedNodeId);
        }
    }
}
