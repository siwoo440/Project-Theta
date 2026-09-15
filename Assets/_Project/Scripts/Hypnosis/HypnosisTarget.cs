using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.NPC;
using ProjectTheta.Ownership;
using ProjectTheta.Player;
using ProjectTheta.Rival;

namespace ProjectTheta.Hypnosis
{
    [RequireComponent(typeof(NpcAgent))]
    public sealed class HypnosisTarget : MonoBehaviour
    {
        /// <summary>
        /// 활성화된 최면 대상 목록이다.
        ///
        /// 18일차 전까지는 대상을 찾을 때마다 FindObjectsByType으로 씬 전체를 뒤졌다.
        /// 최면 시전은 이걸 매 프레임 했고, 16일차에 층이 생기며 NPC가 52명으로 늘어
        /// 매 프레임 씬 검색 + 배열 할당이 일어났다.
        /// 켜질 때 등록하고 꺼질 때 빼는 방식으로 바꿔 검색 비용을 없앴다.
        /// </summary>
        private static readonly System.Collections.Generic.List<HypnosisTarget> ActiveTargets =
            new System.Collections.Generic.List<HypnosisTarget>();

        public static int ActiveCount =>
            ActiveTargets.Count;

        /// <summary>
        /// 활성 대상을 호출한 쪽의 버퍼에 복사한다.
        /// 목록을 그대로 넘기지 않는 이유는, 순회 도중 NPC가 꺼지거나 켜지면
        /// 원본 목록이 바뀌어 순회가 깨지기 때문이다. 버퍼는 재사용하므로 할당이 없다.
        /// </summary>
        public static void CopyActive(
            System.Collections.Generic.List<HypnosisTarget> buffer)
        {
            buffer.Clear();

            buffer.AddRange(
                ActiveTargets);
        }

        private void OnEnable()
        {
            if (!ActiveTargets.Contains(this))
            {
                ActiveTargets.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveTargets.Remove(this);
        }

        /// <summary>도메인 리로드가 꺼져 있어도 이전 플레이의 등록이 남지 않게 한다.</summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetRegistry()
        {
            ActiveTargets.Clear();
        }

        [SerializeField] private float _maximumHypnosis = 100f;
        [SerializeField] private float _buildPerSecond = 32f;
        [SerializeField] private float _playerReclaimPerSecond = 24f;

        private RuntimeCharacterSpriteAnimator _animator;
        private NpcProfile _profile;
        private NpcGazeAverter _gazeAverter;
        private bool _geumtaeyangTargeted;
        private bool _popularGuyTargeted;
        private float _opponentClaimNormalized;

        public float CurrentHypnosis { get; private set; }

        public float MaximumHypnosis =>
            Mathf.Max(
                1f,
                _maximumHypnosis);

        public float HypnosisNormalized =>
            Mathf.Clamp01(
                CurrentHypnosis /
                MaximumHypnosis);

        public NpcOwner Owner { get; private set; } =
            NpcOwner.Neutral;

        /// <summary>현재 이 NPC를 소유한 경쟁자다. 플레이어·중립 소유일 때는 null이다.</summary>
        public OpponentControllerBase OpponentOwner { get; private set; }

        public bool IsHypnotized =>
            Owner !=
            NpcOwner.Neutral;

        public bool IsFollowing { get; private set; }

        public bool IsTargeted { get; private set; }

        public bool IsOpponentTargeted =>
            _geumtaeyangTargeted ||
            _popularGuyTargeted;

        public float OpponentClaimNormalized =>
            Mathf.Clamp01(
                _opponentClaimNormalized);

        public NpcOwner PrimaryThreatOwner =>
            _popularGuyTargeted
                ? NpcOwner.PopularGuy
                : _geumtaeyangTargeted
                    ? NpcOwner.Geumtaeyang
                    : NpcOwner.Neutral;

        public bool CanPlayerFocus =>
            OwnershipContestLogic.CanPlayerContest(
                Owner);

        /// <summary>등급·특성·플레이어 성장·난이도가 모두 반영된 최종 최면 상승 속도다.</summary>
        public float BuildPerSecond =>
            (Profile == null
                ? _buildPerSecond
                : Profile.HypnosisBuildPerSecond) *
            PlayerUpgradeMultipliers.HypnosisSpeed;

        /// <summary>시선 회피형 NPC가 지금 최면 연결을 끊고 있는지 여부다.</summary>
        public bool IsGazeBlocked
        {
            get
            {
                // 시선 회피형 컴포넌트는 특성이 결정된 뒤 Start에서 붙으므로
                // 한 번 null이었다고 확정하지 않고 필요할 때마다 다시 찾는다.
                if (_gazeAverter == null)
                {
                    _gazeAverter =
                        GetComponent<NpcGazeAverter>();
                }

                return _gazeAverter != null &&
                       _gazeAverter.IsBlocking;
            }
        }

        private FollowerManager _playerFollowers;

        private FollowerManager PlayerFollowers
        {
            get
            {
                if (_playerFollowers == null)
                {
                    _playerFollowers =
                        FindFirstObjectByType<
                            FollowerManager>();
                }

                return _playerFollowers;
            }
        }

        private NpcProfile Profile
        {
            get
            {
                if (_profile == null)
                {
                    _profile =
                        GetComponent<NpcProfile>();
                }

                return _profile;
            }
        }

        private void Awake()
        {
            _animator =
                GetComponent<
                    RuntimeCharacterSpriteAnimator>();
        }

        public void SetTargeted(
            bool targeted)
        {
            IsTargeted =
                targeted &&
                CanPlayerFocus;

            _animator?.SetHighlighted(
                IsTargeted);
        }

        public void SetOpponentTargeted(
            NpcOwner opponent,
            bool targeted)
        {
            switch (opponent)
            {
                case NpcOwner.Geumtaeyang:
                    _geumtaeyangTargeted =
                        targeted;
                    break;

                case NpcOwner.PopularGuy:
                    _popularGuyTargeted =
                        targeted;

                    if (!targeted &&
                        Owner ==
                        NpcOwner.Neutral)
                    {
                        ClearOpponentClaimProgress();
                    }
                    break;
            }
        }

        public void SetOpponentClaimProgress(
            float normalized)
        {
            _opponentClaimNormalized =
                Mathf.Clamp01(
                    normalized);
        }

        public void ClearOpponentClaimProgress()
        {
            _opponentClaimNormalized =
                0f;
        }

        public bool ApplyFocus(
            float deltaTime)
        {
            return ApplyPlayerFocus(
                deltaTime);
        }

        public bool ApplyPlayerFocus(
            float deltaTime)
        {
            return ApplyPlayerFocus(
                deltaTime,
                1f);
        }

        /// <summary>체인 최면은 단계가 깊어질수록 속도 배율이 낮아진다.</summary>
        public bool ApplyPlayerFocus(
            float deltaTime,
            float speedMultiplier)
        {
            if (!CanPlayerFocus)
            {
                return false;
            }

            if (Owner ==
                NpcOwner.Neutral)
            {
                // 시선 회피형은 고개를 돌리는 동안 최면 게이지가 오르지 않는다.
                if (IsGazeBlocked)
                {
                    return false;
                }

                CurrentHypnosis =
                    HypnosisTargetingLogic.BuildProgress(
                        CurrentHypnosis,
                        MaximumHypnosis,
                        BuildPerSecond *
                        Mathf.Max(
                            0f,
                            speedMultiplier),
                        deltaTime);

                return CurrentHypnosis >=
                       MaximumHypnosis;
            }

            CurrentHypnosis =
                OwnershipContestLogic.Drain(
                    CurrentHypnosis,
                    _playerReclaimPerSecond,
                    deltaTime);

            return OwnershipContestLogic.IsDepleted(
                CurrentHypnosis);
        }

        /// <summary>
        /// 경쟁자가 이 NPC의 지배 수치를 깎는다.
        /// 지배 수치가 0이 되면 true를 반환한다.
        /// </summary>
        public bool ApplyOpponentPressure(
            NpcOwner attacker,
            float drainPerSecond,
            float deltaTime)
        {
            if (!OwnershipContestLogic.CanContest(
                    attacker,
                    Owner))
            {
                return false;
            }

            if (Owner ==
                NpcOwner.Player)
            {
                if (!IsFollowing)
                {
                    return false;
                }

                // 차단 부적이 켜져 있는 동안에는 경쟁자가 지배 수치를 깎지 못한다.
                if (PlayerFollowers != null &&
                    PlayerFollowers.IsContestWarded)
                {
                    return false;
                }
            }

            CurrentHypnosis =
                OwnershipContestLogic.Drain(
                    CurrentHypnosis,
                    drainPerSecond,
                    deltaTime);

            return OwnershipContestLogic.IsDepleted(
                CurrentHypnosis);
        }

        /// <summary>
        /// 각성 지원형 NPC의 오라가 플레이어 동행 NPC의 지배 수치를 깎는다.
        /// 지배 수치가 0이 되면 true를 반환한다.
        /// </summary>
        public bool ApplyAwakeningDrain(
            float drainPerSecond,
            float deltaTime)
        {
            if (Owner !=
                    NpcOwner.Player ||
                !IsFollowing)
            {
                return false;
            }

            CurrentHypnosis =
                OwnershipContestLogic.Drain(
                    CurrentHypnosis,
                    drainPerSecond,
                    deltaTime);

            return OwnershipContestLogic.IsDepleted(
                CurrentHypnosis);
        }

        public void ClaimByPlayer()
        {
            ApplyClaim(
                NpcOwner.Player,
                null);
        }

        public void ClaimByOpponent(
            OpponentControllerBase opponent)
        {
            if (opponent == null)
            {
                return;
            }

            ApplyClaim(
                opponent.OwnerTag,
                opponent);
        }

        public void BeginFollowing()
        {
            if (Owner !=
                NpcOwner.Player)
            {
                return;
            }

            IsFollowing = true;
        }

        public void StopPlayerFollowingForTransfer()
        {
            IsFollowing = false;
        }

        public void ReleaseFromFollowing()
        {
            IsFollowing = false;

            ResetToNeutral();
        }

        public void ResetHypnosis()
        {
            IsFollowing = false;

            ResetToNeutral();
        }

        private void ApplyClaim(
            NpcOwner owner,
            OpponentControllerBase opponent)
        {
            Owner =
                owner;

            OpponentOwner =
                opponent;

            CurrentHypnosis =
                MaximumHypnosis;

            ClearOpponentClaimProgress();

            IsFollowing = false;

            ClearOpponentTargeting();

            SetTargeted(
                false);
        }

        private void ClearOpponentTargeting()
        {
            _geumtaeyangTargeted = false;
            _popularGuyTargeted = false;
        }

        private void ResetToNeutral()
        {
            Owner =
                NpcOwner.Neutral;

            OpponentOwner = null;

            CurrentHypnosis = 0f;

            ClearOpponentClaimProgress();

            ClearOpponentTargeting();

            SetTargeted(
                false);
        }
    }
}
