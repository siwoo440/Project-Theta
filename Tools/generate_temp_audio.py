"""
프로젝트 θ 임시 배경음악 · 효과음 생성기 (37일차)

정식 음악이 들어오기 전까지 쓰는 임시 음원을 만든다.
19일차 생성기(generate_temp_assets.py)처럼 파이썬 표준 기능만 쓴다.

만드는 것
  Assets/_Project/Resources/Audio/Music/{곡}.wav          배경음악 12곡 (4마디 반복)
  Assets/_Project/Resources/Audio/Music/{곡}_Tension.wav  긴장 겹 12개 (1마디 반복, 타악기만)
  Assets/_Project/Resources/Audio/{효과음}.wav             37일차 효과음 12종

실행
  python Tools/generate_temp_audio.py            (전부, 10초 정도 걸린다)
  python Tools/generate_temp_audio.py sfx        (효과음만)

규칙
  - 파일 이름은 코드의 키와 같다 (MusicTrack, GameSfx 열거형 이름)
  - 곡 길이는 마디의 정수배라서 끊김 없이 반복된다. 긴장 겹은 1마디라 곡과 박자가 맞는다
  - .meta가 이미 있으면 GUID를 유지한다
  - 정식 음원으로 바꿀 때는 같은 이름으로 파일만 덮어쓰면 된다(길이가 달라도 된다)
"""

import math
import os
import random
import struct
import sys
import wave

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

import generate_temp_assets as base  # noqa: E402

AUDIO_DIR = base.AUDIO_DIR
MUSIC_DIR = os.path.join(AUDIO_DIR, 'Music')

MUSIC_RATE = 22050
SFX_RATE = base.SAMPLE_RATE

NOTE_NAMES = {'C': 0, 'C#': 1, 'D': 2, 'D#': 3, 'E': 4, 'F': 5, 'F#': 6, 'G': 7, 'G#': 8, 'A': 9, 'A#': 10, 'B': 11}
MAJOR = [0, 2, 4, 5, 7, 9, 11]
MINOR = [0, 2, 3, 5, 7, 8, 10]


def midi_freq(note):
    return 440.0 * 2 ** ((note - 69) / 12.0)


def root_midi(name, octave):
    return 12 * (octave + 1) + NOTE_NAMES[name]


# ───────────────────────── 합성 ─────────────────────────

def osc(wave_kind, phase):
    """0~1 위상 → -1~1 파형."""
    p = phase % 1.0
    if wave_kind == 'sine':
        return math.sin(2 * math.pi * p)
    if wave_kind == 'tri':
        return 4 * abs(p - 0.5) - 1
    if wave_kind == 'square':
        return 0.6 if p < 0.5 else -0.6
    if wave_kind == 'saw':
        return (2 * p - 1) * 0.6
    return 0.0


def add_note(buf, rate, start, length, freq, volume, wave_kind='sine', attack=0.01, release=0.08, decay=0.0, vibrato=0.0):
    """buf에 음 하나를 더한다. 반복 곡이라 넘치는 꼬리는 앞쪽으로 감는다."""
    n = int(length * rate)
    s0 = int(start * rate)
    total = len(buf)
    phase = 0.0
    for i in range(n + int(release * rate)):
        t = i / rate
        if t < attack:
            env = t / attack
        elif i < n:
            env = math.exp(-decay * (t - attack)) if decay > 0 else 1.0
        else:
            held = math.exp(-decay * (length - attack)) if decay > 0 else 1.0
            env = held * max(0.0, 1.0 - (i - n) / (release * rate))
        f = freq * (1.0 + vibrato * math.sin(2 * math.pi * 5.0 * t))
        phase += f / rate
        buf[(s0 + i) % total] += osc(wave_kind, phase) * volume * env


def add_noise(buf, rate, start, length, volume, decay, lowpass, rng):
    n = int(length * rate)
    s0 = int(start * rate)
    total = len(buf)
    last = 0.0
    for i in range(n):
        t = i / rate
        last += (rng.uniform(-1, 1) - last) * lowpass
        buf[(s0 + i) % total] += last * volume * math.exp(-decay * t)


def add_kick(buf, rate, start, volume):
    n = int(0.22 * rate)
    s0 = int(start * rate)
    total = len(buf)
    phase = 0.0
    for i in range(n):
        t = i / rate
        f = 45 + 90 * math.exp(-t * 30)
        phase += f / rate
        buf[(s0 + i) % total] += math.sin(2 * math.pi * phase) * volume * math.exp(-t * 14)


def normalize(buf, peak=0.85):
    top = max(1e-6, max(abs(s) for s in buf))
    scale = peak / top
    return [s * scale for s in buf]


def write_wav(path, samples, rate):
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(rate)
        w.writeframes(b''.join(struct.pack('<h', int(max(-1.0, min(1.0, s)) * 32000)) for s in samples))


# ───────────────────────── 배경음악 ─────────────────────────

# 곡마다: 으뜸음 · 옥타브 · 음계 · 빠르기 · 코드 진행(음계 도수) · 음색 · 박자 성격
TRACKS = [
    # name,      root, oct, scale, bpm, progression,   pad,     lead,     style
    ('Title',    'A',  3, MINOR, 84,  [0, 5, 3, 4],   'tri',   'sine',   'arp_slow'),
    ('Room',     'F',  3, MAJOR, 76,  [0, 3, 5, 4],   'sine',  'tri',    'arp_slow'),
    ('City',     'D',  3, MINOR, 92,  [0, 6, 5, 4],   'saw',   'sine',   'pad_pulse'),
    ('Training', 'C',  3, MAJOR, 104, [0, 4, 5, 3],   'tri',   'square', 'beat_light'),
    ('Beach',    'G',  3, MAJOR, 112, [0, 3, 4, 3],   'sine',  'tri',    'beat_light'),
    ('Subway',   'E',  3, MINOR, 118, [0, 0, 5, 6],   'saw',   'square', 'beat_drive'),
    ('Fitness',  'B',  2, MINOR, 126, [0, 5, 6, 4],   'square','saw',    'beat_drive'),
    ('Market',   'D',  3, MINOR, 100, [0, 3, 4, 0],   'tri',   'square', 'beat_swing'),
    ('Mall',     'A#', 2, MAJOR, 108, [0, 5, 3, 4],   'sine',  'tri',    'beat_light'),
    ('Office',   'C#', 3, MINOR, 96,  [0, 5, 2, 4],   'tri',   'sine',   'pad_pulse'),
    ('Boss',     'F#', 2, MINOR, 132, [0, 5, 6, 4],   'saw',   'square', 'beat_drive'),
    ('Ending',   'E',  3, MAJOR, 72,  [0, 4, 5, 3],   'sine',  'tri',    'arp_slow'),
]

BARS = 4


def chord(root, scale, degree):
    """3화음(음계 도수 기준) midi 번호."""
    notes = []
    for step in (0, 2, 4):
        idx = degree + step
        octave, pos = divmod(idx, 7)
        notes.append(root + scale[pos] + 12 * octave)
    return notes


def build_track(name, root_name, octave, scale, bpm, progression, pad_wave, lead_wave, style):
    rng = random.Random(sum(ord(c) for c in name))
    beat = 60.0 / bpm
    bar = beat * 4
    # 곡 길이를 긴장 겹(1마디) 길이의 정확한 배수로 맞춘다.
    buf = [0.0] * (int(bar * MUSIC_RATE) * BARS)
    root = root_midi(root_name, octave)

    for b, degree in enumerate(progression):
        start = b * bar
        notes = chord(root, scale, degree)

        # 패드: 마디 내내 이어지는 화음
        for note in notes:
            add_note(buf, MUSIC_RATE, start, bar * 0.98, midi_freq(note + 12), 0.10, pad_wave, attack=0.25, release=0.3, vibrato=0.002)

        # 베이스
        bass = midi_freq(notes[0] - 12)
        if style.startswith('beat'):
            for k in range(8):
                add_note(buf, MUSIC_RATE, start + k * beat / 2, beat * 0.42, bass, 0.22, 'tri', attack=0.005, release=0.03, decay=3)
        else:
            add_note(buf, MUSIC_RATE, start, beat * 1.9, bass, 0.24, 'sine', attack=0.02, release=0.2, decay=0.8)
            add_note(buf, MUSIC_RATE, start + beat * 2, beat * 1.9, bass, 0.20, 'sine', attack=0.02, release=0.2, decay=0.8)

        # 아르페지오 · 멜로디
        arp = notes + [notes[1] + 12]
        if style == 'arp_slow':
            for k in range(8):
                add_note(buf, MUSIC_RATE, start + k * beat / 2, beat * 0.9, midi_freq(arp[k % 4] + 24), 0.09, lead_wave, attack=0.01, release=0.25, decay=4)
        elif style == 'pad_pulse':
            for k in range(4):
                add_note(buf, MUSIC_RATE, start + k * beat, beat * 0.3, midi_freq(arp[(k * 3) % 4] + 24), 0.08, lead_wave, attack=0.005, release=0.2, decay=6)
        else:
            for k in range(8):
                if rng.random() < 0.3:
                    continue
                swing = beat * 0.08 if style == 'beat_swing' and k % 2 == 1 else 0.0
                add_note(buf, MUSIC_RATE, start + k * beat / 2 + swing, beat * 0.35, midi_freq(arp[rng.randrange(4)] + 24), 0.07, lead_wave, attack=0.003, release=0.08, decay=8)

        # 가벼운 타악기(긴장 겹과 겹치지 않게 약하게)
        if style.startswith('beat'):
            for k in range(4):
                add_kick(buf, MUSIC_RATE, start + k * beat, 0.35 if style == 'beat_drive' else 0.25)
            for k in (1, 3):
                add_noise(buf, MUSIC_RATE, start + k * beat, 0.15, 0.18, 22, 0.55, rng)

    return normalize(buf, 0.8), bar


def build_tension(bar, style, seed):
    """긴장 겹: 1마디 16분 하이햇 + 박마다 킥 + 끝에 짧은 스네어. 음높이가 없어 어느 코드와도 맞는다."""
    rng = random.Random(seed)
    buf = [0.0] * int(bar * MUSIC_RATE)
    sixteenth = bar / 16
    for k in range(16):
        accent = 0.22 if k % 4 == 2 else 0.12
        add_noise(buf, MUSIC_RATE, k * sixteenth, sixteenth * 0.9, accent, 60, 0.9, rng)
    for k in range(4):
        add_kick(buf, MUSIC_RATE, k * bar / 4, 0.55)
    for k in (13, 14, 15):
        add_noise(buf, MUSIC_RATE, k * sixteenth, sixteenth, 0.3, 18, 0.5, rng)
    return normalize(buf, 0.7)


# ───────────────────────── 37일차 효과음 ─────────────────────────

tone = base.tone
noise = base.noise
mix = base.mix
offset = base.offset
BELL = base.BELL
WARM = base.WARM


def sfx_chase_start():
    # 두 음 경보가 두 번
    a = tone(740, 0.16, 0.35, decay=4, harmonics=WARM)
    b = tone(988, 0.16, 0.35, decay=4, harmonics=WARM)
    return mix(a, offset(b, 0.17), offset(a, 0.34), offset(b, 0.51), noise(0.3, 0.2, decay=10, lowpass=0.2, seed=21))


def sfx_exit_open():
    notes = [784, 988, 1175]
    return mix(*[offset(tone(f, 0.5, 0.26, decay=5, harmonics=BELL), i * 0.08) for i, f in enumerate(notes)])


def sfx_escape():
    notes = [523, 659, 784, 1047, 1319, 1568]
    return mix(*[offset(tone(f, 0.45, 0.26, decay=5, harmonics=BELL), i * 0.07) for i, f in enumerate(notes)],
               offset(tone(1047, 0.9, 0.2, decay=2.5, harmonics=BELL), 0.42))


def sfx_fail():
    notes = [494, 440, 392, 330]
    return mix(*[offset(tone(f, 0.4, 0.28, decay=4, harmonics=WARM), i * 0.16) for i, f in enumerate(notes)])


def sfx_dialogue_blip():
    return tone(620, 0.035, 0.25, decay=60, attack=0.002, harmonics=((1, 1.0), (3, 0.2)))


def sfx_bubble():
    return tone(520, 0.12, 0.35, decay=20, attack=0.003, sweep=1.2)


def sfx_window_open():
    return mix(noise(0.18, 0.25, decay=12, lowpass=0.12, seed=31), tone(660, 0.12, 0.18, decay=18, sweep=0.5))


def sfx_window_close():
    return mix(noise(0.14, 0.22, decay=16, lowpass=0.12, seed=33), tone(660, 0.1, 0.16, decay=22, sweep=-0.4))


def sfx_save():
    return mix(tone(880, 0.25, 0.3, decay=10, harmonics=BELL), offset(tone(1319, 0.35, 0.3, decay=8, harmonics=BELL), 0.1))


def sfx_night_toggle():
    return mix(tone(220, 0.9, 0.35, decay=3, harmonics=BELL), offset(tone(330, 0.8, 0.2, decay=3.5, harmonics=BELL), 0.12))


def sfx_warning():
    return mix(tone(196, 0.18, 0.45, decay=6, harmonics=WARM), offset(tone(196, 0.18, 0.35, decay=6, harmonics=WARM), 0.24))


def sfx_achievement():
    notes = [1047, 1319, 1568, 2093]
    return mix(*[offset(tone(f, 0.4, 0.22, decay=6, harmonics=BELL), i * 0.05) for i, f in enumerate(notes)],
               noise(0.4, 0.08, decay=6, lowpass=0.8, seed=41))


SOUNDS = [
    ('ChaseStart', sfx_chase_start),
    ('ExitOpen', sfx_exit_open),
    ('Escape', sfx_escape),
    ('Fail', sfx_fail),
    ('DialogueBlip', sfx_dialogue_blip),
    ('Bubble', sfx_bubble),
    ('WindowOpen', sfx_window_open),
    ('WindowClose', sfx_window_close),
    ('Save', sfx_save),
    ('NightToggle', sfx_night_toggle),
    ('Warning', sfx_warning),
    ('Achievement', sfx_achievement),
]

# 음악: 메모리에 압축해 두고(1, Vorbis) 필요할 때 푼다. 곡 위치(timeSamples)를 맞출 수 있다.
MUSIC_META = """AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 1
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 1
    quality: 0.7
    conversionMode: 0
    preloadAudioData: 0
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 0
  loadInBackground: 1
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def main():
    only_sfx = len(sys.argv) > 1 and sys.argv[1] == 'sfx'

    os.makedirs(AUDIO_DIR, exist_ok=True)
    base.folder_meta(AUDIO_DIR)

    for name, fn in SOUNDS:
        path = os.path.join(AUDIO_DIR, name + '.wav')
        samples = fn()
        base.write_wav(path, samples)
        base.write_meta(path + '.meta', base.AUDIO_META)
        print('sound ', name, '%.2fs' % (len(samples) / SFX_RATE))

    if only_sfx:
        return

    os.makedirs(MUSIC_DIR, exist_ok=True)
    base.folder_meta(MUSIC_DIR)

    for index, (name, root, octave, scale, bpm, progression, pad, lead, style) in enumerate(TRACKS):
        samples, bar = build_track(name, root, octave, scale, bpm, progression, pad, lead, style)
        path = os.path.join(MUSIC_DIR, name + '.wav')
        write_wav(path, samples, MUSIC_RATE)
        base.write_meta(path + '.meta', MUSIC_META)

        tension = build_tension(bar, style, 100 + index)
        tension_path = os.path.join(MUSIC_DIR, name + '_Tension.wav')
        write_wav(tension_path, tension, MUSIC_RATE)
        base.write_meta(tension_path + '.meta', MUSIC_META)

        print('music ', name, '%d bpm  %.2fs  (tension %.2fs)' % (bpm, len(samples) / MUSIC_RATE, len(tension) / MUSIC_RATE))


if __name__ == '__main__':
    main()
