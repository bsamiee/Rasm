import { createRequire, isBuiltin } from 'node:module';
import type { CreateNodesResult, Optional, ProjectConfiguration, TargetConfiguration } from '@nx/devkit';
import { findMatchingProjects, interpolate } from '@nx/devkit/internal';
import { createNodes } from '@nx/vitest';
import { ClaudeHooks, decodeFile, TextFormat, Xcconfig, XcodeProject } from '@rasm/eng';
import { Array, Effect, Equal, FileSystem, Function, Option, Path, Predicate, Record, Result, Schema, String, Struct } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parseSync } from 'rolldown/utils';
import { CARGO_MANIFEST, CLOUDFLARE_CONFIG, glob, type PluginServices, PYPROJECT, type readPython, type Workspace, WorkspaceError } from '#/workspace.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Inference = Workspace & { readonly configFiles: ReadonlySet<string>; readonly python: ReturnType<typeof readPython> };

// --- [CONSTANTS] -----------------------------------------------------------------------

const TESTS = 'tests';
const MODULES = 'node_modules';
const VITEST_INTEGRATION = 'vitest.integration.config';
const BLENDER_MANIFEST = 'blender_manifest.toml';
const VITE = { args: ['--configLoader', 'native'], inputs: ['typescript', 'nodePlatform'] } as const;

// --- [MODELS] --------------------------------------------------------------------------

const MSBuildProject = TextFormat('MSBuild XML', (text) => new XMLParser({ ignoreAttributes: false, parseTagValue: false, isArray: (name) => name === 'PropertyGroup' || name === 'ItemGroup' || name === 'PackageReference' }).parse(text)).pipe(
    Schema.decodeTo(
        Schema.Struct({
            Project: Schema.Struct({
                PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String), PlugInIcon: Schema.optionalKey(Schema.String), RhinoHost: Schema.optionalKey(Schema.String), IsPackable: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
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
const imports = (workspaceRoot: string) =>
    Effect.fnUntraced(function* (file: string) {
        const [path, text] = yield* Effect.all([Path.Path, decodeFile(Schema.String, workspaceRoot, file)]);
        const { staticImports, staticExports } = parseSync(file, text).module;
        const reexported = Array.getSomes(staticExports.flatMap(Struct.get('entries')).map(({ moduleRequest }) => Option.fromNullishOr(moduleRequest)));
        const requests = Array.dedupe([...staticImports.map(Struct.get('moduleRequest')), ...reexported].map(Struct.get('value')));
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
const lineage = (path: Path.Path, dir: string) => [...Array.unfold(dir, (at) => (at === path.dirname(at) ? Option.none() : Option.some([at, path.dirname(at)] as const))), '.'];
const owning = Effect.fnUntraced(function* ({ packages }: Workspace, dir: string) {
    const [path, manifests] = yield* Effect.all([Path.Path, packages]);
    const owner = yield* Effect.fromOption(Array.findFirst(lineage(path, dir), (at) => Record.has(manifests, at)));
    return { owner, manifest: yield* Effect.fromOption(Record.get(manifests, owner)) };
});
const rhinoHost = Effect.fnUntraced(function* (workspaceRoot: string, dir: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const props = yield* Effect.findFirst(
        lineage(path, dir).map((at) => path.join(at, 'Directory.Build.props')),
        (file) => fs.exists(path.join(workspaceRoot, file)),
    );
    const decoded = yield* Effect.transposeOption(Option.map(props, (file) => decodeFile(MSBuildProject, workspaceRoot, file)));
    return Option.exists(decoded, ({ Project }) => Project.PropertyGroup.some(({ RhinoHost }) => RhinoHost === 'true'));
});

// --- [INPUTS]

const folderInput = (folder: string) => `{workspaceRoot}/${folder}/**/*`;
const defaultInputs = (files: readonly NonNullable<ProjectConfiguration['namedInputs']>[string][number][]) => (files.length === 0 ? {} : { namedInputs: { default: ['{projectRoot}/**/*', ...files] } });
const externalInputs = (path: Path.Path, root: string, submodules: readonly string[], files: readonly (string | undefined)[], packages: readonly string[] = []) => {
    const outside = Array.dedupe(
        files
            .filter(Predicate.isString)
            .map(path.normalize)
            .filter((file) => file.startsWith('../'))
            .map((file) => path.join(root, file)),
    );
    const linked = submodules.filter((submodule) => outside.some(String.startsWith(`${submodule}${path.sep}`)));
    return defaultInputs([...[...outside, ...linked].map((file) => `{workspaceRoot}/${file}`), ...Array.match(packages, { onEmpty: () => [], onNonEmpty: (externalDependencies) => [{ externalDependencies: [...externalDependencies] }] })]);
};
const sharedInputs = Effect.fnUntraced(function* (workspaceRoot: string, dir: string, file: string) {
    const [path, fs, { files }] = yield* Effect.all([Path.Path, FileSystem.FileSystem, imports(workspaceRoot)(file)]);
    const shared = files.filter((held) => held.endsWith('.config.ts') && path.relative(dir, held).startsWith('..'));
    const modules = yield* Effect.forEach(shared, (held) => Effect.tryPromise((): Promise<Readonly<Record<string, unknown>>> => import(path.join(workspaceRoot, held))));
    const named = modules.flatMap(Object.values).filter(Predicate.isString).filter(path.isAbsolute);
    const marks = yield* Effect.filter(named, (value) => Effect.map(fs.exists(value), (exists) => exists && !path.relative(workspaceRoot, value).startsWith('..')));
    return marks.map((value) => `{workspaceRoot}/${path.relative(workspaceRoot, value)}`);
});
const rootTargets = (targets: Readonly<Record<string, TargetConfiguration>>): CreateNodesResult => ({ projects: { '.': { root: '.', targets } } });

// --- [PROJECTS]

const tagPrefix = ({ nxJsonConfiguration: { release } }: Workspace, root: string, tags: readonly string[], name: string | undefined) =>
    Option.flatMap(Option.fromUndefinedOr(name), (projectName) =>
        Option.flatMap(
            Record.findFirst(release?.groups ?? {}, ({ projects }) => findMatchingProjects(Array.ensure(projects), { [projectName]: { data: { root, tags: [...tags] } } }).length > 0),
            ([releaseGroupName, group]) => Option.map(Option.fromNullishOr(group.releaseTag?.pattern ?? release?.releaseTag?.pattern), (pattern) => interpolate(pattern, { projectName, releaseGroupName }).replace('{version}', '')),
        ),
    );
const swiftProject = (tags: readonly string[], targets: readonly string[]) => ({
    tags: ['language:swift', ...tags],
    targets: Record.fromIterableWith(['build', ...targets], (target) => [target, {}]),
});
const binlog = (path: Path.Path, cwd: string, target: string) => `-bl:${path.join(path.relative(cwd, '.'), '.artifacts/dotnet/binlog/{projectName}', `${target.replaceAll(':', '-')}.binlog`)}`;
const logged = (path: Path.Path, cwd: string, target: string) => ({ env: { MSBUILD_LOGGING_ARGS: binlog(path, cwd, target) } });
const markers: Readonly<
    Record<string, (Optional<ProjectConfiguration, 'root'> & { readonly hosts?: readonly string[] }) | ((file: Path.Path.Parsed, workspace: Inference & { readonly file: string }) => Effect.Effect<Optional<ProjectConfiguration, 'root'> & { readonly hosts?: readonly string[] }, Error, PluginServices>)>
> = {
    '*.csproj': Effect.fnUntraced(function* ({ dir, name }, workspace) {
        const { file, workspaceRoot } = workspace;
        const [path, { Project }, hosted, submodules] = yield* Effect.all([Path.Path, decodeFile(MSBuildProject, workspaceRoot, file), rhinoHost(workspaceRoot, dir), workspace.submodules]);
        const plugin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhp');
        const skin = Project.PropertyGroup.some(({ TargetExt }) => TargetExt === '.rhs');
        const product = plugin || skin;
        const packed = Project.PropertyGroup.some(({ IsPackable }) => IsPackable === 'true');
        const licensed = Project.ItemGroup.flatMap(({ PackageReference = [] }) => PackageReference).some((reference) => reference['@_Include'] === 'SixLabors.ImageSharp');
        const tags = ['language:dotnet', ...(plugin ? ['output:rhp'] : []), ...(packed ? ['output:nupkg'] : []), ...(licensed ? ['license:imagesharp'] : [])];
        const runtime = ['--runtime', 'win-x64'];
        const logs = Record.fromIterableWith((target: string) => [target, { options: logged(path, dir, target) }]);
        const windowsTargets = {
            'build:windows': { command: 'dotnet build', dependsOn: [{ projects: ['rasm'], target: 'restore' }], options: { cwd: '{projectRoot}', args: [...runtime, '--configuration', 'Release'], ...logged(path, dir, 'build:windows') }, parallelism: false },
            'pack:windows': { options: { args: runtime, ...logged(path, dir, 'pack:windows') } },
        };
        const build = {
            ...logs(['build']),
            ...(product ? { 'build:release': { dependsOn: [], options: { args: ['--no-restore', '--configuration', 'Release'], ...logged(path, dir, 'build:release') }, parallelism: false } } : logs(['build:release'])),
        };
        const packaged = {
            ...(product ? windowsTargets : {}),
            ...(plugin ? logs(['pack', 'install', 'publish']) : {}),
            ...(packed ? { ...logs(['pack']), publish: {} } : {}),
            ...(skin ? { install: { command: 'dotnet msbuild -target:InstallRhinoSkin -property:Configuration=Release', options: { cwd: '{projectRoot}', ...logged(path, dir, 'install') }, dependsOn: ['pack:windows'] } } : {}),
        };
        return {
            ...externalInputs(
                path,
                dir,
                submodules,
                Project.PropertyGroup.map(({ PlugInIcon }) => PlugInIcon),
            ),
            ...(product || packed ? { name } : {}),
            hosts: hosted ? ['rhino'] : [],
            tags,
            targets: {
                ...build,
                ...packaged,
                'lint:dotnet-format': { options: { args: [file] } },
                check: product ? { dependsOn: ['build:windows'] } : {},
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base, ext }, { file, workspaceRoot }) {
        yield* Effect.filterOrFail(decodeFile(Schema.String, workspaceRoot, file), (text) => text.startsWith('#!'));
        const [path, hosted] = yield* Effect.all([Path.Path, rhinoHost(workspaceRoot, dir)]);
        return {
            name: dir,
            hosts: hosted ? ['rhino'] : [],
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: `build:*${ext}`, params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: `format:*${ext}` }] },
                [`restore:${base}`]: { options: { args: [file, binlog(path, '.', `restore:${base}`)] } },
                [`build:${base}`]: { command: 'dotnet build', dependsOn: [`restore:${base}`], options: { args: [file, '--no-restore', binlog(path, '.', `build:${base}`)] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                [`lint:dotnet-format:${base}`]: { options: { args: [file] } },
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
    [BLENDER_MANIFEST]: Effect.fnUntraced(function* ({ dir }, { python }) {
        const { members } = yield* python;
        const { name } = yield* Effect.fromOption(Record.get(members, dir));
        return { name, tags: ['output:extension'], targets: { assets: {}, pack: {}, install: {}, publish: {} } };
    }),
    [PYPROJECT]: Effect.fnUntraced(function* ({ dir }, { python, packages }) {
        const [path, { roots, members, owners, mapping }, manifests] = yield* Effect.all([Path.Path, python, packages]);
        const member = yield* Effect.fromOption(Record.get(members, dir));
        const [owner, { name: workspaceName }] = yield* Effect.all([Effect.fromOption(Record.get(roots, member.root)), Effect.fromOption(Record.get(manifests, '.'))]);
        const coverage = Array.getSomes(member.requires.map((required) => Record.get(owners, required)))
            .filter(({ plugin }) => !plugin)
            .map(({ importName }) => `--cov=${importName}`);
        const environment = path.relative(dir, path.join(member.root, '.venv'));
        const command = Array.isReadonlyArrayNonEmpty(member.scripts);
        const fawltydeps = member.root === '.' ? { command: `fawltydeps --check --pyenv ${environment}` } : { command: `fawltydeps --check-unused --pyenv ${environment}`, dependsOn: [{ projects: [workspaceName], target: 'restore:python' }] };
        return {
            name: member.name,
            tags: ['language:python', ...(command ? ['output:command'] : [])],
            implicitDependencies: member.requires.filter((required) => required !== member.name && Record.has(owners, required)),
            namedInputs: { python: [...(member.root === dir ? [] : [owner.lock]), `{workspaceRoot}/${PYPROJECT}`, { externalDependencies: [] }, owner.interpreter] },
            targets: {
                ...(Array.isReadonlyArrayEmpty(member.requires) ? {} : { 'lint:fawltydeps': { ...fawltydeps, options: { cwd: '{projectRoot}', env: { FAWLTYDEPS_CUSTOM_MAPPING: mapping } }, inputs: ['default', 'python'] }, check: {} }),
                ...(command ? { install: {} } : {}),
                ...(member.suite ? { test: { options: { args: coverage } } } : {}),
            },
        };
    }),
    [XcodeProject.file]: Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, fs, { files, targets }, { products }, { DEVELOPMENT_TEAM }, submodules] = yield* Effect.all([Path.Path, FileSystem.FileSystem, decodeFile(XcodeProject.schema, workspace.workspaceRoot, workspace.file), workspace.swift, Xcconfig.read(workspace.workspaceRoot), workspace.submodules]);
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
                submodules,
                targets.flatMap(({ settings }) => [settings.CODE_SIGN_ENTITLEMENTS, settings.INFOPLIST_FILE]),
                catalogs,
            ),
            implicitDependencies: products.filter((product) => local.has(product.name)).flatMap(Struct.get('targets')),
            targets: { ...Record.map(xcode.targets, () => ({ options })), ...Record.fromIterableWith(['build', 'pack'], (step) => [step, { options, dependsOn: bundled }]) },
        };
    }),
    'tsconfig.json': { tags: ['language:typescript'], targets: { check: {} } },
    '.storybook/main.ts': {
        targets: {
            storybook: { command: 'storybook dev --ci', options: { cwd: '{projectRoot}' }, continuous: true },
            'build-storybook': {
                command: 'storybook build --output-dir "$NX_WORKSPACE_ROOT/.artifacts/typescript/storybook/{projectRoot}"',
                options: { cwd: '{projectRoot}' },
                cache: true,
                inputs: ['default', '^production', 'typescript'],
                outputs: ['{workspaceRoot}/.artifacts/typescript/storybook/{projectRoot}'],
            },
        },
    },
    [`${VITEST_INTEGRATION}.ts`]: Effect.fnUntraced(function* ({ dir, base }, { file, workspaceRoot, nxJsonConfiguration, packages }) {
        const suite = 'test:integration';
        const [nodes, manifests] = yield* Effect.all([Effect.tryPromise(() => Promise.resolve(createNodes[1]([file], { testMode: 'run', testTargetName: suite }, { workspaceRoot, nxJsonConfiguration }))), packages]);
        const [{ inputs = [] }, { name }] = yield* Effect.all([Effect.fromNullishOr(nodes[0]?.[1].projects?.[dir]?.targets?.[suite]), Effect.fromOption(Record.get(manifests, '.'))]);
        return {
            targets: { [suite]: { command: `vitest run --config ${base}`, options: { cwd: '{projectRoot}' }, inputs: [...inputs, 'nodePlatform'], outputs: [`{workspaceRoot}/.artifacts/typescript/vitest/{projectRoot}/${VITEST_INTEGRATION}`], dependsOn: [{ projects: [name], target: 'docker' }], parallelism: false } },
        };
    }),
    'vite.config.ts': Effect.fnUntraced(function* ({ dir }, { configFiles }) {
        const path = yield* Path.Path;
        const hosted = configFiles.has(path.join(dir, CLOUDFLARE_CONFIG));
        return { tags: ['bundler:vite', ...(hosted ? ['output:cloudflare'] : [])], targets: { build: { options: { args: [...VITE.args] }, inputs: ['...', ...VITE.inputs] }, ...(hosted ? { infra: {}, publish: {} } : {}) } };
    }),
    ...Record.fromIterableWith(['uxp', 'cep'] as const, (host) => [
        `${host}.config.ts` as const,
        Effect.fnUntraced(function* ({ dir, base }: Path.Path.Parsed, { file, workspaceRoot }: Inference & { readonly file: string }) {
            const marks = yield* sharedInputs(workspaceRoot, dir, file);
            return {
                tags: [`host:${host}`],
                targets: {
                    build: {
                        command: 'vite build',
                        options: { cwd: '{projectRoot}', args: [...VITE.args, '--config', base] },
                        inputs: ['production', '^production', ...VITE.inputs, ...marks, { env: 'LINK_PORT' }, { runtime: 'resvg --version' }, { runtime: 'usvg --version' }, 'version'],
                        outputs: ['{workspaceRoot}/.artifacts/{projectRoot}'],
                    },
                    pack: {},
                    install: {},
                },
            };
        }),
    ]),
    'perso-arabic/rasm.toml': Effect.fnUntraced(function* ({ dir, base, name }, { python }) {
        const [path, { roots, members }] = yield* Effect.all([Path.Path, python]);
        const script = 'foundry';
        const owner = yield* Effect.fromOption(Array.findFirst(Object.values(members), ({ scripts }) => scripts.includes(script)));
        const { interpreter } = yield* Effect.fromOption(Record.get(roots, owner.root));
        const output = `.artifacts/fonts/${name}`;
        const action = (verb: string) => `${script} ${verb} ${path.join(dir, base)} --output ${output}`;
        const closure = [{ input: 'production', projects: [owner.name] }, { input: 'production', projects: [owner.name], dependencies: true }, { externalDependencies: [] }, { runtime: `uv export --frozen --no-hashes --quiet --package ${owner.name}` }, interpreter];
        return {
            root: '.',
            targets: {
                'fonts:build': { command: action('build'), inputs: [folderInput(dir), `!${folderInput(path.join(dir, 'shaping'))}`, ...closure], outputs: [`{workspaceRoot}/${output}/*/**/*`, `!{workspaceRoot}/${output}/check/**/*`], cache: true },
                'fonts:check': { command: action('check'), dependsOn: ['fonts:build'], inputs: [folderInput(dir), ...closure, { runtime: 'fontspector --version' }], outputs: [`{workspaceRoot}/${output}/check`], cache: true },
                'fonts:publish': { command: action('publish'), dependsOn: ['fonts:check'] },
            },
        };
    }),
    'sea.config.ts': Effect.fnUntraced(function* ({ dir, base }, { configFiles, workspaceRoot }) {
        const path = yield* Path.Path;
        const projects = [...configFiles].filter((file) => path.basename(file) === XcodeProject.file && !path.relative(dir, file).startsWith('..'));
        const decoded = yield* Effect.forEach(projects, (file) => decodeFile(XcodeProject.schema, workspaceRoot, file));
        const settings = Array.flatten(Array.zipWith(projects, decoded, (file, { targets }) => targets.map(({ settings: held }) => [file, held.CODE_SIGN_ENTITLEMENTS] as const)));
        const entitlements = Array.filterMap(settings, ([file, held]) => Result.map(Result.fromNullishOr(held, Function.constVoid), (found) => path.join(path.dirname(path.dirname(file)), found)));
        const output = `{workspaceRoot}/.artifacts/${dir}`;
        const name = path.basename(dir);
        return {
            tags: ['output:sea'],
            targets: {
                pack: {
                    command: 'vite build',
                    options: { args: [...VITE.args, '--config', `{projectRoot}/${base}`] },
                    inputs: [...[...projects, Xcconfig.file, ...entitlements].map((file) => `{workspaceRoot}/${file}`), 'production', '^production', ...VITE.inputs],
                    outputs: [`${output}/{${name},${name}.mjs,sea.json,sea.entitlements,LaunchAgents}`],
                },
            },
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
    const { root = marker.split('/').reduce((at) => path.dirname(at), file), tags = [], targets = {}, hosts, ...configuration } = typeof configure === 'function' ? yield* configure(parsed, { ...workspace, file }) : configure;
    const hosted = (hosts ?? Array.intersection(root.split('/'), yield* workspace.hosts)).map((host) => `host:${host}`);
    const released = Option.match(tagPrefix(workspace, root, tags, configuration.name ?? Option.getOrUndefined(Option.map(Record.get(yield* workspace.packages, root), Struct.get('name')))), {
        onNone: () => targets,
        onSome: (MinVerTagPrefix) => Record.map(targets, (target) => ({ ...target, options: { ...target.options, env: { ...target.options?.env, MinVerTagPrefix } } })),
    });
    return { projects: { [root]: { ...configuration, root, targets: released, tags: Array.union(tags, hosted) } } };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { BLENDER_MANIFEST, folderInput, type Inference, imports, infer, logged, markers, owning, packageName, reach, rootTargets, swiftProject, TESTS };
