# [PY_COMPUTE_FIELD]

One finite-element-and-grid field readout owner beside the FEM assemble and solve routes. `FieldQuery` discriminates the three postprocessing operations a discretized or sampled solution admits — `interpolate` lifts a DOF vector into a `skfem.DiscreteField` and reads `value`/`grad`/`hess` under one `ReadoutKind` axis, `project` L2-projects a callable or cross-basis DOF vector onto a target `ElementKind` basis through `basis.project`, `resample` evaluates a regular grid at query points through the JAX-differentiable `interpax.Interpolator{1,2,3}D`. This owner consumes the solution and never produces it: it returns the evaluated array with its measured norm, peak, and residual and never solves, assembles, or aggregates a field across a grid.

`solvers/mesh#MESH_FIELD` owns the whole element vocabulary — `ElementKind`, the `CTOR` element table with its recursive wrapper build, and `MeshField` — so the interpolate and project cases build the same basis the assemble built, scalar, vector, and composite kinds alike, rather than a second constructor map; `solvers/solve#SOLVE` owns the `SolveStatus`/`status_of` residual-floor verdict each case terminates in; the `resample` case realizes the multidimensional consumer `solvers/quadrature#QUADRATURE` defers here. A resolved `Readout` carries the computed value and stamps its scalar `attributes` on the weave span, and `_dispatch`'s `@returns_result` chain `yield from`-binds the `_key` RESULT-identity result so the mesh-minted `field.content_key` enters as one labeled part of the `IdentitySource(parts=...)` preimage the identity owner frames, and distinct operations over one operand carry distinct keys. `_TRAIT` routes each operation by its own hazard — `interpolate` rides the `RELEASING` thread band (pure scikit-fem/NumPy readout), `project` and `resample` the `HOSTILE` process band (caller-supplied `FieldFn` callbacks run GIL-held; the `interpax` resample is JAX-gated and the x64 flag is process-global) — the numpy nodal readout and `np.interp` 1-D resample staying the in-worker fallbacks an absent package's reification `ImportError` alone selects under a `Provider` discriminant, every readout and arity beyond their reach refusing typed rather than degrading silently, isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing; field evidence stays on the span, and no field `HandoffAxis` case exists.

## [01]-[INDEX]

- [02]-[FIELD]: `FieldQuery` owns the interpolate/project/resample postprocessing operations, verdicts folded through the `SolveStatus` floor into one `_SLOTS`-driven `Readout`.

## [02]-[FIELD]

- Owner: `FieldQuery` — the ONE `@tagged_union` field-postprocessing owner; one owner spans the nodal-quadrature readout, the basis-to-basis transfer, and the grid resample.
- Cases: `interpolate` reads its source element off the topology's own `MeshField.element`, never a redundant parallel parameter; `basis.split` is total over every element kind — one `(sub_dofs, sub_basis)` pair per component, a scalar element yielding one pair — so the result retains each component array and derives the component count, aggregate norm, and worst-component peak from those same arrays. `basis.split` leads each pair with the sub-vector, so the destructuring order is load-bearing: reading it as `(basis, dofs)` hands a DOF array the `interpolate` call and raises on every solve. `project` carries the genuinely-distinct target `ElementKind` and a `ProjectSource` (a `FieldFn` over physical points or a `(ElementKind, np.ndarray)` cross-basis DOF pair); the transfer rides the `basis.project` METHOD, never a phantom top-level `skfem.project(basis_from=, basis_to=)`. Its cross-basis residual is a source-space round trip — a P1→P2→P1 reports the lost information in one comparison space, never `DiscreteField` value arrays subtracted at incompatible quadrature points — and its callable residual is the physical-point fidelity at `global_coordinates()` the `compute/.api/scikit-fem.md` `[LOCAL_ADMISSION]` mandates, never a finiteness sentinel. `resample` carries the `GridAxes`, gridded values, query points, and the bounded `ResampleMethod`, and has no `MeshField`, so its key is the `_key` layer `_dispatch` already bound.
- Law: the numpy fallback SERVES what it can evaluate and REFUSES the rest at selection — the nodal `VALUE` array off a Lagrange DOF vector and the 1-D `np.interp` sample are real values, a gradient, a Hessian, and any multidimensional resample are not, and a refusal faults typed off the same `_dispatch` chain the key derive rides. `Provider` names which engine answered on every served `Readout`, and the resample's `method` slot carries the REALIZED kernel, so a linear floor result never reads as the `cubic` interpolant the caller asked for. Framing an untouched grid or raw DOFs as a non-nodal output is the deleted form: it publishes a value no kernel computed and hands the status floor a measurement of the wrong subject.
- Output: `ReadoutKind` (`VALUE`/`GRAD`/`HESS`) is the ONE bounded readout policy keyed through `_READOUT` onto the `DiscreteField` attribute, so a value, a flux-recovery gradient, and a Hessian are one policy row on the interpolate case rather than three parallel entries — the case is parameterized over its OUTPUT shape, not only its input. Its `nodal` predicate is the floor's servability answer, so which readouts survive an absent package is a property of the readout rather than a branch in the fallback.
- Output: `Readout` is the ONE `@tagged_union` field result whose `Literal` `tag` IS the operation. Each case leads with its typed computed value, while `_SLOTS` names the remaining fact sequence (`key` leading, `status` trailing) and drives the strict fact projection; `.value`, `.content_key`, `.element`, and `.status` narrow by the closed cases. Factories derive norm, peak, and residual from the retained value, grade evaluation on finiteness and projection on its measured residual through the shared `status_of` floor, and close through `_noted`, which stamps scalar attributes alone on the weave span. Projection transfer fidelity rides the project case as a first-class `SolveStatus` verdict, not a `Solve` convergence verdict — a projection is not a solve.
- Packages: `skfem` (`Basis`, the `Mesh*`/`Element*` families, `basis.interpolate`/`split`/`project`/`global_coordinates`, `basis.N`, `DiscreteField.value`/`.grad`/`.hess`), `interpax` (`Interpolator{1,2,3}D`, the reusable grid interpolants `FieldEngine.worker()` builds once on the x64-floated path), `jax` (`config.update("jax_enable_x64", True)` floats the worker to float64 so the interpolant's `grad`/`vjp` holds at double precision rather than the float32 default), `numpy`, `expression`, and the `solvers/mesh#MESH_FIELD`, `solvers/solve#SOLVE`, and runtime boundaries above. This cross-module private `CTOR` import is the reuse `solvers/mesh#MESH_FIELD` runs, never the parallel `_ELEMENTCTOR`/`_MESHCTOR` pair.
- Growth: a new element is one shared `CTOR` row, a vector or composite kind reaching the multi-component readout with no edit here; a new readout is one `ReadoutKind` row, one `_READOUT` entry, and its `nodal` answer on the existing interpolate case; a new field operation is one `FieldQuery` case and one `_SLOTS` row sharing the basis-construction fold and the status floor; a new resample arity beyond 3-D is one `_INTERPOLATOR` row; a new resample kernel is one `ResampleMethod` member; a new worker resample module is one `FieldEngine` field; a new readout statistic is one slot on the owning `_SLOTS` row; a new engine tier is one `solvers/solve#SOLVE` `Provider` member reaching every solve route and this owner at once; a new termination class is one `SolveStatus` member shared with every solver route; a new floor refusal is one more subject pair on the ONE `FLOOR_UNSERVED` row, never a sibling row.
- Boundary: field evaluation, projection, and grid resample only — the assemble stays on `solvers/mesh#MESH_FIELD`, the solve on `solvers/quadrature#QUADRATURE`, and columnar/gridded aggregation of the evaluated field in the `data` branch, so this owner returns the in-memory value and never aggregates across a grid. Rejected: a hand-rolled interpolation loop where `basis.interpolate`/`basis.project`/`interpax.Interpolator` own the concern; a worker resample left on the JAX float32 default; a per-call `import interpax`/`import jax` where the frozen `FieldEngine` folds the modules once; a floor result published under the gated engine's requested method or readout; a span-opening decorator on `_dispatch` swallowing the resample key-derive where `@returns_result` threads the `_key` layer and the weave owns the span. Mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections.abc import Callable
from dataclasses import dataclass
from enum import StrEnum
from itertools import chain
from typing import Any, Final, Literal, Self, assert_never

import numpy as np
from expression import Error, case, tag, tagged_union
from expression.collections import Block, Map

from opentelemetry import trace

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.solvers.mesh import CTOR, ElementKind, MeshField
from rasm.compute.solvers.solve import Provider, SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, returns_result, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import skfem


# --- [TYPES] ----------------------------------------------------------------------------

type FieldOp = Literal["interpolate", "project", "resample"]
type FieldFn = Callable[[np.ndarray], np.ndarray]
type ProjectSource = FieldFn | tuple[ElementKind, np.ndarray]
type GridAxes = tuple[np.ndarray, ...]
type ResampleMethod = Literal["nearest", "linear", "cubic", "cubic2", "catmull-rom", "monotonic", "monotonic-0"]


class ReadoutKind(StrEnum):
    VALUE = "value"
    GRAD = "grad"
    HESS = "hess"

    @property
    def nodal(self) -> bool:
        return self is ReadoutKind.VALUE


# --- [CONSTANTS] ------------------------------------------------------------------------

_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("interpolate", KernelTrait.RELEASING),
    ("project", KernelTrait.HOSTILE),
    ("resample", KernelTrait.HOSTILE),
])

_READOUT: Map[ReadoutKind, str] = Map.of_seq([(ReadoutKind.VALUE, "value"), (ReadoutKind.GRAD, "grad"), (ReadoutKind.HESS, "hess")])

_PROJECT_TOL: Final = 1e-6

_INTERPOLATOR: Map[int, str] = Map.of_seq([(1, "Interpolator1D"), (2, "Interpolator2D"), (3, "Interpolator3D")])

_SLOTS: Map[FieldOp, tuple[str, ...]] = Map.of_seq([
    ("interpolate", ("key", "element", "readout", "dof_count", "components", "provider", "norm", "peak", "status")),
    ("project", ("key", "element", "dof_count", "provider", "norm", "peak", "residual", "status")),
    ("resample", ("key", "dim", "query_count", "method", "provider", "norm", "peak", "status")),
])


# --- [TABLES] ---------------------------------------------------------------------------

FLOOR_UNSERVED: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.FIELD, point="floor", arm="config", defect="floor-unserved", retriability=TERMINAL, slots=("op", "request")
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([FLOOR_UNSERVED]))

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class Readout:
    tag: FieldOp = tag()
    interpolate: tuple[tuple[np.ndarray, ...], ContentKey, ElementKind, ReadoutKind, int, int, Provider, float, float, SolveStatus] = case()
    project: tuple[np.ndarray, ContentKey, ElementKind, int, Provider, float, float, float, SolveStatus] = case()
    resample: tuple[np.ndarray, ContentKey, int, int, ResampleMethod, Provider, float, float, SolveStatus] = case()

    @classmethod
    def Interpolate(
        cls,
        value: tuple[np.ndarray, ...],
        key: ContentKey,
        element: ElementKind,
        readout: ReadoutKind,
        dof_count: int,
        components: int,
        provider: Provider,
        norm: float,
        peak: float,
    ) -> Self:
        status = SolveStatus.SUCCESS if all(np.all(np.isfinite(component)) for component in value) else SolveStatus.NONFINITE
        return cls(interpolate=(value, key, element, readout, dof_count, components, provider, norm, peak, status))._noted()

    @classmethod
    def Project(
        cls, value: np.ndarray, key: ContentKey, element: ElementKind, dof_count: int, provider: Provider, norm: float, peak: float, residual: float
    ) -> Self:
        return cls(project=(value, key, element, dof_count, provider, norm, peak, residual, status_of(None, residual, _PROJECT_TOL)))._noted()

    @classmethod
    def Resample(
        cls, value: np.ndarray, key: ContentKey, dim: int, query_count: int, method: ResampleMethod, provider: Provider, norm: float, peak: float
    ) -> Self:
        status = SolveStatus.SUCCESS if np.all(np.isfinite(value)) else SolveStatus.NONFINITE
        return cls(resample=(value, key, dim, query_count, method, provider, norm, peak, status))._noted()

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case (
                Readout(tag="interpolate", interpolate=(_, *payload))
                | Readout(tag="project", project=(_, *payload))
                | Readout(tag="resample", resample=(_, *payload))
            ):
                return dict(zip(_SLOTS[self.tag], payload, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def value(self) -> np.ndarray | tuple[np.ndarray, ...]:
        match self:
            case (
                Readout(tag="interpolate", interpolate=(value, *_))
                | Readout(tag="project", project=(value, *_))
                | Readout(tag="resample", resample=(value, *_))
            ):
                return value
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def content_key(self) -> ContentKey:
        match self:
            case (
                Readout(tag="interpolate", interpolate=(_, ContentKey() as key, *_))
                | Readout(tag="project", project=(_, ContentKey() as key, *_))
                | Readout(tag="resample", resample=(_, ContentKey() as key, *_))
            ):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def element(self) -> ElementKind | None:
        match self:
            case (
                Readout(tag="interpolate", interpolate=(_, _, ElementKind() as element, *_))
                | Readout(tag="project", project=(_, _, ElementKind() as element, *_))
            ):
                return element
            case Readout(tag="resample"):
                return None
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def status(self) -> SolveStatus:
        match self:
            case (
                Readout(tag="interpolate", interpolate=(*_, SolveStatus() as status))
                | Readout(tag="project", project=(*_, SolveStatus() as status))
                | Readout(tag="resample", resample=(*_, SolveStatus() as status))
            ):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status.converged

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        scalars = {name: value for name, value in self.facts.items() if isinstance(value, str | bool | int | float)}
        return {"operation": self.tag, "key": self.content_key.hex, "converged": self.converged, **scalars}

    def _noted(self) -> Self:
        trace.get_current_span().set_attributes(self.attributes)
        return self


@tagged_union(frozen=True)
class FieldQuery:
    tag: FieldOp = tag()
    interpolate: tuple[MeshField, np.ndarray, ReadoutKind] = case()
    project: tuple[MeshField, ElementKind, ProjectSource] = case()
    resample: tuple[GridAxes, np.ndarray, np.ndarray, ResampleMethod] = case()

    @classmethod
    def Interpolate(cls, field: MeshField, dofs: np.ndarray, readout: ReadoutKind = ReadoutKind.VALUE) -> Self:
        return cls(interpolate=(field, dofs, readout))

    @classmethod
    def Project(cls, field: MeshField, target: ElementKind, source: ProjectSource) -> Self:
        return cls(project=(field, target, source))

    @classmethod
    def Resample(cls, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod = "cubic") -> Self:
        return cls(resample=(axes, values, query, method))

    async def evaluate(
        self, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE
    ) -> RuntimeResult[Readout]:
        async def dispatch() -> RuntimeResult[Readout]:
            return (await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self)).bind(lambda held: held)

        return await evidence_run(EvidenceScope.FIELD, f"field.{self.tag}", dispatch, facts={"op": self.tag}, composition=composition)


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class FieldEngine:
    interpax: Any

    @classmethod
    def worker(cls) -> Self:
        import jax

        jax.config.update("jax_enable_x64", True)

        import interpax

        return cls(interpax=interpax)

    def resample(self, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> np.ndarray:
        dim = len(axes)
        interpolant = getattr(self.interpax, _INTERPOLATOR[dim])(*axes, np.asarray(values), method=method)
        if dim == 1:
            return np.asarray(interpolant(np.asarray(query)))
        return np.asarray(interpolant(*np.asarray(query).reshape(-1, dim).T))


# --- [OPERATIONS] -----------------------------------------------------------------------


@returns_result
def _dispatch(query: FieldQuery) -> Readout:
    match query:
        case FieldQuery(tag="interpolate", interpolate=(field, dofs, readout)):
            dofs_arr = np.asarray(dofs)
            key: ContentKey = yield from _key("field-interpolate", field.content_key.project("hex").encode(), readout.value.encode(), dofs_arr)
            settled: Readout = yield from _interpolate_readout(key, field, dofs_arr, readout)
            return settled
        case FieldQuery(tag="project", project=(field, target, source)):
            key = yield from _key("field-project", field.content_key.project("hex").encode(), target.value.encode(), *_source_parts(source))
            return _project_readout(key, field, target, source)
        case FieldQuery(tag="resample", resample=(axes, values, query, method)):
            grid = np.asarray(values)
            key = yield from _key("field-resample", method.encode(), np.concatenate([np.ravel(a) for a in axes]), grid, np.asarray(query))
            sampled: Readout = yield from _resample_readout(key, axes, grid, np.asarray(query), method)
            return sampled
        case _ as unreachable:
            assert_never(unreachable)


def _source_parts(source: ProjectSource) -> tuple["bytes | np.ndarray", ...]:
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            return (origin.value.encode(), origin_dofs)
        case fn:
            return (getattr(fn, "__qualname__", repr(fn)).encode(),)



def _norm_peak(values: np.ndarray) -> tuple[float, float]:
    return (float(np.linalg.norm(values)), float(np.max(np.abs(values)))) if values.size else (0.0, 0.0)


def _basis(field: MeshField, element: ElementKind, skfem: Any) -> Any:
    row = CTOR[element]
    mesh = getattr(skfem, row.mesh)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    return skfem.Basis(mesh, row.built(skfem))


@returns_result
def _interpolate_readout(
    key: ContentKey, field: MeshField, dofs: np.ndarray, readout: ReadoutKind
) -> Readout:
    element = field.element
    try:
        fem = skfem
    except ImportError:
        if not readout.nodal:
            yield from Error(FLOOR_UNSERVED.raised("interpolate", readout.value))
        values = (np.asarray(dofs),)
        norm, peak = _norm_peak(values[0])
        return Readout.Interpolate(values, key, element, readout, int(dofs.size), 1, Provider.FLOOR, norm, peak)
    basis = _basis(field, element, fem)
    components = basis.split(dofs)
    values = tuple(np.asarray(getattr(sub.interpolate(part), _READOUT[readout])) for part, sub in components)
    measurements = tuple(_norm_peak(value) for value in values)
    norm = float(np.linalg.norm(np.asarray([component_norm for component_norm, _ in measurements])))
    peak = max((component_peak for _, component_peak in measurements), default=0.0)
    return Readout.Interpolate(values, key, element, readout, int(basis.N), len(values), Provider.GATED, norm, peak)


def _project_readout(key: ContentKey, field: MeshField, target: ElementKind, source: ProjectSource) -> Readout:
    target_basis = _basis(field, target, skfem)
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            source_basis = _basis(field, origin, skfem)
            projected = np.asarray(target_basis.project(source_basis.interpolate(np.asarray(origin_dofs))))
            round_trip = np.asarray(source_basis.project(target_basis.interpolate(projected)))
            residual = float(np.linalg.norm(round_trip - np.asarray(origin_dofs)))
        case Callable() as fn:
            projected = np.asarray(target_basis.project(fn))
            coords = np.asarray(target_basis.global_coordinates())
            recovered = np.asarray(target_basis.interpolate(projected).value)
            residual = float(np.linalg.norm(recovered - np.asarray(fn(coords))))
        case _ as unreachable:
            assert_never(unreachable)
    norm, peak = _norm_peak(projected)
    return Readout.Project(projected, key, target, int(target_basis.N), Provider.GATED, norm, peak, residual)


@returns_result
def _resample_readout(
    key: ContentKey, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod
) -> Readout:
    dim = len(axes)
    query_count = int(query.reshape(-1, dim).shape[0])
    try:
        engine = FieldEngine.worker()
    except ImportError:
        if dim != 1:
            yield from Error(FLOOR_UNSERVED.raised("resample", str(dim)))
        sampled = np.asarray(np.interp(query, axes[0], values))
        norm, peak = _norm_peak(sampled)
        return Readout.Resample(sampled, key, dim, query_count, "linear", Provider.FLOOR, norm, peak)
    sampled = engine.resample(axes, values, query, method)
    norm, peak = _norm_peak(sampled)
    return Readout.Resample(sampled, key, dim, query_count, method, Provider.GATED, norm, peak)


@returns_result
def _key(label: str, *parts: "bytes | np.ndarray") -> ContentKey:
    key: ContentKey = yield from ContentIdentity.of(label, IdentitySource(parts=tuple(chain.from_iterable(_cells(part) for part in parts))))
    return key


def _cells(part: "bytes | np.ndarray") -> tuple[bytes, ...]:
    if isinstance(part, bytes):
        return (part,)
    arr = np.ascontiguousarray(part)
    return (str(arr.dtype).encode(), repr(arr.shape).encode(), arr.tobytes())
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
