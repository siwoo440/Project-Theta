using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Core;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 수영장 코치의 "전원 입수"로 물속에 들어간 NPC다 (24일차).
    /// 막힌 동안은 최면 게이지가 오르지 않는다.
    /// </summary>
    public sealed class HypnosisBlock : MonoBehaviour
    {
        private const float LabelHeight = 2.1f;

        private float _remaining;
        private Text _label;

        public bool IsBlocked =>
            _remaining > 0f;

        public static void Apply(
            Component target,
            float seconds)
        {
            if (target == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            HypnosisBlock block =
                target.GetComponent<HypnosisBlock>();

            if (block == null)
            {
                block =
                    target.gameObject.AddComponent<HypnosisBlock>();
            }

            block._remaining =
                Mathf.Max(
                    block._remaining,
                    seconds);

            if (block._label == null)
            {
                block._label =
                    WorldLabel.Create(
                        block.transform,
                        "DiveLabel",
                        new Vector2(0f, LabelHeight),
                        20,
                        new Color(0.45f, 0.75f, 1.00f));
            }

            block._label.text = "잠수 중";
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
