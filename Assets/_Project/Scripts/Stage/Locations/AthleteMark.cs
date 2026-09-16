using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Disruptors;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>
    /// 헬스장 특수 대상 "대회를 앞둔 선수"다 (24일차, 부록 B.3 [3]).
    /// 최면이 절반 속도로만 걸리지만, 회수하면 정기 보너스가 크게 붙는다.
    /// </summary>
    public sealed class AthleteMark : MonoBehaviour
    {
        private const float LabelHeight = 2.45f;

        private Text _label;

        /// <summary>머리 위 이름과 회수 보너스다. 오피스 대표 비서처럼 다른 특수 대상에도 쓴다 (25일차).</summary>
        private string _title = "★ 대회 앞둔 선수";
        private int _bonus = CityAbilityValues.AthleteBonusEssence;

        public void Configure(
            string title,
            int bonus)
        {
            _title = title;
            _bonus = bonus;

            if (_label != null)
            {
                _label.text = title;
            }
        }

        public static float GetHypnosisMultiplier(
            AthleteMark mark)
        {
            return mark == null
                ? 1f
                : CityAbilityValues.AthleteHypnosisMultiplier;
        }

        /// <summary>회수할 때 더해 줄 보너스다. 선수가 아니면 0이다.</summary>
        public static int GetBonusEssence(
            Component follower)
        {
            if (follower == null)
            {
                return 0;
            }

            AthleteMark mark = follower.GetComponent<AthleteMark>();

            return mark != null
                ? mark._bonus
                : 0;
        }

        private void Start()
        {
            _label =
                WorldLabel.Create(
                    transform,
                    "AthleteLabel",
                    new Vector2(0f, LabelHeight),
                    20,
                    UiTheme.Gold);

            _label.text = _title;
        }
    }
}
