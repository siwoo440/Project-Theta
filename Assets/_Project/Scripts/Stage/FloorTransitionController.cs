using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Rival;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 계단으로 층을 오르내린다.
    ///
    /// 모든 층이 같은 씬에 세로로 쌓여 있으므로 이동은 "순간이동"이다.
    /// 씬 로드도, NPC 재생성도 없다. 그래서 동행 NPC가 자연스럽게 따라온다.
    ///
    /// 계단 앞에 서면 안내가 뜨고, 상호작용 키 F로 이동한다.
    /// 위층 계단은 복도 오른쪽, 아래층 계단은 왼쪽에 있어 방향은 계단 위치가 정한다.
    /// 닿기만 해도 넘어가면 계단 앞을 지나칠 때마다 층이 바뀌어 조작이 불가능해진다.
    ///
    /// 17일차에 W/S에서 F로 바꿨다. W/S는 이동 키라서, 계단 앞에서 위아래로
    /// 걷기만 해도 층이 바뀌는 문제가 있었다.
    /// </summary>
    public sealed class FloorTransitionController : MonoBehaviour
    {
        /// <summary>경쟁자 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<OpponentControllerBase> _opponentBuffer =
            new System.Collections.Generic.List<OpponentControllerBase>();
        /// <summary>경쟁자가 계단을 타고 쫓아오기까지의 시간이다.</summary>
        [SerializeField] private float _opponentChaseDelay = 3.2f;

        /// <summary>층을 옮긴 직후 잠시 계단 입력을 막아 왕복 튕김을 방지한다.</summary>
        [SerializeField] private float _transitionCooldown = 0.45f;

        private Transform _player;
        private FollowerManager _followers;
        private CameraFollow2D _camera;

        private readonly List<FloorStairway> _stairways =
            new List<FloorStairway>();

        private FloorRunState _run;

        private float _cooldownRemaining;
        private float _opponentChaseRemaining;
        private bool _opponentChasePending;

        /// <summary>
        /// 플레이어와 한 번이라도 같은 층에 있었던 경쟁자다.
        /// 만나기 전에는 쫓아오지 않는다. 그래야 층별 첫 조우가 의미를 가진다.
        /// </summary>
        private readonly HashSet<OpponentControllerBase> _metOpponents =
            new HashSet<OpponentControllerBase>();

        /// <summary>지금 쓸 수 있는 계단이다. 없으면 null이다.</summary>
        public FloorStairway ActiveStairway { get; private set; }

        public FloorRunState Run =>
            _run;

        public int CurrentFloor =>
            _run == null
                ? 0
                : _run.CurrentFloor;

        public int FloorCount =>
            _run == null
                ? 1
                : _run.FloorCount;

        /// <summary>층이 바뀔 때 알린다. 인자는 (이전 층, 새 층, 첫 방문 여부)다.</summary>
        public event System.Action<int, int, bool> FloorChanged;

        public void Configure(
            Transform player,
            FollowerManager followers,
            CameraFollow2D cameraFollow,
            int floorCount)
        {
            _player = player;
            _followers = followers;
            _camera = cameraFollow;

            _run =
                new FloorRunState(
                    floorCount);

            FloorVisibility.ViewFloor =
                _run.CurrentFloor;

            RefreshStairways();

            RecordMeetings();
        }

        /// <summary>씬에 있는 계단을 모두 찾아 둔다. 층은 미리 다 지어져 있으므로 한 번만 하면 된다.</summary>
        public void RefreshStairways()
        {
            _stairways.Clear();

            FloorStairway[] found =
                FindObjectsByType<FloorStairway>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < found.Length;
                 i++)
            {
                _stairways.Add(
                    found[i]);
            }
        }

        private void Update()
        {
            if (GameplayPause.IsPaused)
            {
                ActiveStairway = null;

                return;
            }

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -=
                    Time.deltaTime;
            }

            TickOpponentChase();

            ActiveStairway =
                FindUsableStairway();

            if (ActiveStairway == null ||
                _cooldownRemaining > 0f)
            {
                return;
            }

            if (_emergencyConfirmRemaining > 0f)
            {
                _emergencyConfirmRemaining -= Time.deltaTime;
            }

            if (ReadInteractPressed())
            {
                TryUse(
                    ActiveStairway);
            }
        }

        /// <summary>잠긴 계단을 한 번 누른 뒤 남은 확인 시간이다 (25일차).</summary>
        private float _emergencyConfirmRemaining;

        /// <summary>
        /// 25일차: 오피스 출입증 게이트가 위층 계단을 잠갔으면 한 번은 안내만 하고,
        /// 확인 시간 안에 다시 누르면 비상계단으로 올라가되 경계도가 오른다.
        /// </summary>
        private void TryUse(
            FloorStairway stairway)
        {
            // 26일차: 클럽 바운서는 비상 우회가 없다.
            if (stairway.Direction == FloorStairDirection.Up &&
                Locations.VipEntrance.IsLocked(stairway.SourceFloor))
            {
                GameVfx.FloatText(
                    $"바운서: 동행 {Locations.BouncerLogic.MaximumGuests}명을 넘으면 입장 불가 · [VIP] 게스트와 오거나 파동으로 따돌리세요",
                    (Vector2)_player.position + new Vector2(0f, 2.2f),
                    UI.Framework.UiTheme.Danger,
                    UI.Framework.UiTheme.FontBody,
                    1.6f);

                return;
            }

            bool locked =
                stairway.Direction == FloorStairDirection.Up &&
                Locations.PassGate.IsLocked(stairway.SourceFloor);

            if (!locked)
            {
                Travel(stairway);

                return;
            }

            if (_emergencyConfirmRemaining <= 0f)
            {
                _emergencyConfirmRemaining =
                    Disruptors.PassGateLogic.EmergencyConfirmSeconds;

                GameVfx.FloatText(
                    $"출입증이 필요합니다 · 한 번 더 누르면 비상계단 (경계도 +{Disruptors.PassGateLogic.EmergencyStairAlertRise:0})",
                    (Vector2)_player.position + new Vector2(0f, 2.2f),
                    UI.Framework.UiTheme.Danger,
                    UI.Framework.UiTheme.FontBody,
                    1.6f);

                return;
            }

            _emergencyConfirmRemaining = 0f;

            if (Disruptors.ZoneAlert.Current != null)
            {
                Disruptors.ZoneAlert.Current.Add(
                    Disruptors.PassGateLogic.EmergencyStairAlertRise,
                    _player.position);
            }

            Travel(stairway);
        }

        /// <summary>플레이어가 선 층에서, 손이 닿는 거리의 계단을 고른다.</summary>
        private FloorStairway FindUsableStairway()
        {
            if (_player == null ||
                _run == null)
            {
                return null;
            }

            Vector2 position =
                _player.position;

            FloorStairway nearest = null;

            float nearestDistance =
                float.MaxValue;

            for (int i = 0;
                 i < _stairways.Count;
                 i++)
            {
                FloorStairway stairway =
                    _stairways[i];

                if (stairway == null ||
                    stairway.SourceFloor !=
                    _run.CurrentFloor)
                {
                    continue;
                }

                if (!stairway.IsWithinRange(
                        position))
                {
                    continue;
                }

                float distance =
                    Vector2.Distance(
                        stairway.transform.position,
                        position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = stairway;
                }
            }

            return nearest;
        }

        /// <summary>상호작용 키 F다. 계단이 어느 방향인지는 선 자리의 계단이 정한다.</summary>
        private static bool ReadInteractPressed()
        {
#if ENABLE_INPUT_SYSTEM
            Keyboard keyboard =
                Keyboard.current;

            return keyboard != null &&
                   keyboard.fKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.F);
#endif
        }

        /// <summary>계단을 타고 이동한다. 동행 NPC도 함께 옮긴다.</summary>
        public void Travel(
            FloorStairway stairway)
        {
            if (stairway == null ||
                _run == null ||
                _player == null)
            {
                return;
            }

            TravelTo(
                stairway.TargetFloor,
                stairway.GetArrivalPosition());
        }

        /// <summary>
        /// 디버그 치트: 계단 없이 바로 그 층으로 간다 (20일차).
        /// 올라가면 그 층의 아래층 계단 앞, 내려가면 위층 계단 앞에 선다. 계단으로 온 것과 같은 자리다.
        /// 동행·경쟁자 조우·첫 방문 경험치도 계단 이동과 똑같이 처리된다.
        /// </summary>
        public bool DebugTravelTo(
            int targetFloor)
        {
            if (_run == null ||
                _player == null ||
                !_run.CanMoveTo(
                    targetFloor))
            {
                return false;
            }

            float x =
                targetFloor > _run.CurrentFloor
                    ? FloorLayout.DownStairX
                    : FloorLayout.UpStairX;

            TravelTo(
                targetFloor,
                FloorSpace.ToWorld(
                    targetFloor,
                    new Vector2(
                        x,
                        FloorLayout.StairStandY)));

            return true;
        }

        private void TravelTo(
            int targetFloor,
            Vector2 arrival)
        {
            if (!_run.CanMoveTo(
                    targetFloor))
            {
                return;
            }

            int previousFloor =
                _run.CurrentFloor;

            // MoveTo는 그 층에 처음 도달했을 때만 true를 돌려준다.
            bool firstVisit =
                _run.MoveTo(
                    targetFloor);

            FloorVisibility.ViewFloor =
                _run.CurrentFloor;

            MoveEntities(
                arrival);

            // 도착한 층에 원래 있던 경쟁자와 여기서 처음 만난다.
            RecordMeetings();

            _cooldownRemaining =
                _transitionCooldown;

            _opponentChasePending = true;

            _opponentChaseRemaining =
                _opponentChaseDelay;

            // 도착 효과음과 페이드는 StageVfxDirector가 FloorChanged를 듣고 낸다 (19일차).
            FloorChanged?.Invoke(
                previousFloor,
                _run.CurrentFloor,
                firstVisit);
        }

        /// <summary>플레이어와 동행자, 카메라를 새 층으로 옮긴다.</summary>
        private void MoveEntities(
            Vector2 arrival)
        {
            Vector2 previousPlayer =
                _player.position;

            SetPosition(
                _player,
                arrival);

            if (_followers != null)
            {
                MoveFollowers(
                    previousPlayer,
                    arrival);
            }

            if (_camera != null)
            {
                _camera.SnapToTarget();
            }
        }

        /// <summary>
        /// 동행자는 플레이어를 기준으로 한 상대 위치를 유지한 채 옮긴다.
        /// 전부 같은 점에 모으면 계단 앞에서 한 덩어리로 겹쳐 보인다.
        /// </summary>
        private void MoveFollowers(
            Vector2 previousPlayer,
            Vector2 arrival)
        {
            IReadOnlyList<FollowerController> followers =
                _followers.Followers;

            // 27일차: 오피스 엘리베이터는 한 번에 4명까지만 탄다.
            bool limited =
                Locations.OfficeLayout.Active;

            int riders = 0;
            int left = 0;

            for (int i = 0;
                 i < followers.Count;
                 i++)
            {
                FollowerController follower =
                    followers[i];

                if (follower == null ||
                    ElevatorWait.IsWaitingFollower(follower))
                {
                    continue;
                }

                if (riders >=
                    Locations.ElevatorLogic.GetRidersAllowed(
                        limited,
                        followers.Count))
                {
                    ElevatorWait.Begin(
                        follower,
                        _followers,
                        _player);

                    left++;

                    continue;
                }

                riders++;

                Vector2 offset =
                    (Vector2)follower.transform.position -
                    previousPlayer;

                // 계단 앞은 좁으므로 간격을 절반으로 줄여 벽을 뚫지 않게 한다.
                Vector2 target =
                    arrival +
                    (offset * 0.5f);

                target.x =
                    Mathf.Clamp(
                        target.x,
                        FloorSpace.WalkMinX + 0.8f,
                        FloorSpace.WalkMaxX - 0.8f);

                target.y =
                    FloorSpace.ClampYOn(
                        _run.CurrentFloor,
                        target.y,
                        0.35f,
                        0.25f);

                SetPosition(
                    follower.transform,
                    target);
            }

            if (left > 0)
            {
                GameVfx.FloatText(
                    $"엘리베이터 정원 {Locations.ElevatorLogic.Capacity}명 · {left}명은 아래층에서 대기 ({Locations.ElevatorLogic.WaitSeconds:0}초)",
                    arrival + new Vector2(0f, 2.4f),
                    UI.Framework.UiTheme.Danger,
                    UI.Framework.UiTheme.FontBody,
                    1.8f);
            }
        }

        /// <summary>
        /// 경쟁자는 잠시 뒤 계단을 타고 쫓아온다.
        /// 층 이동이 완전한 도피가 되면 쟁탈 압박이 사라진다.
        /// </summary>
        private void TickOpponentChase()
        {
            if (!_opponentChasePending)
            {
                return;
            }

            _opponentChaseRemaining -=
                Time.deltaTime;

            if (_opponentChaseRemaining > 0f)
            {
                return;
            }

            _opponentChasePending = false;

            ChaseOpponentsToCurrentFloor();
        }

        private void ChaseOpponentsToCurrentFloor()
        {
            OpponentControllerBase.CopyActive(
                _opponentBuffer);

            System.Collections.Generic.List<OpponentControllerBase> opponents =
                _opponentBuffer;

            if (opponents.Count == 0)
            {
                return;
            }

            for (int i = 0;
                 i < opponents.Count;
                 i++)
            {
                OpponentControllerBase opponent =
                    opponents[i];

                if (opponent == null)
                {
                    continue;
                }

                int opponentFloor =
                    FloorSpace.FloorAt(
                        opponent.transform.position.y);

                if (!OpponentFloorPlan.ShouldChase(
                        _metOpponents.Contains(
                            opponent),
                        opponentFloor,
                        _run.CurrentFloor))
                {
                    continue;
                }

                // 경쟁자마다 다른 계단으로 나타나게 해 한곳에 몰리지 않게 한다.
                float x =
                    i % 2 == 0
                        ? FloorLayout.UpStairX
                        : FloorLayout.DownStairX;

                Vector2 arrival =
                    FloorSpace.ToWorld(
                        _run.CurrentFloor,
                        new Vector2(
                            x,
                            FloorLayout.StairStandY - 0.6f));

                MoveOpponent(
                    opponent,
                    arrival);
            }
        }

        /// <summary>플레이어와 같은 층에 있는 경쟁자를 "만난 경쟁자"로 기록한다.</summary>
        private void RecordMeetings()
        {
            if (_run == null)
            {
                return;
            }

            OpponentControllerBase.CopyActive(
                _opponentBuffer);

            for (int i = 0;
                 i < _opponentBuffer.Count;
                 i++)
            {
                OpponentControllerBase opponent =
                    _opponentBuffer[i];

                if (opponent != null &&
                    FloorSpace.FloorAt(
                        opponent.transform.position.y) ==
                    _run.CurrentFloor)
                {
                    _metOpponents.Add(
                        opponent);
                }
            }
        }

        /// <summary>경쟁자와 그가 데리고 있는 NPC를 함께 옮긴다.</summary>
        private void MoveOpponent(
            OpponentControllerBase opponent,
            Vector2 arrival)
        {
            Vector2 previous =
                opponent.transform.position;

            SetPosition(
                opponent.transform,
                arrival);

            OpponentFollowerManager followers =
                opponent.GetComponent<
                    OpponentFollowerManager>();

            if (followers == null)
            {
                return;
            }

            IReadOnlyList<OpponentFollowerController> links =
                followers.Followers;

            for (int i = 0;
                 i < links.Count;
                 i++)
            {
                if (links[i] == null)
                {
                    continue;
                }

                Vector2 offset =
                    (Vector2)links[i].transform.position -
                    previous;

                SetPosition(
                    links[i].transform,
                    arrival +
                    (offset * 0.5f));
            }
        }

        /// <summary>
        /// 물리 몸체가 있으면 위치를 함께 옮긴다.
        /// transform만 바꾸면 Rigidbody2D가 이전 위치로 되돌린다.
        /// </summary>
        private static void SetPosition(
            Transform target,
            Vector2 position)
        {
            if (target == null)
            {
                return;
            }

            target.position =
                new Vector3(
                    position.x,
                    position.y,
                    target.position.z);

            Rigidbody2D body =
                target.GetComponent<Rigidbody2D>();

            if (body == null)
            {
                return;
            }

            body.position = position;
            body.linearVelocity = Vector2.zero;
        }
    }
}
