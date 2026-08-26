# [PY_COMPUTE_SIGNAL]

`SignalOp` folds SciPy filtering, spectral estimation, resampling, and peak detection beside PyWavelets decomposition, denoising, additive bands, CWT, and packet analysis through `apply`, returning each provider product directly. `WaveformExchange` writes the admitted two-axis float corpus through the native HDF5 boundary the C# signal lane consumes.

Operands admit through `numerics/array#PAYLOAD` for the finite gate. Every operation returns the provider product it computes: filtered or resampled samples, spectral axes and densities, peak indices and properties, wavelet coefficients and reconstruction, additive bands, CWT coefficients and frequencies, or the packet tree. Both paths ride one numpy floor — `scipy.signal` entrypoints stay out-of-scope or skip-backend for jax/dask/torch and `pywt` carries no Array-API contract — so every body opens on `np.asarray` over the runtime thread band under the `RELEASING` trait.

## [01]-[INDEX]

- [02]-[DSP]: `SignalOp` folds the stationary `scipy.signal` rows beside the `pywt` wavelet rows, returning their products directly.
- [03]-[WAVEFORM_EXCHANGE]: native HDF5 publication of finite `[samples, channels]` float corpora for the C# interchange reader.

## [02]-[DSP]

- Owner: `SignalOp` — the one operation union `apply` folds; the stationary and wavelet families are rows of one dispatch, and `SpectralForm` is the bounded estimation-form axis on the spectral case.
- Output: Each operation returns the native array, coefficient tuple, property map, or packet tree its provider computes.
- Faults: one `SIGNAL_APPLY` fence row spans every op — the tag is a span fact, never eight subject spellings — and its `catch` names the probed raise set: `ValueError` for every scipy parameter and pywt wavelet/level/mode refusal, `TypeError` for a non-numeric rate reaching `welch`, `np.linalg.LinAlgError` leading as the narrower subclass.
- Packages: `scipy.signal`, `pywt`, and `numpy` per the fence imports; `numerics/array#PAYLOAD` owns namespace resolution and the finite gate at admission, so this owner threads neither and needs no `array-api-extra` `nan_to_num` — a non-finite operand never reaches a body.
- Growth: a new transform is one `SignalOp` case and one `apply` arm; a new filter family is one `FilterKind` row; a new decomposition mode is one `DecompMode` row with its `WAVELET_ROUTES` triple; a new shrink rule is one `ThresholdMode` row owning its callable.

```python
from collections.abc import Callable
from enum import StrEnum
from functools import cache
from math import gcd, isfinite
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Block, Map

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, boundary, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.roots import ResourceRef
from rasm.runtime.workers import Kernel, KernelTrait

lazy import pywt
lazy import h5py
lazy import scipy.signal as sig

if TYPE_CHECKING:
    from pywt import WaveletPacket

# --- [TYPES] ----------------------------------------------------------------------------

type WaveletRoute = tuple[
    Callable[..., list],
    Callable[..., np.ndarray],
    Callable[..., int],
]


class FilterKind(StrEnum):
    LOWPASS = "lowpass"
    HIGHPASS = "highpass"
    BANDPASS = "bandpass"
    BANDSTOP = "bandstop"


class SpectralForm(StrEnum):
    WELCH = "welch"
    SPECTROGRAM = "spectrogram"


class DecompMode(StrEnum):
    DECIMATED = "decimated"
    STATIONARY = "stationary"


class ThresholdMode(StrEnum):
    NONE = "none"
    SOFT = "soft"
    HARD = "hard"
    GARROTE = "garrote"
    GREATER = "greater"
    LESS = "less"
    FIRM = "firm"

    def shrink(self, pywt: object, value: float) -> Callable[[np.ndarray], np.ndarray] | None:
        match self:
            case ThresholdMode.NONE:
                return None
            case ThresholdMode.FIRM:
                return lambda c: pywt.threshold_firm(c, value, 2.0 * value)
            case _:
                return lambda c: pywt.threshold(c, value, mode=self.value)


@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "peaks", "resample", "decompose", "multiresolution", "scalogram", "packet"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int] = case()
    spectral: tuple[int, SpectralForm] = case()
    peaks: tuple[float, int] = case()
    resample: float = case()
    decompose: tuple[str, int, DecompMode, ThresholdMode] = case()
    multiresolution: tuple[str, int] = case()
    scalogram: tuple[str, tuple[float, ...], int] = case()
    packet: tuple[str, int] = case()

    @staticmethod
    def Filter(kind: FilterKind, cutoff: tuple[float, ...], order: int = 4) -> "SignalOp":
        return SignalOp(filter=(kind, cutoff, order))

    @staticmethod
    def Spectral(nperseg: int = 256, form: SpectralForm = SpectralForm.WELCH) -> "SignalOp":
        return SignalOp(spectral=(nperseg, form))

    @staticmethod
    def Peaks(prominence: float = 0.0, distance: int = 1) -> "SignalOp":
        return SignalOp(peaks=(prominence, distance))

    @staticmethod
    def Resample(target_rate: float) -> "SignalOp":
        return SignalOp(resample=target_rate)

    @staticmethod
    def Decompose(
        wavelet: str = "db4", level: int = 0, mode: DecompMode = DecompMode.DECIMATED, denoise: ThresholdMode = ThresholdMode.NONE
    ) -> "SignalOp":
        return SignalOp(decompose=(wavelet, level, mode, denoise))

    @staticmethod
    def Multiresolution(wavelet: str = "db4", level: int = 0) -> "SignalOp":
        return SignalOp(multiresolution=(wavelet, level))

    @staticmethod
    def Scalogram(wavelet: str = "morl", scales: tuple[float, ...] = (), resolution: int = 64) -> "SignalOp":
        return SignalOp(scalogram=(wavelet, scales, resolution))

    @staticmethod
    def Packet(wavelet: str = "db4", maxlevel: int = 3) -> "SignalOp":
        return SignalOp(packet=(wavelet, maxlevel))


# --- [TABLES] ---------------------------------------------------------------------------

SIGNAL_APPLY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.SIGNAL, point="apply", arm="boundary", defect="kernel-refused", retriability=TERMINAL
)
WAVEFORM_WRITE: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.SIGNAL, point="waveform_write", arm="boundary", defect="waveform-write", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([SIGNAL_APPLY, WAVEFORM_WRITE]))


@cache
def _wavelet_routes() -> "Map[DecompMode, WaveletRoute]":
    return Map.of_seq([
        (
            DecompMode.DECIMATED,
            (lambda x, wavelet, level: pywt.wavedec(x, wavelet, level=level), pywt.waverec, lambda n, w: pywt.dwt_max_level(n, w.dec_len)),
        ),
        (
            DecompMode.STATIONARY,
            (lambda x, wavelet, level: pywt.swt(x, wavelet, level=level, trim_approx=True), pywt.iswt, lambda n, _w: pywt.swt_max_level(n)),
        ),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _mad_threshold(detail_finest: np.ndarray, n: int) -> float:
    sigma = float(np.median(np.abs(np.asarray(detail_finest))) / 0.6745)
    return sigma * float(np.sqrt(2.0 * np.log(n)))


def _signal_kernel(
    samples: object, fs: float, op: SignalOp
) -> "RuntimeResult[np.ndarray | tuple[np.ndarray | tuple[np.ndarray, ...] | dict[str, np.ndarray], ...] | WaveletPacket]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda _: boundary(
            SIGNAL_APPLY,
            lambda: _apply(samples, fs, op),
            catch=(np.linalg.LinAlgError, ValueError, TypeError),
        )
    )


async def apply(
    samples: object, fs: float, op: SignalOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeResult[np.ndarray | tuple[np.ndarray | tuple[np.ndarray, ...] | dict[str, np.ndarray], ...] | WaveletPacket]":
    async def dispatch() -> "RuntimeResult[np.ndarray | tuple[np.ndarray | tuple[np.ndarray, ...] | dict[str, np.ndarray], ...] | WaveletPacket]":
        return (await lane.offload(Kernel.of(_signal_kernel, KernelTrait.RELEASING), samples, fs, op)).bind(lambda held: held)

    return await evidence_run(EvidenceScope.SIGNAL, f"signal.{op.tag}", dispatch, facts={"op": op.tag, "fs": fs}, composition=composition)


def _apply(
    samples: object, fs: float, op: SignalOp
) -> "np.ndarray | tuple[np.ndarray | tuple[np.ndarray, ...] | dict[str, np.ndarray], ...] | WaveletPacket":
    xn = np.asarray(samples)
    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            return np.asarray(sig.sosfiltfilt(sos, xn))
        case SignalOp(tag="spectral", spectral=(nperseg, form)):
            if form is SpectralForm.SPECTROGRAM:
                frequencies, times, density = sig.spectrogram(xn, fs=fs, nperseg=min(nperseg, xn.size))
                return np.asarray(frequencies), np.asarray(times), np.asarray(density)
            frequencies, density = sig.welch(xn, fs=fs, nperseg=min(nperseg, xn.size))
            return np.asarray(frequencies), np.asarray(density)
        case SignalOp(tag="peaks", peaks=(prominence, distance)):
            indices, properties = sig.find_peaks(xn, prominence=prominence or None, distance=distance or None)
            return np.asarray(indices), {name: np.asarray(values) for name, values in properties.items()}
        case SignalOp(tag="resample", resample=target):
            g = gcd(int(round(target)), int(round(fs)))
            return np.asarray(sig.resample_poly(xn, int(round(target)) // g, int(round(fs)) // g))
        case SignalOp(tag="decompose", decompose=(wavelet, level, mode, denoise)):
            return _decompose(wavelet, level, mode, denoise, xn)
        case SignalOp(tag="multiresolution", multiresolution=(wavelet, level)):
            bands = pywt.mra(xn, wavelet, level=level or None, transform="swt")
            return tuple(np.asarray(band) for band in bands)
        case SignalOp(tag="scalogram", scalogram=(wavelet, scales, resolution)):
            grid = np.asarray(scales) if scales else np.logspace(0.0, np.log(0.5 * xn.size), resolution, base=np.e)
            coefficients, frequencies = pywt.cwt(xn, grid, wavelet, sampling_period=1.0 / fs, method="fft")
            return np.asarray(coefficients), np.asarray(frequencies)
        case SignalOp(tag="packet", packet=(wavelet, maxlevel)):
            tree = pywt.WaveletPacket(data=xn, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            return tree
        case _ as unreachable:
            assert_never(unreachable)


def _decompose(
    wavelet: str, level: int, mode: DecompMode, denoise: ThresholdMode, x: np.ndarray
) -> tuple[tuple[np.ndarray, ...], np.ndarray]:
    forward, inverse, max_level = _wavelet_routes()[mode]
    depth = level or max_level(x.size, pywt.Wavelet(wavelet))
    coeffs = forward(x, wavelet, depth)
    shrink = denoise.shrink(pywt, _mad_threshold(coeffs[-1], x.size))
    rebuilt = inverse(coeffs if shrink is None else [coeffs[0], *map(shrink, coeffs[1:])], wavelet)[: x.size]
    return tuple(np.asarray(coeff) for coeff in coeffs), np.asarray(rebuilt)


# --- [WAVEFORM_EXCHANGE] ----------------------------------------------------------------


class WaveformExchange:
    """The native `[samples, channels]` HDF5 publisher consumed by C# `ImportWaveforms`."""

    @staticmethod
    def write(ref: ResourceRef, samples: object, sample_rate: float) -> "RuntimeResult[int]":
        def landed(payload: ArrayPayload) -> int:
            values = np.asarray(samples)
            if len(payload.shape) != 2 or any(extent <= 0 for extent in payload.shape):
                raise ValueError(f"waveform shape: {payload.shape}")
            if not np.issubdtype(values.dtype, np.floating):
                raise TypeError(f"waveform dtype: {values.dtype}")
            if not isfinite(sample_rate) or sample_rate <= 0.0:
                raise ValueError(f"waveform sample rate: {sample_rate}")
            wire = np.asarray(values, dtype="<f4")
            sample_chunk = max(1, min(wire.shape[0], (1 << 16) // wire.shape[1]))
            with h5py.File(str(ref.path), "x") as file:
                dataset = file.create_dataset(
                    "waveform",
                    data=wire,
                    chunks=(sample_chunk, wire.shape[1]),
                    compression="gzip",
                    compression_opts=4,
                    shuffle=True,
                )
                dataset.attrs["sample-rate"] = np.asarray(sample_rate, dtype="<f8")
            return ref.path.stat().st_size

        return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
            lambda payload: boundary(
                WAVEFORM_WRITE,
                lambda: landed(payload),
                catch=(OSError, OverflowError, TypeError, ValueError),
            )
        )
```

## [03]-[WAVEFORM_EXCHANGE]

- Owner: `WaveformExchange` — the Python producer paired with `dotnet:Rasm.Compute/Runtime/field#SCIENTIFIC_INGEST` `InterchangeIo.ImportWaveforms`; it emits one fixed `/waveform` dataset and no format registry or alternate carrier.
- Entry: `write(ref, samples, sample_rate)` admits finiteness through `ArrayPayload`, requires a non-empty two-axis floating operand and a finite positive rate, writes create-only HDF5, and returns the flushed byte extent.
- Wire: `/waveform` is little-endian float32 `[samples, channels]`, chunked along samples with the whole channel axis, Shuffle then gzip level 4; its only attribute is little-endian float64 `sample-rate`, and the root owns no attributes.
- Packages: `h5py` (`File`, `create_dataset`, dataset attributes), `numpy`, and the `numerics/array#PAYLOAD` admission substrate.
- Boundary: matrix coefficients, long SHM records, and reference spectra share this two-axis carrier because the C# reader frames them under its own admitted window. A Python-side frame array, arbitrary dataset-name knob, second HDF owner, or hidden structure metadata is rejected.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
