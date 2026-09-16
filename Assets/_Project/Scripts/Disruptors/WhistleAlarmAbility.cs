using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Presentation;
using ProjectTheta.Stage;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 라이프가드 반장의 <b>호루라기 경보</b>다 (부록 C.3 [1]).
    ///
    /// 반장이 플레이어를 발각하면 1초 예고 뒤 호루라기를 분다.
    ///   반경 10m 안의 중립 NPC가 8초 동안 경계 상태(최면 속도 -50%)가 된다.
    ///   구역 경계도 +25.
    /// 대처: 망루 뒤편(시야 사각)에서 활동, 파동으로 반장을 멍하게 하기, 경보 후에는 반경 밖 NPC 공략.
    /// </summary>
    public sealed class WhistleAlarmAbility : SpecialAbility
    {
        private readonly List<HypnosisTarget> _targetBuffer =
            new List<HypnosisTarget>();

        private Transform _player;
        private WatcherRole _watcher;

        public override string DisplayName =>
            "호루라기 경보";

        public void Configure(
            Transform player)
        {
            _player = player;
            _watcher = GetComponent<WatcherRole>();
        }

        protected override bool WantsToStart()
        {
            return _watcher != null &&
                   _watcher.IsSpotting;
        }

        protected override void OnTelegraph()
        {
            if (_player != null)
            {
                Body.FaceToward(
                    _player.position.x);
            }
        }

        protected override void Fire()
        {
            Vector2 self = transform.position;

            HypnosisTarget.CopyActive(
                _targetBuffer);

            int alarmed = 0;

            for (int i = 0;
                 i < _targetBuffer.Count;
                 i++)
            {
                HypnosisTarget target = _targetBuffer[i];

                if (target == null ||
                    !target.isActiveAndEnabled ||
                    target.Owner != NpcOwner.Neutral)
                {
                    continue;
                }

                Vector2 position = target.transform.position;

                if (FloorSpace.FloorAt(position.y) != Body.Floor ||
                    Vector2.Distance(self, position) > BeachMarketAbilityValues.WhistleRadius)
                {
                    continue;
                }

                WhistleAlarm.Apply(
                    target,
                    BeachMarketAbilityValues.WhistleAlarmSeconds);

                alarmed++;
            }

            if (ZoneAlert.Current != null)
            {
                ZoneAlert.Current.Add(
                    BeachMarketAbilityValues.WhistleAlertRise,
                    _player == null
                        ? self
                        : (Vector2)_player.position);
            }

            StageMoments.RaiseAbilityFired(
                self,
                DisplayName);

            GameVfx.Ripple(
                self + new Vector2(0f, 1f),
                new Color(1.00f, 0.45f, 0.40f),
                BeachMarketAbilityValues.WhistleRadius,
                0.7f);

            if (alarmed > 0)
            {
                GameVfx.FloatText(
                    $"주변 {alarmed}명 경계!",
                    self + new Vector2(0f, 2.9f),
                    new Color(1.00f, 0.70f, 0.40f),
                    UI.Framework.UiTheme.FontSmall,
                    1.2f);
            }
        }
    }
}
