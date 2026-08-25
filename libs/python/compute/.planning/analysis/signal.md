# [PY_COMPUTE_SIGNAL]

`SignalOp` is the one classical signal-analysis operation owner, discriminating the `scipy.signal` stationary rows — zero-phase IIR/FIR filtering, `welch`/`spectrogram` estimation, polyphase resampling, `find_peaks` structure — beside the `pywt` multiresolution rows — decimated/stationary decomposition with optional coefficient-shrink denoise, additive `mra` bands, the CWT scalogram, the frequency-ordered packet tree — folded through the single `apply` entry, so a transient or non-stationary mode the Welch estimate averages away is first-class evidence on the same owner, never a per-transform method family. `WaveformExchange` writes the admitted two-axis float corpus through the native HDF5 seam the C# signal lane consumes; it is the boundary producer, not a second analysis owner. Output is parameterized as tightly as input: `SignalEvidence` discriminates one carrier per evidence shape and the thin `SignalReceipt` holds the union whole. No learned or neural filter enters this owner.

Operands admit through `numerics/array#PAYLOAD` for the finite gate and the operand `ContentKey`; every PSD-bearing op reads its dominant band through the reused `SpectralReadout` axis from `analysis/transform#TRANSFORM` under that axis's linear-amplitude contract; the resolved receipt is the `ReceiptContributor` the weave harvest and the study spine consume. Both paths ride one numpy floor — `scipy.signal` entrypoints stay out-of-scope or skip-backend for jax/dask/torch and `pywt` carries no Array-API contract — so every body opens on `np.asarray` over the runtime thread band under the `RELEASING` trait. Receipts key the RESULT: `SignalOp.identity_parts` hands op tag, payload rows, sample rate, and operand key to `IdentitySource(parts=...)` as N SEMANTIC fields, so the count-and-length framing runs at the identity owner and distinct operations over one operand carry distinct receipt keys.

## [01]-[INDEX]

- [02]-[DSP]: the stationary `scipy.signal` rows beside the `pywt` wavelet rows on one `SignalOp` owner, evidence discriminated over `SignalEvidence`.
- [03]-[WAVEFORM_EXCHANGE]: native HDF5 publication of finite `[samples, channels]` float corpora for the C# interchange reader.

## [02]-[DSP]

- Owner: `SignalOp` — the one operation union `apply` folds; the stationary and wavelet families are rows of one dispatch, `SpectralForm` is the bounded estimation-form axis on the spectral case (never a `time_frequency` boolean), and `readout` is a row field so each PSD-bearing op carries its own band projection rather than a fixed `argmax`. `Scalogram` reads the dominant frequency off the `(coefs, freqs)` return `pywt.cwt` already carries under `sampling_period`, never a separate `scale2frequency` call.
- Output: `SignalEvidence` gives `peaks` its own case, so a mean prominence never rides the `band_power` slot behind a meaningless `dominant_hz=0.0`, and an empty peak set carries `Nothing` rather than a fabricated mean; the receipt carries the union whole through each shape's `facts()` projection, one step denser than the sibling projections that flatten evidence into a fixed receipt shape. Wavelet rows report a dominant frequency and leave `band_power` to the spectral evidence.
- Faults: one `SIGNAL_APPLY` fence row spans every op — the tag is a span fact, never eight subject spellings — and its `catch` names the probed raise set: `ValueError` for every scipy parameter and pywt wavelet/level/mode refusal, `TypeError` for a non-numeric rate reaching `welch`, `np.linalg.LinAlgError` leading as the narrower subclass.
- Packages: `scipy.signal`, `pywt`, and `numpy` per the fence imports; `numerics/array#PAYLOAD` owns namespace resolution and the finite gate at admission, so this owner threads neither and needs no `array-api-extra` `nan_to_num` — a non-finite operand never reaches a body.
- Growth: a new transform is one `SignalOp` case with its `identity_parts` arm — `assert_never` surfaces the omission at type-check; a new filter family is one `FilterKind` row; a new decomposition mode is one `DecompMode` row with its `WAVELET_ROUTES` triple; a new shrink rule is one `ThresholdMode` row owning its callable; a new band projection is one `SpectralReadout` row in the transform owner every PSD-bearing op inherits; a new evidence shape is one `SignalEvidence` case with its `facts()` arm.

```python
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache
from math import gcd, isfinite
from typing import TYPE_CHECKING, Final, Literal, assert_never

import numpy as np
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.analysis.transform import SpectralReadout
from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.roots import ResourceRef
from rasm.runtime.workers import Kernel, KernelTrait

lazy import pywt
lazy import h5py
lazy import scipy.signal as sig

if TYPE_CHECKING:
    from types import ModuleType

    from numpy.typing import NDArray

    type Array = NDArray[np.floating]

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


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class SignalEvidence:
    tag: Literal["spectral", "peaks", "multiresolution", "scale", "packet"] = tag()
    spectral: tuple[float, float] = case()
    peaks: tuple[int, Option[float]] = case()
    multiresolution: tuple[tuple[float, ...], float, ThresholdMode] = case()
    scale: tuple[float, float] = case()
    packet: tuple[tuple[float, ...], float] = case()

    @staticmethod
    def Spectral(dominant_hz: float, band_power: float) -> "SignalEvidence":
        return SignalEvidence(spectral=(dominant_hz, band_power))

    @staticmethod
    def Peaks(count: int, mean_prominence: Option[float]) -> "SignalEvidence":
        return SignalEvidence(peaks=(count, mean_prominence))

    @staticmethod
    def Multiresolution(level_energy: tuple[float, ...], residual: float, shrink: ThresholdMode) -> "SignalEvidence":
        return SignalEvidence(multiresolution=(level_energy, residual, shrink))

    @staticmethod
    def Scale(dominant_scale: float, dominant_hz: float) -> "SignalEvidence":
        return SignalEvidence(scale=(dominant_scale, dominant_hz))

    @staticmethod
    def Packet(node_energy: tuple[float, ...], dominant_hz: float) -> "SignalEvidence":
        return SignalEvidence(packet=(node_energy, dominant_hz))

    def facts(self) -> dict[str, object]:
        match self:
            case SignalEvidence(tag="spectral", spectral=(hz, power)):
                return {"dominant_hz": hz, "band_power": power}
            case SignalEvidence(tag="peaks", peaks=(count, prominence)):
                return {"peaks": count, **prominence.map(lambda mean: {"mean_prominence": mean}).default_value({})}
            case SignalEvidence(tag="multiresolution", multiresolution=(energy, residual, shrink)):
                return {"level_energy": energy, "reconstruction_residual": residual, "shrink": shrink}
            case SignalEvidence(tag="scale", scale=(scale, hz)):
                return {"dominant_scale": scale, "dominant_hz": hz}
            case SignalEvidence(tag="packet", packet=(energy, hz)):
                return {"node_energy": energy, "dominant_hz": hz}
            case _ as unreachable:
                assert_never(unreachable)


class SignalReceipt(Struct, frozen=True):
    op: str
    length: int
    lineage: Provenance
    evidence: SignalEvidence

    @staticmethod
    def of(op: str, length: int, lineage: Provenance, evidence: SignalEvidence) -> "SignalReceipt":
        return SignalReceipt(op, length, lineage, evidence)

    @property
    def content_key(self) -> ContentKey:
        return self.lineage.produced

    def contribute(self) -> Iterable[Receipt]:
        facts = {"op": self.op, "length": self.length, **self.evidence.facts()}
        yield Receipt.of(
            EvidenceScope.SIGNAL.value,
            ("emitted", self.op, facts),
            key=Some(self.lineage.produced),
            provenance=Some(self.lineage),
        )


@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "peaks", "resample", "decompose", "multiresolution", "scalogram", "packet"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int, SpectralReadout] = case()
    spectral: tuple[int, SpectralForm, SpectralReadout] = case()
    peaks: tuple[float, int] = case()
    resample: tuple[float, SpectralReadout] = case()
    decompose: tuple[str, int, DecompMode, ThresholdMode] = case()
    multiresolution: tuple[str, int] = case()
    scalogram: tuple[str, tuple[float, ...], int] = case()
    packet: tuple[str, int, SpectralReadout] = case()

    @staticmethod
    def Filter(kind: FilterKind, cutoff: tuple[float, ...], order: int = 4, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(filter=(kind, cutoff, order, readout))

    @staticmethod
    def Spectral(nperseg: int = 256, form: SpectralForm = SpectralForm.WELCH, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(spectral=(nperseg, form, readout))

    @staticmethod
    def Peaks(prominence: float = 0.0, distance: int = 1) -> "SignalOp":
        return SignalOp(peaks=(prominence, distance))

    @staticmethod
    def Resample(target_rate: float, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(resample=(target_rate, readout))

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
    def Packet(wavelet: str = "db4", maxlevel: int = 3, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(packet=(wavelet, maxlevel, readout))

    def identity_parts(self, fs: float, operand_key: ContentKey) -> tuple[bytes, ...]:
        row: tuple[object, ...]
        match self:
            case SignalOp(tag="filter", filter=(kind, cutoff, order, readout)):
                row = (kind.value, *cutoff, order, readout.value)
            case SignalOp(tag="spectral", spectral=(nperseg, form, readout)):
                row = (nperseg, form.value, readout.value)
            case SignalOp(tag="peaks", peaks=(prominence, distance)):
                row = (prominence, distance)
            case SignalOp(tag="resample", resample=(target, readout)):
                row = (target, readout.value)
            case SignalOp(tag="decompose", decompose=(wavelet, level, mode, denoise)):
                row = (wavelet, level, mode.value, denoise.value)
            case SignalOp(tag="multiresolution", multiresolution=(wavelet, level)):
                row = (wavelet, level)
            case SignalOp(tag="scalogram", scalogram=(wavelet, scales, resolution)):
                row = (wavelet, *scales, resolution)
            case SignalOp(tag="packet", packet=(wavelet, maxlevel, readout)):
                row = (wavelet, maxlevel, readout.value)
            case _ as unreachable:
                assert_never(unreachable)
        return (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            np.float64(fs).tobytes(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )


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


def _welch_band(sig: "ModuleType", x: "Array", fs: float, readout: SpectralReadout, nperseg: int = 256) -> tuple[float, float]:
    f, pxx = sig.welch(x, fs=fs, nperseg=min(nperseg, x.size))
    df = float(f[1] - f[0]) if f.shape[0] > 1 else 1.0
    return readout.fold(f, np.sqrt(pxx)), float(np.sum(pxx)) * df


def _coeff_energy(coeffs: Iterable[np.ndarray]) -> tuple[float, ...]:
    return tuple(float(np.einsum("i,i->", np.ravel(c), np.ravel(c))) for c in coeffs)


def _mad_threshold(detail_finest: np.ndarray, n: int) -> float:
    sigma = float(np.median(np.abs(np.asarray(detail_finest))) / 0.6745)
    return sigma * float(np.sqrt(2.0 * np.log(n)))


def _signal_kernel(samples: object, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"signal.{op.tag}", IdentitySource(parts=op.identity_parts(fs, payload.content_key))).bind(
            lambda result_key: boundary(
                SIGNAL_APPLY,
                lambda: _apply(samples, fs, op, Provenance(consumed=Block.singleton(payload.content_key), produced=result_key)),
                catch=(np.linalg.LinAlgError, ValueError, TypeError),
            )
        )
    )


async def apply(samples: object, fs: float, op: SignalOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[SignalReceipt]":
    async def dispatch() -> RuntimeRail[SignalReceipt]:
        return (await lane.offload(Kernel.of(_signal_kernel, KernelTrait.RELEASING), samples, fs, op)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.SIGNAL, f"signal.{op.tag}", dispatch, facts={"op": op.tag, "fs": fs}, composition=composition)


def _apply(samples: object, fs: float, op: SignalOp, lineage: Provenance) -> SignalReceipt:
    xn = np.asarray(samples)
    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order, readout)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            dominant, power = _welch_band(sig, sig.sosfiltfilt(sos, xn), fs, readout)
            return SignalReceipt.of("filter", xn.size, lineage, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="spectral", spectral=(nperseg, form, readout)):
            if form is SpectralForm.SPECTROGRAM:
                sf, _, sxx = sig.spectrogram(xn, fs=fs, nperseg=min(nperseg, xn.size))
                spine = np.sum(sxx, axis=-1)
                return SignalReceipt.of("spectral", xn.size, lineage, SignalEvidence.Spectral(readout.fold(sf, np.sqrt(spine)), float(np.sum(sxx))))
            dominant, power = _welch_band(sig, xn, fs, readout, nperseg)
            return SignalReceipt.of("spectral", xn.size, lineage, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="peaks", peaks=(prominence, distance)):
            idx, props = sig.find_peaks(xn, prominence=prominence or None, distance=distance or None)
            mean_prom = Some(float(np.mean(props["prominences"]))) if idx.shape[0] else Nothing
            return SignalReceipt.of("peaks", xn.size, lineage, SignalEvidence.Peaks(int(idx.shape[0]), mean_prom))
        case SignalOp(tag="resample", resample=(target, readout)):
            g = gcd(int(round(target)), int(round(fs)))
            out = sig.resample_poly(xn, int(round(target)) // g, int(round(fs)) // g)
            dominant, power = _welch_band(sig, out, target, readout)
            return SignalReceipt.of("resample", int(out.shape[0]), lineage, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="decompose", decompose=(wavelet, level, mode, denoise)):
            return _decompose(wavelet, level, mode, denoise, xn, lineage)
        case SignalOp(tag="multiresolution", multiresolution=(wavelet, level)):
            bands = pywt.mra(xn, wavelet, level=level or None, transform="swt")
            residual = float(np.linalg.norm(np.einsum("bi->i", np.asarray(bands)) - xn) / (np.linalg.norm(xn) + 1e-30))
            return SignalReceipt.of(
                "multiresolution", xn.size, lineage, SignalEvidence.Multiresolution(_coeff_energy(bands), residual, ThresholdMode.NONE)
            )
        case SignalOp(tag="scalogram", scalogram=(wavelet, scales, resolution)):
            grid = np.asarray(scales) if scales else np.logspace(0.0, np.log(0.5 * xn.size), resolution, base=np.e)
            coefs, freqs = pywt.cwt(xn, grid, wavelet, sampling_period=1.0 / fs, method="fft")
            mag = np.abs(np.asarray(coefs))
            peak = int(np.argmax(np.einsum("st,st->s", mag, mag)))
            return SignalReceipt.of("scalogram", xn.size, lineage, SignalEvidence.Scale(float(grid[peak]), float(np.asarray(freqs)[peak])))
        case SignalOp(tag="packet", packet=(wavelet, maxlevel, readout)):
            tree = pywt.WaveletPacket(data=xn, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            leaves = tree.get_level(maxlevel, order="freq")
            energy = _coeff_energy(node.data for node in leaves)
            centres = (np.arange(len(energy)) + 0.5) * (0.5 * fs / len(energy))
            band = readout.fold(centres, np.sqrt(np.asarray(energy)))
            return SignalReceipt.of("packet", xn.size, lineage, SignalEvidence.Packet(energy, band))
        case _ as unreachable:
            assert_never(unreachable)


def _decompose(wavelet: str, level: int, mode: DecompMode, denoise: ThresholdMode, x: np.ndarray, lineage: Provenance) -> SignalReceipt:
    forward, inverse, max_level = _wavelet_routes()[mode]
    depth = level or max_level(x.size, pywt.Wavelet(wavelet))
    coeffs = forward(x, wavelet, depth)
    shrink = denoise.shrink(pywt, _mad_threshold(coeffs[-1], x.size))
    rebuilt = inverse(coeffs if shrink is None else [coeffs[0], *map(shrink, coeffs[1:])], wavelet)[: x.size]
    residual = float(np.linalg.norm(rebuilt - x) / (np.linalg.norm(x) + 1e-30))
    return SignalReceipt.of("decompose", x.size, lineage, SignalEvidence.Multiresolution(_coeff_energy(coeffs), residual, denoise))


# --- [WAVEFORM_EXCHANGE] ----------------------------------------------------------------


class WaveformExchange:
    """The native `[samples, channels]` HDF5 publisher consumed by C# `ImportWaveforms`."""

    @staticmethod
    def write(ref: ResourceRef, samples: object, sample_rate: float) -> "RuntimeRail[int]":
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
