namespace ProjectTheta.Items
{
    /// <summary>
    /// 기획서 A.8절 소비 아이템이다.
    /// 충동 안정 · 집중력 회복 · 라이벌 영향 차단 · 이동 보조 네 가지 역할로 제한한다.
    /// </summary>
    public enum ConsumableItem
    {
        None,

        /// <summary>진정제 - 동행 전원의 충동을 크게 낮춘다.</summary>
        Sedative,

        /// <summary>집중 회복제 - 집중력을 즉시 회복한다.</summary>
        FocusTonic,

        /// <summary>차단 부적 - 일정 시간 경쟁자의 쟁탈을 무효화한다.</summary>
        WardCharm,

        /// <summary>질주약 - 일정 시간 이동과 대시를 강화한다.</summary>
        SprintDraught
    }

    public sealed class ConsumableItemProfile
    {
        public ConsumableItemProfile(
            ConsumableItem item,
            string displayName,
            string description,
            float magnitude,
            float durationSeconds)
        {
            Item = item;
            DisplayName = displayName;
            Description = description;
            Magnitude = magnitude;
            DurationSeconds = durationSeconds;
        }

        public ConsumableItem Item { get; }

        public string DisplayName { get; }

        public string Description { get; }

        /// <summary>효과 크기다. 아이템마다 의미가 다르다.</summary>
        public float Magnitude { get; }

        /// <summary>지속 시간이다. 즉시 효과 아이템은 0이다.</summary>
        public float DurationSeconds { get; }
    }

    public static class ConsumableItemTable
    {
        /// <summary>자산에서 읽은 아이템 표다. null이면 아래 기본 표를 쓴다.</summary>
        public static ConsumableItemProfile[] Override { get; set; }

        private static ConsumableItemProfile[] Active =>
            Override != null &&
            Override.Length > 0
                ? Override
                : Profiles;

        private static readonly ConsumableItemProfile[] Profiles =
        {
            new ConsumableItemProfile(
                ConsumableItem.None,
                "-",
                "비어 있음",
                0f,
                0f),

            new ConsumableItemProfile(
                ConsumableItem.Sedative,
                "진정제",
                "동행 전원 충동 -60",
                60f,
                0f),

            new ConsumableItemProfile(
                ConsumableItem.FocusTonic,
                "집중 회복제",
                "집중력 +60",
                60f,
                0f),

            new ConsumableItemProfile(
                ConsumableItem.WardCharm,
                "차단 부적",
                "6초간 쟁탈 무효",
                0f,
                6f),

            new ConsumableItemProfile(
                ConsumableItem.SprintDraught,
                "질주약",
                "8초간 이동 +40%",
                0.40f,
                8f)
        };

        /// <summary>스테이지 시작 시 무작위로 지급할 후보다.</summary>
        public static readonly ConsumableItem[] StartingPool =
        {
            ConsumableItem.Sedative,
            ConsumableItem.FocusTonic,
            ConsumableItem.WardCharm,
            ConsumableItem.SprintDraught
        };

        public static int Count =>
            Active.Length;

        public static ConsumableItemProfile Get(
            ConsumableItem item)
        {
            ConsumableItemProfile[] table =
                Active;

            for (int i = 0;
                 i < table.Length;
                 i++)
            {
                if (table[i].Item ==
                    item)
                {
                    return table[i];
                }
            }

            return table[0];
        }
    }
}
