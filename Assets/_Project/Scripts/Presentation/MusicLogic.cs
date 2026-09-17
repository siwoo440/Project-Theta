using System;
using ProjectTheta.Core;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Presentation
{
    /// <summary>배경음악 곡이다. 이름이 곧 파일 이름이다 (Resources/Audio/Music/{이름}).</summary>
    public enum MusicTrack
    {
        None = 0,
        Title = 1,
        Room = 2,
        City = 3,
        Training = 4,
        Beach = 5,
        Subway = 6,
        Fitness = 7,
        Market = 8,
        Mall = 9,
        Office = 10,
        Boss = 11,
        Ending = 12
    }

    /// <summary>음악 긴장 단계다. 오를수록 긴장 겹이 커지고 곡이 살짝 빨라진다.</summary>
    public enum MusicIntensity
    {
        Calm = 0,
        Chase = 1,
        Danger = 2
    }

    /// <summary>
    /// 배경음악 규칙이다 (37일차).
    ///
    ///   메인 메뉴 → Title   허브 → Room   지도 → City
    ///   장소 → 장소마다 한 곡 (루프탑 클럽은 Boss)   보스 엔딩 → Ending
    ///   추격 → 긴장 겹 60%     위기(남은 15초 · 체력 30% 이하) → 긴장 겹 100% · 5% 빠르게
    ///   심야 모드 → 6% 낮고 느리게     대사 중 → 음악 60%
    /// 곡 파일({이름}.wav)과 긴장 겹({이름}_Tension.wav)은 Tools/generate_temp_audio.py가 만든 임시 음원이다.
    /// </summary>
    public static class MusicLogic
    {
        public const string Folder = "Audio/Music";
        public const string TensionSuffix = "_Tension";

        /// <summary>효과음보다 음악을 조금 작게 둔다.</summary>
        public const float BaseVolume = 0.55f;

        public const float CrossfadeSeconds = 1.2f;
        public const float TensionFadeSeconds = 0.8f;
        public const float DialogueDuck = 0.6f;

        public const float DangerSeconds = 15f;
        public const float DangerHealth = 0.3f;

        public const float NightPitch = 0.94f;
        public const float DangerPitch = 1.05f;

        public static string GetPath(
            MusicTrack track)
        {
            return track == MusicTrack.None
                ? string.Empty
                : $"{Folder}/{track}";
        }

        public static string GetTensionPath(
            MusicTrack track)
        {
            return track == MusicTrack.None
                ? string.Empty
                : $"{Folder}/{track}{TensionSuffix}";
        }

        public static MusicTrack GetLocationTrack(
            LocationId location)
        {
            switch (location)
            {
                case LocationId.TrainingCenter:
                    return MusicTrack.Training;

                case LocationId.Beach:
                    return MusicTrack.Beach;

                case LocationId.SubwayStation:
                    return MusicTrack.Subway;

                case LocationId.FitnessCenter:
                    return MusicTrack.Fitness;

                case LocationId.NightMarket:
                    return MusicTrack.Market;

                case LocationId.ShoppingMall:
                    return MusicTrack.Mall;

                case LocationId.OfficeTower:
                    return MusicTrack.Office;

                case LocationId.RooftopClub:
                    return MusicTrack.Boss;

                default:
                    return MusicTrack.City;
            }
        }

        /// <summary>지금 씬에서 틀 곡이다. 부트 씬 · 모르는 씬은 음악을 바꾸지 않는다(None).</summary>
        public static MusicTrack GetTrack(
            string sceneName,
            LocationId location,
            bool endingPlaying)
        {
            if (endingPlaying)
            {
                return MusicTrack.Ending;
            }

            if (SceneFlowLogic.IsStageScene(sceneName))
            {
                return GetLocationTrack(location);
            }

            switch (sceneName)
            {
                case SceneNames.MainMenu:
                    return MusicTrack.Title;

                case SceneNames.Hub:
                    return MusicTrack.Room;

                case SceneNames.Map:
                    return MusicTrack.City;

                default:
                    return MusicTrack.None;
            }
        }

        /// <summary>곡을 바꿔야 하는지다. None이면 지금 곡을 그대로 둔다.</summary>
        public static bool ShouldSwitch(
            MusicTrack current,
            MusicTrack wanted)
        {
            return wanted != MusicTrack.None &&
                   wanted != current;
        }

        public static MusicIntensity GetIntensity(
            bool running,
            bool chasing,
            float remainingSeconds,
            float healthNormalized)
        {
            if (!running)
            {
                return MusicIntensity.Calm;
            }

            if ((remainingSeconds > 0f && remainingSeconds <= DangerSeconds) ||
                (healthNormalized > 0f && healthNormalized <= DangerHealth))
            {
                return MusicIntensity.Danger;
            }

            return chasing
                ? MusicIntensity.Chase
                : MusicIntensity.Calm;
        }

        public static float GetTensionVolume(
            MusicIntensity intensity)
        {
            switch (intensity)
            {
                case MusicIntensity.Chase:
                    return 0.6f;

                case MusicIntensity.Danger:
                    return 1f;

                default:
                    return 0f;
            }
        }

        public static float GetPitch(
            bool night,
            MusicIntensity intensity)
        {
            float pitch = night ? NightPitch : 1f;

            return intensity == MusicIntensity.Danger
                ? pitch * DangerPitch
                : pitch;
        }

        /// <summary>최종 음량이다. (설정 음악 음량, 대사 중인지, 크로스페이드 0~1)</summary>
        public static float GetVolume(
            float settingVolume,
            bool dialogue,
            float fade)
        {
            float volume =
                BaseVolume *
                Clamp01(settingVolume) *
                Clamp01(fade);

            return dialogue
                ? volume * DialogueDuck
                : volume;
        }

        /// <summary>긴장 겹을 곡 박자에 맞춰 시작할 위치(샘플)다. 곡 길이는 겹 길이의 배수다.</summary>
        public static int GetTensionSample(
            int musicSample,
            int tensionLength)
        {
            if (tensionLength <= 0)
            {
                return 0;
            }

            int position = musicSample % tensionLength;

            return position < 0
                ? position + tensionLength
                : position;
        }

        /// <summary>크로스페이드 한 프레임 진행량이다.</summary>
        public static float Step(
            float current,
            float target,
            float deltaTime,
            float seconds)
        {
            if (seconds <= 0f)
            {
                return target;
            }

            float step = Math.Abs(deltaTime) / seconds;

            return current < target
                ? Math.Min(target, current + step)
                : Math.Max(target, current - step);
        }

        /// <summary>대사 글자 소리를 낼 차례인지다(두 글자마다 한 번, 공백은 건너뜀).</summary>
        public static bool ShouldBlip(
            int previousVisible,
            int visible,
            string text)
        {
            if (text == null ||
                visible <= previousVisible)
            {
                return false;
            }

            for (int i = previousVisible; i < visible && i < text.Length; i++)
            {
                if (i % 2 == 0 &&
                    !char.IsWhiteSpace(text[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>말하는 사람마다 다른 대사 글자 소리 높이다.</summary>
        public static float GetBlipPitch(
            string speaker)
        {
            switch (speaker)
            {
                case Story.StoryCatalog.Me:
                    return 1.25f;

                case Story.StoryCatalog.Rival:
                    return 1.1f;

                case "":
                case null:
                    return 0.9f;

                default:
                    int sum = 0;

                    foreach (char c in speaker)
                    {
                        sum += c;
                    }

                    return 0.8f + (sum % 5) * 0.04f;
            }
        }

        private static float Clamp01(
            float value)
        {
            return float.IsNaN(value)
                ? 0f
                : Math.Max(0f, Math.Min(1f, value));
        }
    }
}
