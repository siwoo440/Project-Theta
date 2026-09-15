using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.Rival
{
    /// <summary>
    /// 금태양·인기남이 공유하는 경쟁자 AI 골격이다.
    /// 상태 머신, 이동, Idle 대기, 추적 포기, 힘겨루기 연동, 소유권 전환 경로를 담당한다.
    /// 파생 클래스는 "누구를 노릴지"와 "어떻게 확보할지"만 구현한다.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(OpponentFollowerManager))]
    public abstract class OpponentControllerBase : MonoBehaviour
    {
        [SerializeField] private OpponentTuning _tuning;

        private Rigidbody2D _body;
        private RuntimeCharacterSpriteAnimator _animator;

        private float _reacquireTimer;
        private float _idleRemaining;
        private float _pursuitElapsed;
        private bool _duelLocked;
        private float _duelStunRemaining;

        public abstract NpcOwner OwnerTag { get; }

        public abstract string DisplayName { get; }

        public abstract bool CanStartPlayerDuel { get; }

        public OpponentState State { get; protected set; } =
            OpponentState.Idle;

        public int FacingDirection { get; private set; } =
            -1;

        public int OwnedFollowerCount =>
            OwnedFollowers == null
                ? 0
                : OwnedFollowers.Count;

        public string CurrentTargetName =>
            Target == null
                ? "-"
                : Target.name;

        public float CurrentTargetControlNormalized =>
            Target == null
                ? 0f
                : Target.HypnosisNormalized;

        public virtual string CurrentModeLabel
        {
            get
            {
                switch (TargetMode)
                {
                    case OpponentTargetMode.NeutralClaim:
                        return "중립 선점";

                    case OpponentTargetMode.Contest:
                        return "쟁탈";

                    case OpponentTargetMode.None:
                    default:
                        return "-";
                }
            }
        }

        protected OpponentTuning Tuning =>
            _tuning;

        protected StageSessionController Stage { get; private set; }

        protected FollowerManager PlayerFollowers { get; private set; }

        protected OpponentFollowerManager OwnedFollowers { get; private set; }

        protected HypnosisTarget Target { get; private set; }

        protected OpponentTargetMode TargetMode { get; private set; } =
            OpponentTargetMode.None;

        protected bool IsDuelLocked =>
            _duelLocked;

        protected float DuelStunRemaining =>
            _duelStunRemaining;

        /// <summary>파생 클래스가 자신의 기본 수치를 제공한다.</summary>
        protected abstract OpponentTuning CreateDefaultTuning();

        /// <summary>
        /// 새 타겟을 찾는다. 찾으면 <see cref="AssignTarget"/>을 호출하고,
        /// 못 찾으면 아무것도 하지 않는다.
        /// </summary>
        protected abstract void SearchForTarget();

        protected abstract bool IsTargetValid(
            HypnosisTarget target,
            OpponentTargetMode mode);

        /// <summary>행동 거리 안에서 매 프레임 수행하는 확보 행동이다.</summary>
        protected abstract void PerformAction(
            HypnosisTarget target,
            OpponentTargetMode mode);

        protected virtual void OnTargetAssigned(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
        }

        protected virtual void OnTargetVisualsCleared(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
        }

        protected virtual void OnTargetReleased()
        {
        }

        protected virtual void Awake()
        {
            // Unity는 직렬화된 사용자 클래스 필드를 기본 생성 인스턴스로 채울 수 있으므로
            // null 검사에 의존하지 않고 항상 파생 클래스의 수치로 초기화한다.
            // 자산이 주입돼 있으면 그 값을 우선 사용한다.
            _tuning =
                OpponentTuningOverrides.Get(
                    OwnerTag) ??
                CreateDefaultTuning();

            _body =
                GetComponent<Rigidbody2D>();

            OwnedFollowers =
                GetComponent<
                    OpponentFollowerManager>();

            _body.gravityScale =
                0f;

            _body.freezeRotation =
                true;

            _body.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;

            _body.interpolation =
                RigidbodyInterpolation2D.Interpolate;
        }

        public void Configure(
            StageSessionController stage,
            FollowerManager playerFollowers,
            RuntimeCharacterSpriteAnimator animator)
        {
            Stage =
                stage;

            PlayerFollowers =
                playerFollowers;

            _animator =
                animator;

            EnterIdle(
                Tuning.MinimumIdleDuration,
                Tuning.MaximumIdleDuration);
        }

        protected virtual void Update()
        {
            if (Stage == null ||
                !Stage.IsRunning)
            {
                StopMovement();

                State =
                    OpponentState.Idle;

                return;
            }

            if (_duelLocked)
            {
                StopMovement();

                return;
            }

            if (_duelStunRemaining >
                0f)
            {
                UpdateDuelStun();

                return;
            }

            if (State ==
                OpponentState.Idle)
            {
                UpdateIdle();

                return;
            }

            OpponentTargetDecision decision =
                OpponentTargetDecisionLogic.Resolve(
                    Target != null,
                    Target != null &&
                    IsTargetValid(
                        Target,
                        TargetMode));

            if (decision ==
                OpponentTargetDecision.Search)
            {
                UpdateTargetSearch();

                return;
            }

            if (decision ==
                OpponentTargetDecision.Wait)
            {
                ClearTarget();

                return;
            }

            Vector2 delta =
                (Vector2)Target.transform.position -
                (Vector2)transform.position;

            FaceHorizontal(
                delta.x);

            float distance =
                delta.magnitude;

            if (distance >
                Tuning.ActionDistance)
            {
                _pursuitElapsed +=
                    Time.deltaTime;

                if (OpponentTargetingLogic.
                        ShouldAbandon(
                            distance,
                            _pursuitElapsed,
                            Tuning.AbandonDistance,
                            Tuning.MaximumPursuitDuration))
                {
                    ClearTarget();

                    return;
                }

                State =
                    OpponentState.Approach;

                return;
            }

            _pursuitElapsed =
                0f;

            StopMovement();

            PerformAction(
                Target,
                TargetMode);
        }

        protected virtual void FixedUpdate()
        {
            if (Stage == null ||
                !Stage.IsRunning ||
                _duelLocked ||
                _duelStunRemaining >
                    0f ||
                State !=
                OpponentState.Approach ||
                Target == null)
            {
                if (State !=
                    OpponentState.Approach)
                {
                    StopMovement();
                }

                return;
            }

            Vector2 delta =
                (Vector2)Target.transform.position -
                (Vector2)transform.position;

            float distance =
                delta.magnitude;

            if (distance <=
                Tuning.StopDistance)
            {
                StopMovement();

                return;
            }

            _body.linearVelocity =
                delta.normalized *
                Tuning.MoveSpeed;
        }

        public void SetDuelLocked(
            bool locked)
        {
            _duelLocked =
                locked;

            if (locked)
            {
                StopMovement();
            }
        }

        public void ApplyDuelStun(
            float duration)
        {
            ClearCurrentTargetVisuals();

            ReleaseTargetState();

            _duelLocked =
                false;

            _duelStunRemaining =
                Mathf.Max(
                    0f,
                    duration);

            State =
                OpponentState.Stunned;

            StopMovement();
        }

        public void ReleaseOwnedTarget(
            HypnosisTarget target)
        {
            if (target == null ||
                OwnedFollowers == null)
            {
                return;
            }

            OwnedFollowers.RemoveTarget(
                target);
        }

        protected void AssignTarget(
            HypnosisTarget target,
            OpponentTargetMode mode)
        {
            if (target == null)
            {
                return;
            }

            Target =
                target;

            TargetMode =
                mode;

            _pursuitElapsed =
                0f;

            OnTargetAssigned(
                target,
                mode);

            target.SetOpponentTargeted(
                OwnerTag,
                true);

            State =
                OpponentState.Approach;
        }

        protected void ClearTarget()
        {
            EnterIdle(
                Tuning.LostTargetIdleMinimum,
                Tuning.LostTargetIdleMaximum);
        }

        /// <summary>
        /// NPC 확보에 성공한 뒤 호출한다. 확보한 NPC는 더 이상 타겟이 아니므로
        /// 타겟 표시를 지우지 않고 곧바로 확보 후 Idle로 넘어간다.
        /// </summary>
        protected void FinishSuccessfulCapture()
        {
            ReleaseTargetState();

            EnterIdle(
                Tuning.PostCaptureIdleMinimum,
                Tuning.PostCaptureIdleMaximum);
        }

        /// <summary>
        /// 플레이어 소유 NPC를 실제로 빼앗는 공통 경로다.
        /// 충동 제어를 정리하고 플레이어 Follower 목록에서 분리한다.
        /// </summary>
        protected bool TryDetachFromPlayer(
            HypnosisTarget target)
        {
            if (target == null)
            {
                return false;
            }

            ImpulseMeter impulse =
                target.GetComponent<
                    ImpulseMeter>();

            impulse?.CancelForRecovery();

            FollowerController follower =
                target.GetComponent<
                    FollowerController>();

            bool detached =
                PlayerFollowers != null &&
                follower != null &&
                PlayerFollowers.TransferOutFollower(
                    follower);

            if (detached)
            {
                StageMoments.RaiseFollowerStolen(
                    target.transform.position);
            }

            return detached;
        }

        /// <summary>플레이어 동행 NPC가 지금 쟁탈 대상이 될 수 있는 상태인지 판정한다.</summary>
        protected static bool IsPlayerFollowerContestable(
            HypnosisTarget target)
        {
            if (target == null ||
                !target.IsFollowing)
            {
                return false;
            }

            ImpulseMeter impulse =
                target.GetComponent<
                    ImpulseMeter>();

            if (impulse == null)
            {
                return true;
            }

            switch (impulse.State)
            {
                case ImpulseState.Preparing:
                case ImpulseState.Rampaging:
                case ImpulseState.Capturing:
                case ImpulseState.Recovering:
                    return false;

                case ImpulseState.Calm:
                case ImpulseState.Warning:
                case ImpulseState.Danger:
                default:
                    return true;
            }
        }

        private void UpdateDuelStun()
        {
            _duelStunRemaining =
                Mathf.Max(
                    0f,
                    _duelStunRemaining -
                    Time.deltaTime);

            State =
                OpponentState.Stunned;

            StopMovement();

            if (_duelStunRemaining <=
                0f)
            {
                EnterIdle(
                    Tuning.LostTargetIdleMinimum,
                    Tuning.LostTargetIdleMaximum);
            }
        }

        private void UpdateTargetSearch()
        {
            _reacquireTimer -=
                Time.deltaTime;

            if (_reacquireTimer >
                0f)
            {
                return;
            }

            _reacquireTimer =
                Mathf.Max(
                    0.05f,
                    Tuning.ReacquireInterval);

            SearchForTarget();

            if (Target == null)
            {
                State =
                    OpponentState.Search;
            }
        }

        private void UpdateIdle()
        {
            StopMovement();

            _idleRemaining -=
                Time.deltaTime;

            if (_idleRemaining >
                0f)
            {
                return;
            }

            State =
                OpponentState.Search;

            _reacquireTimer =
                0f;
        }

        private void EnterIdle(
            float minimum,
            float maximum)
        {
            ClearCurrentTargetVisuals();

            State =
                OpponentState.Idle;

            ReleaseTargetState();

            _idleRemaining =
                OpponentIdleLogic.ResolveDuration(
                    minimum,
                    maximum,
                    Random.value);

            _reacquireTimer =
                0f;

            StopMovement();
        }

        private void ClearCurrentTargetVisuals()
        {
            if (Target == null)
            {
                return;
            }

            OnTargetVisualsCleared(
                Target,
                TargetMode);

            Target.SetOpponentTargeted(
                OwnerTag,
                false);
        }

        private void ReleaseTargetState()
        {
            Target =
                null;

            TargetMode =
                OpponentTargetMode.None;

            _pursuitElapsed =
                0f;

            OnTargetReleased();
        }

        private void FaceHorizontal(
            float deltaX)
        {
            if (Mathf.Abs(
                    deltaX) <=
                0.001f)
            {
                return;
            }

            FacingDirection =
                deltaX > 0f
                    ? 1
                    : -1;

            _animator?.FaceHorizontal(
                deltaX);
        }

        protected void StopMovement()
        {
            if (_body != null)
            {
                _body.linearVelocity =
                    Vector2.zero;
            }
        }

        /// <summary>
        /// 대상이 같은 층에 있는지 본다.
        ///
        /// 층은 세로로 16씩 떨어져 있어서, 다른 층 NPC를 노리면 벽에 막혀 걸어가지 못한다.
        /// 특히 인기남은 탐색 범위가 사실상 무제한(999)이라, 이 확인이 없으면
        /// 위아래 층 NPC를 향해 벽에 붙어 서 있게 된다.
        /// </summary>
        protected bool IsOnSameFloor(
            HypnosisTarget target)
        {
            return target != null &&
                   FloorSpace.FloorAt(
                       target.transform.position.y) ==
                   FloorSpace.FloorAt(
                       transform.position.y);
        }

        /// <summary>활성화된 경쟁자 목록이다. 씬 검색 대신 여기서 읽는다.</summary>
        private static readonly System.Collections.Generic.List<OpponentControllerBase> ActiveOpponents =
            new System.Collections.Generic.List<OpponentControllerBase>();

        /// <summary>활성 경쟁자를 호출한 쪽의 버퍼에 복사한다. 순회 중 목록 변경을 막기 위해 복사한다.</summary>
        public static void CopyActive(
            System.Collections.Generic.List<OpponentControllerBase> buffer)
        {
            buffer.Clear();

            buffer.AddRange(
                ActiveOpponents);
        }

        protected virtual void OnEnable()
        {
            if (!ActiveOpponents.Contains(this))
            {
                ActiveOpponents.Add(this);
            }
        }

        protected virtual void OnDisable()
        {
            ActiveOpponents.Remove(this);

            ClearCurrentTargetVisuals();

            StopMovement();
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveOpponents.Clear();
        }
    }
}
