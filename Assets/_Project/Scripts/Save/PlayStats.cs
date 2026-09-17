using System;

namespace ProjectTheta.Save
{
    /// <summary>
    /// 지금까지의 플레이 누적 기록이다 (29일차). 지도의 [통계] 창이 보여 준다.
    /// JsonUtility로 저장하므로 public 필드만 쓴다. 예전 세이브에는 없어서 0으로 채워진다.
    /// </summary>
    [Serializable]
    public sealed class PlayStats
    {
        // 전체
        public float TotalSeconds;
        public int Attempts;
        public int Clears;
        public int Endings;

        /// <summary>S등급으로 클리어한 횟수다 (30일차 업적).</summary>
        public int SRanks;

        /// <summary>동행자를 한 번도 빼앗기지 않고 클리어한 횟수다 (32일차 업적 "무결점").</summary>
        public int CleanClears;

        // 최면 · 동행
        public int Hypnosis;
        public int MaxFollowers;
        public int RecoveredFollowers;
        public int Stolen;
        public int Reclaimed;

        // 정기
        public int RecoveredEssence;
        public int ContractEssence;
        public int BestEssence;

        // 위기
        public int RampageWindups;
        public int RampageSurvived;
        public int Captures;
        public int DuelWins;

        // 성장
        public int LevelUps;
        public int CardsPicked;

        public PlayStats Clone()
        {
            return (PlayStats)MemberwiseClone();
        }
    }

    /// <summary>장소 하나의 누적 기록이다 (29일차).</summary>
    [Serializable]
    public sealed class LocationStats
    {
        /// <summary><see cref="Stage.Locations.LocationId"/> 번호다.</summary>
        public int Location;

        public int Attempts;
        public int Clears;
        public int BestEssence;

        /// <summary>가장 빨리 클리어한 시간(초)이다. 클리어한 적이 없으면 0이다.</summary>
        public float BestClearSeconds;

        public string BestRank = "-";

        /// <summary>36일차: 심야 모드로 클리어한 횟수다. 1 이상이면 지도에 ☾.</summary>
        public int NightClears;

        public LocationStats Clone()
        {
            return (LocationStats)MemberwiseClone();
        }
    }
}
