# [PY_COMPUTE_MESH]

One simulation mesh-and-field interchange and weak-form assembly owner declares the FEM element axis: `ElementKind` and `FemForm` originate here — `solvers/field#FIELD` composes the `CTOR` element table downward, and `solvers/quadrature#QUADRATURE` consumes only the lowered `AssembledSystem`. `MeshField` is the frozen topology-and-field value object; `MeshExchange` discriminates the four transforms a discretized mesh admits — `generate` mints the mesh from boundary data through the gmsh kernel, `assemble` lowers a `FemForm` weak form to the sparse `(stiffness, load, dirichlet_dofs)` system through the scikit-fem `Basis`/`asm` fold, and `read`/`write` round-trip the meshio registry with physical groups intact. This owner holds element vocabulary, generation, assembly, and interchange only; the solve stays on `solvers/quadrature#QUADRATURE`, the transient integration on `solvers/differential#DIFFERENTIAL`, and the meshio `Mesh` is the one container.

Each operation folds into one `MeshReceipt` whose `Literal` `tag` IS the operation and whose payload shape, `.status` read, accessors, and observability row are all driven by one `_SLOTS` field-name table, as `solvers/field#FIELD` `FieldReceipt` and `solvers/receipt#RECEIPT` `SolverReceipt` drive theirs, each terminating in the shared `SolveStatus` the `solvers/receipt#RECEIPT` `status_of` floor adjudicates by public name. Content key threads `runtime/evidence/identity#IDENTITY` `ContentIdentity` under the `CANONICAL_POLICY` default and keys the `runtime/execution/lanes#LANE` reuse-fabric cache; receipt emission rides the hub `evidence_run` weave. `meshio` is pure-Python core and imports top-level; `read`/`write` block on arbitrary-size disk I/O and cross the thread band under `RELEASING` `_TRAIT` rows, while `assemble` and `generate` cross the process band under `HOSTILE` — `skfem.asm` evaluates the caller-supplied Python `FemForm` integrand thunks GIL-held and the gmsh kernel plus its current-model selector are process-global, so a thread arm serializes the loop against the first and races the second — every arm through `lane.offload`, isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, never a per-page literal or compute-minted limiter.

## [01]-[INDEX]

- [02]-[MESH_FIELD]: the FEM element axis owned here, the frozen mesh-and-field value object, the one public `CTOR` element table every route resolves through with its recursive wrapper build and recursive interpolation degree, and the `ContentIdentity` content key over its array buffers.
- [03]-[EXCHANGE]: the `generate`/`assemble`/`read`/`write` operations on one `MeshExchange`, the gmsh generation kernel over its source and size-field axes, the lane-offloaded scikit-fem lowering, the meshio round-trip with physical-group transfer, and the `_SLOTS`-driven `MeshReceipt`.

## [02]-[MESH_FIELD]

- Owner: `ElementKind` is the one FEM element vocabulary — the scalar Lagrange rungs, the discontinuous cell constants, the nonconforming Crouzeix-Raviart rung, the per-component vector wrappers, and the mixed velocity-pressure composite — declared here because mesh constructs the elements; `field` imports it downward, and a second element vocabulary anywhere in the folder is rejected. `FemForm` is the weak-form request the assemble fold lowers. `MeshField` is the frozen mesh-and-field value object — the topology the assemble fold reads, `solvers/field#FIELD` consumes, and the interchange round-trips with its physical groups intact — and it never assembles, never solves, never stands up a parallel per-format mesh beside the meshio `Mesh`.
- Cases: `CTOR` is the one public `Map[ElementKind, ElementRow]` element table — mesh constructor, element constructor, meshio cell type, and the inner kinds a wrapper or composite builds from — the generation plan, the assemble fold, the field readout, and the meshio round-trip all resolve through; `field` composes it by name, so the cross-module contract is honest rather than a `_CTOR` private masquerade. A new element is one row shared with every route, never three parallel `_ELEMENT_CTOR`/`_MESH_CTOR`/`_CELL_TYPE` maps and never a positional tuple the consumers index by offset.
- Law: every row's `mesh` column is an affine `Mesh*1` constructor and a higher-order kind is a higher-order BASIS over that same geometry, so the generated mesh stays first-order and `_DIM`/`_RECOMBINED` — the meshed dimension and the quad/hex recombination — are the only element facts generation reads. A plan carrying `order: int`/`recombine: bool` is the deleted form twice over: it re-describes an element fact, and its order arm produces a `triangle6`/`tetra10` block neither the `cells_dict` read nor the `Mesh*1` constructor of the row that asked for it can take.
- Auto: `ElementRow.built` is the ONE constructor fold and it recurses through `CTOR` itself, so a wrapper kind names its inner KIND rather than re-spelling an element name — `ElementVector` takes one inner element, `ElementComposite` takes several, a leaf takes none, and a vector-over-composite pair is expressible with no new column. That closure under wrapping is what makes the multi-component readout reachable: a scalar-only vocabulary yields a single-component `basis.split` on every solve, so the component count is a constant and the mixed weak forms — vector elasticity, the inf-sup-stable velocity-pressure pair — have no representable basis at all. `MeshField.content_key` is a stored `ContentKey` minted once through the `_field` single-pass fold under the `CANONICAL_POLICY` default; an explicit `IdentityPolicy()` allocation keys identically by value equality and is pure ceremony. Identical bytes at identical policy key identically, feeding the `runtime/execution/lanes#LANE` `Map[ContentKey, T]` reuse-fabric cache.
- Growth: a new element is one `CTOR` row shared with the generation/quadrature/field routes; a new field array is one `node_fields`/`cell_fields` entry and a new physical group one `node_sets`/`cell_sets` entry, folded into the content key automatically by the `_field` buffer fold; never a parallel mesh container, never a solve on this owner, never a parallel element-spelling map beside `CTOR`, never a write-only physical-group promotion lacking the inbound recovery.
- Boundary: topology, element vocabulary, and field carry only — node coordinates, per-block connectivity, per-node/per-cell field arrays, and the content key; the generation, assemble, and interchange operations live on `MeshExchange`, the solve on `solvers/quadrature#QUADRATURE`, and the transient integration on `solvers/differential#DIFFERENTIAL`. A hand-rolled content digest where `ContentIdentity` owns the concern is rejected, and the mesh shape aligns to the geometry-branch tessellation at the wire and never imports its interior.

## [03]-[EXCHANGE]

- Owner: `MeshExchange` is the one `@tagged_union(frozen=True)` operation owner discriminating `generate`, `assemble`, `read`, and `write` rather than a free `MeshField.assemble` method beside a `read`/`write` static pair; the `@classmethod`-plus-`Self` factory law binds each subtype once, shared with `MeshReceipt`, `solvers/receipt#RECEIPT` `SolverReceipt`, and `solvers/field#FIELD` `FieldQuery`. meshio `Mesh` is the canonical container `read`/`write` project through and meshio owns the ~40-format registry — a hand-rolled format parser, a wrapper-rename of `read`/`write`, a flat `cell_data` dropping the block-parallel list, and a write-only promotion that never recovers on read are all rejected.
- Cases: `generate` mints the mesh gmsh owns — `GmshSource` is the boundary-input axis (a bottom-up planar loop set, an OpenCASCADE primitive, an imported STEP/IGES/BREP or `.geo` model) and `SizeField` the density axis (a uniform target size, or the canonical `Threshold`-over-`Distance` graded refinement installed as the background mesh), so a new source is one case and a new density rule one case, never a second generation entry. The arm terminates in `gmsh.write` and mints its `MeshField` through the SAME `_read` fold every other inbound mesh crosses, so a generated and a read mesh carry identical group and element semantics by construction and no second extraction path, no second element vocabulary, and no gmsh element-type integer table forms beside `CTOR`. Boundary input arrives as data — coordinate arrays, entity tags, a file path — so this owner imports no geometry-branch kernel, exactly as the folder charter rules.
- Law: the promote/recover round-trip reads a USER region alone — `_promotable` refuses a colon-namespaced column because that namespace is the writing format's own bookkeeping, MEASURED on a gmsh `.msh`: the blind integer sweep re-promotes `gmsh:physical` as `set-gmsh:physical-<tag>` beside the named groups meshio already recovered AND promotes `gmsh:geometrical`, an entity-id column naming no region at all, tripling the set roster and re-keying a mesh whose regions never changed. The named `cell_sets` meshio recovers stay authoritative; the gate is the format-agnostic colon discriminant, never a per-format skip list.
- Entry: `MeshExchange.run(lane)` rides the hub weave as `evidence_run(EvidenceScope.MESH, f"mesh.{self.tag}", dispatch, facts=..., composition=...)`, which owns span, fault fence, and receipt harvest, so receipt egress is composed rather than a page-local `_emit`, and the caller's composition key partitions those facts by default-free threading. `dispatch` resolves the `_TRAIT` row through `lane.offload` — the interchange arms ride the `RELEASING` thread band because meshio blocks on disk, the assemble and generate arms ride the `HOSTILE` process band because `skfem.asm` drives the caller's Python form callbacks GIL-held (the closure-bearing `FemForm` thunks cross on the pool's cloudpickle wire) and the gmsh kernel plus its `model.setCurrent` selector are process-global state concurrent in-process callers corrupt — isolation, band, and worker-death retry deriving at the runtime `Kernel` crossing, so no arm stalls the loop. meshio `ReadError`/`WriteError`, the gmsh kernel faults its own `logger.getLastError` reports, and the skfem assembly exceptions convert exactly once at the weave's fence into the `BoundaryFault` rail through the `runtime/reliability/faults#FAULT` `CLASSIFY` fold, so a malformed input or unsupported cell type is a typed rail, never a raised exception in domain flow.
- Receipt: `MeshReceipt` is the one `@tagged_union(frozen=True)` receipt whose `Literal` `tag` IS the operation. One `_SLOTS` row names each payload sequence — `key` leading, `status` trailing — and drives the structural shape, the `.status` read, every named accessor off `.facts` (never parallel `getattr(self, self.tag)[N]` properties), and the `.facts` `zip(..., strict=True)` projection, so the table and the case tuples cannot drift. No operation carries a solve, so every factory floors a well-formedness extent, not a convergence residual, through the shared public `status_of` floor; `assembled` records `load_norm` as evidence yet floors on `dof_count` and a finite load rather than mislabeling a valid large load, and `generated` floors on a non-empty node and cell count while recording the meshed dimension and realized group count as evidence. Weave harvests the resolved receipt; `contribute` carries no decorator.
- Packages: `gmsh` (the process-global `initialize`/`finalize` bracket, `model.add`, the `geo` bottom-up `addPoint`/`addLine`/`addCurveLoop`/`addPlaneSurface` builders and the `occ` primitive/`importShapes` builders each under their own `synchronize`, `addPhysicalGroup(dim, tags, name=)` the sole region-naming route, the `mesh.field` `Distance`/`Threshold` pair under `setAsBackgroundMesh`, `mesh.setSize`/`generate`/`recombine`/`setOrder`, and `write` emitting the `.msh` meshio reads), `skfem` (`Basis`/`asm`, the `Mesh*1`/`Element*` constructors resolved by name through `CTOR` with `ElementVector`/`ElementComposite` wrapping their inner elements, `get_dofs(facets=...).flatten()` the DOF selector, `basis.N` the dof count), `meshio` (the ~40-format registry, the per-type-merged `cell_data_dict`/`cell_sets_dict` read views, the block-parallel write surface, the `cell_sets_to_data`/`cell_data_to_sets` promoter/inverter family, `ReadError`/`WriteError` boundary-folded), `numpy` (`linalg.norm` the load fold, `issubdtype` the integer-label gate), `math.isfinite`, `expression` (`tagged_union`/`Map` the table rail), hub (`evidence_run`), `solvers/receipt#RECEIPT` (`status_of` by public name), runtime (`railed` the `effect.result` builder, `Kernel`/`KernelTrait` the offload crossing, `ContentIdentity` under `CANONICAL_POLICY`, `Receipt`).
- Growth: a new operation (a `Functional` energy-norm evaluation, an adaptive `refined` step) is one `MeshExchange` case, one `_SLOTS` row, and one `_TRAIT` row sharing the `CTOR` resolution and the status floor; a new element is one `CTOR` row, a wrapper or mixed element the same row naming its inner kinds, so an H(div), H(curl), plate, or DG family lands as rows with no builder edit; a new meshable cell shape is one `_DIM` row and, where the kernel reaches it only by recombination, one `_RECOMBINED` member; a new generation source is one `GmshSource` case and one `build` arm; a new density rule is one `SizeField` case and one `install` arm; a new OpenCASCADE primitive is one `GmshSolid` row carrying its own arity; a new assembled field is one slot on `AssembledSystem`; a new format is zero new surface because meshio owns the registry; a new termination class is one `SolveStatus` member; never a parallel mesh container, never a solve on this owner, never a per-operation factory and per-operation fact dict beside the `_SLOTS` projection.

```python signature
# --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
from collections.abc import Iterable
from math import isfinite
from pathlib import Path
from typing import Any, Final, Literal, Self, assert_never

import meshio  # core pure-Python; unconditional top-level, never deferred behind TYPE_CHECKING
import msgspec
import numpy as np
from enum import StrEnum
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.compute.graduation.handoff import ComputeLeg, EvidenceScope, StageTap, evidence_run
from rasm.compute.solvers.receipt import SolveStatus, status_of
from rasm.runtime.identity import ContentIdentity, ContentKey, IdentitySource
from rasm.runtime.faults import TERMINAL, FaultRow, RuntimeRail, railed, rostered
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.receipts import DEFAULT_SCOPE, Provenance, Receipt, ScopeKey
from rasm.runtime.workers import Kernel, KernelTrait

# assemble and generate backends defer: `skfem` pulls a heavy FEM stack and `gmsh` a native process-global kernel, and
# an interchange-only `read`/`write` call never reaches either — neither load falls on a module that only round-trips.
lazy import gmsh
lazy import skfem


# --- [TYPES] -------------------------------------------------------------------------------

type MeshOp = Literal["generated", "assembled", "read", "written"]


class MeshStage(StrEnum):
    # the generate fold's OWN closed milestone roster, and the reason it is closed HERE rather than shared: a stage
    # set is one fold's interior, so a cross-fold phase ladder would name positions this kernel never reaches and
    # leave every subscriber matching on members that cannot fire. The roster erases to `StageMark.stage` at the
    # conduit, so closure costs the registry nothing and buys the fold an exhaustive position vocabulary.
    BUILT = "built"
    GROUPED = "grouped"
    SIZED = "sized"
    MESHED = "meshed"
    WRITTEN = "written"
    READ = "read"


class GmshKernel(StrEnum):
    # the two geometry kernels a gmsh model admits; the value IS the `gmsh.model.<kernel>` namespace attribute the
    # source arm synchronizes through, so a kernel choice is one row rather than a branch re-deciding which
    # `synchronize` to call. A model mixes at most one kernel — the case that builds it names the one it commits.
    GEO = "geo"
    OCC = "occ"


class GmshSolid(StrEnum):
    # value IS the `gmsh.model.occ` constructor and `arity` rides each member, so the parameter vector is gated
    # BEFORE the call and an arity slip is a typed reject rather than the opaque kernel `TypeError` — the same
    # value-plus-arity row `numerics/quantity#QUANTITY` `Umath` carries.
    arity: int

    def __new__(cls, ctor: str, arity: int) -> "GmshSolid":
        member = str.__new__(cls, ctor)
        member._value_ = ctor
        member.arity = arity
        return member

    BOX = "addBox", 6  # (x, y, z, dx, dy, dz)
    SPHERE = "addSphere", 4  # (xc, yc, zc, radius)
    CYLINDER = "addCylinder", 7  # (x, y, z, dx, dy, dz, r)
    TORUS = "addTorus", 5  # (x, y, z, r_major, r_minor)


class ElementKind(StrEnum):
    # ONE vocabulary over every element the weak form admits: the scalar Lagrange rungs, the discontinuous constants,
    # the nonconforming rung, the per-component vector wrappers, and the mixed composite. A vector or composite kind is
    # a row naming its inner kinds, never a second vocabulary beside the scalar one, so `basis.split` reaches a genuine
    # multi-component basis and the component count on the field readout is a measurement rather than a constant.
    P1 = "p1"
    P2 = "p2"
    TRI_P0 = "tri_p0"  # discontinuous cell constant; the pressure half of a P1-P0 pair
    TRI_P1 = "tri_p1"
    TRI_P2 = "tri_p2"
    TRI_CR = "tri_cr"  # Crouzeix-Raviart nonconforming facet element
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
    TAYLOR_HOOD_TRI = "taylor_hood_tri"  # inf-sup-stable velocity-pressure pair: vector P2 over scalar P1


# One element row beside the vocabulary it keys: the affine mesh constructor, the element constructor, the meshio cell
# type, and the INNER kinds a wrapper or composite element constructs from. Named fields, never a positional tuple the
# assemble, readout, and interchange consumers index by offset. `bases` is the growth axis that makes the vocabulary
# closed under wrapping: `ElementVector` takes one inner element and `ElementComposite` takes several, so both families
# are rows over the same column and a deeper pair — a vector over a composite — needs no new shape.
class ElementRow(Struct, frozen=True):
    mesh: str
    element: str
    cell: str
    bases: tuple[ElementKind, ...] = ()

    def built(self, skfem: Any) -> Any:
        # recursion through the SAME table is what keeps one column serving both wrapper families — a leaf row's empty
        # `bases` constructs with no argument, a vector row passes its one inner element, a composite row passes
        # several, each inner element itself built by its own row. Names resolve through `getattr(skfem, ...)` off the
        # module handed in at the call seam, so no spelling is duplicated per family.
        return getattr(skfem, self.element)(*(CTOR[base].built(skfem) for base in self.bases))


# --- [CONSTANTS] ---------------------------------------------------------------------------

# P2 shares its P1 sibling's affine `Mesh*1` geometry and cell-type string, varying only the element; a vector or
# composite kind shares them too, since wrapping changes the DOF layout and never the topology the cell type names.
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

# Per-operation well-formedness floor, one row per MeshOp — no op carries a solve, so a row floors
# `0.0` finite against `inf` degenerate, never a convergence residual.
_TOL: Final[Map[MeshOp, float]] = Map.of_seq([("generated", 1e-6), ("assembled", 1e-6), ("read", 1e-6), ("written", 1e-6)])

# Per-operation payload field names, `key` leading and `status` trailing — the one owner over the case
# shapes the factory packs by and `.facts` projects.
_SLOTS: Final[Map[MeshOp, tuple[str, ...]]] = Map.of_seq([
    ("generated", ("key", "element", "dim", "point_count", "cell_count", "group_count", "status")),
    ("assembled", ("key", "element", "dof_count", "dirichlet_count", "load_norm", "status")),
    ("read", ("key", "element", "point_count", "cell_count", "status")),
    ("written", ("key", "fmt", "byte_count", "status")),
])

# Family trait rows: assemble is HOSTILE — `skfem.asm` evaluates the caller-supplied Python form callbacks GIL-held, so the
# process arm isolates them — and generate is HOSTILE because the gmsh kernel and its `model.setCurrent` selector are
# process-global; read/write block on disk and stay RELEASING; isolation, band, and retry derive at the Kernel crossing.
_TRAIT: Final[Map[str, KernelTrait]] = Map.of_seq([
    ("generate", KernelTrait.HOSTILE),
    ("assemble", KernelTrait.HOSTILE),
    ("read", KernelTrait.RELEASING),
    ("write", KernelTrait.RELEASING),
])

# Topological dimension per cell type — the argument `mesh.generate(dim)` takes, read off the SAME `CTOR` cell column
# the meshio round-trip keys on, so the meshed dimension and the block the read arm recovers can never disagree.
_DIM: Final[Map[str, int]] = Map.of_seq([("line", 1), ("triangle", 2), ("quad", 2), ("tetra", 3), ("hexahedron", 3)])

# cell shapes gmsh reaches only by recombining its simplicial output; membership is the element's own fact, so the
# promotion is derived from the kind the caller names and never a `recombine: bool` knob restating it.
_RECOMBINED: Final[frozenset[str]] = frozenset({"quad", "hexahedron"})

# the arms carrying interior positions worth a beat. `generate` alone drives a multi-phase native kernel whose
# duration a caller cannot otherwise see; the interchange arms are one blocking call each and the assemble arm one
# `skfem.asm`, so opening a pulse stream over them publishes a milestone roster with nothing between its ends.
_STAGED: Final[frozenset[str]] = frozenset({"generate"})


# --- [TABLES] ------------------------------------------------------------------------------

# the arity gate's ONE row: the primitive rides a NAMED slot rather than the subject, because a per-primitive
# subject spelling forks one refusal law across the whole `GmshSolid` roster and seats a census coordinate per
# member — a table no reader can enumerate and no row can own.
GMSH_ARITY: Final[FaultRow[ComputeLeg]] = FaultRow(
    leg=ComputeLeg.MESH, point="generate", arm="config", defect="primitive-arity", retriability=TERMINAL,
    slots=("primitive", "declared", "supplied"),
)
RAISES: Final[Block[FaultRow[ComputeLeg]]] = rostered(Block.of_seq([GMSH_ARITY]))


# --- [MODELS] ------------------------------------------------------------------------------


class FemForm(Struct, frozen=True):
    # bilinear/linear carry the skfem BilinearForm/LinearForm integrand thunks, typed object at the band boundary.
    element: ElementKind
    bilinear: object
    linear: object
    boundary_facets: tuple[str, ...]
    dirichlet: float = 0.0


class PhysicalGroup(Struct, frozen=True):
    # the ONE region-naming route gmsh exposes; the name survives the `.msh` write as a named `cell_sets` entry the
    # read fold recovers, so a group declared here reaches `MeshField.cell_sets`/`node_sets` under the same spelling.
    name: str
    dim: int
    tags: tuple[int, ...]


@tagged_union(frozen=True)
class GmshSource:
    # boundary input as DATA — coordinate arrays, entity tags, a file path — so the generation arm imports no
    # geometry-branch kernel. Each case commits ONE gmsh kernel and synchronizes it; a model mixing both is unspellable.
    tag: Literal["planar", "solid", "imported"] = tag()
    planar: tuple[np.ndarray, tuple[tuple[int, ...], ...], float] = case()
    solid: tuple[GmshSolid, tuple[float, ...]] = case()
    imported: tuple[Path, GmshKernel] = case()

    @classmethod
    def Planar(cls, points: np.ndarray, loops: tuple[tuple[int, ...], ...], size: float, /) -> Self:
        # `points` is the `(n, 3)` coordinate array and each loop a CLOSED ring of row indices; `size` is the
        # per-point target the `addPoint` `meshSize` argument carries, which a background `SizeField` then overrides.
        return cls(planar=(points, loops, size))

    @classmethod
    def Solid(cls, primitive: GmshSolid, params: tuple[float, ...], /) -> Self:
        return cls(solid=(primitive, params))

    @classmethod
    def Imported(cls, path: Path, kernel: GmshKernel = GmshKernel.OCC, /) -> Self:
        # OCC imports a B-Rep exchange file through `importShapes`; GEO merges a `.geo`/`.msh`/`.step` the parser reads.
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

    def build(self, gmsh: Any) -> RuntimeRail[tuple[tuple[int, int], ...]]:
        # returns the synchronized model's own `(dim, tag)` entity roster, so a caller's `PhysicalGroup` tags and the
        # `SizeField` curve lists are checkable against what the kernel actually committed rather than assumed.
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
    # element-density axis: a flat target over every point entity, or the `Threshold`-over-`Distance` pair the gmsh
    # catalog names the canonical graded-refinement source. Each case installs itself, so a new rule is one case and
    # one arm rather than a knob tail the generation body re-reads.
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
                # dimension `0` selects the point entities `setSize` prices; a whole-model `getEntities()` would hand
                # curves and surfaces to a call that only reads point targets.
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
    # the generation policy beside its source: the element the mesh is FOR, the regions to name, and the density rule.
    # Geometric order and quad/hex recombination are absent by construction — the affine-geometry law fixes the first
    # and the element's own `CTOR` cell decides the second, so a plan cannot describe a mesh the assemble fold could
    # not then build a basis over.
    element: ElementKind
    groups: tuple[PhysicalGroup, ...] = ()
    size: SizeField = msgspec.field(default_factory=lambda: SizeField(uniform=0.1))
    optimize: str = ""  # `mesh.optimize` method name (`Laplace2D`, `Netgen`, `HighOrder`); empty leaves the kernel default


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


# stiffness typed object: the scipy.sparse (csr_array) container the solvers/linear#LINEAR
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
    generated: tuple[ContentKey, ElementKind, int, int, int, int, SolveStatus] = case()
    assembled: tuple[ContentKey, ElementKind, int, int, float, SolveStatus] = case()
    read: tuple[ContentKey, ElementKind, int, int, SolveStatus] = case()
    written: tuple[ContentKey, str, int, SolveStatus] = case()

    @classmethod
    def Generated(cls, key: ContentKey, element: ElementKind, dim: int, point_count: int, cell_count: int, group_count: int) -> Self:
        # floors on a non-empty node AND cell count: a kernel that synchronizes an unmeshable model writes a valid
        # `.msh` carrying points and zero cells of the requested type, which the read fold would otherwise admit as a
        # mesh. `group_count` is the REALIZED roster the read recovered, so a group the kernel dropped shows as a gap.
        extent = 0.0 if point_count and cell_count else float("inf")
        return cls(generated=(key, element, dim, point_count, cell_count, group_count, status_of(None, extent, _TOL["generated"])))

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
                MeshReceipt(tag="generated", generated=payload)
                | MeshReceipt(tag="assembled", assembled=payload)
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
                MeshReceipt(tag="generated", generated=(*_, SolveStatus() as status))
                | MeshReceipt(tag="assembled", assembled=(*_, SolveStatus() as status))
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
        # ONE settled-receipt spine: the mesh key, the provenance pair, the warning band, and the stamp are the
        # runtime owner's columns, so `key` leaves the payload rather than publishing the coordinate twice. The band
        # IS the well-formedness roster — an empty node or cell count, a non-finite load, a zero-byte write each name
        # their own termination class — and provenance names the produced key alone, an operation minting one field.
        subject = self.element.value if self.element is not None else self.tag
        facts: dict[str, object] = {
            "operation": self.tag, "converged": self.converged, **{name: value for name, value in self.facts.items() if name != "key"}
        }
        return (
            Receipt.of(
                EvidenceScope.MESH.value,
                ("emitted", subject, facts),
                key=Some(self.content_key),
                provenance=Some(Provenance(consumed=Block.empty(), produced=self.content_key)),
                band=Block.empty() if self.converged else Block.singleton(self.status.value),
            ),
        )


@tagged_union(frozen=True)
class MeshExchange:
    tag: Literal["generate", "assemble", "read", "write"] = tag()
    generate: tuple[GmshSource, MeshPlan, Path] = case()
    assemble: tuple[MeshField, FemForm] = case()
    read: tuple[Path, ElementKind, str | None] = case()
    write: tuple[MeshField, Path, str | None] = case()

    @classmethod
    def Generate(cls, source: GmshSource, plan: MeshPlan, path: Path, /) -> Self:
        # `path` is where the kernel writes its `.msh` — the generated mesh is a durable artifact a resume reads back
        # through the plain `Read` arm, so generation never mints a temp file the caller cannot name or re-key.
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

    async def run(self, lane: LanePolicy, *, composition: ScopeKey = DEFAULT_SCOPE) -> RuntimeRail[MeshReceipt]:
        # _TRAIT routes each arm — assemble to the HOSTILE process band (Python form callbacks run GIL-held), read/write
        # to the RELEASING thread band; the weave owns span, fence, and harvest, the Kernel crossing isolation and retry.
        # The caller's composition key threads onto the weave so an embedded composition's lifecycle facts reach the
        # points IT registered; the default keeps the root call shape scope-free.
        # the mark is built PARENT-side off the lane's own conduit and crosses as an ordinary kernel argument, exactly
        # as the geometry peer's tap does: a worker reaches the queue proxy and never `Hooks`, a span, or the registry.
        # `total` stays absent — the roster length is a constant every subscriber already reads off `MeshStage`, and
        # the extent a subscriber actually wants is the element count, which exists only once `MESHED` has fired.
        mark: Option[StageTap] = Some(StageTap.of(EvidenceScope.MESH, lane.pulses.tap)) if self.tag in _STAGED else Nothing

        async def dispatch() -> RuntimeRail[MeshReceipt]:
            return (await lane.offload(Kernel.of(_dispatch, _TRAIT[self.tag]), self, mark)).bind(lambda rail: rail)

        return await evidence_run(
            EvidenceScope.MESH, f"mesh.{self.tag}", dispatch, facts={"op": self.tag}, composition=composition, stage=mark
        )


# --- [OPERATIONS] --------------------------------------------------------------------------


# `railed` `effect.result` chain: the generate and read arms `yield from`-bind a fallible rail (the kernel build's
# arity gate, `_read`'s meshio parse / canonical-encode); assemble and write hold no fallible derive and lift straight.
@railed
def _dispatch(exchange: MeshExchange, mark: Option[StageTap]) -> MeshReceipt:
    match exchange:
        case MeshExchange(tag="generate", generate=(source, plan, path)):
            meshed: MeshField = yield from _generate(source, plan, path, mark)
            row = CTOR[plan.element]
            realized = len(meshed.cell_sets) + len(meshed.node_sets)
            return MeshReceipt.Generated(
                meshed.content_key, plan.element, _DIM[row.cell], int(meshed.points.shape[0]), int(meshed.cells.shape[0]), realized
            )
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


# `_field` mints the content key from EVERY stored array as a LABELED cell — element tag, then per block slot, map
# key, dtype, shape, and C-contiguous bytes — handed to the identity owner as ONE `IdentitySource(parts=...)` so the
# count-and-length frame runs at its `docs/laws/patterns.md` `[PREIMAGE_FRAMING]` owner. The deleted form chose a
# `len(part).to_bytes(8, "big")` width and byte order INSIDE this page and pre-joined each array's five cells into
# one opaque chunk: a width picked at a call site forks the key namespace with no surface able to report it, and the
# inner join left the five cells sharing one frame where the owner frames each. Dict sections stay sorted so
# insertion order never leaks, and a renamed group, reshaped array, or re-homed value re-keys where raw value bytes
# alone would collide. `ContentIdentity.of` returns `RuntimeRail[ContentKey]`, so the key threads by `yield from`.
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


# `.T` matches the (dim, n)/(verts, n_elem) layout skfem stores mesh.p/mesh.t in. `get_dofs(facets=...)`
# is the one DOF selector, `.flatten()` reducing to the global index array condense accepts as `D=`. The
# row resolves both spellings off the module-scope `lazy` skfem bind — module-load lightness, not cycle evasion —
# and `row.built` constructs a scalar, vector, or composite element identically, so a mixed weak form
# assembles through this one body with no per-family arm.
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


# The generation arm's whole gmsh reach, and the ONE place the kernel opens. The meshed dimension and the cell shape
# are read off the `CTOR` row the caller's `ElementKind` already selects, so the recombination follows the element
# rather than a plan knob; the mesh leaves through `gmsh.write` and re-enters through the SAME `_read` fold every other
# inbound mesh crosses, so a generated mesh's groups, cell block, and content key derive identically to a read one and
# no gmsh element-type integer table forms beside `CTOR`.
# The geometric order stays AFFINE and `mesh.setOrder` is the deleted call, MEASURED: promoting to order 2 writes
# `triangle6`/`tetra10` blocks, which `cells_dict[CTOR[element].cell]` cannot key and the `Mesh*1` constructors the
# same row names cannot take — a P2 element in this vocabulary is a higher-order BASIS over affine geometry, so a
# promoted mesh is not a finer input to the assemble fold but an unreadable one.
def _generate(source: GmshSource, plan: MeshPlan, path: Path, mark: Option[StageTap]) -> RuntimeRail[MeshField]:
    # every position beats through `_beat`, so a lane with no conduit folds to the no-op and the fold reads as one
    # sequence of named milestones rather than six optional branches.
    row = CTOR[plan.element]
    gmsh.initialize()
    try:  # Exemption: the `initialize`/`finalize` bracket is the platform-forced statement kernel — the kernel is
        # process-global and no expression form releases it on the raising arm, which would strand the session for
        # every later dispatch on this worker.
        gmsh.model.add(path.stem)
        match source.build(gmsh):
            case Result(tag="error", error=fault):
                return Error(fault)
            case Result(tag="ok"):
                _beat(mark, MeshStage.BUILT, 1)
        # groups name regions only AFTER the kernel commits its entities — `addPhysicalGroup` on an unsynchronized
        # model tags entity ids the mesher never sees, and the `.msh` then carries a group over nothing.
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
    # ONE optional-beat spelling for the whole fold: an arm outside `_STAGED` carries `Nothing` and every position
    # folds to the no-op, so the kernel body states its milestones once instead of guarding each of them.
    mark.map(lambda held: held.beat(stage, done))


# A column the writing format namespaced under its own `<format>:` prefix is format bookkeeping, never a user region,
# so the promoter refuses it. MEASURED on a gmsh `.msh`: the ungated integer sweep re-promotes `gmsh:physical` as
# `set-gmsh:physical-<tag>` beside the `inlet`/`domain` groups meshio ALREADY recovered as named `cell_sets`, and
# promotes `gmsh:geometrical` — an entity-id column naming no region at all — so a two-group mesh reads back carrying
# five sets and a content key that no longer matches a round trip of the same regions.
def _region(name: str) -> bool:
    return ":" not in name


def _promotable(column: str, values: np.ndarray) -> bool:
    return _region(column) and bool(np.issubdtype(np.asarray(values).dtype, np.integer))


# file_format disambiguates the .msh/.dat extensions, None triggering detection. The per-cell field/set
# arrays read the cell_type-keyed `cell_data_dict[k][cell_type]`/`cell_sets_dict[k][cell_type]` merged
# views — never `cell_data[k][0]`, the block-0 array that misaligns with `cells_dict[cell_type]` when the
# FEM element is not block 0; the `if cell_type in by_type` guard drops a name off the element's block.
# `cell_data_to_sets`/`point_data_to_sets` invert only the `_promotable` columns, so a float field stays in
# `cell_fields`, a format's own namespaced bookkeeping stays out of the set roster, and an integer region tag
# recovers into named sets.
def _read(path: Path, element: ElementKind, fmt: str | None) -> RuntimeRail[MeshField]:
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
        # the recovered set rosters take the SAME region discriminant the promoter takes: meshio surfaces its own
        # `gmsh:bounding_entities` bookkeeping as a named set it never promoted, so a gate on the promoter alone still
        # admits a non-region into `MeshField` and into the content key the `_field` fold takes over every set name.
        {k: np.asarray(v) for k, v in mesh.point_sets.items() if _region(k)},
        {k: np.asarray(by_type[cell_type]) for k, by_type in cell_sets.items() if _region(k) and cell_type in by_type},
        {k: np.asarray(v) for k, v in mesh.field_data.items()},
    )


# cell_data/cell_sets arrays wrap in the single-element per-block list meshio mandates; `cell_sets_to_data`/
# `point_sets_to_data` promote the named groups into integer label fields so region tags round-trip through
# integer-only formats — the write half of the read-recover/write-promote round-trip.
def _write(field: MeshField, path: Path, fmt: str | None) -> str:
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
    return fmt or path.suffix.lstrip(".")
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
