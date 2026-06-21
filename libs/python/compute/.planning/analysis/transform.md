# [PY_COMPUTE_TRANSFORM]

The one frequency-domain transform owner spanning the pocketfft discrete-Fourier family, the trigonometric cosine/sine transforms, the FFTLog fast Hankel transform, and the FFT-backed analytic signal on a single `Transform`. `TransformOp` discriminates the complex DFT, the half-spectrum real DFT, the Hermitian-spectrum DFT, the discrete cosine and sine transforms, the log-radial Hankel transform, and the analytic-signal envelope over `scipy.fft` and `scipy.signal.hilbert`, so a spectrum, an energy-compacted trigonometric basis, a log-radial Hankel coefficient set, and the instantaneous amplitude/phase/frequency of a non-stationary mode are first-class transform evidence on the same owner rather than a per-transform method family. Every Fourier path resolves one `FourierBasis` row through the `FOURIER_ROUTES` callable table — a `(forward, inverse, freqs)` triple per basis — so the eight pocketfft entrypoints (`fft`/`ifft`, `fftn`/`ifftn`, `rfft`/`irfft`, `hfft`/`ihfft`) collapse to one forward body and one inverse body that index the basis, never a nested `if real else ...` ternary ladder selecting an entrypoint inline. The terminal `TransformEvidence` `@tagged_union` over `spectrum`/`compaction`/`envelope`/`roundtrip` cases owns its own `facts()` projection, and `TransformReceipt(op, length, content_key, evidence)` carries it whole — one discriminated outcome the receipt spreads, never a struct of mutually-exclusive default-`0.0` fields — the same collapse `analysis/signal.md#DSP` holds over `Spectral`/`Multiresolution`/`Scale`/`Packet`, `analysis/symbolic.md#OP` over its `Outcome`, and `analysis/spatial.md#SPATIAL` over `Proximity`/`Complex`/`Boundary`. The spectral readout is itself parameterized: one `SpectralReadout` axis folds the linear amplitude spine `|X(f)|` to a `PEAK`, `CENTROID`, `BANDWIDTH`, or `FLATNESS` scalar through one masked projection (`FLATNESS` lifting to power internally by squaring the amplitude, the librosa `power=2.0` convention), so the dominant-band read is a polymorphic output fold rather than a hardcoded `argmax`. The operand admits through `numerics/array.md#PAYLOAD` — `ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT)` resolves the backend `xp` once for the Array-API-aware `scipy.fft` of scipy 1.17, fails a non-finite operand at admission rather than poisoning the spectrum, and keys the `TransformReceipt` by the `ArrayPayload.content_key` so a repeated transform on identical samples is a cache hit by reference. The `scipy.fft` Fourier/Trigonometric/Hankel arms ride that resolved `xp`; the analytic arm is numpy-resident on `xn = np.asarray(x)` because `scipy.signal.hilbert` is jax-skipped and its Array-API support is experimental, the same numpy-floor stance `analysis/signal.md#DSP` holds. These are in-memory transforms; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners. `TransformReceipt` is the `ReceiptContributor` the study spine harvests through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm, so the dominant-band, reconstruction-residual, energy-concentration, and envelope evidence streams without an inline `emit`.

## [01]-[INDEX]

- [01]-[TRANSFORM]: complex/real/Hermitian n-D DFT with an optional inverse round trip over the `FOURIER_ROUTES` callable table, the n-D discrete cosine/sine transforms, the FFTLog fast Hankel transform, and the analytic-signal envelope on one `Transform` owner, the terminal `TransformEvidence` union owning the `facts()` projection one `TransformReceipt` carries whole, the `SpectralReadout` axis folding the linear amplitude spine `|X(f)|` to a parameterized band scalar the Fourier and Hankel paths share (`FLATNESS` squaring to power internally), and the `ArrayPayload`-admitted `ContentIdentity` key the study spine's `@receipted` aspect harvests off the `Ok`-arm contributor.

## [02]-[TRANSFORM]

- Owner: `Transform` — the ONE frequency-domain transform owner; `TransformOp` discriminates `Fourier(basis, axes, readout, pad, invert)` (the pocketfft DFT family routed through `FOURIER_ROUTES`), `Trigonometric(kind, variant, axes, keep)` (`scipy.fft.dct`/`dst` and the n-D `dctn`/`dstn` selected by a non-empty `axes`, `TrigKind` choosing cosine/sine, `variant` the I-IV pocketfft `type`, and `keep` the leading-coefficient fraction the compaction read measures), `Analytic(readout)` (`scipy.signal.hilbert`), and `Hankel(dln, mu, readout, invert)` (the FFTLog `scipy.fft.fht`/`ifht` over a log-spaced periodic sequence, `dln` the uniform log-spacing and `mu` the transform order), all rows on the same owner, never a per-transform method family. `FourierBasis` discriminates `COMPLEX`/`REAL`/`HERMITIAN` and keys `FOURIER_ROUTES` so the 1-D forward transform, the matching inverse, and the `fftfreq`/`rfftfreq` grid builder live as one table row rather than an inline ternary; a non-empty `axes` runs the complex `fftn`/`ifftn` mirror on the `fftfreq` grid regardless of `basis` (scipy catalogs no `rfftn`/`hfftn`), under one `set_workers` pool, padding each transformed axis to its own `next_fast_len`. The n-D magnitude marginalizes to the lead-axis spine through one `xp.max` off-axis projection — `readout.fold` is order-invariant, so the grid and spine never co-order through `fftshift`.
- Entry: `Transform.apply` admits the operand through `ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT)`, binds the resolved payload into `boundary(f"transform.{op.tag}", ...)`, and returns `RuntimeRail[TransformReceipt]`. The Fourier path resolves the `FourierBasis` row, pads each transformed axis to its own `next_fast_len` under the `PadPolicy`, runs the row's 1-D forward transform (or the complex `fftn` mirror under a `set_workers` thread pool for the n-D `axes` case), builds the frequency grid against the sample spacing along the lead axis, marginalizes the n-D magnitude to the lead-axis spine through one `xp.max` off-axis projection, folds that spine to the `SpectralReadout` scalar, and reads the total Parseval energy through one `xp.sum` magnitude-square reduction; when `invert` is set it runs the row's inverse over the spectrum it already holds and folds the normalized round-trip residual into a `roundtrip`-tagged `TransformEvidence` rather than re-deriving the forward transform. The trigonometric path runs the orthonormal `dct`/`dst` (or the n-D `dctn`/`dstn` over `axes`) and folds the fraction of total spectral energy held by the leading `keep`-fraction of coefficients through one descending `xp.flip(xp.sort(...))` `cumsum`, the compaction window a parameter rather than a fixed top-decile. The analytic path materializes the one-shot numpy floor `xn = np.asarray(x)` (scipy.signal.hilbert is numpy-resident for this owner: jax-skipped item-assignment plus the experimental `SCIPY_ARRAY_API` gate), runs `hilbert`, reads the mean envelope as the analytic-signal magnitude `np.abs` and the instantaneous frequency from the unwrap-free product estimator `np.angle(np.conj(z[:-1]) * z[1:]) * fs / (2*np.pi)`, reports the magnitude-weighted mean as the central tone, and folds the `SpectralReadout` band of the instantaneous-frequency track (folded as a linear amplitude spine); the `FiniteGate.REJECT` admission already excludes non-finite input, so the numpy track needs no `nan_to_num` floor guard. The Hankel path runs `fht(x, dln, mu)` over the log-spaced periodic operand, builds the conjugate log-radial abscissa `exp(dln * (arange(n) - n/2))` so the readout reads a real radial frequency, folds the coefficient magnitude through the same `SpectralReadout` axis and Parseval `xp.sum` energy as the Fourier path, and on `invert` runs `ifht` and folds the round-trip residual into the shared `roundtrip` evidence rather than minting a Hankel-specific outcome shape. The `FiniteGate.REJECT` admission fails a non-finite operand before any transform, so a clean `xpx.nan_to_num(..., xp=xp)` inside the spine guards only the divide/log degeneracy of an all-zero band, and an admission fault returns the boundary fault that `ArrayPayload.admit` raises rather than producing a NaN-poisoned spectrum.
- Output: `TransformEvidence` parameterizes the result shape the way the input is parameterized — `spectrum(readout, band_hz, energy)` for a forward DFT, `compaction(leading, concentration, energy)` for a trigonometric basis, `envelope(mean, inst_hz, band_hz)` for the analytic signal, and `roundtrip(band_hz, energy, residual)` for an inverted Fourier path — and owns the total `facts()` projection the receipt spreads, so each shape names only its own slots: a `spectrum` receipt names `readout`/`band_hz`/`spectral_energy`, a `compaction` receipt `leading`/`energy_concentration`/`spectral_energy`, an `envelope` receipt `mean_envelope`/`instantaneous_hz`/`band_hz`, and a `roundtrip` receipt `band_hz`/`spectral_energy`/`reconstruction_residual` — collapsing what would be one struct of six default-`0.0` fields into one discriminated carrier, exactly as `analysis/signal.md#DSP` collapses `Spectral`/`Multiresolution`/`Scale`/`Packet` and `analysis/spatial.md#SPATIAL` collapses `Proximity`/`Complex`/`Boundary`. A new readout is one `SpectralReadout` row; a new outcome is one `TransformEvidence` case plus its `facts()` arm.
- Receipt: `TransformReceipt.of(op, length, key, evidence)` folds the `(op_tag, length, content_key, evidence)` carrier with no per-case projection — the discrimination lives in `TransformEvidence.facts()`, not a `match` over nullable fields — the same shape `analysis/signal.md#DSP` holds. `contribute` returns the `Iterable[Receipt]` the `ReceiptContributor` Protocol declares, one `Receipt.of("compute.transform", ("emitted", op_tag, facts))` row — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit — whose `facts` spreads the `op`/`length`/`content_key.project("hex")` render plus the matched `TransformEvidence.facts()` slots a study run records through `experiments/study.md#STUDY`. `apply` is the `RuntimeRail[TransformReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `apply` but the study spine harvesting the resolved `TransformReceipt` contributor through the `runtime/observability/receipts#RECEIPT` `@receipted` aspect on the `Ok` arm — the same convention `analysis/signal.md#DSP` and `analysis/spatial.md#SPATIAL` hold, the receipt the contributor and the rail the boundary form.
- Packages: `scipy` (`fft.fft`/`ifft`, `fft.fftn`/`ifftn`, `fft.rfft`/`irfft`, `fft.hfft`/`ihfft`, `fft.dct`/`dst`, `fft.dctn`/`dstn`, `fft.fht`/`ifht`, `fft.fftfreq`/`rfftfreq`, `fft.next_fast_len`, `fft.set_workers`, `signal.hilbert`), `array-api-compat` (`array_namespace` resolving the one backend `xp` the Array-API-aware `scipy.fft` dispatches on for the Fourier/Trigonometric/Hankel arms), `array-api-extra` (`xpx.nan_to_num` the all-zero-band sanitizer and `xpx.default_dtype(xp, "real floating")` the backend-agnostic abscissa dtype the Hankel grid allocates against rather than a hardcoded `xp.float64`, both scoped to the `scipy.fft` arms), the resolved `xp` standard namespace scoped to the `scipy.fft` arms (`asarray`, `arange`, `abs`, `conj`, `sort`, `flip`, `cumsum`, `argmax`, `max`, `sum` the weighted-mean and Parseval contraction owner, `sqrt`, `mean`, `log`, `exp`, `reshape`, `zeros`, `linalg.norm` the flattened complex round-trip residual, the `pi` constant), `numpy` (`asarray`/`abs`/`angle`/`conj`/`sum`/`mean`/`zeros`/`pi` the analytic-arm numpy floor — `scipy.signal.hilbert` is numpy-resident per `analysis/signal.md#DSP`), `expression` (`tagged_union`/`tag`/`case` the `TransformOp` and `TransformEvidence` ADTs, `Map` the `FOURIER_ROUTES` table), `msgspec` (`Struct(frozen=True)` the `TransformReceipt`), `numerics/array.md#PAYLOAD` (`ArrayPayload.admit`/`ArraySource.Live`/`FiniteGate` admitting the operand and keying the `content_key`), runtime (`RuntimeRail`, `boundary`, `Receipt`/`ReceiptContributor` from `runtime/receipts`, `ContentKey` carried by the admitted payload, the `runtime/observability/receipts#RECEIPT` `@receipted` study-spine harvest of the `Ok`-arm contributor, not an import this owner threads).
- Growth: a new transform is one `TransformOp` case (the `Hankel` row is exactly this — one case folding into the existing `spectrum`/`roundtrip` evidence, zero new outcome shape); a new spectral basis is one `FourierBasis` row plus its `FOURIER_ROUTES` triple; an n-D spectrum is a non-empty `axes` value on the existing `Fourier`/`Trigonometric` row; a new trigonometric variant is one `TrigKind` row or `variant` value; the compaction window is the `keep` fraction on the `Trigonometric` row, never a body-buried literal; a new band readout is one `SpectralReadout` row; a new terminal outcome is one `TransformEvidence` case plus its `facts()` arm; zero new surface.
- Boundary: in-memory frequency-domain transforms only — the complex/real/Hermitian DFT family, the trigonometric transforms, the FFTLog fast Hankel transform, and the analytic-signal envelope are in-scope; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners, and the stationary Welch spectral estimate, the time-frequency `ShortTimeFFT`, polyphase resampling, and the multiresolution wavelet fold stay on `analysis/signal.md#DSP`. `scipy` carries the companion `python_version<'3.15'` band (no cp315 wheel), so every body is authored against the documented API on the gated band; scipy 1.17 makes `scipy.fft` Array-API-aware so the Fourier/Trigonometric/Hankel arms dispatch through the backend `xp` the `numerics/array.md#PAYLOAD` admission resolves. `scipy.signal.hilbert` is NOT freely Array-API-portable — it is jax-skipped (item assignment) and its Array-API support is experimental behind `SCIPY_ARRAY_API=1` (off by default) — so the analytic arm is numpy-resident on `xn = np.asarray(x)`, the same numpy-floor stance `analysis/signal.md#DSP` holds for its skip-backend `scipy.signal` entrypoints and `analysis/spatial.md#SPATIAL` holds for `scipy.spatial`. Perfect inversion `ifft(fft(x)) ≈ x`, `ifftn(fftn(x)) ≈ x`, `ihfft(hfft(x)) ≈ x`, and `ifht(fht(x)) ≈ x` (pocketfft's and FFTLog's documented exact round trips) is the verification invariant the `invert` fold records as the reconstruction residual on the same arm that produced the spectrum. The `numerics/array.md#PAYLOAD` admission owns backend resolution and finite policy; this owner never re-rolls the `FiniteGate` reduction, and `array_namespace(samples)` recovers the same resolved `xp` the admission used so every `scipy.fft`-arm fold is `xp.<op>`/`xpx.<op>(..., xp=xp)` rather than a hardcoded `np.`, the analytic arm excepted. The deleted forms are a `TransformReceipt` of six default-`0.0` fields where the `TransformEvidence` `facts()` discriminates the outcome, a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory rejects, a `contribute` returning one bare `Receipt` where the `ReceiptContributor` port declares `Iterable[Receipt]`, an unadmitted `np.asarray(samples)` intake bypassing the `ArrayPayload` finite gate and `content_key` mint, a hardcoded `import numpy as np` body on the `scipy.fft` arms that discards the admitted operand's backend where `array_namespace` threads the one `xp` (the `array-api-compat` `[RAIL_LAW]` deletes the vendor import in a generic Array-API kernel — the analytic arm is the documented sibling exception because it is pinned to the numpy-resident `scipy.signal.hilbert`), a `np.errstate(divide=..., invalid=...)` numpy-only scope a jax/dask spine rejects on the `scipy.fft` arms where `xpx.nan_to_num(..., xp=xp)` is the Array-API sanitization owner, an `np.unwrap`/`np.diff` branch-cut phase-derivative chain (neither an Array API standard op, neither catalogued) where the unwrap-free `np.angle(np.conj(z[:-1]) * z[1:])` product estimator reads the instantaneous frequency, an `np.median` central read where the magnitude-weighted `xp.sum(track * weight)` mean is the sample-independent tone, an `[::-1]` negative-stride descending sort where `xp.flip(xp.sort(...))` holds on a graph backend, a nested `fft.rfft(...) if real else fft.fftn(...) if axes else ...` ternary where `FOURIER_ROUTES` indexes the basis, a hardcoded `argmax` dominant-band read where the `SpectralReadout` axis folds the spine, a hardcoded `energy.size // 10` compaction window where the `keep` fraction parameterizes it, a single-threaded n-D transform where `set_workers` pools the pocketfft backend, and a 1-D-only trigonometric path where `dctn`/`dstn` mirror the n-D Fourier reach.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Literal, assert_never

import array_api_extra as xpx
import numpy as np
from array_api_compat import array_namespace
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.content_identity import ContentKey
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

if TYPE_CHECKING:
    # `ModuleType` types the resolved `xp`/`fft`/`sig` module params; the one structural backend
    # `Array` is the canonical `numerics/array.md#PAYLOAD` union (`NDArray[Any] | jax.Array |
    # da.Array | SparseArray`), imported here rather than redefined so the signature never drifts
    # the parameter or drops `SparseArray` — the `scipy.fft` arms are Array-API-aware, so a transform
    # returns an array of the same `xp`; the analytic arm runs the numpy-resident `scipy.signal.hilbert`
    # and returns an `NDArray` (still a member of the `Array` union), and the gated backend symbols
    # never import at runtime.
    from types import ModuleType

    from rasm.compute.numerics.array import Array

type FourierRoute = tuple[
    Callable[..., "Array"],  # forward transform over the lead axis or the `axes` tuple
    Callable[..., "Array"],  # matching inverse over the held spectrum (takes the original `n`)
    Callable[[int, float], "Array"],  # frequency grid for the lead-axis bin count
]


class FourierBasis(StrEnum):
    COMPLEX = "complex"      # fft/ifft, lifted to fftn/ifftn over a non-empty `axes`
    REAL = "real"            # rfft/irfft half-spectrum of real input
    HERMITIAN = "hermitian"  # hfft/ihfft transform of a Hermitian-symmetric spectrum


class TrigKind(StrEnum):
    COSINE = "cosine"
    SINE = "sine"


class PadPolicy(StrEnum):
    EXACT = "exact"        # transform the operand verbatim
    FAST = "fast"          # zero-pad each transformed axis to its own `next_fast_len` for the pocketfft radix


class SpectralReadout(StrEnum):
    PEAK = "peak"            # frequency at the maximum-magnitude bin
    CENTROID = "centroid"    # magnitude-weighted mean frequency
    BANDWIDTH = "bandwidth"  # magnitude-weighted spectral spread about the centroid
    FLATNESS = "flatness"    # geometric-over-arithmetic mean (Wiener entropy)

    def fold(self, freqs: "Array", amplitude: "Array") -> float:
        # the spine is the LINEAR AMPLITUDE spectrum `|X(f)|`, never power: `PEAK`/`CENTROID`/`BANDWIDTH`
        # read amplitude directly (the librosa magnitude convention — `spectral_centroid`/`spectral_bandwidth`
        # consume `S` unsquared), while `FLATNESS` lifts to power internally by squaring the amplitude
        # (librosa `spectral_flatness` default `power=2.0`). An owner holding a power/energy spine (the
        # `analysis/signal.md#DSP` Welch/spectrogram/packet paths) square-roots before the fold so this one
        # input domain stays amplitude. Every reduction is an Array API standard op (`sum`/`mean`/`max`/
        # `argmax`), so the one axis folds a numpy, jax, or dask spine, resolving the namespace off the
        # amplitude operand it is handed.
        xp = array_namespace(amplitude)
        absf = xp.abs(freqs)
        weight = amplitude / (xp.sum(amplitude) + 1e-30)
        match self:
            case SpectralReadout.PEAK:
                return float(absf[xp.argmax(amplitude)])
            case SpectralReadout.CENTROID:
                return float(xp.sum(absf * weight))
            case SpectralReadout.BANDWIDTH:
                center = xp.sum(absf * weight)
                return float(xp.sqrt(xp.sum((absf - center) ** 2 * weight)))
            case SpectralReadout.FLATNESS:
                power = amplitude * amplitude
                safe = power + 1e-30
                return float(xp.exp(xp.mean(xp.log(safe))) / xp.mean(safe))
            case _ as unreachable:
                assert_never(unreachable)


# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class TransformEvidence:
    # the output is parameterized as tightly as the input: one discriminated carrier per
    # outcome shape, not one struct of six default-zero fields. `facts()` is the total
    # projection the receipt spreads, so each shape names only its own slots.
    tag: Literal["spectrum", "compaction", "envelope", "roundtrip"] = tag()
    spectrum: tuple[SpectralReadout, float, float] = case()      # (readout, band_hz, energy)
    compaction: tuple[int, float, float] = case()                # (leading, concentration, energy)
    envelope: tuple[float, float, float] = case()                # (mean, inst_hz, band_hz)
    roundtrip: tuple[float, float, float] = case()               # (band_hz, energy, residual)

    @staticmethod
    def Spectrum(readout: SpectralReadout, band_hz: float, energy: float) -> "TransformEvidence":
        return TransformEvidence(spectrum=(readout, band_hz, energy))

    @staticmethod
    def Compaction(leading: int, concentration: float, energy: float) -> "TransformEvidence":
        return TransformEvidence(compaction=(leading, concentration, energy))

    @staticmethod
    def Envelope(mean: float, inst_hz: float, band_hz: float) -> "TransformEvidence":
        return TransformEvidence(envelope=(mean, inst_hz, band_hz))

    @staticmethod
    def Roundtrip(band_hz: float, energy: float, residual: float) -> "TransformEvidence":
        return TransformEvidence(roundtrip=(band_hz, energy, residual))

    def facts(self) -> dict[str, object]:
        match self:
            case TransformEvidence(tag="spectrum", spectrum=(readout, band, energy)):
                return {"readout": readout.value, "band_hz": f"{band:.3f}", "spectral_energy": f"{energy:.3e}"}
            case TransformEvidence(tag="compaction", compaction=(leading, concentration, energy)):
                return {"leading": leading, "energy_concentration": f"{concentration:.3f}", "spectral_energy": f"{energy:.3e}"}
            case TransformEvidence(tag="envelope", envelope=(mean, inst, band)):
                return {"mean_envelope": f"{mean:.3e}", "instantaneous_hz": f"{inst:.3f}", "band_hz": f"{band:.3f}"}
            case TransformEvidence(tag="roundtrip", roundtrip=(band, energy, residual)):
                return {"band_hz": f"{band:.3f}", "spectral_energy": f"{energy:.3e}", "reconstruction_residual": f"{residual:.3e}"}
            case _ as unreachable:
                assert_never(unreachable)


class TransformReceipt(Struct, frozen=True):
    op: str
    length: int
    content_key: ContentKey
    evidence: TransformEvidence

    @staticmethod
    def of(op: str, length: int, key: ContentKey, evidence: TransformEvidence) -> "TransformReceipt":
        return TransformReceipt(op, length, key, evidence)

    def contribute(self) -> Iterable[Receipt]:
        facts = {"op": self.op, "length": self.length, "content_key": self.content_key.project("hex"), **self.evidence.facts()}
        yield Receipt.of("compute.transform", ("emitted", self.op, facts))


@tagged_union(frozen=True)
class TransformOp:
    tag: Literal["fourier", "trigonometric", "analytic", "hankel"] = tag()
    fourier: tuple[FourierBasis, tuple[int, ...], SpectralReadout, PadPolicy, bool] = case()
    trigonometric: tuple[TrigKind, int, tuple[int, ...], float] = case()  # (kind, variant, axes, keep fraction)
    analytic: SpectralReadout = case()
    hankel: tuple[float, float, SpectralReadout, bool] = case()  # (dln log-spacing, mu order, readout, invert)

    @staticmethod
    def Fourier(
        basis: FourierBasis = FourierBasis.COMPLEX,
        axes: tuple[int, ...] = (),
        readout: SpectralReadout = SpectralReadout.PEAK,
        pad: PadPolicy = PadPolicy.EXACT,
        invert: bool = False,
    ) -> "TransformOp":
        return TransformOp(fourier=(basis, axes, readout, pad, invert))

    @staticmethod
    def Trigonometric(kind: TrigKind = TrigKind.COSINE, variant: int = 2, axes: tuple[int, ...] = (), keep: float = 0.1) -> "TransformOp":
        # `keep` is the leading-coefficient fraction the energy-compaction read measures, so the
        # compaction window is a parameter the way `Scalogram.resolution` sizes its scale grid,
        # never a hardcoded `// 10` literal buried in the body.
        return TransformOp(trigonometric=(kind, variant, axes, keep))

    @staticmethod
    def Analytic(readout: SpectralReadout = SpectralReadout.PEAK) -> "TransformOp":
        return TransformOp(analytic=readout)

    @staticmethod
    def Hankel(dln: float, mu: float = 0.0, readout: SpectralReadout = SpectralReadout.PEAK, invert: bool = False) -> "TransformOp":
        return TransformOp(hankel=(dln, mu, readout, invert))


# --- [TABLES] ---------------------------------------------------------------------------

# one row per FourierBasis carrying the (forward, inverse, freq-grid) triple over the
# lead-axis 1-D entrypoints; `@cache` defers the import-banned scipy load to first call and
# memoizes the one Map so the table builds once. The n-D path runs the complex fftn/ifftn
# mirror directly. Every 1-D inverse accepts the original `n`, so the round trip reconstructs
# to the source length under any PadPolicy.
@cache
def _fourier_routes() -> Map[FourierBasis, FourierRoute]:
    import scipy.fft as fft

    return Map.of_seq([
        (FourierBasis.COMPLEX, (fft.fft, fft.ifft, fft.fftfreq)),
        (FourierBasis.REAL, (fft.rfft, fft.irfft, fft.rfftfreq)),
        (FourierBasis.HERMITIAN, (fft.hfft, fft.ihfft, fft.fftfreq)),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def apply(samples: object, fs: float, op: TransformOp) -> "RuntimeRail[TransformReceipt]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: boundary(f"transform.{op.tag}", lambda: _apply(samples, fs, op, payload.content_key))
    )


def _apply(samples: object, fs: float, op: TransformOp, key: ContentKey) -> TransformReceipt:
    import scipy.fft as fft
    import scipy.signal as sig

    # `array_namespace` resolves the backend once for the whole body the way `numerics/array.md#PAYLOAD`
    # admits the operand; the Array-API-aware `scipy.fft` then dispatches on the same `xp`, so the
    # post-transform folds read `xp.<op>` rather than a hardcoded `np.`. Admission already rejected any
    # non-finite operand, so `xpx.nan_to_num` inside the bodies guards only the all-zero-band log/ratio
    # degeneracy — the Array-API sanitization owner, not a numpy-only `errstate` scope a jax/dask spine
    # would reject.
    xp = array_namespace(samples)
    x = xp.asarray(samples)
    spacing = 1.0 / fs
    match op:
        case TransformOp(tag="fourier", fourier=(basis, axes, readout, pad, invert)):
            return TransformReceipt.of("fourier", x.size, key, _fourier(xp, fft, x, spacing, basis, axes, readout, pad, invert))
        case TransformOp(tag="trigonometric", trigonometric=(kind, variant, axes, keep)):
            return TransformReceipt.of(kind.value, x.size, key, _trigonometric(xp, fft, x, kind, variant, axes, keep))
        case TransformOp(tag="analytic", analytic=readout):
            # the analytic arm folds on the numpy floor (scipy.signal.hilbert is jax-skipped/experimental),
            # so it takes `x`/`fs` without the resolved `xp` the Array-API `scipy.fft` arms thread.
            return TransformReceipt.of("analytic", x.size, key, _analytic(sig, x, fs, readout))
        case TransformOp(tag="hankel", hankel=(dln, mu, readout, invert)):
            return TransformReceipt.of("hankel", x.size, key, _hankel(xp, fft, x, dln, mu, readout, invert))
        case _ as unreachable:
            assert_never(unreachable)


def _fourier(
    xp: "ModuleType", fft: "ModuleType", x: "Array", spacing: float,
    basis: FourierBasis, axes: tuple[int, ...], readout: SpectralReadout, pad: PadPolicy, invert: bool,
) -> TransformEvidence:
    forward, inverse, grid = _fourier_routes()[basis]
    lead = axes[0] if axes else (x.ndim - 1)
    fast = (lambda a, real: fft.next_fast_len(x.shape[a], real=real)) if pad is PadPolicy.FAST else (lambda a, real: x.shape[a])
    n = fast(lead, basis is FourierBasis.REAL)
    # the n-D path is the complex `fftn`/`ifftn` mirror (scipy catalogs no `rfftn`/`hfftn`), so a
    # non-empty `axes` runs the complex transform on the `fftfreq` grid regardless of `basis`,
    # padding each transformed axis to its OWN fast length rather than forcing the lead length.
    with fft.set_workers(-1):
        spectrum = fft.fftn(x, s=tuple(fast(a, False) for a in axes), axes=axes) if axes else forward(x, n=n, axis=lead)
    freqs = xp.asarray(fft.fftfreq(spectrum.shape[lead], spacing) if axes else grid(n, spacing))
    amplitude = xpx.nan_to_num(xp.abs(spectrum), xp=xp)
    energy = float(xp.sum(amplitude * amplitude))
    # max-project the off-lead axes into the LINEAR AMPLITUDE spine `readout.fold` consumes (a peak
    # track per bin, never a summed blur); `lead` is a valid `amplitude` position because the pocketfft
    # forward preserves rank, and the order-invariant fold needs no fftshift co-ordering.
    spine = amplitude if amplitude.ndim == 1 else xp.max(amplitude, axis=tuple(i for i in range(amplitude.ndim) if i != lead))
    band = readout.fold(freqs[: spine.shape[0]], spine)
    if not invert:
        return TransformEvidence.Spectrum(readout, band, energy)
    # the inverse reconstructs to the ORIGINAL lengths so the residual against `x` is
    # shape-correct under PadPolicy.FAST: the n-D path pins `s` to the source `axes` shape
    # and every 1-D basis inverse pins `n` to the source lead length.
    rebuilt = fft.ifftn(spectrum, s=tuple(x.shape[a] for a in axes), axes=axes) if axes else inverse(spectrum, n=x.shape[lead], axis=lead)
    # `linalg.norm` over the flattened complex difference keeps the Hermitian round trip honest —
    # `ihfft` reconstructs a complex half-spectrum, so forcing `xp.real` would truncate its
    # imaginary part and inflate the residual; the complex norm holds for all three bases.
    residual = float(xp.linalg.norm(xp.reshape(rebuilt - x, (-1,))) / (xp.linalg.norm(xp.reshape(x, (-1,))) + 1e-30))
    return TransformEvidence.Roundtrip(band, energy, residual)


def _hankel(xp: "ModuleType", fft: "ModuleType", x: "Array", dln: float, mu: float, readout: SpectralReadout, invert: bool) -> TransformEvidence:
    # the conjugate log-radial abscissa is `exp(dln * (i - n/2))` so the readout reads a real radial
    # frequency rather than a bare bin index; the coefficient AMPLITUDE feeds the shared fold and the
    # Parseval energy/inverse are the Fourier path's, not Hankel-only.
    coeffs = xp.asarray(fft.fht(x, dln, mu))
    grid = xp.exp(dln * (xp.arange(coeffs.shape[-1], dtype=xpx.default_dtype(xp, "real floating")) - 0.5 * coeffs.shape[-1]))
    amplitude = xpx.nan_to_num(xp.abs(coeffs), xp=xp)
    energy = float(xp.sum(amplitude * amplitude))
    spine = amplitude if amplitude.ndim == 1 else xp.max(amplitude, axis=tuple(range(amplitude.ndim - 1)))
    band = readout.fold(grid[: spine.shape[0]], spine)
    if not invert:
        return TransformEvidence.Spectrum(readout, band, energy)
    rebuilt = xp.asarray(fft.ifht(coeffs, dln, mu))
    residual = float(xp.linalg.norm(xp.reshape(rebuilt - x, (-1,))) / (xp.linalg.norm(xp.reshape(x, (-1,))) + 1e-30))
    return TransformEvidence.Roundtrip(band, energy, residual)


def _trigonometric(xp: "ModuleType", fft: "ModuleType", x: "Array", kind: TrigKind, variant: int, axes: tuple[int, ...], keep: float) -> TransformEvidence:
    transform = (fft.dstn if axes else fft.dst) if kind is TrigKind.SINE else (fft.dctn if axes else fft.dct)
    coeffs = xp.asarray(transform(x, type=variant, axes=axes, norm="ortho") if axes else transform(x, type=variant, norm="ortho"))
    energy = xpx.nan_to_num(xp.abs(coeffs) ** 2, xp=xp)
    total = float(xp.sum(energy)) + 1e-30
    # the leading window is the `keep` fraction of the coefficient count, so the compaction read
    # is parameterized by the op rather than a fixed top-decile; the descending `cumsum` reads the
    # fraction of total energy the leading magnitudes hold — `flip(sort(...))` gives the descending
    # order without an `[::-1]` negative stride a jax/dask spine rejects.
    leading = max(1, int(round(keep * energy.size)))
    descending = xp.flip(xp.sort(xp.reshape(energy, (-1,))))
    concentration = float(xp.cumsum(descending)[leading - 1] / total)
    return TransformEvidence.Compaction(leading, concentration, total)


def _analytic(sig: "ModuleType", x: "Array", fs: float, readout: SpectralReadout) -> TransformEvidence:
    # the analytic arm is numpy-resident: `scipy.signal.hilbert` is jax-skipped (item-assignment) and its
    # Array-API support is experimental behind `SCIPY_ARRAY_API=1`, so this arm materializes the one-shot
    # numpy floor `xn = np.asarray(x)` (the same numpy-floor stance `analysis/signal.md#DSP` holds for its
    # skip-backend `scipy.signal` entrypoints and `analysis/spatial.md#SPATIAL` for `scipy.spatial`), while
    # the Fourier/Trigonometric/Hankel arms keep the `xp`-dispatched Array-API-aware `scipy.fft`. The
    # instantaneous frequency rides the unwrap-free product estimator: the phase increment between
    # consecutive analytic samples is `angle(conj(z[i]) * z[i+1])`, so the branch-cut of a raw `angle`
    # track never needs an `np.unwrap`/`np.diff` pass. The `FiniteGate.REJECT` admission already excludes a
    # non-finite operand, so the numpy track needs no `nan_to_num` floor guard; the magnitude-weighted mean
    # is the central instantaneous frequency the envelope reports beside the `readout`-folded band, and the
    # `inst` track folds as a LINEAR AMPLITUDE spine (`np.abs(inst)`), `FLATNESS` squaring internally.
    xn = np.asarray(x)
    analytic = sig.hilbert(xn)
    envelope = np.abs(analytic)
    increment = np.conj(analytic[:-1]) * analytic[1:]
    inst = np.angle(increment) * fs / (2.0 * np.pi) if analytic.size > 1 else np.zeros(1)
    weight = envelope[1:] / (np.sum(envelope[1:]) + 1e-30)
    central = float(np.sum(inst * weight)) if inst.size else 0.0
    band = readout.fold(inst, np.abs(inst))
    return TransformEvidence.Envelope(float(np.mean(envelope)), central, band)
```

## [03]-[RESEARCH]

- [SCIPY_FFT]: the `scipy.fft.fft`/`ifft`/`fftn`/`ifftn`/`rfft`/`irfft`/`hfft`/`ihfft`/`dct`/`dst`/`dctn`/`dstn`/`fht`/`ifht`/`fftfreq`/`rfftfreq`/`next_fast_len`/`set_workers` spellings carry the `python_version<'3.15'` marker and verify against the `scipy.fft` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` (rows [01]-[12]) under a uv-sync reflection pass once the scipy wheel resolves. The `FOURIER_ROUTES` table carries the `(forward, inverse, freq-grid)` triple per `FourierBasis`, so the `COMPLEX`/`REAL`/`HERMITIAN` bases index one 1-D forward body and one 1-D inverse body, never an inline `fft.rfft(...) if real else ...` ternary. The n-D `axes` case is the complex `fftn`/`ifftn` mirror on the `fftfreq` grid regardless of `basis` (scipy catalogs no `rfftn`/`hfftn`), under one `set_workers(-1)` pocketfft thread pool. Every 1-D inverse accepts the source length `n` and the n-D inverse pins `s` to the source `axes` shape, so the round trip reconstructs to the original shape under any `PadPolicy` and the residual subtraction against `x` is shape-correct. The `PadPolicy.FAST` row zero-pads each transformed axis to its own `next_fast_len(n, real=...)` so the pocketfft radix sees an optimal-length operand; `PadPolicy.EXACT` transforms verbatim. scipy 1.17 makes `scipy.fft` Array-API-aware, so the op dispatches through the backend `xp` the `numerics/array.md#PAYLOAD` admission resolves. Perfect inversion `ifft(fft(x)) ≈ x`, `ifftn(fftn(x)) ≈ x`, and `ihfft(hfft(x)) ≈ x` is the verification invariant the `invert` fold records as the `roundtrip` reconstruction residual.
- [SCIPY_HILBERT]: `scipy.signal.hilbert(x, N, axis)` returns the FFT-backed complex analytic signal and verifies against the `scipy.signal` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` (row [09]). Its Array-API support is EXPERIMENTAL, gated behind `SCIPY_ARRAY_API=1` (off by default), and supported on torch/cupy/dask but jax-skipped (`skip_backends=[("jax.numpy", "item assignment")]`, the JAX immutable-array constraint), with no JIT support; because it is experimental/off-by-default and jax-excluded, the analytic arm is numpy-resident on `xn = np.asarray(x)` (the documented Array-API portability is the `scipy.fft` Fourier/Hankel sibling's), the same numpy-floor stance `analysis/signal.md#DSP` and `analysis/spatial.md#SPATIAL` hold for their non-portable scipy submodules. The envelope is the analytic magnitude `np.abs`, and the instantaneous frequency rides the unwrap-free product estimator `np.angle(np.conj(z[:-1]) * z[1:]) * fs / (2*np.pi)` — the phase increment between consecutive analytic samples carries no branch cut, so the track needs no `np.unwrap`/`np.diff` pass (neither catalogued). The envelope case reports the magnitude-weighted mean instantaneous frequency as the central tone (the `np.sum(track * weight)` of the track against the normalized envelope weight, the catalogued standard reduction replacing both the uncatalogued `np.median` and an uncatalogued `vecdot`), while the `SpectralReadout` axis folds the same instantaneous-frequency track (as a linear amplitude spine) to the band scalar so the analytic path reads the parameterized output projection the Fourier path holds.
- [SCIPY_FHT]: `scipy.fft.fht(a, dln, mu)` / `ifht(A, dln, mu)` is the FFTLog fast Hankel transform of a logarithmically-spaced periodic sequence — `a` the log-spaced operand, `dln` its uniform log-spacing, `mu` the transform order — verifying against the `scipy.fft` `[ENTRYPOINT_SCOPE]` table in `compute/.api/scipy.md` (row [10], the n-D / log transform family beside `dctn`/`dstn`). The `Hankel` row is the log-radial member of the one frequency-domain owner: it reads the coefficient amplitude through the same `SpectralReadout.fold` and Parseval `xp.sum` energy as the Fourier path and records the `ifht` round-trip residual on the shared `roundtrip` evidence, so a radial spectral read is one `TransformOp` case folding into the existing outcome shapes rather than a parallel transform surface or a new `TransformEvidence` case. The canonical fold contract is one input domain — the linear amplitude spine `|X(f)|` — so `FLATNESS` squares to power internally (librosa `power=2.0`) and any power/energy-holding owner (the `analysis/signal.md#DSP` Welch/spectrogram/packet paths) passes `sqrt(.)` before the fold; `PEAK` is scale-invariant. The conjugate log-radial abscissa is `exp(dln * (arange(n) - n/2))` so the readout reads a real radial frequency rather than a bare bin index, and the `arange` allocates against `xpx.default_dtype(xp, "real floating")` (the `compute/.api/array-api-extra.md` `[04]-[ARRAY_API_EXTRA_TOPOLOGY]` backend-agnostic dtype query) rather than a hardcoded `xp.float64` that names a numpy attribute a jax/dask namespace need not expose.
- [SPECTRAL_READOUT]: the `SpectralReadout` axis parameterizes the band output through one `fold` `match` closed by `assert_never`, and the single declared input domain is the LINEAR AMPLITUDE spine `|X(f)|`, never power — the librosa DSP-feature convention is the grounded domain truth. Resolving the backend through `array_namespace(amplitude)` so every reduction is an Array API standard op (`xp.sum`/`xp.mean`/`xp.max`/`xp.argmax`) over a numpy, jax, or dask spine — `PEAK` reads the `xp.argmax` bin (scale-invariant, indifferent to amplitude-vs-power), `CENTROID` the amplitude-weighted mean frequency through the `xp.sum(absf * weight)` contraction (librosa `spectral_centroid` consumes magnitude `S` unsquared), `BANDWIDTH` the amplitude-weighted spread about that centroid (librosa `spectral_bandwidth`, same), and `FLATNESS` the geometric-over-arithmetic mean (Wiener entropy) lifted to POWER INTERNALLY by squaring the amplitude (`power = amplitude * amplitude`, the librosa `spectral_flatness` default `power=2.0`) — so the dominant-band read is a polymorphic output fold reused by the Fourier, Hankel, and analytic paths, never a hardcoded `float(np.abs(freqs[argmax]))`. `transform.md` feeds amplitude (`xp.abs(spectrum)`) on every path, so its feed sites are correct-by-construction; an owner holding a power/energy spine (the `analysis/signal.md#DSP` Welch/spectrogram/packet paths) square-roots before the fold (`sqrt(pxx)`) so this one input domain stays amplitude, and `FLATNESS` squares back to power itself. The Parseval energy is the `xp.sum(amplitude * amplitude)` amplitude-square reduction over any rank (the Array API standard reduction; neither the `einsum` contraction nor `vecdot` is a catalogued `xp` op, so the weighted mean rides the same `xp.sum`), and the n-D amplitude marginalizes to the lead-axis spine through one `xp.max(axis=...)` off-lead projection (the peak frequency track per bin, not a summed blur) rather than a summing fold that would average the spectral structure away. The `FiniteGate.REJECT` admission rejects any non-finite operand before the transform, so `xpx.nan_to_num(..., xp=xp)` quiets only the all-zero-band degeneracy — the `FLATNESS` log/ratio and the `0/0` weight normalization the `CENTROID`/`BANDWIDTH` folds form — the Array-API sanitization owner rather than a numpy-only `np.errstate` scope a graph backend rejects, and rather than guarding against an unfiltered NaN input the admission already excludes.
- [TRANSFORM_EVIDENCE]: the terminal `TransformEvidence` `@tagged_union` over `spectrum`/`compaction`/`envelope`/`roundtrip` carries the whole outcome on the `TransformReceipt(op, length, content_key, evidence)` carrier with no per-case projection, collapsing what would be a `TransformReceipt` of six default-`0.0` fields into one discriminated outcome the receipt spreads through the evidence's own total `facts()` projection, mirroring `analysis/signal.md#DSP`'s `SignalEvidence.facts()`, `analysis/symbolic.md#OP`'s `Outcome` union, and `analysis/spatial.md#SPATIAL`'s `Proximity`/`Complex`/`Boundary` collapse; the `roundtrip` case carries the inverse residual so the forward read is never re-derived for the round trip, and a new outcome is one `TransformEvidence` case plus one `facts()` arm rather than a new nullable field on a fat struct.
- [ARRAY_ADMISSION]: `Transform.apply` admits the operand through `ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT)` from `numerics/array.md#PAYLOAD` and binds the resolved payload into the `boundary` thunk, so a non-finite input returns the admission fault rather than producing a NaN-poisoned spectrum, the backend `xp` resolves once through `array_namespace` for the Array-API-aware `scipy.fft`, and the `ArrayPayload.content_key` keys the `TransformReceipt` through `ContentIdentity` the way `analysis/signal.md#DSP` keys its sample query — a repeated transform on identical samples a cache hit by reference. The `FiniteGate.REJECT` row (the `numerics/array.md#PAYLOAD` finite axis forbidding any NaN or ±inf) is the admission policy; the owner never re-rolls the `array_namespace` dispatch or the `FiniteGate.forbidden`/`violated` reduction the payload owner holds.
- [RECEIPT_SHAPE]: `TransformReceipt.contribute` returns the `Iterable[Receipt]` the `runtime/observability/receipts#RECEIPT` `ReceiptContributor` Protocol declares (`contribute(self) -> Iterable[Receipt]`), and the row is `Receipt.of("compute.transform", ("emitted", op_tag, facts))` — the two-argument shape-polymorphic factory over the `(Phase, subject, facts)` `Evidence` triple, never a four-positional `Receipt.of("emitted", owner, subject, facts)` the factory does not admit. `TransformEvidence` parameterizes the output shape and owns its own `facts()` projection, so the receipt spreads only the slots the matched evidence carries beside the `op`/`length`/`content_key.project("hex")` render. `apply` is the `RuntimeRail[TransformReceipt]` boundary owner (the error arm carries no contributor), so emission is not an `@receipted` decorator on `apply` but the study spine harvesting the resolved `TransformReceipt` contributor through the `@receipted` aspect on the `Ok` arm — the same convention `analysis/signal.md#DSP` and `analysis/spatial.md#SPATIAL` hold, the receipt the contributor and the rail the boundary form.
