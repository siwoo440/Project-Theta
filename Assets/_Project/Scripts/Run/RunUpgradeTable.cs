using System;

namespace ProjectTheta.Run
{
    /// <summary>강화 카드 계열이다. 한 번에 보여주는 3장은 서로 다른 계열에서 뽑는다.</summary>
    public enum RunUpgradeCategory
    {
        Hypnosis = 0,
        Command = 1,
        Harvest = 2,
        Mobility = 3
    }

    public enum RunUpgradeCard
    {
        /// <summary>최면 속도 증가.</summary>
        BindingGaze = 0,

        /// <summary>체인 최면 최대 대상 +1.</summary>
        ChainImprint = 1,

        /// <summary>안정 동행 한도 +1.</summary>
        WideEmbrace = 2,

        /// <summary>동행자 충동 상승 감소.</summary>
        CalmWhisper = 3,

        /// <summary>회수 정기 증가.</summary>
        EssenceDrain = 4,

        /// <summary>경험치 획득 증가.</summary>
        Insight = 5,

        /// <summary>이동 속도 증가.</summary>
        LightStep = 6,

        /// <summary>대시 집중력 비용 감소.</summary>
        EasyBreath = 7
    }

    /// <summary>카드 한 장의 정의다.</summary>
    [Serializable]
    public struct RunUpgradeProfile
    {
        public RunUpgradeCard Card;
        public RunUpgradeCategory Category;
        public string DisplayName;

        /// <summary>{0} 자리에 스택 반영 수치가 들어간다.</summary>
        public string DescriptionFormat;

        /// <summary>1스택의 효과량이다. 비율이면 0.2 = 20%, 정수 효과면 1 = +1.</summary>
        public float ValuePerStack;

        public int MaximumStacks;

        /// <summary>정수 효과(인원 수 등)면 스택 감쇠 없이 그대로 더한다.</summary>
        public bool IsInteger;

        public RunUpgradeProfile(
            RunUpgradeCard card,
            RunUpgradeCategory category,
            string displayName,
            string descriptionFormat,
            float valuePerStack,
            int maximumStacks,
            bool isInteger)
        {
            Card = card;
            Category = category;
            DisplayName = displayName;
            DescriptionFormat = descriptionFormat;
            ValuePerStack = valuePerStack;
            MaximumStacks = maximumStacks;
            IsInteger = isInteger;
        }
    }

    /// <summary>
    /// 강화 카드 표다.
    ///
    /// 14일차 방식 그대로, 자산이 <see cref="Override"/>를 채우면 그 값을 쓰고
    /// 비어 있으면 코드 기본표를 쓴다.
    /// </summary>
    public static class RunUpgradeTable
    {
        public static readonly RunUpgradeProfile[] Profiles =
        {
            new RunUpgradeProfile(
                RunUpgradeCard.BindingGaze,
                RunUpgradeCategory.Hypnosis,
                "속박의 눈",
                "최면 속도 +{0}",
                0.20f,
                3,
                false),

            new RunUpgradeProfile(
                RunUpgradeCard.ChainImprint,
                RunUpgradeCategory.Hypnosis,
                "연쇄 각인",
                "체인 최면 대상 +{0}명",
                1f,
                2,
                true),

            new RunUpgradeProfile(
                RunUpgradeCard.WideEmbrace,
                RunUpgradeCategory.Command,
                "넓은 품",
                "안정 동행 한도 +{0}명",
                1f,
                2,
                true),

            new RunUpgradeProfile(
                RunUpgradeCard.CalmWhisper,
                RunUpgradeCategory.Command,
                "차분한 속삭임",
                "동행 충동 상승 −{0}",
                0.20f,
                3,
                false),

            new RunUpgradeProfile(
                RunUpgradeCard.EssenceDrain,
                RunUpgradeCategory.Harvest,
                "정기 흡수",
                "회수 정기 +{0}",
                0.15f,
                3,
                false),

            new RunUpgradeProfile(
                RunUpgradeCard.Insight,
                RunUpgradeCategory.Harvest,
                "통찰",
                "경험치 획득 +{0}",
                0.20f,
                3,
                false),

            new RunUpgradeProfile(
                RunUpgradeCard.LightStep,
                RunUpgradeCategory.Mobility,
                "가벼운 발",
                "이동 속도 +{0}",
                0.12f,
                3,
                false),

            new RunUpgradeProfile(
                RunUpgradeCard.EasyBreath,
                RunUpgradeCategory.Mobility,
                "여유 호흡",
                "대시 집중력 비용 −{0}",
                0.25f,
                3,
                false)
        };

        public static RunUpgradeProfile[] Override { get; set; }

        private static RunUpgradeProfile[] Active =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Profiles;

        public static int Count =>
            Active.Length;

        public static RunUpgradeProfile GetAt(
            int index)
        {
            return Active[index];
        }

        public static RunUpgradeProfile Get(
            RunUpgradeCard card)
        {
            RunUpgradeProfile[] active =
                Active;

            for (int i = 0;
                 i < active.Length;
                 i++)
            {
                if (active[i].Card == card)
                {
                    return active[i];
                }
            }

            return Profiles[0];
        }
    }
}
