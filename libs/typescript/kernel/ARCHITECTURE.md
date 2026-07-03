# [KERNEL_ARCHITECTURE]

The domain map of `kernel` — the W0 contract floor of the TypeScript branch. The `identity`, `clock`, `schema`, and `fault` sub-domains each own a cross-language VALUE family — content and app identity, hybrid-logical time, branded primitives and SI quantity, fault classification and budgets — no wire shape, no transport, no sibling import, every runtime folder typing against them and the frozen parity corpora pinning the byte-level laws.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
kernel/src/            # W0; imports nothing — every runtime folder types against this floor
├── identity/          # the cross-language identity values
│   ├── contentkey.ts  # XxHash128 seed-zero ContentKey brand — the ONE mint; :x32 spelling; LE→BE normalize at every delegate
│   └── appidentity.ts # AppKey / AppIdentity / TenantContext value vocabulary — {app, tenant, build, host-fingerprint} dimensions telemetry, browser boot, and store scopes derive from
├── clock/             # the hybrid-logical time law
│   ├── hlc.ts         # Hlc brand — two-half compose (physical first, logical second, both little-endian), compare/merge folds
│   └── uncertainty.ts # honest clock-uncertainty window vocabulary state causality consumes
├── schema/            # the branded type floor and the quantity law
│   ├── brand.ts       # branded primitive family (Guid-v7, OrdinalKey, JsonPointer, BCP-47 Locale, …) + INGRESS_BUDGET refinement budgets — the decode-once law's type floor
│   └── quantity.ts    # SI Quantity: magnitude + dimension; a {value, unit} shape never exists in TS
└── fault/             # the cross-language fault vocabulary
    ├── classify.ts    # cross-language fault classification values + the enricher CONTRACT (telemetry consumes, wire implements)
    └── budget.ts      # retry/degradation budget vocabulary folder policies type against
```

## [02]-[SEAMS]

```text seams
identity/contentkey  ⇄  csharp:Rasm/Geometry          # [CONTENT_KEY]: content-hashing wasm reproducing the one Domain/ContentHash seed (XxHash128 seed-zero)
identity/contentkey  ←  csharp:Rasm.Compute/Runtime   # [WIRE]: XxHash128 seed-zero two-half
clock/hlc            ←  csharp:Rasm.AppHost/Runtime   # [CONTENT_KEY]: HLC two-half bigint round-trip parity
clock/hlc            ←  csharp:Rasm.AppHost/Runtime   # [WIRE]: HLC two-half + tenant
schema/quantity      ⇄  csharp:Rasm.Compute/Symbolic  # [WIRE]: QuantityFamily SI canonicalization consumed by host-free peers over the wire (AEC-domain admits UnitsNet in-folder, never a downward reference)
```

Every row is a byte-level parity claim, never a code dependency: the C# side mints, the floor reproduces, and the `proof/corpus` parity drivers assert bit-identity against the frozen corpora. The `Quantity` row rides the proto transit at `wire/codec`; the content-key and HLC rows anchor the two identities every other folder's seams derive from.

[FLOOR_LAW]:
- The `ContentKey` mint is singular: `wire/frame`, the `browser/transport` decode worker, and `store/object` delegate to `identity/contentkey` — a second mint, a second content-address notion, or a non-zero seed is the named cross-language drift defect.
- The HLC order law is byte-identical to the C# port law — physical half first, logical half second, both little-endian; `state/causal` consumes the vocabulary, this floor owns the compose/compare/merge folds.
- `AppIdentity` spans {app, tenant, build, host-fingerprint}; the `telemetry` OTel Resource, the `browser` boot, and the `store` scope derive from this ONE value — a per-folder identity re-declaration is the named defect.
- The fault enricher CONTRACT is declared in `fault/classify`; `wire` implements the Layer, `telemetry` consumes it, the app root wires the port — this floor imports neither.
- `Quantity` carries SI magnitude + dimension, canonicalized once at the C# admission; a `{value, unit}` re-decode is the rejected form.
- Every brand decodes under an `INGRESS_BUDGET` refinement budget; the law/arbitrary substrate publishes behind the dev-only subpath so `fast-check` never rides a runtime graph.
