using UnityEngine;
using ProjectTheta.Core;
using ProjectTheta.Disruptors;
using ProjectTheta.Presentation;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 지하철 열차 도착이다 (24일차, 부록 B.3 [2]).
    ///
    /// 45초마다 4초 예고 뒤 문이 열리고, 층마다 휴대폰만 보는 행인이 3~6명 쏟아진다.
    /// 문이 열린 5초 동안은 인파에 섞여 개찰구 검사가 생략된다.
    /// 도착한 열차 수는 생존 목표에 쓴다.
    /// </summary>
    public sealed class TrainArrival : MonoBehaviour
    {
        private static TrainArrival _current;

        private StageSessionController _stage;
        private int _floorCount;
        private float _elapsed;
        private int _spawnedTrains;
        private bool _wasArriving;

        public static TrainArrival Current =>
            _current;

        public int TrainsArrived =>
            TrainLogic.GetArrivedCount(_elapsed);

        public bool IsDoorOpen =>
            TrainLogic.IsDoorOpen(_elapsed);

        public bool IsArriving =>
            TrainLogic.IsArriving(_elapsed);

        public float SecondsUntilNext =>
            TrainLogic.SecondsUntilNext(_elapsed);

        /// <summary>문이 열린 동안은 인파에 섞여 개찰구를 그냥 지난다.</summary>
        public static bool IsCrowdRush =>
            _current != null &&
            _current.IsDoorOpen;

        public void Configure(
            StageSessionController stage,
            int floorCount)
        {
            _stage = stage;
            _floorCount = Mathf.Max(1, floorCount);
            _current = this;

            for (int floor = 0;
                 floor < _floorCount;
                 floor++)
            {
                LocationProps.Box(
                    transform,
                    "Platform",
                    FloorSpace.ToWorld(floor, new Vector2(0f, FloorSpace.WalkMaxY + 0.4f)),
                    new Vector2(FloorSpace.WalkMaxX - FloorSpace.WalkMinX, 0.5f),
                    new Color(0.85f, 0.75f, 0.25f, 0.55f),
                    -46);

                LocationProps.Sign(
                    transform,
                    floor,
                    new Vector2(-9f, FloorSpace.WalkMaxY + 1.3f),
                    floor == 0 ? "1호선 승강장" : "2호선 승강장",
                    new Color(0.60f, 0.85f, 0.60f));
            }
        }

        private void OnDestroy()
        {
            if (_current == this)
            {
                _current = null;
            }
        }

        /// <summary>디버그 치트: 바로 다음 열차를 도착시킨다.</summary>
        public void DebugArriveNow()
        {
            _elapsed =
                (TrainsArrived + 1) *
                TrainLogic.PeriodSeconds;
        }

        private void Update()
        {
            if (GameplayPause.IsPaused ||
                (_stage != null &&
                 !_stage.IsRunning))
            {
                return;
            }

            _elapsed += Time.deltaTime;

            bool arriving = IsArriving;

            if (arriving &&
                !_wasArriving)
            {
                GameVfx.FloatText(
                    "열차가 들어옵니다",
                    FloorSpace.ToWorld(0, new Vector2(0f, FloorSpace.WalkMaxY + 2f)),
                    new Color(0.60f, 0.85f, 0.60f),
                    UI.Framework.UiTheme.FontBody);
            }

            _wasArriving = arriving;

            while (_spawnedTrains < TrainsArrived)
            {
                ReleaseCommuters(_spawnedTrains);

                _spawnedTrains++;

                StageMoments.RaiseTrainArrived(
                    _spawnedTrains);
            }
        }

        /// <summary>층마다 양쪽 끝에서 행인을 내보낸다. 절반은 왼쪽으로, 절반은 오른쪽으로 걷는다.</summary>
        private void ReleaseCommuters(
            int trainIndex)
        {
            DisruptorSpawner spawner =
                DisruptorSpawner.Current;

            if (spawner == null)
            {
                return;
            }

            int requested =
                TrainLogic.GetCommuterCount(trainIndex);

            for (int floor = 0;
                 floor < _floorCount;
                 floor++)
            {
                // 24일차: 층당 인원 제한 안에서만 내린다.
                int count =
                    Mathf.Min(
                        requested,
                        spawner.GetFreeSlots(floor));

                for (int i = 0;
                     i < count;
                     i++)
                {
                    int direction =
                        i % 2 == 0
                            ? 1
                            : -1;

                    // 오른쪽으로 걷는 사람은 왼쪽 끝 근처에서, 왼쪽으로 걷는 사람은 오른쪽 끝 근처에서 내린다.
                    float x =
                        direction > 0
                            ? -11f + i * 0.9f
                            : 11f - i * 0.9f;

                    spawner.Spawn(
                        DisruptorKind.PhoneCommuter,
                        floor,
                        x,
                        direction);
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            _current = null;
        }
    }
}
