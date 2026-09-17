using System.Collections.Generic;

namespace ProjectTheta.UI
{
    /// <summary>
    /// Esc로 닫는 창들의 순서다 (31일차).
    ///
    /// 통계 · 업적 · 설정 · 조작법 · 일시정지 창이 모두 Esc를 쓴다.
    /// Esc는 "가장 위에 열린 창"만 닫고, 한 번 누른 Esc는 한 프레임에 한 번만 쓰인다.
    /// 열린 창이 없을 때만 일시정지 메뉴가 열린다.
    /// </summary>
    public static class UiEscapeStack
    {
        private static readonly List<object> Stack = new List<object>();
        private static int _consumedFrame = -1;

        public static int Count =>
            Stack.Count;

        public static bool IsEmpty =>
            Stack.Count == 0;

        public static void Push(
            object owner)
        {
            if (owner == null)
            {
                return;
            }

            Stack.Remove(owner);
            Stack.Add(owner);
        }

        public static void Remove(
            object owner)
        {
            Stack.Remove(owner);
        }

        public static bool IsTop(
            object owner)
        {
            return owner != null &&
                   Stack.Count > 0 &&
                   ReferenceEquals(Stack[Stack.Count - 1], owner);
        }

        /// <summary>이 프레임의 Esc를 이미 누가 썼는지다.</summary>
        public static bool IsConsumed(
            int frame)
        {
            return _consumedFrame == frame;
        }

        /// <summary>맨 위 창이면 이 프레임의 Esc를 가져간다.</summary>
        public static bool TryConsume(
            object owner,
            int frame)
        {
            if (IsConsumed(frame) ||
                !IsTop(owner))
            {
                return false;
            }

            _consumedFrame = frame;

            return true;
        }

        /// <summary>열린 창이 없을 때 이 프레임의 Esc를 가져간다(일시정지 메뉴 열기용).</summary>
        public static bool TryConsumeWhenEmpty(
            int frame)
        {
            if (IsConsumed(frame) ||
                !IsEmpty)
            {
                return false;
            }

            _consumedFrame = frame;

            return true;
        }

        public static void Clear()
        {
            Stack.Clear();
            _consumedFrame = -1;
        }

        [UnityEngine.RuntimeInitializeOnLoadMethod(
            UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Clear();
        }
    }

    /// <summary>일시정지 메뉴 규칙이다 (31일차).</summary>
    public static class PauseMenuLogic
    {
        /// <summary>포기하면 계약 정기를 주지 않는다.</summary>
        public const int AbandonContractEssence = 0;

        /// <summary>포기 버튼을 한 번 누른 뒤 다시 눌러야 하는 시간이다.</summary>
        public const float AbandonConfirmSeconds = 3f;

        /// <summary>
        /// 일시정지 메뉴를 열 수 있는지다. 이미 게임이 멈춰 있거나 다른 창이 떠 있으면 열지 않는다.
        /// </summary>
        public static bool CanOpen(
            bool stageRunning,
            bool cardChoiceOpen,
            bool endingPlaying,
            bool loading)
        {
            return stageRunning &&
                   !cardChoiceOpen &&
                   !endingPlaying &&
                   !loading;
        }

        public static int GetContractEssence(
            bool abandoned,
            int computed)
        {
            return abandoned
                ? AbandonContractEssence
                : computed;
        }
    }

    /// <summary>조작법 한 줄이다.</summary>
    public struct ControlRow
    {
        public string Group;
        public string Action;
        public string Keys;

        public ControlRow(
            string group,
            string action,
            string keys)
        {
            Group = group;
            Action = action;
            Keys = keys;
        }
    }

    /// <summary>
    /// 조작법 표다 (31일차). 32일차부터 게임 조작 줄은 현재 키 설정(<see cref="Core.GameInput"/>)을 읽는다.
    /// 화면 조작 줄은 바꿀 수 없는 고정 키다.
    /// </summary>
    public static class ControlsCatalog
    {
        public static ControlRow[] All =>
            Build(Core.GameInput.Table);

        public static ControlRow[] Build(
            Core.KeyBindingTable table)
        {
            string P(Core.GameAction action) =>
                Core.InputBindingLogic.GetDisplayName(table?.Get(action, 0));

            string S(Core.GameAction action) =>
                table?.Get(action, 1);

            string move = $"{P(Core.GameAction.MoveUp)} {P(Core.GameAction.MoveLeft)} {P(Core.GameAction.MoveDown)} {P(Core.GameAction.MoveRight)}";

            if (!string.IsNullOrEmpty(S(Core.GameAction.MoveUp)))
            {
                move +=
                    $"  /  {Core.InputBindingLogic.GetDisplayName(S(Core.GameAction.MoveUp))} " +
                    $"{Core.InputBindingLogic.GetDisplayName(S(Core.GameAction.MoveLeft))} " +
                    $"{Core.InputBindingLogic.GetDisplayName(S(Core.GameAction.MoveDown))} " +
                    $"{Core.InputBindingLogic.GetDisplayName(S(Core.GameAction.MoveRight))}";
            }

            string D(Core.GameAction action) =>
                Core.InputBindingLogic.Describe(table, action);

            return new[]
            {
                new ControlRow("기본", "이동", move),
                new ControlRow("기본", "대시", D(Core.GameAction.Dash)),
                new ControlRow("기본", "층 이동 · 상호작용", $"{D(Core.GameAction.Interact)}  (계단 · 문 앞에서)"),
                new ControlRow("최면", "최면 (NPC에 커서를 대고 유지)", D(Core.GameAction.Hypnosis)),
                new ControlRow("최면", "파동 (주변을 잠깐 멍하게, 유지)", D(Core.GameAction.Wave)),
                new ControlRow("최면", "동행자 회수", "동행자를 데리고 회수 지점으로"),
                new ControlRow("최면", "탈출 (목표 달성 뒤)", $"1F 회수 지점에서 {D(Core.GameAction.Interact)}"),
                new ControlRow("위기", "붙잡힘 탈출 · 힘겨루기", $"{D(Core.GameAction.StruggleLeft)} · {D(Core.GameAction.StruggleRight)} 번갈아"),
                new ControlRow("아이템", "아이템 1 · 2", $"{D(Core.GameAction.Item1)} · {D(Core.GameAction.Item2)}"),
                new ControlRow("화면", "강화 카드 고르기", "클릭  /  숫자 1~4  (고정)"),
                new ControlRow("화면", "결과 · 엔딩 연출 건너뛰기", "클릭  (고정)"),
                new ControlRow("화면", "지도: 장소 선택 · 출발 · 통계", "숫자 1~8 · Enter · Tab  (고정)"),
                new ControlRow("화면", "일시정지 · 창 닫기", "Esc  (고정)"),
                new ControlRow("화면", "성능 표시 켜기 · 끄기", "F3  (고정)")
            };
        }
    }
}
