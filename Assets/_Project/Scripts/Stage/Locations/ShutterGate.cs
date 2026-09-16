using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 쇼핑몰 매장 셔터다 (25일차, 부록 C.3 [5] 보안팀장 셔터 봉쇄).
    /// 닫히면 복도를 세로로 가로막아 플레이어와 동행자가 지나가지 못한다. 경쟁자 · NPC는 막지 않는다.
    /// </summary>
    public sealed class ShutterGate : MonoBehaviour
    {
        private static readonly List<ShutterGate> Active =
            new List<ShutterGate>();

        private Rigidbody2D _player;
        private FollowerManager _followers;
        private SpriteRenderer _shutter;
        private SpriteRenderer _lamp;

        private float _closedRemaining;
        private float _warningRemaining;

        public int Floor { get; private set; }

        public float X { get; private set; }

        public bool IsClosed =>
            _closedRemaining > 0f;

        public float ClosedRemaining =>
            Mathf.Max(0f, _closedRemaining);

        public void Configure(
            int floor,
            float x,
            Rigidbody2D player,
            FollowerManager followers)
        {
            Floor = floor;
            X = x;
            _player = player;
            _followers = followers;

            float top = FloorSpace.WalkMaxY;
            float bottom = FloorSpace.WalkMinY;

            Vector2 center =
                FloorSpace.ToWorld(
                    floor,
                    new Vector2(x, (top + bottom) * 0.5f));

            transform.position =
                new Vector3(center.x, center.y, 0f);

            _shutter =
                LocationProps.Box(
                    transform,
                    "Shutter",
                    center,
                    new Vector2(ShutterLogic.BlockHalfWidth * 2f, top - bottom + 0.6f),
                    new Color(0.55f, 0.58f, 0.62f, 0.9f),
                    15000);

            _shutter.enabled = false;

            // 셔터 틀과 경광등은 항상 보인다. 어디가 닫힐 수 있는지 미리 알게 한다.
            LocationProps.Box(
                transform,
                "ShutterFrame",
                FloorSpace.ToWorld(floor, new Vector2(x, top + 0.5f)),
                new Vector2(1.2f, 0.25f),
                new Color(0.35f, 0.36f, 0.40f),
                -44);

            _lamp =
                LocationProps.Blob(
                    transform,
                    "ShutterLamp",
                    FloorSpace.ToWorld(floor, new Vector2(x, top + 0.85f)),
                    new Vector2(0.35f, 0.35f),
                    new Color(0.4f, 0.1f, 0.1f),
                    -43);
        }

        public static int CopyOnFloor(
            int floor,
            List<ShutterGate> buffer)
        {
            buffer.Clear();

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i].Floor == floor)
                {
                    buffer.Add(Active[i]);
                }
            }

            return buffer.Count;
        }

        public static bool AnyClosed
        {
            get
            {
                for (int i = 0;
                     i < Active.Count;
                     i++)
                {
                    if (Active[i] != null &&
                        Active[i].IsClosed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public static float LongestClosedRemaining
        {
            get
            {
                float longest = 0f;

                for (int i = 0;
                     i < Active.Count;
                     i++)
                {
                    if (Active[i] != null)
                    {
                        longest = Mathf.Max(longest, Active[i].ClosedRemaining);
                    }
                }

                return longest;
            }
        }

        /// <summary>경광등을 깜빡이며 곧 닫힌다고 알린다.</summary>
        public void Warn(
            float seconds)
        {
            _warningRemaining = Mathf.Max(_warningRemaining, seconds);
        }

        public void Close(
            float seconds)
        {
            _warningRemaining = 0f;
            _closedRemaining = Mathf.Max(_closedRemaining, seconds);
        }

        private void Update()
        {
            if (GameplayPause.IsPaused)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            if (_warningRemaining > 0f)
            {
                _warningRemaining -= deltaTime;
            }

            if (_closedRemaining > 0f)
            {
                _closedRemaining -= deltaTime;
            }

            _shutter.enabled = IsClosed;

            _lamp.color =
                IsClosed
                    ? new Color(1f, 0.25f, 0.2f)
                    : _warningRemaining > 0f
                        ? Color.Lerp(new Color(0.4f, 0.1f, 0.1f), new Color(1f, 0.3f, 0.2f), Mathf.PingPong(Time.time * 6f, 1f))
                        : new Color(0.4f, 0.1f, 0.1f);
        }

        private void FixedUpdate()
        {
            if (!IsClosed)
            {
                return;
            }

            Block(_player);

            if (_followers == null)
            {
                return;
            }

            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                if (list[i] != null)
                {
                    Block(list[i].GetComponent<Rigidbody2D>());
                }
            }
        }

        private void Block(
            Rigidbody2D body)
        {
            if (body == null ||
                FloorSpace.FloorAt(body.position.y) != Floor)
            {
                return;
            }

            float x =
                ShutterLogic.PushOut(
                    X,
                    body.position.x);

            if (!Mathf.Approximately(x, body.position.x))
            {
                body.position = new Vector2(x, body.position.y);
            }
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

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active.Clear();
        }
    }
}
