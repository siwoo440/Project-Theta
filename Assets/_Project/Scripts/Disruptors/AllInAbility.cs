using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 수영장 코치의 <b>전원 입수</b>다 (부록 C.3 [3]).
    ///
    /// 수영장에서 최면이 진행 중인 NPC가 있으면 1.2초 예고 뒤 "전원 입수!"를 외친다.
    /// 수영장 안 모든 중립 NPC의 최면 게이지가 0이 되고, 5초 동안 물속이라 최면이 걸리지 않는다.
    /// 대처: 코치가 확성기를 들기 전에 최면 완료, 파동으로 코치를 멍하게 하기.
    /// </summary>
    public sealed class AllInAbility : SpecialAbility
    {
        private const float StartThreshold = 0.1f;

        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        public override string DisplayName =>
            "전원 입수";

        protected override bool WantsToStart()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                if (IsPoolNeutral(_targetBuffer[i]) &&
                    _targetBuffer[i].HypnosisNormalized >= StartThreshold)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Fire()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            int dived = 0;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (!IsPoolNeutral(target))
                {
                    continue;
                }

                target.KnockBackNeutralHypnosis(1f);

                HypnosisBlock.Apply(
                    target,
                    CityAbilityValues.DiveSeconds);

                dived++;
            }

            StageMoments.RaiseAbilityFired(
                transform.position,
                DisplayName);

            if (dived > 0)
            {
                GameVfx.Ripple(
                    (Vector2)transform.position + new Vector2(0f, 0.5f),
                    new Color(0.45f, 0.75f, 1.00f),
                    6f,
                    0.6f);
            }
        }

        private bool IsPoolNeutral(
            HypnosisTarget target)
        {
            if (target == null ||
                !target.isActiveAndEnabled ||
                target.Owner != NpcOwner.Neutral)
            {
                return false;
            }

            Vector2 position = target.transform.position;

            return FloorSpace.FloorAt(position.y) == Body.Floor &&
                   GymZone.GetZone(position) == GymZoneKind.Pool;
        }
    }
}
