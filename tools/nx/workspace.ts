import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResult, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type Optional, type ProjectConfiguration, parseJson, type RawProjectGraphDependency, type TargetConfiguration } from '@nx/devkit';
import { Array, Boolean, Effect, Equal, Function, type ManagedRuntime, Option, Order, Path, Predicate, Record, Result, Schema, String, Struct, Tuple } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { packageDirectory } from 'package-directory';
import { quote } from 'shell-quote';
import { parseSync } from 'vite';
import { parseAllDocuments } from 'yaml';
import { ClaudeHooks, decodeFile, extendScriptEntry, libraryRoot, Package } from '#/eng/typescript/manifest';
import { runCommand } from '#/eng/typescript/run-command';
import { TextFormat } from '#/eng/typescript/text-format';
import { APPS, CARGO_MANIFEST, glob, JsonListRecord, openWorkspace, PYPROJECT, PythonRequirement, Release, SWIFT_MANIFEST, type Workspace } from '#/tools/nx/internal/open-workspace';
import { type pluginRuntime, runPlugin } from '#/tools/nx/internal/plugin-runtime';
import { WorkspaceError } from '#/tools/nx/internal/workspace-error';

// --- [TYPES] ---------------------------------------------------------------------------

interface Application {
    readonly root: string;
    readonly env: Readonly<Record<string, string>>;
}
type Inference = Workspace & { readonly configFiles: readonly string[]; readonly applications: Map<string, Application>; readonly rooted: Map<string, readonly string[]> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const XCODE_PROJECT = 'project.xcproj';
const CLOUDFLARE_CONFIG = 'cloudflare.config.ts';
const TESTS = 'tests';
const CONTAINERS = /^(?:testcontainers|@testcontainers\/.+)$/u;
const DOTNET = '{workspaceRoot}/.artifacts/dotnet';
const VERSIONS = new Intl.Collator('en', { numeric: true });
const SGCONFIG = 'sgconfig.yml';
const GRAMMARS = 'tools/ast-grep/grammars';
const GRAMMAR = 'grammar.js';
const AST_GREP_BUILT_INS: Readonly<Record<string, readonly string[]>> = { yaml: ['yml', 'yaml'] };

// --- [MODELS] --------------------------------------------------------------------------

const Sgconfig = TextFormat.Yaml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            ruleDirs: Schema.Array(Schema.String),
            utilDirs: Schema.Array(Schema.String),
            testConfigs: Schema.Array(Schema.Struct({ testDir: Schema.String })),
            customLanguages: Schema.Record(Schema.String, Schema.Struct({ libraryPath: Schema.String, extensions: Schema.Array(Schema.String), outlineRules: Schema.OptionFromOptionalKey(Schema.String) })),
            languageGlobs: Schema.Record(Schema.String, Schema.Array(Schema.String)),
            languageInjections: Schema.Array(Schema.Struct({ hostLanguage: Schema.String })),
        }),
    ),
);
const YamlStream = TextFormat('YAML stream', (text) => parseAllDocuments(text).map((document) => Result.getOrThrow(Array.match(document.errors, { onEmpty: () => Result.succeed(document.toJS()), onNonEmpty: ([error]) => Result.fail(error) }))));
const AstGrepRules = (stem: string) =>
    YamlStream.pipe(
        Schema.decodeTo(
            Schema.Array(
                Schema.Struct({
                    id: Schema.String.pipe(Schema.withDecodingDefaultKey(Effect.succeed(stem))),
                    language: Schema.String,
                    metadata: Schema.Struct({ below: Schema.OptionFromOptionalKey(Release), from: Schema.OptionFromOptionalKey(Release) }).pipe(Schema.encodeKeys({ below: 'python-below', from: 'python-from' }), Schema.withDecodingDefaultKey(Effect.succeed({}))),
                }),
            ),
        ),
    );
const HostPyproject = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ project: PythonRequirement })));
const BlenderManifest = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const Jsonc = TextFormat('JSON', parseJson);
const TreeSitterGrammar = Jsonc.pipe(Schema.decodeTo(Schema.Struct({ name: Schema.String, inherits: Schema.OptionFromOptionalKey(Schema.String), externals: Schema.Array(Schema.Unknown) })));
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
const MSBuildProject = TextFormat('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]
const packageName = (specifier: string) =>
    specifier
        .split('/')
        .slice(0, specifier.startsWith('@') ? 2 : 1)
        .join('/');
const moduleRequests = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    return parseSync(file, yield* decodeFile(Schema.String, workspaceRoot, file)).module.staticImports.map(({ moduleRequest }) => moduleRequest.value);
});
const imports = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const [path, requests] = yield* Effect.all([Path.Path, moduleRequests(workspaceRoot, file)]);
    const [files, specifiers] = Array.partition(requests, (request) => (request.startsWith('.') ? Result.succeed(path.join(path.dirname(file), request)) : Result.fail(request)));
    return { files, specifiers };
});
const reach = (workspaceRoot: string, frontier: readonly string[], graph: Readonly<Record<string, Effect.Success<ReturnType<typeof imports>>>>): Effect.Effect<Readonly<Record<string, Effect.Success<ReturnType<typeof imports>>>>, Effect.Error<ReturnType<typeof imports>>, Effect.Services<ReturnType<typeof imports>>> =>
    Array.match(Array.dedupe(frontier.filter((file) => !Record.has(graph, file))), {
        onEmpty: () => Effect.succeed(graph),
        onNonEmpty: (pending) =>
            Effect.flatMap(
                Effect.forEach(pending, (file) => imports(workspaceRoot, file), { concurrency: 'unbounded' }),
                (found) => reach(workspaceRoot, found.flatMap(Struct.get('files')), { ...graph, ...Object.fromEntries(Array.zip(pending, found)) }),
            ),
    });
const owning = Effect.fnUntraced(function* (workspaceRoot: string, dir: string) {
    const path = yield* Path.Path;
    const directory = yield* Effect.flatMap(
        Effect.tryPromise(() => packageDirectory({ cwd: path.join(workspaceRoot, dir) })),
        Effect.fromNullishOr,
    );
    return { owner: path.normalize(path.relative(workspaceRoot, directory)), manifest: yield* Package.read(directory) };
});

// --- [PROJECTS]
const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const extensionInput = (extensions: readonly string[]) => `{workspaceRoot}/**/*.${extensions.length > 1 ? `{${extensions.join(',')}}` : extensions.join(',')}`;
const tagPrefix = Effect.fnUntraced(function* ({ release }: Workspace, tags: readonly string[], name: string) {
    const { releaseTag, groups } = yield* release;
    const selectors = new Set(tags.map((tag) => `tag:${tag}`));
    const [group] = yield* Effect.fromOption(
        Record.findFirst(groups, ({ projects }) => !selectors.isDisjointFrom(new Set(projects))),
        () => WorkspaceError.missing(`${name} (${tags.join(', ')})`, 'nx.json release group'),
    );
    return releaseTag.pattern.replace('{releaseGroupName}', group).replace('{projectName}', name).replace('{version}', '');
});
const suitePath = (path: Path.Path, base: string, dir: string) => path.join(base, ...dir.split(path.sep).filter((segment) => segment !== path.basename(base)));
const pythonTargets = Effect.fnUntraced(function* (dir: string, { workspaceRoot, pyproject, hosts }: Workspace) {
    const [path, { pythonFiles, testPaths, dataFile, members }, folders] = yield* Effect.all([Path.Path, pyproject, hosts]);
    const suites = yield* Effect.filter(
        testPaths.map((testPath) => suitePath(path, testPath, dir)),
        (suite) => Effect.map(glob(workspaceRoot, [`${suite}/**/{${pythonFiles.join(',')}}`]), Array.isArrayNonEmpty),
    );
    const segments = dir.split(path.sep);
    const coverage = `${dataFile}.{projectName}`;
    const command =
        segments.includes('blender') && Array.isArrayNonEmpty(suites)
            ? `python -m eng.python.blender run {projectName} --package ${quote([yield* Effect.fromOption(Record.get(members, Array.headNonEmpty(suites)), () => WorkspaceError.missing(Array.headNonEmpty(suites), 'workspace package'))])} --groups test -- --python-exit-code 1 --python ${quote([path.join(workspaceRoot, 'tests/python/libs/blender/bpy/background/session.py')])} -- -n0`
            : 'pytest';
    const pytest = folders.some((host) => segments.includes(host)) ? { options: { args: suites }, outputs: [] } : { options: { args: [...suites, '--cov'], env: { COVERAGE_FILE: coverage } }, outputs: [`{workspaceRoot}/${coverage}`] };
    return Array.match(suites, { onEmpty: () => ({}), onNonEmpty: () => ({ check: {}, test: { command, inputs: suites.map(folderInput), ...pytest } }) });
});
const suiteInput = (...suites: readonly string[]) => ({ namedInputs: { default: ['{projectRoot}/**/*', ...suites.map(folderInput)] } });
const suiteInputs = Effect.fnUntraced(function* (language: string, dir: string, workspaceRoot: string) {
    const path = yield* Path.Path;
    const suite = suitePath(path, path.join(TESTS, language), dir);
    return Array.match(yield* glob(workspaceRoot, [`${suite}/**/*`]), { onEmpty: () => ({}), onNonEmpty: () => suiteInput(suite) });
});
const containerTests = Effect.fnUntraced(function* (dir: string, workspaceRoot: string) {
    const path = yield* Path.Path;
    const suite = suitePath(path, path.join(TESTS, 'typescript'), dir);
    const tests = yield* Effect.filter(yield* glob(workspaceRoot, [`${suite}/**/*.test.{ts,tsx}`]), (test) => Effect.map(moduleRequests(workspaceRoot, test), Array.some(CONTAINERS.test.bind(CONTAINERS))), { concurrency: 'unbounded' });
    return tests.map((test) => path.relative(suite, test));
});
const attachedSuites = <Suite>(suites: Readonly<Record<string, Suite>>, module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Record.get(suites, `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: { check: {}, ...Record.fromIterableWith(['build', 'lint:periphery', ...targets], (target) => [target, {}]) },
});
const binlog = (target: string) => `${DOTNET}/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`;
const markers = {
    '*.csproj': Effect.fnUntraced(function* ({ dir }, { file, workspaceRoot }) {
        const [{ Project }, windows] = yield* Effect.all([decodeFile(MSBuildProject, workspaceRoot, file), Effect.map(glob(workspaceRoot, [`${dir}/**/Windows/**/*.cs`]), Array.isArrayNonEmpty)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (target: string, configuration: string) => [`${DOTNET}/{bin,obj}/{projectName}/${configuration}*`, binlog(target), ...(plugin ? [`${DOTNET}/{bin,obj}/*/{projectName}_${configuration}*`] : [])];
        return {
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(windows ? ['platform:windows'] : [])],
            targets: {
                build: { outputs: outputs('build', 'debug') },
                'build:release': { outputs: outputs('build:release', 'release'), defaultConfiguration: 'release' },
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
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, workspaceRoot }) {
        yield* Effect.filterOrFail(decodeFile(Schema.String, workspaceRoot, file), (text) => text.startsWith('#!'));
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
    [CARGO_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { file, workspaceRoot, crates, mise }) {
        const [path, all, { daemons }] = yield* Effect.all([Path.Path, crates, mise], { concurrency: 'unbounded' });
        const crate = yield* Effect.fromOption(Array.findFirst(all, ({ manifest }) => manifest === file));
        const sources = Array.dedupe(crate.targets.map(({ source }) => path.dirname(path.relative(workspaceRoot, source)))).filter((source) => path.relative(dir, source).split(path.sep, 1)[0] === '..');
        const bins = crate.targets.filter(({ kind }) => kind.includes('bin'));
        const named = new Set(bins.map(({ name }) => name));
        const commands = Record.getSomes(Record.map(daemons, (row) => (typeof row === 'string' || !row.boot ? Option.none() : row.run)));
        const launched = Record.keys(Record.filter(commands, (command) => !named.isDisjointFrom(new Set(command.split(' ')))));
        const tests = crate.targets.filter(Struct.get('test'));
        const tested = Array.isArrayNonEmpty(tests);
        const dylint = crate.dependencies.some(({ name }) => name === 'dylint_linting');
        const library = ({ kind, types }: (typeof crate.targets)[number]) => Array.intersection(Array.difference(kind, ['bin']), types).length > 0;
        const selection = tested ? { options: { args: crate.targets.some(library) ? ['--lib', '--bins', '--tests'] : ['--all-targets'] } } : {};
        const args = tests.flatMap((target) => (library(target) ? ['--lib'] : [`--${Array.headNonEmpty(target.kind)}`, target.name]));
        return {
            ...(sources.length > 0 ? suiteInput(...sources) : {}),
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            targets: {
                'typecheck:clippy': { ...selection },
                'lint:cargo-modules': {},
                'lint:dylint': { ...selection },
                check: {},
                ...(tested ? { test: { options: { args } }, ...(dylint ? {} : { coverage: { options: { args } } }) } : {}),
                ...(bins.length > 0 ? { build: {} } : {}),
                ...(launched.length > 0 ? { install: { options: { args: launched } }, 'install:cargo': {}, 'upgrade:cargo': { options: { args: launched }, dependsOn: [{ projects: ['directory:.'], target: 'upgrade:rust' }] } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ packages }, { id }, targets] = yield* Effect.all([workspace.lockedPackages(dir), decodeFile(BlenderManifest, workspace.workspaceRoot, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        const tags = ['language:python', 'output:extension'];
        return {
            name: id,
            tags,
            targets: {
                pack: {
                    inputs: Array.getSomes(packages.map(Struct.get('directory')))
                        .map(Struct.get('path'))
                        .map(folderInput),
                    options: { env: { MinVerTagPrefix: yield* tagPrefix(workspace, tags, id) } },
                },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XCODE_PROJECT]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decodeFile(XcodeProject, workspace.workspaceRoot, workspace.file)]);
        const [root, name] = [path.dirname(dir), path.parse(dir).name];
        const identifier = yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, (target) => target.name === name),
                ({ settings }) => settings.PRODUCT_BUNDLE_IDENTIFIER,
            ),
            () => WorkspaceError.missing(`${workspace.file} target ${name}`, 'PRODUCT_BUNDLE_IDENTIFIER'),
        );
        const updated = targets.flatMap(Struct.get('members')).some(Predicate.Struct({ product: Equal.equals('Sparkle') }));
        const owners = yield* Effect.forEach(
            Array.getSomes(files.map(Struct.get('path')))
                .map((entry) => path.relative('.artifacts', path.join(root, entry)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(owning(workspace.workspaceRoot, path.join(path.dirname(source), path.parse(source).name)), Function.flow(Struct.get('manifest'), Struct.get('name'))),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const tags = ['build:xcode', ...(updated ? ['output:appcast'] : [])];
        const xcode = swiftProject(tags, ['pack', 'install', 'lint:swiftlint', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), MinVerTagPrefix: yield* tagPrefix(workspace, tags, name), PRODUCT_BUNDLE_IDENTIFIER: identifier } };
        yield* Effect.sync(() => workspace.applications.set(workspace.file, { root, env: options.env }));
        return { root, name, ...xcode, targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) } };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots, commands }, targets] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        const command = commands.includes(dir);
        return { name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python', ...(command ? ['output:command'] : [])], targets: { ...targets, ...(command ? { install: {} } : {}) } };
    }),
    'tsconfig.json': Effect.fnUntraced(function* ({ dir }, { file, workspaceRoot, configs, configFiles, rooted }) {
        const [path, readConfig] = yield* Effect.all([Path.Path, configs]);
        const {
            config: { options, errors },
            files,
        } = yield* Effect.try(() => readConfig(file));
        yield* Array.match(errors, { onEmpty: () => Effect.void, onNonEmpty: Effect.fail });
        const [suite, { owner, manifest }, containers, entries, hosts] = yield* Effect.all([suiteInputs('typescript', dir, workspaceRoot), owning(workspaceRoot, dir), containerTests(dir, workspaceRoot), glob(workspaceRoot, [`${dir}/*/main.ts`]), glob(workspaceRoot, [`${dir}/*/host.ts`])], {
            concurrency: 'unbounded',
        });
        const cruised = Array.getSomes([libraryRoot(manifest), extendScriptEntry(manifest)]).map((entry) => path.join(dir, entry));
        yield* libraryRoot(manifest).pipe(
            Option.filter(() => owner === dir),
            Option.as(Effect.sync(rooted.set.bind(rooted, manifest.name, cruised))),
            Effect.transposeOption,
        );
        const named = owner === dir ? {} : { name: path.join(manifest.name, path.relative(owner, dir)) };
        const configured = configFiles.map(path.dirname);
        const mains = entries.map(path.dirname);
        const bundled = configFiles.filter((held) => path.basename(held) === 'vite.config.ts').flatMap((held) => mains.filter(String.startsWith(`${path.dirname(held)}${path.sep}`)));
        const runs = [...Array.difference(mains, [...configured, ...bundled]).map((folder) => [folder, `node ${path.join(folder, 'main.ts')}`] as const), ...hosts.map(path.dirname).map((folder) => [folder, `node ${path.join(dir, 'main.ts')} ${path.basename(folder)}`] as const)].map(
            ([folder, command]) => [path.basename(folder), { command, ...(configured.some(Predicate.and(Predicate.not(Equal.equals(folder)), String.startsWith(`${folder}${path.sep}`))) ? { dependsOn: [{ projects: [`directory:${folder}/**`], target: 'build' }] } : {}) }] as const,
        );
        const integration = Array.match(containers, { onEmpty: () => ({}), onNonEmpty: (tests) => ({ test: { options: { args: tests.map((test) => `--exclude='**/${test}'`) } }, 'test:integration': { options: { args: tests } } }) });
        return {
            ...suite,
            ...named,
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': { inputs: [...files.map((held) => ({ fileset: `{workspaceRoot}/${held}`, includeIgnored: true })), ...(options.typeRoots ?? []).map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.relative(workspaceRoot, typeRoot)}/**/*.d.ts`, includeIgnored: true }))] },
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
    [ClaudeHooks.file]: Effect.fnUntraced(function* (_, { file, workspaceRoot }) {
        yield* Effect.fromOption(Schema.decodeOption(ClaudeHooks.schema)(yield* decodeFile(Schema.String, workspaceRoot, file)));
        return { tags: ['host:claude'], targets: { 'lint:claude': {}, 'typecheck:claude': {} } };
    }),
} as const satisfies Record<string, Optional<ProjectConfiguration, 'root'> | ((file: Path.Path.Parsed, workspace: Inference & { readonly file: string }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, ManagedRuntime.ManagedRuntime.Services<typeof pluginRuntime>>)>;
const infer = Effect.fnUntraced(
    function* (file: string, workspace: Inference) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [marker, configure] = yield* Effect.fromOption(Array.findFirst(Object.entries(markers), ([key]) => [parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`].includes(key)));
        const { root = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], ...configuration }: Optional<ProjectConfiguration, 'root'> = Predicate.isFunction(configure) ? yield* configure(parsed, { ...workspace, file }) : configure;
        const hosted = Array.intersection(root.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        return { projects: { [root]: { ...configuration, root, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);

// --- [ROOT]
const pythonVersions = Effect.fnUntraced(function* ({ workspaceRoot, hosts, pyproject, python, scanExclusions }: Workspace) {
    const [path, folders, { requirements, deptry, fawltydeps }, exclusions, { ruleDirs, utilDirs, testConfigs, customLanguages, languageGlobs, languageInjections }] = yield* Effect.all([Path.Path, hosts, pyproject, scanExclusions, decodeFile(Sgconfig, workspaceRoot, SGCONFIG)]);
    const [{ versions }, scopes, rules] = yield* Effect.all(
        [
            python,
            Effect.flatMap(
                glob(
                    workspaceRoot,
                    folders.map((host) => `**/${host}/**/${PYPROJECT}`),
                ),
                Effect.validate(
                    Effect.fnUntraced(function* (file: string) {
                        const { project } = yield* decodeFile(HostPyproject, workspaceRoot, file);
                        return { scope: path.dirname(file), version: project.minimum[1] };
                    }),
                    { concurrency: 'unbounded' },
                ),
            ),
            glob(
                workspaceRoot,
                ruleDirs.map((dir) => `${dir}/**/*.yml`),
            ).pipe(Effect.flatMap(Effect.validate((file: string) => decodeFile(AstGrepRules(path.basename(file, '.yml')), workspaceRoot, file), { concurrency: 'unbounded' })), Effect.map(Array.flatten)),
        ],
        { concurrency: 'unbounded' },
    );
    const [interpreter] = versions;
    const reaches = (version: string) => (release: string) => VERSIONS.compare(version, release) >= 0;
    const runs =
        (version: string) =>
        ({ metadata: { below, from } }: (typeof rules)[number]) =>
            !Option.exists(below, reaches(version)) && Option.match(from, { onNone: () => true, onSome: reaches(version) });
    const off = Array.dedupe(Array.getSomes(Array.cartesianWith(rules, [interpreter, ...scopes.map(Struct.get('version'))], (rule, version) => Option.liftPredicate(rule, Predicate.not(runs(version))))));
    const ids = (held: typeof rules) => quote(['--filter', `^(${held.map(Struct.get('id')).map(RegExp.escape).join('|')})$`]);
    const dependencyGroups = Record.mapKeys(requirements, String.concat.bind(null, '--no-group '));
    const groups = Record.collect(
        Array.groupBy(scopes, ({ version }) => version),
        (version, members) => {
            const environment = `.cache/python/${version}`;
            const interpreterInput = { runtime: `VIRTUAL_ENV=${environment} uv python find ${version} --show-version` };
            const excludedGroups = Record.keys(Record.filter(dependencyGroups, Function.flow(Struct.get('minimum'), Tuple.get(1), reaches(version), Boolean.not)));
            return {
                version,
                environment,
                executable: `"$(uv python find ${version})"`,
                scoped: quote(members.map(Struct.get('scope'))),
                interpreterInput,
                inputs: [...members.map(Struct.get('scope')).map(folderInput), interpreterInput],
                synced: [`export VIRTUAL_ENV=${environment} && uv sync --active --locked --python ${version}`, ...excludedGroups, '&&'].join(' '),
            };
        },
    );
    const excluded = quote(scopes.flatMap(({ scope }) => ['--exclude', `^${RegExp.escape(`${scope}/`)}`]));
    const scan = (flags: readonly string[]) => ['ast-grep scan --custom-languages allow', ...flags].join(' ');
    const walk = (flags: readonly string[]) => scan([...flags, quote(exclusions.flatMap((pattern) => ['--globs', `!${pattern}`]))]);
    const offs = off.map(({ id }) => quote([`--off=${id}`]));
    const tree = walk(['--error=no-suppress-all', ...offs]);
    const languages = Array.difference(Array.union(rules.map(Struct.get('language')), languageInjections.map(Struct.get('hostLanguage'))), Record.keys(customLanguages));
    const builtIns = Record.filter(AST_GREP_BUILT_INS, (_, language) => languages.includes(language));
    yield* Array.match(Array.difference(languages, Record.keys(builtIns)), { onEmpty: () => Effect.void, onNonEmpty: (missing) => Effect.fail(WorkspaceError.missing(missing.join(', '), 'ast-grep built-in extensions')) });
    const sources = Record.map({ ...Record.map(customLanguages, Struct.get('extensions')), ...builtIns }, extensionInput);
    const rootLanguages = new Set(off.filter(runs(interpreter)).map(Struct.get('language')));
    const ruleInputs = [...ruleDirs, ...utilDirs].map((dir) => `{workspaceRoot}/${dir}/**/*.yml`);
    const classInputs = [
        'typescript',
        ...ruleInputs,
        ...Array.getSomes(Object.values(customLanguages).map(Struct.get('outlineRules'))).map((file) => `{workspaceRoot}/${file}`),
        '{workspaceRoot}/tools/ast-grep/{classes/*.ts,grammars/derive.js}',
        { externalDependencies: ['refa', 'yaml'] },
        { dependentTasksOutputFiles: '**/src/grammar.json', transitive: true },
    ];
    const treeInputs = [...Object.values(sources), ...Object.values(languageGlobs).flatMap(Array.map((pattern) => `{workspaceRoot}/**/${pattern}`)), ...ruleInputs];
    const hosted = Record.filterMap(
        {
            root: {
                version: interpreter,
                command: (filter: string) => walk([filter, ...scopes.map(({ scope }) => quote(['--globs', `!${scope}/**`]))]),
                inputs: [...Object.values(Record.filter(sources, (_, language) => rootLanguages.has(language))), ...scopes.map(({ scope }) => `!${folderInput(scope)}`), ...ruleInputs],
            },
            ...Record.fromIterableWith(groups, ({ version, scoped, inputs }) => [version, { version, command: (filter: string) => scan([filter, scoped]), inputs: [...inputs, ...ruleInputs] }]),
        },
        ({ version, command, inputs }) => Result.map(Result.liftPredicate({ running: off.filter(runs(version)), inputs }, Predicate.Struct({ running: Array.isArrayNonEmpty }), Function.constVoid), Function.flow(Struct.evolve({ running: Function.flow(ids, command) }), Struct.renameKeys({ running: 'command' }))),
    );
    const lock = 'flock .cache/ast-grep/rewrite.lock';
    const split = (target: string, children: Readonly<Record<string, TargetConfiguration>>): Record<string, TargetConfiguration> => {
        const [lead, ...rest] = Object.keys(children).map((child) => `${target}:${child}`);
        return {
            [target]: { dependsOn: [{ target: lead, params: 'forward' }, ...rest] },
            ...Record.mapKeys(children, (child) => `${target}:${child}`),
        };
    };
    const commands = {
        root: {
            inputs: ['pythonFiles'],
            ty: `ty check ${quote(scopes.flatMap(({ scope }) => ['--exclude', scope]))}`,
            mypy: `python -m tools.refurb --quiet -- ${excluded}`,
            native: `mypy --num-workers auto ${excluded}`,
        },
        ...Record.fromIterableWith(groups, ({ version, executable, scoped, inputs, synced }) => [
            version,
            {
                inputs,
                ty: `${synced} ty check --python-version ${version} ${scoped}`,
                mypy: `${synced} python -m tools.refurb --quiet --python-version ${version} ${scoped} -- --python-executable ${executable}`,
                native: `${synced} mypy --num-workers auto --python-version ${version} --python-executable ${executable} ${scoped}`,
            },
        ]),
    };
    const deptryCheck = ({ member, project, tool }: (typeof deptry)[number]) => {
        const roots = [member, ...Array.intersection(member.split(path.sep), folders).map((host) => path.join(APPS, host))];
        const firstParty = tool.hatch.build.devModeDirs.flatMap((folder) => ['--known-first-party', path.relative(path.join(member, folder), member)]);
        const checker = Option.match(
            Option.flatMap(Array.findFirst(scopes, Predicate.Struct({ scope: Equal.equals(member) })), ({ version }) => Array.findFirst(groups, Predicate.Struct({ version: Equal.equals(version) }))),
            {
                onNone: () => ({ command: 'deptry', inputs: [] }),
                onSome: ({ executable, environment, interpreterInput }) => ({ command: `${executable} -m deptry`, options: { env: { VIRTUAL_ENV: environment } }, inputs: [interpreterInput] }),
            },
        );
        return [`lint:deptry:${project.name}`, { ...checker, command: `${checker.command} ${quote(['--config', path.join(member, PYPROJECT), ...firstParty, ...roots])}`, inputs: ['python', ...roots.map(folderInput), ...checker.inputs] }] as const;
    };
    const deptryRuns = deptry.map(deptryCheck);
    const fawltyRuns = fawltydeps.members.map(
        ({ member, project }) => [`lint:fawltydeps:${project.name}`, { command: 'fawltydeps', options: { cwd: member, args: ['--pyenv', '"$VIRTUAL_ENV"'], env: { FAWLTYDEPS_CUSTOM_MAPPING: fawltydeps.mapping } }, inputs: ['python', folderInput(member), { env: 'VIRTUAL_ENV' }] }] as const,
    );
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    ...split('lint:ast-grep', { tree: { command: tree, inputs: treeInputs }, classes: { command: 'node tools/ast-grep/classes/main.ts', inputs: classInputs }, ...hosted }),
                    'test:ast-grep': { command: 'ast-grep test --custom-languages allow', dependsOn: ['grammar'], cache: true, inputs: ['astGrep', ...testConfigs.map(({ testDir }) => `{workspaceRoot}/${testDir}/**/*.yml`), ...ruleInputs] },
                    ...split(
                        'rewrite',
                        Record.map({ tree: { command: walk(offs), inputs: treeInputs }, ...hosted }, ({ command, ...configuration }) => ({ ...configuration, command: `${lock} ${command} --update-all` })),
                    ),
                    ...split(
                        'typecheck:ty',
                        Record.map(commands, ({ inputs, ty }) => ({ command: ty, inputs, options: { env: { TY_UV: '1', UV_LOCKED: '1' } } })),
                    ),
                    ...split(
                        'typecheck:mypy',
                        Record.map(commands, ({ inputs, mypy, native }) => ({ command: mypy, inputs: [...inputs, '{workspaceRoot}/tools/refurb/**/*.py'], configurations: { native: { command: native } } })),
                    ),
                    'lint:refurb': { dependsOn: [{ target: 'typecheck:mypy', params: 'forward' }] },
                    ...Record.fromIterableWith(Object.keys(commands), (scope) => [`lint:refurb:${scope}`, { dependsOn: [{ target: `typecheck:mypy:${scope}`, params: 'forward' }] } satisfies TargetConfiguration]),
                    ...Array.match(deptryRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:deptry': { dependsOn: [{ target: 'lint:deptry:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
                    ...Array.match(fawltyRuns, { onEmpty: () => ({}), onNonEmpty: (checks) => ({ 'lint:fawltydeps': { dependsOn: [{ target: 'lint:fawltydeps:*', params: 'forward' as const }] }, ...Object.fromEntries(checks) }) }),
                },
            },
        },
    } satisfies CreateNodesResult;
});
const swiftModules = Effect.fnUntraced(function* ({ swift, hosts }: Workspace) {
    const [{ targets, platforms, tests }, folders] = yield* Effect.all([swift, hosts]);
    const [suites, sources] = tests;
    const platformed = platforms.map(Struct.get('name'));
    const suiteTargets = Record.fromIterableWith(suites, ({ name, path }) => [name, { path, target: { options: { args: ['--test-product', name] } } }]);
    const excluded = Array.match(suites, { onEmpty: () => ({}), onNonEmpty: (all) => ({ options: { args: ['--exclude-targets', ...all.map(Struct.get('c99name'))] } }) });
    const modules = sources.map((target) => {
        const segments = target.path.split('/');
        const installed = target.type === 'executable' && segments[0] !== TESTS;
        const project = swiftProject(['build:swiftpm', ...Array.intersection(Array.union(platformed, segments), folders).map(String.concat.bind(null, 'host:'))], installed ? ['install', 'upgrade:swiftpm'] : []);
        const attached = attachedSuites(suiteTargets, target.name);
        return [
            target.path,
            {
                ...project,
                ...Array.match(Object.values(attached), { onEmpty: () => ({}), onNonEmpty: (held) => suiteInput(...held.map(Struct.get('path'))) }),
                root: target.path,
                name: target.name,
                targets: { ...project.targets, 'lint:periphery': excluded, ...Record.map(attached, Struct.get('target')) },
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
const upstreamScanners = Effect.fnUntraced(function* (workspaceRoot: string, specifiers: readonly string[]) {
    const path = yield* Path.Path;
    const upstreams = yield* Effect.forEach(
        specifiers.filter(String.endsWith(`/${GRAMMAR}`)).map((specifier) => path.join('node_modules', path.dirname(specifier))),
        (dir) => Effect.map(decodeFile(TreeSitterGrammar, workspaceRoot, dir, 'src', 'grammar.json'), Struct.assign({ dir })),
        { concurrency: 'unbounded' },
    );
    const bases = new Set(Array.getSomes(upstreams.map(Struct.get('inherits'))));
    return upstreams.filter(({ name, externals }) => externals.length > 0 && !bases.has(name)).map(({ dir }) => ({ scanner: path.join(dir, 'src', 'scanner.c'), upstream: Option.some(path.join(dir, 'src', 'grammar.json')) }));
});
const scanners = Effect.fnUntraced(function* (workspaceRoot: string, scanned: readonly string[], file: string, { specifiers }: Effect.Success<ReturnType<typeof imports>>) {
    const path = yield* Path.Path;
    const owned = path.join(path.dirname(file), 'scanner.c');
    return scanned.includes(owned) ? [{ scanner: owned, upstream: Option.none<string>() }] : yield* upstreamScanners(workspaceRoot, specifiers);
});
const generated = Effect.fnUntraced(function* (libraries: Readonly<Record<string, string>>, file: string) {
    const path = yield* Path.Path;
    const stem = path.basename(path.dirname(file));
    const library = yield* Effect.fromOption(Record.get(libraries, stem), () => WorkspaceError.missing(stem, `${SGCONFIG} customLanguages libraryPath`));
    return path.join(path.dirname(library), stem, 'src', 'grammar.json');
});
const grammarTargets = Effect.fnUntraced(function* (workspaceRoot: string, libraries: Readonly<Record<string, string>>, scanned: readonly string[], [stem, library]: readonly [string, string]) {
    const path = yield* Path.Path;
    const entry = path.join(GRAMMARS, stem, GRAMMAR);
    const binary = path.join(path.dirname(library), stem);
    const grammarFile = (file: string) => path.basename(file) === GRAMMAR && path.dirname(path.dirname(file)) === GRAMMARS;
    const folder = (dir: string) => [`{workspaceRoot}/${dir}/**/*`, `!{workspaceRoot}/${dir}/test/**/*`];
    const direct = yield* imports(workspaceRoot, entry);
    const graph = yield* reach(workspaceRoot, direct.files, { [entry]: direct });
    const hosts = Object.entries(graph).filter(([file]) => direct.files.includes(file) && grammarFile(file));
    const [own, output, hosted] = yield* Effect.all([scanners(workspaceRoot, scanned, entry, direct), generated(libraries, entry), Effect.forEach(hosts, ([file, held]) => Effect.all({ found: scanners(workspaceRoot, scanned, file, held), json: generated(libraries, file) }))]);
    const components = [...own.map(({ scanner, upstream }) => [scanner, Option.getOrElse(upstream, Function.constant(output))] as const), ...hosted.flatMap(({ found, json }) => Array.cartesian(found.map(Struct.get('scanner')), [json]))];
    return [
        [
            `grammar:${stem}`,
            {
                options: {
                    commands: [
                        `cmake --fresh -S ${GRAMMARS} -B ${binary} -DCMAKE_BUILD_TYPE=Release -DGRAMMAR_SOURCE:FILEPATH="${path.relative(GRAMMARS, entry)}" -DNODE_MODULES:PATH="$NX_WORKSPACE_ROOT/node_modules" -DCOMPONENTS="${components.map(([scanner, json]) => `${path.relative(GRAMMARS, scanner)}|${path.relative(GRAMMARS, json)}`).join(';')}" {args.cmakeArguments}`,
                        { command: `cmake --build ${binary} --config Release`, forwardAllArgs: false },
                    ],
                },
                dependsOn: hosts.map(([file]) => `grammar:${path.basename(path.dirname(file))}`),
                inputs: [...Array.dedupe(Object.keys(graph).flatMap((file) => (grammarFile(file) ? folder(path.dirname(file)) : [`{workspaceRoot}/${file}`]))), { externalDependencies: Array.dedupe(Object.values(graph).flatMap(Struct.get('specifiers')).map(packageName)) }],
                outputs: [`{workspaceRoot}/${library}`, `{workspaceRoot}/${binary}.yml`, `{workspaceRoot}/${binary}/{src/grammar.json,src/parser.c,tree-sitter.json,lib,test}`],
            },
        ],
        [`test:grammar:${stem}`, { command: 'tree-sitter test', options: { cwd: binary, env: { TREE_SITTER_LIBDIR: 'lib' } }, dependsOn: [`grammar:${stem}`], inputs: [`{workspaceRoot}/${path.dirname(entry)}/test/**/*`] }],
    ] as const satisfies readonly (readonly [string, TargetConfiguration])[];
});
const grammars = Effect.fnUntraced(function* ({ workspaceRoot }: Workspace) {
    const [path, { customLanguages }, scanned] = yield* Effect.all([Path.Path, decodeFile(Sgconfig, workspaceRoot, SGCONFIG), glob(workspaceRoot, [`${GRAMMARS}/*/scanner.c`])]);
    const libraries = Record.fromIterableWith(Object.values(customLanguages), ({ libraryPath }) => [path.basename(libraryPath, path.extname(libraryPath)), libraryPath] as const);
    const inferred = yield* Effect.forEach(Record.toEntries(libraries), (language) => grammarTargets(workspaceRoot, libraries, scanned, language), { concurrency: 'unbounded' });
    return { projects: { '.': { root: '.', targets: Object.fromEntries(inferred.flat()) } } } satisfies CreateNodesResult;
});
const roots = { [PYPROJECT]: pythonVersions, [SWIFT_MANIFEST]: swiftModules, [SGCONFIG]: grammars } as const;

// --- [COMPONENTS]
const components = Effect.fnUntraced(function* (workspaceRoot: string, inferred: CreateNodesResultArray, applications: ReadonlyMap<string, Application>) {
    const path = yield* Path.Path;
    const projects = inferred.flatMap(([, { projects: held = {} }]) => Object.entries(held));
    const declared = (at: string) => Array.dedupe(projects.filter(([directory]) => directory === at).flatMap(([, project]) => Object.keys(project.targets ?? {})));
    const members = yield* Effect.forEach(
        applications.size === 0 ? [] : yield* glob(workspaceRoot, Array.dedupe(projects.map(([at]) => path.join(at, Package.file)))),
        Effect.fnUntraced(function* (file) {
            const at = path.dirname(file);
            const { name } = yield* Package.read(path.join(workspaceRoot, at));
            return { at, name, steps: Array.intersection(['build', 'pack', 'install'], declared(at)) };
        }),
    );
    const component = Effect.fnUntraced(function* ([file, { root, env }]: readonly [string, Application]) {
        const { owner } = yield* owning(workspaceRoot, root);
        const nested = owner === '.' ? [] : members.filter(Predicate.Struct({ at: Predicate.and(Predicate.not(Equal.equals(root)), Function.flow(path.relative.bind(null, owner), Predicate.not(String.startsWith('..')))) }));
        const after = (step: string, needed: string) => (declared(root).includes(step) ? Array.match(nested.filter(Predicate.Struct({ steps: Array.contains(needed) })).map(Struct.get('name')), { onEmpty: () => ({}), onNonEmpty: (names) => ({ [step]: { dependsOn: [{ projects: names, target: needed }] } }) }) : {});
        const targeted = (steps: readonly string[]) => Record.map(Record.fromIterableBy(steps, Function.identity), Function.constant({ options: { env } }));
        const dependencies = Record.map(Record.fromIterableBy(nested, Struct.get('at')), Function.flow(Struct.pick(['at', 'steps']), Struct.evolve({ steps: targeted }), Struct.renameKeys({ at: 'root', steps: 'targets' })));
        return [file, { projects: { ...dependencies, [root]: { root, implicitDependencies: nested.map(Struct.get('name')), targets: { ...after('install', 'install'), ...after('publish', 'pack') } } } }] as const;
    });
    return yield* Effect.forEach(applications, component);
});

// --- [DEPENDENCIES]
const manifestless = ({ projects }: CreateDependenciesContext, manifested: ReadonlySet<string>, project: string) => Option.exists(Record.get(projects, project), ({ root, tags = [] }) => tags.includes('language:typescript') && !manifested.has(root));
const importTarget = ({ projects, externalNodes = {} }: CreateDependenciesContext, specifier: string) => {
    const name = packageName(specifier);
    return Option.firstSomeOf([Option.as(Record.get(externalNodes, `npm:${name}`), `npm:${name}`), Option.as(Record.get(projects, name), name)]);
};
const sourceImports = Effect.fnUntraced(function* (context: CreateDependenciesContext, [source, sourceFile]: readonly [string, string]) {
    const requests = yield* moduleRequests(context.workspaceRoot, sourceFile);
    return Array.dedupe(Array.getSomes(requests.map((request) => importTarget(context, request)))).map((target) => ({ source, target, sourceFile, type: DependencyType.static }));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes<{ readonly root: Record<string, TargetConfiguration> }> = [
    `{${Object.keys(roots).join(',')},*/**/{${Object.keys(markers).join(',')}}}`,
    Effect.fnUntraced(function* (files, options, context) {
        const [path, workspace, { name }] = yield* Effect.all([Path.Path, openWorkspace(context), Package.read(context.workspaceRoot)]);
        const rootNeutrality = 'eng/typescript/root-neutrality.config.ts';
        const applications = new Map<string, Application>();
        const rooted = new Map<string, readonly string[]>();
        const inference = { ...workspace, configFiles: files, applications, rooted };
        const inferred = yield* Effect.promise(
            createNodesFromFiles.bind(
                null,
                Effect.fnUntraced(function* (file: string) {
                    return yield* Option.match(Record.get(file)(roots), { onNone: () => infer(file, inference), onSome: (root) => root(inference) });
                }, runPlugin),
                files,
                options,
                context,
            ),
        );
        const { pythonFiles, testPaths } = yield* workspace.pyproject;
        const testOwners = inferred
            .flatMap(([, { projects = {} }]) => Object.entries(projects))
            .filter(([, project]) => project.tags?.includes('language:python') && project.targets?.test)
            .map(([directory]) => directory);
        const claimed = Array.cartesianWith(testOwners, testPaths, (directory, testPath) => `${suitePath(path, testPath, directory)}/**`);
        const unclaimed = yield* glob(
            context.workspaceRoot,
            Array.cartesianWith(testPaths, pythonFiles, (testPath, pattern) => `${testPath}/**/${pattern}`),
            claimed,
        );
        const configuredCrates = Array.bind(yield* workspace.crates, 'nx', ({ metadata }) => Option.toArray(Option.flatMap(metadata, Struct.get('nx'))));
        return [
            ...inferred,
            ...Array.match(Array.sortWith([...rooted], Tuple.get(0), Order.String), {
                onEmpty: () => [],
                onNonEmpty: (entries) =>
                    [
                        [
                            'nx.json',
                            {
                                projects: {
                                    '.': {
                                        root: '.',
                                        targets: {
                                            'lint:dependency-cruiser': {
                                                command: `depcruise --config ${rootNeutrality} --output-type err-long ${quote(entries.flatMap(Tuple.get(1)))}`,
                                                inputs: [
                                                    { input: 'production', projects: entries.map(Tuple.get(0)) },
                                                    { input: 'production', projects: entries.map(Tuple.get(0)), dependencies: true },
                                                    'typescript',
                                                    `{workspaceRoot}/${rootNeutrality}`,
                                                    '{workspaceRoot}/pnpm-workspace.yaml',
                                                    { externalDependencies: ['dependency-cruiser', 'typescript', 'effect', '@effect/platform-node', 'plist'] },
                                                ],
                                                syncGenerators: ['./tools/nx/generators.json:barrels'],
                                            },
                                        },
                                    },
                                },
                            },
                        ],
                    ] satisfies CreateNodesResultArray,
            }),
            ...Array.match(unclaimed, { onEmpty: () => [], onNonEmpty: (suites) => [[PYPROJECT, { projects: { '.': { root: '.', targets: { test: { command: 'pytest', options: { args: suites }, inputs: ['python', 'pythonFiles'] } } } } }]] satisfies CreateNodesResultArray }),
            ...(yield* components(context.workspaceRoot, inferred, applications)),
            ...configuredCrates.map(({ manifest, nx }) => [manifest, { projects: { [path.dirname(manifest)]: nx } }] as const),
            ...Option.toArray(Option.fromUndefinedOr(options)).map(({ root }) => ['nx.json', { projects: { '.': { root: '.', name, targets: root } } }] as const),
        ];
    }, runPlugin),
];
const createDependencies: CreateDependencies = Effect.fnUntraced(function* (_options, context) {
    const { fileMap, filesToProcess, workspaceRoot } = context;
    const [path, workspace] = yield* Effect.all([Path.Path, openWorkspace(context)]);
    const [sources, crates, { products, tests }, xcode] = yield* Effect.all(
        [
            Array.match(Object.values(filesToProcess.projectFileMap).flat().map(Struct.get('file')).filter(String.endsWith('.py')), {
                onEmpty: () => Effect.succeed(Record.empty<string, readonly string[]>()),
                onNonEmpty: (files) => Effect.flatMap(runCommand('ruff', ['analyze', 'graph', ...files], { cwd: workspaceRoot }), Schema.decodeEffect(JsonListRecord)),
            }),
            workspace.crates,
            workspace.swift,
            Effect.flatMap(
                glob(workspaceRoot, [`*/**/${XCODE_PROJECT}`]),
                Effect.validate(
                    Effect.fnUntraced(function* (file: string) {
                        const { targets: rows } = yield* decodeFile(XcodeProject, workspaceRoot, file);
                        return [file, rows.flatMap(Struct.get('members')).map(Struct.get('product'))] as const;
                    }),
                    { concurrency: 'unbounded' },
                ),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const [suites, modules] = tests;
    const suiteMembers = Record.fromIterableBy(suites, Struct.get('name'));
    const owners = Object.fromEntries(Record.collect(fileMap.projectFileMap, (owner, files) => Array.cartesian(files.map(Struct.get('file')), [owner])).flat());
    const swift: RawProjectGraphDependency[] = modules.flatMap(({ name, locals }) => Array.cartesian([name], [...locals, ...Array.difference(Object.values(attachedSuites(suiteMembers, name)).flatMap(Struct.get('locals')), [name])])).map(([source, target]) => ({ source, target, type: DependencyType.implicit }));
    const linked = xcode.flatMap(([file, names]) => Array.cartesian(Option.toArray(Record.get(owners, file)), products.filter(Predicate.Struct({ name: Array.contains.bind(null, names) })).flatMap(Struct.get('targets'))).map(Tuple.appendElement(file)));
    const references = [
        ...Record.collect(sources, (file, targets) => Array.cartesian([file], targets)).flat(),
        ...crates.flatMap(({ manifest, dependencies }) => Array.cartesian([manifest], Array.getSomes(dependencies.map(Struct.get('path'))))).map(([manifest, member]) => [manifest, path.join(path.relative(workspaceRoot, member), CARGO_MANIFEST)] as const),
    ];
    const resolved = Array.getSomes(references.map(([sourceFile, file]) => Option.map(Option.all([Record.get(owners, sourceFile), Record.get(owners, file)]), Tuple.appendElement(sourceFile)))).filter(([source, target]) => source !== target);
    const manifested = new Set(
        Object.keys(owners)
            .filter((file) => path.basename(file) === Package.file)
            .map(path.dirname),
    );
    const imported = yield* Effect.forEach(
        Object.entries(filesToProcess.projectFileMap)
            .filter(([project]) => manifestless(context, manifested, project))
            .flatMap(([project, files]) => Array.cartesian([project], files.map(Struct.get('file')).filter(Predicate.or(String.endsWith('.ts'), String.endsWith('.tsx'))))),
        (pair) => sourceImports(context, pair),
        { concurrency: 'unbounded' },
    );
    return [...[...resolved, ...linked].map(([source, target, sourceFile]) => ({ source, target, sourceFile, type: DependencyType.static })), ...imported.flat(), ...swift];
}, runPlugin);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
