using UnityEngine;

namespace ProjectTheta.Player
{
    public sealed class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private int _maximumHealth = 100;

        public int MaximumHealth =>
            Mathf.Max(
                1,
                _maximumHealth);

        public int CurrentHealth { get; private set; }

        public float HealthNormalized =>
            Mathf.Clamp01(
                CurrentHealth /
                (float)MaximumHealth);

        public bool IsDead =>
            CurrentHealth <= 0;

        private void Awake()
        {
            CurrentHealth =
                MaximumHealth;
        }

        public void TakeDamage(
            int amount)
        {
            int safeAmount =
                Mathf.Max(
                    0,
                    amount);

            CurrentHealth =
                Mathf.Clamp(
                    CurrentHealth -
                    safeAmount,
                    0,
                    MaximumHealth);
        }
    }
}
