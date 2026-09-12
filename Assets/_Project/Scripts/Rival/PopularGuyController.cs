using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 인기남. 중립 NPC를 3단계에 걸쳐 선점하는 것을 최우선으로 하고,
    /// 중립이 없으면 플레이어·금태양 소유 NPC를 쟁탈한다. 금태양보다 느리고 여유 있다.
    /// </summary>
    public sealed class PopularGuyController : OpponentControllerBase
    {
        [SerializeField] private float _neutralClaimStepInterval = 0.75f;

        private int _neutralClaimStep;
        private float _neutralClaimTimer;
        private AudioSource _claimTickSource;

        public override NpcOwner OwnerTag =>
            NpcOwner.PopularGuy;

        public override string DisplayName =>
            "인기남";

        public override bool CanStartPlayerDuel =>
            PopularGuyDuelLogic.CanStart(
                IsDuelLocked,
                DuelStunRemaining,
                State,
                TargetMode,
                Target != null,
                Target == null
                    ? NpcOwner.PopularGuy
                    : Target.Owner);

        protected override OpponentTuning CreateDefaultTuning()
        {
            return new OpponentTuning
            {
                MoveSpeed = 2.1f,
                StopDistance = 0.92f,

                SearchRange = 999f,
                ReacquireInterval = 0.90f,
                AbandonDistance = 13f,
                MaximumPursuitDuration = 7.5f,

                MinimumIdleDuration = 1.8f,
                MaximumIdleDuration = 4.5f,
                LostTargetIdleMinimum = 1.2f,
                LostTargetIdleMaximum = 2.7f,
                PostCaptureIdleMinimum = 1.5f,
                PostCaptureIdleMaximum = 3.6f,

                ActionDistance = 1.20f,
                ContestDrainPerSecond = 12f
            };
        }

        protected override void Awake()
        {
            base.Awake();

            _claimTickSource =
                gameObject.AddComponent<
                    AudioSource>();

            _claimTickSource.playOnAwake =
                false;

            _claimTickSource.spatialBlend =
                0f;

            _claimTickSource.volume =
                0.45f;
        }

        protected override void SearchForTarget()
        {
            HypnosisTarget neutral =
                FindNearestTarget(
                    OpponentTargetMode.NeutralClaim);

            if (neutral != null)
            {
                AssignTarget(
                    neutral,
                    OpponentTargetMode.NeutralClaim);

                return;
            }

            HypnosisTarget contested =
                FindNearestTarget(
                    OpponentTargetMode.Contest);

            if (contested != null)
            {
                AssignTarget(
                    contested,
                    OpponentTargetMode.Contest);
            }
        }

        protected override bool IsTargetValid(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            if (target == null ||
                !target.isActiveAndEnabled)
            {
                return false;
            }

            if (mode ==
                OpponentTargetMode.NeutralClaim)
            {
                return target.Owner ==
                       NpcOwner.Neutral;
            }

            if (mode !=
                    OpponentTargetMode.Contest ||
                !PopularGuyLogic.CanContest(
                    target.Owner))
            {
                return false;
            }

            if (target.Owner ==
                NpcOwner.Player)
            {
                return IsPlayerFollowerContestable(
                    target);
            }

            return true;
        }

        protected override void PerformAction(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            if (mode ==
                OpponentTargetMode.NeutralClaim)
            {
                State =
                    OpponentState.Claiming;

                ProcessNeutralClaimSteps(
                    target);

                return;
            }

            State =
                OpponentState.Contest;

            bool depleted =
                target.ApplyOpponentPressure(
                    OwnerTag,
                    Tuning.ContestDrainPerSecond,
                    Time.deltaTime);

            if (depleted)
            {
                CaptureContestedTarget(
                    target);
            }
        }

        protected override void OnTargetAssigned(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            ResetNeutralClaim();

            if (mode ==
                OpponentTargetMode.NeutralClaim)
            {
                target.SetOpponentClaimProgress(
                    0f);
            }
        }

        protected override void OnTargetVisualsCleared(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            if (mode ==
                OpponentTargetMode.NeutralClaim)
            {
                target.ClearOpponentClaimProgress();
            }
        }

        protected override void OnTargetReleased()
        {
            ResetNeutralClaim();
        }

        private void ProcessNeutralClaimSteps(
            HypnosisTarget target)
        {
            if (target.Owner !=
                NpcOwner.Neutral)
            {
                ClearTarget();

                return;
            }

            _neutralClaimTimer +=
                Time.deltaTime;

            float interval =
                Mathf.Max(
                    0.05f,
                    _neutralClaimStepInterval);

            while (_neutralClaimTimer >=
                   interval)
            {
                _neutralClaimTimer -=
                    interval;

                _neutralClaimStep =
                    PopularGuyNeutralClaimLogic.
                        NextStep(
                            _neutralClaimStep);

                target.SetOpponentClaimProgress(
                    PopularGuyNeutralClaimLogic.
                        Normalized(
                            _neutralClaimStep));

                PlayClaimTick();

                if (PopularGuyNeutralClaimLogic.
                        IsComplete(
                            _neutralClaimStep))
                {
                    CaptureNeutralTarget(
                        target);

                    break;
                }
            }
        }

        private void CaptureNeutralTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Neutral)
            {
                ClearTarget();

                return;
            }

            target.ClaimByOpponent(
                this);

            OwnedFollowers?.TryAdd(
                target);

            FinishSuccessfulCapture();
        }

        private void CaptureContestedTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                !PopularGuyLogic.CanContest(
                    target.Owner))
            {
                ClearTarget();

                return;
            }

            if (target.Owner ==
                NpcOwner.Player)
            {
                if (!TryDetachFromPlayer(
                        target))
                {
                    ClearTarget();

                    return;
                }
            }
            else
            {
                target.OpponentOwner?.
                    ReleaseOwnedTarget(
                        target);
            }

            target.ClaimByOpponent(
                this);

            OwnedFollowers?.TryAdd(
                target);

            FinishSuccessfulCapture();
        }

        private HypnosisTarget FindNearestTarget(
            OpponentTargetMode mode)
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            HypnosisTarget best =
                null;

            float bestDistanceSquared =
                float.MaxValue;

            float range =
                Mathf.Max(
                    0f,
                    Tuning.SearchRange);

            float rangeSquared =
                range * range;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (!IsTargetValid(
                        candidate,
                        mode))
                {
                    continue;
                }

                float distanceSquared =
                    ((Vector2)candidate.transform.position -
                     (Vector2)transform.position).
                    sqrMagnitude;

                if (distanceSquared >
                    rangeSquared ||
                    distanceSquared >=
                    bestDistanceSquared)
                {
                    continue;
                }

                best =
                    candidate;

                bestDistanceSquared =
                    distanceSquared;
            }

            return best;
        }

        private void ResetNeutralClaim()
        {
            _neutralClaimStep =
                0;

            _neutralClaimTimer =
                0f;
        }

        private void PlayClaimTick()
        {
            GameAudio.PlayAt(
                _claimTickSource,
                GameSfx.ClaimTick);
        }


    }
}
