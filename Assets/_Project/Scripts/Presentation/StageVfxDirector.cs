using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Impulse;
using ProjectTheta.Run;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 스테이지의 "좋은 순간·위험한 순간"에 연출과 효과음을 붙인다 (19일차).
    ///
    /// 게임 규칙 코드는 알림만 보내고 연출을 모른다. 연출은 전부 여기 한 곳에 모인다.
    /// 그래서 연출을 바꾸거나 빼도 게임 규칙은 건드리지 않는다.
    ///
    ///   알림                        연출
    ///   최면 성공                    보라 파문 + 하트
    ///   재탈환 성공                   파문 두 겹 + 금색 파편
    ///   회수 확정                    빛기둥 + 정기 숫자가 HUD로 날아감
    ///   경험치 (20 이상)              작은 "+XP" 글자
    ///   레벨업                      금색 고리 + 멈칫 + 약한 흔들림
    ///   폭주 준비                    붉은 파문 + "!"
    ///   폭주 중                     화면 가장자리 붉은 맥동
    ///   폭주 회피                    "회피!" + 흔들림
    ///   포획당함                     강한 흔들림
    ///   힘겨루기 승리                 파편 + 멈칫 + 흔들림
    ///   층 도착                     짧은 페이드 + 발소리
    /// </summary>
    public sealed class StageVfxDirector : MonoBehaviour
    {
        /// <summary>경험치 글자는 이 값 이상일 때만 띄운다. 최면 한 번(+10)마다 띄우면 화면이 지저분하다.</summary>
        private const int XpTextThreshold = 20;

        private static readonly Color HypnosisColor =
            new Color(0.78f, 0.42f, 1.00f, 0.95f);

        private static readonly Color HeartColor =
            new Color(1.00f, 0.45f, 0.70f, 1.00f);

        private static readonly Color DodgeColor =
            new Color(0.45f, 0.90f, 1.00f, 1.00f);

        private Transform _player;
        private RunProgression _run;
        private FloorTransitionController _floors;
        private RampageCoordinator _rampage;

        private static StageBalanceValues Tuning =>
            BalanceOverrides.StageOrDefault;

        public void Configure(
            Transform player,
            RunProgression run,
            FloorTransitionController floors,
            RampageCoordinator rampage)
        {
            _player = player;
            _run = run;
            _floors = floors;
            _rampage = rampage;

            ApplyShakeSetting();

            Subscribe();
        }

        /// <summary>허브 설정의 화면 흔들림 끄기를 반영한다.</summary>
        private static void ApplyShakeSetting()
        {
            GameSession session =
                GameSession.Instance;

            CameraShake.Enabled =
                session == null ||
                session.Save == null ||
                !session.Save.ScreenShakeDisabled;
        }

        private void OnDestroy()
        {
            Unsubscribe();

            GameVfx.SetDanger(0f);
        }

        private void Subscribe()
        {
            StageMoments.HypnosisSucceeded += HandleHypnosis;
            StageMoments.RecoveryConfirmed += HandleRecovery;
            StageMoments.RampageWindup += HandleRampageWindup;
            StageMoments.RampageSurvived += HandleRampageSurvived;
            StageMoments.CaptureStarted += HandleCapture;
            StageMoments.DuelWon += HandleDuelWon;
            StageMoments.AlertLevelChanged += HandleAlertLevelChanged;
            StageMoments.DisruptorSpotted += HandleSpotted;
            StageMoments.AbilityTelegraphed += HandleAbilityTelegraphed;
            StageMoments.AbilityFired += HandleAbilityFired;
            StageMoments.BreakTimeStarted += HandleBreakTime;
            StageMoments.TideWarning += HandleTideWarning;
            StageMoments.PickpocketStole += HandlePickpocketStole;
            StageMoments.PickpocketResolved += HandlePickpocketResolved;
            StageMoments.TrainArrived += HandleTrainArrived;
            StageMoments.SpecialTargetRecovered += HandleSpecialTargetRecovered;
            StageMoments.BlackoutStarted += HandleBlackoutStarted;
            StageMoments.MallClosing += HandleMallClosing;

            if (_run != null)
            {
                _run.XpGained += HandleXpGained;
                _run.LevelChanged += HandleLevelChanged;
            }

            if (_floors != null)
            {
                _floors.FloorChanged += HandleFloorChanged;
            }
        }

        private void Unsubscribe()
        {
            StageMoments.HypnosisSucceeded -= HandleHypnosis;
            StageMoments.RecoveryConfirmed -= HandleRecovery;
            StageMoments.RampageWindup -= HandleRampageWindup;
            StageMoments.RampageSurvived -= HandleRampageSurvived;
            StageMoments.CaptureStarted -= HandleCapture;
            StageMoments.DuelWon -= HandleDuelWon;
            StageMoments.AlertLevelChanged -= HandleAlertLevelChanged;
            StageMoments.DisruptorSpotted -= HandleSpotted;
            StageMoments.AbilityTelegraphed -= HandleAbilityTelegraphed;
            StageMoments.AbilityFired -= HandleAbilityFired;
            StageMoments.BreakTimeStarted -= HandleBreakTime;
            StageMoments.TideWarning -= HandleTideWarning;
            StageMoments.PickpocketStole -= HandlePickpocketStole;
            StageMoments.PickpocketResolved -= HandlePickpocketResolved;
            StageMoments.TrainArrived -= HandleTrainArrived;
            StageMoments.SpecialTargetRecovered -= HandleSpecialTargetRecovered;
            StageMoments.BlackoutStarted -= HandleBlackoutStarted;
            StageMoments.MallClosing -= HandleMallClosing;

            if (_run != null)
            {
                _run.XpGained -= HandleXpGained;
                _run.LevelChanged -= HandleLevelChanged;
            }

            if (_floors != null)
            {
                _floors.FloorChanged -= HandleFloorChanged;
            }
        }

        /// <summary>폭주가 진행 중인 동안 화면 가장자리를 붉게 맥동시킨다.</summary>
        private void Update()
        {
            GameVfx.SetDanger(
                GetDangerIntensity());
        }

        private float GetDangerIntensity()
        {
            if (_rampage == null ||
                _rampage.ActiveMeter == null)
            {
                return 0f;
            }

            switch (_rampage.ActiveMeter.State)
            {
                case ImpulseState.Preparing:
                    return 0.6f;

                case ImpulseState.Rampaging:
                    return 1f;

                case ImpulseState.Capturing:
                    return 0.8f;

                default:
                    return 0f;
            }
        }

        private Vector2 PlayerPosition =>
            _player == null
                ? Vector2.zero
                : (Vector2)_player.position;

        // 알림 처리 -----------------------------------------------------

        private void HandleHypnosis(
            Vector2 position,
            bool wasReclaim)
        {
            float radius =
                Tuning.VfxRippleRadius;

            GameVfx.Ripple(
                position,
                HypnosisColor,
                radius);

            GameVfx.Pop(
                VfxSprite.Heart,
                position + new Vector2(0f, 1.1f),
                HeartColor,
                0.45f);

            if (wasReclaim)
            {
                // 되찾은 순간은 한 겹 더 크게 퍼지고 금색 파편이 튄다.
                GameVfx.Ripple(
                    position,
                    UiTheme.Gold,
                    radius * 1.6f,
                    0.6f);

                GameVfx.Shards(
                    position + new Vector2(0f, 0.6f),
                    UiTheme.Gold,
                    8);
            }

            GameAudio.Play(
                GameSfx.HypnosisSuccess,
                0.8f);
        }

        private void HandleRecovery(
            Vector2 position,
            int count,
            int essence)
        {
            int safeCount =
                Mathf.Max(
                    1,
                    count);

            // 한꺼번에 많이 회수할수록 빛기둥이 굵어진다. 모아서 회수하는 보상을 눈으로 보여준다.
            float width =
                0.8f +
                0.3f *
                (safeCount - 1);

            GameVfx.Pillar(
                position,
                UiTheme.Gold,
                width,
                4.2f);

            GameVfx.Glow(
                position + new Vector2(0f, 0.5f),
                UiTheme.Gold,
                0.9f + 0.2f * safeCount);

            GameVfx.Shards(
                position + new Vector2(0f, 0.5f),
                UiTheme.Gold,
                4 + safeCount * 2);

            if (essence > 0)
            {
                GameVfx.FlyText(
                    $"+{essence}",
                    position + new Vector2(0f, 1.2f),
                    UiTheme.Gold);
            }

            if (safeCount >= 3)
            {
                GameVfx.Shake(
                    Tuning.VfxShakeSmall,
                    Tuning.VfxShakeSeconds);
            }

            GameAudio.Play(
                GameSfx.Recovery);
        }

        private void HandleXpGained(
            int points)
        {
            if (points < XpTextThreshold)
            {
                return;
            }

            GameVfx.FloatText(
                $"+{points} XP",
                PlayerPosition + new Vector2(0.6f, 0.4f),
                UiTheme.Gold,
                UiTheme.FontBody);
        }

        private void HandleLevelChanged(
            int level)
        {
            Vector2 position =
                PlayerPosition;

            GameVfx.Ripple(
                position,
                UiTheme.Gold,
                Tuning.VfxRippleRadius * 1.8f,
                0.7f);

            GameVfx.Glow(
                position + new Vector2(0f, 0.6f),
                UiTheme.Gold,
                1.4f,
                0.5f);

            GameVfx.Shards(
                position + new Vector2(0f, 0.8f),
                UiTheme.Gold,
                12,
                4f);

            GameVfx.FloatText(
                "LEVEL UP",
                position + new Vector2(0f, 0.6f),
                UiTheme.Gold,
                UiTheme.FontHeading,
                1.1f);

            // 곧바로 카드 화면이 떠서 완전 정지가 걸린다. 그쪽이 이기므로 멈칫이 카드 화면을 풀지 않는다.
            GameVfx.HitStop(
                Tuning.VfxHitStopSeconds,
                Tuning.VfxHitStopScale);

            GameVfx.Shake(
                Tuning.VfxShakeSmall,
                Tuning.VfxShakeSeconds);

            GameAudio.Play(
                GameSfx.LevelUp);
        }

        private void HandleRampageWindup(
            Vector2 position)
        {
            GameVfx.Ripple(
                position,
                UiTheme.Danger,
                Tuning.VfxRippleRadius,
                0.5f);

            GameVfx.FloatText(
                "!",
                position + new Vector2(0f, 0.5f),
                UiTheme.Danger,
                UiTheme.FontTitle,
                0.7f);
        }

        private void HandleRampageSurvived(
            Vector2 position)
        {
            Vector2 player =
                PlayerPosition;

            GameVfx.FloatText(
                "회피!",
                player + new Vector2(0f, 0.5f),
                DodgeColor,
                UiTheme.FontHeading);

            GameVfx.Ripple(
                player,
                DodgeColor,
                Tuning.VfxRippleRadius * 1.2f);

            GameVfx.Shake(
                Tuning.VfxShakeSmall,
                Tuning.VfxShakeSeconds);

            GameAudio.Play(
                GameSfx.Dodge);
        }

        private void HandleCapture(
            Vector2 position)
        {
            GameVfx.Glow(
                position + new Vector2(0f, 0.5f),
                UiTheme.Danger,
                1.2f);

            GameVfx.Shake(
                Tuning.VfxShakeLarge,
                Tuning.VfxShakeSeconds * 1.4f);
        }

        private void HandleDuelWon(
            Vector2 position)
        {
            GameVfx.Shards(
                position + new Vector2(0f, 0.6f),
                Color.white,
                14,
                4.5f);

            GameVfx.Ripple(
                position,
                UiTheme.Gold,
                Tuning.VfxRippleRadius * 1.4f);

            GameVfx.HitStop(
                Tuning.VfxHitStopSeconds * 1.3f,
                Tuning.VfxHitStopScale);

            GameVfx.Shake(
                Tuning.VfxShakeMedium,
                Tuning.VfxShakeSeconds);

            GameAudio.Play(
                GameSfx.DuelWin);
        }

        // 22일차: 방해 세력 ----------------------------------------------

        private void HandleSpotted(
            Vector2 position)
        {
            GameVfx.Ripple(
                position + new Vector2(0f, 1.2f),
                UiTheme.Danger,
                0.7f,
                0.35f);

            GameAudio.Play(
                GameSfx.ClaimTick,
                0.8f);
        }

        /// <summary>단계가 오를 때만 알린다. 내려갈 때는 조용히 둔다.</summary>
        private void HandleAlertLevelChanged(
            AlertLevel previous,
            AlertLevel current)
        {
            if (current <= previous)
            {
                return;
            }

            Color color =
                UI.StageHudView.GetAlertColor(
                    current);

            bool emergency =
                current == AlertLevel.Emergency;

            GameVfx.FloatText(
                emergency
                    ? "비상! 증원 · 회수 지점 잠김"
                    : $"경계도 {ZoneAlertLogic.GetLabel(current)}",
                PlayerPosition + new Vector2(0f, 2.2f),
                color,
                emergency
                    ? UiTheme.FontHeading
                    : UiTheme.FontBody,
                1.2f);

            if (emergency)
            {
                GameVfx.Shake(
                    Tuning.VfxShakeMedium,
                    Tuning.VfxShakeSeconds);
            }

            GameAudio.Play(
                emergency
                    ? GameSfx.UiStamp
                    : GameSfx.UiTick);
        }

        private void HandleAbilityTelegraphed(
            Vector2 position,
            string abilityName)
        {
            GameVfx.Ripple(
                position + new Vector2(0f, 1f),
                UiTheme.Gold,
                1.1f,
                0.6f);

            GameAudio.Play(
                GameSfx.ClaimTick);
        }

        private void HandleAbilityFired(
            Vector2 position,
            string abilityName)
        {
            GameVfx.Ripple(
                position + new Vector2(0f, 1f),
                UiTheme.Danger,
                0.9f,
                0.4f);

            GameVfx.FloatText(
                $"{abilityName}!",
                position + new Vector2(0f, 1.8f),
                UiTheme.Danger,
                UiTheme.FontBody);
        }

        private void HandleBreakTime()
        {
            GameVfx.FloatText(
                "쉬는 시간!",
                PlayerPosition + new Vector2(0f, 2.4f),
                new Color(0.65f, 0.85f, 1.00f),
                UiTheme.FontHeading,
                1.2f);

            GameAudio.Play(
                GameSfx.UiStamp,
                0.6f);
        }

        // 23일차: 해변가 · 야시장 ---------------------------------------

        private void HandleTideWarning()
        {
            GameVfx.FloatText(
                "파도가 온다! 물가에서 떨어지세요",
                PlayerPosition + new Vector2(0f, 2.4f),
                new Color(0.45f, 0.75f, 1.00f),
                UiTheme.FontBody,
                1.4f);

            GameAudio.Play(
                GameSfx.UiTick);
        }

        private void HandlePickpocketStole(
            Vector2 position,
            int amount)
        {
            GameVfx.FloatText(
                $"소매치기! 정기 -{amount}  잡아라!",
                position + new Vector2(0f, 2.2f),
                UiTheme.Danger,
                UiTheme.FontHeading,
                1.4f);

            GameVfx.Shake(
                Tuning.VfxShakeMedium,
                Tuning.VfxShakeSeconds);

            GameAudio.Play(
                GameSfx.UiStamp);
        }

        private void HandlePickpocketResolved(
            Vector2 position,
            bool caught)
        {
            if (caught)
            {
                GameVfx.Pop(
                    VfxSprite.Spark,
                    position + new Vector2(0f, 1f),
                    UiTheme.Gold,
                    1.2f);

                GameVfx.FloatText(
                    $"되찾았다! +{PickpocketLogic.CatchBonus}",
                    position + new Vector2(0f, 2.2f),
                    UiTheme.Gold,
                    UiTheme.FontHeading,
                    1.2f);

                GameAudio.Play(
                    GameSfx.Recovery);

                return;
            }

            GameVfx.FloatText(
                "소매치기가 인파 속으로 사라졌다",
                PlayerPosition + new Vector2(0f, 2.4f),
                UiTheme.TextMuted,
                UiTheme.FontBody,
                1.4f);
        }

        // 24일차: 지하철 · 헬스장 -----------------------------------------

        private void HandleTrainArrived(
            int arrived)
        {
            int required =
                StageSessionController.SurvivalTrainsRequired;

            GameVfx.FloatText(
                arrived >= required
                    ? $"열차 {arrived}대 통과! 버텼다"
                    : $"열차 도착 ({arrived}/{required}) · 인파 주의",
                PlayerPosition + new Vector2(0f, 2.4f),
                new Color(0.60f, 0.85f, 0.60f),
                UiTheme.FontHeading,
                1.2f);

            GameVfx.Shake(
                Tuning.VfxShakeMedium * 0.5f,
                Tuning.VfxShakeSeconds);

            GameAudio.Play(
                GameSfx.FloorArrive);
        }

        private void HandleSpecialTargetRecovered(
            Vector2 position,
            int bonus)
        {
            GameVfx.Pop(
                VfxSprite.Spark,
                position + new Vector2(0f, 1f),
                UiTheme.Gold,
                1.4f);

            GameVfx.FlyText(
                $"★ 특수 대상 함락 +{bonus}",
                position + new Vector2(0f, 2f),
                UiTheme.Gold);

            GameAudio.Play(
                GameSfx.LevelUp);
        }

        // 25일차: 쇼핑몰 · 오피스 --------------------------------------

        private void HandleBlackoutStarted()
        {
            GameVfx.FloatText(
                "정전! 경비원 손전등만 보인다",
                PlayerPosition + new Vector2(0f, 2.4f),
                new Color(0.85f, 0.85f, 0.60f),
                UiTheme.FontHeading,
                1.4f);

            GameAudio.Play(
                GameSfx.UiStamp,
                0.7f);
        }

        private void HandleMallClosing()
        {
            GameVfx.FloatText(
                "폐점 안내 방송 · 시간이 빨리 흐릅니다",
                PlayerPosition + new Vector2(0f, 2.4f),
                new Color(1.00f, 0.75f, 0.40f),
                UiTheme.FontHeading,
                1.6f);

            GameVfx.Shake(
                Tuning.VfxShakeMedium * 0.5f,
                Tuning.VfxShakeSeconds);
        }

        private void HandleFloorChanged(
            int previous,
            int current,
            bool firstVisit)
        {
            // 순간이동이 뚝 끊겨 보이지 않게 잠깐 어둡게 했다가 밝힌다.
            GameVfx.Fade(
                Tuning.VfxFadeSeconds);

            GameAudio.Play(
                GameSfx.FloorArrive);
        }
    }
}
