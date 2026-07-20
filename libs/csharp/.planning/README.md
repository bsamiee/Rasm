# [CSHARP_BRANCH]

`libs/csharp` is the platform's producer branch — the Rhino 9/GH2-aware AEC platform spanning the geometry kernel, the AEC domain, the app platform, and the host boundaries, minting the wire vocabulary and capability descriptors every peer runtime consumes. It routes the C# packages in strata order — the `Rasm` kernel at the base up to the host-boundary shells — and owns the substrate registry every folder composes.

A package earns a registry row only where two or more folders share it; a folder README owns its own additions, `libs/csharp/.api/` holds the shared API catalogues, and the one C# manifest pins versions.

## [01]-[ROUTER]

Package roots in strata order; each root README routes its own design pages.

- [01]-[KERNEL]: [Rasm](../Rasm/README.md) — RhinoCommon-aware geometry/numeric kernel.
- [02]-[AEC-DOMAIN]: [Rasm.Element](../Rasm.Element/README.md) — lowest-AEC element seam.
- [03]-[AEC-DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — profiles, appearance, construction.
- [04]-[AEC-DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — BIM object model and IFC/glTF/STEP exchange.
- [05]-[AEC-DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — host-neutral design-to-fabrication making-domain owner.
- [06]-[APP-PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — runtime spine.
- [07]-[APP-PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured execution.
- [08]-[APP-PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — durable stores.
- [09]-[APP-PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product UI.
- [10]-[HOST-BOUNDARY]: [Rasm.Rhino](../Rasm.Rhino/README.md) — RhinoCommon + Eto host boundary; references only `Rasm`.
- [11]-[HOST-BOUNDARY]: [Rasm.Grasshopper](../Rasm.Grasshopper/README.md) — GH2 + Eto host boundary; references only `Rasm`.

Branch-level pages beside this router: [component-system.md](component-system.md) — the Element/Materials/Bim triad: thing model, projection seams, and the recipes for adding a family, section shape, IFC category, or property under one paradigm.

## [02]-[SUBSTRATE_PACKAGES]

Cross-folder substrate every C# package composes; each folder README names the rows it consumes and its own domain additions, and `libs/csharp/.api/` holds the shared API evidence. Libraries emit telemetry through the in-box `System.Diagnostics` surface, so the OpenTelemetry SDK, exporter, and instrumentation train composes at app roots alone — those packages carry branch-tier catalogs without registry rows. Host SDK assemblies are not packages: `RhinoCommon` evidence homes at the branch tier catalogue-only because the kernel and both host-boundary folders compose one surface.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core`
- `Thinktecture.Runtime.Extensions`
- `Thinktecture.Runtime.Extensions.Json`
- `Thinktecture.Runtime.Extensions.MessagePack`
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
- `System.Numerics.Tensors` — SIMD-lowered tensor folds.
- `UnitsNet` — typed quantity boundary.

[GRAPH_ALGORITHM]:
- `QuikGraph` — pure-managed graph containers and `AlgorithmExtensions` for the graph-algorithm walks.

[PLANAR_GEOMETRY]:
- `Clipper2` — polygon offset and boolean algebra at the planar production boundary.
- `NetTopologySuite` — planar geometry algebra on the float production plane.

[COLOR_SCIENCE]:
- `Wacton.Unicolour` — perceptual-color owner and color-space projection substrate; `Wacton.Unicolour.Datasets` stays a Materials addition.

[RECENCY_CACHE]:
- `Microsoft.Extensions.Caching.Hybrid` — tagged L1/L2 recency substrate behind application and host-session caches.

[DATA_CLASSIFICATION]:
- `Microsoft.Extensions.Compliance.Redaction` — classified-data redaction at the logging and persistence boundary.

[OBSERVABILITY]:
- `Microsoft.Extensions.Logging.Abstractions` — `ILogger` emission contract and the `[LoggerMessage]` generator every instrumented library references.
- `Microsoft.Extensions.Telemetry` — log sampling, buffering, enrichment rows, and the latency-context ledger.
- `Microsoft.Extensions.Telemetry.Abstractions` — `[LogProperties]`/`[TagProvider]` emission grammar and enricher contracts instrumented libraries reference.
- `Microsoft.Extensions.Diagnostics.ResourceMonitoring` — process and container utilization source behind the health fold.

[AI_CONTRACTS]:
- `Microsoft.Extensions.AI` — provider-neutral chat, embedding, and tool-call contracts; runtime `Microsoft.Extensions.AI.Abstractions`.

[WIRE_CODEGEN]:
- `Generator.Equals` — source-generated structural equality and member-level diff; runtime `Generator.Equals.Runtime`.
- `Google.Protobuf` — carries the `rasm.element.v1` graph wire messages.
- `Grpc.AspNetCore` — gRPC server hosting for measured-execution endpoints.
- `Grpc.Net.Client` — outbound gRPC channels with retry and hedging.
- `Grpc.Tools` — message-only codegen (`GrpcServices=None`).
- `Microsoft.AspNetCore.JsonPatch.SystemTextJson` — RFC 6902 document mutation over the STJ wire.
- `Riok.Mapperly` — compile-time graph↔DTO/proto/record mapping; runtime `Riok.Mapperly.Abstractions`.

[RUNTIME_INBOX]:
- `System.Diagnostics.Metrics` — in-box `Meter`/`Instrument` emission surface behind every minted meter.
- `System.Threading.Channels` — bounded producer-consumer transport behind host callbacks and work lanes.
- `System.Security.Cryptography` — PEM, X.509, and ECDSA custody with zeroization at credential and receipt boundaries.
- `System.Runtime.InteropServices` — POSIX signal registration behind drain traps and reload.
- `System.Xml` — LINQ-to-XML, streaming reader/writer, schema, and XSLT behind MaterialX, BCF, and SVG payloads.
- `System.Text.Json.Schema` — schema exporter behind contract self-description.

[DEPENDENCY_FLOORS]:
- `System.Configuration.ConfigurationManager` — transitive XML-configuration floor under log4net and PerformanceCounter, never a direct reference.
- `System.Drawing.Common` — compile-only GDI+ surface at the Rhino/Eto host seam.

[TEST_SUBSTRATE]:
- `xunit.v3.*` — assert/common/extensibility.core/mtp-v2.
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Extensions.TimeProvider.Testing` — `FakeTimeProvider` deterministic clock for the proof gauges.
- `Microsoft.Extensions.Diagnostics.Testing` — `MetricCollector<T>` measurement-assertion rail over any instrument.
- `Microsoft.Testing.Platform` stack.
- `BenchmarkDotNet` — drives the benchmark session over the hot paths.
- `Verify.XunitV3` + `Verify.DiffPlex` — architecture and generator snapshot laws.
