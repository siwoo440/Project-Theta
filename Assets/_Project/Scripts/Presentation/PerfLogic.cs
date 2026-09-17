using System;
using System.Globalization;

namespace ProjectTheta.Presentation
{
    /// <summary>성능 표시 색 단계다 (38일차).</summary>
    public enum PerfGrade
    {
        Good = 0,
        Warn = 1,
        Bad = 2
    }

    /// <summary>
    /// 성능 표시(F3) 계산 규칙이다 (38일차).
    ///
    ///   FPS          최근 창 안 평균 프레임 시간으로 계산
    ///   1% 느린 FPS   최근 창 안에서 가장 느린 1% 프레임의 평균으로 계산
    ///   색           1% 느린 FPS가 30 미만 노랑, 20 미만 빨강
    /// </summary>
    public static class PerfLogic
    {
        public const float TargetFps = 60f;
        public const float WarnFps = 30f;
        public const float BadFps = 20f;

        /// <summary>최근 몇 초를 볼지다.</summary>
        public const float WindowSeconds = 5f;

        /// <summary>링 버퍼 크기다. 240Hz로 5초를 담을 만큼이다.</summary>
        public const int MaxSamples = 1200;

        /// <summary>표시 글자를 다시 쓰는 간격(초)이다. 매 프레임 글자를 만들지 않는다.</summary>
        public const float RefreshSeconds = 0.25f;

        /// <summary>이보다 긴 프레임은 "끊김"으로 센다(초, 30FPS 아래).</summary>
        public const float HitchSeconds = 1f / WarnFps;

        /// <summary>로딩 · 창 전환처럼 한 번 튀는 프레임은 기록에서 뺀다(초).</summary>
        public const float IgnoreAboveSeconds = 0.5f;

        public static PerfGrade GetGrade(
            float lowFps)
        {
            if (float.IsNaN(lowFps) ||
                lowFps < BadFps)
            {
                return PerfGrade.Bad;
            }

            return lowFps < WarnFps
                ? PerfGrade.Warn
                : PerfGrade.Good;
        }

        public static float ToFps(
            float seconds)
        {
            return seconds > 0f &&
                   !float.IsNaN(seconds)
                ? 1f / seconds
                : 0f;
        }

        public static string FormatBytes(
            long bytes)
        {
            return (Math.Max(0L, bytes) / (1024d * 1024d)).ToString("0.0", CultureInfo.InvariantCulture) + " MB";
        }

        /// <summary>표시 창 글자다. 줄마다 한 항목이다.</summary>
        public static string Format(
            PerfSnapshot snapshot)
        {
            string line =
                $"FPS {snapshot.Fps:0}  ·  {snapshot.AverageMs:0.0} ms\n" +
                $"1% 느린 {snapshot.LowFps:0} FPS  ·  최장 {snapshot.WorstMs:0} ms\n" +
                $"GC {snapshot.GcCount}회  ·  메모리 {FormatBytes(snapshot.ManagedBytes)}";

            if (snapshot.Npcs >= 0)
            {
                line += $"\nNPC {snapshot.Npcs}  ·  방해자 {snapshot.Disruptors}";
            }

            return line;
        }

        /// <summary>개발 빌드가 한 도전마다 남기는 한 줄 요약이다.</summary>
        public static string FormatRunLine(
            string date,
            string location,
            bool night,
            PerfRunStats stats)
        {
            return string.Join(
                "\t",
                date,
                location + (night ? " ☾" : string.Empty),
                stats.Seconds.ToString("0", CultureInfo.InvariantCulture) + "s",
                "평균 " + stats.AverageFps.ToString("0", CultureInfo.InvariantCulture) + " FPS",
                "최장 " + (stats.WorstSeconds * 1000f).ToString("0", CultureInfo.InvariantCulture) + " ms",
                "끊김 " + stats.Hitches.ToString(CultureInfo.InvariantCulture) + "회",
                "GC " + stats.GcCount.ToString(CultureInfo.InvariantCulture) + "회",
                "등급 " + GetGrade(stats.AverageFps));
        }
    }

    /// <summary>표시 창에 쓸 한 번의 값이다.</summary>
    public struct PerfSnapshot
    {
        public float Fps;
        public float AverageMs;
        public float LowFps;
        public float WorstMs;
        public int GcCount;
        public long ManagedBytes;

        /// <summary>장소 밖이면 -1이라 줄을 숨긴다.</summary>
        public int Npcs;
        public int Disruptors;
    }

    /// <summary>
    /// 최근 몇 초의 프레임 시간을 담는 링 버퍼다 (38일차).
    /// 매 프레임 <see cref="Add"/>만 부르고, 계산은 표시를 갱신할 때만 한다.
    /// </summary>
    public sealed class PerfSampler
    {
        private readonly float[] _samples;
        private readonly float[] _sorted;
        private readonly float _windowSeconds;
        private int _head;
        private int _count;

        public PerfSampler(
            int capacity = PerfLogic.MaxSamples,
            float windowSeconds = PerfLogic.WindowSeconds)
        {
            _samples = new float[Math.Max(1, capacity)];
            _sorted = new float[_samples.Length];
            _windowSeconds = windowSeconds;
        }

        public int Count => _count;

        public void Clear()
        {
            _head = 0;
            _count = 0;
        }

        public void Add(
            float seconds)
        {
            if (seconds <= 0f ||
                float.IsNaN(seconds) ||
                float.IsInfinity(seconds))
            {
                return;
            }

            _samples[_head] = seconds;
            _head = (_head + 1) % _samples.Length;
            _count = Math.Min(_count + 1, _samples.Length);
        }

        /// <summary>최근 창 안(가장 최근부터 합이 창 길이가 될 때까지) 샘플 수다.</summary>
        private int CountInWindow()
        {
            float total = 0f;
            int used = 0;

            while (used < _count)
            {
                int index = (_head - 1 - used + _samples.Length) % _samples.Length;

                total += _samples[index];
                used++;

                if (total >= _windowSeconds)
                {
                    break;
                }
            }

            return used;
        }

        public PerfSnapshot Compute()
        {
            PerfSnapshot snapshot = new PerfSnapshot { Npcs = -1 };
            int used = CountInWindow();

            if (used == 0)
            {
                return snapshot;
            }

            float total = 0f;

            for (int i = 0; i < used; i++)
            {
                float value = _samples[(_head - 1 - i + _samples.Length) % _samples.Length];

                _sorted[i] = value;
                total += value;
            }

            // 느린 프레임이 앞에 오게 정렬한다.
            Array.Sort(_sorted, 0, used);
            Array.Reverse(_sorted, 0, used);

            int slowCount = Math.Max(1, used / 100);
            float slowTotal = 0f;

            for (int i = 0; i < slowCount; i++)
            {
                slowTotal += _sorted[i];
            }

            float average = total / used;

            snapshot.Fps = PerfLogic.ToFps(average);
            snapshot.AverageMs = average * 1000f;
            snapshot.LowFps = PerfLogic.ToFps(slowTotal / slowCount);
            snapshot.WorstMs = _sorted[0] * 1000f;

            return snapshot;
        }
    }

    /// <summary>한 도전 전체의 성능 누적이다 (38일차, 개발 빌드 기록용).</summary>
    public sealed class PerfRunStats
    {
        public float Seconds { get; private set; }
        public int Frames { get; private set; }
        public float WorstSeconds { get; private set; }
        public int Hitches { get; private set; }
        public int GcCount { get; set; }

        public float AverageFps =>
            Frames == 0
                ? 0f
                : PerfLogic.ToFps(Seconds / Frames);

        public void Reset()
        {
            Seconds = 0f;
            Frames = 0;
            WorstSeconds = 0f;
            Hitches = 0;
            GcCount = 0;
        }

        public void Add(
            float seconds)
        {
            if (seconds <= 0f ||
                float.IsNaN(seconds) ||
                seconds > PerfLogic.IgnoreAboveSeconds)
            {
                return;
            }

            Seconds += seconds;
            Frames++;
            WorstSeconds = Math.Max(WorstSeconds, seconds);

            if (seconds > PerfLogic.HitchSeconds)
            {
                Hitches++;
            }
        }
    }
}
