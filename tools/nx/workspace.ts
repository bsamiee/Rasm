import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import { type CreateDependencies, type CreateNodes, type CreateNodesContext, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type Optional, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type TargetConfiguration, type Tree } from '@nx/devkit';
import { globWithWorkspaceContext, hashWithWorkspaceContext, PluginCache, readNxJsonFromDisk, workspaceDataDirectory } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Filter, Layer, ManagedRuntime, Option, Path, Predicate, Record, Result, Schema, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parseSync } from 'vite';
import { decodeFile, Mise, Package } from '#/eng/typescript/manifest';
import { fromString, Toml, Yaml } from '#/eng/typescript/parsed';
import { run } from '#/eng/typescript/process';

// --- [TYPES] ---------------------------------------------------------------------------

type Workspace = Effect.Success<ReturnType<typeof openWorkspace>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const CARGO_MANIFEST = 'Cargo.toml';
const SWIFT_MANIFEST = 'Package.swift';
const XCODE_PROJECT = 'project.xcproj';
const PYPROJECT = 'pyproject.toml';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const TESTS = 'tests';
const APPS = 'apps';
const CONTAINERS = /^(?:testcontainers|@testcontainers\/.+)$/u;
const DOTNET = '{workspaceRoot}/.artifacts/dotnet';
const VERSIONS = new Intl.Collator('en', { numeric: true });
const DEPLOYMENT_TARGETS = { 'Xcode.xcconfig': /(?<=^MACOSX_DEPLOYMENT_TARGET = ).+$/mu, '.swiftlint.yml': /(?<=^ {4}macOS_deployment_target: (?<quote>['"]?))[^'"\n]+(?=\k<quote>$)/mu } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Jsonc = fromString('JSON', parseJson);
const Release = Schema.String.check(Schema.isPattern(/^\d+\.\d+$/u));
const PythonRuntime = Schema.fromJsonString(Schema.Struct({ versions: Schema.NonEmptyArray(Schema.String) }).pipe(Schema.encodeKeys({ versions: 'active_versions' })));
const ImportGraph = Schema.fromJsonString(Schema.Record(Schema.String, Schema.Array(Schema.String)));
const Sgconfig = Yaml.pipe(Schema.decodeTo(Schema.Struct({ ruleDirs: Schema.Array(Schema.String) })));
const Rule = Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            id: Schema.String,
            metadata: Schema.Struct({ below: Schema.OptionFromOptionalKey(Release), from: Schema.OptionFromOptionalKey(Release) }).pipe(Schema.encodeKeys({ below: 'python-below', from: 'python-from' }), Schema.withDecodingDefaultKey(Effect.succeed({}))),
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
                coverage: Schema.Struct({ run: Schema.Struct({ dataFile: Schema.String }).pipe(Schema.encodeKeys({ dataFile: 'data_file' })) }),
            }),
        }),
    ),
);
const Member = Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            project: Schema.Struct({ name: Schema.String, scripts: Schema.Record(Schema.String, Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))) }),
            tool: Schema.Struct({
                hatch: Schema.Struct({ build: Schema.Struct({ devModeDirs: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ devModeDirs: 'dev-mode-dirs' })) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ build: { 'dev-mode-dirs': [] } }))),
                deptry: Schema.OptionFromOptionalKey(Schema.Record(Schema.String, Schema.Unknown)),
            }),
        }),
    ),
);
const Manifest = Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const PylockExport = Toml.pipe(Schema.decodeTo(Schema.Struct({ packages: Schema.Array(Schema.Struct({ directory: Schema.OptionFromOptionalKey(Schema.Struct({ path: Schema.String })) })) })));
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
        targets: Schema.Array(
            Schema.Struct({ name: Schema.String, c99name: Schema.String, type: Schema.Literals(['executable', 'library', 'test', 'macro', 'plugin', 'snippet', 'system-target', 'binary']), path: Schema.String, locals: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(
                Schema.encodeKeys({ locals: 'target_dependencies' }),
            ),
        ),
        products: Schema.Array(Schema.Struct({ name: Schema.String, targets: Schema.Array(Schema.String) })),
        platforms: Schema.Array(Schema.Struct({ name: Schema.String, version: Schema.String })),
    }),
);
const XcodeProject = Jsonc.pipe(
    Schema.decodeTo(
        Schema.Struct({
            files: Schema.Array(Schema.Struct({ path: Schema.OptionFromOptionalKey(Schema.String) })),
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
const NxReleaseConfiguration = Schema.Struct({ releaseTag: Schema.Struct({ pattern: Schema.String }), groups: Schema.Record(Schema.String, Schema.Struct({ projects: Schema.ArrayEnsure(Schema.String) })) });
const Released = Schema.Struct({ targets: Schema.Struct({ build: Schema.Struct({ options: Schema.Struct({ env: Schema.Struct({ MinVerTagPrefix: Schema.String, PRODUCT_BUNDLE_IDENTIFIER: Schema.String }) }) }) }) });
const TsConfig = Jsonc.pipe(
    Schema.decodeTo(
        Schema.Struct({
            extends: Schema.ArrayEnsure(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            compilerOptions: Schema.Struct({ typeRoots: Schema.Array(Schema.String).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
        }),
    ),
);
const Hooks = Jsonc.pipe(Schema.decodeTo(Schema.Struct({ modules: Schema.NonEmptyArray(Schema.String) })));
const MSBuildProject = fromString('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [ERRORS] --------------------------------------------------------------------------

class InferenceError extends Schema.TaggedError<InferenceError, { readonly brand: unique symbol }>()('InferenceError', { missing: Schema.String, subject: Schema.String }) {
    override get message(): string {
        return `${this.subject} has no ${this.missing}`;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const glob = (root: string, patterns: string[], exclude?: string[]) => Effect.tryPromise(() => globWithWorkspaceContext(root, patterns, exclude));
const owning = Effect.fnUntraced(function* (root: string, dir: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const folders = [...dir.split(path.sep).map((_, at, segments) => segments.slice(0, segments.length - at).join(path.sep)), '.'];
    const owner = yield* Effect.flatMap(
        Effect.findFirst(folders, (folder) => fs.exists(path.join(root, folder, Package.file))),
        Effect.fromOption,
    );
    return { owner, manifest: yield* Package.read(path.join(root, owner)) };
});
const extendedConfigs: (root: string, file: string, config: typeof TsConfig.Type) => Effect.Effect<readonly string[], Effect.Error<ReturnType<typeof decodeFile>>, Effect.Services<ReturnType<typeof decodeFile>>> = Effect.fnUntraced(function* (root, file, config) {
    const path = yield* Path.Path;
    const parents = config.extends.map((parent) => path.join(path.dirname(file), parent));
    const chains = yield* Effect.forEach(parents, (parent) => Effect.flatMap(decodeFile(TsConfig, root, parent), (held) => extendedConfigs(root, parent, held)));
    return [...parents, ...chains.flat()];
});

// --- [WORKSPACE]
const readPyproject = Effect.fnUntraced(function* (root: string) {
    const [path, { tool }] = yield* Effect.all([Path.Path, decodeFile(Root, root, PYPROJECT)]);
    const members = yield* Effect.forEach(tool.uv.workspace.members, (member) => Effect.map(decodeFile(Member, root, path.join(member, PYPROJECT)), (manifest) => ({ member, ...manifest })));
    return {
        moduleRoots: members.flatMap(({ member, tool: { hatch } }) => hatch.build.devModeDirs.map((folder) => path.join(member, folder))),
        commands: members.filter(({ project }) => !Record.isEmptyRecord(project.scripts)).map(Struct.get('member')),
        deptry: members.filter(({ tool: { deptry } }) => Option.isSome(deptry)),
        requirements: tool.uv.requirements,
        dataFile: tool.coverage.run.dataFile,
        ...tool.pytest,
    };
});
const openWorkspace = Effect.fnUntraced(function* ({ workspaceRoot: root, nxJsonConfiguration }: CreateNodesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const cache = yield* Effect.acquireRelease(
        Effect.sync(() => new PluginCache<Record<'hash' | 'output', string>>(path.join(workspaceDataDirectory, 'workspace.hash'))),
        (held) => Effect.sync(() => held.writeToDisk()),
    );
    const cached = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, inputs: string[], [command, ...args]: readonly [string, ...string[]]) {
        const [key, hash] = [[command, ...args].join(' '), yield* Effect.tryPromise(() => hashWithWorkspaceContext(root, inputs))];
        const text = yield* Option.fromUndefinedOr(cache.get(key)).pipe(
            Option.filter((entry) => entry.hash === hash),
            Option.map(Struct.get('output')),
            Option.match({ onSome: Effect.succeed, onNone: () => run(command, args, { cwd: root }) }),
        );
        return yield* Schema.decodeEffect(schema)(text).pipe(Effect.tap(Effect.sync(() => cache.set(key, { hash, output: text }))));
    });
    return {
        root,
        exported: (project: string) => cached(PylockExport, ['uv.lock', `**/${PYPROJECT}`], ['uv', 'export', '--frozen', '--format', 'pylock.toml', '--no-hashes', '--no-emit-project', '--project', project]),
        ...(yield* Effect.all({
            pyproject: Effect.cached(readPyproject(root)),
            mise: Effect.cached(Mise.read(root)),
            hosts: Effect.cached(fs.readDirectory(path.join(root, APPS))),
            release: Effect.cached(Schema.decodeUnknownEffect(NxReleaseConfiguration)(nxJsonConfiguration.release)),
            python: Effect.cached(cached(PythonRuntime, [Mise.file], ['mise', 'tool', 'python', '--json'])),
            crates: Effect.cached(Effect.map(cached(Metadata, [`**/${CARGO_MANIFEST}`, '**/*.rs'], ['cargo', 'metadata', '--format-version', '1', '--no-deps']), ({ packages }) => packages.map((crate) => ({ ...crate, manifest: path.relative(root, crate.manifest) })))),
            swift: Effect.cached(
                Effect.map(cached(SwiftPackage, [SWIFT_MANIFEST], ['swift', 'package', 'describe', '--type', 'json']), (described) => ({
                    ...described,
                    tests: Array.partition(
                        described.targets,
                        Filter.fromPredicate(({ type }) => type === 'test'),
                    ),
                })),
            ),
        })),
    };
});

// --- [PROJECTS]
const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const tagPrefix = Effect.fnUntraced(function* ({ release }: Workspace, tags: readonly string[], name: string) {
    const { releaseTag, groups } = yield* release;
    const [group] = yield* Effect.fromOption(
        Record.findFirst(groups, ({ projects }) => tags.some((tag) => projects.includes(`tag:${tag}`))),
        () => InferenceError.make({ missing: 'nx.json release group', subject: `${name} (${tags.join(', ')})` }),
    );
    return releaseTag.pattern.replace('{releaseGroupName}', group).replace('{projectName}', name).replace('{version}', '');
});
const suitePath = (path: Path.Path, base: string, dir: string) => path.join(base, ...dir.split(path.sep).filter((segment) => segment !== path.basename(base)));
const pythonTargets = Effect.fnUntraced(function* (dir: string, { root, pyproject, hosts }: Workspace) {
    const [path, { pythonFiles, testPaths, dataFile }, folders] = yield* Effect.all([Path.Path, pyproject, hosts]);
    const suites = yield* Effect.filter(
        testPaths.map((testPath) => suitePath(path, testPath, dir)),
        (suite) => Effect.map(glob(root, [`${suite}/**/{${pythonFiles.join(',')}}`]), Array.isArrayNonEmpty),
    );
    const coverage = `${dataFile}.{projectName}`;
    const pytest = folders.some((host) => dir.split(path.sep).includes(host)) ? { options: { args: suites }, outputs: [] } : { options: { args: [...suites, '--cov'], env: { COVERAGE_FILE: coverage } }, outputs: [`{workspaceRoot}/${coverage}`] };
    return Array.match(suites, { onEmpty: () => ({}), onNonEmpty: () => ({ check: {}, test: { command: 'pytest', inputs: suites.map(folderInput), ...pytest } }) });
});
const suiteInput = (...suites: readonly string[]) => ({ namedInputs: { default: ['{projectRoot}/**/*', ...suites.map(folderInput)] } });
const suiteInputs = Effect.fnUntraced(function* (language: string, dir: string, root: string) {
    const path = yield* Path.Path;
    const suite = suitePath(path, path.join(TESTS, language), dir);
    return Array.match(yield* glob(root, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => suiteInput(suite) });
});
const containerTests = Effect.fnUntraced(function* (dir: string, root: string) {
    const path = yield* Path.Path;
    const importsContainers = (file: string, text: string) => parseSync(file, text).module.staticImports.some(({ moduleRequest }) => CONTAINERS.test(moduleRequest.value));
    const suite = suitePath(path, path.join(TESTS, 'typescript'), dir);
    const tests = yield* Effect.filter(yield* glob(root, [`${suite}/**/*.test.{ts,tsx}`]), (test) => Effect.map(decodeFile(Schema.String, root, test), (text) => importsContainers(test, text)), { concurrency: 'unbounded' });
    return tests.map((test) => path.relative(suite, test));
});
const attachedSuites = <Suite extends { readonly name: string }>(suites: readonly Suite[], module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Array.findFirst(suites, ({ name }) => name === `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: { check: {}, ...Record.fromIterableWith(['build', 'lint:periphery', ...targets], (target) => [target, {}]) },
});
const binlog = (target: string) => `${DOTNET}/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`;
const markers = {
    '*.csproj': Effect.fnUntraced(function* ({ dir }, { file, root }) {
        const [{ Project }, windows] = yield* Effect.all([decodeFile(MSBuildProject, root, file), Effect.map(glob(root, [`${dir}/**/Windows/**/*.cs`]), Array.isArrayNonEmpty)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, configuration: string) => [`${DOTNET}/{bin,obj}/{projectName}/${configuration}*`, binlog(target), ...(plugin ? [`${DOTNET}/{bin,obj}/*/{projectName}_${configuration}*`] : [])];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(windows ? ['platform:windows'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                ...(windows
                    ? {
                          'build:windows': {
                              options: { cwd: '{projectRoot}', args: ['--runtime', 'win-x64', '"-p:ArtifactsPath=$NX_WORKSPACE_ROOT/.artifacts/dotnet/windows"'] },
                              outputs: [`${DOTNET}/windows/{bin,obj}/{projectName}`, binlog('build:windows')],
                          },
                      }
                    : {}),
                check: {},
                ...(plugin ? { pack: {}, install: {}, publish: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, root }) {
        yield* Effect.filterOrFail(decodeFile(Schema.String, root, file), (text) => text.startsWith('#!'));
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                ...Record.fromIterableWith(['build', 'format'], (step) => [step, { executor: 'nx:noop', dependsOn: [{ target: `${step}:*${ext}`, params: 'forward' as const }] }]),
                [`restore:${base}`]: { outputs: [`${DOTNET}/obj/${base}/project.assets.json`, `${DOTNET}/obj/${base}/*.nuget.*`, binlog(`restore:${base}`)], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs: ['default'], outputs: [`${DOTNET}/bin/${base}`, `${DOTNET}/obj/${base}/*/**/*`, binlog(`build:${base}`)], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                check: {},
            },
        };
    }),
    '.swcrc': { tags: ['host:extendscript'], targets: { build: {} } },
    [CARGO_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, root, crates, mise }) {
        const [all, { daemons }, suite] = yield* Effect.all([crates, mise, suiteInputs('rust', dir, root)], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const named = new Set(bins.map(({ name }) => name));
        const launched = Record.keys(Record.filter(daemons, (row) => typeof row !== 'string' && row.boot && Option.exists(row.run, (command) => !named.isDisjointFrom(new Set(command.split(' '))))));
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
        const [{ packages }, { id }, targets] = yield* Effect.all([workspace.exported(dir), decodeFile(Manifest, workspace.root, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        const tags = ['language:python', 'output:extension'];
        return {
            name: id,
            tags,
            targets: {
                pack: { inputs: Array.getSomes(packages.map(Struct.get('directory'))).map(Struct.get('path')).map(folderInput), options: { env: { MinVerTagPrefix: yield* tagPrefix(workspace, tags, id) } } },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XCODE_PROJECT]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decodeFile(XcodeProject, workspace.root, workspace.file)]);
        const [root, name] = [path.dirname(dir), path.parse(dir).name];
        const identifier = yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, (target) => target.name === name),
                ({ settings }) => settings.PRODUCT_BUNDLE_IDENTIFIER,
            ),
            () => InferenceError.make({ missing: 'PRODUCT_BUNDLE_IDENTIFIER', subject: `${workspace.file} target ${name}` }),
        );
        const updated = targets.some(({ members }) => members.some(({ product }) => product === 'Sparkle'));
        const owners = yield* Effect.forEach(
            Array.getSomes(files.map(Struct.get('path')))
                .map((entry) => path.relative('.artifacts', path.join(root, entry)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(owning(workspace.root, path.join(path.dirname(source), path.parse(source).name)), ({ manifest }) => manifest.name),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const tags = ['build:xcode', ...(updated ? ['output:appcast'] : [])];
        const xcode = swiftProject(tags, ['pack', 'install', 'lint:swiftlint', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), MinVerTagPrefix: yield* tagPrefix(workspace, tags, name), PRODUCT_BUNDLE_IDENTIFIER: identifier } };
        return { root, name, ...xcode, targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) } };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots, commands }, targets] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        const command = commands.includes(dir);
        return { name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python', ...(command ? ['output:command'] : [])], targets: { ...targets, ...(command ? { install: {} } : {}) } };
    }),
    'tsconfig.json': Effect.fnUntraced(function* ({ dir }, { file, root, configFiles }) {
        const [path, config] = yield* Effect.all([Path.Path, decodeFile(TsConfig, root, file)]);
        const [suite, configs, { owner, manifest }, containers, entries, hosts] = yield* Effect.all([suiteInputs('typescript', dir, root), extendedConfigs(root, file, config), owning(root, dir), containerTests(dir, root), glob(root, [`${dir}/*/main.ts`]), glob(root, [`${dir}/*/host.ts`])], {
            concurrency: 'unbounded',
        });
        const named = owner === dir ? {} : { name: path.join(manifest.name, path.relative(owner, dir)) };
        const runs = [
            ...entries
                .map(path.dirname)
                .filter((folder) => !configFiles.some((held) => path.dirname(held) === folder))
                .map((folder) => [folder, `node ${path.join(folder, 'main.ts')}`] as const),
            ...hosts.map(path.dirname).map((folder) => [folder, `node ${path.join(dir, 'main.ts')} ${path.basename(folder)}`] as const),
        ].map(([folder, command]) => [path.basename(folder), { command, ...(configFiles.some((held) => held.startsWith(`${folder}${path.sep}`) && path.dirname(held) !== folder) ? { dependsOn: [{ projects: [`directory:${folder}/**`], target: 'build' }] } : {}) }] as const);
        const integration = Array.match(containers, { onEmpty: () => ({}), onNonEmpty: (tests) => ({ test: { options: { args: tests.map((test) => `--exclude='**/${test}'`) } }, 'test:integration': { options: { args: tests } } }) });
        return {
            ...suite,
            ...named,
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': { inputs: [...configs.map((held) => `{workspaceRoot}/${held}`), ...config.compilerOptions.typeRoots.map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.join(dir, typeRoot)}/**/*.d.ts`, includeIgnored: true }))] },
                check: {},
                ...Object.fromEntries(runs),
                ...integration,
                ...(owner === dir && Record.has(manifest.dependencies, '@effect/sql-pg') ? { 'lint:squawk': {} } : {}),
            },
        };
    }),
    'vite.config.ts': ({ dir }, { configFiles }) => Effect.map(Path.Path, (path) => configFiles.includes(path.join(dir, CLOUDFLARE_CONFIG))).pipe(Effect.map((hosted) => ({ tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {}, ...(hosted ? { hosting: {} } : {}) } }))),
    ...Record.fromIterableWith(['uxp', 'cep'] as const, (host) => [`${host}.config.ts` as const, { tags: [`host:${host}`], targets: { build: {}, pack: {}, install: {} } }]),
    'script.config.ts': { tags: ['output:script'], targets: { build: {} } },
    'sea.config.ts': { tags: ['output:sea'], targets: { pack: {} } },
    [CLOUDFLARE_CONFIG]: { targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } },
    '.storybook/main.ts': { targets: { storybook: {} } },
    'hooks/hooks.json': Effect.fnUntraced(function* (_, { file, root }) {
        yield* Effect.fromOption(Schema.decodeOption(Hooks)(yield* decodeFile(Schema.String, root, file)));
        return { tags: ['host:claude'] };
    }),
} as const satisfies Record<string, Optional<ProjectConfiguration, 'root'> | ((file: Path.Path.Parsed, workspace: Workspace & { readonly file: string; readonly configFiles: readonly string[] }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, ManagedRuntime.ManagedRuntime.Services<typeof runtime>>)>;
const infer = Effect.fnUntraced(
    function* (file: string, workspace: Workspace & { readonly configFiles: readonly string[] }) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [marker, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(markers), ([key]) => [parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`].includes(key)));
        const { root: directory = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], ...configuration }: Optional<ProjectConfiguration, 'root'> = Predicate.isFunction(configure) ? yield* configure(parsed, { ...workspace, file }) : configure;
        const hosted = Array.intersection(directory.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        return { projects: { [directory]: { ...configuration, root: directory, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);

// --- [ROOT]
const deptryCheck = (path: Path.Path, hosts: readonly string[], { member, project, tool }: typeof Member.Type & { readonly member: string }) => {
    const roots = [member, ...Array.intersection(member.split(path.sep), hosts).map((host) => path.join(APPS, host))];
    const firstParty = tool.hatch.build.devModeDirs.flatMap((folder) => ['--known-first-party', path.relative(path.join(member, folder), member)]);
    return [`lint:deptry:${project.name}`, { command: ['deptry --config', path.join(member, PYPROJECT), ...firstParty, ...roots].join(' '), inputs: ['python', ...roots.map(folderInput)] }] as const;
};
const pythonVersions = Effect.fnUntraced(function* ({ root, hosts, pyproject, python }: Workspace) {
    const [path, folders, { pythonFiles, testPaths, moduleRoots, requirements, deptry }] = yield* Effect.all([Path.Path, hosts, pyproject]);
    const claimed = moduleRoots.flatMap((moduleRoot) => testPaths.map((testPath) => suitePath(path, testPath, moduleRoot)));
    const unclaimed = yield* glob(
        root,
        testPaths.flatMap((testPath) => pythonFiles.map((pattern) => `${testPath}/**/${pattern}`)),
        claimed.map((testDir) => `${testDir}/**`),
    );
    const [{ versions }, scopes, rules] = yield* Effect.all(
        [
            python,
            Effect.flatMap(
                glob(
                    root,
                    folders.map((host) => `**/${host}/**/${PYPROJECT}`),
                ),
                Effect.validate((file: string) => Effect.map(decodeFile(Host, root, file), ({ project }) => ({ scope: path.dirname(file), version: project.minimum[1] })), { concurrency: 'unbounded' }),
            ),
            decodeFile(Sgconfig, root, 'sgconfig.yml').pipe(
                Effect.flatMap(({ ruleDirs }) =>
                    glob(
                        root,
                        ruleDirs.map((dir) => `${dir}/python/**/*.yml`),
                    ),
                ),
                Effect.flatMap(Effect.validate((file: string) => decodeFile(Rule, root, file), { concurrency: 'unbounded' })),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [interpreter] = versions;
    const reaches = (version: string) => (release: string) => VERSIONS.compare(version, release) >= 0;
    const runs =
        (version: string) =>
        ({ metadata: { below, from } }: (typeof rules)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = rules.filter((rule) => [interpreter, ...scopes.map(({ version }) => version)].some((version) => !runs(version)(rule)));
    const ids = (held: typeof rules) => `--filter '^(${held.map(({ id }) => id).join('|')})$'`;
    const groups = Record.collect(
        Array.groupBy(scopes, ({ version }) => version),
        (version, members) => {
            const environment = `.cache/python/${version}`;
            const excludedGroups = Record.collect(
                Record.filter(requirements, ({ minimum }) => !reaches(version)(minimum[1])),
                (group) => `--no-group ${group}`,
            );
            return {
                version,
                environment,
                scoped: members.map(({ scope }) => `"${scope}"`).join(' '),
                inputs: members.map(Struct.get('scope')).map(folderInput),
                synced: [`export VIRTUAL_ENV=${environment} && uv sync --active --locked --python ${version}`, ...excludedGroups, '&&'].join(' '),
            };
        },
    );
    const excluded = scopes.map(({ scope }) => `--exclude '^${RegExp.escape(`${scope}/`)}'`).join(' ');
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan(['--no-ignore hidden', "--globs '!pnpm-lock.yaml'", "--globs '!tools/*/examples/**'", ...flags]);
    const offs = off.map(({ id }) => `--off=${id}`);
    const tree = walk(['--error=no-suppress-all', ...offs]);
    const hosted = Record.filterMap({ root: [interpreter, (filter: string) => walk([filter, ...scopes.map(({ scope }) => `--globs '!${scope}/**'`)])] as const, ...Object.fromEntries(groups.map(({ version, scoped }) => [version, [version, (filter: string) => scan([filter, scoped])] as const])) }, ([version, command]) =>
        Array.match(off.filter(runs(version)), { onEmpty: () => Result.failVoid, onNonEmpty: (running) => Result.succeed(command(ids(running))) }),
    );
    const lock = `python -c "import fcntl, subprocess, sys; lock = open(sys.argv[1], 'w'); fcntl.flock(lock, fcntl.LOCK_EX); sys.exit(subprocess.call(sys.argv[2:]))" .cache/ast-grep/rewrite.lock`;
    const split = (target: string, children: Readonly<Record<string, TargetConfiguration>>) => {
        const [lead, ...rest] = Object.keys(children).map((child) => `${target}:${child}`);
        return {
            [target]: { dependsOn: [{ target: lead, params: 'forward' }, ...rest] },
            ...Record.mapKeys(children, (child) => `${target}:${child}`),
        };
    };
    const commands = (children: Readonly<Record<string, string>>) => Record.map(children, (command) => ({ command }));
    const versioned = (target: string, command: string, perVersion: (group: (typeof groups)[number]) => string, configuration: TargetConfiguration = {}, after?: string) =>
        split(
            target,
            Record.map(
                { root: { ...configuration, command, inputs: ['pythonFiles'] }, ...Object.fromEntries(groups.map((group) => [group.version, { ...configuration, command: `${group.synced} ${perVersion(group)}`, inputs: group.inputs }])) },
                (child: TargetConfiguration, name) => (after === undefined ? child : { ...child, dependsOn: [`${after}:${name}`] }),
            ),
        );
    const deptryRuns = deptry.map((member) => deptryCheck(path, folders, member));
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', commands({ tree, ...hosted })),
                    ...split(
                        'rewrite',
                        commands(Record.map({ tree: walk(offs), ...hosted }, (command) => `${lock} ${command} --update-all`)),
                    ),
                    ...versioned('typecheck:ty', `ty check ${scopes.map(({ scope }) => `--exclude "${scope}"`).join(' ')}`, ({ version, scoped }) => `ty check --python-version ${version} ${scoped}`, { options: { env: { TY_UV: '1', UV_LOCKED: '1' } } }),
                    ...versioned('typecheck:mypy', `mypy --num-workers auto ${excluded}`, ({ version, environment, scoped }) => `mypy --num-workers auto --python-version ${version} --python-executable ${environment}/bin/python ${scoped}`),
                    ...versioned('lint:refurb', `python -m tools.refurb --quiet -- ${excluded}`, ({ version, environment, scoped }) => `python -m tools.refurb --quiet --python-version ${version} ${scoped} -- --python-executable ${environment}/bin/python`, {}, 'typecheck:mypy'),
                    ...Array.match(deptryRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:deptry': { dependsOn: [{ target: 'lint:deptry:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
                    ...(unclaimed.length > 0 ? { test: { command: 'pytest', options: { args: [...testPaths, ...claimed.map((testDir) => `--ignore=${testDir}`)] }, inputs: ['python', 'pythonFiles'] } } : {}),
                },
            },
        },
    };
});
const swiftModules = Effect.fnUntraced(function* ({ swift, hosts }: Workspace) {
    const [{ targets, platforms, tests }, folders] = yield* Effect.all([swift, hosts]);
    const [suites, sources] = tests;
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
                    'typecheck:swift': { inputs: ['swift', ...targets.map(Struct.get('path')).map(folderInput)] },
                },
            },
        },
    };
});
const roots = { [PYPROJECT]: pythonVersions, [SWIFT_MANIFEST]: swiftModules } as const;

// --- [COMPONENTS]
const components = Effect.fnUntraced(function* (root: string, inferred: CreateNodesResultArray) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const projects = inferred.flatMap(([file, { projects: held = {} }]) => Record.collect(held, (at, project) => ({ file, at, project })));
    const declared = (at: string) => Array.dedupe(projects.filter((row) => row.at === at).flatMap(({ project }) => Object.keys(project.targets ?? {})));
    const apps = Array.getSomes(projects.map((row) => Option.map(Schema.decodeUnknownOption(Released)(row.project), ({ targets }) => ({ ...row, env: targets.build.options.env }))));
    const member = Effect.fnUntraced(function* (at: string) {
        const { name } = yield* Package.read(path.join(root, at));
        return { at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) };
    });
    const component = Effect.fnUntraced(function* ({ file, at: app, env }: (typeof apps)[number]) {
        const { owner } = yield* owning(root, app);
        const nested = owner === '.' ? [] : Array.dedupe(projects.map(Struct.get('at'))).filter((at) => at !== app && !path.relative(owner, at).startsWith('..'));
        const members = yield* Effect.forEach(yield* Effect.filter(nested, (at) => fs.exists(path.join(root, at, Package.file))), member);
        const after = (step: string, needed: string) => (declared(app).includes(step) ? Array.match(members.filter(({ steps }) => steps.includes(needed)).map(Struct.get('name')), { onEmpty: () => ({}), onNonEmpty: (names) => ({ [step]: { dependsOn: [{ projects: names, target: needed }] } }) }) : {});
        const targeted = (steps: readonly string[]) => Object.fromEntries(steps.map((step) => [step, { options: { env } }]));
        return [file, { projects: { ...Object.fromEntries(members.map(({ at, steps }) => [at, { root: at, targets: targeted(steps) }])), [app]: { root: app, implicitDependencies: members.map(Struct.get('name')), targets: { ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(apps, component);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const runtime = ManagedRuntime.make(Layer.merge(layer, layerPosix));
const rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(
        function* (files, options, context) {
            const [path, workspace, { name }] = yield* Effect.all([Path.Path, openWorkspace(context), Package.read(context.workspaceRoot)]);
            const node = (file: string) => Option.match(Record.get(file)(roots), { onNone: () => infer(file, { ...workspace, configFiles: files }), onSome: (root) => root(workspace) });
            const inferred = yield* Effect.promise(() => createNodesFromFiles((file) => runtime.runPromise(rendered(node(file))), files, options, context));
            return [
                ...inferred,
                ...(yield* components(context.workspaceRoot, inferred)),
                ...(yield* workspace.crates).flatMap(({ manifest, metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))).map((nx) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const)),
                ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
            ];
        },
        Effect.scoped,
        (effect) => runtime.runPromise(effect),
    ),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(
    function* (_options, context) {
        const { fileMap, filesToProcess, workspaceRoot } = context;
        const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
        const [sources, crates, { products, tests }, xcode] = yield* Effect.all(
            [
                Array.match(
                    Object.values(filesToProcess.projectFileMap).flatMap((files) => files.map(Struct.get('file')).filter((file) => file.endsWith('.py'))),
                    { onEmpty: () => Effect.succeed(Record.empty<string, readonly string[]>()), onNonEmpty: (files) => Effect.flatMap(run('ruff', ['analyze', 'graph', ...files], { cwd: workspaceRoot }), Schema.decodeEffect(ImportGraph)) },
                ),
                workspace.crates,
                workspace.swift,
                Effect.flatMap(
                    glob(workspaceRoot, [`*/**/${XCODE_PROJECT}`]),
                    Effect.validate((file: string) => Effect.map(decodeFile(XcodeProject, workspaceRoot, file), ({ targets: rows }) => [file, rows.flatMap(Struct.get('members')).map(Struct.get('product'))] as const), { concurrency: 'unbounded' }),
                ),
            ],
            { concurrency: 'unbounded' },
        );
        const [suites, modules] = tests;
        const owners = Object.fromEntries(Object.entries(fileMap.projectFileMap).flatMap(([owner, files]) => files.map(({ file }) => [file, owner] as const)));
        const swift: RawProjectGraphDependency[] = [
            ...modules.flatMap((target) => target.locals.map((name) => [target.name, name] as const)),
            ...modules.flatMap(({ name: owner }) => Array.difference(Object.values(attachedSuites(suites, owner)).flatMap(Struct.get('locals')), [owner]).map((name) => [owner, name] as const)),
        ].map(([source, target]) => ({ source, target, type: DependencyType.implicit }));
        const linked: RawProjectGraphDependency[] = xcode.flatMap(([file, names]) =>
            Array.cartesianWith(Option.toArray(Record.get(owners, file)), products.filter(({ name }) => names.includes(name)).flatMap(Struct.get('targets')), (source, target) => ({ source, target, sourceFile: file, type: DependencyType.static })),
        );
        return [
            ...[
                ...Record.collect(sources, (file, targets) => targets.map((target) => [file, target] as const)).flat(),
                ...crates.flatMap(({ manifest, dependencies }) => Array.getSomes(dependencies.map((dependency) => dependency.path)).map((member) => [manifest, path.join(path.relative(workspaceRoot, member), CARGO_MANIFEST)] as const)),
            ].flatMap(([sourceFile, file]) => Option.toArray(Option.filter(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), ([source, target]) => source !== target)).map(([source, target]) => ({ source, target, sourceFile, type: DependencyType.static }))),
            ...swift,
            ...linked,
        ];
    },
    Effect.scoped,
    rendered,
    (effect) => runtime.runPromise(effect),
);
const deploymentTarget = Effect.fnUntraced(
    function* (tree: Tree) {
        const { platforms } = yield* (yield* openWorkspace({ workspaceRoot: tree.root, nxJsonConfiguration: readNxJsonFromDisk(tree.root) })).swift;
        const { version } = yield* Effect.fromOption(Array.findFirst(platforms, ({ name }) => name === 'macos'));
        const rows = yield* Effect.forEach(Object.entries(DEPLOYMENT_TARGETS), ([file, pattern]) => Effect.map(Effect.fromNullishOr(tree.read(file, 'utf-8')), (text) => ({ file, text, next: text.replace(pattern, version) })));
        const drifted = rows.filter(({ text, next }) => next !== text);
        yield* Effect.forEach(drifted, ({ file, next }) => Effect.sync(() => tree.write(file, next)), { discard: true });
        return Array.match(drifted, { onEmpty: () => undefined, onNonEmpty: (changed) => ({ outOfSyncMessage: `macOS deployment target differs from ${SWIFT_MANIFEST} ${version}`, outOfSyncDetails: changed.map(Struct.get('file')) }) });
    },
    Effect.scoped,
    rendered,
    (effect) => runtime.runPromise(effect),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes, deploymentTarget };
