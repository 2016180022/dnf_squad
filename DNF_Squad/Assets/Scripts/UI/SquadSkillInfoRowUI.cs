using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// 왼쪽 위 스킬 정보 목록의 한 행 — 표시 전용, 드래그 없음.
    /// 행의 순서는 마스터 데이터 순서로 고정이며, 퀵슬롯 배치와 무관하다.
    /// </summary>
    public class SquadSkillInfoRowUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;          // 스킬 아이콘 (기존 orderText 자리)
        [SerializeField] private TMP_Text nameText;        // "전투 조력 (Lv.1)"
        [SerializeField] private TMP_Text descriptionText; // 스킬 설명

        /// <summary>
        /// 스킬 정보를 표시. skillLevel은 이후 버프 강화 시스템이 붙으면
        /// buffProgress 기반으로 계산된 값이 넘어오도록 교체될 예정 (현재는 1 고정).
        /// </summary>
        public void Display(SquadSkillData skill, int skillLevel)
        {
            if (skill == null) return;

            iconImage.sprite = Resources.Load<Sprite>($"Image/Skill/{skill.skillId}");
            nameText.text = $"{skill.skillName} (Lv.{skillLevel})";
            descriptionText.text = skill.description;
        }

        // SetOrderNumber() 삭제 — 순번 표시 대신 아이콘을 쓰므로 배치 변경 시 갱신할 필요 없음
    }
}
