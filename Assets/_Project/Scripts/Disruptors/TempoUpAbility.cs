using UnityEngine;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// DJ의 <b>템포 업</b>이다 (부록 C.3 [7]).
    ///
    /// 30초마다 2초 빌드업(예고) 뒤 템포를 한 단계 올린다 (최대 3단계).
    /// 템포가 오를수록 드롭 간격이 짧아지고 드롭 충동이 커진다.
    /// 대처: DJ에게 파동을 맞히면 템포가 한 단계 내려간다 (기획의 DJ 최면 대신).
    /// </summary>
    public sealed class TempoUpAbility : SpecialAbility
    {
        private const float FirstDelaySeconds = 30f;

        private bool _wasStunned;

        public override string DisplayName =>
            "템포 업";

        public void Configure()
        {
            SetInitialCooldown(FirstDelaySeconds);
        }

        protected override bool WantsToStart()
        {
            ClubBeat beat = ClubBeat.Current;

            return beat != null &&
                   beat.Tempo < ClubBeatLogic.MaximumTempo;
        }

        protected override void Fire()
        {
            ClubBeat beat = ClubBeat.Current;

            if (beat == null)
            {
                return;
            }

            beat.TempoUp();

            StageMoments.RaiseAbilityFired(
                transform.position,
                $"템포 {beat.Tempo}단계");
        }

        private void LateUpdate()
        {
            if (Body == null)
            {
                return;
            }

            bool stunned = Body.IsStunned;

            if (stunned &&
                !_wasStunned &&
                ClubBeat.Current != null &&
                ClubBeat.Current.Tempo > ClubBeatLogic.MinimumTempo)
            {
                ClubBeat.Current.TempoDown();

                GameVfx.FloatText(
                    $"템포 다운 · {ClubBeat.Current.Tempo}단계",
                    (Vector2)transform.position + new Vector2(0f, 2.6f),
                    UiTheme.Positive,
                    UiTheme.FontBody);
            }

            _wasStunned = stunned;
        }
    }
}
