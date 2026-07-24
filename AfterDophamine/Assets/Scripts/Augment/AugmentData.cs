using UnityEngine;

namespace AfterDophamine.Augment
{
    public enum AugmentEffectType
    {
        HealPercent,        // 체력 50% 회복 등 즉시 효과
        CritChanceUp,       // 크리티컬 확률 증가 (지속)
        ManaRegenUp,        // 마나 수급량 폭발적 증가 (지속)
        AttackUp,           // 공격력 증가 (지속)
        DefenseUp           // 방어력 증가 (지속)
    }

    [CreateAssetMenu(fileName = "AugmentData", menuName = "AfterDophamine/AugmentData")]
    public class AugmentData : ScriptableObject
    {
        public string augmentName;
        [TextArea] public string description;
        public AugmentEffectType effectType;
        public float value; // 효과 수치 (예: 0.5 = 50%, 1.3 = 30% 증가 배율)
        [Range(0, 100)] public int weight = 10; // 가중치 기반 랜덤 추첨용
    }
}
