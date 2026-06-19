# [PY_COMPUTE_SIGNAL]

The one classical signal-analysis owner spanning stationary spectral estimation and multiresolution wavelet analysis on a single `Signal`. `SignalOp` discriminates IIR/FIR filter design, Welch spectral estimation, and polyphase resampling over `scipy.signal` beside the wavelet case family — multilevel discrete decomposition, the continuous-wavelet scalogram, and the wavelet-packet tree — over `pywt`, so a transient, a localized discontinuity, or a non-stationary mode the Welch estimate averages away becomes first-class signal evidence on the same owner. Each op carries the sample rate; the filter path designs second-order sections and applies zero-phase forward-backward filtering, the spectral path runs Welch's method and reads the peak band, the resample path runs polyphase rational resampling, the wavelet `Decompose` path runs the multilevel DWT and folds the per-level energy distribution under the perfect-reconstruction invariant, the `Scalogram` path runs the CWT and reads the dominant scale through `scale2frequency`, and the `Packet` path walks the full wavelet-packet basis. `scipy.signal` is Array-API-aware in scipy 1.17, so the stationary ops admit any backend array from `numerics/array.md#PAYLOAD`; `pywt` is C-over-numpy and rides the same companion band beside the scipy path. The dominant-band and the wavelet-level energy evidence the study spine reads flow through one `SignalReceipt`. No learned or neural filters enter this owner.

## [1]-[INDEX]

- [1]-[DSP]: IIR/FIR filter design, Welch spectral estimation, polyphase resampling, and the `pywt` wavelet decompose/scalogram/packet multiresolution fold on one `Signal` owner.

## [2]-[DSP]

- Owner: `Signal` — the ONE signal-analysis owner; `SignalOp` discriminates the stationary rows `Filter(kind, cutoff, order)` (`scipy.signal.butter` -> `sosfiltfilt` zero-phase IIR; `FilterKind` selects lowpass/highpass/bandpass), `Spectral(nperseg)` (`scipy.signal.welch` power-spectral-density estimate), and `Resample(target_rate)` (`scipy.signal.resample_poly` polyphase rational resample), beside the multiresolution wavelet rows `Decompose(wavelet, level)` (`pywt.wavedec` multilevel DWT, `pywt.waverec` inverse, `pywt.dwt_max_level` cap), `Scalogram(wavelet, scales)` (`pywt.cwt` continuous transform, `pywt.scale2frequency` scale-frequency map), and `Packet(wavelet, maxlevel)` (`pywt.WaveletPacket` full-basis tree), all as rows on the same owner, never a per-transform method family and never a second signal surface.
- Entry: `Signal.apply` returns `RuntimeRail[SignalReceipt]`; the filter path designs the SOS cascade and applies `sosfiltfilt`, the spectral path runs Welch and reads the peak band, the resample path runs polyphase resampling, the `Decompose` path runs `wavedec` to the level cap and folds the squared-coefficient energy per level plus the `waverec`-reconstruction residual, the `Scalogram` path runs `cwt` and reads the maximum-energy scale mapped to a dominant frequency, and the `Packet` path walks the natural-order leaf nodes folding per-node energy. The stationary ops normalize the cutoff against the Nyquist frequency and read the dominant band from a Welch estimate; the wavelet ops report the dominant frequency from the dominant scale and leave `band_power` to the existing Welch fold where it applies, the wavelet energy carried in the new receipt fields.
- Receipt: `SignalReceipt.contribute` emits one `Receipt.of("emitted", ...)` row; the dominant frequency and band power are the stationary spectral evidence and the `level_energy` distribution, `dominant_scale`, and `reconstruction_residual` are the multiresolution evidence a study run records through `experiments/study.md#STUDY`.
- Packages: `scipy` (`signal.butter`, `signal.sosfiltfilt`, `signal.welch`, `signal.resample_poly`), `pywt` (`wavedec`, `waverec`, `dwt_max_level`, `cwt`, `scale2frequency`, `WaveletPacket`, `threshold`), `numpy` (`asarray`, `argmax`, `trapezoid`, `sum`, `linalg.norm`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new transform is one `SignalOp` case; a new filter family is one `FilterKind` row; a new wavelet transform is one `Wavelet`-family case; zero new surface.
- Boundary: classical signal analysis only — IIR/FIR design, spectral estimation, resampling, and the discrete/continuous/packet wavelet transforms are in-scope; no learned filters and no neural denoising. Both `scipy` and `pywt` carry the companion `python_version<'3.15'` band (neither ships a cp315 wheel), so every body is authored against the documented API on the gated band; scipy 1.17 makes `scipy.signal` Array-API-aware so the stationary ops admit any backend array from the payload admission, while the wavelet ops resolve `pywt`'s numpy-array contract. Perfect reconstruction `waverec(wavedec(x)) ≈ x` (`pywt`'s documented exact inverse) is the wavelet verification invariant; the `Decompose` path folds the reconstruction residual as the evidence of it.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class FilterKind(StrEnum):
    LOWPASS = "lowpass"
    HIGHPASS = "highpass"
    BANDPASS = "bandpass"


class SignalReceipt(Struct, frozen=True):
    op: str
    dominant_hz: float
    band_power: float
    length: int
    level_energy: tuple[float, ...] = ()
    dominant_scale: float = 0.0
    reconstruction_residual: float = 0.0

    def contribute(self) -> Receipt:
        facts = {"op": self.op, "dominant_hz": f"{self.dominant_hz:.3f}", "band_power": f"{self.band_power:.3e}"}
        if self.level_energy:
            facts["level_energy"] = ",".join(f"{e:.3e}" for e in self.level_energy)
        if self.dominant_scale:
            facts["dominant_scale"] = f"{self.dominant_scale:.3f}"
        if self.reconstruction_residual:
            facts["reconstruction_residual"] = f"{self.reconstruction_residual:.3e}"
        return Receipt.of("emitted", "compute.signal", self.op, facts)


@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "resample", "decompose", "scalogram", "packet"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int] = case()
    spectral: int = case()
    resample: float = case()
    decompose: tuple[str, int] = case()
    scalogram: tuple[str, tuple[float, ...]] = case()
    packet: tuple[str, int] = case()

    @staticmethod
    def Filter(kind: FilterKind, cutoff: tuple[float, ...], order: int = 4) -> "SignalOp":
        return SignalOp(filter=(kind, cutoff, order))

    @staticmethod
    def Spectral(nperseg: int = 256) -> "SignalOp":
        return SignalOp(spectral=nperseg)

    @staticmethod
    def Resample(target_rate: float) -> "SignalOp":
        return SignalOp(resample=target_rate)

    @staticmethod
    def Decompose(wavelet: str = "db4", level: int = 0) -> "SignalOp":
        return SignalOp(decompose=(wavelet, level))

    @staticmethod
    def Scalogram(wavelet: str = "morl", scales: tuple[float, ...] = ()) -> "SignalOp":
        return SignalOp(scalogram=(wavelet, scales))

    @staticmethod
    def Packet(wavelet: str = "db4", maxlevel: int = 3) -> "SignalOp":
        return SignalOp(packet=(wavelet, maxlevel))


def apply(samples: np.ndarray, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    return boundary(f"signal.{op.tag}", lambda: _apply(samples, fs, op))


def _apply(samples: np.ndarray, fs: float, op: SignalOp) -> SignalReceipt:
    import scipy.signal as sig

    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            filtered = sig.sosfiltfilt(sos, samples)
            f, pxx = sig.welch(filtered, fs=fs)
            return SignalReceipt("filter", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), filtered.size)
        case SignalOp(tag="spectral", spectral=nperseg):
            f, pxx = sig.welch(samples, fs=fs, nperseg=nperseg)
            return SignalReceipt("spectral", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), f.size)
        case SignalOp(tag="resample", resample=target):
            out = sig.resample_poly(samples, int(target), int(fs))
            f, pxx = sig.welch(out, fs=target)
            return SignalReceipt("resample", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), out.size)
        case SignalOp(tag="decompose", decompose=(wavelet, level)):
            import pywt

            depth = level or pywt.dwt_max_level(samples.size, pywt.Wavelet(wavelet).dec_len)
            coeffs = pywt.wavedec(samples, wavelet, level=depth)
            energy = tuple(float(np.sum(np.asarray(c) ** 2)) for c in coeffs)
            rebuilt = pywt.waverec(coeffs, wavelet)[: samples.size]
            residual = float(np.linalg.norm(rebuilt - samples) / (np.linalg.norm(samples) + 1e-30))
            f, pxx = sig.welch(samples, fs=fs)
            dominant = float(f[int(np.argmax(pxx))])
            return SignalReceipt("decompose", dominant, float(np.trapezoid(pxx, f)), samples.size, energy, float(depth), residual)
        case SignalOp(tag="scalogram", scalogram=(wavelet, scales)):
            import pywt

            grid = np.asarray(scales) if scales else np.geomspace(1.0, 0.5 * samples.size, 64)
            coefs, freqs = pywt.cwt(samples, grid, wavelet, sampling_period=1.0 / fs)
            scale_energy = np.sum(np.abs(np.asarray(coefs)) ** 2, axis=1)
            peak = int(np.argmax(scale_energy))
            return SignalReceipt(
                "scalogram",
                float(np.asarray(freqs)[peak]),
                float(np.sum(scale_energy)),
                samples.size,
                tuple(scale_energy.tolist()),
                float(grid[peak]),
            )
        case SignalOp(tag="packet", packet=(wavelet, maxlevel)):
            import pywt

            tree = pywt.WaveletPacket(data=samples, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            leaves = tree.get_level(maxlevel, order="natural")
            energy = tuple(float(np.sum(np.asarray(node.data) ** 2)) for node in leaves)
            f, pxx = sig.welch(samples, fs=fs)
            return SignalReceipt("packet", float(f[int(np.argmax(pxx))]), float(np.trapezoid(pxx, f)), samples.size, energy, float(maxlevel))
        case unreachable:
            assert_never(unreachable)
```

## [3]-[RESEARCH]

- [SCIPY_SIGNAL]: the `scipy.signal.butter`/`sosfiltfilt`/`welch`/`resample_poly` spellings carry the `python_version<'3.15'` marker and verify against the `scipy.signal` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` under a uv-sync reflection pass once the scipy wheel resolves. scipy 1.17 makes `scipy.signal` Array-API-aware, so the op admits any backend array resolved through `numerics/array.md#PAYLOAD`.
- [PYWT_WAVELET]: `pywt` (dist `pywavelets`) rides the companion `python_version<'3.15'` band beside the scipy path (C-over-numpy, no cp315 wheel); the `wavedec`/`waverec`/`dwt_max_level`/`cwt`/`scale2frequency`/`WaveletPacket`/`Wavelet.dec_len` spellings verify against `compute/.api/pywavelets.md` (documentation-authored, RESEARCH-capture-pending) under a uv-sync reflection pass once the `pywavelets` wheel resolves into the companion interpreter band. Perfect reconstruction `waverec(wavedec(x)) ≈ x` is the verification invariant the `Decompose` reconstruction-residual fold records.
