using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 모든 NPC가 공유하는 방황 성분이다.
    /// 중립 NPC의 제자리 서성임, 플레이어 동행 대열, 경쟁자 동행 대열이 모두 이 값을 사용한다.
    ///
    /// 개체마다 다른 위상·속도·진폭을 가지므로 무리가 커져도 한 덩어리로 뭉쳐 보이지 않는다.
    /// </summary>
    public sealed class NpcWanderMotion : MonoBehaviour
    {
        [SerializeField] private float _horizontalAmplitude =
            NpcWanderLogic.DefaultHorizontalAmplitude;

        [SerializeField] private float _verticalAmplitude =
            NpcWanderLogic.DefaultVerticalAmplitude;

        [SerializeField] private float _minimumSpeed =
            NpcWanderLogic.DefaultMinimumSpeed;

        [SerializeField] private float _maximumSpeed =
            NpcWanderLogic.DefaultMaximumSpeed;

        [SerializeField] private float _amplitudeVariation = 0.35f;

        private float _phase;
        private float _speed;
        private float _personalHorizontalAmplitude;
        private float _personalVerticalAmplitude;

        public float HorizontalAmplitude =>
            _personalHorizontalAmplitude;

        public float VerticalAmplitude =>
            _personalVerticalAmplitude;

        /// <summary>현재 프레임의 방황 오프셋이다.</summary>
        public Vector2 Offset =>
            GetOffset(
                1f);

        private void Awake()
        {
            Reseed();
        }

        /// <summary>
        /// 위상·속도·진폭을 새로 뽑는다.
        /// 소유권이 바뀌어 다른 무리로 옮겨갈 때 호출하면 대열이 다시 흐트러진다.
        /// </summary>
        public void Reseed()
        {
            _phase =
                Random.Range(
                    0f,
                    Mathf.PI * 2f);

            _speed =
                Random.Range(
                    Mathf.Min(
                        _minimumSpeed,
                        _maximumSpeed),
                    Mathf.Max(
                        _minimumSpeed,
                        _maximumSpeed));

            float variation =
                Mathf.Max(
                    0f,
                    _amplitudeVariation);

            _personalHorizontalAmplitude =
                _horizontalAmplitude *
                Random.Range(
                    1f - variation,
                    1f + variation);

            _personalVerticalAmplitude =
                _verticalAmplitude *
                Random.Range(
                    1f - variation,
                    1f + variation);
        }

        /// <summary>진폭 배율을 적용한 방황 오프셋을 얻는다.</summary>
        public Vector2 GetOffset(
            float amplitudeScale)
        {
            return NpcWanderLogic.GetOffset(
                Time.time,
                _phase,
                _speed,
                _personalHorizontalAmplitude *
                amplitudeScale,
                _personalVerticalAmplitude *
                amplitudeScale);
        }
    }
}
