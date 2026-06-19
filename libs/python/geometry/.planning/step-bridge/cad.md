# [PY_GEOMETRY_STEP_BRIDGE_CAD]

The ISO 10303 AP242/AP203/AP214 and IGES CAD-STEP tessellation hop — the second source format the one tessellation daemon serves. `StepBridge` reads STEP/IGES B-rep bytes through the OCCT `STEPCAFControl_Reader`/`IGESCAFControl_Reader` into an XCAF `TDocStd_Document`, meshes the transferred `TopoDS_Shape` in place with `BRepMesh_IncrementalMesh` under the deflection/tolerance policy, and writes the XCAF document to GLB through the native `RWGltf_CafWriter`, returning the same content-keyed GLB the IFC arm produces. The hop rides the `cadquery-ocp` `OCP` binding (the sole PyPI OCCT path); the daemon routes its STEP/IGES `SourceFormat` rows here and re-enters the shared content-identity keying, so one daemon serves AEC (IFC) and mechanical (CAD-STEP) geometry through one content-addressed hop. Aligned at the wire to the C# `StepIso10303` codec, which requests CAD tessellation from this companion rather than re-implementing a managed reader.

## [1]-[INDEX]

[CLUSTERS]:
- `[2]-[BRIDGE]`: the STEP/IGES reader-to-GLB hop over the OCCT XCAF reader, the in-place mesher, and the native glTF CAF writer.

## [2]-[BRIDGE]

- Owner: `StepBridge` — the static surface driving the OCCT XCAF reader chain over `cadquery-ocp`; `BridgeFormat` the closed `StrEnum` aliasing the daemon `SourceFormat` CAD rows to the reader that owns each; `BridgeMesh` the per-format reader carrier holding the populated `TDocStd_Document` and the transferred `TopoDS_Shape` count, so one `tessellate` call meshes and writes the assembly in one pass without a temp file.
- Cases: `BridgeFormat` rows `STEP` (the `STEPCAFControl_Reader` assembly/color/name transfer) and `IGES` (the `IGESCAFControl_Reader` color/name/layer transfer) — matched by `match`/`assert_never`, each binding the CAF reader that owns the format and sharing the XCAF document, the in-place `BRepMesh_IncrementalMesh`, and the `RWGltf_CafWriter` output leg; the daemon never re-discriminates format past this owner.
- Entry: `StepBridge.tessellate` admits source bytes, a `BridgeFormat`, and an `IdentityPolicy`, writes the bytes to the temp path the OCCT path-based reader consumes, runs `ReadFile`/`Transfer` to populate the XCAF document, meshes the `XCAFDoc_DocumentTool.ShapeTool_s` root shape with `BRepMesh_IncrementalMesh(shape, deflection)`, writes the document to a binary GLB through the catalogued `RWGltf_CafWriter(path, binary=True).Perform(doc, progress)`, reads the GLB bytes back, and returns the raw GLB; the daemon — the single `MeshResult` content owner — keys the returned bytes through `ContentIdentity.of` under the same policy, so a CAD source content-addresses byte-identically to its IFC sibling at identical settings and the bridge mints no key.
- Auto: `STEPCAFControl_Reader.SetColorMode(True)`/`SetNameMode(True)`/`SetLayerMode(True)` select the assembly metadata transferred into the XCAF label tree; `ReadFile` returns an `IFSelect_ReturnStatus` checked against `RetDone` before `Transfer(doc)` populates the document, so a failed read raises through the boundary rather than emitting an empty GLB; `BRepMesh_IncrementalMesh` mutates the shape's stored triangulation in place from the `IdentityPolicy.deflection`, and `RWGltf_CafWriter` consumes exactly that triangulation, so meshing precedes the write; `BRepGProp.VolumeProperties_s` populates a `GProp_GProps` accumulator carrying `Mass`/`CentreOfMass` as the per-solid geometric receipt the bridge folds into the element count.
- Receipt: a CAD tessellation contributes an emitted-phase `Receipt.of` row through `ReceiptContributor` carrying the format, the transferred shape count, and the solid mass read off the `GProp_GProps` accumulator; the daemon keys the GLB and folds the element count, so the bridge mints no second content key.
- Packages: `cadquery-ocp` (`OCP.STEPCAFControl.STEPCAFControl_Reader`/`OCP.IGESCAFControl.IGESCAFControl_Reader`/`OCP.TDocStd.TDocStd_Document`/`OCP.XCAFApp.XCAFApp_Application`/`OCP.XCAFDoc.XCAFDoc_DocumentTool`/`OCP.BRepMesh.BRepMesh_IncrementalMesh`/`OCP.RWGltf.RWGltf_CafWriter`/`OCP.IFSelect.IFSelect_ReturnStatus`/`OCP.Message.Message_ProgressRange`/`OCP.TCollection.TCollection_AsciiString`/`OCP.BRepGProp.BRepGProp`/`OCP.GProp.GProp_GProps`), runtime (`IdentityPolicy`/`ReceiptContributor`/`RuntimeRail`).
- Growth: a new CAD source is one `BridgeFormat` row binding its CAF reader plus one alias row on the daemon `SourceFormat`; a new transferred-metadata mode is one `Set*Mode` bind; the deflection knob is the shared `IdentityPolicy.deflection` already folded into the daemon cache key; zero new surface.
- Boundary: the bridge mints no transport, channel, or content key — it returns raw GLB bytes the daemon keys through the shared `ContentIdentity`, and speaks the existing C# `ComputeService`/`ArtifactSync` contract through the daemon's `ServerHost`. The GLB it returns is the shape the C# `SharpGLTF` import reads. The shape-only `STEPControl_Reader` (drops the assembly/color/name metadata), a hand-rolled STEP/IGES parser or B-rep tessellator, the retired conda-only `pythonocc-core` `OCC.Core.*` path, and a second daemon for mechanical geometry are the deleted forms — the XCAF CAF reader is the load-bearing entry because it preserves the assembly instance tree the shape-only reader discards.

```python signature
from enum import StrEnum
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import assert_never

from OCP.BRepGProp import BRepGProp
from OCP.BRepMesh import BRepMesh_IncrementalMesh
from OCP.GProp import GProp_GProps
from OCP.IFSelect import IFSelect_ReturnStatus
from OCP.IGESCAFControl import IGESCAFControl_Reader
from OCP.Message import Message_ProgressRange
from OCP.RWGltf import RWGltf_CafWriter
from OCP.STEPCAFControl import STEPCAFControl_Reader
from OCP.TCollection import TCollection_AsciiString
from OCP.TDocStd import TDocStd_Document
from OCP.XCAFApp import XCAFApp_Application
from OCP.XCAFDoc import XCAFDoc_DocumentTool

from rasm.runtime.content_identity import CANONICAL_POLICY, IdentityPolicy
from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.receipts import ReceiptContributor


class BridgeFormat(StrEnum):
    STEP = "step"
    IGES = "iges"


class StepBridge:
    @staticmethod
    def tessellate(
        contributor: ReceiptContributor,
        source_bytes: bytes,
        fmt: BridgeFormat,
        policy: IdentityPolicy = CANONICAL_POLICY,
    ) -> "RuntimeRail[bytes]":
        return boundary(f"step-bridge.{fmt}", lambda: StepBridge._run(contributor, source_bytes, fmt, policy))

    @staticmethod
    def _run(contributor: ReceiptContributor, source_bytes: bytes, fmt: BridgeFormat, policy: IdentityPolicy) -> bytes:
        app = XCAFApp_Application.GetApplication_s()
        doc = TDocStd_Document(TCollection_AsciiString("MDTV-XCAF"))
        app.InitDocument(doc)
        with NamedTemporaryFile(suffix=f".{fmt}", delete=False) as src:
            src.write(source_bytes)
            src_path = src.name
        match fmt:
            case BridgeFormat.STEP:
                reader = STEPCAFControl_Reader()
            case BridgeFormat.IGES:
                reader = IGESCAFControl_Reader()
            case unreachable:
                assert_never(unreachable)
        reader.SetColorMode(True)
        reader.SetNameMode(True)
        reader.SetLayerMode(True)
        status = reader.ReadFile(src_path)
        if status != IFSelect_ReturnStatus.IFSelect_RetDone or not reader.Transfer(doc):
            raise ValueError(f"step-bridge.{fmt} transfer failed: {status}")
        shape_tool = XCAFDoc_DocumentTool.ShapeTool_s(doc.Main())
        labels = shape_tool.GetFreeShapes()
        root = shape_tool.GetShape_s(labels.Value(1))
        BRepMesh_IncrementalMesh(root, policy.deflection, False, policy.angle_tolerance, True)
        props = GProp_GProps()
        BRepGProp.VolumeProperties_s(root, props)
        with NamedTemporaryFile(suffix=".glb", delete=False) as dst:
            glb_path = dst.name
        writer = RWGltf_CafWriter(TCollection_AsciiString(glb_path), True)
        writer.Perform(doc, Message_ProgressRange())
        glb = Path(glb_path).read_bytes()
        contributor.contribute(
            "emitted", "step-bridge", glb_path, {"format": fmt, "shapes": str(labels.Length()), "mass": str(props.Mass())}
        )
        return glb
```

## [3]-[RESEARCH]

- [XCAF_ROOT_SHAPE]: the `XCAFDoc_ShapeTool.GetFreeShapes() -> TDF_LabelSequence`, the `TDF_LabelSequence.Value(1)`/`.Length()` one-based accessors, and `XCAFDoc_ShapeTool.GetShape_s(label) -> TopoDS_Shape` resolving the single assembly-root `TopoDS_Shape` that feeds `BRepMesh_IncrementalMesh` confirm against the branch `cadquery-ocp` catalogue on the cp312 companion; the catalogue confirms `XCAFDoc_DocumentTool.ShapeTool_s(label)`, `STEPCAFControl_Reader.ReadFile`/`Transfer`/`Set*Mode`, `BRepMesh_IncrementalMesh(shape, deflection)`, and `RWGltf_CafWriter(path, binary).Perform(doc, progress)`. The fence commits to the catalogued 2-arg `RWGltf_CafWriter(path, binary).Perform(doc, progress)`; the unconfirmed members are the `XCAFApp_Application.GetApplication_s`/`InitDocument` document-init pair, the `BRepMesh_IncrementalMesh(shape, deflection, isRelative, angle, parallel)` five-arg overload, and the optional 3-arg `Perform(doc, fileInfo, progress)` `fileInfo` map (`TColStd_IndexedDataMapOfStringString`) that carries glTF metadata — a growth knob the live catalogue resolves, never a fence dependency.
- [GLB_BYTE_RETURN]: the `RWGltf_CafWriter` binary-GLB single-file emission (whether the writer flushes one `.glb` or a `.gltf` plus a side buffer for `binary=True`) confirms against the same catalogue so the `Path.read_bytes` leg reads one self-contained GLB; the in-memory `OSD_FileSystem`/stream sink that retires the temp-path round-trip resolves on the same seam as the warm-pool work in `tessellation/daemon.md#3-RESEARCH`.

## [4]-[UPSTREAM]

- [WHEEL_BAND]: `cadquery-ocp 7.9.3.1.1` requires `<3.15,>=3.10` and ships binary wheels cp310-cp314 only (no abi3, no cp315), so the bridge rides the `python_version<'3.15'` companion band the branch manifest gates, alongside `compas`/`manifold3d` and below the tighter `kiss-matcher` `<'3.13'` band. The cp315 project venv carries no wheel; the bridge runs on the Forge `python312` companion interpreter (`forge-companion-env`) the daemon already hosts, the cp312 floor the native geometry/IFC cores share. This is the resolved replacement for the retired `pythonocc-core` conda-only blocker — the hop is no longer upstream-blocked, only companion-band gated.
