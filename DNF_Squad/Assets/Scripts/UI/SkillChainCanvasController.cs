using System.Collections.Generic;
using DnfSquad.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DnfSquad.UI
{
    public class SkillChainCanvasController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadData;

        [Header("리더 스킬 키 바인딩 (프로토타입 — A/S/D/F)")]
        [SerializeField] private LeaderSkillBinding[] leaderSkillBindings = new LeaderSkillBinding[4];

        [Header("스킬 사용 순서 목록")]
        [SerializeField] private SkillChainListViewUI stepListView;

        [Header("리더 스킬 입력 칸 아이콘 (A/S/D/F 순서로 연결)")]
        [SerializeField] private Image[] leaderSkillIcons = new Image[4];

        [Header("표시")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text totalDamageText;

        [Header("스킬 체인 측정 데미지 — 랜덤 범위 (프로토타입 스텁, 추후 실제 계산식으로 대체 예정)")]
        [SerializeField] private int minMeasuredDamage = 1;
        [SerializeField] private int maxMeasuredDamage = 10_000_000;

        [Header("버튼")]
        [SerializeField] private Button summonButton;   // 소환
        [SerializeField] private Button removeButton;   // 제거
        [SerializeField] private Button resetButton;    // 초기화
        [SerializeField] private Button applyButton;    // 적용
        [SerializeField] private Button exitButton;     // 나가기

        [Header("씬 흐름")]
        [SerializeField] private GameObject skillChainCanvasRoot;
        [SerializeField] private SquadSettingCanvasController settingCanvasController;

        private const float CycleSeconds = 10f;

        // 생성된 아이콘은 리스트 뷰가 직접 관리하므로 별도 추적 불필요
        private readonly List<SkillChainStep> recordedSteps = new List<SkillChainStep>();

        private bool isMeasuring;
        private float remainingSeconds;
        private long totalDamage;

        private bool HasData => recordedSteps.Count > 0 && totalDamage > 0;

        private void OnEnable()
        {
            RefreshLeaderSkillIcons();
            LoadFromRuntimeState();
            RefreshButtonStates();
        }

        /// <summary>A/S/D/F 입력 칸에 배치된 스킬 아이콘 표시</summary>
        private void RefreshLeaderSkillIcons()
        {
            for (int i = 0; i < leaderSkillIcons.Length && i < leaderSkillBindings.Length; i++)
            {
                var binding = leaderSkillBindings[i];
                var icon = leaderSkillIcons[i];
                if (icon == null) continue;

                if (binding == null || string.IsNullOrEmpty(binding.skillId))
                {
                    icon.enabled = false;
                    continue;
                }

                icon.sprite = Resources.Load<Sprite>($"Image/Skill/{binding.skillId}");
                icon.preserveAspect = true;
                icon.enabled = icon.sprite != null;
            }
        }

        private void Update()
        {
            if (!isMeasuring) return;

            remainingSeconds -= Time.deltaTime;

            if (remainingSeconds <= 0f)
            {
                remainingSeconds = 0f;
                UpdateTimerText();
                StopMeasuring();
                return;
            }

            UpdateTimerText();
            CaptureSkillInput();
        }

        private void CaptureSkillInput()
        {
            foreach (var binding in leaderSkillBindings)
            {
                if (binding == null || string.IsNullOrEmpty(binding.skillId)) continue;
                if (!Input.GetKeyDown(binding.key)) continue;

                AddStep(binding);
            }
        }

        private void AddStep(LeaderSkillBinding binding)
        {
            var step = new SkillChainStep
            {
                order = recordedSteps.Count,
                skillId = binding.skillId
            };
            recordedSteps.Add(step);

            stepListView.AddIcon(binding.skillId);
        }

        /// <summary>소환 버튼 — 기존 데이터를 초기화하고 10초 측정 시작</summary>
        public void OnSummonClicked()
        {
            ClearRecordedData();

            isMeasuring = true;
            remainingSeconds = CycleSeconds;
            UpdateTimerText();
            RefreshButtonStates();
        }

        /// <summary>제거 버튼 — 측정을 즉시 중단 (데미지 산출은 그대로 유효)</summary>
        public void OnRemoveClicked()
        {
            if (!isMeasuring) return;
            StopMeasuring();
        }

        private void StopMeasuring()
        {
            isMeasuring = false;
            // Random.Range(int,int)의 상한은 exclusive라서 +1 — 인스펙터에는 "포함" 범위(최소~최대)로 노출.
            totalDamage = Random.Range(minMeasuredDamage, maxMeasuredDamage + 1);
            totalDamageText.text = $"총합 데미지 : {totalDamage:N0}";
            RefreshButtonStates();
        }

        /// <summary>초기화 버튼 — 기록된 순서와 데미지를 모두 비움</summary>
        public void OnResetClicked()
        {
            ClearRecordedData();
            RefreshButtonStates();
        }

        /// <summary>적용 버튼 — 런타임 데이터에 저장하고 스쿼드 세팅으로 복귀</summary>
        public void OnApplyClicked()
        {
            squadData.runtimeState.leaderSkillChain = new List<SkillChainStep>(recordedSteps);
            squadData.runtimeState.leaderSkillChainTotalDamage = totalDamage;

            CloseCanvas();
        }

        /// <summary>나가기 버튼 — 저장 없이 복귀</summary>
        public void OnExitClicked()
        {
            CloseCanvas();
        }

        private void CloseCanvas()
        {
            isMeasuring = false;
            skillChainCanvasRoot.SetActive(false);
            settingCanvasController.ShowCanvas(); // 스쿼드 세팅 캔버스 재활성화 + 총합 데미지 갱신
        }

        private void RefreshButtonStates()
        {
            summonButton.interactable = !isMeasuring;
            removeButton.interactable = isMeasuring;
            resetButton.interactable = !isMeasuring && (recordedSteps.Count > 0 || totalDamage > 0);
            applyButton.interactable = !isMeasuring && HasData;
        }

        private void UpdateTimerText()
        {
            timerText.text = remainingSeconds.ToString("00.00");
        }

        private void ClearRecordedData()
        {
            recordedSteps.Clear();
            stepListView.Clear();

            totalDamage = 0;
            totalDamageText.text = "총합 데미지 : -";
            remainingSeconds = CycleSeconds;
            UpdateTimerText();
        }

        /// <summary>이전에 적용해둔 체인이 있으면 UI에 복원</summary>
        private void LoadFromRuntimeState()
        {
            ClearRecordedData();

            var savedChain = squadData.runtimeState.leaderSkillChain;
            if (savedChain.Count == 0) return;

            recordedSteps.AddRange(savedChain);
            stepListView.Show(savedChain);

            totalDamage = squadData.runtimeState.leaderSkillChainTotalDamage;
            if (totalDamage > 0) totalDamageText.text = $"총합 데미지 : {totalDamage:N0}";
        }
    }
}
