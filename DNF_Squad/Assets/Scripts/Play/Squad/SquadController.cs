using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DnfSquad.Data;
using DnfSquad.Logic;
using DnfSquad.Play.Core;
using DnfSquad.Play.Raid;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 스쿼드 리더/멤버 파견 기능 — 코어/레이드와 별개로 관리하는 Squad 기능.
    /// 파견 UI(PartyTag)는 노드 프리팹에 내장되어 있고(NodePartyTagUI), 이 컨트롤러는 노드가 새로
    /// 스폰될 때마다(RaidBoardController.OnNodeVisualSpawned) 그 버튼에 동작을 주입하고,
    /// 선택 상태/명령 가능 여부를 매 프레임 갱신한다.
    /// </summary>
    public class SquadController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private SquadRuntimeData squadRuntimeData;

        [Header("연동 컨트롤러")]
        [SerializeField] private RaidBoardController raidBoardController;
        [SerializeField] private HealthController healthController;
        [SerializeField] private GlobalWarningUI globalWarningUI;

        [SerializeField] private string standbyNodeId = "StandbyNode";

        [Header("UI")]
        [SerializeField] private SquadStatusPanelUI statusPanel;

        [Header("자동 딜링")]
        [SerializeField] private float autoDamageTickIntervalSeconds = 1f;

        private string lastSelectedNodeId;
        private float autoDamageTimer;
        private readonly Dictionary<string, float> pendingDamage = new Dictionary<string, float>();

        private SquadComposition Composition => squadRuntimeData.runtimeState.composition;

        private void Start()
        {
            RegisterInitialOccupants();

            statusPanel.Display(
                squadRuntimeData.GetCharacter(Composition.leaderCharacterId),
                squadRuntimeData.GetCharacter(Composition.memberCharacterIds[0]),
                squadRuntimeData.GetCharacter(Composition.memberCharacterIds[1]));

            raidBoardController.OnNodeVisualSpawned += HandleNodeVisualSpawned;
        }

        private void OnDestroy()
        {
            if (raidBoardController != null) raidBoardController.OnNodeVisualSpawned -= HandleNodeVisualSpawned;
        }

        private void RegisterInitialOccupants()
        {
            raidRuntimeData.AddOccupant(standbyNodeId, Composition.leaderCharacterId, SlotRole.Leader);
            raidRuntimeData.AddOccupant(standbyNodeId, Composition.memberCharacterIds[0], SlotRole.Member);
            raidRuntimeData.AddOccupant(standbyNodeId, Composition.memberCharacterIds[1], SlotRole.Member);
        }

        private void Update()
        {
            RefreshPartyTagVisibility();
            RefreshNodeTags();
            TickAutoDamage(Time.deltaTime);
        }

        // ===== 노드 프리팹 스폰 시 파견 버튼 연결 =====

        private void HandleNodeVisualSpawned(RaidBoardController.NodeButtonBinding binding)
        {
            var partyTag = binding.spawnedVisualPrefab?.PartyTag;
            if (partyTag == null) return;

            string nodeId = binding.nodeId;

            partyTag.DispatchRButton.onClick.AddListener(() => TryDispatch(SquadColor.R, nodeId));
            partyTag.DispatchYButton.onClick.AddListener(() => raidBoardController.TryEnterNode(nodeId));
            partyTag.DispatchGButton.onClick.AddListener(() => TryDispatch(SquadColor.G, nodeId));

            partyTag.ChainRButton.onClick.AddListener(() => TryUseChainOfDiscipline(SquadColor.R, nodeId));
            partyTag.ChainYButton.onClick.AddListener(() => TryUseChainOfDiscipline(SquadColor.Y, nodeId));
            partyTag.ChainGButton.onClick.AddListener(() => TryUseChainOfDiscipline(SquadColor.G, nodeId));

            partyTag.SetVisible(nodeId == raidBoardController.SelectedNodeId);
            if (nodeId == raidBoardController.SelectedNodeId) RefreshSelectedPartyTagInteractable();
        }

        // ===== 파견 UI 표시/활성화 갱신 =====

        private void RefreshPartyTagVisibility()
        {
            string selected = raidBoardController.SelectedNodeId;
            if (selected == lastSelectedNodeId) return;
            lastSelectedNodeId = selected;

            foreach (var binding in raidBoardController.NodeBindings)
            {
                binding.spawnedVisualPrefab?.PartyTag?.SetVisible(binding.nodeId == selected);
            }

            if (!string.IsNullOrEmpty(selected)) RefreshSelectedPartyTagInteractable();
        }

        private void RefreshSelectedPartyTagInteractable()
        {
            string nodeId = raidBoardController.SelectedNodeId;
            if (string.IsNullOrEmpty(nodeId)) return;

            var binding = raidBoardController.NodeBindings.FirstOrDefault(b => b.nodeId == nodeId);
            var partyTag = binding?.spawnedVisualPrefab?.PartyTag;
            if (partyTag == null) return;

            bool hasMonster = raidRuntimeData.GetMonsterAtNode(nodeId) != null;
            bool canAdd = raidRuntimeData.CanAddOccupant(nodeId);
            // 아랫줄(파견/진입)은 성역 비활성 노드에서는 불가 — 계율의 사슬(윗줄)은 이 체크와 무관하게 항상 가능.
            bool canEnter = raidBoardController.CanEnterNode(nodeId);

            string rNodeId = raidRuntimeData.FindOccupantNode(Composition.leaderCharacterId);
            string yNodeId = raidRuntimeData.FindOccupantNode(Composition.memberCharacterIds[0]);
            string gNodeId = raidRuntimeData.FindOccupantNode(Composition.memberCharacterIds[1]);

            bool rHere = rNodeId == nodeId;
            bool gHere = gNodeId == nodeId;

            // 계율의 사슬: 대상 노드에 몬스터가 있어야 하고, 끌어올 목적지(사용자 자신의 노드)에는
            // 몬스터가 없어야 함 — 몬스터 2마리가 같은 노드에 겹치는 걸 막기 위함.
            bool chainR = hasMonster && HasNoMonster(rNodeId);
            bool chainY = hasMonster && HasNoMonster(yNodeId);
            bool chainG = hasMonster && HasNoMonster(gNodeId);

            partyTag.SetInteractable(
                chainR: chainR, chainY: chainY, chainG: chainG,
                dispatchR: canEnter && canAdd && !rHere,
                dispatchY: canEnter && canAdd,
                dispatchG: canEnter && canAdd && !gHere);
        }

        /// <summary>이 노드가 비어있는지(몬스터 없음) — 계율의 사슬 목적지 겹침 체크용. 위치를 모르면(occupant 없음) false 취급.</summary>
        private bool HasNoMonster(string nodeId) =>
            !string.IsNullOrEmpty(nodeId) && raidRuntimeData.GetMonsterAtNode(nodeId) == null;

        // ===== 명령 처리 =====

        /// <summary>파견 팝업의 "R 파견" / "G 파견" 버튼에서 호출 (Y는 raidBoardController.TryEnterNode로 직접 이동)</summary>
        private void TryDispatch(SquadColor color, string nodeId)
        {
            string characterId = SquadDispatchService.GetCharacterId(Composition, color);
            if (string.IsNullOrEmpty(characterId)) return;

            if (!raidBoardController.CanEnterNode(nodeId))
            {
                globalWarningUI.ShowWarning("지금은 이 노드로 파견할 수 없습니다");
                return;
            }

            if (!raidRuntimeData.CanAddOccupant(nodeId))
            {
                globalWarningUI.ShowWarning("정원 초과로 파견할 수 없습니다");
                return;
            }

            string fromNodeId = raidRuntimeData.FindOccupantNode(characterId) ?? standbyNodeId;
            raidRuntimeData.MoveOccupant(characterId, fromNodeId, nodeId);

            RefreshSelectedPartyTagInteractable();
        }

        /// <summary>계율의 사슬 — nodeId(선택된 노드)의 몬스터를, color가 가리키는 캐릭터의 현재 위치로 옮긴다.
        /// R/Y/G 누구든 사용할 수 있고, 대상은 항상 "그 캐릭터 자신의 현재 노드"다.</summary>
        private void TryUseChainOfDiscipline(SquadColor color, string nodeId)
        {
            var monster = raidRuntimeData.GetMonsterAtNode(nodeId);
            if (monster == null) return;

            string characterId = SquadDispatchService.GetCharacterId(Composition, color);
            string userNodeId = raidRuntimeData.FindOccupantNode(characterId);
            if (string.IsNullOrEmpty(userNodeId)) return;

            // 목적지에 이미 몬스터가 있으면 겹치게 되므로 거부 (2026-08-22, 17차 추가) — 버튼이 정상적으로
            // 비활성화돼 있었다면 애초에 호출되지 않았겠지만, 방어적으로 한 번 더 확인한다.
            if (raidRuntimeData.GetMonsterAtNode(userNodeId) != null)
            {
                globalWarningUI.ShowWarning("이동할 위치에 이미 몬스터가 있어 계율의 사슬을 사용할 수 없습니다");
                return;
            }

            raidRuntimeData.MoveMonsterToNode(monster.monsterId, userNodeId);
            RefreshSelectedPartyTagInteractable();
            // TODO(계율의 사슬 세부 구현 시): 연출, 쿨타임/사용 횟수 제한 등은 여기 추가 예정.
        }

        // ===== 노드 R/Y/G 위치 태그(NowTag) =====

        private void RefreshNodeTags()
        {
            foreach (var binding in raidBoardController.NodeBindings)
            {
                var nowTag = binding.spawnedVisualPrefab?.NowTag;
                if (nowTag == null) continue;

                var occupants = raidRuntimeData.GetNodeState(binding.nodeId)?.occupants;
                bool hasR = false, hasY = false, hasG = false;
                if (occupants != null)
                {
                    foreach (var occupant in occupants)
                    {
                        var color = SquadDispatchService.GetColor(Composition, occupant.characterId);
                        if (color == SquadColor.R) hasR = true;
                        else if (color == SquadColor.Y) hasY = true;
                        else if (color == SquadColor.G) hasG = true;
                    }
                }
                nowTag.SetActiveColors(hasR, hasY, hasG);
            }
        }

        // ===== 자동 딜링 =====

        private void TickAutoDamage(float deltaTime)
        {
            autoDamageTimer += deltaTime;
            if (autoDamageTimer < autoDamageTickIntervalSeconds) return;
            autoDamageTimer -= autoDamageTickIntervalSeconds;

            ApplyAutoDamage(Composition.leaderCharacterId);
            ApplyAutoDamage(Composition.memberCharacterIds[1]);
        }

        private void ApplyAutoDamage(string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;

            string nodeId = raidRuntimeData.FindOccupantNode(characterId);
            if (string.IsNullOrEmpty(nodeId) || nodeId == standbyNodeId) return;

            var character = squadRuntimeData.GetCharacter(characterId);
            if (character == null) return;

            float damagePerSecond = SquadDispatchService.GetAutoDamagePerSecond(character.gearScore);

            pendingDamage.TryGetValue(characterId, out float accumulated);
            accumulated += damagePerSecond * autoDamageTickIntervalSeconds;

            int wholeDamage = Mathf.FloorToInt(accumulated);
            if (wholeDamage > 0)
            {
                healthController.DamageMonsterAtNode(nodeId, wholeDamage);
                accumulated -= wholeDamage;
            }
            pendingDamage[characterId] = accumulated;
        }
    }
}
