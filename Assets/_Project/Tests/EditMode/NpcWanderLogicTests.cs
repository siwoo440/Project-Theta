using NUnit.Framework;
using UnityEngine;
using ProjectTheta.NPC;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class NpcWanderLogicTests
    {
        [Test]
        public void Offset_Never_Exceeds_The_Requested_Amplitude()
        {
            const float horizontal = 0.46f;
            const float vertical = 0.38f;

            for (float t = 0f;
                 t < 40f;
                 t += 0.13f)
            {
                Vector2 offset =
                    NpcWanderLogic.GetOffset(
                        t,
                        1.7f,
                        0.8f,
                        horizontal,
                        vertical);

                Assert.IsTrue(
                    NpcWanderLogic.IsWithinAmplitude(
                        offset,
                        horizontal,
                        vertical),
                    $"t={t} offset={offset}");
            }
        }

        [Test]
        public void Different_Phases_Produce_Different_Offsets()
        {
            // 무리 전체가 같은 방향으로 함께 흔들리면 뭉쳐 보이므로
            // 위상이 다르면 결과도 달라야 한다.
            Vector2 a =
                NpcWanderLogic.GetOffset(
                    3f,
                    0f,
                    0.8f,
                    0.46f,
                    0.38f);

            Vector2 b =
                NpcWanderLogic.GetOffset(
                    3f,
                    2.1f,
                    0.8f,
                    0.46f,
                    0.38f);

            Assert.Greater(
                (a - b).magnitude,
                0.05f);
        }

        [Test]
        public void Different_Speeds_Diverge_Over_Time()
        {
            Vector2 slow =
                NpcWanderLogic.GetOffset(
                    12f,
                    1f,
                    0.42f,
                    0.46f,
                    0.38f);

            Vector2 fast =
                NpcWanderLogic.GetOffset(
                    12f,
                    1f,
                    1.05f,
                    0.46f,
                    0.38f);

            Assert.Greater(
                (slow - fast).magnitude,
                0.05f);
        }

        [Test]
        public void Offset_Is_Continuous_Across_Small_Time_Steps()
        {
            // 방황이 튀지 않고 부드럽게 이어져야 이동이 자연스럽다.
            Vector2 previous =
                NpcWanderLogic.GetOffset(
                    0f,
                    0.7f,
                    1.0f,
                    0.46f,
                    0.38f);

            for (float t = 0.02f;
                 t < 10f;
                 t += 0.02f)
            {
                Vector2 current =
                    NpcWanderLogic.GetOffset(
                        t,
                        0.7f,
                        1.0f,
                        0.46f,
                        0.38f);

                Assert.Less(
                    (current - previous).magnitude,
                    0.05f,
                    $"t={t}");

                previous =
                    current;
            }
        }

        [Test]
        public void Offset_Actually_Moves_Away_From_Center()
        {
            // 진폭이 있는데도 계속 0 근처에 머물면 방황으로 보이지 않는다.
            float maximum =
                0f;

            for (float t = 0f;
                 t < 30f;
                 t += 0.05f)
            {
                Vector2 offset =
                    NpcWanderLogic.GetOffset(
                        t,
                        0.4f,
                        0.8f,
                        0.46f,
                        0.38f);

                maximum =
                    Mathf.Max(
                        maximum,
                        offset.magnitude);
            }

            Assert.Greater(
                maximum,
                0.30f);
        }

        [Test]
        public void Zero_Amplitude_Produces_No_Offset()
        {
            Vector2 offset =
                NpcWanderLogic.GetOffset(
                    5f,
                    1f,
                    0.8f,
                    0f,
                    0f);

            Assert.AreEqual(
                0f,
                offset.magnitude,
                0.0001f);
        }

        [Test]
        public void Negative_Time_Is_Clamped_To_Zero()
        {
            Vector2 atZero =
                NpcWanderLogic.GetOffset(
                    0f,
                    1.2f,
                    0.8f,
                    0.46f,
                    0.38f);

            Vector2 atNegative =
                NpcWanderLogic.GetOffset(
                    -5f,
                    1.2f,
                    0.8f,
                    0.46f,
                    0.38f);

            Assert.AreEqual(
                atZero.x,
                atNegative.x,
                0.0001f);

            Assert.AreEqual(
                atZero.y,
                atNegative.y,
                0.0001f);
        }
    }
}
