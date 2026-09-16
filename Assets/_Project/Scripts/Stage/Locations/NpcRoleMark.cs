using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Disruptors;

namespace ProjectTheta.Stage.Locations
{
    /// <summary>NPC가 장소에서 맡은 신분이다 (25일차).</summary>
    public enum NpcRole
    {
        Employee = 0,
        Visitor = 1,
        PassHolder = 2,
        SecurityRoom = 3,

        /// <summary>루프탑 클럽 VIP 게스트다 (26일차). 데리고 있으면 바운서가 막지 않는다.</summary>
        VipGuest = 4
    }

    /// <summary>
    /// 장소 NPC의 신분 표식이다 (25일차).
    ///   직원       비서실장의 긴급 회의 소집 대상
    ///   방문객     회의 소집에 끌려가지 않는다
    ///   출입증     데리고 있으면 그 층 출입증 게이트가 열린다 (직원이기도 하다)
    ///   보안실     최면하면 그 층 CCTV가 꺼진다
    /// </summary>
    public sealed class NpcRoleMark : MonoBehaviour
    {
        private const float LabelHeight = 2.45f;

        private Text _label;

        public NpcRole Role { get; private set; }

        public bool IsEmployee =>
            Role == NpcRole.Employee ||
            Role == NpcRole.PassHolder;

        public void Configure(
            NpcRole role)
        {
            Role = role;
        }

        public static NpcRoleMark Get(
            Component target)
        {
            return target == null
                ? null
                : target.GetComponent<NpcRoleMark>();
        }

        private void Start()
        {
            string text = GetLabel(Role);

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            _label =
                WorldLabel.Create(
                    transform,
                    "RoleLabel",
                    new Vector2(0f, LabelHeight),
                    18,
                    GetColor(Role));

            _label.text = text;
        }

        public static string GetLabel(
            NpcRole role)
        {
            switch (role)
            {
                case NpcRole.PassHolder:
                    return "[출입증]";

                case NpcRole.SecurityRoom:
                    return "[보안실]";

                case NpcRole.Visitor:
                    return "방문객";

                case NpcRole.VipGuest:
                    return "[VIP]";

                default:
                    return string.Empty;
            }
        }

        private static Color GetColor(
            NpcRole role)
        {
            switch (role)
            {
                case NpcRole.PassHolder:
                    return new Color(0.55f, 0.95f, 0.65f);

                case NpcRole.SecurityRoom:
                    return new Color(0.65f, 0.80f, 1.00f);

                case NpcRole.VipGuest:
                    return new Color(0.85f, 0.60f, 1.00f);

                default:
                    return new Color(0.80f, 0.80f, 0.85f, 0.8f);
            }
        }
    }
}
