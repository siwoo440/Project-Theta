using UnityEngine;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 플레이어의 두 번째 자원인 집중력이다.
    /// 최면 유지·대시·최면 파동·체인 최면이 모두 이 자원을 쓴다.
    /// </summary>
    public sealed class PlayerFocus : MonoBehaviour
    {
        [SerializeField] private float _maximumFocus = 100f;
        [SerializeField] private float _hypnosisDrainPerSecond = 6f;
        [SerializeField] private float _dashCost = 12f;
        [SerializeField] private float _recoveryPerSecond = 14f;
        [SerializeField] private float _recoveryDelay = 0.8f;

        /// <summary>고갈된 뒤 다시 최면을 시작할 수 있는 최소 집중력이다.</summary>
        [SerializeField] private float _resumeThreshold = 30f;

        private float _secondsSinceSpend;
        private bool _exhausted;

        public float CurrentFocus { get; private set; }

        public float MaximumFocus =>
            Mathf.Max(
                1f,
                _maximumFocus);

        public float FocusNormalized =>
            FocusLogic.Normalized(
                CurrentFocus,
                MaximumFocus);

        public float HypnosisDrainPerSecond =>
            Mathf.Max(
                0f,
                _hypnosisDrainPerSecond);

        public float DashCost =>
            Mathf.Max(
                0f,
                _dashCost);

        public float ResumeThreshold =>
            Mathf.Max(
                0f,
                _resumeThreshold);

        /// <summary>집중력이 바닥나 최면이 잠긴 상태다.</summary>
        public bool IsExhausted =>
            _exhausted;

        public bool CanCast =>
            !_exhausted;

        private void Awake()
        {
            CurrentFocus =
                MaximumFocus;
        }

        private void Update()
        {
            _secondsSinceSpend +=
                Time.deltaTime;

            if (!FocusLogic.ShouldRecover(
                    _secondsSinceSpend,
                    _recoveryDelay))
            {
                return;
            }

            CurrentFocus =
                FocusLogic.Recover(
                    CurrentFocus,
                    MaximumFocus,
                    _recoveryPerSecond,
                    Time.deltaTime);

            if (_exhausted &&
                FocusLogic.CanResume(
                    CurrentFocus,
                    ResumeThreshold))
            {
                _exhausted =
                    false;
            }
        }

        /// <summary>지속 소모다. 집중력이 바닥나면 false를 반환하고 최면을 잠근다.</summary>
        public bool DrainContinuous(
            float perSecond,
            float deltaTime)
        {
            CurrentFocus =
                FocusLogic.DrainPerSecond(
                    CurrentFocus,
                    perSecond,
                    deltaTime);

            _secondsSinceSpend =
                0f;

            if (FocusLogic.IsDepleted(
                    CurrentFocus))
            {
                _exhausted =
                    true;

                return false;
            }

            return true;
        }

        /// <summary>즉시 소모다. 집중력이 모자라면 아무것도 쓰지 않고 false를 반환한다.</summary>
        public bool TrySpend(
            float amount)
        {
            if (_exhausted ||
                !FocusLogic.CanAfford(
                    CurrentFocus,
                    amount))
            {
                return false;
            }

            CurrentFocus =
                FocusLogic.Drain(
                    CurrentFocus,
                    amount);

            _secondsSinceSpend =
                0f;

            if (FocusLogic.IsDepleted(
                    CurrentFocus))
            {
                _exhausted =
                    true;
            }

            return true;
        }

        public void Refill(
            float amount)
        {
            CurrentFocus =
                FocusLogic.Recover(
                    CurrentFocus,
                    MaximumFocus,
                    Mathf.Max(
                        0f,
                        amount),
                    1f);

            if (_exhausted &&
                FocusLogic.CanResume(
                    CurrentFocus,
                    ResumeThreshold))
            {
                _exhausted =
                    false;
            }
        }
    }
}
