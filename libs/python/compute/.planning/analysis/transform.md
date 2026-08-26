# [PY_COMPUTE_TRANSFORM]

`TransformOp` folds the pocketfft Fourier family, trigonometric cosine/sine transforms, FFTLog fast Hankel transform, and FFT-backed analytic signal through one `apply` entry. Each arm returns its native transform values: Fourier and Hankel carry their coordinate grid with coefficients and preserve the rebuilt array on a roundtrip, trigonometric rows return coefficients, and the analytic row returns the complex signal. Pocketfft's entrypoints collapse to one forward body and one inverse body indexed by the `FOURIER_ROUTES` row for each `FourierBasis`.

Operands admit through `numerics/array#PAYLOAD` for the finite gate. `scipy.fft` keeps the Fourier, trigonometric, and Hankel arms on the resolved `xp`, while `scipy.signal.hilbert` keeps the analytic arm numpy-resident behind the `SCIPY_ARRAY_API` gate. Every body enters the runtime thread band under the `RELEASING` trait through `lane.whole` and `lane.offload`; the resulting `LaneGrant.width` binds the pocketfft worker team.

## [01]-[INDEX]

- [02]-[TRANSFORM]: `TransformOp` folds Fourier, trigonometric, Hankel, and analytic rows through one `apply` entry returning native transform values.

## [02]-[TRANSFORM]

- Owner: `TransformOp` folds one operation union through `apply`; `FourierBasis` keys the `FOURIER_ROUTES` `(forward, inverse, freqs)` table, and `Trip` selects the native forward product or the forward product paired with its rebuild.
- Faults: one `TRANSFORM_APPLY` fence row spans every op — the tag is a span fact, never four subject spellings — and its `catch` names the probed raise set: `ValueError` for a refused type, axes tuple, or worker team, `IndexError` for an out-of-range axis and a 0-d `hilbert` operand, `TypeError` for a non-numeric length or unresolvable namespace, `np.linalg.LinAlgError` leading as the narrower subclass.
- Output: Fourier and Hankel return `(grid, coefficients)` and append the rebuilt array under `Trip.ROUNDTRIP`; trigonometric rows return coefficients; analytic returns the complex analytic signal.
- Growth: a new transform is one `TransformOp` case and one `apply` arm; a new spectral basis is one `FourierBasis` row with its `FOURIER_ROUTES` triple; an n-D spectrum is a non-empty `axes` value on the existing row; a new trigonometric variant is one `TrigKind` row or `variant` value.

```python
from collections.abc import Callable
from enum import StrEnum
from functools import cache
from typing import TYPE_CHECKING, Final, Literal, assert_never

import array_api_extra as xpx
import numpy as np
from array_api_compat import array_namespace
from expression import case, tag, tagged_union
from expression.collections import Block, Map

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.numerics.array import ArrayPayload, ArraySource, FiniteGate
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, boundary, rostered
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
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


@tagged_union(frozen=True)
class TransformOp:
    tag: Literal["fourier", "trigonometric", "analytic", "hankel"] = tag()
    fourier: tuple[FourierBasis, tuple[int, ...], PadPolicy, Trip] = case()
    trigonometric: tuple[TrigKind, int, tuple[int, ...]] = case()
    analytic: None = case()
    hankel: tuple[float, float, Trip] = case()

    @staticmethod
    def Fourier(
        basis: FourierBasis = FourierBasis.COMPLEX,
        axes: tuple[int, ...] = (),
        pad: PadPolicy = PadPolicy.EXACT,
        trip: Trip = Trip.FORWARD,
    ) -> "TransformOp":
        return TransformOp(fourier=(basis, axes, pad, trip))

    @staticmethod
    def Trigonometric(kind: TrigKind = TrigKind.COSINE, variant: int = 2, axes: tuple[int, ...] = ()) -> "TransformOp":
        return TransformOp(trigonometric=(kind, variant, axes))

    @staticmethod
    def Analytic() -> "TransformOp":
        return TransformOp(analytic=None)

    @staticmethod
    def Hankel(dln: float, mu: float = 0.0, trip: Trip = Trip.FORWARD) -> "TransformOp":
        return TransformOp(hankel=(dln, mu, trip))


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


def _transform_kernel(
    samples: object, fs: float, op: TransformOp, workers: int
) -> "RuntimeResult[Array | tuple[Array, Array] | tuple[Array, Array, Array]]":
    return ArrayPayload.admit(ArraySource.Live(samples), (), FiniteGate.REJECT).bind(
        lambda _: boundary(
            TRANSFORM_APPLY,
            lambda: _apply(samples, fs, op, workers),
            catch=(np.linalg.LinAlgError, ValueError, IndexError, TypeError),
        )
    )


async def apply(
    samples: object, fs: float, op: TransformOp, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
) -> "RuntimeResult[Array | tuple[Array, Array] | tuple[Array, Array, Array]]":
    async def dispatch() -> "RuntimeResult[Array | tuple[Array, Array] | tuple[Array, Array, Array]]":
        return (
            await lane.whole(
                lambda grant: lane.offload(
                    Kernel.of(_transform_kernel, KernelTrait.RELEASING), samples, fs, op, grant.width
                )
            )
        ).bind(lambda held: held)

    return await evidence_run(EvidenceScope.TRANSFORM, f"transform.{op.tag}", dispatch, facts={"op": op.tag, "fs": fs}, composition=composition)


def _apply(samples: object, fs: float, op: TransformOp, workers: int) -> "Array | tuple[Array, Array] | tuple[Array, Array, Array]":
    xp = array_namespace(samples)
    x = xp.asarray(samples)
    spacing = 1.0 / fs
    match op:
        case TransformOp(tag="fourier", fourier=(basis, axes, pad, trip)):
            return _fourier(xp, fft, x, spacing, basis, axes, pad, trip, workers)
        case TransformOp(tag="trigonometric", trigonometric=(kind, variant, axes)):
            transform = (fft.dstn if axes else fft.dst) if kind is TrigKind.SINE else (fft.dctn if axes else fft.dct)
            return transform(x, type=variant, axes=axes, norm="ortho") if axes else transform(x, type=variant, norm="ortho")
        case TransformOp(tag="analytic"):
            return np.asarray(sig.hilbert(np.asarray(x)))
        case TransformOp(tag="hankel", hankel=(dln, mu, trip)):
            return _hankel(xp, fft, x, dln, mu, trip)
        case _ as unreachable:
            assert_never(unreachable)


def _fourier(
    xp: "ModuleType",
    fft: "ModuleType",
    x: "Array",
    spacing: float,
    basis: FourierBasis,
    axes: tuple[int, ...],
    pad: PadPolicy,
    trip: Trip,
    workers: int,
) -> "tuple[Array, Array] | tuple[Array, Array, Array]":
    forward, inverse, grid = _fourier_routes()[basis]
    lead = axes[0] if axes else (x.ndim - 1)
    fast = (lambda a, real: fft.next_fast_len(x.shape[a], real=real)) if pad is PadPolicy.FAST else (lambda a, real: x.shape[a])
    n = fast(lead, basis is FourierBasis.REAL)
    with fft.set_workers(workers):
        spectrum = fft.fftn(x, s=tuple(fast(a, False) for a in axes), axes=axes) if axes else forward(x, n=n, axis=lead)
    freqs = xp.asarray(fft.fftfreq(spectrum.shape[lead], spacing) if axes else grid(n, spacing))
    if trip is Trip.FORWARD:
        return freqs, spectrum
    rebuilt = fft.ifftn(spectrum, s=tuple(x.shape[a] for a in axes), axes=axes) if axes else inverse(spectrum, n=x.shape[lead], axis=lead)
    return freqs, spectrum, xp.asarray(rebuilt)


def _hankel(
    xp: "ModuleType", fft: "ModuleType", x: "Array", dln: float, mu: float, trip: Trip
) -> "tuple[Array, Array] | tuple[Array, Array, Array]":
    coeffs = xp.asarray(fft.fht(x, dln, mu))
    grid = xp.exp(dln * (xp.arange(coeffs.shape[-1], dtype=xpx.default_dtype(xp, "real floating")) - 0.5 * coeffs.shape[-1]))
    if trip is Trip.FORWARD:
        return grid, coeffs
    rebuilt = xp.asarray(fft.ifht(coeffs, dln, mu))
    return grid, coeffs, rebuilt
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
