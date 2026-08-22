using UnityEngine;

namespace DnfSquad.Play.Squad
{
    /// <summary>
    /// 스쿼드 스킬 연출용 프리팹(리더/버퍼)에 붙는 공용 컴포넌트.
    /// 스폰되면 Animator의 현재(기본) 스테이트가 끝날 때까지 재생하고, 끝나면 자동으로 사라진다.
    /// 스킬별 연출 길이는 프리팹의 애니메이션 클립 길이로 결정되므로, 스킬마다 별도 지속시간
    /// 값을 둘 필요가 없다 (SO 쪽 지속시간 필드는 두지 않음).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SquadSkillActorLifetime : MonoBehaviour
    {
        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            if (animator.IsInTransition(0)) return;

            var state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.normalizedTime >= 1f)
            {
                Destroy(gameObject);
            }
        }
    }
}
