# [PY_COMPUTE_MESH]

The one simulation mesh-and-field interchange and weak-form assembly owner beside the FEM solver route. The owner holds assembly and interchange only — never the solve, never a parallel mesh container beside the meshio `Mesh`.

`MeshField` is the frozen topology-and-field value object carrying the `points` node coordinates, the `cells` connectivity for its `ElementKind`, the per-node `node_fields` and per-cell `cell_fields` array maps, the `node_sets`/`cell_sets` named physical groups, the `field_data` format metadata, and the `content_key` over the canonical mesh-and-field buffer through `ContentIdentity`.

`MeshExchange` is the ONE `@tagged_union` operation owner discriminating the three transforms a discretized mesh-and-field admits: `assemble` lowers a `FemForm` weak form to the sparse `(stiffness, load, dirichlet_dofs)` pair through the scikit-fem `Basis`/`asm` fold, `read` parses any meshio-registry format into a `MeshField` recovering its physical groups, and `write` serializes a `MeshField` back through the meshio format dispatch promoting its physical groups.

Each operation folds into one `MeshReceipt` whose `Literal` `tag` IS the operation and whose per-case payload shape, `.status` read, accessor projection, and observability row are all driven by one `_SLOTS` field-name table — exactly as `solvers/field.md#FIELD` `FieldReceipt` and `solvers/receipt.md#RECEIPT` `SolverReceipt` drive their cases, each terminating in the shared `SolveStatus` verdict the `solvers/receipt.md#RECEIPT` floor adjudicates. The three `@classmethod` factories returning `Self` are the canonical constructors, and receipt emission rides the runtime `@receipted` aspect every solver route wears.

`meshio` is pure-Python and core, so the `read`/`write` interchange runs unconditionally as a top-level import; `scikit-fem` carries no package, so the `assemble` fold rides the worker band behind the boundary.

## [01]-[INDEX]

- [01]-[MESH_FIELD]: the frozen mesh-and-field value object, the one `_CTOR` `(Mesh*, Element*, cell-type)` triple table shared with the assemble/solve/readout routes, and the `ContentIdentity` `stream`-modality content key over its array buffers.
- [02]-[EXCHANGE]: the `assemble`/`read`/`write` operations on one `MeshExchange` `@tagged_union`, the scikit-fem weak-form lowering, the meshio registry round-trip with physical-group transfer, and the `_SLOTS`-driven `MeshReceipt` folded through the shared `SolveStatus` floor under `@receipted`.

## [02]-[MESH_FIELD]

- Owner: `MeshField` — the `msgspec.Struct(frozen=True)` mesh-and-field value object carrying the source `ElementKind`, the `points` node coordinates, the `cells` connectivity for that element, the per-node `node_fields` and per-cell `cell_fields` array maps, the `node_sets`/`cell_sets` named physical groups and `field_data` format metadata the meshio `*_sets`/`field_data` surface owns, and `content_key` the `ContentKey` over the canonical mesh-and-field buffer; it carries no `gc=False` because the record holds tracked `ndarray`/`dict` containers, so the leaf-only GC opt-out the `runtime/evidence/identity.md#IDENTITY` `ContentKey` scalar value object takes does not apply here. The topology is the artifact the assemble fold reads, the readout (`solvers/field.md#FIELD`) consumes, and the interchange round-trips with its physical groups intact; it never assembles, never solves, and never stands up a parallel per-format mesh shape beside the meshio `Mesh`.
- Content key: `MeshField.content_key` is a stored `ContentKey` field minted once through the `_field` single-pass fold — the `points`, `cells`, each `node_fields`/`cell_fields` array, each `node_sets`/`cell_sets` physical-group array, and each `field_data` format-metadata array cross as `ascontiguousarray(...).tobytes()` buffers so the `Source`-alias `Iterable[bytes]` keys the `stream` modality's order-sensitive stateful-updater fold the `runtime/evidence/identity.md#IDENTITY` owner mints, never a hand-rolled `b"\x00".join(tobytes())` concatenation that re-implements the digest. Folding `field_data` is load-bearing: it is a stored field, so omitting it from the digest would key two meshes differing only in their format metadata identically and collide them in the reuse-fabric cache. It is a stored field, NOT a `@property`, because `ContentIdentity.of` returns `RuntimeRail[ContentKey]` over the fallible canonical-derive seam, so `_field` is a `@railed` `effect.result` mint that `yield from`-binds the key off the rail before the construct — a canonical-encode fault propagates on the one `RuntimeRail` the `_dispatch` chain returns, never a `@property` masquerading as a pure attribute read, a `RuntimeRail` bound straight into the `ContentKey` field, or a `match`/`raise` re-raise through a phantom `BoundaryFault.as_exception`. Identical bytes at identical `IdentityPolicy` settings key identically, feeding the reuse-fabric cache the `execution/lanes.md#LANE` `Map[ContentKey, T]` keys.
- Growth: a new element is one `_CTOR` row shared with the quadrature/field routes; a new field array is one entry in `node_fields` or `cell_fields` and a new physical group one entry in `node_sets` or `cell_sets`, automatically folded into the content key by the `_field` buffer fold; zero new surface, never a parallel mesh container beside the meshio `Mesh`, never a solve on this owner, never a parallel element-spelling map beside `_CTOR`, never a write-only physical-group promotion lacking the inbound recovery.
- Boundary: topology and field carry only — the node coordinates, the per-block connectivity, the per-node/per-cell field arrays, and the content key are in-scope; the assemble and interchange operations live on `MeshExchange`, the solve on `solvers/quadrature.md#QUADRATURE`, and the transient integration on `solvers/differential.md#DIFFERENTIAL`. A solve on this owner, a hand-rolled content digest where `ContentIdentity`/`IdentitySource` own the concern, three parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps where one `_CTOR` triple carries every spelling, and a parallel per-format mesh container beside the meshio `Mesh` are the deleted forms; the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

## [03]-[EXCHANGE]

- Owner: `MeshExchange` — the ONE `@tagged_union(frozen=True)` operation owner discriminating `assemble` (a `MeshField` plus a `FemForm` weak form → the sparse `(stiffness, load, dirichlet_dofs)` system through the scikit-fem `Basis`/`asm` fold), `read` (a `Path` plus an `ElementKind` plus an optional `file_format` disambiguator → a `MeshField` projected off the meshio `Mesh`), and `write` (a `MeshField` plus a `Path` plus an optional `file_format` → the meshio serialize), exactly as `solvers/quadrature.md#QUADRATURE` `QuadratureIntent` and `solvers/field.md#FIELD` `FieldQuery` discriminate their operations on one owner rather than a free `MeshField.assemble` method beside a `read`/`write` static-method pair. The three `@classmethod` constructors `Assemble`/`Read`/`Write` return `Self`, binding the subtype once rather than a `@staticmethod`-plus-`"MeshExchange"`-forward-ref re-spelled three times — the `@classmethod`-plus-`Self` factory law the sibling `MeshReceipt`, `solvers/receipt.md#RECEIPT` `SolverReceipt`, and `solvers/field.md#FIELD` `FieldQuery` hold; the meshio `Mesh` is the canonical container the `read`/`write` cases project through, never a parallel per-format shape.
- Interchange: the `read` case calls `meshio.read(str(path), file_format)` — passing the optional `file_format` through to disambiguate the ambiguous `.msh`/`.dat` extensions the catalogue flags, `None` triggering extension detection — and projects the meshio `Mesh` onto a `MeshField`: the `points` onto the node coordinates, the `cells_dict[cell_type]` merged connectivity (the `_CTOR[element]` cell-type string) onto the FEM-facing topology, the `point_data` onto `node_fields`, the `cell_data_dict[k][cell_type]` per-type-aligned array onto `cell_fields` (the merged-by-type view matching the connectivity, never the block-0 `cell_data[k][0]` array that misaligns on a multi-block mesh), and the FULL physical-group surface onto `node_sets`/`cell_sets`/`field_data` through the type-aligned `cell_sets_dict[k][cell_type]`. The read first inverts only the integer-kind `cell_data`/`point_data` columns a Gmsh/VTK source wrote — the `np.issubdtype(..., np.integer)` gate, since the meshio inverter expects integer label fields — through `mesh.cell_data_to_sets(column)`/`mesh.point_data_to_sets(column)`, so a region tag that crossed as an integer column recovers into named sets while a float solution field stays in `cell_fields` rather than being force-inverted, the inverse of the write promotion. The `write` case builds `meshio.Mesh(points, [meshio.CellBlock(cell_type, cells)], point_data=..., cell_data=..., field_data=..., point_sets=..., cell_sets=...)` — the `cell_data` field arrays and the `cell_sets` index arrays each wrapped in the single-element per-block list the meshio block-parallel contract mandates — then promotes the named groups through `mesh.cell_sets_to_data()`/`mesh.point_sets_to_data()` so a Gmsh/VTK round-trip carries the region tags through formats that only support integer labels, and calls `meshio.write(str(path), mesh, file_format)` with extension-driven or explicit format detection. A hand-rolled format parser, a wrapper-rename of `read`/`write`, a flat `cell_data` array dropping the block-parallel list, and a write-only `cell_sets_to_data` that promotes physical groups outbound but never recovers them on read are the deleted forms; meshio owns the ~40-format registry and this owner composes its full `Mesh` surface.
- Entry: `MeshExchange.run` enters one `boundary(f"mesh.{self.tag}", lambda: _dispatch(self)).bind(lambda rail: rail)` returning `RuntimeRail[MeshReceipt]`, joining the inner `_dispatch` rail without double-wrapping — the rail-join shape `numerics/array.md#PAYLOAD` `ArrayPayload.admit` and `optimization/design.md#DESIGN` `DesignProblem.solve` hold. `_dispatch` is the `@railed` `effect.result` chain whose `match` dispatches the three cases through total `assert_never` exhaustion: the assemble arm folds the assembled system into the `assembled` case, the read arm `yield from`-binds `_read`'s `RuntimeRail[MeshField]` and folds the projected node/cell counts into the `read` case, and the write arm the serialized byte length and format into the `written` case. The `assemble` and `write` arms hold no fallible derive and reuse the `field.content_key` the mesh owner already minted; only the `read` arm threads the `_field` `ContentIdentity.of` rail, so no case re-digests a buffer it already holds a key for and a canonical-encode fault rides the one rail rather than a `match`/`raise` re-raise. Each arm folds into the `@receipted(_REDACTION)` `_emit` kernel that returns the `MeshReceipt` contributor and emits its harvested stream on exit, so receipt production is a decorator rail rather than an inline `Signals.emit` — `@receipted` decorates the pure builder that returns the contributor, never the receipt's own `contribute`, exactly as the runtime owner declares. The meshio `ReadError`/`WriteError` and the skfem assembly exceptions convert exactly once at the `boundary` fence into the `BoundaryFault` rail through the `runtime/reliability/faults.md#FAULT` `CLASSIFY` fold, so a malformed input or an unsupported cell type is a typed rail rather than a raised exception in domain flow.
- Receipt: `MeshReceipt` is the ONE `@tagged_union(frozen=True)` receipt whose `Literal` `tag` IS the operation, read directly through `.tag`. The per-case payload shapes are NOT hand-spelled: one `_SLOTS` table names each operation's payload field sequence — `assembled → (key, element, dof_count, dirichlet_count, load_norm, status)`, `read → (key, element, point_count, cell_count, status)`, `written → (key, fmt, byte_count, status)` — with `key` the common leading slot and `status` the common trailing slot, exactly as `solvers/receipt.md#RECEIPT` `_SLOTS` drives `SolverReceipt`. That single table drives the structural shape, the `.status` trailing-slot read through one `case (*_, status)` pattern closed by `assert_never`, every named accessor (`.content_key`/`.element` read off `.facts`, never parallel `getattr(self, self.tag)[N]` properties), and the `.facts` projection through `zip(_SLOTS[self.tag], payload, strict=True)` — so the table and the case tuples cannot drift. The three `@classmethod` factories `Assembled`/`Read`/`Written` return `Self`, binding the subtype once rather than a `@staticmethod`-plus-`"MeshReceipt"`-forward-ref re-spelled three times, exactly as `solvers/receipt.md#RECEIPT` `SolverReceipt` and `solvers/field.md#FIELD` `FieldReceipt` do. None of the three operations carries a solve, so every factory adjudicates a well-formedness extent — not a convergence residual — through the shared `_status(None, extent, _TOL[op])` floor imported from `solvers/receipt.md#RECEIPT`: `assembled` records the `load_norm` magnitude as graduation evidence yet floors `SUCCESS` on `dof_count` and a finite load rather than mislabeling a large-but-valid load as `STAGNATION`, while `read`/`written` floor on the produced node/cell/byte extent. This is the same cross-module private `_status` import `solvers/field.md#FIELD` uses, so the mesh receipt reuses the one termination vocabulary every solver route folds into. `MeshReceipt.contribute` implements the runtime `ReceiptContributor` port structurally, returning the one-element `Iterable[Receipt]` the port declares — `(Receipt.of("compute.mesh", ("emitted", subject, facts)),)` — through the runtime two-argument `(owner, (phase, subject, facts))` contract, never the four-positional form the runtime owner deletes and never a bare `Receipt` against the `Iterable[Receipt]` port the siblings yield a one-element tuple for; the row carries the operation tag, the derived `converged` flag, and the spread of `.facts` riding as native `float`/`int` scalars through the runtime `Signals` `msgspec` encoder, never a `str()` coerce, keyed by the content key. `contribute` itself carries NO `@receipted` decorator — the aspect wraps the `_emit` builder that returns the contributor, exactly as `solvers/receipt.md#RECEIPT` `SolverReceipt.contribute` stays an undecorated port method.
- Packages: `skfem` (`Basis`, `asm`, the `MeshLine1`/`MeshTri1`/`MeshTet1`/`MeshQuad1`/`MeshHex1` and `ElementLineP1`/`ElementLineP2`/`ElementTriP1`/`ElementTriP2`/`ElementTetP1`/`ElementTetP2`/`ElementQuad1`/`ElementHex1` constructors resolved by name through `_CTOR`, `BilinearForm`/`LinearForm` the integrand arities the `FemForm` thunks carry, `basis.get_dofs(facets=...)` the polymorphic DOF selector reduced to the global index array through `.flatten()`, `basis.N` the dof count), `meshio` (`read`, `write`, `Mesh`, `CellBlock`, `Mesh.points`/`point_data`/`field_data` and the per-type-merged `cells_dict`/`cell_data_dict`/`cell_sets_dict` read views plus the `cell_data`/`cell_sets`/`point_sets` write-side block-parallel surface, `cell_sets_to_data`/`point_sets_to_data`/`cell_data_to_sets`/`point_data_to_sets` the physical-group promoter/inverter family closing the read-recover/write-promote round-trip, `ReadError`/`WriteError` the boundary-folded failures), `numpy` (`asarray`, `ascontiguousarray`, `linalg.norm` the assembled load-magnitude fold, `issubdtype`/`integer` the integer-label gate keeping a float field out of the set-inversion), stdlib `math` (`isfinite` the assemble well-formedness floor, the dependency-free scalar primitive matching `solvers/receipt.md#RECEIPT`), `expression` (`tag`, `case`, `tagged_union`, `Map.empty` the empty `_REDACTION` classification map), `solvers/quadrature.md#QUADRATURE` (`ElementKind`, `FemForm` — the shared element enum and weak-form axis the assemble reads, the solve route consuming `AssembledSystem` back), `solvers/receipt.md#RECEIPT` (`SolveStatus`, `_status` — the shared termination vocabulary and residual-floor verdict), runtime (`RuntimeRail`, `boundary`, `railed` the bound `effect.result` builder the `_dispatch`/`_field` chains thread, the `@receipted`/`Redaction` aspect pair, `ContentIdentity`/`ContentKey`/`IdentityPolicy`, `Receipt`/`ReceiptContributor` the contributor port, `Signals` the encoder the facts ride — referenced as the egress contract the aspect drives, not imported directly).
- Growth: a new operation (a `Functional` energy-norm evaluation, an adaptive `refined` step) is one `MeshExchange` case plus one `_SLOTS` row sharing the `_CTOR` resolution and the status floor; a new element is one `_CTOR` row shared with every route; a new assembled artifact field is one slot on `AssembledSystem`; a new format is zero new surface because meshio owns the registry; a new termination class is one `SolveStatus` member shared with every solver route; zero new surface, never a parallel mesh container, never a solve on this owner, never a per-operation factory plus per-operation fact dict beside the `_SLOTS` projection.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from math import isfinite
from pathlib import Path
from typing import Literal, Self, assert_never

import meshio  # core pure-Python; unconditional top-level, never deferred behind TYPE_CHECKING
import numpy as np
from beartype import FrozenDict
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct

from rasm.compute.solvers.quadrature import ElementKind, FemForm
from rasm.compute.solvers.receipt import SolveStatus, _status
from rasm.runtime.content_identity import ContentIdentity, ContentKey, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary, railed
from rasm.runtime.receipts import Receipt, Redaction, receipted


# --- [TYPES] -------------------------------------------------------------------------------

type MeshOp = Literal["assembled", "read", "written"]


# --- [CONSTANTS] ---------------------------------------------------------------------------

# ElementKind -> (Mesh*-constructor, Element*-constructor, meshio-cell-type) triple, the ONE
# element-spelling table the assemble fold, the FEM solve, the field readout, and the meshio
# round-trip all resolve through — collapsing the prior parallel _ELEMENT_CTOR/_MESH_CTOR/_CELL_TYPE
# maps. The Mesh*/Element* names resolve through getattr(skfem, ...) behind the gated import; the
# affine Mesh*1 geometry and the cell-type string with their P1 sibling, varying only the Element*.
_CTOR: FrozenDict[ElementKind, tuple[str, str, str]] = FrozenDict(
    {
        ElementKind.P1: ("MeshLine1", "ElementLineP1", "line"),
        ElementKind.P2: ("MeshLine1", "ElementLineP2", "line"),
        ElementKind.TRI_P1: ("MeshTri1", "ElementTriP1", "triangle"),
        ElementKind.TRI_P2: ("MeshTri1", "ElementTriP2", "triangle"),
        ElementKind.TET_P1: ("MeshTet1", "ElementTetP1", "tetra"),
        ElementKind.TET_P2: ("MeshTet1", "ElementTetP2", "tetra"),
        ElementKind.QUAD_P1: ("MeshQuad1", "ElementQuad1", "quad"),
        ElementKind.HEX_P1: ("MeshHex1", "ElementHex1", "hexahedron"),
    }
)

# Per-operation residual-floor tolerance, one row per MeshOp. None of the three operations carries a
# solve, so every row floors a well-formedness verdict (`0.0` finite, `inf` degenerate) rather than a
# convergence residual: assembly checks `dof_count`/finite `load_norm`, read/write the produced extent.
_TOL: FrozenDict[MeshOp, float] = FrozenDict({"assembled": 1e-6, "read": 1e-6, "written": 1e-6})

# Per-operation payload field names, one tuple per tag, `key` the common leading slot and `status`
# the common trailing slot. The single owner over the case shapes: the factory packs by it, the
# accessors read fixed slots off it, `.status` reads the last slot, and `.facts` projects each named
# slot — mirroring solvers/receipt.md#RECEIPT `_SLOTS`, so an operation's evidence is one row.
_SLOTS: FrozenDict[MeshOp, tuple[str, ...]] = FrozenDict(
    {
        "assembled": ("key", "element", "dof_count", "dirichlet_count", "load_norm", "status"),
        "read": ("key", "element", "point_count", "cell_count", "status"),
        "written": ("key", "fmt", "byte_count", "status"),
    }
)

# Field-redaction policy the @receipted aspect binds; the mesh facts carry no secret, so the
# classification Map is empty and every fact reaches the line natively — the one policy object
# the aspect threads, never a per-call construction.
_REDACTION: Redaction = Redaction(classified=Map.empty())


# --- [MODELS] ------------------------------------------------------------------------------

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


# `stiffness` is typed `object`: the assembled matrix is a `scipy.sparse` container (`csr_array`/
# carrier `solvers/linear.md#LINEAR` `LinearMap.SparseMat(matrix: object, ...)` takes — sits at the band
# boundary rather than a gated `scipy.sparse` import forced at module load. No gc=False: the `load`/
# `dirichlet_dofs` arrays are tracked containers, so the leaf-only GC opt-out does not apply, exactly
# as `MeshField` declines it for the same reason.
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
        return cls(assembled=(key, element, dof_count, dirichlet_count, load_norm, _status(None, extent, _TOL["assembled"])))

    @classmethod
    def Read(cls, key: ContentKey, element: ElementKind, point_count: int, cell_count: int) -> Self:
        extent = 0.0 if point_count and cell_count else float("inf")
        return cls(read=(key, element, point_count, cell_count, _status(None, extent, _TOL["read"])))

    @classmethod
    def Written(cls, key: ContentKey, fmt: str, byte_count: int) -> Self:
        extent = 0.0 if byte_count else float("inf")
        return cls(written=(key, fmt, byte_count, _status(None, extent, _TOL["written"])))

    @property
    def facts(self) -> dict[str, object]:
        match self:
            case (
                MeshReceipt(tag="assembled", assembled=payload)
                | MeshReceipt(tag="read", read=payload)
                | MeshReceipt(tag="written", written=payload)
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

    def run(self) -> RuntimeRail[MeshReceipt]:
        return boundary(f"mesh.{self.tag}", lambda: _dispatch(self)).bind(lambda rail: rail)


# --- [OPERATIONS] --------------------------------------------------------------------------

# `_dispatch` is the one `railed` `effect.result` chain `run` joins through `bind`: the read arm
# `yield from`-binds `_read`'s `RuntimeRail[MeshField]` so a meshio parse or canonical-encode fault
# rides the one rail, while the assemble and write arms hold no fallible derive and lift their value
# straight. The chain folds each arm into the `@receipted` `_emit` kernel that returns the `MeshReceipt`
# contributor and emits its `.contribute()` stream on exit, so receipt egress is a decorator rail on the
# pure builder while the fallible read derive stays on the rail — the `railed`-over-`match`/`raise` ROP
# collapse `numerics/array.md#PAYLOAD` `_admit` runs, never a `BoundaryFault.as_exception` re-raise.
@railed
def _dispatch(exchange: MeshExchange) -> MeshReceipt:
    match exchange:
        case MeshExchange(tag="assemble", assemble=(field, form)):
            system = _assemble(field, form)
            load_norm = float(np.linalg.norm(system.load))
            return _emit(MeshReceipt.Assembled(system.content_key, system.element, system.dof_count, int(system.dirichlet_dofs.size), load_norm))
        case MeshExchange(tag="read", read=(path, element, fmt)):
            field: MeshField = yield from _read(path, element, fmt)
            return _emit(MeshReceipt.Read(field.content_key, element, int(field.points.shape[0]), int(field.cells.shape[0])))
        case MeshExchange(tag="write", write=(field, path, fmt)):
            written = _write(field, path, fmt)
            return _emit(MeshReceipt.Written(field.content_key, written, int(path.stat().st_size)))
        case _ as unreachable:
            assert_never(unreachable)


# `@receipted(_REDACTION)` wraps the pure builder that returns the `MeshReceipt` contributor and emits
# its `.contribute()` stream on exit, so receipt production is a decorator rail rather than an inline
# `Signals.emit`. The aspect decorates this kernel — never the receipt's own `contribute` — matching the
# runtime owner's "wraps a ReceiptContributor-returning op"; `_dispatch` calls it once the rail resolves.
@receipted(_REDACTION)
def _emit(receipt: MeshReceipt) -> MeshReceipt:
    return receipt


# `_field` is the one MeshField mint folding the content key from EVERY stored array in a single pass —
# `points`/`cells`/`node_fields`/`cell_fields`/`node_sets`/`cell_sets`/`field_data` each cross as
# C-contiguous bytes keying the identity owner's `stream` modality (order-sensitive stateful-updater
# fold) without a hand-rolled join, so a physical-group OR format-metadata change re-keys the topology
# and two `MeshField`s differing only in `field_data` never collide in the reuse-fabric cache. `ContentIdentity.of` returns `RuntimeRail[ContentKey]`
# (its `derived` aspect fault-fences the canonical-encode against `EncodeError`), so the key threads
# through the `railed` chain by `yield from`-binding the rail — a canonical-encode fault propagates on
# the one `RuntimeRail` the `_dispatch` chain returns rather than a `match`/`raise` re-raise through a
# phantom `BoundaryFault.as_exception`, the `railed`-over-`match` ROP collapse the runtime
# `reliability/faults#FAULT` owner names and `numerics/array.md#PAYLOAD` `_admit` runs.
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
    buffers = tuple(
        np.ascontiguousarray(buf).tobytes()
        for buf in (points, cells, *node_fields.values(), *cell_fields.values(), *node_sets.values(), *cell_sets.values(), *field_data.values())
    )
    key: ContentKey = yield from ContentIdentity.of("mesh-field", buffers, IdentityPolicy())
    return MeshField(
        element=element, points=points, cells=cells, node_fields=node_fields, cell_fields=cell_fields,
        node_sets=node_sets, cell_sets=cell_sets, field_data=field_data, content_key=key,
    )


# The skfem assemble rides the worker lane: _CTOR resolves the (Mesh*, Element*) pair through getattr
# behind the import, the .T transpose matches the (dim, n)/(verts, n_elem) layout skfem stores
# mesh.p/mesh.t in, and asm folds the BilinearForm/LinearForm integrand thunks the FemForm carries.
# `get_dofs(facets=...)` is the one polymorphic DOF selector keyed on the named `facets` keyword (never a
# parallel get_boundary_dofs), `.flatten()` reducing the selected view to the global index array `condense`
# accepts as `D=`; the FEM solve consumes AssembledSystem back.
def _assemble(field: MeshField, form: FemForm) -> AssembledSystem:
    import skfem

    mesh_ctor, element_ctor, _ = _CTOR[field.element]
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
    *_, cell_type = _CTOR[element]
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
    *_, cell_type = _CTOR[field.element]
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

- [SHARED_RECEIPT_FOLD]: `MeshReceipt` mirrors `solvers/receipt.md#RECEIPT` `SolverReceipt` and `solvers/field.md#FIELD` `FieldReceipt` end to end — the `Literal` `tag` IS the operation, the per-case shape is driven by one `_SLOTS` `FrozenDict[MeshOp, tuple[str, ...]]` rather than hand-spelled three times, the three `@classmethod` factories `Assembled`/`Read`/`Written` return `Self` (never a `@staticmethod`-plus-forward-ref the receipt owner deletes), `.status` reads the trailing slot through one `case (*_, status)` pattern closed by `assert_never`, `.content_key`/`.element` read fixed slots off `_SLOTS` through `.facts` rather than parallel `getattr(self, self.tag)[N]` properties, and `.facts` zips `_SLOTS[self.tag]` against the case tuple under `strict=True` so the table and the case shapes cannot drift. No mesh operation carries a solve, so each factory adjudicates a well-formedness extent through the shared `_status(None, extent, _TOL[op])` floor imported from the receipt owner — `assembled` flooring on `dof_count`/finite `load_norm`, `read`/`written` on the produced extent — the same cross-module private import `solvers/field.md#FIELD` uses, so the mesh receipt reuses the one termination vocabulary every solver route folds into rather than smuggling the load magnitude into the verdict slot. `MeshReceipt.contribute` is the undecorated `ReceiptContributor` port method returning the one-element `Iterable[Receipt]` tuple through the runtime two-argument `Receipt.of("compute.mesh", ("emitted", subject, facts))` contract, the facts riding as native scalars through the runtime `Signals` `msgspec` encoder rather than a `str()` coerce; the `@receipted(_REDACTION)` aspect wears the `_emit` builder that returns the contributor, so receipt production is a decorator rail rather than an inline `Signals.emit` and the aspect never decorates the receipt's own `contribute` — exactly as `solvers/receipt.md#RECEIPT` `SolverReceipt.contribute` stays an undecorated port method. The `_dispatch` chain is `@railed` and `run` joins it through `boundary(...).bind(lambda rail: rail)`, the rail-join shape `numerics/array.md#PAYLOAD` `ArrayPayload.admit` holds, so the read-arm `ContentIdentity.of` fault rides the one rail rather than the `@receipted`-on-`_dispatch` shape the prior page drifted to.
- [CONTENT_KEY_STREAM]: `MeshField.content_key` folds the `points`, `cells`, each `node_fields`/`cell_fields` array, each `node_sets`/`cell_sets` physical-group array, and each `field_data` format-metadata array as `ascontiguousarray(...).tobytes()` buffers into a `tuple[bytes, ...]` and hands it to `ContentIdentity.of("mesh-field", buffers, IdentityPolicy())`, whose `Source` alias (`bytes | Iterable[bytes] | tuple[ContentKey, ...] | Struct` per `runtime/evidence/identity.md#IDENTITY`) routes the `Iterable[bytes]` to the `stream` modality the owner's `IdentitySource.lift` selects — the order-sensitive stateful-updater fold, never the caller pre-calling `IdentitySource.lift` since `of` lifts internally. This replaces the prior hand-rolled `b"\x00".join(tobytes())` concatenation that re-implemented the digest the identity owner owns; the `runtime/evidence/identity.md#IDENTITY` `[02]-[IDENTITY]` Boundary row names a hand-rolled second hashing owner as a deleted form. `ascontiguousarray` guarantees the C-contiguous layout the buffer crosses on per the `numpy` `[STACKS_WITH]` msgspec wire-round-trip row. The key feeds the reuse-fabric cache the `execution/lanes.md#LANE` `Map[ContentKey, T]` keys, so a re-assembled mesh at identical settings is a cache hit by reference. `ContentIdentity.of` returns `RuntimeRail[ContentKey]` at the identity owner; `_field` is a `@railed` `effect.result` mint that `yield from`-binds the key off the rail (`key = yield from ContentIdentity.of(...)`) so a canonical-encode fault propagates on the one `RuntimeRail` the `_dispatch` chain returns — the `railed`-over-`match`/`raise` ROP collapse `numerics/array.md#PAYLOAD` `_admit` runs, the derive's own `derived` aspect fencing the canonical-encode while the `run` `boundary` fences the enclosing mesh op — never a rail bound straight into the `ContentKey` field and never a `case Error(fault)`/`raise fault.as_exception()` re-raise through a phantom exception bridge.
