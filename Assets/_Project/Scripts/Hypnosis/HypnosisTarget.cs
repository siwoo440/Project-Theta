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
                Mathf.Max(1f, _maximumHypnosis));

        public bool IsComplete =>
            CurrentHypnosis >= _maximumHypnosis;

        public bool IsHypnotized { get; private set; }

        public bool IsTargeted { get; private set; }

        private void Awake()
        {
            _animator =
                GetComponent<RuntimeCharacterSpriteAnimator>();
        }

        public void SetTargeted(bool targeted)
        {
            IsTargeted =
                targeted && !IsHypnotized;

            _animator?.SetHighlighted(
                IsTargeted);
        }

        public void ApplyFocus(float deltaTime)
        {
            if (IsHypnotized)
            {
                return;
            }

            CurrentHypnosis =
                HypnosisTargetingLogic.BuildProgress(
                    CurrentHypnosis,
                    _maximumHypnosis,
                    _buildPerSecond,
                    deltaTime);

            if (CurrentHypnosis >= _maximumHypnosis)
            {
                CurrentHypnosis = _maximumHypnosis;
                IsHypnotized = true;
                SetTargeted(false);
            }
        }
    }
}
