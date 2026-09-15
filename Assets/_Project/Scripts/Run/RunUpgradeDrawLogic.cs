using System;
using System.Collections.Generic;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 선택지로 보여줄 카드를 뽑는다.
    ///
    /// 규칙
    ///   1. 최대 스택에 닿은 카드는 후보에서 뺀다
    ///   2. 가능하면 서로 다른 계열에서 한 장씩 뽑는다 (같은 계열 3장은 선택이 아니다)
    ///   3. 계열이 모자라면 남은 카드로 채운다
    ///   4. 난수는 밖에서 받는다. 같은 시드면 같은 결과가 나와 테스트와 재현이 가능하다
    /// </summary>
    public static class RunUpgradeDrawLogic
    {
        public const int DefaultChoiceCount = 3;

        public static List<RunUpgradeCard> Draw(
            RunUpgradeState state,
            Random random,
            int choiceCount = DefaultChoiceCount)
        {
            List<RunUpgradeCard> result =
                new List<RunUpgradeCard>();

            if (state == null ||
                random == null ||
                choiceCount <= 0)
            {
                return result;
            }

            List<RunUpgradeCard> available =
                CollectAvailable(
                    state);

            if (available.Count == 0)
            {
                return result;
            }

            // 계열 순서를 섞어서 매번 같은 계열이 첫 칸에 오지 않게 한다.
            List<RunUpgradeCategory> categories =
                CollectCategories(
                    available);

            Shuffle(
                categories,
                random);

            for (int i = 0;
                 i < categories.Count &&
                 result.Count < choiceCount;
                 i++)
            {
                List<RunUpgradeCard> inCategory =
                    FilterByCategory(
                        available,
                        categories[i]);

                RunUpgradeCard picked =
                    inCategory[random.Next(
                        inCategory.Count)];

                result.Add(picked);

                available.Remove(picked);
            }

            // 서로 다른 계열이 모자라면 남은 카드에서 채운다.
            while (result.Count < choiceCount &&
                   available.Count > 0)
            {
                int index =
                    random.Next(
                        available.Count);

                result.Add(
                    available[index]);

                available.RemoveAt(index);
            }

            return result;
        }

        private static List<RunUpgradeCard> CollectAvailable(
            RunUpgradeState state)
        {
            List<RunUpgradeCard> available =
                new List<RunUpgradeCard>();

            for (int i = 0;
                 i < RunUpgradeTable.Count;
                 i++)
            {
                RunUpgradeCard card =
                    RunUpgradeTable.GetAt(i).Card;

                if (!state.IsMaxed(card) &&
                    !available.Contains(card))
                {
                    available.Add(card);
                }
            }

            return available;
        }

        private static List<RunUpgradeCategory> CollectCategories(
            List<RunUpgradeCard> cards)
        {
            List<RunUpgradeCategory> categories =
                new List<RunUpgradeCategory>();

            for (int i = 0;
                 i < cards.Count;
                 i++)
            {
                RunUpgradeCategory category =
                    RunUpgradeTable.Get(
                        cards[i]).Category;

                if (!categories.Contains(category))
                {
                    categories.Add(category);
                }
            }

            return categories;
        }

        private static List<RunUpgradeCard> FilterByCategory(
            List<RunUpgradeCard> cards,
            RunUpgradeCategory category)
        {
            List<RunUpgradeCard> filtered =
                new List<RunUpgradeCard>();

            for (int i = 0;
                 i < cards.Count;
                 i++)
            {
                if (RunUpgradeTable.Get(
                        cards[i]).Category ==
                    category)
                {
                    filtered.Add(cards[i]);
                }
            }

            return filtered;
        }

        private static void Shuffle<T>(
            List<T> list,
            Random random)
        {
            for (int i = list.Count - 1;
                 i > 0;
                 i--)
            {
                int j =
                    random.Next(
                        i + 1);

                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
    }
}
