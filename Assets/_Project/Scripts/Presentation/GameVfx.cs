using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 화면 연출 창구다 (19일차).
    ///
    /// 14일차 <see cref="GameAudio"/>와 짝이다. 호출하는 쪽은 "무엇을, 어디서"만 말한다.
    ///
    /// <code>
    /// GameVfx.Ripple(position, UiTheme.Accent, 1.3f);
    /// GameVfx.FlyText("+54", position, UiTheme.Gold);
    /// GameVfx.Shake(0.14f, 0.22f);
    /// </code>
    ///
    /// 설계 원칙
    ///   - 재사용: 이펙트 오브젝트를 미리 만들어 두고 꺼냈다 돌려놓는다. 18일차에 걷어낸 매 프레임 할당을 되살리지 않는다
    ///   - 보이는 층만: 안 보이는 층에서 일어난 일은 연출하지 않는다 (18일차 <see cref="FloorVisibility"/>)
    ///   - 씬 단위: 담당 오브젝트는 씬마다 새로 생기고 씬과 함께 사라진다
    /// </summary>
    public static class GameVfx
    {
        private static VfxRunner _runner;

        /// <summary>날아가는 숫자가 도착할 HUD 정기 게이지다. HUD가 등록한다.</summary>
        public static RectTransform EssenceTarget { get; set; }

        private static VfxRunner Runner
        {
            get
            {
                if (_runner != null)
                {
                    return _runner;
                }

                GameObject host =
                    new GameObject(
                        "GameVfx");

                _runner =
                    host.AddComponent<VfxRunner>();

                return _runner;
            }
        }

        internal static void NotifyRunnerDestroyed(
            VfxRunner runner)
        {
            if (_runner == runner)
            {
                _runner = null;
            }
        }

        private static bool IsVisible(
            Vector2 position)
        {
            return FloorSpace.FloorAt(
                       position.y) ==
                   FloorVisibility.ViewFloor;
        }

        // 월드 이펙트 ----------------------------------------------------

        /// <summary>퍼지면서 옅어지는 원.</summary>
        public static void Ripple(
            Vector2 position,
            Color color,
            float radius,
            float duration = 0.45f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnWorld(
                VfxRunner.WorldKind.Ripple,
                VfxSprite.Ring,
                position,
                Vector2.zero,
                color,
                radius * 2f,
                0f,
                duration);
        }

        /// <summary>톡 커졌다가 옅어지는 광원.</summary>
        public static void Glow(
            Vector2 position,
            Color color,
            float radius,
            float duration = 0.4f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnWorld(
                VfxRunner.WorldKind.Pop,
                VfxSprite.SoftCircle,
                position,
                Vector2.zero,
                color,
                radius * 2f,
                0f,
                duration);
        }

        /// <summary>바닥에서 솟았다가 가늘어지며 사라지는 빛기둥.</summary>
        public static void Pillar(
            Vector2 position,
            Color color,
            float width,
            float height,
            float duration = 0.7f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnWorld(
                VfxRunner.WorldKind.Pillar,
                VfxSprite.Pillar,
                position,
                Vector2.zero,
                color,
                width,
                height,
                duration);
        }

        /// <summary>톡 튀어나오며 위로 떠오르는 아이콘 (하트 등).</summary>
        public static void Pop(
            VfxSprite sprite,
            Vector2 position,
            Color color,
            float size,
            float rise = 0.6f,
            float duration = 0.7f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnWorld(
                VfxRunner.WorldKind.Pop,
                sprite,
                position,
                new Vector2(
                    0f,
                    rise),
                color,
                size,
                0f,
                duration);
        }

        /// <summary>사방으로 튀는 파편.</summary>
        public static void Shards(
            Vector2 position,
            Color color,
            int count,
            float speed = 3.2f,
            float duration = 0.45f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            VfxRunner runner =
                Runner;

            int safeCount =
                Mathf.Clamp(
                    count,
                    1,
                    24);

            for (int i = 0;
                 i < safeCount;
                 i++)
            {
                // 난수 대신 고르게 나눈 각도에 조금씩 비틀어 준다. 할당 없이 매번 조금 달라 보인다.
                float angle =
                    (i / (float)safeCount) *
                    Mathf.PI *
                    2f +
                    (Time.frameCount % 7) *
                    0.21f;

                float spread =
                    0.75f +
                    ((i * 37) % 10) *
                    0.05f;

                Vector2 velocity =
                    new Vector2(
                        Mathf.Cos(angle),
                        Mathf.Sin(angle) * 0.7f + 0.35f) *
                    speed *
                    spread;

                runner.SpawnWorld(
                    VfxRunner.WorldKind.Shard,
                    VfxSprite.Shard,
                    position,
                    velocity,
                    color,
                    0.14f,
                    0f,
                    duration);
            }
        }

        // 글자 ---------------------------------------------------------

        /// <summary>월드 위치에서 위로 떠오르며 사라지는 글자.</summary>
        public static void FloatText(
            string text,
            Vector2 position,
            Color color,
            int fontSize = UiTheme.FontSubheading,
            float duration = 0.9f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnText(
                VfxRunner.TextKind.Float,
                text,
                position,
                color,
                fontSize,
                duration);
        }

        /// <summary>월드 위치에서 HUD 정기 게이지로 휘어 날아가는 글자. 목표가 없으면 떠오르기로 대신한다.</summary>
        public static void FlyText(
            string text,
            Vector2 position,
            Color color,
            int fontSize = UiTheme.FontHeading,
            float duration = 0.8f)
        {
            if (!IsVisible(position))
            {
                return;
            }

            Runner.SpawnText(
                EssenceTarget != null
                    ? VfxRunner.TextKind.Fly
                    : VfxRunner.TextKind.Float,
                text,
                position,
                color,
                fontSize,
                duration);
        }

        // 화면 --------------------------------------------------------

        public static void Shake(
            float strength,
            float duration)
        {
            // 흔들림은 담당 오브젝트가 매 프레임 흘려야 끝나므로 먼저 만들어 둔다.
            _ = Runner;

            CameraShake.Add(
                strength,
                duration);
        }

        public static void HitStop(
            float seconds,
            float scale)
        {
            // 멈칫도 담당 오브젝트가 실제 시간으로 흘려야 풀린다.
            _ = Runner;

            GameplayPause.HitStop(
                seconds,
                scale);
        }

        /// <summary>화면을 잠깐 어둡게 했다가 밝힌다. 층 이동처럼 장면이 뚝 끊기는 곳을 부드럽게 한다.</summary>
        public static void Fade(
            float seconds,
            float strength = 0.6f)
        {
            Runner.StartFade(
                seconds,
                strength);
        }

        /// <summary>폭주 위험 테두리 세기다. 0이면 사라진다. 매 프레임 불러도 된다.</summary>
        public static void SetDanger(
            float intensity)
        {
            if (_runner == null &&
                intensity <= 0f)
            {
                return;
            }

            Runner.DangerIntensity =
                VfxCurves.Clamp01(
                    intensity);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _runner = null;
            EssenceTarget = null;
        }
    }

    /// <summary>
    /// <see cref="GameVfx"/>의 실제 일꾼이다. 씬마다 하나 생긴다.
    /// 이펙트 오브젝트 재사용, 매 프레임 갱신, 멈칫·흔들림 시간 흘리기를 맡는다.
    /// </summary>
    public sealed class VfxRunner : MonoBehaviour
    {
        internal enum WorldKind
        {
            Ripple,
            Pop,
            Pillar,
            Shard
        }

        internal enum TextKind
        {
            Float,
            Fly
        }

        private sealed class WorldFx
        {
            public GameObject GameObject;
            public Transform Transform;
            public SpriteRenderer Renderer;
            public WorldKind Kind;
            public Vector2 Origin;
            public Vector2 Velocity;
            public Color Color;
            public float Size;
            public float Size2;
            public float Elapsed;
            public float Duration;
        }

        private sealed class TextFx
        {
            public RectTransform Rect;
            public Text Text;
            public TextKind Kind;
            public Vector2 WorldOrigin;
            public Vector2 CanvasStart;
            public Color Color;
            public float Elapsed;
            public float Duration;
        }

        /// <summary>한 번에 떠 있을 수 있는 최대 개수다. 넘치면 가장 오래된 것을 재사용한다.</summary>
        private const int MaximumWorld = 96;
        private const int MaximumText = 24;

        /// <summary>파편이 떨어지는 세기다.</summary>
        private const float ShardGravity = 7.5f;

        private readonly List<WorldFx> _activeWorld = new List<WorldFx>();
        private readonly Stack<WorldFx> _freeWorld = new Stack<WorldFx>();
        private readonly List<TextFx> _activeText = new List<TextFx>();
        private readonly Stack<TextFx> _freeText = new Stack<TextFx>();

        private Canvas _backCanvas;
        private Canvas _textCanvas;
        private RectTransform _textRoot;
        private Image _dangerImage;
        private Image _fadeImage;

        private float _fadeElapsed;
        private float _fadeDuration;
        private float _fadeStrength;

        private int _sortingOrder;

        public float DangerIntensity { get; set; }

        private void Awake()
        {
            // 캐릭터·머리 위 게이지보다 위, 화면 UI 계층보다는 아래다.
            _sortingOrder =
                CharacterSortingLogic.GetLayerBase(
                    CharacterLayer.CharacterOverlay) +
                CharacterSortingLogic.DepthRange +
                100;

            BuildCanvases();
        }

        private void OnDestroy()
        {
            // 씬을 떠나며 사라질 때 멈칫·흔들림이 남지 않게 한다.
            GameplayPause.CancelHitStop();
            CameraShake.Stop();

            GameVfx.NotifyRunnerDestroyed(
                this);
        }

        private void Update()
        {
            float unscaled =
                Time.unscaledDeltaTime;

            GameplayPause.TickHitStop(
                unscaled);

            CameraShake.Tick(
                unscaled);

            // 월드 이펙트는 게임 시간으로 흐른다. 멈칫 중엔 같이 느려지고, 카드 화면에선 멈춘다.
            UpdateWorld(
                Time.deltaTime);

            // 글자와 화면 연출은 실제 시간으로 흐른다. 카드 화면 뒤에서도 자연스럽게 사라진다.
            UpdateText(
                unscaled);

            UpdateScreen(
                unscaled);
        }

        // 월드 --------------------------------------------------------

        internal void SpawnWorld(
            WorldKind kind,
            VfxSprite sprite,
            Vector2 position,
            Vector2 velocity,
            Color color,
            float size,
            float size2,
            float duration)
        {
            WorldFx fx =
                AcquireWorld();

            fx.Kind = kind;
            fx.Origin = position;
            fx.Velocity = velocity;
            fx.Color = color;
            fx.Size = size;
            fx.Size2 = size2;
            fx.Elapsed = 0f;
            fx.Duration = Mathf.Max(0.01f, duration);

            fx.Renderer.sprite =
                VfxLibrary.Get(
                    sprite);

            fx.Transform.position =
                new Vector3(
                    position.x,
                    position.y,
                    0f);

            fx.GameObject.SetActive(true);

            ApplyWorld(fx);

            _activeWorld.Add(fx);
        }

        private WorldFx AcquireWorld()
        {
            if (_freeWorld.Count > 0)
            {
                return _freeWorld.Pop();
            }

            // 가득 찼으면 가장 오래된 것을 가져다 쓴다. 새로 만들지 않는다.
            if (_activeWorld.Count >= MaximumWorld)
            {
                WorldFx oldest =
                    _activeWorld[0];

                _activeWorld.RemoveAt(0);

                return oldest;
            }

            GameObject go =
                new GameObject(
                    "Vfx");

            go.transform.SetParent(
                transform,
                false);

            SpriteRenderer renderer =
                go.AddComponent<SpriteRenderer>();

            renderer.sortingOrder =
                _sortingOrder;

            return new WorldFx
            {
                GameObject = go,
                Transform = go.transform,
                Renderer = renderer
            };
        }

        private void UpdateWorld(
            float deltaTime)
        {
            for (int i = _activeWorld.Count - 1;
                 i >= 0;
                 i--)
            {
                WorldFx fx =
                    _activeWorld[i];

                fx.Elapsed += deltaTime;

                if (fx.Elapsed >= fx.Duration)
                {
                    fx.GameObject.SetActive(false);

                    _activeWorld.RemoveAt(i);

                    _freeWorld.Push(fx);

                    continue;
                }

                ApplyWorld(fx);
            }
        }

        private static void ApplyWorld(
            WorldFx fx)
        {
            float t =
                fx.Elapsed /
                fx.Duration;

            float alpha;
            Vector3 scale;
            Vector2 position = fx.Origin;

            switch (fx.Kind)
            {
                case WorldKind.Ripple:
                {
                    float s =
                        fx.Size *
                        VfxCurves.RippleScale(t);

                    scale = new Vector3(s, s * 0.55f, 1f);
                    alpha = VfxCurves.RippleAlpha(t);

                    break;
                }

                case WorldKind.Pillar:
                {
                    float height =
                        fx.Size2 *
                        VfxCurves.PillarHeight(t);

                    scale =
                        new Vector3(
                            fx.Size * VfxCurves.PillarWidth(t),
                            height,
                            1f);

                    // 스프라이트 중심이 가운데라, 바닥에서 솟게 하려면 높이 절반만큼 올린다.
                    position.y += height * 0.5f;

                    alpha = VfxCurves.PillarAlpha(t);

                    break;
                }

                case WorldKind.Shard:
                {
                    float seconds =
                        fx.Elapsed;

                    position +=
                        fx.Velocity *
                        seconds;

                    position.y -=
                        0.5f *
                        ShardGravity *
                        seconds *
                        seconds;

                    float s =
                        fx.Size *
                        (1f - t * 0.6f);

                    scale = new Vector3(s, s, 1f);
                    alpha = VfxCurves.FadeOutTail(t, 0.4f);

                    break;
                }

                case WorldKind.Pop:
                default:
                {
                    float s =
                        fx.Size *
                        VfxCurves.PopScale(t);

                    scale = new Vector3(s, s, 1f);

                    position.y +=
                        VfxCurves.FloatRise(
                            t,
                            fx.Velocity.y);

                    alpha = VfxCurves.PopAlpha(t);

                    break;
                }
            }

            fx.Transform.position =
                new Vector3(
                    position.x,
                    position.y,
                    0f);

            fx.Transform.localScale = scale;

            Color color =
                fx.Color;

            color.a *= alpha;

            fx.Renderer.color = color;
        }

        // 글자 --------------------------------------------------------

        internal void SpawnText(
            TextKind kind,
            string content,
            Vector2 worldPosition,
            Color color,
            int fontSize,
            float duration)
        {
            Camera camera =
                Camera.main;

            if (camera == null)
            {
                return;
            }

            TextFx fx =
                AcquireText();

            fx.Kind = kind;
            fx.WorldOrigin = worldPosition;
            fx.Color = color;
            fx.Elapsed = 0f;
            fx.Duration = Mathf.Max(0.05f, duration);

            fx.CanvasStart =
                WorldToCanvas(
                    camera,
                    worldPosition);

            fx.Text.text = content;
            fx.Text.fontSize = fontSize;

            fx.Rect.gameObject.SetActive(true);

            ApplyText(
                fx,
                camera);

            _activeText.Add(fx);
        }

        private TextFx AcquireText()
        {
            if (_freeText.Count > 0)
            {
                return _freeText.Pop();
            }

            if (_activeText.Count >= MaximumText)
            {
                TextFx oldest =
                    _activeText[0];

                _activeText.RemoveAt(0);

                return oldest;
            }

            Text text =
                UiFactory.CreateText(
                    _textRoot,
                    "VfxText",
                    string.Empty,
                    UiTheme.FontSubheading,
                    Color.white,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            // 밝은 배경 위에서도 읽히도록 어두운 테두리를 준다.
            Outline outline =
                text.gameObject.AddComponent<Outline>();

            outline.effectColor =
                new Color(0f, 0f, 0f, 0.75f);

            outline.effectDistance =
                new Vector2(2f, -2f);

            RectTransform rect =
                text.rectTransform;

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(360f, 60f);

            return new TextFx
            {
                Rect = rect,
                Text = text
            };
        }

        private void UpdateText(
            float unscaledDeltaTime)
        {
            if (_activeText.Count == 0)
            {
                return;
            }

            Camera camera =
                Camera.main;

            for (int i = _activeText.Count - 1;
                 i >= 0;
                 i--)
            {
                TextFx fx =
                    _activeText[i];

                fx.Elapsed += unscaledDeltaTime;

                if (fx.Elapsed >= fx.Duration ||
                    camera == null)
                {
                    fx.Rect.gameObject.SetActive(false);

                    _activeText.RemoveAt(i);

                    _freeText.Push(fx);

                    continue;
                }

                ApplyText(
                    fx,
                    camera);
            }
        }

        private void ApplyText(
            TextFx fx,
            Camera camera)
        {
            float t =
                fx.Elapsed /
                fx.Duration;

            Vector2 position;
            float alpha;
            float scale = 1f;

            if (fx.Kind == TextKind.Fly &&
                GameVfx.EssenceTarget != null)
            {
                Vector2 end =
                    ScreenToCanvas(
                        RectTransformUtility.WorldToScreenPoint(
                            null,
                            GameVfx.EssenceTarget.position));

                // 출발점에서 위로 크게 휘었다가 게이지로 빨려 들어간다.
                Vector2 control =
                    new Vector2(
                        Mathf.Lerp(
                            fx.CanvasStart.x,
                            end.x,
                            0.2f),
                        Mathf.Max(
                            fx.CanvasStart.y,
                            end.y) +
                        160f);

                VfxCurves.Bezier(
                    fx.CanvasStart.x,
                    fx.CanvasStart.y,
                    control.x,
                    control.y,
                    end.x,
                    end.y,
                    t,
                    out float x,
                    out float y);

                position = new Vector2(x, y);
                alpha = VfxCurves.FlightAlpha(t);

                // 처음 잠깐 커졌다가 날아가며 작아진다.
                scale =
                    VfxCurves.PopScale(
                        Mathf.Min(
                            1f,
                            t * 3f)) *
                    (1f - t * 0.45f);
            }
            else
            {
                // 월드 기준으로 떠오른다. 카메라가 움직여도 제자리에서 뜬다.
                Vector2 world =
                    fx.WorldOrigin +
                    new Vector2(
                        0f,
                        0.9f +
                        VfxCurves.FloatRise(
                            t,
                            0.8f));

                position =
                    WorldToCanvas(
                        camera,
                        world);

                alpha = VfxCurves.FloatAlpha(t);

                scale =
                    VfxCurves.PopScale(
                        Mathf.Min(
                            1f,
                            t * 2.5f));
            }

            fx.Rect.anchoredPosition = position;

            fx.Rect.localScale =
                new Vector3(
                    scale,
                    scale,
                    1f);

            Color color =
                fx.Color;

            color.a *= alpha;

            fx.Text.color = color;
        }

        private Vector2 WorldToCanvas(
            Camera camera,
            Vector2 world)
        {
            return ScreenToCanvas(
                camera.WorldToScreenPoint(
                    new Vector3(
                        world.x,
                        world.y,
                        0f)));
        }

        private Vector2 ScreenToCanvas(
            Vector2 screen)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _textRoot,
                screen,
                null,
                out Vector2 local);

            return local;
        }

        // 화면 --------------------------------------------------------

        internal void StartFade(
            float seconds,
            float strength)
        {
            _fadeDuration = Mathf.Max(0.01f, seconds);
            _fadeElapsed = 0f;
            _fadeStrength = VfxCurves.Clamp01(strength);
        }

        private void UpdateScreen(
            float unscaledDeltaTime)
        {
            if (_fadeImage != null)
            {
                float alpha = 0f;

                if (_fadeElapsed < _fadeDuration)
                {
                    _fadeElapsed += unscaledDeltaTime;

                    alpha =
                        _fadeStrength *
                        VfxCurves.RippleAlpha(
                            _fadeElapsed /
                            _fadeDuration);
                }

                SetImageAlpha(
                    _fadeImage,
                    alpha);
            }

            if (_dangerImage != null)
            {
                SetImageAlpha(
                    _dangerImage,
                    VfxCurves.DangerPulse(
                        Time.unscaledTime,
                        DangerIntensity));
            }
        }

        private static void SetImageAlpha(
            Image image,
            float alpha)
        {
            bool visible =
                alpha > 0.001f;

            if (image.enabled != visible)
            {
                image.enabled = visible;
            }

            if (!visible)
            {
                return;
            }

            Color color =
                image.color;

            color.a = alpha;

            image.color = color;
        }

        private void BuildCanvases()
        {
            // 뒤쪽: 화면 테두리 경고와 페이드. HUD(50)보다 아래라 HUD 글자는 가리지 않는다.
            _backCanvas =
                UiFactory.CreateCanvas(
                    "VfxBackCanvas",
                    40,
                    transform);

            _dangerImage =
                UiFactory.CreateImage(
                    _backCanvas.transform,
                    "DangerEdge",
                    UiTheme.Danger);

            _dangerImage.sprite =
                VfxLibrary.Get(
                    VfxSprite.EdgeVignette);

            UiFactory.Stretch(
                _dangerImage.rectTransform);

            _dangerImage.enabled = false;

            _fadeImage =
                UiFactory.CreateImage(
                    _backCanvas.transform,
                    "Fade",
                    Color.black);

            UiFactory.Stretch(
                _fadeImage.rectTransform);

            _fadeImage.enabled = false;

            // 앞쪽: 떠오르는·날아가는 글자. HUD(50) 위라 게이지에 닿는 순간이 보인다.
            _textCanvas =
                UiFactory.CreateCanvas(
                    "VfxTextCanvas",
                    55,
                    transform);

            _textRoot =
                _textCanvas.GetComponent<RectTransform>();
        }
    }
}
