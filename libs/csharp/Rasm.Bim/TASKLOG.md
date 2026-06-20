# [RASM_BIM_TASKLOG]

Open and closed work distilled from `IDEAS.md`. `[01]-[OPEN]` carries task cards with `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` leaders; `[02]-[CLOSED]` carries `[COMPLETE]` or `[DROPPED]` cards. One idea spawns one or more tasks; each task names the exact sub-domain or file it lands in.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
-->

[T-BSDD-TRANSPORT]-[BLOCKED]: ground the `BsddResolution` live transport leg against the Compute transport seam.
- Capability: live bSDD class/property resolution for the `Classification` axis, with service misses degrading to the row's local code-shape policy.
- Shape: `BsddResolution.Resolve` builds the dictionary-class URI, calls the injected `BsddPort.Fetch`, projects `BsddClassResponse` through `BsddClass.Of`, and falls back to `LocalShape` only on transport miss.
- Unlocks: classification, property templates, and IDS classification facets share one dictionary evidence path without blocking ingest or minting a Bim-owned transport.
- Anchors: `Semantics/classification.md#BSDD_RESOLUTION`, `Semantics/properties.md#PROPERTY_SETS`, `Review/validation.md#IDS_FACETS`, and `csharp:Compute/Runtime/channels#TRANSPORT_AXIS`.
- Tension: blocked until Compute lands the injected `BsddPort` implementation at the composition edge.

[T-IDS-IFCTESTER-COMPANION]-[BLOCKED]: ground the IDS cross-tool audit against the ifctester companion.
- Capability: deterministic IDS conformance evidence pairs the in-process `IdsAudit` fold with the IfcOpenShell ifctester oracle.
- Shape: `IdsSpecification`/`IdsFacet`/`IdsAudit` fold over `BimModel`, parse and author IDS v1.0 XML through `System.Xml.Schema`, and serialize audit receipts through `BimWireContext`.
- Unlocks: requirement-driven model acceptance can compare C# self-audit output with the Python companion's standards-conformant ifctester result on GlobalId and facet.
- Anchors: `Review/validation.md#IDS_FACETS`, `Exchange/wire.md#WIRE_PROJECTION`, `Exchange/tessellation.md#TESSELLATION_BRIDGE`, and `python:geometry/ifc-companion`.
- Tension: blocked until the companion request/response shape lands on the existing Compute companion RPC path.

[T-IMPORT-CODECS]-[BLOCKED]: promote candidate import codec bodies in place.
- Capability: candidate import rows promote into real codec bodies without adding importer families or duplicating the format axis.
- Shape: `InterchangeFormat` rows, `InterchangeCodec` discriminants, `StepProtocol`, frame-normalization columns, and `CataloguePending` markers stay settled while `BimIo` faults `import-catalogue-pending` or `import-needs-companion` at the boundary.
- Unlocks: STL/3MF/OBJ/PLY, AP242 STEP, IGES, Revit, Navisworks, and DWG intake can become row promotions on the existing import fold.
- Anchors: `Exchange/format.md#FORMAT_AXIS`, `Exchange/import.md#IMPORT_RAIL`, `Exchange/tessellation.md#TESSELLATION_BRIDGE`, `python:geometry/mesh/daemon#DAEMON`, and `python:geometry/mesh/cad#BRIDGE`.
- Tension: blocked on mesh-text reader catalogues, a managed AP242 STEP reader catalogue, and companion-backed IGES/native readers; IGES remains the distinct `iges-ansi` codec because ANSI IGES is not ISO 10303 STEP.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
