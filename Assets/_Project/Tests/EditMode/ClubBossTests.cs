using System.Collections.Generic;
using NUnit.Framework;
using ProjectTheta.Boss;
using ProjectTheta.Disruptors;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class ClubBeatLogicTests
    {
        [Test]
        public void Higher_Tempo_Means_Faster_And_Stronger_Drops()
        {
            Assert.Greater(ClubBeatLogic.GetDropInterval(1), ClubBeatLogic.GetDropInterval(2));
            Assert.Greater(ClubBeatLogic.GetDropInterval(2), ClubBeatLogic.GetDropInterval(3));
            Assert.AreEqual(8f, ClubBeatLogic.GetDropImpulse(1));
            Assert.AreEqual(12f, ClubBeatLogic.GetDropImpulse(2));
            Assert.AreEqual(16f, ClubBeatLogic.GetDropImpulse(3));
        }

        [Test]
        public void Tempo_Is_Clamped()
        {
            Assert.AreEqual(1, ClubBeatLogic.ClampTempo(0));
            Assert.AreEqual(3, ClubBeatLogic.ClampTempo(9));
            Assert.AreEqual(ClubBeatLogic.GetDropInterval(3), ClubBeatLogic.GetDropInterval(99));
        }

        [Test]
        public void Drop_Window_Follows_A_Drop()
        {
            Assert.IsFalse(ClubBeatLogic.IsDropWindow(0.5f, false), "판 시작 직후는 드롭 창이 아니다");
            Assert.IsTrue(ClubBeatLogic.IsDropWindow(0.5f, true));
            Assert.IsFalse(ClubBeatLogic.IsDropWindow(ClubBeatLogic.DropWindowSeconds, true));

            // 드롭 창은 가장 짧은 드롭 간격보다 짧아야 한다.
            Assert.Less(ClubBeatLogic.DropWindowSeconds, ClubBeatLogic.GetDropInterval(3));
        }

        [Test]
        public void Drop_Boosts_Hypnosis_And_Club_Md()
        {
            Assert.AreEqual(1.5f, ClubBeatLogic.GetHypnosisMultiplier(true, 1.5f));
            Assert.AreEqual(1f, ClubBeatLogic.GetHypnosisMultiplier(false, 1.5f));
            Assert.AreEqual(1f, ClubBeatLogic.GetHypnosisMultiplier(true, 0.2f), "배율은 1 아래로 내려가지 않는다");
            Assert.AreEqual(3f, ClubBeatLogic.GetContestMultiplier(true));
            Assert.AreEqual(1f, ClubBeatLogic.GetContestMultiplier(false));
            Assert.AreEqual(ClubBeatLogic.DefaultDropHypnosisMultiplier, new Balance.StageBalanceValues().DropHypnosisScale, 0.0001f);
        }

        [Test]
        public void Flash_Comes_Periodically()
        {
            Assert.IsTrue(ClubBeatLogic.IsFlash(0.1f));
            Assert.IsFalse(ClubBeatLogic.IsFlash(1f));
            Assert.IsTrue(ClubBeatLogic.IsFlash(ClubBeatLogic.FlashPeriodSeconds + 0.2f));
            Assert.IsFalse(ClubBeatLogic.IsFlash(-1f));
        }
    }

    public sealed class BouncerLogicTests
    {
        [Test]
        public void Blocks_Only_Large_Groups_Without_Vip()
        {
            Assert.IsFalse(BouncerLogic.IsBlocked(6, false, false));
            Assert.IsTrue(BouncerLogic.IsBlocked(7, false, false));
            Assert.IsFalse(BouncerLogic.IsBlocked(7, true, false));
            Assert.IsFalse(BouncerLogic.IsBlocked(7, false, true));
        }

        [Test]
        public void No_Entrance_Lock_Outside_The_Club()
        {
            Assert.IsFalse(VipEntrance.IsLocked(0));
        }
    }

    public sealed class BossBattleLogicTests
    {
        [Test]
        public void Force_Counts_Followers_And_Essence()
        {
            Assert.AreEqual(50 + 12, BossBattleLogic.GetForce(5, 125));
            Assert.AreEqual(0, BossBattleLogic.GetForce(-3, -10));
        }

        [Test]
        public void Advantage_Needs_Twenty_Percent_More()
        {
            Assert.IsFalse(BossBattleLogic.HasAdvantage(110, 100, 1.2f));
            Assert.IsTrue(BossBattleLogic.HasAdvantage(120, 100, 1.2f));
            Assert.IsTrue(BossBattleLogic.HasAdvantage(1, 0, 1.2f));
            Assert.IsFalse(BossBattleLogic.HasAdvantage(0, 0, 1.2f));
            Assert.IsTrue(BossBattleLogic.HasAdvantage(100, 100, 0.5f), "배율은 1 아래로 내려가지 않는다");
        }

        [Test]
        public void Phases_Follow_Shields_And_Dominance()
        {
            Assert.AreEqual(BossPhase.Contest, BossBattleLogic.GetPhase(3, 0f, false));
            Assert.AreEqual(BossPhase.Shield, BossBattleLogic.GetPhase(3, 0f, true));
            Assert.AreEqual(BossPhase.Dominate, BossBattleLogic.GetPhase(0, 50f, false));
            Assert.AreEqual(BossPhase.Defeated, BossBattleLogic.GetPhase(0, 100f, false));

            Assert.IsFalse(BossBattleLogic.CanFocusBoss(BossPhase.Contest));
            Assert.IsTrue(BossBattleLogic.CanFocusBoss(BossPhase.Shield));
            Assert.IsTrue(BossBattleLogic.CanFocusBoss(BossPhase.Dominate));
            Assert.IsFalse(BossBattleLogic.CanFocusBoss(BossPhase.Defeated));
        }

        [Test]
        public void A_Shield_Breaks_After_Six_Seconds_Of_Focus()
        {
            float progress = 0f;

            for (int i = 0; i < 59; i++)
            {
                Assert.IsFalse(BossBattleLogic.AdvanceShield(ref progress, true, 0.1f), $"{i}");
            }

            Assert.IsTrue(BossBattleLogic.AdvanceShield(ref progress, true, 0.2f));
            Assert.AreEqual(0f, progress);
        }

        [Test]
        public void Shield_Progress_Fades_When_Letting_Go()
        {
            float progress = 3f;

            Assert.IsFalse(BossBattleLogic.AdvanceShield(ref progress, false, 1f));
            Assert.AreEqual(2.5f, progress, 0.0001f);
        }

        [Test]
        public void Dominance_Rises_While_Focusing_And_Fades_Otherwise()
        {
            Assert.AreEqual(8f, BossBattleLogic.AdvanceDominance(0f, true, 1f), 0.0001f);
            Assert.AreEqual(48f, BossBattleLogic.AdvanceDominance(50f, false, 1f), 0.0001f);
            Assert.AreEqual(100f, BossBattleLogic.AdvanceDominance(99f, true, 1f));
            Assert.AreEqual(0f, BossBattleLogic.AdvanceDominance(float.NaN, true, 1f));
        }

        [Test]
        public void Skills_Come_Faster_In_The_Last_Phase()
        {
            Assert.AreEqual(9f, BossBattleLogic.GetSkillCooldown(9f, BossPhase.Shield));
            Assert.AreEqual(6f, BossBattleLogic.GetSkillCooldown(9f, BossPhase.Dominate), 0.0001f);
        }

        [Test]
        public void Rival_Can_Be_Beaten_Within_The_Time_Limit()
        {
            float limit = LocationCatalog.Get(LocationId.RooftopClub).TimeLimitSeconds;
            float fightSeconds =
                BossBattleLogic.ShieldCount * BossBattleLogic.SecondsPerShield +
                BossBattleLogic.DominanceMaximum / BossBattleLogic.DominanceRisePerSecond;

            Assert.Less(fightSeconds, limit * 0.5f);
            Assert.AreEqual(BossBattleLogic.DefaultAdvantageRatio, new Balance.StageBalanceValues().BossAdvantageRatio, 0.0001f);
        }
    }

    public sealed class MindLogicTests
    {
        [Test]
        public void Mind_Regenerates_After_A_Pause()
        {
            Assert.AreEqual(50f, MindLogic.Tick(50f, 1f, 1f));
            Assert.AreEqual(55f, MindLogic.Tick(50f, 3f, 1f), 0.0001f);
            Assert.AreEqual(100f, MindLogic.Tick(99f, 3f, 1f));
            Assert.AreEqual(100f, MindLogic.Tick(float.NaN, 3f, 1f));
        }

        [Test]
        public void Damage_Collapses_At_Zero()
        {
            Assert.AreEqual(80f, MindLogic.Damage(100f, 20f));
            Assert.AreEqual(0f, MindLogic.Damage(10f, 20f));
            Assert.AreEqual(10f, MindLogic.Damage(10f, -5f));
            Assert.IsTrue(MindLogic.IsCollapsed(0f));
            Assert.IsFalse(MindLogic.IsCollapsed(1f));
        }

        [Test]
        public void Two_Quick_Collapses_Lose_The_Fight()
        {
            Assert.IsFalse(MindLogic.IsDefeat(10f, 0f, false));
            Assert.IsTrue(MindLogic.IsDefeat(20f, 5f, true));
            Assert.IsFalse(MindLogic.IsDefeat(60f, 5f, true));
        }

        [Test]
        public void A_Full_Gaze_Does_Not_Break_A_Full_Mind()
        {
            // 시선 한 번(1.2초)에 가득 찬 정신력이 무너지면 안 된다.
            float oneBeam = BossSkillValues.GazeBeamSeconds * BossSkillValues.GazeDamagePerSecond;

            Assert.Less(oneBeam, MindLogic.Maximum * 0.5f);
        }
    }

    public sealed class BossSkillValueTests
    {
        [Test]
        public void Gaze_Hits_Only_On_The_Lane_In_Front_Without_Flash()
        {
            Assert.IsTrue(BossSkillValues.GazeHits(0f, 0.3f, 1, 0f, 5f, false));
            Assert.IsFalse(BossSkillValues.GazeHits(0f, 1f, 1, 0f, 5f, false), "다른 줄");
            Assert.IsFalse(BossSkillValues.GazeHits(0f, 0f, 1, 0f, -5f, false), "등 뒤");
            Assert.IsFalse(BossSkillValues.GazeHits(0f, 0f, 1, 0f, 20f, false), "사거리 밖");
            Assert.IsFalse(BossSkillValues.GazeHits(0f, 0f, 1, 0f, 5f, true), "조명 점멸");
            Assert.IsTrue(BossSkillValues.GazeHits(0f, 0f, -1, 0f, -5f, false));
        }

        [Test]
        public void Charm_Takes_Three_Waves_And_Decays()
        {
            float gauge = 0f;

            gauge = BossSkillValues.AddCharm(gauge, BossSkillValues.CharmGain);
            gauge = BossSkillValues.AddCharm(gauge, BossSkillValues.CharmGain);
            Assert.IsFalse(BossSkillValues.IsCharmed(gauge));

            gauge = BossSkillValues.AddCharm(gauge, BossSkillValues.CharmGain);
            Assert.IsTrue(BossSkillValues.IsCharmed(gauge));

            Assert.AreEqual(35f, BossSkillValues.DecayCharm(40f, 1f), 0.0001f);
            Assert.AreEqual(0f, BossSkillValues.DecayCharm(1f, 1f));
        }

        [Test]
        public void Boss_Skills_Have_Telegraphs()
        {
            Assert.GreaterOrEqual(BossSkillValues.GazeTelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds);
            Assert.GreaterOrEqual(BossSkillValues.CharmTelegraphSeconds, SpecialAbilityLogic.MinimumTelegraphSeconds);
        }
    }

    public sealed class ClubPlacementTests
    {
        [Test]
        public void Club_Has_Bouncer_Mds_And_Dj()
        {
            List<DisruptorPlacement> placements =
                DisruptorCatalog.GetPlacements(LocationId.RooftopClub, 2);

            Assert.IsTrue(placements.Exists(p => p.Kind == DisruptorKind.Bouncer && p.Floor == 0));
            Assert.IsTrue(placements.Exists(p => p.Kind == DisruptorKind.Dj && p.Floor == 1));
            Assert.AreEqual(2, placements.FindAll(p => p.Kind == DisruptorKind.ClubMd).Count);

            // 보스는 배치표가 아니라 보스전이 세운다.
            Assert.IsFalse(placements.Exists(p => p.Kind == DisruptorKind.RivalSuccubus));
        }

        [Test]
        public void Bouncer_Stands_Just_Before_The_Up_Stairs()
        {
            DisruptorPlacement bouncer =
                DisruptorCatalog.GetPlacements(LocationId.RooftopClub, 2)
                    .Find(p => p.Kind == DisruptorKind.Bouncer);

            Assert.Less(bouncer.X, FloorLayout.UpStairX);
            Assert.Greater(bouncer.X, FloorLayout.UpStairX - 4f);
        }

        [Test]
        public void Club_Uses_The_Boss_Objective_And_Has_No_Reinforcements()
        {
            Assert.AreEqual(LocationObjective.Boss, LocationCatalog.Get(LocationId.RooftopClub).Objective);
            Assert.IsNull(DisruptorCatalog.GetReinforcementKind(LocationId.RooftopClub));
            Assert.IsNotEmpty(LocationObjectiveLogic.GetHint(LocationObjective.Boss));
        }

        [Test]
        public void Boss_Objective_Clears_Only_By_Defeating_The_Rival()
        {
            Assert.AreEqual(
                StageState.Running,
                ObjectiveStateLogic.Resolve(LocationObjective.Boss, 10f, 999, 200, 5, 0, 3, 5, false));

            Assert.AreEqual(
                StageState.Cleared,
                ObjectiveStateLogic.Resolve(LocationObjective.Boss, 10f, 0, 200, 5, 0, 3, 0, true));

            Assert.AreEqual(
                StageState.FailedByTime,
                ObjectiveStateLogic.Resolve(LocationObjective.Boss, 0f, 0, 200, 5, 0, 3, 0, false));

            Assert.AreEqual(
                StageState.FailedByHealth,
                ObjectiveStateLogic.Resolve(LocationObjective.Boss, 10f, 0, 200, 0, 0, 3, 0, true));

            // 보스가 아닌 목표는 기존 판정 그대로다.
            Assert.AreEqual(
                StageState.Cleared,
                ObjectiveStateLogic.Resolve(LocationObjective.EssenceQuota, 10f, 200, 200, 5, 0, 3, 0, false));
        }

        [Test]
        public void Rival_Owner_Can_Be_Reclaimed_By_The_Player()
        {
            Assert.IsTrue(OwnershipContestLogic.CanPlayerContest(NpcOwner.Rival));
            Assert.IsFalse(OwnershipContestLogic.CanContest(NpcOwner.Rival, NpcOwner.Player));
        }

        [Test]
        public void Club_Layout_Keeps_The_Dance_Floor_In_The_Walk_Area()
        {
            Assert.Less(ClubLayout.DanceFloorMinX, ClubLayout.DanceFloorMaxX);
            Assert.Greater(ClubLayout.DanceFloorMinX, FloorSpace.WalkMinX);
            Assert.Less(ClubLayout.DanceFloorMaxX, FloorLayout.UpStairX);
            Assert.IsTrue(ClubLayout.IsOnDanceFloor(ClubLayout.BossStartX));
            Assert.IsFalse(ClubLayout.IsOnDanceFloor(ClubLayout.DjBoothX));
        }

        [Test]
        public void Vip_Guest_Has_A_Label()
        {
            Assert.IsNotEmpty(NpcRoleMark.GetLabel(NpcRole.VipGuest));
        }
    }
}
