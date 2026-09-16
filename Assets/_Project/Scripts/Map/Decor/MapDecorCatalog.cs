using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Map.Decor
{
    /// <summary>장소별 꾸미기를 고른다 (27일차).</summary>
    public static class MapDecorCatalog
    {
        public static void Draw(
            LocationId location,
            MapPainter painter)
        {
            switch (location)
            {
                case LocationId.Beach:
                    BeachDecor.Draw(painter);
                    break;

                case LocationId.SubwayStation:
                    SubwayDecor.Draw(painter);
                    break;

                case LocationId.FitnessCenter:
                    FitnessDecor.Draw(painter);
                    break;

                case LocationId.NightMarket:
                    NightMarketDecor.Draw(painter);
                    break;

                case LocationId.ShoppingMall:
                    MallDecor.Draw(painter);
                    break;

                case LocationId.OfficeTower:
                    OfficeDecor.Draw(painter);
                    break;

                case LocationId.RooftopClub:
                    ClubDecor.Draw(painter);
                    break;

                default:
                    TrainingCenterDecor.Draw(painter);
                    break;
            }
        }

        /// <summary>그 장소 꾸미기가 벽 아래쪽에 세우는 물건 자리다. 비워 둔 자리 검사에 쓴다.</summary>
        public static MapFeature[] GetLowWallFeatures(
            LocationId location)
        {
            switch (location)
            {
                case LocationId.Beach:
                    return BeachDecor.LowWall;

                case LocationId.SubwayStation:
                    return SubwayDecor.LowWall;

                case LocationId.FitnessCenter:
                    return FitnessDecor.LowWall;

                case LocationId.NightMarket:
                    return NightMarketDecor.LowWall;

                case LocationId.ShoppingMall:
                    return MallDecor.LowWall;

                case LocationId.OfficeTower:
                    return OfficeDecor.LowWall;

                case LocationId.RooftopClub:
                    return ClubDecor.LowWall;

                default:
                    return TrainingCenterDecor.LowWall;
            }
        }
    }
}
