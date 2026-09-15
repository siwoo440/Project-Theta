using UnityEngine;
using ProjectTheta.Stage;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 플레이어를 따라가는 2D 카메라다.
    ///
    /// 16일차부터 층이 세로로 쌓이므로, 세로 추적과 경계는 모두
    /// <b>플레이어가 선 층의 원점 기준</b>으로 계산한다.
    /// 그렇게 해야 3층에서도 1층과 똑같은 화면 구도가 나온다.
    /// </summary>
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

        /// <summary>층을 옮긴 직후처럼 즉시 따라붙어야 할 때 쓴다.</summary>
        public void SnapToTarget()
        {
            if (_target == null)
            {
                return;
            }

            transform.position = GetDesiredPosition();
            _velocity = Vector3.zero;
        }

        private Vector3 GetDesiredPosition()
        {
            float originY =
                FloorSpace.OriginY(
                    FloorSpace.FloorAt(
                        _target.position.y));

            float localY =
                _target.position.y - originY;

            Vector3 desired = new Vector3(
                _target.position.x + _offset.x,
                (localY * _verticalFollowScale) + _offset.y,
                _offset.z);

            if (_useBounds)
            {
                desired.x = Mathf.Clamp(desired.x, _minimum.x, _maximum.x);
                desired.y = Mathf.Clamp(desired.y, _minimum.y, _maximum.y);
            }

            desired.y += originY;

            return desired;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetDesiredPosition(),
                ref _velocity,
                _smoothTime);
        }
    }
}
