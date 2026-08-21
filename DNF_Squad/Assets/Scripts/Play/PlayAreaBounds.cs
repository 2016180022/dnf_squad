using UnityEngine;

namespace DnfSquad.Play
{
    /// <summary>
    /// 씬에 배치해서 플레이어가 이동할 수 있는 사각 범위를 정의하는 컴포넌트.
    /// 맵마다 다른 값을 가질 수 있도록 Player와 분리된 별도 오브젝트로 둔다.
    /// </summary>
    public class PlayAreaBounds : MonoBehaviour
    {
        [SerializeField] private Vector2 min = new Vector2(-4f, -1f);
        [SerializeField] private Vector2 max = new Vector2(4f, 1f);

        public Vector2 Clamp(Vector2 pos)
        {
            return new Vector2(
                Mathf.Clamp(pos.x, min.x, max.x),
                Mathf.Clamp(pos.y, min.y, max.y));
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0f);
            Vector3 size = new Vector3(max.x - min.x, max.y - min.y, 0f);
            Gizmos.DrawWireCube(center, size);
        }
    }
}
