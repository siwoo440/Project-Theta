using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스 보스전 진행이다 (26일차, 부록 A.10 · C.3 [7]).
    ///
    ///   세력 쟁탈  라이벌이 손님을 1초에 한 명씩 가져간다. 내 세력이 1.2배를 넘어야 공격할 수 있다
    ///   보호막     우세한 채 라이벌에게 최면을 6초 유지하면 한 장씩 깨진다 (3장)
    ///   본체 최면  지배 게이지를 100까지 채우면 함락. 라이벌 기술이 더 자주 나온다
    ///
    /// 보스를 겨누는 동안은 플레이어 최면이 NPC 대신 보스로 향한다
    /// (<see cref="HypnosisCaster.FocusOverride"/>).
    /// </summary>
    public sealed class BossBattle : MonoBehaviour
    {
        private static BossBattle _current;

        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private StageSessionController _stage;
        private FollowerManager _followers;
        private HypnosisCaster _caster;
        private Transform _player;

        private RivalSuccubus _boss;
        private int _floor;

        private int _shields = BossBattleLogic.ShieldCount;
        private float _shieldProgress;
        private float _dominance;
        private int _rivalEssence;

        /// <summary>라이벌 손님 확보 속도 배율이다 (27일차 샴페인 타워가 올린다).</summary>
        public float RivalClaimRate { get; set; } = 1f;

        public static BossBattle Current =>
            _current;

        public BossPhase Phase { get; private set; } =
            BossPhase.Contest;

        public int ShieldsLeft =>
            _shields;

        public float ShieldProgressNormalized =>
            _shieldProgress / BossBattleLogic.SecondsPerShield;

        public float DominanceNormalized =>
            _dominance / BossBattleLogic.DominanceMaximum;

        public int PlayerForce { get; private set; }

        public int RivalForce { get; private set; }

        public int RivalClaimed { get; private set; }

        public bool IsDefeated =>
            Phase == BossPhase.Defeated;

        public bool IsFocusing { get; private set; }

        public RivalSuccubus Boss =>
            _boss;

        public int Floor =>
            _floor;

        public static BossBattle Create(
            StageSessionController stage,
            Transform player,
            FollowerManager followers,
            int floor)
        {
            GameObject root = new GameObject("BossBattle");

            BossBattle battle = root.AddComponent<BossBattle>();

            battle._stage = stage;
            battle._player = player;
            battle._followers = followers;
            battle._caster = player == null ? null : player.GetComponent<HypnosisCaster>();
            battle._floor = floor;

            _current = battle;

            battle._boss =
                RivalSuccubus.Spawn(
                    battle,
                    stage,
                    player,
                    followers,
                    floor,
                    Stage.Locations.ClubLayout.BossStartX);

            HypnosisCaster.FocusOverride = battle.ShouldCaptureFocus;

            return battle;
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
                HypnosisCaster.FocusOverride = null;
            }
        }

        public void AddRivalEssence(
            int amount)
        {
            _rivalEssence += Mathf.Max(0, amount);
            RivalClaimed++;
        }

        /// <summary>디버그 치트: 보호막 한 장을 깬다. 모두 깨졌으면 지배 게이지를 50 올린다.</summary>
        public void DebugAdvance()
        {
            if (_shields > 0)
            {
                BreakShield();

                return;
            }

            _dominance =
                Mathf.Min(
                    BossBattleLogic.DominanceMaximum,
                    _dominance + 50f);
        }

        /// <summary>플레이어 최면이 보스를 겨눌지다. 최면 시전기가 매 프레임 묻는다.</summary>
        private bool ShouldCaptureFocus(
            Vector2 playerPosition)
        {
            if (_boss == null ||
                !_boss.isActiveAndEnabled ||
                IsDefeated ||
                !BossBattleLogic.CanFocusBoss(Phase))
            {
                return false;
            }

            if (FloorSpace.FloorAt(playerPosition.y) != _floor)
            {
                return false;
            }

            return Vector2.Distance(playerPosition, _boss.transform.position) <=
                   BossBattleLogic.FocusRange;
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                IsDefeated ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                IsFocusing = false;

                return;
            }

            float deltaTime = Time.deltaTime;

            UpdateForces();

            bool advantage =
                BossBattleLogic.HasAdvantage(
                    PlayerForce,
                    RivalForce,
                    BalanceOverrides.StageOrDefault.BossAdvantageRatio);

            Phase =
                BossBattleLogic.GetPhase(
                    _shields,
                    _dominance,
                    advantage);

            IsFocusing =
                _caster != null &&
                _caster.IsHolding &&
                _player != null &&
                _boss != null &&
                !_boss.Body.IsStunned &&
                ShouldCaptureFocus(_player.position);

            switch (Phase)
            {
                case BossPhase.Shield:
                    if (BossBattleLogic.AdvanceShield(
                            ref _shieldProgress,
                            IsFocusing,
                            deltaTime))
                    {
                        BreakShield();
                    }
                    break;

                case BossPhase.Dominate:
                    _dominance =
                        BossBattleLogic.AdvanceDominance(
                            _dominance,
                            IsFocusing,
                            deltaTime);

                    if (_dominance >= BossBattleLogic.DominanceMaximum)
                    {
                        Defeat();
                    }
                    break;

                default:
                    BossBattleLogic.AdvanceShield(
                        ref _shieldProgress,
                        false,
                        deltaTime);
                    break;
            }
        }

        private void UpdateForces()
        {
            int essence =
                _stage == null
                    ? 0
                    : _stage.CurrentEssence;

            PlayerForce =
                BossBattleLogic.GetForce(
                    _followers == null ? 0 : _followers.Count,
                    essence);

            HypnosisTarget.CopyActive(_targetBuffer);

            int rivalOwned = 0;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                if (_targetBuffer[i] != null &&
                    _targetBuffer[i].isActiveAndEnabled &&
                    _targetBuffer[i].Owner == NpcOwner.Rival)
                {
                    rivalOwned++;
                }
            }

            RivalForce =
                BossBattleLogic.GetForce(
                    rivalOwned,
                    _rivalEssence);
        }

        private void BreakShield()
        {
            if (_shields <= 0)
            {
                return;
            }

            _shields--;
            _shieldProgress = 0f;

            StageMoments.RaiseBossShieldBroken(
                _boss == null ? (Vector2)transform.position : (Vector2)_boss.transform.position,
                _shields);
        }

        private void Defeat()
        {
            Phase = BossPhase.Defeated;
            IsFocusing = false;

            if (_boss != null)
            {
                _boss.Defeat();
            }

            StageMoments.RaiseBossDefeated(
                _boss == null ? (Vector2)transform.position : (Vector2)_boss.transform.position);

            if (_stage != null)
            {
                _stage.RefreshState();
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
