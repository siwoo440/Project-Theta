using UnityEngine;

namespace ProjectTheta.Impulse
{
    public sealed class RampageCoordinator : MonoBehaviour
    {
        private ImpulseMeter _activeMeter;

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
