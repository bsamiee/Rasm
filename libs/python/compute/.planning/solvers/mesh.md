# [PY_COMPUTE_MESH]

One simulation mesh-and-field interchange and weak-form assembly owner declares the FEM element axis: `ElementKind` and `FemForm` originate here — `solvers/field#FIELD` composes the `CTOR` element table downward, and `solvers/quadrature#QUADRATURE` consumes only the lowered `AssembledSystem`. `MeshField` is the frozen topology-and-field value object; `MeshExchange` discriminates the four transforms a discretized mesh admits — `generate` mints the mesh from boundary data through the gmsh kernel, `assemble` lowers a `FemForm` weak form to the sparse `(stiffness, load, dirichlet_dofs)` system through the scikit-fem `Basis`/`asm` fold, and `read`/`write` round-trip the meshio registry with physical groups intact. This owner holds element vocabulary, generation, assembly, and interchange only; the solve stays on `solvers/quadrature#QUADRATURE`, the transient integration on `solvers/differential#DIFFERENTIAL`, and the meshio `Mesh` is the one container.

Each operation folds into one `MeshCensus` whose `Literal` `tag` IS the operation and whose first payload slot retains the produced `MeshField`, `AssembledSystem`, or emitted `Path` beside its measured facts and shared `SolveStatus`. Content key threads `runtime/evidence/identity#IDENTITY` `ContentIdentity` under the `CANONICAL_POLICY` default and keys the `runtime/execution/lanes#LANE` reuse-fabric cache; each `MeshCensus` projects scalar `attributes` onto the hub `evidence_run` span. `meshio` is pure-Python core and imports top-level; `read`/`write` block on arbitrary-size disk I/O and cross the thread band under `RELEASING` `_TRAIT` rows, while `assemble` and `generate` cross the process band under `HOSTILE` — `skfem.asm` evaluates the caller-supplied Python `FemForm` integrand thunks GIL-held and the gmsh kernel plus its current-model selector are process-global, so a thread arm serializes the loop against the first and races the second — every arm through `lane.offload`, isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, never a per-page literal or compute-minted limiter.

## [01]-[INDEX]

- [02]-[MESH_FIELD]: the FEM element axis owned here, the frozen mesh-and-field value object, the one public `CTOR` element table every route resolves through with its recursive wrapper build and recursive interpolation degree, and the `ContentIdentity` content key over its array buffers.
- [03]-[EXCHANGE]: the `generate`/`assemble`/`read`/`write` operations on one `MeshExchange`, the gmsh generation kernel over its source and size-field axes, the lane-offloaded scikit-fem lowering, the meshio round-trip with physical-group transfer, and the value-carrying `MeshCensus`.

## [02]-[MESH_FIELD]

- Owner: `ElementKind` is the one FEM element vocabulary — the scalar Lagrange rungs, the discontinuous cell constants, the nonconforming Crouzeix-Raviart rung, the per-component vector wrappers, and the mixed velocity-pressure composite — declared here because mesh constructs the elements; `field` imports it downward, and a second element vocabulary anywhere in the folder is rejected. `FemForm` is the weak-form request the assemble fold lowers. `MeshField` is the frozen mesh-and-field value object — the topology the assemble fold reads, `solvers/field#FIELD` consumes, and the interchange round-trips with its physical groups intact — and it never assembles, never solves, never stands up a parallel per-format mesh beside the meshio `Mesh`.
- Cases: `CTOR` is the one public `Map[ElementKind, ElementRow]` element table — mesh constructor, element constructor, meshio cell type, and the inner kinds a wrapper or composite builds from — the generation plan, the assemble fold, the field readout, and the meshio round-trip all resolve through; `field` composes it by name, so the cross-module contract is honest rather than a `_CTOR` private masquerade. A new element is one row shared with every route, never three parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps and never a positional tuple the consumers index by offset.
- Law: every row's `mesh` column is an affine `Mesh*1` constructor and a higher-order kind is a higher-order BASIS over that same geometry, so the generated mesh stays first-order and `_DIM`/`_RECOMBINED` — the meshed dimension and the quad/hex recombination — are the only element facts generation reads. A plan carrying `order: int`/`recombine: bool` is the deleted form twice over: it re-describes an element fact, and its order arm produces a `triangle6`/`tetra10` block neither the `cells_dict` read nor the `Mesh*1` constructor of the row that asked for it can take.
- Auto: `ElementRow.built` is the ONE constructor fold and it recurses through `CTOR` itself, so a wrapper kind names its inner KIND rather than re-spelling an element name — `ElementVector` takes one inner element, `ElementComposite` takes several, a leaf takes none, and a vector-over-composite pair is expressible with no new column. That closure under wrapping is what makes the multi-component readout reachable: a scalar-only vocabulary yields a single-component `basis.split` on every solve, so the component count is a constant and the mixed weak forms — vector elasticity, the inf-sup-stable velocity-pressure pair — have no representable basis at all. `MeshField.content_key` is a stored `ContentKey` minted once through the `_field` single-pass fold under the `CANONICAL_POLICY` default; an explicit `IdentityPolicy()` allocation keys identically by value equality and is pure ceremony. Identical bytes at identical policy key identically, feeding the `runtime/execution/lanes#LANE` `Map[ContentKey, T]` reuse-fabric cache.
- Growth: a new element is one `CTOR` row shared with the generation/quadrature/field routes; a new field array is one `node_fields`/`cell_fields` entry and a new physical group one `node_sets`/`cell_sets` entry, folded into the content key automatically by the `_field` buffer fold; never a parallel mesh container, never a solve on this owner, never a parallel element-spelling map beside `CTOR`, never a write-only physical-group promotion lacking the inbound recovery.
- Boundary: topology, element vocabulary, and field carry only — node coordinates, per-block connectivity, per-node/per-cell field arrays, and the content key; the generation, assemble, and interchange operations live on `MeshExchange`, the solve on `solvers/quadrature#QUADRATURE`, and the transient integration on `solvers/differential#DIFFERENTIAL`. A hand-rolled content digest where `ContentIdentity` owns the concern is rejected, and the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

## [03]-[EXCHANGE]

- Owner: `MeshExchange` is the one `@tagged_union(frozen=True)` operation owner discriminating `generate`, `assemble`, `read`, and `write` rather than a free `MeshField.assemble` method beside a `read`/`write` static pair; each `MeshCensus` case retains that operation's canonical product and measured facts. meshio `Mesh` is the canonical container `read`/`write` project through and meshio owns the ~40-format registry — a hand-rolled format parser, a wrapper-rename of `read`/`write`, a flat `cell_data` dropping the block-parallel list, and a write-only promotion that never recovers on read are all rejected.
- Cases: `generate` mints the mesh gmsh owns — `GmshSource` is the boundary-input axis (a bottom-up planar loop set, an OpenCASCADE primitive, an imported STEP/IGES/BREP or `.geo` model) and `SizeField` the density axis (a uniform target size, or the canonical `Threshold`-over-`Distance` graded refinement installed as the background mesh), so a new source is one case and a new density rule one case, never a second generation entry. The arm terminates in `gmsh.write` and mints its `MeshField` through the SAME `_read` fold every other inbound mesh crosses, so a generated and a read mesh carry identical group and element semantics by construction and no second extraction path, no second element vocabulary, and no gmsh element-type integer table forms beside `CTOR`. Boundary input arrives as data — coordinate arrays, entity tags, a file path — so this owner imports no geometry-branch kernel, exactly as the folder charter rules.
- Law: the promote/recover round-trip reads a USER region alone — `_promotable` refuses a colon-namespaced column because that namespace is the writing format's own bookkeeping, MEASURED on a gmsh `.msh`: the blind integer sweep re-promotes `gmsh:physical` as `set-gmsh:physical-<tag>` beside the named groups meshio already recovered AND promotes `gmsh:geometrical`, an entity-id column naming no region at all, tripling the set roster and re-keying a mesh whose regions never changed. The named `cell_sets` meshio recovers stay authoritative; the gate is the format-agnostic colon discriminant, never a per-format skip list.
- Entry: `MeshExchange.run(lane)` rides the hub weave as `evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch, facts=..., composition=...)`, which owns span and fault fence, while the `MeshCensus` mint retains the operation's product and stamps only scalar `attributes` on that span; the caller's composition key partitions those facts by default-free threading. `dispatch` resolves the `_TRAIT` row through `lane.offload` — the interchange arms ride the `RELEASING` thread band because meshio blocks on disk, the assemble and generate arms ride the `HOSTILE` process band because `skfem.asm` drives the caller's Python form callbacks GIL-held (the closure-bearing `FemForm` thunks cross on the pool's cloudpickle wire) and the gmsh kernel plus its `model.setCurrent` selector are process-global state concurrent in-process callers corrupt — isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, so no arm stalls the loop. meshio `ReadError`/`WriteError`, the gmsh kernel faults its own `logger.getLastError` reports, and the skfem assembly exceptions convert exactly once at the weave's fence into the `BoundaryFault` channel through the `runtime/reliability/faults#FAULT` `CLASSIFY` fold, so a malformed input or unsupported cell type is a typed result, never a raised exception in domain flow.
- Output: `MeshCensus` is the one `@tagged_union(frozen=True)` result whose `Literal` `tag` IS the operation. `generated` and `read` retain the `MeshField`, `assembled` retains the `AssembledSystem`, and `written` retains the emitted `Path`; measured counts, format, key, and terminal `SolveStatus` remain beside that value, while `.attributes` projects only their scalar observations. No operation carries a solve, so every factory floors a well-formedness extent, not a convergence residual, through the shared public `status_of` floor; `assembled` records `load_norm` as evidence yet floors on `dof_count` and a finite load rather than mislabeling a valid large load, and `generated` floors on a non-empty node and cell count while recording the meshed dimension and realized group count as evidence. Every factory closes through `_noted`, stamping `attributes` on the weave span.
- Packages: `gmsh` (the process-global `initialize`/`finalize` bracket, `model.add`, the `geo` bottom-up `addPoint`/`addLine`/`addCurveLoop`/`addPlaneSurface` builders and the `occ` primitive/`importShapes` builders each under their own `synchronize`, `addPhysicalGroup(dim, tags, name=)` the sole region-naming route, the `mesh.field` `Distance`/`Threshold` pair under `setAsBackgroundMesh`, `mesh.setSize`/`generate`/`recombine`/`setOrder`, and `write` emitting the `.msh` meshio reads), `skfem` (`Basis`/`asm`, the `Mesh*1`/`Element*` constructors resolved by name through `CTOR` with `ElementVector`/`ElementComposite` wrapping their inner elements, `get_dofs(facets=...).flatten()` the DOF selector, `basis.N` the dof count), `meshio` (the ~40-format registry, the per-type-merged `cell_data_dict`/`cell_sets_dict` read views, the block-parallel write surface, the `cell_sets_to_data`/`cell_data_to_sets` promoter/inverter family, `ReadError`/`WriteError` boundary-folded), `numpy` (`linalg.norm` the load fold, `issubdtype` the integer-label gate), `math.isfinite`, `expression` (`tagged_union`/`Map` the table carrier), hub (`evidence_run`), `solvers/solve#SOLVE` (`status_of` by public name), runtime (`returns_result` the `effect.result` builder, `Kernel`/`KernelTrait` the offload crossing, `ContentIdentity` under `CANONICAL_POLICY`).
- Growth: a new operation (a `Functional` energy-norm evaluation, an adaptive `refined` step) is one `MeshExchange` case, one product-carrying `MeshCensus` case, and one `_TRAIT` row sharing the `CTOR` resolution and the status floor; a new element is one `CTOR` row, a wrapper or mixed element the same row naming its inner kinds, so an H(div), H(curl), plate, or DG family lands as rows with no builder edit; a new meshable cell shape is one `_DIM` row and, where the kernel reaches it only by recombination, one `_RECOMBINED` member; a new generation source is one `GmshSource` case and one `build` arm; a new density rule is one `SizeField` case and one `install` arm; a new OpenCASCADE primitive is one `GmshSolid` row carrying its own arity; a new assembled field is one slot on `AssembledSystem`; a new format is zero new surface because meshio owns the registry; a new termination class is one `SolveStatus` member; never a parallel mesh container and never a solve on this owner.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from math import isfinite
from pathlib import Path
from typing import Any, Final, Literal, Self, assert_never

import meshio
import msgspec
import numpy as np
from enum import StrEnum
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from opentelemetry import trace

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, StageTap, evidence_run
from rasm.compute.solvers.solve import SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeResult, returns_result, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.observe import DEFAULT_SCOPE, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

lazy import gmsh
lazy import skfem


# --- [TYPES] ----------------------------------------------------------------------------

type MeshOp = Literal["generated", "assembled", "read", "written"]


class MeshStage(StrEnum):
    BUILT = "built"
    GROUPED = "grouped"
    SIZED = "sized"
    MESHED = "meshed"
    WRITTEN = "written"
    READ = "read"


class GmshKernel(StrEnum):
    GEO = "geo"
    OCC = "occ"


class GmshSolid(StrEnum):
    arity: int

    def __new__(cls, ctor: str, arity: int) -> "GmshSolid":
        member = str.__new__(cls, ctor)
        member._value_ = ctor
        member.arity = arity
        return member

    BOX = "addBox", 6
    SPHERE = "addSphere", 4
    CYLINDER = "addCylinder", 7
    TORUS = "addTorus", 5


class ElementKind(StrEnum):
    P1 = "p1"
    P2 = "p2"
    TRI_P0 = "tri_p0"
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TRI_CR = "tri_cr"
    TET_P0 = "tet_p0"
    TET_P1 = "tet_p1"
    TET_P2 = "tet_p2"
    QUAD_P1 = "quad_p1"
    HEX_P1 = "hex_p1"
    VECTOR_TRI_P1 = "vector_tri_p1"
    VECTOR_TRI_P2 = "vector_tri_p2"
    VECTOR_TET_P1 = "vector_tet_p1"
    VECTOR_QUAD_P1 = "vector_quad_p1"
    VECTOR_HEX_P1 = "vector_hex_p1"
    TAYLOR_HOOD_TRI = "taylor_hood_tri"


class ElementRow(Struct, frozen=True):
    mesh: str
    element: str
    cell: str
    bases: tuple[ElementKind, ...] = ()

    def built(self, skfem: Any) -> Any:
        return getattr(skfem, self.element)(*(CTOR[base].built(skfem) for base in self.bases))


# --- [CONSTANTS] ------------------------------------------------------------------------

CTOR: Final[Map[ElementKind, ElementRow]] = Map.of_seq([
    (ElementKind.P1, ElementRow("MeshLine1", "ElementLineP1", "line")),
    (ElementKind.P2, ElementRow("MeshLine1", "ElementLineP2", "line")),
    (ElementKind.TRI_P0, ElementRow("MeshTri1", "ElementTriP0", "triangle")),
    (ElementKind.TRI_P1, ElementRow("MeshTri1", "ElementTriP1", "triangle")),
    (ElementKind.TRI_P2, ElementRow("MeshTri1", "ElementTriP2", "triangle")),
    (ElementKind.TRI_CR, ElementRow("MeshTri1", "ElementTriCR", "triangle")),
    (ElementKind.TET_P0, ElementRow("MeshTet1", "ElementTetP0", "tetra")),
    (ElementKind.TET_P1, ElementRow("MeshTet1", "ElementTetP1", "tetra")),
    (ElementKind.TET_P2, ElementRow("MeshTet1", "ElementTetP2", "tetra")),
    (ElementKind.QUAD_P1, ElementRow("MeshQuad1", "ElementQuad1", "quad")),
    (ElementKind.HEX_P1, ElementRow("MeshHex1", "ElementHex1", "hexahedron")),
    (ElementKind.VECTOR_TRI_P1, ElementRow("MeshTri1", "ElementVector", "triangle", (ElementKind.TRI_P1,))),
    (ElementKind.VECTOR_TRI_P2, ElementRow("MeshTri1", "ElementVector", "triangle", (ElementKind.TRI_P2,))),
    (ElementKind.VECTOR_TET_P1, ElementRow("MeshTet1", "ElementVector", "tetra", (ElementKind.TET_P1,))),
    (ElementKind.VECTOR_QUAD_P1, ElementRow("MeshQuad1", "ElementVector", "quad", (ElementKind.QUAD_P1,))),
    (ElementKind.VECTOR_HEX_P1, ElementRow("MeshHex1", "ElementVector", "hexahedron", (ElementKind.HEX_P1,))),
    (ElementKind.TAYLOR_HOOD_TRI, ElementRow("MeshTri1", "ElementComposite", "triangle", (ElementKind.VECTOR_TRI_P2, ElementKind.TRI_P1))),
])

_TOL: Final[Map[MeshOp, float]] = Map.of_seq([("generated", 1e-6), ("assembled", 1e-6), ("read", 1e-6), ("written", 1e-6)])

_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("generate", KernelTrait.HOSTILE),
    ("assemble", KernelTrait.HOSTILE),
    ("read", KernelTrait.RELEASING),
    ("write", KernelTrait.RELEASING),
])

_DIM: Final[Map[str, int]] = Map.of_seq([("line", 1), ("triangle", 2), ("quad", 2), ("tetra", 3), ("hexahedron", 3)])

_RECOMBINED: Final[frozenset[str]] = frozenset({"quad", "hexahedron"})

_STAGED: Final[frozenset[str]] = frozenset({"generate"})


# --- [TABLES] ---------------------------------------------------------------------------

GMSH_ARITY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MESH, point="generate", arm="config", defect="primitive-arity", retriability=TERMINAL,
    slots=("primitive", "declared", "supplied"),
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([GMSH_ARITY]))


# --- [MODELS] ---------------------------------------------------------------------------


class FemForm(Struct, frozen=True):
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


class PhysicalGroup(Struct, frozen=True):
    name: str
    dim: int
    tags: tuple[int, ...]


@tagged_union(frozen=True)
class GmshSource:
    tag: Literal["planar", "solid", "imported"] = tag()
    planar: tuple[np.ndarray, tuple[tuple[int, ...], ...], float] = case()
    solid: tuple[GmshSolid, tuple[float, ...]] = case()
    imported: tuple[Path, GmshKernel] = case()

    @classmethod
    def Planar(cls, points: np.ndarray, loops: tuple[tuple[int, ...], ...], size: float, /) -> Self:
        return cls(planar=(points, loops, size))

    @classmethod
    def Solid(cls, primitive: GmshSolid, params: tuple[float, ...], /) -> Self:
        return cls(solid=(primitive, params))

    @classmethod
    def Imported(cls, path: Path, kernel: GmshKernel = GmshKernel.OCC, /) -> Self:
        return cls(imported=(path, kernel))

    @property
    def kernel(self) -> GmshKernel:
        match self:
            case GmshSource(tag="planar"):
                return GmshKernel.GEO
            case GmshSource(tag="solid"):
                return GmshKernel.OCC
            case GmshSource(tag="imported", imported=(_, kernel)):
                return kernel
            case _ as unreachable:
                assert_never(unreachable)

    def build(self, gmsh: Any) -> RuntimeResult[tuple[tuple[int, int], ...]]:
        occ, geo = gmsh.model.occ, gmsh.model.geo
        match self:
            case GmshSource(tag="planar", planar=(points, loops, size)):
                nodes = tuple(geo.addPoint(float(x), float(y), float(z), size) for x, y, z in np.asarray(points, dtype=float))
                rings = tuple(tuple(geo.addLine(nodes[a], nodes[b]) for a, b in zip(ring, (*ring[1:], ring[0]), strict=True)) for ring in loops)
                tuple(geo.addPlaneSurface([geo.addCurveLoop(list(curves))]) for curves in rings)
            case GmshSource(tag="solid", solid=(primitive, params)):
                if len(params) != primitive.arity:
                    return Error(GMSH_ARITY.raised(primitive.value, str(primitive.arity), str(len(params))))
                getattr(occ, primitive.value)(*params)
            case GmshSource(tag="imported", imported=(path, kernel)):
                occ.importShapes(str(path)) if kernel is GmshKernel.OCC else gmsh.merge(str(path))
            case _ as unreachable:
                assert_never(unreachable)
        getattr(gmsh.model, self.kernel.value).synchronize()
        return Ok(tuple(gmsh.model.getEntities()))


@tagged_union(frozen=True)
class SizeField:
    tag: Literal["uniform", "graded"] = tag()
    uniform: float = case()
    graded: tuple[tuple[int, ...], float, float, float, float] = case()

    @classmethod
    def Uniform(cls, size: float, /) -> Self:
        return cls(uniform=size)

    @classmethod
    def Graded(cls, curves: tuple[int, ...], size_min: float, size_max: float, dist_min: float, dist_max: float, /) -> Self:
        return cls(graded=(curves, size_min, size_max, dist_min, dist_max))

    def install(self, gmsh: Any) -> None:
        match self:
            case SizeField(tag="uniform", uniform=size):
                gmsh.model.mesh.setSize(gmsh.model.getEntities(0), size)
            case SizeField(tag="graded", graded=(curves, size_min, size_max, dist_min, dist_max)):
                field = gmsh.model.mesh.field
                distance = field.add("Distance")
                field.setNumbers(distance, "CurvesList", list(curves))
                threshold = field.add("Threshold")
                options = (("InField", distance), ("SizeMin", size_min), ("SizeMax", size_max), ("DistMin", dist_min), ("DistMax", dist_max))
                tuple(field.setNumber(threshold, option, value) for option, value in options)
                field.setAsBackgroundMesh(threshold)
            case _ as unreachable:
                assert_never(unreachable)


class MeshPlan(Struct, frozen=True):
    element: ElementKind
    groups: tuple[PhysicalGroup, ...] = ()
    size: SizeField = msgspec.field(default_factory=lambda: SizeField(uniform=0.1))
    optimize: str = ""


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


class AssembledSystem(Struct, frozen=True):
    element: ElementKind
    stiffness: object
    load: np.ndarray
    dirichlet_dofs: np.ndarray
    dof_count: int
    content_key: ContentKey


@tagged_union(frozen=True)
class MeshCensus:
    tag: MeshOp = tag()
    generated: tuple[MeshField, int, int, SolveStatus] = case()
    assembled: tuple[AssembledSystem, float, SolveStatus] = case()
    read: tuple[MeshField, SolveStatus] = case()
    written: tuple[Path, ContentKey, str, int, SolveStatus] = case()

    @classmethod
    def Generated(cls, field: MeshField, dim: int, group_count: int) -> Self:
        extent = 0.0 if field.points.size and field.cells.size else float("inf")
        return cls(generated=(field, dim, group_count, status_of(None, extent, _TOL["generated"])))

    @classmethod
    def Assembled(cls, system: AssembledSystem, load_norm: float) -> Self:
        extent = 0.0 if system.dof_count and isfinite(load_norm) else float("inf")
        return cls(assembled=(system, load_norm, status_of(None, extent, _TOL["assembled"])))

    @classmethod
    def Read(cls, field: MeshField) -> Self:
        extent = 0.0 if field.points.size and field.cells.size else float("inf")
        return cls(read=(field, status_of(None, extent, _TOL["read"])))

    @classmethod
    def Written(cls, path: Path, key: ContentKey, fmt: str, byte_count: int) -> Self:
        extent = 0.0 if byte_count else float("inf")
        return cls(written=(path, key, fmt, byte_count, status_of(None, extent, _TOL["written"])))

    @property
    def value(self) -> MeshField | AssembledSystem | Path:
        match self:
            case (
                MeshCensus(tag="generated", generated=(value, *_))
                | MeshCensus(tag="assembled", assembled=(value, *_))
                | MeshCensus(tag="read", read=(value, *_))
                | MeshCensus(tag="written", written=(value, *_))
            ):
                return value
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def content_key(self) -> ContentKey:
        match self:
            case MeshCensus(tag="generated", generated=(field, *_)) | MeshCensus(tag="read", read=(field, *_)):
                return field.content_key
            case MeshCensus(tag="assembled", assembled=(system, *_)):
                return system.content_key
            case MeshCensus(tag="written", written=(_, key, *_)):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def element(self) -> ElementKind | None:
        match self:
            case MeshCensus(tag="generated", generated=(field, *_)) | MeshCensus(tag="read", read=(field, *_)):
                return field.element
            case MeshCensus(tag="assembled", assembled=(system, *_)):
                return system.element
            case MeshCensus(tag="written"):
                return None
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def status(self) -> SolveStatus:
        match self:
            case (
                MeshCensus(tag="generated", generated=(*_, SolveStatus() as status))
                | MeshCensus(tag="assembled", assembled=(*_, SolveStatus() as status))
                | MeshCensus(tag="read", read=(*_, SolveStatus() as status))
                | MeshCensus(tag="written", written=(*_, SolveStatus() as status))
            ):
                return status
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def converged(self) -> bool:
        return self.status is SolveStatus.SUCCESS

    @property
    def attributes(self) -> dict[str, str | bool | int | float]:
        common: dict[str, str | bool | int | float] = {
            "operation": self.tag,
            "key": self.content_key.hex,
            "converged": self.converged,
            "status": self.status.value,
        }
        match self:
            case MeshCensus(tag="generated", generated=(field, dim, group_count, _)):
                return {
                    **common,
                    "element": field.element.value,
                    "dim": dim,
                    "point_count": int(field.points.shape[0]),
                    "cell_count": int(field.cells.shape[0]),
                    "group_count": group_count,
                }
            case MeshCensus(tag="assembled", assembled=(system, load_norm, _)):
                return {
                    **common,
                    "element": system.element.value,
                    "dof_count": system.dof_count,
                    "dirichlet_count": int(system.dirichlet_dofs.size),
                    "load_norm": load_norm,
                }
            case MeshCensus(tag="read", read=(field, _)):
                return {
                    **common,
                    "element": field.element.value,
                    "point_count": int(field.points.shape[0]),
                    "cell_count": int(field.cells.shape[0]),
                }
            case MeshCensus(tag="written", written=(path, _, fmt, byte_count, _)):
                return {**common, "path": str(path), "fmt": fmt, "byte_count": byte_count}
            case _ as unreachable:
                assert_never(unreachable)

    def _noted(self) -> Self:
        trace.get_current_span().set_attributes(self.attributes)
        return self


@tagged_union(frozen=True)
class MeshExchange:
    tag: Literal["generate", "assemble", "read", "write"] = tag()
    generate: tuple[GmshSource, MeshPlan, Path] = case()
    assemble: tuple[MeshField, FemForm] = case()
    read: tuple[Path, ElementKind, str | None] = case()
    write: tuple[MeshField, Path, str | None] = case()

    @classmethod
    def Generate(cls, source: GmshSource, plan: MeshPlan, path: Path, /) -> Self:
        return cls(generate=(source, plan, path))

    @classmethod
    def Assemble(cls, field: MeshField, form: FemForm, /) -> Self:
        return cls(assemble=(field, form))

    @classmethod
    def Read(cls, path: Path, element: ElementKind, file_format: str | None = None, /) -> Self:
        return cls(read=(path, element, file_format))

    @classmethod
    def Write(cls, field: MeshField, path: Path, file_format: str | None = None, /) -> Self:
        return cls(write=(field, path, file_format))

    async def run(self, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeResult[MeshCensus]:
        mark: Option[StageTap] = Some(StageTap.of(EvidenceScope.MESH, lane.pulses.tap)) if self.tag in _STAGED else Nothing

        async def dispatch() -> RuntimeResult[MeshCensus]:
            return (await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self, mark)).bind(lambda held: held).map(
                lambda census: census._noted()
            )

        return await evidence_run(
            EvidenceScope.MESH, f"mesh.{self.tag}", dispatch, facts={"op": self.tag}, composition=composition, stage=mark
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


@returns_result
def _dispatch(exchange: MeshExchange, mark: Option[StageTap]) -> MeshCensus:
    match exchange:
        case MeshExchange(tag="generate", generate=(source, plan, path)):
            meshed: MeshField = yield from _generate(source, plan, path, mark)
            row = CTOR[plan.element]
            realized = len(meshed.cell_sets) + len(meshed.node_sets)
            return MeshCensus.Generated(meshed, _DIM[row.cell], realized)
        case MeshExchange(tag="assemble", assemble=(field, form)):
            system = _assemble(field, form)
            load_norm = float(np.linalg.norm(system.load))
            return MeshCensus.Assembled(system, load_norm)
        case MeshExchange(tag="read", read=(path, element, fmt)):
            field: MeshField = yield from _read(path, element, fmt)
            return MeshCensus.Read(field)
        case MeshExchange(tag="write", write=(field, path, fmt)):
            written = _write(field, path, fmt)
            return MeshCensus.Written(written, field.content_key, fmt or written.suffix.lstrip("."), int(written.stat().st_size))
        case _ as unreachable:
            assert_never(unreachable)


@returns_result
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
    def cells_of(slot: str, name: str, buf: np.ndarray) -> tuple[bytes, ...]:
        arr = np.ascontiguousarray(buf)
        return (slot.encode(), name.encode(), str(arr.dtype).encode(), repr(arr.shape).encode(), arr.tobytes())

    sections: tuple[tuple[str, dict[str, np.ndarray]], ...] = (
        ("node_fields", node_fields),
        ("cell_fields", cell_fields),
        ("node_sets", node_sets),
        ("cell_sets", cell_sets),
        ("field_data", field_data),
    )
    preimage = (
        element.value.encode(),
        *cells_of("topology", "points", points),
        *cells_of("topology", "cells", cells),
        *(cell for slot, mapping in sections for name in sorted(mapping) for cell in cells_of(slot, name, mapping[name])),
    )
    key: ContentKey = yield from ContentIdentity.of("mesh-field", IdentitySource(parts=preimage))
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


def _assemble(field: MeshField, form: FemForm) -> AssembledSystem:
    row = CTOR[field.element]
    mesh = getattr(skfem, row.mesh)(np.ascontiguousarray(field.points.T), np.ascontiguousarray(field.cells.T))
    basis = skfem.Basis(mesh, row.built(skfem))
    return AssembledSystem(
        element=field.element,
        stiffness=skfem.asm(form.bilinear, basis),
        load=np.asarray(skfem.asm(form.linear, basis)),
        dirichlet_dofs=np.asarray(basis.get_dofs(facets=form.boundary_facets).flatten()),
        dof_count=int(basis.N),
        content_key=field.content_key,
    )


def _generate(source: GmshSource, plan: MeshPlan, path: Path, mark: Option[StageTap]) -> RuntimeResult[MeshField]:
    row = CTOR[plan.element]
    gmsh.initialize()
    try:
        gmsh.model.add(path.stem)
        match source.build(gmsh):
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok"):
                _beat(mark, MeshStage.BUILT, 1)
        tuple(gmsh.model.addPhysicalGroup(group.dim, list(group.tags), name=group.name) for group in plan.groups)
        _beat(mark, MeshStage.GROUPED, 2)
        plan.size.install(gmsh)
        _beat(mark, MeshStage.SIZED, 3)
        gmsh.model.mesh.generate(_DIM[row.cell])
        if row.cell in _RECOMBINED:
            gmsh.model.mesh.recombine()
        if plan.optimize:
            gmsh.model.mesh.optimize(plan.optimize)
        _beat(mark, MeshStage.MESHED, 4)
        gmsh.write(str(path))
        _beat(mark, MeshStage.WRITTEN, 5)
    finally:
        gmsh.finalize()
    read = _read(path, plan.element, "gmsh")
    _beat(mark, MeshStage.READ, 6)
    return read


def _beat(mark: Option[StageTap], stage: MeshStage, done: int) -> None:
    mark.map(lambda held: held.beat(stage, done))


def _region(name: str) -> bool:
    return ":" not in name


def _promotable(column: str, values: np.ndarray) -> bool:
    return _region(column) and bool(np.issubdtype(np.asarray(values).dtype, np.integer))


def _read(path: Path, element: ElementKind, fmt: str | None) -> RuntimeResult[MeshField]:
    cell_type = CTOR[element].cell
    mesh = meshio.read(str(path), fmt)
    for column, blocks in tuple(mesh.cell_data.items()):
        if _promotable(column, np.asarray(blocks[0])):
            mesh.cell_data_to_sets(column)
    for column, values in tuple(mesh.point_data.items()):
        if _promotable(column, np.asarray(values)):
            mesh.point_data_to_sets(column)
    cell_data = mesh.cell_data_dict
    cell_sets = mesh.cell_sets_dict
    return _field(
        element,
        np.asarray(mesh.points),
        np.asarray(mesh.cells_dict[cell_type]),
        {k: np.asarray(v) for k, v in mesh.point_data.items()},
        {k: np.asarray(by_type[cell_type]) for k, by_type in cell_data.items() if cell_type in by_type},
        {k: np.asarray(v) for k, v in mesh.point_sets.items() if _region(k)},
        {k: np.asarray(by_type[cell_type]) for k, by_type in cell_sets.items() if _region(k) and cell_type in by_type},
        {k: np.asarray(v) for k, v in mesh.field_data.items()},
    )


def _write(field: MeshField, path: Path, fmt: str | None) -> Path:
    cell_type = CTOR[field.element].cell
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
    return path
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
