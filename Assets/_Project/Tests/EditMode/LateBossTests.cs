using NUnit.Framework;
using ProjectTheta.Boss;
using ProjectTheta.Disruptors;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class LateBossSkillTests
    {
        [Test]
        public void Tower_Doubles_Rival_Claims_Only_While_Up()
        {
            Assert.AreEqual(2f, LateBossSkillValues.GetClaimRate(true));
            Assert.AreEqual(1f, LateBossSkillValues.GetClaimRate(false));
            Assert.Less(LateBossSkillValues.TowerSeconds, LateBossSkillValues.TowerCooldownSeconds);
        }

        [Test]
        public void Zone_Covers_Only_Its_Floor_And_Width()
        {
            Assert.IsTrue(LateBossSkillValues.IsInZone(2f, 0f, 1, 1));
            Assert.IsFalse(LateBossSkillValues.IsInZone(3f, 0f, 1, 1));
            Assert.IsFalse(LateBossSkillValues.IsInZone(0f, 0f, 0, 1));
            Assert.Less(LateBossSkillValues.ZoneHypnosisMultiplier, 1f);
        }

        [Test]
        public void Stunned_Dj_Halves_The_Zone()
        {
            Assert.AreEqual(LateBossSkillValues.ZoneSeconds, LateBossSkillValues.GetZoneSeconds(false));
            Assert.AreEqual(LateBossSkillValues.ZoneSeconds * 0.5f, LateBossSkillValues.GetZoneSeconds(true));
        }

        [Test]
        public void Clones_Hit_Softer_And_Leave_Before_The_Next_Summon()
        {
            Assert.Less(LateBossSkillValues.CloneDamageScale, 1f);
            Assert.Greater(LateBossSkillValues.CloneDamageScale, 0f);
            Assert.Less(LateBossSkillValues.CloneSeconds, LateBossSkillValues.CloneCooldownSeconds);
            Assert.AreEqual(2, LateBossSkillValues.CloneCount);
        }

        [Test]
        public void Every_Late_Skill_Has_A_Telegraph()
        {
            Assert.GreaterOrEqual(LateBossSkillValues.TowerTelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds);
            Assert.GreaterOrEqual(LateBossSkillValues.CloneTelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds);
            Assert.GreaterOrEqual(LateBossSkillValues.ZoneTelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds);
        }

        [Test]
        public void Clone_Looks_Like_The_Rival()
        {
            DisruptorProfile clone = DisruptorCatalog.Get(DisruptorKind.RivalClone);
            DisruptorProfile boss = DisruptorCatalog.Get(DisruptorKind.RivalSuccubus);

            Assert.AreEqual(boss.DisplayName, clone.DisplayName);
            Assert.AreEqual(boss.Tint, clone.Tint);
            Assert.IsTrue(clone.IsSpecial);
        }

        [Test]
        public void No_Zone_Means_No_Slowdown()
        {
            Assert.IsNull(VipZoneAbility.ActiveZone);
            Assert.AreEqual(1f, VipZoneAbility.GetHypnosisMultiplier(UnityEngine.Vector2.zero));
            Assert.AreEqual(0, CloneDancer.ActiveCount);
        }

        [Test]
        public void Bartender_Has_A_Label()
        {
            Assert.IsNotEmpty(NpcRoleMark.GetLabel(NpcRole.Bartender));
        }

        [Test]
        public void Bartender_Stands_Near_The_Top_Bar_On_The_Dance_Floor_Side()
        {
            // 샴페인 타워가 바에 서므로 바 자리가 계단 · 회수 지점과 겹치면 안 된다.
            Assert.Less(ClubLayout.BarX, ProjectTheta.Stage.FloorLayout.UpStairX);
            Assert.Less(ClubLayout.BarX, ProjectTheta.Stage.FloorLayout.RecoveryX - 2f);
        }
    }

    public sealed class ElevatorLogicTests
    {
        [Test]
        public void Office_Elevator_Takes_Four_At_Most()
        {
            Assert.AreEqual(4, ElevatorLogic.GetRidersAllowed(true, 7));
            Assert.AreEqual(3, ElevatorLogic.GetRidersAllowed(true, 3));
            Assert.AreEqual(7, ElevatorLogic.GetRidersAllowed(false, 7));
            Assert.AreEqual(0, ElevatorLogic.GetRidersAllowed(true, -1));
        }

        [Test]
        public void Waiting_Is_Long_Enough_To_Come_Back()
        {
            // 계단 이동 한 번(오르고 다시 내려오기)보다 넉넉해야 한다.
            Assert.GreaterOrEqual(ElevatorLogic.WaitSeconds, 10f);
            Assert.LessOrEqual(ElevatorLogic.Capacity, PopulationLogic.MaxPerFloor);
        }
    }
}
