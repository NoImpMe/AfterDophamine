using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AfterDophamine.Core;

namespace AfterDophamine.Economy
{
    public enum GachaBankType { Character, Weapon }

    [Serializable]
    public struct RarityWeight
    {
        public Rarity rarity;
        public float weight;
    }

    /// <summary>
    /// Gem으로 진행하는 [캐릭터] / [무기] 분리 가챠 (기획서 4.3).
    /// 이미 보유 중인 항목이 다시 뽑히면 새 개체를 만들지 않고 도감에 등록한다.
    /// </summary>
    public class GachaSystem : MonoBehaviour
    {
        [SerializeField] private List<CharacterData> characterPool;
        [SerializeField] private List<WeaponData> weaponPool;
        [SerializeField] private List<RarityWeight> rarityWeights;

        private readonly CollectionBook collectionBook = new CollectionBook();
        private readonly Dictionary<string, OwnedCharacter> ownedCharacters = new Dictionary<string, OwnedCharacter>();
        private readonly Dictionary<string, OwnedWeapon> ownedWeapons = new Dictionary<string, OwnedWeapon>();

        public CollectionBook Book => collectionBook;

        public event Action<OwnedCharacter, bool /*isNew*/> OnCharacterPulled;
        public event Action<OwnedWeapon, bool /*isNew*/> OnWeaponPulled;

        public void PullCharacter(int gemCost, ref int playerGems)
        {
            if (playerGems < gemCost) { Debug.LogWarning("Gem 부족"); return; }
            playerGems -= gemCost;

            Rarity rarity = RollRarity();
            var candidates = characterPool.Where(c => c.rarity == rarity).ToList();
            if (candidates.Count == 0) candidates = characterPool; // 세이프가드
            var picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            bool isNew = !ownedCharacters.ContainsKey(picked.characterId);
            if (isNew)
            {
                var owned = new OwnedCharacter(picked);
                ownedCharacters[picked.characterId] = owned;
                OnCharacterPulled?.Invoke(owned, true);
            }
            else
            {
                collectionBook.RegisterCharacterDuplicate(picked.characterId, picked.dexGoldBonusPerRegistration);
                OnCharacterPulled?.Invoke(ownedCharacters[picked.characterId], false);
            }
        }

        public void PullWeapon(int gemCost, ref int playerGems)
        {
            if (playerGems < gemCost) { Debug.LogWarning("Gem 부족"); return; }
            playerGems -= gemCost;

            Rarity rarity = RollRarity();
            var candidates = weaponPool.Where(w => w.rarity == rarity).ToList();
            if (candidates.Count == 0) candidates = weaponPool;
            var picked = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            bool isNew = !ownedWeapons.ContainsKey(picked.weaponId);
            if (isNew)
            {
                var owned = new OwnedWeapon(picked);
                ownedWeapons[picked.weaponId] = owned;
                OnWeaponPulled?.Invoke(owned, true);
            }
            else
            {
                collectionBook.RegisterWeaponDuplicate(picked.weaponId, picked.dexGoldBonusPerRegistration);
                OnWeaponPulled?.Invoke(ownedWeapons[picked.weaponId], false);
            }
        }

        private Rarity RollRarity()
        {
            float totalWeight = rarityWeights.Sum(r => r.weight);
            float roll = UnityEngine.Random.Range(0, totalWeight);
            float cumulative = 0f;

            foreach (var rw in rarityWeights)
            {
                cumulative += rw.weight;
                if (roll < cumulative) return rw.rarity;
            }
            return Rarity.Normal;
        }
    }
}
