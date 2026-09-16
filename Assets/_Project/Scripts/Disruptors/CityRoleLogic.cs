using System;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 관문 역할(역무원) 계산이다 (24일차, 부록 C.3 [2]).
    /// 동행자 4명 이하는 그대로 통과하고, 5명 이상이면 1초 간격으로 한 명씩 통과시킨다.
    /// </summary>
    public static class GateLogic
    {
        public const int FreePassLimit = 4;
        public const float PassIntervalSeconds = 1f;

        /// <summary>개찰구 가운데에서 이만큼 안에 있는 동행자를 검사한다.</summary>
        public const float CheckHalfWidth = 2.2f;

        /// <summary>한 번 통과시킨 동행자는 이 시간 동안 다시 세우지 않는다.</summary>
        public const float PassedMemorySeconds = 3f;

        public static bool NeedsCheck(
            int followerCount,
            bool crowdRush)
        {
            // 열차 인파에 섞이면 검사를 생략한다.
            return !crowdRush &&
                   followerCount > FreePassLimit;
        }

        /// <summary>
        /// 동행자가 플레이어와 개찰구 반대편에 있어 아직 건너오지 못했는지다.
        /// 개찰구 한가운데(0.05 이내)는 어느 편도 아니다.
        /// </summary>
        public static bool IsOnOtherSide(
            float gateX,
            float playerX,
            float followerX)
        {
            float player = playerX - gateX;
            float follower = followerX - gateX;

            if (Math.Abs(player) < 0.05f ||
                Math.Abs(follower) < 0.05f)
            {
                return false;
            }

            return Math.Sign(player) != Math.Sign(follower);
        }

        public static bool CanRelease(
            float sinceLastRelease)
        {
            return sinceLastRelease >= PassIntervalSeconds;
        }
    }

    /// <summary>
    /// 구출 역할(퍼스널 트레이너) 계산이다 (24일차, 부록 C.3 [3]).
    /// 담당 회원이 최면되기 시작하면 달려가 어깨를 두드려 게이지를 절반 깎는다.
    /// </summary>
    public static class RescueLogic
    {
        public const float DrainFraction = 0.5f;
        public const float CooldownSeconds = 6f;

        /// <summary>게이지가 이만큼 차야 알아챈다. 막 걸기 시작한 순간은 넘어간다.</summary>
        public const float NoticeThreshold = 0.08f;

        public const float TouchDistance = 1.1f;
        public const float NoticeHalfAngle = 70f;
        public const float NoticeRange = 7f;

        public static bool ShouldRush(
            float hypnosisNormalized,
            bool clientIsNeutral,
            bool canSeeClient,
            float cooldownRemaining)
        {
            return clientIsNeutral &&
                   canSeeClient &&
                   cooldownRemaining <= 0f &&
                   hypnosisNormalized >= NoticeThreshold;
        }

        public static float Drain(
            float current)
        {
            if (float.IsNaN(current) ||
                current <= 0f)
            {
                return 0f;
            }

            return current * (1f - DrainFraction);
        }
    }

    /// <summary>
    /// 힘 대결 역할(헬스 고인물) 계산이다 (24일차, 부록 C.3 [3]).
    ///
    /// 동행자를 데린 플레이어에게 다가와 0.8초 "으랏차!" 자세를 잡은 뒤 밀어붙인다.
    ///   자세 중 대시로 들이받으면  고인물이 30초 퇴장 (받아치기 성공)
    ///   자세가 끝날 때 곁에 있으면  플레이어가 밀려나 1.5초 휘청
    ///   그 사이 거리를 벌리면       헛돎
    /// 기존 힘겨루기(연타)는 경쟁자 전용이라 이 방식으로 대신한다.
    /// </summary>
    public static class BrawlLogic
    {
        public const float EngageDistance = 1.3f;
        public const float WindupSeconds = 0.8f;
        public const float HitDistance = 1.5f;
        public const float StaggerSeconds = 1.5f;
        public const float KnockbackDistance = 1.4f;
        public const float CounterStunSeconds = 30f;
        public const float CooldownSeconds = 4f;

        public enum Outcome
        {
            Miss = 0,
            Hit = 1,
            Countered = 2
        }

        /// <summary>자세가 끝났을 때(또는 자세 중 대시 접촉 시) 결과다.</summary>
        public static Outcome Resolve(
            float distance,
            bool playerDashed)
        {
            if (playerDashed &&
                distance <= HitDistance)
            {
                return Outcome.Countered;
            }

            return distance <= HitDistance
                ? Outcome.Hit
                : Outcome.Miss;
        }
    }

    /// <summary>
    /// 지하철 인파 흐름(승강장 변경 안내) 계산이다 (24일차, 부록 C.3 [2]).
    /// 방송 뒤 5초 동안 동행자가 인파 방향으로 끌려가고 느려진다. 파동을 맞은 동안은 고정된다.
    /// </summary>
    public static class CrowdFlowLogic
    {
        public const float FlowSeconds = 5f;
        public const float DriftSpeed = 1.6f;
        public const float SpeedMultiplier = 0.6f;
        public const float AnchorSeconds = 3f;

        public static float GetDrift(
            bool active,
            bool anchored,
            int direction)
        {
            if (!active ||
                anchored ||
                direction == 0)
            {
                return 0f;
            }

            return Math.Sign(direction) * DriftSpeed;
        }

        public static float GetSpeedMultiplier(
            bool active,
            bool anchored)
        {
            return active && !anchored
                ? SpeedMultiplier
                : 1f;
        }
    }

    /// <summary>
    /// 드론 촬영자(추적 촬영) 계산이다 (24일차, 부록 C.3 [1]).
    /// </summary>
    public static class DroneLogic
    {
        public const float SpotRadius = 2.5f;
        public const float SpotSeconds = 3f;
        public const float TrackSeconds = 6f;
        public const float TrackAlertPerSecond = 6f;
        public const float TrackSpeed = 3.6f;
        public const float OrbitRadiusX = 5f;
        public const float OrbitRadiusY = 1.2f;
        public const float OrbitRadiansPerSecond = 0.35f;

        /// <summary>원형 시야 안에서 쌓이고, 밖에서는 두 배로 빠진다.</summary>
        public static float AdvanceSeen(
            float seen,
            bool inView,
            float deltaTime)
        {
            if (deltaTime <= 0f ||
                float.IsNaN(seen))
            {
                return Math.Max(0f, float.IsNaN(seen) ? 0f : seen);
            }

            float next =
                inView
                    ? seen + deltaTime
                    : seen - deltaTime * 2f;

            return Math.Max(0f, Math.Min(SpotSeconds, next));
        }

        public static bool IsSpotted(
            float seen)
        {
            return seen >= SpotSeconds;
        }

        /// <summary>원 궤도 위 위치(중심 기준)다.</summary>
        public static void GetOrbitOffset(
            float angle,
            out float x,
            out float y)
        {
            x = (float)Math.Cos(angle) * OrbitRadiusX;
            y = (float)Math.Sin(angle) * OrbitRadiusY;
        }
    }

    /// <summary>
    /// 헬스장 · 지하철 특수 능력 수치다 (24일차, 부록 C.3 [2] · [3]).
    /// </summary>
    public static class CityAbilityValues
    {
        /// <summary>단체 PT 호출: 효과 시간 · 충동 배율 · 운동 구역 최면 배율.</summary>
        public const float PtSeconds = 10f;
        public const float PtImpulseMultiplier = 1.6f;
        public const float PtWorkoutHypnosisMultiplier = 1.3f;

        /// <summary>전원 입수: 최면이 막히는 시간이다.</summary>
        public const float DiveSeconds = 5f;

        /// <summary>특수 대상(대회 앞둔 선수): 최면 속도 배율 · 회수 보너스.</summary>
        public const float AthleteHypnosisMultiplier = 0.5f;
        public const int AthleteBonusEssence = 60;
    }
}
