import { layer as nodeSpawner } from '@effect/platform-node/NodeChildProcessSpawner';
import { layer as nodeFileSystem } from '@effect/platform-node/NodeFileSystem';
import { layerPosix } from '@effect/platform-node/NodePath';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, createNodesFromFiles, DependencyType, type FileMap, type Optional, type ProjectConfiguration, parseJson, type StaticDependency, type TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, flow, Layer, ManagedRuntime, Option, Path, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parse as parseToml } from 'smol-toml';
import { parse as parseYaml } from 'yaml';
import { stdout } from '../../eng/typescript/process.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Store = Effect.Success<ReturnType<typeof _store>>;
type Workspace = Effect.Success<ReturnType<typeof _workspace>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CARGO_MANIFEST = 'Cargo.toml';
const _SWIFT_MANIFEST = 'Package.swift';
const _PYPROJECT = 'pyproject.toml';
const _CLOUDFLARE_CONFIG = 'cloudflare.config.ts';

// --- [MODELS] --------------------------------------------------------------------------

const _Parsed = (expected: string, decode: (text: string) => unknown): Schema.decodeTo<Schema.Unknown, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => decode(text), catch: () => new SchemaIssue.InvalidValue({ expected }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const _Toml = _Parsed('TOML', parseToml);
const _Yaml = _Parsed('YAML', parseYaml);
const _Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const _Mise = _Toml.pipe(Schema.decodeTo(Schema.Struct({ daemons: Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Struct({ run: Schema.OptionFromOptionalKey(Schema.String) })])) })));
const _PythonRuntime = Schema.fromJsonString(Schema.NonEmptyArray(Schema.Struct({ version: Schema.String })));
const _ImportGraph = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const _Sgconfig = _Yaml.pipe(Schema.decodeTo(Schema.Struct({ ruleDirs: Schema.Array(Schema.String) })));
const _Rule = _Yaml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String, metadata: Schema.OptionFromOptionalKey(Schema.Struct({ below: _Release }).pipe(Schema.encodeKeys({ below: 'python-below' }))) })));
const _Host = _Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(_Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' })) })));
const _Root = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ buildBackend: Schema.Struct({ moduleRoot: Schema.String }).pipe(Schema.encodeKeys({ moduleRoot: 'module-root' })) }).pipe(Schema.encodeKeys({ buildBackend: 'build-backend' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
            }),
        }),
    ),
);
const _Manifest = _Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const _Metadata = Schema.fromJsonString(
    Schema.Struct({
        packages: Schema.Array(
            Schema.Struct({
                name: Schema.String,
                manifest: Schema.String,
                targets: Schema.Array(Schema.Struct({ name: Schema.String, kind: Schema.Array(Schema.String), types: Schema.Array(Schema.String), test: Schema.Boolean }).pipe(Schema.encodeKeys({ types: 'crate_types' }))),
                dependencies: Schema.Array(Schema.Struct({ name: Schema.String, path: Schema.OptionFromOptionalKey(Schema.String) })),
                metadata: Schema.OptionFromNullOr(Schema.Struct({ nx: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)) })),
            }).pipe(Schema.encodeKeys({ manifest: 'manifest_path' })),
        ),
    }),
);
const _SwiftPackage = Schema.fromJsonString(
    Schema.Struct({
        targets: Schema.Array(Schema.Struct({ type: Schema.String })),
        dependencies: Schema.Array(Schema.Struct({ fileSystem: Schema.OptionFromOptionalKey(Schema.Array(Schema.Struct({ path: Schema.String }))) })),
    }),
);
const _XcodeProject = _Parsed('JSON', parseJson).pipe(
    Schema.decodeTo(
        Schema.Struct({
            packages: Schema.Array(Schema.Union([Schema.Struct({ kind: Schema.Literal('local'), path: Schema.String }), Schema.Struct({ kind: Schema.Literal('remote') })])).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
        }),
    ),
);
const _MSBuildProject = _Parsed('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const _glob = (root: string, patterns: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(root, patterns));
const _decode = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, root: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(root, file)), Schema.decodeEffect(schema));
});
const _store = (root: string) =>
    Effect.acquireRelease(
        Effect.map(Path.Path, (path) => ({ root, cache: new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.hash')) })),
        ({ cache }) => Effect.sync(() => cache.writeToDisk()),
    );
const _cached = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, { root, cache }: Store, { inputs, argv: [command, ...args], env }: { readonly inputs: string[]; readonly argv: [string, ...string[]]; readonly env?: Record<string, string> }) {
    const [key, hash] = [[command, ...args].join(' '), yield* Effect.tryPromise(() => hashWithWorkspaceContext(root, inputs))];
    const text = yield* Option.fromUndefinedOr(cache.get(key)).pipe(
        Option.filter((entry) => entry.hash === hash),
        Option.map(Struct.get('output')),
        Option.match({ onSome: Effect.succeed, onNone: () => stdout(command, args, { cwd: root, env, extendEnv: true }) }),
    );
    return yield* Effect.tap(Schema.decodeEffect(schema)(text), () => Effect.sync(() => cache.set(key, { hash, output: text })));
});
const _crates = Effect.fnUntraced(function* (store: Store) {
    const [path, { packages }] = yield* Effect.all([Path.Path, _cached(_Metadata, store, { inputs: [`**/${_CARGO_MANIFEST}`, '**/*.rs'], argv: ['cargo', 'metadata', '--format-version', '1', '--no-deps'] })]);
    return packages.map((crate) => ({ ...crate, manifest: path.relative(store.root, crate.manifest) }));
});
const _packages = Effect.fnUntraced(function* (store: Store) {
    const [path, manifests] = yield* Effect.all([Path.Path, _glob(store.root, [`*/**/${_SWIFT_MANIFEST}`])]);
    return yield* Effect.validate(
        manifests,
        Effect.fnUntraced(function* (manifest: string) {
            const env = { SWIFTPM_BUILD_DIR: path.join('.cache/swiftpm/build', path.dirname(manifest)) };
            const { targets, dependencies } = yield* _cached(_SwiftPackage, store, { inputs: [manifest], argv: ['swift', 'package', 'dump-package', '--package-path', path.dirname(manifest)], env });
            return {
                manifest,
                env,
                tested: targets.some(({ type }) => type === 'test'),
                locals: dependencies.flatMap(({ fileSystem }) => Option.getOrElse(fileSystem, Array.empty)).map((local) => path.join(path.relative(store.root, local.path), _SWIFT_MANIFEST)),
            };
        }),
        { concurrency: 'unbounded' },
    );
});
const _workspace = Effect.fnUntraced(function* (root: string) {
    const [fs, path, store] = yield* Effect.all([FileSystem.FileSystem, Path.Path, _store(root)]);
    return {
        ...store,
        ...(yield* Effect.all({
            pyproject: Effect.cached(Effect.map(_decode(_Root, root, _PYPROJECT), ({ tool }) => ({ moduleRoot: tool.uv.buildBackend.moduleRoot, ...tool.pytest }))),
            mise: Effect.cached(_decode(_Mise, root, 'mise.toml')),
            hosts: Effect.cached(fs.readDirectory(path.join(root, 'apps'))),
            crates: Effect.cached(_crates(store)),
            packages: Effect.cached(_packages(store)),
        })),
    };
});
const _python = Effect.fnUntraced(function* (dir: string, { root, pyproject }: Workspace) {
    const [path, { moduleRoot, pythonFiles, testPaths }] = yield* Effect.all([Path.Path, pyproject]);
    const mirrors = testPaths.map((testPath) => path.join(testPath, dir.startsWith(`${moduleRoot}/`) ? path.join(path.dirname(moduleRoot), path.relative(moduleRoot, dir)) : dir));
    const suites = yield* Effect.filter(
        mirrors,
        (mirror) =>
            Effect.map(
                _glob(
                    root,
                    pythonFiles.map((pattern) => `${mirror}/**/${pattern}`),
                ),
                Array.isArrayNonEmpty,
            ),
        { concurrency: 'unbounded' },
    );
    return suites.length > 0 ? { check: {}, test: { command: 'pytest', options: { args: [...suites, '--cov'] }, inputs: suites.map((suite) => `{workspaceRoot}/${suite}/**/*`) } } : {};
});
const _sources = (root: string, { projectFileMap }: FileMap) =>
    Array.match(
        Object.values(projectFileMap).flatMap((files) => files.map(({ file }) => file).filter((file) => file.endsWith('.py'))),
        {
            onEmpty: () => Effect.succeed([]),
            onNonEmpty: (files) => Effect.map(Effect.flatMap(stdout('ruff', ['analyze', 'graph', ...files], { cwd: root }), Schema.decodeEffect(_ImportGraph)), (graph) => Object.entries(graph).flatMap(([file, imports]) => imports.map((target) => [file, target] as const))),
        },
    );
const _suite = Effect.fnUntraced(function* (language: string, dir: string, root: string) {
    const path = yield* Path.Path;
    const suite = path.join('tests', language, ...dir.split(path.sep).filter((segment) => segment !== language));
    return Array.match(yield* _glob(root, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => ({ namedInputs: { default: ['{projectRoot}/**/*', `{workspaceRoot}/${suite}/**/*`] } }) });
});
const _swift = (build: string, targets: Record<string, TargetConfiguration>) => ({ tags: ['language:swift', 'host:macos', `build:${build}`], targets: { build: {}, upgrade: {}, check: {}, ...targets } });

const _PROJECTS = {
    '*.csproj': Effect.fnUntraced(function* (_, { file, root }) {
        const { Project } = yield* _decode(_MSBuildProject, root, file);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, pivot: string) => [
            `{workspaceRoot}/.artifacts/dotnet/{bin,obj}/{projectName}/${pivot}*`,
            `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`,
            ...(plugin ? [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/*/{projectName}_${pivot}*`] : []),
        ];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                check: {},
                ...(plugin ? { pack: {}, install: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { file, root }) {
        yield* Effect.filterOrFail(_decode(Schema.String, root, file), (text) => text.startsWith('#!'));
        const inputs = ['default', '{workspaceRoot}/.editorconfig'];
        const binlog = (target: string) => `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target}-${base}.binlog`;
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: 'build:*.cs', params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: 'format:*.cs', params: 'forward' }] },
                lint: { executor: 'nx:noop', dependsOn: [{ target: 'lint:*.cs', params: 'forward' }] },
                [`restore:${base}`]: { inputs: ['default', 'dotnet'], outputs: [`{workspaceRoot}/.artifacts/dotnet/obj/${base}/project.assets.json`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*.nuget.*`, binlog('restore')], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs, outputs: [`{workspaceRoot}/.artifacts/dotnet/bin/${base}`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*/**/*`, binlog('build')], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                [`lint:${base}`]: { options: { args: [file, '--no-restore', '--verify-no-changes'] }, dependsOn: [`restore:${base}`], inputs: [...inputs, 'dotnet'], outputs: [binlog('lint')] },
                check: {},
            },
        };
    }),
    '.swcrc': () => Effect.succeed({ tags: ['host:extendscript'], targets: { build: {} } }),
    [_CARGO_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, root, crates, mise }) {
        const [all, { daemons }, suite] = yield* Effect.all([crates, mise, _suite('rust', dir, root)], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const named = new Set(bins.map(({ name }) => name));
        const launched = Object.values(daemons).some((daemon) => typeof daemon !== 'string' && Option.exists(daemon.run, (run) => !named.isDisjointFrom(new Set(run.split(' ')))));
        const tested = crate.targets.some(({ test }) => test);
        const dylint = crate.dependencies.some(({ name }) => name === 'dylint_linting');
        const selection = tested ? { options: { args: crate.targets.some(({ kind, types }) => kind.some((type) => type !== 'bin' && types.includes(type))) ? ['--lib', '--bins', '--tests'] : ['--all-targets'] } } : {};
        return {
            ...suite,
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            targets: {
                'typecheck:clippy': { ...selection },
                'lint:cargo-modules': {},
                'lint:dylint': { ...selection },
                check: {},
                ...(tested ? { test: {}, ...(dylint ? {} : { coverage: {} }) } : {}),
                ...(bins.length > 0 ? { build: {} } : {}),
                ...(launched ? { install: {}, 'install:cargo': {}, 'upgrade:cargo': { dependsOn: [{ projects: ['directory:.'], target: 'upgrade:rust' }] } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ moduleRoot }, { id }, targets] = yield* Effect.all([workspace.pyproject, _decode(_Manifest, workspace.root, workspace.file), _python(dir, workspace)], { concurrency: 'unbounded' });
        const archive = '.artifacts/blender/{projectName}/extension.zip';
        return {
            name: id,
            tags: ['language:python'],
            targets: {
                pack: { command: 'python -m eng.python.fitout pack', options: { args: ['{projectRoot}', archive] }, inputs: [`{workspaceRoot}/${moduleRoot}/**/*`], outputs: [`{workspaceRoot}/${archive}`] },
                install: { command: 'python -m eng.python.fitout install', options: { args: [archive] } },
                ...targets,
            },
        };
    }),
    'project.xcproj': ({ dir }) =>
        Effect.map(Path.Path, (path) => {
            const root = path.dirname(dir);
            const target = { options: { env: { XCODE_DERIVED_DATA_PATH: `.cache/xcode/${root}` } } };
            return { root, name: path.parse(dir).name, ..._swift('xcode', { build: target, upgrade: target, install: target, 'lint:swiftlint': target }) };
        }),
    [_SWIFT_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, packages }) {
        const { env, tested } = yield* Effect.fromOption(Array.findFirst(yield* packages, ({ manifest }) => manifest === file));
        const target = { options: { env } };
        return { name: dir, ..._swift('swiftpm', { build: target, upgrade: target, ...(tested ? { test: target } : {}) }) };
    }),
    'py.typed': ({ dir }, workspace) => Effect.map(Effect.all([Path.Path, workspace.pyproject, _python(dir, workspace)]), ([path, { moduleRoot }, targets]) => ({ name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python'], targets })),
    'tsconfig.json': ({ dir }, { root }) => Effect.map(_suite('typescript', dir, root), (suite) => ({ ...suite, tags: ['language:typescript'], targets: { 'typecheck:tsc': {}, check: {} } })),
    'vite.config.ts': ({ dir }, { configFiles }) => Effect.map(Path.Path, (path) => ({ tags: ['bundler:vite', ...(configFiles.includes(path.join(dir, _CLOUDFLARE_CONFIG)) ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {} } })),
    'uxp.config.ts': () => Effect.succeed({ tags: ['host:uxp'], targets: { build: {}, install: {} } }),
    [_CLOUDFLARE_CONFIG]: () => Effect.succeed({ targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } }),
    '.storybook/main.ts': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), targets: { storybook: {} } })),
} as const satisfies Record<string, (file: Path.Path.Parsed, workspace: Workspace & { readonly file: string; readonly configFiles: readonly string[] }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, ManagedRuntime.ManagedRuntime.Services<typeof _runtime>>>;

const _project = Effect.fnUntraced(
    function* (file: string, workspace: Workspace, configFiles: readonly string[]) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(_PROJECTS), ([marker]) => [parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`].includes(marker)));
        const { root: directory = parsed.dir, tags = [], ...configuration }: Optional<ProjectConfiguration, 'root'> = yield* configure(parsed, { ...workspace, file, configFiles });
        const hosted = Array.intersection(directory.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        return { projects: { [directory]: { ...configuration, root: directory, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);
const _pythonVersions = Effect.fnUntraced(function* ({ root, cache, hosts }: Workspace) {
    const [path, folders] = yield* Effect.all([Path.Path, hosts]);
    const [[runtime], scopes, gated] = yield* Effect.all(
        [
            _cached(_PythonRuntime, { root, cache }, { inputs: ['mise.toml'], argv: ['mise', 'ls', 'python', '--current', '--json'] }),
            Effect.flatMap(
                _glob(
                    root,
                    folders.map((host) => `**/${host}/**/${_PYPROJECT}`),
                ),
                Effect.validate((file: string) => Effect.map(_decode(_Host, root, file), ({ project }) => ({ scope: path.dirname(file), version: project.minimum[1] })), { concurrency: 'unbounded' }),
            ),
            _decode(_Sgconfig, root, 'sgconfig.yml').pipe(
                Effect.flatMap(({ ruleDirs }) =>
                    _glob(
                        root,
                        ruleDirs.map((dir) => `${dir}/python/**/*.yml`),
                    ),
                ),
                Effect.flatMap(Effect.validate((file: string) => _decode(_Rule, root, file), { concurrency: 'unbounded' })),
                Effect.map(Array.flatMap(({ id, metadata }) => Option.toArray(Option.map(metadata, ({ below }) => ({ id, below }))))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const covered =
        (version: string) =>
        ({ below }: { readonly below: string }) =>
            version.localeCompare(below, 'en', { numeric: true }) >= 0;
    const off = gated.filter(covered(runtime.version));
    const groups = Object.entries(Array.groupBy(scopes, ({ version }) => version)).map(([version, members]) => ({ version, scoped: members.map(({ scope }) => `"${scope}"`).join(' ') }));
    const excluded = scopes.map(({ scope }) => `--exclude '^${RegExp.escape(`${scope}/`)}'`).join(' ');
    const scan = 'ast-grep scan --custom-languages allow';
    const tree = `${scan} --no-ignore hidden --globs '!pnpm-lock.yaml' --globs '!tools/*/examples/**' --error=no-suppress-all ${off.map(({ id }) => `--off=${id}`).join(' ')}`;
    const versioned = (whole: string, hosted: readonly string[], parallel: boolean) => ({
        executor: 'nx:run-commands',
        options: { commands: [whole, ...hosted.map((command) => ({ command, forwardAllArgs: false }))], parallel },
    });
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    'lint:ast-grep': versioned(
                        tree,
                        groups.flatMap(({ version, scoped }) => {
                            const below = off.filter((rule) => !covered(version)(rule));
                            return below.length > 0 ? [`${scan} --filter '^(${below.map(({ id }) => id).join('|')})$' ${scoped}`] : [];
                        }),
                        true,
                    ),
                    rewrite: { command: `python -c "import fcntl, subprocess, sys; lock = open(sys.argv[1], 'w'); fcntl.flock(lock, fcntl.LOCK_EX); sys.exit(subprocess.call(sys.argv[2:]))" .cache/ast-grep/rewrite.lock ${tree} --update-all` },
                    'typecheck:ty': versioned(
                        `ty check ${scopes.map(({ scope }) => `--exclude "${scope}"`).join(' ')}`,
                        groups.map(({ version, scoped }) => `ty check --python-version ${version} ${scoped}`),
                        true,
                    ),
                    'typecheck:mypy': versioned(
                        `mypy ${excluded}`,
                        groups.map(({ version, scoped }) => `mypy --python-version ${version} ${scoped}`),
                        false,
                    ),
                    'lint:refurb': versioned(
                        `python -m tools.refurb --quiet -- ${excluded}`,
                        groups.map(({ version, scoped }) => `python -m tools.refurb --quiet --python-version ${version} ${scoped}`),
                        false,
                    ),
                },
            },
        },
    };
});
const _dependencies = Effect.fnUntraced(function* ({ fileMap, filesToProcess, workspaceRoot }: CreateDependenciesContext) {
    const [path, workspace] = yield* Effect.all([Path.Path, _workspace(workspaceRoot)]);
    const [sources, crates, swift, xcode] = yield* Effect.all(
        [
            _sources(workspaceRoot, filesToProcess),
            workspace.crates,
            workspace.packages,
            Effect.flatMap(
                _glob(workspaceRoot, ['*/**/project.xcproj']),
                Effect.validate(
                    (file: string) =>
                        Effect.map(
                            _decode(_XcodeProject, workspaceRoot, file),
                            flow(
                                Struct.get('packages'),
                                Array.flatMap((row) => (row.kind === 'local' ? [[file, path.join(path.dirname(path.dirname(file)), row.path, _SWIFT_MANIFEST)] as const] : [])),
                            ),
                        ),
                    { concurrency: 'unbounded' },
                ),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const owners = Object.fromEntries(Object.entries(fileMap.projectFileMap).flatMap(([project, files]) => files.map(({ file }) => [file, project] as const)));
    return [
        ...sources,
        ...crates.flatMap(({ manifest, dependencies }) => Array.getSomes(dependencies.map((dependency) => dependency.path)).map((member) => [manifest, path.join(path.relative(workspaceRoot, member), _CARGO_MANIFEST)] as const)),
        ...swift.flatMap(({ manifest, locals }) => locals.map((local) => [manifest, local] as const)),
        ...xcode.flat(),
    ].flatMap(([sourceFile, file]) => Option.toArray(Option.filter(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), ([source, target]) => source !== target)).map(([source, target]): StaticDependency => ({ source, target, sourceFile, type: DependencyType.static })));
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const _runtime = ManagedRuntime.make(Layer.provideMerge(nodeSpawner, Layer.merge(nodeFileSystem, layerPosix)));
const _rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes = [
    `{${_PYPROJECT},*/**/{${Object.keys(_PROJECTS).join(',')}}}`,
    (files, options, context) =>
        _runtime.runPromise(
            Effect.scoped(
                Effect.gen(function* () {
                    const [path, workspace] = yield* Effect.all([Path.Path, _workspace(context.workspaceRoot)]);
                    const inferred = yield* Effect.promise(() => createNodesFromFiles((file) => _runtime.runPromise(_rendered(file === _PYPROJECT ? _pythonVersions(workspace) : _project(file, workspace, files))), files, options, context));
                    return [...inferred, ...(yield* workspace.crates).flatMap(({ manifest, metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))).map((nx) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const))];
                }),
            ),
        ),
];
const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_rendered(_dependencies(context)));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
