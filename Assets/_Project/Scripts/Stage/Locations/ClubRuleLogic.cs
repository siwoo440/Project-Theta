using System;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 루프탑 클럽 음악 박자 계산이다 (26일차, 부록 B.3 [7]).
    ///
    ///   템포 1 → 2 → 3단계 (DJ가 올린다)
    ///   드롭 간격  8초 → 6초 → 4초
    ///   드롭 창    드롭 직후 1.5초. 이 동안 최면 ×1.5, 클럽 MD 쟁탈 ×3
    ///   드롭 충동  드롭 순간 동행자 전원 +8 → +12 → +16
    ///   조명 점멸  3초마다 0.5초. 이 동안 라이벌의 역최면 시선이 통하지 않는다
    /// </summary>
    public static class ClubBeatLogic
    {
        public const int MinimumTempo = 1;
        public const int MaximumTempo = 3;

        public const float DropWindowSeconds = 1.5f;
        public const float DefaultDropHypnosisMultiplier = 1.5f;
        public const float DropContestMultiplier = 3f;

        public const float FlashPeriodSeconds = 3f;
        public const float FlashSeconds = 0.5f;

        private static readonly float[] DropIntervals = { 8f, 6f, 4f };
        private static readonly float[] DropImpulses = { 8f, 12f, 16f };

        public static int ClampTempo(
            int tempo)
        {
            return Math.Max(MinimumTempo, Math.Min(MaximumTempo, tempo));
        }

        public static float GetDropInterval(
            int tempo)
        {
            return DropIntervals[ClampTempo(tempo) - 1];
        }

        public static float GetDropImpulse(
            int tempo)
        {
            return DropImpulses[ClampTempo(tempo) - 1];
        }

        /// <summary>마지막 드롭 뒤 흐른 시간으로 드롭 창 안인지 본다.</summary>
        public static bool IsDropWindow(
            float sinceDrop,
            bool hasDropped)
        {
            return hasDropped &&
                   sinceDrop >= 0f &&
                   sinceDrop < DropWindowSeconds;
        }

        public static bool IsFlash(
            float elapsed)
        {
            if (elapsed < 0f ||
                float.IsNaN(elapsed))
            {
                return false;
            }

            return elapsed % FlashPeriodSeconds < FlashSeconds;
        }

        public static float GetHypnosisMultiplier(
            bool dropWindow,
            float dropScale)
        {
            return dropWindow
                ? Math.Max(1f, float.IsNaN(dropScale) ? DefaultDropHypnosisMultiplier : dropScale)
                : 1f;
        }

        public static float GetContestMultiplier(
            bool dropWindow)
        {
            return dropWindow
                ? DropContestMultiplier
                : 1f;
        }
    }

    /// <summary>
    /// 바운서 계산이다 (26일차, 부록 C.3 [7]).
    /// 동행자가 6명을 넘으면 VIP 라운지(위층)로 들여보내지 않는다. VIP 게스트를 데리고 있으면 무조건 통과한다.
    /// </summary>
    public static class BouncerLogic
    {
        public const int MaximumGuests = 6;

        public static bool IsBlocked(
            int followerCount,
            bool hasVipGuest,
            bool bouncerAway)
        {
            return !bouncerAway &&
                   !hasVipGuest &&
                   followerCount > MaximumGuests;
        }
    }

    /// <summary>
    /// 루프탑 클럽 자리표다 (26일차). 1F 입구 · 바, 2F 댄스플로어 · DJ 부스 · 바.
    /// </summary>
    public static class ClubLayout
    {
        public const float DjBoothX = -9f;
        public const float BarX = 9f;
        public const float DanceFloorMinX = -6f;
        public const float DanceFloorMaxX = 7f;
        public const float VipGuestX = -4f;
        public const float BossStartX = 4f;

        /// <summary>1F 바 자리다. 바텐더가 선다.</summary>
        public const float EntranceBarX = 6f;

        public static bool IsOnDanceFloor(
            float x)
        {
            return x >= DanceFloorMinX &&
                   x <= DanceFloorMaxX;
        }
    }
}
