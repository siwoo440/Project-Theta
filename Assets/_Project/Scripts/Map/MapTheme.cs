using UnityEngine;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Map
{
    /// <summary>계단 입구 모양이다.</summary>
    public enum StairStyle
    {
        Stairs = 0,
        Deck = 1,
        Escalator = 2,
        Elevator = 3
    }

    /// <summary>바닥 무늬다.</summary>
    public enum FloorPattern
    {
        Carpet = 0,
        Sand = 1,
        Tile = 2,
        Rubber = 3,
        Paving = 4,
        Marble = 5,
        LedGrid = 6
    }

    /// <summary>
    /// 장소 하나의 맵 색 묶음이다 (27일차).
    /// 학교 복도 틀(벽 · 걸레받이 · 천장 · 바닥 · 앞 테두리)에 이 색을 입히고, 장소별 꾸미기가 그 위에 그린다.
    /// <see cref="OpenSky"/>면 벽 대신 하늘 배경을 그리고 천장이 없다.
    /// </summary>
    public sealed class MapTheme
    {
        public LocationId Location;
        public bool OpenSky;

        public Color SkyTop;
        public Color SkyBottom;
        public Color Wall;
        public Color LowerWall;
        public Color Divider;
        public Color Ceiling;
        public Color Floor;
        public Color FloorLine;
        public Color FrontBand;

        public Color StairFrame;
        public Color StairOpening;
        public Color Accent;

        public FloorPattern Pattern;
        public StairStyle Stairs;

        /// <summary>층이 오를 때마다 전체를 이만큼 어둡게 한다. 층이 달라 보이게 하는 정도다.</summary>
        public float ShadePerFloor = 0.04f;

        /// <summary>그 층의 명암 배율이다. 짝수 · 홀수 층이 번갈아 조금씩 다르다.</summary>
        public float GetShade(
            int floor)
        {
            return Mathf.Clamp(
                1f - ShadePerFloor * (floor % 2),
                0.5f,
                1f);
        }
    }

    /// <summary>장소별 맵 색 표다 (27일차).</summary>
    public static class MapThemeCatalog
    {
        private static MapTheme[] _all;

        public static MapTheme[] All
        {
            get
            {
                if (_all == null)
                {
                    _all = Create();
                }

                return _all;
            }
        }

        public static MapTheme Get(
            LocationId location)
        {
            MapTheme[] all = All;

            for (int i = 0;
                 i < all.Length;
                 i++)
            {
                if (all[i].Location == location)
                {
                    return all[i];
                }
            }

            return all[0];
        }

        private static MapTheme[] Create()
        {
            return new[]
            {
                new MapTheme
                {
                    Location = LocationId.TrainingCenter,
                    Wall = new Color(0.93f, 0.94f, 0.95f),
                    LowerWall = new Color(0.72f, 0.76f, 0.80f),
                    Divider = new Color(0.30f, 0.42f, 0.58f),
                    Ceiling = new Color(0.86f, 0.87f, 0.88f),
                    Floor = new Color(0.50f, 0.54f, 0.60f),
                    FloorLine = new Color(0.42f, 0.46f, 0.52f, 0.7f),
                    FrontBand = new Color(0.22f, 0.26f, 0.32f),
                    StairFrame = new Color(0.30f, 0.42f, 0.58f),
                    StairOpening = new Color(0.12f, 0.14f, 0.18f),
                    Accent = new Color(0.30f, 0.55f, 0.90f),
                    Pattern = FloorPattern.Carpet,
                    Stairs = StairStyle.Stairs
                },
                new MapTheme
                {
                    Location = LocationId.Beach,
                    OpenSky = true,
                    SkyTop = new Color(0.40f, 0.70f, 0.98f),
                    SkyBottom = new Color(0.78f, 0.92f, 1.00f),
                    Wall = new Color(0.20f, 0.55f, 0.80f),
                    LowerWall = new Color(0.16f, 0.48f, 0.72f),
                    Divider = new Color(0.92f, 0.97f, 1.00f),
                    Floor = new Color(0.95f, 0.86f, 0.64f),
                    FloorLine = new Color(0.86f, 0.75f, 0.52f, 0.8f),
                    FrontBand = new Color(0.25f, 0.60f, 0.85f),
                    StairFrame = new Color(0.62f, 0.45f, 0.28f),
                    StairOpening = new Color(0.48f, 0.34f, 0.20f),
                    Accent = new Color(1.00f, 0.55f, 0.35f),
                    Pattern = FloorPattern.Sand,
                    Stairs = StairStyle.Deck
                },
                new MapTheme
                {
                    Location = LocationId.SubwayStation,
                    Wall = new Color(0.90f, 0.92f, 0.92f),
                    LowerWall = new Color(0.30f, 0.62f, 0.45f),
                    Divider = new Color(0.20f, 0.45f, 0.32f),
                    Ceiling = new Color(0.55f, 0.58f, 0.60f),
                    Floor = new Color(0.62f, 0.64f, 0.66f),
                    FloorLine = new Color(0.50f, 0.52f, 0.55f, 0.8f),
                    FrontBand = new Color(0.20f, 0.22f, 0.24f),
                    StairFrame = new Color(0.40f, 0.44f, 0.48f),
                    StairOpening = new Color(0.16f, 0.18f, 0.20f),
                    Accent = new Color(0.25f, 0.75f, 0.45f),
                    Pattern = FloorPattern.Tile,
                    Stairs = StairStyle.Escalator
                },
                new MapTheme
                {
                    Location = LocationId.FitnessCenter,
                    Wall = new Color(0.62f, 0.63f, 0.64f),
                    LowerWall = new Color(0.25f, 0.26f, 0.28f),
                    Divider = new Color(0.95f, 0.45f, 0.20f),
                    Ceiling = new Color(0.30f, 0.31f, 0.33f),
                    Floor = new Color(0.20f, 0.21f, 0.23f),
                    FloorLine = new Color(0.32f, 0.33f, 0.36f, 0.8f),
                    FrontBand = new Color(0.10f, 0.10f, 0.11f),
                    StairFrame = new Color(0.95f, 0.45f, 0.20f),
                    StairOpening = new Color(0.10f, 0.10f, 0.12f),
                    Accent = new Color(0.95f, 0.45f, 0.20f),
                    Pattern = FloorPattern.Rubber,
                    Stairs = StairStyle.Stairs
                },
                new MapTheme
                {
                    Location = LocationId.NightMarket,
                    OpenSky = true,
                    SkyTop = new Color(0.05f, 0.06f, 0.16f),
                    SkyBottom = new Color(0.20f, 0.14f, 0.28f),
                    Wall = new Color(0.10f, 0.10f, 0.16f),
                    LowerWall = new Color(0.14f, 0.12f, 0.16f),
                    Divider = new Color(0.40f, 0.30f, 0.25f),
                    Floor = new Color(0.36f, 0.33f, 0.32f),
                    FloorLine = new Color(0.24f, 0.22f, 0.22f, 0.9f),
                    FrontBand = new Color(0.12f, 0.11f, 0.12f),
                    StairFrame = new Color(0.30f, 0.24f, 0.22f),
                    StairOpening = new Color(0.08f, 0.07f, 0.08f),
                    Accent = new Color(1.00f, 0.70f, 0.35f),
                    Pattern = FloorPattern.Paving,
                    Stairs = StairStyle.Stairs
                },
                new MapTheme
                {
                    Location = LocationId.ShoppingMall,
                    Wall = new Color(0.96f, 0.93f, 0.88f),
                    LowerWall = new Color(0.82f, 0.76f, 0.68f),
                    Divider = new Color(0.72f, 0.58f, 0.36f),
                    Ceiling = new Color(0.98f, 0.97f, 0.95f),
                    Floor = new Color(0.88f, 0.86f, 0.84f),
                    FloorLine = new Color(0.76f, 0.73f, 0.70f, 0.7f),
                    FrontBand = new Color(0.55f, 0.48f, 0.40f),
                    StairFrame = new Color(0.72f, 0.58f, 0.36f),
                    StairOpening = new Color(0.30f, 0.30f, 0.32f),
                    Accent = new Color(0.85f, 0.35f, 0.55f),
                    Pattern = FloorPattern.Marble,
                    Stairs = StairStyle.Escalator
                },
                new MapTheme
                {
                    Location = LocationId.OfficeTower,
                    Wall = new Color(0.30f, 0.33f, 0.40f),
                    LowerWall = new Color(0.55f, 0.58f, 0.62f),
                    Divider = new Color(0.20f, 0.22f, 0.28f),
                    Ceiling = new Color(0.40f, 0.42f, 0.46f),
                    Floor = new Color(0.22f, 0.26f, 0.36f),
                    FloorLine = new Color(0.18f, 0.21f, 0.30f, 0.9f),
                    FrontBand = new Color(0.12f, 0.13f, 0.18f),
                    StairFrame = new Color(0.62f, 0.64f, 0.68f),
                    StairOpening = new Color(0.40f, 0.43f, 0.48f),
                    Accent = new Color(0.55f, 0.75f, 1.00f),
                    Pattern = FloorPattern.Carpet,
                    Stairs = StairStyle.Elevator
                },
                new MapTheme
                {
                    Location = LocationId.RooftopClub,
                    OpenSky = true,
                    SkyTop = new Color(0.04f, 0.02f, 0.12f),
                    SkyBottom = new Color(0.28f, 0.10f, 0.35f),
                    Wall = new Color(0.08f, 0.06f, 0.14f),
                    LowerWall = new Color(0.12f, 0.10f, 0.18f),
                    Divider = new Color(0.90f, 0.40f, 1.00f),
                    Floor = new Color(0.12f, 0.10f, 0.16f),
                    FloorLine = new Color(0.35f, 0.20f, 0.45f, 0.8f),
                    FrontBand = new Color(0.06f, 0.05f, 0.08f),
                    StairFrame = new Color(0.90f, 0.40f, 1.00f),
                    StairOpening = new Color(0.06f, 0.04f, 0.10f),
                    Accent = new Color(0.30f, 0.90f, 1.00f),
                    Pattern = FloorPattern.LedGrid,
                    Stairs = StairStyle.Stairs
                }
            };
        }
    }
}
