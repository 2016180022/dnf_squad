using UnityEngine;
using DnfSquad.Data;

namespace DnfSquad.Play.Core
{
    /// <summary>
    /// 노드에 입장했을 때 그 노드에 있는 몬스터를 화면에 배치한다 (기본 기능 — 코어).
    /// 행동 패턴은 다루지 않고, 스폰 여부만 확인하는 용도의 1차 버전.
    /// MapTransitionController가 노드 진입 시 SpawnMonsterAtNode를 호출해서 사용한다.
    /// </summary>
    public class MonsterSpawnController : MonoBehaviour
    {
        [SerializeField] private RaidRuntimeData raidRuntimeData;

        private GameObject spawnedMonsterObject;

        /// <summary>지정한 노드에 현재 있는 몬스터를 스폰. 몬스터가 없으면 기존 오브젝트만 정리하고 끝.</summary>
        public void SpawnMonsterAtNode(string nodeId)
        {
            ClearSpawnedMonster();

            MonsterData monster = raidRuntimeData.GetMonsterAtNode(nodeId);
            if (monster == null) return;

            GameObject prefab = Resources.Load<GameObject>($"Prefab/Monster/{monster.monsterPrefabId}");
            if (prefab == null)
            {
                Debug.LogWarning($"[MonsterSpawnController] 프리팹을 찾을 수 없음: Prefab/Monster/{monster.monsterPrefabId}");
                return;
            }

            spawnedMonsterObject = Instantiate(prefab, monster.spawnPosition, Quaternion.identity);
            spawnedMonsterObject.name = $"Monster_{monster.monsterId}";
        }

        public void ClearSpawnedMonster()
        {
            if (spawnedMonsterObject != null) Destroy(spawnedMonsterObject);
        }
    }
}
