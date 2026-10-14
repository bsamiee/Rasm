import type { SgRoot } from '@ast-grep/napi';
import { NodeFileSystem, NodePath } from '@effect/platform-node';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type ProjectConfiguration, type StaticDependency } from '@nx/devkit';
import { createProjectRootMappingsFromProjectConfigurations, findProjectForPath, globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Cause, Effect, FileSystem, Graph, identity, Layer, ManagedRuntime, Option, Path, pipe, Record, Schema, SchemaGetter, SchemaIssue, String, Trie, Tuple } from 'effect';
import { XMLParser } from 'fast-xml-parser';
import { parse as parseToml } from 'smol-toml';
import { parse as parseYaml } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CRATE = 'Cargo.toml';
const _DECLARATION = 'pyproject.toml';
const _PYTHON_LOWER_BOUND = /^>=\s*(?<version>\d+\.\d+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const _Parsed = (expected: string, decode: (text: string) => unknown): Schema.decodeTo<Schema.Unknown, Schema.String> =>
    Schema.String.pipe(
        Schema.decodeTo(Schema.Unknown, {
            decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => decode(text), catch: () => new SchemaIssue.InvalidValue({ expected }, text, options) })),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
const _Toml = _Parsed('TOML', parseToml);
const _Declaration = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            project: Schema.Struct({
                version: Schema.String.pipe(
                    Schema.decodeTo(Schema.String, {
                        decode: SchemaGetter.transformEffect((specifier: string, options) => Effect.fromOption(Option.fromNullishOr(_PYTHON_LOWER_BOUND.exec(specifier)?.groups?.version), () => new SchemaIssue.InvalidValue({ expected: 'a >=<major>.<minor> lower bound' }, specifier, options))),
                        encode: SchemaGetter.forbiddenEncoding,
                    }),
                ),
            }).pipe(Schema.encodeKeys({ version: 'requires-python' })),
        }),
    ),
);
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
const _Dependencies = Schema.Record(Schema.String, Schema.Unknown).pipe(Schema.withDecodingDefaultKey(Effect.succeed({})));
const _TABLES = { dependencies: _Dependencies, devDependencies: _Dependencies, buildDependencies: _Dependencies };
const _TABLE_KEYS = { devDependencies: 'dev-dependencies', buildDependencies: 'build-dependencies' } as const;
const _Crate = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            package: Schema.Struct({ name: Schema.String }),
            bin: Schema.Array(Schema.Struct({ name: Schema.String })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))),
            target: Schema.Record(Schema.String, Schema.Struct(_TABLES).pipe(Schema.encodeKeys(_TABLE_KEYS))).pipe(Schema.withDecodingDefaultKey(Effect.succeed({}))),
            ..._TABLES,
        }).pipe(Schema.encodeKeys(_TABLE_KEYS)),
    ),
);
const _Row = Schema.Struct({ path: Schema.OptionFromOptionalKey(Schema.String) });
const _Catalog = _Toml.pipe(Schema.decodeTo(Schema.Struct({ workspace: Schema.Struct({ dependencies: Schema.Record(Schema.String, Schema.Union([Schema.String.pipe(Schema.decodeTo(_Row, { decode: SchemaGetter.transform(() => ({})), encode: SchemaGetter.forbiddenEncoding })), _Row])) }) })));
const _MSBuildProject = _Parsed('MSBuild XML', (text) => new XMLParser({ isArray: (name) => name === 'PropertyGroup' }).parse(text)).pipe(
    Schema.decodeTo(Schema.Struct({ Project: Schema.Struct({ PropertyGroup: Schema.Array(Schema.Struct({ TargetExt: Schema.optionalKey(Schema.String) })).pipe(Schema.withDecodingDefaultKey(Effect.succeed([]))) }) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const _decode = Effect.fnUntraced(function* <A>(schema: Schema.Codec<A, string>, workspace: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* Effect.flatMap(fs.readFileString(path.join(workspace, file)), Schema.decodeEffect(schema));
});
const _root = Effect.fnUntraced(function* (workspace: string) {
    const { tool } = yield* _decode(_Root, workspace, _DECLARATION);
    return { moduleRoot: tool.uv.buildBackend.moduleRoot, ...tool.pytest };
});
const _python = Effect.fnUntraced(function* (dir: string, workspace: string, root: ReturnType<typeof _root>) {
    const [path, { moduleRoot, pythonFiles, testPaths }] = yield* Effect.all([Path.Path, root]);
    const mirrors = Array.map(testPaths, (testPath) => path.join(testPath, dir.startsWith(`${moduleRoot}/`) ? path.join(path.dirname(moduleRoot), path.relative(moduleRoot, dir)) : dir));
    const tests = yield* Effect.tryPromise(() =>
        globWithWorkspaceContext(
            workspace,
            Array.flatMap(mirrors, (mirror) => Array.map(pythonFiles, (pattern) => `${mirror}/**/${pattern}`)),
        ),
    );
    const suites = Array.filter(mirrors, (mirror) => Array.some(tests, String.startsWith(`${mirror}/`)));
    return Array.isReadonlyArrayNonEmpty(suites) ? { check: {}, test: { command: `pytest ${Array.map(suites, (suite) => `"${suite}"`).join(' ')} --cov`, inputs: Array.map(suites, (suite) => `{workspaceRoot}/${suite}/**/*`) } } : {};
});
const _sources = Effect.fnUntraced(function* (workspace: string, root: ReturnType<typeof _root>) {
    const [path, { moduleRoot }, files, parse, rule] = yield* Effect.all([Path.Path, root, Effect.tryPromise(() => globWithWorkspaceContext(workspace, ['**/*.py'])), _parse, Effect.map(_decode(Schema.String, workspace, 'tools/ast-grep/utils/python/python-static-imported-module.yml'), parseYaml)], {
        concurrency: 'unbounded',
    });
    const sources = yield* Effect.validate(
        files,
        (file) =>
            Effect.map(
                Effect.flatMap(_decode(Schema.String, workspace, file), (text) => Effect.tryPromise(() => parse(text))),
                (tree) => {
                    const source = path.parse(path.relative(file.startsWith(`${moduleRoot}/`) ? moduleRoot : '.', file));
                    const parts = (source.name === '__init__' && source.dir !== '' ? source.dir : path.join(source.dir, source.name)).split(path.sep);
                    const statements = Array.flatMap(tree.root().findAll(rule), (module) => {
                        const runtime = !module.inside({ rule: { kind: 'block', inside: { kind: 'if_statement', field: 'consequence', has: { field: 'condition', any: [{ pattern: 'TYPE_CHECKING' }, { pattern: 'typing.TYPE_CHECKING' }] } } } });
                        const members = pipe(
                            Option.fromNullishOr(module.getMatch('FROM')),
                            Option.toArray,
                            Array.flatMap((statement) => statement.fieldChildren('name')),
                            Array.flatMap((name) => name.findAll({ rule: { kind: 'dotted_name' } })),
                        );
                        return Array.map([module.text(), ...Array.map(members, (member) => `${module.text()}.${member.text()}`)], (name) => ({ name, runtime }));
                    });
                    return { file, name: parts.join('.'), imports: [...Array.map(parts.slice(0, -1), (_, position) => ({ name: parts.slice(0, position + 1).join('.'), runtime: true })), ...statements] };
                },
            ),
        { concurrency: 'unbounded' },
    );
    return Graph.directed<string, boolean>((mutable) => {
        const nodes = Array.map(sources, (source) => ({ ...source, index: Graph.addNode(mutable, source.file) }));
        const modules = Trie.fromIterable(Array.map(nodes, ({ name, index }) => [`${name}.`, index] as const));
        Array.flatMap(nodes, ({ index, imports }) => Array.flatMap(imports, ({ name, runtime }) => Option.toArray(Option.map(Trie.longestPrefixOf(modules, `${name}.`), ([, target]) => [index, target, runtime] as const)))).forEach(([source, target, runtime]) => {
            Graph.addEdge(mutable, source, target, runtime);
        });
    });
});

const _PROJECTS = {
    '*.csproj': Effect.fnUntraced(function* ({ dir }, { file, workspace }) {
        const { Project } = yield* _decode(_MSBuildProject, workspace, file);
        const plugin = Array.some(Project.PropertyGroup, ({ TargetExt }) => TargetExt === '.rhp');
        const outputs = (pivot: string): string[] => [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/{projectName}/${pivot}*`, ...(plugin ? [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/*/{projectName}_${pivot}*`] : [])];
        return {
            root: dir,
            tags: ['language:dotnet', ...(plugin ? ['output:rhp'] : [])],
            targets: {
                build: { outputs: outputs('debug') },
                'build:release': { outputs: outputs('release'), ...(plugin ? { defaultConfiguration: 'release' } : {}) },
                typecheck: {},
                check: {},
                ...(plugin ? { pack: {}, install: {} } : {}),
            },
        };
    }),
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, { file, workspace }) {
        yield* Effect.filterOrFail(_decode(Schema.String, workspace, file), String.startsWith('#!'));
        const inputs = ['default', '{workspaceRoot}/.editorconfig'];
        return {
            root: dir,
            name: dir,
            tags: ['language:dotnet'],
            targets: {
                build: { executor: 'nx:noop', dependsOn: [{ target: 'build:*.cs', params: 'forward' }] },
                format: { executor: 'nx:noop', dependsOn: [{ target: 'format:*.cs', params: 'forward' }] },
                lint: { executor: 'nx:noop', dependsOn: [{ target: 'lint:*.cs', params: 'forward' }] },
                [`restore:${base}`]: { inputs: ['default', 'dotnet'], outputs: [`{workspaceRoot}/.artifacts/dotnet/obj/${base}/project.assets.json`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*.nuget.*`], options: { args: [file] } },
                [`build:${base}`]: { dependsOn: [`restore:${base}`], inputs, outputs: [`{workspaceRoot}/.artifacts/dotnet/bin/${base}`, `{workspaceRoot}/.artifacts/dotnet/obj/${base}/*/**/*`], options: { args: [file, '--no-restore'] } },
                [`run:${base}`]: { dependsOn: [`build:${base}`], options: { args: [file, '--no-build', '--no-restore', '--'] } },
                [`format:${base}`]: { options: { args: [file, '--no-restore'] }, dependsOn: [`restore:${base}`] },
                [`lint:${base}`]: { options: { args: [file, '--no-restore', '--verify-no-changes'] }, dependsOn: [`build:${base}`], cache: true, inputs: [...inputs, 'dotnet'] },
                typecheck: {},
                check: {},
            },
        };
    }),
    '.swcrc': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:extendscript'], targets: { build: {} } }),
    [_CRATE]: ({ dir }, { file, workspace }) =>
        Effect.map(_decode(_Crate, workspace, file), (crate) => ({
            root: dir,
            name: crate.package.name,
            tags: ['language:rust', ...('dylint_linting' in crate.dependencies ? ['tool:dylint'] : [])],
            targets: { typecheck: {}, test: {}, lint: {}, check: {}, ...(Array.isReadonlyArrayNonEmpty(crate.bin) ? { build: { outputs: Array.map(crate.bin, (bin) => `{workspaceRoot}/.artifacts/rust/debug/${bin.name}`) }, install: {} } : {}) },
        })),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir }, { file, workspace, root, sources }) {
        const [path, { moduleRoot }, { id }, targets, runtime] = yield* Effect.all([Path.Path, root, _decode(_Manifest, workspace, file), _python(dir, workspace, root), sources], { concurrency: 'unbounded' });
        const archive = '.artifacts/blender/{projectName}/extension.zip';
        const modules = pipe(Graph.dfs(runtime, { start: Graph.findNodes(runtime, String.startsWith(`${dir}/`)) }), Graph.values, Array.fromIterable, Array.filter(String.startsWith(`${moduleRoot}/`)));
        const folders = Array.dedupe(Array.map(modules, path.dirname));
        return {
            root: dir,
            name: id,
            tags: ['language:python'],
            targets: {
                pack: {
                    command: `python -m eng.python.fitout pack "{projectRoot}" "${archive}" ${Array.map(modules, (module) => `"${path.relative(moduleRoot, module)}"`).join(' ')}`,
                    inputs: Array.map(
                        Array.filter(folders, (folder) => !Array.some(folders, (other) => folder.startsWith(`${other}/`))),
                        (folder) => `{workspaceRoot}/${folder}/**/*`,
                    ),
                    outputs: [`{workspaceRoot}/${archive}`],
                },
                install: { command: `python -m eng.python.fitout install "${archive}"` },
                ...targets,
            },
        };
    }),
    'project.xcproj': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), name: path.parse(dir).name, tags: ['language:swift', 'host:macos'], targets: { build: {}, install: {}, upgrade: {}, lint: {}, check: {} } })),
    'py.typed': ({ dir }, { workspace, root }) => Effect.map(Effect.all([Path.Path, root, _python(dir, workspace, root)]), ([path, { moduleRoot }, targets]) => ({ root: dir, name: path.relative(moduleRoot, dir).replaceAll(path.sep, '.'), tags: ['language:python'], targets })),
    'tsconfig.json': ({ dir }) => Effect.succeed({ root: dir, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'vite.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['bundler:vite'], targets: { build: {}, serve: {} } }),
    'uxp.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:uxp'], targets: { build: {}, install: {} } }),
} as const satisfies Record<string, (file: Path.Path.Parsed, context: { readonly file: string; readonly workspace: string; readonly root: ReturnType<typeof _root>; readonly sources: ReturnType<typeof _sources> }) => Effect.Effect<ProjectConfiguration, unknown, FileSystem.FileSystem | Path.Path>>;

const _pythonVersions = Effect.fnUntraced(function* (workspace: string) {
    const [path, declarations] = yield* Effect.all([Path.Path, Effect.tryPromise(() => globWithWorkspaceContext(workspace, [`*/**/${_DECLARATION}`]))]);
    const versions = pipe(
        yield* Effect.validate(declarations, (file) => Effect.map(_decode(_Declaration, workspace, file), ({ project }) => ({ version: project.version, root: path.dirname(file) })), { concurrency: 'unbounded' }),
        Array.groupBy(({ version }) => version),
        Record.map(Array.map(({ root }) => root)),
        Record.toEntries,
    );
    const roots = Array.flatMap(versions, ([, scope]) => scope);
    return {
        projects: {
            '.': {
                root: '.',
                targets: {
                    ...Record.fromEntries(
                        Array.flatMap(versions, ([version, scope]) => {
                            const paths = Array.map(scope, (root) => `"${root}"`).join(' ');
                            return [
                                [`typecheck:ty-${version}`, { command: `ty check --python-version ${version} ${paths}` }],
                                [`typecheck:mypy-${version}`, { command: `mypy --python-version ${version} ${paths}` }],
                            ] as const;
                        }),
                    ),
                    'typecheck:ty': { options: { args: Array.map(roots, (root) => `--exclude "${root}"`).join(' ') } },
                    'typecheck:mypy': { options: { args: Array.map(roots, (root) => `--exclude '^${RegExp.escape(`${root}/`)}'`).join(' ') } },
                },
            },
        },
    };
});
const _dependencies = Effect.fnUntraced(function* ({ projects, workspaceRoot }: CreateDependenciesContext) {
    const roots = createProjectRootMappingsFromProjectConfigurations(projects);
    const [graph, { workspace }, crates] = yield* Effect.all(
        [
            _sources(workspaceRoot, _root(workspaceRoot)),
            _decode(_Catalog, workspaceRoot, _CRATE),
            Effect.flatMap(
                Effect.tryPromise(() => globWithWorkspaceContext(workspaceRoot, [`*/**/${_CRATE}`])),
                (files) => Effect.validate(files, (file) => Effect.map(_decode(_Crate, workspaceRoot, file), (crate) => [file, crate] as const), { concurrency: 'unbounded' }),
            ),
        ],
        { concurrency: 'unbounded' },
    );
    const owner = (file: string): Option.Option<string> => Option.fromNullishOr(findProjectForPath(file, roots));
    return Array.flatMap(
        [
            ...pipe(
                graph,
                Graph.edges,
                Graph.values,
                Array.fromIterable,
                Array.map(({ source, target }) => Option.product(Graph.getNode(graph, source), Graph.getNode(graph, target))),
                Array.getSomes,
            ),
            ...Array.flatMap(crates, ([file, crate]) =>
                pipe(
                    [crate, ...Record.values(crate.target)],
                    Array.flatMap((tables) => Array.flatMap([tables.dependencies, tables.devDependencies, tables.buildDependencies], Record.keys)),
                    Array.flatMap((key) => Option.toArray(Option.flatMap(Record.get(workspace.dependencies, key), ({ path }) => path))),
                    Array.map((path) => Tuple.make(file, path)),
                ),
            ),
        ],
        ([sourceFile, file]) =>
            pipe(
                Option.product(owner(sourceFile), owner(file)),
                Option.filter(([source, target]) => source !== target),
                Option.map(([source, target]): StaticDependency => ({ source, target, sourceFile, type: DependencyType.static })),
                Option.toArray,
            ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const _parse = Effect.runSync(
    Effect.cached(
        Effect.tryPromise(async () => {
            const grammar = import('@ast-grep/lang-python');
            const { parseAsync, registerDynamicLanguage } = await import('@ast-grep/napi');
            registerDynamicLanguage({ python: (await grammar).default });
            return (text: string): Promise<SgRoot> => parseAsync('python', text);
        }),
    ),
);
const _services = Layer.merge(NodeFileSystem.layer, NodePath.layerPosix);
const _rendered = Effect.catchCause((cause: Cause.Cause<unknown>) => Effect.die(new Error(Cause.pretty(cause))));
const createNodes: CreateNodes = [
    `{${_DECLARATION},*/**/{${Record.keys(_PROJECTS).join(',')}}}`,
    async (files, options, context): Promise<CreateNodesResultArray> => {
        await using runtime = ManagedRuntime.make(_services);
        const workspace = context.workspaceRoot;
        const [path, root] = runtime.runSync(Effect.all([Path.Path, Effect.cached(_root(workspace))]));
        const [sources, hosts] = runtime.runSync(
            Effect.all([
                Effect.cached(
                    Effect.map(_sources(workspace, root), (graph) =>
                        Graph.mutate(graph, (mutable) => {
                            Graph.filterEdges(mutable, identity);
                        }),
                    ),
                ),
                Effect.cached(Effect.flatMap(FileSystem.FileSystem, (fs) => fs.readDirectory(path.join(workspace, 'apps')))),
            ]),
        );
        const project = Effect.fnUntraced(
            function* (file: string) {
                const parsed = path.parse(file);
                const [, configure] = yield* Effect.fromOption(Array.findFirst(Record.toEntries(_PROJECTS), ([key]) => key === parsed.base || key === `*${parsed.ext}`));
                const configuration = yield* configure(parsed, { file, workspace, root, sources });
                const tags = Array.map(Array.intersection(configuration.root.split('/'), yield* hosts), (host) => `host:${host}`);
                return { projects: { [configuration.root]: { ...configuration, tags: Array.union(configuration.tags, tags) } } };
            },
            Effect.catchNoSuchElement,
            Effect.map(Option.getOrElse(() => ({}))),
        );
        return await createNodesFromFiles((file) => runtime.runPromise(_rendered(file === _DECLARATION ? _pythonVersions(workspace) : project(file))), files, options, context);
    },
];
const createDependencies: CreateDependencies = (_options, context) => _dependencies(context).pipe(Effect.provide(_services, { local: true }), _rendered, Effect.runPromise);

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
