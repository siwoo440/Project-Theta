using System;

namespace ProjectTheta.UI
{
    /// <summary>허브(학생의 방)에서 켜고 끄는 창이다 (36일차).</summary>
    public enum HubPanel
    {
        None = 0,
        Upgrades = 1,
        Stats = 2,
        Difficulty = 3,
        Diary = 4
    }

    /// <summary>방 안에서 누를 수 있는 물건이다 (36일차).</summary>
    public enum HubRoomObject
    {
        None = 0,
        Notebook = 1,
        Clock = 2,
        Bookshelf = 3,
        Window = 4,
        Diary = 5
    }

    /// <summary>
    /// 허브 창 · 방 물건 규칙이다 (36일차).
    ///
    ///   책상 위 노트 → 강화      벽시계 → 난이도      책장 → 통계
    ///   침대 옆 일기장 → 이야기   창문 → 출격(지도)
    /// 창은 한 번에 하나만 열리고, 같은 버튼을 다시 누르면 닫힌다.
    /// </summary>
    public static class HubRoomLogic
    {
        /// <summary>창 뒤를 어둡게 하는 정도다. 방이 계속 보이게 옅게 둔다.</summary>
        public const float WindowDim = 0.25f;

        /// <summary>창 바탕 불투명도다.</summary>
        public const float WindowAlpha = 0.82f;

        /// <summary>위 · 아래 띠 불투명도다.</summary>
        public const float BarAlpha = 0.55f;

        public static readonly HubPanel[] BottomPanels =
        {
            HubPanel.Upgrades,
            HubPanel.Stats,
            HubPanel.Difficulty,
            HubPanel.Diary
        };

        public static HubPanel Toggle(
            HubPanel current,
            HubPanel pressed)
        {
            return current == pressed
                ? HubPanel.None
                : pressed;
        }

        /// <summary>물건을 누르면 여는 창이다. 창문은 창이 아니라 출격이라 None이다.</summary>
        public static HubPanel GetPanel(
            HubRoomObject item)
        {
            switch (item)
            {
                case HubRoomObject.Notebook:
                    return HubPanel.Upgrades;

                case HubRoomObject.Clock:
                    return HubPanel.Difficulty;

                case HubRoomObject.Bookshelf:
                    return HubPanel.Stats;

                case HubRoomObject.Diary:
                    return HubPanel.Diary;

                default:
                    return HubPanel.None;
            }
        }

        public static bool IsSortie(
            HubRoomObject item)
        {
            return item == HubRoomObject.Window;
        }

        /// <summary>물건에 마우스를 올리면 뜨는 이름표다.</summary>
        public static string GetObjectLabel(
            HubRoomObject item)
        {
            switch (item)
            {
                case HubRoomObject.Notebook:
                    return "계약 노트 · 강화";

                case HubRoomObject.Clock:
                    return "벽시계 · 난이도";

                case HubRoomObject.Bookshelf:
                    return "책장 · 통계";

                case HubRoomObject.Window:
                    return "창밖의 도시 · 출격";

                case HubRoomObject.Diary:
                    return "일기장 · 이야기";

                default:
                    return string.Empty;
            }
        }

        public static string GetPanelTitle(
            HubPanel panel)
        {
            switch (panel)
            {
                case HubPanel.Upgrades:
                    return "강화 · 계약 노트";

                case HubPanel.Stats:
                    return "통계 · 책장";

                case HubPanel.Difficulty:
                    return "난이도 · 벽시계";

                case HubPanel.Diary:
                    return "일기장 · 본 이야기";

                default:
                    return string.Empty;
            }
        }

        public static string GetButtonLabel(
            HubPanel panel)
        {
            switch (panel)
            {
                case HubPanel.Upgrades:
                    return "강  화";

                case HubPanel.Stats:
                    return "통  계";

                case HubPanel.Difficulty:
                    return "난이도";

                case HubPanel.Diary:
                    return "일기장";

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// 창밖 불빛 중 보라색으로 물드는 개수다. 도시 지배도만큼 물든다.
        /// </summary>
        public static int GetTintedLights(
            float dominionRatio,
            int lightCount)
        {
            if (lightCount <= 0 ||
                float.IsNaN(dominionRatio))
            {
                return 0;
            }

            double ratio = Math.Max(0f, Math.Min(1f, dominionRatio));

            return (int)Math.Round(ratio * lightCount);
        }

        /// <summary>벽시계 바늘 각도(도, 시계 방향이 음수)다. (시, 분)</summary>
        public static float GetHourAngle(
            int hour,
            int minute)
        {
            return -((hour % 12) + minute / 60f) * 30f;
        }

        public static float GetMinuteAngle(
            int minute)
        {
            return -(minute % 60) * 6f;
        }
    }
}
