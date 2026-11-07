import { createRequire, isBuiltin } from 'node:module';
import { matchesGlob } from 'node:path';
import type { CreateNodesResult, Optional, ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { findMatchingProjects, interpolate } from '@nx/devkit/internal';
import { ClaudeHooks, decodeFile, TextFormat, Xcconfig, XcodeProject } from '@rasm/eng';
import { suitePath, VITEST_INTEGRATION_TESTS } from '@rasm/test-support/suite';
import { Array, Effect, Equal, FileSystem, Filter, Function, Option, Path, Predicate, Record, Result, Schema, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parseSync } from 'rolldown/utils';
import { CARGO_MANIFEST, CLOUDFLARE_CONFIG, glob, type PluginServices, suiteParents, type Workspace, WorkspaceError } from '#/workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Inference = Workspace & { readonly configFiles: ReadonlySet<string> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const TESTS = 'tests';
const MODULES = 'node_modules';

// --- [MODELS] --------------------------------------------------------------------------

const BlenderManifest = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
const MSBuildProject = TextFormat('MSBuild XML', (text) => new XMLParser({ ignoreAttributes: false, isArray: (name) => name === 'PropertyGroup' || name === 'ItemGroup' || name === 'PackageReference' }).parse(text)).pipe(
    Schema.decodeTo(
        Schema.Struct({
            Project: Schema.Struct({
                PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String), PlugInIcon: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
                ItemGroup: Schema.Array(Schema.Struct({ PackageReference: Schema.optionalKey(Schema.Array(Schema.Struct({ '@_Include': Schema.optionalKey(Schema.String) }))) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            }),
        }),
    ),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [READS]

const packageName = (specifier: string) =>
    specifier
        .split('/')
        .slice(0, specifier.startsWith('@') ? 2 : 1)
        .join('/');
const moduleRequests = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const { staticImports, staticExports } = parseSync(file, yield* decodeFile(Schema.String, workspaceRoot, file)).module;
    const reexported = staticExports.flatMap(Struct.get('entries')).map(({ moduleRequest }) => Option.fromNullishOr(moduleRequest));
    return Array.dedupe([...staticImports.map(Struct.get('moduleRequest')), ...Array.getSomes(reexported)].map(Struct.get('value')));
});
const imports = (workspaceRoot: string) =>
    Effect.fnUntraced(function* (file: string) {
        const [path, requests] = yield* Effect.all([Path.Path, moduleRequests(workspaceRoot, file)]);
        const { resolve } = createRequire(path.join(workspaceRoot, file));
        const located = (request: string) => path.relative(workspaceRoot, resolve(request));
        const source = (request: string) => Option.filter(Option.liftThrowable(located)(request), (held) => !held.split(path.sep).includes(MODULES));
        const [files, specifiers] = Array.partition(requests.filter(Predicate.not(isBuiltin)), (request) => (request.startsWith('.') || request.startsWith('#') ? Result.succeed(located(request)) : Result.fromOption(source(request), () => request)));
        return { files, specifiers };
    });
const reach = <A extends Effect.Success<ReturnType<ReturnType<typeof imports>>>, E, R>(read: (file: string) => Effect.Effect<A, E, R>, frontier: readonly string[], known: Readonly<Record<string, A>> = {}): Effect.Effect<Readonly<Record<string, A>>, E, R> => {
    const pending = Array.dedupe(frontier.filter((file) => !Object.hasOwn(known, file)));
    return pending.length === 0 ? Effect.succeed(known) : Effect.flatMap(Effect.forEach(pending, read, { concurrency: 'unbounded' }), (found) => reach(read, found.flatMap(Struct.get('files')), { ...known, ...Object.fromEntries(Array.zip(pending, found)) }));
};
const owning = Effect.fnUntraced(function* ({ packages }: Workspace, dir: string) {
    const [path, manifests] = yield* Effect.all([Path.Path, packages]);
    const lineage = Array.unfold(dir, (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const owner = yield* Effect.fromOption(Array.findFirst([...lineage, '.'], (at) => Record.has(manifests, at)));
    return { owner, manifest: yield* Effect.fromOption(Record.get(manifests, owner)) };
});

// --- [INPUTS]

const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const defaultInputs = (files: readonly NonNullable<ProjectConfiguration['namedInputs']>[string][number][]) => (files.length === 0 ? {} : { namedInputs: { default: ['{projectRoot}/**/*', ...files] } });
const externalInputs = (path: Path.Path, root: string, files: readonly (string | undefined)[], packages: readonly string[] = []) =>
    defaultInputs([
        ...Array.dedupe(
            files
                .filter(Predicate.isString)
                .map(path.normalize)
                .filter((file) => file.startsWith('../'))
                .map((file) => `{workspaceRoot}/${path.join(root, file)}`),
        ),
        ...Array.match(packages, { onEmpty: () => [], onNonEmpty: (externalDependencies) => [{ externalDependencies: [...externalDependencies] }] }),
    ]);
const rootTargets = (targets: Readonly<Record<string, TargetConfiguration>>): CreateNodesResult => ({ projects: { '.': { root: '.', targets } } });
const pytest = (args: readonly string[], coverage?: string): TargetConfiguration => ({
    command: 'pytest --report-log=.artifacts/python/test/{projectName}/events.jsonl',
    options: { args, ...(coverage === undefined ? {} : { env: { COVERAGE_FILE: coverage } }) },
    outputs: ['{workspaceRoot}/.artifacts/python/test/{projectName}', ...(coverage === undefined ? [] : [`{workspaceRoot}/${coverage}`])],
});

// --- [PROJECTS]

const tagPrefix = ({ release }: Workspace, root: string, tags: readonly string[], name: string | undefined) =>
    Option.flatMap(Option.fromUndefinedOr(name), (projectName) =>
        Option.flatMap(
            Record.findFirst(release?.groups ?? {}, ({ projects }) => findMatchingProjects(Array.ensure(projects), { [projectName]: { data: { root, tags: [...tags] } } }).length > 0),
            ([releaseGroupName, group]) => Option.map(Option.fromNullishOr(group.releaseTag?.pattern ?? release?.releaseTag?.pattern), (pattern) => interpolate(pattern, { projectName, releaseGroupName }).replace('{version}', '')),
        ),
    );
const pythonTargets = Effect.fnUntraced(function* (dir: string, { workspaceRoot, pyproject, hosts, configFiles }: Inference) {
    const [path, { pythonFiles, testPaths, dataFile }, folders] = yield* Effect.all([Path.Path, pyproject, hosts]);
    const suites = yield* Effect.filter(
        testPaths.map((testPath) => suitePath(path, testPath, dir)),
        (suite) => Effect.map(glob(workspaceRoot, [`${suite}/**/{${pythonFiles.join(',')}}`]), Array.isArrayNonEmpty),
    );
    const segments = dir.split(path.sep);
    const coverage = `${dataFile}.{projectName}`;
    const test = configFiles.has(path.join(dir, 'py.typed')) && !folders.some((host) => segments.includes(host)) ? pytest([...suites, '--cov'], coverage) : pytest(suites);
    const parents = suiteParents(path, suites);
    return Array.match(suites, {
        onEmpty: () => ({ targets: {} }),
        onNonEmpty: (held) => ({
            ...defaultInputs([...held.map(folderInput), '{workspaceRoot}/conftest.py', ...parents.map((parent) => `{workspaceRoot}/${path.join(parent, '*.py')}`)]),
            targets: { test },
        }),
    });
});
const attachedSuites = <Suite>(suites: Readonly<Record<string, Suite>>, module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Record.get(suites, `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: Record.fromIterableWith(['build', ...targets], (target) => [target, {}]),
});
const binlog = (workspaceRoot: string, target: string) => `-bl:${workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`;
const logged = (workspaceRoot: string, target: string) => ({ env: { MSBUILD_LOGGING_ARGS: binlog(workspaceRoot, target) } });
const markers: Readonly<Record<string, Optional<ProjectConfiguration, 'root'> | ((file: Path.Path.Parsed, workspace: Inference & { readonly file: string }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, Error, PluginServices>)>> = {
    '*.csproj': Effect.fnUntraced(function* ({ dir, name }, workspace) {
        const { file, workspaceRoot } = workspace;
        const [path, { Project }] = yield* Effect.all([Path.Path, decodeFile(MSBuildProject, workspaceRoot, file)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const skin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhs');
        const product = plugin || skin;
        const licensed = Project.ItemGroup.flatMap(({ PackageReference = [] }) => PackageReference).some((reference) => reference['@_Include'] === 'SixLabors.ImageSharp');
        const tags = ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(product ? ['platform:windows'] : []), ...(licensed ? ['license:imagesharp'] : [])];
        const runtime = ['--runtime', 'win-x64'];
        const logs = Record.fromIterableWith((target: string) => [target, { options: logged(workspaceRoot, target) }]);
        const windowsTargets = {
            'build:windows': { command: 'dotnet build', options: { cwd: '{projectRoot}', args: [...runtime, '--configuration', 'Release'], ...logged(workspaceRoot, 'build:windows') }, parallelism: false },
            'pack:windows': { options: { args: runtime, ...logged(workspaceRoot, 'pack:windows') } },
        };
        const configurations = {
            debug: { args: ['--no-restore', '--configuration', 'Debug'] },
            release: { args: ['--no-restore', '--configuration', 'Release'] },
        };
        const build = product ? Record.map({ build: ['--no-restore'], 'build:release': configurations.release.args }, (args, target) => ({ dependsOn: [], options: { args, ...logged(workspaceRoot, target) }, configurations, parallelism: false })) : logs(['build', 'build:release']);
        const packaged = {
            ...(product ? windowsTargets : {}),
            ...(plugin ? logs(['pack', 'install', 'publish']) : {}),
            ...(skin ? { install: { command: 'dotnet msbuild -target:InstallRhinoSkin -property:Configuration=Release', options: { cwd: '{projectRoot}', ...logged(workspaceRoot, 'install') }, dependsOn: ['pack:windows'] } } : {}),
        };
        return {
            ...externalInputs(
                path,
                dir,
                Project.PropertyGroup.map(({ PlugInIcon }) => PlugInIcon),
            ),
            ...(product ? { name } : {}),
            tags,
            targets: {
                ...build,
                ...packaged,
                ...(dir.split(path.sep, 1)[0] === TESTS ? {} : { check: {} }),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, workspaceRoot }) {
        yield* Effect.filterOrFail(decodeFile(Schema.String, workspaceRoot, file), (text) => text.startsWith('#!'));
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: `build:*${ext}`, params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: `format:*${ext}` }] },
                [`restore:${base}`]: { options: { args: [file, binlog(workspaceRoot, `restore:${base}`)] } },
                [`build:${base}`]: { command: 'dotnet build', dependsOn: [`restore:${base}`], options: { args: [file, '--no-restore', binlog(workspaceRoot, `build:${base}`)] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                check: {},
            },
        };
    }),
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
        const args = tests.flatMap((target) => (library(target) ? ['--lib'] : [`--${Array.headNonEmpty(target.kind)}`, target.name]));
        const local = new Set(Array.getSomes(crate.dependencies.map(Struct.get('path'))));
        return {
            ...defaultInputs(sources.map(folderInput)),
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            implicitDependencies: all.filter(({ manifest }) => local.has(path.resolve(workspaceRoot, path.dirname(manifest)))).map(Struct.get('name')),
            targets: {
                ...(tested ? { test: { options: { args } } } : {}),
                ...(bins.length > 0 ? { build: {} } : {}),
                ...(launched.length > 0 ? { install: { options: { args: launched } } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ id }, python] = yield* Effect.all([decodeFile(BlenderManifest, workspace.workspaceRoot, workspace.file), pythonTargets(dir, workspace)]);
        return {
            ...python,
            name: id,
            tags: ['language:python', 'host:blender', 'output:extension'],
            targets: {
                assets: {},
                pack: {},
                install: {},
                publish: {},
                ...python.targets,
            },
        };
    }),
    [XcodeProject.file]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, fs, { files, targets }, { products }, { DEVELOPMENT_TEAM }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, decodeFile(XcodeProject.schema, workspace.workspaceRoot, workspace.file), workspace.swift, Xcconfig.read(workspace.workspaceRoot)]);
        const [root, name] = [path.dirname(dir), path.parse(dir).name];
        const links = yield* Effect.forEach(yield* glob(workspace.workspaceRoot, [`${root}/**/*.xcassets/**/*.svg`]), (file) => Effect.option(fs.readLink(path.join(workspace.workspaceRoot, file))));
        const catalogs = Array.dedupe(Array.getSomes(links).flatMap((target) => target.split(`${MODULES}/`).slice(1, 2).map(packageName)));
        const signing = yield* Effect.fromOption(
            Option.flatMap(
                Array.findFirst(targets, (target) => target.name === name),
                ({ settings }) => Option.all({ PRODUCT_BUNDLE_IDENTIFIER: settings.PRODUCT_BUNDLE_IDENTIFIER, CODE_SIGN_ENTITLEMENTS: Option.map(Option.fromNullishOr(settings.CODE_SIGN_ENTITLEMENTS), (file) => path.join(root, file)) }),
            ),
            () => WorkspaceError.missing(`${workspace.file} target ${name}`, 'PRODUCT_BUNDLE_IDENTIFIER and CODE_SIGN_ENTITLEMENTS'),
        );
        const members = targets.flatMap(Struct.get('members'));
        const updated = members.some(Predicate.Struct({ product: Equal.equals('Sparkle') }));
        const local = new Set(members.filter((member) => member.package === '..').map(Struct.get('product')));
        const owners = yield* Effect.forEach(
            Array.getSomes(files.map(Struct.get('path')))
                .map((entry) => path.relative('.artifacts', path.join(root, entry)))
                .filter((source) => !source.startsWith('..')),
            (source) => Effect.map(owning(workspace, path.join(path.dirname(source), path.parse(source).name)), Function.flow(Struct.get('manifest'), Struct.get('name'))),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const xcode = swiftProject(['build:xcode', ...(updated ? ['output:appcast'] : [])], ['check', 'lint:periphery', 'pack', 'install', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), ...signing, DEVELOPMENT_TEAM } };
        return {
            root,
            name,
            ...xcode,
            ...externalInputs(
                path,
                root,
                targets.flatMap(({ settings }) => [settings.CODE_SIGN_ENTITLEMENTS, settings.INFOPLIST_FILE]),
                catalogs,
            ),
            implicitDependencies: products.filter((product) => local.has(product.name)).flatMap(Struct.get('targets')),
            targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) },
        };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots, commands }, python] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        const command = commands.includes(dir);
        return { ...python, name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python', ...(command ? ['output:command'] : [])], targets: { ...python.targets, ...(command ? { install: {} } : {}) } };
    }),
    '__main__.py': Effect.fnUntraced(function* ({ dir }, workspace) {
        const path = yield* Path.Path;
        yield* Effect.filterOrFail(Effect.succeed(dir), (root) => !['py.typed', 'blender_manifest.toml'].some((marker) => workspace.configFiles.has(path.join(root, marker))));
        const python = yield* Effect.filterOrFail(pythonTargets(dir, workspace), ({ targets }) => 'test' in targets);
        return { ...python, name: path.basename(dir), tags: ['language:python'] };
    }),
    'tsconfig.json': Effect.fnUntraced(function* ({ dir }, workspace) {
        const { file, workspaceRoot, configs, configFiles } = workspace;
        const [path, readConfig] = yield* Effect.all([Path.Path, configs]);
        const {
            config: { options, errors, fileNames },
            files,
        } = yield* Effect.try(() => readConfig(file));
        if (Array.isArrayNonEmpty(errors)) {
            return yield* WorkspaceError.unparsed(file, errors.map(Struct.get('text')).join('\n'));
        }
        const suite = suitePath(path, path.join(TESTS, 'typescript'), dir);
        const storybook = path.join(dir, '.storybook', 'main.ts');
        const suiteFiles = yield* glob(workspaceRoot, [`${suite}/**/*`, storybook]);
        const tests = suiteFiles.filter((candidate) => candidate !== storybook);
        const sourceRoots = [dir, suite];
        const parent = `..${path.sep}`;
        const sources = fileNames.map((candidate) => path.relative(workspaceRoot, candidate));
        const externalSources = sources.filter((candidate) => !(path.isAbsolute(candidate) || candidate.startsWith(parent)) && sourceRoots.every((root) => path.relative(root, candidate).startsWith(parent))).map((candidate) => `{workspaceRoot}/${candidate}`);
        const compilerInputs = files.map((held) => ({ fileset: `{workspaceRoot}/${held}`, includeIgnored: true }));
        const [containers, portable] = Array.partition(
            tests.filter((candidate) => matchesGlob(candidate, '**/*.test.{ts,tsx}')),
            Filter.fromPredicate((candidate) => matchesGlob(candidate, VITEST_INTEGRATION_TESTS)),
        );
        const integration = Array.match(containers, {
            onEmpty: () => ({}),
            onNonEmpty: () => ({
                'test:integration': { inputs: [...compilerInputs, folderInput(suite), ...portable.map((candidate) => `!{workspaceRoot}/${candidate}`)] },
            }),
        });
        return {
            ...defaultInputs([...externalSources, ...(tests.length === 0 || suite === dir ? [] : [folderInput(suite)])]),
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': {
                    inputs: [
                        `{workspaceRoot}/${dir}/**/*`,
                        ...compilerInputs,
                        ...(options.typeRoots ?? []).map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.relative(workspaceRoot, typeRoot)}/**/*.d.ts`, includeIgnored: true })),
                        ...(configFiles.has(path.join(dir, CLOUDFLARE_CONFIG)) ? [{ dependentTasksOutputFiles: '**/*.d.ts' }] : []),
                    ],
                },
                check: { inputs: ['default', '^production', 'typescript', ...compilerInputs, { externalDependencies: ['@effect/tsgo', 'typescript'] }] },
                ...(configFiles.has(path.join(dir, 'vitest.config.ts')) && (portable.length > 0 || suiteFiles.includes(storybook)) ? { test: { inputs: [...compilerInputs, ...containers.map((candidate) => `!{workspaceRoot}/${candidate}`)] } } : {}),
                ...integration,
            },
        };
    }),
    'vitest.config.ts': {},
    'vite.config.ts': Effect.fnUntraced(function* ({ dir }, { configFiles }) {
        const path = yield* Path.Path;
        const hosted = configFiles.has(path.join(dir, CLOUDFLARE_CONFIG));
        return {
            tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])],
            targets: {
                build: { command: 'vite build --configLoader native', options: { cwd: '{projectRoot}' } },
                ...(hosted ? { infra: {}, publish: {} } : {}),
            },
        };
    }),
    ...Record.fromIterableWith(['uxp', 'cep'] as const, (host) => [`${host}.config.ts` as const, { tags: [`host:${host}`], targets: { build: { command: `vite build --configLoader native --config ${host}.config.ts`, options: { cwd: '{projectRoot}' } }, pack: {}, install: {} } }]),
    'perso-arabic/rasm.toml': ({ dir, base, name }) =>
        Effect.map(Path.Path, (path) => {
            const output = `.artifacts/fonts/${name}`;
            const action = (verb: string) => `foundry ${verb} ${path.join(dir, base)} --output ${output}`;
            const closure = [{ input: 'production', projects: ['foundry'] }, { input: 'production', projects: ['foundry'], dependencies: true }, { externalDependencies: [] }, { runtime: 'uv tree --frozen --package foundry' }, { runtime: 'python --version' }];
            return {
                root: '.',
                targets: {
                    'fonts:build': { command: action('build'), inputs: [folderInput(dir), `!${folderInput(path.join(dir, 'shaping'))}`, ...closure], outputs: [`{workspaceRoot}/${output}/*/**/*`, `!{workspaceRoot}/${output}/check/**/*`], cache: true },
                    'fonts:check': { command: action('check'), dependsOn: ['fonts:build'], inputs: [folderInput(dir), ...closure, { runtime: 'fontspector --version' }], outputs: [`{workspaceRoot}/${output}/check`], cache: true },
                    'fonts:publish': { command: action('publish'), dependsOn: ['fonts:check'] },
                },
            };
        }),
    'sea.config.ts': Effect.fnUntraced(function* ({ dir }, { configFiles, workspaceRoot }) {
        const path = yield* Path.Path;
        const projects = [...configFiles].filter((file) => path.basename(file) === XcodeProject.file && !path.relative(dir, file).startsWith('..'));
        const decoded = yield* Effect.forEach(projects, (file) => decodeFile(XcodeProject.schema, workspaceRoot, file));
        const settings = Array.flatten(Array.zipWith(projects, decoded, (file, { targets }) => targets.map(({ settings: held }) => [file, held.CODE_SIGN_ENTITLEMENTS] as const)));
        const entitlements = Array.filterMap(settings, ([file, held]) => Result.map(Result.fromNullishOr(held, Function.constVoid), (found) => path.join(path.dirname(path.dirname(file)), found)));
        const output = `{workspaceRoot}/.artifacts/${dir}`;
        const name = path.basename(dir);
        return {
            tags: ['output:sea'],
            targets: { pack: { command: 'vite build --configLoader native --config {projectRoot}/sea.config.ts', inputs: ['...', ...[...projects, Xcconfig.file, ...entitlements].map((file) => `{workspaceRoot}/${file}`)], outputs: [`${output}/{${name},${name}.mjs,sea.json,sea.entitlements,LaunchAgents}`] } },
        };
    }),
    [CLOUDFLARE_CONFIG]: { targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } },
    [ClaudeHooks.file]: Effect.fnUntraced(function* (_, { file, workspaceRoot }) {
        yield* Effect.fromOption(Schema.decodeOption(ClaudeHooks.schema)(yield* decodeFile(Schema.String, workspaceRoot, file)));
        return { tags: ['host:claude'], targets: { 'lint:claude': {}, 'typecheck:claude': {} } };
    }),
};
const marked = (key: string) => Option.map(Record.get(markers, key), (configure) => [key, configure] as const);
const infer = Effect.fnUntraced(function* (file: string, workspace: Inference) {
    const path = yield* Path.Path;
    const parsed = path.parse(file);
    const [marker, configure] = yield* Effect.fromOption(Array.findFirst([parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`], marked));
    const { root = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], targets = {}, ...configuration } = typeof configure === 'function' ? yield* configure(parsed, { ...workspace, file }) : configure;
    const hosted = Array.intersection(root.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
    const released = Option.match(tagPrefix(workspace, root, tags, configuration.name ?? Option.getOrUndefined(Option.map(Record.get(yield* workspace.packages, root), Struct.get('name')))), {
        onNone: () => targets,
        onSome: (MinVerTagPrefix) => Record.map(targets, (target) => ({ ...target, options: { ...target.options, env: { ...target.options?.env, MinVerTagPrefix } } })),
    });
    return { projects: { [root]: { ...configuration, root, targets: released, tags: Array.union(tags, hosted) } } };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachedSuites, folderInput, imports, infer, logged, markers, moduleRequests, owning, packageName, reach, rootTargets, swiftProject, TESTS };
