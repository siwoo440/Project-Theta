using System;
using System.Collections.Generic;
using System.Text;
using ProjectTheta.Save;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Story
{
    /// <summary>
    /// 어떤 이야기를 언제 보여 줄지 정한다 (36일차).
    ///
    ///   장소 안     : 그 장소 입장 장면(아직 안 봤으면)
    ///   지도 · 허브 : 조건을 채웠는데 아직 안 본 장면들(첫 클리어 · 도발 · 후일담), 표 순서대로
    /// 본 장면은 <see cref="SaveData.SeenStories"/>에 남아 저장 칸마다 따로다.
    /// </summary>
    public static class StoryLogic
    {
        /// <summary>대사가 1초에 나오는 글자 수다.</summary>
        public const float CharactersPerSecond = 40f;

        public static void Normalize(
            SaveData save)
        {
            if (save != null &&
                save.SeenStories == null)
            {
                save.SeenStories = new string[0];
            }
        }

        public static bool IsSeen(
            SaveData save,
            string id)
        {
            return save != null &&
                   save.SeenStories != null &&
                   Array.IndexOf(save.SeenStories, id) >= 0;
        }

        /// <summary>본 장면으로 남긴다. 처음 남겼으면 true다.</summary>
        public static bool MarkSeen(
            SaveData save,
            string id)
        {
            if (save == null ||
                string.IsNullOrEmpty(id) ||
                IsSeen(save, id))
            {
                return false;
            }

            Normalize(save);

            string[] grown = new string[save.SeenStories.Length + 1];
            Array.Copy(save.SeenStories, grown, save.SeenStories.Length);
            grown[grown.Length - 1] = id;
            save.SeenStories = grown;

            return true;
        }

        /// <summary>그 장소에 들어갈 때 보여 줄 장면이다. 이미 봤으면 null이다.</summary>
        public static StoryScene GetEnterScene(
            SaveData save,
            LocationId location)
        {
            foreach (StoryScene scene in StoryCatalog.All)
            {
                if (scene.Trigger == StoryTrigger.LocationEnter &&
                    scene.Location == location &&
                    !IsSeen(save, scene.Id))
                {
                    return scene;
                }
            }

            return null;
        }

        /// <summary>조건을 채웠는지다. 장소 입장 장면은 여기서 다루지 않는다(항상 false).</summary>
        public static bool IsConditionMet(
            SaveData save,
            StoryScene scene)
        {
            if (save == null ||
                scene == null)
            {
                return false;
            }

            switch (scene.Trigger)
            {
                case StoryTrigger.LocationClear:
                    return PlayStatsLogic.Get(save, (int)scene.Location).Clears > 0;

                case StoryTrigger.LocationsCleared:
                    return AchievementLogic.GetValue(save, AchievementStat.LocationsCleared) >= scene.Threshold;

                case StoryTrigger.Endings:
                    return save.Stats != null &&
                           save.Stats.Endings >= scene.Threshold;

                case StoryTrigger.Dominion:
                    return DominionLogic.GetPercent(save) >= scene.Threshold;

                default:
                    return false;
            }
        }

        /// <summary>지도 · 허브에 도착했을 때 보여 줄 장면들이다.</summary>
        public static List<StoryScene> GetPending(
            SaveData save)
        {
            List<StoryScene> pending = new List<StoryScene>();

            foreach (StoryScene scene in StoryCatalog.All)
            {
                if (!IsSeen(save, scene.Id) &&
                    IsConditionMet(save, scene))
                {
                    pending.Add(scene);
                }
            }

            return pending;
        }

        /// <summary>본 장면들이다(표 순서). 다시 보기 목록이다.</summary>
        public static List<StoryScene> GetSeen(
            SaveData save)
        {
            List<StoryScene> seen = new List<StoryScene>();

            foreach (StoryScene scene in StoryCatalog.All)
            {
                if (IsSeen(save, scene.Id))
                {
                    seen.Add(scene);
                }
            }

            return seen;
        }

        /// <summary>그 장소의 입장 · 클리어 장면 중 본 것이다.</summary>
        public static List<StoryScene> GetSeenForLocation(
            SaveData save,
            LocationId location)
        {
            List<StoryScene> seen = new List<StoryScene>();

            foreach (StoryScene scene in StoryCatalog.All)
            {
                if (scene.HasLocation &&
                    scene.Location == location &&
                    IsSeen(save, scene.Id))
                {
                    seen.Add(scene);
                }
            }

            return seen;
        }

        public static int CountForLocation(
            LocationId location)
        {
            int count = 0;

            foreach (StoryScene scene in StoryCatalog.All)
            {
                if (scene.HasLocation &&
                    scene.Location == location)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>지도 상세 창 · 일기장에 쓰는 대본이다.</summary>
        public static string BuildTranscript(
            IList<StoryScene> scenes,
            int totalForLocation)
        {
            if (scenes == null ||
                scenes.Count == 0)
            {
                return "아직 본 이야기가 없습니다.\n\n처음 들어갈 때와 처음 클리어했을 때 이야기가 나옵니다.";
            }

            StringBuilder text = new StringBuilder();

            text.Append($"본 이야기 {scenes.Count} / {totalForLocation}\n\n");

            foreach (StoryScene scene in scenes)
            {
                text.Append($"<color=#E8C15A><b>「{scene.Title}」</b></color>\n");

                foreach (StoryLine line in scene.Lines)
                {
                    text.Append(
                        string.IsNullOrEmpty(line.Speaker)
                            ? $"<i>{line.Text}</i>\n"
                            : $"<color={GetSpeakerColor(line.Speaker)}>{line.Speaker}</color>  {line.Text}\n");
                }

                text.Append('\n');
            }

            return text.ToString().TrimEnd();
        }

        public static string GetSpeakerColor(
            string speaker)
        {
            switch (speaker)
            {
                case StoryCatalog.Me:
                    return "#C9A2FF";

                case StoryCatalog.Rival:
                    return "#FF7FB0";

                case "":
                case null:
                    return "#B8B8C8";

                default:
                    return "#9FD3F0";
            }
        }

        /// <summary>한 글자씩 나오는 중 지금 보일 글자 수다.</summary>
        public static int GetVisibleCharacters(
            float elapsed,
            int length)
        {
            if (length <= 0 ||
                float.IsNaN(elapsed) ||
                elapsed <= 0f)
            {
                return 0;
            }

            return (int)Math.Min(length, Math.Floor(elapsed * CharactersPerSecond));
        }

        public static bool IsLineComplete(
            float elapsed,
            int length)
        {
            return GetVisibleCharacters(elapsed, length) >= length;
        }

        /// <summary>한 줄을 다 보여 주는 데 걸리는 시간이다. 다 보이게 건너뛸 때 쓴다.</summary>
        public static float GetLineSeconds(
            int length)
        {
            return Math.Max(0, length) / CharactersPerSecond;
        }
    }
}
