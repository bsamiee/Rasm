# [PY_COMPUTE_FIELD]

The one finite-element-and-grid field readout owner beside the FEM assemble and solve routes. `FieldQuery` discriminates the three postprocessing operations a discretized or sampled solution admits: `interpolate` lifts a DOF vector into a `skfem.DiscreteField` and reads `value`/`grad`/`hess` under one `ReadoutKind` output axis; `project` L2-projects a callable field or a cross-basis DOF vector onto a target `ElementKind` basis through `basis.project`; `resample` evaluates a regular grid at query points through the JAX-differentiable `interpax.Interpolator{1,2,3}D` so a downstream `grad`/`vjp` flows through the sampled values.

The owner consumes the solution, never produces it: the solve stays on `solvers/quadrature.md#QUADRATURE`, the assemble on `solvers/mesh.md#EXCHANGE`. The `interpolate` and `project` cases read the same `mesh.CTOR` `(Mesh*, Element*, cell-type)` triple the assemble reads, so the discretization and its readout share one `Basis(mesh, Element*())`. The `resample` case realizes the multidimensional `interp2d`/`interp3d` consumer the 1-D `solvers/quadrature.md#QUADRATURE` route defers here per `[INTERPAX_QUADAX_USAGE]`.

`FieldReceipt` is one operation-discriminated `@tagged_union` whose `Literal` `tag` IS the operation, with its per-case payload, `.status` read, accessor projection, and observability row all driven by one `_SLOTS` table — the `solvers/receipt.md#RECEIPT` `SolverReceipt` and `solvers/mesh.md#EXCHANGE` `MeshReceipt` discipline — each case terminating in the shared `SolveStatus` verdict the receipt floor adjudicates. `_dispatch` is the `@railed` `effect.result` chain whose every arm `yield from`-binds the `_key` RESULT-identity rail — the mesh-minted `field.content_key` enters as one labeled part beside the DOF bytes, readout/target/source cells, or grid+query+method payload, so an operand-keyed receipt is the deleted form; the weave's `@receipted(REDACTION)` harvest streams the contributor, and `evaluate` joins the rail through `boundary(...).bind(lambda rail: rail)`, so the canonical-encode fault rides the one rail rather than a `@receipted`-on-`_dispatch` shape that raises through a phantom exception bridge. `contribute` stays the plain `ReceiptContributor` projection the aspect harvests.

Every `FieldQuery` dispatches on the PROCESS lane — the `interpax` resample is JAX-gated and the x64 flag is process-global, so the isolation pin covers all three routes uniformly; the numpy nodal readout and the `np.interp` 1-D resample are the in-worker `ImportError` fallbacks executing inside that same lane, never a second lane assignment.

## [01]-[INDEX]

- [01]-[FIELD]: the interpolate/project/resample field operations, the `DiscreteField` `value`/`grad`/`hess` readout with vector/composite split, the `basis.project` L2 transfer with physical-point round-trip residual, and the `interpax` differentiable grid resample, folded through the shared `SolveStatus` floor on one `FieldQuery` owner and one `_SLOTS`-driven `FieldReceipt`.

## [02]-[FIELD]

- Owner: `FieldQuery` — the ONE `@tagged_union` field-postprocessing owner discriminating `interpolate` (DOF vector → `skfem.DiscreteField` `value`/`grad`/`hess` under one `ReadoutKind`, vector/composite split), `project` (callable or cross-basis field → target-basis DOF vector through `basis.project`), and `resample` (regular grid → query-point evaluation through `interpax.Interpolator{1,2,3}D`). The `interpolate` case reads its source element off the `MeshField.element` the topology already owns rather than a parallel element parameter; the `project` case carries the `MeshField`, the genuinely-distinct target `ElementKind`, and the `ProjectSource` (a `FieldFn` over physical points or a `(ElementKind, np.ndarray)` cross-basis DOF pair); the `resample` case carries the `GridAxes`, the gridded values, the query points, and the bounded `ResampleMethod` kernel. One owner spans the nodal-quadrature readout, the basis-to-basis transfer, and the grid resample.
- Output axis: `ReadoutKind` (`VALUE`/`GRAD`/`HESS`) is the ONE bounded `DiscreteField`-readout policy, keyed through `_READOUT` onto the `DiscreteField` attribute so a value readout, a gradient (flux-recovery) readout, and a Hessian readout are one policy row on the `interpolate` case rather than three parallel `Interpolate`/`Gradient`/`Hessian` entries. The cataloged `DiscreteField` carries `value`/`grad`/`hess` directly, so the `interpolate` case is parameterized over its OUTPUT shape, not only its input, and the flux-recovery/Hessian readouts are a `_READOUT` projection rather than a future case.
- Element axis: `solvers/mesh.md#MESH_FIELD` owns the whole element vocabulary — `ElementKind`, the `CTOR` `Map[ElementKind, tuple[str, str, str]]` `(Mesh*, Element*, cell-type)` triple, and `MeshField` — mesh constructs elements and owns assembly, so this owner imports all three from mesh rather than redeclaring a constructor map, and `_basis` reads `mesh_ctor, element_ctor, _ = CTOR[element]` then resolves both spellings through `getattr(skfem, ...)` at evaluation time — the identical row the assemble reads, so the element vocabulary never diverges across assemble, solve, and readout and a new element is one shared `CTOR` row. A field interpolated over a `TRI_P1` `MeshField` reads the same `Basis(MeshTri1(points.T, cells.T), ElementTriP1())` the assemble built.
- Interpolate: the interpolate case builds the source basis over the `MeshField` topology for the topology's own `field.element` (never a redundant element parameter beside the value object that owns it), calls `basis.interpolate(dofs)` to lift the DOF vector into a `skfem.DiscreteField` sampled at the element quadrature points, and reads the `_READOUT[kind]` attribute (`value`/`grad`/`hess`); for a vector or composite element it folds `basis.split(dofs)` — which the catalog returns as a `(subbases, subvectors)` pair-of-lists — into per-component sub-bases through `zip(*basis.split(dofs))` and re-interpolates each so a vector displacement or a mixed velocity/pressure field reports the component count and the worst-component peak rather than collapsing every axis into one scalar. A scalar element yields one component, the `or ((basis, dofs),)` floor covering a split that returns empty for a non-vector space. The evaluation extent is the readout-array L2 norm; the peak is the per-axis max absolute magnitude; the status is the residual-floor verdict over the extent against the interpolate tolerance, the evidence a downstream gate reads to admit a field for kernel handoff. The readout never solves and never assembles.
- Project: the project case folds through the `basis.project` METHOD — the catalog's L2-projection entry `basis.project(interp, elements=None)`, never a phantom top-level `skfem.project(basis_from=, basis_to=)`. A callable source projects directly (`target_basis.project(fun)`); a cross-basis source lifts its source-basis DOF vector to a `DiscreteField` and projects that onto the target (`target_basis.project(source_basis.interpolate(source_dofs))`), returning the target DOF vector. The cross-basis transfer residual is a source-space round trip: the projected vector is interpolated on the target basis, re-projected onto the source basis (`source_basis.project(target_basis.interpolate(projected))`), and the L2 norm taken against the original source DOFs, so a P1→P2→P1 round-trip reports the lost information in a single comparison space rather than subtracting `DiscreteField` value arrays at incompatible quadrature points. The callable path has no source DOF vector, so its residual is the physical-point transfer fidelity the `compute/.api/scikit-fem.md` `[LOCAL_ADMISSION]` mandates — the projected DOFs read back at `target_basis.global_coordinates()` (the `doflocs` surface) against the source callable evaluated there, never a finiteness-only sentinel. The residual feeds the shared `SolveStatus` floor, so the transfer fidelity is a first-class termination verdict carried in the project case rather than a `SolverReceipt` convergence verdict, because a projection is not a solve.
- Receipt: `FieldReceipt` is the ONE `@tagged_union` field receipt whose `Literal` `tag` IS the operation. One `_SLOTS` table names each operation's field sequence — `interpolate → (key, element, readout, dof_count, components, extent, peak, status)`, `project → (key, element, dof_count, extent, residual, status)`, `resample → (key, dim, query_count, extent, peak, status)`, `key` the common leading slot and `status` the common trailing slot — and drives the structural shape, the `.status` trailing-slot read through one `case (*_, status)` closed by `assert_never`, the `.content_key`/`.element` accessors off `.facts`, and the `.facts` projection through `dict(zip(_SLOTS[self.tag], payload, strict=True))`, so the table and the case tuples cannot drift. The four `@classmethod` factories returning `Self` fold their extent or residual through the shared `status_of(None, value, _TOL[op])` floor. `FieldReceipt.contribute` is the undecorated `ReceiptContributor` port method satisfied structurally, returning the one-element `Iterable[Receipt]` `Receipt.of("compute.field", ("emitted", subject, facts))` over the runtime two-argument `(owner, (Phase, subject, facts))` contract, carrying the operation tag, the derived `converged` flag, and the spread of `.facts` riding as native scalars through the runtime `Signals` `msgspec` encoder, keyed by the content key. `contribute` itself carries no decorator — the hub weave's harvest streams the contributor it returns the contributor, exactly as `solvers/mesh.md#EXCHANGE` `MeshReceipt.contribute` and `solvers/receipt.md#RECEIPT` `SolverReceipt.contribute` stay undecorated port methods. An evaluated, projected, or resampled field graduating outward routes through `graduation/handoff.md#GRADUATION` on the `solver` `HandoffAxis` case; the project case carries the transfer residual the graduation fold reads, the interpolate and resample cases carrying the readout extent that floor adjudicates.
- Packages: `skfem` (`Basis`, `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1`, `MeshLine1`/`MeshTri1`/`MeshTet1`/`MeshQuad1`/`MeshHex1`; `basis.interpolate`, `basis.split`, `basis.project`, `basis.N` the dof count, `basis.global_coordinates`, `DiscreteField.value`/`.grad`/`.hess` — the projection is the `basis.project` METHOD, never a top-level `skfem.project`), `interpax` (`Interpolator1D`/`Interpolator2D`/`Interpolator3D` — the JAX-differentiable reusable grid interpolants the `FieldEngine.worker()` resample fold constructs once on the x64-floated rail), `jax` (`config.update("jax_enable_x64", True)` floating the worker `FieldEngine` rail to float64 so the interpax interpolant's `grad`/`vjp` holds at double precision rather than the float32 default — the same x64 contract every sibling JAX route carries), `numpy` (`asarray`, `ascontiguousarray`, `abs`, `max`, `ravel`, `reshape`, `concatenate`, `interp`, `linalg.norm`), `dataclasses` (`dataclass(frozen=True, slots=True)` for the `FieldEngine` carrier folding the worker `interpax`/`jax` modules, `Self`-bound `worker()` matching the sibling routes), `expression` (`tag`, `case`, `tagged_union`, `expression.collections.Map` the `CTOR`/`_TOL`/`_SLOTS`/`_MODALITY` table rail), `solvers/mesh.md#MESH_FIELD` (`ElementKind` the shared element axis enum, `MeshField` the topology and DOF source the readout consumes, `CTOR` the ONE `(Mesh*, Element*, cell-type)` triple table — the one element-vocabulary owner), `solvers/receipt.md#RECEIPT` (`SolveStatus`, `status_of` — the shared termination vocabulary and residual-floor verdict the field receipt folds into), runtime (`RuntimeRail`, `boundary`, `railed` the bound `effect.result` builder the `_dispatch`/`_key` chains thread, `Receipt`/`Redaction`/`receipted` — `Receipt` the lone receipt type the `ReceiptContributor` port is satisfied against structurally; the hub weave (`EvidenceScope`/`evidence_run`) owns span, fence, and the `@receipted(REDACTION)` harvest wears, plus `ContentIdentity`/`ContentKey`/`IdentityPolicy` the resample key fold). The cross-module private `CTOR` import is the same reuse `solvers/mesh.md#MESH_FIELD` runs, never the parallel `_ELEMENTCTOR`/`_MESHCTOR` pair the prior corpus split.
- Growth: a new element is one `CTOR` row shared with the assemble and solve routes; a new readout (gradient already `ReadoutKind.GRAD`, Hessian already `ReadoutKind.HESS`) is one `ReadoutKind` row plus one `_READOUT` entry on the existing interpolate case; a new field operation (flux recovery as a functional, adaptive probing) is one `FieldQuery` case plus one `_SLOTS` row sharing the basis-construction fold and the status floor; a new resample arity beyond 3-D is one `_INTERPOLATOR` row; a new resample kernel is one `ResampleMethod` member; a new worker resample module is one `FieldEngine` field plus one `worker()` import line read off the carrier; a new readout statistic is one slot on the owning `_SLOTS` row; a new termination class is one `SolveStatus` member shared with every solver route; zero new surface, never a parallel evaluation container beside the `DiscreteField`, never a second projection entry, never a separate multidimensional-interpolation owner, never a flat receipt with a stringly `operation` field or a sentinel residual, and never a solve or assemble on this owner.
- Boundary: field evaluation, projection, and grid resample only — the `DiscreteField` quadrature readout, the vector/composite split, the `basis.project` transfer with physical-point residual, and the `interpax` grid resample are in-scope; the assemble stays on `solvers/mesh.md#MESH_FIELD`, the solve on `solvers/quadrature.md#QUADRATURE`, and the columnar/gridded statistical aggregation of the evaluated field stays in the `data` branch, so this owner reports an in-memory extent and residual and never aggregates a field across a grid. The deleted forms: a solve on this owner; a hand-rolled interpolation loop where `basis.interpolate`/`basis.project`/`interpax.Interpolator` own the concern; a phantom top-level `skfem.project(basis_from=, basis_to=)` where the projection is the `basis.project` method; a finiteness-only `0.0`/`inf` callable residual where the catalog mandates the `global_coordinates`/`probes` physical-point readout; a `SolverReceipt` minted for a projection; a free-`str` resample `method` where `ResampleMethod` bounds the interpax kernel; a worker resample left on the JAX default float32 where `FieldEngine.worker()` runs `jax.config.update("jax_enable_x64", True)` before the `Interpolator{1,2,3}D` is built (the differentiable `grad`/`vjp` assuming float64, the same x64 contract every sibling JAX route carries); a per-call `import interpax`/`import jax` re-import or a body-local x64 toggle where the frozen `FieldEngine` folds the modules once; a phantom `np.atleast_2d` query reshape where the branch `numpy.md` catalogs `reshape(-1, dim).T`; a non-total `project` `match` lacking the `assert_never` callable-arm closure; a second local element-constructor table where the one `CTOR` triple is imported; a `@receipted`-on-`_dispatch` shape that swallows the resample content-key derive where the `@railed` `_dispatch` threads the `_key` rail and the weave harvests the receipt; a `match ContentIdentity.of(...)`/`raise fault.as_exception()` re-raise through a phantom exception bridge where the `railed` `yield from`-bind carries the canonical-encode fault onto the one rail; and an inline `Signals.emit` where the weave's harvest owns egress. The mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Callable, Iterable
from dataclasses import dataclass
from enum import StrEnum
from typing import Any, Final, Literal, Self, assert_never

import numpy as np
from expression import case, tag, tagged_union
from expression.collections import Map

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.mesh import CTOR, ElementKind, MeshField
from rasm.compute.solvers.receipt import SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary, railed
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------

type FieldOp = Literal["interpolate", "project", "resample"]
type FieldFn = Callable[[np.ndarray], np.ndarray]
type ProjectSource = FieldFn | tuple[ElementKind, np.ndarray]
type GridAxes = tuple[np.ndarray, ...]
# the shared interpax kernel vocabulary the resample case selects, bounded so the method is never a free str.
type ResampleMethod = Literal["nearest", "linear", "cubic", "cubic2", "catmull-rom", "monotonic", "monotonic-0"]


class ReadoutKind(StrEnum):
    VALUE = "value"
    GRAD = "grad"
    HESS = "hess"


# --- [CONSTANTS] ---------------------------------------------------------------------------

# the family modality rows: every route pins PROCESS — the gated interpax path is JAX-backed and the
# x64 flag is process-global native state, and the numpy fallbacks execute inside the same worker when
# a gated package is absent — policy DATA beside the route tables, never a per-page literal.
_MODALITY: Final[Map[str, Modality | None]] = Map.of_seq([
    ("interpolate", Modality.PROCESS),
    ("project", Modality.PROCESS),
    ("resample", Modality.PROCESS),
])

# ReadoutKind -> DiscreteField attribute. The interpolate readout is parameterized over its output:
# the value array, the recovered gradient (flux), or the Hessian, all cataloged on DiscreteField.
_READOUT: Map[ReadoutKind, str] = Map.of_seq([(ReadoutKind.VALUE, "value"), (ReadoutKind.GRAD, "grad"), (ReadoutKind.HESS, "hess")])

# Per-operation residual-floor tolerance, one row per FieldOp; the resample row floors the resample
# extent verdict where no transfer residual exists. A new operation tolerance is one row.
_TOL: Map[FieldOp, float] = Map.of_seq([("interpolate", 1e-6), ("project", 1e-6), ("resample", 1e-6)])

# Coordinate-tuple arity -> the `interpax.Interpolator{1,2,3}D` constructor NAME, keyed by the
# resolves the worker symbol through `getattr(self.interpax, _INTERPOLATOR[dim])` off the engine's folded
# handle — the same name-keyed deferred-resolver discipline the `solvers/quadrature.md#QUADRATURE` `_QUAD`
# row catalog runs via `getattr(self.quadax, row.adaptive)`, never a `lambda ix: ix.*` closure-per-row nor
# a body-local re-bind. A new arity is one row.
_INTERPOLATOR: Map[int, str] = Map.of_seq([(1, "Interpolator1D"), (2, "Interpolator2D"), (3, "Interpolator3D")])

# Per-operation payload field names, one tuple per tag, `key` the common leading slot and `status` the
# common trailing slot. The single owner over the case shapes: the factory packs by it, the accessors
# read fixed slots off it, `.status` reads the last slot, and `.facts` projects each named slot —
# mirroring solvers/receipt.md#RECEIPT `_SLOTS`, so an operation's evidence is one row.
_SLOTS: Map[FieldOp, tuple[str, ...]] = Map.of_seq([
    ("interpolate", ("key", "element", "readout", "dof_count", "components", "extent", "peak", "status")),
    ("project", ("key", "element", "dof_count", "extent", "residual", "status")),
    ("resample", ("key", "dim", "query_count", "extent", "peak", "status")),
])


# --- [MODELS] ------------------------------------------------------------------------------


@tagged_union(frozen=True)
class FieldReceipt:
    tag: FieldOp = tag()
    interpolate: tuple[ContentKey, ElementKind, ReadoutKind, int, int, float, float, SolveStatus] = case()
    project: tuple[ContentKey, ElementKind, int, float, float, SolveStatus] = case()
    resample: tuple[ContentKey, int, int, float, float, SolveStatus] = case()

    @classmethod
    def Interpolate(
        cls, key: ContentKey, element: ElementKind, readout: ReadoutKind, dof_count: int, components: int, extent: float, peak: float
    ) -> Self:
        return cls(interpolate=(key, element, readout, dof_count, components, extent, peak, status_of(None, extent, _TOL["interpolate"])))

    @classmethod
    def Project(cls, key: ContentKey, element: ElementKind, dof_count: int, extent: float, residual: float) -> Self:
        return cls(project=(key, element, dof_count, extent, residual, status_of(None, residual, _TOL["project"])))

    @classmethod
    def Resample(cls, key: ContentKey, dim: int, query_count: int, extent: float, peak: float) -> Self:
        return cls(resample=(key, dim, query_count, extent, peak, status_of(None, extent, _TOL["resample"])))

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
        subject = self.element.value if self.element is not None else self.tag
        facts: dict[str, object] = {"operation": self.tag, "converged": self.converged, **self.facts}
        return (Receipt.of("compute.field", ("emitted", subject, facts)),)


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

    async def evaluate(self, lane: LanePolicy) -> RuntimeRail[FieldReceipt]:
        # the gated readouts cross the process lane as spec data plus operands (the module-level
        # `_dispatch` kernel resolves by import in the worker, where the x64 gate applies); the weave
        # owns span, fence, and the `@receipted(REDACTION)` receipt harvest.
        async def dispatch() -> RuntimeRail[FieldReceipt]:
            return (await lane.offload(_dispatch, self, modality=_MODALITY[self.tag])).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.FIELD, f"field.{self.tag}", dispatch)


# --- [SERVICES] ----------------------------------------------------------------------------


# The worker `interpax`/`jax` modules folded into ONE frozen value object with behavior — the
# `solvers/quadrature.md#QUADRATURE` `QuadEngine.worker()`/`solvers/differential.md#DIFFERENTIAL`
# `SolveEngine.worker()` discipline every sibling JAX route runs. `worker()` imports once behind the band
# and runs `jax.config.update("jax_enable_x64", True)` BEFORE the interpolant is constructed: the
# `Interpolator{1,2,3}D` is a JAX pytree, so on the float32 default its `grad`/`vjp` through the sampled
# values degrades below double precision — the same x64 contract the FEM band's pure-`scipy` `skfem` eval
# does NOT need (only the JAX-backed interpax resample does). `resample` is the SINGLE arity-keyed
# `Interpolator{1,2,3}D` constructor-and-evaluate fold: the row resolves through `getattr(self.interpax,
# _INTERPOLATOR[dim])`, the reusable interpolant builds ONCE (the cataloged preferred form over the
# recompiled-per-call `interp{1,2,3}d`), and the `(N, dim)` query splits into the `(dim, N)` per-axis
# columns through the cataloged `reshape(-1, dim).T` for `dim>1` / the raw query for 1-D — never a
# per-call re-import, a body-local x64 toggle, or the phantom `np.atleast_2d`.
@dataclass(frozen=True, slots=True)
class FieldEngine:
    interpax: Any

    @classmethod
    def worker(cls) -> Self:
        import interpax
        import jax

        jax.config.update("jax_enable_x64", True)  # interpax Interpolator pytrees default to float32; the differentiable grad/vjp assumes float64
        return cls(interpax=interpax)

    def resample(self, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> np.ndarray:
        dim = len(axes)
        interpolant = getattr(self.interpax, _INTERPOLATOR[dim])(*axes, np.asarray(values), method=method)
        if dim == 1:
            return np.asarray(interpolant(np.asarray(query)))
        return np.asarray(interpolant(*np.asarray(query).reshape(-1, dim).T))


# --- [OPERATIONS] --------------------------------------------------------------------------


# `_dispatch` is the one `railed` `effect.result` chain `evaluate` joins through `bind`: EVERY arm
# `yield from`-binds `_key`'s `RuntimeRail[ContentKey]` so a canonical-encode fault rides the one rail
# and the receipt key names the RESULT — the mesh-minted `field.content_key` enters the fold as one
# labeled part beside the DOF bytes, the readout/target/source discriminants, or the grid+query+method
# payload, so two readouts over one field, two DOF vectors over one mesh, or two resample queries over
# one grid never share a `FieldReceipt` key (an operand-keyed receipt is the deleted form). Each arm
# returns its receipt whole; the weave's harvest emits the `FieldReceipt.contribute()` stream on exit —
# the `railed`-over-`match`/`raise` ROP collapse the mesh route's `_dispatch` runs, never the
# `@receipted`-on-`_dispatch`-plus-`as_exception`-re-raise shape.
@railed
def _dispatch(query: FieldQuery) -> FieldReceipt:
    match query:
        case FieldQuery(tag="interpolate", interpolate=(field, dofs, readout)):
            dofs_arr = np.asarray(dofs)
            key: ContentKey = yield from _key("field-interpolate", field.content_key.project("hex").encode(), readout.value.encode(), dofs_arr)
            return _interpolate_receipt(key, field, dofs_arr, readout)
        case FieldQuery(tag="project", project=(field, target, source)):
            key = yield from _key("field-project", field.content_key.project("hex").encode(), target.value.encode(), *_source_parts(source))
            return _project_receipt(key, field, target, source)
        case FieldQuery(tag="resample", resample=(axes, values, query, method)):
            grid = np.asarray(values)
            key = yield from _key("field-resample", method.encode(), np.concatenate([np.ravel(a) for a in axes]), grid, np.asarray(query))
            return _resample_receipt(key, axes, grid, np.asarray(query), method)
        case _ as unreachable:
            assert_never(unreachable)


def _source_parts(source: ProjectSource) -> tuple["bytes | np.ndarray", ...]:
    # the project-source identity cells: a cross-basis pair folds its origin element and DOF bytes; a
    # callable folds its qualified name — the module-level-kernel identity precedent `numerics/jit.md`
    # sets (closure source is not byte-stable across runs).
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            return (origin.value.encode(), origin_dofs)
        case fn:
            return (getattr(fn, "__qualname__", repr(fn)).encode(),)



def _extent(values: np.ndarray) -> tuple[float, float]:
    return (float(np.linalg.norm(values)), float(np.max(np.abs(values)))) if values.size else (0.0, 0.0)


# `_basis` reads the one `mesh.CTOR` `(Mesh*, Element*, cell-type)` triple the assemble fold reads,
# resolving both spellings through `getattr(skfem, ...)` over the `(dim, n)`/`(verts, n_elem)`
# node-major/element-major layout skfem stores `mesh.p`/`mesh.t` in — never a second element map. The
# reject every attribute access; the three readout/project call sites pass the one deferred import.
def _basis(field: MeshField, element: ElementKind, skfem: Any) -> Any:
    mesh_ctor, element_ctor, _ = CTOR[element]
    mesh = getattr(skfem, mesh_ctor)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    return skfem.Basis(mesh, getattr(skfem, element_ctor)())


# The source element is the topology's own `field.element`, never a redundant parallel parameter; the
# receipt key is the `_dispatch`-minted RESULT key folding the mesh key, the readout row, and the DOF
# bytes. Vector/composite split: `basis.split(dofs)` is cataloged as a (subbases, subvectors)
# pair-of-lists, so `zip(*split)` yields the per-component (sub_basis, sub_dofs) pairs; a scalar
# element splits empty and the `or ((basis, dofs),)` floor reads the whole basis as one component.
def _interpolate_receipt(key: ContentKey, field: MeshField, dofs: np.ndarray, readout: ReadoutKind) -> FieldReceipt:
    element = field.element
    try:
        import skfem

        basis = _basis(field, element, skfem)
        components = tuple(zip(*basis.split(dofs))) or ((basis, dofs),)
        readouts = tuple(_extent(np.asarray(getattr(sub.interpolate(part), _READOUT[readout]))) for sub, part in components)
        extent = float(np.linalg.norm(np.asarray([norm for norm, _ in readouts])))
        peak = max((mag for _, mag in readouts), default=0.0)
        return FieldReceipt.Interpolate(key, element, readout, int(basis.N), len(readouts), extent, peak)
    except ImportError:
        extent, peak = _extent(dofs)
        return FieldReceipt.Interpolate(key, element, readout, int(dofs.size), 1, extent, peak)


# Projection rides the `basis.project` METHOD, never a phantom top-level `skfem.project`. The source
# basis is the topology's `field.element`; `target` is the genuinely-distinct transfer target. A
# callable projects directly; a cross-basis DOF vector projects via its source DiscreteField. The
# residual is a source-space round trip, the callable path reading the projected DOFs back at the
# target basis's physical DOF coordinates (`global_coordinates`) against the source callable there —
# never finiteness. The receipt key is the `_dispatch`-minted RESULT key over mesh key + target +
# source identity cells.
def _project_receipt(key: ContentKey, field: MeshField, target: ElementKind, source: ProjectSource) -> FieldReceipt:
    import skfem  # noqa: F401 — worker; project is reachable only on the FEM band.

    target_basis = _basis(field, target, skfem)
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            source_basis = _basis(field, origin, skfem)
            projected = np.asarray(target_basis.project(source_basis.interpolate(np.asarray(origin_dofs))))
            round_trip = np.asarray(source_basis.project(target_basis.interpolate(projected)))
            residual = float(np.linalg.norm(round_trip - np.asarray(origin_dofs)))
        case Callable() as fn:  # narrows the `ProjectSource` callable arm; residual is the physical-point fidelity, never a sentinel.
            projected = np.asarray(target_basis.project(fn))
            coords = np.asarray(target_basis.global_coordinates())
            residual = float(np.linalg.norm(projected - np.asarray(fn(coords))))
        case _ as unreachable:
            assert_never(unreachable)
    extent, _ = _extent(projected)
    return FieldReceipt.Project(key, target, int(target_basis.N), extent, residual)


# Regular-grid resample over the JAX-differentiable interpax interpolant. The worker `interpax`/`jax`
# modules fold through `FieldEngine.worker()` so the x64 promotion fires ONCE behind the band before the
# `Interpolator{1,2,3}D` is constructed — without it the interpolant builds at float32 and the differentiable
# `grad`/`vjp` through the sampled values degrades below double precision (the `solvers/quadrature.md#QUADRATURE`
# `QuadEngine.worker()`/`solvers/differential.md#DIFFERENTIAL` `SolveEngine.worker()` x64 contract every sibling
# JAX route carries). `engine.resample` builds the reusable interpolant ONCE outside the trace and splits the
# `(N, dim)` query into the `(dim, N)` per-axis columns through the cataloged `reshape(-1, dim).T` (never the
# is absent. The resample has no `MeshField`, so its content key is the `_key` rail `_dispatch` already
# `yield from`-bound and threads in, never re-derived.
def _resample_receipt(key: ContentKey, axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> FieldReceipt:
    dim = len(axes)
    try:
        sampled = FieldEngine.worker().resample(axes, values, query, method)
    except ImportError:
        sampled = np.interp(query, axes[0], values) if dim == 1 else np.asarray(values.ravel())
    extent, peak = _extent(sampled)
    return FieldReceipt.Resample(key, dim, int(query.size), extent, peak)


# The one field RESULT-identity mint every `_dispatch` arm binds: heterogeneous parts — inherited key
# renders, policy/enum cells as bytes, operand arrays as length-prefixed `(dtype, shape, buffer)` cell
# folds — cross length-prefixed so the concatenation-absorbing `stream` updater sees unambiguous
# boundaries. Raw value bytes alone are the deleted keying form (`solvers/mesh.md#EXCHANGE` `_field`
# law): dtype/shape join the canonical byte stream so a reshaped or re-typed operand — a `(4,)` vs
# `(2, 2)` float64, two same-width dtypes over one bit pattern — re-keys, never a second hasher.
# `ContentIdentity.of` returns `RuntimeRail[ContentKey]` (its `derived` aspect fault-fences the
# canonical-encode against `EncodeError`), so `_key` is a `@railed` `effect.result` mint that
# `yield from`-binds the key off the rail and `_dispatch` threads it onto the one `RuntimeRail`
# `evaluate` returns — never a `match`/`raise` re-raise through a phantom exception bridge.
@railed
def _key(label: str, *parts: "bytes | np.ndarray") -> ContentKey:
    def chunk(part: "bytes | np.ndarray") -> bytes:
        if isinstance(part, bytes):
            return part
        arr = np.ascontiguousarray(part)
        cells = (str(arr.dtype).encode(), repr(arr.shape).encode(), arr.tobytes())
        return b"".join(len(cell).to_bytes(8, "big") + cell for cell in cells)

    chunks = tuple(chunk(part) for part in parts)
    key: ContentKey = yield from ContentIdentity.of(label, tuple(len(c).to_bytes(8, "big") + c for c in chunks))
    return key
```
