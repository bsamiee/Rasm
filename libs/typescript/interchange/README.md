# [INTERCHANGE]

`interchange` is the byte-to-typed-and-back wire boundary of the TypeScript branch and the inbound dependency root of the whole branch, spanning descriptor set to wire to everything. It owns the single browser transport over a protocol-selection axis across four browser-dialable generated services, the codec-keyed decode/encode dispatch table, the content-addressed artifact-frame reassembly, the exhaustive fault reconstruction, the contract-drift quarantine, the descriptor-diff evolution gate, the recorded-intent patch application, and the outbound command gateway. This is the single boundary at which wire-projection fences are transcribed verbatim: the domain authors no wire shape and consumes the C# wire only. The folder-map lives in `ARCHITECTURE.md`; forward concepts in `IDEAS.md`; open work in `TASKLOG.md`.

## [1]-[ROUTER]

- [1]-[TRANSPORT](.planning/Transport/transport.md): Protocol-selection transport, the four clients, the capability tuple, the framing fold, and the `buf` + capability-SDK codegen input.
- [2]-[GATEWAY](.planning/Transport/gateway.md): Outbound verb gateway, the dial-time availability gate, and the intent registry.
- [3]-[CODEC](.planning/Codec/codec.md): Codec-keyed dispatch table for both directions, the GeoJSON geometry vocabulary row, the backpressured segment-stream decode, and the compression-codec admission.
- [4]-[PATCH](.planning/Codec/patch.md): Recorded-intent six-verb `PatchOpWire` union, the error-accumulating `applyPatch` fold, the `createTests` concurrency guard, and the `FieldMask` lower.
- [5]-[FRAME](.planning/Codec/frame.md): Content-addressed reassembly, the owned `Crc32`, the `XxHash128` content key, and the transferable worker boundary.
- [6]-[PARITY](.planning/Codec/parity.md): Frozen-corpus content-key reproduction binding, the LE↔BE normalize, and the HLC two-half round-trip.
- [7]-[INVENTORY](.planning/Contract/inventory.md): Thirteen-cluster cluster-to-rail map, the three suite anchors, and the versioning law.
- [8]-[DESCRIPTOR](.planning/Contract/descriptor.md): Runtime descriptor-diff verdict (`Identical`/`Additive`/`Breaking`) over `createFileRegistry` reflection, the declaration-order checksum, and the dial-time gate.
- [9]-[REFINEMENT](.planning/Ingress/refinement.md): Brand identity slots (`Guid`/`ContentKey`/`OrdinalKey`/`JsonPointer`), the filter bounds, and the decode-budget ingress ceilings.
- [10]-[QUARANTINE](.planning/Ingress/quarantine.md): Drift classifier, the structured drift-report, the budget gate, and the DOM sanitizer.
- [11]-[FAULT](.planning/Ingress/fault.md): Tagged fault family, the total wire projections, and the exhaustive render table.

## [2]-[DOMAIN_PACKAGES]

Every interchange-domain library the folder uses, planned or implemented. Versions are centralized in the one workspace catalog; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below.

[TRANSPORT_RPC]:
- `@connectrpc/connect` — `createClient`, `ConnectError.from`, `findDetails`
- `@connectrpc/connect-web` — `createConnectTransport`, `createGrpcWebTransport`, the unary dial

[CODEC]:
- `@msgpack/msgpack` — the binary snapshot/sync codec with `useBigInt64` `Decoder`, the `decodeMultiStream`/`decodeArrayStream` async stream decoders for the sync-segment leg, and the `ExtensionCodec` registry (the `crdt`/extension-row seam)

[PATCH]:
- `rfc6902` — the recorded-intent JSON Patch application owner: the six-verb `Operation` union, the error-accumulating `applyPatch`, the `createTests` optimistic-concurrency guard, and the RFC 6901 `Pointer` evaluator backing the `Codec/patch.md` `FieldMask`-and-patch lower; decodes the C# `JsonPatchDocument`/`FieldMask` partial-update wire

[CODEGEN_PLUGIN]:
- `@bufbuild/protoc-gen-es` — the message-and-service codegen plugin
- `protoc-gen-capability-es` — the second `buf.gen.yaml` plugin deriving the capability-SDK surface off the `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` descriptor (a local buf plugin, not a published library; admitted when the descriptor source lands)

## [3]-[SUBSTRATE_PACKAGES]

Branch-level substrate packages this folder consumes; charters and API evidence live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` branch.

[RUNTIME_CORE]:
- `effect` — the `Effect.Service` transport, `Schema` codec surface, `Stream` framing, `Data.TaggedEnum` fault family, `Match` dispatch, brand/filter refinement

[SECURITY_SUBSTRATE]:
- `isomorphic-dompurify` — the DOM-bound text sanitization at the quarantine terminal

[IDENTITY]:
- `hash-wasm` — the whole-artifact 128-bit content-key digest via `createXXHash128(seedLow, seedHigh)`; one wasm owns the 32/64/128-bit surface, and the `decompress`-side `createXXHash3`/CRC surface backs the per-frame integrity floor. `createXXHash128` emits little-endian while the C# `System.IO.Hashing.XxHash128` persists big-endian, so the `Codec/frame.md` reassembly harness normalizes byte order at the boundary

[WIRE_CODEGEN]:
- `@bufbuild/protobuf` — the descriptor runtime, `create`/`fromBinary`/`toBinary`/`mergeFromBinary`, the file-aware registry (`createFileRegistry`), the `DescMessage`/`DescField`/`DescService`/`DescMethod` reflection surface the descriptor-evolution gate classifies, and the `@bufbuild/protobuf/wkt` `FieldMask`/`Any` well-known types the patch rail lowers
- `@bufbuild/buf` — the build-time `buf generate` CLI driver
