using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 조작법 창이다 (31일차). <see cref="ControlsCatalog"/> 표를 그대로 보여 준다.
    /// 스테이지에서 열면 아래에 이번 장소의 규칙 요약도 붙인다.
    /// </summary>
    public sealed class ControlsPanel : MonoBehaviour
    {
        private const float RowHeight = 40f;

        private UiOverlayParts _parts;
        private Text _extra;

        public bool IsOpen =>
            _parts.Root != null &&
            _parts.Root.activeSelf;

        public static ControlsPanel Create(
            Transform canvas,
            int sortingOrder)
        {
            GameObject host = new GameObject("ControlsPanel");
            host.transform.SetParent(canvas, false);

            ControlsPanel panel = host.AddComponent<ControlsPanel>();
            panel.Build(canvas, sortingOrder);

            return panel;
        }

        private void Build(
            Transform canvas,
            int sortingOrder)
        {
            _parts =
                UiOverlay.Create(
                    canvas,
                    "ControlsOverlay",
                    sortingOrder,
                    new Vector2(1100f, 800f),
                    "조작법",
                    Close);

            RectTransform w = _parts.Window;
            float top = 100f;
            string lastGroup = null;

            for (int i = 0; i < ControlsCatalog.All.Length; i++)
            {
                ControlRow row = ControlsCatalog.All[i];

                if (row.Group != lastGroup)
                {
                    lastGroup = row.Group;
                    UiOverlay.Label(w, row.Group, 44f, top, 120f, UiTheme.FontSmall, UiTheme.Gold, true);
                }

                UiOverlay.Label(w, row.Action, 170f, top, 440f, UiTheme.FontBody, UiTheme.TextPrimary);
                UiOverlay.Label(w, row.Keys, 620f, top, 440f, UiTheme.FontBody, UiTheme.Accent, true);

                top += RowHeight;
            }

            _extra =
                UiFactory.CreateText(
                    w,
                    "Extra",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.UpperLeft);

            _extra.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiFactory.Place(_extra.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(44f, -top - 14f), new Vector2(1010f, 800f - top - 40f));
        }

        /// <summary>창을 연다. extra는 아래에 붙일 설명(장소 규칙 등)이다.</summary>
        public void Open(
            string extra = null)
        {
            if (_parts.Root == null)
            {
                return;
            }

            _extra.text = extra ?? string.Empty;
            _parts.Root.SetActive(true);
            UiEscapeStack.Push(this);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            _parts.Root.SetActive(false);
            UiEscapeStack.Remove(this);
        }

        private void OnDestroy()
        {
            UiEscapeStack.Remove(this);
        }

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard = Keyboard.current;

            if (keyboard != null &&
                keyboard.escapeKey.wasPressedThisFrame &&
                UiEscapeStack.TryConsume(this, Time.frameCount))
            {
                Close();
            }
#endif
        }
    }
}
