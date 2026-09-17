using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Presentation;
using ProjectTheta.Save;
using ProjectTheta.Story;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 허브 방 안의 서큐버스다 (36일차).
    ///
    /// 허브에 들어올 때마다 <see cref="HubSuccubusLogic.Spots"/> 중 한 자리에 무작위로 선다(바로 전 자리는 피함).
    /// 누르면 머리 오른쪽 위 말풍선에 대사가 한 글자씩 나오고, 잠시 뒤 사라진다.
    /// 오른쪽 끝 자리에서는 말풍선이 머리 왼쪽 위로 뒤집힌다.
    ///
    /// 그림: Resources/Characters/Succubus/Room_Stand · Room_Sit → Idle → 임시 실루엣 순서로 찾는다.
    /// </summary>
    public sealed class HubSuccubus : MonoBehaviour
    {
        private static int _lastSpot = -1;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _lastSpot = -1;
        }

        private readonly List<UnityEngine.Object> _created = new List<UnityEngine.Object>();
        private readonly System.Random _random = new System.Random(Environment.TickCount);

        private RectTransform _body;
        private RectTransform _bubble;
        private Text _bubbleText;
        private Func<bool> _blocked;
        private Func<SaveData> _save;

        private HubSuccubusSpot _spot;
        private float _height;
        private float _elapsed;
        private string _line = string.Empty;
        private float _lineElapsed;
        private float _bubbleRemaining;

        /// <summary>지금 서 있는 자리 이름이다(확인용).</summary>
        public string SpotName =>
            _spot.Name;

        public void Build(
            Transform parent,
            Func<SaveData> save,
            Func<bool> blocked)
        {
            _save = save;
            _blocked = blocked;

            int index = HubSuccubusLogic.PickSpot(_random, _lastSpot);
            _lastSpot = index;
            _spot = HubSuccubusLogic.Spots[index];
            _height = HubSuccubusLogic.GetHeight(_spot.Pose);

            // 발끝 기준으로 세운다.
            RectTransform root = UiFactory.CreateRect(parent, "Succubus");
            UiFactory.Place(root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(_spot.X, _spot.Y), new Vector2(_height * 0.6f, _height));

            // 발밑 그림자
            Image shadow = UiDecor.CreateGlow(root, "Shadow", new Color(0f, 0f, 0f, 0.45f), new Vector2(_height * 0.55f, 26f));
            UiFactory.Place(shadow.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(_height * 0.55f, 26f));

            _body = UiFactory.CreateRect(root, "Body");
            UiFactory.Place(_body, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(_height * 0.6f, _height));

            BuildArt();

            // 몸 전체가 누르는 자리다.
            UiButton hit = UiFactory.CreateButton(root, "Hit", string.Empty);
            UiFactory.Stretch(hit.Background.rectTransform);
            hit.Background.color = new Color(1f, 1f, 1f, 0f);
            hit.Button.transition = Selectable.Transition.None;
            hit.Button.onClick.AddListener(Talk);

            BuildBubble(root);
        }

        private void BuildArt()
        {
            Sprite art =
                LoadSprite(HubSuccubusLogic.GetArtPath(_spot.Pose)) ??
                LoadSprite(HubSuccubusLogic.FallbackArtPath);

            if (art != null)
            {
                Image image = UiFactory.CreateImage(_body, "Art", Color.white);
                image.sprite = art;
                image.preserveAspect = true;
                UiFactory.Stretch(image.rectTransform);

                return;
            }

            BuildSilhouette();
        }

        /// <summary>그림이 없을 때 쓰는 임시 실루엣이다. 로딩 화면 실루엣을 방 크기로 키운 모양이다.</summary>
        private void BuildSilhouette()
        {
            RectTransform figure = UiFactory.CreateRect(_body, "Silhouette");
            UiFactory.Place(figure, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(100f, 100f));

            float scale = _height / 100f;
            figure.localScale = new Vector3(scale, scale, 1f);

            Color body = new Color(0.62f, 0.28f, 0.86f);
            Color wing = new Color(0.36f, 0.16f, 0.52f, 0.9f);
            Color glow = new Color(1.00f, 0.45f, 0.85f);
            bool sit = _spot.Pose == HubSuccubusPose.Sit;

            Piece(figure, "Aura", new Vector2(0f, 50f), new Vector2(60f, 90f), new Color(glow.r, glow.g, glow.b, 0.14f), 0f);
            Piece(figure, "WingLeft", new Vector2(-26f, 62f), new Vector2(34f, 22f), wing, 25f);
            Piece(figure, "WingRight", new Vector2(26f, 62f), new Vector2(34f, 22f), wing, -25f);
            Piece(figure, "Body", new Vector2(0f, sit ? 38f : 44f), new Vector2(22f, sit ? 32f : 44f), body, 0f);

            if (sit)
            {
                Piece(figure, "Legs", new Vector2(8f, 18f), new Vector2(28f, 10f), body, 0f);
            }
            else
            {
                Piece(figure, "LegLeft", new Vector2(-5f, 12f), new Vector2(7f, 24f), body, 0f);
                Piece(figure, "LegRight", new Vector2(5f, 12f), new Vector2(7f, 24f), body, 0f);
            }

            Piece(figure, "Head", new Vector2(0f, 76f), new Vector2(20f, 20f), body, 0f);
            Piece(figure, "HornLeft", new Vector2(-8f, 90f), new Vector2(5f, 12f), glow, 20f);
            Piece(figure, "HornRight", new Vector2(8f, 90f), new Vector2(5f, 12f), glow, -20f);
            Piece(figure, "Tail", new Vector2(16f, 26f), new Vector2(4f, 24f), body, 40f);
            Piece(figure, "TailTip", new Vector2(24f, 16f), new Vector2(9f, 9f), glow, 45f);
        }

        private static void Piece(
            RectTransform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            float rotation)
        {
            Image image = UiFactory.CreateImage(parent, name, color);
            UiFactory.Place(image.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), position, size);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        /// <summary>머리 오른쪽 위(또는 왼쪽 위)에 붙는 말풍선이다.</summary>
        private void BuildBubble(
            RectTransform root)
        {
            float headX = _height * 0.12f;
            bool flip = HubSuccubusLogic.ShouldFlip(_spot.X, headX, HubSuccubusLogic.BubbleWidth);

            _bubble = UiFactory.CreateRect(root, "Bubble");
            UiFactory.Place(
                _bubble,
                new Vector2(0.5f, 0f),
                new Vector2(flip ? 1f : 0f, 0f),
                new Vector2(flip ? -headX : headX, _height * 0.92f + 18f),
                new Vector2(HubSuccubusLogic.BubbleWidth, HubSuccubusLogic.BubbleMinHeight));

            Color fill = new Color(0.98f, 0.95f, 1f, 0.96f);
            Color edge = new Color(0.62f, 0.38f, 0.86f, 1f);

            // 꼬리: 말풍선 아래 모서리에서 머리 쪽으로 내려오는 작은 마름모
            Image tailEdge = UiFactory.CreateImage(_bubble, "TailEdge", edge);
            UiFactory.Place(tailEdge.rectTransform, new Vector2(flip ? 1f : 0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(flip ? -28f : 28f, -2f), new Vector2(24f, 24f));
            tailEdge.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            RectTransform box = UiFactory.CreatePanel(_bubble, "Box", fill, edge, 3f);
            UiFactory.Stretch(box);

            Image tail = UiFactory.CreateImage(_bubble, "Tail", fill);
            UiFactory.Place(tail.rectTransform, new Vector2(flip ? 1f : 0f, 0f), new Vector2(0.5f, 0.5f), new Vector2(flip ? -28f : 28f, 1f), new Vector2(19f, 19f));
            tail.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);

            _bubbleText = UiFactory.CreateText(_bubble, "Text", string.Empty, UiTheme.FontBody + 1, new Color(0.18f, 0.10f, 0.26f, 1f), TextAnchor.MiddleLeft, FontStyle.Bold);
            UiFactory.Stretch(_bubbleText.rectTransform, HubSuccubusLogic.BubblePadding);
            _bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            _bubbleText.lineSpacing = 1.15f;
            _bubbleText.raycastTarget = false;

            _bubble.gameObject.SetActive(false);
        }

        private void Talk()
        {
            if (_blocked != null &&
                _blocked())
            {
                return;
            }

            _line =
                HubSuccubusLogic.PickLine(
                    HubSuccubusLogic.GetLines(_save?.Invoke()),
                    _random,
                    _line);

            // 다 나온 글 기준으로 말풍선 높이를 먼저 맞춘다.
            _bubbleText.text = _line;
            float height =
                Mathf.Max(
                    HubSuccubusLogic.BubbleMinHeight,
                    _bubbleText.preferredHeight + HubSuccubusLogic.BubblePadding * 2f);

            _bubble.sizeDelta = new Vector2(HubSuccubusLogic.BubbleWidth, height);
            _bubbleText.text = string.Empty;

            _lineElapsed = 0f;
            _bubbleRemaining = HubSuccubusLogic.GetBubbleSeconds(_line) + StoryLogic.GetLineSeconds(_line.Length);
            _bubble.gameObject.SetActive(true);

            GameAudio.Play(GameSfx.UiTick, 0.7f);
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;
            _elapsed += delta;

            // 숨 쉬듯 살짝 오르내린다.
            if (_body != null)
            {
                _body.anchoredPosition = new Vector2(0f, Mathf.Sin(_elapsed * 1.6f) * 4f);
            }

            if (_bubble == null ||
                !_bubble.gameObject.activeSelf)
            {
                return;
            }

            _lineElapsed += delta;
            _bubbleText.text = _line.Substring(0, StoryLogic.GetVisibleCharacters(_lineElapsed, _line.Length));

            // 시간이 지나면 말풍선을 닫는다.
            _bubbleRemaining -= delta;

            if (_bubbleRemaining <= 0f)
            {
                _bubble.gameObject.SetActive(false);
            }
        }

        private Sprite LoadSprite(
            string path)
        {
            Texture2D texture = Resources.Load<Texture2D>(path);

            if (texture == null)
            {
                return null;
            }

            Sprite sprite =
                Sprite.Create(
                    texture,
                    new Rect(0f, 0f, texture.width, texture.height),
                    new Vector2(0.5f, 0f),
                    100f);

            sprite.name = path.Replace('/', '_') + "_Hub";
            _created.Add(sprite);

            return sprite;
        }

        private void OnDestroy()
        {
            foreach (UnityEngine.Object created in _created)
            {
                if (created != null)
                {
                    Destroy(created);
                }
            }

            _created.Clear();
        }
    }
}
