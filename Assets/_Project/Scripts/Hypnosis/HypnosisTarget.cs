using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.NPC;
using ProjectTheta.Ownership;
using ProjectTheta.Rival;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(NpcAgent))]
    public sealed class HypnosisTarget : MonoBehaviour
    {
        [SerializeField] private float _maximumHypnosis = 100f;
        [SerializeField] private float _buildPerSecond = 32f;
        [SerializeField] private float _playerReclaimPerSecond = 24f;

        private RuntimeCharacterSpriteAnimator _animator;

        public float CurrentHypnosis { get; private set; }

        public float MaximumHypnosis =>
            Mathf.Max(
                1f,
                _maximumHypnosis);

        public float HypnosisNormalized =>
            Mathf.Clamp01(
                CurrentHypnosis /
                MaximumHypnosis);

        public NpcOwner Owner { get; private set; } =
            NpcOwner.Neutral;

        public RivalController RivalOwner { get; private set; }

        public bool IsHypnotized =>
            Owner !=
            NpcOwner.Neutral;

        public bool IsFollowing { get; private set; }

        public bool IsTargeted { get; private set; }

        public bool CanPlayerFocus =>
            OwnershipContestLogic.CanPlayerContest(
                Owner);

        private void Awake()
        {
            _animator =
                GetComponent<
                    RuntimeCharacterSpriteAnimator>();
        }

        public void SetTargeted(
            bool targeted)
        {
            IsTargeted =
                targeted &&
                CanPlayerFocus;

            _animator?.SetHighlighted(
                IsTargeted);
        }

        public bool ApplyFocus(
            float deltaTime)
        {
            return ApplyPlayerFocus(
                deltaTime);
        }

        public bool ApplyPlayerFocus(
            float deltaTime)
        {
            if (!CanPlayerFocus)
            {
                return false;
            }

            if (Owner ==
                NpcOwner.Neutral)
            {
                CurrentHypnosis =
                    HypnosisTargetingLogic.BuildProgress(
                        CurrentHypnosis,
                        MaximumHypnosis,
                        _buildPerSecond,
                        deltaTime);

                return CurrentHypnosis >=
                       MaximumHypnosis;
            }

            if (Owner ==
                NpcOwner.Rival)
            {
                CurrentHypnosis =
                    OwnershipContestLogic.Drain(
                        CurrentHypnosis,
                        _playerReclaimPerSecond,
                        deltaTime);

                return OwnershipContestLogic.IsDepleted(
                    CurrentHypnosis);
            }

            return false;
        }

        public bool ApplyRivalPressure(
            float drainPerSecond,
            float deltaTime)
        {
            if (!OwnershipContestLogic.CanRivalContest(
                    Owner) ||
                !IsFollowing)
            {
                return false;
            }

            CurrentHypnosis =
                OwnershipContestLogic.Drain(
                    CurrentHypnosis,
                    drainPerSecond,
                    deltaTime);

            return OwnershipContestLogic.IsDepleted(
                CurrentHypnosis);
        }

        public void ClaimByPlayer()
        {
            Owner =
                NpcOwner.Player;

            RivalOwner = null;

            CurrentHypnosis =
                MaximumHypnosis;

            IsFollowing = false;

            SetTargeted(
                false);
        }

        public void ClaimByRival(
            RivalController rival)
        {
            if (rival == null)
            {
                return;
            }

            Owner =
                NpcOwner.Rival;

            RivalOwner =
                rival;

            CurrentHypnosis =
                MaximumHypnosis;

            IsFollowing = false;

            SetTargeted(
                false);
        }

        public void BeginFollowing()
        {
            if (Owner !=
                NpcOwner.Player)
            {
                return;
            }

            IsFollowing = true;
        }

        public void StopPlayerFollowingForTransfer()
        {
            IsFollowing = false;
        }

        public void ReleaseFromFollowing()
        {
            IsFollowing = false;

            ResetToNeutral();
        }

        public void ResetHypnosis()
        {
            IsFollowing = false;

            ResetToNeutral();
        }

        private void ResetToNeutral()
        {
            Owner =
                NpcOwner.Neutral;

            RivalOwner = null;

            CurrentHypnosis = 0f;

            SetTargeted(
                false);
        }
    }
}
