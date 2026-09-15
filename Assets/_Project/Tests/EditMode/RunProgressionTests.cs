using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Hypnosis;
using ProjectTheta.Run;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class RunExperienceLogicTests
    {
        [Test]
        public void Reclaim_Adds_A_Bonus_On_Top_Of_Hypnosis()
        {
            Assert.Greater(
                RunExperienceLogic.GetHypnosisPoints(true),
                RunExperienceLogic.GetHypnosisPoints(false));

            Assert.AreEqual(
                RunExperienceLogic.GetPoints(RunXpSource.HypnosisSuccess) +
                RunExperienceLogic.GetPoints(RunXpSource.ReclaimBonus),
                RunExperienceLogic.GetHypnosisPoints(true));
        }

        [Test]
        public void Batch_Recovery_Beats_Recovering_One_By_One()
        {
            // 셋을 한 번에 회수하는 쪽이, 한 명씩 세 번 회수하는 것보다 많이 받는다.
            int together =
                RunExperienceLogic.GetRecoveryPoints(3, 0, 0);

            int separately =
                RunExperienceLogic.GetRecoveryPoints(1, 0, 0) * 3;

            Assert.Greater(
                together,
                separately);
        }

        [Test]
        public void Empty_Recovery_Gives_Nothing()
        {
            Assert.AreEqual(
                0,
                RunExperienceLogic.GetRecoveryPoints(0, 5, 5));
        }

        [Test]
        public void Required_Xp_Grows_Each_Level()
        {
            Assert.AreEqual(60, RunExperienceLogic.GetRequiredXp(1));
            Assert.AreEqual(95, RunExperienceLogic.GetRequiredXp(2));
            Assert.AreEqual(130, RunExperienceLogic.GetRequiredXp(3));

            // 최대 레벨에서는 더 필요하지 않다.
            Assert.AreEqual(
                0,
                RunExperienceLogic.GetRequiredXp(
                    RunExperienceLogic.MaximumLevel));
        }

        [Test]
        public void Gain_Multiplier_Rounds_And_Never_Drops_A_Gain_To_Zero()
        {
            Assert.AreEqual(12, RunExperienceLogic.ApplyGainMultiplier(10, 1.2f));
            Assert.AreEqual(1, RunExperienceLogic.ApplyGainMultiplier(1, 0.1f));
            Assert.AreEqual(0, RunExperienceLogic.ApplyGainMultiplier(0, 3f));
        }

        [Test]
        public void Typical_Run_Reaches_Around_Level_Five()
        {
            // 3분 한 판을 대략 이렇게 논다고 가정한 밸런스 기준선이다.
            // 이 값이 크게 벗어나면 카드가 너무 적거나 너무 많이 나온다.
            int xp = 0;

            xp += RunExperienceLogic.GetHypnosisPoints(false) * 12;
            xp += RunExperienceLogic.GetHypnosisPoints(true) * 3;

            xp += RunExperienceLogic.GetRecoveryPoints(3, 1, 1);
            xp += RunExperienceLogic.GetRecoveryPoints(3, 0, 1);
            xp += RunExperienceLogic.GetRecoveryPoints(2, 1, 0);
            xp += RunExperienceLogic.GetRecoveryPoints(2, 0, 1);
            xp += RunExperienceLogic.GetRecoveryPoints(2, 0, 0);

            xp += RunExperienceLogic.GetPoints(RunXpSource.DuelWin) * 2;
            xp += RunExperienceLogic.GetPoints(RunXpSource.FloorFirstVisit) * 3;

            RunLevelState level =
                new RunLevelState();

            level.AddXp(xp);

            Assert.GreaterOrEqual(level.Level, 4);
            Assert.LessOrEqual(level.Level, 6);
        }
    }

    public sealed class RunLevelStateTests
    {
        [Test]
        public void Starts_At_Level_One_With_No_Xp()
        {
            RunLevelState state =
                new RunLevelState();

            Assert.AreEqual(1, state.Level);
            Assert.AreEqual(0, state.CurrentXp);
            Assert.AreEqual(0f, state.Progress, 0.0001f);
        }

        [Test]
        public void Leftover_Xp_Carries_Into_The_Next_Level()
        {
            RunLevelState state =
                new RunLevelState();

            Assert.AreEqual(1, state.AddXp(70));
            Assert.AreEqual(2, state.Level);
            Assert.AreEqual(10, state.CurrentXp);
        }

        [Test]
        public void One_Big_Gain_Can_Raise_Several_Levels()
        {
            RunLevelState state =
                new RunLevelState();

            // 60 + 95 = 155 → 3레벨, 5 남음
            Assert.AreEqual(2, state.AddXp(160));
            Assert.AreEqual(3, state.Level);
            Assert.AreEqual(5, state.CurrentXp);
        }

        [Test]
        public void Max_Level_Stops_Levelling_But_Still_Counts_Total()
        {
            RunLevelState state =
                new RunLevelState();

            state.AddXp(999999);

            Assert.IsTrue(state.IsMaxLevel);
            Assert.AreEqual(0, state.CurrentXp);
            Assert.AreEqual(1f, state.Progress, 0.0001f);

            Assert.AreEqual(0, state.AddXp(500));
            Assert.AreEqual(999999 + 500, state.TotalXp);
        }

        [Test]
        public void Non_Positive_Gain_Is_Ignored()
        {
            RunLevelState state =
                new RunLevelState();

            Assert.AreEqual(0, state.AddXp(0));
            Assert.AreEqual(0, state.AddXp(-40));
            Assert.AreEqual(0, state.TotalXp);
        }
    }

    public sealed class RunUpgradeStateTests
    {
        [Test]
        public void Ratio_Cards_Fall_Off_Per_Stack()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            state.Apply(RunUpgradeCard.BindingGaze);
            Assert.AreEqual(1.20f, state.HypnosisSpeedMultiplier, 0.0001f);

            // 0.2 × (1.0 + 0.7)
            state.Apply(RunUpgradeCard.BindingGaze);
            Assert.AreEqual(1.34f, state.HypnosisSpeedMultiplier, 0.0001f);

            // 0.2 × (1.0 + 0.7 + 0.5)
            state.Apply(RunUpgradeCard.BindingGaze);
            Assert.AreEqual(1.44f, state.HypnosisSpeedMultiplier, 0.0001f);
        }

        [Test]
        public void Integer_Cards_Do_Not_Fall_Off()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            state.Apply(RunUpgradeCard.ChainImprint);
            state.Apply(RunUpgradeCard.ChainImprint);

            Assert.AreEqual(2, state.ChainExtraTargets);
        }

        [Test]
        public void Cards_Stop_At_Their_Maximum_Stacks()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            Assert.IsTrue(state.Apply(RunUpgradeCard.WideEmbrace));
            Assert.IsTrue(state.Apply(RunUpgradeCard.WideEmbrace));
            Assert.IsFalse(state.Apply(RunUpgradeCard.WideEmbrace));

            Assert.AreEqual(2, state.FollowerLimitBonus);
            Assert.AreEqual(2, state.PickCount);
        }

        [Test]
        public void Reduction_Cards_Have_A_Floor()
        {
            // 최대 스택까지 쌓아도 0이 되거나 음수가 되지 않는다.
            RunUpgradeState state =
                new RunUpgradeState();

            for (int i = 0; i < 5; i++)
            {
                state.Apply(RunUpgradeCard.CalmWhisper);
                state.Apply(RunUpgradeCard.EasyBreath);
            }

            Assert.GreaterOrEqual(state.ImpulseBuildMultiplier, 0.4f);
            Assert.GreaterOrEqual(state.DashCostMultiplier, 0.3f);
        }

        [Test]
        public void Untouched_State_Has_No_Effect()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            Assert.AreEqual(1f, state.HypnosisSpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, state.ImpulseBuildMultiplier, 0.0001f);
            Assert.AreEqual(1f, state.RecoveryEssenceMultiplier, 0.0001f);
            Assert.AreEqual(1f, state.XpGainMultiplier, 0.0001f);
            Assert.AreEqual(1f, state.MoveSpeedMultiplier, 0.0001f);
            Assert.AreEqual(1f, state.DashCostMultiplier, 0.0001f);
            Assert.AreEqual(0, state.ChainExtraTargets);
            Assert.AreEqual(0, state.FollowerLimitBonus);
        }

        [Test]
        public void Description_Shows_The_Total_After_Picking()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            Assert.AreEqual(
                "최면 속도 +20%",
                state.DescribeNextPick(RunUpgradeCard.BindingGaze));

            state.Apply(RunUpgradeCard.BindingGaze);

            // 두 번째를 고르면 합계 34%가 된다.
            Assert.AreEqual(
                "최면 속도 +34%",
                state.DescribeNextPick(RunUpgradeCard.BindingGaze));
        }

        [Test]
        public void History_Keeps_Pick_Order()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            state.Apply(RunUpgradeCard.LightStep);
            state.Apply(RunUpgradeCard.Insight);
            state.Apply(RunUpgradeCard.LightStep);

            CollectionAssert.AreEqual(
                new[]
                {
                    RunUpgradeCard.LightStep,
                    RunUpgradeCard.Insight,
                    RunUpgradeCard.LightStep
                },
                state.History);
        }
    }

    public sealed class RunUpgradeDrawLogicTests
    {
        [Test]
        public void Three_Cards_Come_From_Three_Different_Categories()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            // 시드를 바꿔 가며 여러 번 확인한다. 한 번 통과는 우연일 수 있다.
            for (int seed = 0; seed < 200; seed++)
            {
                List<RunUpgradeCard> cards =
                    RunUpgradeDrawLogic.Draw(
                        state,
                        new System.Random(seed));

                Assert.AreEqual(3, cards.Count);

                HashSet<RunUpgradeCategory> categories =
                    new HashSet<RunUpgradeCategory>();

                foreach (RunUpgradeCard card in cards)
                {
                    categories.Add(
                        RunUpgradeTable.Get(card).Category);
                }

                Assert.AreEqual(
                    3,
                    categories.Count,
                    $"seed {seed}에서 계열이 겹쳤습니다");
            }
        }

        [Test]
        public void Maxed_Cards_Are_Never_Offered()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            for (int i = 0; i < 3; i++)
            {
                state.Apply(RunUpgradeCard.BindingGaze);
            }

            for (int seed = 0; seed < 200; seed++)
            {
                CollectionAssert.DoesNotContain(
                    RunUpgradeDrawLogic.Draw(
                        state,
                        new System.Random(seed)),
                    RunUpgradeCard.BindingGaze);
            }
        }

        [Test]
        public void Same_Seed_Gives_Same_Cards()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            CollectionAssert.AreEqual(
                RunUpgradeDrawLogic.Draw(state, new System.Random(1234)),
                RunUpgradeDrawLogic.Draw(state, new System.Random(1234)));
        }

        [Test]
        public void Offers_Fewer_Cards_When_Few_Remain()
        {
            RunUpgradeState state =
                MaxEverythingExcept(
                    RunUpgradeCard.Insight,
                    RunUpgradeCard.LightStep);

            List<RunUpgradeCard> cards =
                RunUpgradeDrawLogic.Draw(
                    state,
                    new System.Random(7));

            Assert.AreEqual(2, cards.Count);
            CollectionAssert.Contains(cards, RunUpgradeCard.Insight);
            CollectionAssert.Contains(cards, RunUpgradeCard.LightStep);
        }

        [Test]
        public void Nothing_Is_Offered_When_Everything_Is_Maxed()
        {
            RunUpgradeState state =
                MaxEverythingExcept();

            Assert.IsEmpty(
                RunUpgradeDrawLogic.Draw(
                    state,
                    new System.Random(7)));
        }

        [Test]
        public void Same_Category_Fills_In_When_Categories_Run_Out()
        {
            // 최면 계열 두 장만 남으면 계열이 겹치더라도 두 장을 보여준다.
            RunUpgradeState state =
                MaxEverythingExcept(
                    RunUpgradeCard.BindingGaze,
                    RunUpgradeCard.ChainImprint);

            Assert.AreEqual(
                2,
                RunUpgradeDrawLogic.Draw(
                    state,
                    new System.Random(3)).Count);
        }

        private static RunUpgradeState MaxEverythingExcept(
            params RunUpgradeCard[] keep)
        {
            RunUpgradeState state =
                new RunUpgradeState();

            List<RunUpgradeCard> kept =
                new List<RunUpgradeCard>(keep);

            for (int i = 0; i < RunUpgradeTable.Count; i++)
            {
                RunUpgradeProfile profile =
                    RunUpgradeTable.GetAt(i);

                if (kept.Contains(profile.Card))
                {
                    continue;
                }

                for (int s = 0; s < profile.MaximumStacks; s++)
                {
                    state.Apply(profile.Card);
                }
            }

            return state;
        }
    }

    public sealed class RunUpgradeHookTests
    {
        [TearDown]
        public void ClearRun()
        {
            RunUpgradeMultipliers.Clear();
        }

        [Test]
        public void No_Run_Means_No_Effect()
        {
            RunUpgradeMultipliers.Clear();

            Assert.AreEqual(1f, RunUpgradeMultipliers.HypnosisSpeed, 0.0001f);
            Assert.AreEqual(1f, RunUpgradeMultipliers.RecoveryEssence, 0.0001f);
            Assert.AreEqual(0, RunUpgradeMultipliers.ChainExtraTargets);
        }

        [Test]
        public void Chain_Default_Is_Unchanged()
        {
            // 기존 체인 규칙: 0, 1단계에서 이어갈 수 있고 2단계에서 끝난다.
            Assert.IsTrue(ChainHypnosisLogic.CanChain(0));
            Assert.IsTrue(ChainHypnosisLogic.CanChain(1));
            Assert.IsFalse(ChainHypnosisLogic.CanChain(2));
        }

        [Test]
        public void Chain_Imprint_Extends_The_Chain()
        {
            Assert.IsTrue(ChainHypnosisLogic.CanChain(2, 1));
            Assert.IsFalse(ChainHypnosisLogic.CanChain(3, 1));

            Assert.AreEqual(3, ChainHypnosisLogic.Advance(2, 1));
            Assert.AreEqual(2, ChainHypnosisLogic.Advance(2, 0));
        }

        [Test]
        public void Extended_Chain_Steps_Reuse_The_Last_Speed_And_Cost()
        {
            // 표에 없는 4번째 단계는 마지막 값을 그대로 쓴다.
            Assert.AreEqual(
                ChainHypnosisLogic.GetSpeedMultiplier(2),
                ChainHypnosisLogic.GetSpeedMultiplier(3),
                0.0001f);

            Assert.AreEqual(
                ChainHypnosisLogic.GetChainFocusCost(2),
                ChainHypnosisLogic.GetChainFocusCost(3),
                0.0001f);
        }

        [Test]
        public void Active_Run_Feeds_The_Global_Channel()
        {
            RunUpgradeState state =
                new RunUpgradeState();

            state.Apply(RunUpgradeCard.EssenceDrain);
            state.Apply(RunUpgradeCard.ChainImprint);

            RunUpgradeMultipliers.Active = state;

            Assert.AreEqual(1.15f, RunUpgradeMultipliers.RecoveryEssence, 0.0001f);
            Assert.AreEqual(1, RunUpgradeMultipliers.ChainExtraTargets);
        }
    }
}
