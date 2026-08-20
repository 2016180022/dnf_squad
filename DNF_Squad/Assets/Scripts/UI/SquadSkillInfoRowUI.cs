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
        [SerializeField] private TMP_Text nameText;        // "전투 조력"
        [SerializeField] private TMP_Text descriptionText; // 스킬 설명

        /// <summary>
        /// 스킬 정보를 표시. 설명문은 버프가 반영된 값이 넘어온다.
        /// (레벨 표기는 폐기 — 스킬 기본 레벨 1과 버프 기본 레벨 0이 맞지 않아 혼란을 줌)
        /// </summary>
        public void Display(SquadSkillData skill, string description)
        {
            if (skill == null) return;

            iconImage.sprite = Resources.Load<Sprite>($"Image/Skill/{skill.skillId}");
            nameText.text = skill.skillName;
            descriptionText.text = description;
        }

        // SetOrderNumber() 삭제 — 순번 표시 대신 아이콘을 쓰므로 배치 변경 시 갱신할 필요 없음
    }
}
