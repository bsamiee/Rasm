# [PY_COMPUTE_SIGNAL]

The one classical signal-analysis owner spanning stationary spectral estimation and multiresolution wavelet analysis on a single `Signal`. `SignalOp` discriminates the `scipy.signal` stationary rows — zero-phase IIR/FIR filtering, the `welch`/`spectrogram` spectral estimate, polyphase resampling, `find_peaks` peak structure — beside the `pywt` multiresolution rows — the decimated/stationary discrete decomposition with optional coefficient-shrink denoise, the additive `mra` analysis-of-variance bands, the continuous-wavelet scalogram, and the frequency-ordered wavelet-packet tree. A transient, a localized discontinuity, or a non-stationary mode the Welch estimate averages away is first-class evidence on the same owner, never a per-transform method family and never a second signal surface.

Output is parameterized as tightly as input. `SignalEvidence` is the `@tagged_union` over `spectral`, `peaks`, `multiresolution`, `scale`, and `packet` shapes that the thin `SignalReceipt` carries whole, never a fat struct of default-zero fields: `spectral` holds `(dominant_hz, band_power)`, `peaks` holds `(count, mean_prominence)` on its own case rather than overloading the spectral slots with a meaningless `dominant_hz=0.0`, `multiresolution` holds `(level_energy, reconstruction_residual, shrink)`, `scale` holds `(dominant_scale, dominant_hz)`, and `packet` holds the frequency-ordered node energy. Every stationary arm reads its dominant band through the reused `SpectralReadout` axis (`PEAK`/`CENTROID`/`BANDWIDTH`/`FLATNESS`) from `analysis/transform.md#TRANSFORM` rather than a hardcoded `argmax`, so the spectral output shape varies with the `readout` row the way the spectral input shape varies with the op case.

Every operand admits through `numerics/array.md#PAYLOAD`, so the Array-API-aware `scipy.signal` of scipy 1.17 dispatches through the resolved backend `xp`, the `ArrayPayload.content_key` keys the receipt the way `analysis/spatial.md#SPATIAL` keys its point-set query, and `pywt` resolves its numpy contract beside the scipy path on the same `python_version<'3.15'` companion band. `SignalReceipt` is the `ReceiptContributor` the study spine harvests under the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, so the dominant-band and wavelet-energy evidence streams without an inline `emit`. No learned or neural filters enter this owner.

## [01]-[INDEX]

- [01]-[DSP]: zero-phase IIR/FIR filtering, `welch`/`spectrogram` spectral estimation, polyphase resampling, and peak structure beside the `pywt` decimated/stationary/additive decomposition, coefficient-shrink denoise, scalogram, and frequency-ordered packet fold on one `Signal` owner; the output parameterized over the `SignalEvidence` `spectral`/`peaks`/`multiresolution`/`scale`/`packet` shapes and the dominant-band read over the reused `SpectralReadout` axis.

## [02]-[DSP]

- Owner: `Signal` — the ONE signal-analysis owner. `SignalOp` discriminates the stationary rows `Filter(kind, cutoff, order, readout)` (`scipy.signal.butter` -> `sosfiltfilt` zero-phase IIR; `FilterKind` selects `lowpass`/`highpass`/`bandpass`/`bandstop`), `Spectral(nperseg, time_frequency, readout)` (`scipy.signal.welch` PSD, or the `scipy.signal.spectrogram` time-frequency surface when `time_frequency` is set, the comparable stationary view beside the `Scalogram` time-scale view), `Peaks(prominence, distance)` (`scipy.signal.find_peaks` returning the prominence-thresholded peak set and its `properties["prominences"]` fold), and `Resample(target_rate, readout)` (`scipy.signal.resample_poly` polyphase rational resample), beside the multiresolution wavelet rows `Decompose(wavelet, level, mode, denoise)` (`pywt.wavedec` decimated DWT or `pywt.swt` stationary DWT under `DecompMode`, `pywt.dwt_max_level`/`swt_max_level` cap, optional `pywt.threshold`/`threshold_firm` shrink keyed by `ThresholdMode`), `Multiresolution(wavelet, level)` (`pywt.mra` additive analysis-of-variance bands summing to the signal), `Scalogram(wavelet, scales)` (`pywt.cwt` + `pywt.scale2frequency`), and `Packet(wavelet, maxlevel, readout)` (`pywt.WaveletPacket` frequency-ordered leaf tree). Every row sits on the same owner, never a per-transform method family and never a second signal surface. The `readout` field is the reused `SpectralReadout` row: each PSD-bearing op carries its own band projection rather than a fixed `argmax`.
- Entry: `Signal.apply` admits the operand through `ArrayPayload.admit(ArrayOp.Live(samples), (), FiniteGate.REJECT)`, binds the resolved payload into `boundary(f"signal.{op.tag}", ...)`, and returns `RuntimeRail[SignalReceipt]`. The filter path designs the SOS cascade, applies `sosfiltfilt`, and folds the filtered trace through the one `_welch_band` projection that runs Welch and reduces the PSD to `(readout.fold(f, pxx), einsum-energy * df)`. The spectral path runs the same `_welch_band` projection, or the `scipy.signal.spectrogram` time-frequency surface when `time_frequency` is set, reducing the time-summed spectrogram spine through the same `readout`. The peaks path folds the prominence-thresholded count and mean prominence into the dedicated `Peaks` evidence; the resample path reduces the `(target, fs)` pair to its coprime rational ratio through `math.gcd` so `resample_poly` builds the minimal polyphase FIR rather than the raw-rate kernel, and reads its band through `_welch_band` at the target rate. The `Decompose` path indexes the `WAVELET_ROUTES` row for its `DecompMode`, runs the row's one forward decomposition to the `max_level` cap, and folds the shared `_coeff_energy` squared-coefficient energy per level; the denoise shrink is the `ThresholdMode.shrink(pywt, value)` callable the enum owns — `None` on the `NONE` row reconstructs verbatim, otherwise VisuShrink shrinks every detail band under the universal threshold `sigma * sqrt(2 * log(n))` over the robust MAD noise estimate `sigma = median(|cD_finest|) / 0.6745` from the finest detail band while the approximation band passes through, the shrunk coefficient list reconstructing directly through the same row's inverse. The reconstruction residual measures the denoised-against-original deviation under the perfect-reconstruction invariant. The `Multiresolution` path runs `mra` and folds the per-band energy plus the additive `sum(bands) ≈ x` residual; the `Scalogram` path runs `cwt` and reads the maximum-energy scale mapped to a dominant frequency; the `Packet` path walks the `order="freq"` leaf nodes folding per-node energy through the same `_coeff_energy` and reads its band through `_welch_band`. The stationary ops normalize the cutoff against the Nyquist frequency; the wavelet ops report the dominant frequency from the dominant scale and leave `band_power` to the spectral evidence.
- Output: `SignalEvidence` parameterizes the result shape the way input is parameterized — `Spectral(dominant_hz, band_power)` for the stationary spectral rows, `Peaks(count, mean_prominence)` for the `find_peaks` structure on its own case (never a `Spectral(0.0, prominence, count)` overload that would name a mean prominence as `band_power` and emit a meaningless `dominant_hz=0.0`), `Multiresolution(level_energy, reconstruction_residual, shrink)` for the `Decompose`/`Multiresolution` rows, `Scale(dominant_scale, dominant_hz)` for the scalogram, and `Packet(node_energy, dominant_hz)` for the packet tree. `SignalReceipt` carries the `SignalEvidence` union directly — `SignalReceipt.of(op_tag, length, key, evidence)` is the thin constructor, never a flattening `match` over five default-zero field groups, so the carrier is denser than the spatial/transform projection that collapses the evidence into a fixed receipt shape. This is the same discriminated-evidence collapse `analysis/spatial.md#SPATIAL` applies to `Proximity`/`Complex`/`Boundary` and `analysis/symbolic.md#OP` to its `Outcome`, carried one step further by holding the union on the receipt rather than projecting it down.
- Receipt: `SignalReceipt.contribute` yields one `Receipt.of("compute.signal", ("emitted", op_tag, facts))` row into the `Iterable[Receipt]` the `ReceiptContributor` port declares — the identical `yield`-into-`Iterable` shape `analysis/transform.md#TRANSFORM` and `analysis/spatial.md#SPATIAL` hold, never a `tuple[Receipt, ...]` drift on the one co-cited convention; the `facts` map spreads only the slots the matched `SignalEvidence` carries through the evidence's own `facts()` projection, so a spectral receipt names `dominant_hz`/`band_power`, a peaks receipt `peaks`/`mean_prominence`, a multiresolution receipt `level_energy`/`reconstruction_residual`/`shrink`, and the `content_key.project("hex")` render rides every row — the evidence a study run records through `experiments/study.md#STUDY`. `apply` returns the `RuntimeRail[SignalReceipt]` boundary form like every sibling owner; the resolved `SignalReceipt` is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, never an inline `emit` threaded through this body.
- Packages: `scipy` (`signal.butter`, `signal.sosfiltfilt`, `signal.welch`, `signal.spectrogram`, `signal.find_peaks`, `signal.resample_poly`), `pywt` (`wavedec`/`waverec`, `swt`/`iswt`, `swt_max_level`/`dwt_max_level`, `mra`, `cwt`/`scale2frequency`, `WaveletPacket`, `threshold`/`threshold_firm`, `Wavelet.dec_len`), `numpy` (`asarray`, `ravel`, `einsum` the energy/spine contraction owner, `logspace(base=e)`/`log` the geometric CWT scale grid, `argmax`, `median`, `abs`, `sqrt`, `mean`, `linalg.norm`), `expression` (`tagged_union`/`tag`/`case` the `SignalOp` and `SignalEvidence` ADTs, `Map` the `WAVELET_ROUTES` table), `functools.cache` the deferred-import table memo, `math.gcd` the rational-resample ratio reducer, `msgspec` (`Struct(frozen=True)` the `SignalReceipt`), `analysis/transform.md#TRANSFORM` (`SpectralReadout` the reused band-projection axis), `numerics/array.md#PAYLOAD` (`ArrayPayload.admit`/`ArrayOp.Live`/`FiniteGate`, the operand admitting and keying the `content_key` through `ContentIdentity`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor` from `runtime/receipts`, `ContentKey` carried by the admitted payload, the `runtime/observability/receipts#RECEIPT` `@receipted` study-spine harvest of the `Ok`-arm contributor, not an import this owner threads).
- Growth: a new transform is one `SignalOp` case; a new filter family is one `FilterKind` row; a new decomposition mode is one `DecompMode` row plus its `WAVELET_ROUTES` `(forward, inverse, max_level)` triple, the decimated/stationary collapse extending by a table row rather than a new ternary arm; a new coefficient-shrink rule is one `ThresholdMode` row whose `shrink` method arm owns its callable, never a parallel denoiser surface; a new band projection is one `SpectralReadout` row in the transform owner that every PSD-bearing op inherits; a new evidence shape is one `SignalEvidence` case plus its `facts()` arm; zero new surface.
- Boundary: classical signal analysis only — IIR/FIR design, `welch`/`spectrogram` spectral estimation, peak structure, resampling, the decimated/stationary/additive/continuous/packet wavelet transforms, and the `pywt.threshold`/`threshold_firm` coefficient-shrink denoise (a coefficient row between `wavedec` and `waverec`, never a separate denoiser type) are in-scope; no learned filters and no neural denoising. Both `scipy` and `pywt` carry the companion `python_version<'3.15'` band (neither ships a cp315 wheel), so every body is authored against the documented API on the gated band; scipy 1.17 makes `scipy.signal` Array-API-aware so the stationary ops dispatch through the `ArrayPayload`-resolved backend `xp`, while the wavelet ops resolve `pywt`'s numpy-array contract. Perfect reconstruction `waverec(wavedec(x)) ≈ x` and additive closure `sum(mra(x)) ≈ x` (`pywt`'s documented inverses) are the wavelet verification invariants; the `Decompose`/`Multiresolution` paths fold the reconstruction residual as the evidence of them. The `numerics/array.md#PAYLOAD` admission owns backend resolution and finite policy; this owner never re-rolls the `array_namespace` dispatch. The deleted forms are a `SignalReceipt` of five default-zero field groups where the `SignalEvidence` `facts()` discriminates the outcome, a `Spectral(0.0, prominence, count)` overload naming a mean prominence as `band_power` where the dedicated `Peaks` case holds it, a `contribute` returning a `tuple[Receipt, ...]` where the `ReceiptContributor` port declares `Iterable[Receipt]` and the siblings `yield`, a `mode is DecompMode.STATIONARY` ternary repeated at decomposition and reconstruction where `WAVELET_ROUTES` indexes the decimation axis, an inline `is FIRM` shrink ternary where `ThresholdMode.shrink` owns the per-row callable, a hand-built `coeff_slices` index map where the shrunk nested list reconstructs directly, a `resample_poly(x, int(target), int(fs))` raw-rate kernel where `math.gcd` reduces the rational ratio, a per-transform `_filter`/`_spectral`/`_decompose`/`_packet` method family where one `_apply` match folds the union tag, and a hardcoded `argmax` stationary-band read where the reused `SpectralReadout` axis folds the spine.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache
from math import gcd
from typing import Literal, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.analysis.transform import SpectralReadout
from rasm.compute.numerics.array import ArrayOp, ArrayPayload, FiniteGate
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

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
    DECIMATED = "decimated"      # pywt.wavedec / waverec, level cap pywt.dwt_max_level
    STATIONARY = "stationary"    # pywt.swt / iswt (shift-invariant, undecimated), cap swt_max_level


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
                return {
                    "level_energy": ",".join(f"{e:.3e}" for e in energy),
                    "reconstruction_residual": f"{residual:.3e}",
                    "shrink": shrink.value,
                }
            case SignalEvidence(tag="scale", scale=(scale, hz)):
                return {"dominant_scale": f"{scale:.3f}", "dominant_hz": f"{hz:.3f}"}
            case SignalEvidence(tag="packet", packet=(energy, hz)):
                return {"node_energy": ",".join(f"{e:.3e}" for e in energy), "dominant_hz": f"{hz:.3f}"}
            case unreachable:
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
        (DecompMode.DECIMATED, (
            lambda x, wavelet, level: pywt.wavedec(x, wavelet, level=level),
            pywt.waverec,
            lambda n, w: pywt.dwt_max_level(n, w.dec_len),
        )),
        (DecompMode.STATIONARY, (
            lambda x, wavelet, level: pywt.swt(x, wavelet, level=level, trim_approx=True),
            pywt.iswt,
            lambda n, _w: pywt.swt_max_level(n),
        )),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _welch_band(sig: object, x: np.ndarray, fs: float, readout: SpectralReadout, nperseg: int = 256) -> tuple[float, float]:
    # the one Welch projection every stationary arm folds through, collapsing the 4x-repeated
    # welch -> read -> integrate pattern into one read: `SpectralReadout.fold` reuses the
    # transform owner's parameterized band axis (PEAK/CENTROID/BANDWIDTH/FLATNESS) rather than a
    # hardcoded argmax, and the band power is the einsum PSD-times-bin-width energy contraction.
    f, pxx = sig.welch(x, fs=fs, nperseg=nperseg)
    df = float(f[1] - f[0]) if f.size > 1 else 1.0
    return readout.fold(f, pxx), float(np.einsum("i->", pxx)) * df


def _coeff_energy(coeffs: Iterable[np.ndarray]) -> tuple[float, ...]:
    # the per-band/per-node/per-scale magnitude-square contraction shared with the wavelet and
    # packet folds — `einsum("i,i->", ravel(c), ravel(c))` is the energy reduction owner.
    return tuple(float(np.einsum("i,i->", np.ravel(c), np.ravel(c))) for c in coeffs)


def _mad_threshold(detail_finest: np.ndarray, n: int) -> float:
    # VisuShrink universal threshold over the robust MAD noise estimate from the finest band.
    sigma = float(np.median(np.abs(np.asarray(detail_finest))) / 0.6745)
    return sigma * float(np.sqrt(2.0 * np.log(n)))


def apply(samples: object, fs: float, op: SignalOp) -> "RuntimeRail[SignalReceipt]":
    return ArrayPayload.admit(ArrayOp.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: boundary(f"signal.{op.tag}", lambda: _apply(samples, fs, op, payload.content_key))
    )


def _apply(samples: object, fs: float, op: SignalOp, key: ContentKey) -> SignalReceipt:
    import pywt
    import scipy.signal as sig

    x = np.asarray(samples)
    nyquist = 0.5 * fs
    match op:
        case SignalOp(tag="filter", filter=(kind, cutoff, order, readout)):
            wn = tuple(c / nyquist for c in cutoff)
            sos = sig.butter(order, wn[0] if len(wn) == 1 else wn, btype=kind.value, output="sos")
            dominant, power = _welch_band(sig, sig.sosfiltfilt(sos, x), fs, readout)
            return SignalReceipt.of("filter", x.size, key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="spectral", spectral=(nperseg, time_frequency, readout)):
            if time_frequency:
                sf, _, sxx = sig.spectrogram(x, fs=fs, nperseg=nperseg)
                spine = np.einsum("ft->f", sxx)
                return SignalReceipt.of("spectral", x.size, key, SignalEvidence.Spectral(readout.fold(sf, spine), float(np.einsum("ft->", sxx))))
            dominant, power = _welch_band(sig, x, fs, readout, nperseg)
            return SignalReceipt.of("spectral", x.size, key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="peaks", peaks=(prominence, distance)):
            idx, props = sig.find_peaks(x, prominence=prominence or None, distance=distance or None)
            mean_prom = float(np.mean(props["prominences"])) if idx.size else 0.0
            return SignalReceipt.of("peaks", x.size, key, SignalEvidence.Peaks(int(idx.size), mean_prom))
        case SignalOp(tag="resample", resample=(target, readout)):
            # the polyphase up/down are the REDUCED rational ratio, not the raw rates: `gcd`
            # collapses (48000, 44100) to (160, 147) so the polyphase FIR is the minimal filter,
            # never an enormous up=48000/down=44100 kernel scipy would build verbatim.
            g = gcd(int(round(target)), int(round(fs)))
            out = sig.resample_poly(x, int(round(target)) // g, int(round(fs)) // g)
            dominant, power = _welch_band(sig, out, target, readout)
            return SignalReceipt.of("resample", int(out.size), key, SignalEvidence.Spectral(dominant, power))
        case SignalOp(tag="decompose", decompose=(wavelet, level, mode, denoise)):
            return _decompose(wavelet, level, mode, denoise, x, key)
        case SignalOp(tag="multiresolution", multiresolution=(wavelet, level)):
            bands = pywt.mra(x, wavelet, level=level or None, transform="swt")
            residual = float(np.linalg.norm(np.einsum("bi->i", np.asarray(bands)) - x) / (np.linalg.norm(x) + 1e-30))
            return SignalReceipt.of("multiresolution", x.size, key, SignalEvidence.Multiresolution(_coeff_energy(bands), residual, ThresholdMode.NONE))
        case SignalOp(tag="scalogram", scalogram=(wavelet, scales, resolution)):
            grid = np.asarray(scales) if scales else np.logspace(0.0, np.log(0.5 * x.size), resolution, base=np.e)
            # the FFT-domain CWT, not the direct convolution: the geometric grid is the many-scales
            # case `method='fft'` reduces from O(scales*n*filter) to one shared frequency-domain pass.
            coefs, freqs = pywt.cwt(x, grid, wavelet, sampling_period=1.0 / fs, method="fft")
            mag = np.abs(np.asarray(coefs))
            peak = int(np.argmax(np.einsum("st,st->s", mag, mag)))
            return SignalReceipt.of("scalogram", x.size, key, SignalEvidence.Scale(float(grid[peak]), float(np.asarray(freqs)[peak])))
        case SignalOp(tag="packet", packet=(wavelet, maxlevel, readout)):
            tree = pywt.WaveletPacket(data=x, wavelet=wavelet, mode="symmetric", maxlevel=maxlevel)
            leaves = tree.get_level(maxlevel, order="freq")
            band = _welch_band(sig, x, fs, readout)[0]
            return SignalReceipt.of("packet", x.size, key, SignalEvidence.Packet(_coeff_energy(node.data for node in leaves), band))
        case unreachable:
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

- [SCIPY_SIGNAL]: the `scipy.signal.butter`/`sosfiltfilt`/`welch`/`spectrogram`/`resample_poly`/`find_peaks` spellings carry the `python_version<'3.15'` marker and verify against the `scipy.signal` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` (rows [01]-[09]). scipy 1.17 makes `scipy.signal` Array-API-aware, so the stationary ops dispatch through the backend the operand admission resolves through `numerics/array.md#PAYLOAD`; `Spectral(time_frequency=True)` routes the `spectrogram(x, fs)` `(f, t, Sxx)` view (row [05]) per the pywt `SIGNAL_STACKING` row that pairs `scipy.signal.spectrogram` with `pywt.cwt` on a shared `fs` so the stationary-spectrum and time-scale frequency axes are comparable. `resample_poly(x, up, down)` (row [07]) takes `up`/`down` as the rational resampling factors, so the `Resample` path reduces `(target, fs)` to coprime integers through `math.gcd` before the call — `(48000, 44100) -> (160, 147)` builds the minimal polyphase FIR rather than the degenerate `up=48000`/`down=44100` kernel a raw-rate pass would force. `find_peaks` returns the `(indices, properties)` pair (row [08]) whose `properties["prominences"]` carries the prominence fold; the modern `ShortTimeFFT` object (row [05]) is the deferred upgrade, the free `spectrogram` the confirmed entrypoint this owner cites.
- [SPECTRAL_READOUT]: the dominant-band read is the reused `SpectralReadout` axis from `analysis/transform.md#TRANSFORM` rather than a hardcoded `float(f[argmax(pxx)])` repeated per stationary arm. `_welch_band` runs `scipy.signal.welch` and folds the PSD through `readout.fold(f, pxx)` — `PEAK` reads the maximum-magnitude bin, `CENTROID` the magnitude-weighted mean frequency, `BANDWIDTH` the weighted spread, `FLATNESS` the Wiener entropy — so the spectral output shape varies with the `readout` row the same way the spectral input shape varies with the op case. Band power is the catalogued `np.einsum("i->", pxx)` PSD reduction scaled by the uniform Welch bin width `df` (`libs/python/.api/numpy.md` reduction row [07] names `einsum` the general reduction/contraction owner; `np.trapezoid` is not catalogued), and the per-band/per-node/per-scale energies are the `einsum("i,i->", ravel(c), ravel(c))` magnitude-square contraction shared with `analysis/transform.md#TRANSFORM`. The scalogram dominant-scale read stays an `argmax` over the per-scale energy spine because the scale-to-frequency mapping, not a magnitude readout, owns that projection; its geometric `logspace(base=e)` grid is sized by the `Scalogram` `resolution` parameter when no explicit `scales` tuple is supplied, never a hardcoded bin count.
- [PYWT_WAVELET]: `pywt` (dist `pywavelets`) rides the companion `python_version<'3.15'` band beside the scipy path (C-over-numpy, no cp315 wheel); the `wavedec`/`waverec`, `swt`/`iswt`, `swt_max_level`/`dwt_max_level`, `mra`, `cwt`/`scale2frequency`, `WaveletPacket`, `threshold`/`threshold_firm`, `ravel_coeffs`/`unravel_coeffs`, and `Wavelet.dec_len` spellings verify against `compute/.api/pywavelets.md` `[03]`-`[04]` under a uv-sync reflection pass once the `pywavelets` wheel resolves. The `WAVELET_ROUTES` `Map[DecompMode, WaveletRoute]` carries the `(forward, inverse, max_level)` triple per decimation axis — `DecompMode.DECIMATED` the `(wavedec, waverec, dwt_max_level)` row and `DecompMode.STATIONARY` the `(swt with trim_approx=True, iswt, swt_max_level)` row — so `_decompose` indexes one forward and one inverse body the way `analysis/transform.md#TRANSFORM`'s `FOURIER_ROUTES` indexes the basis, never the `mode is STATIONARY` ternary repeated at decomposition and reconstruction (the `.api` decimation-axis rows as a callable table, never duplicated engines). `Multiresolution` routes `mra` whose additive bands SUM to the input (`sum(mra(x)) ≈ x`, the analysis-of-variance decomposition distinct from the `wavedec` coefficient pyramid, the `.api` additive axis), and the packet tree reads `order="freq"` for the frequency-ordered best basis. The `Scalogram` path runs `cwt(..., method="fft")` (row [04], `method='conv'`/`'fft'`) — the frequency-domain CWT, not the direct convolution, because the geometric scale grid is the many-scales case the FFT pass collapses to one shared transform rather than a per-scale convolution. Perfect reconstruction `waverec(wavedec(x)) ≈ x` / `iswt(swt(x)) ≈ x` and additive closure `sum(mra(x)) ≈ x` are the verification invariants the un-denoised `Decompose` and `Multiresolution` residual folds record; under a `ThresholdMode` other than `NONE` the `Decompose` residual measures the denoise deviation instead.
- [PYWT_THRESHOLD]: the denoise axis spans both documented coefficient-shrink surfaces as one `ThresholdMode` row set whose `shrink(pywt, value)` method owns the per-row callable — `threshold(data, value, mode)` modes `soft`/`hard`/`garrote`/`greater`/`less` (`compute/.api/pywavelets.md` `[03]`-row [05]) on the default arm and the two-knee `threshold_firm(data, value_low, value_high)` (row [06]) on the `FIRM` arm, the `NONE` arm returning `None` so the un-denoised list reconstructs verbatim. The shrink lives on the enum rather than an inline `is FIRM` ternary in `_decompose`, collapsing the branch into the vocabulary the way `SpectralReadout.fold` lives on its axis. The VisuShrink denoise maps the shrink across every detail band `coeffs[1:]` with `value = sigma * sqrt(2 log n)` over `sigma = median(|cD_finest|) / 0.6745`, leaving the approximation band `coeffs[0]` unthresholded; the shrunk coefficient list is the same nested structure the `WAVELET_ROUTES` forward returned, reconstructing directly through the row's inverse — the per-detail-band threshold the `[04]-[IMPLEMENTATION_LAW]` denoising-axis rule places between `wavedec` and `waverec`, never a parallel denoiser op and never a hand-built `coeff_slices` index map (the `ravel_coeffs`/`unravel_coeffs` packing bijection of row `[04]`-[06] is the distinct optimizer-rail bridge the `SIGNAL_STACKING` jax/optax row owns for the flat L1-penalized inverse, not this in-place denoise).
- [ARRAY_ADMISSION]: `Signal.apply` admits the operand through `ArrayPayload.admit(ArrayOp.Live(samples), (), FiniteGate.REJECT)` from `numerics/array.md#PAYLOAD` and binds the resolved payload into the `boundary` thunk, so a non-finite input returns the admission fault rather than producing a NaN-poisoned spectrum, the backend `xp` resolves once through `array_namespace`, and the `ArrayPayload.content_key` keys the `SignalReceipt` the way `analysis/spatial.md#SPATIAL` keys its point-set query through `ContentIdentity`. The `FiniteGate.REJECT` row (the `numerics/array.md#PAYLOAD` finite axis forbidding any NaN or ±inf) is the admission policy; the owner never re-rolls the `array_namespace` dispatch or the `FiniteGate.forbidden`/`violated` masked reduction the payload owner holds.
- [RECEIPT_SHAPE]: `SignalReceipt.contribute` yields into the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`), and the row is `Receipt.of("compute.signal", ("emitted", op_tag, facts))` — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit, and never a `tuple[Receipt, ...]` that would drift from the `yield`-into-`Iterable` shape the co-cited `analysis/transform.md#TRANSFORM` and `analysis/spatial.md#SPATIAL` contributors hold. `SignalEvidence` parameterizes the output shape and owns its own `facts()` projection, so the receipt spreads only the slots the matched evidence carries beside the `op`/`length`/`content_key.project("hex")` render, the dedicated `Peaks` case keeping a `find_peaks` receipt off the spectral slots. `apply` is the `RuntimeRail[SignalReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `apply` but the study spine harvesting the resolved `SignalReceipt` contributor through the `@receipted` aspect on the `Ok` arm — the same convention `analysis/transform.md#TRANSFORM` and `analysis/spatial.md#SPATIAL` hold, the receipt the contributor and the rail the boundary form.
