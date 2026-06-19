# [CSHARP_BRANCH]

The C# branch router and the cross-cutting package registry. The branch aggregates eight planning-scoped packages, each carrying its own four index docs (`README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`) at its root and its design pages under one `.planning/`. This node routes to those package roots and registers only the packages shared across two or more C# folders; a folder README lists only its own additions, and versions live in the one C# manifest, never here.

## [1]-[ROUTER]

Eight planning-scoped package roots in strata order; each root `README.md` carries the folder's page router and its own package additions, and each `ARCHITECTURE.md` carries the folder's sub-domain map.

- [1]-[KERNEL]: [Rasm](../Rasm/README.md) — Mature `Vectors`/`Analysis`/`Domain`, greenfield `Geometry/` robust-core scaffolded under `Geometry/.planning/`.
- [2]-[AEC-DOMAIN]: [Rasm.Materials](../Rasm.Materials/README.md) — profiles, appearance, construction.
- [3]-[AEC-DOMAIN]: [Rasm.Bim](../Rasm.Bim/README.md) — BIM object model and IFC/glTF/STEP exchange.
- [4]-[AEC-DOMAIN]: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — portable HLR/CAM/nesting.
- [5]-[APP-PLATFORM]: [Rasm.AppHost](../Rasm.AppHost/README.md) — runtime spine.
- [6]-[APP-PLATFORM]: [Rasm.Compute](../Rasm.Compute/README.md) — measured execution.
- [7]-[APP-PLATFORM]: [Rasm.Persistence](../Rasm.Persistence/README.md) — durable stores.
- [8]-[APP-PLATFORM]: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product UI.

The HOST-BOUNDARY packages `Rasm.Rhino` and `Rasm.Grasshopper` are out-of-scope-durable source with no `.planning/`.

## [2]-[SUBSTRATE_PACKAGES]

The cross-domain C# foundation every package builds on: rails, time/identity, the array substrate, the wire-codegen toolchain, and the test stack. Folder `README.md`s list their own domain additions under their own `## [3]-[SUBSTRATE_PACKAGES]`.

[FUNCTIONAL_CORE]:
- `LanguageExt.Core` — every C# folder.
- `Thinktecture.Runtime.Extensions` — every C# folder.
- `Thinktecture.Runtime.Extensions.Json` — Bim, AppHost, Compute, Persistence, AppUi.
- `JetBrains.Annotations` — every C# folder.

[TIME_IDENTITY]:
- `NodaTime` — Bim, AppHost, Compute, Persistence, AppUi.
- `NodaTime.Serialization.SystemTextJson` — Bim, AppHost, Compute, Persistence, AppUi.
- `NodaTime.Serialization.Protobuf` — AppHost, Compute.
- `System.IO.Hashing` — Rasm, Bim, AppHost, Compute, Persistence, AppUi.

[NUMERIC_SUBSTRATE]:
- `System.Numerics.Tensors` — Rasm, Compute.

[WIRE_CODEGEN]:
- `Google.Protobuf`
- `Grpc.Net.Client`
- `Grpc.Net.Client.Web`
- `Grpc.Net.Common`
- `Grpc.Core.Api`
- `Grpc.AspNetCore`
- `Grpc.AspNetCore.Web`
- `Grpc.AspNetCore.HealthChecks`
- `Grpc.Tools`

[TEST_SUBSTRATE]:
- `xunit.v3.*` — assert/common/extensibility.core/mtp-v2.
- `CsCheck`
- `coverlet.MTP`
- `Microsoft.Testing.Platform` stack.
- `BenchmarkDotNet` — Compute, Persistence.
- `SharpFuzz` — Compute, Persistence.
- `Verify.XunitV3` — Persistence, AppUi.
- `NodaTime.Testing`
- `Microsoft.Extensions.TimeProvider.Testing`
- `Microsoft.Extensions.Diagnostics.Testing`
- `Microsoft.AspNetCore.TestHost`
