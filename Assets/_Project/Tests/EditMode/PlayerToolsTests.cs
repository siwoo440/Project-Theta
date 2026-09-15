using NUnit.Framework;
using ProjectTheta.Hypnosis;
using ProjectTheta.Items;
using ProjectTheta.Player;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class FocusLogicTests
    {
        [Test]
        public void Drain_Never_Goes_Below_Zero()
        {
            Assert.AreEqual(
                0f,
                FocusLogic.Drain(
                    10f,
                    50f),
                0.0001f);
        }

        [Test]
        public void Continuous_Drain_Uses_Delta_Time()
        {
            Assert.AreEqual(
                94f,
                FocusLogic.DrainPerSecond(
                    100f,
                    6f,
                    1f),
                0.0001f);

            Assert.AreEqual(
                97f,
                FocusLogic.DrainPerSecond(
                    100f,
                    6f,
                    0.5f),
                0.0001f);
        }

        [Test]
        public void Recover_Is_Capped_At_Maximum()
        {
            Assert.AreEqual(
                100f,
                FocusLogic.Recover(
                    95f,
                    100f,
                    14f,
                    1f),
                0.0001f);
        }

        [Test]
        public void Recovery_Waits_For_The_Delay()
        {
            Assert.IsFalse(
                FocusLogic.ShouldRecover(
                    0.79f,
                    0.8f));

            Assert.IsTrue(
                FocusLogic.ShouldRecover(
                    0.8f,
                    0.8f));
        }

        [Test]
        public void Remaining_Focus_Speeds_Up_Hypnosis()
        {
            // 19일차: 집중력이 조금이라도 남아 있으면 가속이 붙는다.
            Assert.AreEqual(
                1.6f,
                FocusLogic.GetHypnosisSpeedMultiplier(
                    100f,
                    0.6f),
                0.0001f);

            Assert.AreEqual(
                1.6f,
                FocusLogic.GetHypnosisSpeedMultiplier(
                    0.5f,
                    0.6f),
                0.0001f);
        }

        [Test]
        public void Depleted_Focus_Keeps_Base_Hypnosis_Speed()
        {
            // 집중력이 0이어도 최면은 멈추지 않는다. 기본 속도(1배)로 계속된다.
            Assert.IsTrue(
                FocusLogic.IsDepleted(
                    0f));

            Assert.AreEqual(
                1f,
                FocusLogic.GetHypnosisSpeedMultiplier(
                    0f,
                    0.6f),
                0.0001f);

            Assert.Greater(
                FocusLogic.GetHypnosisSpeedMultiplier(
                    0f,
                    0.6f),
                0f);
        }

        [Test]
        public void Negative_Bonus_Never_Slows_Below_Base()
        {
            Assert.AreEqual(
                1f,
                FocusLogic.GetHypnosisSpeedMultiplier(
                    50f,
                    -0.5f),
                0.0001f);
        }

        [Test]
        public void Base_Hypnosis_Is_Faster_Than_The_Grade_Table()
        {
            // 19일차 밸런스: 등급별 속도 표 위에 전체 1.3배를 곱한다.
            Assert.AreEqual(
                1.3f,
                new ProjectTheta.Balance.StageBalanceValues().PlayerHypnosisSpeedScale,
                0.0001f);
        }

        [Test]
        public void Default_Focus_Bonus_Is_A_Real_Speed_Up()
        {
            Assert.Greater(
                new ProjectTheta.Balance.StageBalanceValues().FocusHypnosisSpeedBonus,
                0f);
        }

        [Test]
        public void Affordability_Uses_Exact_Cost()
        {
            Assert.IsTrue(
                FocusLogic.CanAfford(
                    30f,
                    30f));

            Assert.IsFalse(
                FocusLogic.CanAfford(
                    29.9f,
                    30f));
        }

        [Test]
        public void Normalized_Is_Clamped()
        {
            Assert.AreEqual(
                1f,
                FocusLogic.Normalized(
                    150f,
                    100f),
                0.0001f);

            Assert.AreEqual(
                0f,
                FocusLogic.Normalized(
                    -10f,
                    100f),
                0.0001f);
        }
    }

    public sealed class HypnosisWaveLogicTests
    {
        [Test]
        public void Charge_Completes_At_The_Threshold()
        {
            Assert.IsFalse(
                HypnosisWaveLogic.IsChargeComplete(
                    0.39f,
                    0.40f));

            Assert.IsTrue(
                HypnosisWaveLogic.IsChargeComplete(
                    0.40f,
                    0.40f));
        }

        [Test]
        public void Charge_Normalized_Is_Clamped()
        {
            Assert.AreEqual(
                0.5f,
                HypnosisWaveLogic.GetChargeNormalized(
                    0.2f,
                    0.4f),
                0.0001f);

            Assert.AreEqual(
                1f,
                HypnosisWaveLogic.GetChargeNormalized(
                    5f,
                    0.4f),
                0.0001f);
        }

        [Test]
        public void Radius_Check_Includes_The_Boundary()
        {
            Assert.IsTrue(
                HypnosisWaveLogic.IsInRadius(
                    3.5f,
                    3.5f));

            Assert.IsFalse(
                HypnosisWaveLogic.IsInRadius(
                    3.51f,
                    3.5f));
        }

        [Test]
        public void Cast_Requires_Stage_Cooldown_And_Focus()
        {
            Assert.IsTrue(
                HypnosisWaveLogic.CanCast(
                    true,
                    false,
                    0f,
                    30f,
                    30f));

            // 스테이지 종료
            Assert.IsFalse(
                HypnosisWaveLogic.CanCast(
                    false,
                    false,
                    0f,
                    100f,
                    30f));

            // 힘겨루기·포획 중에는 우클릭이 겹치므로 차단
            Assert.IsFalse(
                HypnosisWaveLogic.CanCast(
                    true,
                    true,
                    0f,
                    100f,
                    30f));

            // 재사용 대기
            Assert.IsFalse(
                HypnosisWaveLogic.CanCast(
                    true,
                    false,
                    1.2f,
                    100f,
                    30f));

            // 집중력 부족
            Assert.IsFalse(
                HypnosisWaveLogic.CanCast(
                    true,
                    false,
                    0f,
                    29f,
                    30f));
        }
    }

    public sealed class ChainHypnosisLogicTests
    {
        [Test]
        public void Speed_Falls_Off_Each_Chain_Step()
        {
            Assert.AreEqual(1.0f, ChainHypnosisLogic.GetSpeedMultiplier(0), 0.0001f);
            Assert.AreEqual(0.7f, ChainHypnosisLogic.GetSpeedMultiplier(1), 0.0001f);
            Assert.AreEqual(0.5f, ChainHypnosisLogic.GetSpeedMultiplier(2), 0.0001f);
        }

        [Test]
        public void Chain_Costs_Focus_From_The_Second_Target()
        {
            Assert.AreEqual(0f, ChainHypnosisLogic.GetChainFocusCost(0), 0.0001f);
            Assert.AreEqual(15f, ChainHypnosisLogic.GetChainFocusCost(1), 0.0001f);
            Assert.AreEqual(25f, ChainHypnosisLogic.GetChainFocusCost(2), 0.0001f);
        }

        [Test]
        public void Chain_Stops_At_Three_Targets()
        {
            Assert.IsTrue(
                ChainHypnosisLogic.CanChain(
                    0));

            Assert.IsTrue(
                ChainHypnosisLogic.CanChain(
                    1));

            Assert.IsFalse(
                ChainHypnosisLogic.CanChain(
                    ChainHypnosisLogic.MaximumChain - 1));
        }

        [Test]
        public void Advance_Is_Clamped_To_The_Maximum()
        {
            Assert.AreEqual(
                1,
                ChainHypnosisLogic.Advance(
                    0));

            Assert.AreEqual(
                ChainHypnosisLogic.MaximumChain - 1,
                ChainHypnosisLogic.Advance(
                    ChainHypnosisLogic.MaximumChain));
        }

        [Test]
        public void Chain_Radius_Includes_The_Boundary()
        {
            Assert.IsTrue(
                ChainHypnosisLogic.IsInChainRadius(
                    2.2f,
                    2.2f));

            Assert.IsFalse(
                ChainHypnosisLogic.IsInChainRadius(
                    2.21f,
                    2.2f));
        }

        [Test]
        public void Full_Chain_Costs_Forty_Focus()
        {
            // 3명을 잇는 데 15 + 25 = 40. 파동(30)을 쓸 수 없게 되는 것이 의도된 대가다.
            float total =
                ChainHypnosisLogic.GetChainFocusCost(
                    1) +
                ChainHypnosisLogic.GetChainFocusCost(
                    2);

            Assert.AreEqual(
                40f,
                total,
                0.0001f);

            Assert.Greater(
                total,
                HypnosisWaveLogic.FocusCost);
        }
    }

    public sealed class ConsumableSlotLogicTests
    {
        [Test]
        public void Only_Two_Slots_Exist()
        {
            Assert.AreEqual(
                2,
                ConsumableSlotLogic.SlotCount);

            Assert.IsTrue(
                ConsumableSlotLogic.IsValidSlot(
                    0));

            Assert.IsTrue(
                ConsumableSlotLogic.IsValidSlot(
                    1));

            Assert.IsFalse(
                ConsumableSlotLogic.IsValidSlot(
                    2));

            Assert.IsFalse(
                ConsumableSlotLogic.IsValidSlot(
                    -1));
        }

        [Test]
        public void Empty_Slot_Cannot_Be_Used()
        {
            Assert.IsFalse(
                ConsumableSlotLogic.CanUse(
                    ConsumableItem.None));

            Assert.IsTrue(
                ConsumableSlotLogic.CanUse(
                    ConsumableItem.Sedative));
        }

        [Test]
        public void Starting_Item_Covers_The_Whole_Pool()
        {
            Assert.AreEqual(
                ConsumableItem.Sedative,
                ConsumableSlotLogic.PickStartingItem(
                    0f));

            Assert.AreEqual(
                ConsumableItem.SprintDraught,
                ConsumableSlotLogic.PickStartingItem(
                    1f));
        }

        [Test]
        public void Second_Slot_Is_Always_A_Different_Item()
        {
            foreach (ConsumableItem first in
                     ConsumableItemTable.StartingPool)
            {
                for (float r = 0f;
                     r <= 1f;
                     r += 0.1f)
                {
                    Assert.AreNotEqual(
                        first,
                        ConsumableSlotLogic.PickSecondItem(
                            first,
                            r),
                        $"first={first} r={r}");
                }
            }
        }

        [Test]
        public void Every_Item_Has_A_Profile()
        {
            foreach (ConsumableItem item in
                     System.Enum.GetValues(
                         typeof(ConsumableItem)))
            {
                Assert.AreEqual(
                    item,
                    ConsumableItemTable.Get(
                        item).Item);
            }
        }

        [Test]
        public void Duration_Items_Have_A_Duration()
        {
            Assert.Greater(
                ConsumableItemTable.Get(
                    ConsumableItem.WardCharm).DurationSeconds,
                0f);

            Assert.Greater(
                ConsumableItemTable.Get(
                    ConsumableItem.SprintDraught).DurationSeconds,
                0f);

            // 즉시 효과 아이템은 지속 시간이 없다.
            Assert.AreEqual(
                0f,
                ConsumableItemTable.Get(
                    ConsumableItem.Sedative).DurationSeconds,
                0.0001f);
        }
    }
}
