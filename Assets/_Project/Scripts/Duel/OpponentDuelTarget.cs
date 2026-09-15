using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Rival;

namespace ProjectTheta.Duel
{
    /// <summary>
    /// 힘겨루기에서 플레이어의 상대가 되는 경쟁자 측 창구다.
    /// 경쟁자 종류별 분기 없이 <see cref="OpponentControllerBase"/> 하나만 다룬다.
    /// </summary>
    public sealed class OpponentDuelTarget : MonoBehaviour
    {
        /// <summary>최면 대상 검색용 버퍼다. 재사용해서 매번 배열을 만들지 않는다.</summary>
        private readonly System.Collections.Generic.List<HypnosisTarget> _targetBuffer =
            new System.Collections.Generic.List<HypnosisTarget>();
        [SerializeField] private int _maximumDefeats = 3;

        private OpponentControllerBase _opponent;
        private int _lossCount;

        private OpponentControllerBase Opponent
        {
            get
            {
                if (_opponent == null)
                {
                    _opponent =
                        GetComponent<
                            OpponentControllerBase>();
                }

                return _opponent;
            }
        }

        public string DisplayName =>
            Opponent == null
                ? "-"
                : Opponent.DisplayName;

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

        public bool CanStartDuel =>
            Opponent != null &&
            Opponent.CanStartPlayerDuel;

        public void BeginDuel()
        {
            Opponent?.SetDuelLocked(
                true);
        }

        public void EndDuelWithoutStun()
        {
            Opponent?.SetDuelLocked(
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
                Opponent?.SetDuelLocked(
                    false);

                ReleaseAllOwnedFollowers();

                gameObject.SetActive(
                    false);

                return true;
            }

            Opponent?.ApplyDuelStun(
                stunDuration);

            return false;
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
            if (Opponent == null)
            {
                return;
            }

            HypnosisTarget.CopyActive(
                _targetBuffer);

            System.Collections.Generic.List<HypnosisTarget> targets =
                _targetBuffer;

            for (int i = 0;
                 i < targets.Count;
                 i++)
            {
                HypnosisTarget target =
                    targets[i];

                if (target == null ||
                    target.OpponentOwner !=
                    Opponent)
                {
                    continue;
                }

                Opponent.ReleaseOwnedTarget(
                    target);

                target.ResetHypnosis();
            }
        }
    }
}
