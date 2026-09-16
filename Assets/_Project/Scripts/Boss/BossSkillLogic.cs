using System;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 27일차 보스 기술 수치다 (부록 C.3 [7]).
    ///   샴페인 타워   10초 동안 바 쪽으로 손님이 모이고 라이벌 확보 속도 ×2. 바텐더를 최면하면 즉시 무너짐
    ///   분신 댄서     분신 2명(15초). 분신도 시선을 쏘지만 피해 절반. 본체만 그림자가 있다. 파동 한 번에 소멸
    ///   VIP 전용 구역  15초 동안 댄스플로어 일부(가로 5m)에서 플레이어 최면 ×0.5. DJ가 멍하면 절반 시간
    /// </summary>
    public static class LateBossSkillValues
    {
        public const float TowerTelegraphSeconds = 1.5f;
        public const float TowerSeconds = 10f;
        public const float TowerClaimRate = 2f;
        public const float TowerPullSpeed = 1.2f;
        public const float TowerPullRange = 9f;
        public const float TowerCooldownSeconds = 28f;

        public const float CloneTelegraphSeconds = 1f;
        public const int CloneCount = 2;
        public const float CloneSeconds = 15f;
        public const float CloneDamageScale = 0.5f;
        public const float CloneCooldownSeconds = 32f;

        public const float ZoneTelegraphSeconds = 1.5f;
        public const float ZoneSeconds = 15f;
        public const float ZoneHalfWidth = 2.5f;
        public const float ZoneHypnosisMultiplier = 0.5f;
        public const float ZoneCooldownSeconds = 26f;

        public static float GetZoneSeconds(
            bool djStunned)
        {
            return djStunned
                ? ZoneSeconds * 0.5f
                : ZoneSeconds;
        }

        public static bool IsInZone(
            float x,
            float zoneCenterX,
            int floor,
            int zoneFloor)
        {
            return floor == zoneFloor &&
                   Math.Abs(x - zoneCenterX) <= ZoneHalfWidth;
        }

        public static float GetClaimRate(
            bool towerUp)
        {
            return towerUp
                ? TowerClaimRate
                : 1f;
        }
    }
}
