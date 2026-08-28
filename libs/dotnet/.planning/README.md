# [DOTNET_BRANCH]

`libs/dotnet` is an independently adoptable Rhino 9/GH2-aware AEC solution spanning the geometry kernel, the element contract and its AEC peers, the app platform, and the host boundaries. C# applications originate and operate these capabilities from the branch package graph alone; polyglot applications exchange contract-conforming artifacts at app roots, aligned by wire bytes and the frozen corpus, never by import.

This branch registry admits a package only where two or more folders share it; a folder README owns its own additions, `libs/dotnet/.api/` holds the shared API catalogues, and the one C# manifest pins versions.

## [01]-[ROUTER]

Package roots in strata order; each root README routes its own design pages, and the host-boundary shells reference only `Rasm`.

- [01]-[KERNEL]: [Rasm](../Rasm/README.md) — RhinoCommon-aware geometry/numeric kernel.
- [02]-[AEC_DOMAIN]: [Rasm.Element](../Rasm.Element/README.md) — canonical property-graph element model; the lowest AEC contract.
- [03]-[AEC_DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — architectural substance, appearance, and buildable component type.
- [04]-[AEC_DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — openBIM object model and IFC/glTF/STEP exchange.
- [05]-[AEC_DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — host-neutral design-to-fabrication making-domain owner.
- [06]-[APP_PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — host-neutral runtime spine every app root folds.
- [07]-[APP_PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured-execution engine and discipline-assessment authority.
- [08]-[APP_PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — content-addressed durable system of record.
- [09]-[APP_PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product surface.
- [11]-[HOST_BOUNDARY]: [Rasm.Rhino](../Rasm.Rhino/README.md) — RhinoCommon + Eto host boundary.
- [12]-[HOST_BOUNDARY]: [Rasm.Grasshopper](../Rasm.Grasshopper/README.md) — GH2 + Eto host boundary.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate every .NET package composes. Libraries emit telemetry through the in-box `System.Diagnostics` surface, so the OpenTelemetry SDK, exporter, and instrumentation train composes at app roots alone, and those packages carry branch-tier catalogues without registry rows.

CloudEvents catalogues split by what each stacks on: the message-envelope core and every event-format sibling catalogue at the branch tier over the kernel's one codec identity, while a protocol binding catalogues at the tier its transport carrier holds, Kafka and AMQP at the Persistence tier and ASP.NET Core at the branch tier its framework carrier sits on.

Host SDK assemblies are not packages and catalogue branch-tier only where more than one folder composes one surface: `RhinoCommon` and `Eto` serve the kernel and both host boundaries, and `Eto.macOS`, `Microsoft.macOS`, and `Rhino.UI` serve the boundary pair, the kernel taking the portable Eto surface alone under a narrower classification. Each folder tier registers the branch catalogue by path and holds only the subsystem its own boundary reaches.

[CORE_SUBSTRATE]:
- `CommunityToolkit.HighPerformance` — 2D span grids and high-performance buffers.
- `Generator.Equals` — Source-generated structural equality and member-level diff; runtime `Generator.Equals.Runtime`.
- `LanguageExt.Core`
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `QuikGraph` — Pure-managed graph containers and `AlgorithmExtensions` for the graph-algorithm walks.
- `Riok.Mapperly` — Compile-time graph↔DTO/proto/record mapping; runtime `Riok.Mapperly.Abstractions`.
- `System.IO.Hashing` — Content-hash mint behind every content key.
- `System.Numerics.Tensors` — SIMD-lowered tensor folds.
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `UnitsNet` — Typed quantity boundary.
- `Wacton.Unicolour` — Perceptual-color owner and color-space projection substrate; `Wacton.Unicolour.Datasets` stays a Materials addition.

[NUMERIC_SUBSTRATE]:
- `CSparse` — Direct sparse Cholesky, LDL', LU, and QR factorization with pattern-reusing refactorization and rank-1 update.
- `MathNet.Numerics` — Distribution-fit, regression, and Monte-Carlo folds.
- `PeterO.Numbers` — Arbitrary-precision `EFloat`/`EDecimal`/`ERational`/`EInteger` exact-arithmetic carriers.

[GPU_DEVICE]:
- `Silk.NET.WebGPU` — WebGPU binding behind the presented, surfaceless-bake, and GPGPU-dispatch device mints.
- `Silk.NET.WebGPU.Extensions.WGPU` — `wgpu_native` extension surface past the standard WebGPU binding.
- `Silk.NET.WebGPU.Native.WGPU` — P/Invoked `wgpu_native` runtime binaries; a device mint carries it, a dispatch-only lane binds the peer's.

[GEOMETRY_INTERCHANGE]:
- `ACadSharp` — DWG/DXF/SVG wire over one `CadDocument`: Bim mesh read, Fabrication profile read, AppUi drafting write.
- `SharpGLTF.Core` — glTF 2.0 schema I/O and the process-global `ExtensionsFactory` every consumer registers on once.
- `SharpGLTF.Toolkit` — Typed vertex, mesh, scene, and material builders folding into a `ModelRoot`.
- `SharpGLTF.Runtime` — Scene templatization and per-instance animation decode.
- `SharpGLTF.Ext.3DTiles` — `EXT_structural_metadata` and `EXT_mesh_features` overlay on the shared glTF graph.
- `Speckle.Sdk` — `Base` object-graph, detach/chunk serialisation, and DI-resolved send/receive transport surface.
- `Speckle.Objects` — Geometry roster and `DataObject` host-object family over `Base`.
- `Unofficial.laszip.netstandard` — One managed LAS/LAZ codec behind scan-to-BIM decode, chunked storage, and `.lax` windowed reads.

[MESH_PROCESSING]:
- `Alimer.Bindings.MeshOptimizer` — GPU-ready mesh optimization, meshlet, LOD, and `EXT_meshopt_compression` codec substrate.
- `geometry3Sharp` — `DMesh3` OBJ/STL/OFF text-mesh decode and the line-sourced `BiArcFit2` biarc fitter.

[PLANAR_GEOSPATIAL]:
- `Clipper2` — Polygon offset and boolean algebra at the planar production boundary.
- `NetTopologySuite` — Planar geometry algebra on the float production plane.
- `NetTopologySuite.IO.GeoJSON4STJ` — STJ-native RFC 7946 GeoJSON converter factory over the NTS feature model.
- `NetTopologySuite.IO.GeoPackage` — OGC GeoPackage geometry-BLOB codec over NTS `Geometry`.
- `pocketken.H3` — Managed Uber-H3 cell index over the NTS coordinate bridge; one cell vocabulary in process and in PostgreSQL.

[ENERGY_SIMULATION]:
- `NREL.OpenStudio.macOS-arm64` — OSM/IDF store and translator matrix; Bim drives exchange, Compute the simulation lane.
- `PollinationSDK` — Pollination cloud-run transport; Compute dispatches, Persistence lands the durable half.

[DATA_SUBSTRATE]:
- `Apache.Arrow` — Columnar `RecordBatch` format and Arrow IPC wire; Compute constructs, Persistence serialises and egresses.
- `Microsoft.Data.Sqlite` — Embedded SQLite ADO.NET transport; the Persistence store driver and the Compute results reader.

[SERVICE_CONTRACTS]:
- `Microsoft.Extensions.AI` — Provider-neutral chat, embedding, and tool-call contracts; runtime `Microsoft.Extensions.AI.Abstractions`.
- `Microsoft.Extensions.Caching.Hybrid` — Tagged L1/L2 recency substrate behind application and host-session caches.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions` — `ILogger` emission contract and the `[LoggerMessage]` generator.
- `Microsoft.Extensions.Telemetry.Abstractions` — `[LogProperties]` emission grammar, enricher contracts, and the latency ledger.
- `Microsoft.Extensions.Compliance.Abstractions` — `DataClassificationAttribute` grammar every classified column declares.
- `Microsoft.Extensions.Compliance.Redaction` — Classified-data redaction at the logging and persistence boundary.

[WIRE_CODEGEN]:
- `Google.Api.CommonProtos` — `google.rpc.Status` and its error details, `google.type` calendar scalars the fault, element, and host families import.
- `Grpc.AspNetCore.Server` — gRPC server hosting for measured-execution endpoints.
- `Grpc.Core.Api` — Method descriptors, marshallers, and the per-call server context every hosted service binds.
- `Grpc.Net.Client` — Outbound gRPC channels with retry and hedging.
- `Grpc.StatusProto` — `google.rpc.Status` carriage on the status trailer: `ToRpcException` at the producer, `GetRpcStatus` at the client.
- `MessagePack` — Explicit primitive op-log envelope and snapshot codec; generated protobuf fills its raw CRDT slot.
- `MessagePackAnalyzer` — Build-only source generator and `MsgPack###` contract gate under `PrivateAssets=all`.
- `NodaTime.Serialization.Protobuf` — NodaTime instant and duration mapping onto the protobuf well-known types.
- `Celly.Protovalidate` — Runtime evaluator of the `buf.validate` rules the emission embeds; one concurrent `Validator` at each branch admission.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — CloudEvents 1.0 message envelope and typed attribute algebra.
- `CloudNative.CloudEvents.SystemTextJson` — Structured/binary/batch JSON event formatter over STJ.
- `NATS.Net` — NATS protocol: Core pub/sub, JetStream durable streams, KV, Object Store, and the capture-ingest boundary.

[MACHINE_CONNECTIVITY]:
- `MTConnect.NET-Common` — MTConnect observation/streams/agent model and the ISO-13399 cutting-tool asset graph.
- `MQTTnet` — MQTT v5 broker-client transport in both directions behind the live-wire row, the egress sink, and the sensor-ingest pump.

[RUNTIME_INBOX]:
- `System.Diagnostics.Metrics` — In-box `Meter`/`Instrument` emission surface behind every minted meter.
- `System.Threading.Channels` — Bounded producer-consumer transport behind host callbacks and work lanes.
- `System.Net.Http` — Outbound handler chain, message pair, and connection pool behind every dial-out hop and exporter egress.
- `System.Security.Cryptography` — DER, X.509, and ECDSA custody with zeroization at credential and attestation boundaries.
- `System.Runtime.InteropServices` — POSIX signal registration behind drain traps and reload.
- `System.Xml` — LINQ-to-XML, streaming reader/writer, schema, and XSLT behind MaterialX, BCF, and SVG payloads.
- `System.Text.Json` — Contract-frozen JSON wire: source-generated contracts, converter dispatch, node models, and the schema exporter.

[DEPENDENCY_FLOORS]:
- `System.Configuration.ConfigurationManager` — Transitive XML-configuration floor under log4net and PerformanceCounter, never a direct reference.
- `System.Drawing.Common` — Compile-only GDI+ surface at the Rhino/Eto host boundary.

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Extensions.TimeProvider.Testing` — `FakeTimeProvider` deterministic clock for the proof gauges.
- `NodaTime.Testing` — `FakeClock` semantic-instant double and the scripted zone sources a DST proof needs.
- `Microsoft.Extensions.Diagnostics.Testing` — `MetricCollector<T>` measurement-assertion harness over any instrument.
- `Polly.Testing` — `ResiliencePipelineDescriptor` composition inspection over a built resilience pipeline.
- `Microsoft.Testing.Platform`
- `Microsoft.AspNetCore.TestHost` — In-memory ASP.NET Core server behind the transport proof harnesses.
- `Verify.XunitV3` + `Verify.DiffPlex` — architecture and generator snapshot laws.
