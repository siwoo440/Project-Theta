using UnityEngine;
using ProjectTheta.Hypnosis;
using ProjectTheta.Stage;

namespace ProjectTheta.UI
{
    public sealed class PrototypeHud : MonoBehaviour
    {
        [SerializeField] private HypnosisCaster _caster; // 시전자 참조

        public void Configure(HypnosisCaster caster)
        {
            _caster = caster; // 시전자 설정
        }

        private void OnGUI()
        {
            StageGoalManager goal = StageGoalManager.Instance; // 목표 참조
            GUI.Box(new Rect(12f, 12f, 300f, 136f), "Project θ Prototype"); // HUD 박스
            GUI.Label(new Rect(24f, 42f, 280f, 22f), "이동: A/D 또는 ←/→   대시: Shift/Space"); // 이동 안내
            GUI.Label(new Rect(24f, 64f, 280f, 22f), "최면: E 또는 마우스 왼쪽 버튼"); // 최면 안내
            if (goal != null) // 목표 확인
            {
                GUI.Label(new Rect(24f, 86f, 280f, 22f), $"정기: {goal.CurrentEssence} / {goal.TargetEssence}"); // 정기 표시
                GUI.Label(new Rect(24f, 108f, 280f, 22f), $"시간: {goal.TimeRemaining:0.0}초"); // 시간 표시
            }

            if (_caster != null) // 시전자 확인
            {
                GUI.Label(new Rect(24f, 130f, 280f, 22f), $"동행: {_caster.FollowersCount}명"); // 동행 표시
                DrawTargetGauge(); // 대상 게이지 표시
            }

            DrawResult(goal); // 결과 표시
        }

        private void DrawTargetGauge()
        {
            if (_caster.CurrentTarget == null) // 대상 확인
            {
                return; // 게이지 숨김
            }

            float value = _caster.CurrentTarget.HypnosisNormalized; // 최면 비율
            GUI.Box(new Rect(12f, 158f, 300f, 48f), "최면 게이지"); // 게이지 배경
            GUI.HorizontalSlider(new Rect(24f, 184f, 276f, 18f), value, 0f, 1f); // 게이지 표시
        }

        private void DrawResult(StageGoalManager goal)
        {
            if (goal == null || !goal.IsFinished) // 종료 확인
            {
                return; // 결과 숨김
            }

            string text = goal.IsCleared ? "CLEAR" : "FAILED"; // 결과 문구
            GUI.Box(new Rect((Screen.width * 0.5f) - 100f, 40f, 200f, 60f), text); // 결과 표시
        }
    }
}
