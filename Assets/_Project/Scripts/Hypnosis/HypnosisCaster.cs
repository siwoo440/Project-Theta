using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Companion;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Rival;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(PlayerSideViewController))]
    [RequireComponent(typeof(FollowerManager))]
    public sealed class HypnosisCaster : MonoBehaviour
    {
        [SerializeField] private float _scanRange = 4.5f;
        [SerializeField] private float _verticalTolerance = 2.4f;

        private PlayerSideViewController _playerController;
        private FollowerManager _followerManager;
        private HypnosisTarget _currentTarget;

        public HypnosisTarget CurrentTarget =>
            _currentTarget;

        public FollowerManager FollowerManager =>
            _followerManager;

        private void Awake()
        {
            _playerController =
                GetComponent<PlayerSideViewController>();

            _followerManager =
                GetComponent<FollowerManager>();
        }

        private void Update()
        {
            if (!ReadHypnosisHeld())
            {
                ChangeTarget(null);
                return;
            }

            HypnosisTarget candidate =
                FindBestTarget();

            if (candidate != null)
            {
                _playerController.FaceToward(
                    candidate.transform.position.x);
            }

            ChangeTarget(candidate);

            if (_currentTarget == null)
            {
                return;
            }

            bool completed =
                _currentTarget.ApplyPlayerFocus(
                    Time.deltaTime);

            if (!completed)
            {
                return;
            }

            HypnosisTarget completedTarget =
                _currentTarget;

            ChangeTarget(null);

            ClaimForPlayer(
                completedTarget);
        }

        private void ClaimForPlayer(
            HypnosisTarget target)
        {
            if (target == null)
            {
                return;
            }

            if (target.Owner ==
                NpcOwner.Rival)
            {
                RivalController rival =
                    target.RivalOwner;

                rival?.ReleaseOwnedTarget(
                    target);
            }

            target.ClaimByPlayer();

            if (_followerManager == null ||
                !_followerManager.TryAdd(
                    target))
            {
                target.ResetHypnosis();
            }
        }

        private HypnosisTarget FindBestTarget()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best = null;

            float bestDistanceSquared =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner ==
                    NpcOwner.Player)
                {
                    continue;
                }

                Vector2 delta =
                    (Vector2)target.transform.position -
                    (Vector2)transform.position;

                if (!HypnosisTargetingLogic.IsCandidate(
                        delta.x,
                        delta.y,
                        _scanRange,
                        _verticalTolerance))
                {
                    continue;
                }

                float distanceSquared =
                    delta.sqrMagnitude;

                if (distanceSquared >=
                    bestDistanceSquared)
                {
                    continue;
                }

                best =
                    target;

                bestDistanceSquared =
                    distanceSquared;
            }

            return best;
        }

        private void ChangeTarget(
            HypnosisTarget nextTarget)
        {
            if (_currentTarget ==
                nextTarget)
            {
                return;
            }

            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(
                    false);
            }

            _currentTarget =
                nextTarget;

            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(
                    true);
            }
        }

        private bool ReadHypnosisHeld()
        {
#if ENABLE_INPUT_SYSTEM
            bool keyboard =
                Keyboard.current != null &&
                Keyboard.current.eKey.isPressed;

            bool mouse =
                Mouse.current != null &&
                Mouse.current.leftButton.isPressed;

            return keyboard ||
                   mouse;
#else
            return Input.GetKey(KeyCode.E) ||
                   Input.GetMouseButton(0);
#endif
        }

        private void OnDisable()
        {
            ChangeTarget(null);
        }
    }
}
