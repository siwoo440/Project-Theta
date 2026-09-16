using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 화면을 넘기는 동안 보이는 로딩창이다 (28일차).
    ///
    ///   ┌──────────────────────────────────────┐
    ///   │                                      │
    ///   │           (서큐버스)                  │
    ///   │              (주인공 →)               │  바가 차오르는 끝 위를 달린다
    ///   │        [■■■■■■□□□□□□]  62%            │
    ///   │                                      │
    ///   │                        TIP  도움말…  │
    ///   └──────────────────────────────────────┘
    ///
    /// 씬을 넘어가도 살아 있다가, 새 화면이 다 지어지면 흐려지며 사라진다.
    /// 게임이 멈춘 상태(timeScale 0)에서 불러도 움직이게 모두 unscaled 시간을 쓴다.
    /// </summary>
    public sealed class LoadingScreen : MonoBehaviour
    {
        private const int SortOrder = 30000;
        private const string PlayerRoot = "Characters/Player";
        private const string SuccubusRoot = "Characters/Succubus";
        private const int RunFrameCount = 4;

        private static LoadingScreen _current;

        private readonly Sprite[] _runSprites = new Sprite[RunFrameCount];
        private readonly System.Collections.Generic.List<Object> _created =
            new System.Collections.Generic.List<Object>();

        private CanvasGroup _group;
        private RectTransform _runner;
        private Image _runnerImage;
        private RectTransform _companion;
        private UiBar _bar;
        private Text _percent;
        private Text _tip;

        private float _elapsed;
        private float _displayed;
        private float _companionX;
        private float _companionY;
        private float _tipTimer;
        private int _tipIndex = -1;
        private bool _pauseHeld;

        /// <summary>로딩창이 떠 있는 동안에는 다른 화면 전환을 받지 않는다.</summary>
        public static bool IsLoading =>
            _current != null;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }

        /// <summary>로딩창을 띄우고 그 뒤에서 씬을 불러온다.</summary>
        public static bool Load(
            string sceneName)
        {
            if (_current != null)
            {
                return false;
            }

            GameObject root =
                new GameObject(
                    "LoadingScreen");

            DontDestroyOnLoad(
                root);

            _current =
                root.AddComponent<LoadingScreen>();

            _current.Build();

            // 비동기 로딩 동안 이전 화면이 뒤에서 계속 돌지 않게 시간을 멈춘다.
            Core.GameplayPause.Request();
            _current._pauseHeld = true;
            _current.StartCoroutine(
                _current.Run(
                    sceneName));

            return true;
        }

        private IEnumerator Run(
            string sceneName)
        {
            // 로딩창이 한 번 그려진 뒤에 불러오기 시작해야 멈칫하는 화면이 안 보인다.
            yield return null;

            AsyncOperation operation =
                SceneManager.LoadSceneAsync(
                    sceneName);

            if (operation == null)
            {
                Debug.LogError(
                    $"[LoadingScreen] 씬을 불러올 수 없습니다: {sceneName}");

                Close();
                yield break;
            }

            operation.allowSceneActivation = false;

            while (true)
            {
                float progress =
                    LoadingScreenLogic.NormalizeProgress(
                        operation.progress,
                        operation.isDone);

                // 0.9에서 멈춘 상태가 "다 불러옴"이다. 바가 100%를 보여 줄 때까지 기다린다.
                _displayed =
                    LoadingScreenLogic.StepDisplayed(
                        _displayed,
                        progress,
                        Time.unscaledDeltaTime);

                if (LoadingScreenLogic.CanActivate(progress, _elapsed) &&
                    LoadingScreenLogic.IsBarFull(_displayed))
                {
                    break;
                }

                yield return null;
            }

            // 오른쪽 끝에 도착한 모습을 잠깐 보여 준다.
            float hold = 0f;

            while (hold < LoadingScreenLogic.ArrivalHoldSeconds)
            {
                hold += Time.unscaledDeltaTime;
                yield return null;
            }

            operation.allowSceneActivation = true;

            while (!operation.isDone)
            {
                yield return null;
            }

            // 이전 화면은 사라졌다. 새 화면은 멈추지 않은 채로 시작해야 한다.
            ReleasePause();

            // 새 씬의 부트스트랩이 sceneLoaded에서 화면을 짓는다. 한 프레임 더 기다려 다 지어진 뒤 걷는다.
            yield return null;

            float fade = 0f;

            while (fade < LoadingScreenLogic.FadeSeconds)
            {
                fade += Time.unscaledDeltaTime;
                _group.alpha = 1f - Mathf.Clamp01(fade / LoadingScreenLogic.FadeSeconds);

                yield return null;
            }

            Close();
        }

        private void Update()
        {
            float delta = Time.unscaledDeltaTime;

            _elapsed += delta;

            AnimateRunner(delta);
            AnimateBar();
            AnimateTip(delta);
        }

        private void AnimateRunner(
            float delta)
        {
            float runnerX = LoadingScreenLogic.GetRunnerX(_displayed);
            float runnerY = _runner.anchoredPosition.y;

            _runner.anchoredPosition = new Vector2(runnerX, runnerY);

            // 도착하면 첫 그림으로 멈춰 선다.
            Sprite frame =
                LoadingScreenLogic.HasArrived(_displayed)
                    ? _runSprites[0]
                    : _runSprites[LoadingScreenLogic.GetRunFrame(_elapsed, RunFrameCount)];

            if (frame != null)
            {
                _runnerImage.sprite = frame;
            }

            float targetX = LoadingScreenLogic.GetCompanionTargetX(runnerX);
            float targetY = LoadingScreenLogic.GetCompanionTargetY(runnerY, _elapsed);

            _companionX = LoadingScreenLogic.Follow(_companionX, targetX, delta);
            _companionY = LoadingScreenLogic.Follow(_companionY, targetY, delta);

            _companion.anchoredPosition = new Vector2(_companionX, _companionY);
        }

        private void AnimateBar()
        {
            _bar.SetValue(_displayed);
            _percent.text = LoadingScreenLogic.FormatPercent(_displayed);
        }

        private void AnimateTip(
            float delta)
        {
            _tipTimer -= delta;

            if (_tipTimer > 0f)
            {
                return;
            }

            _tipTimer = LoadingScreenLogic.TipSeconds;
            _tipIndex =
                LoadingScreenLogic.NextTipIndex(
                    _tipIndex,
                    LoadingTips.All.Length,
                    Random.Range(0, 1000));

            _tip.text = LoadingTips.All[_tipIndex];
        }

        private void ReleasePause()
        {
            if (!_pauseHeld)
            {
                return;
            }

            _pauseHeld = false;
            Core.GameplayPause.Release();
        }

        private void Close()
        {
            if (_current == this)
            {
                _current = null;
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            ReleasePause();

            if (_current == this)
            {
                _current = null;
            }

            for (int i = 0; i < _created.Count; i++)
            {
                if (_created[i] != null)
                {
                    Destroy(_created[i]);
                }
            }

            _created.Clear();
        }

        // 화면 짓기 --------------------------------------------------------

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "LoadingCanvas",
                    SortOrder,
                    transform);

            _group = canvas.gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 1f;
            _group.blocksRaycasts = true;

            // 뒤 화면을 가리고 클릭도 막는다.
            Image backdrop =
                UiFactory.CreateImage(
                    canvas.transform,
                    "Backdrop",
                    UiTheme.Backdrop);

            backdrop.raycastTarget = true;
            UiFactory.Stretch(backdrop.rectTransform);

            RectTransform stage =
                UiFactory.CreateRect(
                    canvas.transform,
                    "Stage");

            UiFactory.Place(stage, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(LoadingScreenLogic.BarWidth + 400f, 400f));

            // 달리는 길.
            Image ground =
                UiFactory.CreateImage(
                    stage,
                    "Ground",
                    new Color(UiTheme.AccentSoft.r, UiTheme.AccentSoft.g, UiTheme.AccentSoft.b, 0.35f));

            UiFactory.Place(ground.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -8f), new Vector2(LoadingScreenLogic.BarWidth + 60f, 3f));

            BuildRunner(stage);
            BuildCompanion(stage);

            // 주인공 아래 로딩 바.
            _bar =
                UiFactory.CreateBar(
                    stage,
                    "LoadingBar",
                    UiTheme.Accent,
                    UiTheme.TrackFill);

            UiFactory.Place(_bar.Root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(LoadingScreenLogic.BarWidth, 22f));

            _percent =
                UiFactory.CreateText(
                    stage,
                    "Percent",
                    "0%",
                    UiTheme.FontSubheading,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(_percent.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(LoadingScreenLogic.BarWidth * 0.5f + 16f, -51f), new Vector2(90f, 30f));

            Text loading =
                UiFactory.CreateText(
                    stage,
                    "LoadingLabel",
                    "LOADING...",
                    UiTheme.FontSmall,
                    UiTheme.TextMuted,
                    TextAnchor.MiddleCenter);

            UiFactory.Place(loading.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(300f, 24f));

            BuildTip(canvas.transform);
        }

        private void BuildRunner(
            RectTransform stage)
        {
            for (int i = 0; i < RunFrameCount; i++)
            {
                _runSprites[i] = LoadSprite($"{PlayerRoot}/Move_{i}");
            }

            _runnerImage =
                UiFactory.CreateImage(
                    stage,
                    "Runner",
                    Color.white);

            _runner = _runnerImage.rectTransform;

            // 발이 길 위에 닿도록 아래쪽을 기준점으로 둔다.
            UiFactory.Place(_runner, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), new Vector2(-LoadingScreenLogic.RunWidth * 0.5f, -12f), new Vector2(120f, 120f));

            if (_runSprites[0] != null)
            {
                _runnerImage.sprite = _runSprites[0];
                _runnerImage.preserveAspect = true;
            }
            else
            {
                // 그림이 없으면 밝은 막대로 대신한다.
                _runnerImage.color = UiTheme.TextPrimary;
                _runner.sizeDelta = new Vector2(40f, 90f);
            }
        }

        private void BuildCompanion(
            RectTransform stage)
        {
            _companion =
                UiFactory.CreateRect(
                    stage,
                    "Succubus");

            UiFactory.Place(_companion, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));

            Sprite art = LoadSprite($"{SuccubusRoot}/Idle");

            if (art != null)
            {
                Image image =
                    UiFactory.CreateImage(
                        _companion,
                        "Art",
                        Color.white);

                image.sprite = art;
                image.preserveAspect = true;
                UiFactory.Stretch(image.rectTransform);
            }
            else
            {
                BuildSuccubusSilhouette(_companion);
            }

            float startX = LoadingScreenLogic.GetCompanionTargetX(_runner.anchoredPosition.x);
            float startY = LoadingScreenLogic.GetCompanionTargetY(_runner.anchoredPosition.y, 0f);

            _companionX = startX;
            _companionY = startY;
            _companion.anchoredPosition = new Vector2(startX, startY);
        }

        /// <summary>서큐버스 그림이 아직 없을 때 쓰는 임시 실루엣이다. 날개 · 뿔 · 꼬리 · 몸통.</summary>
        private static void BuildSuccubusSilhouette(
            RectTransform parent)
        {
            Color body = new Color(0.62f, 0.28f, 0.86f);
            Color wing = new Color(0.36f, 0.16f, 0.52f, 0.9f);
            Color glow = new Color(1.00f, 0.45f, 0.85f);

            Piece(parent, "WingLeft", new Vector2(-26f, 10f), new Vector2(34f, 22f), wing, 25f);
            Piece(parent, "WingRight", new Vector2(26f, 10f), new Vector2(34f, 22f), wing, -25f);
            Piece(parent, "Aura", new Vector2(0f, 0f), new Vector2(56f, 80f), new Color(glow.r, glow.g, glow.b, 0.12f), 0f);
            Piece(parent, "Body", new Vector2(0f, -8f), new Vector2(22f, 44f), body, 0f);
            Piece(parent, "Head", new Vector2(0f, 24f), new Vector2(20f, 20f), body, 0f);
            Piece(parent, "HornLeft", new Vector2(-8f, 38f), new Vector2(5f, 12f), glow, 20f);
            Piece(parent, "HornRight", new Vector2(8f, 38f), new Vector2(5f, 12f), glow, -20f);
            Piece(parent, "Tail", new Vector2(14f, -30f), new Vector2(4f, 24f), body, 40f);
            Piece(parent, "TailTip", new Vector2(22f, -40f), new Vector2(9f, 9f), glow, 45f);
        }

        private static void Piece(
            RectTransform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            float rotation)
        {
            Image image =
                UiFactory.CreateImage(
                    parent,
                    name,
                    color);

            UiFactory.Place(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, size);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        private void BuildTip(
            Transform canvas)
        {
            RectTransform panel =
                UiFactory.CreatePanel(
                    canvas,
                    "TipPanel",
                    UiTheme.PanelFill,
                    UiTheme.PanelEdge);

            // 오른쪽 아래 구석.
            UiFactory.Place(panel, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-UiTheme.PaddingLarge * 2f, UiTheme.PaddingLarge * 2f), new Vector2(640f, 96f));

            Text title =
                UiFactory.CreateText(
                    panel,
                    "TipTitle",
                    "TIP",
                    UiTheme.FontSubheading,
                    UiTheme.Gold,
                    TextAnchor.MiddleLeft,
                    FontStyle.Bold);

            UiFactory.Place(title.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(UiTheme.PaddingLarge, 0f), new Vector2(60f, 40f));

            _tip =
                UiFactory.CreateText(
                    panel,
                    "TipText",
                    string.Empty,
                    UiTheme.FontBody,
                    UiTheme.TextPrimary,
                    TextAnchor.MiddleLeft);

            _tip.horizontalOverflow = HorizontalWrapMode.Wrap;
            UiFactory.Place(_tip.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(UiTheme.PaddingLarge + 70f, 0f), new Vector2(520f, 80f));
        }

        private Sprite LoadSprite(
            string path)
        {
            Texture2D texture =
                Resources.Load<Texture2D>(path);

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

            sprite.name = path.Replace('/', '_') + "_Loading";
            _created.Add(sprite);

            return sprite;
        }
    }
}
