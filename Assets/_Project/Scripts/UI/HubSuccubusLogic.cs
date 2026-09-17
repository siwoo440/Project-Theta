using System;
using System.Collections.Generic;
using ProjectTheta.Save;
using ProjectTheta.Story;

namespace ProjectTheta.UI
{
    /// <summary>방 안 서큐버스의 자세다. 자세마다 그림 파일이 다르다.</summary>
    public enum HubSuccubusPose
    {
        Stand = 0,
        Sit = 1
    }

    /// <summary>서큐버스가 서 있을 수 있는 자리 하나다. 좌표는 발끝(1920×1080 가운데 기준)이다.</summary>
    public struct HubSuccubusSpot
    {
        public readonly string Name;
        public readonly float X;
        public readonly float Y;
        public readonly HubSuccubusPose Pose;

        public HubSuccubusSpot(
            string name,
            float x,
            float y,
            HubSuccubusPose pose)
        {
            Name = name;
            X = x;
            Y = y;
            Pose = pose;
        }
    }

    /// <summary>
    /// 허브 방 안의 서큐버스 규칙이다 (36일차).
    ///
    ///   허브에 들어올 때마다 자리 6곳 중 하나에 무작위로 있다(바로 전 자리는 피한다).
    ///   누르면 머리 오른쪽 위 말풍선에 대사가 나온다. 다시 누르면 다른 대사다.
    ///   대사는 진행 상황(첫 출격 전 · 강화 가능 · 라이벌 · 엔딩 · 지배도)에 맞춰 섞인다.
    ///
    /// 그림 파일 (Resources/Characters/Succubus/):
    ///   Room_Stand.png · Room_Sit.png → 없으면 Idle.png → 없으면 임시 실루엣
    ///   발끝이 그림 아래 가운데에 오게 그리면 된다.
    /// </summary>
    public static class HubSuccubusLogic
    {
        public const string ArtRoot = "Characters/Succubus";

        /// <summary>화면에서의 키(px)다.</summary>
        public const float StandHeight = 300f;
        public const float SitHeight = 220f;

        public const float BubbleWidth = 340f;
        public const float BubbleMinHeight = 64f;
        public const float BubblePadding = 16f;

        /// <summary>말풍선이 이 x를 넘으면 머리 왼쪽 위로 뒤집는다(화면 오른쪽 끝 여백 포함).</summary>
        public const float ScreenHalfWidth = 960f;
        public const float ScreenMargin = 24f;

        /// <summary>
        /// 자리 6곳이다. 방 물건(노트 · 일기장 · 창문 · 책장 · 시계)의 누르는 자리와 겹치지 않게 골랐다.
        /// </summary>
        public static readonly HubSuccubusSpot[] Spots =
        {
            new HubSuccubusSpot("창가", 90f, -330f, HubSuccubusPose.Stand),
            new HubSuccubusSpot("책상 옆", -120f, -250f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("침대 위", 560f, -222f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("책장 앞", -520f, -300f, HubSuccubusPose.Stand),
            new HubSuccubusSpot("러그 위", -60f, -405f, HubSuccubusPose.Sit),
            new HubSuccubusSpot("침대 앞", 430f, -345f, HubSuccubusPose.Stand)
        };

        public static readonly string[] CommonLines =
        {
            "오늘 밤은 어디로 가 볼까?",
            "창밖 불빛 예쁘지? 언젠가 전부 내 거야.",
            "노트에 계약을 적어 두면 더 강해질 수 있어.",
            "시계는 거짓말을 안 해. 밤은 생각보다 짧아.",
            "일기장은 몰래 보지 마… 아니, 봐도 돼.",
            "너무 오래 쉬면 정기가 식어 버려.",
            "학생 방이라 좁지만, 나름 아늑하지?",
            "사람들 마음은 생각보다 쉽게 흔들려. 조심해서 다뤄야 해."
        };

        public static string GetArtPath(
            HubSuccubusPose pose)
        {
            return pose == HubSuccubusPose.Sit
                ? $"{ArtRoot}/Room_Sit"
                : $"{ArtRoot}/Room_Stand";
        }

        public static string FallbackArtPath =>
            $"{ArtRoot}/Idle";

        public static float GetHeight(
            HubSuccubusPose pose)
        {
            return pose == HubSuccubusPose.Sit
                ? SitHeight
                : StandHeight;
        }

        /// <summary>자리 번호를 고른다. 자리가 둘 이상이면 바로 전 자리는 피한다.</summary>
        public static int PickSpot(
            Random random,
            int lastSpot)
        {
            int count = Spots.Length;

            if (count <= 1)
            {
                return 0;
            }

            if (lastSpot < 0 ||
                lastSpot >= count)
            {
                return random.Next(count);
            }

            // 전 자리를 뺀 나머지 중에서 고른다.
            int pick = random.Next(count - 1);

            return pick >= lastSpot
                ? pick + 1
                : pick;
        }

        /// <summary>
        /// 지금 진행 상황에 맞는 대사 목록이다.
        /// 상황 대사는 두 번 넣어 공통 대사보다 자주 나오게 한다.
        /// </summary>
        public static List<string> GetLines(
            SaveData save)
        {
            List<string> lines = new List<string>(CommonLines);

            void Add(string line)
            {
                lines.Add(line);
                lines.Add(line);
            }

            if (save == null)
            {
                return lines;
            }

            PlayStats stats = save.Stats ?? new PlayStats();
            int cleared = AchievementLogic.GetValue(save, AchievementStat.LocationsCleared);
            int dominion = DominionLogic.GetPercent(save);
            bool ended = stats.Endings > 0;

            if (stats.Attempts <= 0)
            {
                Add("처음이니까 연수원부터 가 보자. 지친 사람이 많대.");
                Add("창문을 누르면 바로 도시로 나갈 수 있어.");
            }

            if (CanBuyAnyUpgrade(save))
            {
                Add("계약 정기가 모였어. 노트에서 강화해 볼까?");
            }

            if (StoryLogic.IsSeen(save, "rival_taunt_3") &&
                !ended)
            {
                Add("그 라이벌… 루프탑에서 날 기다리고 있겠지.");
            }

            if (cleared >= 6 &&
                !ended)
            {
                Add("초대장이 왔어. 루프탑 클럽, 이제 갈 때가 됐나?");
            }

            if (ended)
            {
                Add("라이벌을 이겼는데도, 밤이 또 기다려져.");

                if (AchievementLogic.GetValue(save, AchievementStat.NightLocations) <= 0)
                {
                    Add("심야 모드, 아직 안 가 봤지? 지도에서 ☾를 눌러 봐.");
                }
                else
                {
                    Add("심야의 도시는 역시 짜릿해.");
                }
            }

            if (dominion >= 100)
            {
                Add("이 도시의 밤은 이제 전부 내 거야.");
            }
            else if (dominion >= 50)
            {
                Add("도시의 절반이 내 이름을 속삭여.");
            }

            return lines;
        }

        public static bool CanBuyAnyUpgrade(
            SaveData save)
        {
            if (save == null)
            {
                return false;
            }

            foreach (UpgradeTrack track in Enum.GetValues(typeof(UpgradeTrack)))
            {
                int level = SaveDataLogic.GetUpgradeLevel(save, track);

                if (!UpgradeLogic.IsMaxLevel(level) &&
                    UpgradeLogic.CanPurchase(level, save.ContractEssence))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>대사를 고른다. 둘 이상이면 바로 전 대사와 다른 것이다.</summary>
        public static string PickLine(
            IList<string> lines,
            Random random,
            string last)
        {
            if (lines == null ||
                lines.Count == 0)
            {
                return string.Empty;
            }

            for (int attempt = 0; attempt < 8; attempt++)
            {
                string line = lines[random.Next(lines.Count)];

                if (line != last)
                {
                    return line;
                }
            }

            foreach (string line in lines)
            {
                if (line != last)
                {
                    return line;
                }
            }

            return lines[0];
        }

        /// <summary>말풍선이 떠 있는 시간(초)이다. 글이 길수록 오래.</summary>
        public static float GetBubbleSeconds(
            string text)
        {
            int length = text == null ? 0 : text.Length;

            return Math.Max(2.5f, Math.Min(6f, 1.5f + length * 0.08f));
        }

        /// <summary>
        /// 말풍선을 머리 왼쪽 위로 뒤집어야 하는지다.
        /// 오른쪽 위에 두면 화면 밖으로 나갈 때만 뒤집는다.
        /// </summary>
        public static bool ShouldFlip(
            float characterX,
            float headOffsetX,
            float bubbleWidth)
        {
            return characterX + headOffsetX + bubbleWidth > ScreenHalfWidth - ScreenMargin;
        }
    }
}
