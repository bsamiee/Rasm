# [TS_CORE]

`core` is the TypeScript branch's wave-0 law — the single acyclic base every folder composes: the cross-language value floor, the host-free state algebra over it, the C#-minted wire boundary, and the observability vocabulary, one body joined by one content identity, one clock law, one fault vocabulary, and one keyed-decode registry. Its bar is decode-once, travel-settled: every cross-language primitive is admitted and branded at exactly one seam, cross-runtime parity — content keys, clock layout, digests — is proven bit-identical against frozen contract corpora rather than claimed, partiality is honestly typed (never a `NaN`, a silent round, or a fabricated causal order), and every module runs identically under node, bun, and the browser. A new wire family is a census row, a new fault class is a table entry, a new identity dimension is one static — every concept has one authority and no parallel restatement exists to drift.

Core imports nothing host-bound and nothing from the branch; every other folder composes these owners, and the whole C# estate lands its wire families through the one interchange registry. Unit conversion and non-SI admission stay C#-owned — a `{value, unit}` shape never exists in the branch. Core defines the shapes persistence, transport, serving, and rendering carry, and nothing they run.

## [01]-[ROUTER]

- [01]-[VALUE](.planning/value/): Decode-once value floor — content mint, `Hlc` clock, SI quantity, fault policy; one edit site per concept.
- [02]-[STATE](.planning/state/): Host-free algebra over the floor — CRDT merge, the keyed fold with its one `AsOf`, causal lattice, statechart.
- [03]-[INTERCHANGE](.planning/interchange/): One keyed-decode registry every C#-minted wire family lands on; never transport.
- [04]-[OBSERVE](.planning/observe/): Observability vocabulary and its total derivations — SLO-as-algebra and the dashboard model; zero exporters.

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
