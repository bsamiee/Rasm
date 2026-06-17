# [INTERCHANGE]

`interchange` is the byte-to-typed-and-back wire boundary of the TypeScript branch and the inbound dependency root of the whole branch (descriptor set to wire to everything). It owns the single grpc-web transport over four browser-dialable generated services, the six-codec polymorphic decode/encode rail family, the content-addressed artifact-frame reassembly, the exhaustive fault reconstruction, the contract-drift quarantine, and the outbound command gateway. Zero consumers exist; implementation is full-capability with no holding back; `.planning/` pages are transcribed, never re-designed. This folder is the single boundary at which the wire-projection fences are transcribed verbatim — the domain authors no wire shape. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                            | [OWNS]                                                                            |
| :-----: | :-------------------------------------------------------------- | :------------------------------------------------------------------------------- |
|   [1]   | [transport](.planning/transport.md)                             | shared transport, four clients, transport-capability shape, buf codegen edge      |
|   [2]   | [codec-rails](.planning/codec-rails.md)                         | six-codec rail family, encode, refinement, geometry, artifact frames, fault family |
|   [3]   | [gateway-and-quarantine](.planning/gateway-and-quarantine.md)   | quarantine fold, contract inventory, intent registry, outbound command gateway     |

## [2]-[ADMISSIONS_RECORD]

Each package maps to its consuming page, central catalogue at `libs/typescript/.api/`, and admission status. Concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`); this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`.

| [INDEX] | [PACKAGE]                | [PAGE]                    | [CATALOGUE]                         | [STATUS]          |
| :-----: | :----------------------- | :------------------------ | :---------------------------------- | :---------------- |
|   [1]   | @connectrpc/connect      | transport                 | `.api/api-transport-wire.md`        | admitted          |
|   [2]   | @connectrpc/connect-web  | transport, gateway        | `.api/api-transport-wire.md`        | admitted          |
|   [3]   | @bufbuild/protobuf       | transport, codec-rails    | `.api/api-transport-wire.md`        | admitted          |
|   [4]   | @bufbuild/protoc-gen-es  | transport                 | `.api/api-transport-wire.md`        | admitted          |
|   [5]   | @bufbuild/buf            | transport                 | `.api/api-transport-wire.md`        | admitted          |
|   [6]   | @msgpack/msgpack         | codec-rails               | `.api/api-transport-wire.md`        | admitted          |
|   [7]   | xxhash-wasm              | codec-rails               | `.api/api-transport-wire.md`        | admitted          |
|   [8]   | rfc6902                  | codec-rails               | `.api/api-transport-wire.md`        | admitted          |
|   [9]   | effect                   | every page                | `.api/api-effect.md`                | admitted          |
|  [10]   | capability-descriptor codegen | transport             | —                                   | catalogue-pending |

## [3]-[PROOF_GATES]

`[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]            | [RAIL]                     | [EVIDENCE]                                          |
| :-----: | :---------------- | :------------------------- | :------------------------------------------------- |
|  [G1]   | catalog resolve   | `pnpm` install/restore     | `catalogMode` strict resolves `@rasm/ts`            |
|  [G2]   | descriptor codegen | buf codegen rail          | `interchange/gen/*_pb.ts` present after generate    |
|  [G3]   | typecheck         | `tsgo` typecheck           | zero diagnostics over the domain                    |
|  [G4]   | unit-pbt          | `vitest` project `interchange` | codec-fold and frame-stitch algebraic laws pass |
|  [G5]   | content-key spike | tier-2 XxHash128 harness   | byte-identity against the upstream digest, seed=0   |
|  [G6]   | page render       | local mermaid-cli          | page diagrams render through the local renderer      |
