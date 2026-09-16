using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 호루라기 경보를 들은 중립 NPC의 경계 상태다 (23일차).
    /// 경계 중에는 최면 속도가 절반이 되고, 머리 위에 "경계!"가 뜬다.
    /// </summary>
    public sealed class WhistleAlarm : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private float _remaining;
        private Text _label;

        public bool IsAlarmed =>
            _remaining > 0f;

        /// <summary>최면 속도에 곱한다. 경계가 끝났으면 1이다.</summary>
        public float HypnosisMultiplier =>
            BeachMarketAbilityValues.GetAlarmMultiplier(
                _remaining);

        public static void Apply(
            Component target,
            float seconds)
        {
            if (target == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            WhistleAlarm alarm =
                target.GetComponent<WhistleAlarm>();

            if (alarm == null)
            {
                alarm =
                    target.gameObject.AddComponent<WhistleAlarm>();
            }

            alarm.Begin(seconds);
        }

        private void Begin(
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
                        "WhistleAlarmLabel",
                        new Vector2(0f, LabelHeight),
                        20,
                        new Color(1.00f, 0.55f, 0.40f));
            }

            _label.text = "경계!";
        }

        private void Update()
        {
            if (_remaining <= 0f ||
                GameplayPause.IsPaused)
            {
                return;
            }

            _remaining -= Time.deltaTime;

            if (_remaining <= 0f &&
                _label != null)
            {
                _label.text = string.Empty;
            }
        }
    }
}
