# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[1]-[OPEN]` carries task cards with a `[QUEUED]`/`[ACTIVE]`/`[BLOCKED]`/`[UPSTREAM-BLOCKED]`/`[HOST-PROBE-DEFERRED]` leader; `[2]-[CLOSED]` carries `[COMPLETE]`/`[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

## [01]-[OPEN]

[UPSTREAM-BLOCKED] Ground the `BsddResolution` LIVE-WIRE transport leg against the Compute transport seam — `Semantics/classification.md#BSDD_RESOLUTION` `[3]-[BSDD_RESOLUTION]`.
- The spec-grounding leg is CLOSED this pass: the `BsddPort.Fetch` response shape is grounded against the published buildingSMART bSDD API contract (`BsddClassResponse` `Code`/`Name`/`ClassType`/`Uri`/`Definition` + `ClassProperties` with `PropertyCode`/`DataType`/`PropertySet`/`PredefinedValue`/`IsRequired`, the `/api/Class/v1`·`/api/Dictionary/v1` endpoints), projected through `BsddClass.Of`. The in-process degradation to the row's local code-shape policy (`LocalShape`) is the verified settled present-tense fallback.
- REMAINING upstream dependency: the live in-process transport that ISSUES the request and streams the response binds at the cross-folder `csharp:Compute/Runtime/channels#TRANSPORT_AXIS` transport alignment; the `BsddPort` transport rides the injected Compute transport at the composition edge, never a transport minted here. Close-condition: the Compute transport seam lands the injected `BsddPort` implementation.

[UPSTREAM-BLOCKED] Ground the IDS cross-tool audit against the ifctester companion — `Review/validation.md#IDS_FACETS` `[3]-[RESEARCH]` `[IFCTESTER_COMPANION]`.
- The `IdsSpecification`/`IdsFacet`/`IdsAudit` in-process fold over `BimModel`, the IDS v1.0 XSD parse over `System.Xml.Schema`, and the spec authoring are authored and settled; the in-process fold is the immediate self-audit and the `IdsAudit` rides the `Exchange/wire#WIRE_PROJECTION` `BimWireContext` (closed this pass).
- EXTERNAL dependency: the IfcOpenShell ifctester deterministic cross-tool audit is a `python:geometry/ifc-companion` PyPI surface reached only through Compute's companion rpc. Close-condition: the companion request/response shape grounds at the cross-folder companion alignment (the `Exchange/tessellation#TESSELLATION_BRIDGE` two-hop pattern) once the py geometry ifc-companion lands; the companion is the external-conformance oracle, never a transport minted here.

[UPSTREAM-BLOCKED] Promote the candidate `step-iso10303`/`mesh-text`/`iges-ansi`/native-companion codec bodies — `Exchange/import.md#IMPORT_RAIL`·`Exchange/tessellation.md#TESSELLATION_BRIDGE` `[AP242_CODEC]`/`[NATIVE_FORMAT_BRIDGES]`.
- The `InterchangeFormat` rows, the `InterchangeCodec` discriminants, the `StepProtocol` column, the frame columns, and the `CataloguePending` markers are settled; the import fold faults `import-catalogue-pending` on a candidate row and `import-needs-companion` on a companion-gated row (IFC/STEP/IGES/native), which is the correct present-tense behavior. IGES carries the distinct `iges-ansi` companion codec, not `step-iso10303`, because the ANSI IGES section-based grammar shares neither the STEP physical-file token grammar nor the GeometryGym entity surface.
- EXTERNAL dependency: the STL/3MF/OBJ/PLY mesh-text reader packages and the managed AP242 STEP reader are unadmitted (`<stl-3mf-obj-ply-reader-pending>`, `<step-ap242-reader-pending>`), and the IGES, Revit/Navisworks/DWG native readers have no managed loader. Close-condition: each candidate codec promotes in place — capability columns flip and the codec body grounds — when the named package catalogue lands (per-package manifest admission); IGES, native-format, and STEP-solid geometry evaluation route through the Compute companion rail (OpenCascade serves the IGES/STEP solid read in the same two-hop the IFC tessellation uses) because the managed branch carries no kernel.

## [02]-[CLOSED]

(none)
