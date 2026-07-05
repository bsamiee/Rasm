# [TS_CORE]

`libs/typescript/core` is the wave-0 law of the branch: the cross-language value floor (identity, clock, content key, quantity, schema brands, fault policy), the host-free state algebra (keyed folds, lawful CRDT merge, causality, machines, evidence, presence), the interchange plane that decodes every C#-minted wire family through ONE keyed-decode registry, and the observability vocabulary with its SLO/dashboard derivation. Every other folder composes these owners; core imports nothing. `ARCHITECTURE.md` carries the domain map and seams, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[SCHEMA](.planning/value/schema.md)
- [02]-[IDENTITY](.planning/value/identity.md)
- [03]-[CONTENTKEY](.planning/value/contentKey.md)
- [04]-[CLOCK](.planning/value/clock.md)
- [05]-[QUANTITY](.planning/value/quantity.md)
- [06]-[FAULT](.planning/value/fault.md)
- [07]-[MERGE](.planning/state/merge.md)
- [08]-[FOLD](.planning/state/fold.md)
- [09]-[CAUSAL](.planning/state/causal.md)
- [10]-[COMMIT](.planning/state/commit.md)
- [11]-[MACHINE](.planning/state/machine.md)
- [12]-[EVIDENCE](.planning/state/evidence.md)
- [13]-[FEED](.planning/state/feed.md)
- [14]-[PRESENCE](.planning/state/presence.md)
- [15]-[FORMAT](.planning/interchange/format.md)
- [16]-[CODEC](.planning/interchange/codec.md)
- [17]-[FRAME](.planning/interchange/frame.md)
- [18]-[CONTRACT](.planning/interchange/contract.md)
- [19]-[INVOKE](.planning/interchange/invoke.md)
- [20]-[CONVENTION](.planning/observe/convention.md)
- [21]-[SLO](.planning/observe/slo.md)
- [22]-[BOARD](.planning/observe/board.md)

## [02]-[DOMAIN_PACKAGES]

Every folder-specific external library, planned or implemented. Versions are centralized in `pnpm-workspace.yaml`; corroborating API evidence lives in the adjacent `.api/` folder.

[WIRE_CODECS]:
- `@bufbuild/protobuf`
- `@connectrpc/connect`
- `@connectrpc/connect-web`
- `cbor-x`
- `@msgpack/msgpack`
- `rfc6902`

[DIGEST]:
- `hash-wasm`

[FOLD_ALGEBRA]:
- `@electric-sql/d2mini`
- `@electric-sql/d2ts`
- `@effect/typeclass`

[OBSERVE_VOCABULARY]:
- `@opentelemetry/semantic-conventions`

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting TypeScript substrate this folder consumes; canonical registry and charters live in `libs/typescript/.planning/README.md` and the adjacent `libs/typescript/.api/` folder.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
