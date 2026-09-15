using System;
using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Capture;
using ProjectTheta.Duel;
using ProjectTheta.Stage;
using ProjectTheta.UI;

namespace ProjectTheta.Run
{
    /// <summary>
    /// 한 판 안의 성장을 관리한다.
    ///
    /// 흐름
    ///   판 시작 → 카드 1장 선택 (시작 계약)
    ///   행동 → 경험치 → 레벨업 → 카드 1장 선택
    ///
    /// 층을 오르내리는 것만으로는 카드를 주지 않는다.
    /// 새 층에 처음 올라가면 경험치를 줄 뿐이고, 카드는 오직 레벨업으로만 나온다.
    ///
    /// 한 번에 여러 레벨이 오르면 선택을 줄 세워 하나씩 보여준다.
    /// 포획·힘겨루기 중에는 선택 화면을 띄우지 않고 끝날 때까지 기다린다.
    /// </summary>
    public sealed class RunProgression : MonoBehaviour
    {
        /// <summary>경험치를 얻을 때 알린다. HUD의 "+12" 표시가 구독한다.</summary>
        public event Action<int> XpGained;

        public event Action<int> LevelChanged;

        private StageSessionController _stage;
        private StageScoreTracker _tracker;
        private FloorTransitionController _floors;
        private PlayerCaptureController _capture;
        private OpponentDuelController _duel;
        private RunUpgradeChoicePanel _panel;

        private System.Random _random;

        /// <summary>남은 선택 횟수다.</summary>
        private int _pendingChoices;

        /// <summary>첫 선택은 레벨업이 아니라 시작 계약이므로 제목이 다르다.</summary>
        private bool _startChoicePending;

        public RunLevelState Level { get; } =
            new RunLevelState();

        public RunUpgradeState Upgrades { get; } =
            new RunUpgradeState();

        /// <summary>이번 판의 뽑기 시드다. 버그 제보 시 같은 카드 순서를 재현하는 데 쓴다.</summary>
        public int Seed { get; private set; }

        public int PendingChoices =>
            _pendingChoices;

        public void Configure(
            StageSessionController stage,
            StageScoreTracker tracker,
            FloorTransitionController floors,
            RunUpgradeChoicePanel panel)
        {
            _stage = stage;
            _tracker = tracker;
            _floors = floors;
            _panel = panel;

            _capture =
                GetComponent<PlayerCaptureController>();

            _duel =
                GetComponent<OpponentDuelController>();

            Seed =
                Environment.TickCount;

            _random =
                new System.Random(
                    Seed);

            RunUpgradeMultipliers.Active =
                Upgrades;

            Subscribe();

            // 판을 시작하자마자 카드 한 장을 고른다.
            _pendingChoices = 1;
            _startChoicePending = true;
        }

        private void OnDestroy()
        {
            Unsubscribe();

            if (RunUpgradeMultipliers.Active == Upgrades)
            {
                RunUpgradeMultipliers.Clear();
            }
        }

        private void Subscribe()
        {
            if (_tracker != null)
            {
                _tracker.HypnosisSucceeded += HandleHypnosis;
                _tracker.RecoveryConfirmed += HandleRecovery;
                _tracker.DuelWon += HandleDuelWon;
            }

            if (_floors != null)
            {
                _floors.FloorChanged += HandleFloorChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_tracker != null)
            {
                _tracker.HypnosisSucceeded -= HandleHypnosis;
                _tracker.RecoveryConfirmed -= HandleRecovery;
                _tracker.DuelWon -= HandleDuelWon;
            }

            if (_floors != null)
            {
                _floors.FloorChanged -= HandleFloorChanged;
            }
        }

        // 경험치 --------------------------------------------------------

        private void HandleHypnosis(
            bool wasReclaim)
        {
            GrantXp(
                RunExperienceLogic.GetHypnosisPoints(
                    wasReclaim));
        }

        private void HandleRecovery(
            int count,
            int risky,
            int highGrade)
        {
            GrantXp(
                RunExperienceLogic.GetRecoveryPoints(
                    count,
                    risky,
                    highGrade));
        }

        private void HandleDuelWon()
        {
            GrantXp(
                RunExperienceLogic.GetPoints(
                    RunXpSource.DuelWin));
        }

        private void HandleFloorChanged(
            int previous,
            int current,
            bool firstVisit)
        {
            // 오르내리기만 해서는 아무것도 없다. 처음 올라간 층만 경험치를 준다.
            if (!firstVisit)
            {
                return;
            }

            GrantXp(
                RunExperienceLogic.GetPoints(
                    RunXpSource.FloorFirstVisit));
        }

        /// <summary>경험치를 더한다. 강화 "통찰" 배율이 여기서 붙는다.</summary>
        public void GrantXp(
            int basePoints)
        {
            if (basePoints <= 0 ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            int points =
                RunExperienceLogic.ApplyGainMultiplier(
                    basePoints,
                    RunUpgradeMultipliers.XpGain);

            int levelsGained =
                Level.AddXp(
                    points);

            XpGained?.Invoke(
                points);

            if (levelsGained <= 0)
            {
                return;
            }

            _pendingChoices +=
                levelsGained;

            LevelChanged?.Invoke(
                Level.Level);
        }

        // 카드 선택 ------------------------------------------------------

        private void Update()
        {
            if (_panel == null)
            {
                return;
            }

            // 판이 끝났는데 선택 화면이 열려 있으면 적용하지 않고 닫는다.
            if (_stage != null &&
                !_stage.IsRunning)
            {
                _pendingChoices = 0;

                if (_panel.IsOpen)
                {
                    _panel.Hide();
                }

                return;
            }

            if (_panel.IsOpen ||
                _pendingChoices <= 0 ||
                !CanOpenChoice())
            {
                return;
            }

            OpenNextChoice();
        }

        /// <summary>포획·힘겨루기 중에는 선택 화면이 끼어들지 않게 한다.</summary>
        private bool CanOpenChoice()
        {
            if (_capture != null &&
                _capture.IsCapturing)
            {
                return false;
            }

            if (_duel != null &&
                _duel.IsDueling)
            {
                return false;
            }

            return true;
        }

        private void OpenNextChoice()
        {
            List<RunUpgradeCard> cards =
                RunUpgradeDrawLogic.Draw(
                    Upgrades,
                    _random);

            // 모든 카드가 최대 스택이면 고를 게 없으므로 선택을 소진한다.
            if (cards.Count == 0)
            {
                _pendingChoices = 0;

                return;
            }

            string title =
                _startChoicePending
                    ? "시작 계약"
                    : $"레벨 {Level.Level - _pendingChoices + 1} 달성";

            string subtitle =
                _startChoicePending
                    ? "이번 판을 함께할 첫 강화를 고르세요"
                    : _pendingChoices > 1
                        ? $"강화를 고르세요   (남은 선택 {_pendingChoices})"
                        : "강화를 고르세요";

            _panel.Show(
                title,
                subtitle,
                cards,
                Upgrades,
                HandlePicked);
        }

        private void HandlePicked(
            RunUpgradeCard card)
        {
            Upgrades.Apply(card);

            _pendingChoices =
                Math.Max(
                    0,
                    _pendingChoices - 1);

            _startChoicePending = false;
        }
    }
}
