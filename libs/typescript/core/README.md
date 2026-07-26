# [TS_CORE]

`core` is the branch's S0 vocabulary-and-law package — the value floor, the host-free state algebra over it, the contract wire plane with its keyed-decode registry, and the observability vocabulary. Every folder above composes these owners; core defines the shapes they carry and runs none of them.

## [01]-[ROUTER]

- [01]-[VALUE](.planning/value/): Decode-once value floor — every cross-language primitive branded at admission, one edit site per concept.
- [02]-[STATE](.planning/state/): Host-free state algebra over the floor; one `AsOf` coordinate rules every replay.
- [03]-[INTERCHANGE](.planning/interchange/): Wire boundary — each contract family encodes or decodes once at the keyed registry; never serving.
- [04]-[OBSERVE](.planning/observe/): Observability vocabulary and its total derivations — SLO algebra through dashboard model; zero exporters.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[DECODE_SUBSTRATE]:
- `@bufbuild/protobuf`
- `@connectrpc/connect`
- `@connectrpc/connect-web`
- `cbor-x`
- `@msgpack/msgpack`
- `rfc6902`
- `cloudevents`
- `mqtt`
- `hash-wasm`

[FOLD_ALGEBRA]:
- `@electric-sql/d2mini`
- `@electric-sql/d2ts`
- `@effect/typeclass`

[OBSERVE_VOCABULARY]:
- `@opentelemetry/semantic-conventions`

[BENCHMARK_STATISTICS]:
- `mitata`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Ts registry; the registry and its charters own the full contracts, and `libs/typescript/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental`
