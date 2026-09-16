using System;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 쇼핑몰 보안 무전 계산이다 (25일차, 부록 C.3 [5]).
    /// 한 명이 발견하면 같은 층 보안요원이 모두 모이고, 요원 한 명이 멍해지면 그 층 무전이 5초 끊긴다.
    /// </summary>
    public static class RadioLogic
    {
        public const float CutSeconds = 5f;
        public const float RespondSeconds = 5f;

        public static bool CanRelay(
            int spottedFloor,
            int listenerFloor,
            float cutRemaining)
        {
            return spottedFloor == listenerFloor &&
                   cutRemaining <= 0f;
        }
    }

    /// <summary>
    /// 보안팀장 셔터 봉쇄 계산이다 (25일차, 부록 C.3 [5]).
    /// 경계도가 경계(60) 이상일 때만 발동하고, 닫힌 셔터는 캐릭터를 원래 쪽으로 밀어낸다.
    /// </summary>
    public static class ShutterLogic
    {
        public const float CloseSeconds = 12f;
        public const float BlockHalfWidth = 0.5f;

        public static bool CanTrigger(
            AlertLevel level)
        {
            return level >= AlertLevel.Alert;
        }

        /// <summary>닫힌 셔터에 닿은 캐릭터를 셔터 밖으로 민 x다. 닿지 않았으면 그대로다.</summary>
        public static float PushOut(
            float shutterX,
            float x)
        {
            float dx = x - shutterX;

            if (Math.Abs(dx) >= BlockHalfWidth)
            {
                return x;
            }

            return dx >= 0f
                ? shutterX + BlockHalfWidth
                : shutterX - BlockHalfWidth;
        }
    }

    /// <summary>
    /// 꼰대 부장 계산이다 (25일차, 부록 C.3 [6]). 범위형 구출 역할이다.
    ///   반경 4m 안에서 최면이 진행 중인 NPC가 있으면 "이거 오늘까지 해!" — 범위 안 게이지 모두 0 (재사용 8초)
    ///   25초마다 탕비실에 가서 10초 쉰다. 쉬는 동안은 외치지 않는다.
    /// </summary>
    public static class ManagerLogic
    {
        public const float ShoutRadius = 4f;
        public const float ShoutCooldownSeconds = 8f;
        public const float NoticeThreshold = 0.08f;
        public const float BreakIntervalSeconds = 25f;
        public const float BreakSeconds = 10f;

        public static bool IsOnBreak(
            float elapsed)
        {
            if (elapsed < BreakIntervalSeconds ||
                float.IsNaN(elapsed))
            {
                return false;
            }

            return elapsed % (BreakIntervalSeconds + BreakSeconds) >= BreakIntervalSeconds;
        }

        public static bool ShouldShout(
            float hypnosisNormalized,
            float distance,
            bool onBreak,
            float cooldownRemaining)
        {
            return !onBreak &&
                   cooldownRemaining <= 0f &&
                   distance <= ShoutRadius &&
                   hypnosisNormalized >= NoticeThreshold;
        }
    }

    /// <summary>
    /// 비서실장 긴급 회의 소집 계산이다 (25일차, 부록 C.3 [6]).
    /// 동행자 중 직원만 최대 2명 회의실로 끌려가고, 12초 뒤 돌아온다. 그 사이 회의실에 들어가면 경계도 +40.
    /// </summary>
    public static class MeetingLogic
    {
        public const int MaximumSummoned = 2;
        public const float MeetingSeconds = 12f;
        public const float IntrusionAlertRise = 40f;
        public const float RoomHalfWidth = 1.6f;
        public const float WalkSpeed = 3f;

        public static int GetSummonCount(
            int employeeFollowers)
        {
            return Math.Max(0, Math.Min(MaximumSummoned, employeeFollowers));
        }
    }

    /// <summary>
    /// 출입증 게이트 계산이다 (25일차, 부록 C.3 [6]).
    /// 출입증 NPC를 데리고 그 층에 있으면 열리고, 한 번 열리면 계속 열려 있다.
    /// 잠긴 계단은 한 번 더 누르면 비상계단으로 우회하되 경계도가 오른다.
    /// </summary>
    public static class PassGateLogic
    {
        public const float EmergencyStairAlertRise = 20f;

        /// <summary>잠긴 계단을 두 번째로 눌러야 하는 시간이다.</summary>
        public const float EmergencyConfirmSeconds = 2.5f;

        public static bool ShouldOpen(
            bool alreadyOpen,
            bool playerOnGateFloor,
            bool hasPassHolder)
        {
            return alreadyOpen ||
                   (playerOnGateFloor && hasPassHolder);
        }
    }

    /// <summary>
    /// 쇼핑몰 · 오피스 수치다 (25일차).
    /// </summary>
    public static class MallOfficeValues
    {
        /// <summary>보안실 직원을 최면하면 그 층 CCTV가 꺼지는 시간이다.</summary>
        public const float CameraOffSeconds = 30f;

        /// <summary>탕비실 근처에서 사내 인기남 쟁탈 속도 배율 · 범위다.</summary>
        public const float PantryContestMultiplier = 2f;
        public const float PantryHalfWidth = 3f;

        /// <summary>오피스 특수 대상(대표 비서) 회수 보너스다.</summary>
        public const int ExecutiveSecretaryBonus = 80;

        public static float GetPantryMultiplier(
            float x,
            float pantryX,
            bool hasPantry)
        {
            return hasPantry &&
                   Math.Abs(x - pantryX) <= PantryHalfWidth
                ? PantryContestMultiplier
                : 1f;
        }
    }
}
