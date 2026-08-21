# [PY_GEOMETRY_MESH_CAD]

One ISO 10303 STEP and IGES tessellation hop — the CAD source formats the `mesh/daemon#DAEMON` `TessellationDaemon` serves through its `cad` arm. `StepBridge` reads B-rep bytes through the OCCT XCAF readers into a `TDocStd_Document`, meshes the transferred shape in place under the `TessellationPolicy` band, and writes GLB through the native `RWGltf_CafWriter`; one `READERS` row per format makes a new CAD source one row, never a parallel reader method.

`TessellationPolicy` and `GlbArtifact` are minted here beside `BridgeFormat` — the mesher knobs and the folder's ONE outbound GLB carrier are geometry-owned, never runtime `IdentityPolicy` fields, and the `mesh/daemon`/`mesh/serve`/`mesh/brep`/`scan` consumers import them downward. The two-key discipline holds and sharpens: the daemon keys the SOURCE bytes plus the policy spec before the offload hop, so an output-GLB key never serves a cache hit, while the seed-zero (`Some(0)`) `XxHash128` GLB WIRE key equal to the C# `RepresentationContentHash` mints exactly ONCE — on `GlbArtifact.of`, at the site that produced the bytes — so no downstream servicer re-hashes a payload it did not encode. This hop rides `cadquery-ocp`, the sole PyPI OCCT path; the wire aligns to the C# `StepIso10303` codec, which requests CAD tessellation from this companion rather than re-implementing a managed reader.

## [01]-[INDEX]

- [02]-[BRIDGE]: STEP/IGES reader-to-GLB hop over the `READERS` table and the roster-derived metadata cascade, fenced on one `FaultRow` over the typed `BridgeFault`, output-parameterized over `BridgeView`, egressing the wire-keyed `GlbArtifact`.

## [02]-[BRIDGE]

- Owner: `StepBridge` — the static surface over the XCAF reader chain; `READERS` carries one behavior row per format so the kernel never re-discriminates the reader past the table; `BridgeFormat.subject` owns the one `step-bridge.<fmt>` receipt tag and the coordinate every `BridgeFault` case renders, the fence subject deriving from this page's own `FaultRow` row instead; `GlbArtifact` is the folder's ONE outbound GLB carrier, pairing the octets with the wire key that addresses them and the producer that encoded them, so a servicer frames bytes it never re-hashes and a consumer never carries a loose `(bytes, key)` pair; `BridgeView` parameterizes the output so the daemon's lane hop matches the bare artifact while an in-process caller drains the receipt-carrying `CadTessellation`.
- Cases: `STEP` binds the full `COLOR`/`NAME`/`LAYER`/`GDT`/`MAT` channel set, `IGES` the `COLOR`/`NAME`/`LAYER` subset its reader admits; the daemon never re-discriminates format past this owner. `GlbArtifact.producer` closes over the three sites that encode GLB anywhere in the folder — the `ifc` iterator serializer, this `cad` writer, and the `reconstruction` export — so a downstream frame names which kernel produced the payload it streams.
- Auto: the `"glb"` view drops the receipt because a live contributor cannot cross the no-pickle lane hop — the daemon's `@receipted` aspect owns the daemon-level fold; the `"full"` view carries the `CadReceipt` for the in-process harvest. `GlbArtifact.of` is the one wire-key mint: `ContentIdentity.key("glb", octets, seed=Some(0))` is total, so the carrier never rails and a producer never branches on a key it always has.
- Packages: `cadquery-ocp` (the `OCP.*` XCAF reader/writer band, module-scope `lazy from` so the loop-floor consumers of `TessellationPolicy`/`BridgeFormat` never load OCCT — `TCollection_ExtendedString` is the REQUIRED `TDocStd_Document` storage string, an `AsciiString` or bare `str` raises), `expression`, `msgspec`, and the runtime rails; a malformed STEP stream is a deterministic `BridgeFault`, never a transient the resilience owner retries, so this owner stacks no second retry rail; the fence names that token beside `OSError` — the path-based reader and CAF writer both cross the filesystem — and never widens to a bare `Exception`.
- Growth: a new CAD source is one `BridgeFormat` row and one `ReaderRow` and one alias on the daemon `cad` case; a new metadata channel is one `MetadataMode` member whose VALUE is the provider's own selector infix, reaching every reader that admits it with no table edit; a new read-leg failure mode is one `BridgeFault` case and one `_coordinate` arm; a new output projection is one `BridgeView` member and one view arm; a new GLB producer is one `GlbArtifact.producer` literal at the site that encodes it, never a second carrier; `RWGltf_CafWriter.Perform(doc, fileInfo, progress)` is the minimal write arity — there is no 2-arg `Perform(doc, progress)` — so glTF asset metadata populates the already-present `fileInfo` map in place, and the 5-arg selective-root overload threads a partial-assembly export.
- Boundary: the bridge mints no transport or channel, and the one key it mints is the GLB WIRE key on `GlbArtifact.of` — never a cache key, which stays the daemon's SOURCE-plus-policy fold. `GlbArtifact` crosses OUTWARD only: the `artifacts` `SceneGrid.of_glb` chunk-table admission is the single outward seam every geometry-encoded GLB enters the scene plane through, and nothing returns — geometry imports no `artifacts` symbol, so the drawn `[BOUNDARY]: SceneGrid` edge is one-way data with no reverse leg. Evaluating an already-in-memory `TopoDS_Shape` is `mesh/brep#BREP`'s (which reuses neither this reader nor this writer), mesh conditioning is `mesh/repair#MESH`'s, and mesh-file codec is the data `MeshPayload` owner's; the shape-only `STEPControl_Reader` (it drops the assembly/color/name metadata the XCAF reader preserves) and the conda-only `pythonocc-core` `OCC.Core.*` path never enter.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, assert_never, overload

from expression import Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct

from rasm.geometry.graduation import GeometryLeg
from rasm.runtime.faults import TERMINAL, Catch, FaultRow, RuntimeRail, boundary, rostered
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.receipts import Receipt

# loop floor imports this module for the TessellationPolicy/BridgeFormat vocabulary the daemon and serve consume and
# must never load OCCT, so the whole OCP band defers to first worker-side use; the read-leg carriers below stay type
# aliases and call-time thunks because a msgspec field annotation or a module-scope table cell dereferencing a lazy
# name reifies it at import (the language LAZY_IMPORT_SITE law).
lazy from OCP.BRepGProp import BRepGProp
lazy from OCP.BRepMesh import BRepMesh_IncrementalMesh
lazy from OCP.GProp import GProp_GProps
lazy from OCP.IFSelect import IFSelect_ReturnStatus
lazy from OCP.IGESCAFControl import IGESCAFControl_Reader
lazy from OCP.Message import Message_ProgressRange
lazy from OCP.RWGltf import RWGltf_CafWriter
lazy from OCP.STEPCAFControl import STEPCAFControl_Reader
lazy from OCP.TColStd import TColStd_IndexedDataMapOfStringString
lazy from OCP.TCollection import TCollection_AsciiString, TCollection_ExtendedString
lazy from OCP.TDF import TDF_LabelSequence
lazy from OCP.TDocStd import TDocStd_Document
lazy from OCP.TopoDS import TopoDS_Builder, TopoDS_Compound, TopoDS_Shape
lazy from OCP.XCAFApp import XCAFApp_Application
lazy from OCP.XCAFDoc import XCAFDoc_DocumentTool, XCAFDoc_ShapeTool

# --- [TYPES] ----------------------------------------------------------------------------


class BridgeFormat(StrEnum):
    STEP = "step"
    IGES = "iges"

    # one `step-bridge.<fmt>` tag, spelled once on the enum rather than re-interpolated per site.
    @property
    def subject(self) -> str:
        return f"step-bridge.{self}"


# member VALUE is the provider's own `Set<X>Mode` infix, so the cascade is `getattr(reader, f"Set{mode}Mode")(True)` off
# the roster itself and the retired `_APPLY` mirror — five rows re-spelling the five member names — deletes. Each
# `ReaderRow` carries the channel subset its reader admits, so the cascade walks that subset and never the whole roster.
class MetadataMode(StrEnum):
    COLOR = "Color"
    NAME = "Name"
    LAYER = "Layer"
    GDT = "GDT"
    MAT = "Mat"


type BridgeView = Literal["glb", "full"]
type CafReader = STEPCAFControl_Reader | IGESCAFControl_Reader

# read-leg carrier as (document, welded root, free-shape count) — a Struct field annotation resolves at class creation
# and would reify the lazy OCP names, so the carrier stays an annotation-only alias.
type XcafSession = tuple[TDocStd_Document, TopoDS_Shape, int]

# per-format dispatch row as (reader thunk, admitted metadata channels); the thunk defers the class dereference to call
# time, and the fault/span subject is `BridgeFormat.subject`, never a row field.
type ReaderRow = tuple[Callable[[], CafReader], Block[MetadataMode]]

# --- [CONSTANTS] ------------------------------------------------------------------------

# XCAF document storage format the OCAF application initializes the assembly tree under.
_XCAF_STORAGE: Final[str] = "MDTV-XCAF"

# --- [MODELS] ---------------------------------------------------------------------------


# minted here so StepBridge consumes it in-signature — a daemon-sited mint would force a cad -> daemon back-edge;
# `spec` is the canonical seed-byte projection the daemon folds into its policy-keyed cache seed.
class TessellationPolicy(Struct, frozen=True, gc=False):
    deflection: float = 0.01
    angle_tolerance: float = 1e-4

    @property
    def spec(self) -> bytes:
        return f"{self.deflection:.17g}|{self.angle_tolerance:.17g}".encode()


CANONICAL_TESSELLATION: Final[TessellationPolicy] = TessellationPolicy()


class GlbArtifact(Struct, frozen=True, gc=False):
    # the folder's ONE outbound GLB carrier: the octets, the wire key that addresses them, and the kernel that
    # encoded them travel together, so `mesh/serve` frames bytes it never re-hashes, `scan/deviation` reads a
    # reference by the key its producer minted, and no consumer carries a loose `(bytes, key)` pair whose halves
    # can disagree. Picklable whole — bytes, a `ContentKey` Struct, and a literal cross every worker seam.
    bytes: bytes
    wire_key: ContentKey
    producer: Literal["ifc", "cad", "reconstruction"]

    @classmethod
    def of(cls, octets: bytes, producer: Literal["ifc", "cad", "reconstruction"]) -> "GlbArtifact":
        # the ONE seed-zero mint: `ContentIdentity.key` excludes the fallible `Struct` source, so the fold runs no
        # encode and the carrier is total — a producer never rails on a key it always holds. Seed `Some(0)` is the
        # bare `XxHash128.HashToUInt128(span)` parity path the C# `RepresentationContentHash` reads byte-for-byte;
        # a policy-folded seed here is the named drift defect, that seed belonging to the daemon's cache key alone.
        return cls(bytes=octets, wire_key=ContentIdentity.key("glb", octets, seed=Some(0)), producer=producer)


class CadReceipt(Struct, frozen=True, gc=False):
    fmt: BridgeFormat
    shape_count: int
    mass: float

    # native-scalar facts; the one-row tuple return satisfies the contributor port's iterable.
    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("rasm.geometry.mesh.cad", ("emitted", self.fmt.subject, {"format": self.fmt, "shapes": self.shape_count, "mass": self.mass})),)


class CadTessellation(Struct, frozen=True, gc=False):
    glb: GlbArtifact
    receipt: CadReceipt


# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the `boundary` fence as a TYPED token, never a rendered message: the failure STAGE is the tag, so a
# consumer matches the case rather than splitting a string, and only `read_failed` carries the provider's own return
# status — the retired `status: X | None` slot spelled a code on four stages that never issue one. The status
# annotation stays a STRING, since a resolved one would dereference the deferred `IFSelect` name at class creation.
# `BoundaryFault.of` admits a `Tagged()` token AHEAD of every `CLASSIFY` row, so this family crosses the conversion
# door WHOLE on the `domain` case and the catch-all's `str(cause)` half never renders it. A worker seam carries it
# whole too: `execution/workers#CROSSING` lowers the token onto `CrossedFault` DATA at `shipped` and re-mints this
# family's own case parent-side, which is why `mesh/daemon#DAEMON` now re-raises the typed case and its hand-rolled
# `RuntimeError(str(token))` is gone. `__str__` serves the LOG and HOST edge alone — a token surfacing in a worker
# traceback or a log line before the seam lowers it — where `Exception.__str__` answers the EMPTY string here.
@tagged_union(frozen=True)
class BridgeFault(Exception):
    tag: Literal["unknown_format", "read_failed", "transfer_failed", "no_root", "write_failed"] = tag()
    unknown_format: BridgeFormat = case()
    read_failed: "tuple[BridgeFormat, IFSelect_ReturnStatus]" = case()
    transfer_failed: BridgeFormat = case()
    no_root: BridgeFormat = case()
    write_failed: BridgeFormat = case()

    def __str__(self) -> str:
        # the law half IS the tag, so no arm re-spells its own case name and a renamed case cannot drift from its render.
        return f"{self.tag}:{self._coordinate()}"

    def _coordinate(self) -> str:
        match self:
            case BridgeFault(tag="read_failed", read_failed=(fmt, status)):
                return f"{fmt.subject}({status})"
            case (
                BridgeFault(unknown_format=fmt)
                | BridgeFault(transfer_failed=fmt)
                | BridgeFault(no_root=fmt)
                | BridgeFault(write_failed=fmt)
            ):
                return fmt.subject
            case _ as unreachable:
                assert_never(unreachable)


# --- [TABLES] ---------------------------------------------------------------------------

# one behavior row per format — the reader thunk resolves its lazy class at call time, so no row cell dereferences
# an OCP name at module scope and no leg constructs a reader inline.
READERS: Final[Map[BridgeFormat, ReaderRow]] = Map.of_seq([
    (
        BridgeFormat.STEP,
        (
            lambda: STEPCAFControl_Reader(),
            Block.of_seq([MetadataMode.COLOR, MetadataMode.NAME, MetadataMode.LAYER, MetadataMode.GDT, MetadataMode.MAT]),
        ),
    ),
    (BridgeFormat.IGES, (lambda: IGESCAFControl_Reader(), Block.of_seq([MetadataMode.COLOR, MetadataMode.NAME, MetadataMode.LAYER]))),
])

# this module's whole raise roster: the ONE fenced leg anchors one row, so the tessellate call spells no subject and
# the `rostered` door seats every row on the branch census, proving `geometry.mesh.cad` against a real module at import. TERMINAL, because a malformed
# STEP
# stream and an unadmitted format both refuse identically on every re-issue — which is also why this owner stacks no
# retry rail. The FORMAT coordinate the retired per-format subject carried now rides the `BridgeFault` token itself,
# which `BoundaryFault.domain` carries WHOLE, so the collapse of five subjects into one loses nothing.
CAD_TESSELLATE: Final[FaultRow[GeometryLeg]] = FaultRow(
    leg=GeometryLeg.CAD, point="tessellate", arm="boundary", defect="bridge-refused", retriability=TERMINAL
)
RAISES: Final[Block[FaultRow[GeometryLeg]]] = rostered(Block.of_seq([CAD_TESSELLATE]))

# the read and write legs reach TWO raise surfaces and no more: this owner's own typed refusal, and the filesystem the
# path-based OCCT reader and CAF writer both cross. Naming them is what keeps an OCCT segfault-adjacent provider defect
# out of the rail it would otherwise cross as an anonymous `boundary` string.
_BRIDGE_RAISES: Final[Catch] = (BridgeFault, OSError)

# --- [OPERATIONS] -----------------------------------------------------------------------


# every failure raises a `BridgeFault` the enclosing `boundary` converts, never a status the caller checks; the
# metadata cascade reads the roster member's own VALUE, so a channel a reader admits can never miss a selector row.
def _read(src_path: str, fmt: BridgeFormat) -> XcafSession:
    match READERS.try_find(fmt):
        case Option(tag="some", some=(factory, modes)):
            reader = factory()
        case _:
            raise BridgeFault(unknown_format=fmt)
    document = TDocStd_Document(TCollection_ExtendedString(_XCAF_STORAGE))
    XCAFApp_Application.GetApplication_s().InitDocument(document)
    for mode in modes:
        getattr(reader, f"Set{mode}Mode")(True)  # the member VALUE is the provider's own infix, so no mirror table stands between
    if (status := reader.ReadFile(src_path)) != IFSelect_ReturnStatus.IFSelect_RetDone:
        raise BridgeFault(read_failed=(fmt, status))
    if not reader.Transfer(document):
        raise BridgeFault(transfer_failed=fmt)
    tool = XCAFDoc_DocumentTool.ShapeTool_s(document.Main())
    tool.GetFreeShapes(labels := TDF_LabelSequence())
    if labels.Length() < 1:
        raise BridgeFault(no_root=fmt)
    return document, _root(tool, labels), labels.Length()


# every free shape welds into one compound so the mesher and `GProp` span the whole assembly: `Perform` serializes EVERY
# free shape, so meshing only `Value(1)` emits shapes 2..N with an empty triangulation. `Value(i)`/`GetShape_s` are one-based.
def _root(tool: XCAFDoc_ShapeTool, labels: TDF_LabelSequence) -> TopoDS_Shape:
    builder, compound = TopoDS_Builder(), TopoDS_Compound()
    builder.MakeCompound(compound)
    for i in range(1, labels.Length() + 1):
        builder.Add(compound, tool.GetShape_s(labels.Value(i)))
    return compound


def _emit(session: XcafSession, glb_path: str, fmt: BridgeFormat, policy: TessellationPolicy) -> CadTessellation:
    document, root, shape_count = session
    BRepMesh_IncrementalMesh(root, policy.deflection, False, policy.angle_tolerance, True)
    props = GProp_GProps()
    BRepGProp.VolumeProperties_s(root, props)
    written = RWGltf_CafWriter(TCollection_AsciiString(glb_path), True).Perform(document, TColStd_IndexedDataMapOfStringString(), Message_ProgressRange())
    sink = Path(glb_path)
    # `Perform` reports failure as False and can also leave no or empty output; both proofs gate before any bytes read,
    # so a failed export never masquerades as a zero-length tessellation.
    if not written or not sink.is_file() or sink.stat().st_size == 0:
        raise BridgeFault(write_failed=fmt)
    return CadTessellation(GlbArtifact.of(sink.read_bytes(), "cad"), CadReceipt(fmt, shape_count, props.Mass()))


# one `TemporaryDirectory` scopes both round-trip paths under one cleanup — the OCCT reader and CAF writer are path-based.
def _run(source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy) -> CadTessellation:
    with TemporaryDirectory(prefix="step-bridge-") as work:
        src_path = Path(work, f"src.{fmt}")
        src_path.write_bytes(source_bytes)
        return _emit(_read(str(src_path), fmt), str(Path(work, "out.glb")), fmt, policy)


# --- [SERVICES] -------------------------------------------------------------------------


class StepBridge:
    @overload
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = ..., *, view: Literal["glb"] = ...
    ) -> "RuntimeRail[GlbArtifact]": ...
    @overload
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = ..., *, view: Literal["full"]
    ) -> "RuntimeRail[CadTessellation]": ...
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = CANONICAL_TESSELLATION, *, view: BridgeView = "glb"
    ) -> "RuntimeRail[GlbArtifact] | RuntimeRail[CadTessellation]":
        # one rostered fence over both legs; the format the retired per-format subject carried rides the typed
        # `BridgeFault` the door carries whole, so the caller reads WHICH format and WHICH stage off the token.
        railed = boundary(CAD_TESSELLATE, lambda: _run(source_bytes, fmt, policy), catch=_BRIDGE_RAISES)
        return railed if view == "full" else railed.map(lambda t: t.glb)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
