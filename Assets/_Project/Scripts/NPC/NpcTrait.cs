namespace ProjectTheta.NPC
{
    /// <summary>
    /// 기획서 5.4절 저항 패턴을 NPC 특성으로 옮긴 것이다.
    /// 반격형·경비 호출형은 집중력 자원과 방해 NPC 스폰이 필요하므로 이후 일차에서 추가한다.
    /// </summary>
    public enum NpcTrait
    {
        /// <summary>일반형 - 특별한 저항이 없다. 그냥 평범하게 최면당한다.</summary>
        Plain,

        /// <summary>시선 회피형 - 주기적으로 고개를 돌려 최면 게이지 상승을 잠시 끊는다.</summary>
        GazeAverter,

        /// <summary>도주형 - 최면 대상이 되면 플레이어에게서 멀어진다.</summary>
        Fleer,

        /// <summary>고저항형 - 최면이 느린 대신 정기 가치가 높다.</summary>
        Stubborn,

        /// <summary>각성 지원형 - 주변 플레이어 동행 NPC의 지배 수치를 깎는다.</summary>
        AwakeningAura
    }
}
