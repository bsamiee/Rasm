# [TS_CORE]

`core` is the branch's S0 host-free implementation floor: value admission, state folds and machines, contract codecs, and observability derivation. Every higher folder composes these owners; core owns no persistence, serving, renderer, or exporter.

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
- `hash-wasm`

[FOLD_ALGEBRA]:
- `@electric-sql/d2mini`
- `@electric-sql/d2ts`
- `@effect/typeclass`

[OBSERVE_VOCABULARY]:
- `@opentelemetry/semantic-conventions`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry; the registry owns the contracts and `libs/typescript/.api/` holds the API evidence.

[TYPING_RAILS]:
- `effect`

[PLATFORM]:
- `@effect/platform`
- `@effect/experimental` — `VariantSchema` projections and `Machine` state algebra.

[WIRE_ENVELOPE]:
- `cloudevents` — `interchange/carrier.md` seats the branch's one message-envelope mint over its `CloudEvent` class.

[BENCHMARK_STATISTICS]:
- `mitata`
