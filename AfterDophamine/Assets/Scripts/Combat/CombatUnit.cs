using System;
using UnityEngine;

namespace AfterDophamine.Combat
{
    /// <summary>
    /// 아군(PartyMember)과 적(EnemyUnit)이 공유하는 전투 유닛 베이스.
    /// HP 변경/사망은 이벤트로 노출해 HP바 UI 등이 디커플링된 상태로 구독하게 한다
    /// (Observer 패턴, C# event 방식).
    /// </summary>
    public abstract class CombatUnit : MonoBehaviour
    {
        [SerializeField] protected float maxHp;
        protected float currentHp;

        public float MaxHp => maxHp;
        public float CurrentHp => currentHp;
        public bool IsDead => currentHp <= 0f;

        /// <summary>(현재HP, 최대HP) - HP바 등 UI 바인더가 구독</summary>
        public event Action<float, float> OnHpChanged;
        public event Action<CombatUnit> OnDeath;

        protected virtual void Awake()
        {
            currentHp = maxHp;
        }

        public virtual void TakeDamage(float rawDamage, float defense = 0f)
        {
            if (IsDead) return;

            float mitigated = Mathf.Max(1f, rawDamage - defense); // 최소 1 데미지 보장
            currentHp = Mathf.Max(0f, currentHp - mitigated);
            OnHpChanged?.Invoke(currentHp, maxHp);

            if (currentHp <= 0f)
            {
                RaiseOnDeath();
            }
        }

        public virtual void Heal(float amount)
        {
            if (IsDead) return;
            currentHp = Mathf.Min(maxHp, currentHp + amount);
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        public void SetMaxHp(float newMaxHp, bool fillCurrent = true)
        {
            maxHp = newMaxHp;
            if (fillCurrent) currentHp = maxHp;
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        /// <summary>
        /// 서브클래스나 외부에서 직접 사망 처리가 필요할 때 호출.
        /// (컴파일 에러 방지를 위해 protected로 노출 - 이벤트는 선언 클래스에서만 invoke 가능)
        /// </summary>
        protected void RaiseOnDeath()
        {
            OnDeath?.Invoke(this);
        }
    }
}
