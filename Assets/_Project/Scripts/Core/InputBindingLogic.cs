using System;
using System.Collections.Generic;

namespace ProjectTheta.Core
{
    /// <summary>키를 바꿀 수 있는 게임 조작이다 (32일차). 순서가 설정 창의 줄 순서다.</summary>
    public enum GameAction
    {
        MoveUp = 0,
        MoveDown = 1,
        MoveLeft = 2,
        MoveRight = 3,
        Dash = 4,
        Interact = 5,
        Hypnosis = 6,
        Wave = 7,
        StruggleLeft = 8,
        StruggleRight = 9,
        Item1 = 10,
        Item2 = 11
    }

    /// <summary>키를 바꾼 결과다.</summary>
    public enum BindingResult
    {
        Ok,
        Unchanged,

        /// <summary>같은 상황에서 쓰는 다른 조작이 이미 그 키를 쓴다. 저장하지 않는다.</summary>
        Conflict,

        /// <summary>Esc · F1처럼 바꿀 수 없는 키다.</summary>
        Reserved,

        /// <summary>기본 칸은 비울 수 없다.</summary>
        EmptyPrimary,

        Invalid
    }

    /// <summary>
    /// 조작별 키 두 칸(기본 · 보조)의 표다 (32일차).
    /// 키는 글자로 담는다. 키보드는 Input System의 Key 이름("W", "LeftShift", "Digit1"),
    /// 마우스는 "Mouse0"(왼쪽) · "Mouse1"(오른쪽) · "Mouse2"(가운데)다. 빈 칸은 빈 문자열이다.
    /// </summary>
    public sealed class KeyBindingTable
    {
        public const int SlotCount = 2;

        private readonly string[,] _slots;

        public KeyBindingTable()
        {
            _slots = new string[InputBindingLogic.ActionCount, SlotCount];

            for (int a = 0; a < InputBindingLogic.ActionCount; a++)
            {
                for (int s = 0; s < SlotCount; s++)
                {
                    _slots[a, s] = string.Empty;
                }
            }
        }

        public string Get(
            GameAction action,
            int slot)
        {
            return IsInRange(action, slot)
                ? _slots[(int)action, slot]
                : string.Empty;
        }

        public void Set(
            GameAction action,
            int slot,
            string code)
        {
            if (IsInRange(action, slot))
            {
                _slots[(int)action, slot] = code ?? string.Empty;
            }
        }

        public KeyBindingTable Clone()
        {
            KeyBindingTable copy = new KeyBindingTable();

            for (int a = 0; a < InputBindingLogic.ActionCount; a++)
            {
                for (int s = 0; s < SlotCount; s++)
                {
                    copy._slots[a, s] = _slots[a, s];
                }
            }

            return copy;
        }

        private static bool IsInRange(
            GameAction action,
            int slot)
        {
            return (int)action >= 0 &&
                   (int)action < InputBindingLogic.ActionCount &&
                   slot >= 0 &&
                   slot < SlotCount;
        }
    }

    /// <summary>한 칸의 위치다.</summary>
    public struct BindingSlot
    {
        public GameAction Action;
        public int Slot;

        public BindingSlot(
            GameAction action,
            int slot)
        {
            Action = action;
            Slot = slot;
        }
    }

    /// <summary>
    /// 키 설정 규칙이다 (32일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   기본 키표 · 겹침 찾기 · 바꾸기 판정 · 저장 문자열 · 표시 이름
    ///
    /// 겹침은 "같은 상황에서 쓰는 조작"끼리만 본다. 붙잡힘 · 힘겨루기 입력은 평소 조작과 따로라서
    /// 최면(마우스 왼쪽)과 탈출 왼쪽 입력(마우스 왼쪽)이 같아도 된다.
    /// 겹치면 경고만 하고 저장하지 않는다.
    /// </summary>
    public static class InputBindingLogic
    {
        public const string MouseLeft = "Mouse0";
        public const string MouseRight = "Mouse1";
        public const string MouseMiddle = "Mouse2";

        public static readonly int ActionCount =
            Enum.GetValues(typeof(GameAction)).Length;

        /// <summary>바꿀 수 없는 키다. Esc는 창 닫기 · 일시정지, F1은 개발용 패널이다.</summary>
        public static readonly string[] ReservedCodes = { "Escape", "F1" };

        /// <summary>조작별 이름이다.</summary>
        public static string GetLabel(
            GameAction action)
        {
            switch (action)
            {
                case GameAction.MoveUp: return "위로 이동";
                case GameAction.MoveDown: return "아래로 이동";
                case GameAction.MoveLeft: return "왼쪽 이동";
                case GameAction.MoveRight: return "오른쪽 이동";
                case GameAction.Dash: return "대시";
                case GameAction.Interact: return "층 이동 · 상호작용";
                case GameAction.Hypnosis: return "최면 (유지)";
                case GameAction.Wave: return "파동 (유지)";
                case GameAction.StruggleLeft: return "탈출 · 힘겨루기 입력 A";
                case GameAction.StruggleRight: return "탈출 · 힘겨루기 입력 B";
                case GameAction.Item1: return "아이템 1";
                case GameAction.Item2: return "아이템 2";
                default: return action.ToString();
            }
        }

        /// <summary>설정 창 · 조작법 창의 묶음 이름이다.</summary>
        public static string GetGroup(
            GameAction action)
        {
            switch (action)
            {
                case GameAction.Hypnosis:
                case GameAction.Wave:
                    return "최면";

                case GameAction.StruggleLeft:
                case GameAction.StruggleRight:
                    return "위기";

                case GameAction.Item1:
                case GameAction.Item2:
                    return "아이템";

                default:
                    return "기본";
            }
        }

        /// <summary>겹침을 따지는 상황이다. 0 = 평소, 1 = 붙잡힘 · 힘겨루기.</summary>
        public static int GetContext(
            GameAction action)
        {
            return action == GameAction.StruggleLeft ||
                   action == GameAction.StruggleRight
                ? 1
                : 0;
        }

        public static KeyBindingTable CreateDefault()
        {
            KeyBindingTable table = new KeyBindingTable();

            Set(table, GameAction.MoveUp, "W", "UpArrow");
            Set(table, GameAction.MoveDown, "S", "DownArrow");
            Set(table, GameAction.MoveLeft, "A", "LeftArrow");
            Set(table, GameAction.MoveRight, "D", "RightArrow");
            Set(table, GameAction.Dash, "LeftShift", "Space");
            Set(table, GameAction.Interact, "F", string.Empty);

            // 32일차: 최면 기본 키는 마우스 왼쪽 하나다(예전 E는 뺐다).
            Set(table, GameAction.Hypnosis, MouseLeft, string.Empty);
            Set(table, GameAction.Wave, MouseRight, string.Empty);
            Set(table, GameAction.StruggleLeft, MouseLeft, string.Empty);
            Set(table, GameAction.StruggleRight, MouseRight, string.Empty);
            Set(table, GameAction.Item1, "Digit1", string.Empty);
            Set(table, GameAction.Item2, "Digit2", string.Empty);

            return table;
        }

        private static void Set(
            KeyBindingTable table,
            GameAction action,
            string primary,
            string secondary)
        {
            table.Set(action, 0, primary);
            table.Set(action, 1, secondary);
        }

        public static bool IsReserved(
            string code)
        {
            return Array.IndexOf(ReservedCodes, code) >= 0;
        }

        public static bool IsMouse(
            string code)
        {
            return code == MouseLeft ||
                   code == MouseRight ||
                   code == MouseMiddle;
        }

        /// <summary>
        /// 그 키를 같은 상황의 다른 칸이 이미 쓰는지 찾는다. 없으면 false다.
        /// 같은 조작의 다른 칸도 겹침으로 본다(기본 · 보조가 같으면 의미가 없다).
        /// </summary>
        public static bool FindConflict(
            KeyBindingTable table,
            GameAction action,
            int slot,
            string code,
            out BindingSlot conflict)
        {
            conflict = default;

            if (table == null ||
                string.IsNullOrEmpty(code))
            {
                return false;
            }

            int context = GetContext(action);

            for (int a = 0; a < ActionCount; a++)
            {
                GameAction other = (GameAction)a;

                if (GetContext(other) != context)
                {
                    continue;
                }

                for (int s = 0; s < KeyBindingTable.SlotCount; s++)
                {
                    if (other == action &&
                        s == slot)
                    {
                        continue;
                    }

                    if (table.Get(other, s) == code)
                    {
                        conflict = new BindingSlot(other, s);

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 키를 바꿔 본다. 겹치거나 막힌 키면 표를 건드리지 않고 이유를 돌려준다.
        /// </summary>
        public static BindingResult TrySet(
            KeyBindingTable table,
            GameAction action,
            int slot,
            string code,
            out BindingSlot conflict)
        {
            conflict = default;

            if (table == null ||
                slot < 0 ||
                slot >= KeyBindingTable.SlotCount ||
                !IsValidCode(code))
            {
                return BindingResult.Invalid;
            }

            if (IsReserved(code))
            {
                return BindingResult.Reserved;
            }

            if (table.Get(action, slot) == code)
            {
                return BindingResult.Unchanged;
            }

            if (FindConflict(table, action, slot, code, out conflict))
            {
                return BindingResult.Conflict;
            }

            table.Set(action, slot, code);

            return BindingResult.Ok;
        }

        /// <summary>보조 칸을 비운다. 기본 칸은 비울 수 없다.</summary>
        public static BindingResult Clear(
            KeyBindingTable table,
            GameAction action,
            int slot)
        {
            if (table == null)
            {
                return BindingResult.Invalid;
            }

            if (slot == 0)
            {
                return BindingResult.EmptyPrimary;
            }

            if (string.IsNullOrEmpty(table.Get(action, slot)))
            {
                return BindingResult.Unchanged;
            }

            table.Set(action, slot, string.Empty);

            return BindingResult.Ok;
        }

        /// <summary>경고 문구다. 성공 · 변화 없음이면 빈 문자열이다.</summary>
        public static string DescribeResult(
            BindingResult result,
            string code,
            BindingSlot conflict)
        {
            switch (result)
            {
                case BindingResult.Conflict:
                    return $"{GetDisplayName(code)} 키는 이미 '{GetLabel(conflict.Action)}'에서 쓰고 있습니다. 저장하지 않았습니다.";

                case BindingResult.Reserved:
                    return $"{GetDisplayName(code)} 키는 바꿀 수 없는 키입니다.";

                case BindingResult.EmptyPrimary:
                    return "기본 칸은 비울 수 없습니다.";

                case BindingResult.Invalid:
                    return "쓸 수 없는 입력입니다.";

                default:
                    return string.Empty;
            }
        }

        /// <summary>키 글자 형식이 맞는지다. 실제 키 이름인지는 Unity 쪽(<see cref="GameInput"/>)이 따로 본다.</summary>
        public static bool IsValidCode(
            string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return false;
            }

            for (int i = 0; i < code.Length; i++)
            {
                if (!char.IsLetterOrDigit(code[i]))
                {
                    return false;
                }
            }

            return true;
        }

        // 저장 ----------------------------------------------------------

        /// <summary>"MoveUp=W,UpArrow" 줄들로 바꾼다.</summary>
        public static string[] Serialize(
            KeyBindingTable table)
        {
            KeyBindingTable source = table ?? CreateDefault();
            string[] lines = new string[ActionCount];

            for (int a = 0; a < ActionCount; a++)
            {
                GameAction action = (GameAction)a;
                lines[a] = $"{action}={source.Get(action, 0)},{source.Get(action, 1)}";
            }

            return lines;
        }

        /// <summary>
        /// 저장 줄을 표로 읽는다. 없는 조작 · 잘못된 값은 기본 키로 채운다.
        /// isKnownCode는 실제 키 이름인지 확인한다(null이면 형식만 본다).
        /// 읽은 뒤 겹침이 남아 있으면(손으로 고친 파일 등) 표 전체를 기본으로 되돌린다.
        /// </summary>
        public static KeyBindingTable Deserialize(
            string[] lines,
            Func<string, bool> isKnownCode = null)
        {
            KeyBindingTable defaults = CreateDefault();
            KeyBindingTable table = defaults.Clone();

            if (lines == null)
            {
                return table;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];

                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                int equals = line.IndexOf('=');

                if (equals <= 0 ||
                    !TryParseAction(line.Substring(0, equals), out GameAction action))
                {
                    continue;
                }

                string[] codes = line.Substring(equals + 1).Split(',');
                string primary = codes.Length > 0 ? codes[0] : string.Empty;
                string secondary = codes.Length > 1 ? codes[1] : string.Empty;

                if (!IsUsable(primary, isKnownCode))
                {
                    continue;
                }

                table.Set(action, 0, primary);
                table.Set(
                    action,
                    1,
                    string.IsNullOrEmpty(secondary) || IsUsable(secondary, isKnownCode)
                        ? secondary
                        : string.Empty);
            }

            return HasAnyConflict(table)
                ? defaults
                : table;
        }

        public static bool HasAnyConflict(
            KeyBindingTable table)
        {
            for (int a = 0; a < ActionCount; a++)
            {
                for (int s = 0; s < KeyBindingTable.SlotCount; s++)
                {
                    GameAction action = (GameAction)a;

                    if (FindConflict(table, action, s, table.Get(action, s), out _))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool IsUsable(
            string code,
            Func<string, bool> isKnownCode)
        {
            return IsValidCode(code) &&
                   !IsReserved(code) &&
                   (isKnownCode == null || IsMouse(code) || isKnownCode(code));
        }

        private static bool TryParseAction(
            string name,
            out GameAction action)
        {
            // "7"처럼 숫자로 쓴 줄은 받지 않는다. 이름으로만 읽는다.
            action = default;

            return !string.IsNullOrEmpty(name) &&
                   char.IsLetter(name[0]) &&
                   Enum.TryParse(name, false, out action) &&
                   Enum.IsDefined(typeof(GameAction), action);
        }

        // 표시 ----------------------------------------------------------

        private static readonly Dictionary<string, string> DisplayNames =
            new Dictionary<string, string>
            {
                { MouseLeft, "마우스 왼쪽" },
                { MouseRight, "마우스 오른쪽" },
                { MouseMiddle, "마우스 가운데" },
                { "LeftShift", "Shift" },
                { "RightShift", "오른쪽 Shift" },
                { "LeftCtrl", "Ctrl" },
                { "RightCtrl", "오른쪽 Ctrl" },
                { "LeftAlt", "Alt" },
                { "RightAlt", "오른쪽 Alt" },
                { "UpArrow", "↑" },
                { "DownArrow", "↓" },
                { "LeftArrow", "←" },
                { "RightArrow", "→" },
                { "Enter", "Enter" },
                { "NumpadEnter", "숫자패드 Enter" },
                { "Backspace", "Backspace" },
                { "Backquote", "`" },
                { "Minus", "-" },
                { "Equals", "=" },
                { "LeftBracket", "[" },
                { "RightBracket", "]" },
                { "Semicolon", ";" },
                { "Quote", "'" },
                { "Comma", "," },
                { "Period", "." },
                { "Slash", "/" },
                { "Backslash", "\\" }
            };

        /// <summary>"LeftShift" → "Shift", "Digit1" → "1", "Mouse1" → "마우스 오른쪽". 빈 칸은 "-"다.</summary>
        public static string GetDisplayName(
            string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return "-";
            }

            if (DisplayNames.TryGetValue(code, out string name))
            {
                return name;
            }

            if (code.StartsWith("Digit", StringComparison.Ordinal) &&
                code.Length == 6)
            {
                return code.Substring(5);
            }

            if (code.StartsWith("Numpad", StringComparison.Ordinal) &&
                code.Length == 7)
            {
                return "숫자패드 " + code.Substring(6);
            }

            return code;
        }

        /// <summary>"W / ↑"처럼 조작의 키를 모두 보여 준다.</summary>
        public static string Describe(
            KeyBindingTable table,
            GameAction action)
        {
            string primary = GetDisplayName(table?.Get(action, 0));
            string secondary = table?.Get(action, 1);

            return string.IsNullOrEmpty(secondary)
                ? primary
                : $"{primary} / {GetDisplayName(secondary)}";
        }

        /// <summary>안내 문구용 짧은 이름이다. 마우스는 "좌클릭" · "우클릭"으로 줄인다.</summary>
        public static string GetShortName(
            string code)
        {
            switch (code)
            {
                case MouseLeft: return "좌클릭";
                case MouseRight: return "우클릭";
                case MouseMiddle: return "휠클릭";
                default: return GetDisplayName(code);
            }
        }
    }
}
