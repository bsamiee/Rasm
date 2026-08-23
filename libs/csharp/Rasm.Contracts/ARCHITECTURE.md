# [RASM_CONTRACTS_ARCHITECTURE]

`Rasm.Contracts` seats generated wire vocabulary at the C# estate floor. Corpus package identity derives namespace and directory identity, while publisher-owned types stay with their C# distributions.

## [01]-[DOMAIN_MAP]

```text codemap
Rasm.Contracts/
├── .api/rasm-contracts.md                  # Generator grammar and gate-emitted descriptor roster
├── README.md                                # Package readme carried by the NuGet artifact
├── Rasm.Contracts.csproj                   # Generated assembly, NuGet metadata, and direct runtime closure
└── Generated/<Family>/V1/
    ├── <Source>.cs                         # Message, enum, reflection, and embedded option descriptors
    └── <Source>Grpc.cs                     # Service bases and clients when the source declares services
```

## [02]-[STRATA]

`Rasm.Contracts` imports its codegen runtimes and no workspace library. Workspace consumers take the project directly; unrelated applications take the versioned NuGet artifact produced from that same project. Both paths expose one assembly and never require a product composition root.

## [03]-[SEAMS]

| [INDEX] | [KIND]       | [SOURCE]                 | [LANDING]                       |
| :-----: | :----------- | :----------------------- | :------------------------------ |
|  [01]   | `[CONTRACT]` | corpus estate descriptor | generated assembly descriptor   |
|  [02]   | `[BOUNDARY]` | generated `IMessage<T>`  | consumer-owned admission        |
|  [03]   | `[PORT]`     | generated service symbol | consumer-owned server or client |

Selective public-root omission leaves publisher descriptors with their package-shipped C# types, preserving one C# owner per descriptor.

## [04]-[INTERNAL]

Buf remote plugins derive managed namespaces, rewrite the clean output root, and update the catalogue marker block from the same descriptor image. Build compilation proves the generated dependency closure without injecting workspace libraries or analyzers.

`Rasm.Contracts` ships XML documentation and a portable-symbol package whose PDB embeds the exact generated source, so debugging resolves the packaged emission without a mutable source checkout or a second hand-maintained archive.

BSR generated SDKs remain the module/plugin publication rail. Their one-module/one-plugin package shape cannot express this branch's selective public-root closure and merged message-plus-gRPC assembly, so `Rasm.Contracts` is the stable C# application boundary over the same canonical corpus.

## [05]-[ROUTING]

| [INDEX] | [CHANGE]              | [OWNER_SURFACE]           | [EDIT]                          |
| :-----: | :-------------------- | :------------------------ | :------------------------------ |
|  [01]   | corpus declaration    | `tests/contracts/proto`   | edit source and regenerate      |
|  [02]   | public-root selection | root Buf configuration    | change the selective root set   |
|  [03]   | generator behavior    | root generation template  | change option and regenerate    |
|  [04]   | runtime dependency    | central package manifests | align direct project references |
|  [05]   | package release       | `Rasm.Contracts.csproj`   | bump version and release notes  |

## [06]-[BOUNDARIES]

- `Rasm.Contracts` owns generated C# symbols and embedded descriptors.
- `Rasm.Contracts.csproj` owns one versioned NuGet identity, and project and package references resolve the same assembly.
- Consumer packages own parsing limits, validation projection, domain admission, and transport binding.
- Corpus sources own wire shape and compatibility.
