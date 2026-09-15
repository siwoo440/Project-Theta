using System;
using UnityEngine;
using ProjectTheta.Items;
using ProjectTheta.Rival;
using ProjectTheta.Run;

namespace ProjectTheta.Balance
{
    [Serializable]
    public sealed class ConsumableEntry
    {
        public ConsumableItem Item = ConsumableItem.None;
        public string DisplayName = "-";
        public string Description = "비어 있음";
        public float Magnitude;
        public float DurationSeconds;

        public ConsumableItemProfile ToProfile()
        {
            return new ConsumableItemProfile(
                Item,
                DisplayName,
                Description,
                Magnitude,
                DurationSeconds);
        }
    }

    /// <summary>
    /// 스테이지 수치 · 경쟁자 튜닝 · 소비 아이템 · 난이도 배율을 담는 자산이다.
    ///
    /// 각 항목은 비워두면 코드 기본값이 쓰인다.
    /// </summary>
    [CreateAssetMenu(
        fileName = "StageBalanceDatabase",
        menuName = "Project θ/Stage Balance Database")]
    public sealed class StageBalanceDatabase : ScriptableObject
    {
        [Header("스테이지 수치")]
        [SerializeField] private StageBalanceValues _stage =
            new StageBalanceValues();

        [Header("경쟁자 튜닝")]
        [SerializeField] private bool _overrideOpponents;
        [SerializeField] private OpponentTuning _geumtaeyang =
            new OpponentTuning();
        [SerializeField] private OpponentTuning _popularGuy =
            new OpponentTuning();

        [Header("소비 아이템")]
        [SerializeField] private ConsumableEntry[] _consumables;

        [Header("난이도 배율")]
        [SerializeField] private DifficultyMultipliers[] _difficulties;

        [Header("런 강화 카드 (18일차)")]
        [SerializeField] private RunUpgradeProfile[] _runUpgrades;

        /// <summary>자산에 들어 있는 카드 표다. 자산 검사 테스트가 읽는다.</summary>
        public RunUpgradeProfile[] RunUpgrades =>
            _runUpgrades;

        public StageBalanceValues StageValues =>
            _stage;

        public void Apply()
        {
            BalanceOverrides.Stage =
                _stage;

            if (_overrideOpponents)
            {
                OpponentTuningOverrides.Geumtaeyang =
                    _geumtaeyang;

                OpponentTuningOverrides.PopularGuy =
                    _popularGuy;
            }
            else
            {
                OpponentTuningOverrides.Clear();
            }

            ConsumableItemTable.Override =
                BuildConsumables();

            // 비어 있으면 null을 넣어 코드 기본표를 쓰게 한다.
            RunUpgradeTable.Override =
                _runUpgrades != null &&
                _runUpgrades.Length > 0
                    ? _runUpgrades
                    : null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// 디버그 패널의 "자산에 저장"이 호출한다 (20일차, 에디터 전용).
        /// 넘겨받은 값의 복사본으로 자산의 스테이지 수치를 바꾸고 저장한다.
        /// 복사본을 넣으므로 이후 패널에서 계속 바꿔도 저장된 자산은 따라 바뀌지 않는다.
        /// </summary>
        public StageBalanceValues SaveStageValues(
            StageBalanceValues values)
        {
            if (values == null)
            {
                return _stage;
            }

            UnityEditor.Undo.RecordObject(
                this,
                "Debug Panel Save Balance");

            _stage =
                values.Clone();

            UnityEditor.EditorUtility.SetDirty(
                this);

            UnityEditor.AssetDatabase.SaveAssetIfDirty(
                this);

            return _stage;
        }
#endif

        /// <summary>자산에 정의된 난이도 배율을 찾는다. 없으면 코드 기본값을 쓴다.</summary>
        public DifficultyMultipliers FindDifficulty(
            DifficultyLevel level)
        {
            if (_difficulties != null)
            {
                for (int i = 0;
                     i < _difficulties.Length;
                     i++)
                {
                    if (_difficulties[i] != null &&
                        _difficulties[i].Level ==
                        level)
                    {
                        return _difficulties[i];
                    }
                }
            }

            return DifficultyTable.Get(
                level);
        }

        private ConsumableItemProfile[] BuildConsumables()
        {
            if (_consumables == null ||
                _consumables.Length == 0)
            {
                return null;
            }

            ConsumableItemProfile[] result =
                new ConsumableItemProfile[
                    _consumables.Length];

            for (int i = 0;
                 i < _consumables.Length;
                 i++)
            {
                result[i] =
                    _consumables[i].ToProfile();
            }

            return result;
        }
    }
}
