# [BIM_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`; every `QUEUED` row names the depth-fill that flips its cell from `QUEUED` to `FINALIZED`.

## [1]-[OBJECT_MODEL_DEPTH_FILL]

The genuinely-new BIM object-model owners carry transcription-complete cards and growth axes; the signature fences are queued depth-fill. Each row flips its owner registry cell from `QUEUED` to `FINALIZED` when the fence transcribes and resolves its RESEARCH item.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `IfcClass` closed buildingSMART element-class row enumeration + `BimElement`/`BimModel` record fence + the `Project` fold over the `IfcSemanticModel` product/property/quantity/material/type rows; grounds against the GeometryGym entity-class surface and the kernel `Rasm` geometry-handle shape | Model/object-model#ELEMENT_MODEL | QUEUED |
| [2] | `ElementPredicate` `[Union]` arm payloads + the `ElementSet` set-algebraic fold (`Union`/`Intersect`/`Except`/`Where`/`Query`) over the `BimModel` element collection | Model/object-model#ELEMENT_SET | QUEUED |
| [3] | `Classification` standard-systems row set + `ClassificationCode` code-shape policies (Uniclass2015/OmniClass/MasterFormat/Uniformat) + the `IfcRelAssociatesClassification` round-trip | Model/object-model#CLASSIFICATION | QUEUED |
| [4] | `SpatialContainer`/`AssemblyRel` `[Union]` + the `BimAssembly.Assemble` fold projecting the IFC spatial hierarchy and the `IfcRel*` decomposition relationships into the host-neutral tree | Model/object-model#ASSEMBLY | QUEUED |

## [2]-[CROSS_FOLDER_PROBES]

Probes whose owner member shape is transcription-complete; the open gate is a cross-folder or cross-branch alignment named in the page RESEARCH cluster.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `InterchangeIdentity.Key(string formatKey, ReadOnlySpan<byte> bytes, double deflection, double tolerance, double angleTolerance)` content-key public signature confirms against the `csharp:Compute/interchange#CONTENT_ADDRESSING` owner; the `ExportArtifact.ContentKey`/`TessellationRequest.IfcContentKey` slots consume it as settled vocabulary | Exchange/interchange#EXPORT_RAIL · Exchange/interchange#TESSELLATION_BRIDGE | SPIKE |
| [2] | `TessellationRequest` companion orchestration (issue over the companion rpc, re-import the GLB, content-key cache reuse) lands at `csharp:Compute/interchange#TWO_HOP_TESSELLATION`; the IfcOpenShell companion-daemon protocol lands at `python:geometry/ifc-companion`; Bim's request shape is transcription-complete | Exchange/interchange#TESSELLATION_BRIDGE | SPIKE |
| [3] | AP242 / native-companion / IFC5 / Draco / Meshopt / 3D-Tiles codec member spellings ground against the STEP, native-companion, GeometryGym IFC5, and SharpGLTF toolkit surfaces; the mesh-text (STL/3MF/OBJ/PLY) decode bodies ground at the admitted reader packages; candidate USD/FBX/COLLADA rows promote in place at admission | Exchange/interchange#FORMAT_AXIS | SPIKE |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (`Faults.cs` first, `Exchange/Interchange.cs` before `Model/`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order files per `ARCHITECTURE.md` `[SOURCE_TREE]`; `BimFault` (band 2600) and `InterchangeKeyPolicy` land first | Exchange/interchange#FORMAT_AXIS | QUEUED |
