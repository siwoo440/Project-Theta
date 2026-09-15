using UnityEngine;

namespace ProjectTheta.Core
{
    /// <summary>
    /// 디버그 패널의 치트 상태다 (20일차).
    ///
    /// 게임 규칙 코드는 켜져 있는지만 본다. 켜고 끄는 것은 디버그 패널뿐이다.
    /// 치트를 한 번이라도 쓴 판은 <see cref="UsedThisRun"/>가 켜지고, 판 기록에 "치트"로 남는다.
    /// 치트를 쓴 판의 숫자로 밸런스를 판단하지 않기 위해서다.
    /// </summary>
    public static class DebugCheats
    {
        /// <summary>체력이 깎이지 않는다.</summary>
        public static bool Invincible { get; private set; }

        /// <summary>집중력이 항상 가득 찬다.</summary>
        public static bool InfiniteFocus { get; private set; }

        /// <summary>이번 판에 치트를 한 번이라도 썼는지다.</summary>
        public static bool UsedThisRun { get; private set; }

        public static void SetInvincible(
            bool value)
        {
            Invincible = value;

            if (value)
            {
                MarkUsed();
            }
        }

        public static void SetInfiniteFocus(
            bool value)
        {
            InfiniteFocus = value;

            if (value)
            {
                MarkUsed();
            }
        }

        /// <summary>한 번 쓰고 끝나는 치트(경험치 주기, 층 이동 등)가 호출한다.</summary>
        public static void MarkUsed()
        {
            UsedThisRun = true;
        }

        /// <summary>
        /// 새 판을 시작할 때 부른다.
        /// 무적을 켠 채 다음 판에 들어가 "왜 안 죽지?"를 헤매지 않게 치트는 판마다 꺼진다.
        /// </summary>
        public static void ResetForRun()
        {
            Invincible = false;
            InfiniteFocus = false;
            UsedThisRun = false;
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            ResetForRun();
        }
    }

    /// <summary>
    /// 마우스가 게임 조작이 아닌 창(디버그 패널) 위에 있는지다 (20일차).
    ///
    /// 패널의 슬라이더를 끌다가 좌클릭 최면·우클릭 파동이 같이 나가면 안 된다.
    /// 마우스로 조작하는 게임 코드는 이 값이 true면 입력을 무시한다.
    /// </summary>
    public static class PointerGuard
    {
        public static bool IsOverOverlay { get; set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            IsOverOverlay = false;
        }
    }
}
