# [RASM_CONTRACTS]

`Rasm.Contracts` owns the committed generated C# distribution for the corpus contract plane. Workspace consumers reference one isolated project; unrelated applications install the same versioned NuGet package. Buf generation remains the only author of emitted code.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](.api/rasm-contracts.md): Generator symbol grammar, derived descriptor roster, and package admission law.
- [02]-[EMISSION](Generated): Clean-swept message, descriptor, service-base, and client sources.

## [02]-[DISTRIBUTION]

[NUGET]:
- package: `Rasm.Contracts`
- version: `0.1.0`
- target: `net10.0`
- contents: generated assembly, XML documentation, package README, and portable-symbol package with embedded generated source
- workspace: `ProjectReference` to this project
- external: `PackageReference` to the released `Rasm.Contracts` version

```xml copy-safe
<PackageReference Include="Rasm.Contracts" Version="0.1.0" />
```

Generated types are the wire vocabulary, never domain models. Consumers bound binary decoding, evaluate the embedded descriptor rules, then project the admitted message into their own domain:

```csharp copy-safe
using Google.Protobuf;
using Rasm.Contracts.Declaration.V1;

static DeclarationRecord Decode(System.IO.Stream source) {
    System.ArgumentNullException.ThrowIfNull(source);
    using CodedInputStream input = CodedInputStream.CreateWithLimits(source, sizeLimit: 1_048_576, recursionLimit: 100);
    return DeclarationRecord.Parser.ParseFrom(input);
}
```

Canonical schema source and generated-SDK coordinates live at <https://buf.build/rasm/contracts>.

BSR publishes one SDK per module/plugin coordinate. This package is the branch-owned selective projection that combines message and gRPC emission in one assembly, so a BSR package pair cannot replace it.

## [03]-[DOMAIN_PACKAGES]

(none)

## [04]-[SUBSTRATE_PACKAGES]

[WIRE_RUNTIME]:
- `Celly.Protovalidate` — Supplies `Buf.Validate` option descriptors referenced by generated files and evaluates them at consumer admission.
- `Google.Api.CommonProtos` — Supplies imported `google.rpc` details and `google.type` values.
- `Google.Protobuf` — Supplies message, reflection, binary, and ProtoJSON runtime surfaces.
- `Grpc.Core.Api` — Supplies generated service bases, clients, methods, and streaming calls.
