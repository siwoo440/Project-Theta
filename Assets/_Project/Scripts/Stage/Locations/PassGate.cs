using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 오피스 출입증 게이트다 (25일차, 부록 C.3 [6]).
    ///
    /// 그 층의 위층 계단을 잠근다. 출입증 NPC를 데리고 그 층에 오면 열리고, 한 번 열리면 계속 열린다.
    /// 잠긴 계단은 [F]를 한 번 더 누르면 비상계단으로 우회하되 구역 경계도가 +20 오른다
    /// (<see cref="FloorTransitionController"/>가 묻는다).
    /// </summary>
    public sealed class PassGate : MonoBehaviour
    {
        private static readonly List<PassGate> Active =
            new List<PassGate>();

        private Transform _player;
        private FollowerManager _followers;
        private SpriteRenderer _door;
        private SpriteRenderer _light;
        private Text _label;

        public int Floor { get; private set; }

        public bool IsOpen { get; private set; }

        public static int ActiveCount =>
            Active.Count;

        /// <summary>그 층의 위층 계단이 잠겼는지다. 게이트가 없는 층은 열려 있다.</summary>
        public static bool IsLocked(
            int sourceFloor)
        {
            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i].Floor == sourceFloor &&
                    !Active[i].IsOpen)
                {
                    return true;
                }
            }

            return false;
        }

        public static int CountOpen()
        {
            int open = 0;

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i].IsOpen)
                {
                    open++;
                }
            }

            return open;
        }

        /// <summary>디버그 치트: 모든 게이트를 연다.</summary>
        public static void DebugOpenAll()
        {
            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null)
                {
                    Active[i].Open();
                }
            }
        }

        public void Configure(
            int floor,
            Transform player,
            FollowerManager followers)
        {
            Floor = floor;
            _player = player;
            _followers = followers;

            float x = OfficeLayout.GateX;
            float top = FloorSpace.WalkMaxY;
            float bottom = FloorSpace.WalkMinY;

            Vector2 center =
                FloorSpace.ToWorld(floor, new Vector2(x, (top + bottom) * 0.5f));

            transform.position = new Vector3(center.x, center.y, 0f);

            _door =
                LocationProps.Box(
                    transform,
                    "GlassDoor",
                    center,
                    new Vector2(0.3f, top - bottom),
                    new Color(0.55f, 0.75f, 0.95f, 0.35f),
                    -42);

            _light =
                LocationProps.Blob(
                    transform,
                    "GateLight",
                    FloorSpace.ToWorld(floor, new Vector2(x, top + 0.7f)),
                    new Vector2(0.4f, 0.4f),
                    UiTheme.Danger,
                    -41);

            _label =
                WorldLabel.Create(
                    transform,
                    "GateLabel",
                    FloorSpace.ToWorld(floor, new Vector2(x, top + 1.3f)) - center,
                    20,
                    UiTheme.Danger);

            Refresh();
        }

        private void Update()
        {
            if (IsOpen ||
                GameplayPause.IsPaused ||
                _player == null)
            {
                return;
            }

            bool playerHere =
                FloorSpace.FloorAt(_player.position.y) == Floor;

            if (PassGateLogic.ShouldOpen(
                    false,
                    playerHere,
                    playerHere && HasPassHolder()))
            {
                Open();

                GameVfx.FloatText(
                    "출입증 확인 · 게이트 열림",
                    (Vector2)transform.position + new Vector2(0f, 2.5f),
                    UiTheme.Positive,
                    UiTheme.FontBody);

                GameAudio.Play(GameSfx.UiStamp);
            }
        }

        private bool HasPassHolder()
        {
            if (_followers == null)
            {
                return false;
            }

            IReadOnlyList<FollowerController> list =
                _followers.Followers;

            for (int i = 0;
                 i < list.Count;
                 i++)
            {
                NpcRoleMark mark = NpcRoleMark.Get(list[i]);

                if (mark != null &&
                    mark.Role == NpcRole.PassHolder)
                {
                    return true;
                }
            }

            return false;
        }

        private void Open()
        {
            IsOpen = true;

            Refresh();
        }

        private void Refresh()
        {
            _light.color = IsOpen ? UiTheme.Positive : UiTheme.Danger;
            _door.enabled = !IsOpen;
            _label.color = IsOpen ? UiTheme.Positive : UiTheme.Danger;
            _label.text = IsOpen ? "게이트 열림" : "출입증 게이트 · [출입증] 직원 필요";
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
