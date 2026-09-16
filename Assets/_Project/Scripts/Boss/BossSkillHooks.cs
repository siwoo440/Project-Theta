using UnityEngine;
using ProjectTheta.Companion;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 보스 기술을 붙이는 자리다 (26일차).
    /// 26일차에는 역최면 시선 · VIP 매혹 파동만 쓰고, 27일차에 나머지 기술 3개를 여기서 붙인다.
    /// </summary>
    public static class BossSkillHooks
    {
        public static void AddLaterSkills(
            GameObject boss,
            BossBattle battle,
            Transform player,
            FollowerManager followers)
        {
        }
    }
}
