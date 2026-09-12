using System;
using UnityEngine;
using ProjectTheta.NPC;

namespace ProjectTheta.Balance
{
    [Serializable]
    public sealed class NpcGradeEntry
    {
        public NpcGrade Grade = NpcGrade.Common;
        public string DisplayName = "일반";
        public float HypnosisBuildPerSecond = 40f;
        public int EssenceValue = 10;
        public float ImpulseBuildMultiplier = 1f;
        public int FixedTraitCount = 1;
        public int RandomTraitCount = 1;
        public Color MarkerColor = Color.white;

        public NpcGradeProfile ToProfile()
        {
            return new NpcGradeProfile(
                Grade,
                string.IsNullOrEmpty(
                    DisplayName)
                    ? Grade.ToString()
                    : DisplayName,
                HypnosisBuildPerSecond,
                EssenceValue,
                ImpulseBuildMultiplier,
                FixedTraitCount,
                RandomTraitCount,
                MarkerColor);
        }
    }

    [Serializable]
    public sealed class NpcTraitEntry
    {
        public NpcTrait Trait = NpcTrait.Plain;
        public string DisplayName = "일반형";
        public float HypnosisSpeedMultiplier = 1f;
        public float EssenceMultiplier = 1f;
        public Color MarkerColor = Color.white;

        public NpcTraitProfile ToProfile()
        {
            return new NpcTraitProfile(
                Trait,
                string.IsNullOrEmpty(
                    DisplayName)
                    ? Trait.ToString()
                    : DisplayName,
                HypnosisSpeedMultiplier,
                EssenceMultiplier,
                MarkerColor);
        }
    }

    /// <summary>
    /// NPC 등급·특성 수치를 담는 자산이다.
    ///
    /// 이 자산이 없으면 코드 안의 기본 표가 그대로 쓰이므로,
    /// 자산 로딩 실패가 게임을 깨뜨리지 않는다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "NpcDatabase",
        menuName = "Project θ/NPC Database")]
    public sealed class NpcDatabase : ScriptableObject
    {
        [SerializeField] private NpcGradeEntry[] _grades;
        [SerializeField] private NpcTraitEntry[] _traits;

        public void Apply()
        {
            NpcGradeTable.Override =
                BuildGrades();

            NpcTraitTable.Override =
                BuildTraits();
        }

        private NpcGradeProfile[] BuildGrades()
        {
            if (_grades == null ||
                _grades.Length == 0)
            {
                return null;
            }

            NpcGradeProfile[] result =
                new NpcGradeProfile[
                    _grades.Length];

            for (int i = 0;
                 i < _grades.Length;
                 i++)
            {
                result[i] =
                    _grades[i].ToProfile();
            }

            return result;
        }

        private NpcTraitProfile[] BuildTraits()
        {
            if (_traits == null ||
                _traits.Length == 0)
            {
                return null;
            }

            NpcTraitProfile[] result =
                new NpcTraitProfile[
                    _traits.Length];

            for (int i = 0;
                 i < _traits.Length;
                 i++)
            {
                result[i] =
                    _traits[i].ToProfile();
            }

            return result;
        }
    }
}
