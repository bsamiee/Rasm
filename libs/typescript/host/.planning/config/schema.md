# [HOST_SCHEMA]

Configuration is one typed contract resolved exactly once. Every environment fact a host process reads is a described `Config` row inside one `Config.unwrap` record — admitted through the full Schema algebra where the value has shape, sealed `Redacted` where it is secret, namespaced through `Config.nested`, defaulted at the row where the owner fixes the fallback — and the whole record resolves at Layer construction, so a malformed environment fails the root's wiring proof at the boot line, never the first read. This page owns two things: the config-family form every folder and app instantiates, and `Setting`, the host folder's own environment contract carrying the rows the `flag` and `life` owners read. The provider chain answering these reads is `provider.md`'s. A scattered `process.env` read, a per-site `Config.string`, a raw scalar where a brand exists, and any validation past the boot resolve are the named defects.

## [1]-[INDEX]

- [01]-[SETTING_OWNER]: the boot-validated config service — one `Effect.Service` class whose `effect` knob is the `Config.unwrap` record, resolved once at construction.
- [02]-[ADMISSION_ROWS]: the row vocabulary — structural parsers, Schema-shaped scalars, secrets, namespaces, defaults, descriptions.

## [2]-[SETTING_OWNER]

- Owner: `Setting` — the host environment contract. One `Effect.Service` class, `effect: Config.unwrap(record)`, the record nested under the `HOST` namespace with one group per consuming sub-domain (`FLAG`, `LIFE`). `Config` is a subtype of `Effect`, so the record is the constructor: `Setting.Default` resolves the whole environment at Layer construction, its `ConfigError` rides the Default layer's error channel, and the root annotation `Layer.Layer<Out>` is where an unset or malformed variable fails — one line, before any run seam.
- Law: consumers depend on `Setting`, never on `Config` — the built service is a plain resolved struct, so `flag` and `life` owners read fields with no `ConfigError` in their own channels and no second resolve anywhere in the process.
- Law: the form is the family — an app or sibling-folder config contract is declared exactly as `Setting` is (service class, `Config.unwrap` record, described rows, nested groups) under its own namespace; a second config-reading pattern beside this form is the fork this page exists to prevent, and two services never read one variable.
- Law: a group is the growth site — a new host row lands inside its owning group and a new consuming sub-domain lands as one `Config.nested` group; neither adds an export, a service, or a resolve site.
- Boundary: substitution is provider material — a proof or an app overrides rows by swapping the provider chain (`provider.md`), never by a second `Setting`; a `ConfigProvider.fromMap` harness pin composes under `Setting.Default` untouched.
- Entry: `Setting.Default` at the composition root; `yield* Setting` everywhere else.
- Packages: `effect` (`Config`, `Effect.Service`, `Duration`).

```typescript
import { Config, Duration, Effect } from "effect"

const _flag = Config.nested(
  Config.unwrap({
    origin: Config.url("ORIGIN").pipe(
      Config.withDescription("flag provider base URL the verdict feed dials"),
    ),
    cadence: Config.duration("CADENCE").pipe(
      Config.withDefault(Duration.minutes(5)),
      Config.withDescription("backfill poll cadence while the live verdict feed is absent"),
    ),
    sticky: Config.duration("STICKY").pipe(
      Config.withDefault(Duration.hours(12)),
      Config.withDescription("stickiness lease a held variant survives across rule changes"),
    ),
  }),
  "FLAG",
)

const _life = Config.nested(
  Config.unwrap({
    drain: Config.duration("DRAIN").pipe(
      Config.withDefault(Duration.seconds(25)),
      Config.withDescription("total graceful-drain budget before hard interrupt"),
    ),
    probe: Config.duration("PROBE").pipe(
      Config.withDefault(Duration.seconds(4)),
      Config.withDescription("per-probe budget before a lapse verdict"),
    ),
    report: Config.duration("REPORT").pipe(
      Config.withDefault(Duration.seconds(2)),
      Config.withDescription("health report memo window between probe sweeps"),
    ),
  }),
  "LIFE",
)

class Setting extends Effect.Service<Setting>()("host/Setting", {
  effect: Config.nested(Config.unwrap({ flag: _flag, life: _life }), "HOST"),
}) {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Setting }
```

## [3]-[ADMISSION_ROWS]

- Owner: the row vocabulary — which `Config` member admits which environment fact. Selection is mechanical: structure parses at the row (`Config.url`, `Config.port`, `Config.duration`, `Config.integer`); a closed choice is `Config.literal(...keys)` spread from the owning vocabulary anchor so admission and dispatch read one table; a secret is `Config.redacted` and stays `Redacted` end to end; a scalar with real shape — brand, union, bounded numeric, transform — admits through `Schema.Config(name, shape)` with its `ParseError` folded into the same `ConfigError` rail; a brand an owner already carries lifts through `Config.branded`.
- Law: `Config.withDescription` rides every row — a missing or malformed variable reports its meaning, never a bare key name; the description is the row's operator contract with whoever sets the environment.
- Law: `Config.withDefault` states ownership of the fallback — default at the row when the owner fixes the value and no consumer distinguishes absent from defaulted; no default when an unset variable must fail the boot; a fallback repeated at read sites marks a default that belonged on the row.
- Law: shaped rows keep validation at the seam — a `Schema.Config` row arrives branded and bounded, so no regex check, range guard, or parse survives past the resolve; the branded scalar the row admits is the same refinement the owning Schema field carries — one refinement, two admission sites, zero drift.
- Boundary: `Config.string` survives only for a genuinely free-form value; a literal choice restated beside the vocabulary table that anchors it, a raw secret string, and a post-resolve validation pass are rejected on sight.
- Packages: `effect` (`Config`, `Schema.Config`, `Struct.keys`).

```typescript
import { Config, Schema, Struct } from "effect"

const _tiers = {
  dev: { verbose: true },
  prod: { verbose: false },
} as const

declare namespace _tiers {
  type Kind = keyof typeof _tiers
  type _Rows<T extends Record<Kind, { readonly verbose: boolean }> = typeof _tiers> = T
}

const _Extent = Schema.NumberFromString.pipe(Schema.int(), Schema.between(1, 64), Schema.brand("Extent"))

const _shaped = Config.unwrap({
  tier: Config.literal(...Struct.keys(_tiers))("TIER").pipe(
    Config.withDefault("prod"),
    Config.withDescription("deployment tier selecting the verbosity row"),
  ),
  extent: Schema.Config("EXTENT", _Extent).pipe(
    Config.withDescription("bounded worker extent; arrives branded, never re-proven"),
  ),
  token: Config.redacted("TOKEN").pipe(
    Config.withDescription("provider credential; sealed Redacted end to end"),
  ),
  bind: Config.port("PORT").pipe(
    Config.withDefault(8080),
    Config.withDescription("listen port the serve row binds"),
  ),
})

// --- [EXPORTS] --------------------------------------------------------------------------

export {}
```
