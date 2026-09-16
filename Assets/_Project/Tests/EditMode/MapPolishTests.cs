using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Map;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class MapPropTintTests
    {
        [Test]
        public void Without_Theme_Color_Is_Unchanged()
        {
            Color color = new Color(0.3f, 0.4f, 0.5f, 0.7f);

            Assert.AreEqual(color, MapPropTint.Apply(color, null));
        }

        [Test]
        public void Structures_Stand_Out_From_Every_Floor()
        {
            Color[] samples =
            {
                new Color(0.35f, 0.24f, 0.18f),
                new Color(0.55f, 0.60f, 0.65f),
                new Color(0.25f, 0.25f, 0.30f),
                new Color(0.30f, 0.15f, 0.35f),
                new Color(0.85f, 0.70f, 0.50f)
            };

            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                float floor = MapLayoutRules.Luminance(theme.Floor.r, theme.Floor.g, theme.Floor.b);

                foreach (Color sample in samples)
                {
                    Color tinted = MapPropTint.Apply(sample, theme);
                    float gap = System.Math.Abs(MapLayoutRules.Luminance(tinted.r, tinted.g, tinted.b) - floor);

                    Assert.GreaterOrEqual(gap, MapPropTint.MinFloorGap - 0.001f, $"{theme.Location} {sample}");
                    Assert.GreaterOrEqual(tinted.r, 0f);
                    Assert.LessOrEqual(tinted.r, 1f);
                }
            }
        }

        [Test]
        public void Tint_Leans_Toward_The_Place_Accent_And_Keeps_Alpha()
        {
            MapTheme club = MapThemeCatalog.Get(LocationId.RooftopClub);
            Color gray = new Color(0.5f, 0.5f, 0.5f, 0.4f);
            Color tinted = MapPropTint.Apply(gray, club);

            Assert.AreEqual(0.4f, tinted.a, 0.0001f);

            // 밝기 보정 전에 강조색 쪽으로 섞였는지 색 방향으로 확인한다.
            float accentLean =
                (tinted.r - tinted.g) * (club.Accent.r - club.Accent.g) +
                (tinted.b - tinted.g) * (club.Accent.b - club.Accent.g);

            Assert.Greater(accentLean, 0f);
        }
    }

    public sealed class FloorPatternLayoutTests
    {
        [Test]
        public void Every_Place_Has_A_Floor_Pattern_Inside_The_Area()
        {
            foreach (MapTheme theme in MapThemeCatalog.All)
            {
                List<PatternRect> rects = FloorPatternLayout.Build(theme, 0);

                Assert.Greater(rects.Count, 0, theme.Location.ToString());

                foreach (PatternRect rect in rects)
                {
                    // 굽는 영역 밖으로 나간 부분은 잘리므로, 영역과 겹치기만 하면 된다.
                    Assert.Greater(rect.Y + rect.Height * 0.5f, FloorPatternLayout.AreaMinY, theme.Location.ToString());
                    Assert.Less(rect.Y - rect.Height * 0.5f, FloorPatternLayout.AreaMaxY, theme.Location.ToString());
                    Assert.Greater(rect.X + rect.Width * 0.5f, FloorPatternLayout.AreaMinX, theme.Location.ToString());
                    Assert.Less(rect.X - rect.Width * 0.5f, FloorPatternLayout.AreaMaxX, theme.Location.ToString());
                    Assert.Greater(rect.Width, 0f);
                    Assert.Greater(rect.Height, 0f);
                }
            }
        }

        [Test]
        public void Pattern_Is_Stable_For_The_Same_Floor()
        {
            MapTheme beach = MapThemeCatalog.Get(LocationId.Beach);

            List<PatternRect> a = FloorPatternLayout.Build(beach, 1);
            List<PatternRect> b = FloorPatternLayout.Build(beach, 1);

            Assert.AreEqual(a.Count, b.Count);

            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a[i].X, b[i].X);
                Assert.AreEqual(a[i].Y, b[i].Y);
            }
        }

        [Test]
        public void Only_Scattered_Patterns_Change_Per_Floor()
        {
            Assert.IsTrue(FloorPatternLayout.DependsOnFloor(FloorPattern.Sand));
            Assert.IsTrue(FloorPatternLayout.DependsOnFloor(FloorPattern.Rubber));
            Assert.IsFalse(FloorPatternLayout.DependsOnFloor(FloorPattern.Tile));
            Assert.IsFalse(FloorPatternLayout.DependsOnFloor(FloorPattern.LedGrid));

            Assert.AreEqual(
                FloorPatternBaker.GetKey(LocationId.OfficeTower, FloorPattern.Carpet, 0),
                FloorPatternBaker.GetKey(LocationId.OfficeTower, FloorPattern.Carpet, 2));

            Assert.AreNotEqual(
                FloorPatternBaker.GetKey(LocationId.Beach, FloorPattern.Sand, 0),
                FloorPatternBaker.GetKey(LocationId.Beach, FloorPattern.Sand, 1));
        }

        [Test]
        public void Subway_Bakes_The_Tactile_Strip()
        {
            MapTheme subway = MapThemeCatalog.Get(LocationId.SubwayStation);
            MapTheme office = MapThemeCatalog.Get(LocationId.OfficeTower);

            Assert.IsTrue(FloorPatternLayout.Build(subway, 0).Exists(r => r.Y == FloorPatternLayout.TactileY));
            Assert.IsFalse(FloorPatternLayout.Build(office, 0).Exists(r => r.Y == FloorPatternLayout.TactileY));
        }

        [Test]
        public void Baked_Texture_Covers_The_Area_At_A_Sane_Size()
        {
            Assert.AreEqual(
                MapBaseLayers.Width,
                FloorPatternBaker.TextureWidth / (float)FloorPatternBaker.PixelsPerUnit,
                0.05f);

            Assert.LessOrEqual(FloorPatternBaker.TextureWidth, 2048);
            Assert.LessOrEqual(FloorPatternBaker.TextureHeight, 512);
        }

        [Test]
        public void Budget_Rule()
        {
            Assert.IsFalse(MapBudget.IsOver(MapBudget.MaxObjectsPerFloor));
            Assert.IsTrue(MapBudget.IsOver(MapBudget.MaxObjectsPerFloor + 1));
        }
    }

    /// <summary>실제로 맵을 지어 세는 테스트다. Unity 안에서만 돈다.</summary>
    public sealed class MapBudgetBuildTests
    {
        [TearDown]
        public void TearDown()
        {
            GameObject root = GameObject.Find(SchoolHallwayPrototypeBuilder.RootName);

            if (root != null)
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void Every_Place_Stays_Under_The_Object_Budget()
        {
            foreach (LocationDefinition location in LocationCatalog.All)
            {
                SchoolHallwayPrototypeBuilder.Build(location.FloorCount, location.Id);

                Assert.LessOrEqual(
                    SchoolHallwayPrototypeBuilder.MaxThemedFloorObjectCount,
                    MapBudget.MaxObjectsPerFloor,
                    location.Id.ToString());

                TearDown();
            }
        }
    }
}
