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
        [SerializeField] private Transform stepListContent;
        [SerializeField] private SkillChainStepItemUI stepItemPrefab;

        [Header("표시")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text totalDamageText;

        [Header("버튼")]
        [SerializeField] private Button summonButton;   // 소환
        [SerializeField] private Button removeButton;   // 제거
        [SerializeField] private Button resetButton;    // 초기화
        [SerializeField] private Button applyButton;    // 적용
        [SerializeField] private Button exitButton;     // 나가기

        [Header("씬 흐름")]
        [SerializeField] private SquadSettingCanvasController settingCanvasController;

        private const float CycleSeconds = 10f;

        private readonly List<SkillChainStep> recordedSteps = new List<SkillChainStep>();
        private readonly List<SkillChainStepItemUI> spawnedItems = new List<SkillChainStepItemUI>();

        private bool isMeasuring;
        private float remainingSeconds;
        private long totalDamage;

        private bool HasData => recordedSteps.Count > 0 && totalDamage > 0;

        private void OnEnable()
        {
            LoadFromRuntimeState();
            RefreshButtonStates();
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

            var item = Instantiate(stepItemPrefab, stepListContent);
            item.Display(step.order + 1, binding.skillId, binding.skillName);
            spawnedItems.Add(item);
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
            totalDamage = Random.Range(1, 10_000_001); // 1 ~ 10,000,000
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
            gameObject.SetActive(false);
            settingCanvasController.gameObject.SetActive(true);
            settingCanvasController.RefreshSkillChainSummary();
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

            foreach (var item in spawnedItems) Destroy(item.gameObject);
            spawnedItems.Clear();

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

            foreach (var step in savedChain)
            {
                recordedSteps.Add(step);

                var binding = System.Array.Find(leaderSkillBindings, b => b != null && b.skillId == step.skillId);
                var item = Instantiate(stepItemPrefab, stepListContent);
                item.Display(step.order + 1, step.skillId, binding != null ? binding.skillName : step.skillId);
                spawnedItems.Add(item);
            }

            totalDamage = squadData.runtimeState.leaderSkillChainTotalDamage;
            if (totalDamage > 0) totalDamageText.text = $"총합 데미지 : {totalDamage:N0}";
        }
    }
}
