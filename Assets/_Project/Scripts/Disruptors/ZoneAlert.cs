using System;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 구역 하나의 경계도다 (22일차, 부록 C.2.1).
    ///
    /// 감시 역할 방해 세력이 발견하면 오르고, 발견이 없으면 천천히 내려간다.
    /// 단계가 바뀌면 <see cref="StageMoments.AlertLevelChanged"/>로 알린다.
    /// 비상에 닿으면 증원을 부르고(구역당 2회) 회수 지점을 잠깐 잠근다.
    ///
    /// 스테이지마다 하나뿐이라 <see cref="Current"/>로 어디서든 읽는다. 방해 세력이 없는 장소에서는 null이다.
    /// </summary>
    public sealed class ZoneAlert : MonoBehaviour
    {
        private static ZoneAlert _current;

        private StageSessionController _stage;
        private int _reinforcementsUsed;

        /// <summary>이번 프레임에 누군가 경계도를 올렸는지다. 올린 프레임에는 내려가지 않는다.</summary>
        private bool _raisedThisFrame;

        public static ZoneAlert Current =>
            _current;

        /// <summary>지금 경계 단계에 따른 플레이어 최면 배율이다. 경계도가 없는 장소에서는 1이다.</summary>
        public static float PlayerHypnosisMultiplier =>
            _current == null
                ? 1f
                : ZoneAlertLogic.GetHypnosisMultiplier(
                    _current.Level);

        public float Value { get; private set; }

        public AlertLevel Level { get; private set; } =
            AlertLevel.Calm;

        public float Normalized =>
            Value / ZoneAlertLogic.Maximum;

        /// <summary>마지막으로 플레이어가 발견된 자리다. 경계 단계에서 감시자가 이리로 온다.</summary>
        public Vector2 LastSeenPosition { get; private set; }

        public bool HasLastSeen { get; private set; }

        public int ReinforcementsUsed =>
            _reinforcementsUsed;

        /// <summary>비상에서 증원을 부를 때 알린다. 스포너가 구독한다.</summary>
        public event Action<Vector2> ReinforcementRequested;

        public void Configure(
            StageSessionController stage)
        {
            _stage = stage;
            _current = this;
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        /// <summary>경계도를 올린다. 음수를 넣으면 내린다(파동으로 감시자를 멍하게 했을 때).</summary>
        public void Add(
            float amount,
            Vector2 seenAt)
        {
            if (_stage != null &&
                !_stage.IsRunning)
            {
                return;
            }

            if (amount > 0f)
            {
                _raisedThisFrame = true;

                LastSeenPosition = seenAt;
                HasLastSeen = true;
            }

            SetValue(
                ZoneAlertLogic.Add(
                    Value,
                    amount));
        }

        /// <summary>디버그 치트용. 경계도를 바로 정한다.</summary>
        public void DebugSet(
            float value)
        {
            SetValue(
                ZoneAlertLogic.Add(
                    0f,
                    value));
        }

        private void LateUpdate()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                _raisedThisFrame = false;

                return;
            }

            if (!_raisedThisFrame &&
                Value > 0f)
            {
                SetValue(
                    ZoneAlertLogic.Decay(
                        Value,
                        BalanceOverrides.StageOrDefault.AlertDecayPerSecond,
                        Time.deltaTime));
            }

            _raisedThisFrame = false;
        }

        private void SetValue(
            float value)
        {
            Value = value;

            AlertLevel next =
                ZoneAlertLogic.GetLevel(
                    Value);

            if (next == Level)
            {
                return;
            }

            AlertLevel previous = Level;

            Level = next;

            StageMoments.RaiseAlertLevelChanged(
                previous,
                next);

            if (next == AlertLevel.Emergency)
            {
                TriggerEmergency();
            }
        }

        private void TriggerEmergency()
        {
            RecoveryPoint.LockAll(
                ZoneAlertLogic.RecoveryLockSeconds);

            if (ZoneAlertLogic.CanReinforce(
                    _reinforcementsUsed))
            {
                _reinforcementsUsed++;

                ReinforcementRequested?.Invoke(
                    LastSeenPosition);
            }

            // 비상은 한 번 터지면 경계로 내려와 다시 쌓여야 한다. 계속 비상에 머물면 증원이 한 번뿐이다.
            Value = ZoneAlertLogic.AfterEmergencyValue;

            AlertLevel previous = Level;

            Level =
                ZoneAlertLogic.GetLevel(
                    Value);

            StageMoments.RaiseAlertLevelChanged(
                previous,
                Level);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
