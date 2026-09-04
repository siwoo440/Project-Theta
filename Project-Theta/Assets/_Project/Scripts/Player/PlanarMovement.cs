using UnityEngine;

namespace ProjectTheta.Player
{
    public static class PlanarMovement
    {
        public static Vector2 CalculateVelocity(Vector2 input, float speed)
        {
            Vector2 direction = input.sqrMagnitude > 1f ? input.normalized : input; // 입력 크기 제한
            return direction * Mathf.Max(0f, speed); // 속도 계산
        }

        public static Vector2 ClampPosition(Vector2 position, float minX, float maxX, float minY, float maxY)
        {
            float x = Mathf.Clamp(position.x, minX, maxX); // 가로 위치 제한
            float y = Mathf.Clamp(position.y, minY, maxY); // 세로 위치 제한
            return new Vector2(x, y); // 제한 위치 반환
        }

        public static Vector2 ResolveDashDirection(Vector2 input, Vector2 lastDirection)
        {
            Vector2 source = input.sqrMagnitude > 0.0001f ? input : lastDirection; // 대시 기준 선택
            if (source.sqrMagnitude <= 0.0001f) // 방향 유효성 확인
            {
                source = Vector2.right; // 기본 방향 설정
            }

            return source.normalized; // 정규 방향 반환
        }
    }
}
