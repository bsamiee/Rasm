import { matchesGlob } from 'node:path';
import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateNodesContext, hashArray } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { decodeFile, Mise, Package, runCommand, TextFormat } from '@rasm/eng';
import { Array, Config, Effect, Equal, FileSystem, Filter, Function, Layer, ManagedRuntime, Option, Path, Predicate, Record, Result, Schema, SchemaGetter, type Scope, String, Struct } from 'effect';
import { escape } from 'minimatch';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;
type PluginServices = ManagedRuntime.ManagedRuntime.Services<typeof pluginRuntime>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const SWIFT_MANIFEST = 'Package.swift';
const PYPROJECT = 'pyproject.toml';
const UV_LOCK = 'uv.lock';
const APPS = 'apps';

// --- [MODELS] --------------------------------------------------------------------------

const JsonListRecord = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const RequiresPython = Schema.Union([Schema.TemplateLiteralParser(['>=', Release]), Schema.TemplateLiteralParser(['==', Release, '.*'])]).pipe(Schema.decodeTo(Release, { decode: SchemaGetter.transform(([, release]) => release), encode: SchemaGetter.transform((release) => ['>=', release] as const) }));
const WorkspacePyproject = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ tool: Schema.Struct({ pytest: Schema.Struct({ testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ testPaths: 'testpaths' })) }) })));
const MemberPyproject = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            project: Schema.Struct({
                scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                entryPoints: Schema.Record(Schema.String, Schema.Record(Schema.String, Schema.String)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            }).pipe(Schema.encodeKeys({ entryPoints: 'entry-points' })),
        }),
    ),
);
const Requirement = Schema.Struct({ name: Schema.String });
const RequirementGroups = Schema.Record(Schema.String, Schema.Array(Requirement)).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const UvLock = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            version: Schema.Literal(1),
            requiresPython: RequiresPython,
            manifest: Schema.Struct({ members: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ members: [] }))),
            package: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    source: Schema.Struct({ editable: Schema.OptionFromOptionalKey(Schema.String), virtual: Schema.OptionFromOptionalKey(Schema.String) }),
                    dependencies: Schema.Array(Requirement).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                    optionalDependencies: RequirementGroups,
                    devDependencies: RequirementGroups,
                }).pipe(Schema.encodeKeys({ optionalDependencies: 'optional-dependencies', devDependencies: 'dev-dependencies' })),
            ),
        }).pipe(Schema.encodeKeys({ requiresPython: 'requires-python' })),
    ),
);
const CargoMetadata = Schema.Struct({
    packages: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            manifest: Schema.String,
            targets: Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.NonEmptyArray(Schema.String), types: Schema.Array(Schema.String), source: Schema.String, test: Schema.Boolean }).pipe(Schema.encodeKeys({ types: 'crate_types', source: 'src_path' }))),
            dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.OptionFromOptionalKey(Schema.String) })),
        }).pipe(Schema.encodeKeys({ manifest: 'manifest_path' })),
    ),
});
const SwiftPackage = Schema.Struct({
    targets: Schema.Array(
        Schema.Struct({ name: Schema.String, c99name: Schema.String, type: Schema.Literals(['executable', 'library', 'test', 'macro', 'plugin', 'snippet', 'system-target', 'binary']), path: Schema.String, locals: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(
            Schema.encodeKeys({ locals: 'target_dependencies' }),
        ),
    ),
    products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
    platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String })),
});

// --- [ERRORS] --------------------------------------------------------------------------

class WorkspaceError extends Schema.TaggedError<WorkspaceError, { readonly brand: unique symbol }>()('WorkspaceError', {
    reason: Schema.TaggedUnion({ Missing: { value: Schema.String }, Unparsed: { error: Schema.String } }),
    subject: Schema.String,
}) {
    static readonly missing = (subject: string, value: string) => WorkspaceError.make({ reason: { _tag: 'Missing', value }, subject });
    static readonly unparsed = (subject: string, error: string) => WorkspaceError.make({ reason: { _tag: 'Unparsed', error }, subject });

    override get message(): string {
        return WorkspaceError.fields.reason.match(this.reason, {
            Missing: ({ value }) => `${this.subject} has no ${value}`,
            Unparsed: ({ error }) => `${this.subject} does not parse: ${error}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]

const glob = (workspaceRoot: string, patterns: string[], exclude?: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(workspaceRoot, patterns, exclude));
const readPython = Effect.fnUntraced(function* (workspaceRoot: string, locks: readonly string[]) {
    const path = yield* Path.Path;
    const [{ tool }, decoded] = yield* Effect.all([decodeFile(WorkspacePyproject, workspaceRoot, PYPROJECT), Effect.forEach(locks, (lock) => Effect.map(decodeFile(UvLock, workspaceRoot, lock), Struct.assign({ root: path.dirname(lock) })), { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
    const listed = decoded.flatMap(({ root, manifest, package: packages }) => packages.map((entry) => ({ root, entry, at: Option.firstSomeOf([entry.source.editable, entry.source.virtual]), member: manifest.members.includes(entry.name) })));
    const owned = Array.getSomes(
        listed.map(({ root, entry, at, member }) =>
            Option.map(
                Option.filter(at, (held) => member || held === '.'),
                (held) => ({ root, entry, dir: path.join(root, held) }),
            ),
        ),
    );
    const manifests = yield* Effect.forEach(owned, ({ dir }) => decodeFile(MemberPyproject, workspaceRoot, dir, PYPROJECT), { concurrency: 'unbounded' });
    const members = Array.zipWith(owned, manifests, ({ root, dir, entry: { name, source, dependencies, optionalDependencies, devDependencies } }, { project }) => ({
        root,
        dir,
        name,
        importName: String.kebabToSnake(name),
        requires: Array.dedupe([...dependencies, ...Object.values(optionalDependencies).flat(), ...Object.values(devDependencies).flat()].map(Struct.get('name'))),
        suite: Option.isSome(source.virtual) && tool.pytest.testPaths.some((at) => !path.relative(at, dir).startsWith('..')),
        scripts: Object.keys(project.scripts),
        plugin: Record.has(project.entryPoints, 'pytest11'),
    }));
    return {
        roots: Record.fromIterableWith(decoded, ({ root, requiresPython }) => [root, { lock: `{workspaceRoot}/${path.join(root, UV_LOCK)}`, interpreter: { runtime: `uv run --project ${root} --no-sync python -c "import sys, sysconfig; print(sys.version, sysconfig.get_platform())"` }, release: requiresPython }]),
        members: Record.fromIterableBy(members, Struct.get('dir')),
        owners: Record.fromIterableBy(members, Struct.get('name')),
        mapping: yield* Schema.encodeEffect(JsonListRecord)(Record.fromIterableWith(members, ({ name, importName }) => [name, [importName]])).pipe(Effect.orDie),
    };
});

const readPackages = Effect.fnUntraced(function* (workspaceRoot: string) {
    const path = yield* Path.Path;
    const files = yield* glob(workspaceRoot, [`**/${Package.file}`]);
    return Record.fromEntries(Array.zip(files.map(path.dirname), yield* Effect.forEach(files, (file) => decodeFile(Package.schema, workspaceRoot, file), { concurrency: 'unbounded' })));
});
const readDaemons = Effect.fnUntraced(function* (workspaceRoot: string, declared: typeof Mise.declared.Type) {
    const path = yield* Path.Path;
    const owned = Array.filterMap(declared, ({ name, source }) => (source === null || path.relative(workspaceRoot, source).startsWith('..') ? Result.failVoid : Result.succeed({ name, source })));
    const sources = Array.dedupe(owned.map(Struct.get('source')));
    const decoded = Record.fromEntries(Array.zip(sources, yield* Effect.forEach(sources, (file) => decodeFile(Mise.schema, file), { concurrency: 'unbounded' })));
    return {
        daemons: Record.fromEntries(
            Array.getSomes(
                owned.map(({ name, source }) =>
                    Option.map(
                        Option.flatMap(Record.get(decoded, source), ({ daemons }) => Record.get(daemons, name)),
                        (row) => [name, row] as const,
                    ),
                ),
            ),
        ),
    };
});

// --- [WORKSPACE]

const openWorkspace = Effect.fnUntraced(function* ({ workspaceRoot, nxJsonConfiguration }: Pick<CreateNodesContext, 'workspaceRoot' | 'nxJsonConfiguration'>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.cache'))),
        (held) => Effect.sync(held.writeToDisk.bind(held)),
    );
    const cached = Effect.fnUntraced(function* <A, I>(
        schema: Schema.Codec<A, I>,
        inputs: string[],
        [command, ...args]: readonly [string, ...string[]],
        { env, pathInputs = [], ignoredInputs = [] }: { readonly env?: Readonly<Record<string, string>>; readonly pathInputs?: string[]; readonly ignoredInputs?: string[] } = {},
    ) {
        const key = [command, ...args, ...Object.values(env ?? {})].join(' ');
        const [content, paths] = yield* Effect.all([Effect.tryPromise(() => hashWithWorkspaceContext(workspaceRoot, [...inputs, path.relative(workspaceRoot, import.meta.filename)])), pathInputs.length === 0 ? Effect.succeed([]) : glob(workspaceRoot, pathInputs)]);
        const ignored = (yield* Effect.forEach(ignoredInputs, (pattern) => fs.glob(pattern, { root: workspaceRoot, dot: true }))).flat().toSorted();
        const extra = [...paths.toSorted(), ...ignored, ...(yield* Effect.forEach(ignored, (file) => fs.readFileString(path.join(workspaceRoot, file))))];
        const hash = extra.length === 0 ? content : hashArray([content, ...extra]);
        const output = yield* Option.match(Option.filter(Option.fromUndefinedOr(cache.get(key)), Predicate.Struct({ hash: Equal.equals(hash) })), {
            onSome: (held) => Effect.succeed(held.output),
            onNone: () => runCommand(command, args, { cwd: workspaceRoot, env, extendEnv: true }),
        });
        const value = yield* Schema.decodeEffect(Schema.fromJsonString(schema))(output);
        cache.set(key, { hash, output });
        return value;
    });
    const swiftManifest = <A, I>(schema: Schema.Codec<A, I>, ...args: readonly string[]) => cached(schema, [SWIFT_MANIFEST], ['swift', 'package', ...args], { env: { SWIFTPM_BUILD_DIR: path.join(workspaceRoot, '.cache/swiftpm/nx-manifest') } });
    const crates = yield* Effect.cached(
        Effect.map(
            cached(CargoMetadata, [`**/${CARGO_MANIFEST}`, 'rust-toolchain.toml', '.cargo/config.toml'], ['cargo', 'metadata', '--format-version', '1', '--no-deps'], { pathInputs: ['**/{build.rs,src/{main,lib}.rs,{src/bin,examples,tests,benches}/{*.rs,*/main.rs}}'] }),
            Function.flow(Struct.get('packages'), Array.map(Struct.evolve({ manifest: path.relative.bind(null, workspaceRoot) }))),
        ),
    );
    const patterns = ['pnpm-lock.yaml', 'tools/*/examples/**'];
    const hosts = yield* Effect.cached(fs.readDirectory(path.join(workspaceRoot, APPS)));
    return {
        workspaceRoot,
        crates,
        hosts,
        swiftManifest,
        nxJsonConfiguration,
        scanExclusions: Effect.map(
            crates,
            Function.flow(
                Array.flatMap(Struct.get('targets')),
                Array.filter(({ kind, test }) => kind.includes('example') || (kind.includes('test') && !test)),
                Array.map(({ source }) => path.relative(workspaceRoot, source)),
                Array.dedupe,
                Array.filter((source) => !patterns.some(matchesGlob.bind(null, source))),
                Array.map((source) => escape(source, { magicalBraces: true })),
                Array.prependAll(patterns),
            ),
        ),
        ...(yield* Effect.all({
            mise: Effect.cached(
                Effect.flatMap(cached(Mise.declared, [`${path.dirname(Mise.file)}/mise*.toml`], ['mise', 'daemons', 'ls', '--json'], { env: { MISE_ENV: yield* Config.String('MISE_ENV').pipe(Config.withDefault('')) }, ignoredInputs: [`${path.dirname(Mise.file)}/mise*.local.toml`] }), (declared) =>
                    readDaemons(workspaceRoot, declared),
                ),
            ),
            packages: Effect.cached(readPackages(workspaceRoot)),
            submodules: Effect.cached(
                Effect.map(
                    runCommand.records('\n', 'git', ['config', '--file', '.gitmodules', '--get-regexp', '\\.path$'], { cwd: workspaceRoot }),
                    Array.map((row) => row.slice(row.indexOf(' ') + 1)),
                ),
            ),
            swift: Effect.cached(
                Effect.map(swiftManifest(SwiftPackage, 'describe', '--type', 'json'), (described) => ({
                    ...described,
                    tests: Array.partition(described.targets, Filter.fromPredicate<(typeof described.targets)[number]>(Predicate.Struct({ type: Equal.equals('test') }))),
                })),
            ),
        })),
    };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const pluginRuntime = ManagedRuntime.make(Layer.merge(layer, layerPosix));
const runPlugin = <A, E>(effect: Effect.Effect<A, E, Scope.Scope | PluginServices>) => pluginRuntime.runPromise(Effect.scoped(effect));

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS, CARGO_MANIFEST, CLOUDFLARE_CONFIG, glob, openWorkspace, type PluginServices, PYPROJECT, readPython, runPlugin, SWIFT_MANIFEST, UV_LOCK, type Workspace, WorkspaceError };
