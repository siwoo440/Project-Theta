"""
프로젝트 θ 임시 에셋 생성기 (19일차)

정식 아트·사운드가 들어오기 전까지 쓰는 이펙트 이미지와 효과음을 만든다.
외부 라이브러리 없이 파이썬 표준 기능(zlib, struct, wave)만 쓴다.

만드는 것
  Assets/_Project/Resources/Vfx/*.png    이펙트 스프라이트 7종 (흰색, 코드에서 색을 입힌다)
  Assets/_Project/Resources/Audio/*.wav  효과음 10종

실행
  python Tools/generate_temp_assets.py

규칙
  - 파일 이름은 코드의 키와 같아야 한다 (VfxSprite, GameSfx 열거형 이름)
  - .meta 파일이 이미 있으면 GUID를 유지한다. 다시 실행해도 Unity 참조가 끊기지 않는다
  - 정식 에셋으로 교체할 때는 같은 이름으로 파일만 덮어쓰면 된다
"""

import math
import os
import random
import re
import struct
import uuid
import wave
import zlib

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
VFX_DIR = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'Vfx')
AUDIO_DIR = os.path.join(ROOT, 'Assets', '_Project', 'Resources', 'Audio')

SAMPLE_RATE = 44100


# ───────────────────────── PNG ─────────────────────────

def write_png(path, width, height, pixel):
    """pixel(x, y) -> (r, g, b, a) 0~255. y=0이 위쪽이다."""
    raw = bytearray()
    for y in range(height):
        raw.append(0)
        for x in range(width):
            r, g, b, a = pixel(x, y)
            raw += bytes((clamp8(r), clamp8(g), clamp8(b), clamp8(a)))

    def chunk(tag, data):
        body = tag + data
        return struct.pack('>I', len(data)) + body + struct.pack('>I', zlib.crc32(body) & 0xffffffff)

    png = b'\x89PNG\r\n\x1a\n'
    png += chunk(b'IHDR', struct.pack('>IIBBBBB', width, height, 8, 6, 0, 0, 0))
    png += chunk(b'IDAT', zlib.compress(bytes(raw), 9))
    png += chunk(b'IEND', b'')

    with open(path, 'wb') as f:
        f.write(png)


def clamp8(v):
    return max(0, min(255, int(round(v))))


def smoothstep(e0, e1, x):
    if e0 == e1:
        return 0.0 if x < e0 else 1.0
    t = max(0.0, min(1.0, (x - e0) / (e1 - e0)))
    return t * t * (3 - 2 * t)


def white(alpha):
    return 255, 255, 255, alpha * 255


def centered(size, x, y):
    """중심 기준 -1~1 좌표."""
    half = (size - 1) / 2.0
    return (x - half) / half, (y - half) / half


def soft_circle(x, y):
    nx, ny = centered(128, x, y)
    d = math.hypot(nx, ny)
    # 가운데가 가장 밝고 가장자리로 부드럽게 사라지는 광원
    return white((1.0 - smoothstep(0.0, 1.0, d)) ** 1.6)


def ring(x, y):
    nx, ny = centered(128, x, y)
    d = math.hypot(nx, ny)
    # 반경 0.82 근처에 폭이 좁은 고리, 안팎으로 부드럽게
    band = 1.0 - smoothstep(0.0, 0.14, abs(d - 0.82))
    return white(band)


def spark(x, y):
    nx, ny = centered(64, x, y)
    # 네 갈래 빛 + 가운데 점
    cross = max(
        (1.0 - smoothstep(0.0, 0.10, abs(nx))) * (1.0 - smoothstep(0.0, 1.0, abs(ny))),
        (1.0 - smoothstep(0.0, 0.10, abs(ny))) * (1.0 - smoothstep(0.0, 1.0, abs(nx))))
    core = 1.0 - smoothstep(0.0, 0.35, math.hypot(nx, ny))
    return white(max(cross, core))


def pillar(x, y):
    # 64 x 256, 가로는 가운데가 밝고 세로는 아래가 밝다 (y=0이 위)
    nx = (x - 31.5) / 31.5
    vy = y / 255.0
    horizontal = (1.0 - smoothstep(0.0, 1.0, abs(nx))) ** 1.4
    vertical = smoothstep(0.0, 0.85, vy)
    return white(horizontal * vertical)


def shard(x, y):
    nx, ny = centered(16, x, y)
    d = max(abs(nx), abs(ny))
    return white(1.0 - smoothstep(0.55, 1.0, d))


def heart_inside(nx, ny):
    """하트 방정식 (x^2 + y^2 - 1)^3 - x^2 y^3 <= 0. 위아래를 뒤집어 그린다."""
    hx = nx * 1.25
    hy = -ny * 1.25 + 0.2
    return (hx * hx + hy * hy - 1.0) ** 3 - hx * hx * hy ** 3 <= 0.0


def heart(x, y):
    # 하트 방정식은 경계 근처에서 값의 크기가 고르지 않아, 값으로 번짐을 주면 가장자리가 얼룩진다.
    # 대신 픽셀 하나를 4x4로 쪼개 안쪽에 든 비율을 알파로 쓴다 (슈퍼샘플링).
    size = 64
    half = (size - 1) / 2.0
    inside = 0
    for sy in range(4):
        for sx in range(4):
            px = x - 0.375 + sx * 0.25
            py = y - 0.375 + sy * 0.25
            if heart_inside((px - half) / half, (py - half) / half):
                inside += 1
    return white(inside / 16.0)


def edge_vignette(x, y):
    # 화면 가장자리만 밝은 테두리. 가운데는 완전히 투명하다.
    nx, ny = centered(256, x, y)
    edge = max(abs(nx), abs(ny))
    return white(smoothstep(0.62, 1.0, edge) ** 1.5)


SPRITES = [
    ('SoftCircle', 128, 128, soft_circle),
    ('Ring', 128, 128, ring),
    ('Spark', 64, 64, spark),
    ('Pillar', 64, 256, pillar),
    ('Shard', 16, 16, shard),
    ('Heart', 64, 64, heart),
    ('EdgeVignette', 256, 256, edge_vignette),
]


# ───────────────────────── WAV ─────────────────────────

def write_wav(path, samples):
    with wave.open(path, 'wb') as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SAMPLE_RATE)
        frames = bytearray()
        for s in samples:
            frames += struct.pack('<h', int(max(-1.0, min(1.0, s)) * 32000))
        w.writeframes(bytes(frames))


def silence(seconds):
    return [0.0] * int(SAMPLE_RATE * seconds)


def tone(freq, seconds, volume=0.5, decay=6.0, attack=0.004, harmonics=((1, 1.0),), sweep=0.0):
    n = int(SAMPLE_RATE * seconds)
    out = []
    phase = [0.0] * len(harmonics)
    for i in range(n):
        t = i / SAMPLE_RATE
        env = min(1.0, t / attack) * math.exp(-decay * t)
        f = freq * (1.0 + sweep * t / seconds)
        s = 0.0
        for k, (mult, amp) in enumerate(harmonics):
            phase[k] += 2 * math.pi * f * mult / SAMPLE_RATE
            s += math.sin(phase[k]) * amp
        out.append(s * volume * env)
    return out


def noise(seconds, volume=0.4, decay=8.0, lowpass=0.2, seed=7):
    rng = random.Random(seed)
    n = int(SAMPLE_RATE * seconds)
    out = []
    last = 0.0
    for i in range(n):
        t = i / SAMPLE_RATE
        last += (rng.uniform(-1, 1) - last) * lowpass
        out.append(last * volume * math.exp(-decay * t))
    return out


def mix(*tracks):
    length = max(len(t) for t in tracks)
    out = [0.0] * length
    for track in tracks:
        for i, s in enumerate(track):
            out[i] += s
    peak = max(1e-6, max(abs(s) for s in out))
    if peak > 0.95:
        out = [s * 0.95 / peak for s in out]
    return out


def offset(track, seconds):
    return silence(seconds) + track


BELL = ((1, 1.0), (2.01, 0.35), (3.02, 0.12))
WARM = ((1, 1.0), (2, 0.25))


def sfx_ui_tick():
    return tone(1320, 0.06, 0.35, decay=60, harmonics=WARM)


def sfx_ui_stamp():
    return mix(tone(110, 0.35, 0.7, decay=10, harmonics=WARM),
               noise(0.12, 0.5, decay=40, lowpass=0.35))


def sfx_claim_tick():
    return tone(880, 0.07, 0.25, decay=45)


def sfx_purchase():
    return mix(tone(784, 0.18, 0.35, decay=14, harmonics=BELL),
               offset(tone(1175, 0.22, 0.32, decay=12, harmonics=BELL), 0.07))


def sfx_hypnosis_success():
    # 부드러운 종소리가 살짝 올라간다
    return mix(tone(660, 0.45, 0.35, decay=7, harmonics=BELL, sweep=0.05),
               offset(tone(990, 0.40, 0.18, decay=8, harmonics=BELL), 0.05))


def sfx_recovery():
    # 화음이 차례로 쌓이는 회수음
    notes = [523, 659, 784, 1047]
    return mix(*[offset(tone(f, 0.55, 0.28, decay=5, harmonics=BELL), i * 0.06)
                 for i, f in enumerate(notes)])


def sfx_level_up():
    # 빠른 상승 아르페지오
    notes = [523, 659, 784, 1047, 1319]
    return mix(*[offset(tone(f, 0.35, 0.30, decay=7, harmonics=BELL), i * 0.055)
                 for i, f in enumerate(notes)])


def sfx_dodge():
    # 바람 가르는 소리 + 짧은 높은 음
    return mix(noise(0.28, 0.45, decay=9, lowpass=0.08, seed=11),
               offset(tone(1480, 0.12, 0.15, decay=25), 0.08))


def sfx_duel_win():
    return mix(tone(82, 0.40, 0.8, decay=9, harmonics=WARM),
               noise(0.18, 0.6, decay=25, lowpass=0.4, seed=3),
               offset(tone(988, 0.30, 0.2, decay=10, harmonics=BELL), 0.05))


def sfx_floor_arrive():
    # 계단 발소리 두 번
    step = mix(tone(180, 0.09, 0.45, decay=40), noise(0.06, 0.3, decay=60, lowpass=0.3, seed=5))
    return mix(step, offset(step, 0.13))


SOUNDS = [
    ('UiTick', sfx_ui_tick),
    ('UiStamp', sfx_ui_stamp),
    ('ClaimTick', sfx_claim_tick),
    ('Purchase', sfx_purchase),
    ('HypnosisSuccess', sfx_hypnosis_success),
    ('Recovery', sfx_recovery),
    ('LevelUp', sfx_level_up),
    ('Dodge', sfx_dodge),
    ('DuelWin', sfx_duel_win),
    ('FloorArrive', sfx_floor_arrive),
]


# ───────────────────────── .meta ─────────────────────────

def existing_guid(meta_path):
    if not os.path.exists(meta_path):
        return None
    with open(meta_path, encoding='utf-8') as f:
        m = re.search(r'^guid: ([0-9a-f]{32})', f.read(), re.M)
    return m.group(1) if m else None


def write_meta(meta_path, body):
    guid = existing_guid(meta_path) or uuid.uuid4().hex
    with open(meta_path, 'w', encoding='utf-8', newline='\n') as f:
        f.write('fileFormatVersion: 2\nguid: %s\n%s' % (guid, body))


def folder_meta(path):
    meta = path + '.meta'
    if os.path.exists(meta):
        return
    write_meta(meta, 'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n'
                     '  userData: \n  assetBundleName: \n  assetBundleVariant: \n')


# 프로젝트에 이미 있는 캐릭터 스프라이트의 .meta를 본떴다.
# 단일 스프라이트(spriteMode 1), 밉맵 없음, 알파 투명, 바이리니어, 가장자리 고정.
SPRITE_META = """TextureImporter:
  internalIDToNameTable: []
  externalObjects: {}
  serializedVersion: 13
  mipmaps:
    mipMapMode: 0
    enableMipMap: 0
    sRGBTexture: 1
    linearTexture: 0
    fadeOut: 0
    borderMipMap: 0
    mipMapsPreserveCoverage: 0
    alphaTestReferenceValue: 0.5
    mipMapFadeDistanceStart: 1
    mipMapFadeDistanceEnd: 3
  bumpmap:
    convertToNormalMap: 0
    externalNormalMap: 0
    heightScale: 0.25
    normalMapFilter: 0
    flipGreenChannel: 0
  isReadable: 0
  streamingMipmaps: 0
  streamingMipmapsPriority: 0
  vTOnly: 0
  ignoreMipmapLimit: 0
  grayScaleToAlpha: 0
  generateCubemap: 6
  cubemapConvolution: 0
  seamlessCubemap: 0
  textureFormat: 1
  maxTextureSize: 2048
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    mipBias: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  lightmap: 0
  compressionQuality: 50
  spriteMode: 1
  spriteExtrude: 1
  spriteMeshType: 0
  alignment: 0
  spritePivot: {x: 0.5, y: 0.5}
  spritePixelsToUnits: %d
  spriteBorder: {x: 0, y: 0, z: 0, w: 0}
  spriteGenerateFallbackPhysicsShape: 0
  alphaUsage: 1
  alphaIsTransparency: 1
  spriteTessellationDetail: -1
  textureType: 8
  textureShape: 1
  singleChannelComponent: 0
  flipbookRows: 1
  flipbookColumns: 1
  maxTextureSizeSet: 0
  compressionQualitySet: 0
  textureFormatSet: 0
  ignorePngGamma: 0
  applyGammaDecoding: 0
  swizzle: 50462976
  cookieLightType: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    resizeAlgorithm: 0
    textureFormat: -1
    textureCompression: 0
    compressionQuality: 50
    crunchedCompression: 0
    allowsAlphaSplitting: 0
    overridden: 0
    ignorePlatformSupport: 0
    androidETC2FallbackOverride: 0
    forceMaximumCompressionQuality_BC6H_BC7: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    customData:
    physicsShape: []
    bones: []
    spriteID:
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    spriteCustomMetadata:
      entries: []
    nameFileIdTable: {}
  mipmapLimitGroupName:
  pSDRemoveMatte: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""

# 짧은 효과음: 메모리에 풀어 두고(0), 미리 불러 둔다. 모노로 강제한다.
AUDIO_META = """AudioImporter:
  externalObjects: {}
  serializedVersion: 8
  defaultSettings:
    serializedVersion: 2
    loadType: 0
    sampleRateSetting: 0
    sampleRateOverride: 44100
    compressionFormat: 0
    quality: 1
    conversionMode: 0
    preloadAudioData: 1
  platformSettingOverrides: {}
  forceToMono: 1
  normalize: 0
  loadInBackground: 0
  ambisonic: 0
  3D: 0
  userData:
  assetBundleName:
  assetBundleVariant:
"""


def main():
    os.makedirs(VFX_DIR, exist_ok=True)
    os.makedirs(AUDIO_DIR, exist_ok=True)
    folder_meta(VFX_DIR)
    folder_meta(AUDIO_DIR)

    for name, w, h, fn in SPRITES:
        path = os.path.join(VFX_DIR, name + '.png')
        write_png(path, w, h, fn)
        # 128px 이미지가 월드에서 1칸이 되도록 한다. 크기는 코드에서 배율로 조정한다.
        write_meta(path + '.meta', SPRITE_META % max(w, h))
        print('sprite', name, '%dx%d' % (w, h))

    for name, fn in SOUNDS:
        path = os.path.join(AUDIO_DIR, name + '.wav')
        samples = fn()
        write_wav(path, samples)
        write_meta(path + '.meta', AUDIO_META)
        print('sound ', name, '%.2fs' % (len(samples) / SAMPLE_RATE))


if __name__ == '__main__':
    main()
