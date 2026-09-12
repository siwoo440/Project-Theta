using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 경쟁자에게 확보된 NPC가 해당 경쟁자의 대열 슬롯을 따라가게 한다.
    /// 금태양·인기남 구분 없이 하나의 구현을 사용한다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HypnosisTarget))]
    [RequireComponent(typeof(NpcAgent))]
    [RequireComponent(typeof(NpcSoftSeparation))]
    public sealed class OpponentFollowerController : MonoBehaviour
    {
        [SerializeField] private float _followSpeed = 3.20f;
        [SerializeField] private float _catchUpDistance = 4.0f;
        [SerializeField] private float _catchUpSpeed = 4.40f;
        [SerializeField] private float _stopDistance = 0.18f;

        [Header("Loose Formation")]
        [SerializeField] private float _horizontalJitter = 0.62f;
        [SerializeField] private float _verticalJitter = 0.58f;
        [SerializeField] private float _wanderAmplitudeScale = 1.0f;
        [SerializeField] private float _followSpeedVariation = 0.18f;
        [SerializeField] private float _stopDistanceVariation = 0.22f;

        private Rigidbody2D _body;
        private HypnosisTarget _target;
        private NpcAgent _agent;
        private NpcSoftSeparation _separation;
        private NpcWanderMotion _wander;

        private OpponentFollowerManager _manager;
        private Transform _leader;
        private int _slotIndex;
        private bool _isFollowing;

        private Vector2 _personalFormationOffset;
        private float _personalSpeedMultiplier = 1f;
        private float _personalStopDistance;

        private void Awake()
        {
            _body =
                GetComponent<Rigidbody2D>();

            _target =
                GetComponent<HypnosisTarget>();

            _agent =
                GetComponent<NpcAgent>();

            _separation =
                GetComponent<NpcSoftSeparation>();

            _wander =
                GetComponent<NpcWanderMotion>();

            _personalStopDistance =
                _stopDistance;
        }

        public void BeginFollowing(
            OpponentFollowerManager manager,
            Transform leader,
            int slotIndex)
        {
            _manager =
                manager;

            _leader =
                leader;

            _slotIndex =
                Mathf.Max(
                    0,
                    slotIndex);

            _isFollowing =
                true;

            RandomizeFormationPersonality();

            _agent?.EnterFollowing();

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        public void SetSlotIndex(
            int slotIndex)
        {
            _slotIndex =
                Mathf.Max(
                    0,
                    slotIndex);
        }

        public void StopFollowingForOwnershipTransfer()
        {
            _isFollowing = false;
            _manager = null;
            _leader = null;

            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }

            _agent?.EnterFollowing();
        }

        private void FixedUpdate()
        {
            if (!_isFollowing ||
                _manager == null ||
                _leader == null ||
                _target == null ||
                _target.OpponentOwner == null ||
                _target.Owner !=
                _manager.OwnerTag)
            {
                return;
            }

            Vector2 destination =
                _manager.GetSlotWorldPosition(
                    _slotIndex,
                    _personalFormationOffset +
                    GetWanderOffset());

            Vector2 delta =
                destination -
                (Vector2)transform.position;

            float distance =
                delta.magnitude;

            Vector2 separationVelocity =
                _separation == null
                    ? Vector2.zero
                    : _separation.GetCorrectionVelocity();

            if (distance <=
                _personalStopDistance)
            {
                _body.linearVelocity =
                    separationVelocity;

                return;
            }

            float speed =
                (distance >
                 _catchUpDistance
                    ? _catchUpSpeed
                    : _followSpeed) *
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

        /// <summary>같은 대열이라도 개체마다 위치·속도·정지 거리를 다르게 만든다.</summary>
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
