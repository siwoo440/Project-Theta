using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ProjectTheta.UI
{
    public sealed class HypnosisCursorController :
        MonoBehaviour
    {
        [SerializeField] private float _frameDuration = 0.14f;
        [SerializeField] private float _hypnosisCursorScale = 0.5f;

        private Texture2D _coinCursor;
        private Texture2D[] _hypnosisCursors;

        private bool _lastHypnosisMode;
        private int _lastFrameIndex = -1;

        private void Awake()
        {
            BuildTextures();

            // 31일차: 설정 창에서 커서 크기를 바꾸면 다시 만든다.
            Core.SettingsApplier.Changed += HandleSettingsChanged;
        }

        private float _builtScale = -1f;

        private void HandleSettingsChanged()
        {
            if (Mathf.Approximately(_builtScale, Core.SettingsApplier.CursorScale))
            {
                return;
            }

            ReleaseTextures();
            BuildTextures();
        }

        private void BuildTextures()
        {
            // 31일차: 커서 크기 설정(작게 0.75 · 보통 1 · 크게 1.35)을 곱한다.
            _builtScale = Core.SettingsApplier.CursorScale;

            Texture2D coinSource =
                Resources.Load<Texture2D>(
                    "UI/Cursor/CoinCursor");

            Texture2D hypnosisSource0 =
                Resources.Load<Texture2D>(
                    "UI/Cursor/HypnosisCursor_0");

            Texture2D hypnosisSource1 =
                Resources.Load<Texture2D>(
                    "UI/Cursor/HypnosisCursor_1");

            _coinCursor =
                CreateCursorCompatibleTexture(
                    coinSource,
                    "CoinCursor_Runtime",
                    0.5f * _builtScale);

            _hypnosisCursors =
                new Texture2D[2]
                {
                    CreateCursorCompatibleTexture(
                        hypnosisSource0,
                        "HypnosisCursor_0_Runtime",
                        _hypnosisCursorScale * _builtScale),
                    CreateCursorCompatibleTexture(
                        hypnosisSource1,
                        "HypnosisCursor_1_Runtime",
                        _hypnosisCursorScale * _builtScale)
                };

            Cursor.visible = true;
            ApplyCoinCursor();
        }

        private void OnEnable()
        {
            if (_coinCursor != null)
            {
                ApplyCoinCursor();
            }
        }

        private void Update()
        {
            bool hypnosisMode =
                ReadHypnosisHeld();

            if (!hypnosisMode)
            {
                if (_lastHypnosisMode ||
                    _lastFrameIndex != -1)
                {
                    ApplyCoinCursor();
                }

                return;
            }

            int frameIndex =
                HypnosisCursorAnimationLogic.
                GetFrameIndex(
                    Time.unscaledTime,
                    _frameDuration);

            if (!_lastHypnosisMode ||
                frameIndex != _lastFrameIndex)
            {
                ApplyHypnosisCursor(
                    frameIndex);
            }
        }

        private void ApplyCoinCursor()
        {
            _lastHypnosisMode = false;
            _lastFrameIndex = -1;

            if (_coinCursor == null)
            {
                Cursor.SetCursor(
                    null,
                    Vector2.zero,
                    CursorMode.Auto);

                return;
            }

            Vector2 hotspot =
                new Vector2(
                    _coinCursor.width * 0.5f,
                    _coinCursor.height * 0.5f);

            Cursor.SetCursor(
                _coinCursor,
                hotspot,
                CursorMode.ForceSoftware);
        }

        private void ApplyHypnosisCursor(
            int frameIndex)
        {
            _lastHypnosisMode = true;
            _lastFrameIndex = frameIndex;

            if (_hypnosisCursors == null ||
                frameIndex < 0 ||
                frameIndex >=
                _hypnosisCursors.Length ||
                _hypnosisCursors[frameIndex] == null)
            {
                return;
            }

            Texture2D texture =
                _hypnosisCursors[frameIndex];

            Vector2 hotspot =
                new Vector2(
                    texture.width * 0.67f,
                    texture.height * 0.63f);

            Cursor.SetCursor(
                texture,
                hotspot,
                CursorMode.ForceSoftware);
        }

        private static Texture2D
            CreateCursorCompatibleTexture(
                Texture2D source,
                string textureName,
                float scale)
        {
            if (source == null)
            {
                Debug.LogWarning(
                    $"Project Theta: cursor source texture not found: {textureName}");

                return null;
            }

            float safeScale =
                Mathf.Clamp(
                    scale,
                    0.1f,
                    1f);

            int targetWidth =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        source.width *
                        safeScale));

            int targetHeight =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        source.height *
                        safeScale));

            RenderTexture previous =
                RenderTexture.active;

            RenderTexture temporary =
                RenderTexture.GetTemporary(
                    targetWidth,
                    targetHeight,
                    0,
                    RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Default);

            try
            {
                Graphics.Blit(
                    source,
                    temporary);

                RenderTexture.active =
                    temporary;

                Texture2D compatible =
                    new Texture2D(
                        targetWidth,
                        targetHeight,
                        TextureFormat.RGBA32,
                        false,
                        false)
                    {
                        name = textureName,
                        filterMode =
                            FilterMode.Bilinear,
                        wrapMode =
                            TextureWrapMode.Clamp
                    };

                compatible.ReadPixels(
                    new Rect(
                        0f,
                        0f,
                        targetWidth,
                        targetHeight),
                    0,
                    0,
                    false);

                compatible.Apply(
                    false,
                    false);

                return compatible;
            }
            finally
            {
                RenderTexture.active =
                    previous;

                RenderTexture.ReleaseTemporary(
                    temporary);
            }
        }

        private bool ReadHypnosisHeld()
        {
            return Core.GameInput.IsHeld(
                Core.GameAction.Hypnosis);
        }

        private void OnDisable()
        {
            _lastHypnosisMode = false;
            _lastFrameIndex = -1;

            Cursor.SetCursor(
                null,
                Vector2.zero,
                CursorMode.Auto);
        }

        private void OnDestroy()
        {
            Core.SettingsApplier.Changed -= HandleSettingsChanged;

            ReleaseTextures();
        }

        private void ReleaseTextures()
        {
            DestroyCursorTexture(
                ref _coinCursor);

            if (_hypnosisCursors == null)
            {
                return;
            }

            for (int i = 0;
                 i < _hypnosisCursors.Length;
                 i++)
            {
                Texture2D texture =
                    _hypnosisCursors[i];

                if (texture != null)
                {
                    Destroy(texture);
                    _hypnosisCursors[i] =
                        null;
                }
            }
        }

        private static void DestroyCursorTexture(
            ref Texture2D texture)
        {
            if (texture == null)
            {
                return;
            }

            Destroy(texture);
            texture = null;
        }
    }
}
