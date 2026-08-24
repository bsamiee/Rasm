# [RASM_CONTRACTS_RULINGS]

`Rasm.Contracts` rulings settle generated C# distribution decisions.

## [01]-[PACKAGES]

- `Celly.Protovalidate`, `Google.Api.CommonProtos`, `Google.Protobuf`, and `Grpc.Core.Api` form the direct closure compiled by generated sources.
- Selective public-root omission preserves package-shipped CloudEvents and health types as their sole .NET owners.

## [02]-[SHAPE]

- One assembly carries message symbols, service bases, and clients because every consumer reads one descriptor graph.
- `<Nullable>disable</Nullable>` matches protoc's nullable-oblivious emission; consumer admission owns message presence.
- Workspace `ProjectReference` and external `PackageReference` consumption build from the same packable project and expose one assembly identity.
- BSR's per-module/per-plugin packages cannot replace the selective merged assembly; they remain the schema and remote-plugin publication plane.

## [03]-[COLLAPSE]

- `Rasm.Contracts` stays a generated-only assembly — workspace substrate injection makes the wire floor depend on unrelated libraries.

## [04]-[STRUCTURE]

- `Generated/**` stays under the clean generation sweep, while package metadata and durable docs stay outside it.
- Catalogue roster markers contain gate-emitted descriptor data; generator grammar remains the hand-maintained correspondence.
- NuGet output carries the README, XML documentation, and portable symbols with embedded generated source; no source mirror is packaged.

## [05]-[PROCESS]

- `assay contracts generate` authors the tree and roster together, so corpus or generator changes land through full regeneration.
- Releases increment the project version and release notes, regenerate from the canonical corpus, then pack that exact project.
