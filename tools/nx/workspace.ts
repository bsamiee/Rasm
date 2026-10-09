import grammar from '@ast-grep/lang-python';
import { type NapiConfig, parse as parseSource, registerDynamicLanguage } from '@ast-grep/napi';
import { NodeServices } from '@effect/platform-node';
import { type CreateDependencies, type CreateDependenciesContext, type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, DependencyType, type ProjectConfiguration, type TargetConfiguration } from '@nx/devkit';
import { Array, Effect, FileSystem, ManagedRuntime, Option, Path, pipe, Record, Schema, SchemaGetter, SchemaIssue, String, Trie } from 'effect';
import { parse } from 'smol-toml';
import { parse as parseYaml } from 'yaml';

// --- [CONSTANTS] -----------------------------------------------------------------------

const _DECLARATION = 'pyproject.toml';
const _FLOOR = /^>=\s*(?<floor>\d+\.\d+)$/u;
const _REGEX_SYNTAX = /[.*+?^${}()|[\]\\]/gu;

// --- [MODELS] --------------------------------------------------------------------------

const _Toml = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: () => new SchemaIssue.InvalidValue({ expected: 'TOML' }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _Floor = Schema.String.pipe(
    Schema.decodeTo(Schema.String, {
        decode: SchemaGetter.transformEffect((specifier: string, options) =>
            Option.match(Option.fromNullishOr(_FLOOR.exec(specifier)?.groups?.floor), {
                onNone: () => Effect.fail(new SchemaIssue.InvalidValue({ expected: 'a >=<major>.<minor> lower bound' }, specifier, options)),
                onSome: Effect.succeed,
            }),
        ),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _Declaration = _Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ floor: _Floor }).pipe(Schema.encodeKeys({ floor: 'requires-python' })) })));
const _Root = _Toml.pipe(
    Schema.decodeTo(
        Schema.Struct({
            tool: Schema.Struct({
                uv: Schema.Struct({ buildBackend: Schema.Struct({ moduleRoot: Schema.String }).pipe(Schema.encodeKeys({ moduleRoot: 'module-root' })) }).pipe(Schema.encodeKeys({ buildBackend: 'build-backend' })),
                pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files' })),
            }),
        }),
    ),
);
const _Manifest = _Toml.pipe(Schema.decodeTo(Schema.Struct({ id: Schema.String })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const _host = (dir: string): readonly string[] => Array.map(Array.intersection(dir.split('/'), ['rhino', 'blender']), (host) => `host:${host}`);
const _python = Effect.fnUntraced(function* (dir: string, workspace: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const { tool } = yield* Effect.flatMap(fs.readFileString(path.join(workspace, 'pyproject.toml')), Schema.decodeEffect(_Root));
    const tests = yield* Effect.validate(tool.pytest.pythonFiles, (pattern) => fs.glob(`**/${pattern}`, { root: path.join(workspace, dir) }), { concurrency: 'unbounded' });
    return { moduleRoot: tool.uv.buildBackend.moduleRoot, targets: Array.some(tests, Array.isReadonlyArrayNonEmpty) ? { check: {}, test: {} } : {} };
});

const _PROJECTS: Record<string, (file: Path.Path.Parsed, workspace: string) => Effect.Effect<ProjectConfiguration, unknown, FileSystem.FileSystem | Path.Path>> = {
    '*.csproj': ({ dir }) => {
        const host = _host(dir);
        const plugin = dir.startsWith('apps/') && host.includes('host:rhino');
        return Effect.succeed({
            root: dir,
            tags: ['language:dotnet', ...host],
            targets: { typecheck: {}, check: {}, ...(plugin ? { 'build:release': { defaultConfiguration: 'release' }, pack: {}, install: {} } : {}) },
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
    'blender_manifest.toml': Effect.fnUntraced(function* ({ dir, base }, workspace) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [{ id }, python] = yield* Effect.all([Effect.flatMap(fs.readFileString(path.join(workspace, dir, base)), Schema.decodeEffect(_Manifest)), _python(dir, workspace)], { concurrency: 'unbounded' });
        return { root: dir, name: id, tags: ['language:python', 'host:blender'], targets: { pack: {}, install: {}, ...python.targets } };
    }),
    'project.xcproj': ({ dir }) => Effect.map(Path.Path, (path) => ({ root: path.dirname(dir), name: path.parse(dir).name, tags: ['language:swift', 'host:macos'], targets: { build: {}, install: {}, upgrade: {}, lint: {}, format: {}, check: {} } })),
    'py.typed': Effect.fnUntraced(function* ({ dir }, workspace) {
        const [path, python] = yield* Effect.all([Path.Path, _python(dir, workspace)]);
        const parts = yield* Effect.fromOption(Option.liftPredicate(path.relative(python.moduleRoot, dir).split(path.sep), (relative) => !Array.contains(relative, '..')));
        return { root: dir, name: parts.join('.'), tags: ['language:python'], targets: python.targets };
    }),
    'tsconfig.json': ({ dir }) => Effect.succeed({ root: dir, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'vite.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['bundler:vite'], targets: { build: {} } }),
    'uxp.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:uxp'], targets: { build: {} } }),
};

const _project = Effect.fnUntraced(function* (file: string, workspace: string) {
    const project = (yield* Path.Path).parse(file);
    const configuration = yield* pipe(
        Array.findFirst([project.base, `*${project.ext}`], (key) => Record.get(_PROJECTS, key)),
        Effect.fromOption,
        Effect.flatMap((configure) => configure(project, workspace)),
        Effect.catchNoSuchElement,
    );
    return Option.match(configuration, { onNone: () => ({}), onSome: (found) => ({ projects: { [found.root]: found } }) });
});
const _floors = Effect.fnUntraced(function* (files: readonly string[], workspace: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const declarations = Array.filter(files, (file) => path.basename(file) === _DECLARATION);
    const floors = yield* Effect.forEach(
        declarations,
        (file) =>
            pipe(
                fs.readFileString(path.join(workspace, file)),
                Effect.flatMap(Schema.decodeEffect(_Declaration)),
                Effect.map(({ project }) => ({ floor: project.floor, root: path.dirname(file) })),
            ),
        { concurrency: 'unbounded' },
    );
    const targets: Record<string, TargetConfiguration> = {
        ...pipe(
            Array.groupBy(floors, ({ floor }) => floor),
            Record.toEntries,
            Array.flatMap(([floor, members]) => {
                const roots = Array.map(members, ({ root }) => root).join(' ');
                return [
                    [`typecheck:ty-${floor}`, { command: `ty check --python-version ${floor} ${roots}` }],
                    [`typecheck:mypy-${floor}`, { command: `mypy --python-version ${floor} ${roots}` }],
                ] as const;
            }),
            Record.fromEntries,
        ),
        'typecheck:ty': { options: { args: Array.map(floors, ({ root }) => `--exclude ${root}/`).join(' ') } },
        'typecheck:mypy': { options: { args: Array.map(floors, ({ root }) => `--exclude '^${root.replaceAll(_REGEX_SYNTAX, '\\$&')}/'`).join(' ') } },
    };
    return Array.match(declarations, { onEmpty: (): CreateNodesResultArray => [], onNonEmpty: ([file]): CreateNodesResultArray => [[file, { projects: { '.': { root: '.', targets } } }]] });
});
const _dependencies = Effect.fnUntraced(function* ({ projects, fileMap, workspaceRoot }: CreateDependenciesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const rule: NapiConfig = parseYaml(yield* fs.readFileString(path.join(workspaceRoot, 'tools/ast-grep/utils/python/python-static-imported-module.yml')));
    const python = Record.keys(Record.filter(projects, ({ tags }) => Option.exists(Option.fromNullishOr(tags), Array.contains('language:python'))));
    const modules = Trie.fromIterable(Array.map(python, (name) => [`${name}.`, name] as const));
    const files = Array.flatMap(Record.toEntries(Record.filter(fileMap.projectFileMap, (_, source) => Array.contains(python, source))), ([source, data]) => Array.map(data, ({ file }) => ({ source, file })));
    const sources = Array.filter(files, ({ file }) => path.extname(file) === '.py');
    const texts = yield* Effect.validate(sources, ({ file }) => fs.readFileString(path.join(workspaceRoot, file)), { concurrency: 'unbounded' });
    return Array.flatMap(Array.zip(sources, texts), ([{ source, file }, text]) =>
        pipe(
            parseSource('python', text).root().findAll(rule),
            Array.flatMap((module) => [
                [module],
                ...Option.toArray(Option.fromNullishOr(module.getMatch('FROM')))
                    .flatMap((statement) => statement.fieldChildren('name'))
                    .flatMap((member) => member.findAll({ rule: { kind: 'dotted_name' } }))
                    .map((member) => [module, member]),
            ]),
            Array.map(Array.flatMap((name) => name.namedChildren())),
            Array.map((parts) => Trie.longestPrefixOf(modules, `${parts.map((part) => part.text()).join('.')}.`)),
            Array.getSomes,
            Array.map(([, target]) => target),
            Array.filter((target) => target !== source),
            Array.dedupe,
            Array.map((target) => ({ source, target, sourceFile: file, type: DependencyType.static })),
        ),
    );
});

// --- [COMPOSITION] ---------------------------------------------------------------------

registerDynamicLanguage({ python: grammar });
const _runtime = ManagedRuntime.make(NodeServices.layer);
const createNodes: CreateNodes = [
    `*/**/{${[...Record.keys(_PROJECTS), _DECLARATION].join(',')}}`,
    (files, options, context): Promise<CreateNodesResultArray> =>
        _runtime.runPromise(pipe(Effect.all([Effect.tryPromise(() => createNodesFromFiles((file) => _runtime.runPromise(_project(file, context.workspaceRoot)), files, options, context)), _floors(files, context.workspaceRoot)], { concurrency: 'unbounded' }), Effect.map(Array.flatten))),
];
const createDependencies: CreateDependencies = (_options, context) => _runtime.runPromise(_dependencies(context));

// --- [EXPORTS] -------------------------------------------------------------------------

export { createDependencies, createNodes };
