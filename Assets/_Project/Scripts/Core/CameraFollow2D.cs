using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothTime = 0.12f;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.45f, -10f);
        [SerializeField, Range(0f, 1f)] private float _verticalFollowScale = 0.5f;
        [SerializeField] private bool _useBounds;
        [SerializeField] private Vector2 _minimum = new Vector2(-10.5f, -1.25f);
        [SerializeField] private Vector2 _maximum = new Vector2(10.5f, 1.65f);

        private Vector3 _velocity;

        public void Configure(Transform target)
        {
            _target = target;
        }

        public void Configure(Transform target, Vector2 minimum, Vector2 maximum)
        {
            _target = target;
            _minimum = minimum;
            _maximum = maximum;
            _useBounds = true;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = new Vector3(
                _target.position.x + _offset.x,
                (_target.position.y * _verticalFollowScale) + _offset.y,
                _offset.z);

            if (_useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, _minimum.x, _maximum.x);
                desired.y = Mathf.Clamp(desired.y, _minimum.y, _maximum.y);
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _velocity,
                _smoothTime);
        }
    }
}
