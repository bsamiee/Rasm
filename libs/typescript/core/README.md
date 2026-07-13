# [TS_CORE]

`libs/typescript/core` owns the branch's wave-0 law: the cross-language value floor, the host-free state algebra, the interchange plane decoding every C#-minted wire family through one keyed-decode registry, and the observability vocabulary with its SLO and dashboard derivation. Core imports nothing host-bound and nothing from the branch; every other folder composes these owners.

## [01]-[ROUTER]

[VALUE]:
- [01]-[SCHEMA](.planning/value/schema.md): Refined branded-primitive vocabulary and the `Ingress` decode-budget ceilings.
- [02]-[IDENTITY](.planning/value/identity.md): `AppIdentity` deployment spine and the tenant scope key rails resolve against.
- [03]-[CONTENTKEY](.planning/value/contentKey.md): One content-identity digest mint every delegating site seeds from.
- [04]-[CLOCK](.planning/value/clock.md): `Hlc` hybrid-logical stamp and the uncertainty-grade time windows.
- [05]-[QUANTITY](.planning/value/quantity.md): SI-coherent magnitude and its dimension vector, canonicalized at admission.
- [06]-[FAULT](.planning/value/fault.md): Fault severity vocabulary, retry-budget ledger, and degrade ladder every rail inherits.

[STATE]:
- [07]-[MERGE](.planning/state/merge.md): Lawful CRDT merge algebra proving the lattice laws its siblings compose.
- [08]-[FOLD](.planning/state/fold.md): Keyed-fold owner carrying the single `AsOf` time coordinate and replay lane.
- [09]-[CAUSAL](.planning/state/causal.md): Version-vector lattice, causal delivery buffer, and stability frontier.
- [10]-[COMMIT](.planning/state/commit.md): Content-keyed commit graph, branch heads, and Merkle summaries.
- [11]-[MACHINE](.planning/state/machine.md): Data-driven statechart compiled once into fold, drivers, and serializable actor.
- [12]-[EVIDENCE](.planning/state/evidence.md): Decoded outcome family — receipts, progress, and availability.
- [13]-[FEED](.planning/state/feed.md): `Hlc`-ordered evidence timeline and its column band.
- [14]-[PRESENCE](.planning/state/presence.md): Actor-presence CRDT over proven merge rows.

[INTERCHANGE]:
- [15]-[FORMAT](.planning/interchange/format.md): Byte-dialect engines behind one decode transform.
- [16]-[CODEC](.planning/interchange/codec.md): One keyed-decode registry every C#-minted wire family lands on as a census row.
- [17]-[FRAME](.planning/interchange/frame.md): Keyed frame-reassembly fold under the `Ingress` budget.
- [18]-[CONTRACT](.planning/interchange/contract.md): Descriptor-drift diff graded into verdicts before decode fails at runtime.
- [19]-[INVOKE](.planning/interchange/invoke.md): Command capability contract carried both directions.

[OBSERVE]:
- [20]-[CONVENTION](.planning/observe/convention.md): Typed semconv attribute, metric, and event vocabulary.
- [21]-[SLO](.planning/observe/slo.md): Objective and SLI algebra and the burn-rate alert derivation.
- [22]-[BOARD](.planning/observe/board.md): Dashboard model, query, and pack/suite dispatch.

## [02]-[DOMAIN_PACKAGES]

Folder-specific libraries admitted by core; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

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

Shared substrate consumed from the branch registry; `libs/typescript/.planning/README.md` and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`
- `@effect/platform`
