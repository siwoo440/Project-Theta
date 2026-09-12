using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Core;
using ProjectTheta.Save;

namespace ProjectTheta.Player
{
    /// <summary>
    /// 세이브의 성장 레벨과 현재 난이도를 읽어, 각 시스템이 쓸 최종 배율로 환산한다.
    ///
    /// 각 시스템은 자기가 성장했는지 · 난이도가 무엇인지 알 필요 없이
    /// 여기서 나온 배율만 곱한다.
    /// </summary>
    public sealed class PlayerUpgrades : MonoBehaviour
    {
        private int[] _levels =
            new int[UpgradeLogic.TrackCount];

        public int GetLevel(
            UpgradeTrack track)
        {
            int index =
                (int)track;

            return index >= 0 &&
                   index < _levels.Length
                ? _levels[index]
                : 0;
        }

        private DifficultyMultipliers Difficulty =>
            BalanceOverrides.Difficulty ??
            DifficultyTable.Normal;

        // ---------------- 최종 배율 ----------------

        public float HypnosisSpeedMultiplier =>
            UpgradeLogic.GetHypnosisSpeedMultiplier(
                GetLevel(
                    UpgradeTrack.Hypnosis)) *
            Difficulty.HypnosisSpeed;

        public float ImpulseBuildMultiplier =>
            UpgradeLogic.GetImpulseBuildMultiplier(
                GetLevel(
                    UpgradeTrack.Stability)) *
            Difficulty.ImpulseBuild;

        public float RampageWarningBonus =>
            UpgradeLogic.GetRampageWarningBonus(
                GetLevel(
                    UpgradeTrack.Stability));

        public float RampageWarningMultiplier =>
            Difficulty.RampageWarning;

        public float MoveSpeedMultiplier =>
            UpgradeLogic.GetMoveSpeedMultiplier(
                GetLevel(
                    UpgradeTrack.Mobility));

        public float DashCostMultiplier =>
            UpgradeLogic.GetDashCostMultiplier(
                GetLevel(
                    UpgradeTrack.Mobility));

        public float FollowerStabilityDecayMultiplier =>
            UpgradeLogic.GetStabilityDecayMultiplier(
                GetLevel(
                    UpgradeTrack.Control));

        public int GetStableFollowerLimit(
            int baseLimit)
        {
            return UpgradeLogic.GetStableFollowerLimit(
                GetLevel(
                    UpgradeTrack.Control),
                baseLimit);
        }

        public float OpponentPressureMultiplier =>
            Difficulty.OpponentPressure;

        public float TimeLimitMultiplier =>
            Difficulty.TimeLimit;

        public float TargetEssenceMultiplier =>
            Difficulty.TargetEssence;

        private void Awake()
        {
            LoadFromSession();

            // 플레이어를 직접 참조하지 않는 컴포넌트도 배율을 읽을 수 있게 등록한다.
            PlayerUpgradeMultipliers.Active =
                this;
        }

        private void OnDestroy()
        {
            if (PlayerUpgradeMultipliers.Active == this)
            {
                PlayerUpgradeMultipliers.Clear();
            }
        }

        /// <summary>세이브에서 성장 레벨을 읽어온다.</summary>
        public void LoadFromSession()
        {
            SaveData save =
                GameSession.Instance == null
                    ? null
                    : GameSession.Instance.Save;

            if (save?.UpgradeLevels == null)
            {
                return;
            }

            int count =
                Mathf.Min(
                    _levels.Length,
                    save.UpgradeLevels.Length);

            for (int i = 0;
                 i < count;
                 i++)
            {
                _levels[i] =
                    UpgradeLogic.ClampLevel(
                        save.UpgradeLevels[i]);
            }
        }
    }
}
