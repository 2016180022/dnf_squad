using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// 왼쪽 위 스킬 정보 목록의 한 행 — 표시 전용, 드래그 없음.
    /// 행의 순서는 마스터 데이터 순서로 고정이고, 앞의 숫자만 현재 퀵슬롯 배치 번호로 갱신된다.
    /// </summary>
    public class SquadSkillInfoRowUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text orderText;       // 이 스킬이 배치된 퀵슬롯 번호 (1~6)
        [SerializeField] private TMP_Text nameText;        // "전투 조력 (Lv.1)"
        [SerializeField] private TMP_Text descriptionText; // 스킬 설명

        /// <summary>
        /// 스킬 정보를 표시. skillLevel은 이후 버프 강화 시스템이 붙으면
        /// buffProgress 기반으로 계산된 값이 넘어오도록 교체될 예정 (현재는 1 고정).
        /// </summary>
        public void Display(SquadSkillData skill, int skillLevel)
        {
            if (skill == null) return;

            nameText.text = $"{skill.skillName} (Lv.{skillLevel})";
            descriptionText.text = skill.description;
        }

        /// <summary>퀵슬롯 배치가 바뀔 때마다 호출 — 앞쪽 순번 숫자만 갱신</summary>
        public void SetOrderNumber(int quickSlotNumber)
        {
            orderText.text = quickSlotNumber.ToString();
        }
    }
}
