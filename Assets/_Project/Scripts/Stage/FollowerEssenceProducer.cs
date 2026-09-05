using UnityEngine;
using ProjectTheta.Hypnosis;

namespace ProjectTheta.Stage
{
    [RequireComponent(typeof(HypnosisTarget))]
    public sealed class FollowerEssenceProducer : MonoBehaviour
    {
        [SerializeField] private float _productionInterval = 1.0f;

        private HypnosisTarget _target;
        private StageSessionController _stage;
        private float _timer;

        private void Awake()
        {
            _target =
                GetComponent<HypnosisTarget>();
        }

        private void Start()
        {
            ResolveStage();
        }

        private void Update()
        {
            ResolveStage();

            if (_stage == null ||
                !_stage.IsRunning ||
                _target == null ||
                !_target.IsFollowing)
            {
                _timer = 0f;
                return;
            }

            float interval =
                Mathf.Max(
                    0.05f,
                    _productionInterval);

            _timer +=
                Time.deltaTime;

            while (_timer >= interval)
            {
                _timer -= interval;

                _stage.AddEssence(
                    _stage.PassiveEssencePerFollower);

                if (!_stage.IsRunning)
                {
                    break;
                }
            }
        }

        private void ResolveStage()
        {
            if (_stage == null)
            {
                _stage =
                    FindFirstObjectByType<
                        StageSessionController>();
            }
        }

        private void OnDisable()
        {
            _timer = 0f;
        }
    }
}
