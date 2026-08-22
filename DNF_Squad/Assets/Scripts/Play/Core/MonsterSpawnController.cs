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

        /// <summary>현재 맵에 스폰돼 있는 몬스터 ID. 없으면 null.</summary>
        public string CurrentMonsterId { get; private set; }

        /// <summary>현재 맵에 스폰돼 있는 몬스터 이름. 없으면 null.</summary>
        public string CurrentMonsterName { get; private set; }

        /// <summary>현재 맵에 스폰돼 있는 몬스터의 Transform. 없으면 null. (스쿼드 스킬 연출 등장 위치 계산용)</summary>
        public Transform CurrentMonsterTransform => spawnedMonsterObject != null ? spawnedMonsterObject.transform : null;

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
            CurrentMonsterId = monster.monsterId;
            CurrentMonsterName = monster.monsterName;
        }

        // 신규(기능 3, 2026-08-22): 현재 스폰된 몬스터의 체력이 0(isDead)이 되면 오브젝트를 지운다.
        // ClearSpawnedMonster()가 CurrentMonsterId를 null로 만들어주므로, MapMonsterHealthUI는
        // 별도 수정 없이 기존 로직(CurrentMonsterId 없으면 체력바 숨김)대로 자동으로 꺼진다.
        private void Update()
        {
            if (string.IsNullOrEmpty(CurrentMonsterId)) return;

            var state = raidRuntimeData.GetMonsterState(CurrentMonsterId);
            if (state != null && state.isDead)
            {
                ClearSpawnedMonster();
            }
        }

        public void ClearSpawnedMonster()
        {
            if (spawnedMonsterObject != null) Destroy(spawnedMonsterObject);
            CurrentMonsterId = null;
            CurrentMonsterName = null;
        }
    }
}
