using UnityEngine;

namespace ProjectTheta.UI.Framework
{
    /// <summary>
    /// 한글이 나오는 폰트를 확보한다.
    ///
    /// 프로젝트에 폰트 자산을 넣지 않았고, 유니티 기본 내장 폰트(Arial 계열)에는
    /// 한글 글리프가 없어 네모(두부)로 보인다. 그래서 OS에 설치된 한글 폰트를
    /// 런타임에 빌려 쓴다. 후보를 모두 못 찾으면 내장 폰트로 떨어진다.
    /// </summary>
    public static class UiFontProvider
    {
        /// <summary>앞에 있는 것부터 먼저 찾는다.</summary>
        private static readonly string[] PreferredFonts =
        {
            "Malgun Gothic",
            "맑은 고딕",
            "NanumGothic",
            "나눔고딕",
            "Noto Sans KR",
            "Noto Sans CJK KR",
            "AppleSDGothicNeo-Regular",
            "Apple SD Gothic Neo",
            "Gulim",
            "굴림",
            "Dotum",
            "돋움",
            "Batang",
            "바탕"
        };

        private static Font _cached;
        private static bool _resolved;

        /// <summary>확보한 폰트 이름이다. 진단용으로만 쓴다.</summary>
        public static string ResolvedName { get; private set; } = string.Empty;

        public static Font Get()
        {
            if (_resolved &&
                _cached != null)
            {
                return _cached;
            }

            _resolved = true;

            string[] installed =
                Font.GetOSInstalledFontNames();

            for (int i = 0;
                 i < PreferredFonts.Length;
                 i++)
            {
                if (!Contains(
                        installed,
                        PreferredFonts[i]))
                {
                    continue;
                }

                Font created =
                    Font.CreateDynamicFontFromOSFont(
                        PreferredFonts[i],
                        UiTheme.FontBody);

                if (created == null)
                {
                    continue;
                }

                _cached = created;

                ResolvedName =
                    PreferredFonts[i];

                return _cached;
            }

            // 후보를 못 찾았으면 내장 폰트라도 돌려준다. 한글은 깨지지만 화면은 뜬다.
            _cached =
                Resources.GetBuiltinResource<Font>(
                    "LegacyRuntime.ttf");

            if (_cached == null)
            {
                _cached =
                    Resources.GetBuiltinResource<Font>(
                        "Arial.ttf");
            }

            ResolvedName =
                _cached == null
                    ? string.Empty
                    : _cached.name;

            return _cached;
        }

        private static bool Contains(
            string[] names,
            string target)
        {
            if (names == null)
            {
                return false;
            }

            for (int i = 0;
                 i < names.Length;
                 i++)
            {
                if (string.Equals(
                        names[i],
                        target,
                        System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
