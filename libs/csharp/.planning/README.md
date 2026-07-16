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

Cross-folder substrate every C# package composes; each folder README names the rows it consumes and its own domain additions, and `libs/csharp/.api/` holds the shared API evidence.

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

[WIRE_CODEGEN]:
- `Generator.Equals` — source-generated structural equality and member-level diff; runtime `Generator.Equals.Runtime`.
- `Google.Protobuf` — carries the `rasm.element.v1` graph wire messages.
- `Grpc.AspNetCore` — gRPC server hosting for measured-execution endpoints.
- `Grpc.Net.Client` — outbound gRPC channels with retry and hedging.
- `Grpc.Tools` — message-only codegen (`GrpcServices=None`).
- `Riok.Mapperly` — compile-time graph↔DTO/proto/record mapping; runtime `Riok.Mapperly.Abstractions`.

[TEST_SUBSTRATE]:
- `xunit.v3.*` — assert/common/extensibility.core/mtp-v2.
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Extensions.TimeProvider.Testing` — `FakeTimeProvider` deterministic clock for the proof gauges.
- `Microsoft.Testing.Platform` stack.
- `BenchmarkDotNet` — drives the benchmark session over the hot paths.
- `Verify.XunitV3` + `Verify.DiffPlex` — architecture and generator snapshot laws.
