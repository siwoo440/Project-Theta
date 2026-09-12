namespace ProjectTheta.NPC
{
    /// <summary>
    /// 기획서 15장 기준 NPC 5등급이다.
    /// 등급이 높을수록 최면이 느리지만 정기 가치가 크다.
    /// </summary>
    public enum NpcGrade
    {
        /// <summary>일반</summary>
        Common,

        /// <summary>숙련</summary>
        Skilled,

        /// <summary>희귀</summary>
        Rare,

        /// <summary>특수 - 선행 조건을 만족해야 최면을 시작할 수 있다.</summary>
        Special,

        /// <summary>각성 - 최면 저항이 매우 높고 주변 최면을 흔든다.</summary>
        Awakened
    }
}
