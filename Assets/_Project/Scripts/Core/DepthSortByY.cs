using UnityEngine;
using ProjectTheta.Presentation;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 2.5D 사이드뷰에서 Y 좌표로 스프라이트 앞뒤 순서를 정한다.
    ///
    /// 정렬 규칙 자체는 <see cref="CharacterSortingLogic"/>에 있고
    /// 이 컴포넌트는 그 결과를 렌더러에 적용하기만 한다.
    /// </summary>
    public sealed class DepthSortByY : MonoBehaviour
    {
        [SerializeField] private CharacterLayer _layer =
            CharacterLayer.Character;

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

            int depthOrder =
                CharacterSortingLogic.GetSortingOrder(
                    _layer,
                    transform.position.y);

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].sortingOrder = depthOrder + _relativeOrders[i];
                }
            }
        }
    }
}
