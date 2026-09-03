using UnityEngine;

namespace ProjectTheta.Core
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class DepthSortByY : MonoBehaviour
    {
        [SerializeField] private int _baseOrder = 1000; // 기본 정렬값
        [SerializeField] private float _precision = 100f; // 깊이 정밀도
        private SpriteRenderer _renderer; // 스프라이트 렌더러

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>(); // 렌더러 참조
        }

        private void LateUpdate()
        {
            _renderer.sortingOrder = _baseOrder - Mathf.RoundToInt(transform.position.y * _precision); // 아래쪽을 앞에 정렬
        }
    }
}
