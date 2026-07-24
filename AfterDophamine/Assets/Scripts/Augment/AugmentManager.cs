using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AfterDophamine.Combat;
using AfterDophamine.Party;

namespace AfterDophamine.Augment
{
    /// <summary>
    /// x-10 클라이맥스 스테이지에서 중간보스를 처치할 때마다 호출된다.
    /// 3가지 무작위 증강 카드를 (가중치 기반) 뽑아 UI에 제시하고, 플레이어의 선택을 기다린 뒤
    /// 파티 전원에게 즉시/지속 효과를 적용한다. 선택된 증강은 이번 클라이맥스 런 동안 누적된다.
    /// </summary>
    public class AugmentManager : MonoBehaviour
    {
        [SerializeField] private List<AugmentData> augmentPool;

        /// <summary>UI 레이어가 이 이벤트를 구독해 선택창을 띄우고, 선택 완료 시 ResolveChoice를 호출한다.</summary>
        public event Action<List<AugmentData>> OnPresentChoices;

        private readonly List<AugmentData> acquiredThisRun = new List<AugmentData>();
        private AugmentData pendingChoice;
        private bool waitingForChoice;

        public IEnumerator PresentChoicesAndWait(PartyFormationManager party)
        {
            var choices = DrawWeightedChoices(count: 3);
            waitingForChoice = true;
            pendingChoice = null;

            OnPresentChoices?.Invoke(choices);

            while (waitingForChoice)
                yield return null;

            ApplyAugment(pendingChoice, party);
            acquiredThisRun.Add(pendingChoice);
        }

        /// <summary>UI(버튼 클릭 등)에서 선택 결과를 전달할 때 호출</summary>
        public void ResolveChoice(AugmentData chosen)
        {
            pendingChoice = chosen;
            waitingForChoice = false;
        }

        private List<AugmentData> DrawWeightedChoices(int count)
        {
            var pool = new List<AugmentData>(augmentPool);
            var result = new List<AugmentData>();

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int totalWeight = pool.Sum(a => a.weight);
                int roll = UnityEngine.Random.Range(0, totalWeight);
                int cumulative = 0;

                foreach (var candidate in pool)
                {
                    cumulative += candidate.weight;
                    if (roll < cumulative)
                    {
                        result.Add(candidate);
                        pool.Remove(candidate);
                        break;
                    }
                }
            }
            return result;
        }

        private void ApplyAugment(AugmentData augment, PartyFormationManager party)
        {
            if (augment == null) return;

            var members = party.GetAllMembers();
            foreach (var member in members)
            {
                if (member == null || member.IsDead) continue;

                switch (augment.effectType)
                {
                    case AugmentEffectType.HealPercent:
                        member.Heal(member.MaxHp * augment.value);
                        break;
                    case AugmentEffectType.ManaRegenUp:
                        member.ApplyManaRegenMultiplier(1f + augment.value);
                        break;
                    case AugmentEffectType.CritChanceUp:
                    case AugmentEffectType.AttackUp:
                    case AugmentEffectType.DefenseUp:
                        // 지속 스탯 버프는 BattleStatModifier 등 별도 버프 스택 컴포넌트로 확장 권장
                        Debug.Log($"[Augment] {augment.augmentName} 적용 (+{augment.value})");
                        break;
                }
            }
        }

        /// <summary>다음 x단계로 넘어갈 때 (StageManager.StartRun) 이번 런의 증강을 전부 초기화</summary>
        public void ClearAllAugments()
        {
            acquiredThisRun.Clear();
        }

        public IReadOnlyList<AugmentData> AcquiredThisRun => acquiredThisRun;
    }
}
