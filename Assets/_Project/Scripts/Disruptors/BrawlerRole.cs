using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 힘 대결 역할이다 (24일차, 부록 C.3 [3] 헬스 고인물).
    ///
    /// 동행자를 데린 플레이어에게 느리게 다가와 "으랏차!" 자세(0.8초)를 잡고 밀어붙인다.
    ///   자세 중에 대시로 들이받으면 받아치기 — 고인물이 30초 퇴장
    ///   자세가 끝날 때 곁에 있으면 밀려나 1.5초 휘청
    ///   거리를 벌리면 헛돎
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class BrawlerRole : MonoBehaviour
    {
        private const float ChaseRange = 8f;
        private const float RepathSeconds = 0.4f;

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;
        private PlayerSideViewController _movement;
        private Rigidbody2D _playerBody;

        private float _windupRemaining;
        private float _cooldownRemaining;
        private float _repathRemaining;

        public bool IsWindingUp =>
            _windupRemaining > 0f;

        public void Configure(
            FollowerManager followers,
            Transform player)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;

            if (player != null)
            {
                _movement = player.GetComponent<PlayerSideViewController>();
                _playerBody = player.GetComponent<Rigidbody2D>();
            }
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
                if (_body.IsStunned)
                {
                    _windupRemaining = 0f;
                }

                return;
            }

            float deltaTime = Time.deltaTime;

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            Vector2 self = transform.position;
            Vector2 player = _player.position;

            bool sameFloor =
                FloorSpace.FloorAt(player.y) == _body.Floor;

            float distance =
                Vector2.Distance(self, player);

            if (IsWindingUp)
            {
                UpdateWindup(distance, deltaTime);

                return;
            }

            if (!sameFloor ||
                _followers == null ||
                _followers.Count == 0 ||
                distance > ChaseRange)
            {
                return;
            }

            if (distance <= BrawlLogic.EngageDistance &&
                _cooldownRemaining <= 0f)
            {
                _windupRemaining = BrawlLogic.WindupSeconds;

                _body.FaceToward(player.x);

                return;
            }

            _repathRemaining -= deltaTime;

            if (_repathRemaining > 0f)
            {
                return;
            }

            _repathRemaining = RepathSeconds;

            _body.MoveToward(
                player,
                RepathSeconds + 0.2f);
        }

        private void UpdateWindup(
            float distance,
            float deltaTime)
        {
            bool dashed =
                _movement != null &&
                _movement.IsDashing;

            // 자세 중 대시 접촉은 즉시 받아치기다.
            if (dashed &&
                BrawlLogic.Resolve(distance, true) == BrawlLogic.Outcome.Countered)
            {
                Finish(BrawlLogic.Outcome.Countered);

                return;
            }

            _windupRemaining -= deltaTime;

            if (_windupRemaining > 0f)
            {
                return;
            }

            Finish(
                BrawlLogic.Resolve(
                    distance,
                    false));
        }

        private void Finish(
            BrawlLogic.Outcome outcome)
        {
            _windupRemaining = 0f;
            _cooldownRemaining = BrawlLogic.CooldownSeconds;

            Vector2 self = transform.position;

            switch (outcome)
            {
                case BrawlLogic.Outcome.Countered:
                    _body.Stun(BrawlLogic.CounterStunSeconds);

                    GameVfx.FloatText(
                        "받아치기! 고인물 퇴장",
                        self + new Vector2(0f, 2.4f),
                        UiTheme.Gold,
                        UiTheme.FontHeading);

                    GameAudio.Play(GameSfx.DuelWin);
                    break;

                case BrawlLogic.Outcome.Hit:
                    HitPlayer(self);
                    break;

                default:
                    GameVfx.FloatText(
                        "헛돎",
                        self + new Vector2(0f, 2.4f),
                        UiTheme.TextMuted,
                        UiTheme.FontSmall);
                    break;
            }
        }

        private void HitPlayer(
            Vector2 self)
        {
            if (_movement != null)
            {
                _movement.ApplyStagger(
                    BrawlLogic.StaggerSeconds);
            }

            if (_playerBody != null)
            {
                float direction =
                    _playerBody.position.x >= self.x
                        ? 1f
                        : -1f;

                _playerBody.position =
                    new Vector2(
                        Mathf.Clamp(
                            _playerBody.position.x + direction * BrawlLogic.KnockbackDistance,
                            FloorSpace.WalkMinX,
                            FloorSpace.WalkMaxX),
                        _playerBody.position.y);
            }

            GameVfx.FloatText(
                "밀려났다!",
                (Vector2)_player.position + new Vector2(0f, 2f),
                UiTheme.Danger,
                UiTheme.FontBody);

            GameVfx.Shake(0.12f, 0.2f);
        }
    }
}
