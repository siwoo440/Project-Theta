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
        private bool _geumtaeyangTargeted;
        private bool _popularGuyTargeted;
        private float _popularGuyClaimNormalized;

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

        public RivalController GeumtaeyangOwner { get; private set; }

        public PopularGuyController PopularGuyOwner { get; private set; }

        public bool IsHypnotized =>
            Owner !=
            NpcOwner.Neutral;

        public bool IsFollowing { get; private set; }

        public bool IsTargeted { get; private set; }

        public bool IsOpponentTargeted =>
            _geumtaeyangTargeted ||
            _popularGuyTargeted;

        public float PopularGuyClaimNormalized =>
            Mathf.Clamp01(
                _popularGuyClaimNormalized);

        public NpcOwner PrimaryThreatOwner =>
            _popularGuyTargeted
                ? NpcOwner.PopularGuy
                : _geumtaeyangTargeted
                    ? NpcOwner.Geumtaeyang
                    : NpcOwner.Neutral;

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

        public void SetOpponentTargeted(
            NpcOwner opponent,
            bool targeted)
        {
            switch (opponent)
            {
                case NpcOwner.Geumtaeyang:
                    _geumtaeyangTargeted =
                        targeted;
                    break;

                case NpcOwner.PopularGuy:
                    _popularGuyTargeted =
                        targeted;

                    if (!targeted &&
                        Owner ==
                        NpcOwner.Neutral)
                    {
                        ClearPopularGuyClaimProgress();
                    }
                    break;
            }
        }

        public void SetPopularGuyClaimProgress(
            float normalized)
        {
            _popularGuyClaimNormalized =
                Mathf.Clamp01(
                    normalized);
        }

        public void ClearPopularGuyClaimProgress()
        {
            _popularGuyClaimNormalized =
                0f;
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

            CurrentHypnosis =
                OwnershipContestLogic.Drain(
                    CurrentHypnosis,
                    _playerReclaimPerSecond,
                    deltaTime);

            return OwnershipContestLogic.IsDepleted(
                CurrentHypnosis);
        }

        public bool ApplyGeumtaeyangPressure(
            float drainPerSecond,
            float deltaTime)
        {
            if (!OwnershipContestLogic.
                    CanGeumtaeyangContest(
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

        public bool ApplyPopularGuyPressure(
            float drainPerSecond,
            float deltaTime)
        {
            if (!PopularGuyLogic.CanContest(
                    Owner))
            {
                return false;
            }

            if (Owner ==
                    NpcOwner.Player &&
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

            GeumtaeyangOwner = null;
            PopularGuyOwner = null;

            CurrentHypnosis =
                MaximumHypnosis;

            ClearPopularGuyClaimProgress();

            IsFollowing = false;

            ClearOpponentTargeting();

            SetTargeted(
                false);
        }

        public void ClaimByGeumtaeyang(
            RivalController geumtaeyang)
        {
            if (geumtaeyang == null)
            {
                return;
            }

            Owner =
                NpcOwner.Geumtaeyang;

            GeumtaeyangOwner =
                geumtaeyang;

            PopularGuyOwner = null;

            CurrentHypnosis =
                MaximumHypnosis;

            ClearPopularGuyClaimProgress();

            IsFollowing = false;

            ClearOpponentTargeting();

            SetTargeted(
                false);
        }

        public void ClaimByPopularGuy(
            PopularGuyController popularGuy)
        {
            if (popularGuy == null)
            {
                return;
            }

            Owner =
                NpcOwner.PopularGuy;

            PopularGuyOwner =
                popularGuy;

            GeumtaeyangOwner = null;

            CurrentHypnosis =
                MaximumHypnosis;

            ClearPopularGuyClaimProgress();

            IsFollowing = false;

            ClearOpponentTargeting();

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

        private void ClearOpponentTargeting()
        {
            _geumtaeyangTargeted = false;
            _popularGuyTargeted = false;
        }

        private void ResetToNeutral()
        {
            Owner =
                NpcOwner.Neutral;

            GeumtaeyangOwner = null;
            PopularGuyOwner = null;

            CurrentHypnosis = 0f;

            ClearPopularGuyClaimProgress();

            ClearOpponentTargeting();

            SetTargeted(
                false);
        }
    }
}
