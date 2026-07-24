using System.Collections.Generic;
using UnityEngine;
using AfterDophamine.Combat;
using AfterDophamine.Core;

namespace AfterDophamine.Party
{
    /// <summary>
    /// 3인 파티 [전사]-[궁수]-[마법사] 진형을 관리.
    /// 적은 항상 "살아있는 파티원 중 가장 전열"을 우선 타겟으로 삼는다.
    /// 전사 사망 → 궁수, 궁수 사망 → 마법사 순으로 피격 대상이 넘어간다 (기획서 3.2).
    /// </summary>
    public class PartyFormationManager : MonoBehaviour
    {
        [Header("진형 순서 (0=최전열)")]
        [SerializeField] private List<PartyMember> formation = new List<PartyMember>(3);

        public bool IsWiped => formation.TrueForAll(m => m == null || m.IsDead);

        private void OnEnable()
        {
            foreach (var member in formation)
            {
                if (member != null) member.OnDeath += HandleMemberDeath;
            }
        }

        private void OnDisable()
        {
            foreach (var member in formation)
            {
                if (member != null) member.OnDeath -= HandleMemberDeath;
            }
        }

        /// <summary>role 기준 정렬 순서를 고정: 전사(0) → 궁수(1) → 마법사(2)</summary>
        public void SetupFormation(List<PartyMember> members)
        {
            formation = new List<PartyMember>(members);
            formation.Sort((a, b) => RolePriority(a.Data.source.role).CompareTo(RolePriority(b.Data.source.role)));

            foreach (var member in formation)
                member.OnDeath += HandleMemberDeath;
        }

        private static int RolePriority(CombatRole role) => role switch
        {
            CombatRole.Warrior => 0,
            CombatRole.Archer => 1,
            CombatRole.Mage => 2,
            _ => 99
        };

        /// <summary>적이 공격할 타겟을 고를 때 호출 - 살아있는 최전열 파티원 반환</summary>
        public IReadOnlyList<PartyMember> GetAllMembers() => formation;

        public PartyMember GetCurrentFocusTarget()
        {
            foreach (var member in formation)
            {
                if (member != null && !member.IsDead)
                    return member;
            }
            return null; // 전멸
        }

        private void HandleMemberDeath(CombatUnit unit)
        {
            // 별도 처리 없음: GetCurrentFocusTarget이 다음 생존자를 자동으로 반환하므로
            // 진형 배열을 건드릴 필요가 없다. 여기서는 사망 연출/로그 훅만 남겨둔다.
            Debug.Log($"[Formation] {unit.name} 사망 - 다음 순번으로 피격 대상 이전");

            if (IsWiped)
            {
                StageManagerBridge.NotifyPartyWiped();
            }
        }
    }

    /// <summary>StageManager와의 순환 참조를 피하기 위한 최소 브리지 (실제 프로젝트에선 이벤트버스로 대체 가능)</summary>
    public static class StageManagerBridge
    {
        public static System.Action OnPartyWiped;
        public static void NotifyPartyWiped() => OnPartyWiped?.Invoke();
    }
}
