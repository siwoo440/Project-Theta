using System;
using UnityEngine;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 연출이 필요한 순간을 "어디서 일어났는지"와 함께 알린다 (19일차).
    ///
    /// 점수 집계기(<see cref="StageScoreTracker"/>)의 알림은 점수·경험치용이라 위치가 없다.
    /// 파문은 NPC 자리에, 빛기둥은 회수 지점에 떠야 하므로 위치가 필요하다.
    /// 게임 규칙 코드는 여기 한 줄만 호출하고, 연출 담당이 구독한다.
    ///
    /// 정적 이벤트라서 도메인 리로드가 꺼져 있으면 지난 판의 구독이 남는다.
    /// 플레이 진입 시 구독을 모두 비운다.
    /// </summary>
    public static class StageMoments
    {
        /// <summary>최면 성공. (NPC 위치, 재탈환 여부)</summary>
        public static event Action<Vector2, bool> HypnosisSucceeded;

        /// <summary>회수 확정. (회수 위치, 인원, 확정 정기)</summary>
        public static event Action<Vector2, int, int> RecoveryConfirmed;

        /// <summary>동행자가 폭주 준비에 들어갔다. (NPC 위치)</summary>
        public static event Action<Vector2> RampageWindup;

        /// <summary>폭주를 붙잡히지 않고 넘겼다. (폭주한 NPC 위치)</summary>
        public static event Action<Vector2> RampageSurvived;

        /// <summary>플레이어가 붙잡혔다. (플레이어 위치)</summary>
        public static event Action<Vector2> CaptureStarted;

        /// <summary>힘겨루기에서 이겼다. (밀려난 경쟁자 위치)</summary>
        public static event Action<Vector2> DuelWon;

        /// <summary>경쟁자가 플레이어 동행자를 빼앗아 갔다. (NPC 위치) — 20일차 판 기록용</summary>
        public static event Action<Vector2> FollowerStolen;

        public static void RaiseHypnosisSucceeded(
            Vector2 position,
            bool wasReclaim)
        {
            HypnosisSucceeded?.Invoke(
                position,
                wasReclaim);
        }

        public static void RaiseRecoveryConfirmed(
            Vector2 position,
            int count,
            int essence)
        {
            RecoveryConfirmed?.Invoke(
                position,
                count,
                essence);
        }

        public static void RaiseRampageWindup(
            Vector2 position)
        {
            RampageWindup?.Invoke(
                position);
        }

        public static void RaiseRampageSurvived(
            Vector2 position)
        {
            RampageSurvived?.Invoke(
                position);
        }

        public static void RaiseCaptureStarted(
            Vector2 position)
        {
            CaptureStarted?.Invoke(
                position);
        }

        public static void RaiseDuelWon(
            Vector2 position)
        {
            DuelWon?.Invoke(
                position);
        }

        public static void RaiseFollowerStolen(
            Vector2 position)
        {
            FollowerStolen?.Invoke(
                position);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            HypnosisSucceeded = null;
            RecoveryConfirmed = null;
            RampageWindup = null;
            RampageSurvived = null;
            CaptureStarted = null;
            DuelWon = null;
            FollowerStolen = null;
        }
    }
}
