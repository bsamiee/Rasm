# [PY_COMPUTE_FIELD]

One finite-element-and-grid field readout owner beside the FEM assemble and solve routes. `FieldQuery` discriminates the three postprocessing operations a discretized or sampled solution admits — `interpolate` lifts a DOF vector into a `skfem.DiscreteField` and reads `value`/`grad`/`hess` under one `ReadoutKind` axis, `project` L2-projects a callable or cross-basis DOF vector onto a target `ElementKind` basis through `basis.project`, `resample` evaluates a regular grid at query points through the JAX-differentiable `interpax.Interpolator{1,2,3}D`. This owner consumes the solution and never produces it: it reports an in-memory extent and residual and never solves, assembles, or aggregates a field across a grid.

`solvers/mesh#MESH_FIELD` owns the whole element vocabulary — `ElementKind`, the `CTOR` element table with its recursive wrapper build, and `MeshField` — so the interpolate and project cases build the same basis the assemble built, scalar, vector, and composite kinds alike, rather than a second constructor map; `solvers/receipt#RECEIPT` owns the `SolveStatus`/`status_of` residual-floor verdict each case terminates in; the `resample` case realizes the multidimensional consumer `solvers/quadrature#QUADRATURE` defers here. Resolved `FieldReceipt` is the `ReceiptContributor` the weave harvest and the study spine consume, and `_dispatch`'s `@railed` chain `yield from`-binds the `_key` RESULT-identity rail so the mesh-minted `field.content_key` enters as one labeled part of the `IdentitySource(parts=...)` preimage the identity owner frames, and distinct operations over one operand carry distinct keys. `_TRAIT` routes each operation by its own hazard — `interpolate` rides the `RELEASING` thread band (pure scikit-fem/NumPy readout), `project` and `resample` the `HOSTILE` process band (caller-supplied `FieldFn` callbacks run GIL-held; the `interpax` resample is JAX-gated and the x64 flag is process-global) — the numpy nodal readout and `np.interp` 1-D resample staying the in-worker fallbacks an absent package's reification `ImportError` alone selects under a `Provider` discriminant, every readout and arity beyond their reach refusing typed rather than degrading silently, isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing; field evidence stays on the receipt rail, and no field `HandoffAxis` case exists.

## [01]-[INDEX]

- [02]-[FIELD]: `FieldQuery` owns the interpolate/project/resample postprocessing operations, verdicts folded through the `SolveStatus` floor into one `_SLOTS`-driven `FieldReceipt`.

## [02]-[FIELD]

- Owner: `FieldQuery` — the ONE `@tagged_union` field-postprocessing owner; one owner spans the nodal-quadrature readout, the basis-to-basis transfer, and the grid resample.
- Cases: `interpolate` reads its source element off the topology's own `MeshField.element`, never a redundant parallel parameter; `basis.split` is total over every element kind — one `(sub_dofs, sub_basis)` pair per component, a scalar element yielding one pair — so the readout folds a per-component extent and reports the component count and the worst-component peak from the same list rather than collapsing every axis into one scalar or floors an empty case that cannot occur. `basis.split` leads each pair with the sub-vector, so the destructuring order is load-bearing: reading it as `(basis, dofs)` hands a DOF array the `interpolate` call and raises on every solve. `project` carries the genuinely-distinct target `ElementKind` and a `ProjectSource` (a `FieldFn` over physical points or a `(ElementKind, np.ndarray)` cross-basis DOF pair); the transfer rides the `basis.project` METHOD, never a phantom top-level `skfem.project(basis_from=, basis_to=)`. Its cross-basis residual is a source-space round trip — a P1→P2→P1 reports the lost information in one comparison space, never `DiscreteField` value arrays subtracted at incompatible quadrature points — and its callable residual is the physical-point fidelity at `global_coordinates()` the `compute/.api/scikit-fem.md` `[LOCAL_ADMISSION]` mandates, never a finiteness sentinel. `resample` carries the `GridAxes`, gridded values, query points, and the bounded `ResampleMethod`, and has no `MeshField`, so its key is the `_key` rail `_dispatch` already bound.
- Law: the numpy fallback SERVES what it can measure and REFUSES the rest at selection — the nodal `VALUE` extent off a Lagrange DOF vector and the 1-D `np.interp` resample are real measurements, a gradient, a Hessian, and any multidimensional resample are not, and a refusal rails typed off the same `_dispatch` chain the key derive rides. `Provider` then names which engine answered on every served receipt, and the resample's `method` slot carries the REALIZED kernel, so a linear floor result never reads as the `cubic` interpolant the caller asked for. Framing an untouched grid or a raw DOF norm as the operation's extent is the deleted form: it publishes a measurement no kernel took and hands the status floor a number to grade.
- Output: `ReadoutKind` (`VALUE`/`GRAD`/`HESS`) is the ONE bounded readout policy keyed through `_READOUT` onto the `DiscreteField` attribute, so a value, a flux-recovery gradient, and a Hessian are one policy row on the interpolate case rather than three parallel entries — the case is parameterized over its OUTPUT shape, not only its input. Its `nodal` predicate is the floor's servability answer, so which readouts survive an absent package is a property of the readout rather than a branch in the fallback.
- Receipt: `FieldReceipt` is the ONE `@tagged_union` field receipt whose `Literal` `tag` IS the operation. One `_SLOTS` table names each operation's field sequence (`key` leading, `status` trailing) and drives the structural shape, the trailing-slot `.status` read, and the `.facts`/`.content_key`/`.element` accessors, so the table and the case tuples cannot drift. Factories fold extent or residual through the shared `status_of(None, value, _TOL[op])` floor, and `contribute` stays the undecorated `ReceiptContributor` projection settling on the ONE runtime receipt spine — the result key, the provenance pair, the non-convergence band, and the stamp are that owner's columns, so `key` leaves the payload rather than publishing one coordinate twice. Projection transfer fidelity rides the project case as a first-class `SolveStatus` verdict, not a `SolverReceipt` convergence verdict — a projection is not a solve.
- Packages: `skfem` (`Basis`, the `Mesh*`/`Element*` families, `basis.interpolate`/`split`/`project`/`global_coordinates`, `basis.N`, `DiscreteField.value`/`.grad`/`.hess`), `interpax` (`Interpolator{1,2,3}D`, the reusable grid interpolants `FieldEngine.worker()` builds once on the x64-floated rail), `jax` (`config.update("jax_enable_x64", True)` floats the worker to float64 so the interpolant's `grad`/`vjp` holds at double precision rather than the float32 default), `numpy`, `expression`, and the `solvers/mesh#MESH_FIELD`, `solvers/receipt#RECEIPT`, and runtime seams above. This cross-module private `CTOR` import is the reuse `solvers/mesh#MESH_FIELD` runs, never the parallel `_ELEMENTCTOR`/`_MESHCTOR` pair.
- Growth: a new element is one shared `CTOR` row, a vector or composite kind reaching the multi-component readout with no edit here; a new readout is one `ReadoutKind` row, one `_READOUT` entry, and its `nodal` answer on the existing interpolate case; a new field operation is one `FieldQuery` case and one `_SLOTS` row sharing the basis-construction fold and the status floor; a new resample arity beyond 3-D is one `_INTERPOLATOR` row; a new resample kernel is one `ResampleMethod` member; a new worker resample module is one `FieldEngine` field; a new readout statistic is one slot on the owning `_SLOTS` row; a new engine tier is one `solvers/receipt#RECEIPT` `Provider` member reaching every solve route and this owner at once; a new termination class is one `SolveStatus` member shared with every solver route; a new floor refusal is one more subject pair on the ONE `FLOOR_UNSERVED` row, never a sibling row.
- Boundary: field evaluation, projection, and grid resample only — the assemble stays on `solvers/mesh#MESH_FIELD`, the solve on `solvers/quadrature#QUADRATURE`, and columnar/gridded aggregation of the evaluated field in the `data` branch, so this owner reports an in-memory extent and residual and never aggregates across a grid. Rejected: a hand-rolled interpolation loop where `basis.interpolate`/`basis.project`/`interpax.Interpolator` own the concern; a worker resample left on the JAX float32 default; a per-call `import interpax`/`import jax` where the frozen `FieldEngine` folds the modules once; a floor result published under the gated engine's requested method or readout; a `@receipted`-on-`_dispatch` shape swallowing the resample key-derive where `@railed` threads the `_key` rail and the weave harvests. Mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from enum import StrEnum
from itertools import chain
from typing import Any, Final, Literal, Self, assert_never

import numpy as np
from expression import Error, Some, case, tag, tagged_union
from expression.collections import Block, Map

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, evidence_run
from rasm.compute.solvers.mesh import CTOR, ElementKind, MeshField
from rasm.compute.solvers.receipt import Provider, SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, railed, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# cold FEM dependency: the `lazy` bind defers the skfem stack to its first name load, and `_interpolate_receipt`
# traps that reification `ImportError` alone to serve its nodal floor. `interpax`/`jax` are NOT here — they
# ride the `FieldEngine` carrier behind its x64 config seam.
lazy import skfem


# --- [TYPES] -------------------------------------------------------------------------------

type FieldOp = Literal["interpolate", "project", "resample"]
type FieldFn = Callable[[np.ndarray], np.ndarray]
type ProjectSource = FieldFn | tuple[ElementKind, np.ndarray]
type GridAxes = tuple[np.ndarray, ...]
# bounded interpax kernel vocabulary — the resample method is never a free str.
type ResampleMethod = Literal["nearest", "linear", "cubic", "cubic2", "catmull-rom", "monotonic", "monotonic-0"]


class ReadoutKind(StrEnum):
    VALUE = "value"
    GRAD = "grad"
    HESS = "hess"

    @property
    def nodal(self) -> bool:
        # a Lagrange DOF vector IS the nodal value field, so the numpy floor genuinely serves `VALUE` and genuinely
        # serves NEITHER derivative — a gradient and a Hessian need the basis the absent package owns. The predicate
        # is what makes the floor's refusal a selection-time verdict instead of a norm over the wrong quantity.
        return self is ReadoutKind.VALUE


# --- [CONSTANTS] ---------------------------------------------------------------------------

# family trait rows keyed by each route's own hazard: interpolate is a pure scikit-fem/NumPy readout on the
# RELEASING thread band; project runs caller-supplied FieldFn callbacks GIL-held and resample is JAX-gated
# (x64 is process-global), both HOSTILE. Isolation, band, and worker-death retry derive at the Kernel
# crossing owner; numpy fallbacks run inside that same worker when a gated package is absent.
_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("interpolate", KernelTrait.RELEASING),
    ("project", KernelTrait.HOSTILE),
    ("resample", KernelTrait.HOSTILE),
])

# ReadoutKind -> DiscreteField attribute (value, recovered-gradient flux, or Hessian) cataloged on DiscreteField.
_READOUT: Map[ReadoutKind, str] = Map.of_seq([(ReadoutKind.VALUE, "value"), (ReadoutKind.GRAD, "grad"), (ReadoutKind.HESS, "hess")])

# Per-op residual-floor tolerance; the resample row floors its extent verdict where no transfer residual exists.
_TOL: Map[FieldOp, float] = Map.of_seq([("interpolate", 1e-6), ("project", 1e-6), ("resample", 1e-6)])

# Coordinate arity -> `interpax.Interpolator{1,2,3}D` constructor name; the worker resolves it through
# `getattr(self.interpax, _INTERPOLATOR[dim])`, never a `lambda ix: ix.*` per-row closure or body-local re-bind.
_INTERPOLATOR: Map[int, str] = Map.of_seq([(1, "Interpolator1D"), (2, "Interpolator2D"), (3, "Interpolator3D")])

# Per-op payload field names, `key` the leading slot and `status` the trailing slot; the factory packs by it,
# `.status` reads the last slot, and `.facts` projects each named slot, so the table and case tuples cannot drift.
_SLOTS: Map[FieldOp, tuple[str, ...]] = Map.of_seq([
    ("interpolate", ("key", "element", "readout", "dof_count", "components", "provider", "extent", "peak", "status")),
    ("project", ("key", "element", "dof_count", "extent", "residual", "status")),
    # `method` is the REALIZED kernel, never the requested one: the floor runs linear whatever the caller asked for,
    # so a receipt echoing the request would publish a `cubic` extent a cubic interpolant never produced.
    ("resample", ("key", "dim", "query_count", "method", "provider", "extent", "peak", "status")),
])


# --- [TABLES] ------------------------------------------------------------------------------

# ONE parameterized row for the floor's whole refusal law: the numpy fallback serves what it can measure and refuses
# the rest, and the two refusals differ only in WHICH request it cannot take. A per-arm row would fork one law into
# two coordinates the shared census then seats twice, so the arm and the unservable request ride NAMED slots and the
# subject stays this page's one floor point.
FLOOR_UNSERVED: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.FIELD, point="floor", arm="config", defect="floor-unserved", retriability=TERMINAL, slots=("op", "request")
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([FLOOR_UNSERVED]))

# --- [MODELS] ------------------------------------------------------------------------------


@tagged_union(frozen=True)
class FieldReceipt:
    tag: FieldOp = tag()
    interpolate: tuple[ContentKey, ElementKind, ReadoutKind, int, int, Provider, float, float, SolveStatus] = case()
    project: tuple[ContentKey, ElementKind, int, float, float, SolveStatus] = case()
    resample: tuple[ContentKey, int, int, ResampleMethod, Provider, float, float, SolveStatus] = case()

    @classmethod
    def Interpolate(
        cls,
        key: ContentKey,
        element: ElementKind,
        readout: ReadoutKind,
        dof_count: int,
        components: int,
        provider: Provider,
        extent: float,
        peak: float,
    ) -> Self:
        return cls(interpolate=(key, element, readout, dof_count, components, provider, extent, peak, status_of(None, extent, _TOL["interpolate"])))

    @classmethod
    def Project(cls, key: ContentKey, element: ElementKind, dof_count: int, extent: float, residual: float) -> Self:
        return cls(project=(key, element, dof_count, extent, residual, status_of(None, residual, _TOL["project"])))

    @classmethod
    def Resample(
        cls, key: ContentKey, dim: int, query_count: int, method: ResampleMethod, provider: Provider, extent: float, peak: float
    ) -> Self:
        return cls(resample=(key, dim, query_count, method, provider, extent, peak, status_of(None, extent, _TOL["resample"])))

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case (
                FieldReceipt(tag="interpolate", interpolate=payload)
                | FieldReceipt(tag="project", project=payload)
                | FieldReceipt(tag="resample", resample=payload)
            ):
                return dict(zip(_SLOTS[self.tag], payload, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def content_key(self) -> ContentKey:
        return self.facts["key"]

    @property
    def element(self) -> ElementKind | None:
        return self.facts.get("element")

    @property
    def status(self) -> SolveStatus:
        match self:
            case (
                FieldReceipt(tag="interpolate", interpolate=(*_, SolveStatus() as status))
                | FieldReceipt(tag="project", project=(*_, SolveStatus() as status))
                | FieldReceipt(tag="resample", resample=(*_, SolveStatus() as status))
            ):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    def contribute(self) -> Iterable[Receipt]:
        # ONE settled-receipt spine: the result key, the provenance pair, the warning band, and the stamp are the
        # runtime owner's columns, so `key` leaves the payload rather than publishing the coordinate twice. The band
        # IS the non-convergence roster — an extent or residual over the op's floor names its own termination class —
        # and provenance names the produced key alone, this owner minting the RESULT key and retaining no operand key.
        subject = self.element.value if self.element is not None else self.tag
        facts: dict[str, object] = {
            "operation": self.tag, "converged": self.converged, **{name: value for name, value in self.facts.items() if name != "key"}
        }
        return (
            Receipt.of(
                EvidenceScope.FIELD.value,
                ("emitted", subject, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=Block.empty() if self.converged else Block.singleton(self.status.value),
            ),
        )


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

    async def evaluate(self, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[FieldReceipt]:
        # gated readouts cross the process lane as spec + operands; `_dispatch` resolves imports in the
        # worker, where the x64 gate applies, and the weave owns span, fence, and receipt harvest under the
        # caller's composition key, so an embedded composition's lifecycle facts key to it rather than the root's.
        async def dispatch() -> RuntimeRail[FieldReceipt]:
            return (await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.FIELD, f"field.{self.tag}", dispatch, facts={"op": self.tag}, composition=composition)


# --- [SERVICES] ----------------------------------------------------------------------------


# worker `interpax`/`jax` modules folded into ONE frozen value object; `worker()` imports once behind the
# band and floats the rail to x64 before the interpolant is built (pure-`skfem` eval needs no x64). They stay
# function-local against the module-scope `lazy` dialect on the compute RULINGS [04] x64 ruling: the config seam
# must precede the first jax-dependent import, and the frozen carrier enforces that ordering structurally. `resample`
# resolves the arity row through `getattr(self.interpax, _INTERPOLATOR[dim])`, builds the reusable interpolant
# ONCE, and splits the `(N, dim)` query into `(dim, N)` columns via `reshape(-1, dim).T` for `dim>1` / raw for 1-D.
@dataclass(frozen=True, slots=True)
class FieldEngine:
    interpax: Any

    @classmethod
    def worker(cls) -> Self:
        import jax  # ruff:ignore[import-outside-top-level] — x64 config seam

        jax.config.update("jax_enable_x64", True)  # interpax Interpolator pytrees default to float32; the differentiable grad/vjp assumes float64

        import interpax  # ruff:ignore[import-outside-top-level] — binds after the arm so no import-time interpax constant bakes float32

        return cls(interpax=interpax)

    def resample(self, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> np.ndarray:
        dim = len(axes)
        interpolant = getattr(self.interpax, _INTERPOLATOR[dim])(*axes, np.asarray(values), method=method)
        if dim == 1:
            return np.asarray(interpolant(np.asarray(query)))
        return np.asarray(interpolant(*np.asarray(query).reshape(-1, dim).T))


# --- [OPERATIONS] --------------------------------------------------------------------------


# EVERY arm `yield from`-binds `_key`'s rail so a canonical-encode fault rides the one rail and the receipt
# key names the RESULT — the mesh-minted `field.content_key` enters as one labeled part beside the operand
# cells, so two readouts over one field never share a key (an operand-keyed receipt is the deleted form).
@railed
def _dispatch(query: FieldQuery) -> FieldReceipt:
    match query:
        case FieldQuery(tag="interpolate", interpolate=(field, dofs, readout)):
            dofs_arr = np.asarray(dofs)
            key: ContentKey = yield from _key("field-interpolate", field.content_key.project("hex").encode(), readout.value.encode(), dofs_arr)
            receipt: FieldReceipt = yield from _interpolate_receipt(key, field, dofs_arr, readout)
            return receipt
        case FieldQuery(tag="project", project=(field, target, source)):
            key = yield from _key("field-project", field.content_key.project("hex").encode(), target.value.encode(), *_source_parts(source))
            return _project_receipt(key, field, target, source)
        case FieldQuery(tag="resample", resample=(axes, values, query, method)):
            grid = np.asarray(values)
            key = yield from _key("field-resample", method.encode(), np.concatenate([np.ravel(a) for a in axes]), grid, np.asarray(query))
            sampled: FieldReceipt = yield from _resample_receipt(key, axes, grid, np.asarray(query), method)
            return sampled
        case _ as unreachable:
            assert_never(unreachable)


def _source_parts(source: ProjectSource) -> tuple["bytes | np.ndarray", ...]:
    # project-source identity cells: a cross-basis pair folds origin element + DOF bytes; a callable folds
    # its qualified name (closure source is not byte-stable across runs).
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            return (origin.value.encode(), origin_dofs)
        case fn:
            return (getattr(fn, "__qualname__", repr(fn)).encode(),)



def _extent(values: np.ndarray) -> tuple[float, float]:
    return (float(np.linalg.norm(values)), float(np.max(np.abs(values)))) if values.size else (0.0, 0.0)


# reads the one `mesh.CTOR` element row the assemble reads and builds through the row's own recursive `built`, so a
# scalar, vector, or composite element resolves identically over the node-major/element-major `mesh.p`/`mesh.t`
# layout — never a second map and never a per-family construction arm.
def _basis(field: MeshField, element: ElementKind, skfem: Any) -> Any:
    row = CTOR[element]
    mesh = getattr(skfem, row.mesh)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    return skfem.Basis(mesh, row.built(skfem))


# `basis.split(dofs)` returns `list[tuple[ndarray, AbstractBasis]]` — one `(sub_dofs, sub_basis)` pair PER
# COMPONENT, the sub-vector leading — and a scalar element yields a one-element list rather than an empty one, so the
# split IS total over every element kind and no empty-case floor is owed. Destructuring it the other way round
# reads a DOF array as a basis and raises on the first readout of every solve, scalar and mixed alike. Its floor
# serves the NODAL readout alone and REFUSES the two it cannot take: a DOF vector carries the nodal values, so its
# norm is the honest `VALUE` extent, while a gradient and a Hessian exist only through the basis the absent package
# owns. Reporting a DOF norm under a `GRAD`/`HESS` readout publishes a quantity nothing computed, and a
# `components=1` beside it asserts a split that never ran — the two facts the caller most needs are the two forged.
@railed
def _interpolate_receipt(key: ContentKey, field: MeshField, dofs: np.ndarray, readout: ReadoutKind) -> FieldReceipt:
    element = field.element
    # one bare `skfem` NAME LOAD is all the `try` brackets — the module-scope `lazy` proxy reifies on that first
    # LOAD_GLOBAL, so the `ImportError` this arm catches names exactly the absent FEM band; the basis build runs
    # OUTSIDE it, so a constructor raise from an installed skfem propagates to the `@railed` boundary as the
    # defect it is instead of masquerading as the floor.
    try:
        fem = skfem
    except ImportError:
        if not readout.nodal:
            yield from Error(FLOOR_UNSERVED.raised("interpolate", readout.value))
        extent, peak = _extent(dofs)
        return FieldReceipt.Interpolate(key, element, readout, int(dofs.size), 1, Provider.FLOOR, extent, peak)
    basis = _basis(field, element, fem)
    components = basis.split(dofs)
    readouts = tuple(_extent(np.asarray(getattr(sub.interpolate(part), _READOUT[readout]))) for part, sub in components)
    extent = float(np.linalg.norm(np.asarray([norm for norm, _ in readouts])))
    peak = max((mag for _, mag in readouts), default=0.0)
    return FieldReceipt.Interpolate(key, element, readout, int(basis.N), len(readouts), Provider.GATED, extent, peak)


# residual is a source-space round trip; the callable path reads the projected DOFs back at the target
# basis's physical coordinates (`global_coordinates`) against the source callable there — never finiteness.
def _project_receipt(key: ContentKey, field: MeshField, target: ElementKind, source: ProjectSource) -> FieldReceipt:
    target_basis = _basis(field, target, skfem)
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            source_basis = _basis(field, origin, skfem)
            projected = np.asarray(target_basis.project(source_basis.interpolate(np.asarray(origin_dofs))))
            round_trip = np.asarray(source_basis.project(target_basis.interpolate(projected)))
            residual = float(np.linalg.norm(round_trip - np.asarray(origin_dofs)))
        case Callable() as fn:  # ProjectSource callable arm; residual is the physical-point fidelity, never a sentinel.
            projected = np.asarray(target_basis.project(fn))
            coords = np.asarray(target_basis.global_coordinates())
            residual = float(np.linalg.norm(projected - np.asarray(fn(coords))))
        case _ as unreachable:
            assert_never(unreachable)
    extent, _ = _extent(projected)
    return FieldReceipt.Project(key, target, int(target_basis.N), extent, residual)


# regular-grid resample over the interpax interpolant folded through `FieldEngine.worker()`. The resample has no
# `MeshField`, so its content key is the `_key` rail `_dispatch` already bound, never re-derived.
# `np.interp` is a 1-D kernel and numpy ships no multidimensional regular-grid interpolant, so the floor SERVES the
# 1-D case under the realized `linear` method and REFUSES every higher arity. The deleted form returned the untouched
# grid `values.ravel()` and framed its norm as the resample extent beside the caller's `query_count` and requested
# `cubic` — three facts about a resample that never ran, and the one status the floor would then report reads as a
# converged-or-stagnated verdict on a measurement no kernel took.
@railed
def _resample_receipt(key: ContentKey, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> FieldReceipt:
    dim = len(axes)
    # `worker()` alone sits inside the `try` — its body IS the jax/interpax import seam — so the floor fires only
    # on an absent JAX band; an `ImportError` out of the interpolant build or evaluation on an installed band
    # propagates to the `@railed` boundary as the defect it is.
    try:
        engine = FieldEngine.worker()
    except ImportError:
        if dim != 1:
            yield from Error(FLOOR_UNSERVED.raised("resample", str(dim)))
        extent, peak = _extent(np.interp(query, axes[0], values))
        return FieldReceipt.Resample(key, dim, int(query.size), "linear", Provider.FLOOR, extent, peak)
    sampled = engine.resample(axes, values, query, method)
    extent, peak = _extent(sampled)
    return FieldReceipt.Resample(key, dim, int(query.size), method, Provider.GATED, extent, peak)


# field RESULT-identity mint every `_dispatch` arm binds. The framing is the identity owner's and NEVER this
# page's: `IdentitySource(parts=...)` is its ONE spelling for N semantic parts, and the count-and-length frame runs
# under `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` at that owner. The two hand-rolled `len(x).to_bytes(8, "big")`
# layers this deleted chose a width and a byte order at a call site — the fork the framing law exists to foreclose,
# since a width chosen here and a width chosen at a sibling page split the key namespace with no surface able to
# report it. Raw value bytes alone stay the deleted keying form: dtype and shape are their own cells, so a reshaped
# or re-typed operand re-keys. `ContentIdentity.of` returns `RuntimeRail[ContentKey]`, so `_key` `yield from`-binds
# the key off the rail.
@railed
def _key(label: str, *parts: "bytes | np.ndarray") -> ContentKey:
    key: ContentKey = yield from ContentIdentity.of(label, IdentitySource(parts=tuple(chain.from_iterable(_cells(part) for part in parts))))
    return key


def _cells(part: "bytes | np.ndarray") -> tuple[bytes, ...]:
    # one operand's SEMANTIC cells: raw bytes are one cell, an array is its dtype, its shape, and its C-ordered
    # buffer as three, so every cell enters the owner's frame individually and no nested join can collide.
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
