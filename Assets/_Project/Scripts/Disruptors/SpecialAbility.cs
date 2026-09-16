using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 특수 능력의 공통 틀이다 (22일차, 부록 C.2.4).
    ///
    /// 시간 흐름(대기 → 예고 → 발동 → 재사용)은 <see cref="SpecialAbilityLogic"/>이 정하고,
    /// 여기서는 알림과 연결만 한다. 능력마다 "언제 시작하고 싶은가"와 "발동하면 무엇을 하는가"만 구현한다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public abstract class SpecialAbility : MonoBehaviour
    {
        private AbilityTimer _timer;

        protected DisruptorBase Body { get; private set; }

        public abstract string DisplayName { get; }

        public AbilityPhase Phase =>
            _timer.Phase;

        public float TelegraphProgress =>
            SpecialAbilityLogic.GetTelegraphProgress(
                _timer,
                TelegraphSeconds);

        /// <summary>한 번이라도 발동했는지다. 이름표를 숨겼다가 드러내는 개체(소매치기)가 쓴다.</summary>
        public bool HasFired { get; private set; }

        public float CooldownRemaining =>
            _timer.Phase == AbilityPhase.Cooldown
                ? _timer.Remaining
                : 0f;

        protected float TelegraphSeconds =>
            Body == null ||
            Body.Profile == null
                ? SpecialAbilityLogic.MinimumTelegraphSeconds
                : Body.Profile.TelegraphSeconds;

        protected float CooldownSeconds =>
            Body == null ||
            Body.Profile == null
                ? 10f
                : Body.Profile.CooldownSeconds;

        protected virtual void Awake()
        {
            Body = GetComponent<DisruptorBase>();
        }

        /// <summary>판 시작 직후 바로 쓰지 않게 첫 재사용 대기를 걸어 둔다 (24일차).</summary>
        protected void SetInitialCooldown(
            float seconds)
        {
            _timer.Phase = AbilityPhase.Cooldown;
            _timer.Remaining = Mathf.Max(0f, seconds);
        }

        /// <summary>디버그 치트: 조건을 무시하고 바로 예고를 시작한다. 예고는 건너뛰지 않는다.</summary>
        public void DebugTrigger()
        {
            if (_timer.Phase == AbilityPhase.Telegraph)
            {
                return;
            }

            _timer.Phase = AbilityPhase.Ready;
            _timer.Remaining = 0f;

            BeginTelegraph();
        }

        private void Update()
        {
            if (Body == null)
            {
                return;
            }

            if (!Body.CanAct)
            {
                return;
            }

            AbilityPhase before =
                _timer.Phase;

            ZoneAlert alert =
                ZoneAlert.Current;

            bool fired =
                SpecialAbilityLogic.Tick(
                    ref _timer,
                    Time.deltaTime,
                    Body.IsStunned,
                    before == AbilityPhase.Ready &&
                    WantsToStart(),
                    TelegraphSeconds,
                    CooldownSeconds,
                    alert == null
                        ? AlertLevel.Calm
                        : alert.Level);

            if (before == AbilityPhase.Ready &&
                _timer.Phase == AbilityPhase.Telegraph)
            {
                OnTelegraphStarted();
            }

            if (fired)
            {
                HasFired = true;

                Fire();
            }
        }

        private void BeginTelegraph()
        {
            _timer.Phase = AbilityPhase.Telegraph;

            _timer.Remaining =
                SpecialAbilityLogic.SanitizeTelegraph(
                    TelegraphSeconds);

            OnTelegraphStarted();
        }

        private void OnTelegraphStarted()
        {
            OnTelegraph();

            StageMoments.RaiseAbilityTelegraphed(
                transform.position,
                DisplayName);
        }

        /// <summary>대기 중 매 프레임 불린다. true면 예고를 시작한다.</summary>
        protected abstract bool WantsToStart();

        /// <summary>예고가 시작될 때 한 번 불린다. 대상을 바라보는 등 준비 동작을 한다.</summary>
        protected virtual void OnTelegraph()
        {
        }

        /// <summary>예고가 끝나는 순간 한 번 불린다. 대상이 사라졌을 수 있으므로 여기서 다시 찾는다.</summary>
        protected abstract void Fire();
    }
}
