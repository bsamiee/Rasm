# [TS_CORE]

`core` is the branch's S0 host-free implementation floor: value admission, state folds and machines, contract codecs, and observability derivation. Every higher folder composes these owners; core owns no persistence, serving, renderer, or exporter.

## [01]-[ROUTER]

[VALUE]:
- [01]-[SCHEMA](.planning/value/schema.md): Value-shape derivation — refined cross-language primitives, ordered vocabularies, ingress caps.
- [02]-[IDENTITY](.planning/value/identity.md): Process and tenancy identity — boot-set dimensions and the reversible tenant scope spelling.
- [03]-[CONTENTKEY](.planning/value/contentKey.md): Sole content-digest mint — algorithm rows deriving branded keys and both wire codecs.
- [04]-[CLOCK](.planning/value/clock.md): Causal time on the frozen sixteen-byte layout — tick-exact stamps, uncertainty-honest ordering.
- [05]-[QUANTITY](.planning/value/quantity.md): Total SI dimension algebra — partial magnitude operations land on a typed fault, never a throw.
- [06]-[FAULT](.planning/value/fault.md): Recovery policy — classification, capture, budget, and degrade sharing taxonomy, never merging.

[STATE]:
- [07]-[MERGE](.planning/state/merge.md): Lawful merge instances — semigroup, law, and equivalence coupled per replicated type.
- [08]-[FOLD](.planning/state/fold.md): Fold plans replaying by ordinal under full trace coordinates and event-time windows.
- [09]-[CAUSAL](.planning/state/causal.md): Four-way causal ordering honest under clock uncertainty — held delivery, finality reads.
- [10]-[COMMIT](.planning/state/commit.md): Anti-entropy across replicas — each commit carries parents, causal vector, stamp, and author.
- [11]-[MACHINE](.planning/state/machine.md): Statecharts as data — one transition spec whose macrostep folds the declared tree.
- [12]-[EVIDENCE](.planning/state/evidence.md): Peer outcome decode — receipt lifecycle ranks and availability lattices per tenant.
- [13]-[FEED](.planning/state/feed.md): Tenant-scoped evidence-and-document timeline folded by contribution identity.
- [14]-[PRESENCE](.planning/state/presence.md): Live actor faces — join, heartbeat, and ephemeral-move ops converging without consensus.

[INTERCHANGE]:
- [15]-[FORMAT](.planning/interchange/format.md): Encoding arms lifted onto one typed parse rail under one defect normalization.
- [16]-[CODEC](.planning/interchange/codec.md): Closed wire-family roster and one bounded walk over every recursive tree it lands.
- [17]-[FRAME](.planning/interchange/frame.md): Interleaved band assembly with verified rendezvous and IFC container admission.
- [18]-[CONTRACT](.planning/interchange/contract.md): Pinned-against-shipped comparison — binary, JSON, and source compatibility as one gate.
- [19]-[CARRIER](.planning/interchange/carrier.md): Trace continuity on every transport — one context value, tenant promotion, the message envelope.
- [20]-[INVOKE](.planning/interchange/invoke.md): Connect client mint, descriptor-set admission, and command dispatch serving framed sockets.

[OBSERVE]:
- [21]-[CONVENTION](.planning/observe/convention.md): Signal-name conformance as rows — dotted names under UCUM codes, store translation, C# parity.
- [22]-[SLO](.planning/observe/slo.md): Objective grading from data — `Sli` cases schema-gated, burn windows priced, alert rows compiled.
- [23]-[BOARD](.planning/observe/board.md): Observability read surface from data alone — one expression tree rendered per backend target.
- [24]-[TAP](.planning/observe/tap.md): Hook-point admission — app-scoped registries, modality-split handlers, per-subscriber breach isolation.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DECODE_SUBSTRATE]:
- `@bufbuild/protobuf`
- `@connectrpc/connect`
- `@connectrpc/connect-web`
- `cbor-x`
- `@msgpack/msgpack`
- `rfc6902`
- `hash-wasm`

[FOLD_ALGEBRA]:
- `@electric-sql/d2mini`
- `@electric-sql/d2ts`
- `@effect/typeclass`

[OBSERVE_VOCABULARY]:
- `@opentelemetry/semantic-conventions`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental` — `VariantSchema` projections and `Machine` state algebra.

[EVENT_FABRIC]:
- `cloudevents` — `interchange/carrier` seats the branch's one message-envelope mint over its `CloudEvent` class.

[BENCH]:
- `mitata`
