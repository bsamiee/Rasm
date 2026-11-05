import { matchesGlob } from 'node:path';
import type { CreateNodesContext } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Effect, Equal, FileSystem, Filter, Function, Option, Path, Predicate, Record, Schema, Scope, String, Struct } from 'effect';
import { escape } from 'minimatch';
import { serverFS } from 'typescript/fs';
import { API } from 'typescript/sync';
import { decodeFile, Mise } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const SWIFT_MANIFEST = 'Package.swift';
const PYPROJECT = 'pyproject.toml';
const APPS = 'apps';

// --- [MODELS] --------------------------------------------------------------------------

const JsonDocument = Schema.fromJsonString(Schema.Unknown);
const PythonRuntime = Schema.Struct({ versions: Schema.NonEmptyArray(Schema.String) }).pipe(Schema.encodeKeys({ versions: 'active_versions' }));
const JsonListRecord = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const PythonRequirement = Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' }));
const WorkspacePyproject = TextFormat.Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ workspace: Schema.Struct({ members: Schema.Array(Schema.String) }), requirements: Schema.Record(Schema.String, PythonRequirement) }).pipe(Schema.encodeKeys({ requirements: 'dependency-groups' })),
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
const PylockExport = Schema.Struct({ packages: Schema.Array(Schema.Struct({ directory: Schema.OptionFromOptionalKey(Schema.Struct({ path: Schema.String })) })) });
const CargoMetadata = Schema.Struct({
    packages: Schema.Array(
        Schema.Struct({
            name: Schema.String,
            manifest: Schema.String,
            targets: Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.NonEmptyArray(Schema.String), types: Schema.Array(Schema.String), source: Schema.String, test: Schema.Boolean }).pipe(Schema.encodeKeys({ types: 'crate_types', source: 'src_path' }))),
            dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.OptionFromOptionalKey(Schema.String) })),
            metadata: Schema.OptionFromNullOr(Schema.Struct({ nx: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)) })),
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
const NxReleaseConfiguration = Schema.Struct({ releaseTag: Schema.Struct({ pattern: Schema.String }), groups: Schema.Record(Schema.String, Schema.Struct({ projects: Schema.ArrayEnsure(Schema.String) })) });

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const glob = (workspaceRoot: string, patterns: string[], exclude?: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(workspaceRoot, patterns, exclude));
const readPyproject = Effect.fnUntraced(function* (workspaceRoot: string, { packages }: typeof PylockExport.Type) {
    const [path, { tool }] = yield* Effect.all([Path.Path, decodeFile(WorkspacePyproject, workspaceRoot, PYPROJECT)]);
    const directories = Array.getSomes(packages.map(Struct.get('directory'))).map(Struct.get('path'));
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
        members: Record.fromIterableWith(members, ({ member, project }) => [member, project.name]),
        moduleRoots: Array.dedupe(Array.bind(members, 'folder', ({ tool: { hatch } }) => hatch.build.devModeDirs).map(({ member, folder }) => path.join(member, folder))),
        commands: members.filter(({ project }) => !Record.isEmptyRecord(project.scripts)).map(Struct.get('member')),
        deptry: members.filter(({ tool: { deptry } }) => Option.isSome(deptry)),
        fawltydeps: {
            members: members.filter(({ tool: { fawltydeps } }) => Option.isSome(fawltydeps)),
            mapping: yield* Schema.encodeEffect(JsonListRecord)(mapping).pipe(Effect.orDie),
        },
        requirements: tool.uv.requirements,
        dataFile: tool.coverage.run.dataFile,
        ...tool.pytest,
    };
});

// --- [WORKSPACE]
const openWorkspace = Effect.fnUntraced(function* ({ workspaceRoot, nxJsonConfiguration }: Pick<CreateNodesContext, 'workspaceRoot' | 'nxJsonConfiguration'>) {
    const [fs, path, scope] = yield* Effect.all([FileSystem.FileSystem, Path.Path, Effect.scope]);
    const configReads = new Set<string>();
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.cache'))),
        (held) => Effect.sync(held.writeToDisk.bind(held)),
    );
    const cached = <A, I>(document: Schema.Codec<unknown, string>, schema: Schema.Codec<A, I>, inputs: string[], [command, ...args]: readonly [string, ...string[]]) => {
        const [key, held] = [[command, ...args].join(' '), Schema.fromJsonString(schema)];
        return Effect.tryPromise(() => hashWithWorkspaceContext(workspaceRoot, inputs)).pipe(
            Effect.bindTo('hash'),
            Effect.bind('value', ({ hash }) =>
                Option.fromUndefinedOr(cache.get(key)).pipe(
                    Option.filter(Predicate.Struct({ hash: Equal.equals(hash) })),
                    Option.match({ onSome: ({ output }) => Schema.decodeEffect(held)(output), onNone: () => Effect.flatMap(runCommand(command, args, { cwd: workspaceRoot }), Schema.decodeEffect(document.pipe(Schema.decodeTo(schema)))) }),
                ),
            ),
            Effect.bind('output', ({ value }) => Schema.encodeEffect(held)(value)),
            Effect.tap(({ hash, output }) => Effect.sync(cache.set.bind(cache, key, { hash, output }))),
            Effect.map(Struct.get('value')),
        );
    };
    const lockedPackages = (project: string) => cached(TextFormat.Toml, PylockExport, ['uv.lock', `**/${PYPROJECT}`], ['uv', 'export', '--frozen', '--format', 'pylock.toml', '--no-hashes', '--no-emit-project', '--project', project]);
    const crates = yield* Effect.cached(Effect.map(cached(JsonDocument, CargoMetadata, [`**/${CARGO_MANIFEST}`, '**/*.rs'], ['cargo', 'metadata', '--format-version', '1', '--no-deps']), Function.flow(Struct.get('packages'), Array.map(Struct.evolve({ manifest: path.relative.bind(null, workspaceRoot) })))));
    const patterns = ['pnpm-lock.yaml', 'tools/*/examples/**'];
    return {
        workspaceRoot,
        lockedPackages,
        crates,
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
            pyproject: Effect.cached(Effect.flatMap(lockedPackages('.'), (packages) => readPyproject(workspaceRoot, packages))),
            mise: Effect.cached(Mise.read(workspaceRoot)),
            hosts: Effect.cached(fs.readDirectory(path.join(workspaceRoot, APPS))),
            release: Effect.cached(Schema.decodeUnknownEffect(NxReleaseConfiguration)(nxJsonConfiguration.release)),
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
            python: Effect.cached(cached(JsonDocument, PythonRuntime, [Mise.file], ['mise', 'tool', 'python', '--json'])),
            swift: Effect.cached(
                Effect.map(cached(JsonDocument, SwiftPackage, [SWIFT_MANIFEST], ['swift', 'package', 'describe', '--type', 'json']), (described) => ({
                    ...described,
                    tests: Array.partition(described.targets, Filter.fromPredicate<(typeof described.targets)[number]>(Predicate.Struct({ type: Equal.equals('test') }))),
                })),
            ),
        })),
    };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS, CARGO_MANIFEST, glob, JsonListRecord, openWorkspace, PYPROJECT, PythonRequirement, Release, SWIFT_MANIFEST, type Workspace };
