using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Rival;

namespace ProjectTheta.Duel
{
    public enum OpponentDuelKind
    {
        Geumtaeyang,
        PopularGuy
    }

    public sealed class OpponentDuelTarget : MonoBehaviour
    {
        [SerializeField] private OpponentDuelKind _kind;
        [SerializeField] private int _maximumDefeats = 3;

        private RivalController _geumtaeyang;
        private PopularGuyController _popularGuy;
        private int _lossCount;

        public string DisplayName =>
            _kind ==
            OpponentDuelKind.Geumtaeyang
                ? "금태양"
                : "인기남";

        public int LossCount =>
            _lossCount;

        public int MaximumDefeats =>
            Mathf.Max(
                1,
                _maximumDefeats);

        public int RemainingDefeats =>
            Mathf.Max(
                0,
                MaximumDefeats -
                _lossCount);

        public bool CanStartDuel
        {
            get
            {
                switch (_kind)
                {
                    case OpponentDuelKind.Geumtaeyang:
                        return
                            _geumtaeyang != null &&
                            _geumtaeyang.
                                CanStartPlayerDuel;

                    case OpponentDuelKind.PopularGuy:
                        return
                            _popularGuy != null &&
                            _popularGuy.
                                CanStartPlayerDuel;

                    default:
                        return false;
                }
            }
        }

        private void Awake()
        {
            CacheControllers();
        }

        public void Configure(
            OpponentDuelKind kind)
        {
            _kind =
                kind;

            CacheControllers();
        }

        public void BeginDuel()
        {
            SetControllerDuelLock(
                true);
        }

        public void EndDuelWithoutStun()
        {
            SetControllerDuelLock(
                false);
        }

        public bool RegisterPlayerVictory(
            Vector3 playerPosition,
            float stunDuration,
            float knockbackDistance)
        {
            _lossCount =
                OpponentDuelDurabilityLogic.
                    AddLoss(
                        _lossCount,
                        MaximumDefeats);

            KnockAwayFrom(
                playerPosition,
                knockbackDistance);

            if (OpponentDuelDurabilityLogic.
                    IsDefeated(
                        _lossCount,
                        MaximumDefeats))
            {
                SetControllerDuelLock(
                    false);

                ReleaseAllOwnedFollowers();

                gameObject.SetActive(
                    false);

                return true;
            }

            ApplyControllerStun(
                stunDuration);

            return false;
        }

        private void CacheControllers()
        {
            _geumtaeyang =
                GetComponent<
                    RivalController>();

            _popularGuy =
                GetComponent<
                    PopularGuyController>();
        }

        private void SetControllerDuelLock(
            bool locked)
        {
            if (_kind ==
                    OpponentDuelKind.Geumtaeyang &&
                _geumtaeyang != null)
            {
                _geumtaeyang.SetDuelLocked(
                    locked);
            }
            else if (_kind ==
                        OpponentDuelKind.PopularGuy &&
                     _popularGuy != null)
            {
                _popularGuy.SetDuelLocked(
                    locked);
            }
        }

        private void ApplyControllerStun(
            float duration)
        {
            if (_kind ==
                    OpponentDuelKind.Geumtaeyang &&
                _geumtaeyang != null)
            {
                _geumtaeyang.ApplyDuelStun(
                    duration);
            }
            else if (_kind ==
                        OpponentDuelKind.PopularGuy &&
                     _popularGuy != null)
            {
                _popularGuy.ApplyDuelStun(
                    duration);
            }
        }

        private void KnockAwayFrom(
            Vector3 sourcePosition,
            float distance)
        {
            float direction =
                transform.position.x >=
                sourcePosition.x
                    ? 1f
                    : -1f;

            Vector3 position =
                transform.position;

            position.x +=
                direction *
                Mathf.Max(
                    0f,
                    distance);

            transform.position =
                position;

            Rigidbody2D body =
                GetComponent<
                    Rigidbody2D>();

            if (body != null)
            {
                body.linearVelocity =
                    Vector2.zero;
            }
        }

        private void ReleaseAllOwnedFollowers()
        {
            HypnosisTarget[] targets =
                FindObjectsByType<HypnosisTarget>(
                    FindObjectsSortMode.None);

            for (int i = 0;
                 i < targets.Length;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null)
                {
                    continue;
                }

                if (_kind ==
                        OpponentDuelKind.Geumtaeyang &&
                    target.Owner ==
                        NpcOwner.Geumtaeyang &&
                    target.GeumtaeyangOwner ==
                        _geumtaeyang)
                {
                    _geumtaeyang?.
                        ReleaseOwnedTarget(
                            target);

                    target.ResetHypnosis();
                }
                else if (_kind ==
                             OpponentDuelKind.PopularGuy &&
                         target.Owner ==
                             NpcOwner.PopularGuy &&
                         target.PopularGuyOwner ==
                             _popularGuy)
                {
                    _popularGuy?.
                        ReleaseOwnedTarget(
                            target);

                    target.ResetHypnosis();
                }
            }
        }
    }
}
