using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 지하철 안내 방송실의 <b>승강장 변경 안내</b>다 (부록 C.3 [2]).
    ///
    /// 약 45초마다 3초 차임벨(예고) 뒤 방송이 나오고, 5초 동안 인파가 한쪽으로 흐른다.
    /// 그동안 동행자는 흐름 쪽으로 끌려가고 느려진다.
    /// 대처: 방송 전에 흐름과 같은 쪽으로 이동 계획, 파동으로 동행자 고정(3초).
    /// </summary>
    public sealed class PlatformChangeAbility : SpecialAbility
    {
        /// <summary>판 시작 직후에는 방송하지 않는다.</summary>
        private const float FirstDelaySeconds = 20f;

        private StageSessionController _stage;
        private Text _board;
        private int _nextDirection = 1;

        public override string DisplayName =>
            "승강장 변경 안내";

        public void Configure(
            StageSessionController stage)
        {
            _stage = stage;

            SetInitialCooldown(FirstDelaySeconds);

            _board =
                WorldLabel.Create(
                    transform,
                    "Board",
                    new Vector2(0f, 1.1f),
                    22,
                    new Color(1.00f, 0.80f, 0.30f),
                    520f);
        }

        protected override bool WantsToStart()
        {
            return _stage == null ||
                   _stage.IsRunning;
        }

        protected override void OnTelegraph()
        {
            _nextDirection =
                Random.value < 0.5f
                    ? -1
                    : 1;
        }

        protected override void Fire()
        {
            CrowdFlow.Begin(
                _nextDirection,
                CrowdFlowLogic.FlowSeconds);

            StageMoments.RaiseAbilityFired(
                transform.position,
                DisplayName);
        }

        private void LateUpdate()
        {
            if (!GameplayPause.IsPaused &&
                (_stage == null ||
                 _stage.IsRunning))
            {
                CrowdFlow.Tick(Time.deltaTime);
            }

            if (_board == null)
            {
                return;
            }

            string arrow =
                _nextDirection > 0
                    ? "→ → →"
                    : "← ← ←";

            string text =
                CrowdFlow.IsActive
                    ? $"승강장 변경!  인파 {(CrowdFlow.Direction > 0 ? "→ → →" : "← ← ←")}"
                    : Phase == AbilityPhase.Telegraph
                        ? $"♪ 안내 방송  {arrow}"
                        : "전광판";

            if (_board.text != text)
            {
                _board.text = text;
            }
        }
    }
}
