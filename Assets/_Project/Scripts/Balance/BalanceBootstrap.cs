using UnityEngine;

namespace ProjectTheta.Balance
{
    /// <summary>
    /// 실행 시작 시 밸런스 자산을 읽어 각 표에 주입한다.
    ///
    /// 자산이 없으면 아무것도 주입하지 않으므로 코드 기본값이 그대로 쓰인다.
    /// 즉 자산 유무와 무관하게 게임은 항상 동작한다.
    /// </summary>
    public static class BalanceBootstrap
    {
        public const string ResourceFolder = "Balance";

        public const string NpcDatabasePath =
            ResourceFolder + "/NpcDatabase";

        public const string StageBalancePath =
            ResourceFolder + "/StageBalanceDatabase";

        private static bool _applied;

        /// <summary>자산이 실제로 적용되었는지 여부다. 디버그 HUD 표시에 사용한다.</summary>
        public static bool NpcAssetApplied { get; private set; }

        public static bool StageAssetApplied { get; private set; }

        /// <summary>
        /// 플레이 진입마다 가장 먼저 주입을 비운다.
        ///
        /// 도메인 리로드를 끈 설정에서는 static이 이전 플레이의 값을 그대로 들고 있고,
        /// <c>_applied</c>도 true로 남아 자산을 다시 읽지 않는다.
        /// 여기서 비워 두어야 매 플레이가 자산의 현재 값으로 시작한다.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Reset();
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Apply()
        {
            if (_applied)
            {
                return;
            }

            _applied =
                true;

            NpcDatabase npcDatabase =
                Resources.Load<NpcDatabase>(
                    NpcDatabasePath);

            if (npcDatabase != null)
            {
                npcDatabase.Apply();

                NpcAssetApplied =
                    true;
            }

            StageBalanceDatabase stageDatabase =
                Resources.Load<StageBalanceDatabase>(
                    StageBalancePath);

            if (stageDatabase != null)
            {
                stageDatabase.Apply();

                StageAssetApplied =
                    true;
            }
        }

        /// <summary>테스트나 에디터에서 주입을 되돌릴 때 사용한다.</summary>
        public static void Reset()
        {
            _applied =
                false;

            NpcAssetApplied =
                false;

            StageAssetApplied =
                false;

            NPC.NpcGradeTable.Override = null;
            NPC.NpcTraitTable.Override = null;
            Items.ConsumableItemTable.Override = null;
            Run.RunUpgradeTable.Override = null;
            Rival.OpponentTuningOverrides.Clear();

            BalanceOverrides.ResetToDefaults();
        }
    }
}
