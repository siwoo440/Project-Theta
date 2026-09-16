using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Run;

namespace ProjectTheta.Tests.EditMode
{
    /// <summary>
    /// 실제 밸런스 자산 파일이 제대로 읽히는지 검사한다.
    ///
    /// 14일차에 자산의 배열 값이 잘못된 형식으로 기록되어 있었는데,
    /// 로직 테스트는 자산을 읽지 않으므로 아무도 잡지 못했다.
    /// 이 테스트는 Unity가 자산을 불러와야 하므로 Test Runner에서만 돈다.
    /// </summary>
    public sealed class BalanceAssetConsistencyTests
    {
        private static StageBalanceValues LoadStage()
        {
            StageBalanceDatabase database =
                Resources.Load<StageBalanceDatabase>(
                    BalanceBootstrap.StageBalancePath);

            Assert.IsNotNull(
                database,
                "Resources/Balance/StageBalanceDatabase.asset을 찾지 못했습니다");

            return database.StageValues;
        }

        [Test]
        public void Stage_Asset_Arrays_Match_Code_Defaults()
        {
            StageBalanceValues asset =
                LoadStage();

            StageBalanceValues defaults =
                new StageBalanceValues();

            AssertArray(
                defaults.SimultaneousMultipliers,
                asset.SimultaneousMultipliers,
                nameof(StageBalanceValues.SimultaneousMultipliers));

            AssertArray(
                defaults.ChainSpeedMultipliers,
                asset.ChainSpeedMultipliers,
                nameof(StageBalanceValues.ChainSpeedMultipliers));

            AssertArray(
                defaults.ChainFocusCosts,
                asset.ChainFocusCosts,
                nameof(StageBalanceValues.ChainFocusCosts));
        }

        [Test]
        public void Stage_Asset_Run_Xp_Values_Match_Code_Defaults()
        {
            // 17일차에 추가한 런 경험치 · 레벨 곡선 값이다.
            StageBalanceValues asset =
                LoadStage();

            StageBalanceValues d =
                new StageBalanceValues();

            Assert.AreEqual(d.XpHypnosisSuccess, asset.XpHypnosisSuccess, nameof(d.XpHypnosisSuccess));
            Assert.AreEqual(d.XpReclaimBonus, asset.XpReclaimBonus, nameof(d.XpReclaimBonus));
            Assert.AreEqual(d.XpRecoveryPerFollower, asset.XpRecoveryPerFollower, nameof(d.XpRecoveryPerFollower));
            Assert.AreEqual(d.XpBatchBonusPerExtra, asset.XpBatchBonusPerExtra, nameof(d.XpBatchBonusPerExtra));
            Assert.AreEqual(d.XpRiskyRecovery, asset.XpRiskyRecovery, nameof(d.XpRiskyRecovery));
            Assert.AreEqual(d.XpHighGradeRecovery, asset.XpHighGradeRecovery, nameof(d.XpHighGradeRecovery));
            Assert.AreEqual(d.XpDuelWin, asset.XpDuelWin, nameof(d.XpDuelWin));
            Assert.AreEqual(d.XpFloorFirstVisit, asset.XpFloorFirstVisit, nameof(d.XpFloorFirstVisit));
            Assert.AreEqual(d.XpRampageSurvived, asset.XpRampageSurvived, nameof(d.XpRampageSurvived));
            Assert.AreEqual(d.LevelBaseXp, asset.LevelBaseXp, nameof(d.LevelBaseXp));
            Assert.AreEqual(d.LevelXpGrowth, asset.LevelXpGrowth, nameof(d.LevelXpGrowth));
            Assert.AreEqual(d.LevelMaximum, asset.LevelMaximum, nameof(d.LevelMaximum));
        }

        [Test]
        public void Stage_Asset_Run_Upgrade_Cards_Match_Code_Defaults()
        {
            // 18일차에 카드 수치를 자산으로 옮겼다. 자산이 비면 코드 기본표를 쓰지만,
            // 자산에 적힌 값은 코드 기본표와 같아야 한다 (조정 전 기준선).
            StageBalanceDatabase database =
                Resources.Load<StageBalanceDatabase>(
                    BalanceBootstrap.StageBalancePath);

            RunUpgradeProfile[] asset =
                database.RunUpgrades;

            RunUpgradeProfile[] expected =
                RunUpgradeTable.Profiles;

            Assert.IsNotNull(asset, "_runUpgrades가 비어 있습니다");
            Assert.AreEqual(expected.Length, asset.Length, "카드 수가 다릅니다");

            for (int i = 0; i < expected.Length; i++)
            {
                string at = $"_runUpgrades[{i}]";

                Assert.AreEqual(expected[i].Card, asset[i].Card, at + ".Card");
                Assert.AreEqual(expected[i].Category, asset[i].Category, at + ".Category");
                Assert.AreEqual(expected[i].DisplayName, asset[i].DisplayName, at + ".DisplayName");
                Assert.AreEqual(expected[i].DescriptionFormat, asset[i].DescriptionFormat, at + ".DescriptionFormat");
                Assert.AreEqual(expected[i].ValuePerStack, asset[i].ValuePerStack, 0.0001f, at + ".ValuePerStack");
                Assert.AreEqual(expected[i].MaximumStacks, asset[i].MaximumStacks, at + ".MaximumStacks");
                Assert.AreEqual(expected[i].IsInteger, asset[i].IsInteger, at + ".IsInteger");
            }
        }

        [Test]
        public void Stage_Asset_Vfx_Tuning_Matches_Code_Defaults()
        {
            // 19일차 연출 수치다.
            StageBalanceValues asset =
                LoadStage();

            StageBalanceValues d =
                new StageBalanceValues();

            Assert.AreEqual(d.VfxShakeSmall, asset.VfxShakeSmall, 0.0001f, nameof(d.VfxShakeSmall));
            Assert.AreEqual(d.VfxShakeMedium, asset.VfxShakeMedium, 0.0001f, nameof(d.VfxShakeMedium));
            Assert.AreEqual(d.VfxShakeLarge, asset.VfxShakeLarge, 0.0001f, nameof(d.VfxShakeLarge));
            Assert.AreEqual(d.VfxShakeSeconds, asset.VfxShakeSeconds, 0.0001f, nameof(d.VfxShakeSeconds));
            Assert.AreEqual(d.VfxHitStopSeconds, asset.VfxHitStopSeconds, 0.0001f, nameof(d.VfxHitStopSeconds));
            Assert.AreEqual(d.VfxHitStopScale, asset.VfxHitStopScale, 0.0001f, nameof(d.VfxHitStopScale));
            Assert.AreEqual(d.VfxRippleRadius, asset.VfxRippleRadius, 0.0001f, nameof(d.VfxRippleRadius));
            Assert.AreEqual(d.VfxFadeSeconds, asset.VfxFadeSeconds, 0.0001f, nameof(d.VfxFadeSeconds));
            Assert.AreEqual(d.FocusHypnosisSpeedBonus, asset.FocusHypnosisSpeedBonus, 0.0001f, nameof(d.FocusHypnosisSpeedBonus));
            Assert.AreEqual(d.PlayerHypnosisSpeedScale, asset.PlayerHypnosisSpeedScale, 0.0001f, nameof(d.PlayerHypnosisSpeedScale));
            Assert.AreEqual(d.FocusHypnosisDrainPerSecond, asset.FocusHypnosisDrainPerSecond, 0.0001f, nameof(d.FocusHypnosisDrainPerSecond));
            Assert.AreEqual(d.FocusRecoveryPerSecond, asset.FocusRecoveryPerSecond, 0.0001f, nameof(d.FocusRecoveryPerSecond));
            Assert.AreEqual(d.ImpulseBuildScale, asset.ImpulseBuildScale, 0.0001f, nameof(d.ImpulseBuildScale));
            Assert.AreEqual(d.AlertRisePerSecond, asset.AlertRisePerSecond, 0.0001f, nameof(d.AlertRisePerSecond));
            Assert.AreEqual(d.AlertDashRise, asset.AlertDashRise, 0.0001f, nameof(d.AlertDashRise));
            Assert.AreEqual(d.AlertDecayPerSecond, asset.AlertDecayPerSecond, 0.0001f, nameof(d.AlertDecayPerSecond));
            Assert.AreEqual(d.WatcherSightScale, asset.WatcherSightScale, 0.0001f, nameof(d.WatcherSightScale));
            Assert.AreEqual(d.DarkHypnosisRangeScale, asset.DarkHypnosisRangeScale, 0.0001f, nameof(d.DarkHypnosisRangeScale));
            Assert.AreEqual(d.ContestRiseScale, asset.ContestRiseScale, 0.0001f, nameof(d.ContestRiseScale));
            Assert.AreEqual(d.HeartRateImpulseScale, asset.HeartRateImpulseScale, 0.0001f, nameof(d.HeartRateImpulseScale));
            Assert.AreEqual(d.SurvivalTrainCount, asset.SurvivalTrainCount, nameof(d.SurvivalTrainCount));
            Assert.AreEqual(d.DisruptorRampSeconds, asset.DisruptorRampSeconds, 0.0001f, nameof(d.DisruptorRampSeconds));
        }

        [Test]
        public void Stage_Asset_Arrays_Are_Ordered_Sensibly()
        {
            // 자산을 고치다 순서를 뒤집어도 잡히도록, 기본값과 무관한 규칙도 확인한다.
            StageBalanceValues asset =
                LoadStage();

            for (int i = 1;
                 i < asset.SimultaneousMultipliers.Length;
                 i++)
            {
                Assert.GreaterOrEqual(
                    asset.SimultaneousMultipliers[i],
                    asset.SimultaneousMultipliers[i - 1],
                    "동시 회수 배율은 인원이 늘수록 줄어들면 안 됩니다");
            }

            for (int i = 1;
                 i < asset.ChainSpeedMultipliers.Length;
                 i++)
            {
                Assert.LessOrEqual(
                    asset.ChainSpeedMultipliers[i],
                    asset.ChainSpeedMultipliers[i - 1],
                    "체인 속도는 단계가 올라갈수록 늘어나면 안 됩니다");
            }
        }

        private static void AssertArray(
            float[] expected,
            float[] actual,
            string field)
        {
            Assert.IsNotNull(
                actual,
                field + " 배열이 비어 있습니다");

            Assert.AreEqual(
                expected.Length,
                actual.Length,
                field + " 길이가 다릅니다");

            for (int i = 0;
                 i < expected.Length;
                 i++)
            {
                Assert.AreEqual(
                    expected[i],
                    actual[i],
                    0.0001f,
                    $"{field}[{i}] 값이 다릅니다");
            }
        }
    }
}
