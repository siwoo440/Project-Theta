using UnityEngine;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

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

        /// <summary>
        /// 마지막으로 적용한 정렬 값이다.
        /// 서 있는 NPC는 정렬 값이 안 바뀌는데도 매 프레임 렌더러마다 다시 쓰고 있었다.
        /// </summary>
        private int _appliedOrder = int.MinValue;

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

            _appliedOrder = int.MinValue;

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

            // 층은 세로로 쌓여 있으므로 세계 좌표를 그대로 쓰면 정렬 범위를 넘어간다.
            // 자기 층 안에서의 세로 위치로 환산해 정렬한다.
            // 층끼리는 화면에 함께 보이지 않으므로 값이 겹쳐도 문제가 없다.
            float localY =
                transform.position.y -
                FloorSpace.OriginY(
                    FloorSpace.FloorAt(
                        transform.position.y));

            int depthOrder =
                CharacterSortingLogic.GetSortingOrder(
                    _layer,
                    localY);

            if (depthOrder == _appliedOrder)
            {
                return;
            }

            _appliedOrder = depthOrder;

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
