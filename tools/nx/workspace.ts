import { matchesGlob } from 'node:path';
import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateNodesContext, hashArray } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Equal, FileSystem, Filter, Function, Layer, ManagedRuntime, Option, Order, Path, Predicate, Record, Schema, Scope, String, Struct } from 'effect';
import { escape } from 'minimatch';
import { serverFS } from 'typescript/fs';
import { API } from 'typescript/sync';
import { decodeFile, Mise, Package, suitePath } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;
type PluginServices = ManagedRuntime.ManagedRuntime.Services<typeof pluginRuntime>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const SWIFT_MANIFEST = 'Package.swift';
const PYPROJECT = 'pyproject.toml';
const APPS = 'apps';

// --- [MODELS] --------------------------------------------------------------------------

const JsonListRecord = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const PythonRequirement = Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' }));
const HostPyproject = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ project: PythonRequirement })));
const WorkspacePyproject = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({
                    workspace: Schema.Struct({ members: Schema.Array(Schema.String) }),
                    sources: Schema.Record(Schema.String, Schema.Struct({ path: Schema.OptionFromOptionalKey(Schema.String) })),
                }),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
                coverage: Schema.Struct({ run: Schema.Struct({ dataFile: Schema.String }).pipe(Schema.encodeKeys({ dataFile: 'data_file' })) }),
            }),
        }),
    ),
);
const MemberPyproject = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            project: Schema.Struct({ name: Schema.String, scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }),
            tool: Schema.Struct({
                hatch: Schema.Struct({
                    build: Schema.Struct({
                        devModeDirs: Schema.Array(Schema.String),
                        targets: Schema.optionalKey(Schema.Struct({ wheel: Schema.optionalKey(Schema.Struct({ sources: Schema.optionalKey(Schema.Union([Schema.Record(Schema.String, Schema.String), Schema.Array(Schema.String)])) })) })),
                    }).pipe(Schema.encodeKeys({ devModeDirs: 'dev-mode-dirs' })),
                }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ build: { 'dev-mode-dirs': [] } }))),
                deptry: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)),
                fawltydeps: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)),
            }),
        }),
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
    reason: Schema.TaggedUnion({ Missing: { value: Schema.String }, Shadowed: { folder: Schema.String }, Single: { module: Schema.String }, Unparsed: { error: Schema.String } }),
    subject: Schema.String,
}) {
    static readonly missing = (subject: string, value: string) => WorkspaceError.make({ reason: { _tag: 'Missing', value }, subject });
    static readonly shadowed = (subject: string, folder: string) => WorkspaceError.make({ reason: { _tag: 'Shadowed', folder }, subject });
    static readonly single = (subject: string, module: string) => WorkspaceError.make({ reason: { _tag: 'Single', module }, subject });
    static readonly unparsed = (subject: string, error: string) => WorkspaceError.make({ reason: { _tag: 'Unparsed', error }, subject });

    override get message(): string {
        return WorkspaceError.fields.reason.match(this.reason, {
            Missing: ({ value }) => `${this.subject} has no ${value}`,
            Shadowed: ({ folder }) => `${this.subject} shares its name with folder ${folder}, move it into the folder`,
            Single: ({ module }) => `${this.subject} re-exports only ${module}, point its exports entry at that module`,
            Unparsed: ({ error }) => `${this.subject} does not parse: ${error}`,
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const glob = (workspaceRoot: string, patterns: string[], exclude?: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(workspaceRoot, patterns, exclude));
const suiteParents = (path: Path.Path, suites: readonly string[]) => Array.dedupe(suites.flatMap((suite) => Array.unfold(path.dirname(suite), (parent) => (parent === '.' ? Option.none() : Option.some([parent, path.dirname(parent)])))));
const readPyproject = Effect.fnUntraced(function* (workspaceRoot: string) {
    const [path, { tool }] = yield* Effect.all([Path.Path, decodeFile(WorkspacePyproject, workspaceRoot, PYPROJECT)]);
    const directories = Array.getSomes(Object.values(tool.uv.sources).map(Struct.get('path')));
    const members = yield* Effect.forEach(Array.union(tool.uv.workspace.members, directories), (member) => Effect.map(decodeFile(MemberPyproject, workspaceRoot, path.join(member, PYPROJECT)), Struct.assign({ member })));
    const providers = Array.bind(members, 'source', ({ tool: { hatch } }) => {
        const sources = hatch.build.targets?.wheel?.sources;
        return Array.isArray(sources) ? [] : Object.values(sources ?? {});
    })
        .filter(Predicate.Struct({ source: String.isNonEmpty }))
        .map(({ project, source }) => {
            const separator = source.indexOf('/');
            return { distribution: project.name, importRoot: separator < 0 ? source : source.slice(0, separator) };
        });
    const mapping = Record.map(Array.groupBy(providers, Struct.get('distribution')), Function.flow(Array.map(Struct.get('importRoot')), Array.dedupe));
    return {
        moduleRoots: Array.dedupe(Array.bind(members, 'folder', ({ tool: { hatch } }) => hatch.build.devModeDirs).map(({ member, folder }) => path.join(member, folder))),
        commands: members.filter(({ project }) => !Record.isEmptyRecord(project.scripts)).map(Struct.get('member')),
        deptry: members.filter(({ tool: { deptry } }) => Option.isSome(deptry)),
        fawltydeps: {
            members: members.filter(({ tool: { fawltydeps } }) => Option.isSome(fawltydeps)),
            mapping: yield* Schema.encodeEffect(JsonListRecord)(mapping).pipe(Effect.orDie),
        },
        dataFile: tool.coverage.run.dataFile,
        ...tool.pytest,
    };
});
const readScopes = Effect.fnUntraced(function* (workspaceRoot: string, folders: readonly string[], testPaths: readonly string[]) {
    const path = yield* Path.Path;
    const conftest = (directory: string) => path.join(directory, 'conftest.py');
    const [files, sources] = yield* Effect.all(
        [
            glob(
                workspaceRoot,
                folders.map((host) => `**/${host}/**/${PYPROJECT}`),
            ),
            glob(workspaceRoot, [conftest('.'), ...suiteParents(path, testPaths).map(conftest), ...testPaths.map((directory) => `${directory}/**/*.{py,pyi}`)]),
        ],
        { concurrency: 'unbounded' },
    );
    return yield* Effect.validate(
        files,
        Effect.fnUntraced(function* (file: string) {
            const scope = path.dirname(file);
            const { project } = yield* decodeFile(HostPyproject, workspaceRoot, file);
            const suites = testPaths.map((base) => suitePath(path, base, scope)).filter((suite) => sources.some(String.startsWith(`${suite}${path.sep}`)));
            return { scope, version: project.minimum[1], suites, fixtures: suites.length === 0 ? [] : Array.sort(Array.intersection([conftest('.'), ...suiteParents(path, suites).map(conftest)], sources), Order.String) };
        }),
        { concurrency: 'unbounded' },
    );
});

const readPackages = Effect.fnUntraced(function* (workspaceRoot: string) {
    const path = yield* Path.Path;
    const files = yield* glob(workspaceRoot, [`**/${Package.file}`]);
    return Record.fromEntries(Array.zip(files.map(path.dirname), yield* Effect.forEach(files, (file) => decodeFile(Package.schema, workspaceRoot, file), { concurrency: 'unbounded' })));
});

// --- [WORKSPACE]
const openWorkspace = Effect.fnUntraced(function* ({ workspaceRoot, nxJsonConfiguration }: Pick<CreateNodesContext, 'workspaceRoot' | 'nxJsonConfiguration'>) {
    const [fs, path, scope] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.scope]);
    const pyproject = yield* Effect.cached(readPyproject(workspaceRoot));
    const configReads = new Set<string>();
    let cacheChanged = false;
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.cache'))),
        (held) => (cacheChanged ? Effect.sync(held.writeToDisk.bind(held)) : Effect.void),
    );
    const cached = Effect.fnUntraced(function* <A, I>(schema: Schema.Codec<A, I>, inputs: string[], [command, ...args]: readonly [string, ...string[]], { env, pathInputs = [] }: { readonly env?: Readonly<Record<string, string>>; readonly pathInputs?: string[] } = {}) {
        const key = [command, ...args, ...Object.values(env ?? {})].join(' ');
        const [content, paths] = yield* Effect.all([Effect.tryPromise(() => hashWithWorkspaceContext(workspaceRoot, [...inputs, path.relative(workspaceRoot, import.meta.filename)])), pathInputs.length === 0 ? Effect.succeed([]) : glob(workspaceRoot, pathInputs)]);
        const hash = paths.length === 0 ? content : hashArray([content, ...paths.toSorted()]);
        const prior = cache.get(key);
        const output = prior?.hash === hash ? prior.output : yield* runCommand(command, args, { cwd: workspaceRoot, env, extendEnv: true });
        const value = yield* Schema.decodeEffect(Schema.fromJsonString(schema))(output);
        if (prior?.hash !== hash) {
            cache.set(key, { hash, output });
            cacheChanged = true;
        }
        return value;
    });
    const swiftManifest = <A, I>(schema: Schema.Codec<A, I>, ...args: readonly string[]) => cached(schema, [SWIFT_MANIFEST], ['swift', 'package', ...args], { env: { SWIFTPM_BUILD_DIR: path.join(workspaceRoot, '.cache/swiftpm/nx-manifest') } });
    const crates = yield* Effect.cached(
        Effect.map(
            cached(CargoMetadata, [`**/${CARGO_MANIFEST}`, 'rust-toolchain.toml', '.cargo/config.toml'], ['cargo', 'metadata', '--format-version', '1', '--no-deps'], { pathInputs: ['**/*.rs'] }),
            Function.flow(Struct.get('packages'), Array.map(Struct.evolve({ manifest: path.relative.bind(null, workspaceRoot) }))),
        ),
    );
    const patterns = ['pnpm-lock.yaml', 'tools/*/examples/**'];
    const hosts = yield* Effect.cached(fs.readDirectory(path.join(workspaceRoot, APPS)));
    return {
        workspaceRoot,
        crates,
        hosts,
        pyproject,
        swiftManifest,
        release: nxJsonConfiguration.release,
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
            mise: Effect.cached(Mise.read(workspaceRoot)),
            packages: Effect.cached(readPackages(workspaceRoot)),
            pythonScopes: Effect.cached(Effect.flatMap(Effect.all([hosts, pyproject]), ([folders, { testPaths }]) => readScopes(workspaceRoot, folders, testPaths))),
            configs: Effect.cached(
                Effect.acquireRelease(
                    Effect.sync(
                        () =>
                            new API({
                                cwd: workspaceRoot,
                                fs: {
                                    directoryExists: serverFS.useOS,
                                    fileExists: serverFS.useOS,
                                    getAccessibleEntries: serverFS.useOS,
                                    readFile: (file) => {
                                        configReads.add(file);
                                        return serverFS.useOS;
                                    },
                                    realpath: serverFS.useOS,
                                    stat: serverFS.useOS,
                                    writeFile: serverFS.useOS,
                                    removeFile: serverFS.useOS,
                                },
                            }),
                    ),
                    (api) => Effect.sync(api.close),
                ).pipe(
                    Scope.provide(scope),
                    Effect.map((api) => (file: string) => {
                        configReads.clear();
                        const config = api.parseConfigFile(path.join(workspaceRoot, file));
                        return { config, files: [...configReads].map(path.relative.bind(null, workspaceRoot)) };
                    }),
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

export { APPS, CARGO_MANIFEST, CLOUDFLARE_CONFIG, glob, JsonListRecord, openWorkspace, type PluginServices, PYPROJECT, runPlugin, SWIFT_MANIFEST, suiteParents, type Workspace, WorkspaceError };
