using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Player;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 해변가 밀물 · 썰물이다 (23일차, 부록 B.3 [1]).
    ///
    /// 30초마다 3초 예고 뒤 8초 동안 파도가 들어와 바닷가 쪽(화면 아래쪽) 줄이 막힌다.
    /// 밀물 동안 그 줄에 있던 플레이어와 동행자는 모래사장 쪽으로 밀려난다.
    /// 해변가는 1층짜리 장소라 1F에만 파도가 든다.
    /// </summary>
    public sealed class TideCycle : MonoBehaviour
    {
        private const int Floor = 0;

        /// <summary>물이 그려지는 가장 아래쪽이다. 화면 밖까지 덮는다.</summary>
        private const float WaterBottomLocalY = FloorSpace.WalkMinY - 2f;

        /// <summary>썰물 때도 발밑에 조금 보이는 물가 높이다.</summary>
        private const float LowWaterTopLocalY = FloorSpace.WalkMinY - 0.3f;

        private static TideCycle _current;

        private StageSessionController _stage;
        private Rigidbody2D _playerBody;
        private FollowerManager _followers;

        private float _elapsed;
        private TidePhase _phase;
        private SpriteRenderer _water;
        private float _waterTop = LowWaterTopLocalY;

        public static TideCycle Current =>
            _current;

        public TidePhase Phase =>
            _phase;

        public float SecondsUntilHigh =>
            TideLogic.SecondsUntilHigh(_elapsed);

        public float SecondsUntilLow =>
            TideLogic.SecondsUntilLow(_elapsed);

        public void Configure(
            StageSessionController stage,
            PlayerSideViewController player,
            FollowerManager followers)
        {
            _stage = stage;
            _playerBody = player == null ? null : player.GetComponent<Rigidbody2D>();
            _followers = followers;
            _current = this;

            _water =
                LocationProps.Box(
                    transform,
                    "TideWater",
                    Vector2.zero,
                    Vector2.one,
                    new Color(0.30f, 0.62f, 0.95f, 0.35f),
                    -58);

            ApplyWaterShape();
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        /// <summary>디버그 치트: 바로 밀물 예고를 시작한다.</summary>
        public void DebugStartWarning()
        {
            float cycleStart =
                Mathf.Floor(_elapsed / TideLogic.PeriodSeconds) *
                TideLogic.PeriodSeconds;

            float warningAt =
                TideLogic.PeriodSeconds -
                TideLogic.HighSeconds -
                TideLogic.WarningSeconds;

            _elapsed =
                cycleStart + warningAt;

            // 이번 주기의 예고를 이미 지났으면 다음 주기로 넘긴다.
            if (TideLogic.GetPhase(_elapsed) != TidePhase.Warning)
            {
                _elapsed += TideLogic.PeriodSeconds;
            }
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            _elapsed += Time.deltaTime;

            TidePhase phase =
                TideLogic.GetPhase(_elapsed);

            if (phase != _phase)
            {
                HandlePhaseChanged(phase);
            }

            _phase = phase;

            ApplyWaterShape();

            if (_phase == TidePhase.High)
            {
                PushOutOfWater();
            }
        }

        private void HandlePhaseChanged(
            TidePhase phase)
        {
            if (phase == TidePhase.Warning)
            {
                StageMoments.RaiseTideWarning();
            }
            else if (phase == TidePhase.High &&
                     _playerBody != null)
            {
                GameVfx.FloatText(
                    "밀물!",
                    _playerBody.position + new Vector2(0f, 2.4f),
                    new Color(0.45f, 0.75f, 1.00f),
                    UI.Framework.UiTheme.FontHeading);
            }
        }

        private void ApplyWaterShape()
        {
            if (_water == null)
            {
                return;
            }

            float targetTop =
                _phase == TidePhase.High
                    ? TideLogic.FloodTopLocalY
                    : LowWaterTopLocalY;

            _waterTop =
                Mathf.MoveTowards(
                    _waterTop,
                    targetTop,
                    3f * Time.deltaTime);

            float bottom = WaterBottomLocalY;
            float height = Mathf.Max(0.05f, _waterTop - bottom);

            Vector2 center =
                FloorSpace.ToWorld(
                    Floor,
                    new Vector2(0f, bottom + height * 0.5f));

            _water.transform.position =
                new Vector3(center.x, center.y, 0f);

            _water.transform.localScale =
                new Vector3(
                    FloorSpace.WalkMaxX - FloorSpace.WalkMinX + 6f,
                    height,
                    1f);

            // 예고 중에는 물빛이 깜빡인다.
            float alpha =
                _phase == TidePhase.Warning
                    ? 0.30f + 0.20f * Mathf.PingPong(Time.time * 4f, 1f)
                    : _phase == TidePhase.High
                        ? 0.55f
                        : 0.30f;

            Color color = _water.color;

            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                _water.color = color;
            }
        }

        private void PushOutOfWater()
        {
            PushOut(_playerBody);

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
                    PushOut(list[i].GetComponent<Rigidbody2D>());
                }
            }
        }

        private static void PushOut(
            Rigidbody2D body)
        {
            if (body == null)
            {
                return;
            }

            Vector2 position = body.position;

            if (FloorSpace.FloorAt(position.y) != Floor)
            {
                return;
            }

            Vector2 local =
                FloorSpace.ToLocal(Floor, position);

            if (!TideLogic.IsFlooded(TidePhase.High, local.y))
            {
                return;
            }

            // 물가 바로 위로 올린다. 순간이동처럼 보이지 않게 조금씩 민다.
            float targetY =
                FloorSpace.ToWorld(
                    Floor,
                    new Vector2(0f, TideLogic.FloodTopLocalY + 0.05f)).y;

            body.position =
                new Vector2(
                    position.x,
                    Mathf.MoveTowards(
                        position.y,
                        targetY,
                        8f * Time.deltaTime));
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
