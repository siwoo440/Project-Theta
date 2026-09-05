using UnityEngine;
using ProjectTheta.Companion;
using ProjectTheta.Hypnosis;
using ProjectTheta.Impulse;
using ProjectTheta.NPC;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        private HypnosisCaster _caster;
        private FollowerManager _followers;
        private RampageCoordinator _coordinator;

        public void Configure(
            HypnosisCaster caster)
        {
            _caster = caster;

            _followers =
                caster == null
                    ? null
                    : caster.FollowerManager;

            _coordinator =
                caster == null
                    ? null
                    : caster.GetComponent<
                        RampageCoordinator>();
        }

        private void OnGUI()
        {
            GUI.Box(
                new Rect(
                    12f,
                    12f,
                    410f,
                    286f),
                "Project θ - Day 05");

            GUI.Label(
                new Rect(
                    24f,
                    42f,
                    380f,
                    22f),
                "이동: W/A/S/D 또는 방향키");

            GUI.Label(
                new Rect(
                    24f,
                    64f,
                    380f,
                    22f),
                "대시: Left Shift / Space");

            GUI.Label(
                new Rect(
                    24f,
                    86f,
                    380f,
                    22f),
                "최면: E 또는 마우스 왼쪽 버튼 유지");

            GUI.Label(
                new Rect(
                    24f,
                    108f,
                    380f,
                    22f),
                "폭주 경고: ! 아이콘 / 종료 후 하트 복귀");

            DrawFollowerStatus();
            DrawHypnosisStatus();
            DrawImpulseStatus();
        }

        private void DrawFollowerStatus()
        {
            if (_followers == null)
            {
                GUI.Label(
                    new Rect(
                        24f,
                        136f,
                        380f,
                        22f),
                    "동행 인원: -");

                return;
            }

            GUI.Label(
                new Rect(
                    24f,
                    136f,
                    380f,
                    22f),
                $"동행 인원: {_followers.Count}");

            if (_followers.Count > 0)
            {
                GUI.Label(
                    new Rect(
                        220f,
                        136f,
                        180f,
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
                        164f,
                        380f,
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
                    164f,
                    380f,
                    22f),
                $"최면 대상: {target.name} / 상태: {stateText}");

            GUI.HorizontalSlider(
                new Rect(
                    24f,
                    194f,
                    340f,
                    18f),
                target.HypnosisNormalized,
                0f,
                1f);

            GUI.Label(
                new Rect(
                    300f,
                    194f,
                    90f,
                    22f),
                $"{target.HypnosisNormalized * 100f:0}%");
        }

        private void DrawImpulseStatus()
        {
            ImpulseMeter[] meters =
                FindObjectsByType<ImpulseMeter>(
                    FindObjectsSortMode.None);

            float highest =
                0f;

            string highestName =
                "-";

            for (int i = 0;
                 i < meters.Length;
                 i++)
            {
                ImpulseMeter meter =
                    meters[i];

                if (meter == null ||
                    !meter.IsFollowingActive)
                {
                    continue;
                }

                if (meter.ImpulseNormalized <=
                    highest)
                {
                    continue;
                }

                highest =
                    meter.ImpulseNormalized;
                highestName = meter.name;
            }

            GUI.Label(
                new Rect(
                    24f,
                    226f,
                    380f,
                    22f),
                $"최고 충동: {highest * 100f:0}% / {highestName}");

            string activeRampage =
                _coordinator == null
                    ? "-"
                    : _coordinator.ActiveMeterName;

            GUI.Label(
                new Rect(
                    24f,
                    248f,
                    380f,
                    22f),
                $"현재 폭주 NPC: {activeRampage}");
        }
    }
}
