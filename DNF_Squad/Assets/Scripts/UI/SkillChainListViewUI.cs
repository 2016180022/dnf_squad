using System.Collections.Generic;
using DnfSquad.Data;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// "스킬 사용 순서" 목록 뷰. 프리팹으로 만들어 스킬 체인 캔버스와
    /// 스쿼드 세팅의 체인 확인 팝업 양쪽에서 재사용한다.
    /// 아이콘은 프리팹 없이 런타임에 Image 오브젝트로 생성되며,
    /// 정렬은 iconContent에 붙은 GridLayoutGroup이 담당한다.
    /// </summary>
    public class SkillChainListViewUI : MonoBehaviour
    {
        [SerializeField] private Transform iconContent; // GridLayoutGroup이 붙은 오브젝트

        /// <summary>아이콘 1개를 목록 끝에 추가 (측정 중 실시간 기록용)</summary>
        public void AddIcon(string skillId)
        {
            var go = new GameObject($"StepIcon_{skillId}", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(iconContent, false);

            var image = go.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>($"Image/Skill/{skillId}");
            image.preserveAspect = true;
            image.raycastTarget = false;
        }

        public void Clear()
        {
            foreach (Transform child in iconContent) Destroy(child.gameObject);
        }

        /// <summary>저장된 체인 전체를 한 번에 표시 (확인 팝업 / 복원용)</summary>
        public void Show(List<SkillChainStep> steps)
        {
            Clear();
            foreach (var step in steps) AddIcon(step.skillId);
        }
    }
}
