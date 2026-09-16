using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Capture;
using ProjectTheta.Companion;
using ProjectTheta.Duel;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Rival;
using ProjectTheta.Stage;
using ProjectTheta.Core;

namespace ProjectTheta.Hypnosis
{
    /// <summary>
    /// 최면 파동을 시전한다. 우클릭을 짧게 눌러 충전한 뒤 발동한다.
    ///
    /// 힘겨루기·포획 미니게임도 우클릭을 쓰므로 그동안에는 충전이 시작되지 않는다.
    /// </summary>
    [RequireComponent(typeof(PlayerFocus))]
    public sealed class HypnosisWaveCaster : MonoBehaviour
    {
        /// <summary>경쟁자 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<OpponentControllerBase> _opponentBuffer =
            new System.Collections.Generic.List<OpponentControllerBase>();

        /// <summary>최면 대상 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<HypnosisTarget> _targetBuffer =
            new System.Collections.Generic.List<HypnosisTarget>();
        [SerializeField] private float _chargeSeconds =
            HypnosisWaveLogic.ChargeSeconds;

        [SerializeField] private float _radius =
            HypnosisWaveLogic.Radius;

        [SerializeField] private float _impulseRelief =
            HypnosisWaveLogic.ImpulseRelief;

        [SerializeField] private float _opponentStunSeconds =
            HypnosisWaveLogic.OpponentStunSeconds;

        [SerializeField] private float _auraSuppressSeconds =
            HypnosisWaveLogic.AuraSuppressSeconds;

        [SerializeField] private float _focusCost =
            HypnosisWaveLogic.FocusCost;

        [SerializeField] private float _cooldownSeconds =
            HypnosisWaveLogic.CooldownSeconds;

        private PlayerFocus _focus;
        private StageSessionController _stage;
        private PlayerCaptureController _capture;
        private OpponentDuelController _duel;

        private float _heldSeconds;
        private float _cooldownRemaining;
        private float _flashRemaining;

        public float CooldownRemaining =>
            Mathf.Max(
                0f,
                _cooldownRemaining);

        public float ChargeNormalized =>
            HypnosisWaveLogic.GetChargeNormalized(
                _heldSeconds,
                _chargeSeconds);

        public bool IsCharging =>
            _heldSeconds > 0f;

        /// <summary>발동 직후 짧게 켜지는 연출 플래그다.</summary>
        public bool IsFlashing =>
            _flashRemaining > 0f;

        public float Radius =>
            Mathf.Max(
                0f,
                _radius);

        public float FocusCost =>
            Mathf.Max(
                0f,
                _focusCost);

        private void Awake()
        {
            _focus =
                GetComponent<PlayerFocus>();

            _stage =
                GetComponent<StageSessionController>();

            _capture =
                GetComponent<PlayerCaptureController>();

            _duel =
                GetComponent<OpponentDuelController>();
        }

        private void Update()
        {
            if (GameplayPause.IsPaused)
            {
                return;
            }

            _cooldownRemaining =
                Mathf.Max(
                    0f,
                    _cooldownRemaining -
                    Time.deltaTime);

            _flashRemaining =
                Mathf.Max(
                    0f,
                    _flashRemaining -
                    Time.deltaTime);

            if (!HypnosisWaveLogic.CanCast(
                    _stage == null ||
                    _stage.IsRunning,
                    IsBlockedByOtherSystem(),
                    _cooldownRemaining,
                    _focus == null
                        ? 0f
                        : _focus.CurrentFocus,
                    FocusCost))
            {
                _heldSeconds =
                    0f;

                return;
            }

            if (!ReadWaveHeld())
            {
                _heldSeconds =
                    0f;

                return;
            }

            _heldSeconds +=
                Time.deltaTime;

            if (!HypnosisWaveLogic.IsChargeComplete(
                    _heldSeconds,
                    _chargeSeconds))
            {
                return;
            }

            _heldSeconds =
                0f;

            Cast();
        }

        private void Cast()
        {
            if (_focus == null ||
                !_focus.TrySpend(
                    FocusCost))
            {
                return;
            }

            _cooldownRemaining =
                Mathf.Max(
                    0f,
                    _cooldownSeconds);

            _flashRemaining =
                0.18f;

            RelieveFollowerImpulse();
            StunNearbyOpponents();
            SuppressNearbyAuras();
            StunNearbyDisruptors();

            // 26일차: 파동이 라이벌의 매혹 게이지를 모두 지운다.
            Boss.RivalCharm.ClearAll();

            // 24일차: 인파 흐름 중에는 파동이 동행자를 붙잡아 둔다.
            Disruptors.CrowdFlow.Anchor(
                Disruptors.CrowdFlowLogic.AnchorSeconds);
        }

        private readonly System.Collections.Generic.List<Disruptors.DisruptorBase> _disruptorBuffer =
            new System.Collections.Generic.List<Disruptors.DisruptorBase>();

        /// <summary>방해 세력은 파동을 맞으면 멍해진다 (22일차). 감시자를 맞히면 구역 경계도도 내려간다.</summary>
        private void StunNearbyDisruptors()
        {
            Disruptors.DisruptorBase.CopyActive(
                _disruptorBuffer);

            bool hitWatcher = false;

            for (int i = 0;
                 i < _disruptorBuffer.Count;
                 i++)
            {
                Disruptors.DisruptorBase disruptor =
                    _disruptorBuffer[i];

                if (disruptor == null ||
                    !disruptor.isActiveAndEnabled ||
                    !IsInRadius(
                        disruptor.transform.position))
                {
                    continue;
                }

                disruptor.Stun(
                    Disruptors.ZoneAlertLogic.WaveStunSeconds);

                if (disruptor.Profile != null &&
                    disruptor.Profile.Role == Disruptors.DisruptorRole.Watcher)
                {
                    hitWatcher = true;
                }
            }

            if (hitWatcher &&
                Disruptors.ZoneAlert.Current != null)
            {
                Disruptors.ZoneAlert.Current.Add(
                    -Disruptors.ZoneAlertLogic.WaveRelief,
                    transform.position);
            }
        }

        /// <summary>내 동행 NPC의 충동을 낮춘다. 동시 폭주 위기를 끊는 핵심 효과다.</summary>
        private void RelieveFollowerImpulse()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner !=
                    NpcOwner.Player ||
                    !IsInRadius(
                        target.transform.position))
                {
                    continue;
                }

                target.GetComponent<ImpulseMeter>()?.
                    RelieveImpulse(
                        _impulseRelief);
            }
        }

        /// <summary>경쟁자를 정지시키고 진행 중이던 선점·쟁탈을 초기화한다.</summary>
        private void StunNearbyOpponents()
        {
            OpponentControllerBase.CopyActive(
                _opponentBuffer);

            System.Collections.Generic.List<OpponentControllerBase> opponents =
                _opponentBuffer;

            for (int i = 0;
                 i < opponents.Count;
                 i++)
            {
                OpponentControllerBase opponent =
                    opponents[i];

                if (opponent == null ||
                    !opponent.isActiveAndEnabled ||
                    !IsInRadius(
                        opponent.transform.position))
                {
                    continue;
                }

                opponent.ApplyDuelStun(
                    _opponentStunSeconds);
            }
        }

        /// <summary>각성 지원형의 오라를 일시 무력화한다.</summary>
        private void SuppressNearbyAuras()
        {
            NpcAwakeningAura[] auras =
                FindObjectsByType<NpcAwakeningAura>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < auras.Length;
                 i++)
            {
                NpcAwakeningAura aura =
                    auras[i];

                if (aura == null ||
                    !aura.isActiveAndEnabled ||
                    !IsInRadius(
                        aura.transform.position))
                {
                    continue;
                }

                aura.Suppress(
                    _auraSuppressSeconds);
            }
        }

        private bool IsInRadius(
            Vector3 position)
        {
            return HypnosisWaveLogic.IsInRadius(
                Vector2.Distance(
                    transform.position,
                    position),
                Radius);
        }

        private bool IsBlockedByOtherSystem()
        {
            return (_capture != null &&
                    _capture.IsCapturing) ||
                   (_duel != null &&
                    (_duel.IsDueling ||
                     _duel.IsPlayerStunned));
        }

        private static bool ReadWaveHeld()
        {
#if ENABLE_INPUT_SYSTEM
            return Mouse.current != null &&
                   Mouse.current.rightButton.isPressed &&
                   !PointerGuard.IsOverOverlay;
#else
            return Input.GetMouseButton(
                1);
#endif
        }

        private void OnDisable()
        {
            _heldSeconds =
                0f;
        }
    }
}
