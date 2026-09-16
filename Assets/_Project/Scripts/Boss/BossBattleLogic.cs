using System;

namespace ProjectTheta.Boss
{
    /// <summary>보스전 단계다 (부록 A.10 · C.3 [7]).</summary>
    public enum BossPhase
    {
        /// <summary>세력이 밀려 보스를 공격할 수 없다. 중립 손님을 먼저 확보한다.</summary>
        Contest = 0,

        /// <summary>세력이 우세해 보호막을 깰 수 있다.</summary>
        Shield = 1,

        /// <summary>보호막이 모두 깨져 본체를 최면한다.</summary>
        Dominate = 2,

        /// <summary>지배 게이지가 가득 찼다. 승리.</summary>
        Defeated = 3
    }

    /// <summary>
    /// 라이벌 서큐버스 보스전 계산이다 (26일차). Unity 없이 도는 순수 계산이다.
    ///
    ///   세력 점수   동행 인원 × 10 + 확보 정기 ÷ 10
    ///   우세        내 세력 ≥ 라이벌 세력 × 1.2 (라이벌 세력이 0이면 내 세력이 1 이상)
    ///   보호막      3장. 우세한 채 보스에게 최면을 6초 유지하면 1장
    ///   지배 게이지  보호막이 모두 깨지면 최면 유지로 0 → 100 (초당 8), 놓으면 초당 2씩 빠짐
    ///   기술 빈도    본체 최면 단계에서 재사용 대기 ÷ 1.5
    /// </summary>
    public static class BossBattleLogic
    {
        public const int ShieldCount = 3;
        public const float SecondsPerShield = 6f;
        public const float DefaultAdvantageRatio = 1.2f;

        public const float DominanceMaximum = 100f;
        public const float DominanceRisePerSecond = 8f;
        public const float DominanceDecayPerSecond = 2f;

        public const float DominateSkillRate = 1.5f;

        /// <summary>라이벌이 손님 한 명을 확보하는 간격이다.</summary>
        public const float RivalClaimSeconds = 1f;
        public const float RivalClaimRange = 7f;

        /// <summary>라이벌이 손님 한 명을 확보할 때 쌓는 정기다. 세력 점수에 들어간다.</summary>
        public const int RivalEssencePerClaim = 8;

        /// <summary>플레이어가 보스에게 최면을 걸 수 있는 거리다.</summary>
        public const float FocusRange = 4.5f;

        public static int GetForce(
            int followers,
            int essence)
        {
            return Math.Max(0, followers) * 10 +
                   Math.Max(0, essence) / 10;
        }

        public static bool HasAdvantage(
            int playerForce,
            int rivalForce,
            float ratio)
        {
            float safeRatio =
                float.IsNaN(ratio) || ratio < 1f
                    ? 1f
                    : ratio;

            if (rivalForce <= 0)
            {
                return playerForce > 0;
            }

            // 1.2f 같은 배율이 float 오차로 살짝 커져 딱 맞는 세력을 놓치지 않게 여유를 둔다.
            return playerForce + 0.01f >= rivalForce * safeRatio;
        }

        public static BossPhase GetPhase(
            int shieldsLeft,
            float dominance,
            bool advantage)
        {
            if (dominance >= DominanceMaximum)
            {
                return BossPhase.Defeated;
            }

            if (shieldsLeft <= 0)
            {
                return BossPhase.Dominate;
            }

            return advantage
                ? BossPhase.Shield
                : BossPhase.Contest;
        }

        /// <summary>플레이어 최면이 보스를 겨눌 수 있는 단계인지다.</summary>
        public static bool CanFocusBoss(
            BossPhase phase)
        {
            return phase == BossPhase.Shield ||
                   phase == BossPhase.Dominate;
        }

        /// <summary>보호막 진행을 흘리고, 한 장이 깨지면 true다.</summary>
        public static bool AdvanceShield(
            ref float progress,
            bool focusing,
            float deltaTime)
        {
            if (!focusing ||
                deltaTime <= 0f)
            {
                progress = Math.Max(0f, progress - Math.Max(0f, deltaTime) * 0.5f);

                return false;
            }

            progress += deltaTime;

            if (progress < SecondsPerShield)
            {
                return false;
            }

            progress = 0f;

            return true;
        }

        public static float AdvanceDominance(
            float dominance,
            bool focusing,
            float deltaTime)
        {
            if (deltaTime <= 0f ||
                float.IsNaN(dominance))
            {
                return Clamp(dominance);
            }

            return Clamp(
                focusing
                    ? dominance + DominanceRisePerSecond * deltaTime
                    : dominance - DominanceDecayPerSecond * deltaTime);
        }

        public static float GetSkillCooldown(
            float baseCooldown,
            BossPhase phase)
        {
            float safe = Math.Max(0f, baseCooldown);

            return phase == BossPhase.Dominate
                ? safe / DominateSkillRate
                : safe;
        }

        public static string GetPhaseLabel(
            BossPhase phase)
        {
            switch (phase)
            {
                case BossPhase.Shield:
                    return "보호막 공격 가능";

                case BossPhase.Dominate:
                    return "본체 최면";

                case BossPhase.Defeated:
                    return "함락";

                default:
                    return "세력 쟁탈 · 손님을 먼저 확보하세요";
            }
        }

        private static float Clamp(
            float value)
        {
            return float.IsNaN(value)
                ? 0f
                : Math.Max(0f, Math.Min(DominanceMaximum, value));
        }
    }

    /// <summary>
    /// 플레이어 정신력 계산이다 (26일차, 부록 A.10).
    ///   최대 100. 역최면 시선에 맞으면 줄고, 2초 동안 맞지 않으면 초당 5씩 회복
    ///   0이 되면 붕괴 — 5초 조작 불능, 동행자 2명을 라이벌에게 잃고, 절반으로 회복
    ///   30초 안에 두 번 붕괴하면 패배
    /// </summary>
    public static class MindLogic
    {
        public const float Maximum = 100f;
        public const float RegenPerSecond = 5f;
        public const float RegenDelaySeconds = 2f;
        public const float CollapseStunSeconds = 5f;
        public const float RecoverFraction = 0.5f;
        public const int CollapseFollowerLoss = 2;
        public const float DefeatWindowSeconds = 30f;

        public static float Tick(
            float mind,
            float sinceHit,
            float deltaTime)
        {
            if (float.IsNaN(mind))
            {
                return Maximum;
            }

            if (sinceHit < RegenDelaySeconds ||
                deltaTime <= 0f)
            {
                return Math.Max(0f, Math.Min(Maximum, mind));
            }

            return Math.Min(Maximum, mind + RegenPerSecond * deltaTime);
        }

        public static float Damage(
            float mind,
            float amount)
        {
            if (float.IsNaN(amount) ||
                amount <= 0f)
            {
                return mind;
            }

            return Math.Max(0f, mind - amount);
        }

        public static bool IsCollapsed(
            float mind)
        {
            return mind <= 0f;
        }

        /// <summary>이번 붕괴가 직전 붕괴와 가까워 패배인지다.</summary>
        public static bool IsDefeat(
            float now,
            float lastCollapse,
            bool hasCollapsedBefore)
        {
            return hasCollapsedBefore &&
                   now - lastCollapse <= DefeatWindowSeconds;
        }
    }

    /// <summary>
    /// 보스 기술 수치다 (26 · 27일차, 부록 C.3 [7]).
    /// </summary>
    public static class BossSkillValues
    {
        // 역최면 시선
        public const float GazeTelegraphSeconds = 1.5f;
        public const float GazeBeamSeconds = 1.2f;
        public const float GazeLaneHalfHeight = 0.6f;
        public const float GazeDamagePerSecond = 20f;
        public const float GazeRange = 12f;
        public const float GazeCooldownSeconds = 9f;

        // VIP 매혹 파동
        public const float CharmTelegraphSeconds = 1.2f;
        public const float CharmRadius = 5f;
        public const float CharmGain = 40f;
        public const float CharmMaximum = 100f;
        public const float CharmDecayPerSecond = 5f;
        public const float CharmCooldownSeconds = 12f;

        /// <summary>시선이 플레이어를 맞히는지다. 조명 점멸 순간은 무효다.</summary>
        public static bool GazeHits(
            float laneY,
            float playerY,
            int direction,
            float originX,
            float playerX,
            bool flash)
        {
            if (flash ||
                Math.Abs(playerY - laneY) > GazeLaneHalfHeight)
            {
                return false;
            }

            float forward = (playerX - originX) * (direction >= 0 ? 1f : -1f);

            return forward >= 0f &&
                   forward <= GazeRange;
        }

        public static float AddCharm(
            float gauge,
            float amount)
        {
            return Math.Max(0f, Math.Min(CharmMaximum, gauge + Math.Max(0f, amount)));
        }

        public static float DecayCharm(
            float gauge,
            float deltaTime)
        {
            return Math.Max(0f, gauge - CharmDecayPerSecond * Math.Max(0f, deltaTime));
        }

        public static bool IsCharmed(
            float gauge)
        {
            return gauge >= CharmMaximum;
        }
    }
}
