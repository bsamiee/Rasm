# [PY_COMPUTE_FIELD]

The one finite-element-and-grid field readout owner beside the FEM assemble and solve routes. `FieldQuery` discriminates the three postprocessing operations a discretized or sampled solution admits: `interpolate` lifts a DOF vector into a `skfem.DiscreteField` and reads `value`/`grad`/`hess` under one `ReadoutKind` output axis; `project` L2-projects a callable field or a cross-basis DOF vector onto a target `ElementKind` basis through `basis.project`; `resample` evaluates a regular grid at query points through the JAX-differentiable `interpax.Interpolator{1,2,3}D` so a downstream `grad`/`vjp` flows through the sampled values.

The owner consumes the solution, never produces it: the solve stays on `solvers/quadrature.md#QUADRATURE`, the assemble on `solvers/mesh.md#EXCHANGE`. The `interpolate` and `project` cases read the same `mesh._CTOR` `(Mesh*, Element*, cell-type)` triple the assemble reads, so the discretization and its readout share one `Basis(mesh, Element*())`. The `resample` case realizes the multidimensional `interp2d`/`interp3d` consumer the 1-D `solvers/quadrature.md#QUADRATURE` route defers here per `[INTERPAX_QUADAX_USAGE]`.

`FieldReceipt` is one operation-discriminated `@tagged_union` whose `Literal` `tag` IS the operation, with its per-case payload, `.status` read, accessor projection, and observability row all driven by one `_SLOTS` table — the `solvers/receipt.md#RECEIPT` `SolverReceipt` and `solvers/mesh.md#EXCHANGE` `MeshReceipt` discipline — each case terminating in the shared `SolveStatus` verdict the receipt floor adjudicates. The measured kernel wears the runtime `@receipted` aspect; `contribute` stays the plain `ReceiptContributor` projection the aspect harvests.

`scikit-fem` and `interpax` carry no cp315 wheel, so the FEM eval/project fold and the 2-D/3-D resample ride the gated `python_version<'3.15'` band; the numpy nodal-readout floor and the `np.interp` 1-D resample floor run unconditionally on cp315.

## [01]-[INDEX]

- [01]-[FIELD]: the interpolate/project/resample field operations, the `DiscreteField` `value`/`grad`/`hess` readout with vector/composite split, the `basis.project` L2 transfer with physical-point round-trip residual, and the `interpax` differentiable grid resample, folded through the shared `SolveStatus` floor on one `FieldQuery` owner and one `_SLOTS`-driven `FieldReceipt`.

## [02]-[FIELD]

- Owner: `FieldQuery` — the ONE `@tagged_union` field-postprocessing owner discriminating `interpolate` (DOF vector → `skfem.DiscreteField` `value`/`grad`/`hess` under one `ReadoutKind`, vector/composite split), `project` (callable or cross-basis field → target-basis DOF vector through `basis.project`), and `resample` (regular grid → query-point evaluation through `interpax.Interpolator{1,2,3}D`). The `interpolate` case reads its source element off the `MeshField.element` the topology already owns rather than a parallel element parameter; the `project` case carries the `MeshField`, the genuinely-distinct target `ElementKind`, and the `ProjectSource` (a `FieldFn` over physical points or a `(ElementKind, np.ndarray)` cross-basis DOF pair); the `resample` case carries the `GridAxes`, the gridded values, the query points, and the bounded `ResampleMethod` kernel. One owner spans the nodal-quadrature readout, the basis-to-basis transfer, and the grid resample.
- Output axis: `ReadoutKind` (`VALUE`/`GRAD`/`HESS`) is the ONE bounded `DiscreteField`-readout policy, keyed through `_READOUT` onto the `DiscreteField` attribute so a value readout, a gradient (flux-recovery) readout, and a Hessian readout are one policy row on the `interpolate` case rather than three parallel `Interpolate`/`Gradient`/`Hessian` entries. The cataloged `DiscreteField` carries `value`/`grad`/`hess` directly, so the `interpolate` case is parameterized over its OUTPUT shape, not only its input, and the flux-recovery/Hessian readouts are a `_READOUT` projection rather than a future case.
- Element axis: `_CTOR` is the ONE `FrozenDict[ElementKind, tuple[str, str, str]]` `(Mesh*, Element*, cell-type)` triple owned by `solvers/mesh.md#MESH_FIELD`; `ElementKind` is owned by `solvers/quadrature.md#QUADRATURE`. This owner imports both rather than redeclaring a constructor map, and `_basis` reads `mesh_ctor, element_ctor, _ = _CTOR[element]` then resolves both spellings through `getattr(skfem, ...)` at evaluation time — the identical row the assemble reads, so the element vocabulary never diverges across assemble, solve, and readout and a new element is one shared `_CTOR` row. A field interpolated over a `TRI_P1` `MeshField` reads the same `Basis(MeshTri1(points.T, cells.T), ElementTriP1())` the assemble built.
- Interpolate: the interpolate case builds the source basis over the `MeshField` topology for the topology's own `field.element` (never a redundant element parameter beside the value object that owns it), calls `basis.interpolate(dofs)` to lift the DOF vector into a `skfem.DiscreteField` sampled at the element quadrature points, and reads the `_READOUT[kind]` attribute (`value`/`grad`/`hess`); for a vector or composite element it folds `basis.split(dofs)` — which the catalog returns as a `(subbases, subvectors)` pair-of-lists — into per-component sub-bases through `zip(*basis.split(dofs))` and re-interpolates each so a vector displacement or a mixed velocity/pressure field reports the component count and the worst-component peak rather than collapsing every axis into one scalar. A scalar element yields one component, the `or ((basis, dofs),)` floor covering a split that returns empty for a non-vector space. The evaluation extent is the readout-array L2 norm; the peak is the per-axis max absolute magnitude; the status is the residual-floor verdict over the extent against the interpolate tolerance, the evidence a downstream gate reads to admit a field for kernel handoff. The readout never solves and never assembles.
- Project: the project case folds through the `basis.project` METHOD — the catalog's L2-projection entry `basis.project(interp, elements=None)`, never a phantom top-level `skfem.project(basis_from=, basis_to=)`. A callable source projects directly (`target_basis.project(fun)`); a cross-basis source lifts its source-basis DOF vector to a `DiscreteField` and projects that onto the target (`target_basis.project(source_basis.interpolate(source_dofs))`), returning the target DOF vector. The cross-basis transfer residual is a source-space round trip: the projected vector is interpolated on the target basis, re-projected onto the source basis (`source_basis.project(target_basis.interpolate(projected))`), and the L2 norm taken against the original source DOFs, so a P1→P2→P1 round-trip reports the lost information in a single comparison space rather than subtracting `DiscreteField` value arrays at incompatible quadrature points. The callable path has no source DOF vector, so its residual is the physical-point transfer fidelity the `compute/.api/scikit-fem.md` `[LOCAL_ADMISSION]` mandates — the projected DOFs read back at `target_basis.global_coordinates()` (the `doflocs` surface) against the source callable evaluated there, never a finiteness-only sentinel. The residual feeds the shared `SolveStatus` floor, so the transfer fidelity is a first-class termination verdict carried in the project case rather than a `SolverReceipt` convergence verdict, because a projection is not a solve.
- Resample: the resample case dispatches on the coordinate-tuple arity through the module-level `_INTERPOLATOR` table, whose `int`-keyed `lambda ix: ix.Interpolator{1,2,3}D` rows resolve the gated constructor only when called with the imported module inside the gated body — the lambda-deferred discipline `solvers/quadrature.md#QUADRATURE` `_QUADAX_ENTRY` runs, so the table is a cp315-clean constant rather than a body-local re-bind. The selected `Interpolator{1,2,3}D` is constructed ONCE outside the trace (the catalog's preferred reusable form over the one-shot `interp{1,2,3}d` recompiled per call) and evaluated at the query points with the bounded `ResampleMethod` kernel, never a free `str`. Because every `Interpolator{1,2,3}D` is a JAX pytree module, the resampled field is `vmap`/`grad`/`jit`-compatible with respect to BOTH the query points and the sampled values, so `solvers/sensitivity.md#SENSITIVITY` and `optimization/design.md#DESIGN` read an autodifferentiable resampled field through this case. The numpy floor serves the 1-D arity through `np.interp`; the 2-D/3-D arities are gated-only because no in-numpy regular-grid interpolant matches the differentiable `interpax` kernel.
- Entry: `FieldQuery.evaluate` enters one `boundary(f"field.{query.tag}", ...)` returning `RuntimeRail[FieldReceipt]`; the match dispatches the three cases through total `assert_never` exhaustion, the interpolate arm folding the readout array and split components into the `interpolate` case, the project arm folding the target DOF vector and the round-trip transfer residual into the `project` case, and the resample arm folding the query-point evaluation into the `resample` case. The interpolate numpy floor reads the DOF vector norm and peak at the mesh nodes directly when `skfem` is unavailable, passing `result=None` to the shared status floor so the interpolate extent stays reachable on cp315 without the gated basis, the verdict adjudicated by the residual-against-tolerance floor identical to the quadrature numpy floors; the project case has no numpy floor because the L2 basis-to-basis transfer is owned by gated `skfem` with no in-numpy equivalent, so projection is reachable only on the gated band, and the resample 2-D/3-D arities likewise.
- Receipt: `FieldReceipt` is the ONE `@tagged_union` field receipt whose `Literal` `tag` IS the operation. One `_SLOTS` table names each operation's field sequence — `interpolate → (key, element, readout, dof_count, components, extent, peak, status)`, `project → (key, element, dof_count, extent, residual, status)`, `resample → (key, dim, query_count, extent, peak, status)`, `key` the common leading slot and `status` the common trailing slot — and drives the structural shape, the `.status` trailing-slot read through one `case (*_, status)` closed by `assert_never`, the `.content_key`/`.element` accessors off `.facts`, and the `.facts` projection through `dict(zip(_SLOTS[self.tag], payload, strict=True))`, so the table and the case tuples cannot drift. The four `@classmethod` factories returning `Self` fold their extent or residual through the shared `_status(None, value, _TOL[op])` floor. `FieldReceipt.contribute` satisfies the `ReceiptContributor` port structurally, returning the one-element `Iterable[Receipt]` `Receipt.of("compute.field", ("emitted", subject, facts))` over the runtime two-argument `(owner, (Phase, subject, facts))` contract, carrying the operation tag, the derived `converged` flag, and the spread of `.facts` riding as native scalars through the runtime `Signals` `msgspec` encoder, keyed by the content key. An evaluated, projected, or resampled field graduating outward routes through `graduation/handoff.md#GRADUATION` on the `solver` `HandoffAxis` case, the residual the graduation fold reads being the projection residual this receipt carries.
- Packages: `skfem` (`Basis`, `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1`, `MeshLine1`/`MeshTri1`/`MeshTet1`/`MeshQuad1`/`MeshHex1`; `basis.interpolate`, `basis.split`, `basis.project`, `basis.zeros`, `basis.global_coordinates`, `basis.probes`, `DiscreteField.value`/`.grad`/`.hess` — the projection is the `basis.project` METHOD, never a top-level `skfem.project`), `interpax` (`Interpolator1D`/`Interpolator2D`/`Interpolator3D` — the JAX-differentiable reusable grid interpolants the resample case constructs once), `numpy` (`asarray`, `ascontiguousarray`, `abs`, `max`, `ravel`, `concatenate`, `atleast_2d`, `interp`, `linalg.norm`), `expression` (`tag`, `case`, `tagged_union`, `Ok`, `Error` the `RuntimeRail` match arms in `_key`, `expression.collections.Map` the empty persistent `_REDACTION` map), `solvers/quadrature.md#QUADRATURE` (`ElementKind` — the shared element axis enum), `solvers/mesh.md#MESH_FIELD` (`MeshField` the topology and DOF source the readout consumes, `_CTOR` the ONE `(Mesh*, Element*, cell-type)` triple table the mesh owner consolidated), `solvers/receipt.md#RECEIPT` (`SolveStatus`, `_status` — the shared termination vocabulary and residual-floor verdict the field receipt folds into), runtime (`RuntimeRail`, `boundary`, `Receipt`/`Redaction`/`receipted` — `Receipt` the lone receipt type the `ReceiptContributor` port is satisfied against structurally, `Redaction` the empty-`classified` policy the `@receipted` aspect binds, `receipted` the aspect the measured kernel wears, plus `ContentIdentity`/`ContentKey`/`IdentityPolicy` the resample key fold). The cross-module private `_CTOR` import is the same reuse `solvers/mesh.md#MESH_FIELD` runs, never the parallel `_ELEMENT_CTOR`/`_MESH_CTOR` pair the prior corpus split.
- Growth: a new element is one `_CTOR` row shared with the assemble and solve routes; a new readout (gradient already `ReadoutKind.GRAD`, Hessian already `ReadoutKind.HESS`) is one `ReadoutKind` row plus one `_READOUT` entry on the existing interpolate case; a new field operation (flux recovery as a functional, adaptive probing) is one `FieldQuery` case plus one `_SLOTS` row sharing the basis-construction fold and the status floor; a new resample arity beyond 3-D is one `_INTERPOLATOR` row; a new resample kernel is one `ResampleMethod` member; a new readout statistic is one slot on the owning `_SLOTS` row; a new termination class is one `SolveStatus` member shared with every solver route; zero new surface, never a parallel evaluation container beside the `DiscreteField`, never a second projection entry, never a separate multidimensional-interpolation owner, never a flat receipt with a stringly `operation` field or a sentinel residual, and never a solve or assemble on this owner.
- Boundary: field evaluation, projection, and grid resample only — the `DiscreteField` quadrature readout, the vector/composite split, the `basis.project` transfer with physical-point residual, and the `interpax` grid resample are in-scope; the assemble stays on `solvers/mesh.md#MESH_FIELD`, the solve on `solvers/quadrature.md#QUADRATURE`, and the columnar/gridded statistical aggregation of the evaluated field stays in the `data` branch, so this owner reports an in-memory extent and residual and never aggregates a field across a grid. The deleted forms: a solve on this owner; a hand-rolled interpolation loop where `basis.interpolate`/`basis.project`/`interpax.Interpolator` own the concern; a phantom top-level `skfem.project(basis_from=, basis_to=)` where the projection is the `basis.project` method; a finiteness-only `0.0`/`inf` callable residual where the catalog mandates the `global_coordinates`/`probes` physical-point readout; a `SolverReceipt` minted for a projection; a free-`str` resample `method` where `ResampleMethod` bounds the interpax kernel; a non-total `project` `match` lacking the `assert_never` callable-arm closure; and a second local element-constructor table where the one `_CTOR` triple is imported. The mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python signature
# --- [TYPES] -------------------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import Literal, Self, assert_never

import numpy as np
from beartype import FrozenDict
from expression import Error, Ok, case, tag, tagged_union
from expression.collections import Map

from rasm.compute.solvers.mesh import MeshField, _CTOR
from rasm.compute.solvers.quadrature import ElementKind
from rasm.compute.solvers.receipt import SolveStatus, _status
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted


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

# The element/basis axis is the imported `mesh._CTOR` `(Mesh*, Element*, cell-type)` triple keyed by
# `ElementKind`; `_basis` reads the same row the assemble fold reads and resolves both spellings
# through `getattr(skfem, ...)`, so the assemble, the FEM solve, and this readout share ONE constructor
# table and this owner declares no second element-spelling map.

# Field-redaction policy the `@receipted` aspect binds; the field facts carry native scalars and a
# content key, no secret, so the classified map is the empty persistent `Map` exactly as the mesh
# route's `_REDACTION` — never a mutable `{}` dict the FP rail rejects for domain state.
_REDACTION: Redaction = Redaction(classified=Map.empty())

# ReadoutKind -> DiscreteField attribute. The interpolate readout is parameterized over its output:
# the value array, the recovered gradient (flux), or the Hessian, all cataloged on DiscreteField.
_READOUT: FrozenDict[ReadoutKind, str] = FrozenDict(
    {ReadoutKind.VALUE: "value", ReadoutKind.GRAD: "grad", ReadoutKind.HESS: "hess"}
)

# Per-operation residual-floor tolerance, one row per FieldOp; the resample row floors the resample
# extent verdict where no transfer residual exists. A new operation tolerance is one row.
_TOL: FrozenDict[FieldOp, float] = FrozenDict({"interpolate": 1e-6, "project": 1e-6, "resample": 1e-6})

# Coordinate-tuple arity -> the `interpax.Interpolator{1,2,3}D` constructor, keyed by the cp315-clean
# `int` arity so the table is a module-level constant; each row is a `lambda ix: ix.Interpolator*D`
# resolving the gated symbol only when called with the imported module inside the resample body —
# the same module-level lambda-deferred discipline `solvers/quadrature.md#QUADRATURE` `_QUADAX_ENTRY`
# runs, never a body-local table re-bound per call. A new arity is one row.
_INTERPOLATOR: FrozenDict[int, Callable[[object], Callable[..., object]]] = FrozenDict(
    {1: lambda ix: ix.Interpolator1D, 2: lambda ix: ix.Interpolator2D, 3: lambda ix: ix.Interpolator3D}
)

# Per-operation payload field names, one tuple per tag, `key` the common leading slot and `status` the
# common trailing slot. The single owner over the case shapes: the factory packs by it, the accessors
# read fixed slots off it, `.status` reads the last slot, and `.facts` projects each named slot —
# mirroring solvers/receipt.md#RECEIPT `_SLOTS`, so an operation's evidence is one row.
_SLOTS: FrozenDict[FieldOp, tuple[str, ...]] = FrozenDict(
    {
        "interpolate": ("key", "element", "readout", "dof_count", "components", "extent", "peak", "status"),
        "project": ("key", "element", "dof_count", "extent", "residual", "status"),
        "resample": ("key", "dim", "query_count", "extent", "peak", "status"),
    }
)


# --- [MODELS] ------------------------------------------------------------------------------

@tagged_union(frozen=True)
class FieldReceipt:
    tag: FieldOp = tag()
    interpolate: tuple[ContentKey, ElementKind, ReadoutKind, int, int, float, float, SolveStatus] = case()
    project: tuple[ContentKey, ElementKind, int, float, float, SolveStatus] = case()
    resample: tuple[ContentKey, int, int, float, float, SolveStatus] = case()

    @classmethod
    def Interpolate(
        cls, key: ContentKey, element: ElementKind, readout: ReadoutKind,
        dof_count: int, components: int, extent: float, peak: float,
    ) -> Self:
        return cls(interpolate=(key, element, readout, dof_count, components, extent, peak, _status(None, extent, _TOL["interpolate"])))

    @classmethod
    def Project(cls, key: ContentKey, element: ElementKind, dof_count: int, extent: float, residual: float) -> Self:
        return cls(project=(key, element, dof_count, extent, residual, _status(None, residual, _TOL["project"])))

    @classmethod
    def Resample(cls, key: ContentKey, dim: int, query_count: int, extent: float, peak: float) -> Self:
        return cls(resample=(key, dim, query_count, extent, peak, _status(None, extent, _TOL["resample"])))

    @property
    def facts(self) -> dict[str, object]:
        return dict(zip(_SLOTS[self.tag], getattr(self, self.tag), strict=True))

    @property
    def content_key(self) -> ContentKey:
        return self.facts["key"]

    @property
    def element(self) -> ElementKind | None:
        return self.facts.get("element")

    @property
    def status(self) -> SolveStatus:
        match getattr(self, self.tag):
            case (*_, SolveStatus() as status):
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

    def evaluate(self) -> RuntimeRail[FieldReceipt]:
        return boundary(f"field.{self.tag}", lambda: _dispatch(self))


# --- [OPERATIONS] --------------------------------------------------------------------------

# `@receipted(_REDACTION)` wraps the measured readout kernel that returns the `FieldReceipt`
# contributor and emits its `.contribute()` stream on exit; `evaluate` fences it in one `boundary` so
# the aspect fires on the contributor while a skfem/interpax raise still rails. The aspect decorates
# this kernel — never the receipt's own `contribute` — exactly as the mesh route's `_dispatch`.
@receipted(_REDACTION)
def _dispatch(query: FieldQuery) -> FieldReceipt:
    match query:
        case FieldQuery(tag="interpolate", interpolate=(field, dofs, readout)):
            return _interpolate_receipt(field, np.asarray(dofs), readout)
        case FieldQuery(tag="project", project=(field, target, source)):
            return _project_receipt(field, target, source)
        case FieldQuery(tag="resample", resample=(axes, values, query, method)):
            return _resample_receipt(axes, np.asarray(values), np.asarray(query), method)
        case _ as unreachable:
            assert_never(unreachable)


def _extent(values: np.ndarray) -> tuple[float, float]:
    return (float(np.linalg.norm(values)), float(np.max(np.abs(values)))) if values.size else (0.0, 0.0)


# `_basis` reads the one `mesh._CTOR` `(Mesh*, Element*, cell-type)` triple the assemble fold reads,
# resolving both spellings through `getattr(skfem, ...)` over the `(dim, n)`/`(verts, n_elem)`
# node-major/element-major layout skfem stores `mesh.p`/`mesh.t` in — never a second element map.
def _basis(field: MeshField, element: ElementKind, skfem: object) -> object:
    mesh_ctor, element_ctor, _ = _CTOR[element]
    mesh = getattr(skfem, mesh_ctor)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    return skfem.Basis(mesh, getattr(skfem, element_ctor)())


# The source element is the topology's own `field.element`, never a redundant parallel parameter, and
# the receipt reuses the `field.content_key` the mesh owner already minted rather than re-digesting the
# buffers. Vector/composite split: `basis.split(dofs)` is cataloged as a (subbases, subvectors)
# pair-of-lists, so `zip(*split)` yields the per-component (sub_basis, sub_dofs) pairs; a scalar
# element splits empty and the `or ((basis, dofs),)` floor reads the whole basis as one component.
def _interpolate_receipt(field: MeshField, dofs: np.ndarray, readout: ReadoutKind) -> FieldReceipt:
    element = field.element
    try:
        import skfem

        basis = _basis(field, element, skfem)
        components = tuple(zip(*basis.split(dofs))) or ((basis, dofs),)
        readouts = tuple(_extent(np.asarray(getattr(sub.interpolate(part), _READOUT[readout]))) for sub, part in components)
        extent = float(np.linalg.norm(np.asarray([norm for norm, _ in readouts])))
        peak = max((mag for _, mag in readouts), default=0.0)
        return FieldReceipt.Interpolate(field.content_key, element, readout, _ndofs(basis), len(readouts), extent, peak)
    except ImportError:
        extent, peak = _extent(dofs)
        return FieldReceipt.Interpolate(field.content_key, element, readout, int(dofs.size), 1, extent, peak)


# Projection rides the `basis.project` METHOD, never a phantom top-level `skfem.project`. The source
# basis is the topology's `field.element`; `target` is the genuinely-distinct transfer target. A
# callable projects directly; a cross-basis DOF vector projects via its source DiscreteField. The
# residual is a source-space round trip, the callable path reading the projected DOFs back at the
# target basis's physical DOF coordinates (`global_coordinates`) against the source callable there —
# never finiteness. The receipt reuses `field.content_key`, never a re-digested buffer pair.
def _project_receipt(field: MeshField, target: ElementKind, source: ProjectSource) -> FieldReceipt:
    import skfem  # noqa: F401 — gated; project is reachable only on the FEM band.

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
    return FieldReceipt.Project(field.content_key, target, _ndofs(target_basis), extent, residual)


# Regular-grid resample over the JAX-differentiable interpax interpolant, constructed once outside the
# trace (the cataloged reusable form). The arity of the coordinate tuple selects 1-D/2-D/3-D; the
# numpy floor serves 1-D through np.interp on the bare cp315 core. The resample has no `MeshField`, so
# it folds its own content key over the stacked axes and gridded values through the matched
# `ContentIdentity.of` rail — an `Ok` binds the key, an `Error` re-raises for the enclosing `boundary`.
def _resample_receipt(axes: GridAxes, values: np.ndarray, query: np.ndarray, method: ResampleMethod) -> FieldReceipt:
    key = _key("field-resample", np.concatenate([np.ravel(a) for a in axes]), values)
    dim = len(axes)
    try:
        import interpax

        # `_INTERPOLATOR[dim](interpax)` resolves the gated `Interpolator{1,2,3}D` constructor off the
        # module-level lambda-deferred table, the JAX-differentiable reusable interpolant built ONCE
        # outside the trace; the per-axis query columns evaluate it for dim>1, the raw query for 1-D.
        interpolant = _INTERPOLATOR[dim](interpax)(*axes, values, method=method)
        sampled = np.asarray(interpolant(*np.atleast_2d(query).T)) if dim > 1 else np.asarray(interpolant(query))
    except ImportError:
        sampled = np.interp(query, axes[0], values) if dim == 1 else np.asarray(values.ravel())
    extent, peak = _extent(sampled)
    return FieldReceipt.Resample(key, dim, int(query.size), extent, peak)


def _ndofs(basis: object) -> int:
    return int(np.asarray(basis.zeros()).size)


# The resample content-key fold over the stacked grid axes and gridded values — the only case without a
# `MeshField` to inherit `content_key` from. `ContentIdentity.of` returns `RuntimeRail[ContentKey]`
# (the canonical-encode seam fault-rails an `EncodeError`), so the key is matched, NOT read as a bare
# value; the `stream` modality keys the order-sensitive fold rather than a hand-rolled byte join.
def _key(label: str, lead: np.ndarray, values: np.ndarray) -> ContentKey:
    buffers = (np.ascontiguousarray(lead).tobytes(), np.ascontiguousarray(values).tobytes())
    match ContentIdentity.of(label, buffers, IdentityPolicy()):
        case Ok(key):
            return key
        case Error(fault):
            raise fault.as_exception()
```

## [03]-[RESEARCH]

- [SKFEM_FIELD_EVAL]: the `skfem.Basis(mesh, element)`, `basis.interpolate(dofs)` → `DiscreteField`, `basis.split(dofs)`, the `basis.zeros()`-sized dof-count, and the `DiscreteField` `value`/`grad`/`hess` readout verify against `compute/.api/scikit-fem.md` (`DiscreteField` is the cataloged field container carrying `value`/`grad`/`hess`; `basis.interpolate`/`basis.split`/`basis.zeros` are the cataloged postprocess and DOF-vector entries; the dof count reads `basis.zeros().size`). `basis.split(dofs)` returns the `(subbases, subvectors)` pair-of-lists for an `ElementVector`/`ElementVectorH1`/`ElementComposite` space, so `zip(*basis.split(dofs))` yields the per-component `(sub_basis, sub_dofs)` pairs the interpolate fold re-interpolates, reporting the component count and worst-component peak; a scalar element yields an empty split, the `or ((basis, dofs),)` floor reading the whole basis. The `ReadoutKind` axis reads `value`/`grad`/`hess` off the `DiscreteField` through `_READOUT`, so the gradient (flux-recovery) and Hessian readouts are an output-policy row on the one interpolate case rather than parallel entries. The `Mesh(points.T, cells.T)` node-major/element-major transpose matches the `solvers/mesh.md#MESH_FIELD` assemble layout (`skfem` stores `mesh.p` as `(dim, n)` and `mesh.t` as `(verts, n_elem)`). `scikit-fem` carries no cp315 wheel, so the eval fold rides the gated `python_version<'3.15'` band; the numpy nodal floor reads the DOF vector norm and peak directly and runs unconditionally.
- [SKFEM_PROJECT]: projection is the `basis.project(interp, elements=None)` METHOD, not a top-level `skfem.project` — `compute/.api/scikit-fem.md` catalogs `basis.project` as the L2-projection entry ([ENTRYPOINTS] basis row [07]) and `[LOCAL_ADMISSION]` confirms "a callable source/Dirichlet field projects with `basis.project(fun)`". The callable source passes `target_basis.project(fun)`; the cross-basis source has no top-level `basis_from=/basis_to=` route, so it lifts the source DOFs to a `DiscreteField` on the source basis and projects that field onto the target (`target_basis.project(source_basis.interpolate(dofs))`), the documented composition for basis-to-basis transfer. The cross-basis residual is computed in the source DOF space by re-projecting the target field back through the source basis (`source_basis.project(target_basis.interpolate(projected))`) and taking the L2 norm against the original source DOFs — a single-space comparison that avoids subtracting `DiscreteField.value` arrays at incompatible quadrature orders. The callable residual is the physical-point transfer fidelity `[LOCAL_ADMISSION]` mandates: the projected DOF vector is read at the target basis's physical DOF coordinates through `basis.global_coordinates()` (the `doflocs`/`probes` physical-coordinate surface) and compared against the source callable evaluated there, NOT a finiteness-only `0.0`/`inf` check, which the catalog explicitly rejects ("physical-point transfer fidelity uses `basis.probes(x)`/`global_coordinates()`/`doflocs` (not finiteness-only checks), feeding the `solvers/field.md#FIELD` transfer-residual receipt"). The residual feeds the shared `_status` floor rather than a solver convergence verdict.
- [INTERPAX_RESAMPLE]: the resample case realizes the `[INTERPAX_QUADAX_USAGE]` deferred consumer the `compute/.api/interpax.md` catalog records for `solvers/field` and the `solvers/quadrature.md#QUADRATURE` route defers ("the `interpax` `interp2d`/`interp3d` family serves on `solvers/field`"). The `interpax.Interpolator1D(x, f, method)`/`Interpolator2D(x, y, f, method)`/`Interpolator3D(x, y, z, f, method)` reusable interpolant objects verify against `compute/.api/interpax.md` — JAX pytree modules constructed ONCE and then called with query points (the catalog's preferred reusable form over the one-shot `interp{1,2,3}d` recompiled per call), `vmap`/`grad`/`jit`-compatible with respect to both the query points and the sampled values, so the resampled field is autodifferentiable for `solvers/sensitivity.md#SENSITIVITY` and `optimization/design.md#DESIGN`. The module-level `_INTERPOLATOR` `FrozenDict[int, Callable[[object], Callable[..., object]]]` keys the coordinate-tuple arity onto a `lambda ix: ix.Interpolator{1,2,3}D` row that resolves the gated constructor only when called with the imported `interpax` module inside the resample body — the cp315-clean module-level lambda-deferred discipline the quadrature `_QUADAX_ENTRY` `FrozenDict[QuadKind, Callable[[object], ...]]` runs, never a body-local table re-bound per call — and the `ResampleMethod` `Literal` (`"nearest"`/`"linear"`/`"cubic"`/`"cubic2"`/`"catmull-rom"`/`"monotonic"`/`"monotonic-0"`) is the bounded interpax kernel vocabulary the catalog records, never a free `str`. `interpax` (jaxlib) carries no cp315 wheel, so the 2-D/3-D resample rides the gated band; the 1-D arity floors to `np.interp` on the bare cp315 core, the multidimensional arities being gated-only because no in-numpy regular-grid interpolant matches the differentiable kernel.
- [SHARED_RECEIPT_FOLD]: `FieldReceipt` mirrors `solvers/receipt.md#RECEIPT` `SolverReceipt` end to end — the `Literal` `tag` IS the operation, the per-case shape is driven by one `_SLOTS` `FrozenDict[FieldOp, tuple[str, ...]]` rather than hand-spelled three times, the four `@classmethod` factories return `Self` (never a `@staticmethod`-plus-forward-ref the receipt owner deletes), `.status` reads the trailing slot through one `case (*_, status)` pattern closed by `assert_never`, `.content_key`/`.element` read fixed slots off `_SLOTS` through `.facts` rather than parallel `getattr(self, self.tag)[N]` properties, and `.facts` zips `_SLOTS[self.tag]` against the case tuple under `strict=True` so the table and the case shapes cannot drift. The factories fold the extent or residual through the shared `_status(None, value, _TOL[op])` floor imported from the receipt owner — the same cross-module private import the solver routes run to reuse the receipt vocabulary — so the field receipt reuses the one termination vocabulary every solver route folds into. `_TOL` is the one frozen per-operation tolerance table, a new operation one row. `FieldReceipt.contribute` satisfies the `ReceiptContributor` port structurally and returns the one-element `Iterable[Receipt]` through the runtime two-argument `Receipt.of("compute.field", ("emitted", subject, facts))` contract — never the four-positional form the runtime owner deletes and never a bare `Receipt` — the facts riding as native scalars through the runtime `Signals` `msgspec` encoder; the measured readout kernel wears the `@receipted` aspect so receipt production is a decorator rail rather than an inline `Signals.emit`, identical to the measured solver kernels, while `contribute` stays the plain projection the aspect harvests.
- [SHARED_ELEMENT_AXIS]: `ElementKind` is owned by `solvers/quadrature.md#QUADRATURE`; the one `_CTOR` `FrozenDict[ElementKind, tuple[str, str, str]]` `(Mesh*, Element*, cell-type)` triple table and the `MeshField` topology by `solvers/mesh.md#MESH_FIELD`. This owner imports both rather than redeclaring a constructor map, so the element vocabulary and the mesh shape never diverge between the assemble fold, the FEM solve, and the field readout. `_basis` reads `mesh_ctor, element_ctor, _ = _CTOR[element]` and resolves `getattr(skfem, mesh_ctor)`/`getattr(skfem, element_ctor)` at evaluation time, keeping the gated import behind the boundary, exactly as `solvers/mesh.md#MESH_FIELD` resolves the same triple in its `_assemble`. The single `_CTOR` triple is the canonical form `solvers/mesh.md#MESH_FIELD` `[SHARED_ELEMENT_AXIS]` names — it collapsed the prior parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps the corpus split, so this readout resolves the identical row the assemble and solve resolve and a new element is one shared `_CTOR` row read by every route.
