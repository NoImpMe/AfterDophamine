using System;
using UnityEngine;

namespace AfterDophamine.Economy
{
    /// <summary>
    /// 방치 중 Gold를 실시간/오프라인으로 누적시키는 매니저.
    /// 최종 수급량 = 기본 수급량 * (1 + 스탯강화 보너스) * (1 + 도감 보너스).
    /// </summary>
    public class IdleCurrencyManager : MonoBehaviour
    {
        [SerializeField] private GachaSystem gachaSystem; // 도감 보너스 조회용
        [SerializeField] private float baseGoldPerSec = 1f;
        [SerializeField, Tooltip("오프라인 보상에 적용할 최대 시간(시간 단위)")]
        private float maxOfflineHours = 12f;

        public int CurrentGold { get; private set; }

        private DateTime lastActiveTimeUtc;

        private void Awake()
        {
            lastActiveTimeUtc = DateTime.UtcNow;
        }

        private void Update()
        {
            CurrentGold += Mathf.RoundToInt(GetEffectiveGoldPerSec() * Time.deltaTime);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) lastActiveTimeUtc = DateTime.UtcNow;
            else ApplyOfflineEarnings();
        }

        private void OnApplicationQuit()
        {
            lastActiveTimeUtc = DateTime.UtcNow;
            // 실제 프로젝트에서는 PlayerPrefs/세이브 파일에 lastActiveTimeUtc 영속화 필요
        }

        public void ApplyOfflineEarnings()
        {
            var elapsed = DateTime.UtcNow - lastActiveTimeUtc;
            float cappedSeconds = (float)Math.Min(elapsed.TotalSeconds, maxOfflineHours * 3600);

            int earned = Mathf.RoundToInt(GetEffectiveGoldPerSec() * cappedSeconds);
            CurrentGold += earned;

            Debug.Log($"[Idle] 오프라인 보상 +{earned} Gold ({cappedSeconds / 3600f:F1}시간)");
            lastActiveTimeUtc = DateTime.UtcNow;
        }

        private float GetEffectiveGoldPerSec()
        {
            float statBonus = 1f + (Core.StatUpgradeStore.attackLevel * 0f); // 골드획득량 강화 레벨 연동 지점
            float dexBonus = 1f + (gachaSystem != null ? gachaSystem.Book.GetTotalGoldBonusPercent() : 0f);
            return baseGoldPerSec * statBonus * dexBonus;
        }
    }
}
