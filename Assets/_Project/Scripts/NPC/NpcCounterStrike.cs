using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Ownership;
using ProjectTheta.Player;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 반격형 특성을 가진 NPC에만 붙는다.
    ///
    /// 최면당하는 동안 주기적으로 짧은 정신 충격을 일으켜
    /// 플레이어의 집중력을 깎고 최면 연결을 잠시 끊는다.
    /// 10일차에는 집중력 자원이 없어 보류했던 특성이다.
    /// </summary>
    public sealed class NpcCounterStrike : MonoBehaviour
    {
        [SerializeField] private float _intervalSeconds = 3.2f;
        [SerializeField] private float _focusDamage = 20f;
        [SerializeField] private float _interruptSeconds = 1.0f;

        private HypnosisTarget _target;
        private HypnosisCaster _caster;
        private PlayerFocus _focus;
        private float _chargeSeconds;

        /// <summary>다음 반격까지의 진행도다. UI 표시에 쓸 수 있다.</summary>
        public float ChargeNormalized =>
            Mathf.Clamp01(
                _chargeSeconds /
                Mathf.Max(
                    0.01f,
                    _intervalSeconds));

        private void Awake()
        {
            _target =
                GetComponent<HypnosisTarget>();
        }

        private void Update()
        {
            if (_target == null ||
                !_target.IsTargeted ||
                _target.Owner !=
                NpcOwner.Neutral)
            {
                _chargeSeconds =
                    0f;

                return;
            }

            _chargeSeconds +=
                Time.deltaTime;

            if (_chargeSeconds <
                Mathf.Max(
                    0.01f,
                    _intervalSeconds))
            {
                return;
            }

            _chargeSeconds =
                0f;

            Strike();
        }

        private void Strike()
        {
            ResolvePlayerReferences();

            _focus?.TrySpend(
                _focusDamage);

            _caster?.Interrupt(
                _interruptSeconds);
        }

        private void ResolvePlayerReferences()
        {
            if (_caster == null)
            {
                _caster =
                    FindFirstObjectByType<
                        HypnosisCaster>();
            }

            if (_focus == null &&
                _caster != null)
            {
                _focus =
                    _caster.GetComponent<
                        PlayerFocus>();
            }
        }

        private void OnDisable()
        {
            _chargeSeconds =
                0f;
        }
    }
}
