import { layer as nodeSpawner } from '@effect/platform-node/NodeChildProcessSpawner';
import { layer as nodeFileSystem } from '@effect/platform-node/NodeFileSystem';
import { layerPosix } from '@effect/platform-node/NodePath';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, createNodesFromFiles, DependencyType, type FileMap, type Optional, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Filter, Layer, ManagedRuntime, Option, Path, Record, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parse as parseToml } from 'smol-toml';
import { parse as parseYaml } from 'yaml';
import { stdout } from '#/eng/typescript/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Store = Effect.Success<ReturnType<typeof openStore>>;
type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const SWIFT_MANIFEST = 'Package.swift';
const XCODE_PROJECT = 'project.xcproj';
const PYPROJECT = 'pyproject.toml';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const SWIFTPM = { SWIFTPM_BUILD_DIR: '.cache/swiftpm/build' };
const DEPLOYMENT_ROWS = {
    'Xcode.xcconfig': [/^MACOSX_DEPLOYMENT_TARGET = .+$/mu, (version: string) => `MACOSX_DEPLOYMENT_TARGET = ${version}`],
    '.swiftlint.yml': [/^ {4}macOS_deployment_target: .+$/mu, (version: string) => `    macOS_deployment_target: '${version}'`],
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Parsed = (expected: string, parse: (text: string) => unknown): Schema.decodeTo<Schema.Unknown, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: () => new SchemaIssue.InvalidValue({ expected }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const Toml = Parsed('TOML', parseToml);
const Yaml = Parsed('YAML', parseYaml);
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const Mise = Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            daemons: Schema.Record(Schema.String, Schema.Union([Schema.String, Schema.Struct({ run: Schema.OptionFromOptionalKey(Schema.String), boot: Schema.Boolean.pipe(Schema.withDecodingDefaultKey(Effect.succeed(false))) }).pipe(Schema.encodeKeys({ boot: 'boot_start' }))])),
        }),
    ),
);
const PythonRuntime = Schema.fromJsonString(Schema.NonEmptyArray(Schema.Struct({ version: Schema.String })));
const ImportGraph = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Sgconfig = Yaml.pipe(Schema.decodeTo(Schema.Struct({ ruleDirs: Schema.Array(Schema.String) })));
const Rule = Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            id: Schema.String,
            metadata: Schema.OptionFromOptionalKey(Schema.Struct({ below: Schema.OptionFromOptionalKey(Release), from: Schema.OptionFromOptionalKey(Release) }).pipe(Schema.encodeKeys({ below: 'python-below', from: 'python-from' }))),
        }),
    ),
);
const Host = Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' })) })));
const Root = Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ buildBackend: Schema.Struct({ moduleRoot: Schema.String }).pipe(Schema.encodeKeys({ moduleRoot: 'module-root' })) }).pipe(Schema.encodeKeys({ buildBackend: 'build-backend' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
            }),
        }),
    ),
);
const Manifest = Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const Metadata = Schema.fromJsonString(
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
const SwiftPackage = Schema.fromJsonString(
    Schema.Struct({
        targets: Schema.Array(Schema.Struct({ name: Schema.String, type: Schema.String, path: Schema.String, locals: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.encodeKeys({ locals: 'target_dependencies' }))),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
        platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String })),
    }),
);
const XcodeProject = Parsed('JSON', parseJson).pipe(
    Schema.decodeTo(
        Schema.Struct({
            files: Schema.Array(Schema.Unknown),
            targets: Schema.Array(Schema.Struct({ members: Schema.Array(Schema.Struct({ product: Schema.String }).pipe(Schema.encodeKeys({ product: 'product-name' }))).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.encodeKeys({ members: 'package-product-members' }))),
        }),
    ),
);
const XcodeFolder = Schema.Struct({ kind: Schema.Literal('folder'), path: Schema.String });
const PackageName = Schema.fromJsonString(Schema.Struct({ name: Schema.String }));
const MSBuildProject = Parsed('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const glob = (root: string, patterns: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(root, patterns));
const decode = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, root: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(root, file)), Schema.decodeEffect(schema));
});
const openStore = (root: string) =>
    Effect.acquireRelease(
        Effect.map(Path.Path, (path) => ({ root, cache: new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.hash')) })),
        ({ cache }) => Effect.sync(() => cache.writeToDisk()),
    );
const cached = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, { root, cache }: Store, { inputs, argv: [command, ...args], env }: { readonly inputs: string[]; readonly argv: [string, ...string[]]; readonly env?: Record<string, string> }) {
    const [key, hash] = [[command, ...args].join(' '), yield* Effect.tryPromise(() => hashWithWorkspaceContext(root, inputs))];
    const text = yield* Option.fromUndefinedOr(cache.get(key)).pipe(
        Option.filter((entry) => entry.hash === hash),
        Option.map(Struct.get('output')),
        Option.match({ onSome: Effect.succeed, onNone: () => stdout(command, args, { cwd: root, env, extendEnv: true }) }),
    );
    return yield* Effect.tap(Schema.decodeEffect(schema)(text), () => Effect.sync(() => cache.set(key, { hash, output: text })));
});

// --- [WORKSPACE]
const readCrates = Effect.fnUntraced(function* (store: Store) {
    const [path, { packages }] = yield* Effect.all([Path.Path, cached(Metadata, store, { inputs: [`**/${CARGO_MANIFEST}`, '**/*.rs'], argv: ['cargo', 'metadata', '--format-version', '1', '--no-deps'] })]);
    return packages.map((crate) => ({ ...crate, manifest: path.relative(store.root, crate.manifest) }));
});
const openWorkspace = Effect.fnUntraced(function* (root: string) {
    const [fs, path, store] = yield* Effect.all([FileSystem.FileSystem, Path.Path, openStore(root)]);
    return {
        ...store,
        ...(yield* Effect.all({
            pyproject: Effect.cached(Effect.map(decode(Root, root, PYPROJECT), ({ tool }) => ({ moduleRoot: tool.uv.buildBackend.moduleRoot, ...tool.pytest }))),
            mise: Effect.cached(decode(Mise, root, 'mise.toml')),
            hosts: Effect.cached(fs.readDirectory(path.join(root, 'apps'))),
            crates: Effect.cached(readCrates(store)),
            swift: Effect.cached(cached(SwiftPackage, store, { inputs: [SWIFT_MANIFEST], argv: ['swift', 'package', 'describe', '--type', 'json'], env: SWIFTPM })),
        })),
    };
});

// --- [PROJECTS]
const pythonTargets = Effect.fnUntraced(function* (dir: string, { root, pyproject }: Workspace) {
    const [path, { pythonFiles, testPaths }] = yield* Effect.all([Path.Path, pyproject]);
    const mirrors = testPaths.map((testPath) => path.join(testPath, ...dir.split(path.sep).filter((segment) => segment !== 'python')));
    const suites = yield* Effect.filter(
        mirrors,
        (mirror) =>
            Effect.map(
                glob(
                    root,
                    pythonFiles.map((pattern) => `${mirror}/**/${pattern}`),
                ),
                Array.isArrayNonEmpty,
            ),
        { concurrency: 'unbounded' },
    );
    return suites.length > 0 ? { check: {}, test: { command: 'pytest', options: { args: [...suites, '--cov'] }, inputs: suites.map((suite) => `{workspaceRoot}/${suite}/**/*`) } } : {};
});
const suiteInput = (suite: string) => ({ namedInputs: { default: ['{projectRoot}/**/*', `{workspaceRoot}/${suite}/**/*`] } });
const suiteInputs = Effect.fnUntraced(function* (language: string, dir: string, root: string) {
    const path = yield* Path.Path;
    const suite = path.join('tests', language, ...dir.split(path.sep).filter((segment) => segment !== language));
    return Array.match(yield* glob(root, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => suiteInput(suite) });
});
const swiftProject = (tags: readonly string[], env: Record<string, string>, targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: { check: {}, ...Record.fromIterableWith(['build', 'lint:periphery', ...targets], (target) => [target, { options: { env } }]) },
});
const markers = {
    '*.csproj': Effect.fnUntraced(function* (_, { file, root }) {
        const { Project } = yield* decode(MSBuildProject, root, file);
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
                ...(plugin ? { pack: {}, install: {}, publish: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { file, root }) {
        yield* Effect.filterOrFail(decode(Schema.String, root, file), (text) => text.startsWith('#!'));
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
    [CARGO_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, root, crates, mise }) {
        const [all, { daemons }, suite] = yield* Effect.all([crates, mise, suiteInputs('rust', dir, root)], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const named = new Set(bins.map(({ name }) => name));
        const launched = Object.entries(daemons).flatMap(([daemon, row]) => (typeof row !== 'string' && row.boot && Option.exists(row.run, (run) => !named.isDisjointFrom(new Set(run.split(' ')))) ? [daemon] : []));
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
                ...(launched.length > 0 ? { install: { options: { args: launched } }, 'install:cargo': {}, 'upgrade:cargo': { options: { args: launched }, dependsOn: [{ projects: ['directory:.'], target: 'upgrade:rust' }] } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ moduleRoot }, { id }, targets] = yield* Effect.all([workspace.pyproject, decode(Manifest, workspace.root, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        return {
            name: id,
            tags: ['language:python', 'output:extension'],
            targets: {
                pack: { inputs: [`{workspaceRoot}/${moduleRoot}/**/*`] },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XCODE_PROJECT]: Effect.fnUntraced(function* ({ dir }, { file, root: workspace }) {
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decode(XcodeProject, workspace, file)]);
        const root = path.dirname(dir);
        const updated = targets.some(({ members }) => members.some(({ product }) => product === 'Sparkle'));
        const bundled = yield* Effect.forEach(
            files
                .filter(Schema.is(XcodeFolder))
                .map((folder) => path.relative('.artifacts', path.join(root, folder.path)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(decode(PackageName, workspace, path.join(source, 'package.json')), ({ name }) => ({ projects: [name], target: 'build' })),
        );
        const xcode = swiftProject(['build:xcode', ...(updated ? ['output:appcast'] : [])], { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root) }, ['pack', 'install', 'lint:swiftlint', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        return { root, name: path.parse(dir).name, ...xcode, targets: Record.map(xcode.targets, (target, name) => (['build', 'pack'].includes(name) ? { ...target, dependsOn: bundled } : target)) };
    }),
    'py.typed': ({ dir }, workspace) => Effect.map(Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]), ([path, { moduleRoot }, targets]) => ({ name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python'], targets })),
    'tsconfig.json': ({ dir }, { root }) => Effect.map(suiteInputs('typescript', dir, root), (suite) => ({ ...suite, tags: ['language:typescript'], targets: { 'typecheck:tsc': {}, check: {} } })),
    'vite.config.ts': ({ dir }, { configFiles }) => Effect.map(Path.Path, (path) => configFiles.includes(path.join(dir, CLOUDFLARE_CONFIG))).pipe(Effect.map((hosted) => ({ tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {}, ...(hosted ? { hosting: {} } : {}) } }))),
    'uxp.config.ts': () => Effect.succeed({ tags: ['host:uxp'], targets: { build: {}, install: {} } }),
    'script.config.ts': () => Effect.succeed({ tags: ['output:script'], targets: { build: {} } }),
    [CLOUDFLARE_CONFIG]: () => Effect.succeed({ targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } }),
    '.storybook/main.ts': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), targets: { storybook: {} } })),
} as const satisfies Record<string, (file: Path.Path.Parsed, workspace: Workspace & { readonly file: string; readonly configFiles: readonly string[] }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>>;
const infer = Effect.fnUntraced(
    function* (file: string, workspace: Workspace, configFiles: readonly string[]) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(markers), ([marker]) => [parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`].includes(marker)));
        const { root: directory = parsed.dir, tags = [], ...configuration }: Optional<ProjectConfiguration, 'root'> = yield* configure(parsed, { ...workspace, file, configFiles });
        const hosted = Array.intersection(directory.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        return { projects: { [directory]: { ...configuration, root: directory, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);

// --- [ROOT]
const pythonVersions = Effect.fnUntraced(function* ({ root, cache, hosts }: Workspace) {
    const [path, folders] = yield* Effect.all([Path.Path, hosts]);
    const [[interpreter], scopes, gated] = yield* Effect.all(
        [
            cached(PythonRuntime, { root, cache }, { inputs: ['mise.toml'], argv: ['mise', 'ls', 'python', '--current', '--json'] }),
            Effect.flatMap(
                glob(
                    root,
                    folders.map((host) => `**/${host}/**/${PYPROJECT}`),
                ),
                Effect.validate((file: string) => Effect.map(decode(Host, root, file), ({ project }) => ({ scope: path.dirname(file), version: project.minimum[1] })), { concurrency: 'unbounded' }),
            ),
            decode(Sgconfig, root, 'sgconfig.yml').pipe(
                Effect.flatMap(({ ruleDirs }) =>
                    glob(
                        root,
                        ruleDirs.map((dir) => `${dir}/python/**/*.yml`),
                    ),
                ),
                Effect.flatMap(Effect.validate((file: string) => decode(Rule, root, file), { concurrency: 'unbounded' })),
                Effect.map(Array.flatMap(({ id, metadata }) => Option.toArray(Option.map(metadata, ({ below, from }) => ({ id, below, from }))))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const reaches = (version: string) => (release: string) => version.localeCompare(release, 'en', { numeric: true }) >= 0;
    const runs =
        (version: string) =>
        ({ below, from }: (typeof gated)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = gated.filter((rule) => [interpreter.version, ...scopes.map(({ version }) => version)].some((version) => !runs(version)(rule)));
    const ids = (rules: typeof gated) => `--filter '^(${rules.map(({ id }) => id).join('|')})$'`;
    const groups = Object.entries(Array.groupBy(scopes, ({ version }) => version)).map(([version, members]) => ({ version, scoped: members.map(({ scope }) => `"${scope}"`).join(' ') }));
    const excluded = scopes.map(({ scope }) => `--exclude '^${RegExp.escape(`${scope}/`)}'`).join(' ');
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan(['--no-ignore hidden', "--globs '!pnpm-lock.yaml'", "--globs '!tools/*/examples/**'", ...flags]);
    const tree = walk(['--error=no-suppress-all', ...off.map(({ id }) => `--off=${id}`)]);
    const rooted = off.filter(runs(interpreter.version));
    const hosted = [
        ...(rooted.length > 0 ? [walk([ids(rooted), ...scopes.map(({ scope }) => `--globs '!${scope}/**'`)])] : []),
        ...groups.flatMap(({ version, scoped }) => {
            const running = off.filter(runs(version));
            return running.length > 0 ? [scan([ids(running), scoped])] : [];
        }),
    ];
    const rewrite = `python -c "import fcntl, subprocess, sys; lock = open(sys.argv[1], 'w'); fcntl.flock(lock, fcntl.LOCK_EX); sys.exit(subprocess.call(sys.argv[2:]))" .cache/ast-grep/rewrite.lock ${walk(['--update-all'])}`;
    const versioned = (whole: string, rest: readonly string[], parallel: boolean) => ({
        executor: 'nx:run-commands',
        options: { commands: [whole, ...rest.map((command) => ({ command, forwardAllArgs: false }))], parallel },
    });
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    'lint:ast-grep': versioned(tree, hosted, true),
                    rewrite: { command: rewrite },
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
const swiftModules = Effect.fnUntraced(function* ({ swift, hosts }: Workspace) {
    const [{ targets, platforms }, folders] = yield* Effect.all([swift, hosts]);
    const [suites, sources] = Array.partition(
        targets,
        Filter.fromPredicate(({ type }) => type === 'test'),
    );
    const platformed = platforms.map(Struct.get('name'));
    const modules = sources.map((target) => {
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, target.path.split('/')), folders).map((host) => `host:${host}`)], SWIFTPM, target.type === 'executable' ? ['install', 'upgrade:swiftpm'] : []);
        const suite = Array.findFirst(suites, ({ name }) => name === `${target.name}Tests`);
        return [
            target.path,
            {
                ...project,
                root: target.path,
                name: target.name,
                ...Option.match(suite, {
                    onNone: () => ({}),
                    onSome: ({ name, path }) => ({ ...suiteInput(path), targets: { ...project.targets, test: { options: { env: SWIFTPM, args: ['--filter', `'^${name}\\.'`] } } } }),
                }),
            },
        ] as const;
    });
    const check = { SWIFTPM_BUILD_DIR: '.cache/swiftpm/check' };
    return {
        projects: {
            ...Object.fromEntries(modules),
            '.': {
                root: '.',
                targets: {
                    'upgrade:swift': { options: { env: SWIFTPM } },
                    'typecheck:swift': { inputs: ['swift', ...targets.map(({ path }) => `{workspaceRoot}/${path}/**/*`)], outputs: [`{workspaceRoot}/${check.SWIFTPM_BUILD_DIR}/swiftc.log`], options: { env: check } },
                    'lint:swiftlint': { options: { env: check, args: targets.map(({ path }) => path) } },
                },
            },
        },
    };
});

// --- [DEPENDENCIES]
const imports = (root: string, { projectFileMap }: FileMap) =>
    Array.match(
        Object.values(projectFileMap).flatMap((files) => files.map(({ file }) => file).filter((file) => file.endsWith('.py'))),
        {
            onEmpty: () => Effect.succeed([]),
            onNonEmpty: (files) => Effect.map(Effect.flatMap(stdout('ruff', ['analyze', 'graph', ...files], { cwd: root }), Schema.decodeEffect(ImportGraph)), (graph) => Object.entries(graph).flatMap(([file, targets]) => targets.map((target) => [file, target] as const))),
        },
    );
const edges = Effect.fnUntraced(function* ({ fileMap, filesToProcess, workspaceRoot }: CreateDependenciesContext) {
    const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(workspaceRoot)]);
    const [sources, crates, { targets, products }, xcode] = yield* Effect.all(
        [
            imports(workspaceRoot, filesToProcess),
            workspace.crates,
            workspace.swift,
            Effect.flatMap(
                glob(workspaceRoot, [`*/**/${XCODE_PROJECT}`]),
                Effect.validate((file: string) => Effect.map(decode(XcodeProject, workspaceRoot, file), ({ targets: rows }) => [file, rows.flatMap(Struct.get('members')).map(Struct.get('product'))] as const), { concurrency: 'unbounded' }),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const owners = Object.fromEntries(Object.entries(fileMap.projectFileMap).flatMap(([owner, files]) => files.map(({ file }) => [file, owner] as const)));
    const modules: RawProjectGraphDependency[] = targets.filter(({ type }) => type !== 'test').flatMap((target) => target.locals.map((name) => ({ source: target.name, target: name, type: DependencyType.implicit })));
    const linked: RawProjectGraphDependency[] = xcode.flatMap(([file, names]) => Array.cartesianWith(Option.toArray(Record.get(owners, file)), products.filter(({ name }) => names.includes(name)).flatMap(Struct.get('targets')), (source, target) => ({ source, target, sourceFile: file, type: DependencyType.static })));
    return [
        ...[...sources, ...crates.flatMap(({ manifest, dependencies }) => Array.getSomes(dependencies.map((dependency) => dependency.path)).map((member) => [manifest, path.join(path.relative(workspaceRoot, member), CARGO_MANIFEST)] as const))].flatMap(([sourceFile, file]) =>
            Option.toArray(Option.filter(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), ([source, target]) => source !== target)).map(([source, target]) => ({ source, target, sourceFile, type: DependencyType.static })),
        ),
        ...modules,
        ...linked,
    ];
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.provideMerge(nodeSpawner, Layer.merge(nodeFileSystem, layerPosix)));
const rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes = [
    `{${PYPROJECT},${SWIFT_MANIFEST},*/**/{${Object.keys(markers).join(',')}}}`,
    (files, options, context) =>
        runtime.runPromise(
            Effect.scoped(
                Effect.gen(function* () {
                    const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context.workspaceRoot)]);
                    const roots = { [PYPROJECT]: pythonVersions, [SWIFT_MANIFEST]: swiftModules };
                    const node = (file: string) => Option.match(Record.get(file)(roots), { onNone: () => infer(file, workspace, files), onSome: (root) => root(workspace) });
                    const inferred = yield* Effect.promise(() => createNodesFromFiles((file) => runtime.runPromise(rendered(node(file))), files, options, context));
                    return [...inferred, ...(yield* workspace.crates).flatMap(({ manifest, metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))).map((nx) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const))];
                }),
            ),
        ),
];
const createDependencies: CreateDependencies = (_options, context) => runtime.runPromise(rendered(edges(context)));
const deploymentTarget = (tree: Tree) =>
    runtime.runPromise(
        rendered(
            Effect.scoped(
                Effect.gen(function* () {
                    const { platforms } = yield* (yield* openWorkspace(tree.root)).swift;
                    const { version } = yield* Effect.fromOption(Array.findFirst(platforms, ({ name }) => name === 'macos'));
                    const rows = yield* Effect.forEach(Object.entries(DEPLOYMENT_ROWS), ([file, [pattern, row]]) => Effect.map(Effect.fromNullishOr(tree.read(file, 'utf-8')), (text) => ({ file, text, next: text.replace(pattern, row(version)) })));
                    const drifted = rows.filter(({ text, next }) => next !== text);
                    yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(() => tree.write(file, next)), { discard: true });
                    return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `${changed.map(Struct.get('file')).join(', ')} differ from Package.swift macOS deployment target` }) });
                }),
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, deploymentTarget };
