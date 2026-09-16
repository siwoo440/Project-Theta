using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Impulse;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 길막 역할이다 (23일차, 부록 C.3 [4] 취객).
    ///
    /// 비틀거리며 아무 쪽으로나 걷는다. 동행자와 부딪치면 그 동행자의 충동이 오른다.
    /// 플레이어가 대시로 들이받으면 밀려나 잠깐 휘청인다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class BlockerRole : MonoBehaviour
    {
        /// <summary>배치된 자리에서 이만큼 안에서만 비틀거린다.</summary>
        private const float WanderHalfWidth = 4f;

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;
        private PlayerSideViewController _movement;

        private Vector2 _home;
        private float _wanderRemaining;
        private float _bumpCooldown;

        public void Configure(
            FollowerManager followers,
            Transform player)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;
            _movement = player == null ? null : player.GetComponent<PlayerSideViewController>();
            _home = transform.position;
            _wanderRemaining = Random.Range(0.5f, 2f);
        }

        private void Update()
        {
            if (_body == null ||
                !_body.CanAct)
            {
                return;
            }

            float deltaTime =
                Time.deltaTime;

            UpdateWander(deltaTime);
            UpdateBump(deltaTime);
            UpdateDashPush();
        }

        private void UpdateWander(
            float deltaTime)
        {
            _wanderRemaining -= deltaTime;

            if (_wanderRemaining > 0f)
            {
                return;
            }

            _wanderRemaining =
                Random.Range(2f, 3.5f);

            Vector2 target =
                _home +
                new Vector2(
                    Random.Range(-WanderHalfWidth, WanderHalfWidth),
                    Random.Range(-1.4f, 1.4f));

            _body.MoveToward(
                target,
                _wanderRemaining);
        }

        private void UpdateBump(
            float deltaTime)
        {
            if (_bumpCooldown > 0f)
            {
                _bumpCooldown -= deltaTime;

                return;
            }

            if (_followers == null)
            {
                return;
            }

            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            Vector2 self = transform.position;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                FollowerController follower = list[i];

                if (follower == null ||
                    !follower.isActiveAndEnabled)
                {
                    continue;
                }

                Vector2 position = follower.transform.position;

                if (!BlockerLogic.CanBump(
                        Vector2.Distance(self, position),
                        _bumpCooldown))
                {
                    continue;
                }

                ImpulseMeter impulse =
                    follower.GetComponent<ImpulseMeter>();

                if (impulse == null ||
                    !impulse.AddImpulse(BlockerLogic.BumpImpulse))
                {
                    continue;
                }

                _bumpCooldown = BlockerLogic.BumpCooldownSeconds;

                GameVfx.FloatText(
                    $"휘청! 충동 +{BlockerLogic.BumpImpulse:0}",
                    position + new Vector2(0f, 1.8f),
                    new Color(1.00f, 0.55f, 0.65f),
                    UI.Framework.UiTheme.FontSmall);

                return;
            }
        }

        private void UpdateDashPush()
        {
            if (_movement == null ||
                !_movement.IsDashing)
            {
                return;
            }

            Vector2 self = transform.position;
            Vector2 player = _player.position;

            // 대시 중에 스치면 밀린다. 밀리면 멍해져 이번 대시에 다시 밀리지 않는다.
            if (Vector2.Distance(self, player) > BlockerLogic.DashPushRadius)
            {
                return;
            }

            float direction =
                self.x >= player.x
                    ? 1f
                    : -1f;

            Vector3 position = transform.position;

            position.x =
                Mathf.Clamp(
                    position.x + direction * BlockerLogic.DashPushDistance,
                    FloorSpace.WalkMinX + 1f,
                    FloorSpace.WalkMaxX - 1f);

            transform.position = position;

            _body.Stun(
                BlockerLogic.DashStunSeconds);

            GameVfx.FloatText(
                "밀침!",
                (Vector2)position + new Vector2(0f, 1.8f),
                UI.Framework.UiTheme.Gold,
                UI.Framework.UiTheme.FontSmall);
        }
    }
}
