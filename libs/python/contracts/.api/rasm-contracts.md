# [PY_CONTRACTS_API_RASM_CONTRACTS]

`rasm.contracts` owns the Python binding of the `rasm.contracts.<family>.v1` corpus: `protoc-gen-py` emits one `<family>_pb` module of `protobuf.Message` classes per source, `protoc-gen-connectrpc` one `<family>_connect` module per service-bearing source, and this catalog indexes the generator symbol grammar and the family roster those modules declare — no hand-authored member exists under it.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `rasm.contracts`
- root: `libs/python/contracts/src`, a root-declared source root — `[tool.pytest] pythonpath`, `[tool.ty.environment] root`, `[tool.mypy] mypy_path`/`files` carry it; no dist, no manifest under the folder
- module: `rasm.contracts`
- namespaces: `rasm.contracts.channels.v1`, `rasm.contracts.compute.v1`, `rasm.contracts.element.v1`, `rasm.contracts.organization.v1`, `rasm.contracts.scene.v1`, `rasm.contracts.vendor.io.cloudevents.v1`, `rasm.contracts.vendor.grpc.health.v1`
- abi: pure-Python emission of `protoc-gen-py` and `protoc-gen-connectrpc` driven by `assay contracts generate`; `rasm/` and `rasm/contracts/` are PEP 420 namespace directories with no `__init__.py`
- depends: `protobuf-py` supplies `Message`, `Oneof`, `wkt`, and `_codegen.file_desc`; `connectrpc` supplies `ConnectASGIApplication`, `Endpoint`, `MethodInfo`, `ConnectClient`
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: generator symbol grammar — how one proto declaration spells in Python; `<f>` the family token, `<Svc>` the service name, `<rpc>` the snake_case method

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [CAPABILITY]                                                                |
| :-----: | :---------------------------------------- | :------------ | :-------------------------------------------------------------------------- |
|  [01]   | `rasm.contracts.<f>.v1.<f>_pb.<Message>`  | class         | `Message[Literal[<fields>]]` subclass, one per proto message, fields inline |
|  [02]   | `<f>_pb.<Outer>.<Inner>`                  | class         | a nested message spells under its enclosing class                           |
|  [03]   | `rasm.contracts.<f>.v1.<f>_pb.desc()`     | static        | the module's `DescFile` booted through `protobuf._codegen.file_desc`        |
|  [04]   | `rasm.contracts.<f>.v1.<f>_connect.<Svc>` | protocol      | async handler protocol, one `<rpc>` per rpc, defaults raise `UNIMPLEMENTED` |
|  [05]   | `<f>_connect.<Svc>ASGIApplication`        | class         | `ConnectASGIApplication[<Svc>]` seating every endpoint under `path`         |
|  [06]   | `<f>_connect.<Svc>Client`                 | class         | `ConnectClient` carrying one typed `<rpc>(request, *, headers, timeout_ms)` |
|  [07]   | `rasm.contracts.vendor.<path>.<m>_pb.<M>` | class         | a vendored publisher module seated under `rasm.contracts.vendor`            |

- `Oneof(field, value)` constructs a oneof on its NAME with `from protobuf import Oneof`; well-known types import from `protobuf.wkt` (`Empty`, `Duration`, `Timestamp`, `Any`, `Struct`, `FieldMask`).
- `io=async` emits the async trio alone; no `<Svc>Sync`, `<Svc>WSGIApplication`, or `<Svc>ClientSync` exists in this tree.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: family roster — every message class each `_pb` module declares, in declaration order, derived from the emission's `desc().messages` and censused by the corpus gate against the built descriptor set

[channels_pb]: `WireProvenance` `PlaneRefWire` `PlaneRef` `ChannelWire` `PackWire` `PressReceiptWire` `TextureSetWire` `MapEntry` `PackEntry` `IblEntry` `AssetSetManifest`
[compute_pb]: `FaultRecovery` `FaultDetail` `SymbolicDim` `PointCloudTensor` `MeshTensor` `VoxelTensor` `GeometryPayload` `GaussianSplatScan` `TransactionRequest` `TransactionReceipt` `QueryRequest` `QueryResponse` `InferRequest` `InferResponse` `SolveRequest` `SolveResponse` `GenerateRequest` `TokenChunk` `GraphDiffRequest` `GraphDiffResponse` `SubtreeFetchRequest` `GraphChunk` `TessellationRequest` `TessellationReceipt` `DispatchToolRequest` `DispatchReceipt` `CommandReply` `DispatchPatchRequest` `ReloadReply` `SetDegradationRequest` `DegradationReply` `DrainRuntimeRequest` `DrainStepRow` `DrainReply` `SupportBundleRequest` `SupportBundleReply` `ArtifactFrame`
[element_pb]: `ElementGraphWire` `RedactionManifestWire` `GraphDeltaWire` `NodeRevisionWire` `HeaderWire` `UnitAxisWire` `NodeWire` `ObjectWire` `PlacementWire` `ClassificationWire` `StepHeaderWire` `OwnerHistoryWire` `SchemaSpanWire` `AppearanceWire` `PropertyValueWire` `LogicalWire` `EnumeratedWire` `TemporalWire` `ReferenceWire` `BoundedWire` `ListWire` `TableWire` `TableRowWire` `ComplexWire` `MeasureValueWire` `MeasureBandWire` `PropertySetWire` `QuantitySetWire` `GroupIdentityWire` `RelationshipWire` `ComposeWire` `AssignWire` `AssociateWire` `ConnectWire` `VoidWire` `GenericWire` `RelationshipParticipantWire` `MaterialUsageWire` `LayerSetUsageWire` `ProfileSetUsageWire` `MaterialWire` `MaterialCompositionWire` `SingleWire` `LayerSetWire` `MaterialLayerWire` `ProfileSetWire` `MaterialProfileWire` `ProfileRefWire` `ConstituentSetWire` `MaterialConstituentWire` `SectionPropertiesWire` `MaterialPropertySetWire` `PropertyEvidenceWire` `AttestationWire` `MechanicalWire` `OrthotropicWire` `ThermalWire` `AcousticWire` `FireWire` `FireResistanceWire` `EnvironmentalWire` `CostWire` `DampingWire` `RayleighWire` `HygrothermalWire` `SampledCurveWire` `DurabilityWire` `OpticalWire` `ElectricalWire` `AssessmentWire` `DiagnosticWire` `ProvenanceWire` `ObservationWire` `ObservationChunkWire` `SensorProvenanceWire` `SeriesStatisticsWire` `CoverageWire` `CellLatticeWire` `CoverageBandWire` `ColorBinWire` `OverviewLevelWire` `GeoReferenceWire` `ProjectedCrsWire`
[organization_pb]: `EntityWire` `ContainmentWire` `ViewOverrideWire` `OrganizationWire`
[scene_pb]: `SceneVector` `SceneSpectrum` `SolarFrame` `SolarAngles` `SitedSun` `AuthoredSun` `SceneSun` `PhotometricWebRef` `PhotometricPower` `SpotCone` `AreaExtent` `ScenePhotometry` `TessellationFidelity` `ShadingArtifact` `SceneDescriptor`
[cloudevents_pb]: `CloudEvent` `CloudEvent.CloudEventAttributeValue` `CloudEventBatch`
[health_pb]: `HealthCheckRequest` `HealthCheckResponse`

[ENTRYPOINT_SCOPE]: service roster — every `<Svc>` protocol `compute_connect` and `health_connect` declare with rpc methods and kinds; each carries the `<Svc>ASGIApplication` and `<Svc>Client` twins

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                                                        |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------------------------------------------- |
|  [01]   | `ComputeService.<rpc>(request, ctx)`    | instance | `infer` `solve` `graph_diff` `tessellate`; `generate` `subtree_fetch` stream        |
|  [02]   | `DocumentService.<rpc>(request, ctx)`   | instance | `execute_transaction` `query` unary                                                 |
|  [03]   | `ControlService.<rpc>(request, ctx)`    | instance | `reload_options` `dispatch_tool` `dispatch_patch` `set_degradation` `drain_runtime` |
|  [04]   | `DiagnosticService.<rpc>(request, ctx)` | instance | `capture_bundle` unary                                                              |
|  [05]   | `ArtifactSyncService.sync(frames, ctx)` | instance | bidi stream: `AsyncIterator[ArtifactFrame]` in and out                              |
|  [06]   | `Health.<rpc>(request, ctx)`            | instance | `check` unary; `watch` server stream — `rasm.contracts.vendor.grpc.health.v1`       |
|  [07]   | `<Svc>ASGIApplication(service, *, ...)` | ctor     | mount one service over `interceptors` `read_max_bytes` `compressions` `codecs`      |
|  [08]   | `<Svc>ASGIApplication.path`             | property | `/<package>.<Svc>`, the dispatcher mount prefix                                     |

- server-stream rpcs return `AsyncIterator` from a plain `def` and unary rpcs await inside `async def`; an implementing servicer subclasses `<Svc>` and overrides the rpcs it serves.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `assay contracts generate` is the one author: a proto edit lands through one regeneration and the whole tree commits together, and `assay contracts check` proves freshness by scratch regeneration and byte diff.
- bindings record their own field types, optionality, and collection cardinality as generated (`docs/laws/topology.md` `[FENCE_SEAM]`); a consuming fence binds the generated declaration, never the proto source's spelling.
- every module imports `protobuf` and `connectrpc` alone — both root `pyproject.toml` rows — so the source root references no sibling and ranks in no stratum.

[STACKING]:
- `protobuf-py`(`libs/python/.api/protobuf-py.md`): every class is a `Message` subclass carrying `to_binary`/`from_binary`/`to_json`/`from_json`, `desc()` seats a module's `DescFile` into a `Registry`, and a oneof takes `Oneof(field, value)`.
- `connectrpc`(`libs/python/.api/connectrpc.md`): `<Svc>ASGIApplication(service, interceptors=...)` is the value a composition root mounts under `DispatcherMiddleware` and `<Svc>Client(address, http_client=...)` the typed dialer; `RequestContext[REQ, RES]` is the `ctx` every handler takes.
- `runtime/transport/serve`: constructs every served `<Svc>ASGIApplication` over its servicer with the one `Admission` interceptor under one hypercorn host; `runtime/transport/shapes` proves each served `WireService` row against its generated application class at boot.

[LOCAL_ADMISSION]:
- generated classes ARE the proto vocabulary; a msgspec struct restating one is the deleted mirror, and a branch fence imports the class, never a hand copy.
- nothing under `src/` is edited by hand and no file is added beneath it; a needed change lands at the corpus source and regenerates.

[RAIL_LAW]:
- Package: `rasm.contracts` source root under `libs/python/contracts/src`
- Owns: the Python spelling of every corpus message and service — classes, protocols, ASGI applications, and clients — as buf emits them
- Accept: `from rasm.contracts.<f>.v1.<f>_pb import <Message>`, `from rasm.contracts.compute.v1.compute_connect import <Svc>, <Svc>ASGIApplication, <Svc>Client`, `from rasm.contracts.vendor.io.cloudevents.v1.cloudevents_pb import CloudEvent`, `from rasm.contracts.vendor.grpc.health.v1.health_connect import Health, HealthASGIApplication`
- Reject: a hand-written `Message` subclass or msgspec twin of a corpus message, a hand-built `ConnectASGIApplication` where a generated `<Svc>ASGIApplication` exists, a `sys.path` or `.pth` view onto `src/` in a fence, a `pyproject.toml` or editable install under the folder, any file authored under `src/`
