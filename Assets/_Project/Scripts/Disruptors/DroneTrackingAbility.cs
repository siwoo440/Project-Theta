using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 드론 촬영자의 <b>추적 촬영</b>이다 (부록 C.3 [1]).
    ///
    /// 촬영자는 한자리에 서 있고, 드론이 넓은 원을 그리며 날면서 바닥에 원형 시야(반경 2.5m)를 비춘다.
    /// 원형 시야는 파라솔 그늘을 무시한다. 3초 이상 잡히면 1.5초 예고(불빛 빨강) 뒤
    /// 6초 동안 플레이어를 따라다니며 촬영하고, 원 안에 있는 동안 경계도가 초당 +6 오른다.
    /// 대처: 원 밖으로 피하기, 추적 중 파라솔 아래로 들어가 추적 끊기, 촬영자를 파동으로 멍하게 해 드론 착륙(이 구역 동안 영구).
    /// </summary>
    public sealed class DroneTrackingAbility : SpecialAbility
    {
        private const float DroneHeight = 2.6f;
        private const float ReturnSpeed = 3f;

        private static readonly Color IdleSpot = new Color(1.00f, 1.00f, 1.00f, 0.14f);
        private static readonly Color SeenSpot = new Color(1.00f, 0.85f, 0.35f, 0.24f);
        private static readonly Color TrackSpot = new Color(1.00f, 0.30f, 0.30f, 0.30f);

        private Transform _player;
        private StageSessionController _stage;

        private GameObject _drone;
        private SpriteRenderer _droneBody;
        private SpriteRenderer _spot;

        private Vector2 _center;
        private Vector2 _ground;
        private float _angle;
        private float _seen;
        private float _trackRemaining;
        private bool _landed;

        public override string DisplayName =>
            "추적 촬영";

        public bool IsTracking =>
            _trackRemaining > 0f;

        public float TrackRemaining =>
            Mathf.Max(0f, _trackRemaining);

        public bool IsLanded =>
            _landed;

        public void Configure(
            Transform player,
            StageSessionController stage)
        {
            _player = player;
            _stage = stage;

            _center = transform.position;
            _ground = _center;

            _drone = new GameObject("Drone");
            _drone.transform.SetParent(transform, false);

            _spot =
                LocationProps.Blob(
                    _drone.transform,
                    "DroneSpot",
                    _ground,
                    new Vector2(
                        DroneLogic.SpotRadius * 2f,
                        DroneLogic.SpotRadius * 2f * LanternLogic.VerticalRatio),
                    IdleSpot,
                    -52);

            _droneBody =
                LocationProps.Box(
                    _drone.transform,
                    "DroneBody",
                    _ground + new Vector2(0f, DroneHeight),
                    new Vector2(0.7f, 0.18f),
                    new Color(0.20f, 0.22f, 0.28f),
                    15500);
        }

        private bool PlayerInSpot =>
            _player != null &&
            FloorSpace.FloorAt(_player.position.y) == Body.Floor &&
            AreaLogic.IsInsideEllipse(
                _player.position.x - _ground.x,
                _player.position.y - _ground.y,
                DroneLogic.SpotRadius,
                DroneLogic.SpotRadius * LanternLogic.VerticalRatio);

        protected override bool WantsToStart()
        {
            return !_landed &&
                   !IsTracking &&
                   DroneLogic.IsSpotted(_seen);
        }

        protected override void Fire()
        {
            if (_landed)
            {
                return;
            }

            _seen = 0f;
            _trackRemaining = DroneLogic.TrackSeconds;

            StageMoments.RaiseAbilityFired(
                _ground,
                DisplayName);
        }

        private void LateUpdate()
        {
            if (_drone == null ||
                Body == null)
            {
                return;
            }

            if (!_landed &&
                Body.IsStunned)
            {
                Land();
            }

            bool running =
                !GameplayPause.IsPaused &&
                (_stage == null ||
                 _stage.IsRunning);

            if (running &&
                !_landed)
            {
                Fly(Time.deltaTime);
            }

            ApplyVisual();
        }

        private void Fly(
            float deltaTime)
        {
            if (IsTracking)
            {
                _trackRemaining -= deltaTime;

                _ground =
                    Vector2.MoveTowards(
                        _ground,
                        _player.position,
                        DroneLogic.TrackSpeed * deltaTime);

                bool inSpot = PlayerInSpot;

                if (inSpot &&
                    ZoneAlert.Current != null)
                {
                    ZoneAlert.Current.Add(
                        DroneLogic.TrackAlertPerSecond * deltaTime,
                        _player.position);
                }

                // 파라솔 아래로 들어가면 놓친다.
                if (ParasolShade.ActiveCount > 0 &&
                    ParasolShade.IsShaded(_player.position))
                {
                    _trackRemaining = 0f;

                    GameVfx.FloatText(
                        "드론이 놓쳤다",
                        _ground + new Vector2(0f, DroneHeight + 0.5f),
                        UI.Framework.UiTheme.TextMuted,
                        UI.Framework.UiTheme.FontSmall);
                }

                return;
            }

            _angle += DroneLogic.OrbitRadiansPerSecond * deltaTime;

            DroneLogic.GetOrbitOffset(
                _angle,
                out float x,
                out float y);

            _ground =
                Vector2.MoveTowards(
                    _ground,
                    _center + new Vector2(x, y),
                    ReturnSpeed * deltaTime);

            // 예고가 시작되면 더 쌓지 않는다.
            if (Phase == AbilityPhase.Ready)
            {
                _seen =
                    DroneLogic.AdvanceSeen(
                        _seen,
                        PlayerInSpot,
                        deltaTime);
            }
        }

        private void Land()
        {
            _landed = true;
            _trackRemaining = 0f;
            _seen = 0f;
            _ground = (Vector2)transform.position + new Vector2(0.6f, 0f);

            GameVfx.FloatText(
                "드론 착륙!",
                _ground + new Vector2(0f, 1.2f),
                UI.Framework.UiTheme.Gold,
                UI.Framework.UiTheme.FontBody);
        }

        private void ApplyVisual()
        {
            _spot.enabled = !_landed;

            _spot.transform.position =
                new Vector3(_ground.x, _ground.y, 0f);

            _droneBody.transform.position =
                _landed
                    ? new Vector3(_ground.x, _ground.y + 0.1f, 0f)
                    : new Vector3(
                        _ground.x,
                        _ground.y + DroneHeight + Mathf.Sin(Time.time * 6f) * 0.05f,
                        0f);

            Color spot =
                IsTracking || Phase == AbilityPhase.Telegraph
                    ? TrackSpot
                    : _seen > 0.01f
                        ? Color.Lerp(IdleSpot, SeenSpot, _seen / DroneLogic.SpotSeconds)
                        : IdleSpot;

            // 예고 중에는 불빛이 깜빡인다.
            if (Phase == AbilityPhase.Telegraph)
            {
                spot.a *= 0.5f + 0.5f * Mathf.PingPong(Time.time * 6f, 1f);
            }

            if (_spot.color != spot)
            {
                _spot.color = spot;
            }
        }
    }
}
