# [INTERCHANGE_ARCHITECTURE]

The domain map of `interchange` — the host-free wire boundary and inbound dependency root of the TypeScript branch. The `transport`, `codec`, `contract`, and `ingress` sub-domains decode the C# wire, dial the browser transport, reassemble artifact frames, and reconstruct the .NET fault family; it mints no parallel wire shape and owns no geometry.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
interchange/
├── transport/         # Outbound transport edge, command gateway, and codegen input
│   ├── transport.ts   # Protocol-selection dial, capability tuple, framing fold, and codegen input
│   └── gateway.ts     # Outbound command-dial face over control verbs
├── codec/             # Byte-to-typed decode/encode interior
│   ├── codec.ts       # Codec-keyed decode/encode dispatch table
│   ├── patch.ts       # Recorded-intent partial-update patch rail
│   ├── frame.ts       # Content-addressed artifact-frame reassembly
│   └── parity.ts      # Cross-runtime byte-identity reproduction binding
├── contract/          # Canonical contract surface
│   ├── inventory.ts   # Cluster-to-rail wire inventory and suite anchors
│   └── descriptor.ts  # Runtime descriptor-diff contract-evolution gate
└── ingress/           # Untrusted-ingress enforcement, tolerance, and fault rails
    ├── refinement.ts  # Decode-enforcement brand/filter/budget vocabulary
    ├── quarantine.ts  # Contract-drift tolerance terminal
    └── fault.ts       # Exhaustive .NET fault reconstruction
```

`transport` is the inbound root every rail composes and the outbound dial face; `codec` is the byte-to-typed interior; `contract` is the canonical inventory and descriptor-evolution gate; `ingress` carries the enforcement, drift-tolerance, and fault rails over untrusted input. `@connectrpc/*` stops here; the fold tier reads only decoded shapes.

## [02]-[SEAMS]

```text seams
codec/codec          ←  csharp:Rasm.AppHost/Agent         # [CONTENT_KEY]: CapabilityDescriptor command-shape
codec/parity         ←  csharp:Rasm.AppHost/Runtime       # [CONTENT_KEY]: HLC two-half bigint round-trip parity
codec/parity         ⇄  csharp:Rasm/Geometry              # [CONTENT_KEY]: XxHash128 canonical-byte content-key
*                    ←  csharp:Rasm.AppHost               # [WIRE]: support-capture verb
*                    ←  csharp:Rasm.Compute               # [WIRE]: ProgressStore stream proto
codec/codec          ←  csharp:Rasm.Persistence/Sync      # [WIRE]: OpLogEntryWire / CrdtOpWire / sync-segment stream
codec/codec          ←  csharp:Rasm.Persistence/Version   # [WIRE]: SnapshotHeaderWire messagepack
codec/codec          ←  csharp:Rasm.Bim/Review            # [WIRE]: DiffWire ElementChange content-keyed join
codec/codec          ←  csharp:Rasm.Bim/Exchange          # [WIRE]: BimWire snapshot / OpLogWire / BimWireDescriptor
codec/codec          ←  csharp:Rasm.Materials/Appearance  # [WIRE]: MaterialWire OpenPBR vector wire
transport/gateway    ←  csharp:Rasm.AppUi/Shell           # [WIRE]: CommandPayloadWire / CommandReceiptWire
transport/transport  ←  csharp:Rasm.Persistence/Version   # [WIRE]: SnapshotHeaderWire
codec/codec          ←  csharp:Rasm.AppUi/Render          # [PROJECTION]: RenderReceiptWire frame-hash proof
ingress/fault        ←  csharp:Rasm.Compute/Runtime       # [FAULT]: FaultDetailWire trailer package/code/case/evidence
ingress              ⇄  typescript:platform/transport     # [CONTENT_KEY]: ContentKey brand mint and tile keying
transport            →  typescript:ui/interaction         # [PORT]: CommandGateway / IntentRegistry intent dial
```

## [03]-[CHARTERS]

- `transport`: the boundary edge — one polymorphic browser transport over a protocol-selection axis (Connect for server-streams, gRPC-Web where the backend dictates), one interceptor stamping correlation/traceparent/bearer, and the chunked-framing fold. The `buf`/capability-SDK codegen leg emits the browser-dialable client set (`src/gen/*_pb.ts`, one `createClient` per service) when the descriptor source lands. `gateway` co-locates as the outbound command-dial face — one `CommandGateway` over the control verbs reading the `projection` `AvailabilityStore` at dial time.
- `codec`: the byte-to-typed interior — one decode/encode dispatch table read by codec key (proto, messagepack, json-stj `Schema.Class`, embedded geometry), each row carrying `decode` and an `Option`-carried `encode`. `patch` admits a recorded mutation against an already-admitted value; `frame` is the content-addressed artifact-frame reassembly through per-frame `Crc32` into a pre-sized sink with whole-artifact `XxHash128` derivation; `parity` binds the worker-minted `ContentKey` to the C# `XxHash128` digest bit-identically over the frozen `ONE_WIRE_FIXTURE_CORPUS`.
- `contract`: the canonical contract surface — `inventory` maps every consumed C# wire cluster to its codec and TS rail; `descriptor` is the runtime descriptor-diff gate, one `DescriptorGate` classifying the running `FileDescriptorSet` as `Identical`/`Additive`/`Breaking` and gating dial-time client construction before the first call.
- `ingress`: the untrusted-input rails — `refinement` is the decode-enforcement vocabulary (`Schema.brand` slots, `Schema.filter` bounds, decode budgets, the RFC 6901 `JsonPointer` brand); `quarantine` is the contract-drift tolerance terminal with an `ArrayFormatter` drift-report and `isomorphic-dompurify` sanitization; `fault` is the exhaustive .NET fault reconstruction, one `Data.TaggedEnum` rebuilt from the status-details trailer through `Match.tagsExhaustive`.
