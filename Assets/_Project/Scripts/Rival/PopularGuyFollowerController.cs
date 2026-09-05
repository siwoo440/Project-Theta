using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;

namespace ProjectTheta.Rival
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HypnosisTarget))]
    [RequireComponent(typeof(NpcAgent))]
    [RequireComponent(typeof(NpcSoftSeparation))]
    public sealed class PopularGuyFollowerController : MonoBehaviour
    {
        [SerializeField] private float _followSpeed = 4.8f;
        [SerializeField] private float _catchUpDistance = 4.0f;
        [SerializeField] private float _catchUpSpeed = 6.6f;
        [SerializeField] private float _stopDistance = 0.18f;

        private Rigidbody2D _body;
        private HypnosisTarget _target;
        private NpcAgent _agent;
        private NpcSoftSeparation _separation;

        private PopularGuyFollowerManager _manager;
        private Transform _leader;
        private int _slotIndex;
        private bool _isFollowing;

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
        }

        public void BeginFollowing(
            PopularGuyFollowerManager manager,
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
                _target.PopularGuyOwner == null)
            {
                return;
            }

            Vector2 destination =
                _manager.GetSlotWorldPosition(
                    _slotIndex);

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
                _stopDistance)
            {
                _body.linearVelocity =
                    separationVelocity;

                return;
            }

            float speed =
                distance >
                _catchUpDistance
                    ? _catchUpSpeed
                    : _followSpeed;

            Vector2 followVelocity =
                delta.normalized *
                speed;

            _body.linearVelocity =
                Vector2.ClampMagnitude(
                    followVelocity +
                    separationVelocity,
                    speed + 0.65f);
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
