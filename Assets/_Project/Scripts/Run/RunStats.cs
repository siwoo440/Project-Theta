using System;
using System.Collections.Generic;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 한 판의 기록이다 (20일차).
    ///
    /// "느낌상 느리다"를 숫자로 확인하려고 만들었다.
    /// Unity 없이 도는 순수 계산이라 EditMode 테스트로 확인한다.
    /// 기록을 모으는 쪽은 <see cref="RunStatsRecorder"/>다.
    /// </summary>
    [Serializable]
    public sealed class RunStats
    {
        public float[] FloorSeconds;
        public float TotalSeconds;
        public float FocusEmptySeconds;

        public int HypnosisCount;
        public int ReclaimCount;
        public int StolenCount;
        public int RecoveredFollowers;
        public int RecoveredEssence;
        public int RampageWindups;
        public int RampageSurvived;
        public int CaptureCount;
        public int DuelWins;
        public int HighestFloor;

        /// <summary>레벨이 오른 시각(판 시작부터 초)이다. 순서대로 2레벨, 3레벨…</summary>
        public List<float> LevelUpTimes =
            new List<float>();

        /// <summary>치트를 쓴 판이다. 이 판 숫자로 밸런스를 판단하지 않는다.</summary>
        public bool Cheated;

        public RunStats(
            int floorCount)
        {
            FloorSeconds =
                new float[Math.Max(1, floorCount)];
        }

        public int FloorCount =>
            FloorSeconds.Length;

        /// <summary>
        /// 시간을 흘린다. 멈춘 시간(카드 화면)은 호출하는 쪽이 deltaTime 0으로 넘긴다.
        /// 범위를 벗어난 층은 전체 시간에만 더한다.
        /// </summary>
        public void Tick(
            float deltaTime,
            int currentFloor,
            bool focusEmpty)
        {
            if (deltaTime <= 0f ||
                float.IsNaN(deltaTime))
            {
                return;
            }

            TotalSeconds += deltaTime;

            if (currentFloor >= 0 &&
                currentFloor < FloorSeconds.Length)
            {
                FloorSeconds[currentFloor] += deltaTime;

                HighestFloor =
                    Math.Max(
                        HighestFloor,
                        currentFloor);
            }

            if (focusEmpty)
            {
                FocusEmptySeconds += deltaTime;
            }
        }

        public void RecordHypnosis(
            bool wasReclaim)
        {
            HypnosisCount++;

            if (wasReclaim)
            {
                ReclaimCount++;
            }
        }

        public void RecordRecovery(
            int followers,
            int essence)
        {
            RecoveredFollowers += Math.Max(0, followers);
            RecoveredEssence += Math.Max(0, essence);
        }

        /// <summary>
        /// 레벨업을 기록한다. 한 번에 여러 레벨이 올라도 레벨마다 한 줄씩 남긴다.
        /// 이미 기록한 레벨이면 무시한다.
        /// </summary>
        public void RecordLevel(
            int newLevel)
        {
            // 1레벨은 시작 레벨이므로 2레벨부터 LevelUpTimes[0]이다.
            while (LevelUpTimes.Count < newLevel - 1)
            {
                LevelUpTimes.Add(
                    TotalSeconds);
            }
        }

        /// <summary>집중력이 0이었던 시간 비율이다. 0초 판이면 0이다.</summary>
        public float FocusEmptyRatio =>
            TotalSeconds <= 0f
                ? 0f
                : Math.Min(
                    1f,
                    FocusEmptySeconds / TotalSeconds);

        /// <summary>층별 막대 길이다. 가장 오래 머문 층이 1이다.</summary>
        public float GetFloorShare(
            int floor)
        {
            if (floor < 0 ||
                floor >= FloorSeconds.Length)
            {
                return 0f;
            }

            float longest = 0f;

            for (int i = 0;
                 i < FloorSeconds.Length;
                 i++)
            {
                longest =
                    Math.Max(
                        longest,
                        FloorSeconds[i]);
            }

            return longest <= 0f
                ? 0f
                : FloorSeconds[floor] / longest;
        }

        public static string FormatTime(
            float seconds)
        {
            int total =
                (int)Math.Max(
                    0f,
                    Math.Floor(seconds));

            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
