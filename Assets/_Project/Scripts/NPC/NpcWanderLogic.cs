using System;
using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 캐릭터가 제자리에 굳어 있거나 대열에 딱 붙어 다니지 않도록
    /// 시간에 따라 부드럽게 변하는 방황 오프셋을 계산한다.
    ///
    /// 서로 다른 두 주파수를 겹쳐 단순한 원운동으로 보이지 않게 하고,
    /// 개체마다 다른 위상(phase)과 속도를 주어 무리 전체가 같은 방향으로 흔들리지 않게 한다.
    /// </summary>
    public static class NpcWanderLogic
    {
        public const float DefaultHorizontalAmplitude = 0.46f;
        public const float DefaultVerticalAmplitude = 0.38f;
        public const float DefaultMinimumSpeed = 0.42f;
        public const float DefaultMaximumSpeed = 1.05f;

        private const double PrimaryWeight = 0.66;
        private const double SecondaryWeight = 0.34;

        public static Vector2 GetOffset(
            float time,
            float phase,
            float speed,
            float horizontalAmplitude,
            float verticalAmplitude)
        {
            double t =
                Math.Max(
                    0f,
                    time) *
                Math.Max(
                    0f,
                    speed);

            double x =
                (Math.Sin(t + phase) *
                 PrimaryWeight) +
                (Math.Sin(
                     (t * 1.73) +
                     (phase * 2.10)) *
                 SecondaryWeight);

            double y =
                (Math.Cos(
                     (t * 0.83) +
                     phase) *
                 PrimaryWeight) +
                (Math.Sin(
                     (t * 1.31) +
                     (phase * 1.70)) *
                 SecondaryWeight);

            return new Vector2(
                (float)(x * horizontalAmplitude),
                (float)(y * verticalAmplitude));
        }

        /// <summary>진폭 합이 1이므로 오프셋은 항상 지정한 진폭 안에 머문다.</summary>
        public static bool IsWithinAmplitude(
            Vector2 offset,
            float horizontalAmplitude,
            float verticalAmplitude)
        {
            const float tolerance =
                0.0001f;

            return Math.Abs(offset.x) <=
                   Math.Abs(horizontalAmplitude) +
                   tolerance &&
                   Math.Abs(offset.y) <=
                   Math.Abs(verticalAmplitude) +
                   tolerance;
        }
    }
}
