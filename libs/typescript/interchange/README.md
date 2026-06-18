# [INTERCHANGE]

`interchange` is the byte-to-typed-and-back wire boundary of the TypeScript branch and the inbound dependency root of the whole branch (descriptor set to wire to everything). It owns the single browser transport over a protocol-selection axis across four browser-dialable generated services, the codec-keyed decode/encode dispatch table, the content-addressed artifact-frame reassembly, the exhaustive fault reconstruction, the contract-drift quarantine, and the outbound command gateway. It is the single boundary at which the wire-projection fences are transcribed verbatim — the domain authors no wire shape and consumes the C# wire only. The professional domain folder-map lives in `ARCHITECTURE.md`; the forward concepts in `IDEAS.md`; open work in `TASKLOG.md`.

## [1]-[ROUTER]

The design pages under `.planning/`, organized by sub-domain:

- [transport/transport.md](.planning/transport/transport.md) — the protocol-selection transport, four clients, the capability tuple, the framing fold, and the buf + capability-SDK codegen input
- [codecs/decode-rail.md](.planning/codecs/decode-rail.md) — the codec-keyed dispatch table, both directions, the embedded-geometry row
- [refinement/schema-refinement.md](.planning/refinement/schema-refinement.md) — the brand identity slots, the filter bounds, the decode-budget ingress ceilings
- [artifacts/frame-reassembly.md](.planning/artifacts/frame-reassembly.md) — the content-addressed reassembly, the owned Crc32, the XxHash128 content key, the transferable worker boundary
- [faults/fault-family.md](.planning/faults/fault-family.md) — the tagged fault family, the total wire projections, the exhaustive render table
- [quarantine/drift-terminal.md](.planning/quarantine/drift-terminal.md) — the drift classifier, the structured drift-report, the budget gate, the DOM sanitizer
- [gateway/command-gateway.md](.planning/gateway/command-gateway.md) — the outbound verb gateway, the dial-time availability gate, the intent registry
- [contracts/wire-inventory.md](.planning/contracts/wire-inventory.md) — the eleven-cluster cluster-to-rail map, the three suite anchors, the versioning law

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions are centralized in the one workspace catalog; this registry carries no pin.

- `@connectrpc/connect` — `createClient`, `ConnectError.from`, `findDetails`
- `@connectrpc/connect-web` — `createConnectTransport`, `createGrpcWebTransport`, the unary dial
- `@bufbuild/protobuf` — the descriptor runtime, `create`/`fromBinary`/`toBinary`, the file-aware registry
- `@bufbuild/protoc-gen-es` — the message-and-service codegen plugin
- `@bufbuild/buf` — the build-time `buf generate` CLI driver
- `@msgpack/msgpack` — the binary snapshot/sync codec with `useBigInt64` `Decoder`
- `xxhash-wasm` — the whole-artifact content-key digest (`h32`/`h64`); the 128-bit parity with the C# `XxHash128` seed is the `artifacts/frame-reassembly.md` `CONTENT_HASHING` research gate
- `protoc-gen-capability-es` — the second `buf.gen.yaml` plugin deriving the capability-SDK surface off the `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` descriptor (a local buf plugin, not a published library; admitted when the descriptor source lands)

## [3]-[CROSS_CUTTING]

Branch-level cross-cutting packages consumed by this folder.

- `effect` — the `Effect.Service` transport, `Schema` codec surface, `Stream` framing, `Data.TaggedEnum` fault family, `Match` dispatch, brand/filter refinement
- `isomorphic-dompurify` — the DOM-bound text sanitization at the quarantine terminal
