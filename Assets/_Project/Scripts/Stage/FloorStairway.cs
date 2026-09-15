using UnityEngine;

namespace ProjectTheta.Stage
{
    /// <summary>
    /// 벽면 계단 하나다.
    ///
    /// 실제 이동은 <see cref="FloorTransitionController"/>가 처리하고,
    /// 이 컴포넌트는 "여기 서면 어느 층으로 갈 수 있는가"만 들고 있다.
    /// </summary>
    public sealed class FloorStairway : MonoBehaviour
    {
        [SerializeField] private FloorStairDirection _direction =
            FloorStairDirection.Up;

        [SerializeField] private int _sourceFloor;

        [SerializeField] private int _targetFloor = 1;

        /// <summary>이 거리 안에 들어오면 계단을 쓸 수 있다.</summary>
        [SerializeField] private float _interactRadius = 2.1f;

        public FloorStairDirection Direction =>
            _direction;

        public int SourceFloor =>
            _sourceFloor;

        public int TargetFloor =>
            _targetFloor;

        public float InteractRadius =>
            _interactRadius;

        public void Configure(
            FloorStairDirection direction,
            int sourceFloor,
            int targetFloor)
        {
            _direction = direction;
            _sourceFloor = sourceFloor;
            _targetFloor = targetFloor;
        }

        /// <summary>플레이어가 계단 앞에 서 있는지 본다.</summary>
        public bool IsWithinRange(
            Vector2 position)
        {
            Vector2 self =
                transform.position;

            return Vector2.Distance(
                       self,
                       position) <=
                   _interactRadius;
        }

        /// <summary>
        /// 도착 지점이다. 올라왔으면 그 층의 아래층 계단 앞에,
        /// 내려왔으면 위층 계단 앞에 선다. 왕복이 자연스럽게 이어진다.
        /// </summary>
        public Vector2 GetArrivalPosition()
        {
            float x =
                _direction == FloorStairDirection.Up
                    ? FloorLayout.DownStairX
                    : FloorLayout.UpStairX;

            return FloorSpace.ToWorld(
                _targetFloor,
                new Vector2(
                    x,
                    FloorLayout.StairStandY));
        }

        public string GetPromptText()
        {
            return _direction == FloorStairDirection.Up
                ? $"[W] {FloorPlanLogic.GetLabel(_targetFloor)}로 올라가기"
                : $"[S] {FloorPlanLogic.GetLabel(_targetFloor)}로 내려가기";
        }
    }
}
