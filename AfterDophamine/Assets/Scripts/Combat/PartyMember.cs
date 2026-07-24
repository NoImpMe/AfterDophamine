using System;
using UnityEngine;
using AfterDophamine.Core;

namespace AfterDophamine.Combat
{
    /// <summary>
    /// 아군 캐릭터의 전투 인스턴스.
    /// - 기본 공격 없음. 오직 마나 게이지가 100%가 되면 무기 귀속 스킬을 자동 발동한다.
    /// - 각 캐릭터는 완전히 독립된 마나 게이지를 가진다 (파티원끼리 공유 X).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PartyMember : CombatUnit
    {
        public OwnedCharacter Data { get; private set; }
        public int FormationSlot { get; private set; } // 0=전열(전사) ... 2=후열(마법사)

        private float currentMana;
        private float maxMana;
        private float manaRegenPerSec;

        /// <summary>마나가 100% 차서 스킬이 발동될 때 발생. BattleManager가 구독해 실제 대미지를 적용.</summary>
        public event Action<PartyMember> OnSkillReady;

        public void Initialize(OwnedCharacter data, int formationSlot)
        {
            Data = data;
            FormationSlot = formationSlot;

            SetMaxHp(data.FinalMaxHp);
            maxMana = data.FinalMaxMana;
            manaRegenPerSec = data.FinalManaRegen;
            currentMana = 0f;
        }

        private void Update()
        {
            if (IsDead) return;

            currentMana += manaRegenPerSec * Time.deltaTime;
            if (currentMana >= maxMana)
            {
                currentMana = 0f; // 발동 즉시 초기화 (오버플로우 캐리는 기획 의도에 따라 조정 가능)
                OnSkillReady?.Invoke(this);
            }
        }

        public float ManaPercent01 => maxMana <= 0f ? 0f : Mathf.Clamp01(currentMana / maxMana);

        /// <summary>x-10 증강 등으로 마나 수급량을 즉시 배율 조정할 때 사용</summary>
        public void ApplyManaRegenMultiplier(float multiplier)
        {
            manaRegenPerSec *= multiplier;
        }

        public float ComputeSkillDamage()
        {
            var weapon = Data.equippedWeapon;
            var skill = weapon?.skill;
            if (skill == null) return 0f;

            // 무기 개체값(스킬 데미지 %) * 캐릭터 공격력 * 스킬 기본 배율
            float weaponMultiplier = 1f; // OwnedWeapon 롤값은 실제로는 Data 쪽에 보관되도록 확장 가능
            return Data.FinalAttack * (skill.baseDamagePercent / 100f) * weaponMultiplier;
        }
    }
}
