using System;
using NUnit.Framework;
using UnityEngine;
using ProjectTheta.Balance;
using ProjectTheta.Presentation;
using ProjectTheta.Save;

namespace ProjectTheta.Tests.EditMode
{
    public sealed class TimeScaleLogicTests
    {
        [Test]
        public void Normal_Play_Runs_At_Full_Speed()
        {
            Assert.AreEqual(
                1f,
                TimeScaleLogic.Resolve(0, 0f, 0.05f),
                0.0001f);
        }

        [Test]
        public void Hit_Stop_Slows_Time()
        {
            Assert.AreEqual(
                0.05f,
                TimeScaleLogic.Resolve(0, 0.06f, 0.05f),
                0.0001f);
        }

        [Test]
        public void Full_Pause_Always_Wins_Over_Hit_Stop()
        {
            // 레벨업 멈칫 직후 카드 화면이 뜬다. 멈칫이 카드 화면을 풀면 안 된다.
            Assert.AreEqual(
                0f,
                TimeScaleLogic.Resolve(1, 0.06f, 0.05f),
                0.0001f);

            // 멈칫이 끝나도 카드 화면이 떠 있으면 계속 멈춰 있다.
            Assert.AreEqual(
                0f,
                TimeScaleLogic.Resolve(1, 0f, 0.05f),
                0.0001f);
        }

        [Test]
        public void Overlapping_Hit_Stops_Do_Not_Stack()
        {
            // 체인 최면처럼 연달아 걸려도 멈칫이 길게 늘어나지 않는다.
            float remaining =
                TimeScaleLogic.Extend(0.04f, 0.06f);

            Assert.AreEqual(0.06f, remaining, 0.0001f);

            Assert.AreEqual(
                0.06f,
                TimeScaleLogic.Extend(0.06f, 0.02f),
                0.0001f);
        }

        [Test]
        public void Hit_Stop_Runs_Out()
        {
            Assert.AreEqual(
                0f,
                TimeScaleLogic.Tick(0.06f, 0.1f),
                0.0001f);
        }

        [Test]
        public void Hit_Stop_Scale_Is_Clamped()
        {
            Assert.AreEqual(1f, TimeScaleLogic.Resolve(0, 1f, 5f), 0.0001f);
            Assert.AreEqual(0f, TimeScaleLogic.Resolve(0, 1f, -3f), 0.0001f);
        }
    }

    public sealed class VfxCurvesTests
    {
        private static readonly Func<float, float>[] FadeCurves =
        {
            VfxCurves.RippleAlpha,
            VfxCurves.PillarAlpha,
            VfxCurves.PopAlpha,
            VfxCurves.FloatAlpha,
            VfxCurves.FlightAlpha
        };

        [Test]
        public void Every_Fade_Curve_Ends_Exactly_At_Zero()
        {
            // 끝났는데 투명도가 조금 남으면 화면에 흔적이 쌓인다.
            for (int i = 0; i < FadeCurves.Length; i++)
            {
                Assert.AreEqual(
                    0f,
                    FadeCurves[i](1f),
                    0.0001f,
                    $"곡선 {i}");
            }
        }

        [Test]
        public void Every_Fade_Curve_Starts_Visible()
        {
            for (int i = 0; i < FadeCurves.Length; i++)
            {
                Assert.AreEqual(
                    1f,
                    FadeCurves[i](0f),
                    0.0001f,
                    $"곡선 {i}");
            }
        }

        [Test]
        public void Fade_Curves_Never_Brighten_Over_Time()
        {
            for (int i = 0; i < FadeCurves.Length; i++)
            {
                float previous = 1f;

                for (int step = 1; step <= 50; step++)
                {
                    float current =
                        FadeCurves[i](step / 50f);

                    Assert.LessOrEqual(
                        current,
                        previous + 0.0001f,
                        $"곡선 {i}, 단계 {step}");

                    previous = current;
                }
            }
        }

        [Test]
        public void Curves_Clamp_Out_Of_Range_Progress()
        {
            Assert.AreEqual(0f, VfxCurves.RippleAlpha(3f), 0.0001f);
            Assert.AreEqual(1f, VfxCurves.RippleAlpha(-2f), 0.0001f);
        }

        [Test]
        public void Ripple_Grows_To_Full_Size()
        {
            Assert.Less(VfxCurves.RippleScale(0f), 0.5f);
            Assert.AreEqual(1f, VfxCurves.RippleScale(1f), 0.0001f);
        }

        [Test]
        public void Pillar_Rises_Quickly_Then_Holds()
        {
            Assert.AreEqual(0f, VfxCurves.PillarHeight(0f), 0.0001f);
            Assert.AreEqual(1f, VfxCurves.PillarHeight(0.25f), 0.0001f);
            Assert.AreEqual(1f, VfxCurves.PillarHeight(0.9f), 0.0001f);
        }

        [Test]
        public void Pop_Overshoots_Then_Settles()
        {
            float peak = 0f;

            for (int step = 0; step <= 35; step++)
            {
                peak = Math.Max(
                    peak,
                    VfxCurves.PopScale(step / 100f));
            }

            Assert.Greater(peak, 1.02f);
            Assert.AreEqual(1f, VfxCurves.PopScale(0.5f), 0.0001f);
        }

        [Test]
        public void Bezier_Starts_And_Ends_At_Its_Endpoints()
        {
            VfxCurves.Bezier(10f, 20f, 50f, 300f, 400f, 500f, 0f, out float sx, out float sy);
            VfxCurves.Bezier(10f, 20f, 50f, 300f, 400f, 500f, 1f, out float ex, out float ey);

            Assert.AreEqual(10f, sx, 0.001f);
            Assert.AreEqual(20f, sy, 0.001f);
            Assert.AreEqual(400f, ex, 0.001f);
            Assert.AreEqual(500f, ey, 0.001f);
        }

        [Test]
        public void Shake_Fades_And_Stops()
        {
            Assert.AreEqual(0.2f, VfxCurves.ShakeAmplitude(0f, 0.3f, 0.2f), 0.0001f);
            Assert.Less(VfxCurves.ShakeAmplitude(0.15f, 0.3f, 0.2f), 0.2f);
            Assert.AreEqual(0f, VfxCurves.ShakeAmplitude(0.3f, 0.3f, 0.2f), 0.0001f);
            Assert.AreEqual(0f, VfxCurves.ShakeAmplitude(0f, 0f, 0.2f), 0.0001f);
        }

        [Test]
        public void Shake_Direction_Stays_Within_One_Unit()
        {
            for (int i = 0; i < 200; i++)
            {
                VfxCurves.ShakeDirection(i * 0.013f, out float x, out float y);

                Assert.LessOrEqual(Math.Abs(x), 1.0001f);
                Assert.LessOrEqual(Math.Abs(y), 1.0001f);
            }
        }

        [Test]
        public void Danger_Pulse_Disappears_At_Zero_Intensity()
        {
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(
                    0f,
                    VfxCurves.DangerPulse(i * 0.1f, 0f),
                    0.0001f);
            }

            Assert.Greater(
                VfxCurves.DangerPulse(0.3f, 1f),
                0f);
        }
    }

    public sealed class VfxSettingsTests
    {
        [Test]
        public void Screen_Shake_Is_On_For_New_And_Old_Saves()
        {
            // 예전 세이브는 이 항목이 없어 false로 읽힌다. false가 "켜짐"이어야 한다.
            SaveData data =
                SaveDataLogic.CreateDefault();

            Assert.IsFalse(data.ScreenShakeDisabled);
        }

        [Test]
        public void Screen_Shake_Setting_Survives_Clone()
        {
            SaveData data =
                SaveDataLogic.CreateDefault();

            data.ScreenShakeDisabled = true;

            Assert.IsTrue(data.Clone().ScreenShakeDisabled);
        }

        [Test]
        public void Vfx_Tuning_Has_Sensible_Defaults()
        {
            StageBalanceValues v =
                new StageBalanceValues();

            Assert.Less(v.VfxShakeSmall, v.VfxShakeMedium);
            Assert.Less(v.VfxShakeMedium, v.VfxShakeLarge);

            // 멈칫이 0.2초를 넘으면 조작이 끊긴 것처럼 느껴진다.
            Assert.Greater(v.VfxHitStopSeconds, 0f);
            Assert.LessOrEqual(v.VfxHitStopSeconds, 0.2f);

            Assert.Greater(v.VfxHitStopScale, 0f);
            Assert.Less(v.VfxHitStopScale, 1f);
        }
    }

    /// <summary>
    /// 연출·효과음 키마다 실제 파일이 있는지 본다.
    /// 키 이름과 파일 이름이 어긋나면 조용히 대체 그림·합성음으로 떨어져서 눈치채기 어렵다.
    /// Unity가 Resources를 읽어야 하므로 Test Runner에서만 돈다.
    /// </summary>
    public sealed class VfxAssetPresenceTests
    {
        [Test]
        public void Every_Vfx_Sprite_Has_A_File()
        {
            foreach (VfxSprite kind in Enum.GetValues(typeof(VfxSprite)))
            {
                Assert.IsNotNull(
                    Resources.Load<Texture2D>(
                        VfxLibrary.ResourceFolder + "/" + kind),
                    $"Resources/Vfx/{kind}.png이 없습니다");
            }
        }

        [Test]
        public void Every_Sound_Has_A_File()
        {
            foreach (GameSfx sfx in Enum.GetValues(typeof(GameSfx)))
            {
                Assert.IsNotNull(
                    Resources.Load<AudioClip>(
                        GameAudio.ResourceFolder + "/" + sfx),
                    $"Resources/Audio/{sfx}.wav가 없습니다");
            }
        }
    }
}
