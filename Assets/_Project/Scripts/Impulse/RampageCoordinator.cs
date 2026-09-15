using UnityEngine;

namespace ProjectTheta.Impulse
{
    public sealed class RampageCoordinator : MonoBehaviour
    {
        private ImpulseMeter _activeMeter;

        /// <summary>
        /// 폭주를 붙잡히지 않고 넘겼을 때 알린다.
        /// 점수 집계기가 구독해 튜토리얼 마지막 단계와 경험치로 넘긴다.
        /// </summary>
        public event System.Action RampageSurvived;

        /// <summary>폭주한 NPC가 플레이어를 붙잡지 못하고 폭주 시간이 끝났을 때 호출한다.</summary>
        public void NotifySurvived(
            ImpulseMeter meter)
        {
            if (meter == null)
            {
                return;
            }

            RampageSurvived?.Invoke();
        }

        public ImpulseMeter ActiveMeter =>
            _activeMeter;

        public string ActiveMeterName =>
            _activeMeter == null
                ? "-"
                : _activeMeter.name;

        public bool TryBegin(
            ImpulseMeter meter)
        {
            if (meter == null)
            {
                return false;
            }

            if (_activeMeter != null &&
                _activeMeter != meter)
            {
                return false;
            }

            _activeMeter = meter;

            return true;
        }

        public void End(
            ImpulseMeter meter)
        {
            if (_activeMeter == meter)
            {
                _activeMeter = null;
            }
        }

        private void OnDisable()
        {
            _activeMeter = null;
        }
    }
}
