using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.UI;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class LoadingScreenLogicTests
    {
        [Test]
        public void Runner_Goes_From_Left_End_To_Right_End_With_The_Bar()
        {
            Assert.AreEqual(-LoadingScreenLogic.RunWidth * 0.5f, LoadingScreenLogic.GetRunnerX(0f), 0.001f);
            Assert.AreEqual(0f, LoadingScreenLogic.GetRunnerX(0.5f), 0.001f);
            Assert.AreEqual(LoadingScreenLogic.RunWidth * 0.5f, LoadingScreenLogic.GetRunnerX(1f), 0.001f);
            Assert.AreEqual(LoadingScreenLogic.RunWidth * 0.5f, LoadingScreenLogic.GetRunnerX(3f), 0.001f);

            float previous = LoadingScreenLogic.GetRunnerX(0f);

            for (float p = 0.05f; p <= 1f; p += 0.05f)
            {
                float x = LoadingScreenLogic.GetRunnerX(p);

                Assert.Greater(x, previous);
                previous = x;
            }

            Assert.IsFalse(LoadingScreenLogic.HasArrived(0.9f));
            Assert.IsTrue(LoadingScreenLogic.HasArrived(1f));
        }

        [Test]
        public void Runner_Crossing_Is_Not_A_Blink()
        {
            // 로딩이 즉시 끝나도 바가 차는 동안 달리는 모습이 보여야 한다.
            float crossing = 1f / LoadingScreenLogic.BarFillSpeed;

            Assert.GreaterOrEqual(crossing, 1.2f);
            Assert.LessOrEqual(crossing, LoadingScreenLogic.MinVisibleSeconds + 0.5f);

            // 주인공은 가운데 로딩 바 길이만큼만 달린다.
            Assert.AreEqual(LoadingScreenLogic.BarWidth, LoadingScreenLogic.RunWidth);
        }

        [Test]
        public void Run_Frames_Cycle_Through_All_Pictures()
        {
            HashSet<int> seen = new HashSet<int>();

            for (float t = 0f; t < 1f; t += 0.05f)
            {
                int frame = LoadingScreenLogic.GetRunFrame(t, 4);

                Assert.GreaterOrEqual(frame, 0);
                Assert.Less(frame, 4);
                seen.Add(frame);
            }

            Assert.AreEqual(4, seen.Count);
            Assert.AreEqual(0, LoadingScreenLogic.GetRunFrame(3f, 0));
        }

        [Test]
        public void Succubus_Stays_Upper_Left_Of_The_Runner()
        {
            for (float t = 0f; t < 5f; t += 0.25f)
            {
                float runnerX = LoadingScreenLogic.GetRunnerX(t / 5f);

                Assert.Less(LoadingScreenLogic.GetCompanionTargetX(runnerX), runnerX);
                Assert.Greater(LoadingScreenLogic.GetCompanionTargetY(0f, t), 0f);
            }
        }

        [Test]
        public void Succubus_Follows_Smoothly_Without_Overshoot()
        {
            float x = 0f;

            for (int i = 0; i < 60; i++)
            {
                float next = LoadingScreenLogic.Follow(x, 100f, 1f / 60f);

                Assert.GreaterOrEqual(next, x);
                Assert.LessOrEqual(next, 100f);
                x = next;
            }

            Assert.Greater(x, 90f);
            Assert.AreEqual(5f, LoadingScreenLogic.Follow(5f, 100f, 0f));
        }

        [Test]
        public void Progress_Is_Normalized_From_Unity_Range()
        {
            Assert.AreEqual(0f, LoadingScreenLogic.NormalizeProgress(0f, false));
            Assert.AreEqual(0.5f, LoadingScreenLogic.NormalizeProgress(0.45f, false), 0.001f);
            Assert.AreEqual(1f, LoadingScreenLogic.NormalizeProgress(0.9f, false), 0.001f);
            Assert.AreEqual(1f, LoadingScreenLogic.NormalizeProgress(0.3f, true));
        }

        [Test]
        public void Bar_Never_Goes_Backwards()
        {
            float shown = 0.6f;

            shown = LoadingScreenLogic.StepDisplayed(shown, 0.2f, 0.1f);
            Assert.AreEqual(0.6f, shown, 0.0001f);

            float previous = shown;

            for (int i = 0; i < 100; i++)
            {
                shown = LoadingScreenLogic.StepDisplayed(shown, 1f, 0.016f);

                Assert.GreaterOrEqual(shown, previous);
                Assert.LessOrEqual(shown, 1f);
                previous = shown;
            }

            Assert.IsTrue(LoadingScreenLogic.IsBarFull(shown));
        }

        [Test]
        public void Screen_Stays_For_The_Minimum_Time()
        {
            Assert.IsFalse(LoadingScreenLogic.CanActivate(1f, 0.1f));
            Assert.IsFalse(LoadingScreenLogic.CanActivate(0.8f, 5f));
            Assert.IsTrue(LoadingScreenLogic.CanActivate(1f, LoadingScreenLogic.MinVisibleSeconds));
        }

        [Test]
        public void Percent_Text_Is_Rounded_And_Clamped()
        {
            Assert.AreEqual("0%", LoadingScreenLogic.FormatPercent(-1f));
            Assert.AreEqual("62%", LoadingScreenLogic.FormatPercent(0.62f));
            Assert.AreEqual("100%", LoadingScreenLogic.FormatPercent(2f));
        }

        [Test]
        public void Tips_Never_Repeat_Back_To_Back()
        {
            int count = LoadingTips.All.Length;
            int current = LoadingScreenLogic.NextTipIndex(-1, count, 7);

            Assert.GreaterOrEqual(current, 0);
            Assert.Less(current, count);

            for (int random = -50; random < 500; random += 7)
            {
                int next = LoadingScreenLogic.NextTipIndex(current, count, random);

                Assert.AreNotEqual(current, next);
                Assert.GreaterOrEqual(next, 0);
                Assert.Less(next, count);
                current = next;
            }

            Assert.AreEqual(0, LoadingScreenLogic.NextTipIndex(0, 1, 5));
        }

        [Test]
        public void Tips_Are_Filled_And_Short()
        {
            Assert.GreaterOrEqual(LoadingTips.All.Length, 10);

            foreach (string tip in LoadingTips.All)
            {
                Assert.IsFalse(string.IsNullOrWhiteSpace(tip));
                Assert.LessOrEqual(tip.Length, 40, tip);
            }
        }
    }
}
