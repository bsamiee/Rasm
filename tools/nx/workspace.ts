import { layer as nodeSpawner } from '@effect/platform-node/NodeChildProcessSpawner';
import { layer as nodeFileSystem } from '@effect/platform-node/NodeFileSystem';
import { layerPosix } from '@effect/platform-node/NodePath';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type FileMap, type Optional, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type TargetConfiguration, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Filter, Layer, ManagedRuntime, Option, Path, Record, Result, Schema, SchemaGetter, SchemaIssue, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parse as parseToml } from 'smol-toml';
import { parseSync } from 'vite';
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
const TESTS = 'tests';
const CONTAINERS = /^(?:testcontainers|@testcontainers\/.+)$/u;
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
const PythonRuntime = Schema.fromJsonString(Schema.Struct({ versions: Schema.NonEmptyArray(Schema.String) }).pipe(Schema.encodeKeys({ versions: 'active_versions' })));
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
const PythonRequirement = Schema.Struct({ minimum: Schema.TemplateLiteralParser(['>=', Schema.Trim.pipe(Schema.decodeTo(Release))]) }).pipe(Schema.encodeKeys({ minimum: 'requires-python' }));
const Host = Toml.pipe(Schema.decodeTo(Schema.Struct({ project: PythonRequirement })));
const Root = Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ workspace: Schema.Struct({ members: Schema.Array(Schema.String) }), requirements: Schema.Record(Schema.String, PythonRequirement) }).pipe(Schema.encodeKeys({ requirements: 'dependency-groups' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
            }),
        }),
    ),
);
const Member = Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                hatch: Schema.OptionFromOptionalKey(Schema.Struct({ build: Schema.Struct({ devModeDirs: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ devModeDirs: 'dev-mode-dirs' })) })),
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
        targets: Schema.Array(Schema.Struct({ name: Schema.String, c99name: Schema.String, type: Schema.String, path: Schema.String, locals: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.encodeKeys({ locals: 'target_dependencies' }))),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
        platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String })),
    }),
);
const XcodeProject = Parsed('JSON', parseJson).pipe(
    Schema.decodeTo(
        Schema.Struct({
            files: Schema.Array(Schema.Unknown),
            targets: Schema.Array(
                Schema.Struct({
                    name: Schema.String,
                    members: Schema.Array(Schema.Struct({ product: Schema.String }).pipe(Schema.encodeKeys({ product: 'product-name' }))).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                    settings: Schema.Struct({ PRODUCT_BUNDLE_IDENTIFIER: Schema.OptionFromOptionalKey(Schema.String) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
                }).pipe(Schema.encodeKeys({ members: 'package-product-members', settings: 'build-settings' })),
            ),
        }),
    ),
);
const NxJson = Parsed('JSON', parseJson).pipe(Schema.decodeTo(Schema.Struct({ release: Schema.Struct({ releaseTag: Schema.Struct({ pattern: Schema.String }), groups: Schema.Record(Schema.String, Schema.Struct({ projects: Schema.ArrayEnsure(Schema.String) })) }) })));
const Released = Schema.Struct({ targets: Schema.Struct({ build: Schema.Struct({ options: Schema.Struct({ env: Schema.Struct({ MinVerTagPrefix: Schema.String, PRODUCT_BUNDLE_IDENTIFIER: Schema.String }) }) }) }) });
const XcodeEntry = Schema.Struct({ path: Schema.String });
const TsConfig = Parsed('JSON', parseJson).pipe(
    Schema.decodeTo(
        Schema.Struct({
            extends: Schema.ArrayEnsure(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            compilerOptions: Schema.Struct({ typeRoots: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        }),
    ),
);
const Package = Schema.fromJsonString(Schema.Struct({ name: Schema.String, dependencies: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }));
const Hooks = Parsed('JSON', parseJson).pipe(Schema.decodeTo(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) })));
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
const cached = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, { root, cache }: Store, { inputs, argv: [command, ...args] }: { readonly inputs: string[]; readonly argv: [string, ...string[]] }) {
    const [key, hash] = [[command, ...args].join(' '), yield* Effect.tryPromise(() => hashWithWorkspaceContext(root, inputs))];
    const text = yield* Option.fromUndefinedOr(cache.get(key)).pipe(
        Option.filter((entry) => entry.hash === hash),
        Option.map(Struct.get('output')),
        Option.match({ onSome: Effect.succeed, onNone: () => stdout(command, args, { cwd: root }) }),
    );
    return yield* Effect.tap(Schema.decodeEffect(schema)(text), () => Effect.sync(() => cache.set(key, { hash, output: text })));
});
const owning = Effect.fnUntraced(function* (root: string, dir: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folders = [...dir.split(path.sep).map((_, at, segments) => segments.slice(0, segments.length - at).join(path.sep)), '.'];
    const owner = yield* Effect.flatMap(
        Effect.findFirst(folders, (folder) => fs.exists(path.join(root, folder, 'package.json'))),
        Effect.fromOption,
    );
    return { owner, manifest: yield* decode(Package, root, path.join(owner, 'package.json')) };
});
const extendedConfigs: (root: string, file: string) => Effect.Effect<readonly string[], Effect.Error<ReturnType<typeof decode>>, Effect.Services<ReturnType<typeof decode>>> = Effect.fnUntraced(function* (root: string, file: string) {
    const [path, config] = yield* Effect.all([Path.Path, decode(TsConfig, root, file)]);
    const parents = config.extends.map((parent) => path.join(path.dirname(file), parent));
    const chains = yield* Effect.forEach(parents, (parent) => extendedConfigs(root, parent));
    return [...parents, ...chains.flat()];
});

// --- [WORKSPACE]
const readCrates = Effect.fnUntraced(function* (store: Store) {
    const [path, { packages }] = yield* Effect.all([Path.Path, cached(Metadata, store, { inputs: [`**/${CARGO_MANIFEST}`, '**/*.rs'], argv: ['cargo', 'metadata', '--format-version', '1', '--no-deps'] })]);
    return packages.map((crate) => ({ ...crate, manifest: path.relative(store.root, crate.manifest) }));
});
const readPyproject = Effect.fnUntraced(function* (root: string) {
    const [path, { tool }] = yield* Effect.all([Path.Path, decode(Root, root, PYPROJECT)]);
    const { members } = tool.uv.workspace;
    const manifests = yield* Effect.forEach(members, (member) => decode(Member, root, path.join(member, PYPROJECT)));
    const moduleRoots = Array.zip(members, manifests).flatMap(([member, { tool: held }]) => Option.match(held.hatch, { onNone: () => [], onSome: ({ build }) => build.devModeDirs.map((folder) => path.join(member, folder)) }));
    return { moduleRoots, requirements: tool.uv.requirements, ...tool.pytest };
});
const openWorkspace = Effect.fnUntraced(function* (root: string) {
    const [fs, path, store] = yield* Effect.all([FileSystem.FileSystem, Path.Path, openStore(root)]);
    return {
        ...store,
        ...(yield* Effect.all({
            pyproject: Effect.cached(readPyproject(root)),
            mise: Effect.cached(decode(Mise, root, 'mise.toml')),
            nx: Effect.cached(decode(NxJson, root, 'nx.json')),
            hosts: Effect.cached(fs.readDirectory(path.join(root, 'apps'))),
            crates: Effect.cached(readCrates(store)),
            swift: Effect.cached(cached(SwiftPackage, store, { inputs: [SWIFT_MANIFEST], argv: ['swift', 'package', 'describe', '--type', 'json'] })),
        })),
    };
});

// --- [PROJECTS]
const tagPrefix = Effect.fnUntraced(function* ({ nx }: Workspace, tags: readonly string[], name: string) {
    const { release } = yield* nx;
    const [group] = yield* Effect.fromOption(Record.findFirst(release.groups, ({ projects }) => tags.some((tag) => projects.includes(`tag:${tag}`))));
    return release.releaseTag.pattern.replace('{releaseGroupName}', group).replace('{projectName}', name).replace('{version}', '');
});
const testDirs = (path: Path.Path, testPaths: readonly string[], dir: string) => testPaths.map((testPath) => path.join(testPath, ...dir.split(path.sep).filter((segment) => segment !== 'python')));
const pythonTargets = Effect.fnUntraced(function* (dir: string, { root, pyproject, hosts }: Workspace) {
    const [path, { pythonFiles, testPaths }, folders] = yield* Effect.all([Path.Path, pyproject, hosts]);
    const suites = yield* Effect.filter(
        testDirs(path, testPaths, dir),
        (testDir) =>
            Effect.map(
                glob(
                    root,
                    pythonFiles.map((pattern) => `${testDir}/**/${pattern}`),
                ),
                Array.isArrayNonEmpty,
            ),
        { concurrency: 'unbounded' },
    );
    const covered = !folders.some((host) => dir.split(path.sep).includes(host));
    const coverage = '.artifacts/python/coverage/data/.coverage.{projectName}';
    return suites.length > 0
        ? {
              check: {},
              test: {
                  command: 'pytest',
                  options: covered ? { args: [...suites, '--cov'], env: { COVERAGE_FILE: coverage } } : { args: suites },
                  inputs: suites.map((suite) => `{workspaceRoot}/${suite}/**/*`),
                  outputs: covered ? [`{workspaceRoot}/${coverage}`] : [],
              },
          }
        : {};
});
const suiteInput = (...suites: readonly string[]) => ({ namedInputs: { default: ['{projectRoot}/**/*', ...suites.map((suite) => `{workspaceRoot}/${suite}/**/*`)] } });
const suitePath = (path: Path.Path, language: string, dir: string) => path.join(TESTS, language, ...dir.split(path.sep).filter((segment) => segment !== language));
const suiteInputs = Effect.fnUntraced(function* (language: string, dir: string, root: string) {
    const suite = suitePath(yield* Path.Path, language, dir);
    return Array.match(yield* glob(root, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => suiteInput(suite) });
});
const containerTests = Effect.fnUntraced(function* (dir: string, root: string) {
    const path = yield* Path.Path;
    const importsContainers = (file: string, text: string) => parseSync(file, text).module.staticImports.some(({ moduleRequest }) => CONTAINERS.test(moduleRequest.value));
    const suite = suitePath(path, 'typescript', dir);
    const tests = yield* Effect.filter(yield* glob(root, [`${suite}/**/*.test.{ts,tsx}`]), (test) => Effect.map(decode(Schema.String, root, test), (text) => importsContainers(test, text)), { concurrency: 'unbounded' });
    return tests.map((test) => path.relative(suite, test));
});
const attachedSuites = <Suite extends { readonly name: string }>(suites: readonly Suite[], module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Array.findFirst(suites, ({ name }) => name === `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: { check: {}, ...Record.fromIterableWith(['build', 'lint:periphery', ...targets], (target) => [target, {}]) },
});
const markers = {
    '*.csproj': Effect.fnUntraced(function* ({ dir }, { file, root }) {
        const [{ Project }, windows] = yield* Effect.all([decode(MSBuildProject, root, file), Effect.map(glob(root, [`${dir}/**/Windows/**/*.cs`]), Array.isArrayNonEmpty)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, configuration: string) => [
            `{workspaceRoot}/.artifacts/dotnet/{bin,obj}/{projectName}/${configuration}*`,
            `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`,
            ...(plugin ? [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/*/{projectName}_${configuration}*`] : []),
        ];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(windows ? ['platform:windows'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                ...(windows
                    ? {
                          'build:windows': {
                              options: { cwd: '{projectRoot}', args: ['--runtime', 'win-x64', '"-p:ArtifactsPath=$NX_WORKSPACE_ROOT/.artifacts/dotnet/windows"'] },
                              outputs: ['{workspaceRoot}/.artifacts/dotnet/windows/{bin,obj}/{projectName}', '{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/build-windows.binlog'],
                          },
                      }
                    : {}),
                check: {},
                ...(plugin ? { pack: {}, install: {}, publish: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { file, root }) {
        yield* Effect.filterOrFail(decode(Schema.String, root, file), (text) => text.startsWith('#!'));
        const binlog = (target: string) => `{workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target}-${base}.binlog`;
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: 'build:*.cs', params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: 'format:*.cs', params: 'forward' }] },
                [`restore:${base}`]: { outputs: [`{workspaceRoot}/.artifacts/dotnet/obj/${base}/project.assets.json`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*.nuget.*`, binlog('restore')], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs: ['default'], outputs: [`{workspaceRoot}/.artifacts/dotnet/bin/${base}`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*/**/*`, binlog('build')], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
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
        const launched = Array.filterMap(Object.entries(daemons), ([daemon, row]) => (typeof row !== 'string' && row.boot && Option.exists(row.run, (run) => !named.isDisjointFrom(new Set(run.split(' ')))) ? Result.succeed(daemon) : Result.failVoid));
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
        const [{ moduleRoots }, { id }, targets] = yield* Effect.all([workspace.pyproject, decode(Manifest, workspace.root, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        const tags = ['language:python', 'output:extension'];
        return {
            name: id,
            tags,
            targets: {
                pack: { inputs: moduleRoots.map((moduleRoot) => `{workspaceRoot}/${moduleRoot}/**/*`), options: { env: { MinVerTagPrefix: yield* tagPrefix(workspace, tags, id) } } },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XCODE_PROJECT]: Effect.fnUntraced(function* ({ dir }, held) {
        const { file, root: workspace } = held;
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decode(XcodeProject, workspace, file)]);
        const [root, name] = [path.dirname(dir), path.parse(dir).name];
        const identifier = yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, (target) => target.name === name),
                ({ settings }) => settings.PRODUCT_BUNDLE_IDENTIFIER,
            ),
        );
        const updated = targets.some(({ members }) => members.some(({ product }) => product === 'Sparkle'));
        const owners = yield* Effect.forEach(
            files
                .filter(Schema.is(XcodeEntry))
                .map((entry) => path.relative('.artifacts', path.join(root, entry.path)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(owning(workspace, path.join(path.dirname(source), path.parse(source).name)), ({ manifest }) => manifest.name),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const tags = ['build:xcode', ...(updated ? ['output:appcast'] : [])];
        const xcode = swiftProject(tags, ['pack', 'install', 'lint:swiftlint', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), MinVerTagPrefix: yield* tagPrefix(held, tags, name), PRODUCT_BUNDLE_IDENTIFIER: identifier } };
        return { root, name, ...xcode, targets: Record.map(xcode.targets, (target, step) => ({ ...target, options, ...(['build', 'pack'].includes(step) ? { dependsOn: bundled } : {}) })) };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots }, targets] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        return { name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python'], targets };
    }),
    'tsconfig.json': Effect.fnUntraced(function* ({ dir }, { file, root, configFiles }) {
        const path = yield* Path.Path;
        const [suite, configs, { compilerOptions }, { owner, manifest }, containers, entries, hosts] = yield* Effect.all(
            [suiteInputs('typescript', dir, root), extendedConfigs(root, file), decode(TsConfig, root, file), owning(root, dir), containerTests(dir, root), glob(root, [`${dir}/*/main.ts`]), glob(root, [`${dir}/*/host.ts`])],
            {
                concurrency: 'unbounded',
            },
        );
        const named = owner === dir ? {} : { name: path.join(manifest.name, path.relative(owner, dir)) };
        const below = (folder: string) => configFiles.filter((config) => config.startsWith(`${folder}${path.sep}`) && path.dirname(config) !== folder);
        const building = (folder: string) => (below(folder).length > 0 ? { dependsOn: [{ projects: [`directory:${folder}/**`], target: 'build' }] } : {});
        const runs = [
            ...entries.flatMap((entry) => {
                const folder = path.dirname(entry);
                return configFiles.some((config) => path.dirname(config) === folder) ? [] : [[path.basename(folder), { command: `node ${entry}`, ...building(folder) }] as const];
            }),
            ...hosts.map((entry) => [path.basename(path.dirname(entry)), { command: `node ${path.join(dir, 'main.ts')} ${path.basename(path.dirname(entry))}`, ...building(path.dirname(entry)) }] as const),
        ];
        const integration = Array.match(containers, { onEmpty: () => ({}), onNonEmpty: (tests) => ({ test: { options: { args: tests.map((test) => `--exclude='**/${test}'`) } }, 'test:integration': { options: { args: tests } } }) });
        return {
            ...suite,
            ...named,
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': { inputs: [...configs.map((config) => `{workspaceRoot}/${config}`), ...compilerOptions.typeRoots.map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.join(dir, typeRoot)}/**/*.d.ts`, includeIgnored: true }))] },
                check: {},
                ...Object.fromEntries(runs),
                ...integration,
                ...(owner === dir && Record.has(manifest.dependencies, '@effect/sql-pg') ? { 'lint:squawk': {} } : {}),
            },
        };
    }),
    'vite.config.ts': ({ dir }, { configFiles }) => Effect.map(Path.Path, (path) => configFiles.includes(path.join(dir, CLOUDFLARE_CONFIG))).pipe(Effect.map((hosted) => ({ tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {}, ...(hosted ? { hosting: {} } : {}) } }))),
    'uxp.config.ts': () => Effect.succeed({ tags: ['host:uxp'], targets: { build: {}, pack: {}, install: {} } }),
    'cep.config.ts': () => Effect.succeed({ tags: ['host:cep'], targets: { build: {}, pack: {}, install: {} } }),
    'script.config.ts': () => Effect.succeed({ tags: ['output:script'], targets: { build: {} } }),
    'sea.config.ts': () => Effect.succeed({ tags: ['output:sea'], targets: { pack: {} } }),
    [CLOUDFLARE_CONFIG]: () => Effect.succeed({ targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } }),
    '.storybook/main.ts': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), targets: { storybook: {} } })),
    'hooks/hooks.json': Effect.fnUntraced(function* ({ dir }, { file, root }) {
        const [path, text] = yield* Effect.all([Path.Path, decode(Schema.String, root, file)]);
        yield* Effect.fromOption(Schema.decodeOption(Hooks)(text));
        return { root: path.dirname(dir), tags: ['host:claude'] };
    }),
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
const pythonVersions = Effect.fnUntraced(function* ({ root, cache, hosts, pyproject }: Workspace) {
    const [path, folders, { pythonFiles, testPaths, moduleRoots, requirements }] = yield* Effect.all([Path.Path, hosts, pyproject]);
    const claimed = moduleRoots.flatMap((moduleRoot) => testDirs(path, testPaths, moduleRoot));
    const unclaimed = (yield* glob(
        root,
        testPaths.flatMap((testPath) => pythonFiles.map((pattern) => `${testPath}/**/${pattern}`)),
    )).filter((file) => !claimed.some((testDir) => !path.relative(testDir, file).startsWith('..')));
    const [
        {
            versions: [interpreter],
        },
        scopes,
        gated,
    ] = yield* Effect.all(
        [
            cached(PythonRuntime, { root, cache }, { inputs: ['mise.toml'], argv: ['mise', 'tool', 'python', '--json'] }),
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
                Effect.map(Array.filterMap(({ id, metadata }) => Option.match(metadata, { onNone: () => Result.failVoid, onSome: ({ below, from }) => Result.succeed({ id, below, from }) }))),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const reaches = (version: string) => (release: string) => version.localeCompare(release, 'en', { numeric: true }) >= 0;
    const runs =
        (version: string) =>
        ({ below, from }: (typeof gated)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = gated.filter((rule) => [interpreter, ...scopes.map(({ version }) => version)].some((version) => !runs(version)(rule)));
    const ids = (rules: typeof gated) => `--filter '^(${rules.map(({ id }) => id).join('|')})$'`;
    const groups = Record.collect(
        Array.groupBy(scopes, ({ version }) => version),
        (version, members) => {
            const environment = `.cache/python/${version}`;
            const excludedGroups = Array.filterMap(Object.entries(requirements), ([group, { minimum }]) => (reaches(version)(minimum[1]) ? Result.failVoid : Result.succeed(`--no-group ${group}`)));
            return { version, environment, scoped: members.map(({ scope }) => `"${scope}"`).join(' '), synced: [`export VIRTUAL_ENV=${environment} && uv sync --active --python ${version}`, ...excludedGroups, '&&'].join(' ') };
        },
    );
    const excluded = scopes.map(({ scope }) => `--exclude '^${RegExp.escape(`${scope}/`)}'`).join(' ');
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan(['--no-ignore hidden', "--globs '!pnpm-lock.yaml'", "--globs '!tools/*/examples/**'", ...flags]);
    const offs = off.map(({ id }) => `--off=${id}`);
    const tree = walk(['--error=no-suppress-all', ...offs]);
    const rooted = off.filter(runs(interpreter));
    const hosted = Object.fromEntries([
        ...(rooted.length > 0 ? [['root', walk([ids(rooted), ...scopes.map(({ scope }) => `--globs '!${scope}/**'`)])] as const] : []),
        ...groups.flatMap(({ version, scoped }) => {
            const running = off.filter(runs(version));
            return running.length > 0 ? [[version, scan([ids(running), scoped])] as const] : [];
        }),
    ]);
    const lock = `python -c "import fcntl, subprocess, sys; lock = open(sys.argv[1], 'w'); fcntl.flock(lock, fcntl.LOCK_EX); sys.exit(subprocess.call(sys.argv[2:]))" .cache/ast-grep/rewrite.lock`;
    const split = (target: string, children: Readonly<Record<string, string>>) => {
        const [lead, ...rest] = Object.keys(children).map((child) => `${target}:${child}`);
        return {
            [target]: { dependsOn: [{ target: lead, params: 'forward' }, ...rest] },
            ...Record.mapEntries(children, (command, child) => [`${target}:${child}`, { command }]),
        };
    };
    const perVersion = (command: (group: (typeof groups)[number]) => string) => Object.fromEntries(groups.map((group) => [group.version, command(group)]));
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', { tree, ...hosted }),
                    ...split(
                        'rewrite',
                        Record.map({ tree: walk(offs), ...hosted }, (command) => `${lock} ${command} --update-all`),
                    ),
                    ...split('typecheck:ty', { root: `ty check ${scopes.map(({ scope }) => `--exclude "${scope}"`).join(' ')}`, ...perVersion(({ version, scoped, synced }) => `${synced} ty check --python-version ${version} ${scoped}`) }),
                    ...split('typecheck:mypy', {
                        root: `mypy ${excluded}`,
                        ...perVersion(({ version, environment, scoped, synced }) => `${synced} mypy --python-version ${version} --python-executable ${environment}/bin/python ${scoped}`),
                    }),
                    ...split('lint:refurb', {
                        root: `python -m tools.refurb --quiet -- ${excluded}`,
                        ...perVersion(({ version, environment, scoped, synced }) => `${synced} python -m tools.refurb --quiet --python-version ${version} ${scoped} -- --python-executable ${environment}/bin/python`),
                    }),
                    ...(unclaimed.length > 0 ? { test: { command: 'pytest', options: { args: [...testPaths, ...claimed.map((testDir) => `--ignore=${testDir}`)] }, inputs: ['python', 'pythonFiles'] } } : {}),
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
    const excluded = Array.match(suites, { onEmpty: () => ({}), onNonEmpty: (all) => ({ options: { args: ['--exclude-targets', ...all.map(Struct.get('c99name'))] } }) });
    const modules = sources.map((target) => {
        const segments = target.path.split('/');
        const installed = target.type === 'executable' && segments[0] !== TESTS;
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, segments), folders).map((host) => `host:${host}`)], installed ? ['install', 'upgrade:swiftpm'] : []);
        const attached = attachedSuites(suites, target.name);
        return [
            target.path,
            {
                ...project,
                ...Array.match(Object.values(attached), { onEmpty: () => ({}), onNonEmpty: (held) => suiteInput(...held.map(Struct.get('path'))) }),
                root: target.path,
                name: target.name,
                targets: { ...project.targets, 'lint:periphery': excluded, ...Record.map(attached, ({ name }) => ({ options: { args: ['--test-product', name] } })) },
            },
        ] as const;
    });
    return {
        projects: {
            ...Object.fromEntries(modules),
            '.': {
                root: '.',
                targets: {
                    'typecheck:swift': { inputs: ['swift', ...targets.map(({ path }) => `{workspaceRoot}/${path}/**/*`)] },
                },
            },
        },
    };
});

// --- [COMPONENTS]
const components = Effect.fnUntraced(function* (root: string, inferred: CreateNodesResultArray) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const projects = inferred.flatMap(([file, { projects: held = {} }]) => Record.collect(held, (at, project) => ({ file, at, project })));
    const declared = (at: string) => Array.dedupe(projects.filter((row) => row.at === at).flatMap(({ project }) => Object.keys(project.targets ?? {})));
    const apps = Array.getSomes(projects.map((row) => Option.map(Schema.decodeUnknownOption(Released)(row.project), ({ targets }) => ({ ...row, env: targets.build.options.env }))));
    const member = Effect.fnUntraced(function* (at: string) {
        const { name } = yield* decode(Package, root, path.join(at, 'package.json'));
        return { at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) };
    });
    const component = Effect.fnUntraced(function* ({ file, at: app, env }: (typeof apps)[number]) {
        const { owner } = yield* owning(root, app);
        const nested = owner === '.' ? [] : Array.dedupe(projects.map(Struct.get('at'))).filter((at) => at !== app && !path.relative(owner, at).startsWith('..'));
        const members = yield* Effect.forEach(yield* Effect.filter(nested, (at) => fs.exists(path.join(root, at, 'package.json'))), member);
        const holding = (step: string) => members.filter(({ steps }) => steps.includes(step)).map(Struct.get('name'));
        const after = (step: string, needed: string) => (declared(app).includes(step) ? Array.match(holding(needed), { onEmpty: () => ({}), onNonEmpty: (names) => ({ [step]: { dependsOn: [{ projects: names, target: needed }] } }) }) : {});
        const targeted = (steps: readonly string[]) => Object.fromEntries(steps.map((step) => [step, { options: { env } }]));
        return [file, { projects: { ...Object.fromEntries(members.map(({ at, steps }) => [at, { root: at, targets: targeted(steps) }])), [app]: { root: app, implicitDependencies: members.map(Struct.get('name')), targets: { ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(apps, component);
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
    const [suites, modules] = Array.partition(
        targets,
        Filter.fromPredicate(({ type }) => type === 'test'),
    );
    const swift: RawProjectGraphDependency[] = [
        ...modules.flatMap((target) => target.locals.map((name) => [target.name, name] as const)),
        ...modules.flatMap(({ name: owner }) => Array.difference(Object.values(attachedSuites(suites, owner)).flatMap(Struct.get('locals')), [owner]).map((name) => [owner, name] as const)),
    ].map(([source, target]) => ({ source, target, type: DependencyType.implicit }));
    const linked: RawProjectGraphDependency[] = xcode.flatMap(([file, names]) => Array.cartesianWith(Option.toArray(Record.get(owners, file)), products.filter(({ name }) => names.includes(name)).flatMap(Struct.get('targets')), (source, target) => ({ source, target, sourceFile: file, type: DependencyType.static })));
    return [
        ...[...sources, ...crates.flatMap(({ manifest, dependencies }) => Array.getSomes(dependencies.map((dependency) => dependency.path)).map((member) => [manifest, path.join(path.relative(workspaceRoot, member), CARGO_MANIFEST)] as const))].flatMap(([sourceFile, file]) =>
            Option.toArray(Option.filter(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), ([source, target]) => source !== target)).map(([source, target]) => ({ source, target, sourceFile, type: DependencyType.static })),
        ),
        ...swift,
        ...linked,
    ];
}, Effect.scoped);

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.provideMerge(nodeSpawner, Layer.merge(nodeFileSystem, layerPosix)));
const rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${PYPROJECT},${SWIFT_MANIFEST},*/**/{${Object.keys(markers).join(',')}}}`,
    (files, options, context) =>
        runtime.runPromise(
            Effect.scoped(
                Effect.gen(function* () {
                    const [path, workspace, { name }] = yield* Effect.all([Path.Path, openWorkspace(context.workspaceRoot), decode(Package, context.workspaceRoot, 'package.json')]);
                    const roots = { [PYPROJECT]: pythonVersions, [SWIFT_MANIFEST]: swiftModules };
                    const node = (file: string) => Option.match(Record.get(file)(roots), { onNone: () => infer(file, workspace, files), onSome: (root) => root(workspace) });
                    const inferred = yield* Effect.promise(() => createNodesFromFiles((file) => runtime.runPromise(rendered(node(file))), files, options, context));
                    return [
                        ...inferred,
                        ...(yield* components(context.workspaceRoot, inferred)),
                        ...(yield* workspace.crates).flatMap(({ manifest, metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))).map((nx) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const)),
                        ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
                    ];
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
