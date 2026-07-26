# [RUNTIME_CONFIG]

The one config owner of the process plane: an ordered provider chain answers every `Config` read, and one boot-validated `Setting` contract resolves against it exactly once. A source is a case of one closed `Stage` family — process env (where `doppler run` injection lands), dotenv file, K8s file tree, remote document, literal table — folded left through `ConfigProvider.orElse` into one provider installed once beneath the whole graph, so precedence is tuple order and the empty chain is unspellable. Construction faults keep their channel: only a dotenv file's verified absence (`SystemError` with `reason: "NotFound"`) folds to a skipped stage, and every other construction failure rides the layer's typed error channel to the root proof. `Setting` is the runtime folder's environment contract, the config-family form every folder and app instantiates, and the seat where the supplied `Profile` consumption row admits: described rows, structural parsers, `Schema.Config` shaped scalars, sealed secrets, `Config.nested` namespaces, the whole record resolved at Layer construction so a malformed environment fails the root's wiring proof at the boot line. A scattered `process.env` read, a per-site `Config.string`, a second resolve, a second `setConfigProvider` altitude, and a blanket construction-fault-to-absence fold are the named defects. The module is `runtime/src/proc/config.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                    | [PUBLIC]   |
| :-----: | :--------------- | :------------------------------------------------------------------------ | :--------- |
|  [01]   | `STAGE_FAMILY`   | the closed source vocabulary and the doppler-injection law                | `Provider` |
|  [02]   | `CHAIN_FOLD`     | the orElse fold, skip-versus-fail construction, the one install site      | `Provider` |
|  [03]   | `SETTING_OWNER`  | the boot-validated runtime contract and the config-family form            | `Setting`  |
|  [04]   | `ADMISSION_ROWS` | the row vocabulary and the six-axis consumption profile the root supplies | `Profile`  |

## [02]-[STAGE_FAMILY]

[STAGE_FAMILY]:
- Owner: `Provider.Stage` — one `Data.taggedEnum` family: `Env` (process environment), `DotEnv` carrying its file path, `Tree` carrying the mount root (the K8s secret-mount form, one file per key), `Remote` carrying the document origin, `Table` carrying a literal row map (the harness and inline-override form); constructors ride the exported `Provider` owner, so declaring a chain is one import.
- Law: doppler is env — `doppler run --` injects leased secrets as process environment before the runtime boots, so the doppler stage IS the `Env` stage's content and holds env precedence; the chain never dials Doppler at runtime, and the runtime leased-secret axis — TTL rotation, `Redacted` end to end — is `security`'s crypt owner.
- Law: the remote stage is a boot-time document — fetched once at chain construction through the `batch` egress lane (`net/client#DIAL_SEAM`), so the chain inherits the branch egress posture, decoded as one JSON document, served through `ConfigProvider.fromJson` under `ConfigProvider.constantCase` so a camelCase remote document answers CONSTANT_CASE reads; live re-evaluation is not a config concern — the one live remote surface is the flag feed (`flag#GATE_SERVICE`), never a mutating config chain.
- Law: a stage carries data, not behavior — path, root, origin, rows are case payloads; the build arm owns the mechanics, so a chain declaration reads as policy, and proof pins ride the same family: `Table` under `ConfigProvider.fromMap` composed at the head of a spec chain, never a second override mechanism.
- Growth: a new source is one case plus one build arm.
- Packages: `effect` (`ConfigProvider`, `Data`); `@effect/platform` (`PlatformConfigProvider`, `HttpClientRequest`); `../net/client.ts` (`Client`).

## [03]-[CHAIN_FOLD]

[CHAIN_FOLD]:
- Owner: `Provider.chain` — the one entry: a `NonEmptyReadonlyArray<Stage>` folds to the installing Layer; each stage builds effectfully, the fold is `Array.reduce` over `ConfigProvider.orElse` with the head as seed, so precedence is structural.
- Law: construction failure splits by stage nature, and only proven absence skips — the `DotEnv` stage reads its file eagerly, so it folds exactly the `SystemError` whose `reason` is `"NotFound"` to a skipped stage through `Effect.catchIf` (a dev-only file legitimately does not exist in prod) while an unreadable or permission-denied file stays a `PlatformError` on the channel; the `Tree` stage is construction-total — the file-tree provider reads per key, so an absent mount answers missing data at read time and `orElse` falls through to the next stage; the `Remote` stage was declared to be answered, so its fetch or decode failure rides the layer's error channel and fails the boot at the root proof; `Env` and `Table` are total; a chain whose every skippable stage skipped folds to the surviving stages — and an all-skipped chain folds to the empty provider, every read failing as missing data at the boot proof, never a forged fallback the chain never declared.
- Law: one install site — the returned `Layer<never>` merges once beneath the root; a second `setConfigProvider` at a deeper altitude shadows the root's chain and is the named defect; requirements (`FileSystem`, `Path` for file stages; `HttpClient` for the remote dial) are satisfied by the runtime row's context and the shared client, so the chain layer composes after the platform layer and before every config-reading service.
- Entry: `Provider.chain(stages)` at the app root; nothing else in the branch touches `ConfigProvider`.
- Receipt: the layer's stated annotation is the chain's contract — fault union and requirement set readable at the root without opening the fold.
- Packages: `effect` (`Array`, `Effect`, `Layer`, `Option`, `Schema`).

```typescript signature
import {
    type FileSystem,
    type HttpClient,
    type HttpClientError,
    HttpClientRequest,
    type Path,
    PlatformConfigProvider,
    type PlatformError,
} from '@effect/platform';
import { Array, ConfigProvider, Data, Effect, Layer, Option, type ParseResult, Schema, pipe } from 'effect';
import { Client, type Lapse } from '../net/client.ts';

declare namespace Provider {
    type Stage = Data.TaggedEnum<{
        Env: {};
        DotEnv: { readonly path: string };
        Tree: { readonly root: string };
        Remote: { readonly origin: URL };
        Table: { readonly rows: ReadonlyMap<string, string> };
    }>;
    type Faults = HttpClientError.HttpClientError | Lapse | ParseResult.ParseError | PlatformError.PlatformError;
    type Needs = FileSystem.FileSystem | HttpClient.HttpClient | Path.Path;
}

const _Stage = Data.taggedEnum<Provider.Stage>();

const _fetched = (origin: URL): Effect.Effect<ConfigProvider.ConfigProvider, Provider.Faults, HttpClient.HttpClient> =>
    Effect.map(Client.dial('batch', HttpClientRequest.get(origin.href), Schema.Unknown), (document) =>
        ConfigProvider.constantCase(ConfigProvider.fromJson(document)),
    );

const _built = (stage: Provider.Stage): Effect.Effect<Option.Option<ConfigProvider.ConfigProvider>, Provider.Faults, Provider.Needs> =>
    _Stage.$match(stage, {
        Env: () => Effect.succeed(Option.some(ConfigProvider.fromEnv())),
        DotEnv: ({ path }) =>
            PlatformConfigProvider.fromDotEnv(path).pipe(
                Effect.map(Option.some),
                // only proven absence skips: every other PlatformError stays on the chain's typed channel
                Effect.catchIf(
                    (fault) => fault._tag === 'SystemError' && fault.reason === 'NotFound',
                    () => Effect.succeedNone,
                ),
            ),
        Tree: ({ root }) => Effect.map(PlatformConfigProvider.fromFileTree({ rootDirectory: root }), Option.some),
        Remote: ({ origin }) => Effect.map(_fetched(origin), Option.some),
        Table: ({ rows }) => Effect.succeed(Option.some(ConfigProvider.fromMap(rows))),
    });

const _folded = (
    stages: Array.NonEmptyReadonlyArray<Provider.Stage>,
): Effect.Effect<ConfigProvider.ConfigProvider, Provider.Faults, Provider.Needs> =>
    Effect.map(Effect.forEach(stages, _built, { concurrency: 'inherit' }), (built) =>
        pipe(Array.getSomes(built), (chain) =>
            Array.isNonEmptyReadonlyArray(chain)
                ? Array.reduce(Array.tailNonEmpty(chain), Array.headNonEmpty(chain), (acc, next) => ConfigProvider.orElse(acc, () => next))
                : ConfigProvider.fromMap(new Map()),
        ),
    );

const Provider: Data.TaggedEnum.Constructor<Provider.Stage> & {
    readonly chain: (stages: Array.NonEmptyReadonlyArray<Provider.Stage>) => Layer.Layer<never, Provider.Faults, Provider.Needs>;
} = {
    ..._Stage,
    chain: (stages) => Layer.unwrapEffect(Effect.map(_folded(stages), Layer.setConfigProvider)),
};
```

## [04]-[SETTING_OWNER]

[SETTING_OWNER]:
- Owner: `Setting` — the runtime environment contract: one `Effect.Service` class, `effect: Config.unwrap(record)`, the record nested under the `RUNTIME` namespace with one group per consuming sub-domain (`CLUSTER`, `FANOUT`, `FLAG`, `LIFE`, `MAIL`, `OTEL`, `SERVE`); `Config` is a subtype of `Effect`, so the record is the constructor, `Setting.Default` resolves the whole environment at Layer construction, its `ConfigError` rides the Default layer's error channel, and the root annotation `Layer.Layer<Out>` is where an unset or malformed variable fails — one line, before any run seam.
- Law: consumers depend on `Setting`, never on `Config` — the built service is a plain resolved struct, so the `flag`, `life`, and `pubsub` owners read fields with no `ConfigError` in their own channels and no second resolve anywhere in the process.
- Law: the form is the family — an app or sibling-folder contract is declared exactly as `Setting` is (service class, `Config.unwrap` record, described rows, nested groups) under its own namespace; a second config-reading pattern beside this form is the fork this page exists to prevent, and two services never read one variable.
- Law: a group is the growth site — a new runtime row lands inside its owning group, a new consuming sub-domain lands as one `Config.nested` group; neither adds an export, a service, or a resolve site; substitution is provider material — a proof overrides rows by swapping the chain, never by a second `Setting`; the `OTEL` group homes the export transport rows (`otel/emit`'s `Export.Policy` reads its collector origin, sealed headers, cadence, sampling ratio, and baggage promotion prefixes from `Setting.otel`, keeping structural tuning — temporality, span limits, redaction rules, placement, engine vitals — as its own policy defaults) and the profiling backend rows (`otel/profile`'s `Profile.Policy` reads the optional Pyroscope origin and sealed credential from `Setting.otel.profile`, an absent origin leaving the lane unarmed).
- Law: `Setting.tiers` rides the class as the tier-table static — `otel/meter` projects its `verbose` column into `Logger.minimumLogLevel`, so the tier row governs the process log floor through one consumer and no page carries a level literal.
- Entry: `Setting.Default` at the composition root; `yield* Setting` everywhere else.
- Packages: `effect` (`Config`, `Duration`, `Effect`, `Schema`, `Struct`).

```typescript signature
import { Config, Duration, Effect, Redacted, Schema, Struct } from 'effect';

const _tiers = {
    dev: { verbose: true },
    prod: { verbose: false },
} as const;

declare namespace _tiers {
    type Kind = keyof typeof _tiers;
    type _Rows<T extends Record<Kind, { readonly verbose: boolean }> = typeof _tiers> = T;
}

const _Extent = Schema.NumberFromString.pipe(Schema.int(), Schema.between(1, 64), Schema.brand('Extent'));

const _flag = Config.nested(
    Config.unwrap({
        origin: Config.url('ORIGIN').pipe(Config.withDescription('flag provider base URL the verdict feed dials')),
        cadence: Config.duration('CADENCE').pipe(
            Config.withDefault(Duration.minutes(5)),
            Config.withDescription('reconnect pacing while the live verdict feed is absent'),
        ),
        sticky: Config.duration('STICKY').pipe(
            Config.withDefault(Duration.hours(12)),
            Config.withDescription('stickiness lease a held variant survives across rule changes'),
        ),
        quarantine: Config.duration('QUARANTINE').pipe(
            Config.withDefault(Duration.seconds(20)),
            Config.withDescription('degraded-verdict memo lease before the provider retries evaluation'),
        ),
        memo: Config.integer('MEMO').pipe(
            Config.withDefault(4096),
            Config.validate({ message: 'RUNTIME.FLAG.MEMO must be positive', validation: (value) => value > 0 }),
            Config.withDescription('process verdict-cache entry ceiling'),
        ),
    }),
    'FLAG',
);

const _life = Config.nested(
    Config.unwrap({
        drain: Config.duration('DRAIN').pipe(
            Config.withDefault(Duration.seconds(25)),
            Config.withDescription('total graceful-drain budget mirrored into terminationGracePeriod'),
        ),
        probe: Config.duration('PROBE').pipe(
            Config.withDefault(Duration.seconds(4)),
            Config.withDescription('per-row budget before a lapse verdict in the ranked fold'),
        ),
        report: Config.duration('REPORT').pipe(
            Config.withDefault(Duration.seconds(2)),
            Config.withDescription('health report memo window between probe sweeps'),
        ),
    }),
    'LIFE',
);

const _fanout = Config.nested(
    Config.unwrap({
        origin: Config.url('ORIGIN').pipe(Config.withDescription('NATS origin the jetstream engine row dials through the runtime nats binding')),
        brokers: Config.array(Config.string(), 'BROKERS').pipe(
            Config.withDefault([]),
            Config.withDescription('Kafka bootstrap brokers the kafka engine row dials; empty leaves the row unarmed'),
        ),
        registry: Config.option(Config.url('REGISTRY')).pipe(
            Config.withDescription('Schema Registry origin the Kafka contract row requires'),
        ),
        dedup: Config.duration('DEDUP').pipe(
            Config.withDefault(Duration.minutes(2)),
            Config.withDescription('stream duplicate-detection window the msgID dedup rides'),
        ),
        chunk: Config.integer('CHUNK').pipe(
            Config.withDefault(131_072),
            Config.validate({ message: 'RUNTIME.FANOUT.CHUNK must be positive', validation: (value) => value > 0 }),
            Config.withDescription("object-row max chunk size the blob lane's chunked put rides"),
        ),
    }),
    'FANOUT',
);

const _cluster = Config.nested(
    Config.unwrap({
        lockRefresh: Config.duration('LOCK_REFRESH').pipe(
            Config.withDefault(Duration.seconds(20)),
            Config.withDescription('shard advisory-lock refresh interval the leaderless grid rides'),
        ),
        lockExpiry: Config.duration('LOCK_EXPIRY').pipe(
            Config.withDefault(Duration.minutes(1)),
            Config.withDescription('shard advisory-lock expiration bounding runner-death takeover'),
        ),
    }),
    'CLUSTER',
);

const _mail = Config.nested(
    Config.unwrap({
        host: Config.string('HOST').pipe(Config.withDescription('SMTP host the pooled transporter dials')),
        port: Config.port('PORT').pipe(Config.withDefault(465), Config.withDescription('SMTP port')),
        user: Config.string('USER').pipe(Config.withDescription('SMTP credential user')),
        pass: Config.redacted('PASS').pipe(Config.withDescription('SMTP credential; sealed Redacted to the transport seam')),
        domain: Config.string('DOMAIN').pipe(Config.withDescription('DKIM signing domain')),
        selector: Config.string('SELECTOR').pipe(Config.withDescription('DKIM key selector')),
        key: Config.redacted('KEY').pipe(Config.withDescription('DKIM signing key; sealed Redacted to the transport seam')),
        rate: Config.integer('RATE').pipe(Config.withDefault(60), Config.withDescription('pooled-transport messages-per-window ceiling')),
    }),
    'MAIL',
);

const _otel = Config.nested(
    Config.unwrap({
        origin: Config.url('ORIGIN').pipe(Config.withDescription('OTLP collector base URL the export lanes derive per-signal paths from')),
        headers: Config.redacted('HEADERS').pipe(
            Config.withDefault(Redacted.make('')),
            Config.withDescription('collector auth header value; sealed Redacted to the lane construction'),
        ),
        cadence: Config.duration('CADENCE').pipe(
            Config.withDefault(Duration.seconds(10)),
            Config.withDescription('per-signal export interval the batch processors ride'),
        ),
        sample: Config.number('SAMPLE').pipe(
            Config.withDefault(1),
            Config.validate({
                message: 'RUNTIME.OTEL.SAMPLE must be finite and inside [0,1]',
                validation: (value) => Number.isFinite(value) && value >= 0 && value <= 1,
            }),
            Config.withDescription('head-sampling ratio in [0,1] the trace lane applies at span start'),
        ),
        promote: Config.array(Config.string(), 'PROMOTE').pipe(
            Config.withDefault(['rasm.']),
            Config.withDescription('baggage key prefixes promoted onto span attributes; the tenant projection rides the rasm. prefix'),
        ),
        profile: Config.option(Config.url('PROFILE')).pipe(
            Config.withDescription('Pyroscope backend origin the profiling lane pushes to; absence leaves the lane unarmed'),
        ),
        profileToken: Config.redacted('PROFILE_TOKEN').pipe(
            Config.withDefault(Redacted.make('')),
            Config.withDescription('Pyroscope credential; sealed Redacted to the profiler init'),
        ),
    }),
    'OTEL',
);

const _serve = Config.nested(
    Config.unwrap({
        tier: Config.literal(...Struct.keys(_tiers))('TIER').pipe(
            Config.withDefault('prod'),
            Config.withDescription('deployment tier selecting the verbosity row'),
        ),
        extent: Schema.Config('EXTENT', _Extent).pipe(Config.withDescription('bounded worker-pool extent; arrives branded, never re-proven')),
        bind: Config.port('PORT').pipe(Config.withDefault(8080), Config.withDescription('listen port the serve row binds')),
    }),
    'SERVE',
);

class Setting extends Effect.Service<Setting>()('runtime/Setting', {
    // Config is a subtype of Effect, so the axis gate rides the SAME construction the record resolves
    // in: a refused axis value and a malformed variable both fail the Default layer at one boot line.
    effect: Effect.flatMap(
        Config.nested(
            Config.unwrap({ cluster: _cluster, fanout: _fanout, flag: _flag, life: _life, mail: _mail, otel: _otel, profile: _profile, serve: _serve }),
            'RUNTIME',
        ),
        (resolved) => Effect.map(Profile.admit(resolved.profile.row), () => resolved),
    ),
}) {
    static readonly tiers = _tiers;
}
```

## [05]-[ADMISSION_ROWS]

[ADMISSION_ROWS]:
- Owner: the row vocabulary — structure parses at the row (`Config.url`, `Config.port`, `Config.duration`, `Config.integer`), and semantic bounds remain at admission through `Config.validate` (`fanout.chunk > 0`, finite `otel.sample ∈ [0,1]`); a closed choice is `Config.literal(...keys)` spread from the owning vocabulary anchor, a secret is `Config.redacted`, and a scalar with richer shape admits through `Schema.Config(name, shape)` with its `ParseError` folded into the same `ConfigError` rail.
- Law: `Config.withDescription` rides every row — a missing or malformed variable reports its meaning, never a bare key name; the description is the row's operator contract with whoever sets the environment.
- Law: `Config.withDefault` states ownership of the fallback — default at the row when the owner fixes the value and no consumer distinguishes absent from defaulted; no default when an unset variable must fail the boot; a fallback repeated at read sites marks a default that belonged on the row.
- Law: shaped rows keep validation at the seam — a `Schema.Config` row arrives branded and bounded, so no regex check, range guard, or parse survives past the resolve; the branded scalar the row admits is the same refinement the owning Schema field carries — one refinement, two admission sites, zero drift; `Config.string` survives only for a genuinely free-form value.
- Law: the family form is proven by `Setting` itself — the `SERVE` group carries the vocabulary's every member (literal spread from the `_tiers` anchor, `Schema.Config` branded scalar, defaulted structural port) and the `MAIL` group carries the sealed-secret rows; a sibling contract instantiates the identical form under its own namespace, and a second demonstration service beside the real owner is the duplication this page deletes.
- Owner: `Profile` — the six-axis consumption row a composition root supplies as one canonical-json document on `RUNTIME.PROFILE.ROW`: `tenancy`, `topology`, `lifecycle`, and `isolation` are closed literal unions, `host` and `providers` carry descriptor structs whose rows this branch supplies, and `_crossing` is a mapped type over the isolation union so a new axis value fails the object literal at compile time instead of falling through at runtime. `Profile.topologies` and the `Consumption` type namespace publish those closed rosters branch-wide, so `iac/program/spec` spreads this spelling into `StackSpec` and the branch carries one topology vocabulary.
- Law: deployment shape is data the root states, never a fact the branch infers — an ambient `process.platform` read, a build flag, a bundler condition, and a branch on which product embeds the runtime are the four deleted forms; `Profile.admit` runs inside `Setting`'s own effect, so an unservable axis value fails the boot line beside every `ConfigError` and the graph never half-builds.
- Law: refusal names the axis — `ProfileRefused` carries `axis`, `value`, and `reason`, matching the deploy plane's own refusal grammar, so a caller reads which of the six coordinates to restate; silent degradation and a narrowed public surface are the two failed forms.
- Law: this branch answers `in-proc` on the Effect fiber runtime, `thread` through `proc/worker`'s pool, `process` through `proc/exec`'s subprocess spec behind a bound `local-spawn` provider, and `remote` through `net/client` behind a bound `remote-compute` provider; `wasm` refuses outright because no guest runtime hosts foreign bytecode here, and the worker pool nearest it gives thread isolation alone.
- Law: `Profile`, its descriptor schemas, and `_profile` seat above the `Setting` region of `runtime/src/proc/config.ts` — the fences split by cluster, never by file order, so `Setting` composes them as one module's earlier declarations.
- Entry: `Profile.admit(row)` at `Setting` construction; `yield* Setting` then reads `profile.row` everywhere else.
- Packages: `effect` (`Config`, `Data`, `Effect`, `Option`, `Schema`).

```typescript signature
const _tenancies = ['none', 'single', 'multi'] as const;
const _topologies = ['in-host', 'sidecar', 'companion', 'service', 'edge', 'cli'] as const;
const _lifecycles = ['caller-owned', 'package-owned'] as const;
const _isolations = ['in-proc', 'thread', 'process', 'wasm', 'remote'] as const;
const _axes = ['tenancy', 'topology', 'host', 'lifecycle', 'isolation', 'providers'] as const;
const _capabilities = ['host-document', 'local-spawn', 'remote-compute', 'store-read', 'store-write', 'telemetry-export'] as const;

declare namespace Consumption {
    type Tenancy = (typeof _tenancies)[number];
    type Topologies = typeof _topologies;
    type Topology = (typeof _topologies)[number];
    type Lifecycle = (typeof _lifecycles)[number];
    type Isolation = (typeof _isolations)[number];
    type Axis = (typeof _axes)[number];
    type Capability = (typeof _capabilities)[number];
    type Host = Schema.Schema.Type<typeof _Host>;
    type Provider = Schema.Schema.Type<typeof _Provider>;
}

const _Host = Schema.Struct({
    key: Schema.NonEmptyString,
    surface: Schema.Literal('embedded', 'windowed', 'offscreen', 'none'),
    lanes: _Extent,
    document: Schema.Boolean,
});

const _Provider = Schema.Struct({
    key: Schema.NonEmptyString,
    supplies: Schema.Literal(..._capabilities),
    reach: Schema.Literal(..._isolations),
});

// Mapped over the isolation union, so adding a value to _isolations breaks this literal at compile
// time: 'served' runs unconditionally, 'unserved' refuses always, a capability gates on a bound row.
const _crossing: { readonly [K in Consumption.Isolation]: Consumption.Capability | 'served' | 'unserved' } = {
    'in-proc': 'served',
    thread: 'served',
    process: 'local-spawn',
    wasm: 'unserved',
    remote: 'remote-compute',
};

class ProfileRefused extends Data.TaggedError('ProfileRefused')<{
    readonly axis: Consumption.Axis;
    readonly value: string;
    readonly reason: string;
}> {}

class Profile extends Schema.Class<Profile>('runtime/Profile')({
    tenancy: Schema.optionalWith(Schema.Literal(..._tenancies), { default: () => 'single' as const }),
    topology: Schema.optionalWith(Schema.Literal(..._topologies), { default: () => 'service' as const }),
    host: Schema.optionalWith(_Host, { as: 'Option' }),
    lifecycle: Schema.optionalWith(Schema.Literal(..._lifecycles), { default: () => 'package-owned' as const }),
    isolation: Schema.optionalWith(Schema.Literal(..._isolations), { default: () => 'thread' as const }),
    providers: Schema.optionalWith(Schema.Array(_Provider), { default: () => [] }),
}) {
    // Peers spread this roster into their own literals instead of re-declaring the axis, so the branch
    // carries one topology spelling and a new value breaks every consumer at compile time.
    static readonly topologies: Consumption.Topologies = _topologies;

    get grants(): ReadonlySet<Consumption.Capability> {
        return new Set(this.providers.map((row) => row.supplies));
    }

    get hostKey(): string {
        return Option.match(this.host, { onNone: () => 'none', onSome: (row) => row.key });
    }

    // Six rows in roster order under an ordinal provider-key sort: the canonical-json preimage the
    // corpus parity reads, so a provider array reordered at the root re-serializes byte-identically.
    get canonical(): readonly (readonly [Consumption.Axis, string])[] {
        return [
            ['tenancy', this.tenancy],
            ['topology', this.topology],
            ['host', this.hostKey],
            ['lifecycle', this.lifecycle],
            ['isolation', this.isolation],
            ['providers', [...this.providers.map((row) => row.key)].sort().join(',')],
        ];
    }

    static readonly admit = (row: Profile): Effect.Effect<Profile, ProfileRefused> =>
        row.topology === 'in-host' && Option.isNone(row.host)
            ? Effect.fail(new ProfileRefused({ axis: 'host', value: 'none', reason: 'in-host topology carries no host descriptor row' }))
            : _crossing[row.isolation] === 'served' || row.grants.has(_crossing[row.isolation] as Consumption.Capability)
              ? Effect.succeed(row)
              : Effect.fail(new ProfileRefused({ axis: 'isolation', value: row.isolation, reason: _crossing[row.isolation] }));
}

const _profile = Config.nested(
    Config.unwrap({
        row: Schema.Config('ROW', Schema.parseJson(Profile)).pipe(
            Config.withDefault(Profile.make({})),
            Config.withDescription('canonical-json consumption profile row the composition root supplies'),
        ),
    }),
    'PROFILE',
);
```

```typescript signature
// --- [EXPORTS] --------------------------------------------------------------------------

export { type Consumption, Profile, ProfileRefused, Provider, Setting };
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
