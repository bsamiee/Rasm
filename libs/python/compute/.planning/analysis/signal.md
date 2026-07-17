# [PY_COMPUTE_SIGNAL]

`Signal` is the one classical signal-analysis owner: `SignalOp` discriminates the `scipy.signal` stationary rows — zero-phase IIR/FIR filtering, `welch`/`spectrogram` estimation, polyphase resampling, `find_peaks` structure — beside the `pywt` multiresolution rows — decimated/stationary decomposition with optional coefficient-shrink denoise, additive `mra` bands, the CWT scalogram, the frequency-ordered packet tree — on a single `Signal` surface, so a transient or non-stationary mode the Welch estimate averages away is first-class evidence on the same owner, never a per-transform method family. Output is parameterized as tightly as input: `SignalEvidence` discriminates one carrier per evidence shape and the thin `SignalReceipt` holds the union whole. No learned or neural filter enters this owner.

Operands admit through `numerics/array.md#PAYLOAD` for the finite gate and the operand `ContentKey`; every PSD-bearing op reads its dominant band through the reused `SpectralReadout` axis from `analysis/transform.md#TRANSFORM` under that axis's linear-amplitude contract; the resolved receipt is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect. Both paths ride one numpy floor — scipy 1.17 lists this owner's `scipy.signal` entrypoints as out-of-scope or skip-backend for jax/dask/torch and `pywt` carries no Array-API contract — so every body opens on `np.asarray` over the runtime thread band under the `RELEASING` trait. Receipts key the RESULT: `SignalOp.identity_buffer` folds op tag, payload rows, sample rate, and operand key into one `ContentIdentity.of` derivation, so distinct operations over one operand carry distinct receipt keys.

## [01]-[INDEX]

- [01]-[DSP]: the stationary `scipy.signal` rows beside the `pywt` wavelet rows on one `Signal` owner, evidence discriminated over `SignalEvidence`.

## [02]-[DSP]

- Owner: `Signal` — the one owner over `SignalOp`; the stationary and wavelet families are rows of one dispatch, and `readout` is a row field so each PSD-bearing op carries its own band projection rather than a fixed `argmax`. `Scalogram` reads the dominant frequency off the `(coefs, freqs)` return `pywt.cwt` already carries under `sampling_period`, never a separate `scale2frequency` call.
- Output: `SignalEvidence` gives `peaks` its own case, so a mean prominence never rides the `band_power` slot behind a meaningless `dominant_hz=0.0`; the receipt carries the union whole through each shape's `facts()` projection, one step denser than the sibling projections that flatten evidence into a fixed receipt shape. Wavelet rows report a dominant frequency and leave `band_power` to the spectral evidence.
- Packages: `scipy.signal`, `pywt`, and `numpy` per the fence imports; `numerics/array.md#PAYLOAD` owns namespace resolution and the finite gate at admission, so this owner threads neither and needs no `array-api-extra` `nan_to_num` — a non-finite operand never reaches a body.
- Growth: a new transform is one `SignalOp` case plus its `identity_buffer` arm — `assert_never` surfaces the omission at type-check; a new filter family is one `FilterKind` row; a new decomposition mode is one `DecompMode` row plus its `WAVELET_ROUTES` triple; a new shrink rule is one `ThresholdMode` row owning its callable; a new band projection is one `SpectralReadout` row in the transform owner every PSD-bearing op inherits; a new evidence shape is one `SignalEvidence` case plus its `facts()` arm.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache
from math import gcd
from typing import TYPE_CHECKING, Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.analysis.transform import SpectralReadout
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

if TYPE_CHECKING:
    # `ModuleType` types the boundary-imported `scipy.signal` handle the Welch projection takes.
    from types import ModuleType

    from numpy.typing import NDArray

    type Array = NDArray[np.floating]

# --- [TYPES] ----------------------------------------------------------------------------

type WaveletRoute = tuple[
    Callable[..., list],  # forward decomposition to the level cap (the nested coefficient list)
    Callable[..., np.ndarray],  # matching inverse over the (possibly shrunk) coefficient list
    Callable[..., int],  # maximum useful level for this decimation axis
]


class FilterKind(StrEnum):
    LOWPASS = "lowpass"
    HIGHPASS = "highpass"
    BANDPASS = "bandpass"
    BANDSTOP = "bandstop"


class DecompMode(StrEnum):
    DECIMATED = "decimated"  # pywt.wavedec / waverec, level cap pywt.dwt_max_level
    STATIONARY = "stationary"  # pywt.swt / iswt (shift-invariant, undecimated), cap swt_max_level


class ThresholdMode(StrEnum):
    # every row IS a pywt threshold rule with the shrink callable on the enum, so the denoise fold reads `mode.shrink(value)`
    # and the axis spans the single-knee `pywt.threshold` and two-knee `pywt.threshold_firm` surfaces as one row set.
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
    peaks: tuple[int, float] = case()
    multiresolution: tuple[tuple[float, ...], float, ThresholdMode] = case()
    scale: tuple[float, float] = case()
    packet: tuple[tuple[float, ...], float] = case()

    @staticmethod
    def Spectral(dominant_hz: float, band_power: float) -> "SignalEvidence":
        return SignalEvidence(spectral=(dominant_hz, band_power))

    @staticmethod
    def Peaks(count: int, mean_prominence: float) -> "SignalEvidence":
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
        # native scalars only — floats, ints, and float tuples ride the receipt slots directly so the
        # receipt layer aggregates and compares; a `str()`/`f""` coerce here erases comparability and
        # is the deleted form. Rendering to text is the export layer's concern, never a fact's.
        match self:
            case SignalEvidence(tag="spectral", spectral=(hz, power)):
                return {"dominant_hz": hz, "band_power": power}
            case SignalEvidence(tag="peaks", peaks=(count, prominence)):
                return {"peaks": count, "mean_prominence": prominence}
            case SignalEvidence(tag="multiresolution", multiresolution=(energy, residual, shrink)):
                # `ThresholdMode` member IS its `StrEnum` str — riding it whole keeps enum
                # comparability at the receipt layer; a `.value` projection is the deleted coerce.
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
    content_key: ContentKey
    evidence: SignalEvidence

    @staticmethod
    def of(op: str, length: int, key: ContentKey, evidence: SignalEvidence) -> "SignalReceipt":
        return SignalReceipt(op, length, key, evidence)

    def contribute(self) -> Iterable[Receipt]:
        facts = {"op": self.op, "length": self.length, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of("compute.signal", ("emitted", self.op, facts))


@tagged_union(frozen=True)
class SignalOp:
    tag: Literal["filter", "spectral", "peaks", "resample", "decompose", "multiresolution", "scalogram", "packet"] = tag()
    filter: tuple[FilterKind, tuple[float, ...], int, SpectralReadout] = case()
    spectral: tuple[int, bool, SpectralReadout] = case()
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
    def Spectral(nperseg: int = 256, time_frequency: bool = False, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(spectral=(nperseg, time_frequency, readout))

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
        # `resolution` sizes the geometric scale grid only when `scales` is empty; an explicit
        # `scales` tuple overrides it, so the grid density is a parameter, never a magic literal.
        return SignalOp(scalogram=(wavelet, scales, resolution))

    @staticmethod
    def Packet(wavelet: str = "db4", maxlevel: int = 3, readout: SpectralReadout = SpectralReadout.PEAK) -> "SignalOp":
        return SignalOp(packet=(wavelet, maxlevel, readout))

    def identity_buffer(self, fs: float, operand_key: ContentKey) -> bytes:
        # enum rows serialize by value, numeric rows as canonical float64 bytes; length-prefixed parts keep the buffer unambiguous.
        row: tuple[object, ...]
        match self:
            case SignalOp(tag="filter", filter=(kind, cutoff, order, readout)):
                row = (kind.value, *cutoff, order, readout.value)
            case SignalOp(tag="spectral", spectral=(nperseg, time_frequency, readout)):
                row = (nperseg, time_frequency, readout.value)
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
        parts = (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            np.float64(fs).tobytes(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )
        return b"".join(len(part).to_bytes(8, "big") + part for part in parts)


# --- [TABLES] ---------------------------------------------------------------------------


# one row per DecompMode: `_decompose` indexes the (forward, inverse, max-level) triple and runs one forward and one inverse body
# rather than a mode ternary repeated at decomposition and reconstruction. `@cache` defers the import-banned pywt load to first call.
# stationary forward trims via `trim_approx=True` so both axes return the same `[cAn, cDn, …, cD1]` list the shrink and inverse consume.
@cache
def _wavelet_routes() -> "Map[DecompMode, WaveletRoute]":
    import pywt

    # both forward rows share the (x, wavelet, level) positional contract — `level=` is keyword
    # because `wavedec`/`swt` take `mode` at the third positional slot; both inverse rows share
    # their (coeffs, wavelet) contract so `waverec`/`iswt` pass through bare.
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
    # one Welch projection every stationary arm folds through: the `(f, pxx)` PSD power spine square-roots to amplitude for the
    # fold per the `SpectralReadout` linear-amplitude contract, while band power stays the Parseval integral `np.sum(pxx) * df`;
    # `min(nperseg, x.size)` keeps a short trace off the `welch` length error.
    f, pxx = sig.welch(x, fs=fs, nperseg=min(nperseg, x.size))
    df = float(f[1] - f[0]) if f.shape[0] > 1 else 1.0
    return readout.fold(f, np.sqrt(pxx)), float(np.sum(pxx)) * df


def _coeff_energy(coeffs: Iterable[np.ndarray]) -> tuple[float, ...]:
    # per-band/per-node/per-scale magnitude-square contraction shared with the wavelet and
    # packet folds — `einsum("i,i->", ravel(c), ravel(c))` is the energy reduction owner.
    return tuple(float(np.einsum("i,i->", np.ravel(c), np.ravel(c))) for c in coeffs)


def _mad_threshold(detail_finest: np.ndarray, n: int) -> float:
    # VisuShrink universal threshold over the robust MAD noise estimate from the finest band.
    sigma = float(np.median(np.abs(np.asarray(detail_finest))) / 0.6745)
    return sigma * float(np.sqrt(2.0 * np.log(n)))


def _signal_kernel(samples: object, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    # module-level so REFERENCE shipping resolves it by import — a closure would pay an eager cloudpickle
    # round-trip the thread arm never needs.
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"signal.{op.tag}", op.identity_buffer(fs, payload.content_key)).bind(
            lambda result_key: boundary(f"signal.{op.tag}", lambda: _apply(samples, fs, op, result_key))
        )
    )


async def apply(samples: object, fs: float, op: SignalOp, lane: LanePolicy) -> "RuntimeRail[SignalReceipt]":
    # weave owns span, fence, and the `@receipted(REDACTION)` receipt harvest.
    async def dispatch() -> RuntimeRail[SignalReceipt]:
        return (await lane.offload(Kernel.of(_signal_kernel, KernelTrait.RELEASING), samples, fs, op)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.SIGNAL, f"signal.{op.tag}", dispatch)


def _apply(samples: object, fs: float, op: SignalOp, key: ContentKey) -> SignalReceipt:
    import pywt
    import scipy.signal as sig

    xn = np.asarray(samples)
    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order, readout)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            dominant, power = _welch_band(sig, sig.sosfiltfilt(sos, xn), fs, readout)
            return SignalReceipt.of("filter", xn.size, key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="spectral", spectral=(nperseg, time_frequency, readout)):
            if time_frequency:
                # time-summed `np.sum(sxx, axis=-1)` POWER spine square-roots to amplitude for the fold — the comparable
                # stationary view beside `Scalogram`; the band power stays the power.
                sf, _, sxx = sig.spectrogram(xn, fs=fs, nperseg=min(nperseg, xn.size))
                spine = np.sum(sxx, axis=-1)
                return SignalReceipt.of("spectral", xn.size, key, SignalEvidence.Spectral(readout.fold(sf, np.sqrt(spine)), float(np.sum(sxx))))
            dominant, power = _welch_band(sig, xn, fs, readout, nperseg)
            return SignalReceipt.of("spectral", xn.size, key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="peaks", peaks=(prominence, distance)):
            idx, props = sig.find_peaks(xn, prominence=prominence or None, distance=distance or None)
            mean_prom = float(np.mean(props["prominences"])) if idx.shape[0] else 0.0
            return SignalReceipt.of("peaks", xn.size, key, SignalEvidence.Peaks(int(idx.shape[0]), mean_prom))
        case SignalOp(tag="resample", resample=(target, readout)):
            # polyphase up/down are the REDUCED rational ratio, not the raw rates: `gcd`
            # collapses (48000, 44100) to (160, 147) so the polyphase FIR is the minimal filter,
            # never an enormous up=48000/down=44100 kernel scipy would build verbatim.
            g = gcd(int(round(target)), int(round(fs)))
            out = sig.resample_poly(xn, int(round(target)) // g, int(round(fs)) // g)
            dominant, power = _welch_band(sig, out, target, readout)
            return SignalReceipt.of("resample", int(out.shape[0]), key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="decompose", decompose=(wavelet, level, mode, denoise)):
            return _decompose(wavelet, level, mode, denoise, xn, key)
        case SignalOp(tag="multiresolution", multiresolution=(wavelet, level)):
            bands = pywt.mra(xn, wavelet, level=level or None, transform="swt")
            residual = float(np.linalg.norm(np.einsum("bi->i", np.asarray(bands)) - xn) / (np.linalg.norm(xn) + 1e-30))
            return SignalReceipt.of(
                "multiresolution", xn.size, key, SignalEvidence.Multiresolution(_coeff_energy(bands), residual, ThresholdMode.NONE)
            )
        case SignalOp(tag="scalogram", scalogram=(wavelet, scales, resolution)):
            grid = np.asarray(scales) if scales else np.logspace(0.0, np.log(0.5 * xn.size), resolution, base=np.e)
            # FFT-domain CWT, not the direct convolution: the geometric grid is the many-scales
            # case `method='fft'` reduces from O(scales*n*filter) to one shared frequency-domain pass.
            coefs, freqs = pywt.cwt(xn, grid, wavelet, sampling_period=1.0 / fs, method="fft")
            mag = np.abs(np.asarray(coefs))
            peak = int(np.argmax(np.einsum("st,st->s", mag, mag)))
            return SignalReceipt.of("scalogram", xn.size, key, SignalEvidence.Scale(float(grid[peak]), float(np.asarray(freqs)[peak])))
        case SignalOp(tag="packet", packet=(wavelet, maxlevel, readout)):
            tree = pywt.WaveletPacket(data=xn, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            leaves = tree.get_level(maxlevel, order="freq")
            energy = _coeff_energy(node.data for node in leaves)
            # frequency-ordered leaves ARE the band spectrum — each of the `2**maxlevel` nodes owns `[k, k+1) * fs / (2 * count)` —
            # so the readout folds per-leaf energy (square-rooted to amplitude) over the band centres rather than re-running Welch on
            # that raw trace, which would report the source band instead of the packet structure; `node_energy` stays energy.
            centres = (np.arange(len(energy)) + 0.5) * (0.5 * fs / len(energy))
            band = readout.fold(centres, np.sqrt(np.asarray(energy)))
            return SignalReceipt.of("packet", xn.size, key, SignalEvidence.Packet(energy, band))
        case _ as unreachable:
            assert_never(unreachable)


def _decompose(wavelet: str, level: int, mode: DecompMode, denoise: ThresholdMode, x: np.ndarray, key: ContentKey) -> SignalReceipt:
    import pywt

    forward, inverse, max_level = _wavelet_routes()[mode]
    depth = level or max_level(x.size, pywt.Wavelet(wavelet))
    coeffs = forward(x, wavelet, depth)
    # NONE yields None so the un-denoised list reconstructs verbatim; otherwise VisuShrink shrinks every detail band under the
    # universal threshold over the finest-band MAD noise, the approximation passing through — the shrunk list IS the nested
    # structure the row's inverse consumes, never a hand-built coeff-slice index map.
    shrink = denoise.shrink(pywt, _mad_threshold(coeffs[-1], x.size))
    rebuilt = inverse(coeffs if shrink is None else [coeffs[0], *map(shrink, coeffs[1:])], wavelet)[: x.size]
    residual = float(np.linalg.norm(rebuilt - x) / (np.linalg.norm(x) + 1e-30))
    return SignalReceipt.of("decompose", x.size, key, SignalEvidence.Multiresolution(_coeff_energy(coeffs), residual, denoise))
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
