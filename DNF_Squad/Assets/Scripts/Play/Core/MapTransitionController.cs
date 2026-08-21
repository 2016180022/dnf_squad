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

        public void EnterNode(string nodeId)
        {
            string backgroundId = raidRuntimeData.GetNodeBackgroundImageId(nodeId);
            backgroundRenderer.sprite = Resources.Load<Sprite>($"Image/Map/{backgroundId}");

            monsterSpawnController.SpawnMonsterAtNode(nodeId);
        }
    }
}
