using System;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 기획서 A.11절: 별도 튜토리얼 스테이지 없이 첫 구역에서
    /// 최면 → 동행 → 회수 → 쟁탈 → 폭주 순으로 기능을 단계적으로 개방한다.
    /// </summary>
    public enum TutorialStep
    {
        Hypnosis = 0,
        Follow = 1,
        Recover = 2,
        Contest = 3,
        Rampage = 4,
        Completed = 5
    }

    /// <summary>
    /// 튜토리얼 진행 규칙이다.
    ///
    /// 실제 안내 문구 배치는 구역 콘텐츠 단계에서 하고,
    /// 여기서는 "무엇을 하면 다음 단계로 넘어가는가"만 정의한다.
    /// </summary>
    public static class TutorialFlowLogic
    {
        public const int StepCount = 5;

        public static TutorialStep GetNext(
            TutorialStep step)
        {
            int next =
                (int)step + 1;

            return next >=
                   (int)TutorialStep.Completed
                ? TutorialStep.Completed
                : (TutorialStep)next;
        }

        public static bool IsCompleted(
            TutorialStep step)
        {
            return step ==
                   TutorialStep.Completed;
        }

        /// <summary>해당 단계가 요구하는 행동이 일어났는지 판정한다.</summary>
        public static bool IsSatisfied(
            TutorialStep step,
            TutorialProgress progress)
        {
            switch (step)
            {
                case TutorialStep.Hypnosis:
                    return progress.HypnosisCount >= 1;

                case TutorialStep.Follow:
                    return progress.MaximumFollowers >= 2;

                case TutorialStep.Recover:
                    return progress.RecoveryCount >= 1;

                case TutorialStep.Contest:
                    return progress.ReclaimCount >= 1;

                case TutorialStep.Rampage:
                    return progress.RampageSurvivedCount >= 1;

                case TutorialStep.Completed:
                default:
                    return true;
            }
        }

        /// <summary>현재 단계를 진행 상황에 맞게 전진시킨다. 여러 단계를 한 번에 넘길 수 있다.</summary>
        public static TutorialStep Advance(
            TutorialStep step,
            TutorialProgress progress)
        {
            TutorialStep current =
                step;

            int guard =
                0;

            while (!IsCompleted(
                       current) &&
                   IsSatisfied(
                       current,
                       progress) &&
                   guard < StepCount + 1)
            {
                current =
                    GetNext(
                        current);

                guard++;
            }

            return current;
        }

        public static string GetHint(
            TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.Hypnosis:
                    return "E 또는 좌클릭을 유지해 NPC를 최면하세요";

                case TutorialStep.Follow:
                    return "두 명 이상을 동행시켜 보세요";

                case TutorialStep.Recover:
                    return "회수 지점으로 데려가야 정기가 확정됩니다";

                case TutorialStep.Contest:
                    return "빼앗긴 NPC에 다시 최면해 되찾으세요";

                case TutorialStep.Rampage:
                    return "폭주 경고를 보고 회피하거나 파동으로 진정시키세요";

                case TutorialStep.Completed:
                default:
                    return string.Empty;
            }
        }
    }

    /// <summary>튜토리얼 진행 판정에 쓰는 플레이 기록이다.</summary>
    [Serializable]
    public struct TutorialProgress
    {
        public int HypnosisCount;
        public int MaximumFollowers;
        public int RecoveryCount;
        public int ReclaimCount;
        public int RampageSurvivedCount;
    }
}
