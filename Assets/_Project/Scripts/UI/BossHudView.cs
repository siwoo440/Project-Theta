using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Boss;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 보스전 HUD다 (26일차). 루프탑 클럽에서만 만든다.
    ///
    ///   화면 위 가운데  단계 · 세력 비교 막대 · 보호막 ◆◆◆ · 보호막/지배 진행 막대
    ///   화면 왼쪽 아래  정신력 막대
    ///   화면 오른쪽 위  템포 단계 · 다음 드롭까지
    /// 기존 스테이지 HUD(50)보다 한 칸 위(55)에 그린다.
    /// </summary>
    public sealed class BossHudView : MonoBehaviour
    {
        private static readonly Color PlayerColor = new Color(1.00f, 0.42f, 0.78f);
        private static readonly Color RivalColor = new Color(0.65f, 0.35f, 1.00f);

        private PlayerMind _mind;

        private Text _phaseText;
        private Text _forceText;
        private UiBar _forceBar;
        private Text _shieldText;
        private UiBar _progressBar;
        private UiBar _mindBar;
        private Text _mindText;
        private Text _beatText;
        private Text _skillText;

        private long _phaseKey = UiChangeKey.Unset;
        private long _forceKey = UiChangeKey.Unset;
        private long _beatKey = UiChangeKey.Unset;

        public static BossHudView Create(
            PlayerMind mind)
        {
            GameObject go = new GameObject("BossHud");

            BossHudView view = go.AddComponent<BossHudView>();

            view._mind = mind;
            view.Build();

            return view;
        }

        private void Build()
        {
            Canvas canvas =
                UiFactory.CreateCanvas(
                    "BossHudCanvas",
                    55,
                    transform);

            RectTransform panel =
                UiFactory.CreatePanel(
                    canvas.transform,
                    "BossPanel",
                    new Color(0.08f, 0.04f, 0.12f, 0.85f),
                    RivalColor);

            UiFactory.Place(
                panel,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -278f),
                new Vector2(640f, 112f));

            _phaseText = AddText(panel, "Phase", new Vector2(0f, -8f), UiTheme.FontBody, UiTheme.Gold);

            _forceBar = UiFactory.CreateBar(panel, "Force", PlayerColor, RivalColor);

            UiFactory.Place(
                _forceBar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -40f),
                new Vector2(560f, 12f));

            _forceText = AddText(panel, "ForceText", new Vector2(0f, -52f), UiTheme.FontSmall, UiTheme.TextPrimary);

            _shieldText = AddText(panel, "Shield", new Vector2(-180f, -82f), UiTheme.FontBody, RivalColor);

            _progressBar = UiFactory.CreateBar(panel, "Progress", UiTheme.Gold, UiTheme.TrackFill);

            UiFactory.Place(
                _progressBar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(90f, -94f),
                new Vector2(340f, 10f));

            // 정신력 — 왼쪽 아래
            RectTransform mindGroup = UiFactory.CreateRect(canvas.transform, "Mind");

            UiFactory.Place(
                mindGroup,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                new Vector2(170f, 150f),
                new Vector2(300f, 40f));

            _mindText = AddText(mindGroup, "MindText", Vector2.zero, UiTheme.FontSmall, new Color(0.75f, 0.85f, 1.00f));

            _mindBar = UiFactory.CreateBar(mindGroup, "MindBar", new Color(0.55f, 0.75f, 1.00f), UiTheme.TrackFill);

            UiFactory.Place(
                _mindBar.Root,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                new Vector2(0f, -26f),
                new Vector2(260f, 12f));

            // 박자 — 오른쪽 위
            RectTransform beatGroup = UiFactory.CreateRect(canvas.transform, "Beat");

            UiFactory.Place(
                beatGroup,
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-190f, -160f),
                new Vector2(340f, 30f));

            _beatText = AddText(beatGroup, "BeatText", Vector2.zero, UiTheme.FontBody, UiTheme.TextPrimary);

            // 27일차: 진행 중인 라이벌 기술 — 보스 패널 아래
            _skillText = AddText(panel, "Skills", new Vector2(0f, -122f), UiTheme.FontSmall, new Color(0.85f, 0.60f, 1.00f));
        }

        private static Text AddText(
            RectTransform parent,
            string name,
            Vector2 position,
            int size,
            Color color)
        {
            Text text =
                UiFactory.CreateText(
                    parent,
                    name,
                    string.Empty,
                    size,
                    color,
                    TextAnchor.MiddleCenter,
                    FontStyle.Bold);

            UiFactory.Place(
                text.rectTransform,
                new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f),
                position,
                new Vector2(600f, 24f));

            return text;
        }

        private void Update()
        {
            RefreshBattle();
            RefreshMind();
            RefreshBeat();
            RefreshSkills();
        }

        private void RefreshSkills()
        {
            BossBattle battle = BossBattle.Current;

            if (battle == null ||
                battle.Boss == null)
            {
                return;
            }

            ChampagneTowerAbility tower = battle.Boss.GetComponent<ChampagneTowerAbility>();
            VipZoneAbility zone = VipZoneAbility.ActiveZone;

            string text = string.Empty;

            if (tower != null &&
                tower.IsUp)
            {
                text += "샴페인 타워 · 손님이 바로 모임 ([바텐더]를 최면하면 붕괴)   ";
            }

            if (zone != null)
            {
                text += $"VIP 구역 {Mathf.CeilToInt(zone.Remaining)}초 · 안에서 최면 절반   ";
            }

            if (CloneDancer.ActiveCount > 0)
            {
                text += $"분신 {CloneDancer.ActiveCount}명 · 그림자 있는 쪽이 진짜";
            }

            if (_skillText.text != text)
            {
                _skillText.text = text;
            }
        }

        private void RefreshBattle()
        {
            BossBattle battle = BossBattle.Current;

            if (battle == null)
            {
                return;
            }

            if (UiChangeKey.Changed(
                    ref _phaseKey,
                    UiChangeKey.Of((int)battle.Phase, battle.ShieldsLeft, battle.IsFocusing ? 1 : 0)))
            {
                _phaseText.text =
                    battle.IsFocusing
                        ? $"라이벌 서큐버스 · {BossBattleLogic.GetPhaseLabel(battle.Phase)} · 최면 중"
                        : $"라이벌 서큐버스 · {BossBattleLogic.GetPhaseLabel(battle.Phase)}";

                _shieldText.text =
                    $"보호막 {new string('◆', battle.ShieldsLeft)}{new string('◇', BossBattleLogic.ShieldCount - battle.ShieldsLeft)}";

                _progressBar.SetColor(
                    battle.Phase == BossPhase.Dominate
                        ? PlayerColor
                        : UiTheme.Gold);
            }

            if (UiChangeKey.Changed(
                    ref _forceKey,
                    UiChangeKey.Of(battle.PlayerForce, battle.RivalForce)))
            {
                int total = battle.PlayerForce + battle.RivalForce;

                _forceBar.SetValue(
                    total <= 0
                        ? 0.5f
                        : battle.PlayerForce / (float)total);

                _forceText.text =
                    $"내 세력 {battle.PlayerForce}   ·   라이벌 세력 {battle.RivalForce}  (×{Balance.BalanceOverrides.StageOrDefault.BossAdvantageRatio:0.0} 이상이면 공격 가능)";
            }

            _progressBar.SetValue(
                battle.Phase == BossPhase.Dominate ||
                battle.Phase == BossPhase.Defeated
                    ? battle.DominanceNormalized
                    : battle.ShieldProgressNormalized);
        }

        private void RefreshMind()
        {
            if (_mind == null)
            {
                return;
            }

            _mindBar.SetValue(_mind.Normalized);

            _mindText.text =
                _mind.CollapseCount > 0
                    ? $"정신력 {Mathf.RoundToInt(_mind.Normalized * 100f)}  ·  붕괴 {_mind.CollapseCount}회 (30초 안에 두 번이면 패배)"
                    : $"정신력 {Mathf.RoundToInt(_mind.Normalized * 100f)}";
        }

        private void RefreshBeat()
        {
            ClubBeat beat = ClubBeat.Current;

            if (beat == null)
            {
                return;
            }

            int seconds = Mathf.CeilToInt(beat.SecondsUntilDrop);

            if (!UiChangeKey.Changed(
                    ref _beatKey,
                    UiChangeKey.Of(beat.Tempo, seconds, beat.IsDropWindow ? 1 : 0)))
            {
                return;
            }

            _beatText.text =
                beat.IsDropWindow
                    ? $"템포 {beat.Tempo}  ·  ♪ 드롭! 최면 강화"
                    : $"템포 {beat.Tempo}  ·  드롭까지 {seconds}초";

            Color color = ClubBeat.GetTempoColor(beat.Tempo);
            color.a = 1f;

            _beatText.color = beat.IsDropWindow ? UiTheme.Gold : color;
        }
    }
}
