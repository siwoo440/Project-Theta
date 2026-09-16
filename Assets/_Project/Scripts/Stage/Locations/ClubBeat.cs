using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Impulse;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 루프탑 클럽 음악 박자다 (26일차, 부록 B.3 [7]).
    ///
    /// 템포에 따라 일정 간격으로 드롭이 떨어진다. 드롭 순간 모든 동행자 충동이 오르고,
    /// 드롭 직후 1.5초 동안 최면이 잘 걸리고 클럽 MD가 빨라진다.
    /// 3초마다 조명이 번쩍이고, 그 순간은 라이벌의 역최면 시선이 통하지 않는다.
    /// </summary>
    public sealed class ClubBeat : MonoBehaviour
    {
        private static ClubBeat _current;

        private readonly List<SpriteRenderer> _floorLights =
            new List<SpriteRenderer>();

        private StageSessionController _stage;
        private FollowerManager _followers;

        private float _elapsed;
        private float _sinceDrop;
        private bool _hasDropped;
        private int _tempo = ClubBeatLogic.MinimumTempo;

        public static ClubBeat Current =>
            _current;

        public int Tempo =>
            _tempo;

        public bool IsDropWindow =>
            ClubBeatLogic.IsDropWindow(_sinceDrop, _hasDropped);

        public bool IsFlash =>
            ClubBeatLogic.IsFlash(_elapsed);

        public float SecondsUntilDrop =>
            Mathf.Max(0f, ClubBeatLogic.GetDropInterval(_tempo) - _sinceDrop);

        /// <summary>드롭 창 동안의 최면 배율이다. 클럽이 아니면 1이다.</summary>
        public static float HypnosisMultiplier =>
            _current == null
                ? 1f
                : ClubBeatLogic.GetHypnosisMultiplier(
                    _current.IsDropWindow,
                    BalanceOverrides.StageOrDefault.DropHypnosisScale);

        public static float ContestMultiplier =>
            _current == null
                ? 1f
                : ClubBeatLogic.GetContestMultiplier(_current.IsDropWindow);

        public static bool FlashActive =>
            _current != null &&
            _current.IsFlash;

        public void Configure(
            StageSessionController stage,
            FollowerManager followers,
            int floorCount)
        {
            _stage = stage;
            _followers = followers;
            _current = this;

            for (int floor = 0;
                 floor < floorCount;
                 floor++)
            {
                float minX = floor == 0 ? -12f : ClubLayout.DanceFloorMinX;
                float maxX = floor == 0 ? 4f : ClubLayout.DanceFloorMaxX;

                _floorLights.Add(
                    LocationProps.Box(
                        transform,
                        "DanceLight",
                        FloorSpace.ToWorld(
                            floor,
                            new Vector2(
                                (minX + maxX) * 0.5f,
                                (FloorSpace.WalkMinY + FloorSpace.WalkMaxY) * 0.5f)),
                        new Vector2(maxX - minX, FloorSpace.WalkMaxY - FloorSpace.WalkMinY),
                        new Color(0.6f, 0.3f, 1f, 0.08f),
                        -57));
            }
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        public void TempoUp()
        {
            SetTempo(_tempo + 1);
        }

        public void TempoDown()
        {
            SetTempo(_tempo - 1);
        }

        private void SetTempo(
            int tempo)
        {
            int next = ClubBeatLogic.ClampTempo(tempo);

            if (next == _tempo)
            {
                return;
            }

            _tempo = next;

            StageMoments.RaiseTempoChanged(_tempo);
        }

        /// <summary>디버그 치트: 바로 드롭을 떨어뜨린다.</summary>
        public void DebugDrop()
        {
            Drop();
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            _elapsed += deltaTime;
            _sinceDrop += deltaTime;

            if (_sinceDrop >= ClubBeatLogic.GetDropInterval(_tempo))
            {
                Drop();
            }

            ApplyLights();
        }

        private void Drop()
        {
            _sinceDrop = 0f;
            _hasDropped = true;

            float impulse = ClubBeatLogic.GetDropImpulse(_tempo);

            if (_followers != null)
            {
                IReadOnlyList<FollowerController> list = _followers.Followers;

                for (int i = 0;
                     i < list.Count;
                     i++)
                {
                    if (list[i] == null)
                    {
                        continue;
                    }

                    ImpulseMeter meter = list[i].GetComponent<ImpulseMeter>();

                    if (meter != null)
                    {
                        meter.AddImpulse(impulse);
                    }
                }
            }

            StageMoments.RaiseClubDrop(_tempo);
        }

        private void ApplyLights()
        {
            Color color =
                IsDropWindow
                    ? GetTempoColor(_tempo)
                    : IsFlash
                        ? new Color(1f, 1f, 1f, 0.18f)
                        : new Color(0.6f, 0.3f, 1f, 0.08f);

            for (int i = 0;
                 i < _floorLights.Count;
                 i++)
            {
                if (_floorLights[i] != null &&
                    _floorLights[i].color != color)
                {
                    _floorLights[i].color = color;
                }
            }
        }

        public static Color GetTempoColor(
            int tempo)
        {
            switch (ClubBeatLogic.ClampTempo(tempo))
            {
                case 3:
                    return new Color(1.00f, 0.25f, 0.45f, 0.30f);

                case 2:
                    return new Color(1.00f, 0.55f, 0.20f, 0.28f);

                default:
                    return new Color(0.45f, 0.55f, 1.00f, 0.26f);
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
