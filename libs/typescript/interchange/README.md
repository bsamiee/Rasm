# [INTERCHANGE]

`interchange` is the byte-to-typed-and-back wire boundary of the TypeScript branch and the inbound dependency root of the whole branch (descriptor set to wire to everything). It owns the single browser transport over a protocol-selection axis across four browser-dialable generated services, the codec-keyed decode/encode dispatch table, the content-addressed artifact-frame reassembly, the exhaustive fault reconstruction, the contract-drift quarantine, the descriptor-diff evolution gate, the recorded-intent patch application, and the outbound command gateway. It is the single boundary at which the wire-projection fences are transcribed verbatim — the domain authors no wire shape and consumes the C# wire only. The professional domain folder-map lives in `ARCHITECTURE.md`; the forward concepts in `IDEAS.md`; open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, organized by sub-domain:

- [transport/transport.md](.planning/transport/transport.md) — the protocol-selection transport, four clients, the capability tuple, the framing fold, and the buf + capability-SDK codegen input
- [codecs/decode-rail.md](.planning/codecs/decode-rail.md) — the codec-keyed dispatch table, both directions, the GeoJSON geometry vocabulary row, the backpressured segment-stream decode, the compression-codec admission
- [codecs/patch-rail.md](.planning/codecs/patch-rail.md) — the recorded-intent six-verb `PatchOpWire` union, the error-accumulating `applyPatch` fold, the `createTests` concurrency guard, the `FieldMask` lower
- [evolution/descriptor-gate.md](.planning/evolution/descriptor-gate.md) — the runtime descriptor-diff verdict (`Identical`/`Additive`/`Breaking`) over `createFileRegistry` reflection, the declaration-order checksum, the dial-time gate
- [refinement/schema-refinement.md](.planning/refinement/schema-refinement.md) — the brand identity slots (`Guid`/`ContentKey`/`OrdinalKey`/`JsonPointer`), the filter bounds, the decode-budget ingress ceilings
- [artifacts/frame-reassembly.md](.planning/artifacts/frame-reassembly.md) — the content-addressed reassembly, the owned Crc32, the XxHash128 content key, the transferable worker boundary
- [parity/content-key-parity.md](.planning/parity/content-key-parity.md) — the frozen-corpus content-key reproduction binding, the LE↔BE normalize, the HLC two-half round-trip
- [faults/fault-family.md](.planning/faults/fault-family.md) — the tagged fault family, the total wire projections, the exhaustive render table
- [quarantine/drift-terminal.md](.planning/quarantine/drift-terminal.md) — the drift classifier, the structured drift-report, the budget gate, the DOM sanitizer
- [gateway/command-gateway.md](.planning/gateway/command-gateway.md) — the outbound verb gateway, the dial-time availability gate, the intent registry
- [contracts/wire-inventory.md](.planning/contracts/wire-inventory.md) — the thirteen-cluster cluster-to-rail map, the three suite anchors, the versioning law

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one workspace catalog; this registry carries no pin.

- `@connectrpc/connect` — `createClient`, `ConnectError.from`, `findDetails`
- `@connectrpc/connect-web` — `createConnectTransport`, `createGrpcWebTransport`, the unary dial
- `@bufbuild/protobuf` — the descriptor runtime, `create`/`fromBinary`/`toBinary`/`mergeFromBinary`, the file-aware registry (`createFileRegistry`), the `DescMessage`/`DescField`/`DescService`/`DescMethod` reflection surface the descriptor-evolution gate classifies, and the `@bufbuild/protobuf/wkt` `FieldMask`/`Any` well-known types the patch rail lowers
- `@bufbuild/protoc-gen-es` — the message-and-service codegen plugin
- `@bufbuild/buf` — the build-time `buf generate` CLI driver
- `@msgpack/msgpack` — the binary snapshot/sync codec with `useBigInt64` `Decoder`, the `decodeMultiStream`/`decodeArrayStream` async stream decoders for the sync-segment leg, and the `ExtensionCodec` registry (the `crdt`/extension-row seam)
- `hash-wasm` — the whole-artifact 128-bit content-key digest via `createXXHash128(seedLow, seedHigh)`; one wasm owns the 32/64/128-bit surface, and the `decompress`-side `createXXHash3`/CRC surface backs the per-frame integrity floor. `createXXHash128` emits little-endian while the C# `System.IO.Hashing.XxHash128` persists big-endian, so the `artifacts/frame-reassembly.md` reassembly harness normalizes byte order at the boundary
- `rfc6902` — the recorded-intent JSON Patch application owner: the six-verb `Operation` union, the error-accumulating `applyPatch`, the `createTests` optimistic-concurrency guard, and the RFC 6901 `Pointer` evaluator backing the `codecs/patch-rail.md` `FieldMask`-and-patch lower; decodes the C# `JsonPatchDocument`/`FieldMask` partial-update wire
- `protoc-gen-capability-es` — the second `buf.gen.yaml` plugin deriving the capability-SDK surface off the `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` descriptor (a local buf plugin, not a published library; admitted when the descriptor source lands)

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect` — the `Effect.Service` transport, `Schema` codec surface, `Stream` framing, `Data.TaggedEnum` fault family, `Match` dispatch, brand/filter refinement
- `isomorphic-dompurify` — the DOM-bound text sanitization at the quarantine terminal
