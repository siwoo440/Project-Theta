using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.NPC;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 소매치기의 <b>슬쩍하기</b>다 (부록 C.3 [4]). 도둑 역할이다.
    ///
    /// 평소에는 이름표 없이 손님처럼 걷는다. 동행자를 데리고 가까이 오면 등 뒤로 다가가(예고 1초)
    /// 아직 회수하지 않은 정기의 30%를 훔쳐 달아난다. 지금 데리고 있는 동행자 모두에게 도난 표식이 붙고,
    /// 표식이 붙은 동행자는 회수해도 정기가 70%만 들어온다.
    ///
    /// 20초 안에 소매치기에게 닿으면 표식이 모두 풀리고 보너스 +50. 못 잡으면 인파 속으로 사라진다.
    /// 구역당 한 번만 훔친다. 예고 중에 대시로 거리를 벌리면 실패하고 잠시 뒤 다시 노린다.
    /// </summary>
    public sealed class PickpocketAbility : SpecialAbility
    {
        private const float FleeRepathSeconds = 0.5f;
        private const float FleeStep = 5f;

        /// <summary>대시로 들이받으면 조금 더 멀리서도 잡힌다.</summary>
        private const float DashCatchBonus = 0.5f;

        private static PickpocketAbility _fleeing;

        private Transform _player;
        private PlayerSideViewController _movement;
        private FollowerManager _followers;
        private StageSessionController _stage;

        private bool _stole;
        private bool _resolved;
        private float _fleeRemaining;
        private float _repathRemaining;

        /// <summary>지금 도주 중인 소매치기다. HUD가 남은 시간을 읽는다.</summary>
        public static PickpocketAbility Fleeing =>
            _fleeing;

        public override string DisplayName =>
            "슬쩍하기";

        public bool IsFleeing =>
            _stole &&
            !_resolved;

        public float FleeRemaining =>
            Mathf.Max(0f, _fleeRemaining);

        public int StolenAmount { get; private set; }

        public void Configure(
            Transform player,
            FollowerManager followers,
            StageSessionController stage)
        {
            _player = player;
            _movement = player == null ? null : player.GetComponent<PlayerSideViewController>();
            _followers = followers;
            _stage = stage;
        }

        private void OnDestroy()
        {
            if (_fleeing == this)
            {
                _fleeing = null;
            }
        }

        private float PlayerDistance =>
            _player == null
                ? float.MaxValue
                : Vector2.Distance(
                    transform.position,
                    _player.position);

        private bool PlayerOnMyFloor =>
            _player != null &&
            FloorSpace.FloorAt(_player.position.y) == Body.Floor;

        /// <summary>지금 데리고 있는 동행자의 정기 합이다. 이미 도난 표식이 붙은 동행자는 뺀다.</summary>
        private int UnrecoveredEssence
        {
            get
            {
                if (_followers == null)
                {
                    return 0;
                }

                IReadOnlyList<FollowerController> list =
                    _followers.Followers;

                int total = 0;

                for (int i = 0;
                     i < list.Count;
                     i++)
                {
                    FollowerController follower = list[i];

                    if (follower == null ||
                        PickpocketMark.GetEssenceMultiplier(follower) < 1f)
                    {
                        continue;
                    }

                    NpcProfile profile =
                        follower.GetComponent<NpcProfile>();

                    total +=
                        profile == null
                            ? NpcGradeTable.ReferenceEssenceValue
                            : profile.EssenceValue;
                }

                return total;
            }
        }

        protected override bool WantsToStart()
        {
            return PlayerOnMyFloor &&
                   PickpocketLogic.WantsToSteal(
                       UnrecoveredEssence,
                       PlayerDistance,
                       _stole);
        }

        protected override void OnTelegraph()
        {
            if (_player == null)
            {
                return;
            }

            // 플레이어 등 뒤로 붙는다. 등 뒤에 작은 손이 떠서 알린다.
            int facing =
                _movement == null
                    ? 1
                    : _movement.FacingDirection;

            Vector2 behind =
                (Vector2)_player.position +
                new Vector2(-facing * 0.8f, 0f);

            Body.MoveToward(
                behind,
                TelegraphSeconds);

            GameVfx.FloatText(
                "✋ 등 뒤!",
                behind + new Vector2(0f, 1.6f),
                new Color(1.00f, 0.75f, 0.35f),
                UiTheme.FontBody,
                TelegraphSeconds);
        }

        protected override void Fire()
        {
            if (_stole)
            {
                return;
            }

            Vector2 self = transform.position;

            if (!PlayerOnMyFloor ||
                PlayerDistance > PickpocketLogic.StealDistance)
            {
                GameVfx.FloatText(
                    "쳇, 놓쳤다",
                    self + new Vector2(0f, 2.4f),
                    UiTheme.TextMuted,
                    UiTheme.FontSmall);

                return;
            }

            int unrecovered =
                UnrecoveredEssence;

            StolenAmount =
                PickpocketLogic.GetStolenAmount(
                    unrecovered,
                    PickpocketLogic.StealRatio);

            if (StolenAmount <= 0)
            {
                return;
            }

            MarkFollowers();

            _stole = true;
            _fleeRemaining = PickpocketLogic.EscapeSeconds;
            _repathRemaining = 0f;
            _fleeing = this;

            StageMoments.RaisePickpocketStole(
                self,
                StolenAmount);
        }

        private void MarkFollowers()
        {
            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                if (list[i] != null)
                {
                    PickpocketMark.Apply(list[i]);
                }
            }
        }

        private void LateUpdate()
        {
            if (!IsFleeing ||
                GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            _fleeRemaining -= Time.deltaTime;

            float catchDistance =
                PickpocketLogic.CatchDistance +
                (_movement != null && _movement.IsDashing
                    ? DashCatchBonus
                    : 0f);

            if (PlayerOnMyFloor &&
                PlayerDistance <= catchDistance)
            {
                Resolve(true);

                return;
            }

            if (_fleeRemaining <= 0f)
            {
                Resolve(false);

                return;
            }

            UpdateFlee();
        }

        /// <summary>플레이어 반대쪽으로 달아난다. 벽에 몰리면 플레이어 옆을 스쳐 반대편으로 빠진다.</summary>
        private void UpdateFlee()
        {
            if (!Body.CanAct ||
                _player == null)
            {
                return;
            }

            _repathRemaining -= Time.deltaTime;

            if (_repathRemaining > 0f)
            {
                return;
            }

            _repathRemaining = FleeRepathSeconds;

            Vector2 self = transform.position;
            Vector2 player = _player.position;

            float away =
                self.x >= player.x
                    ? 1f
                    : -1f;

            float targetX =
                self.x + away * FleeStep;

            if (targetX > FloorSpace.WalkMaxX - 1.5f ||
                targetX < FloorSpace.WalkMinX + 1.5f)
            {
                targetX = self.x - away * FleeStep * 2f;
            }

            // 플레이어와 다른 줄로 비껴 달린다.
            float targetY =
                self.y + (self.y >= player.y ? 1f : -1f);

            Body.MoveToward(
                new Vector2(targetX, targetY),
                FleeRepathSeconds + 0.2f);
        }

        private void Resolve(
            bool caught)
        {
            _resolved = true;
            _fleeRemaining = 0f;

            if (_fleeing == this)
            {
                _fleeing = null;
            }

            Vector2 self = transform.position;

            if (caught)
            {
                PickpocketMark.ClearAll();

                if (_stage != null)
                {
                    _stage.AddEssence(
                        PickpocketLogic.CatchBonus);
                }
            }

            StageMoments.RaisePickpocketResolved(
                self,
                caught);

            // 잡히든 달아나든 이 구역에서는 퇴장한다.
            gameObject.SetActive(false);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _fleeing = null;
        }
    }
}
