# [PY_GEOMETRY_MESH_CAD]

The ISO 10303 AP242/AP203/AP214 and IGES CAD-STEP tessellation hop is the second source format the one `mesh/daemon.md#DAEMON` `TessellationDaemon` serves through its `cad` arm. `StepBridge` reads STEP/IGES B-rep bytes through the OCCT `STEPCAFControl_Reader`/`IGESCAFControl_Reader` into an XCAF `TDocStd_Document`, meshes the transferred `TopoDS_Shape` in place with `BRepMesh_IncrementalMesh` under the `IdentityPolicy` deflection/tolerance, and writes the XCAF document to GLB through the native `RWGltf_CafWriter`, returning the content-keyless GLB the IFC arm also produces.

Three owners carry the variation. One `READERS` data table folds each format's reader and `Set*Mode` metadata channels into one row, so a new CAD source is one row rather than a parallel reader method, and `BridgeFormat.subject` owns the one `step-bridge.<fmt>` fault/span/receipt tag. One `boundary`-fenced kernel runs the read/mesh/write under the runtime `reliability/faults#FAULT` rail, so a failed `ReadFile` or `Transfer` converts through the one `BoundaryFault` taxonomy rather than a domain `raise`. One polymorphic `tessellate` parameterizes its output over `BridgeView`, so the daemon's lane-subinterpreter call matches raw `bytes` while an in-process caller drains the full `CadTessellation` carrying the typed `CadReceipt`.

The hop rides the `cadquery-ocp` `OCP` binding, the sole PyPI OCCT path. The daemon routes its `cad(source_bytes, fmt)` case here; the bridge mints no `ContentKey`, since the daemon keys the SOURCE bytes through the shared `evidence/identity#IDENTITY` `ContentIdentity.of("cad", source_bytes, policy)` before the offload hop so a cache hit skips the OCCT pass entirely. One daemon content-addresses AEC (IFC) and mechanical (CAD-STEP) geometry through one keyed admission. The wire aligns to the C# `StepIso10303` codec, which requests CAD tessellation from this companion rather than re-implementing a managed reader.

## [01]-[INDEX]

- [01]-[BRIDGE]: the STEP/IGES reader-to-GLB hop over the `READERS`-folded OCCT XCAF reader table and the `_APPLY` metadata-channel cascade, the `boundary`-fenced in-place mesher minting the closed-`BridgeStage` `BridgeFault` on a failed read, the native glTF CAF writer, the typed `CadReceipt` `ReceiptContributor`, and the `BridgeView`-overloaded single entry parameterizing the daemon's raw-GLB drain against the in-process receipt-carrying drain.

## [02]-[BRIDGE]

- Owner: `StepBridge` — the static surface driving the OCCT XCAF reader chain over `cadquery-ocp`; `BridgeFormat` the closed `StrEnum` aliasing the daemon `cad` case rows to the reader that owns each; `MetadataMode` the closed `StrEnum` naming the XCAF transfer channels (`COLOR`/`NAME`/`LAYER`/`GDT`/`MAT`) the `_APPLY` table binds to its explicit `Set*Mode` selector per channel — a closed dispatch row, never a stringly `getattr(reader, f"Set{mode}Mode")`; `ReaderRow` the per-format dispatch row binding the CAF reader factory and the `Block[MetadataMode]` channels its `Set*Mode` cascade walks, so the command pattern is spelled once; `BridgeFormat.subject` the `step-bridge.<fmt>` fault/span/receipt tag owned on the enum so it is total for every format (the table miss included) rather than a per-row field; `READERS` the `Map[BridgeFormat, ReaderRow]` of one behavior row per format so the kernel never re-discriminates the reader past this table; `BridgeStage` the closed `StrEnum` of read-leg failure stages the one `BridgeFault.of` message factory discriminates so the failure mode is one row not a sibling exception class; `BridgeFault` the single raised `Exception` whose `of` factory mints the distinct stage messages the `boundary` fence converts; `XcafSession` the frozen value object carrying the populated `TDocStd_Document`, the assembly-root `TopoDS_Shape`, and the transferred free-shape count so the read leg hands one typed carrier to the mesh/write/evidence legs; `CadTessellation` the result pairing the raw GLB with the typed `CadReceipt`; `CadReceipt` the typed receipt — itself a `ReceiptContributor` whose `contribute` folds its fields into one emitted-phase `Receipt.of` row — carrying the format, the transferred shape count, and the solid mass read off the `GProp_GProps` accumulator; `BridgeView` the output-projection axis selecting the raw GLB the daemon drains or the full `CadTessellation` the in-process caller drains.
- Cases: `BridgeFormat` rows `STEP` (the `STEPCAFControl_Reader` assembly/color/name/layer transfer) and `IGES` (the `IGESCAFControl_Reader` color/name/layer transfer) — each a `READERS` row binding the CAF reader factory and the `MetadataMode` channels that reader admits, sharing the one XCAF document, the in-place `BRepMesh_IncrementalMesh`, and the `RWGltf_CafWriter` output leg. The reader resolves once through `READERS.try_find`, never re-matched per leg, and a `BridgeFormat` absent from the table is the `Map.try_find` `Nothing` whose none arm raises `UNKNOWN_FORMAT` into the `boundary` fence rather than a `KeyError` escaping the hop; the daemon never re-discriminates format past this owner.
- Entry: `StepBridge.tessellate` admits source bytes, a `BridgeFormat`, an `IdentityPolicy`, and a `BridgeView`, returning `RuntimeRail[bytes]` at the default `"glb"` view the daemon matches `Ok(glb)`/`Error(fault)` or `RuntimeRail[CadTessellation]` at the `"full"` view carrying the receipt. The `@overload` arms keyed on the `BridgeView` `Literal` carry the output shape statically, so a caller narrows on the view it passes rather than re-matching the return. The body is the one `boundary(fmt.subject, ...)` fence over `_run`: it scopes one `TemporaryDirectory`, resolves the row through the total `READERS.try_find` `Option` fold, writes the bytes to the temp path the OCCT path-based reader consumes, runs `ReadFile`/`Transfer` to populate the XCAF document into an `XcafSession`, meshes the root shape with `BRepMesh_IncrementalMesh(shape, deflection, False, angle, True)`, reads the `GProp_GProps` mass evidence, writes a binary GLB through `RWGltf_CafWriter(path, True).Perform(doc, fileInfo, progress)` — `fileInfo` an empty `TColStd_IndexedDataMapOfStringString` (the minimal 3-arg overload; there is no 2-arg `Perform(doc, progress)`) — reads the GLB bytes back, and folds the `CadTessellation`. The bridge mints no `ContentKey`: the daemon admits each source under the SOURCE-keyed `ContentIdentity.of("cad", source_bytes, policy)` BEFORE the offload hop, so a re-tessellation at identical bytes and settings is a lane cache hit that never runs this kernel. An output-GLB key would only exist after the kernel runs and so could never serve the hit, the deleted form the daemon's `[KEYED_ADMISSION]` seam rejects.
- Auto: cross-cutting concerns ride the data table and the rail, not inline call sites. The `MetadataMode` channels select the assembly metadata transferred into the XCAF label tree — `STEP` binds the full `COLOR`/`NAME`/`LAYER`/`GDT`/`MAT` set, `IGES` the `COLOR`/`NAME`/`LAYER` subset its reader admits — through one `for mode in row.modes: _APPLY[mode](reader)` boundary-kernel cascade over the `_APPLY` selector table, total by construction since every `MetadataMode` member carries an `_APPLY` row, rather than a per-reader `Set*Mode` re-spell. The `ReadFile` `IFSelect_ReturnStatus` checked against `IFSelect_RetDone` and the `Transfer(doc)` verdict each raise a `BridgeFault` — minted off the closed `BridgeStage` vocabulary through the one `BridgeFault.of` message factory — inside the `boundary` fence, so a failed read converts through the runtime `_convert`/`CLASSIFY` fold into the `boundary` `BoundaryFault` case and never emits an empty GLB. The failure mode rides the message, never a parallel tagged error rail; the contributor-free contract holds because the bridge raises into its own `boundary` rather than a domain `raise ValueError` the lane re-wraps. `BRepMesh_IncrementalMesh` mutates the shape's stored triangulation in place from `IdentityPolicy.deflection`/`angle_tolerance` and `RWGltf_CafWriter` consumes exactly that triangulation, so meshing precedes the write. `BRepGProp.VolumeProperties_s` populates a `GProp_GProps` accumulator whose `Mass()` the bridge folds into the `CadReceipt`; the `CentreOfMass`/`MatrixOfInertia` the same accumulator exposes are the read-available growth knob a richer receipt arm folds.
- Receipt: `CadReceipt` conforms to `ReceiptContributor` — `contribute` folds the format, the `XcafSession` transferred shape count, and the `GProp_GProps.Mass` into one `Receipt.of("mesh.cad", ("emitted", fmt.subject, facts))` row through the runtime stream, the `facts` carrying the native `BridgeFormat`/`int`/`float` the receipts `Encoder(enc_hook=repr, order="deterministic")` serializes without a `str()`/`f"{...:.6g}"` coerce, the same `ReceiptContributor`-owns-its-projection shape the `mesh/brep.md#BREP` `BrepReceipt` holds. The `"glb"` view drops the receipt because the daemon's `@receipted` aspect owns the daemon-level fold across the no-pickle lane hop where a live contributor cannot cross; the `"full"` view carries it for the in-process caller that harvests it directly. The bridge keys nothing — the daemon keys the SOURCE bytes through the shared `ContentIdentity` before the hop, so the bridge mints no content key.
- Packages: `cadquery-ocp` (`OCP.STEPCAFControl.STEPCAFControl_Reader`/`OCP.IGESCAFControl.IGESCAFControl_Reader` the CAF readers, `OCP.TDocStd.TDocStd_Document`/`OCP.XCAFApp.XCAFApp_Application`/`OCP.XCAFDoc.XCAFDoc_DocumentTool` the XCAF document, the `XCAFApp_Application.GetApplication_s()` static plus `InitDocument(document)` instance pair that scaffolds the document before transfer, and `ShapeTool_s(label) -> XCAFDoc_ShapeTool` typed label-tree access — the `XcafSession`/`_root` carry the real `XCAFDoc_ShapeTool`, never an erased `object`, `OCP.TDF.TDF_LabelSequence` the `GetFreeShapes` out-parameter with `Value(i)`/`Length` one-based accessors, `OCP.TopoDS.TopoDS_Builder`/`TopoDS_Compound` the one `MakeCompound`/`Add` weld folding EVERY free shape into the single root the mesher and `GProp` span, `OCP.BRepMesh.BRepMesh_IncrementalMesh` the in-place mesher, `OCP.RWGltf.RWGltf_CafWriter` the glTF CAF writer, `OCP.IFSelect.IFSelect_ReturnStatus` the read-status enum, `OCP.Message.Message_ProgressRange` the writer progress, `OCP.TCollection.TCollection_ExtendedString` the REQUIRED `TDocStd_Document` storage-format string (an `AsciiString`/bare `str` raises) and `OCP.TCollection.TCollection_AsciiString` the `RWGltf_CafWriter` file-path string, `OCP.TColStd.TColStd_IndexedDataMapOfStringString` the empty writer `fileInfo` glTF-metadata map, `OCP.BRepGProp.BRepGProp`/`OCP.GProp.GProp_GProps` the volume-properties evidence), `expression` (`Map`/`Map.of_seq`/`Map.try_find` the `READERS`/`_APPLY` dispatch tables — `READERS.try_find` the row-resolution `Option`, `_APPLY` total by construction over the closed `MetadataMode`, `Block`/`Block.of_seq`/`Block[MetadataMode]` the per-row channel block the `Set*Mode` cascade walks, `Some` the total `_read` row-resolution `match` raising `UNKNOWN_FORMAT` in its none arm), `msgspec` (`Struct(frozen=True, gc=False)` the `XcafSession`/`CadTessellation`/`CadReceipt`/`ReaderRow` carriers), runtime `evidence/identity#IDENTITY` (`IdentityPolicy`/`CANONICAL_POLICY` the deflection/tolerance the daemon also folds into the cache key), `reliability/faults#FAULT` (`RuntimeRail`/`boundary` the one fence the lane's `traced_kernel`/`async_boundary` re-converts across the offload hop, so this owner stacks no second retry/span/log rail — a malformed STEP byte stream is a deterministic `BridgeFault`, not a transient the resilience owner retries), `observability/receipts#RECEIPT` (`Receipt`/`ReceiptContributor` the typed evidence port).
- Growth: a new CAD source is one `BridgeFormat` row plus one `READERS` `ReaderRow` binding its CAF reader factory and `MetadataMode` channels, plus one alias on the daemon `cad` case; a new transferred-metadata channel is one `MetadataMode` member plus one `_APPLY` selector row reaching every reader that admits it; a new output projection is one `BridgeView` member plus one `view` arm; the deflection knob is the shared `IdentityPolicy.deflection` already folded into the daemon cache key; the `RWGltf_CafWriter.Perform(doc, fileInfo, progress)` is the minimal write arity (there is no 2-arg `Perform(doc, progress)` base it grows over), so a future glTF asset-metadata arm populates the already-present empty `TColStd_IndexedDataMapOfStringString` `fileInfo` in place rather than widening the call, and the 5-arg `Perform(doc, rootLabels, labelFilter, fileInfo, progress)` selective-root overload is the further knob a partial-assembly export threads; zero new surface, no parallel per-format reader method.
- Boundary: the bridge mints no transport, channel, or content key — it returns raw GLB bytes, and the daemon keys the SOURCE bytes through the shared `evidence/identity#IDENTITY` `ContentIdentity` before the offload hop. It speaks the existing C# `ComputeService`/`ArtifactSync` contract through the daemon's `transport/serve#SERVE` `ServerHost`. The GLB it returns is the shape the C# `SharpGLTF` import reads; the IFC semantic graph the C# `IfcSemanticModel` projects in-process is never re-derived here. This owner reads source bytes into a GLB — it does not evaluate an already-in-memory `TopoDS_Shape` (that is the `mesh/brep.md#BREP` `BrepOp` exact-kernel evaluator, which re-uses neither this reader nor this writer), condition a triangle mesh (that is `mesh/repair.md#MESH`), or encode a mesh file (that is the data `MeshPayload` owner). The deleted forms: the shape-only `STEPControl_Reader` that drops the assembly/color/name metadata the XCAF reader preserves, a hand-rolled STEP/IGES parser or B-rep tessellator, the retired conda-only `pythonocc-core` `OCC.Core.*` path, a second daemon for mechanical geometry, a per-format `_run_step`/`_run_iges` method family over the `READERS` row, an inline `match`/`assert_never` reader construction where the table resolves it, a `Set*Mode` cascade re-spelled per reader where the `MetadataMode`/`_APPLY` cascade owns it, a domain `raise ValueError` on a failed `ReadFile`/`Transfer` where the `boundary` fence converts the `BridgeFault` through the one rail, a parallel tagged `BridgeError` error family no caller matches on where the runtime `BoundaryFault` taxonomy already owns the fault discrimination and the closed `BridgeStage` vocabulary names the failure mode in one message factory, a contributor-first `tessellate(contributor, ...)` signature the daemon's contributor-free `Ok(glb)`/`Error(fault)` match across the no-pickle lane hop cannot satisfy, a `delete=False` `NamedTemporaryFile` orphaning the source and GLB temp files on every call where one `TemporaryDirectory` scopes both under one cleanup, a `_raise` single-caller thunk where the `READERS.try_find` `match` raises `UNKNOWN_FORMAT` in its none arm, a per-row `subject` field where `BridgeFormat.subject` owns the one `step-bridge.<fmt>` tag, a pre-stringified `f"{mass:.6g}"`/`str()` fact where the receipts `Encoder(enc_hook=repr)` carries the native `float`/`int`/`StrEnum`, and a single fixed-`bytes` return where the `BridgeView` overload parameterizes the daemon's raw-GLB drain against the in-process receipt-carrying drain.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, overload

from OCP.BRepGProp import BRepGProp
from OCP.BRepMesh import BRepMesh_IncrementalMesh
from OCP.GProp import GProp_GProps
from OCP.IFSelect import IFSelect_ReturnStatus
from OCP.IGESCAFControl import IGESCAFControl_Reader
from OCP.Message import Message_ProgressRange
from OCP.RWGltf import RWGltf_CafWriter
from OCP.STEPCAFControl import STEPCAFControl_Reader
from OCP.TColStd import TColStd_IndexedDataMapOfStringString
from OCP.TCollection import TCollection_AsciiString, TCollection_ExtendedString
from OCP.TDF import TDF_LabelSequence
from OCP.TDocStd import TDocStd_Document
from OCP.TopoDS import TopoDS_Builder, TopoDS_Compound, TopoDS_Shape
from OCP.XCAFApp import XCAFApp_Application
from OCP.XCAFDoc import XCAFDoc_DocumentTool, XCAFDoc_ShapeTool
from expression import Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.content_identity import CANONICAL_POLICY, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import Receipt, ReceiptContributor

# --- [TYPES] ----------------------------------------------------------------------------


class BridgeFormat(StrEnum):
    STEP = "step"
    IGES = "iges"

    # the one fault/span/receipt subject for this format, owned on the enum so the `step-bridge.<fmt>`
    # tag is spelled once rather than re-interpolated in `READERS`, `BridgeFault.of`, the receipt, and
    # the `tessellate` table-miss fallback.
    @property
    def subject(self) -> str:
        return f"step-bridge.{self}"


# the XCAF metadata channels a CAF reader transfers into the label tree; the row carries the
# subset its reader admits, so the `Set{mode}Mode(True)` cascade is a table walk not a re-spell.
class MetadataMode(StrEnum):
    COLOR = "Color"
    NAME = "Name"
    LAYER = "Layer"
    GDT = "GDT"
    MAT = "Mat"


# the closed read-leg failure stage the one `BridgeFault.of` message factory discriminates, so a new
# failure mode is one row not a new exception class — the fault taxonomy itself stays the faults owner's.
class BridgeStage(StrEnum):
    UNKNOWN_FORMAT = "unknown format"
    READ_FAILED = "ReadFile failed"
    TRANSFER_FAILED = "Transfer failed"
    NO_ROOT = "no free-shape root"


type BridgeView = Literal["glb", "full"]
type CafReader = STEPCAFControl_Reader | IGESCAFControl_Reader

# --- [CONSTANTS] ------------------------------------------------------------------------

# the XCAF document storage format the OCAF application initializes the assembly tree under.
_XCAF_STORAGE: Final[str] = "MDTV-XCAF"

# --- [MODELS] ---------------------------------------------------------------------------


# the read leg's typed carrier: the populated XCAF document, the assembly-root shape feeding the
# mesher, and the transferred free-shape count the receipt folds — one value object the mesh/write/
# evidence legs read rather than three loose locals threaded through the kernel.
class XcafSession(Struct, frozen=True, gc=False):
    document: TDocStd_Document
    root: TopoDS_Shape
    shape_count: int


class CadReceipt(Struct, frozen=True, gc=False):
    fmt: BridgeFormat
    shape_count: int
    mass: float

    # the `ReceiptContributor` port yields an `Iterable[Receipt]` (not a bare `Receipt`) so the
    # receipts `_stream`/`@receipted` fold iterates it directly; the format/shape-count/mass fold
    # the `(Phase, subject, facts)` evidence the `Receipt.of` factory mints into the `fact` case.
    # The facts carry native scalars (`StrEnum`/`int`/`float`) the receipts owner's
    # `Encoder(enc_hook=repr, order="deterministic")` renders without a `str()`/`f"{...:.6g}"` coerce.
    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("mesh.cad", ("emitted", self.fmt.subject, {"format": self.fmt, "shapes": self.shape_count, "mass": self.mass})),)


class CadTessellation(Struct, frozen=True, gc=False):
    glb: bytes
    receipt: CadReceipt


# the per-format dispatch row: the CAF reader factory and the metadata channels its `Set*Mode` cascade
# folds. The fault/span subject is `BridgeFormat.subject`, not a row field. A new CAD source is one row.
class ReaderRow(Struct, frozen=True, gc=False):
    reader: Callable[[], CafReader]
    modes: Block[MetadataMode]


# --- [ERRORS] ---------------------------------------------------------------------------


# the one read-leg failure the kernel raises INTO its own `boundary` so the runtime `_convert`/`CLASSIFY`
# fold lands it in the `boundary` `BoundaryFault` case carrying the `fmt.subject` and — because `BridgeFault`
# is UNCLASSIFIED, hitting the faults catch-all whose `detail = str(cause) or type(cause).__name__` — the
# full `step-bridge.<stage>` discriminating message in `detail`, so the failure mode rides the message the
# receipts `rejected` projection spreads and the daemon's offload re-raise preserves, never a parallel tagged
# error rail the faults owner already owns. `BridgeFault.of` is the one factory minting the distinct
# read/transfer/root/format messages, so the kernel raises `BridgeFault.of(...)` rather than four sibling exception classes.
class BridgeFault(Exception):
    @staticmethod
    def of(stage: BridgeStage, fmt: BridgeFormat, status: IFSelect_ReturnStatus | None = None) -> "BridgeFault":
        suffix = f" ({status})" if status is not None else ""
        return BridgeFault(f"{fmt.subject}: {stage}{suffix}")


# --- [TABLES] ---------------------------------------------------------------------------

# one behavior row per format: the kernel resolves the reader and its metadata channels once through
# `READERS[fmt]` rather than an inline `match`/`assert_never` reader construction per leg.
READERS: Final[Map[BridgeFormat, ReaderRow]] = Map.of_seq([
    (
        BridgeFormat.STEP,
        ReaderRow(
            STEPCAFControl_Reader, Block.of_seq([MetadataMode.COLOR, MetadataMode.NAME, MetadataMode.LAYER, MetadataMode.GDT, MetadataMode.MAT])
        ),
    ),
    (BridgeFormat.IGES, ReaderRow(IGESCAFControl_Reader, Block.of_seq([MetadataMode.COLOR, MetadataMode.NAME, MetadataMode.LAYER]))),
])

# the `Set{mode}Mode` selector per channel: the cascade walks `row.modes` through this table so a new
# channel is one row reaching every reader that admits it, never a per-reader `Set*Mode` re-spell.
_APPLY: Final[Map[MetadataMode, Callable[[CafReader], None]]] = Map.of_seq([
    (MetadataMode.COLOR, lambda r: r.SetColorMode(True)),
    (MetadataMode.NAME, lambda r: r.SetNameMode(True)),
    (MetadataMode.LAYER, lambda r: r.SetLayerMode(True)),
    (MetadataMode.GDT, lambda r: r.SetGDTMode(True)),
    (MetadataMode.MAT, lambda r: r.SetMatMode(True)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


# the read leg: resolve the row (a table miss raises `UNKNOWN_FORMAT` in the none arm, never a
# statement-bodied raise thunk a lambda cannot host), init the XCAF document, write the bytes to the
# temp path the path-based OCCT reader consumes, apply the row's metadata channels, read+transfer, and
# resolve the assembly-root shape — every failure raised as a `BridgeFault` the enclosing `boundary`
# converts, never a status the caller checks. The `_APPLY[mode]` index is total by construction: every
# `MetadataMode` member carries an `_APPLY` row, so the cascade is a closed-vocabulary side-effect walk.
def _read(src_path: str, fmt: BridgeFormat) -> XcafSession:
    match READERS.try_find(fmt):
        case Some(row):
            reader = row.reader()
        case _:
            raise BridgeFault.of(BridgeStage.UNKNOWN_FORMAT, fmt)
    document = TDocStd_Document(TCollection_ExtendedString(_XCAF_STORAGE))
    XCAFApp_Application.GetApplication_s().InitDocument(document)
    for mode in row.modes:
        _APPLY[mode](reader)
    if (status := reader.ReadFile(src_path)) != IFSelect_ReturnStatus.IFSelect_RetDone:
        raise BridgeFault.of(BridgeStage.READ_FAILED, fmt, status)
    if not reader.Transfer(document):
        raise BridgeFault.of(BridgeStage.TRANSFER_FAILED, fmt)
    tool = XCAFDoc_DocumentTool.ShapeTool_s(document.Main())
    tool.GetFreeShapes(labels := TDF_LabelSequence())
    if labels.Length() < 1:
        raise BridgeFault.of(BridgeStage.NO_ROOT, fmt)
    return XcafSession(document, _root(tool, labels), labels.Length())


# every free shape welded into one `TopoDS_Compound` so the mesher and the `GProp` accumulator span the
# whole transferred assembly: `RWGltf_CafWriter.Perform` serializes EVERY free shape, so meshing only
# `Value(1)` would emit shapes 2..N with an empty triangulation. The one-based `Value(i)`/`GetShape_s`
# walk welds under one `TopoDS_Builder`, the boundary-kernel `for`-loop the sibling `mesh/brep.md`
# OCCT owner shares for its `TopExp.MapShapes_s` index — language control flow earned at the OCCT edge.
def _root(tool: XCAFDoc_ShapeTool, labels: TDF_LabelSequence) -> TopoDS_Shape:
    builder, compound = TopoDS_Builder(), TopoDS_Compound()
    builder.MakeCompound(compound)
    for i in range(1, labels.Length() + 1):
        builder.Add(compound, tool.GetShape_s(labels.Value(i)))
    return compound


# the mesh+write+evidence leg: mesh in place under the policy deflection, read the `GProp` mass, write
# the binary GLB through the CAF writer, and read the bytes back — one `CadTessellation` the view selects.
def _emit(session: XcafSession, glb_path: str, fmt: BridgeFormat, policy: IdentityPolicy) -> CadTessellation:
    BRepMesh_IncrementalMesh(session.root, policy.deflection, False, policy.angle_tolerance, True)
    props = GProp_GProps()
    BRepGProp.VolumeProperties_s(session.root, props)
    RWGltf_CafWriter(TCollection_AsciiString(glb_path), True).Perform(
        session.document, TColStd_IndexedDataMapOfStringString(), Message_ProgressRange()
    )
    return CadTessellation(Path(glb_path).read_bytes(), CadReceipt(fmt, session.shape_count, props.Mass()))


# one `TemporaryDirectory` scopes the source and GLB round-trip paths under one cleanup, so neither the
# OCCT path-based reader's input nor the CAF writer's output leaks past the call.
def _run(source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy) -> CadTessellation:
    with TemporaryDirectory(prefix="step-bridge-") as work:
        src_path = Path(work, f"src.{fmt}")
        src_path.write_bytes(source_bytes)
        return _emit(_read(str(src_path), fmt), str(Path(work, "out.glb")), fmt, policy)


# --- [SERVICES] -------------------------------------------------------------------------


class StepBridge:
    @overload
    @staticmethod
    def tessellate(source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy = ..., *, view: Literal["glb"] = ...) -> "RuntimeRail[bytes]": ...
    @overload
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy = ..., *, view: Literal["full"]
    ) -> "RuntimeRail[CadTessellation]": ...
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy = CANONICAL_POLICY, *, view: BridgeView = "glb"
    ) -> "RuntimeRail[bytes] | RuntimeRail[CadTessellation]":
        # run the kernel inside one `boundary` so a table miss, a failed `ReadFile`/`Transfer`, or a
        # missing root converts through the one `_convert` rail, then project the view: `glb` drains raw
        # bytes the daemon matches `Ok(glb)`/`Error(fault)`; `full` carries the `CadReceipt` for the
        # in-process harvest. The fence subject is `fmt.subject`, total for every format and so always
        # bound even when the table miss whose absence is the fault is itself the failure being raised.
        railed = boundary(fmt.subject, lambda: _run(source_bytes, fmt, policy))
        return railed if view == "full" else railed.map(lambda t: t.glb)
```

## [03]-[RESEARCH]

- [XCAF_ROOT_SHAPE]: the `XCAFDoc_ShapeTool.GetFreeShapes(TDF_LabelSequence)` out-parameter fill, the `TDF_LabelSequence.Value(i)`/`.Length()` one-based accessors, and `XCAFDoc_ShapeTool.GetShape_s(label) -> TopoDS_Shape` resolving each free-shape root the `_root` `TopoDS_Builder.Add` welds into the single `TopoDS_Compound` that feeds `BRepMesh_IncrementalMesh` — every free shape, since `RWGltf_CafWriter.Perform` serializes them all and meshing only `Value(1)` would emit shapes 2..N with an empty triangulation — confirm against the branch `cadquery-ocp` catalogue `[03]`/`[04]` on the cp312 companion; the catalogue confirms `XCAFDoc_DocumentTool.ShapeTool_s(label)`, the `TopoDS_Builder.MakeCompound`/`Add` compound weld, the seven `STEPCAFControl_Reader.Set*Mode` toggles (`SetColorMode`/`SetNameMode`/`SetLayerMode`/`SetGDTMode`/`SetMatMode`/`SetViewMode`/`SetPropsMode`) the `_APPLY` table folds the `COLOR`/`NAME`/`LAYER`/`GDT`/`MAT` subset of, `ReadFile -> IFSelect_ReturnStatus`/`Transfer(doc) -> bool`, the 5-arg `BRepMesh_IncrementalMesh(shape, deflection, isRelative, angle, parallel)`, and the 3-arg `RWGltf_CafWriter(path, binary).Perform(doc, fileInfo, progress)`. The catalogue carries every member this kernel cites with no residual fence: the `XCAFApp_Application.GetApplication_s()` static plus `InitDocument(document)` instance document-init pair, the 5-arg `BRepMesh_IncrementalMesh` overload (shared with `mesh/brep.md` on the same seam), the `XCAFDoc_ShapeTool.GetFreeShapes(TDF_LabelSequence)` out-parameter fill (filling a caller-constructed sequence), the `TDocStd_Document(TCollection_ExtendedString(...))` ctor (an `AsciiString`/bare `str` raises `TypeError`), and the 3-arg `Perform(doc, fileInfo, progress)` minimal write whose empty `TColStd_IndexedDataMapOfStringString` returns `True` — there is no 2-arg `Perform(doc, progress)` overload.
- [GLB_BYTE_RETURN]: the `RWGltf_CafWriter` binary-GLB single-file emission (whether the writer flushes one `.glb` or a `.gltf` plus a side buffer for `binary=True`) confirms against the same catalogue so the `Path.read_bytes` leg reads one self-contained GLB; the in-memory `OSD_FileSystem`/stream sink that retires the temp-path round-trip resolves on the same seam as the warm-pool work in `mesh/daemon.md#3-RESEARCH`. The temp-path round-trip is the deleted form once that seam lands; until then one `TemporaryDirectory` scopes both the source-write and GLB-read paths under one cleanup, the path-based OCCT reader/writer admission the catalogue confirms with no `delete=False` orphan.

## [04]-[UPSTREAM]
