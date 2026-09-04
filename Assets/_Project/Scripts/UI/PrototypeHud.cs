using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.NPC;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private HypnosisCaster _caster;
        private FollowerManager _followers;

        public void Configure(
            HypnosisCaster caster)
        {
            _caster = caster;

            _followers =
                caster == null
                    ? null
                    : caster.FollowerManager;
        }

        private void OnGUI()
        {
            GUI.Box(
                new Rect(
                    12f,
                    12f,
                    390f,
                    232f),
                "Project θ - Day 04");

            GUI.Label(
                new Rect(
                    24f,
                    42f,
                    360f,
                    22f),
                "이동: W/A/S/D 또는 방향키");

            GUI.Label(
                new Rect(
                    24f,
                    64f,
                    360f,
                    22f),
                "대시: Left Shift / Space");

            GUI.Label(
                new Rect(
                    24f,
                    86f,
                    360f,
                    22f),
                "최면: E 또는 마우스 왼쪽 버튼 유지");

            GUI.Label(
                new Rect(
                    24f,
                    108f,
                    360f,
                    22f),
                "NPC: Idle / Move / Alert / Following");

            DrawFollowerStatus();
            DrawHypnosisStatus();
        }

        private void DrawFollowerStatus()
        {
            if (_followers == null)
            {
                GUI.Label(
                    new Rect(
                        24f,
                        134f,
                        360f,
                        22f),
                    "동행 인원: -");

                return;
            }

            GUI.Label(
                new Rect(
                    24f,
                    134f,
                    360f,
                    22f),
                $"동행 인원: {_followers.Count}");

            if (_followers.Count > 0)
            {
                GUI.Label(
                    new Rect(
                        220f,
                        134f,
                        170f,
                        22f),
                    $"최저 유지도: {_followers.LowestStabilityNormalized * 100f:0}%");
            }
        }

        private void DrawHypnosisStatus()
        {
            if (_caster == null ||
                _caster.CurrentTarget == null)
            {
                GUI.Label(
                    new Rect(
                        24f,
                        162f,
                        360f,
                        22f),
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
                new Rect(
                    24f,
                    162f,
                    360f,
                    22f),
                $"최면 대상: {target.name} / 상태: {stateText}");

            GUI.HorizontalSlider(
                new Rect(
                    24f,
                    192f,
                    340f,
                    18f),
                target.HypnosisNormalized,
                0f,
                1f);

            GUI.Label(
                new Rect(
                    300f,
                    192f,
                    80f,
                    22f),
                $"{target.HypnosisNormalized * 100f:0}%");
        }
    }
}
