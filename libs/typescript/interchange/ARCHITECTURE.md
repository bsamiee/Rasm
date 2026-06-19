# [INTERCHANGE_ARCHITECTURE]

The professional-domain map of `interchange` — the host-free wire boundary and inbound dependency root of the TypeScript branch. The folder decodes the C# wire vocabulary, dials the single browser transport over a protocol-selection axis, reassembles content-addressed artifact frames, reconstructs the exhaustive .NET fault family, classifies descriptor-diff contract evolution, applies recorded-intent partial-update patches, tolerates contract drift at the untrusted-ingress terminal, and exposes the outbound command gateway. It mints no parallel wire shape and owns no geometry or domain state. Each sub-domain below names a real domain concept and carries a one-line charter; the planned-but-empty `clients` sub-domain is a visible gap the ideas and tasks fuel. Dependency direction lives once in the branch `ARCHITECTURE.md`; boundaries and wires live on the tasks that build them.

## [1]-[DOMAIN_MAP]

The sub-domain folders mirror the eventual source tree. The transport edge is the inbound root every rail composes; the codec family is the byte-to-typed interior; refinement, artifacts, faults, quarantine, gateway, and contracts carry the enforcement, reassembly, fault, tolerance, egress, and inventory concerns. `@connectrpc/*` stops at this folder; the fold tier reads only the decoded shapes.

```text codemap
interchange/
├── transport/                  # the outbound transport edge and its codegen input
│   └── transport.md
├── clients/                    # (planned) the generated browser-dialable client set
├── codecs/                     # the byte-to-typed decode and encode rail family
│   ├── decode-rail.md
│   └── patch-rail.md
├── evolution/                  # the runtime descriptor-diff contract-evolution gate
│   └── descriptor-gate.md
├── refinement/                 # the decode-enforcement vocabulary
│   └── schema-refinement.md
├── artifacts/                  # content-addressed artifact-frame reassembly
│   └── frame-reassembly.md
├── parity/                     # the cross-runtime byte-identity reproduction binding
│   └── content-key-parity.md
├── faults/                     # the exhaustive .NET fault reconstruction
│   └── fault-family.md
├── quarantine/                 # the contract-drift tolerance terminal for untrusted ingress
│   └── drift-terminal.md
├── gateway/                    # the outbound command-dial face
│   └── command-gateway.md
└── contracts/                  # the canonical contract inventory and suite anchors
    └── wire-inventory.md
```

## [2]-[CHARTERS]

- `transport`: one polymorphic browser transport over a protocol-selection axis — the Connect protocol for long-lived server-streams, gRPC-Web where the backend dictates — one interceptor stamping correlation, traceparent, and bearer in one pass, the capability tuple per protocol, the chunked-framing fold riding the same transport, and the `buf.gen.yaml` plus the capability-SDK codegen leg folded in as the build-time input edge.
- `clients` (planned): the generated browser-dialable client set, the eventual `src/gen/*_pb.ts` source sub-tree the `buf generate` pipeline emits — one `createClient` row per service over the one shared transport, no hand-shaped client. The derivation lives inside `transport` while the client set is one table; the folder materializes when the generated modules land, fueled by `PROTOCOL_SELECTION_AXIS` (the four wire-service clients) and `CAPABILITY_SDK_CODEGEN` (the fifth capability client).
- `codecs`: the byte-to-typed decode and encode dispatch table read by codec key (proto, messagepack, json-stj `Schema.Class`, embedded geometry), each row carrying its `decode` and an `Option`-carried `encode`; one `DecodeRail` vocabulary owner, every codec and both directions as rows. The `geometry` row carries the real GeoJSON geometry-type vocabulary and the WGS84/precision admission floor mirrored from the C# NTS interior, and the `messagepack` row carries the `none`/`lz4`/`zstd` decompression-codec admission read off the snapshot header. The `patch-rail.md` page co-locates here as the recorded-intent partial-update sibling of the decode table — `DecodeRail` admits a whole value, `PatchRail` admits a recorded mutation against an already-admitted value.
- `evolution`: the runtime descriptor-diff contract-evolution gate — one `DescriptorGate` classifying the running `FileDescriptorSet` against the descriptor the transport dialed as `Identical`, `Additive`, or `Breaking` through `@bufbuild/protobuf` `createFileRegistry` reflection over `DescMessage`/`DescField`/`DescService`, the verdict gating dial-time client construction so a peer running a breaking-drifted contract faults at construction before the first call rather than mid-decode; the TS mirror of the C# `transport#DESCRIPTOR_GATE` `Verdict` the .NET process emits, judged once against the wire descriptor, never a per-call re-classify.
- `refinement`: the decode-enforcement vocabulary — `Schema.brand` identity slots and `Schema.filter` bounds extended with decode budgets (max depth, frame count, assembled-byte ceiling) enforced as `Schema.filter` and `Stream` bounds, plus the RFC 6901 `JsonPointer` brand the patch rail resolves against, the security floor the quarantine terminal relies on.
- `artifacts`: content-addressed artifact-frame reassembly — the framing fold stitched through a per-frame Crc32 verify into one pre-sized sink with whole-artifact XxHash128 content-key derivation, run off the main thread over a transferable-stream worker boundary.
- `parity`: the cross-runtime byte-identity reproduction binding — the interchange-side obligation that the worker-minted `ContentKey` reproduces the C#-owned `XxHash128` digest bit-identically (the LE↔BE normalize) and the HLC two-half bigint round-trip preserves the C# two-64-bit-half order, both anchored to the FROZEN `ONE_WIRE_FIXTURE_CORPUS`; the cross-package harness driver is the future `typescript:testing/` corpus consumer, never re-minting a fixture or a second content-address notion.
- `faults`: the exhaustive fault reconstruction — one `Data.TaggedEnum` family rebuilding every .NET fault from the status-details trailer through the total `faultTagOf` and `faultOf` projections, rendered through one `Match.tagsExhaustive` table.
- `quarantine`: the contract-drift tolerance terminal for untrusted ingress — the decode-stream fold classifying Identical, Additive, or Breaking drift, emitting a structured `ArrayFormatter` drift-report, enforcing the decode budgets, and sanitizing DOM-bound text through `isomorphic-dompurify`.
- `gateway`: the outbound command-dial face — one `CommandGateway` over the control verbs reading the `projection` `AvailabilityStore` as a dial-time gate, plus the `IntentRegistry` deep-link key vocabulary; co-located with the transport owner so `@connectrpc/*` never leaks into the fold tier.
- `contracts`: the canonical contract inventory mapping every consumed C# wire cluster to its codec and TS rail, plus the suite anchors and the wire-projection shape catalogue the rails transcribe verbatim.
