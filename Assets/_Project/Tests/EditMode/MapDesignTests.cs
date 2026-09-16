using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Map;
using ProjectTheta.Map.Decor;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class MapThemeTests
    {
        [Test]
        public void Every_Location_Has_Its_Own_Theme()
        {
            HashSet<LocationId> seen = new HashSet<LocationId>();

            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                Assert.IsTrue(seen.Add(theme.Location), $"{theme.Location} 테마가 두 번 있습니다");
            }

            foreach (LocationDefinition location in LocationCatalog.All)
            {
                Assert.AreEqual(location.Id, MapThemeCatalog.Get(location.Id).Location);
            }
        }

        [Test]
        public void Wall_Boundary_Is_Visible()
        {
            // 벽(하늘 장소는 바다 · 난간 띠)과 경계선이 구분돼야 바닥선이 읽힌다.
            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                float gap =
                    MapLayoutRules.LuminanceGap(
                        theme.Wall.r, theme.Wall.g, theme.Wall.b,
                        theme.Divider.r, theme.Divider.g, theme.Divider.b);

                Assert.GreaterOrEqual(gap, 0.08f, theme.Location.ToString());
            }
        }

        [Test]
        public void Floors_Are_Neither_Pure_Black_Nor_Pure_White()
        {
            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                float luminance = MapLayoutRules.Luminance(theme.Floor.r, theme.Floor.g, theme.Floor.b);

                Assert.Greater(luminance, 0.08f, theme.Location.ToString());
                Assert.Less(luminance, 0.95f, theme.Location.ToString());
            }
        }

        [Test]
        public void Outdoor_And_Night_Places_Use_Open_Sky()
        {
            Assert.IsTrue(MapThemeCatalog.Get(LocationId.Beach).OpenSky);
            Assert.IsTrue(MapThemeCatalog.Get(LocationId.NightMarket).OpenSky);
            Assert.IsTrue(MapThemeCatalog.Get(LocationId.RooftopClub).OpenSky);
            Assert.IsFalse(MapThemeCatalog.Get(LocationId.TrainingCenter).OpenSky);
            Assert.IsFalse(MapThemeCatalog.Get(LocationId.OfficeTower).OpenSky);
        }

        [Test]
        public void Stair_Styles_Match_The_Places()
        {
            Assert.AreEqual(StairStyle.Elevator, MapThemeCatalog.Get(LocationId.OfficeTower).Stairs);
            Assert.AreEqual(StairStyle.Escalator, MapThemeCatalog.Get(LocationId.SubwayStation).Stairs);
            Assert.AreEqual(StairStyle.Escalator, MapThemeCatalog.Get(LocationId.ShoppingMall).Stairs);
            Assert.AreEqual(StairStyle.Deck, MapThemeCatalog.Get(LocationId.Beach).Stairs);
        }

        [Test]
        public void Floor_Shade_Stays_In_Range()
        {
            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                for (int floor = 0; floor < 6; floor++)
                {
                    float shade = theme.GetShade(floor);

                    Assert.GreaterOrEqual(shade, 0.5f);
                    Assert.LessOrEqual(shade, 1f);
                }
            }
        }
    }

    public sealed class MapLayoutRulesTests
    {
        [Test]
        public void Decor_Never_Covers_Stairs_Recovery_Or_Rule_Props()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                foreach (MapFeature feature in MapDecorCatalog.GetLowWallFeatures(location.Id))
                {
                    Assert.IsTrue(
                        MapLayoutRules.IsAllowed(location.Id, feature),
                        $"{location.Id}: x={feature.X} ±{feature.HalfWidth} 꾸미기가 비워 둔 자리와 겹칩니다");
                }
            }
        }

        [Test]
        public void Decor_Features_Do_Not_Overlap_Each_Other()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                MapFeature[] features = MapDecorCatalog.GetLowWallFeatures(location.Id);

                for (int i = 0; i < features.Length; i++)
                {
                    for (int j = i + 1; j < features.Length; j++)
                    {
                        Assert.IsFalse(
                            MapLayoutRules.Overlaps(features[i], features[j]),
                            $"{location.Id}: {features[i].X} · {features[j].X}");
                    }
                }
            }
        }

        [Test]
        public void Reserved_Zones_Always_Include_Stairs_And_Recovery()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                List<MapFeature> reserved = MapLayoutRules.GetReserved(location.Id);

                Assert.IsTrue(reserved.Exists(r => r.X == FloorLayout.UpStairX), location.Id.ToString());
                Assert.IsTrue(reserved.Exists(r => r.X == FloorLayout.DownStairX), location.Id.ToString());
                Assert.IsTrue(reserved.Exists(r => r.X == FloorLayout.RecoveryX), location.Id.ToString());
            }
        }

        [Test]
        public void Overlap_And_Bounds_Rules()
        {
            Assert.IsTrue(MapLayoutRules.Overlaps(new MapFeature(0f, 1f), new MapFeature(1.5f, 1f)));
            Assert.IsFalse(MapLayoutRules.Overlaps(new MapFeature(0f, 1f), new MapFeature(2f, 1f)));

            Assert.IsFalse(MapLayoutRules.IsAllowed(LocationId.TrainingCenter, new MapFeature(FloorSpace.WalkMaxX, 0.5f)));
            Assert.IsFalse(MapLayoutRules.IsAllowed(LocationId.TrainingCenter, new MapFeature(FloorLayout.UpStairX, 0.2f)));
            Assert.IsTrue(MapLayoutRules.IsAllowed(LocationId.TrainingCenter, new MapFeature(0f, 0.5f)));
        }

        [Test]
        public void Colliders_Keep_The_Old_School_Spots()
        {
            // 학교 복도의 벤치 · 자판기와 같은 자리여야 이동 판정이 장소마다 같다.
            Assert.AreEqual(5.2f, MapBaseLayers.BenchX);
            Assert.AreEqual(-0.95f, MapBaseLayers.BenchY);
            Assert.AreEqual(-10.2f, MapBaseLayers.VendingX);
            Assert.AreEqual(0.12f, MapBaseLayers.VendingY);
        }

        [Test]
        public void Map_Base_Matches_The_Walk_Area()
        {
            Assert.GreaterOrEqual(MapBaseLayers.FloorTop, FloorSpace.WalkMaxY - 0.25f);
            Assert.Less(MapBaseLayers.FloorBottom, FloorSpace.WalkMinY);
            Assert.GreaterOrEqual(MapBaseLayers.Width * 0.5f, FloorSpace.WalkMaxX);
        }
    }

    public sealed class MapArtLibraryTests
    {
        [Test]
        public void Art_Paths_Follow_The_Folder_Rule()
        {
            Assert.AreEqual("Maps/Beach/Background", MapArtLibrary.GetPath(LocationId.Beach, MapArtLayer.Background));
            Assert.AreEqual("Maps/OfficeTower/Floor", MapArtLibrary.GetPath(LocationId.OfficeTower, MapArtLayer.Floor));
        }

        [Test]
        public void Hash_Is_Stable_And_In_Range()
        {
            Assert.AreEqual(MapPainter.Hash(3, 7), MapPainter.Hash(3, 7));

            for (int i = 0; i < 50; i++)
            {
                float value = MapPainter.Hash(i, i * 3);

                Assert.GreaterOrEqual(value, 0f);
                Assert.Less(value, 1f);
            }
        }

        [Test]
        public void Character_Order_Puts_Lower_Things_In_Front()
        {
            Assert.Greater(MapPainter.CharacterOrder(-2f), MapPainter.CharacterOrder(0f));
        }
    }
}
