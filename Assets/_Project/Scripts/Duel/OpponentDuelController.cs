using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Capture;
using ProjectTheta.Hypnosis;
using ProjectTheta.Player;
using ProjectTheta.Stage;

namespace ProjectTheta.Duel
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerHealth))]
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class OpponentDuelController : MonoBehaviour
    {
        [Header("Gauge")]
        [SerializeField] private float _maximum = 100f;
        [SerializeField] private float _startingValue = 50f;
        [SerializeField] private float _playerPushPerCorrect = 8f;
        [SerializeField] private float _opponentPressurePerSecond = 10f;
        [SerializeField, Range(0f, 1f)] private float _opponentWinThresholdNormalized = 0.10f;
        [SerializeField, Range(0f, 1f)] private float _playerWinThresholdNormalized = 0.90f;

        [Header("Results")]
        [SerializeField] private int _playerLossDamage = 10;
        [SerializeField] private float _opponentStunDuration = 2.5f;
        [SerializeField] private float _playerStunDuration = 1.75f;
        [SerializeField] private float _opponentKnockbackDistance = 1.8f;
        [SerializeField] private float _playerKnockbackDistance = 1.6f;

        [Header("Visual")]
        [SerializeField] private float _visualJoltDistance = 14f;
        [SerializeField] private float _visualJoltRecoverySpeed = 120f;

        private PlayerSideViewController _movement;
        private HypnosisCaster _hypnosis;
        private PlayerCaptureController _capture;
        private PlayerHealth _health;
        private StageSessionController _stage;
        private Rigidbody2D _body;

        private OpponentDuelTarget _activeTarget;
        private bool _isDueling;
        private float _progress;
        private float _playerStunRemaining;
        private float _visualJoltOffsetX;

        private OpponentDuelInputSide _expectedInput =
            OpponentDuelInputSide.Left;

        public bool IsDueling =>
            _isDueling;

        public bool IsPlayerStunned =>
            _playerStunRemaining >
            0f;

        public float ProgressNormalized =>
            Mathf.Clamp01(
                _progress /
                Mathf.Max(
                    1f,
                    _maximum));

        public float OpponentWinThresholdNormalized =>
            Mathf.Clamp01(
                _opponentWinThresholdNormalized);

        public float PlayerWinThresholdNormalized =>
            Mathf.Clamp01(
                _playerWinThresholdNormalized);

        public string ActiveOpponentName =>
            _activeTarget == null
                ? "-"
                : _activeTarget.DisplayName;

        public int ActiveOpponentLossCount =>
            _activeTarget == null
                ? 0
                : _activeTarget.LossCount;

        public int ActiveOpponentMaximumDefeats =>
            _activeTarget == null
                ? 3
                : _activeTarget.MaximumDefeats;

        public string ExpectedInputLabel =>
            _expectedInput ==
            OpponentDuelInputSide.Left
                ? "좌클릭"
                : "우클릭";

        public float VisualJoltOffsetX =>
            _visualJoltOffsetX;

        private void Awake()
        {
            _movement =
                GetComponent<
                    PlayerSideViewController>();

            _hypnosis =
                GetComponent<
                    HypnosisCaster>();

            _capture =
                GetComponent<
                    PlayerCaptureController>();

            _health =
                GetComponent<
                    PlayerHealth>();

            _stage =
                GetComponent<
                    StageSessionController>();

            _body =
                GetComponent<
                    Rigidbody2D>();
        }

        private void Update()
        {
            RecoverVisualJolt();

            if (_stage == null ||
                !_stage.IsRunning)
            {
                if (_isDueling)
                {
                    ForceEndForStage();
                }

                return;
            }

            if (_playerStunRemaining >
                0f)
            {
                SetPlayerLocked(
                    true);

                _playerStunRemaining =
                    Mathf.Max(
                        0f,
                        _playerStunRemaining -
                        Time.deltaTime);

                if (_playerStunRemaining <=
                    0f &&
                    (_capture == null ||
                     !_capture.IsCapturing))
                {
                    SetPlayerLocked(
                        false);
                }

                return;
            }

            if (!_isDueling)
            {
                return;
            }

            if (_activeTarget == null ||
                !_activeTarget.gameObject.activeInHierarchy)
            {
                EndDuel(
                    true);

                return;
            }

            SetPlayerLocked(
                true);

            ProcessAlternatingInput();

            // 입력으로 임계선을 넘은 그 프레임에 즉시 판정한다.
            // 상대 압력이 같은 프레임에 게이지를 다시 임계선 안으로
            // 밀어 넣어 승패가 누락되는 것을 방지한다.
            if (TryResolveDuel())
            {
                return;
            }

            _progress =
                OpponentDuelLogic.
                    ApplyOpponentPressure(
                        _progress,
                        _opponentPressurePerSecond,
                        Time.deltaTime);

            TryResolveDuel();
        }

        private bool TryResolveDuel()
        {
            OpponentDuelResult result =
                OpponentDuelLogic.Resolve(
                    _progress,
                    _maximum,
                    OpponentWinThresholdNormalized,
                    PlayerWinThresholdNormalized);

            switch (result)
            {
                case OpponentDuelResult.PlayerWin:
                    ResolvePlayerWin();
                    return true;

                case OpponentDuelResult.OpponentWin:
                    ResolveOpponentWin();
                    return true;

                case OpponentDuelResult.None:
                default:
                    return false;
            }
        }

        private void OnTriggerStay2D(
            Collider2D other)
        {
            if (_isDueling ||
                IsPlayerStunned ||
                _stage == null ||
                !_stage.IsRunning ||
                (_capture != null &&
                 _capture.IsCapturing))
            {
                return;
            }

            OpponentDuelTarget target =
                other.GetComponent<
                    OpponentDuelTarget>();

            if (target == null ||
                !target.CanStartDuel)
            {
                return;
            }

            BeginDuel(
                target);
        }

        private void BeginDuel(
            OpponentDuelTarget target)
        {
            if (target == null)
            {
                return;
            }

            _activeTarget =
                target;

            _isDueling =
                true;

            _progress =
                Mathf.Clamp(
                    _startingValue,
                    0f,
                    Mathf.Max(
                        1f,
                        _maximum));

            _expectedInput =
                OpponentDuelInputSide.Left;

            _visualJoltOffsetX =
                0f;

            target.BeginDuel();

            SetPlayerLocked(
                true);

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void ProcessAlternatingInput()
        {
            bool leftPressed =
                ReadLeftPressed();

            bool rightPressed =
                ReadRightPressed();

            if (leftPressed ==
                rightPressed)
            {
                return;
            }

            OpponentDuelInputSide actual =
                leftPressed
                    ? OpponentDuelInputSide.Left
                    : OpponentDuelInputSide.Right;

            if (!OpponentDuelLogic.
                    IsCorrectInput(
                        _expectedInput,
                        actual))
            {
                return;
            }

            _progress =
                OpponentDuelLogic.
                    AddPlayerPush(
                        _progress,
                        _maximum,
                        _playerPushPerCorrect);

            _expectedInput =
                OpponentDuelLogic.
                    GetNextExpected(
                        _expectedInput);

            _visualJoltOffsetX =
                actual ==
                OpponentDuelInputSide.Left
                    ? -_visualJoltDistance
                    : _visualJoltDistance;
        }

        private void ResolvePlayerWin()
        {
            OpponentDuelTarget defeated =
                _activeTarget;

            if (defeated != null)
            {
                defeated.RegisterPlayerVictory(
                    transform.position,
                    _opponentStunDuration,
                    _opponentKnockbackDistance);
            }

            EndDuel(
                true);
        }

        private void ResolveOpponentWin()
        {
            OpponentDuelTarget winner =
                _activeTarget;

            winner?.EndDuelWithoutStun();

            KnockPlayerAwayFrom(
                winner == null
                    ? transform.position
                    : winner.transform.position);

            if (_health != null)
            {
                _health.TakeDamage(
                    Mathf.Max(
                        0,
                        _playerLossDamage));
            }

            _stage?.RefreshState();

            _isDueling =
                false;

            _activeTarget =
                null;

            _progress =
                0f;

            _visualJoltOffsetX =
                0f;

            if (_health != null &&
                _health.IsDead)
            {
                SetPlayerLocked(
                    true);

                return;
            }

            _playerStunRemaining =
                Mathf.Max(
                    0f,
                    _playerStunDuration);

            SetPlayerLocked(
                true);
        }

        private void EndDuel(
            bool restorePlayer)
        {
            _activeTarget?.
                EndDuelWithoutStun();

            _isDueling =
                false;

            _activeTarget =
                null;

            _progress =
                0f;

            _visualJoltOffsetX =
                0f;

            if (restorePlayer &&
                _stage != null &&
                _stage.IsRunning &&
                _playerStunRemaining <=
                    0f &&
                (_capture == null ||
                 !_capture.IsCapturing))
            {
                SetPlayerLocked(
                    false);
            }
            else
            {
                SetPlayerLocked(
                    true);
            }
        }

        public void ForceEndForStage()
        {
            _playerStunRemaining =
                0f;

            EndDuel(
                false);
        }

        private void KnockPlayerAwayFrom(
            Vector3 sourcePosition)
        {
            float direction =
                transform.position.x >=
                sourcePosition.x
                    ? 1f
                    : -1f;

            Vector3 position =
                transform.position;

            position.x +=
                direction *
                Mathf.Max(
                    0f,
                    _playerKnockbackDistance);

            transform.position =
                position;

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void SetPlayerLocked(
            bool locked)
        {
            _movement?.
                SetInputLocked(
                    locked);

            if (_hypnosis != null)
            {
                bool canEnable =
                    !locked &&
                    _stage != null &&
                    _stage.IsRunning &&
                    (_capture == null ||
                     !_capture.IsCapturing);

                _hypnosis.enabled =
                    canEnable;
            }
        }

        private void RecoverVisualJolt()
        {
            _visualJoltOffsetX =
                Mathf.MoveTowards(
                    _visualJoltOffsetX,
                    0f,
                    _visualJoltRecoverySpeed *
                    Time.deltaTime);
        }

        private bool ReadLeftPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return
                Mouse.current != null &&
                Mouse.current.leftButton.
                    wasPressedThisFrame;
#else
            return
                Input.GetMouseButtonDown(
                    0);
#endif
        }

        private bool ReadRightPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return
                Mouse.current != null &&
                Mouse.current.rightButton.
                    wasPressedThisFrame;
#else
            return
                Input.GetMouseButtonDown(
                    1);
#endif
        }

        private void OnDisable()
        {
            ForceEndForStage();
        }
    }
}
