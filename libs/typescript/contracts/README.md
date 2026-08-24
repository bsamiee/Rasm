# [TS_CONTRACTS]

`@rasm/contracts/*` is the branch's one publishable package: the generated TypeScript SDK for corpus descriptors and publisher assets. One wildcard preserves contract paths across ESM JavaScript and declarations, so a foreign application installs the tarball without reaching the private `@rasm/*` estate.

## [01]-[ROUTER]

[GENERATED]:
- [01]-[CATALOGUE](.api/rasm-ts-contracts.md): Package surface, generator symbol grammar, and derived descriptor roster.
- [02]-[EMISSION](gen): Estate, support-closure, publisher-descriptor, and publisher-asset modules by contract path.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in `pnpm-workspace.yaml` and corroborate against this folder's `.api/`.

[GENERATOR]:
- `@bufbuild/protoc-gen-es` — Emits TypeScript descriptor modules and protovalidate-refined value types.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the TypeScript registry, whose charters own the full contracts; `libs/typescript/.api/` holds the shared API evidence.

[WIRE_RUNTIME]:
- `@bufbuild/protobuf` — Supplies generated-code boot, message codecs, reflection, registries, and well-known types.
