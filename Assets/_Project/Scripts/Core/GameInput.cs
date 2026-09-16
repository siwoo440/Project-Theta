using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace ProjectTheta.Core
{
    /// <summary>
    /// 게임 조작 입력의 창구다 (32일차).
    ///
    /// 예전에는 이동 · 대시 · 최면 · 파동 · 층 이동 · 탈출 · 아이템 키를 각 스크립트가 직접 읽었다.
    /// 이제 모두 여기서 현재 키표(<see cref="Table"/>)를 보고 읽는다. 키표는 세이브에 저장되고 설정 창에서 바꾼다.
    /// 화면 조작(지도 숫자 · 카드 · Esc)은 바꾸지 않으므로 각 화면이 그대로 읽는다.
    /// </summary>
    public static class GameInput
    {
        private static KeyBindingTable _table = InputBindingLogic.CreateDefault();

#if ENABLE_INPUT_SYSTEM
        private static readonly Dictionary<string, Key> KeyCache = new Dictionary<string, Key>();
#endif

        public static KeyBindingTable Table =>
            _table;

        /// <summary>키표가 바뀔 때 알린다. 안내 문구가 다시 그린다.</summary>
        public static event Action Changed;

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _table = InputBindingLogic.CreateDefault();
            Changed = null;
        }

        public static void SetTable(
            KeyBindingTable table)
        {
            _table = table ?? InputBindingLogic.CreateDefault();
            Changed?.Invoke();
        }

        /// <summary>저장 줄을 읽어 키표로 쓴다. 모르는 키 이름은 기본 키로 채운다.</summary>
        public static void Load(
            string[] lines)
        {
            SetTable(
                InputBindingLogic.Deserialize(
                    lines,
                    IsKnownCode));
        }

        public static bool IsKnownCode(
            string code)
        {
            if (InputBindingLogic.IsMouse(code))
            {
                return true;
            }

#if ENABLE_INPUT_SYSTEM
            return TryGetKey(code, out _);
#else
            return false;
#endif
        }

        // 읽기 ----------------------------------------------------------

        public static bool IsHeld(
            GameAction action)
        {
            return Read(action, 0, false) ||
                   Read(action, 1, false);
        }

        public static bool WasPressed(
            GameAction action)
        {
            return Read(action, 0, true) ||
                   Read(action, 1, true);
        }

        /// <summary>이동 방향이다. 대각선도 길이 1로 맞춘다.</summary>
        public static Vector2 GetMove()
        {
            float x = 0f;
            float y = 0f;

            if (IsHeld(GameAction.MoveLeft)) x -= 1f;
            if (IsHeld(GameAction.MoveRight)) x += 1f;
            if (IsHeld(GameAction.MoveUp)) y += 1f;
            if (IsHeld(GameAction.MoveDown)) y -= 1f;

            Vector2 move = new Vector2(x, y);

            return move.sqrMagnitude > 1f
                ? move.normalized
                : move;
        }

        /// <summary>안내 문구용 기본 키 이름이다("좌클릭", "F").</summary>
        public static string ShortLabel(
            GameAction action)
        {
            return InputBindingLogic.GetShortName(
                _table.Get(action, 0));
        }

        /// <summary>조작법 창용 전체 키 이름이다("W / ↑").</summary>
        public static string FullLabel(
            GameAction action)
        {
            return InputBindingLogic.Describe(_table, action);
        }

        private static bool Read(
            GameAction action,
            int slot,
            bool pressedThisFrame)
        {
            string code = _table.Get(action, slot);

            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

#if ENABLE_INPUT_SYSTEM
            if (InputBindingLogic.IsMouse(code))
            {
                // 디버그 패널 위에서 누른 클릭은 평소 조작이 아니다 (20일차).
                if (InputBindingLogic.GetContext(action) == 0 &&
                    PointerGuard.IsOverOverlay)
                {
                    return false;
                }

                ButtonControl button = GetMouseButton(code);

                return button != null &&
                       (pressedThisFrame
                           ? button.wasPressedThisFrame
                           : button.isPressed);
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null ||
                !TryGetKey(code, out Key key))
            {
                return false;
            }

            KeyControl control = keyboard[key];

            return control != null &&
                   (pressedThisFrame
                       ? control.wasPressedThisFrame
                       : control.isPressed);
#else
            return false;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static ButtonControl GetMouseButton(
            string code)
        {
            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return null;
            }

            switch (code)
            {
                case InputBindingLogic.MouseLeft: return mouse.leftButton;
                case InputBindingLogic.MouseRight: return mouse.rightButton;
                case InputBindingLogic.MouseMiddle: return mouse.middleButton;
                default: return null;
            }
        }

        private static bool TryGetKey(
            string code,
            out Key key)
        {
            if (string.IsNullOrEmpty(code))
            {
                key = Key.None;

                return false;
            }

            if (KeyCache.TryGetValue(code, out key))
            {
                return key != Key.None;
            }

            if (!Enum.TryParse(code, false, out key) ||
                !Enum.IsDefined(typeof(Key), key) ||
                key == Key.None ||
                key.ToString() != code)
            {
                key = Key.None;
            }

            KeyCache[code] = key;

            return key != Key.None;
        }

        /// <summary>
        /// 이번 프레임에 새로 누른 키나 마우스 버튼을 찾는다. 키 설정 창이 쓴다.
        /// 찾으면 저장용 글자("W", "Mouse1")를 돌려준다.
        /// </summary>
        public static bool TryReadAnyPress(
            out string code)
        {
            code = null;

            Mouse mouse = Mouse.current;

            if (mouse != null)
            {
                if (mouse.leftButton.wasPressedThisFrame) { code = InputBindingLogic.MouseLeft; return true; }
                if (mouse.rightButton.wasPressedThisFrame) { code = InputBindingLogic.MouseRight; return true; }
                if (mouse.middleButton.wasPressedThisFrame) { code = InputBindingLogic.MouseMiddle; return true; }
            }

            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return false;
            }

            foreach (KeyControl control in keyboard.allKeys)
            {
                if (control != null &&
                    control.wasPressedThisFrame &&
                    control.keyCode != Key.None)
                {
                    code = control.keyCode.ToString();

                    return true;
                }
            }

            return false;
        }
#else
        public static bool TryReadAnyPress(
            out string code)
        {
            code = null;

            return false;
        }
#endif
    }
}
