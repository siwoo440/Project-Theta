using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Player;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class HypnosisCaster : MonoBehaviour
    {
        [SerializeField] private float _scanRange = 4.5f;
        [SerializeField] private float _verticalTolerance = 2.4f;

        private PlayerSideViewController _playerController;
        private HypnosisTarget _currentTarget;

        public HypnosisTarget CurrentTarget =>
            _currentTarget;

        private void Awake()
        {
            _playerController =
                GetComponent<PlayerSideViewController>();
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

            ChangeTarget(candidate);

            if (_currentTarget == null)
            {
                return;
            }

            _currentTarget.ApplyFocus(
                Time.deltaTime);

            if (_currentTarget.IsHypnotized)
            {
                ChangeTarget(null);
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

            for (int i = 0; i < targets.Length; i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    target.IsHypnotized)
                {
                    continue;
                }

                Vector2 delta =
                    (Vector2)target.transform.position -
                    (Vector2)transform.position;

                if (!HypnosisTargetingLogic.IsCandidate(
                        delta.x,
                        delta.y,
                        _playerController.FacingDirection,
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

                best = target;
                bestDistanceSquared =
                    distanceSquared;
            }

            return best;
        }

        private void ChangeTarget(
            HypnosisTarget nextTarget)
        {
            if (_currentTarget == nextTarget)
            {
                return;
            }

            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(false);
            }

            _currentTarget = nextTarget;

            if (_currentTarget != null)
            {
                _currentTarget.SetTargeted(true);
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

            return keyboard || mouse;
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
