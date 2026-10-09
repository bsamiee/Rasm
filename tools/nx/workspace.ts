import type { NapiConfig } from '@ast-grep/napi';
import { NodeFileSystem, NodePath } from '@effect/platform-node';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type ImplicitDependency, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { globWithWorkspaceContext } from '@nx/devkit/internal';
import { Array, Effect, FileSystem, Layer, ManagedRuntime, Option, Path, pipe, Record, Schema, SchemaGetter, SchemaIssue, String, Trie } from 'effect';
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
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files' })),
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
    return { moduleRoot, sourceRoot: path.join(moduleRoot, ...moduleName.split('.')), pythonFiles: tool.pytest.pythonFiles, perFileTargetVersion: tool.ruff.perFileTargetVersion };
});
const _python = Effect.fnUntraced(function* (dir: string, workspace: string, root: ReturnType<typeof _root>) {
    const { moduleRoot, sourceRoot, pythonFiles } = yield* root;
    const tests = yield* Effect.tryPromise(() =>
        globWithWorkspaceContext(
            workspace,
            Array.map(pythonFiles, (pattern) => `${dir}/**/${pattern}`),
        ),
    );
    return { moduleRoot, sourceRoot, targets: Array.isReadonlyArrayNonEmpty(tests) ? { check: {}, test: {} } : {} };
});

const _PROJECTS = {
    '*.csproj': ({ dir, base }) => {
        const host = _host(dir);
        const plugin = dir.startsWith('apps/') && host.includes('host:rhino');
        const projectFile = `${dir}/${base}`;
        const outputPath = '.artifacts/rhino/{projectName}';
        return Effect.succeed({
            root: dir,
            tags: ['language:dotnet', ...host],
            targets: {
                typecheck: {},
                check: {},
                ...(plugin
                    ? {
                          'build:release': { defaultConfiguration: 'release' },
                          pack: { command: `dotnet publish "${projectFile}" --no-restore --output "${outputPath}"`, outputs: [`{workspaceRoot}/${outputPath}`] },
                          install: { command: `dotnet msbuild "${projectFile}" -target:InstallYakPackage -property:PublishDir="${outputPath}"` },
                      }
                    : {}),
            },
            ...(plugin ? { namedInputs: { icons: [`{workspaceRoot}/${dir.split('/', 2).join('/')}/icons/**/*.svg`, { runtime: 'resvg --version' }, { runtime: 'magick -version' }] } } : {}),
        });
    },
    '*.cs': Effect.fnUntraced(function* ({ dir, base }, workspace) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const file = path.join(dir, base);
        yield* Effect.filterOrFail(fs.readFileString(path.join(workspace, file)), String.startsWith('#!'));
        const families: Record<string, TargetConfiguration> = {
            build: { inputs: ['default', '{workspaceRoot}/.editorconfig'], outputs: [`{workspaceRoot}/.artifacts/dotnet/{bin,obj}/${base}`], options: { args: [file] } },
            format: { command: `dotnet format ${file}` },
            lint: { command: `dotnet format style ${file} --verify-no-changes`, cache: true, inputs: ['default', '{workspaceRoot}/.editorconfig', 'dotnet'] },
        };
        return {
            root: dir,
            name: dir,
            tags: ['language:dotnet', ..._host(dir)],
            targets: {
                ...Record.fromEntries(
                    Array.flatMap(
                        Record.toEntries(families),
                        ([target, configuration]): ReadonlyArray<readonly [string, TargetConfiguration]> => [
                            [target, { executor: 'nx:noop', dependsOn: [{ target: `${target}:*`, params: 'forward' }] }],
                            [`${target}:${base}`, configuration],
                        ],
                    ),
                ),
                typecheck: {},
                check: {},
            },
        };
    }),
    '.swcrc': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:extendscript'], targets: { build: {} } }),
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir, base }, workspace, root) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [{ id }, python] = yield* Effect.all([Effect.flatMap(fs.readFileString(path.join(workspace, dir, base)), Schema.decodeEffect(_Manifest)), _python(dir, workspace, root)], { concurrency: 'unbounded' });
        const outputPath = '.artifacts/blender/{projectName}';
        const archive = `${outputPath}/extension.zip`;
        return {
            root: dir,
            name: id,
            tags: ['language:python', 'host:blender'],
            targets: {
                pack: {
                    command: `python -m eng.python.fitout pack "{projectRoot}" "${archive}"`,
                    inputs: [`{workspaceRoot}/${python.sourceRoot}/**/*`, `{workspaceRoot}/${path.dirname(dir)}/icons/*.svg`],
                    outputs: [`{workspaceRoot}/${outputPath}`],
                },
                install: { command: `python -m eng.python.fitout install "${archive}"` },
                ...python.targets,
            },
        };
    }),
    'project.xcproj': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), name: path.parse(dir).name, tags: ['language:swift', 'host:macos'], targets: { build: {}, install: {}, upgrade: {}, lint: {}, format: {}, check: {} } })),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace, root) {
        const [path, python] = yield* Effect.all([Path.Path, _python(dir, workspace, root)]);
        const parts = yield* Effect.fromOption(Option.liftPredicate(path.relative(python.moduleRoot, dir).split(path.sep), (relative) => !Array.contains(relative, '..')));
        return { root: dir, name: parts.join('.'), tags: ['language:python'], targets: python.targets };
    }),
    'tsconfig.json': ({ dir }) => Effect.succeed({ root: dir, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'vite.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['bundler:vite'], targets: { build: {} } }),
    'uxp.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:uxp'], targets: { build: {} } }),
} as const satisfies Record<string, (file: Path.Path.Parsed, workspace: string, root: ReturnType<typeof _root>) => Effect.Effect<ProjectConfiguration, unknown, FileSystem.FileSystem | Path.Path>>;

const _pythonVersions = Effect.fnUntraced(function* (declarations: readonly string[], workspace: string, configuration: ReturnType<typeof _root>) {
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
    const { perFileTargetVersion } = yield* Effect.filterOrFail(
        configuration,
        (input) => Array.every(Array.flatMap(Option.toArray(input.perFileTargetVersion), Record.keys), (file) => !Array.some(directories, ({ root }) => file.startsWith(root))),
        (input) => new SchemaIssue.InvalidValue({ expected: 'file runtime paths outside nested Python projects' }, input.perFileTargetVersion),
    );
    const versions = [...directories, ...Array.map(Array.flatMap(Option.toArray(perFileTargetVersion), Record.toEntries), ([file, version]) => ({ version, root: file }))];
    const targets: Record<string, TargetConfiguration> = {
        ...pipe(
            Array.groupBy(versions, ({ version }) => version),
            Record.toEntries,
            Array.flatMap(([version, members]) => {
                const roots = Array.map(members, ({ root }) => root).join(' ');
                return [
                    [`typecheck:ty-${version}`, { command: `ty check --python-version ${version} ${roots}` }],
                    [`typecheck:mypy-${version}`, { command: `mypy --python-version ${version} ${roots}` }],
                ] as const;
            }),
            Record.fromEntries,
        ),
        'typecheck:ty': { options: { args: Array.map(versions, ({ root }) => `--exclude ${root}`).join(' ') } },
        'typecheck:mypy': { options: { args: Array.map(versions, ({ root }) => `--exclude '^${RegExp.escape(root)}${root.endsWith('/') ? '' : '$'}'`).join(' ') } },
    };
    return { projects: { '.': { root: '.', targets } } };
});
const _dependencies = Effect.fnUntraced(function* ({ projects, fileMap, workspaceRoot }: CreateDependenciesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const python = Record.filter(projects, ({ tags }) => Option.exists(Option.fromNullishOr(tags), Array.contains('language:python')));
    const { sourceRoot } = yield* _root(workspaceRoot);
    const bundled = Record.keys(Record.filter(python, ({ root }) => root === sourceRoot || root.startsWith(`${sourceRoot}/`)));
    const assembly = Array.flatMap(Record.keys(Record.filter(python, ({ tags, targets }) => Option.exists(Option.fromNullishOr(tags), Array.contains('host:blender')) && Option.exists(Option.fromNullishOr(targets), Record.has('pack')))), (source) =>
        Array.map(bundled, (target): ImplicitDependency => ({ source, target, type: DependencyType.implicit })),
    );
    const sources = Array.flatMap(Record.toEntries(Record.filter(fileMap.projectFileMap, (_, source) => Record.has(python, source))), ([source, data]) =>
        Array.map(
            Array.filter(data, ({ file }) => path.extname(file) === '.py'),
            ({ file }) => ({ source, file }),
        ),
    );
    if (Array.isReadonlyArrayEmpty(sources)) {
        return assembly;
    }
    const modules = Trie.fromIterable(Array.map(Record.keys(python), (name) => [`${name}.`, name] as const));
    const [, rule]: [Effect.Success<typeof _registerPython>, NapiConfig] = yield* Effect.all([_registerPython, Effect.map(fs.readFileString(path.join(workspaceRoot, 'tools/ast-grep/utils/python/python-static-imported-module.yml')), parseYaml)], { concurrency: 'unbounded' });
    const dependencies = yield* Effect.validate(
        sources,
        Effect.fnUntraced(function* ({ source, file }) {
            const text = yield* fs.readFileString(path.join(workspaceRoot, file));
            return yield* Effect.tryPromise(async () => {
                const { parseAsync } = await import('@ast-grep/napi');
                const parsed = await parseAsync('python', text);
                const imports = parsed.root().findAll(rule);
                return pipe(
                    imports,
                    Array.flatMap((module) => Array.cartesian([module], Option.toArray(Option.fromNullishOr(module.getMatch('FROM'))))),
                    Array.flatMap(([module, statement]) => Array.cartesian([module], statement.fieldChildren('name'))),
                    Array.flatMap(([module, member]) => Array.cartesian([module], member.findAll({ rule: { kind: 'dotted_name' } }))),
                    Array.prependAll(Array.map(imports, (module) => [module])),
                    Array.map(Array.flatMap((name) => name.namedChildren())),
                    Array.map(Array.map((part) => part.text())),
                    Array.map((parts) => Trie.longestPrefixOf(modules, `${parts.join('.')}.`)),
                    Array.flatMap(Option.toArray),
                    Array.map(([, target]): ImplicitDependency => ({ source, target, type: DependencyType.implicit })),
                );
            });
        }),
        { concurrency: 'unbounded' },
    );
    return [...assembly, ...Array.flatten(dependencies)];
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
const createNodes: CreateNodes = [
    `{${_DECLARATION},*/**/{${[...Record.keys(_PROJECTS), _DECLARATION].join(',')}}}`,
    async (files, options, context): Promise<CreateNodesResultArray> => {
        await using runtime = ManagedRuntime.make(_services);
        const [root, path] = runtime.runSync(Effect.all([Effect.cached(_root(context.workspaceRoot)), Path.Path]));
        const declarations = Array.filter(files, (file) => file !== _DECLARATION && path.basename(file) === _DECLARATION);
        const project = Effect.fnUntraced(
            function* (file: string) {
                if (file === _DECLARATION) {
                    return yield* _pythonVersions(declarations, context.workspaceRoot, root);
                }
                const parsed = path.parse(file);
                const [, configure] = yield* Effect.fromOption(Array.findFirst(Record.toEntries(_PROJECTS), ([key]) => key === parsed.base || key === `*${parsed.ext}`));
                const configuration = yield* configure(parsed, context.workspaceRoot, root);
                return { projects: { [configuration.root]: configuration } };
            },
            Effect.catchNoSuchElement,
            Effect.map(Option.getOrElse(() => ({}))),
        );
        return await createNodesFromFiles((file) => runtime.runPromise(project(file)), files, options, context);
    },
];
const createDependencies: CreateDependencies = (_options, context) => Effect.runPromise(Effect.provide(_dependencies(context), _services, { local: true }));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
