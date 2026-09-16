using System;
using System.Collections.Generic;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Map
{
    /// <summary>벽 아래쪽(바닥선 근처)에 서는 꾸미기 한 개의 가로 자리다.</summary>
    public struct MapFeature
    {
        public float X;
        public float HalfWidth;

        public MapFeature(
            float x,
            float halfWidth)
        {
            X = x;
            HalfWidth = halfWidth;
        }

        public float MinX => X - HalfWidth;
        public float MaxX => X + HalfWidth;
    }

    /// <summary>
    /// 맵 꾸미기가 비워 둬야 하는 자리다 (27일차).
    ///
    /// 계단 · 회수 지점 · 장소 규칙 소품(노점 · 망루 · 셔터 · 게이트 · 탕비실 · DJ 부스 …) 앞에는
    /// 벽 아래쪽 꾸미기를 세우지 않는다. 겹치면 판정 소품이 가려져 읽기 어렵다.
    /// 벽 위쪽(창 · 간판)과 바닥 무늬는 제한하지 않는다.
    /// </summary>
    public static class MapLayoutRules
    {
        public const float StairHalfWidth = 1.0f;
        public const float RecoveryHalfWidth = 1.2f;

        public static List<MapFeature> GetReserved(
            LocationId location)
        {
            List<MapFeature> result = new List<MapFeature>
            {
                new MapFeature(FloorLayout.DownStairX, StairHalfWidth),
                new MapFeature(FloorLayout.UpStairX, StairHalfWidth),
                new MapFeature(FloorLayout.RecoveryX, RecoveryHalfWidth)
            };

            switch (location)
            {
                case LocationId.Beach:
                    result.Add(new MapFeature(DisruptorCatalog.BeachTowerX, 1.2f));
                    break;

                case LocationId.NightMarket:
                    foreach (float x in DisruptorCatalog.NightMarketStallX)
                    {
                        result.Add(new MapFeature(x, 1.6f));
                    }
                    break;

                case LocationId.SubwayStation:
                    result.Add(new MapFeature(DisruptorCatalog.SubwayGateX, 0.6f));
                    break;

                case LocationId.ShoppingMall:
                    foreach (float x in MallLayout.ShutterX)
                    {
                        result.Add(new MapFeature(x, 0.8f));
                    }

                    result.Add(new MapFeature(MallLayout.SecurityRoomX, 1.2f));
                    break;

                case LocationId.OfficeTower:
                    foreach (float x in OfficeLayout.PantryX)
                    {
                        result.Add(new MapFeature(x, 1.3f));
                    }

                    foreach (float x in OfficeLayout.MeetingRoomX)
                    {
                        result.Add(new MapFeature(x, Disruptors.MeetingLogic.RoomHalfWidth + 0.1f));
                    }

                    result.Add(new MapFeature(OfficeLayout.GateX, 0.6f));
                    break;

                case LocationId.RooftopClub:
                    result.Add(new MapFeature(ClubLayout.DjBoothX, 1.3f));
                    result.Add(new MapFeature(ClubLayout.BarX, 1.6f));
                    result.Add(new MapFeature(ClubLayout.EntranceBarX, 1.6f));
                    break;
            }

            return result;
        }

        public static bool Overlaps(
            MapFeature a,
            MapFeature b)
        {
            return a.MinX < b.MaxX &&
                   b.MinX < a.MaxX;
        }

        /// <summary>꾸미기가 걷는 폭 안에 있고, 비워 둔 자리와 겹치지 않는지다.</summary>
        public static bool IsAllowed(
            LocationId location,
            MapFeature feature)
        {
            if (feature.MinX < FloorSpace.WalkMinX ||
                feature.MaxX > FloorSpace.WalkMaxX)
            {
                return false;
            }

            List<MapFeature> reserved = GetReserved(location);

            for (int i = 0;
                 i < reserved.Count;
                 i++)
            {
                if (Overlaps(feature, reserved[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 색 두 개의 밝기 차다 (0~1). 바닥과 캐릭터가 섞이지 않게, 벽과 바닥이 구분되게 검사하는 데 쓴다.
        /// </summary>
        public static float LuminanceGap(
            float r1,
            float g1,
            float b1,
            float r2,
            float g2,
            float b2)
        {
            return Math.Abs(Luminance(r1, g1, b1) - Luminance(r2, g2, b2));
        }

        public static float Luminance(
            float r,
            float g,
            float b)
        {
            return 0.2126f * r + 0.7152f * g + 0.0722f * b;
        }
    }
}
