using UnityEngine;
using DnfSquad.Data;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// 현재 화면(배경 + 몬스터)을 지정한 노드 기준으로 전환한다 (기본 기능 — 코어).
    /// 씬을 새로 로드하지 않고, 배경 스프라이트와 몬스터만 갈아끼운다.
    /// </summary>
    public class MapTransitionController : MonoBehaviour
    {
        [SerializeField] private RaidRuntimeData raidRuntimeData;
        [SerializeField] private SpriteRenderer backgroundRenderer;
        [SerializeField] private MonsterSpawnController monsterSpawnController;
        [Tooltip("플레이 씬 시작 시 플레이어가 기본으로 위치할 노드 (기존엔 특정 노드 판정 없이 시작했으나, 대기 노드 도입 후 이 노드에서 시작하도록 변경)")]
        [SerializeField] private string startingNodeId = "StandbyNode";

        /// <summary>플레이어가 현재 입장해 있는 노드 ID (레이드 기능의 "보스 점유 여부" 판정 등에서 사용)</summary>
        public string CurrentNodeId { get; private set; }

        private void Start()
        {
            EnterNode(startingNodeId);
        }

        public void EnterNode(string nodeId)
        {
            CurrentNodeId = nodeId;

            string backgroundId = raidRuntimeData.GetNodeBackgroundImageId(nodeId);
            backgroundRenderer.sprite = Resources.Load<Sprite>($"Image/Map/{backgroundId}");

            monsterSpawnController.SpawnMonsterAtNode(nodeId);
        }
    }
}
