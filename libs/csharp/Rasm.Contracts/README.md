# [RASM_CONTRACTS]

`Rasm.Contracts` owns the committed generated C# distribution for the corpus contract plane. Workspace consumers reference one isolated project; unrelated applications install the same versioned NuGet package. Buf generation remains the only author of emitted code.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](https://github.com/bsamiee/Rasm/blob/main/libs/csharp/Rasm.Contracts/.api/rasm-contracts.md): Symbol grammar and admission law.
- [02]-[EMISSION](https://github.com/bsamiee/Rasm/tree/main/libs/csharp/Rasm.Contracts/Generated): Swept message, descriptor, and service sources.

## [02]-[DISTRIBUTION]

[NUGET]:
- package: `Rasm.Contracts`
- version: MinVer derivation from the repository's `v*` tags; an untagged tree floors at `0.1.0-alpha` height builds
- target: `net10.0`
- contents: generated assembly, XML documentation, this README, and a `.snupkg` with embedded generated source
- provenance: SourceLink stamps repository, branch, and commit from [github.com/bsamiee/Rasm](https://github.com/bsamiee/Rasm) at pack time
- workspace: `ProjectReference` to this project
- external: `PackageReference` to the released `Rasm.Contracts` version

```xml copy-safe
<PackageReference Include="Rasm.Contracts" Version="<tag-derived version>" />
```

[NAMESPACES]:
- Emitted types sit at `Rasm.Contracts.<Family>`, one namespace per corpus package family, with the directory mirroring that tail.
- `ArtifactService`, `CapabilityDiscoveryService`, `ComputeService`, and `ControlService` are the emitted service surfaces.
- Server code derives `<Svc>.<Svc>Base` and client code binds `<Svc>.<Svc>Client` over a `CallInvoker`, both out of this one assembly.
- Publisher CloudEvents and gRPC health types stay with their own packages, so nothing here shadows a package-shipped C# owner.

Generated types are the wire vocabulary, never domain models. Consumers bound binary decoding, evaluate the embedded descriptor rules, then project the admitted message into their own domain:

```csharp copy-safe
using Google.Protobuf;
using Rasm.Contracts.Declaration;

static DeclarationRecord Decode(System.IO.Stream source) {
    System.ArgumentNullException.ThrowIfNull(source);
    using CodedInputStream input = CodedInputStream.CreateWithLimits(source, sizeLimit: 1_048_576, recursionLimit: 100);
    return DeclarationRecord.Parser.ParseFrom(input);
}
```

Canonical schema source and generated-SDK coordinates live at <https://buf.build/rasm/contracts>.

BSR publishes one SDK per module and plugin coordinate, and that pipeline carries no type filter while fixing `opt` at the plugin, so roots widen and emission flags vanish. This package is the branch-owned selective projection combining message and gRPC emission in one assembly, and publication opens generated SDKs to foreign consumers alone.

## [03]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `Directory.Packages.props` and corroborate against this folder's `.api/`.

(none) — every admitted package is shared wire substrate and registers under `[04]`.

## [04]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the C# registry, whose charters own the full contracts; `libs/csharp/.api/` holds the shared API evidence.

[WIRE_RUNTIME]:
- `Celly.Protovalidate` — Supplies `Buf.Validate` option descriptors referenced by generated files and evaluates them at consumer admission.
- `Google.Api.CommonProtos` — Supplies imported `google.rpc` details and `google.type` values.
- `Google.Protobuf` — Supplies message, reflection, binary, and ProtoJSON runtime surfaces.
- `Grpc.Core.Api` — Supplies generated service bases, clients, methods, and streaming calls.
