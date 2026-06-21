# [PY_COMPUTE_TRANSFORM]

The one frequency-domain transform owner spanning the pocketfft discrete-Fourier family, the trigonometric cosine/sine transforms, and the FFT-backed analytic signal on a single `Transform`. `TransformOp` discriminates the 1-D-or-n-D complex DFT (`fft`/`ifft`, `fftn`/`ifftn`), the half-spectrum real DFT (`rfft`/`irfft`), the discrete cosine and sine transforms (`dct`/`dst`), and the analytic-signal envelope (`scipy.signal.hilbert`) over `scipy.fft`, so a spectrum, an energy-compacted trigonometric basis, and the instantaneous amplitude/phase/frequency of a non-stationary mode are first-class transform evidence on the same owner rather than a per-transform method family. One `Fourier` row carries the `real`, `axes`, and `invert` policy as case data: the forward path runs the complex or real DFT (multi-axis `fftn` over the `axes` tuple, or the half-spectrum `rfft` along the lead axis), folds the dominant frequency from the `fftfreq`/`rfftfreq` grid against the magnitude spectrum, and reads the total Parseval energy; setting `invert` runs the matching inverse over the same spectrum and folds the round-trip reconstruction residual under the perfect-inversion invariant in the same arm — never a second arm re-deriving the forward spectrum. The trigonometric path folds the leading-coefficient spectral-energy concentration, and the `hilbert` path folds the mean envelope and the instantaneous-frequency band from the analytic phase derivative. `scipy.fft` is the pocketfft transform tier `scipy.md#TRANSFORM` routes here and is Array-API-aware in scipy 1.17, so every op admits any backend array from `numerics/array.md#PAYLOAD`. These are in-memory transforms; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners. The dominant-band, reconstruction-residual, energy-concentration, and envelope evidence the study spine reads flow through one `TransformReceipt`.

## [01]-[INDEX]

- [01]-[TRANSFORM]: 1-D-or-n-D complex DFT with optional inverse round trip, half-spectrum real DFT, discrete cosine/sine transforms, and the analytic-signal envelope on one `Transform` owner over `scipy.fft` + `scipy.signal.hilbert`.

## [02]-[TRANSFORM]

- Owner: `Transform` — the ONE frequency-domain transform owner; `TransformOp` discriminates the rows `Fourier(real, axes, invert)` (`scipy.fft.fft`/`ifft` complex DFT, `scipy.fft.fftn`/`ifftn` multi-axis complex DFT over the `axes` tuple, and `scipy.fft.rfft`/`irfft` half-spectrum real DFT along the lead axis — `real` selects the half-spectrum entrypoints, a non-empty `axes` tuple selects the complex n-D entrypoints (the half-spectrum path stays single-axis along `axes[0]`), and `invert` adds the inverse round trip within the same arm), `Trigonometric(kind, variant)` (`scipy.fft.dct`/`dst` discrete cosine/sine transforms, `TrigKind` selecting cosine/sine and `variant` the I-IV pocketfft `type`), and `Analytic()` (`scipy.signal.hilbert` FFT-backed analytic signal), all as rows on the same owner, never a per-transform method family and never a second transform surface. The `fftfreq`/`rfftfreq` frequency-grid helpers are composed inside the Fourier fold to read the dominant band, never standalone ops.
- Entry: `Transform.apply` returns `RuntimeRail[TransformReceipt]`; the Fourier path selects one of the six pocketfft entrypoints from `(real, axes)`, builds the matching `fftfreq`/`rfftfreq` grid against the sample spacing along the first transformed axis, reads the dominant frequency at the peak magnitude, and reads the total Parseval energy; when `invert` is set it runs the matching `ifft`/`ifftn`/`irfft` over the spectrum it already holds and folds the normalized round-trip residual in the same arm rather than re-deriving the forward transform. The trigonometric path runs the orthonormal `dct`/`dst` of the requested type and folds the fraction of total spectral energy held by the leading coefficients through one descending `cumsum`; the analytic path runs `hilbert`, reads the mean envelope as the analytic-signal magnitude and the instantaneous frequency from the unwrapped-phase derivative scaled by the sample rate, and reads the dominant instantaneous band. Every backend array is admitted through `numerics/array.md#PAYLOAD` before the transform, and a non-finite input returns the boundary fault that admission raises.
- Receipt: `TransformReceipt.contribute` emits one `Receipt.of("emitted", ...)` row; the dominant frequency and total spectral energy are the DFT evidence, the `energy_concentration` is the trigonometric compaction evidence, and the `mean_envelope`, `instantaneous_hz`, and `reconstruction_residual` are the analytic-signal and inverse-round-trip evidence a study run records through `experiments/study.md#STUDY`.
- Packages: `scipy` (`fft.fft`, `fft.ifft`, `fft.fftn`, `fft.ifftn`, `fft.rfft`, `fft.irfft`, `fft.dct`, `fft.dst`, `fft.fftfreq`, `fft.rfftfreq`, `signal.hilbert`), `numpy` (`asarray`, `abs`, `argmax`, `angle`, `unwrap`, `diff`, `sort`, `cumsum`, `sum`, `mean`, `median`, `linalg.norm`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor`).
- Growth: a new transform is one `TransformOp` case; an n-D spectrum is a non-empty `axes` value on the existing `Fourier` row; a new trigonometric variant is one `TrigKind` row or `variant` value; zero new surface.
- Boundary: in-memory frequency-domain transforms only — the 1-D and n-D DFT family, the trigonometric transforms, and the analytic-signal envelope are in-scope; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners, and the stationary Welch spectral estimate, polyphase resampling, and the multiresolution wavelet fold stay on `analysis/signal.md#DSP`. `scipy` carries the companion `python_version<'3.15'` band (no cp315 wheel), so every body is authored against the documented API on the gated band; scipy 1.17 makes `scipy.fft` Array-API-aware so the ops admit any backend array from the payload admission. Perfect inversion `ifft(fft(x)) ≈ x` and `ifftn(fftn(x)) ≈ x` (pocketfft's documented exact round trip) is the verification invariant the `invert` fold records as the reconstruction residual on the same arm that produced the spectrum.

```python signature
from enum import StrEnum
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


class TrigKind(StrEnum):
    COSINE = "cosine"
    SINE = "sine"


class TransformReceipt(Struct, frozen=True):
    op: str
    dominant_hz: float
    spectral_energy: float
    length: int
    energy_concentration: float = 0.0
    mean_envelope: float = 0.0
    instantaneous_hz: float = 0.0
    reconstruction_residual: float = 0.0

    def contribute(self) -> Receipt:
        facts = {"op": self.op, "dominant_hz": f"{self.dominant_hz:.3f}", "spectral_energy": f"{self.spectral_energy:.3e}"}
        if self.energy_concentration:
            facts["energy_concentration"] = f"{self.energy_concentration:.3f}"
        if self.mean_envelope:
            facts["mean_envelope"] = f"{self.mean_envelope:.3e}"
        if self.instantaneous_hz:
            facts["instantaneous_hz"] = f"{self.instantaneous_hz:.3f}"
        if self.reconstruction_residual:
            facts["reconstruction_residual"] = f"{self.reconstruction_residual:.3e}"
        return Receipt.of("emitted", "compute.transform", self.op, facts)


@tagged_union(frozen=True)
class TransformOp:
    tag: Literal["fourier", "trigonometric", "analytic"] = tag()
    fourier: tuple[bool, tuple[int, ...], bool] = case()
    trigonometric: tuple[TrigKind, int] = case()
    analytic: tuple[()] = case()

    @staticmethod
    def Fourier(real: bool = False, axes: tuple[int, ...] = (), invert: bool = False) -> "TransformOp":
        return TransformOp(fourier=(real, axes, invert))

    @staticmethod
    def Trigonometric(kind: TrigKind = TrigKind.COSINE, variant: int = 2) -> "TransformOp":
        return TransformOp(trigonometric=(kind, variant))

    @staticmethod
    def Analytic() -> "TransformOp":
        return TransformOp(analytic=())


def apply(samples: np.ndarray, fs: float, op: TransformOp) -> "RuntimeRail[TransformReceipt]":
    return boundary(f"transform.{op.tag}", lambda: _apply(samples, fs, op))


def _apply(samples: np.ndarray, fs: float, op: TransformOp) -> TransformReceipt:
    import scipy.fft as fft
    import scipy.signal as sig

    x = np.asarray(samples)
    spacing = 1.0 / fs
    match op:
        case TransformOp(tag="fourier", fourier=(real, axes, invert)):
            lead = axes[0] if axes else (x.ndim - 1)
            spectrum = (fft.rfft(x, axis=lead) if real else fft.fftn(x, axes=axes)) if axes else (fft.rfft(x) if real else fft.fft(x))
            freqs = fft.rfftfreq(x.shape[lead], d=spacing) if real else fft.fftfreq(x.shape[lead], d=spacing)
            magnitude = np.abs(spectrum)
            spine = magnitude if magnitude.ndim == 1 else np.sum(magnitude, axis=tuple(a for a in range(magnitude.ndim) if a != lead))
            dominant = float(np.abs(freqs[int(np.argmax(spine))]))
            energy = float(np.sum(magnitude ** 2))
            residual = 0.0
            if invert:
                rebuilt = (fft.irfft(spectrum, n=x.shape[lead], axis=lead) if real else fft.ifftn(spectrum, axes=axes)) if axes else (fft.irfft(spectrum, n=x.size) if real else fft.ifft(spectrum))
                residual = float(np.linalg.norm((np.real(rebuilt) - x).ravel()) / (np.linalg.norm(x.ravel()) + 1e-30))
            return TransformReceipt("fourier", dominant, energy, x.size, reconstruction_residual=residual)
        case TransformOp(tag="trigonometric", trigonometric=(kind, variant)):
            coeffs = fft.dst(x, type=variant, norm="ortho") if kind is TrigKind.SINE else fft.dct(x, type=variant, norm="ortho")
            energy = np.abs(coeffs) ** 2
            total = float(np.sum(energy)) + 1e-30
            leading = max(1, coeffs.size // 10)
            concentration = float(np.cumsum(np.sort(energy)[::-1])[leading - 1] / total)
            dominant = float(np.argmax(energy) * fs / (2 * x.size))
            return TransformReceipt(kind.value, dominant, total, x.size, energy_concentration=concentration)
        case TransformOp(tag="analytic", analytic=()):
            analytic = sig.hilbert(x)
            envelope = np.abs(analytic)
            phase = np.unwrap(np.angle(analytic))
            inst = np.diff(phase) * fs / (2.0 * np.pi)
            dominant = float(np.median(inst)) if inst.size else 0.0
            return TransformReceipt(
                "analytic",
                dominant,
                float(np.sum(envelope ** 2)),
                x.size,
                mean_envelope=float(np.mean(envelope)),
                instantaneous_hz=dominant,
            )
        case unreachable:
            assert_never(unreachable)
```

## [03]-[RESEARCH]

- [SCIPY_FFT]: the `scipy.fft.fft`/`ifft`/`fftn`/`ifftn`/`rfft`/`irfft`/`dct`/`dst`/`fftfreq`/`rfftfreq` spellings carry the `python_version<'3.15'` marker and verify against the `scipy.fft` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` under a uv-sync reflection pass once the scipy wheel resolves. The single `Fourier` arm selects among the six entrypoints from `(real, axes)` and runs the matching inverse over the spectrum it holds only when `invert` is set, so the forward read is never re-derived for the round trip. scipy 1.17 makes `scipy.fft` Array-API-aware, so the op admits any backend array resolved through `numerics/array.md#PAYLOAD`. Perfect inversion `ifft(fft(x)) ≈ x` and `ifftn(fftn(x)) ≈ x` is the verification invariant the `invert` fold records as the reconstruction residual.
- [SCIPY_HILBERT]: `scipy.signal.hilbert(x, N, axis)` returns the FFT-backed complex analytic signal and verifies against the `scipy.signal` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md`; the envelope is the analytic magnitude `np.abs`, the instantaneous frequency the unwrapped-phase derivative scaled by `fs / (2π)`.
