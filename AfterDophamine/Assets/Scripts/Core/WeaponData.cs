using UnityEngine;

namespace AfterDophamine.Core
{
    /// <summary>
    /// 무기 원본 데이터. 스킬이 캐릭터가 아닌 "무기"에 귀속되므로
    /// 같은 캐릭터라도 어떤 무기를 장착하느냐에 따라 스킬이 달라진다 (기획서 4.2).
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "AfterDophamine/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("식별")]
        public string weaponId;
        public string displayName;
        public Rarity rarity;

        [Header("귀속 스킬")]
        public SkillDefinition skill;

        [Header("개체값 범위")]
        [Tooltip("예: 스킬 데미지 개체값 (180% ~ 250%)")]
        public IVRange skillDamageIV;

        [Header("도감 보너스")]
        public float dexGoldBonusPerRegistration = 0.3f;
    }

    [System.Serializable]
    public class OwnedWeapon
    {
        public WeaponData source;
        public float rolledSkillDamageMultiplier; // 이번 개체의 실제 스킬 데미지 배율(%)

        public OwnedWeapon(WeaponData source)
        {
            this.source = source;
            this.rolledSkillDamageMultiplier = source.skillDamageIV.Roll();
        }
    }

    /// <summary>
    /// 스킬 자체의 정의. ScriptableObject로 분리해 무기마다 스킬을 갈아끼울 수 있게 한다.
    /// SkillEffect는 실제 전투 로직에서 구현하는 인터페이스.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDefinition", menuName = "AfterDophamine/SkillDefinition")]
    public class SkillDefinition : ScriptableObject
    {
        public string skillName;
        public bool isAoe; // 광역 스킬 여부 (쫄몹 학살용)
        public float baseDamagePercent = 100f; // 공격력 대비 %
        public GameObject vfxPrefab; // 연출 프리팹
    }
}
