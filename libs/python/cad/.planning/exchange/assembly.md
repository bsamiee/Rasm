# [PY_CAD_EXCHANGE_ASSEMBLY]

`assembly` owns the CAF reader family and the XCAF document every tessellation path reads: one transfer admits a STEP or IGES source into a document carrying colour, name, and layer, and one flatten yields the located compound the mesher triangulates and the metrology fold measures. Source-path custody, triangulation, budget preflight, and glTF emission all stay outside this owner.

`exchange/step#PROTOCOL` supplies `declared` and `schema` and `exchange/step#CODEC` supplies `gated`; `faults#ROWS` supplies `CAF_TRANSFER`, `CAF_ROOTS`, and `STEP_SCHEMA`, and every refusal is `Error(<ROW>.at(...))` on `CadRail`. `tessellation/mesh#MESH` consumes `Assembly` whole — `document` feeds `RWGltf_CafWriter.Perform` and `root` feeds `BRepMesh_IncrementalMesh` — and `TessellateRequest.source` is the one closed dispatch that mints a reader.

## [01]-[INDEX]

- [02]-[DOCUMENT]: `TessellateRequest.source` dispatch, the CAF reader family, its surviving channels, and the transfer rail.
- [03]-[ROOTS]: `Assembly`, the located free-shape flatten, and the part identity the wire cannot yet carry.

## [02]-[DOCUMENT]

- Owner: `admitted` folds one `TessellateRequest.source` arm into a channelled reader, a transferred document, and a located root, so no consumer sequences those legs itself.
- Cases: `CafReader` closes over `STEPCAFControl_Reader` and `IGESCAFControl_Reader`, and each arm mints its reader together with the protocol arrow that reader can serve.
- Law: colour, name, and layer stay switched on because `RWGltf_CafWriter.Perform` reads all three back off the document — they are emission inputs, and the GLB is where they land.
- Law: `SetGDTMode` and `SetMatMode` are DELETED — glTF carries neither geometric tolerance nor physical material, and `TessellateResponse` exposes no field either channel projects onto.
- Law: both CAF readers carry all three surviving channels, so the family needs no narrowing cast and no per-source branch to configure them.
- Law: STEP admits its file-local protocol between `ReadFile` and `Transfer`; IGES declares no application protocol, so its arm mints the identity arrow and never a false protocol coordinate.
- Law: the STEP-only `Reader().StepModel()` hop rides its own arm's closure over the concrete reader, which is what retires `cast(STEPCAFControl_Reader, reader)` from the family.
- Entry: `pipeline` composes read, protocol, transfer, and root at four arrows, under the six its overload ladder types before degrading to `Result[Any, Any]`.
- Growth: a new exchange format is one `TessellateRequest.source` arm carrying its reader and its protocol arrow, and nothing downstream moves.
- Boundary: `service/spool` resolves the source reference onto the path this owner reads; no member here opens a store or touches an `ArtifactRef`.

```python signature
from pathlib import Path
from typing import assert_never

from expression import Error, Ok
from expression.collections import Block
from expression.extra.result import pipeline
from msgspec import Struct
from OCP.IGESCAFControl import IGESCAFControl_Reader
from OCP.STEPCAFControl import STEPCAFControl_Reader
from OCP.TCollection import TCollection_ExtendedString
from OCP.TDF import TDF_Label, TDF_LabelSequence
from OCP.TDocStd import TDocStd_Document
from OCP.TopoDS import TopoDS_Builder, TopoDS_Compound, TopoDS_Shape
from OCP.XCAFApp import XCAFApp_Application
from OCP.XCAFDoc import XCAFDoc_DocumentTool, XCAFDoc_ShapeTool
from protobuf import Oneof
from rasm.contracts.rasm.contracts.cad.service_pb import TessellateRequest
from rasm.contracts.rasm.contracts.cad.types_pb import SealedStep

from rasm.cad.exchange.step import ExchangeArrow, declared, gated, schema
from rasm.cad.faults import CAF_ROOTS, CAF_TRANSFER, STEP_SCHEMA, CadRail

# --- [TYPES] ----------------------------------------------------------------------------

# both CAF readers answer `ReadFile`, `Transfer`, and the three surviving `Set*Mode` channels, so the union is a
# structural family the codec drives directly rather than a base class needing a narrowing cast per call.
type CafReader = STEPCAFControl_Reader | IGESCAFControl_Reader


# --- [MODELS] ---------------------------------------------------------------------------


class Assembly(Struct, frozen=True):
    # one transfer yields two projections, neither derivable from the other: `document` keeps the label tree with
    # colour, name, and layer for `RWGltf_CafWriter.Perform`, and `root` is the geometry mesh and metrology read.
    document: TDocStd_Document
    root: TopoDS_Shape


# --- [OPERATIONS] -----------------------------------------------------------------------


def _document() -> TDocStd_Document:
    # `MDTV-XCAF` is the required storage format and the ctor rejects a bare `str` or an `AsciiString`; `InitDocument`
    # scaffolds the label tree, so a `Transfer` onto an un-inited document seats nothing and reports success.
    document = TDocStd_Document(TCollection_ExtendedString("MDTV-XCAF"))
    XCAFApp_Application.GetApplication_s().InitDocument(document)
    return document


def _channelled(reader: CafReader, /) -> CafReader:
    # colour, name, and layer are exactly the channels `RWGltf_CafWriter.Perform` reads back off the document; GD&T
    # and material transfer into labels no emission path this provider owns has any reader for.
    reader.SetColorMode(True)
    reader.SetNameMode(True)
    reader.SetLayerMode(True)
    return reader


def _reader(request: TessellateRequest, /) -> tuple[CafReader, ExchangeArrow[CafReader]]:
    # each arm mints its reader AND its protocol arrow, so the STEP-only `Reader().StepModel()` hop closes over the
    # concrete reader and the family never needs a cast; `Ok` is the identity arrow the protocol-less source takes.
    match request.source:
        case Oneof(field="step", value=SealedStep() as source):
            reader = STEPCAFControl_Reader()
            return reader, lambda held: declared(reader.Reader().StepModel()).bind(
                lambda found: Ok(held)
                if found == source.protocol
                else Error(STEP_SCHEMA.at(f"file-schema:{schema(found)}!={schema(source.protocol)}"))
            )
        case Oneof(field="iges"):
            return IGESCAFControl_Reader(), Ok
        case _ as unreachable:
            assert_never(unreachable)


def _transferred(reader: CafReader, /) -> CadRail[TDocStd_Document]:
    # `Transfer` answers a bare bool, not an `IFSelect_ReturnStatus`, so this leg states its own predicate
    document = _document()
    return Ok(document) if reader.Transfer(document) else Error(CAF_TRANSFER.at("CafReader.Transfer"))


def admitted(request: TessellateRequest, path: Path, /) -> CadRail[Assembly]:
    reader, matched = _reader(request)
    return pipeline(
        gated(CAF_TRANSFER, "CafReader.ReadFile", lambda held: held.ReadFile(str(path))),
        matched,
        _transferred,
        _rooted,
    )(_channelled(reader))
```

## [03]-[ROOTS]

- Owner: `_rooted` mints `Assembly`, and the flatten it performs is a second projection of the transferred document rather than a replacement for it.
- Law: `GetShape_s` on an assembly label returns the compound with every component location already applied, so the flatten preserves instance placement and the measured volume counts each instance once.
- Law: `GetReferredShape_s` and `GetLocation_s` stay uncomposed — `GetShape_s` already returns located geometry, so walking the instance tree rebuilds the same shapes as a second authority.
- Law: a null root label seats no geometry and is skipped by count; a document whose every free shape is null refuses on `CAF_ROOTS` instead of meshing an empty compound.
- Law: per-part identity reaches no receipt — `TessellateResponse` carries counts, one `BrepKernelReceipt`, and one `ArtifactRef`, and holds no part roster field to project a label onto.
- Law: instance count is read back from the emitted GLB at `metrology/census`, so a native label-tree count here is a parallel model of one number rather than new evidence.
- Growth: carrying part identity forward is a wire change first — one repeated part message on `TessellateResponse` — and only then one label walk seated on this owner.
- Boundary: triangulation, budget preflight, and the glTF writer belong to `tessellation/mesh` and `tessellation/emission`; this owner hands them the document and the root and stops.

```python signature
# --- [OPERATIONS] -----------------------------------------------------------------------


def _added(builder: TopoDS_Builder, compound: TopoDS_Compound, label: TDF_Label, /) -> int:
    # answers 1 for a root that seated geometry and 0 for a null label, so the caller's fold counts admitted roots
    shape = XCAFDoc_ShapeTool.GetShape_s(label)
    if shape.IsNull():
        return 0
    builder.Add(compound, shape)
    return 1


def _rooted(document: TDocStd_Document, /) -> CadRail[Assembly]:
    # `TDF_LabelSequence` fills as an out-parameter and reads one-based, and `Block.fold` drives the walk eagerly
    # because the builder mutation must run before the count is read; a lazy projection defers both.
    labels = TDF_LabelSequence()
    XCAFDoc_DocumentTool.ShapeTool_s(document.Main()).GetFreeShapes(labels)
    compound, builder = TopoDS_Compound(), TopoDS_Builder()
    builder.MakeCompound(compound)
    seated = Block.of_seq(range(1, labels.Length() + 1)).fold(
        lambda count, index: count + _added(builder, compound, labels.Value(index)), 0
    )
    return (
        Ok(Assembly(document=document, root=compound))
        if seated
        else Error(CAF_ROOTS.at(f"xcaf.free-shapes:{labels.Length()}:seated:{seated}"))
    )
```

## [04]-[RESEARCH]

- [PART_IDENTITY]-[OPEN]: which wire shape carries per-part name, colour, layer, and instance transform onto `TessellateResponse` without duplicating the GLB node graph; settle it at the contracts owner before any label walk lands here.
- [GLTF_CHANNELS]-[OPEN]: does `RWGltf_CafWriter.Perform` emit XCAF layer assignments into glTF, or only colour and name; write one layered document and read the emitted node extras back.
