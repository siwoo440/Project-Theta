using UnityEngine;

namespace ProjectTheta.Core
{
    public sealed class DepthSortByY : MonoBehaviour
    {
        [SerializeField] private int _baseOrder = 1000;
        [SerializeField] private float _unitsToOrder = 100f;
        [SerializeField] private bool _includeChildren = true;

        private SpriteRenderer[] _renderers;
        private int[] _relativeOrders;

        private void Awake()
        {
            CacheRenderers();
            ApplySorting();
        }

        private void LateUpdate()
        {
            ApplySorting();
        }

        public void Refresh()
        {
            CacheRenderers();
            ApplySorting();
        }

        private void CacheRenderers()
        {
            _renderers = _includeChildren
                ? GetComponentsInChildren<SpriteRenderer>(true)
                : GetComponents<SpriteRenderer>();

            _relativeOrders = new int[_renderers.Length];
            if (_renderers.Length == 0)
            {
                return;
            }

            int minimumOrder = _renderers[0].sortingOrder;
            for (int i = 1; i < _renderers.Length; i++)
            {
                minimumOrder = Mathf.Min(minimumOrder, _renderers[i].sortingOrder);
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                _relativeOrders[i] = _renderers[i].sortingOrder - minimumOrder;
            }
        }

        private void ApplySorting()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                return;
            }

            int yOrder = -Mathf.RoundToInt(transform.position.y * _unitsToOrder);
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].sortingOrder = _baseOrder + yOrder + _relativeOrders[i];
                }
            }
        }
    }
}
