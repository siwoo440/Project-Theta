using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform _target; // 추적 대상
        [SerializeField] private float _smoothTime = 0.15f; // 추적 보간
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.2f, -10f); // 카메라 오프셋
        private Vector3 _velocity; // 보간 속도

        public void Configure(Transform target)
        {
            _target = target; // 대상 설정
        }

        private void LateUpdate()
        {
            if (_target == null) // 대상 확인
            {
                return; // 추적 중단
            }

            Vector3 desired = _target.position + _offset; // 목표 위치
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime); // 카메라 추적
        }
    }
}
