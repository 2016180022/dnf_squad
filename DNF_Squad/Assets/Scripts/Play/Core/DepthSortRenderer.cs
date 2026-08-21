using UnityEngine;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// Y 좌표 기준으로 스프라이트 정렬 순서(sortingOrder)를 계산한다.
    /// groundAnchor를 지정하면 스프라이트의 시각적 피벗과 무관하게
    /// "바닥에 닿는 것으로 취급할 지점"을 기준으로 정렬할 수 있다.
    /// (예: 날개가 몸통보다 아래로 내려오는 비행 몬스터 — 앵커를 몸통 아래에 따로 배치)
    /// </summary>
    public class DepthSortRenderer : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("비워두면 이 오브젝트 자신의 위치를 기준으로 정렬")]
        [SerializeField] private Transform groundAnchor;
        [SerializeField] private float sortingOrderPerDepthUnit = 100f;

        private void Reset()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void LateUpdate()
        {
            Transform anchor = groundAnchor != null ? groundAnchor : transform;
            spriteRenderer.sortingOrder = -Mathf.RoundToInt(anchor.position.y * sortingOrderPerDepthUnit);
        }
    }
}
