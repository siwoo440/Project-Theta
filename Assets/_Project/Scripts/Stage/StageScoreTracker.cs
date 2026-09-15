using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Impulse;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 한 판 동안의 플레이 성과를 모아 점수와 랭크로 환산한다.
    /// 각 시스템은 성과가 발생한 순간 Report 계열 메서드를 호출하기만 하면 된다.
    /// </summary>
    public sealed class StageScoreTracker : MonoBehaviour
    {
        private StageSessionController _stage;
        private FollowerManager _followers;
        private RampageCoordinator _rampage;

        private int _combo;
        private float _secondsSinceLastCombo;

        public int RecoveredEssence { get; private set; }

        public int MaximumFollowers { get; private set; }

        public int ReclaimCount { get; private set; }

        public int DuelWinCount { get; private set; }

        public int RiskyRecoveryCount { get; private set; }

        public int HighGradeRecoveredCount { get; private set; }

        public int CaptureCount { get; private set; }

        /// <summary>최면 성공 시 알린다. 인자는 재탈환 여부다. 런 경험치가 구독한다.</summary>
        public event System.Action<bool> HypnosisSucceeded;

        /// <summary>회수 묶음 확정 시 알린다. 인자는 (인원, 위험 회수, 고급 등급)이다.</summary>
        public event System.Action<int, int, int> RecoveryConfirmed;

        public event System.Action DuelWon;

        /// <summary>폭주를 붙잡히지 않고 넘긴 횟수다. 튜토리얼 마지막 단계와 경험치에 쓴다.</summary>
        public int RampageSurvivedCount { get; private set; }

        public event System.Action RampageSurvived;

        /// <summary>최면 성공 횟수다. 튜토리얼 진행 판정에 쓴다.</summary>
        public int HypnosisCount { get; private set; }

        /// <summary>회수 묶음이 확정된 횟수다. 튜토리얼 진행 판정에 쓴다.</summary>
        public int RecoveryCount { get; private set; }

        public int CurrentCombo =>
            _combo;

        public float CurrentComboMultiplier =>
            ComboLogic.GetMultiplier(
                _combo);

        public float BestComboMultiplier { get; private set; } =
            ComboLogic.MinimumMultiplier;

        public float ComboRemainingSeconds =>
            ComboLogic.GetRemainingSeconds(
                _combo,
                _secondsSinceLastCombo);

        public void Configure(
            StageSessionController stage,
            FollowerManager followers)
        {
            _stage =
                stage;

            _followers =
                followers;

            if (_rampage != null)
            {
                _rampage.RampageSurvived -= ReportRampageSurvived;
            }

            _rampage =
                followers == null
                    ? null
                    : followers.GetComponent<RampageCoordinator>();

            if (_rampage != null)
            {
                _rampage.RampageSurvived += ReportRampageSurvived;
            }
        }

        private void OnDestroy()
        {
            if (_rampage != null)
            {
                _rampage.RampageSurvived -= ReportRampageSurvived;
            }
        }

        /// <summary>폭주를 피해냈을 때 호출된다. 점수에는 넣지 않고 기록과 알림만 한다.</summary>
        public void ReportRampageSurvived()
        {
            if (_stage != null &&
                !_stage.IsRunning)
            {
                return;
            }

            RampageSurvivedCount++;

            RampageSurvived?.Invoke();
        }

        private void Update()
        {
            if (_stage == null ||
                !_stage.IsRunning)
            {
                return;
            }

            if (_followers != null &&
                _followers.Count >
                MaximumFollowers)
            {
                MaximumFollowers =
                    _followers.Count;
            }

            if (_combo > 0)
            {
                _secondsSinceLastCombo +=
                    Time.deltaTime;

                _combo =
                    ComboLogic.Tick(
                        _combo,
                        _secondsSinceLastCombo);
            }
        }

        /// <summary>최면 성공 시 호출한다. 상대에게서 되찾은 경우 재탈환으로 함께 집계한다.</summary>
        public void ReportHypnosisSuccess(
            bool wasReclaim)
        {
            HypnosisCount++;

            HypnosisSucceeded?.Invoke(
                wasReclaim);

            AddCombo();

            if (wasReclaim)
            {
                ReclaimCount++;

                AddCombo();
            }
        }

        public void ReportDuelWin()
        {
            DuelWinCount++;

            DuelWon?.Invoke();

            AddCombo();
        }

        /// <summary>회수 묶음이 확정될 때 호출한다.</summary>
        public void ReportRecovery(
            int confirmedEssence,
            int recoveredCount,
            int riskyRecoveries,
            int highGradeRecoveries)
        {
            RecoveryCount++;

            RecoveredEssence +=
                Mathf.Max(
                    0,
                    confirmedEssence);

            RiskyRecoveryCount +=
                Mathf.Max(
                    0,
                    riskyRecoveries);

            HighGradeRecoveredCount +=
                Mathf.Max(
                    0,
                    highGradeRecoveries);

            RecoveryConfirmed?.Invoke(
                recoveredCount,
                riskyRecoveries,
                highGradeRecoveries);

            AddCombo();

            if (recoveredCount >= 2)
            {
                AddCombo();
            }
        }

        public void ReportCapture()
        {
            CaptureCount++;

            // 붙잡히면 콤보가 즉시 끊긴다.
            _combo = 0;

            _secondsSinceLastCombo =
                0f;
        }

        public StageScoreBreakdown BuildBreakdown()
        {
            return StageScoreLogic.Compute(
                RecoveredEssence,
                MaximumFollowers,
                BestComboMultiplier,
                ReclaimCount,
                DuelWinCount,
                RiskyRecoveryCount,
                _stage == null
                    ? 0f
                    : _stage.RemainingTime,
                CaptureCount);
        }

        public bool IsSConditionMet =>
            StageRankLogic.IsSConditionMet(
                CaptureCount,
                HighGradeRecoveredCount);

        public StageRank ResolveRank()
        {
            return StageRankLogic.Resolve(
                BuildBreakdown().Total,
                IsSConditionMet);
        }

        private void AddCombo()
        {
            if (_stage != null &&
                !_stage.IsRunning)
            {
                return;
            }

            _combo =
                ComboLogic.AddCombo(
                    _combo);

            _secondsSinceLastCombo =
                0f;

            float multiplier =
                ComboLogic.GetMultiplier(
                    _combo);

            if (multiplier >
                BestComboMultiplier)
            {
                BestComboMultiplier =
                    multiplier;
            }
        }
    }
}
