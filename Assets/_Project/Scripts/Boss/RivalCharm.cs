using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Companion;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Hypnosis;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Boss
{
    /// <summary>
    /// 라이벌의 매혹 게이지다 (26일차). 동행자에게 붙는다.
    /// 가득 차면 그 동행자는 라이벌 편이 된다. 천천히 빠지고, 플레이어 최면 파동을 쓰면 모두 지워진다.
    /// </summary>
    [RequireComponent(typeof(FollowerController))]
    public sealed class RivalCharm : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private static readonly List<RivalCharm> Active =
            new List<RivalCharm>();

        private FollowerController _follower;
        private FollowerManager _manager;
        private Text _label;
        private float _gauge;

        public float Gauge =>
            _gauge;

        public static void Add(
            FollowerController follower,
            FollowerManager manager,
            float amount)
        {
            if (follower == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            RivalCharm charm = follower.GetComponent<RivalCharm>();

            if (charm == null)
            {
                charm = follower.gameObject.AddComponent<RivalCharm>();
            }

            charm._manager = manager;
            charm._gauge = BossSkillValues.AddCharm(charm._gauge, amount);
        }

        /// <summary>플레이어 최면 파동이 모든 매혹을 지운다.</summary>
        public static int ClearAll()
        {
            int cleared = 0;

            for (int i = 0;
                 i < Active.Count;
                 i++)
            {
                if (Active[i] != null &&
                    Active[i]._gauge > 0f)
                {
                    Active[i]._gauge = 0f;
                    cleared++;
                }
            }

            return cleared;
        }

        private void Awake()
        {
            _follower = GetComponent<FollowerController>();
        }

        private void Update()
        {
            if (GameplayPause.IsPaused)
            {
                return;
            }

            if (_gauge > 0f &&
                BossSkillValues.IsCharmed(_gauge))
            {
                Charm();
            }

            _gauge = BossSkillValues.DecayCharm(_gauge, Time.deltaTime);

            RefreshLabel();
        }

        private void Charm()
        {
            _gauge = 0f;

            Vector2 position = transform.position;

            if (_manager != null)
            {
                _manager.RequestRelease(_follower);
            }

            HypnosisTarget target = GetComponent<HypnosisTarget>();

            if (target != null)
            {
                target.ClaimByRival();
            }

            StageMoments.RaiseFollowerStolen(position);

            GameVfx.FloatText(
                "라이벌에게 매혹됐다!",
                position + new Vector2(0f, 1.9f),
                new Color(0.80f, 0.40f, 1.00f),
                UiTheme.FontBody);
        }

        private void RefreshLabel()
        {
            if (_gauge <= 0f)
            {
                if (_label != null &&
                    _label.text.Length > 0)
                {
                    _label.text = string.Empty;
                }

                return;
            }

            if (_label == null)
            {
                _label =
                    WorldLabel.Create(
                        transform,
                        "CharmLabel",
                        new Vector2(0f, LabelHeight),
                        20,
                        new Color(0.80f, 0.40f, 1.00f));
            }

            _label.text = $"매혹 {Mathf.RoundToInt(_gauge)}%";
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
