using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 허브 배경인 "밤의 학생 방"이다 (36일차).
    ///
    ///   포스터        창문(밤하늘 · 달 · 도시 불빛) + 커튼        벽시계
    ///   책장          책상(노트 · 스탠드) · 의자        협탁(일기장) · 침대
    ///   ─────────────────── 바닥 · 러그 ───────────────────
    ///
    /// 그림 자산이 없어 사각형 · 흐린 원으로만 그린다. 그림이 생기면 이 파일 하나만 바꾸면 된다.
    /// 누를 수 있는 물건(노트 · 시계 · 책장 · 창문 · 일기장)은 <see cref="ObjectClicked"/>로 알린다.
    /// 창밖 불빛은 천천히 깜빡이고, 도시 지배도만큼 보라색으로 물든다(<see cref="SetDominion"/>).
    /// 좌표는 1920×1080 화면 가운데 기준이다.
    /// </summary>
    public sealed class HubRoomBackdrop : MonoBehaviour
    {
        private static readonly Color Wall = new Color(0.105f, 0.085f, 0.150f, 1f);
        private static readonly Color WallStripe = new Color(0.14f, 0.11f, 0.20f, 0.55f);
        private static readonly Color Floor = new Color(0.145f, 0.100f, 0.105f, 1f);
        private static readonly Color Plank = new Color(0.11f, 0.075f, 0.08f, 1f);
        private static readonly Color Wood = new Color(0.30f, 0.20f, 0.17f, 1f);
        private static readonly Color WoodDark = new Color(0.20f, 0.13f, 0.12f, 1f);
        private static readonly Color Frame = new Color(0.24f, 0.19f, 0.30f, 1f);
        private static readonly Color Curtain = new Color(0.36f, 0.20f, 0.46f, 1f);
        private static readonly Color CurtainFold = new Color(0.28f, 0.15f, 0.37f, 1f);
        private static readonly Color Building = new Color(0.07f, 0.06f, 0.12f, 1f);
        private static readonly Color LightWarm = new Color(1.00f, 0.84f, 0.48f, 1f);
        private static readonly Color LightOwned = new Color(0.82f, 0.46f, 1.00f, 1f);
        private static readonly Color LampLight = new Color(1.00f, 0.78f, 0.45f, 1f);

        private readonly List<Image> _cityLights = new List<Image>();
        private readonly List<float> _lightPhases = new List<float>();
        private RectTransform _root;
        private RectTransform _hourHand;
        private RectTransform _minuteHand;
        private Image _lampGlow;
        private float _dominion;
        private float _elapsed;

        /// <summary>방 물건을 눌렀다.</summary>
        public event Action<HubRoomObject> ObjectClicked;

        public void Build(
            Transform parent)
        {
            _root = UiFactory.CreateRect(parent, "Room");
            UiFactory.Stretch(_root);

            BuildWallAndFloor();
            BuildWindow();
            BuildPoster();
            BuildClock();
            BuildBookshelf();
            BuildDesk();
            BuildBed();

            SetDominion(0f);
        }

        /// <summary>도시 지배도(0~1)만큼 창밖 불빛을 보라색으로 물들인다.</summary>
        public void SetDominion(
            float ratio)
        {
            _dominion = Mathf.Clamp01(ratio);

            int tinted = HubRoomLogic.GetTintedLights(_dominion, _cityLights.Count);

            for (int i = 0; i < _cityLights.Count; i++)
            {
                // 불빛 순서가 도시 왼쪽부터라, 섞어서 물들게 짝 · 홀 순서로 고른다.
                int order = i % 2 == 0 ? i / 2 : _cityLights.Count - 1 - i / 2;
                Color color = order < tinted ? LightOwned : LightWarm;
                color.a = _cityLights[i].color.a;
                _cityLights[i].color = color;
            }
        }

        // 조립 ----------------------------------------------------------

        private Image Box(
            Transform parent,
            string name,
            Color color,
            float x,
            float y,
            float width,
            float height)
        {
            Image image = UiFactory.CreateImage(parent, name, color);
            UiFactory.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(width, height));

            return image;
        }

        private Image Glow(
            Transform parent,
            string name,
            Color color,
            float x,
            float y,
            float width,
            float height)
        {
            Image glow = UiDecor.CreateGlow(parent, name, color, new Vector2(width, height));
            glow.raycastTarget = false;
            UiFactory.Place(glow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(width, height));

            return glow;
        }

        private void BuildWallAndFloor()
        {
            Image wall = UiFactory.CreateImage(_root, "Wall", Wall);
            UiFactory.Stretch(wall.rectTransform);

            for (int i = -8; i <= 8; i++)
            {
                Box(_root, "Stripe", WallStripe, i * 120f, 120f, 3f, 840f);
            }

            // 바닥: 화면 아래 300px
            Image floor = UiFactory.CreateImage(_root, "Floor", Floor);
            floor.rectTransform.anchorMin = new Vector2(0f, 0f);
            floor.rectTransform.anchorMax = new Vector2(1f, 0f);
            floor.rectTransform.pivot = new Vector2(0.5f, 0f);
            floor.rectTransform.sizeDelta = new Vector2(0f, 300f);
            floor.rectTransform.anchoredPosition = Vector2.zero;

            for (int i = 0; i < 5; i++)
            {
                Box(_root, "Plank", Plank, 0f, -250f - i * 60f, 1920f, 2f);
            }

            Box(_root, "Baseboard", WoodDark, 0f, -242f, 1920f, 14f);

            // 창에서 들어오는 달빛 · 러그
            Glow(_root, "MoonFloor", new Color(0.55f, 0.55f, 1f, 0.10f), 40f, -360f, 900f, 220f);
            Box(_root, "RugEdge", new Color(0.42f, 0.24f, 0.46f, 0.75f), 40f, -390f, 860f, 110f);
            Box(_root, "Rug", new Color(0.30f, 0.16f, 0.36f, 0.85f), 40f, -390f, 830f, 86f);
        }

        private void BuildWindow()
        {
            const float x = 40f;
            const float y = 180f;
            const float width = 600f;
            const float height = 380f;

            Box(_root, "WindowFrame", Frame, x, y, width + 30f, height + 30f);
            Box(_root, "SkyTop", new Color(0.04f, 0.04f, 0.12f, 1f), x, y + height * 0.25f, width, height * 0.5f);
            Box(_root, "SkyBottom", new Color(0.12f, 0.07f, 0.20f, 1f), x, y - height * 0.25f, width, height * 0.5f);

            // 별
            System.Random random = new System.Random(36);

            for (int i = 0; i < 18; i++)
            {
                float sx = x - width * 0.5f + 10f + (float)random.NextDouble() * (width - 20f);
                float sy = y + (float)random.NextDouble() * height * 0.45f;
                Box(_root, "Star", new Color(1f, 1f, 1f, 0.35f + (float)random.NextDouble() * 0.4f), sx, sy, 3f, 3f);
            }

            // 달
            Glow(_root, "MoonHalo", new Color(0.85f, 0.85f, 1f, 0.35f), x + 180f, y + 110f, 150f, 150f);
            Glow(_root, "Moon", new Color(1f, 0.98f, 0.88f, 1f), x + 180f, y + 110f, 56f, 56f);

            // 도시 실루엣과 창 불빛
            float left = x - width * 0.5f;
            float bottom = y - height * 0.5f;
            float cursor = left;

            while (cursor < left + width - 10f)
            {
                float buildingWidth = 40f + (float)random.NextDouble() * 50f;
                float buildingHeight = 60f + (float)random.NextDouble() * 150f;

                buildingWidth = Mathf.Min(buildingWidth, left + width - cursor);

                Box(_root, "Building", Building, cursor + buildingWidth * 0.5f, bottom + buildingHeight * 0.5f, buildingWidth, buildingHeight);

                for (float ly = bottom + 12f; ly < bottom + buildingHeight - 10f; ly += 18f)
                {
                    for (float lx = cursor + 8f; lx < cursor + buildingWidth - 8f; lx += 14f)
                    {
                        if (random.NextDouble() < 0.45)
                        {
                            continue;
                        }

                        Image light = Box(_root, "CityLight", LightWarm, lx + 3f, ly + 4f, 6f, 8f);
                        _cityLights.Add(light);
                        _lightPhases.Add((float)random.NextDouble() * 10f);
                    }
                }

                cursor += buildingWidth + 4f;
            }

            // 창틀 십자 · 창턱
            Box(_root, "MullionV", Frame, x, y, 12f, height);
            Box(_root, "MullionH", Frame, x, y, width, 12f);
            Box(_root, "Sill", WoodDark, x, y - height * 0.5f - 22f, width + 80f, 18f);

            // 커튼
            BuildCurtain(x - width * 0.5f - 70f, y + 10f);
            BuildCurtain(x + width * 0.5f + 70f, y + 10f);
            Box(_root, "CurtainRod", WoodDark, x, y + height * 0.5f + 40f, width + 340f, 10f);

            // 창문 전체가 출격 버튼이다.
            Hotspot(HubRoomObject.Window, x, y, width + 30f, height + 30f);
        }

        private void BuildCurtain(
            float x,
            float y)
        {
            Box(_root, "Curtain", Curtain, x, y, 130f, 460f);

            for (int i = -2; i <= 2; i++)
            {
                Box(_root, "Fold", CurtainFold, x + i * 24f, y, 6f, 460f);
            }

            Box(_root, "Tie", new Color(0.85f, 0.70f, 0.35f, 1f), x, y - 60f, 130f, 10f);
        }

        private void BuildPoster()
        {
            const float x = -640f;
            const float y = 230f;

            Box(_root, "PosterBack", new Color(0.92f, 0.86f, 0.95f, 1f), x, y, 190f, 260f);
            Box(_root, "PosterArt", new Color(0.55f, 0.25f, 0.62f, 1f), x, y + 20f, 164f, 170f);
            Glow(_root, "PosterStar", new Color(1f, 0.6f, 0.85f, 0.9f), x + 20f, y + 40f, 110f, 110f);

            Text text = UiFactory.CreateText(_root, "PosterText", "NIGHT\nIDOL", UiTheme.FontSubheading, new Color(0.30f, 0.12f, 0.36f, 1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Place(text.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y - 95f), new Vector2(180f, 50f));
            text.raycastTarget = false;

            Box(_root, "PosterPin", new Color(0.9f, 0.3f, 0.35f, 1f), x, y + 124f, 10f, 10f);
        }

        private void BuildClock()
        {
            const float x = 640f;
            const float y = 300f;

            Glow(_root, "ClockShadow", new Color(0f, 0f, 0f, 0.5f), x + 6f, y - 6f, 150f, 150f);
            Glow(_root, "ClockRim", Frame, x, y, 140f, 140f);
            Glow(_root, "ClockFace", new Color(0.93f, 0.90f, 0.85f, 1f), x, y, 118f, 118f);

            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Box(_root, "Tick", new Color(0.25f, 0.2f, 0.3f, 1f), x + Mathf.Sin(angle) * 44f, y + Mathf.Cos(angle) * 44f, 4f, 4f);
            }

            _hourHand = Hand("HourHand", x, y, 5f, 30f);
            _minuteHand = Hand("MinuteHand", x, y, 3f, 44f);
            Glow(_root, "ClockPin", new Color(0.8f, 0.3f, 0.4f, 1f), x, y, 12f, 12f);

            Hotspot(HubRoomObject.Clock, x, y, 150f, 150f);
        }

        private RectTransform Hand(
            string name,
            float x,
            float y,
            float width,
            float length)
        {
            Image hand = UiFactory.CreateImage(_root, name, new Color(0.18f, 0.14f, 0.22f, 1f));
            UiFactory.Place(hand.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(x, y), new Vector2(width, length));

            return hand.rectTransform;
        }

        private void BuildBookshelf()
        {
            const float x = -770f;
            const float y = -40f;
            const float width = 230f;
            const float height = 440f;

            Box(_root, "ShelfBack", WoodDark, x, y, width, height);
            Box(_root, "ShelfInner", new Color(0.13f, 0.09f, 0.10f, 1f), x, y, width - 24f, height - 24f);

            Color[] spines =
            {
                new Color(0.55f, 0.25f, 0.30f, 1f),
                new Color(0.25f, 0.35f, 0.55f, 1f),
                new Color(0.60f, 0.50f, 0.25f, 1f),
                new Color(0.35f, 0.50f, 0.40f, 1f),
                new Color(0.50f, 0.30f, 0.60f, 1f)
            };

            System.Random random = new System.Random(7);

            for (int shelf = 0; shelf < 4; shelf++)
            {
                float shelfY = y - height * 0.5f + 12f + shelf * 104f;

                Box(_root, "Shelf", Wood, x, shelfY, width - 12f, 10f);

                float bookX = x - width * 0.5f + 20f;

                while (bookX < x + width * 0.5f - 30f)
                {
                    float bookWidth = 14f + (float)random.NextDouble() * 10f;
                    float bookHeight = 60f + (float)random.NextDouble() * 26f;

                    Box(_root, "Book", spines[random.Next(spines.Length)], bookX + bookWidth * 0.5f, shelfY + 5f + bookHeight * 0.5f, bookWidth, bookHeight);

                    bookX += bookWidth + 2f;
                }
            }

            Hotspot(HubRoomObject.Bookshelf, x, y, width, height);
        }

        private void BuildDesk()
        {
            const float x = -300f;
            const float top = -150f;

            // 의자(책상 뒤)
            Box(_root, "ChairBack", new Color(0.22f, 0.16f, 0.28f, 1f), x + 40f, top - 20f, 150f, 150f);

            // 책상
            Box(_root, "DeskTop", Wood, x, top, 560f, 26f);
            Box(_root, "DeskLegL", WoodDark, x - 255f, top - 90f, 20f, 160f);
            Box(_root, "DeskLegR", WoodDark, x + 255f, top - 90f, 20f, 160f);
            Box(_root, "Drawer", WoodDark, x + 170f, top - 50f, 150f, 70f);
            Box(_root, "DrawerKnob", new Color(0.85f, 0.70f, 0.35f, 1f), x + 170f, top - 50f, 16f, 6f);
            Box(_root, "ChairSeat", new Color(0.25f, 0.18f, 0.32f, 1f), x + 40f, top - 120f, 170f, 22f);

            // 스탠드와 불빛
            _lampGlow = Glow(_root, "LampGlow", new Color(LampLight.r, LampLight.g, LampLight.b, 0.28f), x - 170f, top + 70f, 420f, 300f);
            Box(_root, "LampBase", new Color(0.2f, 0.2f, 0.25f, 1f), x - 190f, top + 18f, 70f, 10f);
            Box(_root, "LampArm", new Color(0.25f, 0.25f, 0.3f, 1f), x - 190f, top + 70f, 6f, 100f);
            Box(_root, "LampShade", new Color(0.85f, 0.55f, 0.35f, 1f), x - 170f, top + 125f, 90f, 36f);

            // 연필꽂이 · 머그
            Box(_root, "Cup", new Color(0.35f, 0.40f, 0.55f, 1f), x + 210f, top + 32f, 30f, 40f);
            Box(_root, "Pencil", new Color(0.95f, 0.8f, 0.3f, 1f), x + 204f, top + 60f, 4f, 30f);
            Box(_root, "Pencil", new Color(0.9f, 0.4f, 0.5f, 1f), x + 214f, top + 58f, 4f, 26f);

            // 계약 노트(보라 표지) — 강화
            const float noteX = x - 30f;
            const float noteY = top + 22f;

            Glow(_root, "NoteAura", new Color(0.75f, 0.45f, 1f, 0.35f), noteX, noteY + 6f, 180f, 90f);
            Box(_root, "Notebook", new Color(0.40f, 0.22f, 0.55f, 1f), noteX, noteY, 120f, 18f);
            Box(_root, "NotebookPages", new Color(0.92f, 0.90f, 0.85f, 1f), noteX + 4f, noteY + 2f, 110f, 6f);
            Box(_root, "NotebookMark", new Color(0.95f, 0.75f, 0.35f, 1f), noteX - 40f, noteY - 4f, 6f, 20f);

            Hotspot(HubRoomObject.Notebook, noteX, noteY + 10f, 170f, 70f);
        }

        private void BuildBed()
        {
            const float x = 600f;
            const float y = -270f;

            // 협탁 · 일기장
            const float standX = 260f;

            Box(_root, "Nightstand", WoodDark, standX, y + 10f, 120f, 110f);
            Box(_root, "NightstandTop", Wood, standX, y + 68f, 136f, 12f);
            Box(_root, "StandKnob", new Color(0.85f, 0.70f, 0.35f, 1f), standX, y + 10f, 14f, 6f);
            Glow(_root, "DiaryAura", new Color(1f, 0.55f, 0.75f, 0.35f), standX, y + 90f, 130f, 70f);
            Box(_root, "Diary", new Color(0.62f, 0.25f, 0.38f, 1f), standX, y + 84f, 70f, 16f);
            Box(_root, "DiaryLock", new Color(0.95f, 0.8f, 0.4f, 1f), standX + 30f, y + 84f, 6f, 10f);

            Hotspot(HubRoomObject.Diary, standX, y + 88f, 130f, 70f);

            // 침대
            Box(_root, "Headboard", WoodDark, x + 280f, y + 60f, 40f, 240f);
            Box(_root, "BedFrame", Wood, x, y - 40f, 620f, 60f);
            Box(_root, "Mattress", new Color(0.85f, 0.82f, 0.88f, 1f), x, y + 4f, 600f, 36f);
            Box(_root, "Blanket", new Color(0.42f, 0.26f, 0.58f, 1f), x - 60f, y + 10f, 480f, 48f);
            Box(_root, "BlanketFold", new Color(0.52f, 0.34f, 0.68f, 1f), x + 180f, y + 30f, 20f, 50f);
            Box(_root, "Pillow", new Color(0.95f, 0.92f, 0.96f, 1f), x + 220f, y + 40f, 100f, 36f);
            Glow(_root, "Plush", new Color(0.95f, 0.65f, 0.80f, 1f), x + 120f, y + 50f, 50f, 46f);
        }

        /// <summary>눌러지는 투명한 자리와, 마우스를 올리면 떠오르는 빛 · 이름표다.</summary>
        private void Hotspot(
            HubRoomObject item,
            float x,
            float y,
            float width,
            float height)
        {
            UiButton button = UiFactory.CreateButton(_root, $"Hotspot_{item}", string.Empty);
            UiFactory.Place(button.Background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(x, y), new Vector2(width, height));
            button.Background.color = new Color(1f, 1f, 1f, 0f);
            button.Button.transition = Selectable.Transition.None;
            button.Button.onClick.AddListener(() => ObjectClicked?.Invoke(item));

            GameObject highlight = new GameObject("Highlight", typeof(RectTransform), typeof(CanvasGroup));
            highlight.transform.SetParent(button.Background.transform, false);
            UiFactory.Stretch((RectTransform)highlight.transform);

            CanvasGroup group = highlight.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            Image glow = UiDecor.CreateGlow(highlight.transform, "Glow", new Color(0.85f, 0.65f, 1f, 0.45f), new Vector2(width + 60f, height + 60f));
            glow.rectTransform.anchoredPosition = Vector2.zero;

            Image tag = UiFactory.CreateImage(highlight.transform, "Tag", new Color(0.05f, 0.04f, 0.09f, 0.85f));
            UiFactory.Place(tag.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(230f, 36f));

            Text label = UiFactory.CreateText(tag.transform, "Label", HubRoomLogic.GetObjectLabel(item), UiTheme.FontBody, UiTheme.Gold, TextAnchor.MiddleCenter, FontStyle.Bold);
            UiFactory.Stretch(label.rectTransform);
            label.raycastTarget = false;

            button.Background.gameObject.AddComponent<HubHotspot>().Highlight = group;
        }

        // 움직임 --------------------------------------------------------

        private void Update()
        {
            _elapsed += Time.unscaledDeltaTime;

            // 창밖 불빛이 하나씩 천천히 깜빡인다.
            for (int i = 0; i < _cityLights.Count; i++)
            {
                float wave = Mathf.Sin(_elapsed * 0.7f + _lightPhases[i]);
                Color color = _cityLights[i].color;
                color.a = wave > 0.92f ? 0.25f : 0.9f;
                _cityLights[i].color = color;
            }

            // 스탠드 불빛이 아주 약하게 일렁인다.
            if (_lampGlow != null)
            {
                Color lamp = _lampGlow.color;
                lamp.a = 0.26f + Mathf.Sin(_elapsed * 2.3f) * 0.02f + Mathf.Sin(_elapsed * 7.1f) * 0.01f;
                _lampGlow.color = lamp;
            }

            // 벽시계는 실제 시각을 가리킨다.
            if (_hourHand != null)
            {
                DateTime now = DateTime.Now;
                _hourHand.localRotation = Quaternion.Euler(0f, 0f, HubRoomLogic.GetHourAngle(now.Hour, now.Minute));
                _minuteHand.localRotation = Quaternion.Euler(0f, 0f, HubRoomLogic.GetMinuteAngle(now.Minute));
            }
        }
    }
}
