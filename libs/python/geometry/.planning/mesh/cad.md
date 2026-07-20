# [PY_GEOMETRY_MESH_CAD]

One ISO 10303 STEP and IGES tessellation hop — the CAD source formats the `mesh/daemon.md#DAEMON` `TessellationDaemon` serves through its `cad` arm. `StepBridge` reads B-rep bytes through the OCCT XCAF readers into a `TDocStd_Document`, meshes the transferred shape in place under the `TessellationPolicy` band, and writes GLB through the native `RWGltf_CafWriter`; one `READERS` row per format makes a new CAD source one row, never a parallel reader method.

`TessellationPolicy` is minted here beside `BridgeFormat` — the mesher knobs are geometry-owned, never runtime `IdentityPolicy` fields, and the `mesh/daemon`/`mesh/brep` consumers import them downward. This bridge mints no `ContentKey`: the daemon keys the SOURCE bytes before the offload hop, so an output-GLB key — existing only after the kernel runs — never serves the cache hit. This hop rides `cadquery-ocp`, the sole PyPI OCCT path; the wire aligns to the C# `StepIso10303` codec, which requests CAD tessellation from this companion rather than re-implementing a managed reader.

## [01]-[INDEX]

- [01]-[BRIDGE]: STEP/IGES reader-to-GLB hop over the `READERS` table and the `_APPLY` metadata cascade, `boundary`-fenced, output-parameterized over `BridgeView`.

## [02]-[BRIDGE]

- Owner: `StepBridge` — the static surface over the XCAF reader chain; `READERS` carries one behavior row per format so the kernel never re-discriminates the reader past the table; `BridgeFormat.subject` owns the one `step-bridge.<fmt>` fault/span/receipt tag; `BridgeView` parameterizes the output so the daemon's lane-subinterpreter call matches raw `bytes` while an in-process caller drains the receipt-carrying `CadTessellation`.
- Cases: `STEP` binds the full `COLOR`/`NAME`/`LAYER`/`GDT`/`MAT` channel set, `IGES` the `COLOR`/`NAME`/`LAYER` subset its reader admits; the daemon never re-discriminates format past this owner.
- Auto: the `"glb"` view drops the receipt because a live contributor cannot cross the no-pickle lane hop — the daemon's `@receipted` aspect owns the daemon-level fold; the `"full"` view carries the `CadReceipt` for the in-process harvest.
- Packages: `cadquery-ocp` (the `OCP.*` XCAF reader/writer band, module-scope `lazy from` so the loop-floor consumers of `TessellationPolicy`/`BridgeFormat` never load OCCT — `TCollection_ExtendedString` is the REQUIRED `TDocStd_Document` storage string, an `AsciiString` or bare `str` raises), `expression`, `msgspec`, and the runtime rails; a malformed STEP stream is a deterministic `BridgeFault`, never a transient the resilience owner retries, so this owner stacks no second retry rail.
- Growth: a new CAD source is one `BridgeFormat` row and one `ReaderRow` and one alias on the daemon `cad` case; a new metadata channel is one `MetadataMode` member and one `_APPLY` row reaching every reader that admits it; a new output projection is one `BridgeView` member and one view arm; `RWGltf_CafWriter.Perform(doc, fileInfo, progress)` is the minimal write arity — there is no 2-arg `Perform(doc, progress)` — so glTF asset metadata populates the already-present `fileInfo` map in place, and the 5-arg selective-root overload threads a partial-assembly export.
- Boundary: the bridge mints no transport, channel, or content key; evaluating an already-in-memory `TopoDS_Shape` is `mesh/brep.md#BREP`'s (which reuses neither this reader nor this writer), mesh conditioning is `mesh/repair.md#MESH`'s, and mesh-file codec is the data `MeshPayload` owner's; the shape-only `STEPControl_Reader` (it drops the assembly/color/name metadata the XCAF reader preserves) and the conda-only `pythonocc-core` `OCC.Core.*` path never enter.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import Final, Literal, overload

from expression import Option, Some
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.faults import RuntimeRail, boundary
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


# each ReaderRow carries the channel subset its reader admits, so the `Set{mode}Mode(True)` cascade is a table walk.
class MetadataMode(StrEnum):
    COLOR = "Color"
    NAME = "Name"
    LAYER = "Layer"
    GDT = "GDT"
    MAT = "Mat"


# closed read-leg failure stage `BridgeFault.of` discriminates — a new failure mode is one row, never a new exception class.
class BridgeStage(StrEnum):
    UNKNOWN_FORMAT = "unknown format"
    READ_FAILED = "ReadFile failed"
    TRANSFER_FAILED = "Transfer failed"
    NO_ROOT = "no free-shape root"
    WRITE_FAILED = "glTF write failed"


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


class CadReceipt(Struct, frozen=True, gc=False):
    fmt: BridgeFormat
    shape_count: int
    mass: float

    # native-scalar facts; the one-row tuple return satisfies the contributor port's iterable.
    def contribute(self) -> Iterable[Receipt]:
        return (Receipt.of("rasm.geometry.mesh.cad", ("emitted", self.fmt.subject, {"format": self.fmt, "shapes": self.shape_count, "mass": self.mass})),)


class CadTessellation(Struct, frozen=True, gc=False):
    glb: bytes
    receipt: CadReceipt


# --- [ERRORS] ---------------------------------------------------------------------------


# raised INTO the `boundary` fence; UNCLASSIFIED, so the faults catch-all carries the full `step-bridge.<stage>` message in
# `detail` — the failure mode rides the message the daemon's offload re-raise preserves, never a parallel tagged rail.
class BridgeFault(Exception):
    @staticmethod
    def of(stage: BridgeStage, fmt: BridgeFormat, status: IFSelect_ReturnStatus | None = None) -> "BridgeFault":
        suffix = f" ({status})" if status is not None else ""
        return BridgeFault(f"{fmt.subject}: {stage}{suffix}")


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

# `Set{mode}Mode` selector per channel; the cascade walks `row.modes` through this table.
_APPLY: Final[Map[MetadataMode, Callable[[CafReader], None]]] = Map.of_seq([
    (MetadataMode.COLOR, lambda r: r.SetColorMode(True)),
    (MetadataMode.NAME, lambda r: r.SetNameMode(True)),
    (MetadataMode.LAYER, lambda r: r.SetLayerMode(True)),
    (MetadataMode.GDT, lambda r: r.SetGDTMode(True)),
    (MetadataMode.MAT, lambda r: r.SetMatMode(True)),
])

# --- [OPERATIONS] -----------------------------------------------------------------------


# every failure raises a `BridgeFault` the enclosing `boundary` converts, never a status the caller checks; the
# `_APPLY[mode]` index is total by construction — every `MetadataMode` member carries an `_APPLY` row.
def _read(src_path: str, fmt: BridgeFormat) -> XcafSession:
    match READERS.try_find(fmt):
        case Option(tag="some", some=(factory, modes)):
            reader = factory()
        case _:
            raise BridgeFault.of(BridgeStage.UNKNOWN_FORMAT, fmt)
    document = TDocStd_Document(TCollection_ExtendedString(_XCAF_STORAGE))
    XCAFApp_Application.GetApplication_s().InitDocument(document)
    for mode in modes:
        _APPLY[mode](reader)
    if (status := reader.ReadFile(src_path)) != IFSelect_ReturnStatus.IFSelect_RetDone:
        raise BridgeFault.of(BridgeStage.READ_FAILED, fmt, status)
    if not reader.Transfer(document):
        raise BridgeFault.of(BridgeStage.TRANSFER_FAILED, fmt)
    tool = XCAFDoc_DocumentTool.ShapeTool_s(document.Main())
    tool.GetFreeShapes(labels := TDF_LabelSequence())
    if labels.Length() < 1:
        raise BridgeFault.of(BridgeStage.NO_ROOT, fmt)
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
        raise BridgeFault.of(BridgeStage.WRITE_FAILED, fmt)
    return CadTessellation(sink.read_bytes(), CadReceipt(fmt, shape_count, props.Mass()))


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
    def tessellate(source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = ..., *, view: Literal["glb"] = ...) -> "RuntimeRail[bytes]": ...
    @overload
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = ..., *, view: Literal["full"]
    ) -> "RuntimeRail[CadTessellation]": ...
    @staticmethod
    def tessellate(
        source_bytes: bytes, fmt: BridgeFormat, policy: TessellationPolicy = CANONICAL_TESSELLATION, *, view: BridgeView = "glb"
    ) -> "RuntimeRail[bytes] | RuntimeRail[CadTessellation]":
        # fence subject is `fmt.subject`, total for every format — bound even when the table miss itself is the failure raised.
        railed = boundary(fmt.subject, lambda: _run(source_bytes, fmt, policy))
        return railed if view == "full" else railed.map(lambda t: t.glb)
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
