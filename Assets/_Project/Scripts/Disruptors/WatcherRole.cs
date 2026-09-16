using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Player;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 감시 역할이다 (22일차, 부록 C.2.3).
    ///
    /// 시야 부채꼴 안에서 플레이어가 <b>수상한 행동</b>(최면 유지 · 대시)을 하면 발각 진행도가 오른다.
    ///   진행 중  머리 위 `?`
    ///   발각     머리 위 `!` → 최면을 유지하는 동안 구역 경계도 상승, 대시마다 추가 상승
    /// 걷기만 하는 플레이어는 문제 삼지 않는다. "보이면 안 된다"가 아니라 "보는 앞에서 하면 안 된다"다.
    ///
    /// 발각하면 잠깐 쫓아오고, 구역이 경계 단계면 마지막 발견 지점을 살피러 간다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class WatcherRole : MonoBehaviour
    {
        /// <summary>`!`가 뜬 뒤 수상한 행동이 멈춰도 이만큼은 발각 상태로 남는다.</summary>
        private const float SpottedHoldSeconds = 1.5f;

        /// <summary>경계 단계에서 마지막 발견 지점을 살피러 가는 간격이다.</summary>
        private const float InvestigateIntervalSeconds = 6f;

        private DisruptorBase _body;
        private Transform _player;
        private PlayerSideViewController _movement;
        private HypnosisCaster _caster;
        private FollowerManager _followers;

        private float _progress;
        private float _spottedRemaining;
        private float _investigateCooldown;
        private bool _wasDashing;

        /// <summary>발각 진행도(0~1)다. 머리 위 `?` 채움에 쓴다.</summary>
        public float SuspicionProgress =>
            _progress;

        /// <summary>`!` 상태인지다.</summary>
        public bool IsSpotting =>
            _spottedRemaining > 0f;

        /// <summary>`!`로 바뀌는 순간 알린다 (25일차, 쇼핑몰 무전). (플레이어 위치)</summary>
        public event System.Action<Vector2> Spotted;

        /// <summary>이번 프레임에 플레이어가 시야 안에 있는지다. 바닥 시야 표시를 진하게 한다.</summary>
        public bool PlayerInSight { get; private set; }

        public float SightRange =>
            _body == null ||
            _body.Profile == null
                ? 0f
                : _body.Profile.SightRange *
                  Mathf.Max(
                      0f,
                      BalanceOverrides.StageOrDefault.WatcherSightScale);

        public float SightHalfAngle =>
            _body == null ||
            _body.Profile == null
                ? 0f
                : _body.Profile.SightHalfAngle;

        public void Configure(
            Transform player)
        {
            _body = GetComponent<DisruptorBase>();
            _player = player;

            if (player != null)
            {
                _movement = player.GetComponent<PlayerSideViewController>();
                _caster = player.GetComponent<HypnosisCaster>();
                _followers = player.GetComponent<FollowerManager>();
            }
        }

        /// <summary>파동으로 멍해지면 발각도 풀린다.</summary>
        public void ClearSuspicion()
        {
            _progress = 0f;
            _spottedRemaining = 0f;
        }

        private void Update()
        {
            if (_body == null ||
                _player == null)
            {
                return;
            }

            if (!_body.CanAct)
            {
                PlayerInSight = false;

                if (_body.IsStunned)
                {
                    ClearSuspicion();
                }

                return;
            }

            float deltaTime =
                Time.deltaTime;

            Vector2 self = transform.position;
            Vector2 player = _player.position;

            bool sameFloor =
                FloorSpace.FloorAt(player.y) ==
                _body.Floor;

            // 발밑 기준이 아니라 눈높이에서 본다. 캐릭터 발 위치 차이로 판정이 흔들리지 않게 한다.
            PlayerInSight =
                sameFloor &&
                DetectionLogic.IsInSight(
                    self.x,
                    self.y,
                    _body.Facing,
                    player.x,
                    player.y,
                    SightHalfAngle,
                    SightRange);

            // 25일차: 오피스 정전 중에는 손전등을 든 경비원만 본다.
            if (PlayerInSight &&
                _body.Profile != null &&
                !BlackoutLogic.CanSee(Blackout.IsActive, _body.Profile.SeesInBlackout))
            {
                PlayerInSight = false;
            }

            // 23일차: 해변가 파라솔 그늘 안은 보이지 않는다.
            if (PlayerInSight &&
                _body.Profile != null &&
                !_body.Profile.IgnoresShade &&
                ParasolShade.ActiveCount > 0 &&
                ParasolShade.IsShaded(player))
            {
                PlayerInSight = false;
            }

            bool casting =
                _caster != null &&
                _caster.CurrentTarget != null;

            bool dashing =
                _movement != null &&
                _movement.IsDashing;

            bool dashStarted =
                dashing &&
                !_wasDashing;

            _wasDashing = dashing;

            bool suspicious =
                PlayerInSight &&
                (casting || dashing);

            bool wasSpotting =
                IsSpotting;

            _progress =
                DetectionLogic.Advance(
                    _progress,
                    suspicious,
                    _followers == null
                        ? 0
                        : _followers.Count,
                    deltaTime);

            if (_progress >= 1f &&
                suspicious)
            {
                _spottedRemaining =
                    SpottedHoldSeconds;
            }
            else if (_spottedRemaining > 0f)
            {
                _spottedRemaining -=
                    deltaTime;
            }

            if (IsSpotting &&
                !wasSpotting)
            {
                HandleSpotted(player);
            }

            RaiseAlert(
                player,
                casting,
                dashStarted);

            UpdateInvestigation();
        }

        private void HandleSpotted(
            Vector2 player)
        {
            StageMoments.RaiseDisruptorSpotted(
                transform.position);

            // 25일차: 쇼핑몰 잠입 목표는 한 번이라도 들키면 보너스가 사라진다.
            if (ZoneAlert.Current != null)
            {
                ZoneAlert.Current.MarkSpotted();
            }

            Spotted?.Invoke(player);

            DisruptorProfile profile =
                _body.Profile;

            if (profile == null)
            {
                return;
            }

            if (profile.Stationary)
            {
                _body.FaceToward(
                    player.x);
            }
            else if (profile.ChaseSeconds > 0f)
            {
                _body.MoveToward(
                    player,
                    profile.ChaseSeconds);
            }
        }

        private void RaiseAlert(
            Vector2 player,
            bool casting,
            bool dashStarted)
        {
            ZoneAlert alert =
                ZoneAlert.Current;

            if (alert == null ||
                !IsSpotting)
            {
                return;
            }

            StageBalanceValues values =
                BalanceOverrides.StageOrDefault;

            if (casting &&
                PlayerInSight)
            {
                alert.Add(
                    values.AlertRisePerSecond *
                    Time.deltaTime,
                    player);
            }

            if (dashStarted &&
                PlayerInSight)
            {
                alert.Add(
                    values.AlertDashRise,
                    player);
            }
        }

        /// <summary>구역이 경계 단계면, 같은 층의 마지막 발견 지점을 주기적으로 살피러 간다.</summary>
        private void UpdateInvestigation()
        {
            if (_investigateCooldown > 0f)
            {
                _investigateCooldown -=
                    Time.deltaTime;

                return;
            }

            ZoneAlert alert =
                ZoneAlert.Current;

            if (alert == null ||
                alert.Level < AlertLevel.Alert ||
                !alert.HasLastSeen ||
                IsSpotting ||
                _body.Profile == null ||
                _body.Profile.Stationary ||
                FloorSpace.FloorAt(alert.LastSeenPosition.y) != _body.Floor)
            {
                return;
            }

            _investigateCooldown =
                InvestigateIntervalSeconds;

            _body.MoveToward(
                alert.LastSeenPosition,
                4f);
        }
    }
}
