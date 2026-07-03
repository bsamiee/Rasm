# [PY_COMPUTE_SIGNAL]

The one classical signal-analysis owner spanning stationary spectral estimation and multiresolution wavelet analysis on a single `Signal`. `SignalOp` discriminates the `scipy.signal` stationary rows — zero-phase IIR/FIR filtering, the `welch`/`spectrogram` spectral estimate, polyphase resampling, `find_peaks` peak structure — beside the `pywt` multiresolution rows — the decimated/stationary discrete decomposition with optional coefficient-shrink denoise, the additive `mra` analysis-of-variance bands, the continuous-wavelet scalogram, and the frequency-ordered wavelet-packet tree. A transient, a localized discontinuity, or a non-stationary mode the Welch estimate averages away is first-class evidence on the same owner, never a per-transform method family and never a second signal surface.

Output is parameterized as tightly as input. `SignalEvidence` is the `@tagged_union` over `spectral`, `peaks`, `multiresolution`, `scale`, and `packet` shapes that the thin `SignalReceipt` carries whole, never a fat struct of default-zero fields: `spectral` holds `(dominant_hz, band_power)`, `peaks` holds `(count, mean_prominence)` on its own case rather than overloading the spectral slots with a meaningless `dominant_hz=0.0`, `multiresolution` holds `(level_energy, reconstruction_residual, shrink)`, `scale` holds `(dominant_scale, dominant_hz)`, and `packet` holds the frequency-ordered node energy. Every stationary arm reads its dominant band through the reused `SpectralReadout` axis (`PEAK`/`CENTROID`/`BANDWIDTH`/`FLATNESS`) from `analysis/transform.md#TRANSFORM` rather than a hardcoded `argmax`, so the spectral output shape varies with the `readout` row the way the spectral input shape varies with the op case.

Every operand admits through `numerics/array.md#PAYLOAD` for the finite gate and the `ContentIdentity` key, then materializes the numpy floor `xn` once: scipy 1.17 lists this owner's `scipy.signal` entrypoints — `sosfiltfilt`/`welch`/`spectrogram`/`find_peaks`/`resample_poly` — as out-of-scope or skip-backend for jax/dask/torch, so the stationary spine is numpy-resident beside the numpy-only `pywt` wavelet path, both on the worker worker lane, and the `ArrayPayload.content_key` keys the receipt the way `analysis/spatial.md#SPATIAL` keys its numpy-bound point-set query. `SpectralReadout.fold` owns the linear-amplitude contract from `analysis/transform.md#TRANSFORM` and re-resolves its namespace off the numpy spine it is handed, so an owner holding a power/energy spine square-roots it (`np.sqrt(pxx)`) before the fold while band power stays the Parseval power integral; the parameterized band read is the same axis the Array-API-aware `scipy.fft` transform owner folds, reused here over numpy. `SignalReceipt` is the `ReceiptContributor` the study spine harvests under the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, so the dominant-band and wavelet-energy evidence streams without an inline `emit`. No learned or neural filters enter this owner.

## [01]-[INDEX]

- [01]-[DSP]: zero-phase IIR/FIR filtering, `welch`/`spectrogram` spectral estimation, polyphase resampling, and peak structure beside the `pywt` decimated/stationary/additive decomposition, coefficient-shrink denoise, scalogram, and frequency-ordered packet fold on one `Signal` owner; the output parameterized over the `SignalEvidence` `spectral`/`peaks`/`multiresolution`/`scale`/`packet` shapes and the dominant-band read over the reused `SpectralReadout` axis.

## [02]-[DSP]

- Owner: `Signal` — the ONE signal-analysis owner. `SignalOp` discriminates the stationary rows `Filter(kind, cutoff, order, readout)` (`scipy.signal.butter` -> `sosfiltfilt` zero-phase IIR; `FilterKind` selects `lowpass`/`highpass`/`bandpass`/`bandstop`), `Spectral(nperseg, time_frequency, readout)` (`scipy.signal.welch` PSD, or the `scipy.signal.spectrogram` time-frequency surface when `time_frequency` is set, the comparable stationary view beside the `Scalogram` time-scale view), `Peaks(prominence, distance)` (`scipy.signal.find_peaks` returning the prominence-thresholded peak set and its `properties["prominences"]` fold), and `Resample(target_rate, readout)` (`scipy.signal.resample_poly` polyphase rational resample), beside the multiresolution wavelet rows `Decompose(wavelet, level, mode, denoise)` (`pywt.wavedec` decimated DWT or `pywt.swt` stationary DWT under `DecompMode`, `pywt.dwt_max_level`/`swt_max_level` cap, optional `pywt.threshold`/`threshold_firm` shrink keyed by `ThresholdMode`), `Multiresolution(wavelet, level)` (`pywt.mra` additive analysis-of-variance bands summing to the signal), `Scalogram(wavelet, scales)` (`pywt.cwt` whose `(coefs, freqs)` return carries the scale→frequency mapping directly under `sampling_period`, so the dominant scale reads its frequency off the returned `freqs` rather than a separate `scale2frequency` call), and `Packet(wavelet, maxlevel, readout)` (`pywt.WaveletPacket` frequency-ordered leaf tree). Every row sits on the same owner, never a per-transform method family and never a second signal surface. The `readout` field is the reused `SpectralReadout` row: each PSD-bearing op carries its own band projection rather than a fixed `argmax`.
- Entry: `Signal.apply` admits the operand through `ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT)`, binds the resolved payload into `boundary(f"signal.{op.tag}", ...)`, and returns `RuntimeRail[SignalReceipt]`. `_apply` materializes the numpy floor `xn = np.asarray(samples)` once because every `scipy.signal` entrypoint this owner uses skips the non-numpy backends in scipy 1.17 and `pywt` is numpy-only, so one floor serves both the stationary and the wavelet path rather than two intakes. The filter path designs the SOS cascade, applies `sosfiltfilt`, and folds the filtered trace through the one `_welch_band` projection that runs Welch and reduces the PSD spine to `(readout.fold(f, np.sqrt(pxx)), np.sum(pxx) * df)` under the `min(nperseg, xn.size)` clamp that keeps a short trace off the `welch` length error — the fold consumes the amplitude spine `np.sqrt(pxx)` per the `analysis/transform.md#TRANSFORM` linear-amplitude contract while band power stays the Parseval power integral `np.sum(pxx) * df`. The spectral path runs the same `_welch_band` projection, or the legacy out-of-scope `scipy.signal.spectrogram` time-frequency surface when `time_frequency` is set, square-rooting the numpy `np.sum(sxx, axis=-1)` time-summed spectrogram power spine to amplitude for the same `readout`. The peaks path folds the prominence-thresholded count and mean prominence into the dedicated `Peaks` evidence; the resample path reduces the `(target, fs)` pair to its coprime rational ratio through `math.gcd` so `resample_poly` builds the minimal polyphase FIR rather than the raw-rate kernel, and reads its band through `_welch_band` at the target rate. The `Decompose` path indexes the `WAVELET_ROUTES` row for its `DecompMode`, runs the row's one forward decomposition to the `max_level` cap, and folds the shared `_coeff_energy` squared-coefficient energy per level; the denoise shrink is the `ThresholdMode.shrink(pywt, value)` callable the enum owns — `None` on the `NONE` row reconstructs verbatim, otherwise VisuShrink shrinks every detail band under the universal threshold `sigma * sqrt(2 * log(n))` over the robust MAD noise estimate `sigma = median(|cD_finest|) / 0.6745` from the finest detail band while the approximation band passes through, the shrunk coefficient list reconstructing directly through the same row's inverse. The reconstruction residual measures the denoised-against-original deviation under the perfect-reconstruction invariant. The `Multiresolution` path runs `mra` and folds the per-band energy plus the additive `sum(bands) ≈ x` residual; the `Scalogram` path runs `cwt` and reads the maximum-energy scale mapped to a dominant frequency; the `Packet` path walks the `order="freq"` leaf nodes folding per-node energy through the same `_coeff_energy`, then reads its dominant band by square-rooting that per-leaf power spectrum to amplitude (`np.sqrt`) and folding it over the leaf band-centres `(arange(count) + 0.5) * fs / (2 * count)` through the same `SpectralReadout` (the linear-amplitude contract), so the packet band reflects the leaf structure rather than the raw-signal Welch spectrum while the `node_energy` evidence stays energy. The stationary ops normalize the cutoff against the Nyquist frequency; the wavelet ops report the dominant frequency from the dominant scale or packet band and leave `band_power` to the spectral evidence.
- Output: `SignalEvidence` parameterizes the result shape the way input is parameterized — `Spectral(dominant_hz, band_power)` for the stationary spectral rows, `Peaks(count, mean_prominence)` for the `find_peaks` structure on its own case (never a `Spectral(0.0, prominence, count)` overload that would name a mean prominence as `band_power` and emit a meaningless `dominant_hz=0.0`), `Multiresolution(level_energy, reconstruction_residual, shrink)` for the `Decompose`/`Multiresolution` rows, `Scale(dominant_scale, dominant_hz)` for the scalogram, and `Packet(node_energy, dominant_hz)` for the packet tree. `SignalReceipt` carries the `SignalEvidence` union directly — `SignalReceipt.of(op_tag, length, key, evidence)` is the thin constructor, never a flattening `match` over five default-zero field groups, so the carrier is denser than the spatial/transform projection that collapses the evidence into a fixed receipt shape. This is the same discriminated-evidence collapse `analysis/spatial.md#SPATIAL` applies to `Proximity`/`Complex`/`Boundary` and `analysis/symbolic.md#OP` to its `Outcome`, carried one step further by holding the union on the receipt rather than projecting it down.
- Receipt: `SignalReceipt.contribute` yields one `Receipt.of("compute.signal", ("emitted", op_tag, facts))` row into the `Iterable[Receipt]` the `ReceiptContributor` port declares — the one corpus-wide `contribute(self) -> Iterable[Receipt]` annotation every owner carries, the `yield` body the analysis siblings use and the one-element tuple-return the numerics/solver/graduation siblings use both satisfying the one port type, never a narrowed `tuple[Receipt, ...]` annotation drifting from the port declaration nor a bare single-`Receipt` return; the `facts` map spreads only the slots the matched `SignalEvidence` carries through the evidence's own `facts()` projection, so a spectral receipt names `dominant_hz`/`band_power`, a peaks receipt `peaks`/`mean_prominence`, a multiresolution receipt `level_energy`/`reconstruction_residual`/`shrink`, and the `content_key.project("hex")` render rides every row — the evidence a study run records through `experiments/study.md#STUDY`. `apply` returns the `RuntimeRail[SignalReceipt]` boundary form like every sibling owner; the resolved `SignalReceipt` is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, never an inline `emit` threaded through this body.
- Packages: `scipy` (`signal.butter`, `signal.sosfiltfilt`, `signal.welch`, `signal.spectrogram`, `signal.find_peaks`, `signal.resample_poly` — the numpy-resident stationary path; scipy 1.17 lists these entrypoints out-of-scope or skip-backend for jax/dask/torch, so the floor is the `np.ndarray` `xn`, not a resolved `xp`), `pywt` (`wavedec`/`waverec`, `swt`/`iswt`, `swt_max_level`/`dwt_max_level`, `mra`, `cwt`, `WaveletPacket`, `threshold`/`threshold_firm`, `Wavelet.dec_len` — the numpy-only wavelet path over the same `xn` floor; `numerics/array.md#PAYLOAD` owns the `array_namespace` resolution and the finite gate at admission, so this owner threads neither in its body and needs no `array-api-extra` `nan_to_num` because the `FiniteGate.REJECT` admission already excludes a non-finite operand), `numpy` (`asarray` the one floor, `ravel` the energy contraction, `einsum` the per-band/per-node squared-coefficient energy contraction and the additive-band/scalogram spine reductions, `logspace(base=e)`/`log` the geometric CWT scale grid, `sum`/`mean`/`argmax`/`median`/`abs`/`sqrt`/`linalg.norm` the numpy-resident stationary and wavelet folds), `expression` (`tagged_union`/`tag`/`case` the `SignalOp` and `SignalEvidence` ADTs, `Map` the `WAVELET_ROUTES` table), `functools.cache` the deferred-import table memo, `math.gcd` the rational-resample ratio reducer, `msgspec` (`Struct(frozen=True)` the `SignalReceipt`), `analysis/transform.md#TRANSFORM` (`SpectralReadout` the reused band-projection axis folding the PSD/spectrogram spine, re-resolving its namespace off the numpy spine it is handed), `numerics/array.md#PAYLOAD` (`ArrayPayload.admit`/`ArraySource.Live`/`FiniteGate`, the operand admitting and keying the `content_key` through `ContentIdentity`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor` from `runtime/receipts`, `ContentKey` carried by the admitted payload, the `runtime/observability/receipts#RECEIPT` `@receipted` study-spine harvest of the `Ok`-arm contributor, not an import this owner threads).
- Growth: a new transform is one `SignalOp` case; a new filter family is one `FilterKind` row; a new decomposition mode is one `DecompMode` row plus its `WAVELET_ROUTES` `(forward, inverse, max_level)` triple, the decimated/stationary collapse extending by a table row rather than a new ternary arm; a new coefficient-shrink rule is one `ThresholdMode` row whose `shrink` method arm owns its callable, never a parallel denoiser surface; a new band projection is one `SpectralReadout` row in the transform owner that every PSD-bearing op inherits; a new evidence shape is one `SignalEvidence` case plus its `facts()` arm; zero new surface.

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
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

if TYPE_CHECKING:
    # `scipy.signal` is numpy-resident for this owner's entrypoints — `sosfiltfilt`/`welch`/
    # `spectrogram`/`find_peaks`/`resample_poly` skip the jax/dask/torch backends in scipy 1.17,
    # so the stationary spine materializes the numpy floor `xn` (the `pywt` arms already do), and
    # the `np.ndarray` floor is the one carrier every body threads. `ModuleType` types the
    # boundary-imported `scipy.signal` handle the Welch projection takes.
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
    # the closed coefficient-shrink vocabulary: every row IS a pywt threshold rule, the shrink
    # callable living on the enum so the denoise fold reads `mode.shrink(value)` rather than an
    # inline `is FIRM` ternary. NONE yields no shrink; SOFT/HARD/GARROTE/GREATER/LESS route the
    # single-knee `pywt.threshold`; FIRM routes the two-knee `pywt.threshold_firm` — the axis
    # spans both documented surfaces as one row set, never a parallel denoiser.
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
    # the output is parameterized as tightly as the input: one discriminated carrier per
    # evidence shape, not one struct of default-zero field groups. `facts()` is the total
    # projection the receipt spreads, so each shape names only its own slots. `peaks` is its
    # own case rather than a `Spectral(0.0, prominence, count)` overload that would name a mean
    # prominence as `band_power` and emit a meaningless `dominant_hz=0.0`.
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
        match self:
            case SignalEvidence(tag="spectral", spectral=(hz, power)):
                return {"dominant_hz": f"{hz:.3f}", "band_power": f"{power:.3e}"}
            case SignalEvidence(tag="peaks", peaks=(count, prominence)):
                return {"peaks": count, "mean_prominence": f"{prominence:.3e}"}
            case SignalEvidence(tag="multiresolution", multiresolution=(energy, residual, shrink)):
                return {"level_energy": ",".join(f"{e:.3e}" for e in energy), "reconstruction_residual": f"{residual:.3e}", "shrink": shrink.value}
            case SignalEvidence(tag="scale", scale=(scale, hz)):
                return {"dominant_scale": f"{scale:.3f}", "dominant_hz": f"{hz:.3f}"}
            case SignalEvidence(tag="packet", packet=(energy, hz)):
                return {"node_energy": ",".join(f"{e:.3e}" for e in energy), "dominant_hz": f"{hz:.3f}"}
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


# --- [TABLES] ---------------------------------------------------------------------------


# one row per DecompMode carrying the (forward, inverse, max-level) triple over the decimated
# and undecimated DWT families, the same callable-table collapse `analysis/transform.md#TRANSFORM`
# applies to `FOURIER_ROUTES`: `_decompose` indexes the row and runs ONE forward body and ONE
# inverse body rather than a `mode is STATIONARY` ternary repeated at decomposition and
# reconstruction. `@cache` defers the import-banned pywt load to first call and memoizes the one
# Map. The stationary forward trims the approximation through `trim_approx=True` so both axes
# return the same `[cAn, cDn, …, cD1]` nested list the shrink and inverse consume uniformly.
@cache
def _wavelet_routes() -> "Map[DecompMode, WaveletRoute]":
    import pywt

    # both forward rows share the (x, wavelet, level) positional contract — `level=` is keyword
    # because `wavedec`/`swt` take `mode` at the third positional slot; both inverse rows share
    # the (coeffs, wavelet) contract so `waverec`/`iswt` pass through bare.
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
    # the one Welch projection every stationary arm folds through, collapsing the 4x-repeated
    # welch -> read -> integrate pattern into one read: `scipy.signal.welch` returns the `(f, pxx)`
    # numpy PSD power spine. `SpectralReadout.fold` owns the LINEAR AMPLITUDE contract from
    # `analysis/transform.md#TRANSFORM`, so the PSD power is square-rooted to amplitude (`np.sqrt(pxx)`)
    # before the fold (PEAK/CENTROID/BANDWIDTH/FLATNESS, re-resolving its namespace off the numpy spine,
    # FLATNESS squaring back to power internally) rather than a hardcoded argmax; the band power stays
    # the true Parseval power integral `np.sum(pxx) * df` (the `np.sum` PSD reduction scaled by the
    # uniform bin width). `min(nperseg, x.size)` keeps `welch` from a short-signal `ValueError`.
    f, pxx = sig.welch(x, fs=fs, nperseg=min(nperseg, x.size))
    df = float(f[1] - f[0]) if f.shape[0] > 1 else 1.0
    return readout.fold(f, np.sqrt(pxx)), float(np.sum(pxx)) * df


def _coeff_energy(coeffs: Iterable[np.ndarray]) -> tuple[float, ...]:
    # the per-band/per-node/per-scale magnitude-square contraction shared with the wavelet and
    # packet folds — `einsum("i,i->", ravel(c), ravel(c))` is the energy reduction owner.
    return tuple(float(np.einsum("i,i->", np.ravel(c), np.ravel(c))) for c in coeffs)


def _mad_threshold(detail_finest: np.ndarray, n: int) -> float:
    # VisuShrink universal threshold over the robust MAD noise estimate from the finest band.
    sigma = float(np.median(np.abs(np.asarray(detail_finest))) / 0.6745)
    return sigma * float(np.sqrt(2.0 * np.log(n)))


def apply(samples: object, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: boundary(f"signal.{op.tag}", lambda: _apply(samples, fs, op, payload.content_key))
    )


def _apply(samples: object, fs: float, op: SignalOp, key: ContentKey) -> SignalReceipt:
    import pywt
    import scipy.signal as sig

    # the admission already minted the `content_key` and resolved the backend; the body materializes
    # the numpy floor `xn` once because every entrypoint this owner uses is numpy-resident in scipy
    # 1.17 — `sosfiltfilt`/`welch`/`spectrogram`/`resample_poly`/`find_peaks` skip the jax/dask/torch
    # backends, and `pywt` carries no Array-API contract — so one floor serves both the stationary and
    # the wavelet path and the dominant-band read folds through `SpectralReadout` over the numpy spine.
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
                # `scipy.signal.spectrogram` is out-of-scope/legacy for the Array API, so its `(f, t, Sxx)`
                # return is numpy: the time-summed `np.sum(sxx, axis=-1)` POWER spine is square-rooted to
                # amplitude for the fold (which re-resolves numpy off the spine and owns the linear-amplitude
                # contract), the comparable stationary view beside `Scalogram`; the band power stays the power.
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
            # the polyphase up/down are the REDUCED rational ratio, not the raw rates: `gcd`
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
            # the FFT-domain CWT, not the direct convolution: the geometric grid is the many-scales
            # case `method='fft'` reduces from O(scales*n*filter) to one shared frequency-domain pass.
            coefs, freqs = pywt.cwt(xn, grid, wavelet, sampling_period=1.0 / fs, method="fft")
            mag = np.abs(np.asarray(coefs))
            peak = int(np.argmax(np.einsum("st,st->s", mag, mag)))
            return SignalReceipt.of("scalogram", xn.size, key, SignalEvidence.Scale(float(grid[peak]), float(np.asarray(freqs)[peak])))
        case SignalOp(tag="packet", packet=(wavelet, maxlevel, readout)):
            tree = pywt.WaveletPacket(data=xn, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            leaves = tree.get_level(maxlevel, order="freq")
            energy = _coeff_energy(node.data for node in leaves)
            # the frequency-ordered leaves ARE the band spectrum: each of the `2**maxlevel` nodes owns
            # `[k, k+1) * fs / (2 * count)`, so the readout folds the per-leaf energy over the band centres
            # rather than re-running Welch on the raw trace (which would report the source band, not the
            # packet structure) — the `readout` parameter genuinely selects the packet dominant band. The
            # per-leaf energy is squared-coefficient POWER, so it is square-rooted to amplitude for the fold
            # (the linear-amplitude contract `analysis/transform.md#TRANSFORM` owns); `node_energy` stays energy.
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
    # the shrink callable lives on `ThresholdMode`: NONE yields None so the un-denoised list
    # reconstructs verbatim; otherwise VisuShrink shrinks every detail band `coeffs[1:]` under
    # the one universal threshold `sigma*sqrt(2 log n)` over the MAD noise `median(|cD_finest|)
    # / 0.6745`, the approximation band `coeffs[0]` passing through. The shrunk list IS the
    # nested structure the row's inverse consumes — never a hand-built coeff-slice index map.
    shrink = denoise.shrink(pywt, _mad_threshold(coeffs[-1], x.size))
    rebuilt = inverse(coeffs if shrink is None else [coeffs[0], *map(shrink, coeffs[1:])], wavelet)[: x.size]
    residual = float(np.linalg.norm(rebuilt - x) / (np.linalg.norm(x) + 1e-30))
    return SignalReceipt.of("decompose", x.size, key, SignalEvidence.Multiresolution(_coeff_energy(coeffs), residual, denoise))
```

## [03]-[RESEARCH]

- [SPECTRAL_READOUT]: the dominant-band read is the reused `SpectralReadout.fold(freqs, amplitude)` axis from `analysis/transform.md#TRANSFORM` rather than a hardcoded `float(f[argmax(pxx)])` repeated per stationary arm — the same `fold` the Fourier path folds, whose canonical input domain is the linear amplitude spine `|X(f)|`. `scipy.signal.welch` returns PSD POWER `pxx`, so this owner square-roots it to the amplitude spine the fold requires (`np.sqrt(pxx)`, matching `transform.md`) so `PEAK` reads the maximum bin, `CENTROID` the weighted mean frequency, `BANDWIDTH` the weighted spread, and `FLATNESS` the Wiener entropy (`FLATNESS` squaring back to power internally per the librosa `power=2.0` default), the spectral output shape varying with the `readout` row the way the spectral input shape varies with the op case. The `fold` resolves its namespace off its `amplitude` argument through `array_namespace`, so handed a numpy `np.sqrt(pxx)` it reduces over numpy — the axis is backend-polymorphic but this owner only ever feeds it the numpy `scipy.signal` spine, where the transform sibling feeds it the Array-API `scipy.fft` spectrum. Band power stays the unchanged Parseval power integral `np.sum(pxx)` numpy PSD reduction scaled by the uniform Welch bin width `df` (a genuine power fact); the per-band/per-node/per-scale wavelet energies stay the `np.einsum("i,i->", ravel(c), ravel(c))` magnitude-square contraction over the same `xn` floor (`libs/python/.api/numpy.md` reduction row [07] names `einsum` the general reduction/contraction owner) shared with `analysis/transform.md#TRANSFORM`, the packet path square-rooting that per-leaf energy to amplitude before its fold. The scalogram dominant-scale read stays an `argmax` over the per-scale energy spine because the scale-to-frequency mapping, not a magnitude readout, owns that projection; its geometric `logspace(base=e)` grid is sized by the `Scalogram` `resolution` parameter when no explicit `scales` tuple is supplied, never a hardcoded bin count.
- [PYWT_THRESHOLD]: the denoise axis spans both documented coefficient-shrink surfaces as one `ThresholdMode` row set whose `shrink(pywt, value)` method owns the per-row callable — `threshold(data, value, mode)` modes `soft`/`hard`/`garrote`/`greater`/`less` (`compute/.api/pywavelets.md` `[03]`-row [05]) on the default arm and the two-knee `threshold_firm(data, value_low, value_high)` (row [06]) on the `FIRM` arm, the `NONE` arm returning `None` so the un-denoised list reconstructs verbatim. The shrink lives on the enum rather than an inline `is FIRM` ternary in `_decompose`, collapsing the branch into the vocabulary the way `SpectralReadout.fold` lives on its axis. The VisuShrink denoise maps the shrink across every detail band `coeffs[1:]` with `value = sigma * sqrt(2 log n)` over `sigma = median(|cD_finest|) / 0.6745`, leaving the approximation band `coeffs[0]` unthresholded; the shrunk coefficient list is the same nested structure the `WAVELET_ROUTES` forward returned, reconstructing directly through the row's inverse — the per-detail-band threshold the `[04]-[IMPLEMENTATION_LAW]` denoising-axis rule places between `wavedec` and `waverec`, never a parallel denoiser op and never a hand-built `coeff_slices` index map (the `ravel_coeffs`/`unravel_coeffs` packing bijection of row `[04]`-[06] is the distinct optimizer-rail bridge the `SIGNAL_STACKING` jax/optax row owns for the flat L1-penalized inverse, not this in-place denoise).
- [ARRAY_ADMISSION]: `Signal.apply` admits the operand through `ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT)` from `numerics/array.md#PAYLOAD` and binds the resolved payload into the `boundary` thunk, so a non-finite input returns the admission fault rather than producing a NaN-poisoned spectrum and the `ArrayPayload.content_key` keys the `SignalReceipt` the way `analysis/spatial.md#SPATIAL` keys its point-set query through `ContentIdentity`. Unlike the `scipy.fft` transform sibling, this owner does NOT re-resolve `array_namespace` in `_apply`: its `scipy.signal` entrypoints skip the non-numpy backends in scipy 1.17, so `_apply` materializes the one numpy floor `xn = np.asarray(samples)` that both the stationary and the `pywt` wavelet path bind — the same numpy-bound stance `analysis/spatial.md#SPATIAL` holds for the not-Array-API-aware `scipy.spatial`, distinct from the `xp`-dispatch the Array-API `scipy.fft` owner runs. The `FiniteGate.REJECT` row (the `numerics/array.md#PAYLOAD` finite axis forbidding any NaN or ±inf) is the admission policy; the owner never re-rolls the `FiniteGate.forbidden`/`violated` masked reduction or the identity logic the payload owner holds.
- [RECEIPT_SHAPE]: `SignalReceipt.contribute` yields into the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`), and the row is `Receipt.of("compute.signal", ("emitted", op_tag, facts))` — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit, and never a narrowed `tuple[Receipt, ...]` annotation that would drift from the one `Iterable[Receipt]` port-type annotation the whole corpus carries (the `yield` body here and the one-element tuple-return body the numerics/solver/graduation siblings hold both satisfy it). `SignalEvidence` parameterizes the output shape and owns its own `facts()` projection, so the receipt spreads only the slots the matched evidence carries beside the `op`/`length`/`content_key.project("hex")` render, the dedicated `Peaks` case keeping a `find_peaks` receipt off the spectral slots. `apply` is the `RuntimeRail[SignalReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `apply` but the study spine harvesting the resolved `SignalReceipt` contributor through the `@receipted` aspect on the `Ok` arm — the same convention `analysis/transform.md#TRANSFORM` and `analysis/spatial.md#SPATIAL` hold, the receipt the contributor and the rail the boundary form.
