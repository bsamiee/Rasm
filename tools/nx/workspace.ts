import { NodeServices } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, type ProjectConfiguration } from '@nx/devkit';
import { Array, Effect, FileSystem, ManagedRuntime, Path, type PlatformError, Record, Schema, SchemaGetter, SchemaIssue } from 'effect';
import { parse } from 'smol-toml';

// --- [MODELS] --------------------------------------------------------------------------

const _Toml = Schema.String.pipe(
    Schema.decodeTo(Schema.Unknown, {
        decode: SchemaGetter.transformEffect((text: string, options) => Effect.try({ try: () => parse(text), catch: () => new SchemaIssue.InvalidValue({ expected: 'TOML' }, text, options) })),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);
const _Project = _Toml.pipe(Schema.decodeTo(Schema.Struct({ project: Schema.Struct({ name: Schema.String }) })));
const _Pytest = _Toml.pipe(
    Schema.decodeTo(Schema.Struct({ tool: Schema.Struct({ pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files' })) }) })),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

const _PROJECTS: Record<
    string,
    (file: Path.Path.Parsed, workspace: string) => Effect.Effect<ProjectConfiguration, Schema.SchemaError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path>
> = {
    '*.csproj': ({ dir }) =>
        Effect.map(Path.Path, (path) => {
            const plugin = dir.startsWith('apps/') && path.basename(dir) === 'rhino';
            return {
                root: dir,
                tags: ['language:dotnet', ...(plugin ? ['host:rhino'] : [])],
                targets: { typecheck: {}, check: {}, ...(plugin ? { pack: {}, install: {} } : {}) },
            };
        }),
    '.swcrc': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:extendscript'], targets: { build: {} } }),
    'project.pbxproj': ({ dir }) =>
        Effect.map(Path.Path, (path) => {
            const xcodeproj = path.parse(dir);
            return { root: xcodeproj.dir, name: xcodeproj.name, tags: ['language:swift', 'host:macos'], targets: { build: {}, install: {}, upgrade: {}, lint: {}, format: {}, check: {} } };
        }),
    'pyproject.toml': Effect.fnUntraced(function* ({ dir, base }, workspace) {
        const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
        const [{ project }, { tool }] = yield* Effect.all(
            [
                Effect.flatMap(fs.readFileString(path.join(workspace, dir, base)), Schema.decodeEffect(_Project)),
                Effect.flatMap(fs.readFileString(path.join(workspace, base)), Schema.decodeEffect(_Pytest)),
            ],
            { concurrency: 'unbounded' },
        );
        const tests = yield* fs.glob(`**/{${tool.pytest.pythonFiles.join(',')}}`, { root: path.join(workspace, dir) });
        return { root: dir, name: project.name, tags: ['language:python'], targets: Array.isReadonlyArrayNonEmpty(tests) ? { check: {}, test: {} } : {} };
    }),
    'tsconfig.json': ({ dir }) => Effect.succeed({ root: dir, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'uxp.config.ts': ({ dir }) => Effect.succeed({ root: dir, tags: ['host:uxp'], targets: { build: {} } }),
};

const _project = Effect.fnUntraced(function* (file: string, workspace: string) {
    const project = (yield* Path.Path).parse(file);
    const configure = yield* Effect.fromOption(Array.findFirst([project.base, `*${project.ext}`], (key) => Record.get(_PROJECTS, key)));
    const configuration = yield* configure(project, workspace);
    return { projects: { [configuration.root]: configuration } };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const _runtime = ManagedRuntime.make(NodeServices.layer);
const createNodes: CreateNodes = [
    `*/**/{${Record.keys(_PROJECTS).join(',')}}`,
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles((file) => _runtime.runPromise(_project(file, context.workspaceRoot)), files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
