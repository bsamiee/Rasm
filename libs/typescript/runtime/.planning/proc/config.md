# [RUNTIME_CONFIG]

The one config owner of the process plane: an ordered provider chain answers every `Config` read, and one boot-validated `Setting` contract resolves against it exactly once. A source is a case of one closed `Stage` family — process env (where `doppler run` injection lands), dotenv file, K8s file tree, remote document, literal table — folded left through `ConfigProvider.orElse` into one provider installed once beneath the whole graph, so precedence is tuple order and the empty chain is unspellable. Construction faults keep their channel: only a dotenv file's verified absence (`SystemError` with `reason: "NotFound"`) folds to a skipped stage, and every other construction failure rides the layer's typed error channel to the root proof. `Setting` is the runtime folder's environment contract, the config-family form every folder and app instantiates, and the seat where the supplied `Profile` consumption row admits: described rows, structural parsers, `Schema.Config` shaped scalars, sealed secrets, `Config.nested` namespaces, the whole record resolved at Layer construction so a malformed environment fails the root's wiring proof at the boot line. A scattered `process.env` read, a per-site `Config.string`, a second resolve, a second `setConfigProvider` altitude, and a blanket construction-fault-to-absence fold are the named defects. The module is `runtime/src/proc/config.ts`.

## [01]-[INDEX]

- [02]-[STAGE_FAMILY]: the closed source vocabulary and the doppler-injection law; `Provider`.
- [03]-[CHAIN_FOLD]: the orElse fold, skip-versus-fail construction, the one install site; `Provider`.
- [04]-[SETTING_OWNER]: the boot-validated runtime contract and the config-family form; `Setting`.
- [05]-[ADMISSION_ROWS]: the row vocabulary, the six-axis consumption profile the root supplies, and the topology-keyed durability table; `Profile`.

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
import { Array, ConfigProvider, Data, Effect, Layer, Option, type ParseResult, Record, Schema, pipe } from 'effect';
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
- Owner: `Setting` — the runtime environment contract: one `Effect.Service` class, `effect: Config.unwrap(record)`, the runtime-owned groups nested under the `RUNTIME` namespace and the deploy-owned backend file pair read at their exact `RASM_BACKEND_*` names; `Config` is a subtype of `Effect`, so the record is the constructor, `Setting.Default` resolves the whole environment at Layer construction, its `ConfigError` rides the Default layer's error channel, and the root annotation `Layer.Layer<Out>` is where an unset or malformed variable fails — one line, before any run seam.
- Law: consumers depend on `Setting`, never on `Config` — the built service is a plain resolved struct, so the `flag`, `life`, and `pubsub` owners read fields with no `ConfigError` in their own channels and no second resolve anywhere in the process.
- Law: the backend file pair is optional as a pair because backendless roots are valid; if either exact deploy variable is present, `Life` registers the projected-contract readiness row, and a partial pair is typed failing readiness rather than an unarmed probe or a guessed path. These names are the deployment ABI across Kubernetes, Docker, and Fargate, not a second `RUNTIME` vocabulary an app must translate.
- Law: the form is the family — a sibling or app contract instantiates this exact shape (`Config.unwrap` record, described rows, nested groups) under its own namespace; the branch ruling owns the custody claim.
- Law: a group is the growth site — a new runtime row lands inside its owning group, a new consuming sub-domain lands as one `Config.nested` group; neither adds an export, a service, or a resolve site; substitution is provider material — a proof overrides rows by swapping the chain, never by a second `Setting`; the `OTEL` group homes the export transport rows and the profiling backend rows.
- Law: the deploy-to-process seam has one named writing counterpart — `iac`'s `StackOutputs.channels` (`iac/.planning/program/spec.md`) is the total `<plane>.<field>`-to-variable catalog populating the deployed environment this contract resolves against, so `FANOUT.ORIGIN` reads what the `fanout.origin` channel writes and `OTEL.ORIGIN` what `otlp.endpoint` writes, and neither side restates the other's spelling; a row here needing a stack-realized value obligates its channel row at that owner in the same pass, and a realized plane consumed outside this contract (the `sharding` pair, which `@effect/cluster`'s own `layerFromEnv` reads) never mints a `Setting` group to shadow it.
- Law: `otel/emit`'s `Export.Policy` reads collector origin, sealed headers, cadence, transport deadline and concurrency, sampling ratio, diagnostic floor, and baggage promotion prefixes from `Setting.otel` — every axis a fleet retunes without a rebuild.
- Law: structural tuning stays policy default — temporality, histogram sizing, cardinality budgets, span and log limits, redaction rules, placement, engine health, and instrumentation postures never enter the environment.
- Law: the diagnostic row crosses as a name, so `otel/emit` owns the one total map onto `DiagLogLevel` and no numeric level reaches an environment.
- Law: `otel/profile`'s `Profile.Policy` reads the optional Pyroscope origin, the credential tag, and its sealed material from `Setting.otel`; an absent origin leaves the lane unarmed.
- Law: the credential tag selects the `Basic` or `Token` arm at the root, so a bearer field never stands in for a closed family.
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
        transport: Config.literal('smtp', 'json', 'stream', 'ethereal')('TRANSPORT').pipe(
            Config.withDefault('smtp' as const),
            Config.withDescription('mail sink row deliver#MAIL_ROW keys its transport table on; capture sinks open no socket'),
        ),
        host: Config.string('HOST').pipe(Config.withDescription('SMTP host the pooled transporter dials; smtp arm alone consumes it')),
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
        timeout: Config.duration('TIMEOUT').pipe(
            Config.withDefault(Duration.seconds(10)),
            Config.withDescription('per-request export deadline every signal exporter and the metric reader ride'),
        ),
        concurrency: Config.integer('CONCURRENCY').pipe(
            Config.withDefault(4),
            Config.validate({
                message: 'RUNTIME.OTEL.CONCURRENCY must exceed zero',
                validation: (value) => value > 0,
            }),
            Config.withDescription('in-flight export requests each signal exporter admits'),
        ),
        diagnostic: Config.literal('none', 'error', 'warn', 'info', 'debug', 'verbose', 'all')('DIAGNOSTIC').pipe(
            Config.withDefault('error'),
            Config.withDescription('SDK diagnostic floor; otel/emit maps this roster onto DiagLogLevel at its lane bracket'),
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
        profileAuth: Config.literal('basic', 'token')('PROFILE_AUTH').pipe(
            Config.withDefault('token'),
            Config.withDescription('Pyroscope credential shape selecting the Basic or Token arm of Profile.Credential'),
        ),
        profileSecret: Config.redacted('PROFILE_SECRET').pipe(
            Config.withDefault(Redacted.make('')),
            Config.withDescription('Pyroscope credential secret; sealed Redacted to the profiler init'),
        ),
        profileUser: Config.string('PROFILE_USER').pipe(
            Config.withDefault(''),
            Config.withDescription('Pyroscope basic-auth user; the token arm reads it never'),
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

const _backend = Config.unwrap({
    contractRoot: Config.option(Config.string('RASM_BACKEND_CONTRACT_ROOT')).pipe(
        Config.withDescription('root containing the mounted data-owned backend contract document'),
    ),
    pointerPath: Config.option(Config.string('RASM_BACKEND_POINTER_PATH')).pipe(
        Config.withDescription('mounted generation pointer compared with the projected backend contract identity'),
    ),
});

class Setting extends Effect.Service<Setting>()('runtime/Setting', {
    // Config is a subtype of Effect, so the axis gate rides the SAME construction the record resolves
    // in: a refused axis value and a malformed variable both fail the Default layer at one boot line.
    effect: Effect.flatMap(
        Config.unwrap({
            runtime: Config.nested(
                Config.unwrap({ cluster: _cluster, fanout: _fanout, flag: _flag, life: _life, mail: _mail, otel: _otel, profile: _profile, serve: _serve }),
                'RUNTIME',
            ),
            backend: _backend,
        }),
        ({ backend, runtime }) => Effect.map(Profile.admit(runtime.profile.row), () => ({ ...runtime, backend })),
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
- Owner: `Profile` — the native runtime consumption row a composition root supplies on `RUNTIME.PROFILE.ROW`. `topology`, `lifecycle`, and `isolation` are branch-domain literals, `tenancy` reads `Identity.tenancy`, and the richer `host` and `providers` descriptors remain local to admission and recovery. `_crossing` beside `_TOPOLOGY_RECOVERY` map over the domain unions, so a new axis value fails at its owning table.
- Law: both open-axis descriptors answer the consumption coordinates on the row, and the two coordinates no row in either family varies on ride this sentence instead — a host row states no tenancy mechanism because one host instance runs one profile and every tenant boundary inside it belongs to the data plane the profile scopes, and a provider row states no admit member because every provider enters through the declared port Tag the composition root binds, with `supplies` naming which port; `degrade` states the forfeit no capability cell already carries, so a host row reads its surface and lane cells for what they foreclose and a provider row reads the isolation values `_crossing` gates behind its supply.
- Law: `recovery` is a HOST-family extension column and the provider family forecloses it — a host integration decides where the store lands and therefore what a restore may lose, while a bound port supplies capability and decides no durability window, so a provider row answering one answers by guess; `degrade` cannot carry the pair either, naming forfeits rather than declared targets, and `lifetime` bounds what entered rather than what a failure costs.
- Law: durability resolves once — a host row's `recovery` overrides its topology's `_TOPOLOGY_RECOVERY` entry and an unhosted row reads the table, so `iac/program/spec` spreads `Profile.recoveryOf` rather than restating windows and the deploy plane threads the same pair the booted process carries; a second table at either plane grades a deployment against a target nobody declared.
- Law: `recovery` remains runtime policy on the native profile; no peer contract restates any profile axis.
- Law: admission ACCUMULATES — the host-descriptor axis and the isolation-crossing axis decide nothing about each other, so both columns run and every offender rides one census; a first-failure ladder hid the second refusal behind the first and cost one boot per axis to discover a profile wrong on both.
- Law: deployment shape is data the root states, never a fact the branch infers — an ambient `process.platform` read, a build flag, a bundler condition, and a branch on which product embeds the runtime are the four deleted forms; `Profile.admit` runs inside `Setting`'s own effect, so an unservable axis value fails the boot line beside every `ConfigError` and the graph never half-builds.
- Law: this branch answers `in-proc` on the Effect fiber runtime, `thread` through `proc/worker`'s pool, `process` through `proc/exec`'s subprocess spec behind a bound `local-spawn` provider, and `remote` through `net/client` behind a bound `remote-compute` provider; `wasm` refuses outright because the axis names where THIS branch's own work runs and no packaging compiles it into a guest — an embedded wasm-built engine is a dependency's own implementation, selected by `topology` and realizing no isolation value — and the worker pool nearest it gives thread isolation alone.
- Law: `Profile`, its descriptor schemas, and `_profile` seat above the `Setting` region of `runtime/src/proc/config.ts` — the fences split by cluster, never by file order, so `Setting` composes them as one module's earlier declarations.
- Entry: `Profile.admit(row)` at `Setting` construction; `yield* Setting` then reads `profile.row` everywhere else.
- Receipt: the admitted `Profile` value itself is the local deployment receipt.
- Packages: `effect` (`Array`, `Config`, `Duration`, `Effect`, `Option`, `Record`, `Schema`); `@rasm/core` (`Fault.Class`, `Identity`).

```typescript signature
import { Fault, Identity } from '@rasm/core';

const _topologies = ['in-host', 'sidecar', 'companion', 'service', 'edge', 'cli'] as const;
const _lifecycles = ['caller-owned', 'package-owned'] as const;
const _isolations = ['in-proc', 'thread', 'process', 'wasm', 'remote'] as const;
const _capabilities = ['host-document', 'local-spawn', 'remote-compute', 'store-read', 'store-write', 'telemetry-export'] as const;
// Who ends what a descriptor row admitted. Both families share the roster because a reader comparing a host row
// against a provider row compares one coordinate, and a second spelling forks it.
const _ends = ['package', 'host', 'deploy'] as const;

declare namespace Consumption {
    type Topologies = typeof _topologies;
    type Topology = (typeof _topologies)[number];
    type Lifecycle = (typeof _lifecycles)[number];
    type Isolation = (typeof _isolations)[number];
    type Capability = (typeof _capabilities)[number];
    type Host = Schema.Schema.Type<typeof _Host>;
    type Issue = typeof _refusal.payload.Type;
    type Objective = Schema.Schema.Type<typeof _Objective>;
    type Provider = Schema.Schema.Type<typeof _Provider>;
    type Refused = InstanceType<typeof ProfileRefused>;
}

// DECLARED durability window: how much data a restore may lose, and how long it may take. It rides the profile row
// as supplied data, so a package grades against the target its deployment set rather than one it invented.
// `data`'s `Backend.Objective` is this same pair one stratum below, and the two unify STRUCTURALLY rather than by
// import, because an S2 folder cannot reach an S3 schema — the grader takes the resolved value with no adapter.
const _Objective = Schema.Struct({ rpo: Schema.Duration, rto: Schema.Duration });

// Deployment CLASS decides the window, so six topology values answer as three: a desktop shape holds one operator's
// own store, an attached shape trails a service it does not own, and a fleet shape carries the estate's data. The
// mapped type is the enforcement — a domain key added to `_topologies` breaks this literal at compile time,
// exactly as `_crossing` breaks. This table is the branch's ONE durability source: `iac/program/spec` reads it through
// `Profile.recoveryOf` so the deploy plane and the process it deploys grade against one window.
const _TOPOLOGY_RECOVERY: { readonly [K in Consumption.Topology]: Consumption.Objective } = {
    'in-host': { rpo: Duration.minutes(15), rto: Duration.minutes(60) },
    sidecar: { rpo: Duration.minutes(5), rto: Duration.minutes(30) },
    companion: { rpo: Duration.minutes(5), rto: Duration.minutes(30) },
    service: { rpo: Duration.minutes(1), rto: Duration.minutes(15) },
    edge: { rpo: Duration.minutes(1), rto: Duration.minutes(15) },
    cli: { rpo: Duration.minutes(15), rto: Duration.minutes(60) },
};

// `document` is the foreclosure cell stated as data on every row: a host carrying no document says so here rather
// than omitting the field, so the fold reading a host row beside its siblings reads one shape.
const _Host = Schema.Struct({
    key: Schema.NonEmptyString,
    surface: Schema.Literal('embedded', 'windowed', 'offscreen', 'none'),
    lanes: _Extent,
    document: Schema.Boolean,
    fits: Schema.NonEmptyString,
    // Where the branch's work lands inside this host — a plug-in command, a page script, a process entry — and how
    // long it stays there under the owner that ends it.
    admit: Schema.NonEmptyString,
    lifetime: Schema.Struct({ bound: Schema.NonEmptyString, owner: Schema.Literal(..._ends) }),
    // Durability answers at the integration, which decides where the store lands — a plug-in host writes one
    // operator's local disk, a served root writes the estate's cluster. Every row states its window even where it
    // agrees with its topology, spelling `_TOPOLOGY_RECOVERY[<topology>]`, so no row answers by omission.
    recovery: _Objective,
    degrade: Schema.NonEmptyString,
});

// `supplies` decides every isolation value this row crosses through `_crossing`, so the crossing verdict is read
// once at admission and the row carries the forfeit alone.
const _Provider = Schema.Struct({
    key: Schema.NonEmptyString,
    supplies: Schema.Literal(..._capabilities),
    fits: Schema.NonEmptyString,
    tenancy: Schema.NonEmptyString,
    lifetime: Schema.Struct({ bound: Schema.NonEmptyString, owner: Schema.Literal(..._ends) }),
    degrade: Schema.NonEmptyString,
});

// Mapped over the isolation union, so adding a key to `_isolations` breaks this literal at compile
// time: 'served' runs unconditionally, 'unserved' refuses always, a capability gates on a bound row.
const _crossing: { readonly [K in Consumption.Isolation]: Consumption.Capability | 'served' | 'unserved' } = {
    'in-proc': 'served',
    thread: 'served',
    process: 'local-spawn',
    wasm: 'unserved',
    remote: 'remote-compute',
};

const _Isolation = Schema.Literal(..._isolations);
const _Lifecycle = Schema.Literal(..._lifecycles);
const _Topology = Schema.Literal(..._topologies);

const _LEG = 'profile';

// A free-string reason is unroutable and unfoldable, so the refusal grammar closes here: `reason` is the discriminant
// a caller dispatches on, and each row declares its OWN coordinates rather than sharing one free `value` beside an
// optional note — the gated crossing carries the capability it needed as a rostered word, and the unserved arm
// carries none because there is none to name. `class` projects the roster through one core family mint.
const _refusal = Fault.Class.family(['missing', 'uncrossed', 'unserved'] as const, {
    missing: Fault.Class.row({
        class: 'absent',
        leg: _LEG,
        detail: Schema.Struct({ topology: _Topology }),
        render: ({ topology }) => `topology ${topology} demands a host row this profile did not supply`,
    }),
    uncrossed: Fault.Class.row({
        class: 'absent',
        leg: _LEG,
        detail: Schema.Struct({ capability: Schema.Literal(..._capabilities), isolation: _Isolation }),
        render: ({ capability, isolation }) =>
            `isolation ${isolation} crosses only through ${capability}, which no provider row on this profile supplies`,
    }),
    unserved: Fault.Class.row({
        class: 'denied',
        leg: _LEG,
        detail: Schema.Struct({ isolation: _Isolation }),
        render: ({ isolation }) => `isolation ${isolation} has no serving path in this branch at all`,
    }),
});

// Both axes are admitted INDEPENDENTLY — a topology missing its host row decides nothing about an isolation value's
// crossing — so the carrier is the family's own census and every offender rides one refusal. A first-failure ladder
// reported the host gap and hid the crossing behind it, which cost one boot per axis to discover a profile that was
// wrong on both, and `class`, `leg`, and `message` all elect off the rank lattice with nothing declared here.
const ProfileRefused = _refusal.census('ProfileRefused');

// One column per admitted axis, each answering its own offender against the supplied row and nothing about its
// sibling. The crossing column orders its arms so the capability narrows on its own discriminant — the cast that
// re-asserted what `_crossing` already stated is gone with the ladder that needed it.
const _COLUMNS: { readonly [Axis in 'host' | 'isolation']: (row: Profile) => Option.Option<Consumption.Issue> } = {
    host: (row) =>
        row.topology === 'in-host' && Option.isNone(row.host)
            ? Option.some({ reason: 'missing', topology: row.topology } as const)
            : Option.none(),
    isolation: (row) =>
        pipe(_crossing[row.isolation], (crossing) =>
            crossing === 'unserved'
                ? Option.some({ reason: 'unserved', isolation: row.isolation } as const)
                : crossing === 'served' || row.grants.has(crossing)
                  ? Option.none()
                  : Option.some({ reason: 'uncrossed', capability: crossing, isolation: row.isolation } as const),
        ),
};

class Profile extends Schema.Class<Profile>('runtime/Profile')({
    tenancy: Schema.optionalWith(Identity.tenancy.schema, { default: () => 'single' as const }),
    topology: Schema.optionalWith(_Topology, { default: () => 'service' as const }),
    host: Schema.optionalWith(_Host, { as: 'Option' }),
    lifecycle: Schema.optionalWith(_Lifecycle, { default: () => 'package-owned' as const }),
    isolation: Schema.optionalWith(_Isolation, { default: () => 'thread' as const }),
    providers: Schema.optionalWith(Schema.Array(_Provider), { default: () => [] }),
}) {
    // Peers spread this roster into their own literals instead of re-declaring the axis, so the branch
    // carries one topology spelling and a new value breaks every consumer at compile time.
    static readonly topologies: Consumption.Topologies = _topologies;

    get grants(): ReadonlySet<Consumption.Capability> {
        return new Set(this.providers.map((row) => row.supplies));
    }

    // Host rows override their topology's window and an unhosted row reads the table, so one resolution serves both
    // this boot line and the deploy plane, and no runner grades a measured window against a target this process
    // never carried. Runtime recovery policy does not enter the parity wire.
    get recovery(): Consumption.Objective {
        return Option.match(this.host, { onNone: () => _TOPOLOGY_RECOVERY[this.topology], onSome: (row) => row.recovery });
    }

    // Peers resolve a topology's declared window without re-declaring the table, the same spread that keeps one
    // topology spelling branch-wide.
    static readonly recoveryOf = (topology: Consumption.Topology): Consumption.Objective => _TOPOLOGY_RECOVERY[topology];

    static readonly admit = (row: Profile): Effect.Effect<Profile, Consumption.Refused> =>
        Array.match(Array.getSomes(Array.map(Record.values(_COLUMNS), (column) => column(row))), {
            onEmpty: () => Effect.succeed(row),
            onNonEmpty: (issues) => Effect.fail(new ProfileRefused({ issues })),
        });
}

const _profile = Config.nested(
    Config.unwrap({
        row: Schema.Config('ROW', Schema.parseJson(Profile)).pipe(
            Config.withDefault(Profile.make({})),
            Config.withDescription('consumption profile row the composition root supplies'),
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
