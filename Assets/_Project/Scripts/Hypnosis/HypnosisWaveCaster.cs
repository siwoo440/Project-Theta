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
        }

        /// <summary>내 동행 NPC의 충동을 낮춘다. 동시 폭주 위기를 끊는 핵심 효과다.</summary>
        private void RelieveFollowerImpulse()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < targets.Length;
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
            OpponentControllerBase[] opponents =
                FindObjectsByType<OpponentControllerBase>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < opponents.Length;
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
                   Mouse.current.rightButton.isPressed;
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
