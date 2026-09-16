using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 꼰대 부장이다 (25일차, 부록 C.3 [6]). 범위형 구출 역할이다.
    ///
    /// 반경 4m 안에서 최면이 진행 중인 NPC가 있으면 "이거 오늘까지 해!"를 외쳐 범위 안 게이지를 모두 0으로 만든다 (재사용 8초).
    /// 25초마다 탕비실에 커피를 리필하러 가서 10초 쉰다. 쉬는 동안은 외치지 않는다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class ManagerRole : MonoBehaviour
    {
        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private DisruptorBase _body;
        private float _elapsed;
        private float _cooldownRemaining;
        private bool _wasOnBreak;

        public bool IsOnBreak =>
            ManagerLogic.IsOnBreak(_elapsed);

        public float ShoutFlashRemaining { get; private set; }

        public void Configure()
        {
            _body = GetComponent<DisruptorBase>();

            // 부장마다 쉬는 때가 조금씩 어긋나게 한다.
            _elapsed = Random.Range(0f, 8f);
        }

        private void Update()
        {
            if (_body == null ||
                !_body.CanAct)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            _elapsed += deltaTime;

            if (_cooldownRemaining > 0f)
            {
                _cooldownRemaining -= deltaTime;
            }

            if (ShoutFlashRemaining > 0f)
            {
                ShoutFlashRemaining -= deltaTime;
            }

            bool onBreak = IsOnBreak;

            if (onBreak)
            {
                if (!_wasOnBreak)
                {
                    GameVfx.FloatText(
                        "커피 좀 마시고 올게",
                        (Vector2)transform.position + new Vector2(0f, 2.8f),
                        new Color(0.85f, 0.70f, 0.50f),
                        UI.Framework.UiTheme.FontSmall);
                }

                // 쉬는 동안은 탕비실 자리에 머문다. 도착하면 순찰로 돌아가지 않게 계속 붙잡아 둔다.
                _body.MoveToward(
                    new Vector2(
                        OfficeLayout.GetPantryX(_body.Floor),
                        transform.position.y),
                    0.5f);

                _wasOnBreak = true;

                return;
            }

            _wasOnBreak = false;

            TryShout();
        }

        private void TryShout()
        {
            if (_cooldownRemaining > 0f)
            {
                return;
            }

            HypnosisTarget.CopyActive(_targetBuffer);

            Vector2 self = transform.position;

            bool triggered = false;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (!IsNeutralOnFloor(target))
                {
                    continue;
                }

                if (ManagerLogic.ShouldShout(
                        target.HypnosisNormalized,
                        Vector2.Distance(self, target.transform.position),
                        false,
                        0f))
                {
                    triggered = true;

                    break;
                }
            }

            if (!triggered)
            {
                return;
            }

            int reset = 0;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (!IsNeutralOnFloor(target) ||
                    Vector2.Distance(self, target.transform.position) > ManagerLogic.ShoutRadius)
                {
                    continue;
                }

                if (target.HypnosisNormalized > 0f)
                {
                    target.KnockBackNeutralHypnosis(1f);
                    reset++;
                }
            }

            _cooldownRemaining = ManagerLogic.ShoutCooldownSeconds;
            ShoutFlashRemaining = 1.2f;

            GameVfx.Ripple(
                self + new Vector2(0f, 1f),
                new Color(1.00f, 0.55f, 0.35f),
                ManagerLogic.ShoutRadius,
                0.5f);

            GameVfx.FloatText(
                $"이거 오늘까지 해! (최면 {reset}명 초기화)",
                self + new Vector2(0f, 2.8f),
                new Color(1.00f, 0.55f, 0.35f),
                UI.Framework.UiTheme.FontBody);
        }

        private bool IsNeutralOnFloor(
            HypnosisTarget target)
        {
            return target != null &&
                   target.isActiveAndEnabled &&
                   target.Owner == NpcOwner.Neutral &&
                   FloorSpace.FloorAt(target.transform.position.y) == _body.Floor;
        }
    }
}
