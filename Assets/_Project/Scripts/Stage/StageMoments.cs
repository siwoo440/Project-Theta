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

        // --- 22일차: 방해 세력 ---

        /// <summary>구역 경계도 단계가 바뀌었다. (이전 단계, 새 단계)</summary>
        public static event Action<Disruptors.AlertLevel, Disruptors.AlertLevel> AlertLevelChanged;

        /// <summary>감시자가 플레이어를 발견했다. (감시자 위치)</summary>
        public static event Action<Vector2> DisruptorSpotted;

        /// <summary>특수 개체가 능력 예고를 시작했다. (개체 위치, 능력 이름)</summary>
        public static event Action<Vector2, string> AbilityTelegraphed;

        /// <summary>특수 능력이 발동했다. (효과 위치, 능력 이름)</summary>
        public static event Action<Vector2, string> AbilityFired;

        /// <summary>기업 연수원 쉬는 시간 종이 울렸다.</summary>
        public static event Action BreakTimeStarted;

        // --- 23일차: 해변가 · 야시장 ---

        /// <summary>해변가 밀물 예고가 시작됐다.</summary>
        public static event Action TideWarning;

        /// <summary>소매치기가 정기를 훔쳤다. (소매치기 위치, 훔친 양)</summary>
        public static event Action<Vector2, int> PickpocketStole;

        /// <summary>소매치기 도주가 끝났다. (위치, 잡았는지)</summary>
        public static event Action<Vector2, bool> PickpocketResolved;

        // --- 24일차: 지하철 · 헬스장 ---

        /// <summary>열차가 도착했다. (지금까지 도착한 열차 수)</summary>
        public static event Action<int> TrainArrived;

        /// <summary>특수 대상(선수)을 회수했다. (위치, 보너스 정기)</summary>
        public static event Action<Vector2, int> SpecialTargetRecovered;

        // --- 25일차: 쇼핑몰 · 오피스 ---

        /// <summary>오피스 정전이 시작됐다.</summary>
        public static event Action BlackoutStarted;

        /// <summary>쇼핑몰 폐점 방송이 나왔다.</summary>
        public static event Action MallClosing;

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

        public static void RaiseAlertLevelChanged(
            Disruptors.AlertLevel previous,
            Disruptors.AlertLevel current)
        {
            AlertLevelChanged?.Invoke(
                previous,
                current);
        }

        public static void RaiseDisruptorSpotted(
            Vector2 position)
        {
            DisruptorSpotted?.Invoke(
                position);
        }

        public static void RaiseAbilityTelegraphed(
            Vector2 position,
            string abilityName)
        {
            AbilityTelegraphed?.Invoke(
                position,
                abilityName);
        }

        public static void RaiseAbilityFired(
            Vector2 position,
            string abilityName)
        {
            AbilityFired?.Invoke(
                position,
                abilityName);
        }

        public static void RaiseBreakTimeStarted()
        {
            BreakTimeStarted?.Invoke();
        }

        public static void RaiseTrainArrived(
            int arrived)
        {
            TrainArrived?.Invoke(
                arrived);
        }

        public static void RaiseSpecialTargetRecovered(
            Vector2 position,
            int bonus)
        {
            SpecialTargetRecovered?.Invoke(
                position,
                bonus);
        }

        public static void RaiseBlackoutStarted()
        {
            BlackoutStarted?.Invoke();
        }

        public static void RaiseMallClosing()
        {
            MallClosing?.Invoke();
        }

        public static void RaiseTideWarning()
        {
            TideWarning?.Invoke();
        }

        public static void RaisePickpocketStole(
            Vector2 position,
            int amount)
        {
            PickpocketStole?.Invoke(
                position,
                amount);
        }

        public static void RaisePickpocketResolved(
            Vector2 position,
            bool caught)
        {
            PickpocketResolved?.Invoke(
                position,
                caught);
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
            AlertLevelChanged = null;
            DisruptorSpotted = null;
            AbilityTelegraphed = null;
            AbilityFired = null;
            BreakTimeStarted = null;
            TideWarning = null;
            PickpocketStole = null;
            PickpocketResolved = null;
            TrainArrived = null;
            SpecialTargetRecovered = null;
            BlackoutStarted = null;
            MallClosing = null;
        }
    }
}
