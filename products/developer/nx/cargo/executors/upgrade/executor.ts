import { parse } from '@decimalturn/toml-patch';
import { dried, executor, spawned } from '@rasm/nx-distribution/inference';
import { Array, Cause, DateTime, Effect, FileSystem, Option, Order, Path, Record, Result, Schema, SchemaGetter, SemVer, String } from 'effect';
import { FetchHttpClient, HttpClient, HttpClientResponse } from 'effect/http';
import { MANIFEST } from '#/workspace';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REGISTRY = { source: 'registry+https://github.com/rust-lang/crates.io-index', index: 'https://index.crates.io' } as const;
const SHARDS: Readonly<Record<number, (name: string) => readonly string[]>> = { 1: () => ['1'], 2: () => ['2'], 3: (name) => ['3', name.slice(0, 1)] };
const COMMIT = /^g?[\da-f]{7,40}$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Upgrade = Schema.Struct({
    cwd: Schema.String.annotate({ description: 'Cargo workspace root, relative to the Nx workspace root' }),
    dryRun: Schema.optionalKey(Schema.Boolean).annotate({ description: 'Prints each move and makes none, passing --dry-run to cargo upgrade and cargo update, as NX_DRY_RUN=true does' }),
}).annotate({
    title: 'Upgrade',
    description:
        'Moves every dependency requirement of a Cargo workspace to its newest release with cargo upgrade --incompatible allow, then each crates.io requirement on a stable release to the newest prerelease above it with cargo upgrade --package <crate>@<version>, then the lock with cargo update; a prerelease counts where it names no commit build and its line opened after the newest stable line, as cargo upgrade follows prereleases only from a requirement already on one',
});
const Dependencies = Schema.fromJsonString(Schema.Struct({ packages: Schema.Array(Schema.Struct({ dependencies: Schema.Array(Schema.Struct({ name: Schema.String, req: Schema.String, source: Schema.NullOr(Schema.String) })) })) }));
const Row = Schema.Struct({ version: Schema.optionalKey(Schema.String), package: Schema.optionalKey(Schema.String), path: Schema.optionalKey(Schema.String), git: Schema.optionalKey(Schema.String), registry: Schema.optionalKey(Schema.String) });
const Shared = Schema.Struct({
    workspace: Schema.Struct({
        dependencies: Schema.Record(Schema.String, Schema.Union([Schema.String.pipe(Schema.decodeTo(Row, { decode: SchemaGetter.transform((version) => ({ version })), encode: SchemaGetter.transform((row) => row.version ?? '*') })), Row])).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
    }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ dependencies: {} }))),
});
class Release extends Schema.Opaque<Release, unknown>()(Schema.fromJsonString(Schema.Struct({ vers: Schema.SemVerFromString, yanked: Schema.Boolean, pubtime: Schema.DateTimeUtcFromString }))) {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const byVersion = Order.mapInput(SemVer.Order, ({ vers }: Release) => vers);

const line = ({ major, minor, patch }: SemVer.SemVer) => {
    const parts = [major, minor, patch];
    return parts.slice(0, Math.min(Array.takeWhile(parts, (part) => part === 0).length + 1, parts.length)).join('.');
};

const newest = (releases: readonly Release[]) => {
    const listed = releases.filter(({ yanked }) => !yanked);
    const stable = listed.filter(({ vers }) => !SemVer.isPrerelease(vers));
    const opened = Option.map(Array.match(stable, { onEmpty: Option.none, onNonEmpty: (held) => Option.some(Array.max(held, byVersion)) }), (top) =>
        Array.min(
            Array.map(
                Array.prepend(
                    stable.filter(({ vers }) => line(vers) === line(top.vers)),
                    top,
                ),
                ({ pubtime }) => pubtime,
            ),
            DateTime.Order,
        ),
    );
    const offered = listed.filter(({ vers, pubtime }) => !SemVer.isPrerelease(vers) || (!vers.prerelease.some((id) => COMMIT.test(`${id}`)) && Option.match(opened, { onNone: () => true, onSome: (at) => DateTime.isGreaterThan(pubtime, at) })));
    return Option.map(Array.match(offered, { onEmpty: Option.none, onNonEmpty: (held) => Option.some(Array.max(held, byVersion)) }), ({ vers }) => vers);
};

const indexed = Effect.fnUntraced(function* (name: string) {
    const lower = name.toLowerCase();
    const shard = SHARDS[lower.length]?.(lower) ?? [lower.slice(0, 2), lower.slice(2, 4)];
    const response = yield* Effect.flatMap(HttpClient.get([REGISTRY.index, ...shard, lower].join('/')), HttpClientResponse.filterStatusOk);
    return Array.getSomes(String.split(yield* response.text, '\n').map((row) => Schema.decodeOption(Release)(row)));
}, Effect.provide(FetchHttpClient.layer));

// --- [COMPOSITION] ---------------------------------------------------------------------

const execute = executor(Upgrade, function* ({ cwd: root, dryRun }, context) {
    const [path, fs, dry] = yield* Effect.all([Path.Path, FileSystem.FileSystem, dried(dryRun)]);
    const words = dry ? ['--dry-run'] : [];
    const cwd = path.resolve(context.root, root);
    const manifest = path.join(cwd, MANIFEST);
    const shell = { cwd, stdin: 'inherit', stdout: 'inherit', stderr: 'inherit' } as const;
    yield* spawned(['cargo', 'upgrade', '--incompatible', 'allow', ...words], shell);
    const [{ packages }, { workspace }] = yield* Effect.all([
        Effect.flatMap(spawned(['cargo', 'metadata', '--format-version', '1', '--no-deps'], { cwd }), Schema.decodeEffect(Dependencies)),
        fs.readFile(manifest).pipe(
            Effect.flatMap((bytes) => Effect.try({ try: (): unknown => parse(bytes), catch: (cause) => new Cause.UnknownError(cause, `${manifest} does not read as TOML`) })),
            Effect.flatMap(Schema.decodeUnknownEffect(Shared)),
        ),
    ]);
    const shared = Record.filterMap(workspace.dependencies, (row, key) => (row.version === undefined || row.path !== undefined || row.git !== undefined || row.registry !== undefined ? Result.failVoid : Result.succeed({ name: row.package ?? key, req: row.version })));
    const required = Array.groupBy([...packages.flatMap(({ dependencies }) => dependencies.filter(({ source }) => source === REGISTRY.source)), ...Object.values(shared)], ({ name }) => name);
    const stable = Record.keys(Record.filter(required, (rows) => rows.every(({ req }) => !req.includes('-') && !req.startsWith('='))));
    const found = yield* Effect.forEach(stable, (name) => Effect.map(indexed(name), newest), { concurrency: 'unbounded' });
    const specs = Array.getSomes(Array.zipWith(stable, found, (name, version) => Option.map(Option.filter(version, SemVer.isPrerelease), (held) => `${name}@${SemVer.format(held)}`)));
    yield* Array.match(specs, {
        onEmpty: () => Effect.void,
        onNonEmpty: (selected) => spawned(['cargo', 'upgrade', '--incompatible', 'allow', ...selected.flatMap((spec) => ['--package', spec]), ...words], shell),
    });
    yield* spawned(['cargo', 'update', ...words], shell);
    return { success: true };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { execute, Upgrade };
