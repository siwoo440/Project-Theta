using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Run;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.UI.DebugTools
{
    /// <summary>
    /// ③ 치트 탭이다. 특정 상황을 바로 만들어 확인한다 (20일차).
    ///
    /// 치트는 전부 게임의 원래 길을 탄다. 예를 들어 "층 이동"은 계단 이동과 같은 처리(동행 인계·첫 방문 경험치)를,
    /// "최면"은 실제 최면 성공과 같은 처리(점수·연출·동행 합류)를 거친다.
    /// 그래야 치트로 만든 상황에서 본 문제가 실제 플레이에서도 같은 문제다.
    /// </summary>
    public sealed class DebugCheatTab : IDebugTab
    {
        private static readonly float[] Speeds =
        {
            0.25f,
            0.5f,
            1f,
            2f
        };

        private readonly DebugPanelContext _context;

        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private UiButton[] _floorButtons;
        private UiButton _invincibleButton;
        private UiButton _infiniteFocusButton;
        private UiButton _levelUpButton;
        private readonly UiButton[] _speedButtons =
            new UiButton[Speeds.Length];

        private Text _cheatedText;
        private Text _messageText;
        private float _messageRemaining;

        public DebugCheatTab(
            DebugPanelContext context)
        {
            _context = context;
        }

        public string Title => "치트";

        public void Build(
            RectTransform root)
        {
            float y = 0f;
            const float gap = 8f;
            const float height = 36f;
            float half = (DebugUi.ContentWidth - gap) * 0.5f;
            float third = (DebugUi.ContentWidth - gap * 2f) / 3f;

            // 이동
            y = DebugUi.Section(root, "층 이동 (동행도 함께)", y);

            int floorCount =
                _context.Floors == null
                    ? 1
                    : _context.Floors.FloorCount;

            _floorButtons = new UiButton[floorCount];

            float floorWidth =
                (DebugUi.ContentWidth - gap * (floorCount - 1)) / floorCount;

            for (int i = 0;
                 i < floorCount;
                 i++)
            {
                int floor = i;

                _floorButtons[i] =
                    DebugUi.Button(
                        root,
                        $"{i + 1}F",
                        i * (floorWidth + gap),
                        y,
                        floorWidth,
                        height,
                        () => TravelTo(floor));
            }

            y += height + DebugUi.SectionGap + 4f;

            // 플레이어
            y = DebugUi.Section(root, "플레이어", y);

            DebugUi.Button(root, "체력 채우기", 0f, y, half, height, FillHealth);
            DebugUi.Button(root, "집중력 채우기", half + gap, y, half, height, FillFocus);
            y += height + gap;

            _invincibleButton =
                DebugUi.Button(
                    root,
                    string.Empty,
                    0f,
                    y,
                    half,
                    height,
                    () => DebugCheats.SetInvincible(!DebugCheats.Invincible));

            _infiniteFocusButton =
                DebugUi.Button(
                    root,
                    string.Empty,
                    half + gap,
                    y,
                    half,
                    height,
                    () => DebugCheats.SetInfiniteFocus(!DebugCheats.InfiniteFocus));

            y += height + DebugUi.SectionGap + 4f;

            // 진행
            y = DebugUi.Section(root, "진행", y);

            DebugUi.Button(root, "+50 XP", 0f, y, third, height, () => GrantXp(50));
            _levelUpButton = DebugUi.Button(root, "레벨업", third + gap, y, third, height, LevelUp);
            DebugUi.Button(root, "+100 정기", (third + gap) * 2f, y, third, height, AddEssence);
            y += height + gap;

            DebugUi.Button(root, "이 층 NPC 전부 최면", 0f, y, half, height, HypnotizeFloor);
            DebugUi.Button(root, "구역 즉시 클리어", half + gap, y, half, height, ClearZone);
            y += height + DebugUi.SectionGap + 4f;

            // 위험
            y = DebugUi.Section(root, "위험", y);

            DebugUi.Button(root, "가장 가까운 동행 폭주", 0f, y, half, height, TriggerRampage);
            DebugUi.Button(root, "동행 충동 0", half + gap, y, half, height, ClearImpulse);
            y += height + DebugUi.SectionGap + 4f;

            // 시간
            y = DebugUi.Section(root, "게임 속도", y);

            float speedWidth =
                (DebugUi.ContentWidth - gap * (Speeds.Length - 1)) / Speeds.Length;

            for (int i = 0;
                 i < Speeds.Length;
                 i++)
            {
                float speed = Speeds[i];

                _speedButtons[i] =
                    DebugUi.Button(
                        root,
                        $"×{speed:0.##}",
                        i * (speedWidth + gap),
                        y,
                        speedWidth,
                        height,
                        () => GameplayPause.SetDebugSpeed(speed));
            }

            y += height + DebugUi.SectionGap + 8f;

            _cheatedText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.TextMuted);
            y += DebugUi.RowHeight;

            _messageText = DebugUi.Label(root, string.Empty, 0f, y, DebugUi.ContentWidth, UiTheme.Positive);
        }

        public void Refresh()
        {
            int currentFloor =
                _context.Floors == null
                    ? 0
                    : _context.Floors.CurrentFloor;

            for (int i = 0;
                 i < _floorButtons.Length;
                 i++)
            {
                bool here = i == currentFloor;

                DebugUi.SetToggleLook(
                    _floorButtons[i],
                    here,
                    here
                        ? $"{i + 1}F  (현재)"
                        : $"{i + 1}F");
            }

            DebugUi.SetToggleLook(
                _invincibleButton,
                DebugCheats.Invincible,
                DebugCheats.Invincible
                    ? "무적  ON"
                    : "무적  OFF");

            DebugUi.SetToggleLook(
                _infiniteFocusButton,
                DebugCheats.InfiniteFocus,
                DebugCheats.InfiniteFocus
                    ? "집중력 무한  ON"
                    : "집중력 무한  OFF");

            _levelUpButton.SetInteractable(
                _context.Run != null &&
                !_context.Run.Level.IsMaxLevel);

            for (int i = 0;
                 i < Speeds.Length;
                 i++)
            {
                DebugUi.SetToggleLook(
                    _speedButtons[i],
                    Mathf.Approximately(
                        GameplayPause.DebugSpeed,
                        Speeds[i]),
                    $"×{Speeds[i]:0.##}");
            }

            _cheatedText.text =
                DebugCheats.UsedThisRun
                    ? "[주의] 이번 판은 치트를 썼습니다. 판 기록에 \"치트\"로 남습니다"
                    : "치트를 쓰면 판 기록에 \"치트\"로 표시됩니다";

            _cheatedText.color =
                DebugCheats.UsedThisRun
                    ? UiTheme.Gold
                    : UiTheme.TextMuted;

            if (_messageRemaining > 0f)
            {
                _messageRemaining -= 0.1f;

                if (_messageRemaining <= 0f)
                {
                    _messageText.text = string.Empty;
                }
            }
        }

        private void ShowMessage(
            string message,
            bool success = true)
        {
            _messageText.text = message;
            _messageText.color = success ? UiTheme.Positive : UiTheme.Danger;
            _messageRemaining = 3f;
        }

        // 치트 ----------------------------------------------------------

        private void TravelTo(
            int floor)
        {
            if (_context.Floors == null ||
                !_context.Floors.DebugTravelTo(floor))
            {
                return;
            }

            DebugCheats.MarkUsed();

            ShowMessage($"{floor + 1}F로 이동했습니다");
        }

        private void FillHealth()
        {
            if (_context.Health == null)
            {
                return;
            }

            _context.Health.RestoreFull();

            DebugCheats.MarkUsed();
        }

        private void FillFocus()
        {
            if (_context.Focus == null)
            {
                return;
            }

            _context.Focus.Refill(
                _context.Focus.MaximumFocus);

            DebugCheats.MarkUsed();
        }

        private void GrantXp(
            int points)
        {
            if (_context.Run == null)
            {
                return;
            }

            _context.Run.GrantXp(
                points);

            DebugCheats.MarkUsed();
        }

        /// <summary>
        /// 다음 레벨까지 딱 필요한 만큼 준다.
        /// 경험치 배율 카드("통찰")가 곱해지므로 그만큼 나눠서 넣는다. 한 번에 두 레벨이 오르지 않게 하기 위해서다.
        /// </summary>
        private void LevelUp()
        {
            RunProgression run = _context.Run;

            if (run == null ||
                run.Level.IsMaxLevel)
            {
                return;
            }

            int missing =
                run.Level.RequiredXp -
                run.Level.CurrentXp;

            int points =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        missing /
                        Mathf.Max(
                            1f,
                            RunUpgradeMultipliers.XpGain)));

            GrantXp(
                points);
        }

        private void AddEssence()
        {
            if (_context.Stage == null)
            {
                return;
            }

            _context.Stage.AddEssence(
                100);

            DebugCheats.MarkUsed();
        }

        /// <summary>
        /// 목표 정기를 채워 구역을 바로 클리어한다 (21일차).
        /// 지도 → 다음 구역 흐름을 빠르게 확인하는 용도다. 결과 화면 · 판 기록 · 저장은 평소대로 탄다.
        /// </summary>
        private void ClearZone()
        {
            if (_context.Stage == null ||
                !_context.Stage.IsRunning)
            {
                return;
            }

            _context.Stage.AddEssence(
                _context.Stage.TargetEssence);

            DebugCheats.MarkUsed();
        }

        private void HypnotizeFloor()
        {
            if (_context.Caster == null)
            {
                return;
            }

            int floor =
                _context.Floors == null
                    ? 0
                    : _context.Floors.CurrentFloor;

            HypnosisTarget.CopyActive(
                _targetBuffer);

            int claimed = 0;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null)
                {
                    continue;
                }

                NpcAgent agent =
                    target.GetComponent<NpcAgent>();

                if (agent == null ||
                    agent.CurrentFloor != floor)
                {
                    continue;
                }

                if (_context.Caster.DebugClaim(
                        target))
                {
                    claimed++;
                }
            }

            DebugCheats.MarkUsed();

            // 동행 인원에 상한이 있어서, 넘친 NPC는 실제 게임처럼 최면이 풀린다.
            ShowMessage(
                claimed > 0
                    ? $"{claimed}명을 최면했습니다"
                    : "최면할 NPC가 없거나 동행 인원이 가득 찼습니다",
                claimed > 0);
        }

        private void TriggerRampage()
        {
            FollowerManager followers = _context.Followers;

            if (followers == null ||
                followers.Count == 0)
            {
                ShowMessage("동행 NPC가 없습니다", false);

                return;
            }

            Vector2 player =
                _context.Caster == null
                    ? Vector2.zero
                    : (Vector2)_context.Caster.transform.position;

            ImpulseMeter nearest = null;
            float nearestDistance = float.MaxValue;

            IReadOnlyList<FollowerController> list = followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                ImpulseMeter meter =
                    list[i] == null
                        ? null
                        : list[i].GetComponent<ImpulseMeter>();

                if (meter == null)
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        meter.transform.position,
                        player);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = meter;
                }
            }

            if (nearest == null ||
                !nearest.DebugFillImpulse())
            {
                ShowMessage("지금은 폭주시킬 수 없습니다 (이미 폭주 중이거나 회수 중)", false);

                return;
            }

            DebugCheats.MarkUsed();

            ShowMessage($"{nearest.name} 충동을 가득 채웠습니다");
        }

        private void ClearImpulse()
        {
            FollowerManager followers = _context.Followers;

            if (followers == null)
            {
                return;
            }

            IReadOnlyList<FollowerController> list = followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                ImpulseMeter meter =
                    list[i] == null
                        ? null
                        : list[i].GetComponent<ImpulseMeter>();

                if (meter != null)
                {
                    meter.RelieveImpulse(
                        float.MaxValue);
                }
            }

            DebugCheats.MarkUsed();

            ShowMessage("동행 충동을 0으로 만들었습니다");
        }
    }
}
