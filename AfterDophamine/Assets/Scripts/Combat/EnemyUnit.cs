using System.Collections;
using UnityEngine;
using AfterDophamine.Party;

namespace AfterDophamine.Combat
{
    public enum EnemyKind { Minion, StageBoss, MidBoss, FinalBoss }

    /// <summary>
    /// 쫄몹 / 스테이지 보스 / (x-10의) 중간보스 / 최종보스 공용 클래스.
    /// 일정 주기로 파티의 최전열 타겟을 공격한다.
    /// </summary>
    public class EnemyUnit : CombatUnit
    {
        [SerializeField] private EnemyKind kind;
        [SerializeField] private float attackDamage;
        [SerializeField] private float attackDefense;
        [SerializeField] private float attackIntervalSec = 1.5f;

        private PartyFormationManager formation;
        private Coroutine attackRoutine;

        public EnemyKind Kind => kind;

        public void Initialize(PartyFormationManager targetFormation, float hp, float atk, float def)
        {
            formation = targetFormation;
            SetMaxHp(hp);
            attackDamage = atk;
            attackDefense = def;
        }

        private void OnEnable()
        {
            attackRoutine = StartCoroutine(AttackLoop());
        }

        private void OnDisable()
        {
            if (attackRoutine != null) StopCoroutine(attackRoutine);
        }

        // while(true) 폴링 대신 코루틴 기반 대기 - 에디터 멈춤 방지
        private IEnumerator AttackLoop()
        {
            var wait = new WaitForSeconds(attackIntervalSec);
            while (!IsDead)
            {
                var target = formation.GetCurrentFocusTarget();
                if (target != null)
                {
                    target.TakeDamage(attackDamage, target.Data.FinalDefense);
                }
                yield return wait;
            }
        }

        public void TakeSkillDamage(float amount)
        {
            TakeDamage(amount, attackDefense); // 몬스터 방어 계수는 필요 시 확장
        }
    }
}
