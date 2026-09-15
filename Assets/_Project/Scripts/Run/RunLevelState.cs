using System;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 한 판 동안의 레벨과 경험치다.
    ///
    /// 판이 시작될 때 만들고 끝나면 버린다. 세이브하지 않는다.
    /// </summary>
    public sealed class RunLevelState
    {
        public int Level { get; private set; } = 1;

        /// <summary>현재 레벨 안에서 쌓인 경험치다. 레벨이 오르면 초과분만 남는다.</summary>
        public int CurrentXp { get; private set; }

        /// <summary>이번 판 전체에서 얻은 경험치다. 결과 화면에 쓴다.</summary>
        public int TotalXp { get; private set; }

        public int RequiredXp =>
            RunExperienceLogic.GetRequiredXp(
                Level);

        public bool IsMaxLevel =>
            Level >=
            RunExperienceLogic.MaximumLevel;

        /// <summary>HUD 게이지용 진행률이다. 최대 레벨이면 1이다.</summary>
        public float Progress
        {
            get
            {
                int required =
                    RequiredXp;

                return required <= 0
                    ? 1f
                    : Math.Min(
                        1f,
                        CurrentXp /
                        (float)required);
            }
        }

        /// <summary>
        /// 경험치를 더하고, 이번에 오른 레벨 수를 돌려준다.
        /// 큰 회수 한 번으로 두 레벨이 오를 수 있으므로 여러 번 오르는 경우를 처리한다.
        /// </summary>
        public int AddXp(
            int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            TotalXp += amount;

            if (IsMaxLevel)
            {
                return 0;
            }

            CurrentXp += amount;

            int gained = 0;

            while (!IsMaxLevel)
            {
                int required =
                    RequiredXp;

                if (required <= 0 ||
                    CurrentXp < required)
                {
                    break;
                }

                CurrentXp -= required;

                Level++;

                gained++;
            }

            // 최대 레벨에 닿으면 남은 경험치는 의미가 없으므로 비운다.
            if (IsMaxLevel)
            {
                CurrentXp = 0;
            }

            return gained;
        }
    }
}
