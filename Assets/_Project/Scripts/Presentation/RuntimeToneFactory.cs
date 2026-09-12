using UnityEngine;

namespace ProjectTheta.Presentation
{
    /// <summary>
    /// 오디오 파일 없이 간단한 단음을 만든다.
    ///
    /// 9일차 인기남 선점음과 11일차 결과 화면 효과음이 각각 갖고 있던
    /// 거의 동일한 생성 코드를 하나로 합친 것이다.
    /// </summary>
    public static class RuntimeToneFactory
    {
        public const int SampleRate = 44100;

        public static AudioClip CreateTone(
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

                float progress =
                    i /
                    (float)samples.Length;

                float envelope =
                    Mathf.Exp(
                        -decay *
                        progress *
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
