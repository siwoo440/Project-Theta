using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Rival;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>
    /// ① 상태 탭이다. 지금 게임 상태를 묶음별로 보여준다 (20일차).
    /// 예전 F1 창(PrototypeHud)의 내용을 전부 옮기고, 층·레벨·집중력 가속을 더했다.
    /// </summary>
    public sealed class DebugStatusTab : IDebugTab
    {
        private const int MaximumOpponents = 3;

        private readonly DebugPanelContext _context;

        private readonly List<OpponentControllerBase> _opponentBuffer =
            new List<OpponentControllerBase>();

        private Text _floorText;
        private Text _levelText;
        private UiBar _xpBar;
        private Text _xpText;
        private UiBar _essenceBar;
        private Text _essenceText;

        private UiBar _healthBar;
        private Text _healthText;
        private UiBar _focusBar;
        private Text _focusText;
        private Text _followerText;
        private Text _speedText;

        private Text _targetText;
        private Text _targetDetailText;

        private readonly Text[] _opponentTexts =
            new Text[MaximumOpponents];

        private UiBar _impulseBar;
        private Text _impulseText;
        private Text _rampageText;
        private Text _captureText;

        // 22일차: 방해 세력
        private readonly List<DisruptorBase> _disruptorBuffer =
            new List<DisruptorBase>();

        private UiBar _alertBar;
        private Text _alertText;
        private Text _disruptorText;

        public DebugStatusTab(
            DebugPanelContext context)
        {
            _context = context;
        }

        public string Title => "상태";

        public void Build(
            RectTransform root)
        {
            float y = 0f;
            const float labelWidth = 90f;
            const float valueX = 96f;
            const float barWidth = 200f;
            const float afterBarX = valueX + barWidth + 10f;
            float afterBarWidth = DebugUi.ContentWidth - afterBarX;
            float row = DebugUi.RowHeight;

            // 도전 진행
            y = DebugUi.Section(root, "도전 진행", y);

            DebugUi.Label(root, "층", 0f, y, labelWidth, UiTheme.TextMuted);
            _floorText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
            y += row;

            _levelText = DebugUi.Label(root, "Lv 1", 0f, y, labelWidth, UiTheme.Gold, UiTheme.FontSmall, TextAnchor.MiddleLeft, true);
            _xpBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Gold);
            _xpText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "정기", 0f, y, labelWidth, UiTheme.TextMuted);
            _essenceBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Accent);
            _essenceText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row + DebugUi.SectionGap;

            // 플레이어
            y = DebugUi.Section(root, "플레이어", y);

            DebugUi.Label(root, "체력", 0f, y, labelWidth, UiTheme.TextMuted);
            _healthBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Health);
            _healthText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "집중력", 0f, y, labelWidth, UiTheme.TextMuted);
            _focusBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Focus);
            _focusText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "동행", 0f, y, labelWidth, UiTheme.TextMuted);
            _followerText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "최면 배율", 0f, y, labelWidth, UiTheme.TextMuted);
            _speedText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
            y += row + DebugUi.SectionGap;

            // 조준 중인 NPC
            y = DebugUi.Section(root, "조준 중인 NPC", y);

            _targetText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextPrimary);
            y += row;

            _targetDetailText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextMuted);
            y += row + DebugUi.SectionGap;

            // 경쟁자
            y = DebugUi.Section(root, "경쟁자", y);

            for (int i = 0;
                 i < MaximumOpponents;
                 i++)
            {
                _opponentTexts[i] = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextMuted);
                y += row;
            }

            y += DebugUi.SectionGap;

            // 충동 · 폭주
            y = DebugUi.Section(root, "충동 · 폭주", y);

            DebugUi.Label(root, "최고 충동", 0f, y, labelWidth, UiTheme.TextMuted);
            _impulseBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Danger);
            _impulseText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "폭주", 0f, y, labelWidth, UiTheme.TextMuted);
            _rampageText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "포획", 0f, y, labelWidth, UiTheme.TextMuted);
            _captureText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
            y += row + DebugUi.SectionGap;

            // 방해 세력 (22일차)
            y = DebugUi.Section(root, "방해 세력", y);

            DebugUi.Label(root, "경계도", 0f, y, labelWidth, UiTheme.TextMuted);
            _alertBar = DebugUi.Bar(root, valueX, y, barWidth, UiTheme.Gold);
            _alertText = DebugUi.Label(root, string.Empty, afterBarX, y, afterBarWidth, UiTheme.TextPrimary);
            y += row;

            DebugUi.Label(root, "가까운 적", 0f, y, labelWidth, UiTheme.TextMuted);
            _disruptorText = DebugUi.Label(root, string.Empty, valueX, y, DebugUi.ContentWidth - valueX, UiTheme.TextPrimary);
        }

        public void Refresh()
        {
            RefreshRun();
            RefreshPlayer();
            RefreshTarget();
            RefreshOpponents();
            RefreshImpulse();
            RefreshDisruptors();
        }

        private void RefreshDisruptors()
        {
            ZoneAlert alert =
                ZoneAlert.Current;

            if (alert == null)
            {
                _alertBar.SetValue(0f);
                _alertText.text = "없음";
                _disruptorText.text = "이 장소에는 방해 세력이 없습니다";

                return;
            }

            _alertBar.SetValue(alert.Normalized);
            _alertBar.SetColor(StageHudView.GetAlertColor(alert.Level));

            DisruptorSpawner spawner =
                DisruptorSpawner.Current;

            string population =
                spawner == null
                    ? string.Empty
                    : $"  층당 {spawner.AllowedPerFloor}/{PopulationLogic.MaxPerFloor}명 · 대기 {spawner.PendingCount}";

            _alertText.text =
                $"{alert.Value:0}  {ZoneAlertLogic.GetLabel(alert.Level)}  증원 {alert.ReinforcementsUsed}/{ZoneAlertLogic.MaximumReinforcements}{population}";

            DisruptorBase.CopyActive(
                _disruptorBuffer);

            Vector2 player =
                _context.Caster == null
                    ? Vector2.zero
                    : (Vector2)_context.Caster.transform.position;

            DisruptorBase nearest = null;
            float nearestDistance = float.MaxValue;

            for (int i = 0;
                 i < _disruptorBuffer.Count;
                 i++)
            {
                DisruptorBase body = _disruptorBuffer[i];

                if (body == null)
                {
                    continue;
                }

                float distance =
                    ((Vector2)body.transform.position - player).sqrMagnitude;

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = body;
                }
            }

            if (nearest == null ||
                nearest.Profile == null)
            {
                _disruptorText.text = "-";

                return;
            }

            WatcherRole watcher =
                nearest.GetComponent<WatcherRole>();

            SpecialAbility ability =
                nearest.GetComponent<SpecialAbility>();

            string state =
                nearest.IsStunned
                    ? $"멍함 {nearest.StunRemaining:0.0}초"
                    : watcher != null && watcher.IsSpotting
                        ? "발각!"
                        : watcher != null && watcher.SuspicionProgress > 0f
                            ? $"의심 {DebugUi.Percent(watcher.SuspicionProgress)}"
                            : "순찰";

            string abilityState =
                ability == null
                    ? string.Empty
                    : ability.Phase == AbilityPhase.Cooldown
                        ? $"   {ability.DisplayName} 대기 {ability.CooldownRemaining:0}초"
                        : $"   {ability.DisplayName} {ability.Phase}";

            _disruptorText.text =
                $"{nearest.Profile.DisplayName} {nearest.Floor + 1}F   {state}{abilityState}";
        }

        private void RefreshRun()
        {
            FloorTransitionController floors = _context.Floors;
            StageSessionController stage = _context.Stage;

            string floorLabel =
                floors == null
                    ? "-"
                    : $"{Stage.Locations.LocationContext.Current.DisplayName} {floors.CurrentFloor + 1}F/{floors.FloorCount}F";

            string elapsed =
                stage == null
                    ? "-"
                    : RunStats.FormatTime(stage.ElapsedTime);

            string remaining =
                stage == null
                    ? "-"
                    : RunStats.FormatTime(stage.RemainingTime);

            _floorText.text =
                $"{floorLabel}   경과 {elapsed}   남음 {remaining}";

            RunProgression run = _context.Run;

            if (run != null)
            {
                RunLevelState level = run.Level;

                _levelText.text = $"Lv {level.Level}";
                _xpBar.SetValue(level.Progress);

                _xpText.text =
                    level.IsMaxLevel
                        ? "최대"
                        : $"{level.CurrentXp} / {level.RequiredXp}";
            }

            if (stage != null)
            {
                _essenceBar.SetValue(stage.EssenceNormalized);
                _essenceText.text = $"{stage.CurrentEssence} / {stage.TargetEssence}";
            }
        }

        private void RefreshPlayer()
        {
            PlayerHealth health = _context.Health;

            if (health != null)
            {
                _healthBar.SetValue(health.HealthNormalized);

                _healthText.text = $"{health.CurrentHealth} / {health.MaximumHealth}";

                _healthText.color =
                    health.HealthNormalized <= 0.3f
                        ? UiTheme.Danger
                        : UiTheme.TextPrimary;
            }

            PlayerFocus focus = _context.Focus;

            if (focus != null)
            {
                _focusBar.SetValue(focus.FocusNormalized);

                _focusBar.SetColor(
                    focus.IsExhausted
                        ? UiTheme.FocusExhausted
                        : UiTheme.Focus);

                _focusText.text =
                    focus.IsExhausted
                        ? "0   기본 속도"
                        : $"{focus.CurrentFocus:0}   ×{focus.HypnosisSpeedMultiplier:0.00}";

                _focusText.color =
                    focus.IsExhausted
                        ? UiTheme.TextDisabled
                        : UiTheme.Focus;
            }

            FollowerManager followers = _context.Followers;

            _followerText.text =
                followers == null
                    ? "-"
                    : $"{followers.Count}명   최저 유지도 {DebugUi.Percent(followers.LowestStabilityNormalized)}";

            float baseScale =
                Balance.BalanceOverrides.StageOrDefault.PlayerHypnosisSpeedScale;

            _speedText.text =
                $"기본 ×{baseScale:0.00}   난이도·성장·카드 ×{PlayerUpgradeMultipliers.HypnosisSpeed:0.00}";
        }

        private void RefreshTarget()
        {
            HypnosisTarget target =
                _context.Caster == null
                    ? null
                    : _context.Caster.CurrentTarget;

            if (target == null)
            {
                _targetText.text = "없음";
                _targetDetailText.text = string.Empty;

                return;
            }

            NpcProfile profile = target.GetComponent<NpcProfile>();
            NpcAgent agent = target.GetComponent<NpcAgent>();

            string grade =
                profile == null
                    ? "-"
                    : profile.GradeDisplayName;

            float focusMultiplier =
                _context.Focus == null
                    ? 1f
                    : _context.Focus.HypnosisSpeedMultiplier;

            _targetText.text =
                $"{target.name}  [{grade}]   최면 {DebugUi.Percent(target.HypnosisNormalized)}   초당 {target.BuildPerSecond * focusMultiplier:0.0}";

            string state =
                agent == null
                    ? "-"
                    : agent.State.ToString();

            if (agent != null &&
                agent.IsFleeing)
            {
                state += " (도주)";
            }

            if (target.IsGazeBlocked)
            {
                state += " (시선 끊김)";
            }

            string traits =
                profile == null
                    ? "-"
                    : profile.GetTraitSummary();

            _targetDetailText.text =
                $"{state}   소유 {target.Owner}   {traits}";
        }

        private void RefreshOpponents()
        {
            OpponentControllerBase.CopyActive(
                _opponentBuffer);

            int playerFloor =
                _context.Floors == null
                    ? 0
                    : _context.Floors.CurrentFloor;

            for (int i = 0;
                 i < MaximumOpponents;
                 i++)
            {
                Text text = _opponentTexts[i];

                if (i >= _opponentBuffer.Count ||
                    _opponentBuffer[i] == null)
                {
                    text.text = i == 0 ? "없음" : string.Empty;
                    text.color = UiTheme.TextMuted;

                    continue;
                }

                OpponentControllerBase opponent = _opponentBuffer[i];

                int floor =
                    FloorSpace.FloorAt(
                        opponent.transform.position.y);

                bool sameFloor = floor == playerFloor;

                text.text =
                    $"{(sameFloor ? "●" : "○")} {opponent.DisplayName}   {floor + 1}F   {opponent.State}   보유 {opponent.OwnedFollowerCount}명";

                text.color =
                    sameFloor
                        ? DebugUi.SameFloor
                        : UiTheme.TextDisabled;
            }
        }

        private void RefreshImpulse()
        {
            float highest = 0f;
            string highestName = "-";

            FollowerManager followers = _context.Followers;

            if (followers != null)
            {
                IReadOnlyList<FollowerController> list = followers.Followers;

                for (int i = 0;
                     i < list.Count;
                     i++)
                {
                    ImpulseMeter meter =
                        list[i] == null
                            ? null
                            : list[i].GetComponent<ImpulseMeter>();

                    if (meter == null ||
                        !meter.IsFollowingActive ||
                        meter.ImpulseNormalized <= highest)
                    {
                        continue;
                    }

                    highest = meter.ImpulseNormalized;
                    highestName = meter.name;
                }
            }

            _impulseBar.SetValue(highest);

            _impulseText.text =
                highest <= 0f
                    ? "-"
                    : $"{DebugUi.Percent(highest)}  {highestName}";

            // 80%를 넘으면 곧 폭주한다.
            _impulseText.color =
                highest >= 0.8f
                    ? UiTheme.Danger
                    : UiTheme.TextPrimary;

            RampageCoordinator rampage = _context.Rampage;

            _rampageText.text =
                rampage == null ||
                rampage.ActiveMeter == null
                    ? "없음"
                    : $"{rampage.ActiveMeterName}   {rampage.ActiveMeter.State}";

            _rampageText.color =
                rampage != null &&
                rampage.ActiveMeter != null
                    ? UiTheme.Danger
                    : UiTheme.TextPrimary;

            Capture.PlayerCaptureController capture = _context.Capture;

            _captureText.text =
                capture == null ||
                !capture.IsCapturing
                    ? "없음"
                    : $"진행 중   다음 {capture.ExpectedInputLabel}   탈출 {DebugUi.Percent(capture.EscapeNormalized)}   피해 {capture.DamageTaken}/{capture.DamageCap}";
        }
    }
}
