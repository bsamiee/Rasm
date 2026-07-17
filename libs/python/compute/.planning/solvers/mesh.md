# [PY_COMPUTE_MESH]

One simulation mesh-and-field interchange and weak-form assembly owner, and the declaring owner of the FEM element axis: `ElementKind` and `FemForm` originate here so `solvers/quadrature.md#QUADRATURE` imports the axis downward for its FEM condense-solve arm and `solvers/field.md#FIELD` reads the element vocabulary from here. `MeshField` is the frozen topology-and-field value object; `MeshExchange` discriminates the three transforms a discretized mesh admits — `assemble` lowers a `FemForm` weak form to the sparse `(stiffness, load, dirichlet_dofs)` system through the scikit-fem `Basis`/`asm` fold, and `read`/`write` round-trip the meshio registry with physical groups intact. This owner holds element vocabulary, assembly, and interchange only — never the solve (`solvers/quadrature.md#QUADRATURE`), never the transient integration (`solvers/differential.md#DIFFERENTIAL`), never a parallel mesh container beside the meshio `Mesh`.

Each operation folds into one `MeshReceipt` whose `Literal` `tag` IS the operation and whose payload shape, `.status` read, accessors, and observability row are all driven by one `_SLOTS` field-name table, as `solvers/field.md#FIELD` `FieldReceipt` and `solvers/receipt.md#RECEIPT` `SolverReceipt` drive theirs, each terminating in the shared `SolveStatus` the `solvers/receipt.md#RECEIPT` `status_of` floor adjudicates by public name. Content key threads `runtime/evidence/identity.md#IDENTITY` `ContentIdentity` under the `CANONICAL_POLICY` default and keys the `execution/lanes.md#LANE` reuse-fabric cache; receipt emission rides the hub `evidence_run` weave. `meshio` is pure-Python core and imports top-level; `read`/`write` block on arbitrary-size disk I/O and cross the thread band under `RELEASING` `_TRAIT` rows, while `assemble` crosses the process band under `HOSTILE` — `skfem.asm` evaluates the caller-supplied Python `FemForm` integrand thunks GIL-held, so a thread arm serializes the loop against them — every arm through `lane.offload`, isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, never a per-page literal or compute-minted limiter.

## [01]-[INDEX]

- [01]-[MESH_FIELD]: the FEM element axis owned here, the frozen mesh-and-field value object, the one public `CTOR` `(Mesh*, Element*, cell-type)` triple every route resolves through, and the `ContentIdentity` content key over its array buffers.
- [02]-[EXCHANGE]: the `assemble`/`read`/`write` operations on one `MeshExchange`, the lane-offloaded scikit-fem lowering, the meshio round-trip with physical-group transfer, and the `_SLOTS`-driven `MeshReceipt`.

## [02]-[MESH_FIELD]

- Owner: `ElementKind` is the one FEM element vocabulary (P1/P2 line, TRI/TET P1/P2, QUAD/HEX P1), declared here because mesh constructs the elements; `quadrature` and `field` import it downward, and a second element vocabulary anywhere in the folder is rejected. `FemForm` is the weak-form request the assemble fold lowers. `MeshField` is the frozen mesh-and-field value object — the topology the assemble fold reads, `solvers/field.md#FIELD` consumes, and the interchange round-trips with its physical groups intact — and it never assembles, never solves, never stands up a parallel per-format mesh beside the meshio `Mesh`.
- Element table: `CTOR` is the one public `Map[ElementKind, tuple[str, str, str]]` element-spelling table — `(Mesh*-constructor, Element*-constructor, meshio-cell-type)` per element — the assemble fold, the FEM solve, the field readout, and the meshio round-trip all resolve through; `field` composes it by name, so the cross-module contract is honest rather than a `_CTOR` private masquerade. A new element is one row shared with every route, never three parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps.
- Content key: `MeshField.content_key` is a stored `ContentKey` minted once through the `_field` single-pass fold under the `CANONICAL_POLICY` default; an explicit `IdentityPolicy()` allocation keys identically by value equality and is pure ceremony. Identical bytes at identical policy key identically, feeding the `execution/lanes.md#LANE` `Map[ContentKey, T]` reuse-fabric cache.
- Growth: a new element is one `CTOR` row shared with the quadrature/field routes; a new field array is one `node_fields`/`cell_fields` entry and a new physical group one `node_sets`/`cell_sets` entry, folded into the content key automatically by the `_field` buffer fold; never a parallel mesh container, never a solve on this owner, never a parallel element-spelling map beside `CTOR`, never a write-only physical-group promotion lacking the inbound recovery.
- Boundary: topology, element vocabulary, and field carry only — node coordinates, per-block connectivity, per-node/per-cell field arrays, and the content key; the assemble and interchange operations live on `MeshExchange`, the solve on `solvers/quadrature.md#QUADRATURE`, and the transient integration on `solvers/differential.md#DIFFERENTIAL`. A hand-rolled content digest where `ContentIdentity` owns the concern is rejected, and the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

## [03]-[EXCHANGE]

- Owner: `MeshExchange` is the one `@tagged_union(frozen=True)` operation owner discriminating `assemble`, `read`, and `write` rather than a free `MeshField.assemble` method beside a `read`/`write` static pair; the `@classmethod`-plus-`Self` factory law binds each subtype once, shared with `MeshReceipt`, `solvers/receipt.md#RECEIPT` `SolverReceipt`, and `solvers/field.md#FIELD` `FieldQuery`. meshio `Mesh` is the canonical container `read`/`write` project through and meshio owns the ~40-format registry — a hand-rolled format parser, a wrapper-rename of `read`/`write`, a flat `cell_data` dropping the block-parallel list, and a write-only promotion that never recovers on read are all rejected.
- Entry: `MeshExchange.run(lane)` rides the hub weave as `evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch)`, which owns span, fault fence, and receipt harvest, so receipt egress is composed rather than a page-local `_emit`. `dispatch` resolves the `_TRAIT` row through `lane.offload` — the interchange arms ride the `RELEASING` thread band because meshio blocks on disk, the assemble arm rides the `HOSTILE` process band because `skfem.asm` drives the caller's Python form callbacks GIL-held (the closure-bearing `FemForm` thunks cross on the pool's cloudpickle wire) — isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, so no arm stalls the loop. meshio `ReadError`/`WriteError` and the skfem assembly exceptions convert exactly once at the weave's fence into the `BoundaryFault` rail through the `runtime/reliability/faults.md#FAULT` `CLASSIFY` fold, so a malformed input or unsupported cell type is a typed rail, never a raised exception in domain flow.
- Receipt: `MeshReceipt` is the one `@tagged_union(frozen=True)` receipt whose `Literal` `tag` IS the operation. One `_SLOTS` row names each payload sequence — `key` leading, `status` trailing — and drives the structural shape, the `.status` read, every named accessor off `.facts` (never parallel `getattr(self, self.tag)[N]` properties), and the `.facts` `zip(..., strict=True)` projection, so the table and the case tuples cannot drift. No operation carries a solve, so every factory floors a well-formedness extent, not a convergence residual, through the shared public `status_of` floor; `assembled` records `load_norm` as evidence yet floors on `dof_count` and a finite load rather than mislabeling a valid large load. Weave harvests the resolved receipt; `contribute` carries no decorator.
- Packages: `skfem` (`Basis`/`asm`, the `Mesh*1`/`Element*` constructors resolved by name through `CTOR`, `get_dofs(facets=...).flatten()` the DOF selector, `basis.N` the dof count), `meshio` (the ~40-format registry, the per-type-merged `cell_data_dict`/`cell_sets_dict` read views, the block-parallel write surface, the `cell_sets_to_data`/`cell_data_to_sets` promoter/inverter family, `ReadError`/`WriteError` boundary-folded), `numpy` (`linalg.norm` the load fold, `issubdtype` the integer-label gate), `math.isfinite`, `expression` (`tagged_union`/`Map` the table rail), hub (`evidence_run`), `solvers/receipt.md#RECEIPT` (`status_of` by public name), runtime (`railed` the `effect.result` builder, `Kernel`/`KernelTrait` the offload crossing, `ContentIdentity` under `CANONICAL_POLICY`, `Receipt`).
- Growth: a new operation (a `Functional` energy-norm evaluation, an adaptive `refined` step) is one `MeshExchange` case plus one `_SLOTS` row plus one `_TRAIT` row sharing the `CTOR` resolution and the status floor; a new element is one `CTOR` row; a new assembled field is one slot on `AssembledSystem`; a new format is zero new surface because meshio owns the registry; a new termination class is one `SolveStatus` member; never a parallel mesh container, never a solve on this owner, never a per-operation factory plus per-operation fact dict beside the `_SLOTS` projection.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from math import isfinite
from pathlib import Path
from typing import Final, Literal, Self, assert_never

import meshio  # core pure-Python; unconditional top-level, never deferred behind TYPE_CHECKING
import numpy as np
from enum import StrEnum
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.graduation.handoff import EvidenceScope, evidence_run
from rasm.compute.solvers.receipt import SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary, railed
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import Receipt
from rasm.runtime.workers import Kernel, KernelTrait


# --- [TYPES] -------------------------------------------------------------------------------

type MeshOp = Literal["assembled", "read", "written"]


class ElementKind(StrEnum):
    P1 = "p1"
    P2 = "p2"
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TET_P1 = "tet_p1"
    TET_P2 = "tet_p2"
    QUAD_P1 = "quad_p1"
    HEX_P1 = "hex_p1"


# --- [CONSTANTS] ---------------------------------------------------------------------------

# ElementKind -> (Mesh*, Element*, meshio-cell-type). Names resolve through getattr(skfem, ...) behind
# deferred worker import; P2 shares its P1 sibling's affine Mesh*1 geometry and cell-type string,
# varying only the Element*.
CTOR: Final[Map[ElementKind, tuple[str, str, str]]] = Map.of_seq([
    (ElementKind.P1, ("MeshLine1", "ElementLineP1", "line")),
    (ElementKind.P2, ("MeshLine1", "ElementLineP2", "line")),
    (ElementKind.TRI_P1, ("MeshTri1", "ElementTriP1", "triangle")),
    (ElementKind.TRI_P2, ("MeshTri1", "ElementTriP2", "triangle")),
    (ElementKind.TET_P1, ("MeshTet1", "ElementTetP1", "tetra")),
    (ElementKind.TET_P2, ("MeshTet1", "ElementTetP2", "tetra")),
    (ElementKind.QUAD_P1, ("MeshQuad1", "ElementQuad1", "quad")),
    (ElementKind.HEX_P1, ("MeshHex1", "ElementHex1", "hexahedron")),
])

# Per-operation well-formedness floor, one row per MeshOp — no op carries a solve, so a row floors
# `0.0` finite against `inf` degenerate, never a convergence residual.
_TOL: Final[Map[MeshOp, float]] = Map.of_seq([("assembled", 1e-6), ("read", 1e-6), ("written", 1e-6)])

# Per-operation payload field names, `key` leading and `status` trailing — the one owner over the case
# shapes the factory packs by and `.facts` projects.
_SLOTS: Final[Map[MeshOp, tuple[str, ...]]] = Map.of_seq([
    ("assembled", ("key", "element", "dof_count", "dirichlet_count", "load_norm", "status")),
    ("read", ("key", "element", "point_count", "cell_count", "status")),
    ("written", ("key", "fmt", "byte_count", "status")),
])

# Family trait rows: assemble is HOSTILE — `skfem.asm` evaluates the caller-supplied Python form callbacks GIL-held, so the
# process arm isolates them; read/write block on disk and stay RELEASING; isolation, band, and retry derive at the Kernel crossing.
_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([("assemble", KernelTrait.HOSTILE), ("read", KernelTrait.RELEASING), ("write", KernelTrait.RELEASING)])


# --- [MODELS] ------------------------------------------------------------------------------


class FemForm(Struct, frozen=True):
    # bilinear/linear carry the skfem BilinearForm/LinearForm integrand thunks, typed object at the band boundary.
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


# content_key is a stored field, not a property: ContentIdentity.of runs the fallible canonical-derive
# seam, so `_field` mints the key once inside the `_dispatch` boundary. No gc=False — the record holds
# tracked ndarray/dict containers, so the leaf-only opt-out does not apply.
class MeshField(Struct, frozen=True):
    element: ElementKind
    points: np.ndarray
    cells: np.ndarray
    node_fields: dict[str, np.ndarray]
    cell_fields: dict[str, np.ndarray]
    node_sets: dict[str, np.ndarray]
    cell_sets: dict[str, np.ndarray]
    field_data: dict[str, np.ndarray]
    content_key: ContentKey


# stiffness typed object: the scipy.sparse (csr_array) container the solvers/linear.md#LINEAR
# LinearMap.SparseMat carrier takes, kept at the band boundary rather than a module-load scipy import.
# No gc=False — load/dirichlet_dofs are tracked containers.
class AssembledSystem(Struct, frozen=True):
    element: ElementKind
    stiffness: object
    load: np.ndarray
    dirichlet_dofs: np.ndarray
    dof_count: int
    content_key: ContentKey


@tagged_union(frozen=True)
class MeshReceipt:
    tag: MeshOp = tag()
    assembled: tuple[ContentKey, ElementKind, int, int, float, SolveStatus] = case()
    read: tuple[ContentKey, ElementKind, int, int, SolveStatus] = case()
    written: tuple[ContentKey, str, int, SolveStatus] = case()

    @classmethod
    def Assembled(cls, key: ContentKey, element: ElementKind, dof_count: int, dirichlet_count: int, load_norm: float) -> Self:
        extent = 0.0 if dof_count and isfinite(load_norm) else float("inf")
        return cls(assembled=(key, element, dof_count, dirichlet_count, load_norm, status_of(None, extent, _TOL["assembled"])))

    @classmethod
    def Read(cls, key: ContentKey, element: ElementKind, point_count: int, cell_count: int) -> Self:
        extent = 0.0 if point_count and cell_count else float("inf")
        return cls(read=(key, element, point_count, cell_count, status_of(None, extent, _TOL["read"])))

    @classmethod
    def Written(cls, key: ContentKey, fmt: str, byte_count: int) -> Self:
        extent = 0.0 if byte_count else float("inf")
        return cls(written=(key, fmt, byte_count, status_of(None, extent, _TOL["written"])))

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case (
                MeshReceipt(tag="assembled", assembled=payload) | MeshReceipt(tag="read", read=payload) | MeshReceipt(tag="written", written=payload)
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
                MeshReceipt(tag="assembled", assembled=(*_, SolveStatus() as status))
                | MeshReceipt(tag="read", read=(*_, SolveStatus() as status))
                | MeshReceipt(tag="written", written=(*_, SolveStatus() as status))
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
        return (Receipt.of("compute.mesh", ("emitted", subject, facts)),)


@tagged_union(frozen=True)
class MeshExchange:
    tag: Literal["assemble", "read", "write"] = tag()
    assemble: tuple[MeshField, FemForm] = case()
    read: tuple[Path, ElementKind, str | None] = case()
    write: tuple[MeshField, Path, str | None] = case()

    @classmethod
    def Assemble(cls, field: MeshField, form: FemForm, /) -> Self:
        return cls(assemble=(field, form))

    @classmethod
    def Read(cls, path: Path, element: ElementKind, file_format: str | None = None, /) -> Self:
        return cls(read=(path, element, file_format))

    @classmethod
    def Write(cls, field: MeshField, path: Path, file_format: str | None = None, /) -> Self:
        return cls(write=(field, path, file_format))

    async def run(self, lane: LanePolicy) -> RuntimeRail[MeshReceipt]:
        # _TRAIT routes each arm — assemble to the HOSTILE process band (Python form callbacks run GIL-held), read/write
        # to the RELEASING thread band; the weave owns span, fence, and harvest, the Kernel crossing isolation and retry.
        async def dispatch() -> RuntimeRail[MeshReceipt]:
            return (await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch)


# --- [OPERATIONS] --------------------------------------------------------------------------


# `railed` `effect.result` chain: only the read arm `yield from`-binds a fallible rail (`_read`'s
# meshio parse / canonical-encode); assemble and write hold no fallible derive and lift straight.
@railed
def _dispatch(exchange: MeshExchange) -> MeshReceipt:
    match exchange:
        case MeshExchange(tag="assemble", assemble=(field, form)):
            system = _assemble(field, form)
            load_norm = float(np.linalg.norm(system.load))
            return MeshReceipt.Assembled(system.content_key, system.element, system.dof_count, int(system.dirichlet_dofs.size), load_norm)
        case MeshExchange(tag="read", read=(path, element, fmt)):
            field: MeshField = yield from _read(path, element, fmt)
            return MeshReceipt.Read(field.content_key, element, int(field.points.shape[0]), int(field.cells.shape[0]))
        case MeshExchange(tag="write", write=(field, path, fmt)):
            written = _write(field, path, fmt)
            return MeshReceipt.Written(field.content_key, written, int(path.stat().st_size))
        case _ as unreachable:
            assert_never(unreachable)


# `_field` mints the content key from EVERY stored array as a LABELED chunk — element tag, then per block
# slot, map key, dtype, shape, and C-contiguous bytes, each part length-prefixed so the identity
# owner's `stream` updater sees unambiguous boundaries, dict sections sorted so insertion order never
# leaks. A renamed group, reshaped array, or re-homed value therefore re-keys where raw value bytes alone
# would collide. `ContentIdentity.of` returns `RuntimeRail[ContentKey]`, so the key threads by `yield from`.
@railed
def _field(
    element: ElementKind,
    points: np.ndarray,
    cells: np.ndarray,
    node_fields: dict[str, np.ndarray],
    cell_fields: dict[str, np.ndarray],
    node_sets: dict[str, np.ndarray],
    cell_sets: dict[str, np.ndarray],
    field_data: dict[str, np.ndarray],
) -> MeshField:
    def chunk(slot: str, name: str, buf: np.ndarray) -> bytes:
        arr = np.ascontiguousarray(buf)
        parts = (slot.encode(), name.encode(), str(arr.dtype).encode(), repr(arr.shape).encode(), arr.tobytes())
        return b"".join(len(part).to_bytes(8, "big") + part for part in parts)

    sections: tuple[tuple[str, dict[str, np.ndarray]], ...] = (
        ("node_fields", node_fields),
        ("cell_fields", cell_fields),
        ("node_sets", node_sets),
        ("cell_sets", cell_sets),
        ("field_data", field_data),
    )
    buffers = (
        element.value.encode(),
        chunk("topology", "points", points),
        chunk("topology", "cells", cells),
        *(chunk(slot, name, mapping[name]) for slot, mapping in sections for name in sorted(mapping)),
    )
    key: ContentKey = yield from ContentIdentity.of("mesh-field", buffers)
    return MeshField(
        element=element,
        points=points,
        cells=cells,
        node_fields=node_fields,
        cell_fields=cell_fields,
        node_sets=node_sets,
        cell_sets=cell_sets,
        field_data=field_data,
        content_key=key,
    )


# `.T` matches the (dim, n)/(verts, n_elem) layout skfem stores mesh.p/mesh.t in. `get_dofs(facets=...)`
# is the one DOF selector, `.flatten()` reducing to the global index array condense accepts as `D=`. The
# getattr resolves the pair behind the deferred skfem import — module-load lightness, not cycle evasion.
def _assemble(field: MeshField, form: FemForm) -> AssembledSystem:
    import skfem

    mesh_ctor, element_ctor, _ = CTOR[field.element]
    mesh = getattr(skfem, mesh_ctor)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    basis = skfem.Basis(mesh, getattr(skfem, element_ctor)())
    return AssembledSystem(
        element=field.element,
        stiffness=skfem.asm(form.bilinear, basis),
        load=np.asarray(skfem.asm(form.linear, basis)),
        dirichlet_dofs=np.asarray(basis.get_dofs(facets=form.boundary_facets).flatten()),
        dof_count=int(basis.N),
        content_key=field.content_key,
    )


# file_format disambiguates the .msh/.dat extensions, None triggering detection. The per-cell field/set
# arrays read the cell_type-keyed `cell_data_dict[k][cell_type]`/`cell_sets_dict[k][cell_type]` merged
# views — never `cell_data[k][0]`, the block-0 array that misaligns with `cells_dict[cell_type]` when the
# FEM element is not block 0; the `if cell_type in by_type` guard drops a name off the element's block.
# `cell_data_to_sets`/`point_data_to_sets` invert ONLY integer-kind columns (the `np.issubdtype` gate),
# so a float field stays in `cell_fields` while an integer region tag recovers into named sets.
def _read(path: Path, element: ElementKind, fmt: str | None) -> RuntimeRail[MeshField]:
    *_, cell_type = CTOR[element]
    mesh = meshio.read(str(path), fmt)
    for column, blocks in tuple(mesh.cell_data.items()):
        if np.issubdtype(np.asarray(blocks[0]).dtype, np.integer):
            mesh.cell_data_to_sets(column)
    for column, values in tuple(mesh.point_data.items()):
        if np.issubdtype(np.asarray(values).dtype, np.integer):
            mesh.point_data_to_sets(column)
    cell_data = mesh.cell_data_dict
    cell_sets = mesh.cell_sets_dict
    return _field(
        element,
        np.asarray(mesh.points),
        np.asarray(mesh.cells_dict[cell_type]),
        {k: np.asarray(v) for k, v in mesh.point_data.items()},
        {k: np.asarray(by_type[cell_type]) for k, by_type in cell_data.items() if cell_type in by_type},
        {k: np.asarray(v) for k, v in mesh.point_sets.items()},
        {k: np.asarray(by_type[cell_type]) for k, by_type in cell_sets.items() if cell_type in by_type},
        {k: np.asarray(v) for k, v in mesh.field_data.items()},
    )


# cell_data/cell_sets arrays wrap in the single-element per-block list meshio mandates; `cell_sets_to_data`/
# `point_sets_to_data` promote the named groups into integer label fields so region tags round-trip through
# integer-only formats — the write half of the read-recover/write-promote round-trip.
def _write(field: MeshField, path: Path, fmt: str | None) -> str:
    *_, cell_type = CTOR[field.element]
    mesh = meshio.Mesh(
        points=np.asarray(field.points),
        cells=[meshio.CellBlock(cell_type, np.asarray(field.cells))],
        point_data={k: np.asarray(v) for k, v in field.node_fields.items()},
        cell_data={k: [np.asarray(v)] for k, v in field.cell_fields.items()},
        field_data={k: np.asarray(v) for k, v in field.field_data.items()},
        point_sets={k: np.asarray(v) for k, v in field.node_sets.items()},
        cell_sets={k: [np.asarray(v)] for k, v in field.cell_sets.items()},
    )
    mesh.cell_sets_to_data()
    mesh.point_sets_to_data()
    meshio.write(str(path), mesh, fmt)
    return fmt or path.suffix.lstrip(".")
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
