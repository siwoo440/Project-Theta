using System;
using System.Collections.Generic;

namespace ProjectTheta.Balance
{
    /// <summary>디버그 패널 수치 탭의 묶음이다.</summary>
    public enum TuningGroup
    {
        Hypnosis,
        Focus,
        Growth,
        Danger,
        Presentation
    }

    /// <summary>
    /// 디버그 패널에서 슬라이더 하나로 조정하는 수치 한 개다 (20일차).
    ///
    /// 읽기·쓰기 방법을 함수로 들고 있어서, 새 수치를 추가할 때 목록에 한 줄만 넣으면 된다.
    /// </summary>
    public sealed class TuningParameter
    {
        private readonly Func<StageBalanceValues, float> _read;
        private readonly Action<StageBalanceValues, float> _write;

        public TuningParameter(
            string id,
            TuningGroup group,
            string label,
            float minimum,
            float maximum,
            bool isInteger,
            Func<StageBalanceValues, float> read,
            Action<StageBalanceValues, float> write)
        {
            Id = id;
            Group = group;
            Label = label;
            Minimum = Math.Min(minimum, maximum);
            Maximum = Math.Max(minimum, maximum);
            IsInteger = isInteger;
            _read = read;
            _write = write;
        }

        /// <summary>StageBalanceValues의 필드 이름과 같다. 테스트가 필드 누락을 찾을 때 쓴다.</summary>
        public string Id { get; }

        public TuningGroup Group { get; }

        public string Label { get; }

        public float Minimum { get; }

        public float Maximum { get; }

        public bool IsInteger { get; }

        public float Read(
            StageBalanceValues values)
        {
            return values == null
                ? 0f
                : _read(values);
        }

        /// <summary>범위 안으로 자르고, 정수 수치는 반올림해서 쓴다.</summary>
        public void Write(
            StageBalanceValues values,
            float value)
        {
            if (values == null)
            {
                return;
            }

            _write(
                values,
                Sanitize(value));
        }

        public float Sanitize(
            float value)
        {
            if (float.IsNaN(value))
            {
                value = Minimum;
            }

            float clamped =
                Math.Max(
                    Minimum,
                    Math.Min(
                        Maximum,
                        value));

            return IsInteger
                ? (float)Math.Round(
                    clamped,
                    MidpointRounding.AwayFromZero)
                : clamped;
        }

        /// <summary>슬라이더 옆에 쓸 글자다.</summary>
        public string Format(
            float value)
        {
            if (IsInteger)
            {
                return ((int)Math.Round(value)).ToString();
            }

            // 범위가 작은 수치는 소수 둘째 자리까지 보여야 슬라이더 움직임이 읽힌다.
            return Maximum <= 10f
                ? value.ToString("0.00")
                : value.ToString("0.0");
        }
    }

    /// <summary>디버그 패널에서 조정할 수 있는 수치 목록이다.</summary>
    public static class BalanceTuningCatalog
    {
        private static readonly TuningParameter[] Parameters =
        {
            // 최면
            new TuningParameter(
                nameof(StageBalanceValues.PlayerHypnosisSpeedScale),
                TuningGroup.Hypnosis, "일반 최면 배율", 0.5f, 3f, false,
                v => v.PlayerHypnosisSpeedScale,
                (v, x) => v.PlayerHypnosisSpeedScale = x),
            new TuningParameter(
                nameof(StageBalanceValues.FocusHypnosisSpeedBonus),
                TuningGroup.Hypnosis, "집중력 가속", 0f, 2f, false,
                v => v.FocusHypnosisSpeedBonus,
                (v, x) => v.FocusHypnosisSpeedBonus = x),
            new TuningParameter(
                nameof(StageBalanceValues.ChainRadius),
                TuningGroup.Hypnosis, "체인 범위", 0.5f, 6f, false,
                v => v.ChainRadius,
                (v, x) => v.ChainRadius = x),

            // 집중력
            new TuningParameter(
                nameof(StageBalanceValues.FocusHypnosisDrainPerSecond),
                TuningGroup.Focus, "최면 중 초당 소모", 0f, 30f, false,
                v => v.FocusHypnosisDrainPerSecond,
                (v, x) => v.FocusHypnosisDrainPerSecond = x),
            new TuningParameter(
                nameof(StageBalanceValues.FocusRecoveryPerSecond),
                TuningGroup.Focus, "초당 회복", 0f, 40f, false,
                v => v.FocusRecoveryPerSecond,
                (v, x) => v.FocusRecoveryPerSecond = x),
            new TuningParameter(
                nameof(StageBalanceValues.WaveFocusCost),
                TuningGroup.Focus, "파동 비용", 0f, 100f, true,
                v => v.WaveFocusCost,
                (v, x) => v.WaveFocusCost = x),
            new TuningParameter(
                nameof(StageBalanceValues.WaveCooldownSeconds),
                TuningGroup.Focus, "파동 재사용(초)", 0.5f, 15f, false,
                v => v.WaveCooldownSeconds,
                (v, x) => v.WaveCooldownSeconds = x),

            // 성장
            new TuningParameter(
                nameof(StageBalanceValues.LevelBaseXp),
                TuningGroup.Growth, "경험치 기본", 10f, 300f, true,
                v => v.LevelBaseXp,
                (v, x) => v.LevelBaseXp = (int)x),
            new TuningParameter(
                nameof(StageBalanceValues.LevelXpGrowth),
                TuningGroup.Growth, "레벨당 증가", 0f, 150f, true,
                v => v.LevelXpGrowth,
                (v, x) => v.LevelXpGrowth = (int)x),
            new TuningParameter(
                nameof(StageBalanceValues.XpHypnosisSuccess),
                TuningGroup.Growth, "최면 경험치", 0f, 50f, true,
                v => v.XpHypnosisSuccess,
                (v, x) => v.XpHypnosisSuccess = (int)x),
            new TuningParameter(
                nameof(StageBalanceValues.XpFloorFirstVisit),
                TuningGroup.Growth, "새 층 경험치", 0f, 100f, true,
                v => v.XpFloorFirstVisit,
                (v, x) => v.XpFloorFirstVisit = (int)x),

            // 위험
            new TuningParameter(
                nameof(StageBalanceValues.ImpulseBuildScale),
                TuningGroup.Danger, "충동 증가 배율", 0f, 4f, false,
                v => v.ImpulseBuildScale,
                (v, x) => v.ImpulseBuildScale = x),
            new TuningParameter(
                nameof(StageBalanceValues.RecoveryBatchWindowSeconds),
                TuningGroup.Danger, "묶음 회수 대기(초)", 0.2f, 5f, false,
                v => v.RecoveryBatchWindowSeconds,
                (v, x) => v.RecoveryBatchWindowSeconds = x),
            new TuningParameter(
                nameof(StageBalanceValues.ComboTimeoutSeconds),
                TuningGroup.Danger, "콤보 유지(초)", 1f, 15f, false,
                v => v.ComboTimeoutSeconds,
                (v, x) => v.ComboTimeoutSeconds = x),

            // 방해 세력 (22일차)
            new TuningParameter(
                nameof(StageBalanceValues.AlertRisePerSecond),
                TuningGroup.Danger, "경계도 상승(초당)", 0f, 40f, false,
                v => v.AlertRisePerSecond,
                (v, x) => v.AlertRisePerSecond = x),
            new TuningParameter(
                nameof(StageBalanceValues.AlertDecayPerSecond),
                TuningGroup.Danger, "경계도 감소(초당)", 0f, 20f, false,
                v => v.AlertDecayPerSecond,
                (v, x) => v.AlertDecayPerSecond = x),
            new TuningParameter(
                nameof(StageBalanceValues.WatcherSightScale),
                TuningGroup.Danger, "감시 시야 배율", 0.3f, 2f, false,
                v => v.WatcherSightScale,
                (v, x) => v.WatcherSightScale = x),

            // 연출
            new TuningParameter(
                nameof(StageBalanceValues.VfxShakeMedium),
                TuningGroup.Presentation, "흔들림 세기(중)", 0f, 0.5f, false,
                v => v.VfxShakeMedium,
                (v, x) => v.VfxShakeMedium = x),
            new TuningParameter(
                nameof(StageBalanceValues.VfxHitStopSeconds),
                TuningGroup.Presentation, "멈칫 시간(초)", 0f, 0.2f, false,
                v => v.VfxHitStopSeconds,
                (v, x) => v.VfxHitStopSeconds = x)
        };

        public static IReadOnlyList<TuningParameter> All =>
            Parameters;

        public static string GetGroupName(
            TuningGroup group)
        {
            switch (group)
            {
                case TuningGroup.Hypnosis:
                    return "최면";

                case TuningGroup.Focus:
                    return "집중력 · 파동";

                case TuningGroup.Growth:
                    return "성장";

                case TuningGroup.Danger:
                    return "위험 · 회수";

                default:
                    return "연출";
            }
        }
    }

    /// <summary>
    /// 디버그 패널로 수치를 바꾸는 동안의 상태다 (20일차).
    ///
    /// <b>자산 원본을 직접 고치지 않는다.</b>
    /// <see cref="StageBalanceDatabase.Apply"/>는 자산 안의 값 묶음을 그대로 주입하므로,
    /// 그걸 고치면 에디터에서는 자산이 메모리에서 바뀐 채로 남고 나중에 저장까지 될 수 있다.
    /// 그래서 처음 바꾸는 순간 복사본을 만들어 주입을 복사본으로 바꾸고, 원본은 "되돌릴 기준"으로 보관한다.
    /// </summary>
    public static class BalanceTuningSession
    {
        private const float Epsilon = 0.0001f;

        private static StageBalanceValues _baseline;
        private static bool _baselineWasAsset;

        /// <summary>한 번이라도 바꿔서 복사본이 주입되어 있는지다.</summary>
        public static bool IsEditing { get; private set; }

        /// <summary>되돌릴 기준 값이다. 바꾸기 전이면 지금 값과 같다.</summary>
        public static StageBalanceValues Baseline =>
            IsEditing
                ? _baseline
                : BalanceOverrides.StageOrDefault;

        public static float Get(
            TuningParameter parameter)
        {
            return parameter == null
                ? 0f
                : parameter.Read(
                    BalanceOverrides.StageOrDefault);
        }

        public static void Set(
            TuningParameter parameter,
            float value)
        {
            if (parameter == null)
            {
                return;
            }

            if (Math.Abs(
                    parameter.Sanitize(value) -
                    Get(parameter)) <
                Epsilon)
            {
                return;
            }

            BeginEdit();

            parameter.Write(
                BalanceOverrides.Stage,
                value);
        }

        public static bool IsModified(
            TuningParameter parameter)
        {
            return IsEditing &&
                   parameter != null &&
                   Math.Abs(
                       parameter.Read(BalanceOverrides.StageOrDefault) -
                       parameter.Read(_baseline)) >=
                   Epsilon;
        }

        public static int CountModified()
        {
            if (!IsEditing)
            {
                return 0;
            }

            int count = 0;

            IReadOnlyList<TuningParameter> all =
                BalanceTuningCatalog.All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (IsModified(all[i]))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>한 묶음만 기준 값으로 되돌린다.</summary>
        public static void ResetGroup(
            TuningGroup group)
        {
            if (!IsEditing)
            {
                return;
            }

            IReadOnlyList<TuningParameter> all =
                BalanceTuningCatalog.All;

            for (int i = 0;
                 i < all.Count;
                 i++)
            {
                if (all[i].Group != group)
                {
                    continue;
                }

                all[i].Write(
                    BalanceOverrides.Stage,
                    all[i].Read(_baseline));
            }
        }

        /// <summary>전부 되돌리고 원래 주입(자산 또는 코드 기본값)으로 돌아간다.</summary>
        public static void ResetAll()
        {
            if (!IsEditing)
            {
                return;
            }

            BalanceOverrides.Stage =
                _baselineWasAsset
                    ? _baseline
                    : null;

            Clear();
        }

        /// <summary>
        /// 지금 값이 새 기준이 되었다고 알린다. 자산에 저장한 직후 부른다.
        /// <paramref name="saved"/>는 저장된 자산 안의 값 묶음이다.
        /// </summary>
        public static void CommitAsBaseline(
            StageBalanceValues saved)
        {
            if (saved != null)
            {
                BalanceOverrides.Stage =
                    saved;
            }

            Clear();
        }

        /// <summary>편집 상태만 잊는다. 주입은 건드리지 않는다. 테스트 격리와 플레이 진입에 쓴다.</summary>
        public static void Clear()
        {
            IsEditing = false;
            _baseline = null;
            _baselineWasAsset = false;
        }

        private static void BeginEdit()
        {
            if (IsEditing)
            {
                return;
            }

            _baselineWasAsset =
                BalanceOverrides.Stage != null;

            _baseline =
                BalanceOverrides.StageOrDefault;

            BalanceOverrides.Stage =
                _baseline.Clone();

            IsEditing = true;
        }
    }
}
