using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;

namespace ProjectTheta.Companion
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(NpcAgent))]
    [RequireComponent(typeof(HypnosisTarget))]
    [RequireComponent(typeof(NpcSoftSeparation))]
    public sealed class FollowerController : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private float _followSpeed = 5.0f;
        [SerializeField] private float _catchUpDistance = 4.0f;
        [SerializeField] private float _catchUpSpeed = 7.0f;
        [SerializeField] private float _stopDistance = 0.14f;

        [Header("Loose Formation")]
        [SerializeField] private float _horizontalJitter = 0.38f;
        [SerializeField] private float _verticalJitter = 0.42f;
        [SerializeField] private float _wanderHorizontal = 0.08f;
        [SerializeField] private float _wanderVertical = 0.11f;
        [SerializeField] private float _wanderSpeedMin = 0.55f;
        [SerializeField] private float _wanderSpeedMax = 0.95f;
        [SerializeField] private float _followSpeedVariation = 0.10f;
        [SerializeField] private float _stopDistanceVariation = 0.10f;

        [Header("Stability")]
        [SerializeField] private float _maximumStability = 100f;
        [SerializeField] private float _breakDistance = 9.0f;
        [SerializeField] private float _stabilityDecayPerSecond = 12f;
        [SerializeField] private float _stabilityRecoveryPerSecond = 20f;

        private Rigidbody2D _body;
        private NpcAgent _agent;
        private HypnosisTarget _target;
        private NpcSoftSeparation _separation;

        private FollowerManager _manager;
        private Transform _leader;
        private int _slotIndex;
        private float _stability;
        private bool _isFollowing;
        private bool _isUnderExternalControl;

        private Vector2 _personalFormationOffset;
        private float _wanderPhase;
        private float _wanderSpeed;
        private float _personalSpeedMultiplier = 1f;
        private float _personalStopDistance;

        public float StabilityNormalized =>
            Mathf.Clamp01(
                _stability /
                Mathf.Max(
                    1f,
                    _maximumStability));

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _agent =
                GetComponent<NpcAgent>();

            _target =
                GetComponent<HypnosisTarget>();

            _separation =
                GetComponent<NpcSoftSeparation>();

            _stability =
                _maximumStability;

            _personalStopDistance =
                _stopDistance;
        }

        public void BeginFollowing(
            FollowerManager manager,
            Transform leader,
            int slotIndex)
        {
            _manager = manager;
            _leader = leader;
            _slotIndex =
                Mathf.Max(
                    0,
                    slotIndex);

            _stability =
                _maximumStability;

            _isFollowing = true;
            _isUnderExternalControl = false;

            RandomizeFormationPersonality();

            _target?.BeginFollowing();
            _agent?.EnterFollowing();
        }

        public void SetSlotIndex(
            int slotIndex)
        {
            _slotIndex =
                Mathf.Max(
                    0,
                    slotIndex);
        }

        public void SetExternalControl(
            bool isActive)
        {
            _isUnderExternalControl =
                isActive;

            if (_body != null &&
                isActive)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        public void StopFollowing()
        {
            _isFollowing = false;
            _isUnderExternalControl = false;
            _manager = null;
            _leader = null;

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            _target?.ReleaseFromFollowing();
            _agent?.ReturnToRoaming();
        }

        private void FixedUpdate()
        {
            if (!_isFollowing ||
                _manager == null ||
                _leader == null)
            {
                return;
            }

            if (_isUnderExternalControl)
            {
                return;
            }

            Vector2 looseOffset =
                _personalFormationOffset +
                GetWanderOffset();

            Vector2 targetPosition =
                _manager.GetSlotWorldPosition(
                    _slotIndex,
                    looseOffset);

            Vector2 delta =
                targetPosition -
                (Vector2)transform.position;

            float targetDistance =
                delta.magnitude;

            _stability =
                FollowerStabilityLogic.Tick(
                    _stability,
                    _maximumStability,
                    targetDistance,
                    _breakDistance,
                    _stabilityDecayPerSecond,
                    _stabilityRecoveryPerSecond,
                    Time.fixedDeltaTime);

            if (_stability <= 0f)
            {
                _body.linearVelocity =
                    Vector2.zero;

                _manager.RequestRelease(
                    this);

                return;
            }

            Vector2 separationVelocity =
                _separation == null
                    ? Vector2.zero
                    : _separation.GetCorrectionVelocity();

            if (targetDistance <=
                _personalStopDistance)
            {
                _body.linearVelocity =
                    separationVelocity;

                return;
            }

            float baseSpeed =
                targetDistance >
                _catchUpDistance
                    ? _catchUpSpeed
                    : _followSpeed;

            float speed =
                baseSpeed *
                _personalSpeedMultiplier;

            Vector2 followVelocity =
                delta.normalized *
                speed;

            _body.linearVelocity =
                Vector2.ClampMagnitude(
                    followVelocity +
                    separationVelocity,
                    speed + 0.65f);
        }

        private void RandomizeFormationPersonality()
        {
            _personalFormationOffset =
                new Vector2(
                    Random.Range(
                        -_horizontalJitter,
                        _horizontalJitter),
                    Random.Range(
                        -_verticalJitter,
                        _verticalJitter));

            _wanderPhase =
                Random.Range(
                    0f,
                    Mathf.PI * 2f);

            _wanderSpeed =
                Random.Range(
                    _wanderSpeedMin,
                    _wanderSpeedMax);

            _personalSpeedMultiplier =
                Random.Range(
                    1f - _followSpeedVariation,
                    1f + _followSpeedVariation);

            _personalStopDistance =
                _stopDistance +
                Random.Range(
                    0f,
                    _stopDistanceVariation);
        }

        private Vector2 GetWanderOffset()
        {
            float time =
                Time.fixedTime *
                _wanderSpeed;

            return new Vector2(
                Mathf.Sin(
                    time +
                    _wanderPhase) *
                _wanderHorizontal,
                Mathf.Cos(
                    (time * 0.83f) +
                    _wanderPhase) *
                _wanderVertical);
        }

        private void OnDisable()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }
    }
}
