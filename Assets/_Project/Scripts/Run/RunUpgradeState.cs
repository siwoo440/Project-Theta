using System;
using System.Collections.Generic;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 한 판 동안 고른 강화 카드와 그 효과다.
    ///
    /// 판이 시작될 때 만들고 끝나면 버린다. 세이브하지 않는다.
    /// 효과 계산은 모두 여기서 끝내고, 게임 쪽은 배율만 읽는다.
    /// </summary>
    public sealed class RunUpgradeState
    {
        /// <summary>
        /// 같은 카드를 여러 번 고를 때 스택별 효율이다.
        /// 한 카드만 계속 고르는 것이 무조건 최선이 되지 않도록 줄여 나간다.
        /// </summary>
        public static readonly float[] StackFalloff =
        {
            1.0f,
            0.7f,
            0.5f
        };

        private readonly Dictionary<RunUpgradeCard, int> _stacks =
            new Dictionary<RunUpgradeCard, int>();

        /// <summary>고른 순서다. 결과 화면에서 "이번 판은 이렇게 풀었다"를 보여준다.</summary>
        private readonly List<RunUpgradeCard> _history =
            new List<RunUpgradeCard>();

        public IReadOnlyList<RunUpgradeCard> History =>
            _history;

        public int PickCount =>
            _history.Count;

        public int GetStacks(
            RunUpgradeCard card)
        {
            return _stacks.TryGetValue(
                card,
                out int stacks)
                ? stacks
                : 0;
        }

        public bool IsMaxed(
            RunUpgradeCard card)
        {
            return GetStacks(card) >=
                   RunUpgradeTable.Get(
                       card).MaximumStacks;
        }

        /// <summary>카드를 한 장 적용한다. 이미 최대 스택이면 false다.</summary>
        public bool Apply(
            RunUpgradeCard card)
        {
            if (IsMaxed(card))
            {
                return false;
            }

            _stacks[card] =
                GetStacks(card) + 1;

            _history.Add(card);

            return true;
        }

        /// <summary>
        /// 스택을 반영한 효과 총량이다.
        /// 비율 효과는 0.2 × (1.0 + 0.7 + 0.5)처럼 감쇠해서 더하고,
        /// 정수 효과는 감쇠 없이 스택 수만큼 더한다.
        /// </summary>
        public static float GetTotalValue(
            RunUpgradeProfile profile,
            int stacks)
        {
            int safe =
                Math.Max(
                    0,
                    Math.Min(
                        stacks,
                        profile.MaximumStacks));

            if (profile.IsInteger)
            {
                return profile.ValuePerStack *
                       safe;
            }

            float sum = 0f;

            for (int i = 0;
                 i < safe;
                 i++)
            {
                float falloff =
                    i < StackFalloff.Length
                        ? StackFalloff[i]
                        : StackFalloff[StackFalloff.Length - 1];

                sum += falloff;
            }

            return profile.ValuePerStack *
                   sum;
        }

        public float GetTotalValue(
            RunUpgradeCard card)
        {
            return GetTotalValue(
                RunUpgradeTable.Get(card),
                GetStacks(card));
        }

        // 게임 쪽이 읽는 효과 --------------------------------------------

        public float HypnosisSpeedMultiplier =>
            1f +
            GetTotalValue(
                RunUpgradeCard.BindingGaze);

        /// <summary>충동 상승 배율이다. 아무리 쌓아도 40% 밑으로는 내려가지 않는다.</summary>
        public float ImpulseBuildMultiplier =>
            Math.Max(
                0.4f,
                1f -
                GetTotalValue(
                    RunUpgradeCard.CalmWhisper));

        public float RecoveryEssenceMultiplier =>
            1f +
            GetTotalValue(
                RunUpgradeCard.EssenceDrain);

        public float XpGainMultiplier =>
            1f +
            GetTotalValue(
                RunUpgradeCard.Insight);

        public float MoveSpeedMultiplier =>
            1f +
            GetTotalValue(
                RunUpgradeCard.LightStep);

        /// <summary>대시 비용 배율이다. 30% 밑으로는 내려가지 않는다.</summary>
        public float DashCostMultiplier =>
            Math.Max(
                0.3f,
                1f -
                GetTotalValue(
                    RunUpgradeCard.EasyBreath));

        public int ChainExtraTargets =>
            (int)Math.Round(
                GetTotalValue(
                    RunUpgradeCard.ChainImprint));

        public int FollowerLimitBonus =>
            (int)Math.Round(
                GetTotalValue(
                    RunUpgradeCard.WideEmbrace));

        /// <summary>카드 설명문에 넣을 수치다. 다음 스택을 골랐을 때의 총량을 보여준다.</summary>
        public string DescribeNextPick(
            RunUpgradeCard card)
        {
            RunUpgradeProfile profile =
                RunUpgradeTable.Get(card);

            float next =
                GetTotalValue(
                    profile,
                    GetStacks(card) + 1);

            string amount =
                profile.IsInteger
                    ? ((int)Math.Round(next)).ToString()
                    : $"{Math.Round(next * 100f)}%";

            return string.Format(
                profile.DescriptionFormat,
                amount);
        }
    }
}
