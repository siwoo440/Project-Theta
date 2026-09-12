using UnityEngine;
using ProjectTheta.Stage;
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
        private PlayerFocus _focus;
        private HypnosisTarget _currentTarget;

        private int _chainIndex;
        private float _interruptRemaining;

        /// <summary>현재 체인 단계다. 0이면 일반 최면이다.</summary>
        public int ChainIndex =>
            _chainIndex;

        public bool IsInterrupted =>
            _interruptRemaining > 0f;

        /// <summary>반격형 특성 등이 최면 연결을 강제로 끊을 때 호출한다.</summary>
        public void Interrupt(
            float seconds)
        {
            _interruptRemaining =
                Mathf.Max(
                    _interruptRemaining,
                    Mathf.Max(
                        0f,
                        seconds));

            _chainIndex =
                0;

            ChangeTarget(
                null);
        }

        public HypnosisTarget CurrentTarget =>
            _currentTarget;

        public FollowerManager FollowerManager =>
            _followerManager;

        private StageScoreTracker _scoreTracker;

        /// <summary>점수 집계기는 플레이어 오브젝트에 함께 붙어 있다.</summary>
        private StageScoreTracker ResolveScoreTracker()
        {
            if (_scoreTracker == null)
            {
                _scoreTracker =
                    GetComponent<StageScoreTracker>();
            }

            if (_scoreTracker == null)
            {
                _scoreTracker =
                    FindFirstObjectByType<
                        StageScoreTracker>();
            }

            return _scoreTracker;
        }

        private void Awake()
        {
            _playerController =
                GetComponent<PlayerSideViewController>();

            _followerManager =
                GetComponent<FollowerManager>();

            _focus =
                GetComponent<PlayerFocus>();
        }

        private void Update()
        {
            _interruptRemaining =
                Mathf.Max(
                    0f,
                    _interruptRemaining -
                    Time.deltaTime);

            if (_interruptRemaining > 0f ||
                !ReadHypnosisHeld() ||
                (_focus != null &&
                 !_focus.CanCast))
            {
                ChangeTarget(null);

                _chainIndex =
                    0;

                return;
            }

            // 체인으로 이어붙인 대상이 아직 살아 있으면 그 대상을 계속 공략한다.
            HypnosisTarget candidate =
                _chainIndex > 0 &&
                IsChainTargetValid()
                    ? _currentTarget
                    : FindBestTarget();

            if (candidate == null)
            {
                _chainIndex =
                    0;
            }

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

            // 최면을 유지하는 동안 집중력이 계속 소모된다.
            if (_focus != null &&
                !_focus.DrainContinuous(
                    _focus.HypnosisDrainPerSecond,
                    Time.deltaTime))
            {
                ChangeTarget(null);

                _chainIndex =
                    0;

                return;
            }

            bool completed =
                _currentTarget.ApplyPlayerFocus(
                    Time.deltaTime,
                    ChainHypnosisLogic.GetSpeedMultiplier(
                        _chainIndex));

            if (!completed)
            {
                return;
            }

            HypnosisTarget completedTarget =
                _currentTarget;

            ChangeTarget(null);

            ClaimForPlayer(
                completedTarget);

            TryStartChain(
                completedTarget);
        }

        /// <summary>
        /// 최면이 끝난 대상 근처에 중립 NPC가 있으면 체인을 이어붙인다.
        /// 단계마다 추가 집중력을 소모하므로, 감당이 안 되면 체인은 자동으로 끊긴다.
        /// </summary>
        private void TryStartChain(
            HypnosisTarget source)
        {
            if (source == null ||
                !ChainHypnosisLogic.CanChain(
                    _chainIndex))
            {
                _chainIndex =
                    0;

                return;
            }

            HypnosisTarget next =
                FindChainTarget(
                    source);

            if (next == null)
            {
                _chainIndex =
                    0;

                return;
            }

            int nextIndex =
                ChainHypnosisLogic.Advance(
                    _chainIndex);

            float cost =
                ChainHypnosisLogic.GetChainFocusCost(
                    nextIndex);

            if (_focus != null &&
                !_focus.TrySpend(
                    cost))
            {
                _chainIndex =
                    0;

                return;
            }

            _chainIndex =
                nextIndex;

            ChangeTarget(
                next);
        }

        private HypnosisTarget FindChainTarget(
            HypnosisTarget source)
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best =
                null;

            float bestDistance =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    target == source ||
                    !target.isActiveAndEnabled ||
                    target.Owner !=
                    NpcOwner.Neutral)
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        source.transform.position,
                        target.transform.position);

                if (!ChainHypnosisLogic.IsInChainRadius(
                        distance,
                        ChainHypnosisLogic.ChainRadius) ||
                    distance >=
                    bestDistance)
                {
                    continue;
                }

                best =
                    target;

                bestDistance =
                    distance;
            }

            return best;
        }

        private bool IsChainTargetValid()
        {
            return _currentTarget != null &&
                   _currentTarget.isActiveAndEnabled &&
                   _currentTarget.Owner ==
                   NpcOwner.Neutral;
        }

        private void ClaimForPlayer(
            HypnosisTarget target)
        {
            if (target == null)
            {
                return;
            }

            bool wasReclaim =
                target.Owner !=
                NpcOwner.Neutral;

            target.OpponentOwner?.
                ReleaseOwnedTarget(
                    target);

            target.ClaimByPlayer();

            ResolveScoreTracker()?.
                ReportHypnosisSuccess(
                    wasReclaim);

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
