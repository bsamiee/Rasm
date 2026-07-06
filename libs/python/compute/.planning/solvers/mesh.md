# [PY_COMPUTE_MESH]

The one simulation mesh-and-field interchange and weak-form assembly owner beside the FEM solver route, and the OWNER of the FEM element axis: `ElementKind` and `FemForm` are declared here — mesh constructs elements, assembles systems, and owns the meshio interchange, so `quadrature` imports this page downward for its FEM condense-solve arm and `field` reads the whole element axis from here. The owner holds the element vocabulary, assembly, and interchange only — never the solve, never a parallel mesh container beside the meshio `Mesh`.

`MeshField` is the frozen topology-and-field value object carrying the `points` node coordinates, the `cells` connectivity for its `ElementKind`, the per-node `node_fields` and per-cell `cell_fields` array maps, the `node_sets`/`cell_sets` named physical groups, the `field_data` format metadata, and the `content_key` over the canonical mesh-and-field buffer through `ContentIdentity`.

`MeshExchange` is the ONE `@tagged_union` operation owner discriminating the three transforms a discretized mesh-and-field admits: `assemble` lowers a `FemForm` weak form to the sparse `(stiffness, load, dirichlet_dofs)` pair through the scikit-fem `Basis`/`asm` fold, `read` parses any meshio-registry format into a `MeshField` recovering its physical groups, and `write` serializes a `MeshField` back through the meshio format dispatch promoting its physical groups.

Each operation folds into one `MeshReceipt` whose `Literal` `tag` IS the operation and whose per-case payload shape, `.status` read, accessor projection, and observability row are all driven by one `_SLOTS` field-name table — exactly as `solvers/field.md#FIELD` `FieldReceipt` and `solvers/receipt.md#RECEIPT` `SolverReceipt` drive their cases, each terminating in the shared `SolveStatus` verdict the `solvers/receipt.md#RECEIPT` `status_of` floor adjudicates. The three `@classmethod` factories returning `Self` are the canonical constructors, and receipt emission rides the hub `evidence_run` weave every compute evidence owner composes.

`meshio` is pure-Python and core, so the `read`/`write` interchange runs unconditionally as a top-level import and inline; the `scikit-fem` `assemble` fold is native work composed onto the runtime THREAD band through `lane.offload` — a modality row on this family's own table, never a per-page literal and never a compute-minted limiter.

## [01]-[INDEX]

- [01]-[MESH_FIELD]: the FEM element axis (`ElementKind`, `FemForm`) owned here, the frozen mesh-and-field value object, the one public `CTOR` `(Mesh*, Element*, cell-type)` triple table shared with the assemble/solve/readout routes, and the `ContentIdentity` `stream`-modality content key over its array buffers.
- [02]-[EXCHANGE]: the `assemble`/`read`/`write` operations on one `MeshExchange` `@tagged_union`, the lane-offloaded scikit-fem weak-form lowering, the meshio registry round-trip with physical-group transfer, and the `_SLOTS`-driven `MeshReceipt` folded through the shared `status_of` floor under the hub weave.

## [02]-[MESH_FIELD]

- Owner: `ElementKind` — the ONE FEM element vocabulary (P1/P2 line, TRI/TET P1/P2, QUAD/HEX P1), declared here because mesh constructs the elements and owns their spellings; `FemForm` — the weak-form request carrying the element, the `BilinearForm`/`LinearForm` integrand thunks, the Dirichlet facet names, and the boundary value; a second element vocabulary anywhere in the folder is a deleted form, as is a `TYPE_CHECKING` import of a runtime-constructed symbol or a function-local import whose purpose is cycle evasion — the axis re-homing killed the mesh⇄quadrature cycle outright. `MeshField` — the `msgspec.Struct(frozen=True)` mesh-and-field value object carrying the source `ElementKind`, the `points` node coordinates, the `cells` connectivity for that element, the per-node `node_fields` and per-cell `cell_fields` array maps, the `node_sets`/`cell_sets` named physical groups and `field_data` format metadata the meshio `*_sets`/`field_data` surface owns, and `content_key` the `ContentKey` over the canonical mesh-and-field buffer; it carries no `gc=False` because the record holds tracked `ndarray`/`dict` containers. The topology is the artifact the assemble fold reads, the readout (`solvers/field.md#FIELD`) consumes, and the interchange round-trips with its physical groups intact; it never assembles, never solves, and never stands up a parallel per-format mesh shape beside the meshio `Mesh`.
- Element table: `CTOR` is the ONE PUBLIC `Map[ElementKind, tuple[str, str, str]]` element-spelling table — `(Mesh*-constructor, Element*-constructor, meshio-cell-type)` per element — the assemble fold, the FEM solve, the field readout, and the meshio round-trip all resolve through; `field` composes it by name, so the cross-module contract is honest rather than a `_CTOR` private masquerade. A new element is one row shared with every route, never three parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps.
- Content key: `MeshField.content_key` is a stored `ContentKey` field minted once through the `_field` single-pass fold — the element tag leads, then `points`, `cells`, each `node_fields`/`cell_fields` array, each `node_sets`/`cell_sets` physical-group array, and each `field_data` format-metadata array cross as LABELED canonical chunks (slot discriminator, map key, dtype, shape, and `ascontiguousarray(...).tobytes()` bytes, each part length-prefixed so the concatenation-absorbing `stream` updater still sees unambiguous boundaries, dict sections sorted by key) keying the `stream` modality's order-sensitive stateful-updater fold the `runtime/evidence/identity.md#IDENTITY` owner mints under the `CANONICAL_POLICY` default (an explicit `IdentityPolicy()` allocation is the deleted form — the default and a fresh allocation key identically by value equality, so the allocation is pure ceremony). The labels are load-bearing: raw value bytes alone would key a renamed physical group, a reshaped array, or a value re-homed across slots identically — the deleted keying form; folding `field_data` is equally load-bearing, since omitting a stored field would collide two meshes differing only in format metadata in the reuse-fabric cache. It is a stored field, NOT a `@property`, because `ContentIdentity.of` returns `RuntimeRail[ContentKey]` over the fallible canonical-derive seam, so `_field` is a `@railed` `effect.result` mint that `yield from`-binds the key off the rail before the construct — a canonical-encode fault propagates on the one `RuntimeRail` the `_dispatch` chain returns. Identical bytes at identical policy settings key identically, feeding the reuse-fabric cache the `execution/lanes.md#LANE` `Map[ContentKey, T]` keys.
- Growth: a new element is one `CTOR` row shared with the quadrature/field routes; a new field array is one entry in `node_fields` or `cell_fields` and a new physical group one entry in `node_sets` or `cell_sets`, automatically folded into the content key by the `_field` buffer fold; zero new surface, never a parallel mesh container beside the meshio `Mesh`, never a solve on this owner, never a parallel element-spelling map beside `CTOR`, never a write-only physical-group promotion lacking the inbound recovery.
- Boundary: topology, element vocabulary, and field carry only — the node coordinates, the per-block connectivity, the per-node/per-cell field arrays, and the content key are in-scope; the assemble and interchange operations live on `MeshExchange`, the solve on `solvers/quadrature.md#QUADRATURE`, and the transient integration on `solvers/differential.md#DIFFERENTIAL`. A solve on this owner, a hand-rolled content digest where `ContentIdentity`/`IdentitySource` own the concern, three parallel element-spelling maps where the one `CTOR` triple carries every spelling, a second `ElementKind`/`FemForm` declaration anywhere in the folder, and a parallel per-format mesh container beside the meshio `Mesh` are the deleted forms; the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

## [03]-[EXCHANGE]

- Owner: `MeshExchange` — the ONE `@tagged_union(frozen=True)` operation owner discriminating `assemble` (a `MeshField` plus a `FemForm` weak form → the sparse `(stiffness, load, dirichlet_dofs)` system through the scikit-fem `Basis`/`asm` fold), `read` (a `Path` plus an `ElementKind` plus an optional `file_format` disambiguator → a `MeshField` projected off the meshio `Mesh`), and `write` (a `MeshField` plus a `Path` plus an optional `file_format` → the meshio serialize), exactly as `solvers/quadrature.md#QUADRATURE` `QuadratureIntent` and `solvers/field.md#FIELD` `FieldQuery` discriminate their operations on one owner rather than a free `MeshField.assemble` method beside a `read`/`write` static-method pair. The three `@classmethod` constructors `Assemble`/`Read`/`Write` return `Self`, binding the subtype once — the `@classmethod`-plus-`Self` factory law the sibling `MeshReceipt`, `solvers/receipt.md#RECEIPT` `SolverReceipt`, and `solvers/field.md#FIELD` `FieldQuery` hold; the meshio `Mesh` is the canonical container the `read`/`write` cases project through, never a parallel per-format shape.
- Interchange: the `read` case calls `meshio.read(str(path), file_format)` — passing the optional `file_format` through to disambiguate the ambiguous `.msh`/`.dat` extensions the catalogue flags, `None` triggering extension detection — and projects the meshio `Mesh` onto a `MeshField`: the `points` onto the node coordinates, the `cells_dict[cell_type]` merged connectivity (the `CTOR[element]` cell-type string) onto the FEM-facing topology, the `point_data` onto `node_fields`, the `cell_data_dict[k][cell_type]` per-type-aligned array onto `cell_fields` (the merged-by-type view matching the connectivity, never the block-0 `cell_data[k][0]` array that misaligns on a multi-block mesh), and the FULL physical-group surface onto `node_sets`/`cell_sets`/`field_data` through the type-aligned `cell_sets_dict[k][cell_type]`. The read first inverts only the integer-kind `cell_data`/`point_data` columns a Gmsh/VTK source wrote — the `np.issubdtype(..., np.integer)` gate, since the meshio inverter expects integer label fields — through `mesh.cell_data_to_sets(column)`/`mesh.point_data_to_sets(column)`, so a region tag that crossed as an integer column recovers into named sets while a float solution field stays in `cell_fields` rather than being force-inverted, the inverse of the write promotion. The `write` case builds `meshio.Mesh(points, [meshio.CellBlock(cell_type, cells)], point_data=..., cell_data=..., field_data=..., point_sets=..., cell_sets=...)` — the `cell_data` field arrays and the `cell_sets` index arrays each wrapped in the single-element per-block list the meshio block-parallel contract mandates — then promotes the named groups through `mesh.cell_sets_to_data()`/`mesh.point_sets_to_data()` so a Gmsh/VTK round-trip carries the region tags through formats that only support integer labels, and calls `meshio.write(str(path), mesh, file_format)` with extension-driven or explicit format detection. A hand-rolled format parser, a wrapper-rename of `read`/`write`, a flat `cell_data` array dropping the block-parallel list, and a write-only `cell_sets_to_data` that promotes physical groups outbound but never recovers them on read are the deleted forms; meshio owns the ~40-format registry and this owner composes its full `Mesh` surface.
- Entry: `MeshExchange.run(lane)` is `async def`, riding the hub weave as `evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch)` — the weave owns span, fault fence, and the fenced `@receipted(REDACTION)` harvest of the resolved `MeshReceipt`, so receipt egress is composed, never a page-local `_emit` aspect. `dispatch` resolves the family modality row: the `assemble` arm is native scikit-fem work offloaded through `lane.offload(_dispatch, self, modality=Modality.THREAD)` under the runtime-owned THREAD band (compute mints zero `CapacityLimiter`s; the deterministic assemble takes no retry — worker-death retry rides the process lane only), while the pure-Python `read`/`write` arms run `_dispatch` inline — the modality is one `_MODALITY` policy row per operation, never a per-page literal. `_dispatch` is the `@railed` `effect.result` chain whose `match` dispatches the three cases through total `assert_never` exhaustion: the assemble arm folds the assembled system into the `assembled` case, the read arm `yield from`-binds `_read`'s `RuntimeRail[MeshField]` and folds the projected node/cell counts into the `read` case, and the write arm the serialized byte length and format into the `written` case. The `assemble` and `write` arms hold no fallible derive and reuse the `field.content_key` the mesh owner already minted; only the `read` arm threads the `_field` `ContentIdentity.of` rail, so no case re-digests a buffer it already holds a key for. The meshio `ReadError`/`WriteError` and the skfem assembly exceptions convert exactly once at the weave's fence into the `BoundaryFault` rail through the `runtime/reliability/faults.md#FAULT` `CLASSIFY` fold, so a malformed input or an unsupported cell type is a typed rail rather than a raised exception in domain flow.
- Receipt: `MeshReceipt` is the ONE `@tagged_union(frozen=True)` receipt whose `Literal` `tag` IS the operation, read directly through `.tag`. The per-case payload shapes are NOT hand-spelled: one `_SLOTS` table names each operation's payload field sequence — `assembled → (key, element, dof_count, dirichlet_count, load_norm, status)`, `read → (key, element, point_count, cell_count, status)`, `written → (key, fmt, byte_count, status)` — with `key` the common leading slot and `status` the common trailing slot, exactly as `solvers/receipt.md#RECEIPT` `_SLOTS` drives `SolverReceipt`. That single table drives the structural shape, the `.status` trailing-slot read through one `case (*_, status)` pattern closed by `assert_never`, every named accessor (`.content_key`/`.element` read off `.facts`, never parallel `getattr(self, self.tag)[N]` properties), and the `.facts` projection through `zip(_SLOTS[self.tag], payload, strict=True)` — so the table and the case tuples cannot drift. The three `@classmethod` factories `Assembled`/`Read`/`Written` return `Self`, binding the subtype once. None of the three operations carries a solve, so every factory adjudicates a well-formedness extent — not a convergence residual — through the shared public `status_of(None, extent, _TOL[op])` floor imported from `solvers/receipt.md#RECEIPT`: `assembled` records the `load_norm` magnitude as graduation evidence yet floors `SUCCESS` on `dof_count` and a finite load rather than mislabeling a large-but-valid load as `STAGNATION`, while `read`/`written` floor on the produced node/cell/byte extent — the one termination vocabulary every solver route folds into, composed by its public name. `MeshReceipt.contribute` implements the runtime `ReceiptContributor` port structurally, returning the one-element `Iterable[Receipt]` the port declares — `(Receipt.of("compute.mesh", ("emitted", subject, facts)),)` — through the runtime two-argument `(owner, (phase, subject, facts))` contract; the row carries the operation tag, the derived `converged` flag, and the spread of `.facts` riding as native `float`/`int` scalars through the runtime `Signals` `msgspec` encoder, keyed by the content key. The weave's harvest streams it — `contribute` itself carries no decorator.
- Packages: `skfem` (`Basis`, `asm`, the `MeshLine1`/`MeshTri1`/`MeshTet1`/`MeshQuad1`/`MeshHex1` and `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1` constructors resolved by name through `CTOR`, `BilinearForm`/`LinearForm` the integrand arities the `FemForm` thunks carry, `basis.get_dofs(facets=...)` the polymorphic DOF selector reduced to the global index array through `.flatten()`, `basis.N` the dof count), `meshio` (`read`, `write`, `Mesh`, `CellBlock`, `Mesh.points`/`point_data`/`field_data` and the per-type-merged `cells_dict`/`cell_data_dict`/`cell_sets_dict` read views plus the `cell_data`/`cell_sets`/`point_sets` write-side block-parallel surface, `cell_sets_to_data`/`point_sets_to_data`/`cell_data_to_sets`/`point_data_to_sets` the physical-group promoter/inverter family closing the read-recover/write-promote round-trip, `ReadError`/`WriteError` the boundary-folded failures), `numpy` (`asarray`, `ascontiguousarray`, `linalg.norm` the assembled load-magnitude fold, `issubdtype`/`integer` the integer-label gate keeping a float field out of the set-inversion), stdlib `math` (`isfinite` the assemble well-formedness floor), `expression` (`tag`, `case`, `tagged_union`, `Map` the `CTOR`/`_TOL`/`_SLOTS`/`_MODALITY` table rail), hub (`EvidenceScope`/`evidence_run` — the span/fence/harvest weave), `solvers/receipt.md#RECEIPT` (`SolveStatus`, `status_of` — the shared termination vocabulary and residual-floor verdict, composed by public name), runtime (`RuntimeRail`, `boundary`, `railed` the bound `effect.result` builder the `_dispatch`/`_field` chains thread, `LanePolicy`/`Modality` the offload axis, `ContentIdentity`/`ContentKey` keyed under the `CANONICAL_POLICY` default, `Receipt`/`ReceiptContributor` the contributor port).
- Growth: a new operation (a `Functional` energy-norm evaluation, an adaptive `refined` step) is one `MeshExchange` case plus one `_SLOTS` row plus one `_MODALITY` row sharing the `CTOR` resolution and the status floor; a new element is one `CTOR` row shared with every route; a new assembled artifact field is one slot on `AssembledSystem`; a new format is zero new surface because meshio owns the registry; a new termination class is one `SolveStatus` member shared with every solver route; zero new surface, never a parallel mesh container, never a solve on this owner, never a per-operation factory plus per-operation fact dict beside the `_SLOTS` projection.

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
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.receipts import Receipt


# --- [TYPES] -------------------------------------------------------------------------------

type MeshOp = Literal["assembled", "read", "written"]


class ElementKind(StrEnum):
    # the ONE FEM element vocabulary, owned here beside the constructors that realize it; quadrature
    # and field import it downward — a second declaration anywhere in the folder is a deleted form.
    P1 = "p1"
    P2 = "p2"
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TET_P1 = "tet_p1"
    TET_P2 = "tet_p2"
    QUAD_P1 = "quad_p1"
    HEX_P1 = "hex_p1"


# --- [CONSTANTS] ---------------------------------------------------------------------------

# ElementKind -> (Mesh*-constructor, Element*-constructor, meshio-cell-type) triple, the ONE PUBLIC
# element-spelling table the assemble fold, the FEM solve, the field readout, and the meshio
# round-trip all resolve through. The Mesh*/Element* names resolve through getattr(skfem, ...)
# behind the worker import; the affine Mesh*1 geometry and the cell-type string with their P1
# sibling, varying only the Element*.
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

# Per-operation residual-floor tolerance, one row per MeshOp. None of the three operations carries a
# solve, so every row floors a well-formedness verdict (`0.0` finite, `inf` degenerate) rather than a
# convergence residual: assembly checks `dof_count`/finite `load_norm`, read/write the produced extent.
_TOL: Final[Map[MeshOp, float]] = Map.of_seq([("assembled", 1e-6), ("read", 1e-6), ("written", 1e-6)])

# Per-operation payload field names, one tuple per tag, `key` the common leading slot and `status`
# the common trailing slot. The single owner over the case shapes: the factory packs by it, the
# accessors read fixed slots off it, `.status` reads the last slot, and `.facts` projects each named
# slot — mirroring solvers/receipt.md#RECEIPT `_SLOTS`, so an operation's evidence is one row.
_SLOTS: Final[Map[MeshOp, tuple[str, ...]]] = Map.of_seq([
    ("assembled", ("key", "element", "dof_count", "dirichlet_count", "load_norm", "status")),
    ("read", ("key", "element", "point_count", "cell_count", "status")),
    ("written", ("key", "fmt", "byte_count", "status")),
])

# The family modality rows: native scikit-fem assembly rides the runtime THREAD band; the
# pure-Python meshio interchange runs inline (`None`). The modality is policy DATA beside the route
# tables — never a per-page literal, never a compute-minted limiter.
_MODALITY: Final[Map[str, Modality | None]] = Map.of_seq([("assemble", Modality.THREAD), ("read", None), ("write", None)])


# --- [MODELS] ------------------------------------------------------------------------------


class FemForm(Struct, frozen=True):
    # the weak-form request the assemble fold lowers: the `bilinear`/`linear` slots carry the
    # skfem `BilinearForm`/`LinearForm` integrand thunks typed `object` at the band boundary.
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


# content_key is a stored field, not a property, because ContentIdentity.of runs the fallible
# canonical-derive seam: the key folds once at the `_field` mint inside the `_dispatch` boundary and
# rides as a plain field rather than a derive-returning attribute masquerading as a pure read. No
# gc=False: the record holds tracked ndarray/dict containers, so the leaf-only GC opt-out does not apply.
# `node_sets`/`cell_sets`/`field_data` carry the named physical groups and format metadata the meshio
# `*_sets`/`field_data` surface owns, so a Gmsh/VTK round-trip survives read->write through the
# `cell_data_to_sets`/`cell_sets_to_data` promoter family rather than dropping the region tags on read.
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


# `stiffness` is typed `object`: the assembled matrix is a `scipy.sparse` container (`csr_array`) the
# `solvers/linear.md#LINEAR` `LinearMap.SparseMat(matrix: object, ...)` carrier takes — it sits at the
# band boundary rather than a worker `scipy.sparse` import forced at module load. No gc=False: the
# `load`/`dirichlet_dofs` arrays are tracked containers, so the leaf-only GC opt-out does not apply.
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
        # the family modality row routes the native assemble onto the runtime THREAD band and runs
        # the pure-Python interchange inline; the weave owns span, fence, and receipt harvest. The
        # deterministic assemble takes no retry — worker-death retry rides the process lane only.
        async def dispatch() -> RuntimeRail[MeshReceipt]:
            match _MODALITY[self.tag]:
                case None:
                    return _dispatch(self)
                case modality:
                    return (await lane.offload(_dispatch, self, modality=modality)).bind(lambda rail: rail)

        return await evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch)


# --- [OPERATIONS] --------------------------------------------------------------------------


# `_dispatch` is the one `railed` `effect.result` chain the weave flattens: the read arm
# `yield from`-binds `_read`'s `RuntimeRail[MeshField]` so a meshio parse or canonical-encode fault
# rides the one rail, while the assemble and write arms hold no fallible derive and lift their value
# straight. The resolved `MeshReceipt` is harvested by the weave's `@receipted(REDACTION)` aspect —
# no page-local `_emit`, no inline `Signals.emit`.
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


# `_field` is the one MeshField mint folding the content key from EVERY stored array as a LABELED
# canonical chunk — element tag first, then per block the slot discriminator, the map key, the dtype,
# the shape, and the C-contiguous bytes, each part length-prefixed so the identity owner's `stream`
# modality (a concatenation-absorbing stateful updater) still sees unambiguous boundaries, and the
# dict-owned sections sorted by key so insertion order never leaks into identity. A renamed physical
# group, a reshaped or re-typed array, or a value re-homed across slots therefore re-keys, and two
# `MeshField`s with byte-identical values under different labels never collide in the reuse-fabric
# cache — raw value bytes alone are the deleted keying form. `ContentIdentity.of` returns
# `RuntimeRail[ContentKey]`, so the key threads through the `railed` chain by `yield from`-binding
# the rail — a canonical-encode fault propagates on the one `RuntimeRail` the `_dispatch` chain
# returns rather than a `match`/`raise` re-raise.
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


# The skfem assemble is the THREAD-band kernel `run` offloads: `CTOR` resolves the (Mesh*, Element*)
# pair through getattr behind the deferred native import (module-load lightness, not cycle evasion),
# the .T transpose matches the (dim, n)/(verts, n_elem) layout skfem stores mesh.p/mesh.t in, and asm
# folds the BilinearForm/LinearForm integrand thunks the FemForm carries. `get_dofs(facets=...)` is
# the one polymorphic DOF selector, `.flatten()` reducing the selected view to the global index array
# `condense` accepts as `D=`; the FEM solve consumes AssembledSystem back.
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


# meshio owns the ~40-format registry; file_format disambiguates the .msh/.dat extensions the
# catalogue flags, None triggering extension detection. The FEM-facing topology reads the one element's
# connectivity through `cells_dict[cell_type]` (the merged-by-type view) for the assemble fold, and the
# per-cell field and set arrays read the SAME `cell_type`-keyed views through `cell_data_dict[k][cell_type]`
# /`cell_sets_dict[k][cell_type]` — never `cell_data[k][0]`, whose block-0 array belongs to the first
# `CellBlock` and misaligns with the merged `cells_dict[cell_type]` connectivity on any multi-block mesh
# where the FEM element is not block 0. The `if cell_type in by_type` guard drops a name that does not
# touch the element's block. The physical-group round-trip recovers the FULL `point_sets`/`cell_sets`/
# `field_data` surface: `cell_data_to_sets`/`point_data_to_sets` invert ONLY the integer-kind columns
# (the `np.issubdtype(..., np.integer)` gate) since the meshio inverter expects integer label fields —
# a float solution field stays in `cell_fields` rather than being force-inverted into nonsensical sets,
# while a Gmsh/VTK region tag that crossed as an integer column survives back into `MeshField.cell_sets`.
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


# cell_data field arrays wrap in the single-element per-block list meshio mandates; the named physical
# groups rebind onto `Mesh.point_sets`/`cell_sets` and `field_data`, then `cell_sets_to_data`/
# `point_sets_to_data` promote them into integer label fields so the region tags round-trip through
# formats that carry only integer labels — closing the read-recover/write-promote round-trip a
# write-only `cell_sets_to_data` left half-open. The returned format string rides the written receipt.
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
