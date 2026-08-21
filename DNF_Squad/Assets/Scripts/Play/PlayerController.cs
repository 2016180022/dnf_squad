using UnityEngine;
using UnityEngine.InputSystem;

namespace DnfSquad.Play
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private PlayAreaBounds areaBounds;
        [SerializeField] private float sortingOrderPerDepthUnit = 100f;

        [Tooltip("InputSystem_Actions 에셋 안의 Player > Move 액션을 여기로 드래그")]
        [SerializeField] private InputActionReference moveActionReference;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private static readonly int MoveSpeedParam = Animator.StringToHash("MoveSpeed");

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable() => moveActionReference.action.Enable();
        private void OnDisable() => moveActionReference.action.Disable();

        private void Update()
        {
            Vector2 moveInput = moveActionReference.action.ReadValue<Vector2>();

            Vector2 nextPos = (Vector2)transform.position + moveInput * moveSpeed * Time.deltaTime;
            if (areaBounds != null) nextPos = areaBounds.Clamp(nextPos);
            transform.position = nextPos;

            if (Mathf.Abs(moveInput.x) > 0.01f) spriteRenderer.flipX = moveInput.x < 0f;

            spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * sortingOrderPerDepthUnit);
            animator.SetFloat(MoveSpeedParam, moveInput.magnitude);
        }
    }
}
