import { createRequire, isBuiltin } from 'node:module';
import type { CreateNodesResult, Optional, ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { interpolate } from '@nx/devkit/internal';
import { Array, Boolean, Effect, Equal, Function, Option, Path, Predicate, Record, Result, Schema, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { quote } from 'shell-quote';
import { parseSync } from 'vite';
import { ClaudeHooks, decodeFile, XcodeProject } from '#/eng/typescript/manifest';
import { TextFormat } from '#/eng/typescript/text-format';
import { CARGO_MANIFEST, CLOUDFLARE_CONFIG, glob, type PluginServices, type Workspace, WorkspaceError } from '#/tools/nx/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Inference = Workspace & { readonly configFiles: readonly string[]; readonly dotnetRoots: readonly string[] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const TESTS = 'tests';
const CONTAINERS = /^(?:testcontainers|@testcontainers\/.+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const BlenderManifest = TextFormat.Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));
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
    const { staticImports, staticExports } = parseSync(file, yield* decodeFile(Schema.String, workspaceRoot, file)).module;
    const reexported = staticExports.flatMap(Struct.get('entries')).map(({ moduleRequest }) => Option.fromNullishOr(moduleRequest));
    return Array.dedupe([...staticImports.map(Struct.get('moduleRequest')), ...Array.getSomes(reexported)].map(Struct.get('value')));
});
const imports = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const [path, requests] = yield* Effect.all([Path.Path, moduleRequests(workspaceRoot, file)]);
    const { resolve } = createRequire(path.join(workspaceRoot, file));
    const [files, specifiers] = Array.partition(requests.filter(Predicate.not(isBuiltin)), (request) => (request.startsWith('.') || request.startsWith('#') ? Result.succeed(path.relative(workspaceRoot, resolve(request))) : Result.fail(request)));
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
const owning = Effect.fnUntraced(function* ({ packages }: Workspace, dir: string) {
    const [path, manifests] = yield* Effect.all([Path.Path, packages]);
    const lineage = Array.unfold(dir, (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const)));
    const owner = yield* Effect.fromOption(Array.findFirst([...lineage, '.'], (at) => Record.has(manifests, at)));
    return { owner, manifest: yield* Effect.fromOption(Record.get(manifests, owner)) };
});

// --- [INPUTS]
const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const suitePath = (path: Path.Path, base: string, dir: string) => path.join(base, ...dir.split(path.sep).filter((segment) => segment !== path.basename(base)));
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
const rootTargets = (targets: Readonly<Record<string, TargetConfiguration>>): CreateNodesResult => ({ projects: { '.': { root: '.', targets } } });

// --- [PROJECTS]
const tagPrefix = Effect.fnUntraced(function* ({ release }: Workspace, tags: readonly string[], name: string | undefined) {
    const { releaseTag, groups } = yield* release;
    const selectors = new Set(tags.map((tag) => `tag:${tag}`));
    return Option.map(Option.all([Option.fromUndefinedOr(name), Record.findFirst(groups, ({ projects }) => !selectors.isDisjointFrom(new Set(projects)))]), ([projectName, [releaseGroupName]]) => interpolate(releaseTag.pattern, { projectName, releaseGroupName }).replace('{version}', ''));
});
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
const attachedSuites = <Suite>(suites: Readonly<Record<string, Suite>>, module: string) => Record.getSomes(Record.map({ test: 'Tests', 'test:integration': 'IntegrationTests' }, (suffix) => Record.get(suites, `${module}${suffix}`)));
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: Record.fromIterableWith(['build', ...targets], (target) => [target, {}]),
});
const binlog = (workspaceRoot: string, target: string) => `-bl:${workspaceRoot}/.artifacts/dotnet/binlog/{projectName}/${target.replaceAll(':', '-')}.binlog`;
const logged = (workspaceRoot: string, target: string) => ({ env: { MSBUILD_LOGGING_ARGS: binlog(workspaceRoot, target) } });
const markers = {
    '*.csproj': Effect.fnUntraced(function* ({ dir, name }, workspace) {
        const { file, workspaceRoot } = workspace;
        const [{ Project }, sources] = yield* Effect.all([decodeFile(MSBuildProject, workspaceRoot, file), Effect.map(glob(workspaceRoot, [`${dir}/**/Windows/**/*.cs`]), Array.isArrayNonEmpty)]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const skin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhs');
        const product = plugin || skin;
        const windows = product || sources;
        const tags = ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(windows ? ['platform:windows'] : [])];
        const runtime = ['--runtime', 'win-x64'];
        const logs = Record.fromIterableWith((target: string) => [target, { options: logged(workspaceRoot, target) }]);
        const windowsPack = { 'pack:windows': { options: { args: runtime, ...logged(workspaceRoot, 'pack:windows') } } };
        const packaged = {
            ...(product ? windowsPack : {}),
            ...(plugin ? logs(['pack', 'install', 'publish']) : {}),
            ...(skin ? { install: { command: 'dotnet msbuild -target:InstallRhinoSkin -property:Configuration=Release', options: { cwd: '{projectRoot}', ...logged(workspaceRoot, 'install') }, dependsOn: ['pack:windows'] } } : {}),
        };
        return {
            ...(product ? { name } : {}),
            tags,
            targets: {
                ...logs(['build']),
                ...packaged,
                'build:release': { options: logged(workspaceRoot, 'build:release'), defaultConfiguration: 'release' },
                ...(windows ? { 'build:windows': { options: { cwd: '{projectRoot}', args: [...runtime, ...(product ? ['--configuration', 'Release'] : [])], ...logged(workspaceRoot, 'build:windows') } } } : {}),
                check: {},
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, workspaceRoot, dotnetRoots }) {
        const path = yield* Path.Path;
        yield* Effect.filterOrFail(Effect.succeed(dotnetRoots.some((project) => !path.relative(project, dir).startsWith('..'))), Boolean.not);
        yield* Effect.filterOrFail(decodeFile(Schema.String, workspaceRoot, file), (text) => text.startsWith('#!'));
        return {
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                ...Record.fromIterableWith(['build', 'format'], (step) => [step, { executor: 'nx:noop', dependsOn: [{ target: `${step}:*${ext}`, params: 'forward' as const }] }]),
                [`restore:${base}`]: { options: { args: [file, binlog(workspaceRoot, `restore:${base}`)] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], options: { args: [file, '--no-restore', binlog(workspaceRoot, `build:${base}`)] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--'] } },
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
        return {
            ...(sources.length > 0 ? suiteInput(...sources) : {}),
            name: crate.name,
            tags: ['language:rust', ...(dylint ? ['host:dylint'] : [])],
            targets: {
                ...(tested ? { check: {}, test: { options: { args } }, ...(dylint ? {} : { coverage: { options: { args } } }) } : {}),
                ...(bins.length > 0 ? { build: {} } : {}),
                ...(launched.length > 0 ? { install: { options: { args: launched } } } : {}),
            },
        };
    }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [{ packages }, { id }, targets] = yield* Effect.all([workspace.lockedPackages(dir), decodeFile(BlenderManifest, workspace.workspaceRoot, workspace.file), pythonTargets(dir, workspace)], { concurrency: 'unbounded' });
        return {
            name: id,
            tags: ['language:python', 'output:extension'],
            targets: {
                assets: {},
                pack: {
                    inputs: Array.getSomes(packages.map(Struct.get('directory')))
                        .map(Struct.get('path'))
                        .map(folderInput),
                },
                install: {},
                publish: {},
                ...targets,
            },
        };
    }),
    [XcodeProject.file]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { files, targets }] = yield* Effect.all([Path.Path, decodeFile(XcodeProject.schema, workspace.workspaceRoot, workspace.file)]);
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
            (source) => Effect.map(owning(workspace, path.join(path.dirname(source), path.parse(source).name)), Function.flow(Struct.get('manifest'), Struct.get('name'))),
        );
        const bundled = Array.match(Array.dedupe(owners), { onEmpty: () => [], onNonEmpty: (projects) => [{ projects, target: 'pack' }] });
        const xcode = swiftProject(['build:xcode', ...(updated ? ['output:appcast'] : [])], ['check', 'lint:periphery', 'pack', 'install', 'upgrade:xcode', ...(updated ? ['publish'] : [])]);
        const options = { env: { XCODE_DERIVED_DATA_PATH: path.join('.cache/xcode', root), PRODUCT_BUNDLE_IDENTIFIER: identifier } };
        return { root, name, ...xcode, targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) } };
    }),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, { moduleRoots, commands }, targets] = yield* Effect.all([Path.Path, workspace.pyproject, pythonTargets(dir, workspace)]);
        const moduleRoot = yield* Effect.fromOption(Array.findFirst(moduleRoots, (held) => !path.relative(held, dir).startsWith('..')));
        const command = commands.includes(dir);
        return { name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python', ...(command ? ['output:command'] : [])], targets: { ...targets, ...(command ? { install: {} } : {}) } };
    }),
    'tsconfig.json': Effect.fnUntraced(function* ({ dir }, workspace) {
        const { file, workspaceRoot, configs } = workspace;
        const [path, readConfig] = yield* Effect.all([Path.Path, configs]);
        const {
            config: { options, errors },
            files,
        } = yield* Effect.try(() => readConfig(file));
        yield* Array.match(errors, { onEmpty: () => Effect.void, onNonEmpty: Effect.fail });
        const [suite, { owner, manifest }, containers] = yield* Effect.all([suiteInputs('typescript', dir, workspaceRoot), owning(workspace, dir), containerTests(dir, workspaceRoot)], { concurrency: 'unbounded' });
        const named = owner === dir ? {} : { name: path.join(manifest.name, path.relative(owner, dir)) };
        const integration = Array.match(containers, { onEmpty: () => ({}), onNonEmpty: (tests) => ({ test: { options: { args: tests.map((test) => `--exclude='**/${test}'`) } }, 'test:integration': { options: { args: tests } } }) });
        return {
            ...suite,
            ...named,
            tags: ['language:typescript'],
            targets: {
                'typecheck:tsc': { inputs: [...files.map((held) => ({ fileset: `{workspaceRoot}/${held}`, includeIgnored: true })), ...(options.typeRoots ?? []).map((typeRoot) => ({ fileset: `{workspaceRoot}/${path.relative(workspaceRoot, typeRoot)}/**/*.d.ts`, includeIgnored: true }))] },
                check: {},
                ...integration,
                ...(owner === dir && Record.has(manifest.dependencies, '@effect/sql-pg') ? { 'lint:squawk': {} } : {}),
            },
        };
    }),
    'vite.config.ts': ({ dir }, { configFiles }) =>
        Effect.map(Path.Path, (path) => configFiles.includes(path.join(dir, CLOUDFLARE_CONFIG))).pipe(Effect.map((hosted) => ({ tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: {}, serve: {}, ...(hosted ? { hosting: {}, publish: {} } : {}) } }))),
    ...Record.fromIterableWith(['uxp', 'cep'] as const, (host) => [`${host}.config.ts` as const, { tags: [`host:${host}`], targets: { build: {}, pack: {}, install: {} } }]),
    'fonts/rasm.toml': ({ dir, base, name }) =>
        Effect.map(Path.Path, (path) => {
            const output = `.artifacts/fonts/${name}`;
            const action = (verb: string) => `foundry ${verb} ${path.join(dir, base)} --output ${output}`;
            const closure = [folderInput('libs/python/typography'), folderInput('tools/foundry'), { externalDependencies: [] }, { runtime: 'uv tree --frozen --package typography' }, { runtime: 'python --version' }];
            return {
                root: '.',
                targets: {
                    'fonts:build': { command: action('build'), inputs: [folderInput(dir), `!${folderInput(path.join(dir, 'shaping'))}`, ...closure], outputs: [`{workspaceRoot}/${output}/*/**/*`, `!{workspaceRoot}/${output}/check/**/*`], cache: true },
                    'fonts:check': { command: action('check'), dependsOn: ['fonts:build'], inputs: [folderInput(dir), ...closure, { runtime: 'fontspector --version' }], outputs: [`{workspaceRoot}/${output}/check`], cache: true },
                    'fonts:publish': { command: action('publish'), dependsOn: ['fonts:check'] },
                },
            };
        }),
    'sea.config.ts': { tags: ['output:sea'], targets: { pack: {} } },
    [CLOUDFLARE_CONFIG]: { targets: { types: {}, 'typecheck:tsc': { dependsOn: ['types'] } } },
    '.storybook/main.ts': { targets: { storybook: {} } },
    [ClaudeHooks.file]: Effect.fnUntraced(function* (_, { file, workspaceRoot }) {
        yield* Effect.fromOption(Schema.decodeOption(ClaudeHooks.schema)(yield* decodeFile(Schema.String, workspaceRoot, file)));
        return { tags: ['host:claude'], targets: { 'lint:claude': {}, 'typecheck:claude': {} } };
    }),
} as const satisfies Record<string, Optional<ProjectConfiguration, 'root'> | ((file: Path.Path.Parsed, workspace: Inference & { readonly file: string }) => Effect.Effect<Optional<ProjectConfiguration, 'root'>, unknown, PluginServices>)>;
const marked = (key: string) => Option.map(Record.get(markers, key), (configure) => [key, configure] as const);
const infer = Effect.fnUntraced(
    function* (file: string, workspace: Inference) {
        const path = yield* Path.Path;
        const parsed = path.parse(file);
        const [marker, configure] = yield* Effect.fromOption(Array.findFirst([parsed.base, path.join(path.basename(parsed.dir), parsed.base), `*${parsed.ext}`], marked));
        const { root = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], targets = {}, ...configuration }: Optional<ProjectConfiguration, 'root'> = Predicate.isFunction(configure) ? yield* configure(parsed, { ...workspace, file }) : configure;
        const hosted = Array.intersection(root.split('/'), yield* workspace.hosts).map((host) => `host:${host}`);
        const released = Option.match(yield* tagPrefix(workspace, tags, configuration.name), {
            onNone: () => targets,
            onSome: (MinVerTagPrefix) => Record.map(targets, (target) => ({ ...target, options: { ...target.options, env: { ...target.options?.env, MinVerTagPrefix } } })),
        });
        return { projects: { [root]: { ...configuration, root, targets: released, tags: Array.union(tags, hosted) } } };
    },
    Effect.catchNoSuchElement,
    Effect.map(Option.getOrElse(() => ({}))),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { attachedSuites, folderInput, type Inference, imports, infer, logged, markers, moduleRequests, owning, packageName, reach, rootTargets, suiteInput, suitePath, swiftProject, TESTS };
