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
        [SerializeField] private TMP_Text globalWarningText;

        private void Start()
        {
            leaderSlot.Init(this, SlotRole.Leader);
            bufferSlot.Init(this, SlotRole.Buffer);
            memberSlot1.Init(this, SlotRole.Member, 0);
            memberSlot2.Init(this, SlotRole.Member, 1);

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
                globalWarningText.text = error;
                globalWarningText.gameObject.SetActive(true);
                return;
            }

            globalWarningText.gameObject.SetActive(false);
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

            memberSlot1.SetWarning(null);
            memberSlot2.SetWarning(null);

            if (member1Filled && !member2Filled) memberSlot2.SetWarning(warning);
            else if (!member1Filled && member2Filled) memberSlot1.SetWarning(warning);
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
            // TODO: SquadSettingCanvas로 전환 (Canvas SetActive 토글) — SettingSceneFlowController 작업 시 연결
        }
    }
}