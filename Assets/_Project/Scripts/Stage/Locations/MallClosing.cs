using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 쇼핑몰 폐점 방송이다 (25일차, 부록 C.3 [5]).
    /// 제한 시간의 70%가 지나면 폐점 방송이 나오고, 이후 남은 시간이 1.3배 빨리 흐른다.
    /// </summary>
    public sealed class MallClosing : MonoBehaviour
    {
        private static MallClosing _current;

        private StageSessionController _stage;

        public static MallClosing Current =>
            _current;

        public bool HasAnnounced { get; private set; }

        public float SecondsUntilAnnouncement =>
            _stage == null
                ? 0f
                : Mathf.Max(
                    0f,
                    _stage.TimeLimitSeconds * ClosingLogic.AnnounceAt - _stage.ElapsedTime);

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

        /// <summary>디버그 치트: 바로 폐점 방송을 낸다.</summary>
        public void DebugAnnounce()
        {
            Announce();
        }

        private void Update()
        {
            if (HasAnnounced ||
                _stage == null ||
                !_stage.IsRunning ||
                GameplayPause.IsPaused)
            {
                return;
            }

            if (ClosingLogic.ShouldAnnounce(
                    _stage.ElapsedTime,
                    _stage.TimeLimitSeconds))
            {
                Announce();
            }
        }

        private void Announce()
        {
            if (HasAnnounced ||
                _stage == null)
            {
                return;
            }

            HasAnnounced = true;

            _stage.TimeFlowMultiplier =
                Mathf.Max(
                    1f,
                    Balance.BalanceOverrides.StageOrDefault.ClosingTimeScale);

            StageMoments.RaiseMallClosing();

            GameAudio.Play(GameSfx.UiStamp);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
