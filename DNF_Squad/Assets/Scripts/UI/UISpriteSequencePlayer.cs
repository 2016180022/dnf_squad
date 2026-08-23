using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    /// <summary>
    /// UI Image에 스프라이트 배열을 정해진 fps로 순차 재생시키는 프레임 애니메이션 플레이어.
    /// SpriteRenderer+Animator 클립은 컴포넌트 타입까지 바인딩돼 있어 Image에 그대로 재사용할 수 없어서,
    /// 같은 스프라이트 프레임들을 인스펙터에 등록해두고 코드로 직접 순환시키는 방식으로 대체.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UISpriteSequencePlayer : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float frameRate = 10f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnEnable = false; // 켜지자마자 자동 재생할지 여부(외부에서 Play() 호출하는 용도면 꺼둠)

        private Image image;
        private int currentFrame;
        private float frameTimer;
        private bool isPlaying;

        private void Awake()
        {
            image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (playOnEnable) Play();
        }

        /// <summary>처음(0번 프레임)부터 재생 시작</summary>
        public void Play()
        {
            if (frames == null || frames.Length == 0)
            {
                Debug.LogWarning("[UISpriteSequencePlayer] frames가 비어있어 재생할 수 없음");
                return;
            }

            isPlaying = true;
            currentFrame = 0;
            frameTimer = 0f;
            image.sprite = frames[0];
        }

        /// <summary>현재 프레임에서 멈춤 (이미지는 마지막으로 표시된 프레임 그대로 유지)</summary>
        public void Stop()
        {
            isPlaying = false;
        }

        private void Update()
        {
            if (!isPlaying || frames == null || frames.Length == 0) return;

            frameTimer += Time.deltaTime;
            float frameDuration = 1f / Mathf.Max(frameRate, 0.01f);
            if (frameTimer < frameDuration) return;

            frameTimer -= frameDuration;
            currentFrame++;

            if (currentFrame >= frames.Length)
            {
                if (!loop)
                {
                    isPlaying = false;
                    currentFrame = frames.Length - 1;
                    return;
                }
                currentFrame = 0;
            }

            image.sprite = frames[currentFrame];
        }
    }
}
