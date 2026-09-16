using UnityEngine;
using ProjectTheta.Stage;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Companion;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Rival;
using ProjectTheta.Run;
using ProjectTheta.Core;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(PlayerSideViewController))]
    [RequireComponent(typeof(FollowerManager))]
    public sealed class HypnosisCaster : MonoBehaviour
    {
        /// <summary>최면 대상 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<HypnosisTarget> _targetBuffer =
            new System.Collections.Generic.List<HypnosisTarget>();
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

        /// <summary>이번 프레임에 최면 입력을 누르고 있는지다 (26일차, 보스전이 읽는다).</summary>
        public bool IsHolding { get; private set; }

        /// <summary>
        /// 최면을 NPC 대신 다른 대상(보스)이 가져갈지 묻는다 (26일차). 플레이어 위치를 받아 true면 NPC를 잡지 않는다.
        /// 보스전이 설정하고 끝날 때 지운다.
        /// </summary>
        public static System.Func<Vector2, bool> FocusOverride { get; set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            FocusOverride = null;
        }

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
            if (GameplayPause.IsPaused)
            {
                return;
            }

            _interruptRemaining =
                Mathf.Max(
                    0f,
                    _interruptRemaining -
                    Time.deltaTime);

            // 19일차: 집중력은 더 이상 최면을 막지 않는다. 가속만 준다.
            IsHolding =
                _interruptRemaining <= 0f &&
                ReadHypnosisHeld();

            if (!IsHolding)
            {
                ChangeTarget(null);

                _chainIndex =
                    0;

                return;
            }

            // 26일차: 보스를 겨누는 동안은 NPC를 잡지 않는다. 보스전이 최면을 가져간다.
            if (FocusOverride != null &&
                FocusOverride(transform.position))
            {
                ChangeTarget(null);

                _chainIndex = 0;

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

            // 집중력이 남아 있으면 가속이 붙는다. 이번 프레임에 쓰기 전의 양으로 판정한다.
            // 바닥나도 최면은 기본 속도로 계속된다.
            float focusMultiplier =
                _focus == null
                    ? 1f
                    : _focus.HypnosisSpeedMultiplier;

            if (_focus != null)
            {
                _focus.DrainContinuous(
                    _focus.HypnosisDrainPerSecond,
                    Time.deltaTime);
            }

            bool completed =
                _currentTarget.ApplyPlayerFocus(
                    Time.deltaTime,
                    ChainHypnosisLogic.GetSpeedMultiplier(
                        _chainIndex) *
                    focusMultiplier);

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
                    _chainIndex,
                    RunUpgradeMultipliers.ChainExtraTargets))
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
                    _chainIndex,
                    RunUpgradeMultipliers.ChainExtraTargets);

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
            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            HypnosisTarget best =
                null;

            float bestDistance =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Count;
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

            StageMoments.RaiseHypnosisSucceeded(
                target.transform.position,
                wasReclaim);

            if (_followerManager == null ||
                !_followerManager.TryAdd(
                    target))
            {
                target.ResetHypnosis();
            }
        }

        /// <summary>
        /// 디버그 치트: 게이지를 채우지 않고 바로 최면을 성공시킨다 (20일차).
        /// 실제 최면 성공과 같은 길(점수·경험치·연출·동행 합류)을 그대로 탄다.
        /// 이미 플레이어 소유면 아무것도 하지 않는다.
        /// </summary>
        public bool DebugClaim(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner ==
                NpcOwner.Player)
            {
                return false;
            }

            ClaimForPlayer(
                target);

            return target.Owner ==
                   NpcOwner.Player;
        }

        private HypnosisTarget FindBestTarget()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            HypnosisTarget best = null;

            // 23일차: 야시장 어둠 속에서는 등불 · 조명 밖이면 사거리가 줄어든다.
            float scanRange =
                _scanRange *
                Stage.Locations.LanternLight.GetRangeMultiplier(
                    transform.position) *
                // 25일차: 오피스 정전 중에는 사거리가 줄어든다.
                Stage.Locations.Blackout.RangeMultiplier;

            float bestDistanceSquared =
                float.MaxValue;

            for (int i = 0;
                 i < targets.Count;
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
                        scanRange,
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
            // 32일차: 키 설정을 따른다. 디버그 패널 위 클릭 무시도 GameInput이 한다.
            return GameInput.IsHeld(
                GameAction.Hypnosis);
        }

        private void OnDisable()
        {
            ChangeTarget(null);
        }
    }
}
