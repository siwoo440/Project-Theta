using UnityEngine;
using ProjectTheta.Companion;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 보스 기술을 붙이는 자리다 (26일차).
    /// 26일차: 역최면 시선 · VIP 매혹 파동 (본체가 직접 붙인다)
    /// 27일차: 샴페인 타워 · 분신 댄서 · VIP 전용 구역 (여기서 붙인다)
    /// </summary>
    public static class BossSkillHooks
    {
        public static void AddLaterSkills(
            GameObject boss,
            BossBattle battle,
            Transform player,
            FollowerManager followers)
        {
            boss.AddComponent<ChampagneTowerAbility>().Configure(battle);
            boss.AddComponent<CloneDancerAbility>().Configure(player, battle);
            boss.AddComponent<VipZoneAbility>().Configure(player, battle);
        }
    }
}
