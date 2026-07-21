# [PYTHON_BRANCH_ARCHITECTURE]

`libs/python` is the host-free science, compute, data, geometry, and IFC companion. `runtime` mints the shared value shapes; `compute`, `data`, `geometry`, and `artifacts` compose them at their boundary.

## [01]-[DOMAIN_MAP]

```text codemap
libs/python/
├── runtime/    # Host-free execution foundation four siblings compose
├── compute/    # Offline scientific evidence that graduates through one rail
├── data/       # Portable data interchange: tabular, spatial, gridded, graph
├── geometry/   # Host-free geometry + IFC/BIM companion and cross-boundary owner
└── artifacts/  # Self-contained artifact-production utility under one ArtifactReceipt
```

## [02]-[STRATA]

- S0 `runtime` — imports no sibling and mints every shared rail exactly once; machinery a second sibling composes homes here, at the lowest stratum every consumer references, so a sibling extends a runtime owner by one row and mints no parallel.
- S1 `data` — composes runtime alone and publishes the surfaces its upper strata import: the tabular contract (`FrameAdmission`/`FrameInterop`) and the columnar `arrow_bytes` projection; the mesh and point-record shapes (`MeshPayload`, `PointRecordTable`) cross to geometry as seam payloads, never imports.
- S2 `compute` + `geometry` — peers composing runtime and data; no import crosses between them, so geometry evidence enters `compute` as `GeometryHandoff` wire data.
- S3 `artifacts` — composes runtime and compute's graduation handoff; geometry scene facts cross one-way as GLB bytes admitted through `SceneGrid.of_glb`, and no package imports another's interior — cross-package coupling is a published boundary import or a content-keyed wire.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart TB
    accTitle: Python branch import strata
    accDescr: Four import strata — artifacts over the compute-geometry peer pair over data over the runtime foundation — every package importing runtime directly with each labeled edge naming its one sourced surface, geometry evidence crossing into compute as dashed wire data rather than import, one forbidden upward edge named as such, and runtime's interior rails descending from wire/serve through lanes/workers, joined by roots, onto resilience, receipts, identity, and faults.
    subgraph S3["S3 ARTIFACTS"]
        Artifacts[artifacts]
    end
    subgraph S2["S2 COMPUTE + GEOMETRY"]
        Compute[compute]
        Geometry[geometry]
    end
    subgraph S1["S1 DATA"]
        Data[data]
    end
    subgraph S0["S0 RUNTIME"]
        Wire["wire / serve"]
        Lanes["lanes / workers"]
        Roots[roots]
        Resilience[resilience]
        Receipts[receipts]
        Identity[identity]
        Faults[faults]
    end
    Artifacts e1@-->|"[IMPORT]: GraduationReceipt"| Compute
    Compute e2@-->|"[IMPORT]: FrameAdmission"| Data
    Geometry e3@-->|"[IMPORT]: arrow_bytes"| Data
    Geometry e4@-.->|"[WIRE]: GeometryHandoff"| Compute
    Data e5@-->|"[IMPORT]: ResourceRef"| S0
    Geometry e6@-->|"[IMPORT]: ContentKey"| S0
    Compute e7@-->|"[IMPORT]: Kernel"| S0
    Artifacts e8@-->|"[IMPORT]: ContentKey"| S0
    Artifacts ~~~ Compute
    S0 e9@-->|"forbidden: upward import"| S3
    Wire r1@--> Lanes
    Lanes r2@--> Resilience
    Roots r3@--> Resilience
    Resilience r4@--> Receipts
    Receipts r5@--> Identity
    Identity r6@--> Faults
```

## [03]-[SEAMS]

Python couples to C# only at the wire — content-keyed shapes cross serialized, never as imported code. Each edge freezes one {KIND, name, direction} representative at the owner's spelling; companion legs fold to prose — runtime↔Rasm.AppHost also carries `TraceContext` and `HlcStamp`, runtime↔Rasm.Compute an `XxHash128` leg, `ContentAddress` spells from the Element owner over the runtime `ContentKey` minting, and the graduation seam's reverse payload is `EvidenceBundle`, C#-owned as `GraduationEvidence`. File-level detail lives on the owning folder's design page and the cross-`libs/` ledger.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Python branch C# seam registry
    accDescr: Python packages exchanging kinded contract shapes with their C# counterparts — every edge labeled kind plus shape — persistence as the one data store, bidirectional peers as hexagons, one-way sources and sinks as stadiums.
    subgraph python[LIBS/PYTHON]
        Geometry[geometry]
        Runtime[runtime]
        Compute[compute]
        Data[data]
        Artifacts[artifacts]
    end
    Bim{{Rasm.Bim}}
    Element{{Rasm.Element}}
    Rasm{{Rasm}}
    AppHost{{Rasm.AppHost}}
    RasmCompute{{Rasm.Compute}}
    Persistence[(Rasm.Persistence)]
    Materials([Rasm.Materials])
    Fabrication([Rasm.Fabrication])
    Geometry e12@<-->|"[WIRE]: IfcWire"| Bim
    Geometry e13@<-->|"[WIRE]: GlbContentHash"| Element
    Geometry e11@<-->|"[WIRE]: ComputeService"| RasmCompute
    Runtime e5@<-->|"[CONTENT_KEY]: ContentAddress"| Element
    Runtime e1@<-->|"[CONTENT_KEY]: XxHash128"| Rasm
    Runtime e2@<-->|"[WIRE]: DiscoveryResult"| AppHost
    Runtime e4@<-->|"[WIRE]: ProtoVocabulary"| RasmCompute
    Runtime e3@<-->|"[WIRE]: OpLogEntry"| Persistence
    Compute e10@<-->|"[GRADUATION]: HandoffAxis"| RasmCompute
    Data e7@-->|"[SHAPE]: DoeDataset"| RasmCompute
    Data e6@<-->|"[WIRE]: SubstraitPlan"| Persistence
    Data e8@-->|"[WIRE]: Environmental"| Materials
    Bim e9@-->|"[WIRE]: GeoFeatureWkb"| Data
    Artifacts e14@-->|"[CONTENT_KEY]: SignedArtifact"| Persistence
    Fabrication e15@-->|"[SHAPE]: Tolerance"| Artifacts
```

Every crossing decodes exactly once, at the owning package endpoint its edge names; a sibling composes the decoded vocabulary through that endpoint. Runtime's transport plane alone holds the branch proto vocabulary and its descriptor drift gate, grading schema drift at boot before the first RPC.

## [04]-[INTERNAL]

Branch evidence crosses outward through one spine: geometry's evidence graduates into `compute` as `GeometryHandoff` wire data, every compute producer folds its receipts onto the graduation hub, `artifacts` projects its graduating evidence onto the same axis, and the hub's `HandoffAxis` is the one egress all branch evidence crosses. Reverse evidence returns as `EvidenceBundle` and decodes at the same hub, so egress and return meet at one owner.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Python branch graduation spine
    accDescr: Geometry, compute producers, and artifacts folding evidence onto the compute graduation hub, the one handoff axis crossing outward, and the reverse evidence envelope returning to the hub as dashed wire.
    Geometry[geometry] e1@-->|"GeometryHandoff"| Hub[compute graduation hub]
    Producers[compute producers] e2@-->|"GraduationReceipt"| Hub
    Artifacts[artifacts] e3@-->|"HandoffAxis"| Hub
    Hub e4@-->|"HandoffAxis"| Egress([outward egress])
    Egress e5@-.->|"EvidenceBundle"| Hub
```

Telemetry converges the same way: runtime's observability owner is the branch's one emission substrate — `Hooks` registers every package's hook points under package-qualified ids, one `INSTRUMENTS` table owns every instrument as a row, and `Telemetry` alone installs OTLP egress at the composition root. A sibling's telemetry is a registration row on that owner — a hook point, an instrument row, a receipt folded through the drain — and its series leave the branch as opaque OTLP transport, never decoded branch vocabulary.

```mermaid
---
config:
  layout: elk
  flowchart:
    curve: linear
    padding: 25
---
flowchart LR
    accTitle: Python branch observability spine
    accDescr: Sibling packages firing hook facts and folding receipts into the runtime observability owners, hook taps and receipt records metering onto the one instrument table, and the telemetry install alone carrying OTLP egress outward to the collector.
    Siblings[compute · data · geometry · artifacts]
    subgraph runtime[RUNTIME OBSERVABILITY]
        Hooks[Hooks registry]
        Drain[receipts drain]
        Instruments[INSTRUMENTS meter]
        Telemetry[Telemetry install]
    end
    Collector([collector])
    Siblings e1@-->|"hook facts"| Hooks
    Siblings e2@-->|"receipts"| Drain
    Hooks e3@-->|"taps"| Instruments
    Drain e4@-->|"record"| Instruments
    Instruments e5@-->|"metered series"| Telemetry
    Telemetry e6@-->|"OTLP"| Collector
```

## [05]-[ROUTING]

| [INDEX] | [CHANGE]                            | [OWNER_SURFACE]                    | [SHAPE_OF_THE_EDIT]                              |
| :-----: | :---------------------------------- | :--------------------------------- | :----------------------------------------------- |
|  [01]   | machinery a second sibling composes | `runtime`                          | one S0 owner row every consumer imports          |
|  [02]   | a graduating evidence axis          | `compute/graduation/handoff.py`    | one `HandoffAxis` case                           |
|  [03]   | a branch metric or signal           | `runtime/observability/metrics.py` | one `INSTRUMENTS` row                            |
|  [04]   | a hook point                        | `runtime/observability/hooks.py`   | one `HookPoint` row under a package-qualified id |
|  [05]   | a C#-minted proto wire family       | `runtime/transport/shapes.py`      | one `PROTO_VOCABULARY` row the drift gate proves |
|  [06]   | a package dependency                | root `pyproject.toml`              | one admission row in the owning group            |

## [06]-[ADMISSION_POLICY]

One root manifest owns interpreter admission, dependency groups, version bounds, and `python_version` markers. This branch targets a normal-GIL CPython core; worker-lane exceptions stay in the root manifest until resolver evidence permits removal. Installation rationale stays in the manifest; package-local docs name capability, entrypoints, boundaries, and exclusions.

`protobuf` and `grpcio` are core runtime dependencies. `grpcio-tools` is codegen-only. Native rendering and OCCT/STEP concerns stay on their owning geometry/artifacts tasks and root-manifest admissions. `specklepy` is not a branch dependency.
