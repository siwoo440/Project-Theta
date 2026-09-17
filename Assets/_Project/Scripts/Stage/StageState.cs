namespace ProjectTheta.Stage
{
    public enum StageState
    {
        Running,
        Cleared,
        FailedByTime,
        FailedByHealth,

        /// <summary>일시정지 메뉴에서 포기했다 (31일차). 실패로 치고 계약 정기는 0이다.</summary>
        Abandoned,

        /// <summary>
        /// 목표를 채웠지만 아직 끝나지 않았다 (33일차). 게임은 계속되고,
        /// 1F 회수 지점에서 탈출하거나 제한 시간이 끝나면 클리어다 (<see cref="StageExitLogic"/>).
        /// </summary>
        ExitReady
    }
}
