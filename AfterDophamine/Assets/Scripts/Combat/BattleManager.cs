using System.Collections.Generic;
using UnityEngine;
using AfterDophamine.Party;

namespace AfterDophamine.Combat
{
    /// <summary>
    /// 현재 필드에 존재하는 적 목록을 추적하고, 파티원의 스킬 발동(OnSkillReady) 이벤트를
    /// 실제 대미지 적용으로 연결한다. 광역기는 필드의 모든 살아있는 적에게 적중한다.
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        [SerializeField] private PartyFormationManager party;
        private readonly List<EnemyUnit> activeEnemies = new List<EnemyUnit>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void RegisterEnemy(EnemyUnit enemy)
        {
            activeEnemies.Add(enemy);
            enemy.OnDeath += _ => activeEnemies.Remove(enemy);
        }

        public void RegisterPartyMember(PartyMember member)
        {
            member.OnSkillReady += HandleSkillReady;
        }

        private void HandleSkillReady(PartyMember caster)
        {
            float damage = caster.ComputeSkillDamage();
            bool isAoe = caster.Data.equippedWeapon?.skill?.isAoe ?? false;

            if (isAoe)
            {
                // 스냅샷을 순회해 컬렉션 변경(사망으로 인한 제거) 중 예외 방지
                var snapshot = new List<EnemyUnit>(activeEnemies);
                foreach (var enemy in snapshot)
                {
                    if (enemy != null && !enemy.IsDead)
                        enemy.TakeSkillDamage(damage);
                }
            }
            else
            {
                var target = GetFrontMostEnemy();
                if (target != null)
                    target.TakeSkillDamage(damage);
            }
        }

        private EnemyUnit GetFrontMostEnemy()
        {
            // 전진 스크롤 상 아군에 가장 가까운(=가장 왼쪽) 적을 우선 타겟
            EnemyUnit closest = null;
            float minX = float.MaxValue;
            foreach (var enemy in activeEnemies)
            {
                if (enemy == null || enemy.IsDead) continue;
                if (enemy.transform.position.x < minX)
                {
                    minX = enemy.transform.position.x;
                    closest = enemy;
                }
            }
            return closest;
        }
    }
}
