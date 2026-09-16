using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 구출 역할이다 (24일차, 부록 C.3 [3] 퍼스널 트레이너).
    ///
    /// 담당 회원(중립 NPC 1명) 곁을 지킨다. 회원이 최면되기 시작하는 것을 보면 달려가
    /// 어깨를 두드려 최면 게이지를 절반으로 깎는다 (재사용 6초).
    /// 대처: 트레이너가 반대쪽을 볼 때 최면, 트레이너가 멀리 있을 때 체인으로 한 번에 끝내기, 파동.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class RescuerRole : MonoBehaviour
    {
        private const float PickRadius = 6f;
        private const float GuardRepathSeconds = 1.2f;
        private const float RushRepathSeconds = 0.3f;
        private const float GuardOffset = 0.9f;

        /// <summary>다른 트레이너가 이미 맡은 회원이다. 둘이 한 회원을 지키지 않게 한다.</summary>
        private static readonly HashSet<HypnosisTarget> Claimed =
            new HashSet<HypnosisTarget>();

        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private DisruptorBase _body;
        private HypnosisTarget _client;
        private Vector2 _home;
        private float _repathRemaining;
        private float _cooldownRemaining;
        private float _repickRemaining;
        private bool _rushing;

        public bool IsRushing =>
            _rushing;

        public HypnosisTarget Client =>
            _client;

        public void Configure()
        {
            _body = GetComponent<DisruptorBase>();
            _home = transform.position;

            PickClient();
        }

        private void OnDestroy()
        {
            if (_client != null)
            {
                Claimed.Remove(_client);
            }
        }

        private void Update()
        {
            if (_body == null ||
                !_body.CanAct)
            {
                if (_body != null &&
                    _body.IsStunned)
                {
                    _rushing = false;
                }

                return;
            }

            float deltaTime = Time.deltaTime;

            _repathRemaining -= deltaTime;

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            if (!IsClientValid())
            {
                _rushing = false;

                _repickRemaining -= deltaTime;

                if (_repickRemaining <= 0f)
                {
                    _repickRemaining = 2f;
                    PickClient();
                }

                return;
            }

            Vector2 self = transform.position;
            Vector2 client = _client.transform.position;

            if (!_rushing)
            {
                bool canSee =
                    DetectionLogic.IsInSight(
                        self.x,
                        self.y,
                        _body.Facing,
                        client.x,
                        client.y,
                        RescueLogic.NoticeHalfAngle,
                        RescueLogic.NoticeRange);

                if (RescueLogic.ShouldRush(
                        _client.HypnosisNormalized,
                        _client.Owner == NpcOwner.Neutral,
                        canSee,
                        _cooldownRemaining))
                {
                    _rushing = true;
                    _repathRemaining = 0f;

                    GameVfx.FloatText(
                        "회원님!",
                        self + new Vector2(0f, 2.4f),
                        new Color(0.60f, 0.85f, 0.95f),
                        UI.Framework.UiTheme.FontSmall);
                }
            }

            if (_rushing)
            {
                UpdateRush(self, client);
            }
            else
            {
                UpdateGuard(client);
            }
        }

        private void UpdateGuard(
            Vector2 client)
        {
            if (_repathRemaining > 0f)
            {
                return;
            }

            _repathRemaining = GuardRepathSeconds;

            // 회원 옆, 회원을 바라보는 쪽에 선다.
            float side =
                transform.position.x >= client.x
                    ? 1f
                    : -1f;

            _body.MoveToward(
                client + new Vector2(side * GuardOffset, 0f),
                GuardRepathSeconds);
        }

        private void UpdateRush(
            Vector2 self,
            Vector2 client)
        {
            if (Vector2.Distance(self, client) <= RescueLogic.TouchDistance)
            {
                _client.KnockBackNeutralHypnosis(
                    RescueLogic.DrainFraction);

                _rushing = false;
                _cooldownRemaining = RescueLogic.CooldownSeconds;

                GameVfx.FloatText(
                    "정신 차려요! 게이지 절반",
                    client + new Vector2(0f, 2f),
                    new Color(0.60f, 0.85f, 0.95f),
                    UI.Framework.UiTheme.FontBody);

                return;
            }

            if (_repathRemaining > 0f)
            {
                return;
            }

            _repathRemaining = RushRepathSeconds;

            _body.MoveToward(
                client,
                RushRepathSeconds + 0.2f);
        }

        private bool IsClientValid()
        {
            if (_client != null &&
                _client.isActiveAndEnabled &&
                _client.Owner == NpcOwner.Neutral &&
                FloorSpace.FloorAt(_client.transform.position.y) == _body.Floor)
            {
                return true;
            }

            if (_client != null)
            {
                Claimed.Remove(_client);
                _client = null;
            }

            return false;
        }

        private void PickClient()
        {
            HypnosisTarget.CopyActive(
                _targetBuffer);

            HypnosisTarget best = null;
            float bestDistance = PickRadius * PickRadius;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner != NpcOwner.Neutral ||
                    Claimed.Contains(target) ||
                    FloorSpace.FloorAt(target.transform.position.y) != _body.Floor)
                {
                    continue;
                }

                float distance =
                    ((Vector2)target.transform.position - _home).sqrMagnitude;

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = target;
                }
            }

            _client = best;

            if (best != null)
            {
                Claimed.Add(best);
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Claimed.Clear();
        }
    }
}
