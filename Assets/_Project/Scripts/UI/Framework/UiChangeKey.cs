namespace ProjectTheta.UI.Framework
{
    /// <summary>
    /// "표시할 값이 바뀌었을 때만 문자열을 만든다"를 돕는 도구다.
    ///
    /// HUD는 매 프레임 갱신되는데, 체력·정기처럼 대부분의 프레임에서 값이 그대로다.
    /// 그런데도 <c>$"체력 {현재} / {최대}"</c>를 매 프레임 만들면 프레임마다
    /// 문자열이 십수 개씩 생겼다가 버려져 가비지 수집이 주기적으로 멈칫거린다.
    ///
    /// 값을 정수 키 하나로 접어 두고, 키가 같으면 문자열 만들기를 건너뛴다.
    /// </summary>
    public static class UiChangeKey
    {
        /// <summary>한 번도 그리지 않은 상태다. 첫 프레임은 반드시 갱신된다.</summary>
        public const long Unset = long.MinValue;

        /// <summary>키가 바뀌었으면 기록하고 true를 돌려준다.</summary>
        public static bool Changed(
            ref long cache,
            long key)
        {
            if (cache == key)
            {
                return false;
            }

            cache = key;

            return true;
        }

        /// <summary>정수 두 개를 키 하나로 접는다.</summary>
        public static long Of(
            int a,
            int b)
        {
            return ((long)a << 32) |
                   (uint)b;
        }

        /// <summary>정수 세 개를 키 하나로 접는다. 세 번째는 작은 값(상태 번호 등)이어야 한다.</summary>
        public static long Of(
            int a,
            int b,
            int state)
        {
            return Of(
                a,
                b) *
                   31L +
                   state;
        }

        /// <summary>소수 첫째 자리까지 보이는 값의 키다. 화면 표시가 안 바뀌면 키도 안 바뀐다.</summary>
        public static int Tenths(
            float value)
        {
            return (int)System.Math.Round(
                value * 10f,
                System.MidpointRounding.AwayFromZero);
        }

        /// <summary>정수로 보이는 값의 키다.</summary>
        public static int Whole(
            float value)
        {
            return (int)System.Math.Round(
                value,
                System.MidpointRounding.AwayFromZero);
        }
    }
}
