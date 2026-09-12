using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Stage;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 각성 지원형 특성을 가진 NPC에만 붙는다.
    /// 반경 안의 플레이어 동행 NPC의 지배 수치를 지속적으로 깎고,
    /// 0이 되면 해당 NPC를 무리에서 이탈시킨다.
    /// </summary>
    public sealed class NpcAwakeningAura : MonoBehaviour
    {
        [SerializeField] private float _radius = 3.2f;
        [SerializeField] private float _drainPerSecond = 6f;
        [SerializeField] private float _rescanInterval = 0.25f;

        private readonly System.Collections.Generic.List<HypnosisTarget> _nearby =
            new System.Collections.Generic.List<HypnosisTarget>();

        private HypnosisTarget _self;
        private StageSessionController _stage;
        private FollowerManager _playerFollowers;
        private float _rescanTimer;
        private float _suppressedRemaining;

        /// <summary>최면 파동에 맞아 오라가 꺼져 있는 상태다.</summary>
        public bool IsSuppressed =>
            _suppressedRemaining > 0f;

        /// <summary>최면 파동이 이 오라를 일정 시간 무력화한다.</summary>
        public void Suppress(
            float seconds)
        {
            _suppressedRemaining =
                Mathf.Max(
                    _suppressedRemaining,
                    Mathf.Max(
                        0f,
                        seconds));
        }

        public float Radius =>
            Mathf.Max(
                0f,
                _radius);

        private void Awake()
        {
            _self =
                GetComponent<HypnosisTarget>();
        }

        private void Update()
        {
            ResolveRuntimeReferences();

            if (_stage == null ||
                !_stage.IsRunning)
            {
                return;
            }

            if (_suppressedRemaining > 0f)
            {
                _suppressedRemaining =
                    Mathf.Max(
                        0f,
                        _suppressedRemaining -
                        Time.deltaTime);

                return;
            }

            // 각성 NPC 본인이 누군가에게 최면당한 상태라면 각성 오라도 멈춘다.
            if (_self != null &&
                _self.Owner !=
                NpcOwner.Neutral)
            {
                return;
            }

            _rescanTimer -=
                Time.deltaTime;

            if (_rescanTimer <=
                0f)
            {
                _rescanTimer =
                    Mathf.Max(
                        0.05f,
                        _rescanInterval);

                RefreshNearbyTargets();
            }

            ApplyDrain();
        }

        private void RefreshNearbyTargets()
        {
            _nearby.Clear();

            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            float radiusSquared =
                Radius *
                Radius;

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget candidate =
                    targets[i];

                if (candidate == null ||
                    candidate == _self ||
                    !candidate.isActiveAndEnabled ||
                    candidate.Owner !=
                    NpcOwner.Player ||
                    !candidate.IsFollowing)
                {
                    continue;
                }

                float distanceSquared =
                    ((Vector2)candidate.transform.position -
                     (Vector2)transform.position).
                    sqrMagnitude;

                if (distanceSquared >
                    radiusSquared)
                {
                    continue;
                }

                _nearby.Add(
                    candidate);
            }
        }

        private void ApplyDrain()
        {
            float radiusSquared =
                Radius *
                Radius;

            for (int i = _nearby.Count - 1;
                 i >= 0;
                 i--)
            {
                HypnosisTarget target =
                    _nearby[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner !=
                    NpcOwner.Player ||
                    !target.IsFollowing)
                {
                    _nearby.RemoveAt(
                        i);

                    continue;
                }

                float distanceSquared =
                    ((Vector2)target.transform.position -
                     (Vector2)transform.position).
                    sqrMagnitude;

                if (distanceSquared >
                    radiusSquared)
                {
                    _nearby.RemoveAt(
                        i);

                    continue;
                }

                bool depleted =
                    target.ApplyAwakeningDrain(
                        _drainPerSecond,
                        Time.deltaTime);

                if (depleted)
                {
                    ReleaseFromPlayer(
                        target);

                    _nearby.RemoveAt(
                        i);
                }
            }
        }

        private void ReleaseFromPlayer(
            HypnosisTarget target)
        {
            FollowerController follower =
                target.GetComponent<
                    FollowerController>();

            if (_playerFollowers == null ||
                follower == null)
            {
                target.ResetHypnosis();

                return;
            }

            _playerFollowers.RequestRelease(
                follower);
        }

        private void ResolveRuntimeReferences()
        {
            if (_stage == null)
            {
                _stage =
                    FindFirstObjectByType<
                        StageSessionController>();
            }

            if (_playerFollowers == null)
            {
                _playerFollowers =
                    FindFirstObjectByType<
                        FollowerManager>();
            }
        }
    }
}
