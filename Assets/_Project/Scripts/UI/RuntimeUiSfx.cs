using UnityEngine;

namespace ProjectTheta.UI
{
    /// <summary>
    /// 프로토타입 단계에서 오디오 파일 없이 간단한 UI 효과음을 런타임 생성한다.
    /// 정식 사운드가 준비되면 호출 지점은 그대로 두고 클립만 교체하면 된다.
    /// </summary>
    public static class RuntimeUiSfx
    {
        private const int SampleRate = 44100;

        /// <summary>결과 화면 항목이 튀어나올 때의 짧은 "딱" 소리다.</summary>
        public static AudioClip CreateTick(
            string name = "UiTick",
            float frequency = 1180f,
            float duration = 0.045f,
            float volume = 0.18f)
        {
            return CreateTone(
                name,
                frequency,
                duration,
                volume,
                4f);
        }

        /// <summary>랭크 도장이 찍힐 때의 묵직한 소리다.</summary>
        public static AudioClip CreateStamp(
            string name = "UiStamp",
            float frequency = 190f,
            float duration = 0.22f,
            float volume = 0.30f)
        {
            return CreateTone(
                name,
                frequency,
                duration,
                volume,
                2.2f);
        }

        /// <summary>지수 감쇠를 적용한 단음 클립을 만든다.</summary>
        private static AudioClip CreateTone(
            string name,
            float frequency,
            float duration,
            float volume,
            float decay)
        {
            int sampleCount =
                Mathf.Max(
                    1,
                    Mathf.RoundToInt(
                        SampleRate *
                        Mathf.Max(
                            0.005f,
                            duration)));

            float[] samples =
                new float[
                    sampleCount];

            for (int i = 0;
                 i < samples.Length;
                 i++)
            {
                float t =
                    i /
                    (float)SampleRate;

                float envelope =
                    Mathf.Exp(
                        -decay *
                        (i /
                         (float)samples.Length) *
                        4f);

                samples[i] =
                    Mathf.Sin(
                        Mathf.PI *
                        2f *
                        frequency *
                        t) *
                    volume *
                    envelope;
            }

            AudioClip clip =
                AudioClip.Create(
                    name,
                    sampleCount,
                    1,
                    SampleRate,
                    false);

            clip.SetData(
                samples,
                0);

            return clip;
        }
    }
}
