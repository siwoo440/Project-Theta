using System;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 자산에서 읽은 경쟁자 수치를 담아두는 주입 지점이다.
    /// 값이 없으면 각 컨트롤러의 CreateDefaultTuning()이 그대로 쓰인다.
    /// </summary>
    public static class OpponentTuningOverrides
    {
        public static OpponentTuning Geumtaeyang { get; set; }

        public static OpponentTuning PopularGuy { get; set; }

        public static OpponentTuning Get(
            NpcOwner ownerTag)
        {
            switch (ownerTag)
            {
                case NpcOwner.Geumtaeyang:
                    return Geumtaeyang;

                case NpcOwner.PopularGuy:
                    return PopularGuy;

                default:
                    return null;
            }
        }

        public static void Clear()
        {
            Geumtaeyang = null;
            PopularGuy = null;
        }
    }

    /// <summary>
    /// 경쟁자 한 종류의 행동 수치를 한 곳에 모은 설정 블록이다.
    /// 금태양과 인기남의 차이는 대부분 이 값들의 차이로만 표현한다.
    /// </summary>
    [Serializable]
    public sealed class OpponentTuning
    {
        public float MoveSpeed = 4.2f;
        public float StopDistance = 0.92f;

        public float SearchRange = 10f;
        public float ReacquireInterval = 0.30f;
        public float AbandonDistance = 13f;
        public float MaximumPursuitDuration = 5.0f;

        public float MinimumIdleDuration = 0.6f;
        public float MaximumIdleDuration = 1.5f;
        public float LostTargetIdleMinimum = 0.4f;
        public float LostTargetIdleMaximum = 0.9f;
        public float PostCaptureIdleMinimum = 0.5f;
        public float PostCaptureIdleMaximum = 1.2f;

        public float ActionDistance = 1.20f;
        public float ContestDrainPerSecond = 18f;
    }
}
