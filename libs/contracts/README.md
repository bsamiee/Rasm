# [CONTRACTS]

`contracts` is the estate's one wire corpus: the `manifest.json` registry, the `proto/` estate module, frozen publisher bytes under `vendor/`, the `conformance/` vector corpus, and the three emissions `gen/` carries for C#, Python, and TypeScript. Every value crossing a process or publisher boundary registers here once, no domain model or hand code lands here, and each consumer admits generated values at its own boundary.

## [01]-[ROUTER]

[CORPUS]:
- [01]-[BUF](.api/bufbuild-buf.md): Buf driver catalog — `buf.yaml` gate keys, `buf.gen.yaml` template keys, verbs, exit algebra, and carve law.
- [02]-[MANIFEST](manifest.json): Registry of every wire case — authority class, definition, actors, readiness, and fingerprinted assets.

[DOTNET]:
- [03]-[DOTNET_CATALOG](.api/dotnet.md): C# symbol grammar for one proto declaration and the gate-emitted roster per family.
- [04]-[DOTNET_EMISSION](gen/dotnet): Swept `Rasm.Contracts.<Family>` message, descriptor, and service sources compiled into one assembly.

[PYTHON]:
- [05]-[PYTHON_CATALOG](.api/python.md): Python module grammar for one proto declaration and the gate-emitted roster per package.
- [06]-[PYTHON_EMISSION](gen/python): Swept `rasm.contracts` portions — `_pb` messages and `_connect` stubs at package and proto path.

[TYPESCRIPT]:
- [07]-[TYPESCRIPT_CATALOG](.api/typescript.md): TypeScript schema grammar for one proto declaration and the gate-emitted roster per package.
- [08]-[TYPESCRIPT_EMISSION](gen/typescript): Swept `<proto path>_pb.ts` schema modules and the publisher-asset projection by contract path.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; each group names its language, versions centralize in that language's central manifest, and this folder's `.api/` corroborates.

[DOTNET]:
- `buf.build/protocolbuffers/csharp` — Remote BSR message emitter pinned by version and revision, so no C# generator rides the machine PATH.
- `buf.build/grpc/csharp` — Remote BSR service emitter pinned beside it; `<Svc>Base` and `<Svc>Client` land in the same assembly.

[PYTHON]:
- `protoc-gen-py` — Emits `_pb` message modules from the descriptor image, pinned as a pair with `protobuf-py`.
- `protoc-gen-connectrpc` — Emits asynchronous `_connect` service protocols, applications, and clients, pinned as a pair with `connectrpc`.

[TYPESCRIPT]:
- `@bufbuild/protoc-gen-es` — Emits `_pb.ts` schema modules and protovalidate-refined valid types, pinned as a pair with `@bufbuild/protobuf`.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from each language's registry, whose charters own the full contracts; that branch's `libs/<lang>/.api/` holds the shared API evidence.

[DOTNET]:
- `Google.Protobuf` — Message, reflection, binary, and ProtoJSON runtime under every generated class.
- `Grpc.Core.Api` — Service bases, clients, methods, and streaming calls the service emission derives.
- `Google.Api.CommonProtos` — Imported `google.rpc` details and `google.type` values the emission references.
- `Celly.Protovalidate` — `Buf.Validate` option descriptors the embedded rules reference; consumers evaluate them at admission.

[PYTHON]:
- `protobuf-py` — Message, descriptor, WKT, binary, and ProtoJSON runtime under every `_pb` module.
- `connectrpc` — Asynchronous Connect applications, clients, interceptors, and codecs under every `_connect` module.

[TYPESCRIPT]:
- `@bufbuild/protobuf` — Generated-code boot, message codecs, reflection, registries, and well-known types under every `_pb.ts` module.

## [04]-[DISTRIBUTION]

Each emission ships as one independent distribution a foreign consumer installs with no branch estate present, and workspace consumers reach the same emission by dependency. This README rides inside the NuGet package and the npm tarball.

[NUGET]:
- package: `Rasm.Contracts` — generated assembly, XML documentation, this README, and a `.snupkg` embedding the generated source.
- version: MinVer derivation from the repository's `v*` tags; an untagged tree floors at `MinVerMinimumMajorMinor` as an alpha height build.
- workspace: `ProjectReference` to `Rasm.Contracts.csproj`; external: `PackageReference` to the released package.

```xml copy-safe
<PackageReference Include="Rasm.Contracts" Version="<tag-derived version>" />
```

Generated types are the wire vocabulary, never domain models: a consumer bounds binary decoding, evaluates the embedded descriptor rules, then projects the admitted message into its own domain.

```csharp copy-safe
using Google.Protobuf;
using Rasm.Contracts.Declaration;

static DeclarationRecord Decode(System.IO.Stream source) {
    System.ArgumentNullException.ThrowIfNull(source);
    using CodedInputStream input = CodedInputStream.CreateWithLimits(source, sizeLimit: 1_048_576, recursionLimit: 100);
    return DeclarationRecord.Parser.ParseFrom(input);
}
```

[PYPI]:
- package: `rasm-contracts` — `uv_build` wheel whose module root is `gen/python`, `rasm` and `rasm.contracts` both PEP 420 portions.
- workspace: uv workspace member; external: one bare `rasm-contracts` dependency row, `uv.lock` fixing the version.
- imports: package and proto path — `rasm.contracts.rasm.contracts.<family>.<source>_pb` and `<source>_connect` for services.

[NPM]:
- package: `@rasm/contracts` — `./*` resolves `gen/typescript/*.ts` in the workspace and `dist/*` declarations and ESM in the tarball.
- workspace: `workspace:*` dependency; external: `@rasm/contracts/<proto path>_pb` subpath imports over the published wildcard.

[BSR]:
- module: `buf.build/rasm/contracts` — the named estate module alone, pushed on the `main` label; publisher modules stay unnamed and unpublished.
- consumers: `deps: [buf.build/rasm/contracts]` in a foreign `buf.yaml`, `buf dep update` locking one immutable commit.
