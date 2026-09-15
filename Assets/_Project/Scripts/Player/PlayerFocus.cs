using UnityEngine;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 플레이어의 두 번째 자원인 집중력이다.
    ///
    /// 19일차부터 집중력은 최면을 막지 않는다.
    /// 남아 있는 동안 최면이 빨라지고, 0이 되면 기본 속도로 계속 걸린다.
    /// 대시·최면 파동·체인 최면은 집중력이 충분할 때만 쓸 수 있다.
    /// </summary>
    public sealed class PlayerFocus : MonoBehaviour
    {
        [SerializeField] private float _maximumFocus = 100f;
        [SerializeField] private float _hypnosisDrainPerSecond = 6f;
        [SerializeField] private float _dashCost = 12f;
        [SerializeField] private float _recoveryPerSecond = 14f;
        [SerializeField] private float _recoveryDelay = 0.8f;

        private float _secondsSinceSpend;

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

        /// <summary>집중력이 바닥났는지다. 최면은 계속 걸리고 가속만 빠진다. HUD 표시에 쓴다.</summary>
        public bool IsExhausted =>
            FocusLogic.IsDepleted(
                CurrentFocus);

        /// <summary>남은 집중력에 따른 최면 가속 배율이다. 0이면 1배다.</summary>
        public float HypnosisSpeedMultiplier =>
            FocusLogic.GetHypnosisSpeedMultiplier(
                CurrentFocus,
                HypnosisSpeedBonus);

        /// <summary>집중력이 남아 있을 때 최면 속도에 더해지는 비율이다. 자산에서 조정한다.</summary>
        public static float HypnosisSpeedBonus =>
            Balance.BalanceOverrides.StageOrDefault.FocusHypnosisSpeedBonus;

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
        }

        /// <summary>
        /// 최면을 유지하는 동안의 지속 소모다.
        /// 바닥나도 최면을 끊지 않는다. 가속만 빠진다.
        /// </summary>
        public void DrainContinuous(
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
        }

        /// <summary>즉시 소모다. 집중력이 모자라면 아무것도 쓰지 않고 false를 반환한다. 대시·파동·체인이 쓴다.</summary>
        public bool TrySpend(
            float amount)
        {
            if (!FocusLogic.CanAfford(
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

            return true;
        }

        /// <summary>
        /// 모자라도 있는 만큼 깎는다. 반격형 NPC의 집중력 피해처럼 "비용"이 아닌 "피해"에 쓴다.
        /// 전에는 피해도 TrySpend로 처리해서, 집중력이 피해량보다 적으면 아예 깎이지 않았다.
        /// </summary>
        public void TakeDamage(
            float amount)
        {
            CurrentFocus =
                FocusLogic.Drain(
                    CurrentFocus,
                    amount);

            _secondsSinceSpend =
                0f;
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
        }
    }
}
