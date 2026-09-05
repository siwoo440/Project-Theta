using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Player;

namespace ProjectTheta.Stage
{
    public sealed class StageTelemetry : MonoBehaviour
    {
        private StageSessionController _stage;
        private FollowerManager _followers;
        private PlayerHealth _health;

        public float FirstFollowerTime { get; private set; } = -1f;
        public float ThreeFollowersTime { get; private set; } = -1f;
        public float FiveFollowersTime { get; private set; } = -1f;
        public float Essence100Time { get; private set; } = -1f;
        public float Essence200Time { get; private set; } = -1f;

        public int FinalHealth =>
            _health == null
                ? 0
                : _health.CurrentHealth;

        public void Configure(
            StageSessionController stage,
            FollowerManager followers,
            PlayerHealth health)
        {
            _stage = stage;
            _followers = followers;
            _health = health;
        }

        private void Update()
        {
            if (_stage == null ||
                _followers == null)
            {
                return;
            }

            float elapsed =
                _stage.ElapsedTime;

            if (FirstFollowerTime < 0f &&
                _followers.Count >= 1)
            {
                FirstFollowerTime =
                    elapsed;
            }

            if (ThreeFollowersTime < 0f &&
                _followers.Count >= 3)
            {
                ThreeFollowersTime =
                    elapsed;
            }

            if (FiveFollowersTime < 0f &&
                _followers.Count >= 5)
            {
                FiveFollowersTime =
                    elapsed;
            }

            if (Essence100Time < 0f &&
                _stage.CurrentEssence >= 100)
            {
                Essence100Time =
                    elapsed;
            }

            if (Essence200Time < 0f &&
                _stage.CurrentEssence >= 200)
            {
                Essence200Time =
                    elapsed;
            }
        }

        public string FormatTime(
            float seconds)
        {
            if (seconds < 0f)
            {
                return "-";
            }

            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.RoundToInt(
                        seconds));

            int minutes =
                totalSeconds /
                60;

            int remainder =
                totalSeconds %
                60;

            return $"{minutes:00}:{remainder:00}";
        }
    }
}
