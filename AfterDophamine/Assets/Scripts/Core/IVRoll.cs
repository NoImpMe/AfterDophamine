using UnityEngine;

namespace AfterDophamine.Core
{
    /// <summary>
    /// 캐릭터/무기 공통 "개체값(IV)" 롤링 구조체.
    /// 기획서 4.1~4.2: 같은 캐릭터/무기라도 획득 시마다 능력치 상한선이
    /// (min ~ max) 범위 내에서 무작위로 결정된다.
    /// 예) 공격력 개체값 20~30, 스킬 데미지 개체값 180%~250%
    /// </summary>
    [System.Serializable]
    public struct IVRange
    {
        public float min;
        public float max;

        public IVRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        /// <summary>실제 획득 시 굴리는 랜덤 개체값</summary>
        public float Roll() => Random.Range(min, max);

        /// <summary>0~1 사이 정규화된 "얼마나 좋은 개체값인가" (도감/UI 표시용)</summary>
        public float NormalizedQuality(float rolledValue)
        {
            if (Mathf.Approximately(max, min)) return 1f;
            return Mathf.InverseLerp(min, max, rolledValue);
        }
    }

    public enum Rarity
    {
        Normal,
        Rare,
        Epic,
        Unique,
        Legendary
    }
}
