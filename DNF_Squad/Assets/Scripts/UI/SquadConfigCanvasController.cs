using System.Linq;
using DnfSquad.Data;
using DnfSquad.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class SquadConfigCanvasController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadData;

        [Header("레이드 명성 조건 (TODO: 추후 레이드 마스터 데이터로 이전 예정)")]
        [SerializeField] private int requiredLeaderFame;
        [SerializeField] private int requiredEntryFame;

        [Header("슬롯")]
        [SerializeField] private CharacterSlotUI leaderSlot;
        [SerializeField] private CharacterSlotUI bufferSlot;
        [SerializeField] private CharacterSlotUI memberSlot1;
        [SerializeField] private CharacterSlotUI memberSlot2;

        [Header("캐릭터 리스트")]
        [SerializeField] private Transform listContent;
        [SerializeField] private CharacterListItemUI listItemPrefab;
        [SerializeField] private Transform dragLayer;

        [Header("하단")]
        [SerializeField] private Button confirmButton;
        [SerializeField] private GameObject warningTextBox; // 표시/숨김 대상 루트
        [SerializeField] private TMP_Text warningText;      // 실제 문구가 들어가는 텍스트

        [Header("씬 흐름")]
        [SerializeField] private DnfSquad.Scene.SettingSceneFlowController flowController;

        private void Start()
        {
            leaderSlot.Init(this, SlotRole.Leader, dragLayer);
            bufferSlot.Init(this, SlotRole.Buffer, dragLayer);
            memberSlot1.Init(this, SlotRole.Member, dragLayer, 0);
            memberSlot2.Init(this, SlotRole.Member, dragLayer, 1);

            PopulateCharacterList();
            RefreshFromRuntimeState();
        }

        private void PopulateCharacterList()
        {
            var sorted = squadData.adventurerCharacters
                .Where(c => c.fame >= requiredEntryFame)
                .OrderByDescending(c => c.fame);

            foreach (var character in sorted)
            {
                var item = Instantiate(listItemPrefab, listContent);
                item.Setup(character, dragLayer);
            }
        }

        public void TryAssignCharacter(CharacterSlotUI slot, AdventurerCharacterData character)
        {
            int requiredFame = slot.Role == SlotRole.Leader ? requiredLeaderFame : requiredEntryFame;

            if (!SquadValidationService.CanAssign(squadData.runtimeState.composition, slot.Role, character, requiredFame, out string error))
            {
                ShowWarning(error);
                return;
            }

            HideWarning();
            ApplyAssignment(slot, character.characterId);
            slot.AssignCharacter(character);

            RefreshMemberWarning();
            RefreshConfirmButtonState();
        }

        private void ApplyAssignment(CharacterSlotUI slot, string characterId)
        {
            var comp = squadData.runtimeState.composition;
            switch (slot.Role)
            {
                case SlotRole.Leader: comp.leaderCharacterId = characterId; break;
                case SlotRole.Buffer: comp.bufferCharacterId = characterId; break;
                case SlotRole.Member: comp.memberCharacterIds[slot.MemberIndex] = characterId; break;
            }
        }

        /// <summary>슬롯에서 슬롯으로 드래그했을 때 이동(빈 슬롯) 또는 스왑(채워진 슬롯)을 시도</summary>
        public void TrySwapOrMove(CharacterSlotUI sourceSlot, CharacterSlotUI targetSlot)
        {
            string sourceCharacterId = sourceSlot.AssignedCharacterId;
            if (string.IsNullOrEmpty(sourceCharacterId)) return;

            var sourceCharacter = squadData.GetCharacter(sourceCharacterId);
            string targetCharacterId = targetSlot.AssignedCharacterId;
            int sourceRequiredFame = sourceSlot.Role == SlotRole.Leader ? requiredLeaderFame : requiredEntryFame;
            int targetRequiredFame = targetSlot.Role == SlotRole.Leader ? requiredLeaderFame : requiredEntryFame;

            if (string.IsNullOrEmpty(targetCharacterId))
            {
                // 빈 슬롯으로 이동: 기존 CanAssign 재사용
                ApplyAssignment(sourceSlot, null);

                if (!SquadValidationService.CanAssign(squadData.runtimeState.composition, targetSlot.Role, sourceCharacter, targetRequiredFame, out string moveError))
                {
                    ApplyAssignment(sourceSlot, sourceCharacterId); // 원위치 복구
                    ShowWarning(moveError);
                    return;
                }

                HideWarning();
                ApplyAssignment(targetSlot, sourceCharacterId);
                targetSlot.AssignCharacter(sourceCharacter);
                sourceSlot.Clear();
            }
            else
            {
                // 스왑: 두 슬롯을 임시로 비운 뒤 서로의 자리에 들어갈 수 있는지 양방향 검사
                var targetCharacter = squadData.GetCharacter(targetCharacterId);
                ApplyAssignment(sourceSlot, null);
                ApplyAssignment(targetSlot, null);

                bool sourceIntoTargetOk = SquadValidationService.CanAssign(squadData.runtimeState.composition, targetSlot.Role, sourceCharacter, targetRequiredFame, out string errorA);
                bool targetIntoSourceOk = SquadValidationService.CanAssign(squadData.runtimeState.composition, sourceSlot.Role, targetCharacter, sourceRequiredFame, out string errorB);

                if (!sourceIntoTargetOk || !targetIntoSourceOk)
                {
                    ApplyAssignment(sourceSlot, sourceCharacterId); // 원상 복구
                    ApplyAssignment(targetSlot, targetCharacterId);
                    ShowWarning(!sourceIntoTargetOk ? errorA : errorB);
                    return;
                }

                HideWarning();
                ApplyAssignment(sourceSlot, targetCharacterId);
                ApplyAssignment(targetSlot, sourceCharacterId);
                sourceSlot.AssignCharacter(targetCharacter);
                targetSlot.AssignCharacter(sourceCharacter);
            }

            RefreshMemberWarning();
            RefreshConfirmButtonState();
        }

        /// <summary>슬롯이 아닌 빈 공간으로 드래그해서 놓았을 때 배치 해제</summary>
        public void TryUnassignCharacter(CharacterSlotUI slot)
        {
            ApplyAssignment(slot, null);
            slot.Clear();
            HideWarning();
            RefreshMemberWarning();
            RefreshConfirmButtonState();
        }

        private void RefreshFromRuntimeState()
        {
            var comp = squadData.runtimeState.composition;
            AssignIfPresent(leaderSlot, comp.leaderCharacterId);
            AssignIfPresent(bufferSlot, comp.bufferCharacterId);
            AssignIfPresent(memberSlot1, comp.memberCharacterIds[0]);
            AssignIfPresent(memberSlot2, comp.memberCharacterIds[1]);

            RefreshMemberWarning();
            RefreshConfirmButtonState();
        }

        private void AssignIfPresent(CharacterSlotUI slot, string characterId)
        {
            if (string.IsNullOrEmpty(characterId)) return;
            var character = squadData.GetCharacter(characterId);
            if (character != null) slot.AssignCharacter(character);
        }

        private void RefreshMemberWarning()
        {
            var comp = squadData.runtimeState.composition;
            bool member1Filled = !string.IsNullOrEmpty(comp.memberCharacterIds[0]);
            bool member2Filled = !string.IsNullOrEmpty(comp.memberCharacterIds[1]);
            const string warning = "멤버를 추가로 구성하지 않을 경우 던전 클리어가 어려울 수 있습니다";

            if (member1Filled != member2Filled) ShowWarning(warning);
            else HideWarning();
        }

        private void ShowWarning(string message)
        {
            warningText.text = message;
            warningTextBox.SetActive(true);
        }

        private void HideWarning()
        {
            warningTextBox.SetActive(false);
        }

        private void RefreshConfirmButtonState()
        {
            var comp = squadData.runtimeState.composition;
            bool hasLeader = !string.IsNullOrEmpty(comp.leaderCharacterId);
            bool hasBuffer = !string.IsNullOrEmpty(comp.bufferCharacterId);
            bool hasAtLeastOneMember = !string.IsNullOrEmpty(comp.memberCharacterIds[0]) || !string.IsNullOrEmpty(comp.memberCharacterIds[1]);

            confirmButton.interactable = hasLeader && hasBuffer && hasAtLeastOneMember;
        }

        public void OnConfirmButtonClicked()
        {
            flowController.ShowSettingCanvas();
        }
    }
}