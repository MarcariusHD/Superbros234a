#!/usr/bin/env python3
"""Genera la música ambiental (chiptune original) en Assets/Audio/Music/*.wav

Uso:  python3 Tools/generate_music.py [carpeta_salida]
Requiere numpy y scipy. Las melodías son composiciones originales (no usan
ningún tema de Nintendo), así que no hay problema de derechos.
Los loops terminan en silencio dentro del último compás: son sin cortes.
"""
import os, re, sys, wave
import numpy as np
from scipy.signal import decimate

SR_OUT = 44100
OS = 2                      # sobremuestreo para reducir aliasing
SR = SR_OUT * OS
NOTE = {'C': 0, 'D': 2, 'E': 4, 'F': 5, 'G': 7, 'A': 9, 'B': 11}


def midi(name):
    m = re.fullmatch(r'([A-G])([#b]?)(\d)', name)
    n = NOTE[m.group(1)] + (1 if m.group(2) == '#' else -1 if m.group(2) == 'b' else 0)
    return 12 * (int(m.group(3)) + 1) + n


def freq(m):
    return 440.0 * 2 ** ((m - 69) / 12)


def env(n, a=0.003, d=0.06, s=0.7, r=0.012):
    e = np.full(n, s)
    na, nd, nr = int(a * SR), int(d * SR), int(r * SR)
    na, nr = min(na, n), min(nr, n)
    if na: e[:na] = np.linspace(0, 1, na)
    nd = min(nd, max(0, n - na))
    if nd: e[na:na + nd] = np.linspace(1, s, nd)
    if nr: e[-nr:] *= np.linspace(1, 0, nr)
    return e


def osc_pulse(f, n, duty):
    t = np.arange(n) / SR
    return np.where(((t * f) % 1.0) < duty, 1.0, -1.0)


def osc_tri(f, n):
    t = np.arange(n) / SR
    x = 2 * np.abs(2 * ((t * f) % 1.0) - 1) - 1
    return np.round((x + 1) * 7.5) / 7.5 - 1          # 4 bits como la NES


def parse(seq):
    out = []
    for tok in seq.split():
        nm, ln = tok.split(':')
        out.append((None if nm == 'R' else midi(nm), int(ln)))
    return out


class Track:
    def __init__(self, bpm, spb, bars):
        self.step = 60.0 / bpm / 4            # una semicorchea
        self.spb = spb                        # semicorcheas por compás
        self.bars = bars
        self.n = int(round(bars * spb * self.step * SR))
        self.buf = np.zeros(self.n)

    def at(self, s):
        return int(round(s * self.step * SR))

    def melodic(self, bar_seqs, kind, vol, duty=0.5, gate=0.9, **e):
        pos = 0
        for seq in bar_seqs:
            total = 0
            for m, ln in parse(seq):
                if m is not None:
                    i0 = self.at(pos + total)
                    n = int(ln * self.step * SR * gate)
                    f = freq(m)
                    w = osc_tri(f, n) if kind == 'tri' else osc_pulse(f, n, duty)
                    end = min(i0 + n, self.n)
                    self.buf[i0:end] += w[:end - i0] * env(n, **e)[:end - i0] * vol
                total += ln
            assert total == self.spb, f'compás con {total} pasos (esperado {self.spb}): {seq}'
            pos += self.spb

    def drum(self, step, kind, vol):
        i0 = self.at(step)
        rng = np.random.default_rng(step * 7 + len(kind))
        if kind == 'kick':
            n = int(0.11 * SR); t = np.arange(n) / SR
            w = np.sin(2 * np.pi * np.cumsum(130 * np.exp(-t * 28) + 38) / SR) * np.exp(-t * 22)
        elif kind == 'snare':
            n = int(0.10 * SR); t = np.arange(n) / SR
            w = rng.uniform(-1, 1, n) * np.exp(-t * 38)
        else:
            n = int(0.03 * SR); t = np.arange(n) / SR
            w = np.diff(rng.uniform(-1, 1, n + 1)) * np.exp(-t * 120)
        end = min(i0 + n, self.n)
        self.buf[i0:end] += w[:end - i0] * vol

    def render(self):
        x = decimate(self.buf, OS, zero_phase=True)
        return x


def loudness_norm(x, rms=0.16, peak=0.9):
    x = x - x.mean()
    g = rms / (np.sqrt(np.mean(x ** 2)) + 1e-9)
    y = x * g
    if np.abs(y).max() > peak:
        y = np.tanh(y / peak) * peak                      # limitador suave
    fi, fo = int(0.003 * SR_OUT), int(0.012 * SR_OUT)  # bordes a cero: loop sin 'clic'
    y[:fi] *= np.linspace(0, 1, fi)
    y[-fo:] *= np.linspace(1, 0, fo)
    return y


def save(path, x):
    pcm = (np.clip(x, -1, 1) * 32767).astype('<i2')
    with wave.open(path, 'wb') as w:
        w.setnchannels(1); w.setsampwidth(2); w.setframerate(SR_OUT)
        w.writeframes(pcm.tobytes())


CH = {  # acordes: (nota grave, tercera, quinta) en octava 3
    'C': ('C3', 'E3', 'G3'), 'Dm': ('D3', 'F3', 'A3'), 'G': ('G2', 'B2', 'D3'),
    'Am': ('A2', 'C3', 'E3'), 'F': ('F2', 'A2', 'C3'), 'E': ('E2', 'G#2', 'B2'),
    'Dm4': ('D3', 'F3', 'A3'),
}


def arp(chord, pattern, octave_up=1):
    """Arpegio en semicorcheas: pattern = índices 0..2 (o None)."""
    notes = [midi(n) + 12 * octave_up for n in CH[chord]]
    toks = []
    for p in pattern:
        toks.append('R:1' if p is None else f'{_name(notes[p % 3] + 12 * (p // 3))}:1')
    return ' '.join(toks)


def _name(m):
    names = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B']
    return f'{names[m % 12]}{m // 12 - 1}'


def bass_eighths(chord, spb=16):
    r, _, f = [midi(n) for n in CH[chord]]
    toks = []
    for i in range(spb // 2):
        toks.append(f'{_name(r if i % 2 == 0 else f)}:2')
    return ' '.join(toks)


# ───────────────────────── PISTAS ─────────────────────────
def overworld():
    t = Track(138, 16, 16)
    mel = [
        'G4:2 C5:2 E5:2 G5:2 E5:2 C5:2 G4:4', 'A4:2 D5:2 F5:2 A5:2 F5:2 D5:2 A4:4',
        'B4:2 D5:2 G5:2 B5:2 A5:2 G5:2 F5:2 D5:2', 'E5:4 C5:4 R:2 G4:2 C5:4',
        'E5:2 F5:2 G5:4 E5:2 C5:2 D5:4', 'D5:2 E5:2 F5:4 D5:2 B4:2 C5:4',
        'A4:2 C5:2 E5:2 A5:2 G5:2 E5:2 C5:2 A4:2', 'G4:4 B4:4 D5:4 G5:4',
        'A4:4 C5:2 E5:2 A5:4 E5:4', 'F5:4 A5:2 C6:2 A5:4 F5:4',
        'G5:2 G5:2 G5:2 B5:2 D6:4 B5:4', 'C6:4 G5:4 E5:4 C5:4',
        'A5:2 G5:2 E5:2 C5:2 A4:4 R:4', 'F5:2 E5:2 C5:2 A4:2 F4:4 R:4',
        'G4:2 B4:2 D5:2 G5:2 B5:2 G5:2 D5:2 B4:2', 'C5:2 E5:2 G5:2 C6:2 G5:4 R:4']
    chords = ['C', 'Dm', 'G', 'C', 'C', 'Dm', 'G', 'C', 'Am', 'F', 'G', 'C', 'Am', 'F', 'G', 'C']
    t.melodic(mel, 'pulse', 0.30, duty=0.25, gate=0.85)
    t.melodic([arp(c, [0, 1, 2, 1, 0, 1, 2, 1, 0, 1, 2, 1, 0, 1, 2, 1], 1) for c in chords],
              'pulse', 0.10, duty=0.125, gate=0.8)
    t.melodic([bass_eighths(c) for c in chords], 'tri', 0.45, gate=0.9, d=0.02, s=0.9)
    for b in range(16):
        o = b * 16
        for s in (0, 8): t.drum(o + s, 'kick', 0.5)
        for s in (4, 12): t.drum(o + s, 'snare', 0.22)
        for s in range(0, 16, 2): t.drum(o + s, 'hat', 0.10)
    return t


def underground():
    t = Track(104, 16, 8)
    ch = ['Am', 'Am', 'F', 'E', 'Am', 'Am', 'Dm', 'E']
    mel = ['A3:2 R:2 C4:2 R:2 E4:4 R:4', 'A3:2 R:2 C4:2 R:2 B3:4 R:4',
           'F3:2 R:2 A3:2 R:2 C4:4 R:4', 'E3:2 G#3:2 B3:2 G#3:2 E4:4 R:4',
           'A3:2 C4:2 E4:2 C4:2 A4:4 E4:4', 'G4:2 E4:2 C4:2 A3:2 E4:4 R:4',
           'D4:2 F4:2 A4:2 F4:2 D4:4 A3:4', 'E4:2 D4:2 B3:2 G#3:2 E3:8']
    t.melodic(mel, 'pulse', 0.26, duty=0.5, gate=0.6)
    bass = []
    for c in ch:
        r = midi(CH[c][0]) - 12
        bass.append(' '.join(f'{_name(r)}:2 R:2' if i % 2 == 0 else f'{_name(r + 12)}:2 R:2' for i in range(4)))
    t.melodic(bass, 'tri', 0.55, gate=0.8)
    for b in range(8):
        o = b * 16
        t.drum(o, 'kick', 0.45); t.drum(o + 10, 'kick', 0.35)
        for s in range(0, 16, 4): t.drum(o + s + 2, 'hat', 0.08)
    return t


def underwater():
    t = Track(92, 12, 8)         # vals 3/4
    ch = ['F', 'F', 'Dm', 'G', 'F', 'Am', 'Dm', 'F']
    mel = ['A4:6 G4:2 F4:4', 'C5:6 A4:2 G4:4', 'D5:6 C5:2 A4:4', 'B4:4 D5:4 G4:4',
           'A4:6 C5:2 F5:4', 'E5:6 C5:2 A4:4', 'F5:4 D5:4 A4:4', 'F4:12']
    t.melodic(mel, 'pulse', 0.26, duty=0.5, gate=0.95, a=0.02, d=0.2, s=0.6, r=0.04)
    t.melodic([_wv(c) for c in ch], 'pulse', 0.11, duty=0.25, gate=0.9)
    t.melodic([f'{_name(midi(CH[c][0]))}:4 R:4 {_name(midi(CH[c][2]))}:4' for c in ch],
              'tri', 0.5, gate=0.9)
    for b in range(8):
        t.drum(b * 12, 'kick', 0.3)
        for s in (4, 8): t.drum(b * 12 + s, 'hat', 0.05)
    return t


def _wv(c):  # arpegio de vals: 12 pasos
    n = [midi(x) + 12 for x in CH[c]]
    pat = [None, None, None, None, 0, 1, 2, 1, 2, 1, 2, 1]
    return ' '.join('R:1' if p is None else f'{_name(n[p])}:1' for p in pat)


def title():
    t = Track(124, 16, 8)
    mel = ['C5:4 E5:4 G5:4 C6:4', 'A5:6 G5:2 E5:4 C5:4', 'D5:4 F5:4 A5:4 D6:4', 'B5:6 A5:2 G5:8',
           'E5:2 G5:2 C6:4 B5:2 A5:2 G5:4', 'F5:2 A5:2 C6:4 A5:2 F5:2 D5:4',
           'G5:4 B5:4 D6:4 B5:4', 'C6:12 R:4']
    ch = ['C', 'Am', 'Dm', 'G', 'C', 'F', 'G', 'C']
    t.melodic(mel, 'pulse', 0.30, duty=0.5, gate=0.9)
    t.melodic([arp(c, [0, 2, 1, 2] * 4, 1) for c in ch], 'pulse', 0.10, duty=0.25, gate=0.8)
    t.melodic([bass_eighths(c) for c in ch], 'tri', 0.45, gate=0.9)
    for b in range(8):
        o = b * 16
        for s in (0, 8): t.drum(o + s, 'kick', 0.5)
        for s in (4, 12): t.drum(o + s, 'snare', 0.2)
        for s in range(0, 16, 2): t.drum(o + s, 'hat', 0.09)
    return t


def gameover():
    t = Track(88, 16, 3)
    t.melodic(['C5:4 B4:4 Bb4:4 A4:4', 'Ab4:4 G4:4 F4:4 E4:4', 'D4:4 C4:12'], 'pulse', 0.32, duty=0.25,
              gate=0.95, a=0.01, d=0.2, s=0.6, r=0.05)
    t.melodic(['C3:8 G2:8', 'Ab2:8 E2:8', 'D2:4 C2:12'], 'tri', 0.55, gate=0.97)
    return t


TRACKS = {'Music_Overworld': overworld, 'Music_Underground': underground,
          'Music_Underwater': underwater, 'Music_Title': title, 'Music_GameOver': gameover}

if __name__ == '__main__':
    out = sys.argv[1] if len(sys.argv) > 1 else os.path.join(os.path.dirname(__file__), '..', 'Assets', 'Audio', 'Music')
    os.makedirs(out, exist_ok=True)
    for name, fn in TRACKS.items():
        x = loudness_norm(fn().render())
        save(os.path.join(out, name + '.wav'), x)
        print(f'{name}.wav  {len(x) / SR_OUT:5.1f}s  rms={np.sqrt(np.mean(x**2)):.3f}  peak={np.abs(x).max():.2f}')
