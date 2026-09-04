using UnityEngine;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private void OnGUI()
        {
            GUI.Box(
                new Rect(12f, 12f, 340f, 118f),
                "Project θ - Day 02");

            GUI.Label(
                new Rect(24f, 42f, 320f, 22f),
                "이동: W/A/S/D 또는 방향키");

            GUI.Label(
                new Rect(24f, 64f, 320f, 22f),
                "대시: Left Shift 또는 Space");

            GUI.Label(
                new Rect(24f, 86f, 320f, 22f),
                "목표: 학교 복도 이동·충돌·깊이 정렬 테스트");

            GUI.Label(
                new Rect(24f, 108f, 320f, 22f),
                "분홍색 NPC는 2일차 정렬 확인용 임시 오브젝트");
        }
    }
}
