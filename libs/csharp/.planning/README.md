# [CSHARP_BRANCH]

The C# branch router and the cross-cutting package registry. The branch aggregates eight planning-scoped packages, each carrying its own four index docs (`README.md`, `ARCHITECTURE.md`, `IDEAS.md`, `TASKLOG.md`) at its root and its design pages under one `.planning/`. This node routes to those package roots and registers only the packages shared across two or more C# folders; a folder README lists only its own additions, and versions live in the one C# manifest, never here.

## [1]-[ROUTER]

The eight planning-scoped package roots, in strata order. Each root README carries the folder's page router and its own package additions; each `ARCHITECTURE.md` carries the folder's sub-domain map.

- KERNEL: [Rasm](../Rasm/README.md) — geometry/numeric kernel; mature `Vectors`/`Analysis`/`Domain` plus the greenfield `Geometry/` robust-core scaffolded under `Geometry/.planning/`.
- AEC-DOMAIN: [Rasm.Materials](../Rasm.Materials/README.md) — profiles, appearance, construction.
- AEC-DOMAIN: [Rasm.Bim](../Rasm.Bim/README.md) — BIM object model and IFC/glTF/STEP exchange.
- AEC-DOMAIN: [Rasm.Fabrication](../Rasm.Fabrication/README.md) — portable HLR/CAM/nesting frontier.
- APP-PLATFORM: [Rasm.AppHost](../Rasm.AppHost/README.md) — runtime spine.
- APP-PLATFORM: [Rasm.Compute](../Rasm.Compute/README.md) — measured execution.
- APP-PLATFORM: [Rasm.Persistence](../Rasm.Persistence/README.md) — durable stores.
- APP-PLATFORM: [Rasm.AppUi](../Rasm.AppUi/README.md) — Avalonia product UI.

The HOST-BOUNDARY packages `Rasm.Rhino` and `Rasm.Grasshopper` are out-of-scope-durable source with no `.planning/`; the future app root composes them, and the open seams it spans ride this branch `TASKLOG.md`.

## [2]-[CROSS_CUTTING_PACKAGES]

The packages every C# folder shares or that span two or more C# folders, trimmed from the per-folder registries so a folder README lists only its own additions. Per-folder-only packages (Avalonia, GeometryGym, Clipper2, ONNX Runtime, the OPC-UA/MQTT stack, EF Core providers, the cloud object-store SDKs) stay in their owning folder README.

[FUNCTIONAL_CORE]:
- LanguageExt.Core — `Fin`/`Validation`/`Eff` result rails, `Seq`/`Option`/`HashMap`/`Set` immutable collections, `Error`. Every C# folder.
- Thinktecture.Runtime.Extensions — `[Union]`/`[SmartEnum]`/`[ValueObject]` generated dispatch and value objects. Every C# folder.
- Thinktecture.Runtime.Extensions.Json — the generated-owner JSON converters. Bim, AppHost, Compute, Persistence, AppUi.

[TIME_IDENTITY]:
- NodaTime — `Instant`/`Duration` receipt stamping and the semantic clock. Bim, AppHost, Compute, Persistence, AppUi.
- System.IO.Hashing — `XxHash128`, the one content-address seed reproduced across runtimes. Rasm, Bim, AppHost, Compute, Persistence, AppUi.
- UnitsNet — the quantity owner the Compute units boundary composes; consumed by AppUi and, through the Compute seam, by Materials.

[NUMERIC]:
- MathNet.Numerics — dense linear algebra. Rasm (constraint solve), Compute (the dense lane).
- CSparse — sparse direct solves. Rasm (mature `Vectors` lane), Compute (the sparse lane).
- System.Numerics.Tensors — `TensorPrimitives` SIMD. Rasm (field/spectral lane), Compute (the tensor lane).

[GEOMETRY_INTERCHANGE]:
- SharpGLTF.Core / SharpGLTF.Toolkit — the glTF read/write path. Bim (the exchange codec), Compute (the tile-emit path); Bim also admits SharpGLTF.Runtime.
- meshoptimizer — leaf-geometry optimization. Bim (via Alimer.Bindings.MeshOptimizer), Compute.

[RUNTIME_PLATFORM]:
- Microsoft.Extensions.Caching.Hybrid — the cache port and its L2 contribution. AppHost (the port), Compute (the result cache), Persistence (the L2 contribution).
- Microsoft.Extensions.AI.Abstractions — the `AIFunction`/`IChatClient` surface. AppHost (the MCP projection), Compute (the model lane).
- The gRPC and ASP.NET Core hosting surface (`Grpc.Net.Client`, `Grpc.AspNetCore`, the test host) — the wire transport. AppHost (the ports), Compute (the remote lane).

[TESTING]:
- BenchmarkDotNet — micro-benchmark claims. Compute, Persistence.
- SharpFuzz — out-of-process decode fuzzing. Compute, Persistence.
- Verify.XunitV3 — snapshot verification. Persistence, AppUi.

The universal managed-law, property-based, coverage, mutation, and architecture rails (xUnit v3, CsCheck, coverlet, Stryker, ArchUnitNET) apply to every C# package per the route-owned `testing-cs` doctrine and are not duplicated as folder package additions.
