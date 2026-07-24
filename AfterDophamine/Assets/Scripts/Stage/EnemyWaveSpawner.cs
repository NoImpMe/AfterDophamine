using System.Collections;
using UnityEngine;
using AfterDophamine.Party;

namespace AfterDophamine.Stage
{
    /// <summary>
    /// 쫄몹 웨이브 및 보스 스폰 담당. 우측 화면 밖 스폰 → 좌측으로 이동하는
    /// 전진 스크롤 연출은 별도 MoveTowardParty 컴포넌트(또는 DOTween)로 처리한다고 가정.
    /// </summary>
    public class EnemyWaveSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] minionPrefabs;
        [SerializeField] private GameObject[] subStageBossPrefabs; // index 0 = x-1 보스 ... index 8 = x-9 보스
        [SerializeField] private GameObject finalBossPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int minionsPerWave = 8;
        [SerializeField] private float spawnIntervalSec = 0.4f;

        public IEnumerator SpawnMinionWave(PartyFormationManager party)
        {
            for (int i = 0; i < minionsPerWave; i++)
            {
                var prefab = minionPrefabs[Random.Range(0, minionPrefabs.Length)];
                var go = Instantiate(prefab, spawnPoint.position, Quaternion.identity);
                var enemy = go.GetComponent<Combat.EnemyUnit>();
                enemy.Initialize(party, hp: 20f, atk: 5f, def: 0f);
                yield return new WaitForSeconds(spawnIntervalSec);
            }

            // 웨이브 내 쫄몹이 모두 처리될 때까지 대기 (간단화를 위해 고정 대기 시간 사용,
            // 실전에서는 살아있는 쫄몹 카운트를 추적하는 것을 권장)
            yield return new WaitForSeconds(3f);
        }

        public GameObject GetBossPrefabForSubStage(int sub)
        {
            int index = Mathf.Clamp(sub - 1, 0, subStageBossPrefabs.Length - 1);
            return subStageBossPrefabs[index];
        }

        public GameObject GetFinalBossForMajorStage(int majorStage)
        {
            return finalBossPrefab; // 스테이지별 최종보스 풀을 두고 싶다면 배열/딕셔너리로 확장
        }
    }
}
