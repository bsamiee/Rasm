# [INTERCHANGE_ARCHITECTURE]

The professional domain map of `interchange` — the host-free wire boundary and inbound dependency root of the TypeScript branch. Four sub-domains (`transport`, `codec`, `contract`, `ingress`) decode the C# wire, dial the browser transport, reassemble artifact frames, and reconstruct the .NET fault family; it mints no parallel wire shape and owns no geometry.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [1]-[DOMAIN_MAP]

```text codemap
interchange/
├── transport/         # the outbound transport edge, command gateway, and codegen input
│   ├── transport.ts   # the protocol-selection dial, capability tuple, framing fold, and codegen input
│   └── gateway.ts     # the outbound command-dial face over the control verbs
├── codec/             # the byte-to-typed decode/encode interior
│   ├── codec.ts       # the codec-keyed decode/encode dispatch table
│   ├── patch.ts       # the recorded-intent partial-update patch rail
│   ├── frame.ts       # the content-addressed artifact-frame reassembly
│   └── parity.ts      # the cross-runtime byte-identity reproduction binding
├── contract/          # the canonical contract surface
│   ├── inventory.ts   # the cluster-to-rail wire inventory and the suite anchors
│   └── descriptor.ts  # the runtime descriptor-diff contract-evolution gate
└── ingress/           # the untrusted-ingress enforcement, tolerance, and fault rails
    ├── refinement.ts  # the decode-enforcement brand/filter/budget vocabulary
    ├── quarantine.ts  # the contract-drift tolerance terminal
    └── fault.ts       # the exhaustive .NET fault reconstruction
```

`transport` is the inbound root every rail composes and the outbound dial face; `codec` is the byte-to-typed interior; `contract` is the canonical inventory and descriptor-evolution gate; `ingress` carries the enforcement, drift-tolerance, and fault rails over untrusted input. `@connectrpc/*` stops here; the fold tier reads only decoded shapes.

## [2]-[SEAMS]

```text seams
codec/codec          ←  csharp:Rasm.AppHost/Agent         # CapabilityDescriptor command-shape (content-key)
codec/parity         ←  csharp:Rasm.AppHost/Runtime       # HLC two-half bigint round-trip parity (content-key)
codec/parity         ⇄  csharp:Rasm/Geometry              # XxHash128 canonical-byte content-key (content-key)
*                    ←  csharp:Rasm.AppHost               # support-capture verb (wire)
*                    ←  csharp:Rasm.Compute               # ProgressStore stream proto (wire)
codec/codec          ←  csharp:Rasm.Persistence/Sync      # OpLogEntryWire / CrdtOpWire / sync-segment stream (wire)
codec/codec          ←  csharp:Rasm.Persistence/Version   # SnapshotHeaderWire messagepack (wire)
codec/codec          ←  csharp:Rasm.Bim/Review            # DiffWire ElementChange content-keyed join (wire)
codec/codec          ←  csharp:Rasm.Bim/Exchange          # BimWire snapshot / OpLogWire / BimWireDescriptor (wire)
codec/codec          ←  csharp:Rasm.Materials/Appearance  # MaterialWire OpenPBR vector wire (wire)
transport/gateway    ←  csharp:Rasm.AppUi/Shell           # CommandPayloadWire / CommandReceiptWire (wire)
transport/transport  ←  csharp:Rasm.Persistence/Version   # SnapshotHeaderWire (wire)
codec/codec          ←  csharp:Rasm.AppUi/Render          # RenderReceiptWire frame-hash proof (projection)
ingress/fault        ←  csharp:Rasm.Compute/Runtime       # FaultDetailWire trailer package/code/case/evidence (fault)
```

## [3]-[CHARTERS]

- `transport`: the boundary edge — one polymorphic browser transport over a protocol-selection axis (Connect for server-streams, gRPC-Web where the backend dictates), one interceptor stamping correlation/traceparent/bearer, and the chunked-framing fold. The `buf`/capability-SDK codegen leg emits the browser-dialable client set (`src/gen/*_pb.ts`, one `createClient` per service) when the descriptor source lands. `gateway` co-locates as the outbound command-dial face — one `CommandGateway` over the control verbs reading the `projection` `AvailabilityStore` at dial time.
- `codec`: the byte-to-typed interior — one decode/encode dispatch table read by codec key (proto, messagepack, json-stj `Schema.Class`, embedded geometry), each row carrying `decode` and an `Option`-carried `encode`. `patch` admits a recorded mutation against an already-admitted value; `frame` is the content-addressed artifact-frame reassembly through per-frame `Crc32` into a pre-sized sink with whole-artifact `XxHash128` derivation; `parity` binds the worker-minted `ContentKey` to the C# `XxHash128` digest bit-identically over the frozen `ONE_WIRE_FIXTURE_CORPUS`.
- `contract`: the canonical contract surface — `inventory` maps every consumed C# wire cluster to its codec and TS rail; `descriptor` is the runtime descriptor-diff gate, one `DescriptorGate` classifying the running `FileDescriptorSet` as `Identical`/`Additive`/`Breaking` and gating dial-time client construction before the first call.
- `ingress`: the untrusted-input rails — `refinement` is the decode-enforcement vocabulary (`Schema.brand` slots, `Schema.filter` bounds, decode budgets, the RFC 6901 `JsonPointer` brand); `quarantine` is the contract-drift tolerance terminal with an `ArrayFormatter` drift-report and `isomorphic-dompurify` sanitization; `fault` is the exhaustive .NET fault reconstruction, one `Data.TaggedEnum` rebuilt from the status-details trailer through `Match.tagsExhaustive`.
