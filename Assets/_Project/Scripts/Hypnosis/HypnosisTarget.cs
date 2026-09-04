using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.NPC;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(NpcAgent))]
    public sealed class HypnosisTarget : MonoBehaviour
    {
        [SerializeField] private float _maximumHypnosis = 100f;
        [SerializeField] private float _buildPerSecond = 32f;

        private RuntimeCharacterSpriteAnimator _animator;

        public float CurrentHypnosis { get; private set; }

        public float HypnosisNormalized =>
            Mathf.Clamp01(
                CurrentHypnosis /
                Mathf.Max(
                    1f,
                    _maximumHypnosis));

        public bool IsHypnotized { get; private set; }

        public bool IsFollowing { get; private set; }

        public bool IsTargeted { get; private set; }

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
                !IsHypnotized;

            _animator?.SetHighlighted(
                IsTargeted);
        }

        public bool ApplyFocus(
            float deltaTime)
        {
            if (IsHypnotized)
            {
                return false;
            }

            CurrentHypnosis =
                HypnosisTargetingLogic.BuildProgress(
                    CurrentHypnosis,
                    _maximumHypnosis,
                    _buildPerSecond,
                    deltaTime);

            if (CurrentHypnosis <
                _maximumHypnosis)
            {
                return false;
            }

            CurrentHypnosis =
                _maximumHypnosis;

            IsHypnotized = true;

            SetTargeted(false);

            return true;
        }

        public void BeginFollowing()
        {
            if (!IsHypnotized)
            {
                return;
            }

            IsFollowing = true;
        }

        public void ReleaseFromFollowing()
        {
            IsFollowing = false;
            IsHypnotized = false;
            CurrentHypnosis = 0f;

            SetTargeted(false);
        }

        public void ResetHypnosis()
        {
            IsFollowing = false;
            IsHypnotized = false;
            CurrentHypnosis = 0f;

            SetTargeted(false);
        }
    }
}
