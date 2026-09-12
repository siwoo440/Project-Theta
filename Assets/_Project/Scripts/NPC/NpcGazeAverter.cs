using UnityEngine;

namespace ProjectTheta.NPC
{
    /// <summary>
    /// 시선 회피형 특성을 가진 NPC에만 붙는다.
    /// 차단 구간 동안 <see cref="IsBlocking"/>이 true가 되고, 최면 게이지 상승이 멈춘다.
    /// </summary>
    public sealed class NpcGazeAverter : MonoBehaviour
    {
        [SerializeField] private float _cycleDuration =
            NpcGazeAverterLogic.DefaultCycleDuration;

        [SerializeField] private float _blockDuration =
            NpcGazeAverterLogic.DefaultBlockDuration;

        private float _elapsed;

        public bool IsBlocking { get; private set; }

        private void Awake()
        {
            // 같은 프레임에 모든 회피형이 동시에 고개를 돌리지 않도록 시작 위상을 흩뿌린다.
            _elapsed =
                Random.Range(
                    0f,
                    Mathf.Max(
                        0.01f,
                        _cycleDuration));
        }

        private void Update()
        {
            _elapsed +=
                Time.deltaTime;

            IsBlocking =
                NpcGazeAverterLogic.IsBlocking(
                    _elapsed,
                    _cycleDuration,
                    _blockDuration);
        }

        private void OnDisable()
        {
            IsBlocking =
                false;
        }
    }
}
