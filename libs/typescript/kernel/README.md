# [KERNEL]

`kernel` is the W0 contract floor of the TypeScript branch: cross-language identity, clock, schema-brand, quantity, and fault-classification VALUES — no wire shape, no transport, no sibling import. Every runtime folder types against it, so a C# wire drift never ripples through it. The one `XxHash128` seed-zero mint lives here; `wire/frame`, the `browser/transport` decode worker, and `store/object` delegate to it, never re-mint. A new cross-language value is a new brand or vocabulary row here — never a per-folder re-declaration; a new fault class is a `fault/classify` row every rail inherits. The domain map and seam record live in `ARCHITECTURE.md`, the forward pool in `IDEAS.md`, and the work log in `TASKLOG.md`.

## [1]-[ROUTER]

- [01]-[CONTENTKEY](.planning/identity/contentkey.md): the `XxHash128` seed-zero `ContentKey` brand — the ONE mint; `:x32` spelling; LE→BE normalize at every delegate.
- [02]-[APPIDENTITY](.planning/identity/appidentity.md): the `AppKey`/`AppIdentity`/`TenantContext` value vocabulary — the {app, tenant, build, host-fingerprint} dimensions `telemetry`, `browser` boot, and `store` scopes derive from.
- [03]-[HLC](.planning/clock/hlc.md): the `Hlc` brand — two-half compose (physical first, logical second, both little-endian), compare/merge folds.
- [04]-[UNCERTAINTY](.planning/clock/uncertainty.md): the honest clock-uncertainty window vocabulary `state` causality consumes.
- [05]-[BRAND](.planning/schema/brand.md): the branded primitive family (Guid-v7, `OrdinalKey`, `JsonPointer`, BCP-47 `Locale`, …) plus the `INGRESS_BUDGET` refinement budgets — the decode-once law's type floor.
- [06]-[QUANTITY](.planning/schema/quantity.md): the SI `Quantity` — magnitude + dimension; a `{value, unit}` shape never exists in TS.
- [07]-[CLASSIFY](.planning/fault/classify.md): the cross-language fault classification values plus the enricher CONTRACT (`telemetry` consumes, `wire` implements).
- [08]-[BUDGET](.planning/fault/budget.md): the retry/degradation budget vocabulary folder policies type against.

## [2]-[DOMAIN_PACKAGES]

The folder-local packages this floor owns; versions live only in the `pnpm-workspace.yaml` catalog.

[CONTENT_IDENTITY]:
- `hash-wasm` — the WebAssembly `XxHash128` kernel behind the one seed-zero `ContentKey` mint; folder-local, catalogued only at this folder's `.api/`.

## [3]-[SUBSTRATE_PACKAGES]

The branch substrate this folder consumes; the full registry lives in `libs/typescript/.planning/README.md`, with the substrate catalogues at `libs/typescript/.api/`.

- `effect` — rails, `Schema`, branded types, `Match`, vocabulary substrate; the only substrate package the contract floor consumes.
