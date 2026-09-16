using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스 본체다 (26일차, 부록 C.3 [7]).
    ///
    /// 댄스플로어를 천천히 돌며 1초마다 가장 가까운 중립 손님을 자기 편으로 만든다.
    /// 방해 세력 몸체(<see cref="DisruptorBase"/>)를 그대로 써서 멍함 · 이름표 · 기술 예고 표시를 공유한다.
    /// 층당 인원 제한에는 세지 않는다 (스포너가 아니라 보스전이 만든다).
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class RivalSuccubus : MonoBehaviour
    {
        private const float Scale = 1.15f;
        private const float SeekRepathSeconds = 1f;

        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private BossBattle _battle;
        private SpriteRenderer _shadow;
        private float _claimRemaining = BossBattleLogic.RivalClaimSeconds;
        private float _seekRemaining;
        private bool _defeated;

        public DisruptorBase Body { get; private set; }

        public static RivalSuccubus Spawn(
            BossBattle battle,
            StageSessionController stage,
            Transform player,
            FollowerManager followers,
            int floor,
            float x)
        {
            DisruptorProfile profile =
                DisruptorCatalog.Get(DisruptorKind.RivalSuccubus);

            Vector2 position =
                FloorSpace.ToWorld(floor, new Vector2(x, -1.6f));

            GameObject go = new GameObject("RivalSuccubus");

            go.transform.SetParent(battle.transform, false);
            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(Scale, Scale, 1f);

            go.AddComponent<SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                go.AddComponent<RuntimeCharacterSpriteAnimator>();

            animator.Configure(profile.SpriteRoot, 7f, 390f);
            animator.SetBaseTint(profile.Tint);

            go.AddComponent<DepthSortByY>();

            DisruptorBase body = go.AddComponent<DisruptorBase>();

            body.Configure(profile, stage, animator, floor);

            RivalSuccubus boss = go.AddComponent<RivalSuccubus>();

            boss.Body = body;
            boss._battle = battle;

            // 분신과 구분하는 그림자다 (27일차 분신 댄서). 본체만 발밑에 진한 그림자가 있다.
            boss._shadow =
                Stage.Locations.LocationProps.Blob(
                    go.transform,
                    "BossShadow",
                    position,
                    new Vector2(1.4f, 0.45f),
                    new Color(0f, 0f, 0f, 0.55f),
                    -35);

            go.AddComponent<ReverseGazeAbility>().Configure(player, battle, 1f);
            go.AddComponent<CharmWaveAbility>().Configure(player, followers, battle);

            BossSkillHooks.AddLaterSkills(go, battle, player, followers);

            go.AddComponent<DisruptorStatusView>().Configure(body);

            return boss;
        }

        public void Defeat()
        {
            _defeated = true;

            Body.Stun(9999f);

            GameVfx.FloatText(
                "라이벌 서큐버스 함락!",
                (Vector2)transform.position + new Vector2(0f, 3f),
                UI.Framework.UiTheme.Gold,
                UI.Framework.UiTheme.FontTitle,
                2f);
        }

        private void Update()
        {
            if (_defeated ||
                Body == null ||
                !Body.CanAct)
            {
                return;
            }

            float deltaTime = Time.deltaTime;

            _claimRemaining -=
                deltaTime *
                Mathf.Max(0f, _battle == null ? 1f : _battle.RivalClaimRate);

            HypnosisTarget nearest =
                FindNearestNeutral(out float distance);

            if (_claimRemaining <= 0f)
            {
                _claimRemaining = BossBattleLogic.RivalClaimSeconds;

                if (nearest != null &&
                    distance <= BossBattleLogic.RivalClaimRange)
                {
                    Claim(nearest);
                }
            }

            _seekRemaining -= deltaTime;

            // 가까이에 손님이 없으면 가장 가까운 손님 쪽으로 걸어간다.
            if (_seekRemaining <= 0f &&
                nearest != null &&
                distance > BossBattleLogic.RivalClaimRange * 0.6f)
            {
                _seekRemaining = SeekRepathSeconds;

                Body.MoveToward(
                    nearest.transform.position,
                    SeekRepathSeconds + 0.2f);
            }
        }

        private HypnosisTarget FindNearestNeutral(
            out float distance)
        {
            HypnosisTarget.CopyActive(_targetBuffer);

            HypnosisTarget best = null;
            distance = float.MaxValue;

            Vector2 self = transform.position;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner != NpcOwner.Neutral ||
                    FloorSpace.FloorAt(target.transform.position.y) != Body.Floor)
                {
                    continue;
                }

                float d = Vector2.Distance(self, target.transform.position);

                if (d < distance)
                {
                    distance = d;
                    best = target;
                }
            }

            return best;
        }

        private void Claim(
            HypnosisTarget target)
        {
            target.ClaimByRival();

            if (_battle != null)
            {
                _battle.AddRivalEssence(BossBattleLogic.RivalEssencePerClaim);
            }

            GameVfx.Ripple(
                (Vector2)target.transform.position + new Vector2(0f, 1f),
                new Color(0.75f, 0.35f, 1.00f),
                0.8f,
                0.4f);
        }
    }
}
