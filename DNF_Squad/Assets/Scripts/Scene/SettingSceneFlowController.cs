using DnfSquad.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DnfSquad.Scene
{
    public class SettingSceneFlowController : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private SquadRuntimeData squadData;

        [Header("캔버스")]
        [SerializeField] private GameObject squadConfigCanvas;
        [SerializeField] private GameObject squadSettingCanvas;

        [Header("씬 전환")]
        [SerializeField] private string playSceneName = "PlayScene";

        private void Start()
        {
            ShowConfigCanvas();
        }

        public void ShowConfigCanvas()
        {
            squadConfigCanvas.SetActive(true);
            squadSettingCanvas.SetActive(false);
        }

        public void ShowSettingCanvas()
        {
            squadConfigCanvas.SetActive(false);
            squadSettingCanvas.SetActive(true);
        }

        public void OnStartRaidClicked()
        {
            // TODO: SquadSaveService 구현 후 실제 저장 로직으로 교체 (현재는 저장 단계 미구현, 5단계 작업 예정)
            Debug.LogWarning("[SettingSceneFlowController] SquadSaveService가 아직 없어 저장을 건너뜁니다.");

            SceneManager.LoadScene(playSceneName);
        }
    }
}
