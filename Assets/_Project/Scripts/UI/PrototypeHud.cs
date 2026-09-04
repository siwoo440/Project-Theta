using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private HypnosisCaster _caster;

        public void Configure(
            HypnosisCaster caster)
        {
            _caster = caster;
        }

        private void OnGUI()
        {
            GUI.Box(
                new Rect(12f, 12f, 370f, 186f),
                "Project θ - Day 03");

            GUI.Label(
                new Rect(24f, 42f, 345f, 22f),
                "이동: W/A/S/D 또는 방향키");

            GUI.Label(
                new Rect(24f, 64f, 345f, 22f),
                "대시: Left Shift / Space");

            GUI.Label(
                new Rect(24f, 86f, 345f, 22f),
                "최면: E 또는 마우스 왼쪽 버튼 유지");

            GUI.Label(
                new Rect(24f, 108f, 345f, 22f),
                "NPC: Idle / Move / Alert");

            DrawHypnosisStatus();
        }

        private void DrawHypnosisStatus()
        {
            if (_caster == null ||
                _caster.CurrentTarget == null)
            {
                GUI.Label(
                    new Rect(24f, 136f, 345f, 22f),
                    "최면 대상: 없음");

                return;
            }

            HypnosisTarget target =
                _caster.CurrentTarget;

            NpcAgent agent =
                target.GetComponent<NpcAgent>();

            string stateText =
                agent == null
                    ? "-"
                    : agent.State.ToString();

            GUI.Label(
                new Rect(24f, 136f, 345f, 22f),
                $"최면 대상: {target.name} / 상태: {stateText}");

            GUI.HorizontalSlider(
                new Rect(24f, 164f, 330f, 18f),
                target.HypnosisNormalized,
                0f,
                1f);

            GUI.Label(
                new Rect(286f, 164f, 80f, 22f),
                $"{target.HypnosisNormalized * 100f:0}%");
        }
    }
}
