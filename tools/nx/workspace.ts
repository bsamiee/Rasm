import type { NapiConfig } from '@ast-grep/napi';
import { NodeFileSystem, NodePath } from '@effect/platform-node';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type ImplicitDependency, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, findProjectForPath, globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, flow, Graph, identity, Layer, ManagedRuntime, Option, Path, pipe, Record, Schema, SchemaGetter, SchemaIssue, String, Trie } from 'effect';
import { parse } from 'smol-toml';
import { parse as parseYaml } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _DECLARATION = 'pyproject.toml';
const _PYTHON_LOWER_BOUND = /^>=\s*(?<version>\d+\.\d+)$/u;
const _PYTHON_TARGET_VERSION = /^py(?<major>\d)(?<minor>\d+)$/u;
const _PYTHON_FILE = /^(?!\.{1,2}\/)(?!.*\/\.{1,2}(?:\/|$))[\p{L}\p{N}_.][\p{L}\p{N}_.-]*(?:\/[\p{L}\p{N}_.-]+)+\.pyi?$/u;

// --- [MODELS] --------------------------------------------------------------------------

const _Toml = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: () => new SchemaIssue.InvalidValue({ expected: 'TOML' }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _PythonVersion = Schema.String.pipe(
    Schema.decodeTo(Schema.String, {
        decode: SchemaGetter.transformEffect((specifier: string, options) =>
            Option.match(Option.fromNullishOr(_PYTHON_LOWER_BOUND.exec(specifier)?.groups?.version), {
                onNone: () => Effect.fail(new SchemaIssue.InvalidValue({ expected: 'a >=<major>.<minor> lower bound' }, specifier, options)),
                onSome: Effect.succeed,
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _Declaration = _Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ version: _PythonVersion }).pipe(Schema.encodeKeys({ version: 'requires-python' })) })));
const _Root = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ buildBackend: Schema.Struct({ moduleRoot: Schema.String, moduleName: Schema.String }).pipe(Schema.encodeKeys({ moduleRoot: 'module-root', moduleName: 'module-name' })) }).pipe(Schema.encodeKeys({ buildBackend: 'build-backend' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String), testPaths: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files', testPaths: 'testpaths' })),
                ruff: Schema.Struct({
                    perFileTargetVersion: Schema.OptionFromOptionalKey(
                        Schema.Record(Schema.String, Schema.String.check(Schema.isPattern(_PYTHON_TARGET_VERSION)).pipe(Schema.decodeTo(Schema.String, { decode: SchemaGetter.transform(String.replace(_PYTHON_TARGET_VERSION, '$<major>.$<minor>')), encode: SchemaGetter.forbiddenEncoding }))).check(
                            Schema.makeFilter((input) => Array.every(Record.keys(input), _PYTHON_FILE.test.bind(_PYTHON_FILE)), { expected: 'literal qualified workspace-relative Python file paths' }),
                        ),
                    ),
                }).pipe(Schema.encodeKeys({ perFileTargetVersion: 'per-file-target-version' })),
            }),
        }),
    ),
);
const _Manifest = _Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const _host = (dir: string): readonly string[] => Array.map(Array.intersection(dir.split('/'), ['rhino', 'blender']), (host) => `host:${host}`);
const _root = Effect.fnUntraced(function* (workspace: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { tool } = yield* Effect.flatMap(fs.readFileString(path.join(workspace, _DECLARATION)), Schema.decodeEffect(_Root));
    const { moduleRoot, moduleName } = tool.uv.buildBackend;
    return { moduleRoot, sourceRoot: path.join(moduleRoot, ...moduleName.split('.')), ...tool.pytest, perFileTargetVersion: tool.ruff.perFileTargetVersion };
});
const _python = Effect.fnUntraced(function* (dir: string, workspace: string, root: ReturnType<typeof _root>) {
    const [path, { moduleRoot, sourceRoot, pythonFiles, testPaths }] = yield* Effect.all([Path.Path, root]);
    const mirrors = Array.map(testPaths, (testPath) => path.join(testPath, dir.startsWith(`${moduleRoot}/`) ? path.join(path.dirname(moduleRoot), path.relative(moduleRoot, dir)) : dir));
    const tests = yield* Effect.tryPromise(() =>
        globWithWorkspaceContext(
            workspace,
            Array.flatMap(mirrors, (mirror) => Array.map(pythonFiles, (pattern) => `${mirror}/**/${pattern}`)),
        ),
    );
    const suites = Array.filter(mirrors, (mirror) => Array.some(tests, String.startsWith(`${mirror}/`)));
    return {
        moduleRoot,
        sourceRoot,
        targets: Array.isReadonlyArrayNonEmpty(suites) ? { check: {}, test: { command: `pytest ${Array.map(suites, (suite) => `"${suite}"`).join(' ')} --cov`, inputs: Array.map(suites, (suite) => `{workspaceRoot}/${suite}/**/*`) } } : {},
    };
});
const _sources = Effect.fnUntraced(function* (workspace: string, configuration: ReturnType<typeof _root>) {
    const [fs, path, { moduleRoot }] = yield* Effect.all([FileSystem.FileSystem, Path.Path, configuration]);
    const files = yield* Effect.tryPromise(() => globWithWorkspaceContext(workspace, ['**/*.py']));
    const [, rule]: [Effect.Success<typeof _registerPython>, NapiConfig] = yield* Effect.all([_registerPython, Effect.map(fs.readFileString(path.join(workspace, 'tools/ast-grep/utils/python/python-static-imported-module.yml')), parseYaml)], { concurrency: 'unbounded' });
    const sources = yield* Effect.validate(
        files,
        Effect.fnUntraced(function* (file) {
            const text = yield* fs.readFileString(path.join(workspace, file));
            return yield* Effect.tryPromise(async () => {
                const { parseAsync } = await import('@ast-grep/napi');
                const parsed = await parseAsync('python', text);
                const imports = parsed.root().findAll(rule);
                const source = path.parse(path.relative(file.startsWith(`${moduleRoot}/`) ? moduleRoot : '.', file));
                return {
                    file,
                    name: (source.name === '__init__' && source.dir !== '' ? source.dir : path.join(source.dir, source.name)).replaceAll(path.sep, '.'),
                    imports: Array.flatMap(imports, (module) => {
                        const statement = Option.fromNullishOr(module.getMatch('FROM'));
                        const members = Array.flatMap(Option.toArray(statement), (node) => Array.flatMap(node.fieldChildren('name'), (member) => member.findAll({ rule: { kind: 'dotted_name' } })));
                        const runtime = !module.inside({ rule: { kind: 'block', inside: { kind: 'if_statement', field: 'consequence', has: { field: 'condition', any: [{ pattern: 'TYPE_CHECKING' }, { pattern: 'typing.TYPE_CHECKING' }] } } } });
                        return Array.map([[module], ...Array.map(members, (member) => [module, member])], (names) => ({
                            name: Array.flatMap(names, (name) => name.namedChildren())
                                .map((part) => part.text())
                                .join('.'),
                            runtime,
                        }));
                    }),
                };
            });
        }),
        { concurrency: 'unbounded' },
    );
    const graph = Graph.directed<string, boolean>((mutable) => {
        const nodes = Array.map(sources, (source) => ({ ...source, index: Graph.addNode(mutable, source.file) }));
        const modules = Trie.fromIterable(Array.map(nodes, ({ name, index }) => [`${name}.`, index] as const));
        nodes.forEach(({ index, imports }) => {
            imports.forEach((dependency) => {
                Option.toArray(Trie.longestPrefixOf(modules, `${dependency.name}.`)).forEach(([, target]) => {
                    Graph.addEdge(mutable, index, target, dependency.runtime);
                });
            });
        });
    });
    const runtime = Graph.mutate(graph, (mutable) => {
        Graph.filterEdges(mutable, identity);
    });
    const closure = (roots: readonly string[]): readonly string[] =>
        Array.fromIterable(
            Graph.values(
                Graph.dfs(runtime, {
                    start: Array.map(
                        Array.filter(Array.fromIterable(graph), ([, file]) => Array.some(roots, (root) => (root.endsWith('/') ? file.startsWith(root) : file === root))),
                        ([index]) => index,
                    ),
                }),
            ),
        );
    return { graph, closure };
});

const _PROJECTS = {
    '*.csproj': ({ dir }) => {
        const host = _host(dir);
        const plugin = dir.startsWith('apps/rhino/');
        const outputs = (pivot: string): string[] => [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/{projectName}${host.includes('host:rhino') ? '_*' : ''}/${pivot}`, ...(plugin ? [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/*/${pivot}_{projectName}`] : [])];
        return Effect.succeed({
            root: dir,
            tags: ['language:dotnet', ...host],
            targets: {
                build: { outputs: outputs('debug') },
                'build:release': { outputs: outputs('release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                typecheck: {},
                check: {},
                ...(plugin ? { pack: {}, install: {} } : {}),
            },
        });
    },
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { workspace }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = path.join(dir, base);
        yield* Effect.filterOrFail(fs.readFileString(path.join(workspace, file)), String.startsWith('#!'));
        const inputs = ['default', '{workspaceRoot}/.editorconfig'];
        return {
            root: dir,
            name: dir,
            tags: ['language:dotnet', ..._host(dir)],
            targets: {
                ...Record.fromIterableWith(['build', 'format', 'lint'], (target) => [target, { executor: 'nx:noop', dependsOn: [{ target: `${target}:*.cs`, params: 'forward' }] }]),
                [`restore:${base}`]: { inputs: ['default', 'dotnet'], outputs: [`{workspaceRoot}/.artifacts/dotnet/obj/${base}/project.assets.json`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*.nuget.*`], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs, outputs: [`{workspaceRoot}/.artifacts/dotnet/bin/${base}`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*/**/*`], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--no-restore', '--'] } },
                ...Record.fromIterableWith(['format', 'lint'], (target) => [
                    `${target}:${base}`,
                    {
                        options: { args: [file, '--no-restore', ...(target === 'lint' ? ['--verify-no-changes'] : [])] },
                        dependsOn: [`${target === 'lint' ? 'build' : 'restore'}:${base}`],
                        ...(target === 'lint' ? { cache: true, inputs: [...inputs, 'dotnet'] } : {}),
                    },
                ]),
                typecheck: {},
                check: {},
            },
        };
    }),
    '.swcrc': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:extendscript'], targets: { build: {} } }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir, base }, { workspace, root, sources }) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [{ id }, python] = yield* Effect.all([Effect.flatMap(fs.readFileString(path.join(workspace, dir, base)), Schema.decodeEffect(_Manifest)), _python(dir, workspace, root)], { concurrency: 'unbounded' });
        const outputPath = '.artifacts/blender/{projectName}';
        const archive = `${outputPath}/extension.zip`;
        const modules = Array.filter((yield* sources).closure([`${dir}/`]), String.startsWith(`${python.sourceRoot}/`));
        return {
            root: dir,
            name: id,
            tags: ['language:python', 'host:blender'],
            targets: {
                pack: {
                    command: `python -m eng.python.fitout pack "{projectRoot}" "${archive}" ${Array.map(modules, (file) => `--modules "${path.relative(python.moduleRoot, file)}"`).join(' ')}`,
                    inputs: Array.map(Array.dedupe(Array.map(modules, path.dirname)), flow(String.replace('', '{workspaceRoot}/'), String.concat('/**/*'))),
                    outputs: [`{workspaceRoot}/${outputPath}`],
                },
                install: { command: `python -m eng.python.fitout install "${archive}"` },
                ...python.targets,
            },
        };
    }),
    'project.xcproj': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), name: path.parse(dir).name, tags: ['language:swift', 'host:macos'], targets: { build: {}, install: {}, upgrade: {}, lint: {}, check: {} } })),
    'py.typed': Effect.fnUntraced(function* ({ dir }, { workspace, root }) {
        const [path, python] = yield* Effect.all([Path.Path, _python(dir, workspace, root)]);
        const parts = yield* Effect.fromOption(Option.liftPredicate(path.relative(python.moduleRoot, dir).split(path.sep), (relative) => !Array.contains(relative, '..')));
        return { root: dir, name: parts.join('.'), tags: ['language:python'], targets: python.targets };
    }),
    'tsconfig.json': ({ dir }) => Effect.succeed({ root: dir, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'vite.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['bundler:vite'], targets: { build: {}, serve: {} } }),
    'uxp.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:uxp'], targets: { build: {} } }),
} as const satisfies Record<string, (file: Path.Path.Parsed, context: { workspace: string; root: ReturnType<typeof _root>; sources: ReturnType<typeof _sources> }) => Effect.Effect<ProjectConfiguration, unknown, FileSystem.FileSystem | Path.Path>>;

const _pythonVersions = Effect.fnUntraced(function* (declarations: readonly string[], workspace: string, configuration: ReturnType<typeof _root>, sources: ReturnType<typeof _sources>) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const directories = yield* Effect.validate(
        declarations,
        (file) =>
            pipe(
                fs.readFileString(path.join(workspace, file)),
                Effect.flatMap(Schema.decodeEffect(_Declaration)),
                Effect.map(({ project }) => ({ version: project.version, root: `${path.dirname(file)}/` })),
            ),
        { concurrency: 'unbounded' },
    );
    const [{ perFileTargetVersion }, { closure }] = yield* Effect.all([configuration, sources]);
    const versions = [...directories, ...Array.map(Array.flatMap(Option.toArray(perFileTargetVersion), Record.toEntries), ([file, version]) => ({ version, root: file }))];
    const runtimeVersions = pipe(
        versions,
        Array.groupBy(({ version }) => version),
        Record.map((members, version) => Record.fromIterableWith(closure(Array.map(members, ({ root }) => root)), (file) => [file, version])),
        Record.reduce(Record.empty<string, string>(), (runtime, scope) => Record.union(runtime, scope, (a, b) => (a.localeCompare(b, 'en', { numeric: true }) <= 0 ? a : b))),
    );
    const scopes = pipe(
        Record.toEntries(runtimeVersions),
        Array.groupBy(([, version]) => version),
        Record.map(Array.map(([file]) => file)),
    );
    const runtimeFiles = Record.keys(runtimeVersions);
    const ruff = `--config 'per-file-target-version = { ${pipe(
        Record.toEntries(runtimeVersions),
        Array.map(([file, version]) => `${JSON.stringify(file)} = ${JSON.stringify(`py${version.replace('.', '')}`)}`),
    ).join(', ')} }'`;
    const targets: Record<string, TargetConfiguration> = {
        ...pipe(
            scopes,
            Record.toEntries,
            Array.flatMap(([version, files]) => {
                const roots = Array.map(files, (file) => `"${file}"`).join(' ');
                return [
                    [`typecheck:ty-${version}`, { command: `ty check --python-version ${version} ${roots}` }],
                    [`typecheck:mypy-${version}`, { command: `mypy --python-version ${version} ${roots}` }],
                ] as const;
            }),
            Record.fromEntries,
        ),
        'typecheck:ty': { options: { args: Array.map(runtimeFiles, (file) => `--exclude "${file}"`).join(' ') } },
        'typecheck:mypy': { options: { args: Array.map(runtimeFiles, (file) => `--exclude '^${RegExp.escape(file)}$'`).join(' ') } },
        ...Record.fromIterableWith(Array.cartesian(['lint', 'format'], ['check', 'format']), ([target, operation]) => [`${target}:ruff-${operation}`, { options: { args: ruff } }]),
    };
    return { projects: { '.': { root: '.', targets } } };
});
const _dependencies = Effect.fnUntraced(function* ({ projects, workspaceRoot }: CreateDependenciesContext) {
    const { graph } = yield* _sources(workspaceRoot, _root(workspaceRoot));
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const owner = (index: Graph.NodeIndex): Option.Option<string> => Option.flatMap(Graph.getNode(graph, index), (file) => Option.fromNullishOr(findProjectForPath(file, roots)));
    return pipe(
        Graph.edges(graph),
        Graph.values,
        Array.fromIterable,
        Array.flatMap(({ source, target }) => Option.toArray(Option.product(owner(source), owner(target)))),
        Array.filter(([source, target]) => source !== target),
        Array.map(([source, target]): ImplicitDependency => ({ source, target, type: DependencyType.implicit })),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const _registerPython = Effect.runSync(
    Effect.cached(
        Effect.tryPromise(async () => {
            const grammar = import('@ast-grep/lang-python');
            const { registerDynamicLanguage } = await import('@ast-grep/napi');
            registerDynamicLanguage({ python: (await grammar).default });
        }),
    ),
);
const _services = Layer.merge(NodeFileSystem.layer, NodePath.layerPosix);
const _hosted = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes = [
    `{${_DECLARATION},*/**/{${[...Record.keys(_PROJECTS), _DECLARATION].join(',')}}}`,
    async (files, options, context): Promise<CreateNodesResultArray> => {
        await using runtime = ManagedRuntime.make(_services);
        const [root, path] = runtime.runSync(Effect.all([Effect.cached(_root(context.workspaceRoot)), Path.Path]));
        const sources = runtime.runSync(Effect.cached(_sources(context.workspaceRoot, root)));
        const declarations = Array.filter(files, (file) => file !== _DECLARATION && path.basename(file) === _DECLARATION);
        const project = Effect.fnUntraced(
            function* (file: string) {
                if (file === _DECLARATION) {
                    return yield* _pythonVersions(declarations, context.workspaceRoot, root, sources);
                }
                const parsed = path.parse(file);
                const [, configure] = yield* Effect.fromOption(Array.findFirst(Record.toEntries(_PROJECTS), ([key]) => key === parsed.base || key === `*${parsed.ext}`));
                const configuration = yield* configure(parsed, { workspace: context.workspaceRoot, root, sources });
                return { projects: { [configuration.root]: configuration } };
            },
            Effect.catchNoSuchElement,
            Effect.map(Option.getOrElse(() => ({}))),
            _hosted,
        );
        return await createNodesFromFiles((file) => runtime.runPromise(project(file)), files, options, context);
    },
];
const createDependencies: CreateDependencies = (_options, context) => _dependencies(context).pipe(Effect.provide(_services, { local: true }), _hosted, Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
