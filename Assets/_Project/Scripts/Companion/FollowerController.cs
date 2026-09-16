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
        [SerializeField] private float _followSpeed = 3.33f;
        [SerializeField] private float _catchUpDistance = 4.0f;
        [SerializeField] private float _catchUpSpeed = 4.67f;
        [SerializeField] private float _stopDistance = 0.14f;

        [Header("Loose Formation")]
        [SerializeField] private float _horizontalJitter = 0.62f;
        [SerializeField] private float _verticalJitter = 0.58f;
        [SerializeField] private float _wanderAmplitudeScale = 1.0f;
        [SerializeField] private float _followSpeedVariation = 0.18f;
        [SerializeField] private float _stopDistanceVariation = 0.22f;

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

        /// <summary>근태 체크 표식이다. 표식이 한 번 붙으면 찾아 두고 계속 쓴다.</summary>
        private Disruptors.AttendanceMark _attendanceMark;
        private bool _isFollowing;
        private bool _isUnderExternalControl;

        private NpcWanderMotion _wander;
        private Vector2 _personalFormationOffset;
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

            _wander =
                GetComponent<NpcWanderMotion>();

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

        public void StopFollowingForOwnershipTransfer()
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

            _target?.StopPlayerFollowingForTransfer();
            _agent?.EnterFollowing();
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

            // 22일차: 인사팀 평가관의 근태 체크 표식이 붙어 있으면 유지도가 빨리 줄고, 가까이 있어도 조금씩 준다.
            bool marked =
                _attendanceMark != null &&
                _attendanceMark.IsMarked;

            if (!marked &&
                _attendanceMark == null &&
                TryGetComponent(out Disruptors.AttendanceMark found))
            {
                _attendanceMark = found;
                marked = found.IsMarked;
            }

            _stability =
                FollowerStabilityLogic.Tick(
                    _stability,
                    _maximumStability,
                    targetDistance,
                    _breakDistance,
                    _stabilityDecayPerSecond *
                    (marked
                        ? Disruptors.AttendanceMark.DecayMultiplier
                        : 1f),
                    _stabilityRecoveryPerSecond,
                    Time.fixedDeltaTime);

            if (marked)
            {
                _stability =
                    Mathf.Max(
                        0f,
                        _stability -
                        Disruptors.AttendanceMark.DrainPerSecond *
                        Time.fixedDeltaTime);
            }

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

            // 24일차: 지하철 승강장 변경 안내 뒤에는 인파에 끌려가고 느려진다.
            Vector2 crowdDrift =
                new Vector2(
                    Disruptors.CrowdFlow.Drift,
                    0f);

            if (targetDistance <=
                _personalStopDistance)
            {
                _body.linearVelocity =
                    separationVelocity +
                    crowdDrift;

                return;
            }

            float baseSpeed =
                targetDistance >
                _catchUpDistance
                    ? _catchUpSpeed
                    : _followSpeed;

            float speed =
                baseSpeed *
                _personalSpeedMultiplier *
                Disruptors.CrowdFlow.SpeedMultiplier;

            Vector2 followVelocity =
                delta.normalized *
                speed;

            _body.linearVelocity =
                Vector2.ClampMagnitude(
                    followVelocity +
                    separationVelocity,
                    speed + 0.65f) +
                crowdDrift;
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

            _wander?.Reseed();

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
            return _wander == null
                ? Vector2.zero
                : _wander.GetOffset(
                    _wanderAmplitudeScale);
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
