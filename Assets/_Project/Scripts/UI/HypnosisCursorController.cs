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

        private Texture2D _coinCursor;
        private Texture2D[] _hypnosisCursors;

        private bool _lastHypnosisMode;
        private int _lastFrameIndex = -1;

        private void Awake()
        {
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
                    "CoinCursor_Runtime");

            _hypnosisCursors =
                new Texture2D[2]
                {
                    CreateCursorCompatibleTexture(
                        hypnosisSource0,
                        "HypnosisCursor_0_Runtime"),
                    CreateCursorCompatibleTexture(
                        hypnosisSource1,
                        "HypnosisCursor_1_Runtime")
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
                string textureName)
        {
            if (source == null)
            {
                Debug.LogWarning(
                    $"Project Theta: cursor source texture not found: {textureName}");

                return null;
            }

            RenderTexture previous =
                RenderTexture.active;

            RenderTexture temporary =
                RenderTexture.GetTemporary(
                    source.width,
                    source.height,
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
                        source.width,
                        source.height,
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
                        source.width,
                        source.height),
                    0,
                    0,
                    false);

                // false, false:
                // 1) mip chain을 만들지 않음
                // 2) CPU Read/Write 가능 상태를 유지
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
#if ENABLE_INPUT_SYSTEM
            bool keyboard =
                Keyboard.current != null &&
                Keyboard.current.eKey.isPressed;

            bool mouse =
                Mouse.current != null &&
                Mouse.current.leftButton.isPressed;

            return keyboard || mouse;
#else
            return Input.GetKey(KeyCode.E) ||
                   Input.GetMouseButton(0);
#endif
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
