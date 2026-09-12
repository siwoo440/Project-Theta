using System;
using ProjectTheta.Balance;

namespace ProjectTheta.Hypnosis
{
    /// <summary>
    /// 기획서 A.4절 체인 최면이다.
    ///
    /// 최면이 끝나는 순간 근처 중립 NPC로 자동 연결되어 뭉쳐 있는 대상을 한 번에 쓸어담는다.
    /// 대신 단계마다 속도가 떨어지고 집중력을 추가로 먹으므로,
    /// 체인을 이어가면 이후 최면 파동을 쓸 수 없게 된다. 자원 배분의 선택을 만드는 것이 목적이다.
    /// </summary>
    public static class ChainHypnosisLogic
    {
        /// <summary>최대 연결 인원이다. 구조적 상한이므로 조정 대상이 아니다.</summary>
        public const int MaximumChain = 3;

        public const float DefaultChainRadius = 2.2f;

        public static float ChainRadius =>
            BalanceOverrides.StageOrDefault.ChainRadius;

        /// <summary>체인 단계별 최면 속도 배율이다. 0단계는 일반 최면과 같다.</summary>
        public static float GetSpeedMultiplier(
            int chainIndex)
        {
            return StageBalanceValues.ReadClamped(
                BalanceOverrides.StageOrDefault.
                    ChainSpeedMultipliers,
                Math.Max(
                    0,
                    chainIndex),
                0.5f);
        }

        /// <summary>체인을 한 단계 잇는 데 드는 추가 집중력이다.</summary>
        public static float GetChainFocusCost(
            int chainIndex)
        {
            return StageBalanceValues.ReadClamped(
                BalanceOverrides.StageOrDefault.
                    ChainFocusCosts,
                Math.Max(
                    0,
                    chainIndex),
                25f);
        }

        /// <summary>현재 단계에서 체인을 더 이을 수 있는지 판정한다.</summary>
        public static bool CanChain(
            int chainIndex)
        {
            return Math.Max(
                       0,
                       chainIndex) <
                   MaximumChain - 1;
        }

        public static int Advance(
            int chainIndex)
        {
            return Math.Min(
                MaximumChain - 1,
                Math.Max(
                    0,
                    chainIndex) + 1);
        }

        public static bool IsInChainRadius(
            float distance,
            float radius)
        {
            return distance <=
                   Math.Max(
                       0f,
                       radius);
        }
    }
}
