using System.Collections.Generic;

namespace AfterDophamine.Economy
{
    /// <summary>
    /// 캐릭터/무기 도감. 이미 보유한 개체를 중복 획득했을 때 이곳에 등록되며,
    /// 등록 횟수에 비례해 방치 골드 수급량이 영구적으로 증가한다.
    /// 뽑기 중복에 대한 손해감을 제거하는 것이 목적 (기획서 4.1).
    /// </summary>
    public class CollectionBook
    {
        // key: characterId 또는 weaponId, value: 등록 횟수
        private readonly Dictionary<string, int> characterRegistrations = new Dictionary<string, int>();
        private readonly Dictionary<string, int> weaponRegistrations = new Dictionary<string, int>();

        // key: id, value: 개별 도감 보너스율 (CharacterData.dexGoldBonusPerRegistration 등에서 가져옴)
        private readonly Dictionary<string, float> characterBonusRate = new Dictionary<string, float>();
        private readonly Dictionary<string, float> weaponBonusRate = new Dictionary<string, float>();

        public void RegisterCharacterDuplicate(string characterId, float bonusPerRegistration)
        {
            characterRegistrations.TryGetValue(characterId, out int count);
            characterRegistrations[characterId] = count + 1;
            characterBonusRate[characterId] = bonusPerRegistration;
        }

        public void RegisterWeaponDuplicate(string weaponId, float bonusPerRegistration)
        {
            weaponRegistrations.TryGetValue(weaponId, out int count);
            weaponRegistrations[weaponId] = count + 1;
            weaponBonusRate[weaponId] = bonusPerRegistration;
        }

        /// <summary>도감으로부터 얻는 방치 골드 수급량 총 증가율(%). 예: 0.35 = +35%</summary>
        public float GetTotalGoldBonusPercent()
        {
            float total = 0f;

            foreach (var kv in characterRegistrations)
            {
                float rate = characterBonusRate.TryGetValue(kv.Key, out var r) ? r : 0f;
                total += rate * kv.Value / 100f; // dexGoldBonusPerRegistration은 % 단위로 정의됨
            }
            foreach (var kv in weaponRegistrations)
            {
                float rate = weaponBonusRate.TryGetValue(kv.Key, out var r) ? r : 0f;
                total += rate * kv.Value / 100f;
            }

            return total;
        }

        public int GetCharacterRegistrationCount(string characterId) =>
            characterRegistrations.TryGetValue(characterId, out int c) ? c : 0;

        public int GetWeaponRegistrationCount(string weaponId) =>
            weaponRegistrations.TryGetValue(weaponId, out int c) ? c : 0;
    }
}
