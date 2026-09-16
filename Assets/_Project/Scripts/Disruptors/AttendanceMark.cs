using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Presentation;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 근태 체크 표식이다 (22일차). 인사팀 평가관의 능력이 동행자에게 붙인다.
    ///
    /// 표식이 붙은 동안
    ///   · 유지도가 초당 <see cref="DrainPerSecond"/>씩 줄고, 멀어져서 줄어드는 속도도 두 배다
    ///   · 이 동행자를 회수하면 정기가 <see cref="EssencePenalty"/>만큼 깎인다
    /// 원래 유지도는 가까이 있으면 회복만 해서, "두 배로 줄어든다"만으로는 가까이 둔 동행자에게 아무 일도 없다.
    /// 그래서 가까이 있어도 천천히 줄어드는 몫을 더했다. 10초면 40이 준다(최대 100).
    /// </summary>
    public sealed class AttendanceMark : MonoBehaviour
    {
        public const float DecayMultiplier = 2f;
        public const float DrainPerSecond = 4f;
        public const float EssencePenalty = 0.2f;

        private float _remaining;
        private Text _label;

        public bool IsMarked =>
            _remaining > 0f;

        public void Apply(
            float seconds)
        {
            _remaining =
                Mathf.Max(
                    _remaining,
                    seconds);

            if (_label == null)
            {
                _label =
                    WorldLabel.Create(
                        transform,
                        "AttendanceMark",
                        new Vector2(0f, 2.1f),
                        22,
                        UiTheme.Danger);
            }

            GameAudio.Play(
                GameSfx.UiStamp,
                0.7f);
        }

        /// <summary>회수 시 정기에 곱하는 값이다. 표식이 없으면 1이다.</summary>
        public static float GetEssenceMultiplier(
            Component follower)
        {
            if (follower == null)
            {
                return 1f;
            }

            AttendanceMark mark =
                follower.GetComponent<AttendanceMark>();

            return mark != null &&
                   mark.IsMarked
                ? 1f - EssencePenalty
                : 1f;
        }

        private void Update()
        {
            if (_remaining <= 0f)
            {
                return;
            }

            _remaining -=
                Time.deltaTime;

            if (_label != null)
            {
                _label.text =
                    _remaining > 0f
                        ? $"근태 체크 {Mathf.CeilToInt(_remaining)}"
                        : string.Empty;
            }
        }
    }
}
