using System;
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
        [SerializeField] private SanctuaryController sanctuaryController;
        [SerializeField] private RaidClockController raidClockController;
        [Tooltip("스쿼드 파견 시스템용 — 플레이어(Y) occupant 갱신, 정원 초과 경고에 사용")]
        [SerializeField] private SquadRuntimeData squadRuntimeData;
        [SerializeField] private Core.GlobalWarningUI globalWarningUI;
        [Tooltip("occupant 조회 실패 시(이론상 발생 안 함) 대비용 fallback — SquadController/SanctuaryController와 동일한 기본값")]
        [SerializeField] private string standbyNodeId = "StandbyNode";

        [Header("투명도 조절")]
        [SerializeField] private CanvasGroup boardCanvasGroup;
        [SerializeField] private Slider opacitySlider;

        private string selectedNodeId;

        public string SelectedNodeId => selectedNodeId;
        public IReadOnlyList<NodeButtonBinding> NodeBindings => nodeButtons;

        /// <summary>노드 외형 프리팹이 새로 스폰될 때마다 발생 (Squad 기능이 그 프리팹의 파견 버튼에
        /// 동작을 연결할 수 있도록). RaidBoardController는 구독자가 누구인지 전혀 모른다(단방향 의존 유지).</summary>
        public event Action<NodeButtonBinding> OnNodeVisualSpawned;

        /// <summary>Awake는 씬의 모든 오브젝트에서 Start보다 항상 먼저 실행되므로, 다른 컨트롤러(SquadController 등)가
        /// Start()에서 runtimeState(occupants 포함)를 안전하게 사용할 수 있도록 초기화를 여기로 옮겨둔다.</summary>
        private void Awake()
        {
            raidRuntimeData.InitializeRuntimeState();
        }

        private void Start()
        {
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
                RefreshNodeSelectability(binding);
                RefreshNodeSanctuaryTimer(binding);
            }

            RefreshEnterButtonState();
        }

        /// <summary>이 노드에 지금 파견/진입할 수 있는지(=성역 활성 상태인지). 계율의 사슬은 노드가 닫혀 있어도
        /// 걸 수 있어야 하므로(닫힌 노드의 몬스터를 끌어오는 게 계율의 사슬의 존재 이유) 이 체크와는 무관하다.</summary>
        public bool CanEnterNode(string nodeId) => sanctuaryController.IsNodeActive(nodeId);

        /// <summary>플레이어가 이미 입장해 있는 노드는 버튼 자체를 선택 불가(interactable=false) 처리해서
        /// 재입장을 막는다. (2026-08-22, 17차 변경) 성역 비활성 상태는 더 이상 "선택" 자체를 막지 않음 —
        /// 닫힌 노드의 몬스터에게도 계율의 사슬을 걸 수 있어야 하므로, 성역 상태는 파견/진입(아랫줄) 버튼
        /// 쪽(CanEnterNode)에서만 체크한다.</summary>
        private void RefreshNodeSelectability(NodeButtonBinding binding)
        {
            bool isCurrentNode = binding.nodeId == mapTransitionController.CurrentNodeId;
            binding.button.interactable = !isCurrentNode;

            if (isCurrentNode && binding.nodeId == selectedNodeId)
            {
                selectedNodeId = null;
                RefreshHighlights();
            }
        }

        /// <summary>현황판 하단 '진입' 버튼도 선택된 노드가 지금 입장 가능한 상태일 때만 눌리도록 동기화한다.</summary>
        private void RefreshEnterButtonState()
        {
            enterButton.interactable = !string.IsNullOrEmpty(selectedNodeId) && CanEnterNode(selectedNodeId);
        }

        /// <summary>노드의 성역 전환까지 남은 시간을, 방금 스폰된 노드 프리팹의 카운트다운 표시에 반영한다.</summary>
        private void RefreshNodeSanctuaryTimer(NodeButtonBinding binding)
        {
            int remaining = raidRuntimeData.GetSecondsUntilSanctuaryTransition(binding.nodeId, raidClockController.ElapsedSeconds);
            binding.spawnedVisualPrefab?.SetSanctuaryTimer(remaining);
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

            OnNodeVisualSpawned?.Invoke(binding);
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
            TryEnterNode(selectedNodeId);
        }

        /// <summary>지정한 노드로 플레이어(Y)를 실제로 이동시킨다 — 정원 체크 + occupant 갱신 + 배경/몬스터 전환.
        /// 현황판 하단 '진입' 버튼과, 파견 UI의 노란(Y) 버튼이 공통으로 사용한다(Y는 파견이 아니라 직접 이동이므로).</summary>
        public void TryEnterNode(string nodeId)
        {
            // 성역 비활성 노드 방어 체크 (2026-08-22, 17차 추가) — 버튼이 정상적으로 비활성화돼 있었다면
            // 애초에 호출되지 않았겠지만, 방어적으로 한 번 더 확인한다.
            if (!CanEnterNode(nodeId))
            {
                globalWarningUI.ShowWarning("지금은 이 노드에 입장할 수 없습니다");
                return;
            }

            // 정원 체크 (2026-08-22 추가) — 플레이어(Y)도 R/G와 동일하게 노드 정원 대상.
            if (!raidRuntimeData.CanAddOccupant(nodeId))
            {
                globalWarningUI.ShowWarning("정원 초과로 입장할 수 없습니다");
                return;
            }

            string playerCharacterId = squadRuntimeData.runtimeState.composition.memberCharacterIds[0];
            // 신규(버그 수정): "현재 화면 노드"가 아니라 실제 occupant 데이터를 조회해서 출발 노드를 구함
            // — CurrentNodeId와 occupant 위치가 어긋나는 경우(성역 강제 퇴장 등)에도 정확히 동작하도록.
            // SquadController.TryDispatch의 R/G 파견과 동일한 패턴.
            string fromNodeId = raidRuntimeData.FindOccupantNode(playerCharacterId) ?? standbyNodeId;
            raidRuntimeData.MoveOccupant(playerCharacterId, fromNodeId, nodeId);

            boardCanvas.SetActive(false);
            mapTransitionController.EnterNode(nodeId);
        }
    }
}
