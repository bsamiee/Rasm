# [PY_COMPUTE_TRANSFORM]

One frequency-domain transform owner rules: `TransformOp` discriminates the pocketfft Fourier family, the trigonometric cosine/sine transforms, the FFTLog fast Hankel transform, and the FFT-backed analytic signal, folded through the single `apply` entry, so a spectrum, an energy-compacted basis, a log-radial coefficient set, and an instantaneous envelope are transform evidence on one owner rather than a per-transform method family. Pocketfft's eight entrypoints collapse to one forward body and one inverse body indexing a `FOURIER_ROUTES` row per `FourierBasis`, and one `SpectralReadout` axis folds every dominant-band read, so output is parameterized as tightly as input. These are in-memory transforms; columnar and gridded statistical aggregation defers to the `data` branch gridded/field owners.

Operands admit through `numerics/array#PAYLOAD` for the finite gate and the operand `ContentKey`; the receipt keys the RESULT through the op-owned `identity_parts` fold handed to `IdentitySource(parts=...)`, so the count-and-length framing runs at the identity owner and two different ops over one operand never share a key; the resolved receipt is the `ReceiptContributor` the weave harvest and the study spine consume. `scipy.fft` is Array-API-aware, so the Fourier/Trigonometric/Hankel arms ride the resolved `xp` while the analytic arm stays numpy-resident — `scipy.signal.hilbert` is jax-skipped behind the `SCIPY_ARRAY_API` gate. Every body enters the runtime thread band under the `RELEASING` trait through `lane.whole` and `lane.offload`; the resulting `LaneGrant.width` binds the pocketfft worker team, never an allocator-total read or the unbounded `-1` team that multiplies inner and outer parallelism.

## [01]-[INDEX]

- [02]-[TRANSFORM]: the `TransformOp` Fourier/trigonometric/Hankel/analytic rows folded through one `apply` entry, evidence discriminated over `TransformEvidence`, the dominant-band read folding the `SpectralReadout` axis.

## [02]-[TRANSFORM]

- Owner: `TransformOp` — the one operation union `apply` folds; `FourierBasis` keys the `FOURIER_ROUTES` `(forward, inverse, freqs)` table so the entrypoint family is a row lookup, never an inline ternary ladder; `Trip` is the bounded pass axis on the Fourier and Hankel cases (never an `invert` boolean); the n-D magnitude marginalizes to the lead-axis spine through one `xp.max` off-axis projection because `readout.fold` is order-invariant, so the grid and spine never co-order through `fftshift`.
- Faults: one `TRANSFORM_APPLY` fence row spans every op — the tag is a span fact, never four subject spellings — and its `catch` names the probed raise set: `ValueError` for a refused type, axes tuple, or worker team, `IndexError` for an out-of-range axis and a 0-d `hilbert` operand, `TypeError` for a non-numeric length or unresolvable namespace, `np.linalg.LinAlgError` leading as the narrower subclass.
- Output: `TransformEvidence` parameterizes the result per case — `spectrum`, `compaction`, `envelope`, `roundtrip` — and the `Trip.ROUNDTRIP` pass folds its residual into the shared `roundtrip` case rather than minting a per-transform outcome shape; every `facts()` slot stays a native scalar so the receipt layer aggregates and compares.
- Growth: a new transform is one `TransformOp` case with its `identity_parts` arm — the `Hankel` row is exactly this, one case folding into the existing `spectrum`/`roundtrip` evidence with zero new outcome shape; a new spectral basis is one `FourierBasis` row with its `FOURIER_ROUTES` triple; an n-D spectrum is a non-empty `axes` value on the existing row; a new trigonometric variant is one `TrigKind` row or `variant` value; a new band readout is one `SpectralReadout` row; a new outcome is one `TransformEvidence` case with its `facts()` arm.

```python signature
from collections.abc import Callable, Iterable
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Final, Literal, assert_never

import array_api_extra as xpx
import numpy as np
from array_api_compat import array_namespace
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import scipy.fft as fft
lazy import scipy.signal as sig

# --- [TYPES] ----------------------------------------------------------------------------

if TYPE_CHECKING:
    from types import ModuleType

    from rasm.compute.numerics.array import Array

type FourierRoute = tuple[
    Callable[..., "Array"],
    Callable[..., "Array"],
    Callable[[int, float], "Array"],
]


class FourierBasis(StrEnum):
    COMPLEX = "complex"
    REAL = "real"
    HERMITIAN = "hermitian"


class TrigKind(StrEnum):
    COSINE = "cosine"
    SINE = "sine"


class PadPolicy(StrEnum):
    EXACT = "exact"
    FAST = "fast"


class Trip(StrEnum):
    FORWARD = "forward"
    ROUNDTRIP = "roundtrip"


class SpectralReadout(StrEnum):
    PEAK = "peak"
    CENTROID = "centroid"
    BANDWIDTH = "bandwidth"
    FLATNESS = "flatness"

    def fold(self, freqs: "Array", amplitude: "Array") -> float:
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
    spectrum: tuple[SpectralReadout, float, float] = case()
    compaction: tuple[int, float, float] = case()
    envelope: tuple[float, Option[float], float] = case()
    roundtrip: tuple[float, float, float] = case()

    @staticmethod
    def Spectrum(readout: SpectralReadout, band_hz: float, energy: float) -> "TransformEvidence":
        return TransformEvidence(spectrum=(readout, band_hz, energy))

    @staticmethod
    def Compaction(leading: int, concentration: float, energy: float) -> "TransformEvidence":
        return TransformEvidence(compaction=(leading, concentration, energy))

    @staticmethod
    def Envelope(mean: float, inst_hz: Option[float], band_hz: float) -> "TransformEvidence":
        return TransformEvidence(envelope=(mean, inst_hz, band_hz))

    @staticmethod
    def Roundtrip(band_hz: float, energy: float, residual: float) -> "TransformEvidence":
        return TransformEvidence(roundtrip=(band_hz, energy, residual))

    def facts(self) -> dict[str, object]:
        match self:
            case TransformEvidence(tag="spectrum", spectrum=(readout, band, energy)):
                return {"readout": readout.value, "band_hz": band, "spectral_energy": energy}
            case TransformEvidence(tag="compaction", compaction=(leading, concentration, energy)):
                return {"leading": leading, "energy_concentration": concentration, "spectral_energy": energy}
            case TransformEvidence(tag="envelope", envelope=(mean, inst, band)):
                return {"mean_envelope": mean, "band_hz": band, **inst.map(lambda hz: {"instantaneous_hz": hz}).default_value({})}
            case TransformEvidence(tag="roundtrip", roundtrip=(band, energy, residual)):
                return {"band_hz": band, "spectral_energy": energy, "reconstruction_residual": residual}
            case _ as unreachable:
                assert_never(unreachable)


class TransformReceipt(Struct, frozen=True):
    op: str
    length: int
    lineage: Provenance
    evidence: TransformEvidence

    @staticmethod
    def of(op: str, length: int, lineage: Provenance, evidence: TransformEvidence) -> "TransformReceipt":
        return TransformReceipt(op, length, lineage, evidence)

    @property
    def content_key(self) -> ContentKey:
        return self.lineage.produced

    def contribute(self) -> Iterable[Receipt]:
        facts = {"op": self.op, "length": self.length, **self.evidence.facts()}
        yield Receipt.of(
            EvidenceScope.TRANSFORM.value,
            ("emitted", self.op, facts),
            key=Some(self.lineage.produced),
            provenance=Some(self.lineage),
        )


@tagged_union(frozen=True)
class TransformOp:
    tag: Literal["fourier", "trigonometric", "analytic", "hankel"] = tag()
    fourier: tuple[FourierBasis, tuple[int, ...], SpectralReadout, PadPolicy, Trip] = case()
    trigonometric: tuple[TrigKind, int, tuple[int, ...], float] = case()
    analytic: SpectralReadout = case()
    hankel: tuple[float, float, SpectralReadout, Trip] = case()

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
        return TransformOp(trigonometric=(kind, variant, axes, keep))

    @staticmethod
    def Analytic(readout: SpectralReadout = SpectralReadout.PEAK) -> "TransformOp":
        return TransformOp(analytic=readout)

    @staticmethod
    def Hankel(dln: float, mu: float = 0.0, readout: SpectralReadout = SpectralReadout.PEAK, trip: Trip = Trip.FORWARD) -> "TransformOp":
        return TransformOp(hankel=(dln, mu, readout, trip))

    def identity_parts(self, fs: float, operand_key: ContentKey) -> tuple[bytes, ...]:
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
        return (
            self.tag.encode(),
            operand_key.project("hex").encode(),
            np.float64(fs).tobytes(),
            *(cell.encode() if isinstance(cell, str) else np.float64(cell).tobytes() for cell in row),
        )


# --- [TABLES] ---------------------------------------------------------------------------

TRANSFORM_APPLY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.TRANSFORM, point="apply", arm="boundary", defect="kernel-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([TRANSFORM_APPLY]))


@cache
def _fourier_routes() -> Map[FourierBasis, FourierRoute]:
    return Map.of_seq([
        (FourierBasis.COMPLEX, (fft.fft, fft.ifft, fft.fftfreq)),
        (FourierBasis.REAL, (fft.rfft, fft.irfft, fft.rfftfreq)),
        (FourierBasis.HERMITIAN, (fft.hfft, fft.ihfft, fft.fftfreq)),
    ])


# --- [OPERATIONS] -----------------------------------------------------------------------


def _transform_kernel(samples: object, fs: float, op: TransformOp, workers: int) -> "RuntimeRail[TransformReceipt]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda payload: ContentIdentity.of(f"transform.{op.tag}", IdentitySource(parts=op.identity_parts(fs, payload.content_key))).bind(
            lambda result_key: boundary(
                TRANSFORM_APPLY,
                lambda: _apply(samples, fs, op, Provenance(consumed=Block.singleton(payload.content_key), produced=result_key), workers),
                catch=(np.linalg.LinAlgError, ValueError, IndexError, TypeError),
            )
        )
    )


async def apply(samples: object, fs: float, op: TransformOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> "RuntimeRail[TransformReceipt]":
    async def dispatch() -> RuntimeRail[TransformReceipt]:
        return (
            await lane.whole(
                lambda grant: lane.offload(
                    Kernel.of(_transform_kernel, KernelTrait.RELEASING), samples, fs, op, grant.width
                )
            )
        ).bind(lambda rail: rail)

    return await evidence_run(EvidenceScope.TRANSFORM, f"transform.{op.tag}", dispatch, facts={"op": op.tag, "fs": fs}, composition=composition)


def _apply(samples: object, fs: float, op: TransformOp, lineage: Provenance, workers: int) -> TransformReceipt:
    xp = array_namespace(samples)
    x = xp.asarray(samples)
    spacing = 1.0 / fs
    match op:
        case TransformOp(tag="fourier", fourier=(basis, axes, readout, pad, trip)):
            return TransformReceipt.of("fourier", x.size, lineage, _fourier(xp, fft, x, spacing, basis, axes, readout, pad, trip, workers))
        case TransformOp(tag="trigonometric", trigonometric=(kind, variant, axes, keep)):
            return TransformReceipt.of(kind.value, x.size, lineage, _trigonometric(xp, fft, x, kind, variant, axes, keep))
        case TransformOp(tag="analytic", analytic=readout):
            return TransformReceipt.of("analytic", x.size, lineage, _analytic(sig, x, fs, readout))
        case TransformOp(tag="hankel", hankel=(dln, mu, readout, trip)):
            return TransformReceipt.of("hankel", x.size, lineage, _hankel(xp, fft, x, dln, mu, readout, trip))
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
    with fft.set_workers(workers):
        spectrum = fft.fftn(x, s=tuple(fast(a, False) for a in axes), axes=axes) if axes else forward(x, n=n, axis=lead)
    freqs = xp.asarray(fft.fftfreq(spectrum.shape[lead], spacing) if axes else grid(n, spacing))
    amplitude = xpx.nan_to_num(xp.abs(spectrum), xp=xp)
    energy = float(xp.sum(amplitude * amplitude))
    spine = amplitude if amplitude.ndim == 1 else xp.max(amplitude, axis=tuple(i for i in range(amplitude.ndim) if i != lead))
    band = readout.fold(freqs[: spine.shape[0]], spine)
    if trip is Trip.FORWARD:
        return TransformEvidence.Spectrum(readout, band, energy)
    rebuilt = fft.ifftn(spectrum, s=tuple(x.shape[a] for a in axes), axes=axes) if axes else inverse(spectrum, n=x.shape[lead], axis=lead)
    residual = float(xp.linalg.norm(xp.reshape(rebuilt - x, (-1,))) / (xp.linalg.norm(xp.reshape(x, (-1,))) + 1e-30))
    return TransformEvidence.Roundtrip(band, energy, residual)


def _hankel(xp: "ModuleType", fft: "ModuleType", x: "Array", dln: float, mu: float, readout: SpectralReadout, trip: Trip) -> TransformEvidence:
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
    leading = max(1, int(round(keep * energy.size)))
    descending = xp.flip(xp.sort(xp.reshape(energy, (-1,))))
    concentration = float(xp.cumsum(descending)[leading - 1] / total)
    return TransformEvidence.Compaction(leading, concentration, total)


def _analytic(sig: "ModuleType", x: "Array", fs: float, readout: SpectralReadout) -> TransformEvidence:
    xn = np.asarray(x)
    analytic = sig.hilbert(xn)
    envelope = np.abs(analytic)
    inst = np.angle(np.conj(analytic[:-1]) * analytic[1:]) * fs / (2.0 * np.pi) if analytic.size > 1 else np.empty(0)
    weight = envelope[1:] / (np.sum(envelope[1:]) + 1e-30)
    central = Some(float(np.sum(inst * weight))) if inst.size else Nothing
    band = readout.fold(inst, np.abs(inst))
    return TransformEvidence.Envelope(float(np.mean(envelope)), central, band)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
