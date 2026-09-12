using System;

namespace ProjectTheta.Presentation
{
    /// <summary>캐릭터가 속한 표현 계층이다. 정렬 기준을 계층별로 분리한다.</summary>
    public enum CharacterLayer
    {
        /// <summary>배경 장식. 항상 뒤.</summary>
        Background = 0,

        /// <summary>일반 캐릭터. 깊이(Y)로 서로 앞뒤가 갈린다.</summary>
        Character = 1,

        /// <summary>NPC 머리 위 게이지·아이콘.</summary>
        CharacterOverlay = 2,

        /// <summary>화면 고정 UI.</summary>
        ScreenUi = 3
    }

    /// <summary>
    /// 2.5D 사이드뷰에서 스프라이트 앞뒤 순서를 결정한다.
    ///
    /// 기획서 26.3절: "SpriteRenderer 정렬 순서와 Y/Depth 값을 연동해 앞뒤 관계를 표현한다."
    /// 계층마다 정렬 구간을 나눠 두어, 캐릭터가 아무리 아래에 있어도
    /// 머리 위 UI나 화면 UI를 가리지 않는다.
    /// </summary>
    public static class CharacterSortingLogic
    {
        /// <summary>한 계층이 쓰는 정렬 값 폭이다.</summary>
        public const int LayerBand = 10000;

        /// <summary>Y 좌표 1당 정렬 값 변화량이다. 값이 클수록 미세한 깊이 차이를 구분한다.</summary>
        public const float DepthScale = 100f;

        /// <summary>계층 안에서 허용하는 최대 깊이 변위다.</summary>
        public const int DepthRange = 4000;

        public static int GetLayerBase(
            CharacterLayer layer)
        {
            return (int)layer *
                   LayerBand;
        }

        /// <summary>
        /// 깊이(보통 Y 좌표)에서 정렬 순서를 만든다.
        /// 아래에 있을수록(Y가 작을수록) 앞에 그려진다.
        /// </summary>
        public static int GetSortingOrder(
            CharacterLayer layer,
            float depth)
        {
            double raw =
                -depth *
                DepthScale;

            int clamped =
                (int)Math.Round(
                    Math.Max(
                        -DepthRange,
                        Math.Min(
                            DepthRange,
                            raw)),
                    MidpointRounding.AwayFromZero);

            return GetLayerBase(
                       layer) +
                   clamped;
        }

        /// <summary>같은 계층에서 두 깊이의 앞뒤를 비교한다. 양수면 a가 앞이다.</summary>
        public static int CompareDepth(
            float a,
            float b)
        {
            return GetSortingOrder(
                       CharacterLayer.Character,
                       a)
                .CompareTo(
                    GetSortingOrder(
                        CharacterLayer.Character,
                        b));
        }
    }
}
