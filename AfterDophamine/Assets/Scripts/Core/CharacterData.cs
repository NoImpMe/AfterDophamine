using UnityEngine;

namespace AfterDophamine.Core
{
    public enum CombatRole
    {
        Warrior,  // 전사 - 최전열, 최우선 피격
        Archer,   // 궁수 - 중열
        Mage      // 마법사 - 후열, 최후 피격
    }

    /// <summary>
    /// 캐릭터 "원본" 정의. 등급별로 완전히 별개의 캐릭터로 존재하므로
    /// (기획서 4.1) Normal 슬라임과 Legendary 슬라임은 서로 다른 CharacterData 애셋이다.
    /// 이 자체는 마스터 데이터이며, 실제 보유 개체는 OwnedCharacter가 감싼다.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterData", menuName = "AfterDophamine/CharacterData")]
    public class CharacterData : ScriptableObject
    {
        [Header("식별")]
        public string characterId;     // 도감/가챠 중복 판정 키
        public string displayName;
        public Rarity rarity;
        public CombatRole role;
        public Sprite portrait;

        [Header("기본 스탯 (Lv.1 기준, Gold 강화 전)")]
        public float baseAttack;
        public float baseDefense;
        public float baseMaxHp;
        public float baseManaRegenPerSec;
        public float baseMaxMana;

        [Header("개체값 범위 (획득 시 이 안에서 랜덤 결정)")]
        public IVRange attackIV;   // 예: 공격력 개체값 (20~30)

        [Header("도감 보너스")]
        [Tooltip("도감에 이 캐릭터가 등록된 횟수당 방치 골드 수급량에 더해지는 영구 증가율(%)")]
        public float dexGoldBonusPerRegistration = 0.5f;
    }

    /// <summary>
    /// 플레이어가 실제로 보유한 캐릭터 개체.
    /// 원본 CharacterData + 이번 획득에서 롤링된 개체값을 함께 들고 있다.
    /// </summary>
    [System.Serializable]
    public class OwnedCharacter
    {
        public CharacterData source;
        public float rolledAttackIV;   // 이 개체의 실제 공격력 개체값
        public int level = 1;
        public WeaponData equippedWeapon; // 무기 귀속 스킬 시스템 (기획서 4.2)

        public OwnedCharacter(CharacterData source)
        {
            this.source = source;
            this.rolledAttackIV = source.attackIV.Roll();
        }

        public float FinalAttack => source.baseAttack + rolledAttackIV + StatUpgradeStore.GetAttackBonus();
        public float FinalDefense => source.baseDefense + StatUpgradeStore.GetDefenseBonus();
        public float FinalMaxHp => source.baseMaxHp + StatUpgradeStore.GetHpBonus();
        public float FinalManaRegen => source.baseManaRegenPerSec + StatUpgradeStore.GetManaRegenBonus();
        public float FinalMaxMana => source.baseMaxMana;
    }

    /// <summary>
    /// Gold로 구매하는 영구 강화치 저장소 (기획서 4.3: 공/방/체/마나회복/소모마나감소/골드획득량).
    /// 실제 구현에서는 세이브 시스템과 연동되는 싱글턴/서비스로 대체 가능.
    /// 여기서는 핵심 흐름을 보여주기 위한 정적 스텁으로 둔다.
    /// </summary>
    public static class StatUpgradeStore
    {
        public static int attackLevel, defenseLevel, hpLevel, manaRegenLevel;

        public static float GetAttackBonus() => attackLevel * 2f;
        public static float GetDefenseBonus() => defenseLevel * 1.5f;
        public static float GetHpBonus() => hpLevel * 10f;
        public static float GetManaRegenBonus() => manaRegenLevel * 0.5f;
    }
}
