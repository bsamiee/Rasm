# [PY_COMPUTE_TRANSFORM]

One frequency-domain transform owner rules: `TransformOp` discriminates the pocketfft Fourier family, the trigonometric cosine/sine transforms, the FFTLog fast Hankel transform, and the FFT-backed analytic signal, folded through the single `apply` entry, so a spectrum, an energy-compacted basis, a log-radial coefficient set, and an instantaneous envelope are transform evidence on one owner rather than a per-transform method family. Pocketfft's eight entrypoints collapse to one forward body and one inverse body indexing a `FOURIER_ROUTES` row per `FourierBasis`, and one `SpectralReadout` axis folds every dominant-band read, so output is parameterized as tightly as input. These are in-memory transforms; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners.

Operands admit through `numerics/array.md#PAYLOAD` for the finite gate and the operand `ContentKey`; the receipt keys the RESULT through the op-owned `identity_buffer` fold, so two different ops over one operand never share a key; the resolved receipt is the `ReceiptContributor` the weave harvest and the study spine consume. `scipy.fft` is Array-API-aware, so the Fourier/Trigonometric/Hankel arms ride the resolved `xp` while the analytic arm stays numpy-resident — `scipy.signal.hilbert` is jax-skipped behind the `SCIPY_ARRAY_API` gate. Every body crosses the runtime thread band under the `RELEASING` trait through `lane.offload`, and the pocketfft worker team binds to `LanePolicy.capacity`, never the unbounded `-1` team that oversubscribes an already-offloaded kernel.

## [01]-[INDEX]

- [01]-[TRANSFORM]: the `TransformOp` Fourier/trigonometric/Hankel/analytic rows folded through one `apply` entry, evidence discriminated over `TransformEvidence`, the dominant-band read folding the `SpectralReadout` axis.

## [02]-[TRANSFORM]

- Owner: `TransformOp` — the one operation union `apply` folds; `FourierBasis` keys the `FOURIER_ROUTES` `(forward, inverse, freqs)` table so the entrypoint family is a row lookup, never an inline ternary ladder; `Trip` is the bounded pass axis on the Fourier and Hankel cases (never an `invert` boolean); the n-D magnitude marginalizes to the lead-axis spine through one `xp.max` off-axis projection because `readout.fold` is order-invariant, so the grid and spine never co-order through `fftshift`.
- Output: `TransformEvidence` parameterizes the result per case — `spectrum`, `compaction`, `envelope`, `roundtrip` — and the `Trip.ROUNDTRIP` pass folds its residual into the shared `roundtrip` case rather than minting a per-transform outcome shape; every `facts()` slot stays a native scalar so the receipt layer aggregates and compares.
- Growth: a new transform is one `TransformOp` case with its `identity_buffer` arm — the `Hankel` row is exactly this, one case folding into the existing `spectrum`/`roundtrip` evidence with zero new outcome shape; a new spectral basis is one `FourierBasis` row with its `FOURIER_ROUTES` triple; an n-D spectrum is a non-empty `axes` value on the existing row; a new trigonometric variant is one `TrigKind` row or `variant` value; a new band readout is one `SpectralReadout` row; a new outcome is one `TransformEvidence` case with its `facts()` arm.

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

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait

# --- [TYPES] ----------------------------------------------------------------------------

if TYPE_CHECKING:
    # `ModuleType` types the resolved `xp`/`fft`/`sig` module params; `Array` imports the canonical `numerics/array.md#PAYLOAD`
    # backend union rather than redefining it, so the signature never drifts a member, and the gated backend symbols never import at runtime.
    from types import ModuleType

    from rasm.compute.numerics.array import Array

type FourierRoute = tuple[
    Callable[..., "Array"],  # forward transform over the lead axis or the `axes` tuple
    Callable[..., "Array"],  # matching inverse over the held spectrum (takes the original `n`)
    Callable[[int, float], "Array"],  # frequency grid for the lead-axis bin count
]


class FourierBasis(StrEnum):
    COMPLEX = "complex"  # fft/ifft, lifted to fftn/ifftn over a non-empty `axes`
    REAL = "real"  # rfft/irfft half-spectrum of real input
    HERMITIAN = "hermitian"  # hfft/ihfft transform of a Hermitian-symmetric spectrum


class TrigKind(StrEnum):
    COSINE = "cosine"
    SINE = "sine"


class PadPolicy(StrEnum):
    EXACT = "exact"  # transform the operand verbatim
    FAST = "fast"  # zero-pad each transformed axis to its own `next_fast_len` for the pocketfft radix


class Trip(StrEnum):
    FORWARD = "forward"  # spectrum evidence off the forward transform
    ROUNDTRIP = "roundtrip"  # forward-then-inverse, folding the reconstruction residual


class SpectralReadout(StrEnum):
    PEAK = "peak"  # frequency at the maximum-magnitude bin
    CENTROID = "centroid"  # magnitude-weighted mean frequency
    BANDWIDTH = "bandwidth"  # magnitude-weighted spectral spread about the centroid
    FLATNESS = "flatness"  # geometric-over-arithmetic mean (Wiener entropy)

    def fold(self, freqs: "Array", amplitude: "Array") -> float:
        # spine is the LINEAR AMPLITUDE spectrum `|X(f)|`, never power — an owner holding a power/energy spine square-roots
        # before the fold, while `FLATNESS` squares back to power internally (the librosa `power=2.0` convention). Every reduction
        # is an Array API standard op, the namespace resolved off the amplitude operand.
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
    tag: Literal["spectrum", "compaction", "envelope", "roundtrip"] = tag()
    spectrum: tuple[SpectralReadout, float, float] = case()  # (readout, band_hz, energy)
    compaction: tuple[int, float, float] = case()  # (leading, concentration, energy)
    envelope: tuple[float, float, float] = case()  # (mean, inst_hz, band_hz)
    roundtrip: tuple[float, float, float] = case()  # (band_hz, energy, residual)

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
        # native scalars only — a `str()`/`f""` coerce erases comparability at the receipt layer; rendering is the export layer's.
        match self:
            case TransformEvidence(tag="spectrum", spectrum=(readout, band, energy)):
                return {"readout": readout.value, "band_hz": band, "spectral_energy": energy}
            case TransformEvidence(tag="compaction", compaction=(leading, concentration, energy)):
                return {"leading": leading, "energy_concentration": concentration, "spectral_energy": energy}
            case TransformEvidence(tag="envelope", envelope=(mean, inst, band)):
                return {"mean_envelope": mean, "instantaneous_hz": inst, "band_hz": band}
            case TransformEvidence(tag="roundtrip", roundtrip=(band, energy, residual)):
                return {"band_hz": band, "spectral_energy": energy, "reconstruction_residual": residual}
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
        yield Receipt.of(EvidenceScope.TRANSFORM.value, ("emitted", self.op, facts))


@tagged_union(frozen=True)
class TransformOp:
    tag: Literal["fourier", "trigonometric", "analytic", "hankel"] = tag()
    fourier: tuple[FourierBasis, tuple[int, ...], SpectralReadout, PadPolicy, Trip] = case()
    trigonometric: tuple[TrigKind, int, tuple[int, ...], float] = case()  # (kind, variant, axes, keep fraction)
    analytic: SpectralReadout = case()
    hankel: tuple[float, float, SpectralReadout, Trip] = case()  # (dln log-spacing, mu order, readout, trip)

    @staticmethod
    def Fourier(
        basis: FourierBasis = FourierBasis.COMPLEX,
        axes: tuple[int, ...] = (),
        readout: SpectralReadout = SpectralReadout.PEAK,
        pad: PadPolicy = PadPolicy.EXACT,
        trip: Trip = Trip.FORWARD,
    ) -> "TransformOp":
        return TransformOp(fourier=(basis, axes, readout, pad, trip))

    @staticmethod
    def Trigonometric(kind: TrigKind = TrigKind.COSINE, variant: int = 2, axes: tuple[int, ...] = (), keep: float = 0.1) -> "TransformOp":
        # `keep` parameterizes the leading-coefficient window the compaction read measures, never a hardcoded top-decile in the body.
        return TransformOp(trigonometric=(kind, variant, axes, keep))

    @staticmethod
    def Analytic(readout: SpectralReadout = SpectralReadout.PEAK) -> "TransformOp":
        return TransformOp(analytic=readout)

    @staticmethod
    def Hankel(dln: float, mu: float = 0.0, readout: SpectralReadout = SpectralReadout.PEAK, trip: Trip = Trip.FORWARD) -> "TransformOp":
        return TransformOp(hankel=(dln, mu, readout, trip))

    def identity_buffer(self, fs: float, operand_key: ContentKey) -> bytes:
        # enum rows serialize by value, numeric rows as canonical float64 bytes; length-prefixed parts keep the buffer unambiguous.
        row: tuple[object, ...]
        match self:
            case TransformOp(tag="fourier", fourier=(basis, axes, readout, pad, trip)):
                row = (basis.value, *axes, readout.value, pad.value, trip.value)
            case TransformOp(tag="trigonometric", trigonometric=(kind, variant, axes, keep)):
                row = (kind.value, variant, *axes, keep)
            case TransformOp(tag="analytic", analytic=readout):
                row = (readout.value,)
            case TransformOp(tag="hankel", hankel=(dln, mu, readout, trip)):
                row = (dln, mu, readout.value, trip.value)
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


# one (forward, inverse, freq-grid) row per FourierBasis; `@cache` defers the import-banned scipy load to first call. Every 1-D
# inverse accepts the original `n`, so the round trip reconstructs to the source length under any PadPolicy.
@cache
def _fourier_routes() -> Map[FourierBasis, FourierRoute]:
    import scipy.fft as fft

    return Map.of_seq([
        (FourierBasis.COMPLEX, (fft.fft, fft.ifft, fft.fftfreq)),
        (FourierBasis.REAL, (fft.rfft, fft.irfft, fft.rfftfreq)),
        (FourierBasis.HERMITIAN, (fft.hfft, fft.ihfft, fft.fftfreq)),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transform_kernel(samples: object, fs: float, op: TransformOp, workers: int) -> "RuntimeRail[TransformReceipt]":
    # module-level so REFERENCE shipping resolves it by import — a closure pays an eager cloudpickle round-trip
    # no thread arm needs; `workers` arrives as the lane capacity the pocketfft team binds to.
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"transform.{op.tag}", op.identity_buffer(fs, payload.content_key)).bind(
            lambda result_key: boundary(f"transform.{op.tag}", lambda: _apply(samples, fs, op, result_key, workers))
        )
    )


async def apply(samples: object, fs: float, op: TransformOp, lane: LanePolicy) -> "RuntimeRail[TransformReceipt]":
    # weave owns span, fence, and the fenced contributor harvest.
    async def dispatch() -> RuntimeRail[TransformReceipt]:
        # One flatten from `RuntimeRail[RuntimeRail[TransformReceipt]]` to `RuntimeRail[TransformReceipt]`.
        return (await lane.offload(Kernel.of(_transform_kernel, KernelTrait.RELEASING), samples, fs, op, lane.capacity)).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.TRANSFORM, f"transform.{op.tag}", dispatch, facts={"op": op.tag, "fs": fs})


def _apply(samples: object, fs: float, op: TransformOp, key: ContentKey, workers: int) -> TransformReceipt:
    import scipy.fft as fft
    import scipy.signal as sig

    # `array_namespace` resolves the backend once and the Array-API-aware `scipy.fft` dispatches on the same `xp`. Admission already
    # rejected non-finite operands, so `xpx.nan_to_num` inside the bodies guards only the all-zero-band log/ratio degeneracy — the
    # Array-API sanitization owner, not a numpy-only `errstate` scope a jax/dask spine rejects.
    xp = array_namespace(samples)
    x = xp.asarray(samples)
    spacing = 1.0 / fs
    match op:
        case TransformOp(tag="fourier", fourier=(basis, axes, readout, pad, trip)):
            return TransformReceipt.of("fourier", x.size, key, _fourier(xp, fft, x, spacing, basis, axes, readout, pad, trip, workers))
        case TransformOp(tag="trigonometric", trigonometric=(kind, variant, axes, keep)):
            return TransformReceipt.of(kind.value, x.size, key, _trigonometric(xp, fft, x, kind, variant, axes, keep))
        case TransformOp(tag="analytic", analytic=readout):
            return TransformReceipt.of("analytic", x.size, key, _analytic(sig, x, fs, readout))
        case TransformOp(tag="hankel", hankel=(dln, mu, readout, trip)):
            return TransformReceipt.of("hankel", x.size, key, _hankel(xp, fft, x, dln, mu, readout, trip))
        case _ as unreachable:
            assert_never(unreachable)


def _fourier(
    xp: "ModuleType",
    fft: "ModuleType",
    x: "Array",
    spacing: float,
    basis: FourierBasis,
    axes: tuple[int, ...],
    readout: SpectralReadout,
    pad: PadPolicy,
    trip: Trip,
    workers: int,
) -> TransformEvidence:
    forward, inverse, grid = _fourier_routes()[basis]
    lead = axes[0] if axes else (x.ndim - 1)
    fast = (lambda a, real: fft.next_fast_len(x.shape[a], real=real)) if pad is PadPolicy.FAST else (lambda a, real: x.shape[a])
    n = fast(lead, basis is FourierBasis.REAL)
    # n-D path is the complex `fftn`/`ifftn` mirror (scipy catalogs no `rfftn`/`hfftn`): a non-empty `axes` runs the complex
    # transform on the `fftfreq` grid regardless of `basis`, padding each transformed axis to its OWN fast length. `set_workers`
    # binds the pocketfft team to the lane capacity — `-1` spawns an unbounded page-local team under an already-offloaded kernel.
    with fft.set_workers(workers):
        spectrum = fft.fftn(x, s=tuple(fast(a, False) for a in axes), axes=axes) if axes else forward(x, n=n, axis=lead)
    freqs = xp.asarray(fft.fftfreq(spectrum.shape[lead], spacing) if axes else grid(n, spacing))
    amplitude = xpx.nan_to_num(xp.abs(spectrum), xp=xp)
    energy = float(xp.sum(amplitude * amplitude))
    # max-project the off-lead axes into the amplitude spine `readout.fold` consumes — a peak track per bin, never a summed blur.
    spine = amplitude if amplitude.ndim == 1 else xp.max(amplitude, axis=tuple(i for i in range(amplitude.ndim) if i != lead))
    band = readout.fold(freqs[: spine.shape[0]], spine)
    if trip is Trip.FORWARD:
        return TransformEvidence.Spectrum(readout, band, energy)
    # inverse pins `s`/`n` to the SOURCE lengths so the residual against `x` is shape-correct under PadPolicy.FAST.
    rebuilt = fft.ifftn(spectrum, s=tuple(x.shape[a] for a in axes), axes=axes) if axes else inverse(spectrum, n=x.shape[lead], axis=lead)
    # complex norm holds for all three bases — `ihfft` reconstructs a complex half-spectrum, and forcing `xp.real` truncates
    # its imaginary part and inflates the residual.
    residual = float(xp.linalg.norm(xp.reshape(rebuilt - x, (-1,))) / (xp.linalg.norm(xp.reshape(x, (-1,))) + 1e-30))
    return TransformEvidence.Roundtrip(band, energy, residual)


def _hankel(xp: "ModuleType", fft: "ModuleType", x: "Array", dln: float, mu: float, readout: SpectralReadout, trip: Trip) -> TransformEvidence:
    # conjugate log-radial abscissa `exp(dln * (i - n/2))` gives the readout a real radial frequency, never a bare bin index.
    coeffs = xp.asarray(fft.fht(x, dln, mu))
    grid = xp.exp(dln * (xp.arange(coeffs.shape[-1], dtype=xpx.default_dtype(xp, "real floating")) - 0.5 * coeffs.shape[-1]))
    amplitude = xpx.nan_to_num(xp.abs(coeffs), xp=xp)
    energy = float(xp.sum(amplitude * amplitude))
    spine = amplitude if amplitude.ndim == 1 else xp.max(amplitude, axis=tuple(range(amplitude.ndim - 1)))
    band = readout.fold(grid[: spine.shape[0]], spine)
    if trip is Trip.FORWARD:
        return TransformEvidence.Spectrum(readout, band, energy)
    rebuilt = xp.asarray(fft.ifht(coeffs, dln, mu))
    residual = float(xp.linalg.norm(xp.reshape(rebuilt - x, (-1,))) / (xp.linalg.norm(xp.reshape(x, (-1,))) + 1e-30))
    return TransformEvidence.Roundtrip(band, energy, residual)


def _trigonometric(
    xp: "ModuleType", fft: "ModuleType", x: "Array", kind: TrigKind, variant: int, axes: tuple[int, ...], keep: float
) -> TransformEvidence:
    transform = (fft.dstn if axes else fft.dst) if kind is TrigKind.SINE else (fft.dctn if axes else fft.dct)
    coeffs = xp.asarray(transform(x, type=variant, axes=axes, norm="ortho") if axes else transform(x, type=variant, norm="ortho"))
    energy = xpx.nan_to_num(xp.abs(coeffs) ** 2, xp=xp)
    total = float(xp.sum(energy)) + 1e-30
    # `flip(sort(...))` gives the descending order without an `[::-1]` negative stride a jax/dask spine rejects.
    leading = max(1, int(round(keep * energy.size)))
    descending = xp.flip(xp.sort(xp.reshape(energy, (-1,))))
    concentration = float(xp.cumsum(descending)[leading - 1] / total)
    return TransformEvidence.Compaction(leading, concentration, total)


def _analytic(sig: "ModuleType", x: "Array", fs: float, readout: SpectralReadout) -> TransformEvidence:
    # numpy-resident: `hilbert` is jax-skipped (item-assignment), its Array-API support behind the experimental `SCIPY_ARRAY_API=1`
    # gate. The instantaneous frequency rides the unwrap-free product estimator `angle(conj(z[i]) * z[i+1])`, so a raw `angle`
    # track's branch cut never needs an `np.unwrap`/`np.diff` pass.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
