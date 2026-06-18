# [PY_COMPUTE_DSP]

The one classical digital-signal-processing owner. `Signal` discriminates IIR filter design, spectral estimation, and polyphase resampling over `scipy.signal`, emitting the dominant-band evidence the study spine reads. Each op carries the sample rate; the filter path designs second-order sections and applies zero-phase forward-backward filtering, the spectral path runs Welch's method and reads the peak band, and the resample path runs polyphase rational resampling. `scipy.signal` is Array-API-aware in scipy 1.17, so the op admits any backend array from `arrays/payload.md#PAYLOAD`; the bodies gate on the scipy wheel. No learned or neural filters enter this owner.

## [1]-[INDEX]

[DSP]: IIR/FIR filter design, Welch spectral estimation, and polyphase resampling on one `Signal` owner.

## [2]-[DSP]

- Owner: `Signal` — the ONE DSP owner over `scipy.signal`; `SignalOp` discriminates `Filter(kind, cutoff, order)` (`scipy.signal.butter` -> `sosfiltfilt` zero-phase IIR; `FilterKind` selects lowpass/highpass/bandpass), `Spectral(nperseg)` (`scipy.signal.welch` power-spectral-density estimate), and `Resample(target_rate)` (`scipy.signal.resample_poly` polyphase rational resample), as rows on the same owner, never a per-transform method family.
- Entry: `Signal.apply` returns `RuntimeRail[SignalReceipt]` carrying the op, the dominant frequency, the band power, and the output length; the filter path designs the SOS cascade and applies `sosfiltfilt`, the spectral path runs Welch and reads the peak band, and the resample path runs polyphase resampling. Each op normalizes the cutoff against the Nyquist frequency and reads the dominant band from a Welch estimate of the output.
- Receipt: `SignalReceipt.contribute` emits one `Receipt.of("emitted", ...)` row; the dominant frequency and band power are the spectral evidence a study run records through `experiments/study.md#STUDY`.
- Packages: `scipy` (`signal.butter`, `signal.sosfiltfilt`, `signal.welch`, `signal.resample_poly`), `numpy` (`asarray`, `argmax`, `trapezoid`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new transform is one `SignalOp` case; a new filter family is one `FilterKind` row; zero new surface.
- Boundary: classical DSP only — IIR/FIR design, spectral estimation, and resampling are in-scope; no learned filters and no neural denoising. `scipy` carries no cp315 wheel, so every `Signal` body is authored against the documented `scipy.signal` API; scipy 1.17 makes `scipy.signal` Array-API-aware, so the op admits any backend array from the payload admission.

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

    def contribute(self) -> Receipt:
        facts = {"op": self.op, "dominant_hz": f"{self.dominant_hz:.3f}", "band_power": f"{self.band_power:.3e}"}
        return Receipt.of("emitted", "compute.signal", self.op, facts)


@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "resample"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int] = case()
    spectral: int = case()
    resample: float = case()

    @staticmethod
    def Filter(kind: FilterKind, cutoff: tuple[float, ...], order: int = 4) -> "SignalOp":
        return SignalOp(filter=(kind, cutoff, order))

    @staticmethod
    def Spectral(nperseg: int = 256) -> "SignalOp":
        return SignalOp(spectral=nperseg)

    @staticmethod
    def Resample(target_rate: float) -> "SignalOp":
        return SignalOp(resample=target_rate)


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
        case unreachable:
            assert_never(unreachable)
```

## [3]-[RESEARCH]

- [SCIPY_SIGNAL]: the `scipy.signal.butter`/`sosfiltfilt`/`welch`/`resample_poly` spellings carry the `python_version<'3.15'` marker; the bodies verify against the `.api` catalogue once the scipy wheel resolves. scipy 1.17 makes `scipy.signal` Array-API-aware, so the op admits any backend array resolved through `arrays/payload.md#PAYLOAD`.
