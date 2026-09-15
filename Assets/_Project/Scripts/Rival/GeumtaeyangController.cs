using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 금태양. 플레이어가 이미 확보한 동행 NPC를 직접 빼앗는 공격적인 경쟁자다.
    /// 중립 NPC는 노리지 않는다.
    /// </summary>
    public sealed class GeumtaeyangController : OpponentControllerBase
    {
        /// <summary>최면 대상 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<HypnosisTarget> _targetBuffer =
            new System.Collections.Generic.List<HypnosisTarget>();
        public override NpcOwner OwnerTag =>
            NpcOwner.Geumtaeyang;

        public override string DisplayName =>
            "금태양";

        public override bool CanStartPlayerDuel =>
            !IsDuelLocked &&
            DuelStunRemaining <=
                0f &&
            State ==
                OpponentState.Contest &&
            Target != null &&
            Target.Owner ==
                NpcOwner.Player;

        protected override OpponentTuning CreateDefaultTuning()
        {
            return new OpponentTuning
            {
                MoveSpeed = 4.2f,
                StopDistance = 0.92f,

                SearchRange = 10f,
                ReacquireInterval = 0.30f,
                AbandonDistance = 13f,
                MaximumPursuitDuration = 5.0f,

                MinimumIdleDuration = 0.6f,
                MaximumIdleDuration = 1.5f,
                LostTargetIdleMinimum = 0.4f,
                LostTargetIdleMaximum = 0.9f,
                PostCaptureIdleMinimum = 0.5f,
                PostCaptureIdleMaximum = 1.2f,

                ActionDistance = 1.20f,
                ContestDrainPerSecond = 18f
            };
        }

        protected override void SearchForTarget()
        {
            HypnosisTarget best =
                FindBestPlayerTarget();

            if (best == null)
            {
                return;
            }

            AssignTarget(
                best,
                OpponentTargetMode.Contest);
        }

        protected override bool IsTargetValid(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            if (target == null ||
                !target.isActiveAndEnabled ||
                target.Owner !=
                NpcOwner.Player ||
                !IsOnSameFloor(
                    target))
            {
                return false;
            }

            return IsPlayerFollowerContestable(
                target);
        }

        protected override void PerformAction(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            State =
                OpponentState.Contest;

            bool depleted =
                target.ApplyOpponentPressure(
                    OwnerTag,
                    Tuning.ContestDrainPerSecond,
                    Time.deltaTime);

            if (depleted)
            {
                CaptureTarget(
                    target);
            }
        }

        private void CaptureTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                target.Owner !=
                NpcOwner.Player ||
                !TryDetachFromPlayer(
                    target))
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

        private HypnosisTarget FindBestPlayerTarget()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            HypnosisTarget best =
                null;

            float bestScore =
                float.NegativeInfinity;

            float range =
                Mathf.Max(
                    0f,
                    Tuning.SearchRange);

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (!IsTargetValid(
                        candidate,
                        OpponentTargetMode.Contest))
                {
                    continue;
                }

                float opponentDistance =
                    Vector2.Distance(
                        transform.position,
                        candidate.transform.position);

                if (opponentDistance >
                    range)
                {
                    continue;
                }

                float playerDistance =
                    PlayerFollowers == null
                        ? 0f
                        : Vector2.Distance(
                            PlayerFollowers.transform.position,
                            candidate.transform.position);

                float score =
                    OpponentTargetingLogic.
                        ScoreGeumtaeyangTarget(
                            opponentDistance,
                            playerDistance);

                if (score <=
                    bestScore)
                {
                    continue;
                }

                best =
                    candidate;

                bestScore =
                    score;
            }

            return best;
        }
    }
}
