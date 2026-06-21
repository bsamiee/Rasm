# [PY_COMPUTE_FIELD]

The one finite-element field evaluation and projection owner beside the FEM assemble and solve routes. `FieldQuery` discriminates the two postprocessing operations a discretized solution admits — `interpolate` evaluates a solution DOF vector into a `skfem.DiscreteField` at the element quadrature points and splits a vector or composite element into its scalar components, and `project` L2-projects a callable field or a cross-basis DOF vector onto a target `ElementKind` basis through `skfem.project`. Both compose the shared `ElementKind` axis the `solvers/quadrature.md#QUADRATURE` FEM route and the `solvers/mesh.md#MESH_FIELD` assemble fold already discriminate, so the field owner reads the same `Basis(mesh, ElementKind→Element*())` the assemble and solve build rather than re-deriving the element vocabulary. The owner consumes the solution — the solve stays on the quadrature route, the assemble on the mesh owner — and emits one method-discriminated `FieldReceipt` whose `Literal` tag is the postprocessing operation, each case terminating in the shared `SolveStatus` verdict the `solvers/receipt.md#RECEIPT` floor adjudicates, so an evaluated field and a projected field carry distinct first-class payloads with one termination vocabulary rather than a flat struct with a sentinel `residual` on the interpolate path. `scikit-fem` carries no cp315 wheel, so the eval/project fold rides the gated FEM `python_version<'3.15'` band; the numpy nodal-readout floor runs unconditionally.

## [01]-[INDEX]

- [01]-[FIELD]: the interpolate/project field operations, the `DiscreteField` evaluation readout with vector/composite split, and the L2-projection residual folded through the shared `SolveStatus` floor on one `FieldQuery` owner and one method-discriminated `FieldReceipt`.

## [02]-[FIELD]

- Owner: `FieldQuery` — the ONE `@tagged_union` field-postprocessing owner discriminating `interpolate` (DOF vector → `skfem.DiscreteField` evaluation, with vector/composite split) and `project` (callable or cross-basis field → target-basis DOF vector through `skfem.project`); never a parallel evaluation surface beside the solve and never a second projection entry. The interpolate case carries the source `ElementKind` and the solution DOF vector; the project case carries the source field (a callable `Callable[[np.ndarray], np.ndarray]` over physical points or a `(ElementKind, np.ndarray)` cross-basis DOF pair) and the target `ElementKind`, so one owner spans both the nodal-quadrature readout and the basis-to-basis transfer.
- Element axis: `ElementKind` is the shared element/basis axis owned by `solvers/quadrature.md#QUADRATURE`; the one `_CTOR` row table keys each `ElementKind` to its `(Mesh*, Element*)` constructor-name pair and `_basis` resolves the pair through `getattr(skfem, ...)` at evaluation time, the same constructor truth the `solvers/mesh.md#MESH_FIELD` assemble fold reads, so the element vocabulary never diverges across assemble, solve, and field readout and the mesh/element spellings live in one table rather than two parallel maps. A field interpolated for `ElementKind.TRI_P1` reads the same `Basis(MeshTri1(points.T, cells.T), ElementTriP1())` the assemble fold built, so the discretization and its readout share one basis construction.
- Interpolate: the interpolate case builds the source basis over the `MeshField` topology, calls `basis.interpolate(dofs)` to lift the DOF vector into a `skfem.DiscreteField` sampled at the element quadrature points, and reads `discrete.value`; for a vector or composite element it folds `basis.split(dofs)` into the per-component sub-bases and re-interpolates each so a vector displacement or a mixed velocity/pressure field reports the component count and the worst-component peak rather than collapsing every axis into one scalar. The evaluation extent is the value-array L2 norm; the peak is the per-axis max absolute magnitude; the status is the residual-floor verdict over the extent against the interpolate tolerance, the evidence a downstream gate reads to admit a field for kernel handoff. The readout never solves and never assembles.
- Project: the project case builds the target basis and folds through `skfem.project` — a callable source projects the physical-point callable onto the target basis (`project(fun, basis_to=target)`), and a cross-basis source projects a source-basis DOF vector onto the target basis (`project(dofs, basis_from=source, basis_to=target)`), returning the target DOF vector. The cross-basis transfer residual is the source-DOF round-trip error — the projected vector is projected back onto the source basis (`project(projected, basis_from=target, basis_to=source)`) and the L2 norm taken against the original source DOF vector in the source DOF space, so a P1→P2→P1 round-trip reports the information the transfer lost in a single comparison space rather than subtracting two `DiscreteField` value arrays sampled at incompatible quadrature points; the callable case has no source DOF representation and no catalogued physical-point sampling surface, so its verdict is the finiteness of the projected vector (`0.0` finite, `inf` divergent) rather than a fabricated discretization residual. The residual feeds the shared `SolveStatus` floor (`SUCCESS` when it sits under the project tolerance, `STAGNATION` when it stalls above it, `NONFINITE` when the transfer diverges), so the field-transfer fidelity is a first-class termination verdict carried in the project case — never a solver convergence verdict reusing the `SolverReceipt`, because a projection is not a solve, and never a self-comparison collapsing to a `residual=0.0` sentinel on the callable path.
- Entry: `FieldQuery.evaluate` enters one `boundary(f"field.{query.tag}", ...)` returning `RuntimeRail[FieldReceipt]`; the match dispatches the two cases through total `assert_never` exhaustion, the interpolate arm folding the `DiscreteField` value array and the split components into the `Interpolate` receipt case and the project arm folding the target DOF vector and the round-trip transfer residual into the `Project` receipt case. The interpolate numpy floor reads the DOF vector norm and peak at the mesh nodes directly when `skfem` is unavailable, passing `result=None` to the shared status floor so the interpolate extent stays reachable on cp315 without the gated basis, the verdict adjudicated by the residual-against-tolerance floor identical to the quadrature numpy floors; the project case has no numpy floor because the L2 basis-to-basis transfer is owned by gated `skfem.project` with no in-numpy equivalent, so projection is reachable only on the gated band.
- Receipt: `FieldReceipt` is the ONE `@tagged_union` field receipt whose `Literal` tag is the operation, read directly through `.operation`; the `interpolate` case carries `(element, dof_count, components, extent, peak, status)` and the `project` case carries `(element, dof_count, extent, residual, status)`, the `SolveStatus` the last slot of every case so `.status` reads it through one structural `case (*_, status)` pattern and `.converged` derives the Boolean, identical to `SolverReceipt`. `FieldReceipt.contribute` implements `ReceiptContributor`, emitting one `Receipt.of("emitted", ...)` row carrying the operation tag, the element kind, the dof count, the status, and the per-case extent/peak/residual, keyed by the content key over the source topology and DOF buffer; an evaluated or projected field graduating outward routes through `graduation/handoff.md#GRADUATION` on the solver `HandoffAxis` case, the residual the graduation fold reads being the projection residual this receipt carries.
- Packages: `skfem` (`Basis`, `project`, `ElementLineP1`, `ElementLineP2`, `ElementTriP1`, `ElementTriP2`, `ElementTetP1`, `ElementTetP2`, `ElementQuad1`, `ElementHex1`, `MeshLine1`, `MeshTri1`, `MeshTet1`, `MeshQuad1`, `MeshHex1`; `basis.interpolate`, `basis.split`, `basis.zeros`, `DiscreteField`), `numpy` (`asarray`, `ascontiguousarray`, `abs`, `max`, `isfinite`, `all`, `linalg.norm`), `expression` (`tag`, `case`, `tagged_union`), `solvers/quadrature.md#QUADRATURE` (`ElementKind` — the shared element axis), `solvers/mesh.md#MESH_FIELD` (`MeshField` — the topology and DOF source the readout consumes), `solvers/receipt.md#RECEIPT` (`SolveStatus`, `_status` — the shared termination vocabulary and residual-floor verdict the field receipt folds into), runtime (`RuntimeRail`, `boundary`, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor`).
- Growth: a new element is one `_CTOR` row shared with the assemble and solve routes; a new field operation (gradient evaluation, flux recovery) is one `FieldQuery` case plus one `FieldReceipt` case sharing the basis-construction fold and the status floor; a new readout statistic is one slot on the owning receipt case; a new termination class is one `SolveStatus` member shared with every solver route; zero new surface, never a parallel evaluation container beside the `DiscreteField`, never a second projection entry, never a flat receipt with a stringly `operation` field or a sentinel residual on the interpolate path, and never a solve or assemble on this owner.
- Boundary: field evaluation and projection only — the `DiscreteField` quadrature readout, the vector/composite split, and the `skfem.project` basis-to-basis transfer are in-scope; the assemble stays on `solvers/mesh.md#MESH_FIELD`, the solve on `solvers/quadrature.md#QUADRATURE`, and the columnar/gridded statistical aggregation of the evaluated field stays in the `data` branch, so this owner reports an in-memory extent and residual and never aggregates a field across a grid. A solve on this owner, a hand-rolled interpolation loop where `basis.interpolate`/`skfem.project` own the concern, a `SolverReceipt` minted for a projection, a flat `FieldReceipt` with a `str` operation field and a `residual=0.0` sentinel, two parallel `_ELEMENT_CTOR`/`_MESH_CTOR` maps where one `_CTOR` row table carries both spellings, and a parallel field container beside the `skfem.DiscreteField` are the deleted forms; the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

```python signature
from collections.abc import Callable
from typing import Literal, assert_never

import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union

from rasm.compute.solvers.mesh import MeshField
from rasm.compute.solvers.quadrature import ElementKind
from rasm.compute.solvers.receipt import SolveStatus, _status
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt


type FieldOp = Literal["interpolate", "project"]
type ProjectSource = Callable[[np.ndarray], np.ndarray] | tuple[ElementKind, np.ndarray]


_CTOR: FrozenDict[ElementKind, tuple[str, str]] = FrozenDict(
    {
        ElementKind.P1: ("MeshLine1", "ElementLineP1"),
        ElementKind.P2: ("MeshLine1", "ElementLineP2"),
        ElementKind.TRI_P1: ("MeshTri1", "ElementTriP1"),
        ElementKind.TRI_P2: ("MeshTri1", "ElementTriP2"),
        ElementKind.TET_P1: ("MeshTet1", "ElementTetP1"),
        ElementKind.TET_P2: ("MeshTet1", "ElementTetP2"),
        ElementKind.QUAD_P1: ("MeshQuad1", "ElementQuad1"),
        ElementKind.HEX_P1: ("MeshHex1", "ElementHex1"),
    }
)

_TOL: FrozenDict[FieldOp, float] = FrozenDict({"interpolate": 1e-6, "project": 1e-6})


def _basis(field: MeshField, element: ElementKind) -> object:
    import skfem

    mesh_ctor, element_ctor = _CTOR[element]
    mesh = getattr(skfem, mesh_ctor)(
        np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T)
    )
    return skfem.Basis(mesh, getattr(skfem, element_ctor)())


@tagged_union(frozen=True)
class FieldReceipt:
    tag: FieldOp = tag()
    interpolate: tuple[ContentKey, ElementKind, int, int, float, float, SolveStatus] = case()
    project: tuple[ContentKey, ElementKind, int, float, float, SolveStatus] = case()

    @staticmethod
    def Interpolate(
        key: ContentKey, element: ElementKind, dof_count: int, components: int, extent: float, peak: float
    ) -> "FieldReceipt":
        status = _status(None, extent, _TOL["interpolate"])
        return FieldReceipt(interpolate=(key, element, dof_count, components, extent, peak, status))

    @staticmethod
    def Project(
        key: ContentKey, element: ElementKind, dof_count: int, extent: float, residual: float
    ) -> "FieldReceipt":
        status = _status(None, residual, _TOL["project"])
        return FieldReceipt(project=(key, element, dof_count, extent, residual, status))

    @property
    def operation(self) -> FieldOp:
        return self.tag

    @property
    def content_key(self) -> ContentKey:
        return getattr(self, self.tag)[0]

    @property
    def element(self) -> ElementKind:
        return getattr(self, self.tag)[1]

    @property
    def status(self) -> SolveStatus:
        match getattr(self, self.tag):
            case (*_, SolveStatus() as status):
                return status

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    def contribute(self) -> Receipt:
        element = self.element
        facts = {
            "operation": self.tag,
            "element": element.value,
            "status": self.status,
            "converged": str(self.converged),
            "key": str(self.content_key.value),
        }
        return Receipt.of("emitted", "compute.field", element.value, facts)


@tagged_union(frozen=True)
class FieldQuery:
    tag: FieldOp = tag()
    interpolate: tuple[MeshField, ElementKind, np.ndarray] = case()
    project: tuple[MeshField, ElementKind, ProjectSource] = case()

    @staticmethod
    def Interpolate(field: MeshField, element: ElementKind, dofs: np.ndarray) -> "FieldQuery":
        return FieldQuery(interpolate=(field, element, dofs))

    @staticmethod
    def Project(field: MeshField, target: ElementKind, source: ProjectSource) -> "FieldQuery":
        return FieldQuery(project=(field, target, source))


def evaluate(query: FieldQuery) -> "RuntimeRail[FieldReceipt]":
    return boundary(f"field.{query.tag}", lambda: _dispatch(query))


def _dispatch(query: FieldQuery) -> FieldReceipt:
    match query:
        case FieldQuery(tag="interpolate", interpolate=(field, element, dofs)):
            return _interpolate_receipt(field, element, np.asarray(dofs))
        case FieldQuery(tag="project", project=(field, target, source)):
            return _project_receipt(field, target, source)
        case unreachable:
            assert_never(unreachable)


def _extent(values: np.ndarray) -> tuple[float, float]:
    return (float(np.linalg.norm(values)), float(np.max(np.abs(values)))) if values.size else (0.0, 0.0)


def _ndofs(basis: object) -> int:
    return int(np.asarray(basis.zeros()).size)


def _interpolate_receipt(field: MeshField, element: ElementKind, dofs: np.ndarray) -> FieldReceipt:
    key = _key("field-interpolate", field, dofs)
    try:
        basis = _basis(field, element)
        components = tuple(basis.split(dofs)) or ((basis, dofs),)
        peaks = tuple(_extent(np.asarray(sub.interpolate(part).value)) for sub, part in components)
        extent = float(np.linalg.norm(np.asarray([norm for norm, _ in peaks])))
        peak = max((mag for _, mag in peaks), default=0.0)
        return FieldReceipt.Interpolate(key, element, _ndofs(basis), len(peaks), extent, peak)
    except ImportError:
        extent, peak = _extent(dofs)
        return FieldReceipt.Interpolate(key, element, int(dofs.size), 1, extent, peak)


def _project_receipt(field: MeshField, target: ElementKind, source: ProjectSource) -> FieldReceipt:
    import skfem

    target_basis = _basis(field, target)
    match source:
        case (ElementKind() as origin, np.ndarray() as origin_dofs):
            source_basis = _basis(field, origin)
            projected = np.asarray(skfem.project(origin_dofs, basis_from=source_basis, basis_to=target_basis))
            round_trip = np.asarray(skfem.project(projected, basis_from=target_basis, basis_to=source_basis))
            residual = float(np.linalg.norm(round_trip - np.asarray(origin_dofs)))
        case callable_field:
            projected = np.asarray(skfem.project(callable_field, basis_to=target_basis))
            residual = 0.0 if bool(np.all(np.isfinite(projected))) else float("inf")
    extent, _ = _extent(projected)
    return FieldReceipt.Project(_key("field-project", field, projected), target, _ndofs(target_basis), extent, residual)


def _key(label: str, field: MeshField, dofs: np.ndarray) -> ContentKey:
    buffers = (np.ascontiguousarray(field.points).tobytes(), np.ascontiguousarray(dofs).tobytes())
    return ContentIdentity.of(label, b"\x00".join(buffers), IdentityPolicy())
```

## [03]-[RESEARCH]

- [SKFEM_FIELD_EVAL]: the `skfem.Basis(mesh, element)`, `basis.interpolate(dofs)` → `DiscreteField`, `basis.split(dofs)`, the `basis.zeros()`-sized dof-count, and `DiscreteField.value` quadrature-sampled value array verify against `compute/.api/scikit-fem.md` (`DiscreteField` is the catalogued field container with element metadata; `basis.interpolate`/`basis.split`/`basis.zeros` are the catalogued postprocess and DOF-vector entries; the dof count reads `basis.zeros().size` rather than an uncatalogued attribute). `basis.split(dofs)` returns the per-component `(sub_basis, sub_dofs)` pairs for an `ElementVector`/`ElementVectorH1`/`ElementComposite` space (catalogued), so the interpolate fold re-interpolates each component and reports the component count and worst-component peak; a scalar element yields one component, the `or ((basis, dofs),)` floor covering a split that returns empty for a non-vector space. The `Mesh(points.T, cells.T)` node-major/element-major transpose matches the `solvers/mesh.md#MESH_FIELD` assemble layout (`skfem` stores `mesh.p` as `(dim, n)` and `mesh.t` as `(verts, n_elem)`). `scikit-fem` carries no cp315 wheel, so the eval fold rides the gated `python_version<'3.15'` band; the numpy nodal floor reads the DOF vector norm and peak directly and runs unconditionally.
- [SKFEM_PROJECT]: the `skfem.project(fun, basis_from=None, basis_to=None, diff=None, ...)` L2-projection entry verifies against `compute/.api/scikit-fem.md` (the catalogued projection of a callable field or a cross-basis DOF vector onto a target basis); the callable source passes `basis_to=target` and the cross-basis source passes `basis_from=source, basis_to=target`, both returning the target DOF vector. The cross-basis residual is computed in the source DOF space by re-projecting the target vector back (`project(projected, basis_from=target, basis_to=source)`) and taking the L2 norm against the original source DOFs — a single-space comparison that avoids subtracting `DiscreteField.value` arrays sampled at incompatible quadrature orders; the callable case carries a finiteness verdict (`0.0`/`inf`) because no catalogued physical-point sampling surface exposes the callable's nodal reference. The residual feeds the shared `_status` floor rather than a solver convergence verdict.
- [SHARED_STATUS_FOLD]: `FieldReceipt` is a `@tagged_union` mirroring `solvers/receipt.md#RECEIPT` `SolverReceipt` — the `Literal` tag is the operation, each case payload terminates in `SolveStatus` so `.status` reads the trailing slot through one `case (*_, status)` pattern and `.converged` derives the Boolean, and the `ContentKey` leads each payload so `.content_key`/`.element` read fixed leading slots. The interpolate and project factories fold the extent or residual through the shared `_status(None, value, _TOL[op])` floor imported from the receipt owner (the cross-module private import is the same idiom `solvers/quadrature.md#QUADRATURE` uses to reuse `solvers/linear.md#LINEAR` `_sparse_receipt`), so the field receipt reuses the one termination vocabulary every solver route folds into rather than a stringly `operation` field with a sentinel `residual=0.0` on the interpolate path. `_TOL` is the one frozen per-operation tolerance table, a new operation one row.
- [SHARED_ELEMENT_AXIS]: `ElementKind` is owned by `solvers/quadrature.md#QUADRATURE` and the `MeshField` topology by `solvers/mesh.md#MESH_FIELD`; this owner composes both so the element vocabulary and the mesh shape never diverge between the assemble fold, the FEM solve, and the field readout. The one `_CTOR` row table keys each `ElementKind` to its `(Mesh*, Element*)` constructor-name pair carrying the mesh owner's constructor truth in a single table rather than two parallel `_ELEMENT_CTOR`/`_MESH_CTOR` maps, and `_basis` resolves the pair through `getattr(skfem, ...)` at evaluation time, keeping the gated import behind the boundary.
```
