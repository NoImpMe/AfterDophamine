using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AfterDophamine.Combat;
using AfterDophamine.Party;

namespace AfterDophamine.Stage
{
    /// <summary>
    /// 싱글턴. 스테이지 x의 1~10 서브스테이지 진행을 관장한다.
    /// - sub 1~9: 쫄몹 웨이브 + 해당 서브스테이지 보스 처치 (보스 프리팹을 기록해둔다)
    /// - sub 10 : x-1~x-9에서 기록된 보스 3종이 "중간보스"로 순차 등장.
    ///            중간보스 1마리 처치 시마다 증강 3장 중 1장 선택 → 최종보스전으로 이어짐.
    /// </summary>
    public class StageManager : MonoBehaviour
    {
        public static StageManager Instance { get; private set; }

        [Header("참조")]
        [SerializeField] private PartyFormationManager party;
        [SerializeField] private EnemyWaveSpawner waveSpawner;
        [SerializeField] private AfterDophamine.Augment.AugmentManager augmentManager;

        public int CurrentMajorStage { get; private set; } = 1; // x
        public int CurrentSubStage { get; private set; } = 1;   // 1~10

        // x-1~x-9에서 등장한 보스 프리팹을 순서대로 저장해뒀다가 x-10에서 재사용
        private readonly List<GameObject> recordedBossesThisMajorStage = new List<GameObject>();

        public event Action<int, int> OnSubStageChanged; // (major, sub)
        public event Action<int> OnMajorStageCleared;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnEnable() => PartyFormationManagerBridgeSubscribe();
        private void PartyFormationManagerBridgeSubscribe()
        {
            AfterDophamine.Party.StageManagerBridge.OnPartyWiped += HandlePartyWiped;
        }

        private void OnDisable()
        {
            AfterDophamine.Party.StageManagerBridge.OnPartyWiped -= HandlePartyWiped;
        }

        public void StartRun()
        {
            recordedBossesThisMajorStage.Clear();
            augmentManager.ClearAllAugments();
            StartCoroutine(RunSubStage(CurrentSubStage));
        }

        private IEnumerator RunSubStage(int sub)
        {
            OnSubStageChanged?.Invoke(CurrentMajorStage, sub);

            if (sub < 10)
            {
                yield return StartCoroutine(waveSpawner.SpawnMinionWave(party));

                GameObject bossPrefab = waveSpawner.GetBossPrefabForSubStage(sub);
                recordedBossesThisMajorStage.Add(bossPrefab);

                yield return StartCoroutine(RunSingleEnemy(bossPrefab, isMidBoss: false));

                AdvanceSubStage();
            }
            else
            {
                yield return StartCoroutine(RunClimaxStage());
            }
        }

        /// <summary>x-10 클라이맥스: 기록된 보스 3종이 중간보스로 순차 등장, 처치할 때마다 증강 선택</summary>
        private IEnumerator RunClimaxStage()
        {
            foreach (var bossPrefab in recordedBossesThisMajorStage)
            {
                yield return StartCoroutine(RunSingleEnemy(bossPrefab, isMidBoss: true));
                if (party.IsWiped) yield break;

                // 중간보스 처치 -> 증강 3장 중 1장 선택 (플레이어 입력 대기)
                yield return StartCoroutine(augmentManager.PresentChoicesAndWait(party));
            }

            // 최종 보스 (누적된 증강 시너지로 처치)
            GameObject finalBoss = waveSpawner.GetFinalBossForMajorStage(CurrentMajorStage);
            yield return StartCoroutine(RunSingleEnemy(finalBoss, isMidBoss: false));

            if (!party.IsWiped)
            {
                OnMajorStageCleared?.Invoke(CurrentMajorStage);
                CurrentMajorStage++;
                CurrentSubStage = 1;
                StartRun(); // 증강 초기화 후 (x+1)-1로 진입
            }
        }

        private IEnumerator RunSingleEnemy(GameObject enemyPrefab, bool isMidBoss)
        {
            var enemyGo = Instantiate(enemyPrefab);
            var enemy = enemyGo.GetComponent<EnemyUnit>();

            bool enemyDefeated = false;
            void HandleDeath(CombatUnit u) => enemyDefeated = true;
            enemy.OnDeath += HandleDeath;

            while (!enemyDefeated && !party.IsWiped)
                yield return null;

            enemy.OnDeath -= HandleDeath;
            if (enemyGo != null) Destroy(enemyGo);
        }

        private void AdvanceSubStage()
        {
            CurrentSubStage++;
            StartCoroutine(RunSubStage(CurrentSubStage));
        }

        private void HandlePartyWiped()
        {
            Debug.Log("[StageManager] 파티 전멸 - 스테이지 도전 실패");
            // 실패 처리: 재화 정산, 재도전 UI 노출 등은 별도 매니저에 위임
        }
    }
}
