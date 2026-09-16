using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTheta.Disruptors
{
    /// <summary>
    /// 소매치기에게 정기를 털린 동행자의 표식이다 (23일차).
    /// 표식이 붙은 채 회수하면 정기가 70%만 들어온다. 소매치기를 잡으면 모두 풀린다.
    /// </summary>
    public sealed class PickpocketMark : MonoBehaviour
    {
        private const float LabelHeight = 2.45f;

        private static readonly List<PickpocketMark> Marked =
            new List<PickpocketMark>();

        private Text _label;

        public bool IsMarked { get; private set; }

        public static void Apply(
            Component follower)
        {
            if (follower == null)
            {
                return;
            }

            // GetComponent는 에디터에서 "가짜 null"을 돌려줄 수 있어 ?? 대신 명시적으로 확인한다.
            PickpocketMark mark =
                follower.GetComponent<PickpocketMark>();

            if (mark == null)
            {
                mark =
                    follower.gameObject.AddComponent<PickpocketMark>();
            }

            mark.SetMarked(true);
        }

        /// <summary>소매치기를 잡았을 때 모든 표식을 푼다.</summary>
        public static void ClearAll()
        {
            for (int i = Marked.Count - 1;
                 i >= 0;
                 i--)
            {
                if (Marked[i] != null)
                {
                    Marked[i].SetMarked(false);
                }
            }

            Marked.Clear();
        }

        public static float GetEssenceMultiplier(
            Component follower)
        {
            if (follower == null)
            {
                return 1f;
            }

            PickpocketMark mark =
                follower.GetComponent<PickpocketMark>();

            return mark != null &&
                   mark.IsMarked
                ? PickpocketLogic.RemainingMultiplier
                : 1f;
        }

        private void SetMarked(
            bool marked)
        {
            IsMarked = marked;

            if (marked)
            {
                if (!Marked.Contains(this))
                {
                    Marked.Add(this);
                }

                if (_label == null)
                {
                    _label =
                        WorldLabel.Create(
                            transform,
                            "PickpocketLabel",
                            new Vector2(0f, LabelHeight),
                            18,
                            new Color(0.80f, 0.80f, 0.90f));
                }

                _label.text =
                    $"털림 -{Mathf.RoundToInt(PickpocketLogic.StealRatio * 100f)}%";

                return;
            }

            Marked.Remove(this);

            if (_label != null)
            {
                _label.text = string.Empty;
            }
        }

        private void OnDestroy()
        {
            Marked.Remove(this);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnPlayModeEnter()
        {
            Marked.Clear();
        }
    }
}
