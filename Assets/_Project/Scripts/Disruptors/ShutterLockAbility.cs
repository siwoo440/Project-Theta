using System.Collections.Generic;
using UnityEngine;
using ProjectTheta.Stage;
using ProjectTheta.Stage.Locations;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 보안팀장의 <b>셔터 봉쇄</b>다 (부록 C.3 [5]).
    ///
    /// 구역 경계도가 경계(60) 이상일 때 1.5초 경광등 예고 뒤, 플레이어가 있는 층의 셔터 2곳을 12초 닫는다 (재사용 30초).
    /// 대처: 경계도를 60 아래로 유지, 예고 중에 셔터 밖으로 빠져나가기, 파동으로 팀장을 멍하게 하기.
    /// </summary>
    public sealed class ShutterLockAbility : SpecialAbility
    {
        private readonly List<ShutterGate> _shutters =
            new List<ShutterGate>();

        private Transform _player;
        private int _targetFloor;

        public override string DisplayName =>
            "셔터 봉쇄";

        public void Configure(
            Transform player)
        {
            _player = player;
        }

        protected override bool WantsToStart()
        {
            ZoneAlert alert = ZoneAlert.Current;

            return _player != null &&
                   alert != null &&
                   ShutterLogic.CanTrigger(alert.Level) &&
                   ShutterGate.CopyOnFloor(
                       FloorSpace.FloorAt(_player.position.y),
                       _shutters) > 0;
        }

        protected override void OnTelegraph()
        {
            _targetFloor =
                _player == null
                    ? Body.Floor
                    : FloorSpace.FloorAt(_player.position.y);

            ShutterGate.CopyOnFloor(_targetFloor, _shutters);

            for (int i = 0;
                 i < _shutters.Count;
                 i++)
            {
                _shutters[i].Warn(TelegraphSeconds);
            }
        }

        protected override void Fire()
        {
            ShutterGate.CopyOnFloor(_targetFloor, _shutters);

            for (int i = 0;
                 i < _shutters.Count;
                 i++)
            {
                _shutters[i].Close(ShutterLogic.CloseSeconds);
            }

            if (_shutters.Count > 0)
            {
                StageMoments.RaiseAbilityFired(
                    _shutters[0].transform.position,
                    DisplayName);
            }
        }
    }
}
