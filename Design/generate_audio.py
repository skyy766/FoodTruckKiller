"""
Food Truck Killer - 占位音效/BGM 生成脚本
依赖：numpy + wave 标准库
输出：22050Hz mono 16bit PCM WAV

用法：
    python3 generate_audio.py

产出目录：
    ../Assets/_Project/Art/Audio/SFX/   12 个音效
    ../Assets/_Project/Art/Audio/Music/ 1 首 BGM 循环
"""

import os
import wave
import struct
import numpy as np

# === 全局参数 ===
SR = 22050          # 采样率
CH = 1              # 单声道
SW = 2              # 16bit
OUT_SFX = os.path.join(os.path.dirname(__file__), "..", "Assets", "_Project", "Art", "Audio", "SFX")
OUT_MUS = os.path.join(os.path.dirname(__file__), "..", "Assets", "_Project", "Art", "Audio", "Music")


def ensure_dir(p):
    os.makedirs(p, exist_ok=True)


def normalize(sig, peak=0.9):
    """归一化到指定峰值，避免削顶。"""
    m = np.max(np.abs(sig)) + 1e-9
    return sig * (peak / m)


def to_int16(sig):
    sig = np.clip(sig, -1.0, 1.0)
    return (sig * 32767).astype(np.int16)


def save_wav(path, sig):
    """将 float 数组写入 16bit mono WAV。"""
    data = to_int16(sig)
    with wave.open(path, "w") as w:
        w.setnchannels(CH)
        w.setsampwidth(SW)
        w.setframerate(SR)
        w.writeframes(data.tobytes())
    print("  [OK]", os.path.relpath(path, os.path.dirname(__file__) + "/.."), "|", len(data), "samples")


# === 基础波形/噪声工具 ===
def t(dur):
    return np.linspace(0, dur, int(SR * dur), endpoint=False)


def sine(freq, dur, amp=1.0):
    return amp * np.sin(2 * np.pi * freq * t(dur))


def square_wave(freq, dur, amp=1.0, duty=0.5):
    tt = t(dur)
    return amp * (np.mod(tt * freq, 1.0) < duty).astype(np.float32) * 2 - 1


def tri(freq, dur, amp=1.0):
    tt = t(dur)
    return amp * (2 * np.abs(2 * (tt * freq - np.floor(tt * freq + 0.5))) - 1)


def noise(dur):
    return np.random.uniform(-1, 1, int(SR * dur))


def env_exp(dur, tau):
    """指数衰减包络 e^{-t/tau}。"""
    return np.exp(-t(dur) / tau)


def env_adsr(dur, a=0.005, d=0.05, s=0.7, r=0.05):
    n = int(SR * dur)
    tt = np.linspace(0, dur, n, endpoint=False)
    env = np.zeros(n)
    a_n = max(1, int(a * SR))
    d_n = max(1, int(d * SR))
    r_n = max(1, int(r * SR))
    s_n = max(0, n - a_n - d_n - r_n)
    # Attack
    env[:a_n] = np.linspace(0, 1, a_n)
    # Decay
    env[a_n:a_n + d_n] = np.linspace(1, s, d_n)
    # Sustain
    env[a_n + d_n:a_n + d_n + s_n] = s
    # Release
    env[a_n + d_n + s_n:] = np.linspace(s, 0, n - (a_n + d_n + s_n))
    return env


def one_pole_lp(x, alpha=0.2):
    """简单一阶低通滤波（用于滤波白噪声）。"""
    y = np.zeros_like(x)
    y[0] = x[0]
    for i in range(1, len(x)):
        y[i] = alpha * x[i] + (1 - alpha) * y[i - 1]
    return y


def one_pole_hp(x, alpha=0.9):
    """简单一阶高通滤波。"""
    y = np.zeros_like(x)
    y[0] = x[0]
    for i in range(1, len(x)):
        y[i] = alpha * y[i - 1] + alpha * (x[i] - x[i - 1])
    return y


# ===================== 音效合成 =====================

def sfx_chop():
    """切菜：短促高频敲击 ~0.1s。短噪声 + 高频共振。"""
    dur = 0.1
    n = noise(dur)
    env = env_exp(dur, 0.02)
    base = n * env
    # 叠加一个高频正弦共振，模拟刀刃金属感
    sine_hi = sine(2200, dur, 0.5) * env
    sig = 0.6 * base + 0.4 * sine_hi
    return normalize(sig, 0.85)


def sfx_sizzle():
    """煎烤滋滋：滤波白噪声 ~0.5s。"""
    dur = 0.5
    n = noise(dur)
    # 带通：先低通保留中频，再高通去直流
    lp = one_pole_lp(n, 0.15)
    bp = one_pole_hp(lp, 0.85)
    # 缓慢起伏包络模拟油花不均
    tt = t(dur)
    amp_mod = 0.6 + 0.4 * np.sin(2 * np.pi * 6 * tt)
    env = np.minimum(1.0, tt * 40)  # 起音
    env[-int(0.05 * SR):] *= np.linspace(1, 0, int(0.05 * SR))  # 尾收
    sig = bp * amp_mod * env
    return normalize(sig, 0.7)


def sfx_serve():
    """出餐叮：正弦波 880Hz 短音。"""
    dur = 0.18
    env = env_exp(dur, 0.08)
    sig = sine(880, dur, 1.0) * env
    # 加一个八度泛音让铃声更亮
    sig += sine(1760, dur, 0.3) * env
    return normalize(sig, 0.8)


def sfx_cash():
    """收款：上升音 600→900Hz。"""
    dur = 0.22
    tt = t(dur)
    freq = 600 + (900 - 600) * (tt / dur)
    phase = 2 * np.pi * np.cumsum(freq) / SR
    env = env_adsr(dur, 0.005, 0.05, 0.6, 0.08)
    sig = np.sin(phase) * env
    # 加一点高八度增加硬币感
    sig += 0.25 * np.sin(2 * phase) * env
    return normalize(sig, 0.8)


def sfx_footstep():
    """脚步：低频短噪声 ~0.08s。"""
    dur = 0.08
    n = noise(dur)
    lp = one_pole_lp(n, 0.12)  # 低通 -> 沉闷
    env = env_exp(dur, 0.03)
    sig = lp * env
    # 叠一个 80Hz 低频 thump
    sig += 0.4 * sine(80, dur, 1.0) * env
    return normalize(sig, 0.7)


def sfx_kill():
    """击杀：低频冲击 + 噪声衰减 ~0.3s。"""
    dur = 0.3
    env = env_exp(dur, 0.12)
    # 低频冲击（120Hz 下滑到 50Hz）
    tt = t(dur)
    freq = 120 - 70 * (tt / dur)
    phase = 2 * np.pi * np.cumsum(freq) / SR
    impact = np.sin(phase) * env
    # 噪声层
    nz = noise(dur) * env_exp(dur, 0.06)
    nz = one_pole_lp(nz, 0.3)
    sig = 0.65 * impact + 0.5 * nz
    return normalize(sig, 0.95)


def sfx_grind():
    """绞肉：持续噪声 ~0.6s。粗糙调制感。"""
    dur = 0.6
    tt = t(dur)
    n = noise(dur)
    lp = one_pole_lp(n, 0.25)
    # 用低频方波幅度调制模拟绞盘卡顿
    lfo = 0.5 + 0.5 * square_wave(18, dur, 1.0, 0.3)
    env = np.minimum(1.0, tt * 30)  # 起音
    env[-int(0.1 * SR):] *= np.linspace(1, 0, int(0.1 * SR))
    sig = lp * lfo * env
    # 叠加 90Hz 低频嗡鸣
    sig += 0.3 * sine(90, dur) * env
    return normalize(sig, 0.7)


def sfx_explosion():
    """爆炸：噪声指数衰减 ~0.5s。"""
    dur = 0.5
    env = env_exp(dur, 0.15)
    n = noise(dur)
    lp = one_pole_lp(n, 0.2)
    # 低频 boom
    tt = t(dur)
    freq = 90 - 60 * (tt / dur)
    phase = 2 * np.pi * np.cumsum(freq) / SR
    boom = np.sin(phase) * env
    sig = 0.7 * lp * env + 0.6 * boom
    return normalize(sig, 0.98)


def sfx_scream():
    """尖叫：频率调制 ~0.4s。"""
    dur = 0.4
    tt = t(dur)
    # 载波 700~1100Hz 上下扫
    carrier = 700 + 400 * np.sin(2 * np.pi * 5 * tt) * (tt / dur)
    phase = 2 * np.pi * np.cumsum(carrier) / SR
    # 加点颤音
    vibrato = 30 * np.sin(2 * np.pi * 14 * tt)
    phase += 2 * np.pi * np.cumsum(vibrato) / SR
    env = np.zeros_like(tt)
    a = int(0.05 * SR)
    env[:a] = np.linspace(0, 1, a)
    env[a:] = np.exp(-(tt[a:] - tt[a]) / 0.18)
    sig = np.sin(phase) * env
    # 叠一些失真
    sig = np.tanh(2.0 * sig) * env
    return normalize(sig, 0.85)


def sfx_alarm():
    """警报：方波双音交替 ~0.6s。"""
    dur = 0.6
    tt = t(dur)
    # 8 段交替 600/800Hz
    seg = int(SR * 0.075)
    sig = np.zeros(int(SR * dur))
    for i, f in enumerate([800, 600] * 4):
        s = i * seg
        e = min(len(sig), s + seg)
        if s >= e:
            break
        sig[s:e] = square_wave(f, (e - s) / SR, 0.7)
    env = np.ones_like(sig)
    # 整体缓出
    env[-int(0.05 * SR):] *= np.linspace(1, 0, int(0.05 * SR))
    sig *= env
    return normalize(sig, 0.7)


def sfx_order_in():
    """订单到来：两音提示（C6->E6）。"""
    dur = 0.3
    half = int(SR * 0.13)
    sig = np.zeros(int(SR * dur))
    # 第一音 1046.5 (C6)
    s1 = sine(1046.5, 0.13, 1.0) * env_adsr(0.13, 0.005, 0.02, 0.7, 0.04)
    sig[:half] = s1
    # 第二音 1318.5 (E6)
    s2 = sine(1318.5, 0.13, 1.0) * env_adsr(0.13, 0.005, 0.02, 0.7, 0.06)
    sig[half:half * 2] = s2
    return normalize(sig, 0.75)


def sfx_wanted_warning():
    """通缉警告：急促三音。"""
    dur = 0.42
    seg = int(SR * 0.12)
    sig = np.zeros(int(SR * dur))
    freqs = [523.25, 659.25, 880.0]  # C5 E5 A5 警报上行
    for i, f in enumerate(freqs):
        s = i * seg
        e = min(len(sig), s + seg)
        seg_dur = (e - s) / SR
        tone = square_wave(f, seg_dur, 0.6) * env_exp(seg_dur, 0.05)
        sig[s:e] = tone
    return normalize(sig, 0.7)


# ===================== BGM 合成 =====================

def bgm_day():
    """白天经营 BGM：轻快电子风，和弦序列 + 鼓点，可循环 ~12s。"""
    bpm = 120
    beat = 60.0 / bpm  # 0.5s
    bars = 4
    beats_per_bar = 4
    total_beats = bars * beats_per_bar
    dur = beat * total_beats  # 8s -> 用 4 小节 4/4 = 8s，再加 1 小节尾过渡到循环
    # 改为 8 小节让更丰满 ~16s
    bars = 8
    total_beats = bars * beats_per_bar
    dur = beat * total_beats  # 16s

    n_total = int(SR * dur)
    out = np.zeros(n_total)
    tt_all = np.linspace(0, dur, n_total, endpoint=False)

    # 和弦进行 (C - G - Am - F) x2，每小节一个和弦
    # 频率（根音 + 三音 + 五音）
    chords = [
        (130.81, 164.81, 196.00),  # C major: C3 E3 G3
        (196.00, 246.94, 293.66),  # G major: G3 B3 D4
        (220.00, 261.63, 329.63),  # A minor: A3 C4 E4
        (174.61, 220.00, 261.63),  # F major: F3 A3 C4
    ] * 2

    # --- 贝斯线（根音）---
    for i, ch in enumerate(chords):
        s = int(i * beat * beats_per_bar * SR)
        e = min(n_total, s + int(beat * beats_per_bar * SR))
        seg = e - s
        root = ch[0] / 2  # 低八度
        wave_bass = tri(root, seg / SR, 0.5)
        # 八分音符节奏型
        for b in range(beats_per_bar):
            bs = int(b * beat * SR)
            be = int(bs + beat * 0.9 * SR)
            if bs + s >= n_total:
                break
            wave_bass[bs:be] *= 0.9
            wave_bass[be:min(seg, be + int(beat * 0.1 * SR))] *= 0.0
        out[s:e] += wave_bass

    # --- 和弦垫（pad）---
    for i, ch in enumerate(chords):
        s = int(i * beat * beats_per_bar * SR)
        e = min(n_total, s + int(beat * beats_per_bar * SR))
        seg = e - s
        pad = np.zeros(seg)
        for f in ch:
            pad += sine(f, seg / SR, 0.15)
        # 缓入缓出
        env = np.ones(seg)
        fa = int(0.1 * SR)
        env[:fa] = np.linspace(0, 1, fa)
        env[-fa:] = np.linspace(1, 0, fa)
        out[s:e] += pad * env

    # --- 旋律（每拍一个音，简单 pentatonic）---
    # C 大调五声音阶
    pent = [523.25, 587.33, 659.25, 783.99, 880.00]
    melody_pattern = [0, 2, 1, 3, 4, 3, 2, 1] * (total_beats // 8)
    for beat_idx in range(total_beats):
        s = int(beat_idx * beat * SR)
        e = min(n_total, s + int(beat * SR))
        seg = e - s
        f = pent[melody_pattern[beat_idx % len(melody_pattern)]]
        note = sine(f, seg / SR, 0.2) + 0.05 * sine(2 * f, seg / SR, 1.0)
        env = env_adsr(seg / SR, 0.01, 0.05, 0.5, 0.08)
        out[s:e] += note * env

    # --- 鼓点 ---
    # Kick 在每拍头，Snare 在 2/4 拍，Hat 在八分
    for beat_idx in range(total_beats):
        s = int(beat_idx * beat * SR)
        # Kick
        ks = s
        ke = min(n_total, ks + int(0.12 * SR))
        kseg = ke - ks
        kt = np.linspace(0, kseg / SR, kseg, endpoint=False)
        kfreq = 120 * np.exp(-kt * 30)
        kphase = 2 * np.pi * np.cumsum(kfreq) / SR
        kick = np.sin(kphase) * np.exp(-kt * 12)
        out[ks:ke] += 0.6 * kick

        # Snare 在 2、4 拍
        if beat_idx % 2 == 1:
            ss = s
            se = min(n_total, ss + int(0.1 * SR))
            sseg = se - ss
            sn = noise(sseg / SR)
            sn = one_pole_hp(sn, 0.85)
            sn *= np.exp(-np.linspace(0, 25, sseg))
            out[ss:se] += 0.35 * sn

        # Hi-hat 八分
        for sub in range(2):
            hs = s + int(sub * beat * 0.5 * SR)
            he = min(n_total, hs + int(0.04 * SR))
            hseg = he - hs
            if hseg <= 0:
                continue
            hat = noise(hseg / SR)
            hat = one_pole_hp(hat, 0.92)
            hat *= np.exp(-np.linspace(0, 60, hseg))
            amp = 0.18 if sub == 0 else 0.10
            out[hs:he] += amp * hat

    # 软压缩 + 归一化，确保循环点无明显爆音
    out = np.tanh(0.9 * out)
    # 起止淡入淡出极短，避免循环咔哒
    fa = int(0.01 * SR)
    out[:fa] *= np.linspace(0, 1, fa)
    out[-fa:] *= np.linspace(1, 0, fa)
    return normalize(out, 0.85)


# ===================== 主流程 =====================

def main():
    ensure_dir(OUT_SFX)
    ensure_dir(OUT_MUS)

    print("== 生成 SFX ==")
    sfx = [
        ("chop.wav", sfx_chop()),
        ("sizzle.wav", sfx_sizzle()),
        ("serve.wav", sfx_serve()),
        ("cash.wav", sfx_cash()),
        ("footstep.wav", sfx_footstep()),
        ("kill.wav", sfx_kill()),
        ("grind.wav", sfx_grind()),
        ("explosion.wav", sfx_explosion()),
        ("scream.wav", sfx_scream()),
        ("alarm.wav", sfx_alarm()),
        ("order_in.wav", sfx_order_in()),
        ("wanted_warning.wav", sfx_wanted_warning()),
    ]
    for name, sig in sfx:
        save_wav(os.path.join(OUT_SFX, name), sig)

    print("== 生成 BGM ==")
    save_wav(os.path.join(OUT_MUS, "bgm_day.wav"), bgm_day())

    print("完成。")


if __name__ == "__main__":
    main()
