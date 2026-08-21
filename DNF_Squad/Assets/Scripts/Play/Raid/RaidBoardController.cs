using System.Collections.Generic;
using UnityEngine;
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
            OpenBoard();
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
            boardCanvas.SetActive(true);
        }

        /// <summary>Insert 키로 현황판을 열고 닫는다.</summary>
        public void ToggleBoard()
        {
            if (boardCanvas.activeSelf) boardCanvas.SetActive(false);
            else OpenBoard();
        }

        private void SelectNode(string nodeId)
        {
            selectedNodeId = nodeId;
        }

        private void EnterSelectedNode()
        {
            if (string.IsNullOrEmpty(selectedNodeId)) return;

            boardCanvas.SetActive(false);
            mapTransitionController.EnterNode(selectedNodeId);
        }
    }
}
