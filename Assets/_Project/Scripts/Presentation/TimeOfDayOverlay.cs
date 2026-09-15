using UnityEngine;
using UnityEngine.UI;
using ProjectTheta.Stage.Locations;
using ProjectTheta.UI.Framework;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 장소 시간대에 맞춰 화면 전체에 색을 얇게 덮는다 (21일차).
    ///
    /// 낮은 그대로, 저녁은 주황빛, 밤은 남색으로 어둡게 한다.
    /// 캐릭터와 배경 위, HUD 아래에 깔리므로 글자는 어두워지지 않는다.
    /// 밤 장소의 등불 · 조명 영역은 23일차 야시장에서 이 위에 밝은 구멍을 내는 방식으로 붙인다.
    /// </summary>
    public static class TimeOfDayOverlay
    {
        /// <summary>월드 연출 캔버스(40)와 HUD(50)보다 아래다.</summary>
        public const int SortOrder = 30;

        public static Color GetOverlayColor(
            LocationTimeOfDay time)
        {
            switch (time)
            {
                case LocationTimeOfDay.Evening:
                    return new Color(1.00f, 0.42f, 0.22f, 0.13f);

                case LocationTimeOfDay.Night:
                    return new Color(0.05f, 0.07f, 0.28f, 0.34f);

                default:
                    return new Color(1f, 1f, 1f, 0f);
            }
        }

        public static Color GetCameraBackground(
            LocationTimeOfDay time)
        {
            switch (time)
            {
                case LocationTimeOfDay.Evening:
                    return new Color(0.22f, 0.15f, 0.16f);

                case LocationTimeOfDay.Night:
                    return new Color(0.05f, 0.06f, 0.12f);

                default:
                    return new Color(0.16f, 0.20f, 0.21f);
            }
        }

        public static void Create(
            LocationTimeOfDay time)
        {
            if (Camera.main != null)
            {
                Camera.main.backgroundColor =
                    GetCameraBackground(
                        time);
            }

            Color color =
                GetOverlayColor(
                    time);

            if (color.a <= 0f)
            {
                return;
            }

            Canvas canvas =
                UiFactory.CreateCanvas(
                    "TimeOfDayOverlay",
                    SortOrder);

            // 클릭을 막지 않게 레이캐스터를 뺀다.
            GraphicRaycaster raycaster =
                canvas.GetComponent<GraphicRaycaster>();

            if (raycaster != null)
            {
                Object.Destroy(
                    raycaster);
            }

            Image tint =
                UiFactory.CreateImage(
                    canvas.transform,
                    "Tint",
                    color);

            UiFactory.Stretch(
                tint.rectTransform);
        }
    }
}
