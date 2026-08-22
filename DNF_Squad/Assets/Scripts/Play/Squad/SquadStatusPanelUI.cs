using DnfSquad.Data;
using UnityEngine;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 좌상단 전용 패널 — 현재 레이드에 참여 중인 R/Y/G 3인을 표시 전용으로 보여준다 (Squad 기능).
    /// </summary>
    public class SquadStatusPanelUI : MonoBehaviour
    {
        [SerializeField] private SquadStatusSlotUI redSlot;
        [SerializeField] private SquadStatusSlotUI yellowSlot;
        [SerializeField] private SquadStatusSlotUI greenSlot;

        public void Display(AdventurerCharacterData leader, AdventurerCharacterData memberY, AdventurerCharacterData memberG)
        {
            redSlot.Display(leader);
            yellowSlot.Display(memberY);
            greenSlot.Display(memberG);
        }
    }
}
