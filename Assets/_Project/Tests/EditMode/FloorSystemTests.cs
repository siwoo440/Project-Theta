using NUnit.Framework;
using UnityEngine;
using ProjectTheta.NPC;
using ProjectTheta.Stage;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class FloorSpaceTests
    {
        [Test]
        public void Floors_Are_Stacked_By_Fixed_Height()
        {
            Assert.AreEqual(
                0f,
                FloorSpace.OriginY(0),
                0.0001f);

            Assert.AreEqual(
                FloorSpace.FloorHeight * 3f,
                FloorSpace.OriginY(3),
                0.0001f);
        }

        [Test]
        public void Floor_Is_Resolved_From_Vertical_Position()
        {
            // 보행 구역 어디에 서 있든 같은 층으로 읽혀야 한다.
            Assert.AreEqual(
                0,
                FloorSpace.FloorAt(
                    FloorSpace.WalkMinY));

            Assert.AreEqual(
                0,
                FloorSpace.FloorAt(
                    FloorSpace.WalkMaxY));

            Assert.AreEqual(
                2,
                FloorSpace.FloorAt(
                    FloorSpace.MinYOn(2)));

            Assert.AreEqual(
                2,
                FloorSpace.FloorAt(
                    FloorSpace.MaxYOn(2)));
        }

        [Test]
        public void Floor_Gap_Is_Wider_Than_The_Walk_Band()
        {
            // 층 간격이 보행 구역보다 좁으면 층이 서로 겹친다.
            Assert.Greater(
                FloorSpace.FloorHeight,
                FloorSpace.WalkMaxY -
                FloorSpace.WalkMinY);
        }

        [Test]
        public void Walking_Cannot_Leave_The_Current_Floor()
        {
            // 2층에 선 채로 아주 큰 값이 들어와도 2층 안에 남아야 한다.
            // 층을 "가두려는 값"으로 정하면 여기서 5층으로 새어나간다.
            float y =
                FloorSpace.ClampYNear(
                    FloorSpace.MinYOn(2) + 1f,
                    FloorSpace.MaxYOn(2) + 40f,
                    0f,
                    0f);

            Assert.AreEqual(
                2,
                FloorSpace.FloorAt(y));

            Assert.LessOrEqual(
                y,
                FloorSpace.MaxYOn(2) + 0.0001f);
        }

        [Test]
        public void Clamp_Also_Holds_When_Pushed_Downward()
        {
            float y =
                FloorSpace.ClampYNear(
                    FloorSpace.MaxYOn(1) - 1f,
                    FloorSpace.MinYOn(1) - 40f,
                    0f,
                    0f);

            Assert.AreEqual(
                1,
                FloorSpace.FloorAt(y));

            Assert.GreaterOrEqual(
                y,
                FloorSpace.MinYOn(1) - 0.0001f);
        }

        [Test]
        public void World_And_Local_Round_Trip()
        {
            Vector2 local =
                new Vector2(3.5f, -2.2f);

            Vector2 world =
                FloorSpace.ToWorld(
                    3,
                    local);

            Vector2 back =
                FloorSpace.ToLocal(
                    3,
                    world);

            Assert.AreEqual(local.x, back.x, 0.0001f);
            Assert.AreEqual(local.y, back.y, 0.0001f);
        }
    }

    public sealed class FloorPlanLogicTests
    {
        [Test]
        public void Ground_Floor_Has_No_Down_Stair()
        {
            Assert.IsFalse(
                FloorPlanLogic.HasDownStair(0));

            Assert.IsTrue(
                FloorPlanLogic.HasDownStair(1));
        }

        [Test]
        public void Top_Floor_Has_No_Up_Stair()
        {
            Assert.IsTrue(
                FloorPlanLogic.HasUpStair(
                    0,
                    4));

            Assert.IsFalse(
                FloorPlanLogic.HasUpStair(
                    3,
                    4));
        }

        [Test]
        public void Labels_Start_At_One()
        {
            Assert.AreEqual(
                "1F",
                FloorPlanLogic.GetLabel(0));

            Assert.AreEqual(
                "4F",
                FloorPlanLogic.GetLabel(3));
        }

        [Test]
        public void Upper_Floors_Hold_More_Npcs()
        {
            Assert.Greater(
                FloorPlanLogic.GetNpcCount(3),
                FloorPlanLogic.GetNpcCount(0));

            Assert.LessOrEqual(
                FloorPlanLogic.GetNpcCount(99),
                FloorPlanLogic.MaximumNpcCount);
        }

        [Test]
        public void Grade_Mix_Is_Deterministic()
        {
            NpcGrade[] first =
                FloorPlanLogic.BuildGrades(
                    2,
                    12);

            NpcGrade[] second =
                FloorPlanLogic.BuildGrades(
                    2,
                    12);

            CollectionAssert.AreEqual(
                first,
                second);
        }

        [Test]
        public void Ground_Floor_Has_No_Awakened()
        {
            NpcGrade[] grades =
                FloorPlanLogic.BuildGrades(
                    0,
                    FloorPlanLogic.GetNpcCount(0));

            CollectionAssert.DoesNotContain(
                grades,
                NpcGrade.Awakened);
        }

        [Test]
        public void Top_Floor_Is_Richer_Than_Ground()
        {
            // 위층일수록 정기 가치 합이 커야 "올라갈 이유"가 생긴다.
            Assert.Greater(
                SumEssence(3),
                SumEssence(0));
        }

        private static int SumEssence(
            int floor)
        {
            NpcGrade[] grades =
                FloorPlanLogic.BuildGrades(
                    floor,
                    FloorPlanLogic.GetNpcCount(
                        floor));

            int total = 0;

            for (int i = 0;
                 i < grades.Length;
                 i++)
            {
                total +=
                    NpcGradeTable.Get(
                        grades[i]).EssenceValue;
            }

            return total;
        }
    }

    public sealed class FloorLayoutTests
    {
        [Test]
        public void Up_Stair_Is_On_The_Right_And_Down_Stair_On_The_Left()
        {
            Assert.Greater(FloorLayout.UpStairX, 0f);
            Assert.Less(FloorLayout.DownStairX, 0f);
        }

        [Test]
        public void Stairs_Sit_Inside_The_Walk_Area()
        {
            // 계단 앞에 설 수 없으면 이동할 방법이 없다.
            Assert.GreaterOrEqual(FloorLayout.DownStairX, FloorSpace.WalkMinX);
            Assert.LessOrEqual(FloorLayout.UpStairX, FloorSpace.WalkMaxX);

            Assert.GreaterOrEqual(FloorLayout.StairStandY, FloorSpace.WalkMinY);
            Assert.LessOrEqual(FloorLayout.StairStandY, FloorSpace.WalkMaxY);
        }

        [Test]
        public void Up_Stair_Does_Not_Overlap_The_Recovery_Point()
        {
            // 위층 계단과 회수 지점이 둘 다 오른쪽에 있다.
            // 겹치면 계단을 쓰려다 동행자가 회수되어 버린다.
            float stairReach =
                FloorLayout.UpStairX +
                FloorLayout.StairInteractRadius;

            float recoveryStart =
                FloorLayout.RecoveryX -
                FloorLayout.RecoveryWidth * 0.5f;

            Assert.Less(
                stairReach,
                recoveryStart);
        }

        [Test]
        public void Arrival_Lands_In_Front_Of_The_Return_Stair()
        {
            // 올라오면 위층의 아래층 계단(왼쪽) 앞, 내려오면 아래층의 위층 계단(오른쪽) 앞에 선다.
            Assert.Less(
                FloorLayout.DownStairX,
                FloorLayout.UpStairX);
        }
    }

    public sealed class FloorRunStateTests
    {
        [Test]
        public void Run_Starts_On_The_Ground_Floor()
        {
            FloorRunState run =
                new FloorRunState(4);

            Assert.AreEqual(0, run.CurrentFloor);
            Assert.AreEqual(1, run.VisitedCount);
            Assert.IsTrue(run.IsVisited(0));
            Assert.IsFalse(run.IsVisited(1));
        }

        [Test]
        public void First_Arrival_Is_Reported_Once()
        {
            FloorRunState run =
                new FloorRunState(4);

            Assert.IsTrue(
                run.MoveTo(1));

            Assert.IsTrue(
                run.MoveTo(2));

            // 다시 내려갔다 올라와도 처음이 아니다.
            Assert.IsFalse(
                run.MoveTo(1));

            Assert.IsFalse(
                run.MoveTo(2));
        }

        [Test]
        public void Highest_Floor_Is_Remembered_After_Coming_Back_Down()
        {
            FloorRunState run =
                new FloorRunState(4);

            run.MoveTo(1);
            run.MoveTo(2);
            run.MoveTo(3);
            run.MoveTo(0);

            Assert.AreEqual(0, run.CurrentFloor);
            Assert.AreEqual(3, run.HighestReached);
            Assert.AreEqual(4, run.VisitedCount);
        }

        [Test]
        public void Cannot_Move_Outside_The_Building()
        {
            FloorRunState run =
                new FloorRunState(3);

            Assert.IsFalse(
                run.CanMoveTo(-1));

            Assert.IsFalse(
                run.CanMoveTo(3));

            // 같은 층으로의 이동도 막는다.
            Assert.IsFalse(
                run.CanMoveTo(0));
        }

        [Test]
        public void Stair_Availability_Follows_The_Current_Floor()
        {
            FloorRunState run =
                new FloorRunState(3);

            Assert.IsTrue(run.HasUpStair);
            Assert.IsFalse(run.HasDownStair);

            run.MoveTo(2);

            Assert.IsFalse(run.HasUpStair);
            Assert.IsTrue(run.HasDownStair);
        }
    }
}
