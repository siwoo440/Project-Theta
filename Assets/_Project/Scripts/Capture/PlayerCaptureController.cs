using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Player;
using ProjectTheta.Stage;

namespace ProjectTheta.Capture
{
    public sealed class PlayerCaptureController : MonoBehaviour
    {
        [SerializeField] private float _escapeMaximum = 100f;
        [SerializeField] private float _escapeGainPerCorrect = 12f;
        [SerializeField] private float _damageTickInterval = 0.40f;
        [SerializeField] private float _visualJoltDistance = 18f;
        [SerializeField] private float _visualJoltRecoverySpeed = 140f;

        private PlayerSideViewController _movementController;
        private HypnosisCaster _hypnosisCaster;
        private Rigidbody2D _body;
        private PlayerHealth _health;
        private StageSessionController _stage;

        private bool _isCapturing;
        private float _escapeProgress;
        private float _damageTimer;
        private int _damageTaken;
        private CaptureInputSide _expectedInput =
            CaptureInputSide.Left;
        private float _visualJoltOffsetX;
        private ImpulseMeter _activeCaptor;

        public bool IsCapturing =>
            _isCapturing;

        public float EscapeNormalized =>
            Mathf.Clamp01(
                _escapeProgress /
                Mathf.Max(
                    1f,
                    _escapeMaximum));

        public int DamageTaken =>
            _damageTaken;

        public int DamageCap =>
            _stage == null
                ? 10
                : _stage.CaptureMaxDamage;

        public string ExpectedInputLabel =>
            _expectedInput ==
            CaptureInputSide.Left
                ? "좌클릭"
                : "우클릭";

        public float VisualJoltOffsetX =>
            _visualJoltOffsetX;

        public ImpulseMeter ActiveCaptor =>
            _activeCaptor;

        private void Awake()
        {
            _movementController =
                GetComponent<
                    PlayerSideViewController>();

            _hypnosisCaster =
                GetComponent<
                    HypnosisCaster>();

            _body =
                GetComponent<
                    Rigidbody2D>();

            _health =
                GetComponent<
                    PlayerHealth>();

            _stage =
                GetComponent<
                    StageSessionController>();
        }

        public bool TryBeginCapture(
            ImpulseMeter captor)
        {
            if (_isCapturing ||
                captor == null ||
                _stage == null ||
                !_stage.IsRunning)
            {
                return false;
            }

            _isCapturing = true;
            _activeCaptor = captor;
            _escapeProgress = 0f;
            _damageTimer = 0f;
            _damageTaken = 0;
            _expectedInput =
                CaptureInputSide.Left;
            _visualJoltOffsetX = 0f;

            _stage.GrantRampageCaptureReward();

            ApplyCaptureLocks(
                true);

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            return true;
        }

        public void NotifyCaptorUnavailable(
            ImpulseMeter captor)
        {
            if (!_isCapturing ||
                captor == null ||
                _activeCaptor != captor)
            {
                return;
            }

            EndCapture(
                _stage != null &&
                _stage.IsRunning);
        }

        public void ForceEndCapture(
            bool restoreGameplayInput)
        {
            if (!_isCapturing)
            {
                if (!restoreGameplayInput)
                {
                    ApplyCaptureLocks(
                        true);
                }

                return;
            }

            EndCapture(
                restoreGameplayInput);
        }

        private void Update()
        {
            if (!_isCapturing)
            {
                RecoverVisualJolt();

                return;
            }

            if (_stage == null ||
                !_stage.IsRunning)
            {
                EndCapture(
                    false);

                return;
            }

            ApplyCaptureLocks(
                true);

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            ProcessAlternatingInput();
            ProcessDamageTicks();
            RecoverVisualJolt();

            if (_escapeProgress >=
                _escapeMaximum)
            {
                EndCapture(
                    true);
            }
        }

        private void ProcessAlternatingInput()
        {
            bool leftPressed =
                ReadLeftPressed();

            bool rightPressed =
                ReadRightPressed();

            if (leftPressed)
            {
                HandleInput(
                    CaptureInputSide.Left,
                    -_visualJoltDistance);
            }

            if (rightPressed)
            {
                HandleInput(
                    CaptureInputSide.Right,
                    _visualJoltDistance);
            }
        }

        private void HandleInput(
            CaptureInputSide actual,
            float joltOffset)
        {
            if (!CaptureEscapeLogic.IsCorrectInput(
                    _expectedInput,
                    actual))
            {
                return;
            }

            _escapeProgress =
                CaptureEscapeLogic.AddEscapeProgress(
                    _escapeProgress,
                    _escapeMaximum,
                    _escapeGainPerCorrect);

            _expectedInput =
                CaptureEscapeLogic.GetNextExpected(
                    _expectedInput);

            _visualJoltOffsetX =
                joltOffset;
        }

        private void ProcessDamageTicks()
        {
            float interval =
                Mathf.Max(
                    0.05f,
                    _damageTickInterval);

            _damageTimer +=
                Time.deltaTime;

            while (_damageTimer >=
                   interval)
            {
                _damageTimer -=
                    interval;

                ApplyDamageTick();

                if (!_isCapturing)
                {
                    break;
                }
            }
        }

        private void ApplyDamageTick()
        {
            if (_health == null ||
                _stage == null)
            {
                EndCapture(
                    true);

                return;
            }

            int tickDamage =
                Mathf.Max(
                    1,
                    _stage.CaptureTickDamage);

            _health.TakeDamage(
                tickDamage);

            _damageTaken +=
                tickDamage;

            _stage.RefreshState();

            if (_health.IsDead)
            {
                EndCapture(
                    false);

                return;
            }

            if (_damageTaken >=
                _stage.CaptureMaxDamage)
            {
                EndCapture(
                    true);
            }
        }

        private void EndCapture(
            bool restoreGameplayInput)
        {
            _isCapturing = false;
            _escapeProgress = 0f;
            _damageTimer = 0f;
            _damageTaken = 0;
            _expectedInput =
                CaptureInputSide.Left;
            _visualJoltOffsetX = 0f;
            _activeCaptor = null;

            if (restoreGameplayInput &&
                _stage != null &&
                _stage.IsRunning)
            {
                ApplyCaptureLocks(
                    false);
            }
            else
            {
                ApplyCaptureLocks(
                    true);
            }

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void ApplyCaptureLocks(
            bool isLocked)
        {
            if (_movementController != null)
            {
                _movementController.SetInputLocked(
                    isLocked);
            }

            if (_hypnosisCaster != null)
            {
                bool canEnable =
                    !isLocked &&
                    (_stage == null ||
                     _stage.IsRunning);

                _hypnosisCaster.enabled =
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
                Mouse.current.leftButton.wasPressedThisFrame;
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
                Mouse.current.rightButton.wasPressedThisFrame;
#else
            return
                Input.GetMouseButtonDown(
                    1);
#endif
        }

        private void OnDisable()
        {
            EndCapture(
                false);
        }
    }
}
