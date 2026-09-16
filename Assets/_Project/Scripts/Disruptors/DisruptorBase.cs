using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 모든 방해 세력의 공통 몸체다 (22일차, 부록 C.9).
    ///
    /// 순찰 · 멍함 · 바라보는 방향 · 층 판정 · 활성 목록만 맡는다.
    /// 무엇을 하는지(감시 · 구출 · 쟁탈 …)는 같은 오브젝트에 붙는 역할 컴포넌트가 정한다.
    ///
    /// 경쟁자(<c>OpponentControllerBase</c>)와 달리 물리 몸체를 쓰지 않고 위치를 직접 옮긴다.
    /// 방해 세력은 NPC를 밀치거나 길을 막지 않으므로(길막 역할은 23일차에 따로) 충돌이 필요 없다.
    /// </summary>
    public sealed class DisruptorBase : MonoBehaviour
    {
        private static readonly List<DisruptorBase> Active =
            new List<DisruptorBase>();

        /// <summary>순찰 끝에서 돌아서기 전 잠깐 멈추는 시간이다.</summary>
        private const float TurnPauseSeconds = 0.8f;

        /// <summary>순찰 반경이다. 배치된 자리에서 좌우로 이만큼 오간다.</summary>
        private const float PatrolHalfWidth = 6f;

        private StageSessionController _stage;
        private RuntimeCharacterSpriteAnimator _animator;

        private float _patrolCenterX;
        private float _homeY;
        private float _pauseRemaining;
        private float _lookAroundRemaining;
        private float _stunRemaining;

        private bool _hasMoveTarget;
        private Vector2 _moveTarget;
        private float _moveTargetRemaining;

        public DisruptorProfile Profile { get; private set; }

        /// <summary>+1이면 오른쪽, -1이면 왼쪽을 본다.</summary>
        public int Facing { get; private set; } = 1;

        public int Floor { get; private set; }

        public bool IsStunned =>
            _stunRemaining > 0f;

        public float StunRemaining =>
            _stunRemaining;

        /// <summary>판이 돌고 있고 멈춤 화면이 아니며 멍하지 않은지다. 역할 컴포넌트는 이것만 보고 움직인다.</summary>
        public bool CanAct =>
            !IsStunned &&
            !GameplayPause.IsPaused &&
            (_stage == null ||
             _stage.IsRunning);

        public static int ActiveCount =>
            Active.Count;

        public static void CopyActive(
            List<DisruptorBase> buffer)
        {
            if (buffer == null)
            {
                return;
            }

            buffer.Clear();
            buffer.AddRange(Active);
        }

        public void Configure(
            DisruptorProfile profile,
            StageSessionController stage,
            RuntimeCharacterSpriteAnimator animator,
            int floor)
        {
            Profile = profile;
            _stage = stage;
            _animator = animator;
            Floor = floor;

            _patrolCenterX = transform.position.x;
            _homeY = transform.position.y;

            _lookAroundRemaining =
                profile == null
                    ? 5f
                    : profile.LookAroundSeconds;

            // 층마다 처음 보는 방향을 엇갈려 둔다.
            Facing =
                floor % 2 == 0
                    ? 1
                    : -1;

            ApplyFacing();
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        /// <summary>파동을 맞으면 멍해진다. 멍한 동안은 보지도, 능력을 쓰지도 않는다.</summary>
        public void Stun(
            float seconds)
        {
            _stunRemaining =
                Mathf.Max(
                    _stunRemaining,
                    seconds);
        }

        /// <summary>감시자가 발견 지점으로 달려갈 때 쓴다. 시간이 지나면 순찰로 돌아간다.</summary>
        public void MoveToward(
            Vector2 target,
            float seconds)
        {
            if (Profile == null ||
                Profile.Stationary)
            {
                return;
            }

            _hasMoveTarget = true;
            _moveTarget = target;
            _moveTargetRemaining = seconds;
        }

        /// <summary>제자리 개체가 특정 방향을 바라보게 한다.</summary>
        public void FaceToward(
            float worldX)
        {
            float delta =
                worldX -
                transform.position.x;

            if (Mathf.Abs(delta) < 0.05f)
            {
                return;
            }

            Facing =
                delta > 0f
                    ? 1
                    : -1;

            ApplyFacing();
        }

        private void Update()
        {
            if (_stunRemaining > 0f &&
                !GameplayPause.IsPaused)
            {
                _stunRemaining -=
                    Time.deltaTime;
            }

            if (!CanAct ||
                Profile == null)
            {
                return;
            }

            if (Profile.Stationary)
            {
                UpdateLookAround();

                return;
            }

            if (_hasMoveTarget)
            {
                UpdateMoveTarget();

                return;
            }

            UpdatePatrol();
        }

        private void UpdateLookAround()
        {
            _lookAroundRemaining -=
                Time.deltaTime;

            if (_lookAroundRemaining > 0f)
            {
                return;
            }

            _lookAroundRemaining =
                Mathf.Max(
                    0.5f,
                    Profile.LookAroundSeconds);

            Facing = -Facing;

            ApplyFacing();
        }

        private void UpdatePatrol()
        {
            if (_pauseRemaining > 0f)
            {
                _pauseRemaining -=
                    Time.deltaTime;

                return;
            }

            float speed =
                Profile.MoveSpeed *
                GetLevelSpeedMultiplier();

            float minX =
                Mathf.Max(
                    FloorSpace.WalkMinX + 1f,
                    _patrolCenterX - PatrolHalfWidth);

            float maxX =
                Mathf.Min(
                    FloorSpace.WalkMaxX - 1f,
                    _patrolCenterX + PatrolHalfWidth);

            Vector3 position =
                transform.position;

            // 발견 지점에 갔다가 순찰 구역 밖에 있으면 순간이동하지 않고 걸어서 돌아온다.
            bool outside =
                position.x > maxX + 0.01f ||
                position.x < minX - 0.01f;

            if (outside)
            {
                int back =
                    position.x > maxX
                        ? -1
                        : 1;

                if (back != Facing)
                {
                    Facing = back;

                    ApplyFacing();
                }
            }

            position.x +=
                Facing *
                speed *
                Time.deltaTime;

            if (!outside &&
                (position.x >= maxX ||
                 position.x <= minX))
            {
                position.x =
                    Mathf.Clamp(
                        position.x,
                        minX,
                        maxX);

                Facing = -Facing;

                _pauseRemaining =
                    TurnPauseSeconds;

                ApplyFacing();
            }

            // 발견 지점으로 달려갔다가 돌아오는 동안 벗어난 세로 자리를 천천히 되돌린다.
            position.y =
                Mathf.MoveTowards(
                    position.y,
                    _homeY,
                    speed * 0.5f * Time.deltaTime);

            transform.position = position;
        }

        private void UpdateMoveTarget()
        {
            _moveTargetRemaining -=
                Time.deltaTime;

            Vector2 current =
                transform.position;

            // 다른 층의 발견 지점으로는 가지 않는다. 층을 넘는 것은 계단뿐이다.
            float targetY =
                FloorSpace.ClampYNear(
                    current.y,
                    _moveTarget.y,
                    0.2f,
                    0.2f);

            Vector2 target =
                new Vector2(
                    Mathf.Clamp(
                        _moveTarget.x,
                        FloorSpace.WalkMinX + 1f,
                        FloorSpace.WalkMaxX - 1f),
                    targetY);

            float speed =
                Profile.MoveSpeed *
                1.5f;

            Vector2 next =
                Vector2.MoveTowards(
                    current,
                    target,
                    speed * Time.deltaTime);

            if (Mathf.Abs(next.x - current.x) > 0.001f)
            {
                Facing =
                    next.x > current.x
                        ? 1
                        : -1;

                ApplyFacing();
            }

            transform.position =
                new Vector3(
                    next.x,
                    next.y,
                    transform.position.z);

            if (_moveTargetRemaining <= 0f ||
                (next - target).sqrMagnitude < 0.04f)
            {
                _hasMoveTarget = false;

                // 도착한 자리를 새 순찰 중심으로 삼지 않는다. 원래 구역으로 돌아가 순찰한다.
                _pauseRemaining = TurnPauseSeconds;
            }
        }

        private static float GetLevelSpeedMultiplier()
        {
            ZoneAlert alert =
                ZoneAlert.Current;

            return alert == null
                ? 1f
                : ZoneAlertLogic.GetPatrolSpeedMultiplier(
                    alert.Level);
        }

        private void ApplyFacing()
        {
            if (_animator != null)
            {
                _animator.FaceHorizontal(
                    Facing);
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active.Clear();
        }
    }
}
