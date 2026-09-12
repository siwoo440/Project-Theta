using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// NPC 한 명의 등급과 특성을 보관하고, 다른 시스템이 참조할 최종 수치를 계산한다.
    /// 최면·충동·정기 시스템은 이 컴포넌트만 보고 개체 차이를 반영한다.
    /// </summary>
    public sealed class NpcProfile : MonoBehaviour
    {
        [SerializeField] private NpcGrade _grade = NpcGrade.Common;

        private NpcTrait[] _traits =
            { NpcTrait.Plain };

        private bool _behavioursAttached;

        public NpcGrade Grade =>
            _grade;

        public NpcGradeProfile GradeProfile =>
            NpcGradeTable.Get(
                _grade);

        public string GradeDisplayName =>
            GradeProfile.DisplayName;

        public Color GradeMarkerColor =>
            GradeProfile.MarkerColor;

        public NpcTrait[] Traits =>
            _traits;

        /// <summary>특별한 저항 특성이 하나도 없는 평범한 NPC인지 여부다.</summary>
        public bool IsPlain =>
            _traits.Length == 1 &&
            _traits[0] ==
            NpcTrait.Plain;

        /// <summary>중립 상태에서 플레이어 최면 게이지가 초당 오르는 최종 양이다.</summary>
        public float HypnosisBuildPerSecond =>
            GradeProfile.HypnosisBuildPerSecond *
            NpcTraitTable.AggregateHypnosisSpeedMultiplier(
                _traits);

        /// <summary>회수 시 확정되는 최종 정기 가치다.</summary>
        public int EssenceValue =>
            Mathf.Max(
                1,
                Mathf.RoundToInt(
                    GradeProfile.EssenceValue *
                    NpcTraitTable.AggregateEssenceMultiplier(
                        _traits)));

        /// <summary>일반 등급을 1.0으로 본 최종 정기 가치 배율이다.</summary>
        public float EssenceMultiplier =>
            EssenceValue /
            (float)NpcGradeTable.ReferenceEssenceValue;

        /// <summary>동행 중 충동 상승 속도 배율이다.</summary>
        public float ImpulseBuildMultiplier =>
            GradeProfile.ImpulseBuildMultiplier;

        public bool HasTrait(
            NpcTrait trait)
        {
            return NpcTraitTable.Contains(
                _traits,
                trait);
        }

        /// <summary>특성 목록을 "고저항형 · 도주형" 형태의 한 줄로 만든다.</summary>
        public string GetTraitSummary()
        {
            if (_traits == null ||
                _traits.Length == 0)
            {
                return NpcTraitTable.Get(
                    NpcTrait.Plain).DisplayName;
            }

            string summary =
                string.Empty;

            for (int i = 0;
                 i < _traits.Length;
                 i++)
            {
                if (i > 0)
                {
                    summary +=
                        " · ";
                }

                summary +=
                    NpcTraitTable.Get(
                        _traits[i]).DisplayName;
            }

            return summary;
        }

        /// <summary>런 시작 시 등급을 지정하고 보조 특성을 무작위로 결정한다.</summary>
        public void Configure(
            NpcGrade grade)
        {
            Configure(
                grade,
                Random.value);
        }

        public void Configure(
            NpcGrade grade,
            float randomValue)
        {
            _grade =
                grade;

            _traits =
                NpcTraitAssignmentLogic.Resolve(
                    grade,
                    randomValue);
        }

        private void Start()
        {
            AttachTraitBehaviours();
        }

        /// <summary>특성에 대응하는 동작 컴포넌트를 붙인다.</summary>
        private void AttachTraitBehaviours()
        {
            if (_behavioursAttached)
            {
                return;
            }

            _behavioursAttached =
                true;

            if (HasTrait(
                    NpcTrait.GazeAverter) &&
                GetComponent<NpcGazeAverter>() == null)
            {
                gameObject.AddComponent<
                    NpcGazeAverter>();
            }

            if (HasTrait(
                    NpcTrait.AwakeningAura) &&
                GetComponent<NpcAwakeningAura>() == null)
            {
                gameObject.AddComponent<
                    NpcAwakeningAura>();
            }
        }
    }
}
