using System.Collections.Generic;
using UnityEngine;

namespace ProjectTheta.NPC
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class NpcSoftSeparation : MonoBehaviour
    {
        [SerializeField] private float _desiredDistance = 0.65f;
        [SerializeField] private float _maximumPushSpeed = 0.9f;

        private static readonly List<NpcSoftSeparation> Active =
            new List<NpcSoftSeparation>();

        private Collider2D _collider;

        private void Awake()
        {
            _collider =
                GetComponent<Collider2D>();
        }

        private void OnEnable()
        {
            RemoveMissingEntries();

            if (_collider == null)
            {
                _collider =
                    GetComponent<Collider2D>();
            }

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                NpcSoftSeparation other =
                    Active[i];

                if (other == null ||
                    other._collider == null ||
                    _collider == null)
                {
                    continue;
                }

                Physics2D.IgnoreCollision(
                    _collider,
                    other._collider,
                    true);
            }

            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        public Vector2 GetCorrectionVelocity()
        {
            Vector2 origin =
                transform.position;

            Vector2 push =
                Vector2.zero;

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                NpcSoftSeparation other =
                    Active[i];

                if (other == null ||
                    other == this)
                {
                    continue;
                }

                Vector2 delta =
                    origin -
                    (Vector2)other.transform.position;

                float distance =
                    delta.magnitude;

                float weight =
                    NpcSoftSeparationLogic.ComputeWeight(
                        distance,
                        _desiredDistance);

                if (weight <= 0f)
                {
                    continue;
                }

                Vector2 direction;

                if (distance <= 0.001f)
                {
                    direction =
                        GetFallbackDirection(
                            other);
                }
                else
                {
                    direction =
                        delta /
                        distance;
                }

                push +=
                    direction *
                    weight;
            }

            if (push.sqrMagnitude <=
                0.0001f)
            {
                return Vector2.zero;
            }

            return Vector2.ClampMagnitude(
                push * _maximumPushSpeed,
                _maximumPushSpeed);
        }

        private Vector2 GetFallbackDirection(
            NpcSoftSeparation other)
        {
            int sign =
                GetInstanceID() <
                other.GetInstanceID()
                    ? -1
                    : 1;

            return new Vector2(
                sign,
                sign * 0.35f).normalized;
        }

        private static void RemoveMissingEntries()
        {
            for (int i = Active.Count - 1;
                 i >= 0;
                 i--)
            {
                if (Active[i] == null)
                {
                    Active.RemoveAt(i);
                }
            }
        }
    }
}
