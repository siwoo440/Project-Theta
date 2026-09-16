namespace ProjectTheta.Stage
{
    public enum StageState
    {
        Running,
        Cleared,
        FailedByTime,
        FailedByHealth,

        /// <summary>일시정지 메뉴에서 포기했다 (31일차). 실패로 치고 계약 정기는 0이다.</summary>
        Abandoned
    }
}
