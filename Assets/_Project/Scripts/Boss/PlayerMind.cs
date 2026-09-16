using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Hypnosis;
using ProjectTheta.Player;
using ProjectTheta.Stage;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 플레이어 정신력이다 (26일차, 부록 A.10). 보스전이 있는 장소에서만 붙는다.
    ///
    /// 역최면 시선에 맞으면 줄고, 2초 동안 맞지 않으면 회복한다.
    /// 0이 되면 붕괴 — 5초 조작 불능, 동행자 2명을 라이벌에게 잃고, 절반으로 회복한다.
    /// 30초 안에 두 번 붕괴하면 패배(체력 0과 같은 실패)다.
    /// </summary>
    [RequireComponent(typeof(PlayerSideViewController))]
    public sealed class PlayerMind : MonoBehaviour
    {
        private PlayerSideViewController _movement;
        private FollowerManager _followers;
        private PlayerHealth _health;
        private StageSessionController _stage;

        private float _mind = MindLogic.Maximum;
        private float _sinceHit = 99f;
        private float _clock;
        private float _lastCollapse;
        private bool _collapsedBefore;

        public float Normalized =>
            _mind / MindLogic.Maximum;

        public int CollapseCount { get; private set; }

        public float SinceHit =>
            _sinceHit;

        private void Awake()
        {
            _movement = GetComponent<PlayerSideViewController>();
            _followers = GetComponent<FollowerManager>();
            _health = GetComponent<PlayerHealth>();
            _stage = GetComponent<StageSessionController>();
        }

        public void Damage(
            float amount)
        {
            if (DebugCheats.Invincible)
            {
                return;
            }

            _mind =
                MindLogic.Damage(
                    _mind,
                    amount *
                    Mathf.Max(0f, BalanceOverrides.StageOrDefault.MindDamageScale));

            _sinceHit = 0f;

            if (MindLogic.IsCollapsed(_mind))
            {
                Collapse();
            }
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            _clock += deltaTime;
            _sinceHit += deltaTime;

            _mind = MindLogic.Tick(_mind, _sinceHit, deltaTime);
        }

        private void Collapse()
        {
            CollapseCount++;

            StageMoments.RaiseMindCollapsed(transform.position);

            if (MindLogic.IsDefeat(_clock, _lastCollapse, _collapsedBefore))
            {
                if (_health != null)
                {
                    _health.TakeDamage(_health.MaximumHealth);
                }

                if (_stage != null)
                {
                    _stage.RefreshState();
                }

                return;
            }

            _collapsedBefore = true;
            _lastCollapse = _clock;

            if (_movement != null)
            {
                _movement.ApplyStagger(MindLogic.CollapseStunSeconds);
            }

            LoseFollowers();

            _mind = MindLogic.Maximum * MindLogic.RecoverFraction;
        }

        private void LoseFollowers()
        {
            if (_followers == null)
            {
                return;
            }

            IReadOnlyList<FollowerController> list = _followers.Followers;

            for (int lost = 0;
                 lost < MindLogic.CollapseFollowerLoss && list.Count > 0;
                 lost++)
            {
                FollowerController follower = list[list.Count - 1];

                if (follower == null)
                {
                    break;
                }

                _followers.RequestRelease(follower);

                HypnosisTarget target = follower.GetComponent<HypnosisTarget>();

                if (target != null)
                {
                    target.ClaimByRival();
                }
            }
        }
    }
}
