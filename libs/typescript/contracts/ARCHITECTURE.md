# [TS_CONTRACTS_ARCHITECTURE]

`@rasm/contracts/*` seats generated wire vocabulary as an independent TypeScript SDK. Contract paths survive generation, compilation, and publication through one wildcard.

## [01]-[DOMAIN_MAP]

```text codemap
contracts/
├── .api/rasm-ts-contracts.md
├── package.json
├── project.json
├── tsconfig.build.json
└── gen/
    ├── buf/validate/validate_pb.ts
    ├── google/{rpc,type}/<source>_pb.ts
    ├── io/cloudevents/v1/
    │   ├── cloudevents_pb.ts
    │   └── cloudevents_avro.ts
    └── rasm/contracts/<family>/v1/<source>_pb.ts
```

## [02]-[STRATA]

Generated modules import `@bufbuild/protobuf` and no sibling library. `@rasm/contracts` publishes them independently; `@rasm/ts` is one workspace consumer, not their owner.

## [03]-[SEAMS]

| [INDEX] | [KIND]       | [SOURCE]                 | [LANDING]                      |
| :-----: | :----------- | :----------------------- | :----------------------------- |
|  [01]   | `[CONTRACT]` | corpus estate descriptor | generated estate module        |
|  [02]   | `[BOUNDARY]` | publisher descriptor     | generated publisher module     |
|  [03]   | `[ASSET]`    | frozen publisher AVSC    | generated readonly JSON module |
|  [04]   | `[SHAPE]`    | generated message schema | consumer-owned validated type  |

`CloudEventSchema` stays a direct publisher module input to the event codec. `CloudEventsAvro` supplies the exact publisher AVSC to a consumer-owned Avro codec; neither enters an estate registry.

## [04]-[INTERNAL]

`protoc-gen-es` writes schema-first modules with protovalidate-required refinements. Buf emits reachable support descriptors beside selected roots; Assay projects the frozen AVSC bytes into one readonly literal. TypeScript clean-builds both rails into `dist` and the package wildcard exposes only compiled JavaScript and declarations.

## [05]-[ROUTING]

| [INDEX] | [CHANGE]           | [OWNER_SURFACE]          | [EDIT]                       |
| :-----: | :----------------- | :----------------------- | :--------------------------- |
|  [01]   | corpus declaration | `tests/contracts/proto`  | edit source and regenerate   |
|  [02]   | publisher binding  | root Buf configuration   | change package filter        |
|  [03]   | publisher asset    | `tests/contracts/vendor` | replace bytes and regenerate |
|  [04]   | generator pair     | pnpm catalog             | align runtime and regenerate |
|  [05]   | consumer family    | root generation template | add or remove package token  |

## [06]-[BOUNDARIES]

- `contracts` owns generated descriptor constants, decoded value types, and exact publisher-asset projections.
- `dist` is derived package output; `gen` remains the committed generation authority and never publishes raw.
- Consumer packages own validation, domain admission, transport construction, event SDK projection, and Avro codec compilation.
- Corpus sources and frozen publisher assets own wire shape.
