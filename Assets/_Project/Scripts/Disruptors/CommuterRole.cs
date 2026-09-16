using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Player;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 휴대폰만 보는 행인이다 (24일차, 부록 C.3 [2]). 길막 역할의 "직진" 형태다.
    ///
    /// 열차에서 내려 한 방향으로 느리게 걷고, 복도 끝에 닿으면 사라진다.
    /// 최면 대상이 아니다. 동행자와 부딪치면 동행자가 옆으로 밀려 대형이 흐트러진다.
    /// 대처: 줄을 바꿔 피하기, 대시로 밀치기.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class CommuterRole : MonoBehaviour
    {
        private const float BumpDistance = 0.65f;
        private const float BumpPush = 0.8f;
        private const float BumpCooldownSeconds = 0.8f;
        private const float DashPushRadius = 1.0f;
        private const float DashPushDistance = 1.2f;

        private DisruptorBase _body;
        private FollowerManager _followers;
        private Transform _player;
        private PlayerSideViewController _movement;
        private int _direction = 1;
        private float _bumpCooldown;

        public void Configure(
            FollowerManager followers,
            Transform player,
            int direction)
        {
            _body = GetComponent<DisruptorBase>();
            _followers = followers;
            _player = player;
            _movement = player == null ? null : player.GetComponent<PlayerSideViewController>();
            _direction = direction >= 0 ? 1 : -1;

            _body.FaceToward(
                transform.position.x + _direction);
        }

        private void Update()
        {
            if (_body == null ||
                _body.Profile == null ||
                !_body.CanAct)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            Vector3 position = transform.position;

            position.x +=
                _direction *
                _body.Profile.MoveSpeed *
                deltaTime;

            transform.position = position;

            if (position.x > FloorSpace.WalkMaxX - 0.5f ||
                position.x < FloorSpace.WalkMinX + 0.5f)
            {
                Destroy(gameObject);

                return;
            }

            if (_bumpCooldown > 0f)
            {
                _bumpCooldown -= deltaTime;
            }
            else
            {
                BumpFollowers();
            }

            PushedByDash();
        }

        private void BumpFollowers()
        {
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

                Rigidbody2D body = follower.GetComponent<Rigidbody2D>();

                if (body == null ||
                    Vector2.Distance(self, body.position) > BumpDistance)
                {
                    continue;
                }

                // 옆 줄로 밀어낸다. 층 밖으로는 밀지 않는다.
                float side =
                    body.position.y >= self.y
                        ? 1f
                        : -1f;

                body.position =
                    new Vector2(
                        body.position.x,
                        FloorSpace.ClampYNear(
                            body.position.y,
                            body.position.y + side * BumpPush,
                            0.2f,
                            0.2f));

                _bumpCooldown = BumpCooldownSeconds;

                GameVfx.FloatText(
                    "툭",
                    body.position + new Vector2(0f, 1.6f),
                    new Color(0.75f, 0.78f, 0.85f),
                    UI.Framework.UiTheme.FontSmall);

                return;
            }
        }

        private void PushedByDash()
        {
            if (_movement == null ||
                !_movement.IsDashing)
            {
                return;
            }

            Vector2 self = transform.position;
            Vector2 player = _player.position;

            if (Vector2.Distance(self, player) > DashPushRadius)
            {
                return;
            }

            float side =
                self.y >= player.y
                    ? 1f
                    : -1f;

            transform.position =
                new Vector3(
                    self.x,
                    FloorSpace.ClampYNear(
                        self.y,
                        self.y + side * DashPushDistance,
                        0.2f,
                        0.2f),
                    transform.position.z);

            // 잠깐 휘청여 같은 대시에 다시 밀리지 않는다.
            _body.Stun(0.6f);
        }
    }
}
