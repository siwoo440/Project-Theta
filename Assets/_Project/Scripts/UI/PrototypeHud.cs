using UnityEngine;
using ProjectTheta.Capture;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Rival;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private HypnosisCaster _caster;
        private FollowerManager _followers;
        private RampageCoordinator _coordinator;
        private StageSessionController _stage;
        private PlayerHealth _health;
        private PlayerCaptureController _capture;
        private StageTelemetry _telemetry;
        private RivalController _geumtaeyang;
        private PopularGuyController _popularGuy;

        private GUIStyle _centerLabelStyle;
        private GUIStyle _centerTitleStyle;
        private GUIStyle _leftLabelStyle;
        private GUIStyle _resultStyle;

        public void Configure(
            HypnosisCaster caster,
            StageSessionController stage,
            PlayerHealth health,
            PlayerCaptureController capture)
        {
            _caster = caster;
            _stage = stage;
            _health = health;
            _capture = capture;

            _telemetry =
                FindFirstObjectByType<StageTelemetry>();

            _geumtaeyang =
                FindFirstObjectByType<RivalController>();

            _popularGuy =
                FindFirstObjectByType<
                    PopularGuyController>();

            _followers =
                caster == null
                    ? null
                    : caster.FollowerManager;

            _coordinator =
                caster == null
                    ? null
                    : caster.GetComponent<
                        RampageCoordinator>();
        }

        private void OnGUI()
        {
            EnsureStyles();

            DrawHealthHud();
            DrawStageHud();
            DrawDebugHud();
            DrawStageEndOverlay();
        }

        private void DrawHealthHud()
        {
            if (_health == null ||
                _stage == null)
            {
                return;
            }

            const float x = 20f;
            const float y = 18f;
            const float width = 280f;
            const float barHeight = 22f;

            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    24f),
                $"HP  {_health.CurrentHealth} / {_health.MaximumHealth}",
                _leftLabelStyle);

            DrawProgressBar(
                new Rect(
                    x,
                    y + 27f,
                    width,
                    barHeight),
                _health.HealthNormalized,
                new Color(
                    0.88f,
                    0.20f,
                    0.27f,
                    1f),
                new Color(
                    0.14f,
                    0.06f,
                    0.07f,
                    0.92f));

            GUI.Label(
                new Rect(
                    x,
                    y + 54f,
                    width + 80f,
                    20f),
                $"폭주 피격: 정기 +{_stage.RampageCaughtReward} / 포획 중 HP -{_stage.CaptureTickDamage} 누적 {_stage.CaptureMaxDamage}",
                _leftLabelStyle);
        }

        private void DrawStageHud()
        {
            if (_stage == null)
            {
                return;
            }

            float width =
                Mathf.Min(
                    520f,
                    Screen.width * 0.42f);

            float x =
                (Screen.width - width) *
                0.5f;

            float y = 12f;

            GUI.Label(
                new Rect(
                    x,
                    y,
                    width,
                    28f),
                $"남은 시간  {FormatTime(_stage.RemainingTime)}",
                _centerTitleStyle);

            GUI.Label(
                new Rect(
                    x,
                    y + 31f,
                    width,
                    24f),
                $"정기  {_stage.CurrentEssence} / {_stage.TargetEssence}",
                _centerLabelStyle);

            DrawProgressBar(
                new Rect(
                    x,
                    y + 57f,
                    width,
                    24f),
                _stage.EssenceNormalized,
                new Color(
                    0.70f,
                    0.25f,
                    1.00f,
                    1f),
                new Color(
                    0.11f,
                    0.06f,
                    0.16f,
                    0.94f));

            int followerCount =
                _followers == null
                    ? 0
                    : _followers.Count;

            int production =
                StageRules.ComputeProductionPerSecond(
                    followerCount,
                    _stage.PassiveEssencePerFollower);

            GUI.Label(
                new Rect(
                    x,
                    y + 84f,
                    width,
                    22f),
                $"지속 생산 +{production} / sec   |   회수 +{_stage.RecoveryReward}",
                _centerLabelStyle);

            if (!_stage.IsRunning)
            {
                GUI.Label(
                    new Rect(
                        x,
                        y + 112f,
                        width,
                        44f),
                    _stage.GetStateLabel(),
                    _resultStyle);
            }
        }

        private void DrawDebugHud()
        {
            const float width = 360f;
            float x =
                Mathf.Max(
                    0f,
                    Screen.width -
                    width -
                    18f);

            const float y = 18f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    424f),
                "Day 09 Debug");

            if (_followers != null)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 32f,
                        width - 28f,
                        22f),
                    $"동행 인원: {_followers.Count}");

                if (_followers.Count > 0)
                {
                    GUI.Label(
                        new Rect(
                            x + 14f,
                            y + 54f,
                            width - 28f,
                            22f),
                        $"최저 유지도: {_followers.LowestStabilityNormalized * 100f:0}%");
                }
            }

            DrawTargetDebug(
                x,
                y,
                width);

            DrawImpulseDebug(
                x,
                y,
                width);

            DrawCaptureDebug(
                x,
                y,
                width);

            DrawTelemetryDebug(
                x,
                y,
                width);

            if (_stage != null)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 282f,
                        width - 28f,
                        22f),
                    $"상태: {_stage.GetStateLabel()}");

                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 304f,
                        width - 28f,
                        22f),
                    "회수 지점: 복도 오른쪽 보라색 영역");
            }

            DrawOpponentDebug(
                x,
                y,
                width);
        }

        private void DrawOpponentDebug(
            float x,
            float y,
            float width)
        {
            if (_geumtaeyang == null)
            {
                _geumtaeyang =
                    FindFirstObjectByType<
                        RivalController>();
            }

            if (_popularGuy == null)
            {
                _popularGuy =
                    FindFirstObjectByType<
                        PopularGuyController>();
            }

            if (_geumtaeyang == null)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 326f,
                        width - 28f,
                        22f),
                    "금태양: 없음");
            }
            else
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 326f,
                        width - 28f,
                        22f),
                    $"금태양: {_geumtaeyang.State} / 보유 {_geumtaeyang.OwnedFollowerCount}명");

                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 348f,
                        width - 28f,
                        22f),
                    $"금태양 대상: {_geumtaeyang.CurrentTargetName} / 지배 {_geumtaeyang.CurrentTargetControlNormalized * 100f:0}%");
            }

            if (_popularGuy == null)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 370f,
                        width - 28f,
                        22f),
                    "인기남: 없음");
            }
            else
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 370f,
                        width - 28f,
                        22f),
                    $"인기남: {_popularGuy.State} / {_popularGuy.CurrentModeLabel} / 보유 {_popularGuy.OwnedFollowerCount}명");

                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 392f,
                        width - 28f,
                        22f),
                    $"인기남 대상: {_popularGuy.CurrentTargetName} / 지배 {_popularGuy.CurrentTargetControlNormalized * 100f:0}%");
            }
        }

        private void DrawTargetDebug(
            float x,
            float y,
            float width)
        {
            if (_caster == null ||
                _caster.CurrentTarget == null)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 82f,
                        width - 28f,
                        22f),
                    "최면 대상: 없음");

                return;
            }

            HypnosisTarget target =
                _caster.CurrentTarget;

            NpcAgent agent =
                target.GetComponent<NpcAgent>();

            string stateText =
                agent == null
                    ? "-"
                    : agent.State.ToString();

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 82f,
                    width - 28f,
                    22f),
                $"최면 대상: {target.name}");

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 104f,
                    width - 28f,
                    22f),
                $"NPC 상태: {stateText} / 최면 {target.HypnosisNormalized * 100f:0}%");
        }

        private void DrawImpulseDebug(
            float x,
            float y,
            float width)
        {
            ImpulseMeter[] meters =
                FindObjectsByType<ImpulseMeter>(
                    FindObjectsSortMode.None);

            float highest = 0f;
            string highestName = "-";

            for (int i = 0;
                 i < meters.Length;
                 i++)
            {
                ImpulseMeter meter =
                    meters[i];

                if (meter == null ||
                    !meter.IsFollowingActive)
                {
                    continue;
                }

                if (meter.ImpulseNormalized <=
                    highest)
                {
                    continue;
                }

                highest =
                    meter.ImpulseNormalized;

                highestName =
                    meter.name;
            }

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 132f,
                    width - 28f,
                    22f),
                $"최고 충동: {highest * 100f:0}% / {highestName}");

            string activeRampage =
                _coordinator == null
                    ? "-"
                    : _coordinator.ActiveMeterName;

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 154f,
                    width - 28f,
                    22f),
                $"현재 폭주 NPC: {activeRampage}");
        }

        private void DrawCaptureDebug(
            float x,
            float y,
            float width)
        {
            if (_capture == null ||
                !_capture.IsCapturing)
            {
                GUI.Label(
                    new Rect(
                        x + 14f,
                        y + 182f,
                        width - 28f,
                        22f),
                    "포획 상태: 없음");

                return;
            }

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 182f,
                    width - 28f,
                    22f),
                $"포획 상태: 진행 중 / 다음 입력: {_capture.ExpectedInputLabel}");

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 204f,
                    width - 28f,
                    22f),
                $"탈출 { _capture.EscapeNormalized * 100f:0}% / 피해 {_capture.DamageTaken}/{_capture.DamageCap}");
        }

        private void DrawTelemetryDebug(
            float x,
            float y,
            float width)
        {
            if (_telemetry == null)
            {
                _telemetry =
                    FindFirstObjectByType<
                        StageTelemetry>();
            }

            if (_telemetry == null ||
                _stage == null)
            {
                return;
            }

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 228f,
                    width - 28f,
                    22f),
                $"획득 시간 1/3/5명: {_telemetry.FormatTime(_telemetry.FirstFollowerTime)} / {_telemetry.FormatTime(_telemetry.ThreeFollowersTime)} / {_telemetry.FormatTime(_telemetry.FiveFollowersTime)}");

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 250f,
                    width - 28f,
                    22f),
                $"정기 100/200: {_telemetry.FormatTime(_telemetry.Essence100Time)} / {_telemetry.FormatTime(_telemetry.Essence200Time)}");

            GUI.Label(
                new Rect(
                    x + 14f,
                    y + 272f,
                    width - 28f,
                    22f),
                $"폭주 피격: {_stage.RampageCaptureCount} / 회수: {_stage.RecoveredFollowerCount}");
        }

        private void DrawStageEndOverlay()
        {
            if (_stage == null ||
                _stage.IsRunning)
            {
                return;
            }

            float width =
                Mathf.Min(
                    560f,
                    Screen.width *
                    0.64f);

            const float height = 250f;

            float x =
                (Screen.width -
                 width) *
                0.5f;

            float y =
                (Screen.height -
                 height) *
                0.5f;

            GUI.Box(
                new Rect(
                    x,
                    y,
                    width,
                    height),
                string.Empty);

            GUI.Label(
                new Rect(
                    x,
                    y + 18f,
                    width,
                    48f),
                _stage.GetStateLabel(),
                _resultStyle);

            GUI.Label(
                new Rect(
                    x + 24f,
                    y + 78f,
                    width - 48f,
                    28f),
                $"정기 {_stage.CurrentEssence} / {_stage.TargetEssence}",
                _centerTitleStyle);

            GUI.Label(
                new Rect(
                    x + 24f,
                    y + 112f,
                    width - 48f,
                    24f),
                $"플레이 시간 {FormatTime(_stage.ElapsedTime)}",
                _centerLabelStyle);

            if (_health != null)
            {
                GUI.Label(
                    new Rect(
                        x + 24f,
                        y + 140f,
                        width - 48f,
                        24f),
                    $"남은 체력 {_health.CurrentHealth} / {_health.MaximumHealth}",
                    _centerLabelStyle);
            }

            GUI.Label(
                new Rect(
                    x + 24f,
                    y + 168f,
                    width - 48f,
                    24f),
                $"폭주 피격 {_stage.RampageCaptureCount}회   |   회수 {_stage.RecoveredFollowerCount}명",
                _centerLabelStyle);

            GUI.Label(
                new Rect(
                    x + 24f,
                    y + 202f,
                    width - 48f,
                    24f),
                "스테이지 종료 - 입력 및 NPC 진행 정지",
                _centerLabelStyle);
        }

        private void EnsureStyles()
        {
            if (_centerLabelStyle != null)
            {
                return;
            }

            _centerTitleStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 19,
                    fontStyle =
                        FontStyle.Bold
                };

            _centerLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 15,
                    fontStyle =
                        FontStyle.Bold
                };

            _leftLabelStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleLeft,
                    fontSize = 14,
                    fontStyle =
                        FontStyle.Bold
                };

            _resultStyle =
                new GUIStyle(
                    GUI.skin.label)
                {
                    alignment =
                        TextAnchor.MiddleCenter,
                    fontSize = 28,
                    fontStyle =
                        FontStyle.Bold
                };
        }

        private static void DrawProgressBar(
            Rect rect,
            float normalized,
            Color fillColor,
            Color backgroundColor)
        {
            float value =
                Mathf.Clamp01(
                    normalized);

            Color previousColor =
                GUI.color;

            GUI.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.82f);

            GUI.DrawTexture(
                new Rect(
                    rect.x - 2f,
                    rect.y - 2f,
                    rect.width + 4f,
                    rect.height + 4f),
                Texture2D.whiteTexture);

            GUI.color =
                backgroundColor;

            GUI.DrawTexture(
                rect,
                Texture2D.whiteTexture);

            GUI.color =
                fillColor;

            GUI.DrawTexture(
                new Rect(
                    rect.x,
                    rect.y,
                    rect.width *
                    value,
                    rect.height),
                Texture2D.whiteTexture);

            GUI.color =
                previousColor;
        }

        private static string FormatTime(
            float seconds)
        {
            int totalSeconds =
                Mathf.Max(
                    0,
                    Mathf.CeilToInt(
                        seconds));

            int minutes =
                totalSeconds / 60;

            int remainder =
                totalSeconds % 60;

            return $"{minutes:00}:{remainder:00}";
        }
    }
}
