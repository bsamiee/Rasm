import { dirname, isAbsolute, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';
import { layer, type NodeServices } from '@effect/platform-node/NodeServices';
import { readJsonFile, type TargetConfiguration } from '@nx/devkit';
import type { PackageJson } from '@nx/devkit/internal';
import { Array, Cause, Context, Effect, FileSystem, Layer, ManagedRuntime, Option, Predicate, Record, Result, Schema, type Scope, Stream, String, UndefinedOr } from 'effect';
import { Toml } from 'effect/encoding';
import { ChildProcess, CommandLine } from 'effect/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Reading = Effect.Success<ReturnType<typeof read>>;
type Workspace = ReturnType<typeof resolved>;
type Member = Workspace['members'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const MANIFEST = 'Cargo.toml';
const LOCK = 'Cargo.lock';
const CRATE = 'nx_cargo';
const PLUGIN = readJsonFile<PackageJson>(fileURLToPath(import.meta.resolve('#package'))).name;
const DENY = Record.map({ config: 'deny', exceptions: 'deny.exceptions' }, (stem) => [`${stem}.toml`, `.${stem}.toml`, `.cargo/${stem}.toml`]);

// --- [MODELS] --------------------------------------------------------------------------

const Target = Schema.Struct({ kind: Schema.Array(Schema.String), crate_types: Schema.Array(Schema.String), name: Schema.String, src_path: Schema.String, test: Schema.Boolean, doctest: Schema.Boolean });
const Package = Schema.Struct({
    name: Schema.String,
    version: Schema.String,
    manifest_path: Schema.String,
    readme: Schema.NullOr(Schema.String),
    publish: Schema.NullOr(Schema.Array(Schema.String)),
    dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.optionalKey(Schema.String) })),
    targets: Schema.Array(Target),
    features: Schema.Record(Schema.String, Schema.Array(Schema.String)),
    metadata: Schema.NullOr(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ implicitDependencies: Schema.optionalKey(Schema.Array(Schema.String)) })) })),
});
const Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(Package),
        workspace_root: Schema.String,
        target_directory: Schema.String,
        metadata: Schema.NullOr(Schema.Struct({ dylint: Schema.optionalKey(Schema.Struct({ libraries: Schema.Array(Schema.Struct({ path: Schema.optionalKey(Schema.String) })) })) })),
    }),
);
const Table = Schema.Record(Schema.String, Schema.Unknown);
const Profiles = Schema.Struct({ profile: Schema.Record(Schema.String, Schema.Struct({ inherits: Schema.optionalKey(Schema.String), panic: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) });
const Input = Schema.declare((input: unknown): input is NonNullable<TargetConfiguration['inputs']>[number] => Predicate.isString(input) || Predicate.isObject(input));
const Overrides = Schema.fromJsonString(Schema.Struct({ nx: Schema.optionalKey(Schema.Struct({ targets: Schema.optionalKey(Schema.Record(Schema.String, Schema.Struct({ inputs: Schema.optionalKey(Schema.mutable(Schema.Array(Input))) }))) })) })).annotate({
    description: "The nx.targets rows of a project's package.json, whose inputs Nx merges over the inferred targets after every plugin",
});

// --- [SERVICES] ------------------------------------------------------------------------

class Links extends Context.Service<Links, Map<string, Member>>()(`${PLUGIN}/workspace/Links`) {
    static readonly layer = Layer.sync(Links, () => new Map());
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = (cause: Cause.Cause<unknown>) => Effect.fail(new Cause.UnknownError(cause, Cause.pretty(cause)));

const spawned = Effect.fnUntraced(function* (argv: Array.NonEmptyReadonlyArray<string>, options: ChildProcess.CommandOptions = {}) {
    const handle = yield* ChildProcess.make(Array.headNonEmpty(argv), Array.tailNonEmpty(argv), { stdin: 'ignore', ...options, extendEnv: true, detached: false });
    const [stdout, stderr, code] = yield* Effect.all([Stream.mkString(Stream.decodeText(handle.stdout)), Stream.mkString(Stream.decodeText(handle.stderr)), handle.exitCode], { concurrency: 'unbounded' });
    return code === 0 ? stdout : yield* new Cause.UnknownError(code, [`${CommandLine.display(argv)} exits ${code}`, ...[stderr.trim()].filter(String.isNonEmpty)].join(': '));
}, Effect.scoped);

const manifested = Effect.fnUntraced(function* (workspaceRoot: string, root: string) {
    const fs = yield* FileSystem.FileSystem;
    const manifest = join(workspaceRoot, root, 'package.json');
    return (yield* fs.exists(manifest)) ? ((yield* Effect.flatMap(fs.readFileString(manifest), Schema.decodeEffect(Overrides))).nx?.targets ?? {}) : {};
});

const ancestors = (folder: string): readonly string[] =>
    Array.unfold(
        Option.some(folder),
        Option.map((at: string) => [at, Option.liftPredicate(dirname(at), () => at !== '.')] as const),
    );

const nearest = (files: ReadonlySet<string>, folder: string, names: readonly string[]) => Array.findFirst(ancestors(folder), (at) => Array.findFirst(names, (name) => files.has(join(at, name))).pipe(Option.map((name) => join(at, name))));

const toml = <S extends Schema.Decoder<unknown>>(schema: S, file: string) =>
    FileSystem.FileSystem.pipe(
        Effect.flatMap((fs) => fs.readFileString(file)),
        Effect.flatMap((text) => Effect.mapError(Effect.fromResult(Toml.parse(text)), (cause) => new Cause.UnknownError(cause, `${file} does not read as TOML`))),
        Effect.flatMap(Schema.decodeUnknownEffect(schema)),
    );

const read = Effect.fnUntraced(function* (workspaceRoot: string, manifest: string, files: ReadonlySet<string>) {
    const metadata = yield* Effect.flatMap(spawned(['cargo', 'metadata', '--format-version', '1', '--no-deps', '--manifest-path', manifest], { cwd: workspaceRoot }), Schema.decodeEffect(Metadata));
    const root = relative(workspaceRoot, metadata.workspace_root) || '.';
    const types = Array.difference(Array.dedupe(metadata.packages.flatMap(({ targets }) => targets.flatMap((target) => target.crate_types))), ['lib', 'rlib']);
    const [{ profile }, names, denied] = yield* Effect.all(
        [
            toml(Profiles, join(metadata.workspace_root, MANIFEST)),
            Effect.map(spawned(['rustc', '--print', 'file-names', '--crate-name', CRATE, ...types.flatMap((type) => ['--crate-type', type]), '-'], { cwd: metadata.workspace_root }), (stdout) => stdout.split('\n')),
            Effect.transposeOption(Option.map(nearest(files, root, DENY.config), (file) => Effect.map(toml(Table, join(workspaceRoot, file)), (tables) => ({ files: [file, ...Option.toArray(nearest(files, root, DENY.exceptions))], tables: Record.keys(tables) })))),
        ],
        { concurrency: 'unbounded' },
    );
    return { metadata, profile, uplifted: Record.fromEntries(Array.zip(types, names)), deny: Option.toArray(denied) };
});

const moved = ({ metadata, ...reading }: Reading, to: (at: string) => string): Reading => ({
    ...reading,
    metadata: {
        ...metadata,
        workspace_root: to(metadata.workspace_root),
        target_directory: to(metadata.target_directory),
        packages: metadata.packages.map((crate) => ({
            ...crate,
            manifest_path: to(crate.manifest_path),
            targets: crate.targets.map((target) => ({ ...target, src_path: to(target.src_path) })),
            dependencies: crate.dependencies.map((dependency) => (dependency.path === undefined ? dependency : { ...dependency, path: to(dependency.path) })),
        })),
    },
});

const placed = (workspaceRoot: string) => ({ stored: (reading: Reading) => moved(reading, (at) => relative(workspaceRoot, at)), loaded: (reading: Reading) => moved(reading, (at) => resolve(workspaceRoot, at)) });

const resolved = (workspaceRoot: string, { metadata, profile, uplifted, deny }: Reading) => {
    const within = (at: string) => relative(workspaceRoot, at) || '.';
    const inside = (at: string) => Option.liftPredicate(within(at), (folder) => !(folder.startsWith('..') || isAbsolute(folder)));
    const root = within(metadata.workspace_root);
    const profiles = Record.keys(Record.filter(profile, ({ inherits }) => inherits !== undefined));
    const strategy = (name: string): string => profile[name]?.panic ?? UndefinedOr.match(profile[name]?.inherits, { onUndefined: () => 'unwind', onDefined: strategy });
    return {
        root,
        members: metadata.packages.map((crate) => ({ crate, root: within(dirname(crate.manifest_path)), manifest: within(crate.manifest_path), versioned: crate.version !== '0.0.0' || crate.publish === null })),
        libraries: Array.getSomes((metadata.metadata?.dylint?.libraries ?? []).map(({ path: at }) => Option.map(Option.fromUndefinedOr(at), (folder) => join(root, folder)))),
        profiles,
        strategies: Array.dedupeWith(['release', ...profiles], (left, right) => strategy(left) === strategy(right)).filter((name) => strategy(name) !== strategy('dev')),
        deny: Array.head(deny),
        placed: inside(metadata.target_directory),
        uplifted: (target: typeof Target.Type) => Array.getSomes(target.crate_types.map((type) => Option.map(Record.get(uplifted, type), (file) => file.replace(CRATE, target.name)))),
    };
};

const workspaces = (workspaceRoot: string, manifests: readonly string[], files: ReadonlySet<string>) =>
    Effect.reduce(
        manifests.toSorted((left, right) => left.split('/').length - right.split('/').length || left.localeCompare(right)),
        (): readonly (readonly [string, Result.Result<Reading, Cause.UnknownError>])[] => [],
        (done, manifest) =>
            done.some(([, reading]) => Result.isSuccess(reading) && reading.success.metadata.packages.some(({ manifest_path }) => relative(workspaceRoot, manifest_path) === manifest))
                ? Effect.succeed(done)
                : Effect.map(Effect.result(Effect.catchCause(read(workspaceRoot, manifest, files), rendered)), (reading) => [...done, [manifest, reading] as const]),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(Links.layer, layer));

const run = <A, E>(effect: Effect.Effect<A, E, Links | NodeServices | Scope.Scope>) => runtime.runPromise(Effect.scoped(effect));

// --- [EXPORTS] -------------------------------------------------------------------------

export { ancestors, Links, LOCK, MANIFEST, type Member, Metadata, manifested, PLUGIN, placed, type Reading, resolved, run, spawned, type Workspace, workspaces };
