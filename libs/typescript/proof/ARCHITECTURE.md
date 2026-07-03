# [PROOF_ARCHITECTURE]

The domain map of `proof` — the dev-plane infrastructure folder of `libs/typescript`. Four sub-domains: `corpus` owns the frozen cross-language fixtures and typed readers, `law` the reusable combinators and Schema-driven arbitraries, `harness` the container and unit verification lanes, `gauge` the branch-wide checks neither Nx tags nor the exports map can express. Specs live beside their owning folders; `proof` is infrastructure, never the spec home.

Each codemap node is the eventual source file its `.planning/` design page becomes, named in the language's own folder and file casing — PascalCase `.cs`, lowercase `.py`, lowercase `.ts`. Treat every node as realized code; the `.planning/` scaffold is the authoring substrate, never part of the map.

## [01]-[DOMAIN_MAP]

```text codemap
proof/src/
├── corpus/          # Frozen cross-language fixture corpora and their typed readers
│   ├── parity.ts    # byte-identity digest + content-key + HLC two-half corpus readers (HLC vectors upstream-gated)
│   └── golden.ts    # MATERIAL_LAYER_GOLDEN, BimWire golden bytes, convergence corpus readers
├── law/             # Branch law combinators and Schema-driven arbitraries
│   ├── property.ts  # reusable fast-check law combinators: fold identity, merge commutativity, upcast totality
│   └── arbitrary.ts # Schema-driven arbitraries for every kernel brand and decoded wire shape
├── harness/         # The container and unit verification lanes
│   ├── container.ts # testcontainers pg-18.4-with-extensions + S3-compatible object-store rows — the store capability-row and object-presign verification lanes
│   └── unit.ts      # pglite fast unit lane (no server extensions) + layer-sharing patterns
└── gauge/           # The branch-wide gauges the exports map cannot express
    ├── mutation.ts  # Stryker + coverage thresholds as data
    ├── purity.ts    # edge-ledger import / subpath-purity / bundle-law / sub-folder crypto-admission gauge — the checks the exports map cannot express; asserts zero @effect/sql/Migrator | @effect/sql-pg/PgMigrator imports branch-wide
    └── e2e.ts       # playwright + k6 drivers
```

## [02]-[SEAMS]

`proof` consumes the frozen C# corpora read-only; the corpus readers drive each cross-language parity claim as a bit-parity assertion against frozen bytes.

```text seams
corpus/parity  ←  csharp:Rasm/Geometry/Spatial/reconciliation  # [CONTENT_KEY]: byte-identity corpus reproducing the one Domain/ContentHash seed (XxHash128 seed-zero) — the parity driver asserts bit-for-bit agreement
corpus/parity  ←  csharp:Rasm.AppHost/Runtime/Ports            # [WIRE]: HLC two-half + tenant vector corpus — physical half first, logical half second, both little-endian; vectors upstream-gated on the C# fixture corpus landing
corpus/parity  ←  csharp:Rasm.Compute/Runtime/codecs           # [WIRE]: XxHash128 seed-zero two-half digest vectors the parity readers assert
corpus/golden  ←  csharp:Rasm.Bim/Exchange/wire                # [WIRE]: BimWire/DiffWire/OpLogWire/IdsAudit golden-byte parity fixtures
```

`proof` is `plane:dev`: it imports anything and is imported by nothing — the one folder outside the runtime accounting. Every row above is read-only fixture consumption; no runtime edge originates or terminates here.
