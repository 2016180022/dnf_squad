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
    /// <summary>노드 외형 상태 — 비어있음 / 네임드 / 보스. None은 최초 1회 강제 갱신용 초기값.</summary>
    public enum NodeVisualState { None, EmptyNode, NamedNode, BossNode }

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

            // 노드 외형 프리팹은 button의 Transform 아래에 바로 생성한다 —
            // 발판 위치 = 버튼 위치라서 별도 위치 지정이 필요 없음
            [System.NonSerialized] public GameObject spawnedVisual;
            // 프리팹 내부(하이라이트, 체력 게이지)는 NodeVisualPrefab이 캡슐화 — 여기서 직접 뒤지지 않음
            [System.NonSerialized] public NodeVisualPrefab spawnedVisualPrefab;
            [System.NonSerialized] public NodeVisualState currentVisualState = NodeVisualState.None;
        }

        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private GameObject boardCanvas;
        [SerializeField] private List<NodeButtonBinding> nodeButtons = new List<NodeButtonBinding>();
        [SerializeField] private Button enterButton;
        [SerializeField] private MapTransitionController mapTransitionController;
        [SerializeField] private LuminousGaugeController luminousGaugeController;

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

            RefreshNodes();
        }

        private void RefreshNodes()
        {
            foreach (var binding in nodeButtons)
            {
                var monster = raidRuntimeData.GetMonsterAtNode(binding.nodeId);
                // 미카엘라도 보스 노드 프리팹(체력 게이지 + 성광 게이지)을 그대로 쓰므로 여기서만 Boss/Michaela를 함께 취급
                NodeVisualState state = monster == null ? NodeVisualState.EmptyNode
                    : (monster.tier == MonsterTier.Boss || monster.tier == MonsterTier.Michaela)
                        ? NodeVisualState.BossNode : NodeVisualState.NamedNode;

                RefreshNodeVisual(binding, state, monster);
                RefreshNodeMonsterHp(binding, monster);
            }
        }

        /// <summary>노드 외형 상태가 바뀌었을 때만 프리팹을 교체한다 (매 프레임 재생성 방지)</summary>
        private void RefreshNodeVisual(NodeButtonBinding binding, NodeVisualState state, MonsterData monster)
        {
            if (binding.currentVisualState == state) return;
            binding.currentVisualState = state;

            if (binding.spawnedVisual != null) Destroy(binding.spawnedVisual);
            binding.spawnedVisualPrefab = null;

            GameObject prefab = Resources.Load<GameObject>($"Prefab/Node/{state}");
            if (prefab == null)
            {
                Debug.LogWarning($"[RaidBoardController] 노드 외형 프리팹을 찾을 수 없음: Prefab/Node/{state}");
                return;
            }
            binding.spawnedVisual = Instantiate(prefab, binding.button.transform);
            binding.spawnedVisualPrefab = binding.spawnedVisual.GetComponent<NodeVisualPrefab>();
            // 방금 프리팹이 바뀌었으니 현재 선택 상태를 새로 스폰된 프리팹에도 즉시 반영
            binding.spawnedVisualPrefab?.SetHighlighted(binding.nodeId == selectedNodeId);
            // Named/Boss로 바뀐 경우, 그 노드에 있는 몬스터 아이콘을 동기화
            if (monster != null) binding.spawnedVisualPrefab?.SetMonsterIcon(monster.monsterId);
        }

        /// <summary>이번에 스폰된 노드 프리팹 안에 체력 게이지가 있으면(=네임드/보스) 값을 갱신하고, 미카엘라 노드면 위치 고정 카운트다운 게이지도 갱신한다</summary>
        private void RefreshNodeMonsterHp(NodeButtonBinding binding, MonsterData monster)
        {
            if (monster == null) return;

            var hpGauge = binding.spawnedVisualPrefab?.HpGauge;
            if (hpGauge != null)
            {
                int currentHp = raidRuntimeData.GetMonsterState(monster.monsterId)?.currentHp ?? 0;
                hpGauge.SetRatio(currentHp, monster.maxHp);
            }

            // 미카엘라 노드에만 존재하는 위치 고정 카운트다운 게이지 갱신
            if (monster.tier == MonsterTier.Michaela)
            {
                binding.spawnedVisualPrefab?.LuminousGauge?.SetRatio(
                    luminousGaugeController.FixedPositionCountdownRemaining,
                    luminousGaugeController.FixedPositionCountdownMax);
            }
            // 보스(우리엘/라파엘) 노드는 같은 게이지 슬롯을 비점유 카운트다운으로 갱신
            else if (monster.tier == MonsterTier.Boss)
            {
                binding.spawnedVisualPrefab?.LuminousGauge?.SetRatio(
                    luminousGaugeController.GetBossUnoccupiedCountdownRemaining(monster.monsterId),
                    luminousGaugeController.BossUnoccupiedCountdownMax);
            }
        }

        public void OpenBoard()
        {
            selectedNodeId = null;
            RefreshHighlights();
            EventSystem.current.SetSelectedGameObject(null);
            boardCanvas.SetActive(true);
        }

        /// <summary>Insert 키로 현황판을 열고 닫는다.</summary>
        public void ToggleBoard()
        {
            if (boardCanvas.activeSelf) boardCanvas.SetActive(false);
            else OpenBoard();
        }

        /// <summary>선택된 노드의 하이라이트만 켜고 나머지는 끈다. 버튼 자체 그래픽은 건드리지 않음.</summary>
        private void RefreshHighlights()
        {
            foreach (var binding in nodeButtons)
                binding.spawnedVisualPrefab?.SetHighlighted(binding.nodeId == selectedNodeId);
        }

        private void SelectNode(string nodeId)
        {
            selectedNodeId = nodeId;
            RefreshHighlights();

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
