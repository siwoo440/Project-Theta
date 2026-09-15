using NUnit.Framework;
using ProjectTheta.Presentation;
using ProjectTheta.Run;
using ProjectTheta.Save;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class OpponentFloorPlanTests
    {
        [Test]
        public void Ground_Floor_Has_No_Opponent()
        {
            // 1층은 연습 층이다.
            Assert.Greater(OpponentFloorPlan.GeumtaeyangStartFloor, 0);
            Assert.Greater(OpponentFloorPlan.PopularGuyStartFloor, 0);
        }

        [Test]
        public void Opponents_Appear_One_Floor_At_A_Time()
        {
            // 위층일수록 경쟁자가 한 명씩 늘어난다. 둘이 같은 층에서 처음 나오면 안 된다.
            Assert.Less(
                OpponentFloorPlan.GeumtaeyangStartFloor,
                OpponentFloorPlan.PopularGuyStartFloor);
        }

        [Test]
        public void Opponent_Floors_Exist_In_The_Building()
        {
            Assert.Less(
                OpponentFloorPlan.PopularGuyStartFloor,
                FloorPlanLogic.DefaultFloorCount);
        }

        [Test]
        public void Unmet_Opponent_Waits_On_Its_Floor()
        {
            Assert.IsFalse(
                OpponentFloorPlan.ShouldChase(
                    false,
                    1,
                    0));
        }

        [Test]
        public void Met_Opponent_Follows_To_Another_Floor()
        {
            Assert.IsTrue(
                OpponentFloorPlan.ShouldChase(
                    true,
                    1,
                    2));
        }

        [Test]
        public void Opponent_Already_On_The_Floor_Does_Not_Move()
        {
            Assert.IsFalse(
                OpponentFloorPlan.ShouldChase(
                    true,
                    2,
                    2));
        }
    }

    public sealed class RampageSurvivalTests
    {
        [Test]
        public void Surviving_A_Rampage_Gives_Experience()
        {
            Assert.Greater(
                RunExperienceLogic.GetPoints(
                    RunXpSource.RampageSurvived),
                0);
        }

        [Test]
        public void Tutorial_Completes_After_Surviving_A_Rampage()
        {
            TutorialProgress progress =
                new TutorialProgress
                {
                    HypnosisCount = 1,
                    MaximumFollowers = 2,
                    RecoveryCount = 1,
                    ReclaimCount = 1,
                    RampageSurvivedCount = 0
                };

            // 폭주를 넘기기 전에는 마지막 단계에서 멈춘다.
            Assert.AreEqual(
                TutorialStep.Rampage,
                TutorialFlowLogic.Advance(
                    TutorialStep.Hypnosis,
                    progress));

            progress.RampageSurvivedCount = 1;

            Assert.AreEqual(
                TutorialStep.Completed,
                TutorialFlowLogic.Advance(
                    TutorialStep.Rampage,
                    progress));
        }

        [Test]
        public void Tutorial_Flag_Survives_Clone()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            Assert.IsFalse(data.TutorialCompleted);

            data.TutorialCompleted = true;

            Assert.IsTrue(
                data.Clone().TutorialCompleted);

            Assert.IsTrue(
                SaveDataLogic.Normalize(data).TutorialCompleted);
        }
    }

    public sealed class RunUpgradeAssetFallbackTests
    {
        [Test]
        public void Card_Table_Works_Without_Asset()
        {
            Assert.IsNull(
                RunUpgradeTable.Override);

            Assert.AreEqual(
                RunUpgradeTable.Profiles.Length,
                RunUpgradeTable.Count);
        }
    }

    public sealed class UiChangeKeyTests
    {
        [Test]
        public void First_Value_Always_Counts_As_Changed()
        {
            long cache =
                UiChangeKey.Unset;

            Assert.IsTrue(
                UiChangeKey.Changed(
                    ref cache,
                    0));
        }

        [Test]
        public void Same_Value_Is_Skipped()
        {
            long cache =
                UiChangeKey.Unset;

            UiChangeKey.Changed(ref cache, 42);

            Assert.IsFalse(
                UiChangeKey.Changed(
                    ref cache,
                    42));

            Assert.IsTrue(
                UiChangeKey.Changed(
                    ref cache,
                    43));
        }

        [Test]
        public void Pair_Order_Matters()
        {
            // "체력 3 / 7"과 "체력 7 / 3"이 같은 키면 표시가 안 바뀐다.
            Assert.AreNotEqual(
                UiChangeKey.Of(3, 7),
                UiChangeKey.Of(7, 3));
        }

        [Test]
        public void State_Separates_Otherwise_Equal_Values()
        {
            Assert.AreNotEqual(
                UiChangeKey.Of(10, 0, 1),
                UiChangeKey.Of(10, 0, 2));
        }

        [Test]
        public void Tenths_Change_Only_When_The_Shown_Digit_Changes()
        {
            // "2.3초"로 보이는 동안은 키가 같아야 한다.
            Assert.AreEqual(
                UiChangeKey.Tenths(2.31f),
                UiChangeKey.Tenths(2.34f));

            Assert.AreNotEqual(
                UiChangeKey.Tenths(2.34f),
                UiChangeKey.Tenths(2.36f));
        }

        [Test]
        public void Negative_Values_Do_Not_Collide()
        {
            Assert.AreNotEqual(
                UiChangeKey.Of(-1, 5),
                UiChangeKey.Of(1, 5));

            Assert.AreNotEqual(
                UiChangeKey.Of(5, -1),
                UiChangeKey.Of(5, 1));
        }
    }
}
