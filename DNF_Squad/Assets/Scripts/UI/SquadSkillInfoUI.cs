using DnfSquad.Data;
using TMPro;
using UnityEngine;

namespace DnfSquad.UI
{
    /// <summary>
    /// 좌상단 스킬 정보 패널의 한 행 — 순번/이름/설명만 고정 표시.
    /// 드래그/재배치 없음. squadSkills 리스트 순서 그대로 항상 1~6번 고정.
    /// </summary>
    public class SquadSkillInfoUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text orderText;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;

        /// <summary>
        /// 스킬 데이터를 화면에 표시. 지금은 마스터 데이터 원본 값을 그대로 보여주지만,
        /// 이후 버프 강화 시스템이 붙으면 버프 레벨 반영된 설명으로 교체될 예정.
        /// </summary>
        public void Display(int order, SquadSkillData skill)
        {
            if (skill == null) return;

            orderText.text = order.ToString();
            nameText.text = skill.skillName;
            descriptionText.text = skill.description;
        }
    }
}
