using UnityEngine;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 쇼핑몰 자리표다 (25일차). 방해 세력 배치 · 셔터 · 기둥 · 보안실 직원 자리를 맞춘다.
    /// 3층짜리라 층 번호 0~2로 읽는다.
    /// </summary>
    public static class MallLayout
    {
        /// <summary>층마다 셔터 두 곳이다. 셔터 사이가 매장 구역이다.</summary>
        public static readonly float[] ShutterX =
        {
            -5f, 5f
        };

        /// <summary>CCTV 자리다. [층][순번]</summary>
        public static readonly float[][] CameraX =
        {
            new[] { -8f, 8f },
            new[] { -3f, 9f },
            new[] { -9f, 0f, 9f }
        };

        /// <summary>기둥 · 진열대(사각지대) 자리다. CCTV · 보안요원 시야를 가린다.</summary>
        public static readonly float[] PillarX =
        {
            -1.5f, 3f
        };

        /// <summary>보안실 직원이 서는 자리다. 이 직원을 최면하면 그 층 CCTV가 꺼진다.</summary>
        public const float SecurityRoomX = -10f;

        public static float[] GetCameraX(
            int floor)
        {
            return floor >= 0 &&
                   floor < CameraX.Length
                ? CameraX[floor]
                : new float[0];
        }
    }

    /// <summary>
    /// 오피스 타워 자리표다 (25일차). 탕비실 · 회의실 · 출입증 게이트 · 출입증 NPC 자리를 맞춘다.
    /// </summary>
    public static class OfficeLayout
    {
        /// <summary>지금 스테이지가 오피스 타워인지다. 부트스트랩이 구역마다 정한다 (탕비실 배율에 쓴다).</summary>
        public static bool Active { get; set; }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active = false;
        }

        public static readonly float[] PantryX =
        {
            -6f, 6f, -6f
        };

        public static readonly float[] MeetingRoomX =
        {
            8f, -8f, 5f
        };

        /// <summary>위층 계단 바로 앞이다. 게이트는 맨 위층을 뺀 층마다 있다.</summary>
        public static readonly float GateX =
            FloorLayout.UpStairX - 1.6f;

        /// <summary>층마다 출입증을 가진 NPC(비서 · 팀장)가 서는 자리다.</summary>
        public const float PassHolderX = -3f;

        /// <summary>최상층 특수 대상(대표 비서) 자리다.</summary>
        public const float ExecutiveX = 9f;

        public static float GetPantryX(
            int floor)
        {
            return PantryX[Clamp(floor, PantryX.Length)];
        }

        public static float GetMeetingRoomX(
            int floor)
        {
            return MeetingRoomX[Clamp(floor, MeetingRoomX.Length)];
        }

        private static int Clamp(
            int floor,
            int length)
        {
            return floor < 0
                ? 0
                : floor >= length
                    ? length - 1
                    : floor;
        }
    }
}
