namespace ProjectTheta.Map
{
    /// <summary>
    /// 맵 한 층에 만들 수 있는 오브젝트 수 예산이다 (28일차).
    /// 바닥 무늬를 구운 뒤 가장 많은 장소도 100개 안팎이라, 여유를 두고 상한을 정한다.
    /// 넘으면 빌더가 경고를 남기고 Unity 테스트가 실패한다.
    /// </summary>
    public static class MapBudget
    {
        public const int MaxObjectsPerFloor = 180;

        public static bool IsOver(
            int createdCount)
        {
            return createdCount > MaxObjectsPerFloor;
        }
    }
}
