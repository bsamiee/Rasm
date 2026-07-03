# [CSHARP_BRANCH]

The C# branch router and the cross-cutting package registry. The branch aggregates nine planning-scoped packages, each carrying its own four index docs (`README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`) at its root and its design pages under one `.planning/`. This node routes to those package roots and registers only the packages shared across two or more C# folders; a folder README lists only its own additions, shared API catalogues live in `libs/csharp/.api/`, and versions live in the one C# manifest, never here.

## [01]-[ROUTER]

Nine planning-scoped package roots in strata order; each root `README.md` carries the folder's page router and its own package additions, and each `ARCHITECTURE.md` carries the folder's sub-domain map.

- [01]-[KERNEL]: [Rasm](../Rasm/README.md) — RhinoCommon-aware geometry/numeric kernel.
- [02]-[AEC-DOMAIN]: [Rasm.Element](../Rasm.Element/README.md) — lowest-AEC element seam.
- [03]-[AEC-DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — profiles, appearance, construction.
- [04]-[AEC-DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — BIM object model and IFC/glTF/STEP exchange.
- [05]-[AEC-DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — portable HLR/CAM/nesting.
- [06]-[APP-PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — runtime spine.
- [07]-[APP-PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured execution.
- [08]-[APP-PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — durable stores.
- [09]-[APP-PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product UI.

The HOST-BOUNDARY packages `Rasm.Rhino` and `Rasm.Grasshopper` are out-of-scope-durable source with no `.planning/`.

Branch-level pages beside this router: [component-system.md](component-system.md) — the Element/Materials/Bim triad: the thing model, the projection seams, and the extension recipes for adding a family, a section shape, an IFC category, or a property without a second paradigm.

## [02]-[SUBSTRATE_PACKAGES]

The cross-domain C# foundation every package builds on: rails, time/identity, the array substrate, the wire-codegen toolchain, and the test stack. Folder `README.md`s list their own domain additions under their own `## [3]-[SUBSTRATE_PACKAGES]`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core` — every C# folder.
- `Thinktecture.Runtime.Extensions` — every C# folder.
- `Thinktecture.Runtime.Extensions.Json` — Element, Materials, Bim, AppHost, Compute, Persistence, AppUi.
- `Thinktecture.Runtime.Extensions.MessagePack` — Materials, Persistence.
- `JetBrains.Annotations` — every C# folder.

[TIME_IDENTITY]:
- `NodaTime` — Element, Bim, AppHost, Compute, Persistence, AppUi.
- `NodaTime.Serialization.SystemTextJson` — AppHost, Persistence.
- `NodaTime.Serialization.Protobuf` — Element, Compute.
- `System.IO.Hashing` — Rasm, Element, Bim, Fabrication, AppHost, Compute, Persistence, AppUi.

[NUMERIC_SUBSTRATE]:
- `CommunityToolkit.HighPerformance` — Compute, Persistence.
- `System.Numerics.Tensors` — Rasm, AppHost, Compute.
- `UnitsNet` — Element, Bim, Compute, Fabrication, Materials, AppUi.

[GRAPH_ALGORITHM]:
- `QuikGraph` — Rasm, Element, Bim, Persistence (the pure-managed graph containers + `AlgorithmExtensions` facade; the kernel `Spatial/neighbors` Prim-MST normal orientation, the seam `ElementGraph` topology view, the Persistence synchronous `Query/topology` lane, the Bim CPM/`SystemTrace`/version-DAG walks).

[WIRE_CODEGEN]:
- `Generator.Equals` — Element, Bim, Persistence (source-generated structural equality + member-level diff; runtime `Generator.Equals.Runtime`).
- `Google.Protobuf` — Element (the `rasm.element.v1` graph wire messages), Compute.
- `Grpc.AspNetCore`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.AspNetCore.Web`
- `Grpc.Core.Api`
- `Grpc.Net.Client`
- `Grpc.Net.Client.Web`
- `Grpc.Net.Common`
- `Grpc.Tools` — Element (`GrpcServices=None` message codegen), Compute.
- `Riok.Mapperly` — Element, Bim, Persistence (compile-time graph↔DTO/proto mapping; runtime `Riok.Mapperly.Abstractions`).

[TEST_SUBSTRATE]:
- `xunit.v3.*` — assert/common/extensibility.core/mtp-v2.
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Testing.Platform` stack.
- `BenchmarkDotNet` — Compute, Persistence.
- `Verify.XunitV3` — Persistence, AppUi.
- `NodaTime.Testing`
- `Microsoft.Extensions.TimeProvider.Testing`
- `Microsoft.Extensions.Diagnostics.Testing`
- `Microsoft.AspNetCore.TestHost`
