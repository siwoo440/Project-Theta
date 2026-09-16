using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Player;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 기업 연수원의 자습실 구역이다 (22일차, 부록 B.3 [0]).
    ///
    /// 층마다 복도 한 칸을 자습실로 둔다. 안에서 대시하면 조용한 곳에서 뛴 소리로 구역 경계도가 오른다.
    /// 감시자가 보고 있지 않아도 오른다. 대신 걸어서 지나가면 아무 일도 없다.
    /// </summary>
    public sealed class QuietRoomZone : MonoBehaviour
    {
        /// <summary>대시 한 번에 오르는 경계도다 (부록 C.3 [0]).</summary>
        public const float DashAlertRise = 10f;

        public const float Width = 4.4f;

        private Transform _player;
        private PlayerSideViewController _movement;
        private bool _wasDashing;
        private float _minX;
        private float _maxX;
        private int _floor;

        public void Configure(
            PlayerSideViewController player,
            int floor,
            float centerX)
        {
            _player = player == null ? null : player.transform;
            _movement = player;
            _floor = floor;
            _minX = centerX - Width * 0.5f;
            _maxX = centerX + Width * 0.5f;

            BuildVisual(
                centerX);
        }

        private void BuildVisual(
            float centerX)
        {
            Vector2 floorOrigin =
                FloorSpace.ToWorld(
                    _floor,
                    Vector2.zero);

            // 바닥에 옅은 파란 사각형과 "자습실" 글자를 깐다.
            GameObject area =
                new GameObject(
                    "QuietRoomArea");

            area.transform.SetParent(
                transform,
                false);

            area.transform.position =
                new Vector3(
                    centerX,
                    floorOrigin.y +
                    (FloorSpace.WalkMinY + FloorSpace.WalkMaxY) * 0.5f,
                    0f);

            area.transform.localScale =
                new Vector3(
                    Width,
                    FloorSpace.WalkMaxY - FloorSpace.WalkMinY,
                    1f);

            SpriteRenderer renderer =
                area.AddComponent<SpriteRenderer>();

            renderer.sprite =
                VfxLibrary.Get(
                    VfxSprite.SoftCircle);

            renderer.drawMode = SpriteDrawMode.Simple;
            renderer.color = new Color(0.45f, 0.65f, 1.00f, 0.16f);
            renderer.sortingOrder = -60;

            UnityEngine.UI.Text label =
                WorldLabel.Create(
                    transform,
                    "QuietRoomLabel",
                    new Vector2(
                        centerX,
                        floorOrigin.y + FloorSpace.WalkMaxY + 0.35f),
                    24,
                    new Color(0.65f, 0.80f, 1.00f, 0.9f));

            label.text = "자습실 · 뛰지 마세요";
        }

        private void Update()
        {
            if (_player == null ||
                _movement == null ||
                GameplayPause.IsPaused)
            {
                return;
            }

            bool dashing =
                _movement.IsDashing;

            bool started =
                dashing &&
                !_wasDashing;

            _wasDashing = dashing;

            if (!started ||
                ZoneAlert.Current == null)
            {
                return;
            }

            Vector2 position =
                _player.position;

            if (FloorSpace.FloorAt(position.y) != _floor ||
                position.x < _minX ||
                position.x > _maxX)
            {
                return;
            }

            ZoneAlert.Current.Add(
                DashAlertRise,
                position);

            GameVfx.FloatText(
                "쉿!",
                position + new Vector2(0f, 1.6f),
                new Color(0.65f, 0.80f, 1.00f),
                28);
        }
    }
}
