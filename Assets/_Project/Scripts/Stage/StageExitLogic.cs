using System;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Stage
{
    /// <summary>장소가 어떻게 끝났는지다 (33일차). 결과 화면 · 기록에 쓴다.</summary>
    public enum StageExitKind
    {
        /// <summary>탈출 규칙이 없는 끝(보스 함락 · 열차 생존 · 실패 · 포기).</summary>
        None = 0,

        /// <summary>목표를 채우고 1F 회수 지점에서 탈출했다.</summary>
        Escaped = 1,

        /// <summary>목표를 채운 채 제한 시간이 끝났다. 클리어로 친다.</summary>
        TimeUp = 2
    }

    /// <summary>
    /// 목표 달성 뒤 탈출 규칙이다 (33일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   정기 목표 달성 → 곧바로 끝나지 않고 "탈출 가능"(ExitReady)
    ///     · 1F 회수 지점에서 상호작용 키 → 클리어(탈출)
    ///     · 남아서 더 모으다가 제한 시간이 끝남 → 클리어(시간 종료)
    ///     · 탈출 전에 쓰러짐 → 실패
    ///
    /// 열차 생존(지하철) · 보스 함락(루프탑 클럽)은 끝나는 조건이 따로 있어 예전 규칙 그대로다.
    /// </summary>
    public static class StageExitLogic
    {
        /// <summary>탈출하는 층이다. 0 = 1F.</summary>
        public const int ExitFloor = 0;

        public static bool UsesExit(
            LocationObjective objective)
        {
            return objective != LocationObjective.Survival &&
                   objective != LocationObjective.Boss;
        }

        /// <summary>
        /// 상태를 정한다. 탈출 규칙이 없는 목표는 기존 판정(<see cref="ObjectiveStateLogic"/>)을 그대로 돌려준다.
        /// </summary>
        public static StageState Resolve(
            LocationObjective objective,
            float remainingTime,
            int currentEssence,
            int targetEssence,
            int currentHealth,
            int trainsArrived,
            int trainsRequired,
            int followerCount,
            bool bossDefeated,
            bool exitRequested)
        {
            if (!UsesExit(objective))
            {
                return ObjectiveStateLogic.Resolve(
                    objective,
                    remainingTime,
                    currentEssence,
                    targetEssence,
                    currentHealth,
                    trainsArrived,
                    trainsRequired,
                    followerCount,
                    bossDefeated);
            }

            if (currentHealth <= 0)
            {
                return StageState.FailedByHealth;
            }

            bool goalMet =
                currentEssence >= Math.Max(1, targetEssence);

            if (!goalMet)
            {
                return remainingTime <= 0f
                    ? StageState.FailedByTime
                    : StageState.Running;
            }

            if (exitRequested ||
                remainingTime <= 0f)
            {
                return StageState.Cleared;
            }

            return StageState.ExitReady;
        }

        /// <summary>끝난 방식이다. 클리어가 아니면 None이다.</summary>
        public static StageExitKind GetExitKind(
            LocationObjective objective,
            StageState state,
            bool exitRequested)
        {
            if (state != StageState.Cleared ||
                !UsesExit(objective))
            {
                return StageExitKind.None;
            }

            return exitRequested
                ? StageExitKind.Escaped
                : StageExitKind.TimeUp;
        }

        /// <summary>지금 탈출할 수 있는지다. 1F 회수 지점 안에 서 있고 게임이 멈추지 않았어야 한다.</summary>
        public static bool CanExit(
            StageState state,
            int playerFloor,
            bool insideExitZone,
            bool paused)
        {
            return state == StageState.ExitReady &&
                   playerFloor == ExitFloor &&
                   insideExitZone &&
                   !paused;
        }

        /// <summary>목표를 넘겨 모은 정기다.</summary>
        public static int GetOverflow(
            int recoveredEssence,
            int targetEssence)
        {
            return Math.Max(0, recoveredEssence - Math.Max(0, targetEssence));
        }

        /// <summary>결과 화면 제목에 붙는 글자다.</summary>
        public static string GetTitleSuffix(
            StageExitKind kind)
        {
            switch (kind)
            {
                case StageExitKind.Escaped:
                    return "  ·  탈출 성공";

                case StageExitKind.TimeUp:
                    return "  ·  시간 종료 (목표 달성)";

                default:
                    return string.Empty;
            }
        }

        /// <summary>화면 위쪽 안내 문구다.</summary>
        public static string GetBanner(
            string interactKey,
            int playerFloor,
            int overflow,
            float remainingTime)
        {
            string where =
                playerFloor == ExitFloor
                    ? $"1F 회수 지점에서 [{interactKey}] 탈출"
                    : "1F로 내려가 회수 지점에서 탈출";

            int seconds = (int)Math.Ceiling(Math.Max(0f, remainingTime));

            return $"목표 달성!  {where}   ·   남아서 더 모으면 보상 ↑  (초과 +{overflow} · 남은 {seconds}초)";
        }
    }
}
