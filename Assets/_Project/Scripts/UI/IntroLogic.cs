using System;

namespace ProjectTheta.UI
{
    /// <summary>첫 소개 화면 한 장이다 (35일차).</summary>
    public struct IntroPage
    {
        public string Title;
        public string Body;

        public IntroPage(
            string title,
            string body)
        {
            Title = title;
            Body = body;
        }
    }

    /// <summary>
    /// 첫 소개 화면의 내용 · 넘기기 규칙이다 (35일차).
    ///
    /// 처음부터로 새 게임을 시작하면 한 번 보여 주고, 메인 메뉴 [소개]로 다시 본다.
    /// 그림 자산이 생기면 장마다 그림을 붙일 수 있게 장 단위로 나눠 둔다.
    /// </summary>
    public static class IntroLogic
    {
        /// <summary>장이 바뀔 때 글자가 떠오르는 시간(초)이다.</summary>
        public const float FadeSeconds = 0.45f;

        public static readonly IntroPage[] Pages =
        {
            new IntroPage(
                "인간 세상에 내려온 서큐버스",
                "마계에서 쫓겨나듯 내려온 한 서큐버스.\n이 도시에서 살아남으려면 힘이 필요하다."),

            new IntroPage(
                "정기",
                "사람들의 마음에서 흘러나오는 정기.\n모은 정기는 계약 정기가 되어 나를 강하게 만든다."),

            new IntroPage(
                "최면과 동행",
                "최면으로 사람들을 동행자로 만들고,\n회수 지점까지 무사히 데려가 정기를 얻는다."),

            new IntroPage(
                "방해하는 사람들",
                "감시하는 눈, 동행자를 빼앗는 경쟁자,\n그리고 이 도시를 차지한 라이벌 서큐버스."),

            new IntroPage(
                "도시의 정점으로",
                "도시 8곳을 돌며 힘을 기르고,\n루프탑 클럽의 라이벌을 함락하자.")
        };

        public static int PageCount =>
            Pages.Length;

        public static bool IsLast(
            int page)
        {
            return page >= PageCount - 1;
        }

        /// <summary>다음 장 번호다. 마지막 장에서 넘기면 <see cref="PageCount"/>(끝)다.</summary>
        public static int Next(
            int page)
        {
            return Math.Min(
                PageCount,
                Math.Max(0, page) + 1);
        }

        public static bool IsFinished(
            int page)
        {
            return page >= PageCount;
        }

        /// <summary>"2 / 5"다.</summary>
        public static string GetCounter(
            int page)
        {
            int clamped = Math.Max(0, Math.Min(PageCount - 1, page));

            return $"{clamped + 1} / {PageCount}";
        }

        public static string GetAdvanceLabel(
            int page)
        {
            return IsLast(page)
                ? "시작하기  ▶"
                : "다음  ▶";
        }

        /// <summary>장이 바뀐 뒤 글자 투명도다(0 → 1).</summary>
        public static float GetFade(
            float elapsed)
        {
            if (float.IsNaN(elapsed) ||
                elapsed <= 0f)
            {
                return 0f;
            }

            return Math.Min(1f, elapsed / FadeSeconds);
        }
    }
}
