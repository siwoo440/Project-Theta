using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌 서큐버스의 분신이다 (27일차).
    /// 15초 동안 댄스플로어를 돌며 역최면 시선(피해 절반)을 쏜다. 그림자가 없고, 멍해지면(파동) 바로 사라진다.
    /// </summary>
    [RequireComponent(typeof(DisruptorBase))]
    public sealed class CloneDancer : MonoBehaviour
    {
        private const float Scale = 1.15f;

        private static readonly List<CloneDancer> Active =
            new List<CloneDancer>();

        private DisruptorBase _body;
        private float _remaining;

        public static int ActiveCount =>
            Active.Count;

        public static CloneDancer Spawn(
            BossBattle battle,
            Transform player,
            int floor,
            float x)
        {
            DisruptorProfile profile =
                DisruptorCatalog.Get(DisruptorKind.RivalClone);

            Vector2 position =
                FloorSpace.ToWorld(
                    floor,
                    new Vector2(
                        Mathf.Clamp(x, FloorSpace.WalkMinX + 2f, FloorSpace.WalkMaxX - 4f),
                        -1.6f));

            GameObject go = new GameObject("RivalClone");

            if (battle != null)
            {
                go.transform.SetParent(battle.transform, false);
            }

            go.transform.position = new Vector3(position.x, position.y, 0f);
            go.transform.localScale = new Vector3(Scale, Scale, 1f);

            go.AddComponent<SpriteRenderer>();

            RuntimeCharacterSpriteAnimator animator =
                go.AddComponent<RuntimeCharacterSpriteAnimator>();

            animator.Configure(profile.SpriteRoot, 7f, 390f);
            animator.SetBaseTint(profile.Tint);

            go.AddComponent<DepthSortByY>();

            DisruptorBase body = go.AddComponent<DisruptorBase>();

            body.Configure(profile, null, animator, floor);

            CloneDancer clone = go.AddComponent<CloneDancer>();

            clone._body = body;
            clone._remaining = LateBossSkillValues.CloneSeconds;

            go.AddComponent<ReverseGazeAbility>().Configure(
                player,
                battle,
                LateBossSkillValues.CloneDamageScale);

            go.AddComponent<DisruptorStatusView>().Configure(body);

            GameVfx.Ripple(
                position + new Vector2(0f, 1f),
                new Color(0.75f, 0.35f, 1.00f),
                1.2f,
                0.4f);

            return clone;
        }

        private void Update()
        {
            if (GameplayPause.IsPaused)
            {
                return;
            }

            _remaining -= Time.deltaTime;

            bool defeated =
                BossBattle.Current == null ||
                BossBattle.Current.IsDefeated;

            if (_body != null &&
                _body.IsStunned)
            {
                GameVfx.FloatText(
                    "분신 소멸!",
                    (Vector2)transform.position + new Vector2(0f, 2.4f),
                    UiTheme.Positive,
                    UiTheme.FontBody);

                Vanish();

                return;
            }

            if (_remaining <= 0f ||
                defeated)
            {
                Vanish();
            }
        }

        private void Vanish()
        {
            GameVfx.Ripple(
                (Vector2)transform.position + new Vector2(0f, 1f),
                new Color(0.75f, 0.35f, 1.00f, 0.6f),
                0.8f,
                0.3f);

            Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (!Active.Contains(this))
            {
                Active.Add(this);
            }
        }

        private void OnDisable()
        {
            Active.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Active.Clear();
        }
    }
}
