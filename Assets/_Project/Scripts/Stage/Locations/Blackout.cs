using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 오피스 정전이다 (25일차, 부록 C.3 [6]).
    ///
    /// 제한 시간의 35% · 70% 지점에 3초 예고 뒤 8초 동안 불이 꺼진다.
    ///   손전등을 든 야근 경비원을 뺀 모든 감시 시야가 사라진다
    ///   최면 사거리가 30% 줄어든다
    /// </summary>
    public sealed class Blackout : MonoBehaviour
    {
        private static Blackout _current;

        private StageSessionController _stage;
        private SpriteRenderer _shade;
        private float _warningRemaining;
        private float _darkRemaining;
        private int _nextIndex;

        public static Blackout Current =>
            _current;

        public static bool IsActive =>
            _current != null &&
            _current._darkRemaining > 0f;

        /// <summary>정전 중 최면 사거리 배율이다. 정전이 아니면 1이다.</summary>
        public static float RangeMultiplier =>
            IsActive
                ? BlackoutLogic.RangeMultiplier
                : 1f;

        public bool IsWarning =>
            _warningRemaining > 0f;

        public float DarkRemaining =>
            Mathf.Max(0f, _darkRemaining);

        public float WarningRemaining =>
            Mathf.Max(0f, _warningRemaining);

        public void Configure(
            StageSessionController stage,
            int floorCount)
        {
            _stage = stage;
            _current = this;

            // 건물 전체를 덮는 어두운 막이다. 캐릭터(10000대) 위, 머리 위 글자(24000) 아래에 그린다.
            float height = FloorSpace.FloorHeight * Mathf.Max(1, floorCount) + 20f;

            _shade =
                LocationProps.Box(
                    transform,
                    "BlackoutShade",
                    new Vector2(0f, height * 0.5f - 10f),
                    new Vector2(80f, height),
                    new Color(0.01f, 0.01f, 0.05f, 0f),
                    19000);
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        /// <summary>디버그 치트: 바로 정전 예고를 시작한다.</summary>
        public void DebugStart()
        {
            if (_darkRemaining <= 0f &&
                _warningRemaining <= 0f)
            {
                BeginWarning();
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

            float deltaTime = Time.deltaTime;

            if (_warningRemaining > 0f)
            {
                _warningRemaining -= deltaTime;

                if (_warningRemaining <= 0f)
                {
                    _darkRemaining = BlackoutLogic.DarkSeconds;

                    StageMoments.RaiseBlackoutStarted();
                }
            }
            else if (_darkRemaining > 0f)
            {
                _darkRemaining -= deltaTime;
            }
            else if (_stage != null &&
                     BlackoutLogic.ShouldStart(
                         _nextIndex,
                         _stage.ElapsedTime,
                         _stage.TimeLimitSeconds))
            {
                BeginWarning();
            }

            ApplyShade();
        }

        private void BeginWarning()
        {
            _nextIndex++;
            _warningRemaining = BlackoutLogic.WarningSeconds;

            if (_stage != null)
            {
                GameVfx.FloatText(
                    "전등이 깜빡인다…",
                    (Vector2)_stage.transform.position + new Vector2(0f, 2.4f),
                    new Color(0.85f, 0.85f, 0.60f),
                    UI.Framework.UiTheme.FontBody);
            }
        }

        private void ApplyShade()
        {
            float alpha =
                _darkRemaining > 0f
                    ? BlackoutLogic.ShadeAlpha
                    : _warningRemaining > 0f
                        ? (Mathf.PingPong(Time.time * 5f, 1f) > 0.6f ? 0.35f : 0f)
                        : 0f;

            Color color = _shade.color;

            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                _shade.color = color;
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
