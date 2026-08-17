# [CSHARP_BRANCH]

`libs/csharp` is an independently adoptable Rhino 9/GH2-aware AEC estate spanning the geometry kernel, the element seam and its AEC peers, the app platform, and the host boundaries. C# applications originate and operate these capabilities from the branch package graph alone; polyglot applications exchange contract-conforming artifacts at app roots, aligned by wire bytes and the frozen corpus, never by import.

This branch registry admits a package only where two or more folders share it; a folder README owns its own additions, `libs/csharp/.api/` holds the shared API catalogues, and the one C# manifest pins versions.

## [01]-[ROUTER]

Package roots in strata order; each root README routes its own design pages, and the host-boundary shells reference only `Rasm`.

- [01]-[KERNEL]: [Rasm](../Rasm/README.md) — RhinoCommon-aware geometry/numeric kernel.
- [02]-[AEC-DOMAIN]: [Rasm.Element](../Rasm.Element/README.md) — canonical property-graph element model; the lowest AEC seam.
- [03]-[AEC-DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — architectural substance, appearance, and buildable component type.
- [04]-[AEC-DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — openBIM object model and IFC/glTF/STEP exchange.
- [05]-[AEC-DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — host-neutral design-to-fabrication making-domain owner.
- [06]-[APP-PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — host-neutral runtime spine every app root folds.
- [07]-[APP-PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured-execution engine and discipline-assessment authority.
- [08]-[APP-PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — content-addressed durable system of record.
- [09]-[APP-PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product surface over the receipt spine.
- [10]-[HOST-BOUNDARY]: [Rasm.Rhino](../Rasm.Rhino/README.md) — RhinoCommon + Eto host boundary.
- [11]-[HOST-BOUNDARY]: [Rasm.Grasshopper](../Rasm.Grasshopper/README.md) — GH2 + Eto host boundary.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate every C# package composes. Libraries emit telemetry through the in-box `System.Diagnostics` surface, so the OpenTelemetry SDK, exporter, and instrumentation train composes at app roots alone — those packages carry branch-tier catalogues without registry rows.

CloudEvents siblings split by what a catalogue STACKS on: the message-envelope core and every event FORMAT sibling catalogue at the branch tier, since the kernel owns one codec identity every folder composes, while a protocol BINDING catalogues at the tier its transport carrier already holds — Kafka and AMQP beside `api-kafka.md` and `api-amqpnetlite.md` at the Persistence tier, ASP.NET Core at the branch tier its framework carrier sits on. Promoting a binding catalogue above its carrier reaches DOWN into a folder tier for the surface it stacks on, which is the inversion this split forecloses. Host SDK assemblies are not packages and home at the branch tier catalogue-only where more than one folder composes one surface: `RhinoCommon` because the kernel and both host-boundary folders reach it, `Eto` because the kernel and both host-boundary folders do — the kernel seats the interaction machinery both boundaries compose — and `Eto.macOS`, `Microsoft.macOS`, and `Rhino.UI` because both host-boundary folders do, the kernel taking the portable UI surface alone under a narrower classification — each folder tier then registers the branch catalogue by path and holds only the subsystem its own boundary reaches.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
- `Generator.Equals` — source-generated structural equality and member-level diff; runtime `Generator.Equals.Runtime`.
- `JetBrains.Annotations`

[TIME_IDENTITY]:
- `NodaTime`
- `NodaTime.Serialization.SystemTextJson`
- `NodaTime.Serialization.Protobuf`
- `System.IO.Hashing` — content-hash mint behind every content key.

[NUMERIC_SUBSTRATE]:
- `CommunityToolkit.HighPerformance` — 2D span grids and high-performance buffers.
- `CSparse` — direct sparse Cholesky, LDL', LU, and QR factorization with pattern-reusing refactorization and rank-1 update.
- `MathNet.Numerics` — distribution-fit, regression, and Monte-Carlo folds.
- `PeterO.Numbers` — arbitrary-precision `EFloat`/`EDecimal`/`ERational`/`EInteger` exact-arithmetic carriers.
- `System.Numerics.Tensors` — SIMD-lowered tensor folds.
- `UnitsNet` — typed quantity boundary.

[GPU_DEVICE]:
- `Silk.NET.WebGPU` — WebGPU binding behind the presented, surfaceless-bake, and GPGPU-dispatch device mints.
- `Silk.NET.WebGPU.Extensions.WGPU` — `wgpu_native` extension surface past the standard WebGPU binding.
- `Silk.NET.WebGPU.Native.WGPU` — P/Invoked `wgpu_native` runtime binaries; a device mint carries it, a dispatch-only lane binds the peer's.

[GEOMETRY_INTERCHANGE]:
- `ACadSharp` — DWG/DXF/SVG wire over one `CadDocument`: Bim mesh read, Fabrication profile read, AppUi drafting write.
- `SharpGLTF.Core` — glTF 2.0 schema I/O and the process-global `ExtensionsFactory` every consumer registers on once.
- `SharpGLTF.Toolkit` — typed vertex, mesh, scene, and material builders folding into a `ModelRoot`.
- `SharpGLTF.Runtime` — scene templatization and per-instance animation decode.
- `SharpGLTF.Ext.3DTiles` — `EXT_structural_metadata` and `EXT_mesh_features` overlay on the shared glTF graph.
- `Speckle.Sdk` — `Base` object-graph, detach/chunk serialisation, and DI-resolved send/receive transport surface.
- `Speckle.Objects` — geometry roster and `DataObject` host-object family over `Base`.
- `Unofficial.laszip.netstandard` — one managed LAS/LAZ codec behind scan-to-BIM decode, chunked residence, and `.lax` windowed reads.

[MESH_PROCESSING]:
- `Alimer.Bindings.MeshOptimizer` — GPU-ready mesh optimization, meshlet, LOD, and `EXT_meshopt_compression` codec substrate.
- `geometry3Sharp` — `DMesh3` OBJ/STL/OFF text-mesh decode and the line-sourced `BiArcFit2` biarc fitter.

[GRAPH_ALGORITHM]:
- `QuikGraph` — pure-managed graph containers and `AlgorithmExtensions` for the graph-algorithm walks.

[PLANAR_GEOMETRY]:
- `Clipper2` — polygon offset and boolean algebra at the planar production boundary.
- `NetTopologySuite` — planar geometry algebra on the float production plane.
- `NetTopologySuite.IO.GeoJSON4STJ` — STJ-native RFC 7946 GeoJSON converter factory over the NTS feature model.
- `NetTopologySuite.IO.GeoPackage` — OGC GeoPackage geometry-BLOB codec over NTS `Geometry`.

[GEOSPATIAL_INDEX]:
- `pocketken.H3` — managed Uber-H3 cell index over the NTS coordinate bridge; one cell vocabulary in process and in PostgreSQL.

[ENERGY_SIMULATION]:
- `NREL.OpenStudio.macOS-arm64` — OSM/IDF store and translator matrix; Bim drives exchange, Compute the simulation lane.
- `PollinationSDK` — Pollination cloud-run transport; Compute dispatches, Persistence lands the durable half.

[DATA_SUBSTRATE]:
- `Apache.Arrow` — columnar `RecordBatch` format and Arrow IPC wire; Compute constructs, Persistence serialises and egresses.
- `Microsoft.Data.Sqlite` — embedded SQLite ADO.NET transport; the Persistence store rail and the Compute results reader.

[COLOR_SCIENCE]:
- `Wacton.Unicolour` — perceptual-color owner and color-space projection substrate; `Wacton.Unicolour.Datasets` stays a Materials addition.

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — tagged L1/L2 recency substrate behind application and host-session caches.

[DATA_CLASSIFICATION]:
- `Microsoft.Extensions.Compliance.Redaction` — classified-data redaction at the logging and persistence boundary.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions` — `ILogger` emission contract and the `[LoggerMessage]` generator.
- `Microsoft.Extensions.Telemetry.Abstractions` — `[LogProperties]` emission grammar, enricher contracts, and the latency ledger.

[AI_CONTRACTS]:
- `Microsoft.Extensions.AI` — provider-neutral chat, embedding, and tool-call contracts; runtime `Microsoft.Extensions.AI.Abstractions`.

[WIRE_CODEGEN]:
- `Google.Protobuf` — carries the `rasm.element.v1` graph wire messages.
- `Grpc.AspNetCore` — gRPC server hosting for measured-execution endpoints.
- `Grpc.Core.Api` — method descriptors, marshallers, and the per-call server context every hosted service binds.
- `Grpc.Net.Client` — outbound gRPC channels with retry and hedging.
- `Grpc.Tools` — build-only `<Protobuf>` codegen under `PrivateAssets=all`; each consumer sets its `GrpcServices` mode, server codegen at app roots.
- `MessagePack` — schemaless binary wire with in-codec LZ4 framing; per-folder resolver profiles over one codec.
- `MessagePackAnalyzer` — build-only source generator and `MsgPack###` contract gate under `PrivateAssets=all`.
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson` — RFC 6902 document mutation over the STJ wire.
- `Riok.Mapperly` — compile-time graph↔DTO/proto/record mapping; runtime `Riok.Mapperly.Abstractions`.

[EVENT_TRANSPORT]:
- `CloudNative.CloudEvents` — CloudEvents 1.0 message envelope and typed attribute algebra.
- `CloudNative.CloudEvents.SystemTextJson` — structured/binary/batch JSON event formatter over STJ.
- `NATS.Net` — NATS protocol: Core pub/sub, JetStream durable streams, KV, Object Store, and the capture-ingest seam.

[MACHINE_CONNECTIVITY]:
- `MTConnect.NET-Common` — MTConnect observation/streams/agent model and the ISO-13399 cutting-tool asset graph.
- `MQTTnet` — MQTT v5 broker-client transport in both directions behind the live-wire row, the egress sink, and the sensor-ingest pump.

[RUNTIME_INBOX]:
- `System.Diagnostics.Metrics` — in-box `Meter`/`Instrument` emission surface behind every minted meter.
- `System.Threading.Channels` — bounded producer-consumer transport behind host callbacks and work lanes.
- `System.Net.Http` — outbound handler chain, message pair, and connection pool behind every dial-out hop and exporter egress.
- `System.Security.Cryptography` — PEM, X.509, and ECDSA custody with zeroization at credential and receipt boundaries.
- `System.Runtime.InteropServices` — POSIX signal registration behind drain traps and reload.
- `System.Xml` — LINQ-to-XML, streaming reader/writer, schema, and XSLT behind MaterialX, BCF, and SVG payloads.
- `System.Text.Json` — contract-frozen JSON wire: source-generated contracts, converter dispatch, node models, and the schema exporter.

[DEPENDENCY_FLOORS]:
- `System.Configuration.ConfigurationManager` — transitive XML-configuration floor under log4net and PerformanceCounter, never a direct reference.
- `System.Drawing.Common` — compile-only GDI+ surface at the Rhino/Eto host seam.

[TEST_SUBSTRATE]:
- `xunit.v3.*`
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Extensions.TimeProvider.Testing` — `FakeTimeProvider` deterministic clock for the proof gauges.
- `Microsoft.Extensions.Diagnostics.Testing` — `MetricCollector<T>` measurement-assertion rail over any instrument.
- `Polly.Testing` — `ResiliencePipelineDescriptor` composition inspection over a built resilience pipeline.
- `Microsoft.Testing.Platform`
- `BenchmarkDotNet`
- `Verify.XunitV3` + `Verify.DiffPlex` — architecture and generator snapshot laws.
