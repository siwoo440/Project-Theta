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

            return follower.GetComponent<AthleteMark>() != null
                ? CityAbilityValues.AthleteBonusEssence
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

            _label.text = "★ 대회 앞둔 선수";
        }
    }
}
