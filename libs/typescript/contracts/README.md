# [TS_CONTRACTS]

`@rasm/contracts/*` is the publishable generated TypeScript SDK for corpus descriptors and publisher assets. One wildcard preserves contract paths across ESM JavaScript and declarations without coupling an application to `@rasm/ts`.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](.api/rasm-ts-contracts.md): Package surface, generator symbol grammar, and derived descriptor roster.
- [02]-[EMISSION](gen): Clean-swept estate, support, publisher descriptor, and publisher-asset modules.

## [02]-[DOMAIN_PACKAGES]

[GENERATOR]:
- `@bufbuild/protoc-gen-es` — Emits TypeScript descriptor modules and protovalidate-refined value types.

## [03]-[SUBSTRATE_PACKAGES]

[WIRE_RUNTIME]:
- `@bufbuild/protobuf` — Supplies generated-code boot, message codecs, reflection, registries, and well-known types.

## [04]-[PACKAGE]

- `pnpm --filter @rasm/contracts build` clean-builds `gen/**` into `dist/**` through TypeScript project build state under root `.cache/`.
- `pnpm --filter @rasm/contracts pack` runs that build and packs only `dist`, `README.md`, `LICENSE`, and package metadata.
- `@rasm/ts` consumes `@rasm/contracts` through `workspace:*`; an unrelated application installs the tarball, reaching no private branch estate.
