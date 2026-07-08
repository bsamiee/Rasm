# [CSHARP_BRANCH]

The C# branch router and the cross-cutting package registry. The branch aggregates the planning-scoped packages, each carrying its own four index docs (`README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`) at its root and its design pages under one `.planning/`; the two HOST-BOUNDARY roots carry durable host-bound source with card pools and a folder `.api/` tier, their index docs and design pages arriving with conversion. This node routes to those package roots and registers only the packages shared across two or more C# folders; a folder README lists only its own additions, shared API catalogues live in `libs/csharp/.api/`, and versions live in the one C# manifest, never here.

## [01]-[ROUTER]

The package roots in strata order; each planning-scoped root `README.md` carries the folder's page router and its own package additions, and each `ARCHITECTURE.md` carries the folder's sub-domain map. The HOST-BOUNDARY rows route to source folders.

- [01]-[KERNEL]: [Rasm](../Rasm/README.md) — RhinoCommon-aware geometry/numeric kernel.
- [02]-[AEC-DOMAIN]: [Rasm.Element](../Rasm.Element/README.md) — lowest-AEC element seam.
- [03]-[AEC-DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — profiles, appearance, construction.
- [04]-[AEC-DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — BIM object model and IFC/glTF/STEP exchange.
- [05]-[AEC-DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — the making-domain owner: design-to-fabrication derivation, process planning, CAM, nesting, forming, joining, additive, verification, spec, and shop documentation.
- [06]-[APP-PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — runtime spine.
- [07]-[APP-PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured execution.
- [08]-[APP-PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — durable stores.
- [09]-[APP-PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product UI.
- [10]-[HOST-BOUNDARY]: [Rasm.Rhino](../Rasm.Rhino/) — RhinoCommon + Eto host boundary; references only `Rasm`.
- [11]-[HOST-BOUNDARY]: [Rasm.Grasshopper](../Rasm.Grasshopper/) — GH2 + Eto host boundary; references only `Rasm`.

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
- `NodaTime` — Element, Bim, Fabrication, AppHost, Compute, Persistence, AppUi.
- `NodaTime.Serialization.SystemTextJson` — AppHost, Persistence.
- `NodaTime.Serialization.Protobuf` — Element, Compute.
- `System.IO.Hashing` — Rasm, Element, Bim, Fabrication, AppHost, Compute, Persistence, AppUi.

[NUMERIC_SUBSTRATE]:
- `CommunityToolkit.HighPerformance` — Rasm, Materials, Bim, Fabrication, Compute, Persistence.
- `System.Numerics.Tensors` — Rasm, Fabrication, AppHost, Compute, Persistence.
- `UnitsNet` — Element, Bim, Compute, Fabrication, Materials, AppUi.

[GRAPH_ALGORITHM]:
- `QuikGraph` — Rasm, AppUi, Bim, Compute, Element, Fabrication, Materials, Persistence: pure-managed graph containers + `AlgorithmExtensions` behind the MST/topology/CPM/version-DAG/setup-precedence walks.

[WIRE_CODEGEN]:
- `Generator.Equals` — Bim, Compute, Element, Persistence (source-generated structural equality + member-level diff; runtime `Generator.Equals.Runtime`).
- `Google.Protobuf` — Element (the `rasm.element.v1` graph wire messages), Compute.
- `Grpc.AspNetCore`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.AspNetCore.Web`
- `Grpc.Core.Api`
- `Grpc.Net.Client`
- `Grpc.Net.Client.Web`
- `Grpc.Net.Common`
- `Grpc.Tools` — Element (`GrpcServices=None` message codegen), Compute.
- `Riok.Mapperly` — Bim, Compute, Element, Fabrication, Materials, Persistence (compile-time graph↔DTO/proto/record mapping; runtime `Riok.Mapperly.Abstractions`).

[TEST_SUBSTRATE]:
- `xunit.v3.*` — assert/common/extensibility.core/mtp-v2.
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Testing.Platform` stack.
- `BenchmarkDotNet` — Compute, Persistence.
- `Verify.XunitV3` — Persistence, AppUi.
