using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 화면 위쪽 가운데에 "업적 달성!"을 한 장씩 띄운다 (30일차).
    /// 여러 개면 차례로 보여 준다. 멈춘 화면에서도 움직이게 unscaled 시간을 쓴다.
    /// </summary>
    public sealed class AchievementToast : MonoBehaviour
    {
        public const float ShowSeconds = 2.8f;
        public const float FadeSeconds = 0.3f;

        private readonly Queue<AchievementDefinition> _queue =
            new Queue<AchievementDefinition>();

        private CanvasGroup _group;
        private Text _name;
        private Text _detail;
        private float _elapsed;
        private bool _showing;

        /// <summary>새 업적이 있으면 알림을 붙인다. 없으면 아무것도 하지 않는다.</summary>
        public static AchievementToast Show(
            Transform canvas,
            List<AchievementDefinition> unlocked)
        {
            if (canvas == null ||
                unlocked == null ||
                unlocked.Count == 0)
            {
                return null;
            }

            GameObject root =
                new GameObject(
                    "AchievementToast",
                    typeof(RectTransform),
                    typeof(Canvas),
                    typeof(CanvasGroup));

            root.transform.SetParent(canvas, false);
            UiFactory.Stretch((RectTransform)root.transform);

            Canvas overlay = root.GetComponent<Canvas>();
            overlay.overrideSorting = true;
            overlay.sortingOrder = 90;

            AchievementToast toast = root.AddComponent<AchievementToast>();

            toast.Build();

            for (int i = 0; i < unlocked.Count; i++)
            {
                toast._queue.Enqueue(unlocked[i]);
            }

            toast.Next();

            return toast;
        }

        private void Build()
        {
            _group = GetComponent<CanvasGroup>();
            _group.blocksRaycasts = false;
            _group.interactable = false;

            RectTransform panel =
                UiFactory.CreatePanel(
                    transform,
                    "Toast",
                    UiTheme.PanelFill,
                    UiTheme.Gold);

            UiFactory.Place(panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), new Vector2(620f, 96f));

            Text caption =
                UiFactory.CreateText(
                    panel,
                    "Caption",
                    "업적 달성!",
                    UiTheme.FontSmall,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(caption.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -10f), new Vector2(300f, 24f));

            _name =
                UiFactory.CreateText(
                    panel,
                    "Name",
                    string.Empty,
                    UiTheme.FontHeading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(_name.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -34f), new Vector2(572f, 32f));

            _detail =
                UiFactory.CreateText(
                    panel,
                    "Detail",
                    string.Empty,
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleLeft);

            UiFactory.Place(_detail.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -66f), new Vector2(572f, 22f));
        }

        private void Next()
        {
            if (_queue.Count == 0)
            {
                Destroy(gameObject);

                return;
            }

            AchievementDefinition definition = _queue.Dequeue();

            _name.text = definition.Name;
            _detail.text = $"{definition.Description}   ·   계약 정기 +{definition.Reward}";
            _elapsed = 0f;
            _showing = true;
            _group.alpha = 0f;

            GameAudio.Play(GameSfx.LevelUp);
        }

        private void Update()
        {
            if (!_showing)
            {
                return;
            }

            _elapsed += Time.unscaledDeltaTime;
            _group.alpha = GetAlpha(_elapsed);

            if (_elapsed >= ShowSeconds)
            {
                _showing = false;
                Next();
            }
        }

        /// <summary>나타남 → 머묾 → 사라짐 불투명도다.</summary>
        public static float GetAlpha(
            float elapsed)
        {
            if (elapsed <= 0f || elapsed >= ShowSeconds)
            {
                return 0f;
            }

            if (elapsed < FadeSeconds)
            {
                return elapsed / FadeSeconds;
            }

            if (elapsed > ShowSeconds - FadeSeconds)
            {
                return (ShowSeconds - elapsed) / FadeSeconds;
            }

            return 1f;
        }
    }
}
