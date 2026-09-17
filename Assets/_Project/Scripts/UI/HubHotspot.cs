using UnityEngine;
using UnityEngine.EventSystems;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 방 물건 위에 마우스를 올리면 빛과 이름표를 서서히 보여 준다 (36일차).
    /// 누르는 동작은 같은 오브젝트의 Button이 맡는다.
    /// </summary>
    public sealed class HubHotspot :
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private const float Speed = 8f;

        /// <summary>빛 · 이름표를 묶은 그룹이다.</summary>
        public CanvasGroup Highlight;

        private bool _hovered;

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            _hovered = true;
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            _hovered = false;
        }

        private void OnDisable()
        {
            _hovered = false;

            if (Highlight != null)
            {
                Highlight.alpha = 0f;
            }
        }

        private void Update()
        {
            if (Highlight == null)
            {
                return;
            }

            Highlight.alpha =
                Mathf.MoveTowards(
                    Highlight.alpha,
                    _hovered ? 1f : 0f,
                    Speed * Time.unscaledDeltaTime);
        }
    }
}
