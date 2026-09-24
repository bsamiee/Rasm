// --- [IMPORTS] -------------------------------------------------------------------------

import { Lang, parse, type SgNode } from '@ast-grep/napi';
import { NodeFileSystem, NodePath } from '@effect/platform-node';
import { type CreateNodes, type CreateNodesResultArray, createNodesFromFiles, type ProjectConfiguration } from '@nx/devkit';
import { Array, Data, Effect, FileSystem, Layer, ManagedRuntime, Option, Path, type PlatformError, Record, Schema } from 'effect';
import { parse as toml } from 'smol-toml';

// --- [TYPES] ---------------------------------------------------------------------------

interface ProjectFile {
    readonly file: string;
    readonly directory: string;
    readonly text: string;
    readonly workspace: string;
}

type Configure = (file: ProjectFile) => Effect.Effect<ProjectConfiguration, ProjectFileError | Schema.SchemaError | PlatformError.PlatformError, FileSystem.FileSystem | Path.Path>;

// --- [MODELS] --------------------------------------------------------------------------

const _Project = Schema.Struct({ project: Schema.Struct({ name: Schema.String }) });
const _Pytest = Schema.Struct({ tool: Schema.Struct({ pytest: Schema.Struct({ pythonFiles: Schema.Array(Schema.String) }).pipe(Schema.encodeKeys({ pythonFiles: 'python_files' })) }) });

// --- [ERRORS] --------------------------------------------------------------------------

class ProjectFileError extends Data.TaggedError('ProjectFileError')<{ readonly file: string; readonly cause: unknown }> {
    override get message(): string {
        return `${this.file} does not define a project`;
    }
}

// --- [SYNTAX] --------------------------------------------------------------------------

const _matches = (source: SgNode, pattern: string, variable: string): readonly SgNode[] => Array.flatMapNullishOr(source.findAll({ rule: { pattern } }), (node) => node.getMatch(variable));
const _tsx = (text: string): SgNode => parse(Lang.Tsx, text).root();
const _toml = (file: string, text: string): Effect.Effect<unknown, ProjectFileError> => Effect.try({ try: () => toml(text), catch: (cause) => new ProjectFileError({ file, cause }) });
const _literals = (source: SgNode, pattern: string, variable: string): readonly string[] =>
    Array.flatMapNullishOr(_matches(source, pattern, variable), (literal) => literal.namedChildren()[0]?.text());

// --- [PROJECTS] ------------------------------------------------------------------------

const _PROJECTS: Record<string, Configure> = {
    '*.csproj': ({ directory }) => Effect.succeed({ root: directory, tags: ['language:dotnet'], targets: { typecheck: {}, check: {} } }),
    '.swcrc': ({ directory }) => Effect.succeed({ root: directory, tags: ['host:extendscript'], targets: { build: {} } }),
    'cli.ts': ({ directory, text }) =>
        Effect.succeed({
            root: directory,
            targets: Record.fromIterableWith(_literals(_tsx(text), 'Command.make($NAME, $CONFIG, $HANDLER)', 'NAME'), (name) => [
                name,
                { command: `node cli.ts ${name}`, options: { cwd: '{projectRoot}' } },
            ]),
        }),
    'project.pbxproj': ({ directory }) =>
        Effect.map(Path.Path, (path) => ({
            root: path.dirname(directory),
            name: path.basename(directory, '.xcodeproj'),
            tags: ['language:swift', 'host:macos'],
            targets: { build: {}, install: {}, lint: {}, format: {}, check: {} },
        })),
    'pyproject.toml': ({ file, directory, text, workspace }) =>
        Effect.gen(function* () {
            const fs = yield* FileSystem.FileSystem;
            const path = yield* Path.Path;
            const { project } = yield* Effect.flatMap(_toml(file, text), Schema.decodeUnknownEffect(_Project));
            const { tool } = yield* Effect.flatMap(_toml('pyproject.toml', yield* fs.readFileString(path.join(workspace, 'pyproject.toml'))), Schema.decodeUnknownEffect(_Pytest));
            const tests = yield* Effect.forEach(tool.pytest.pythonFiles, (pattern) => fs.glob(`**/${pattern}`, { root: path.join(workspace, directory) }), { concurrency: 'unbounded' });
            return {
                root: directory,
                name: project.name,
                tags: ['language:python'],
                targets: { typecheck: {}, check: {}, ...(Array.isReadonlyArrayNonEmpty(Array.flatten(tests)) ? { test: {} } : {}) },
            };
        }),
    'tsconfig.json': ({ directory }) => Effect.succeed({ root: directory, tags: ['language:typescript'], targets: { typecheck: {}, check: {} } }),
    'uxp.config.ts': ({ directory }) => Effect.succeed({ root: directory, tags: ['host:uxp'], targets: { build: {} } }),
};

const _project = Effect.fnUntraced(function* (file: string, workspace: string) {
    const fs = yield* FileSystem.FileSystem;
    const path = yield* Path.Path;
    const configure = yield* Effect.fromOption(
        Option.orElse(Record.get(_PROJECTS, path.basename(file)), () => Record.get(_PROJECTS, `*${path.extname(file)}`)),
        () => new ProjectFileError({ file, cause: 'No project kind reads this file' }),
    );
    const configuration = yield* configure({ file, workspace, directory: path.dirname(file), text: yield* fs.readFileString(path.join(workspace, file)) });
    return { projects: { [configuration.root]: configuration } };
});

// --- [REGISTRATION] --------------------------------------------------------------------

const _runtime = ManagedRuntime.make(Layer.mergeAll(NodeFileSystem.layer, NodePath.layer));
const createNodes: CreateNodes = [
    `{apps,eng,libs,tests,tools,.claude/plugins}/**/{${Record.keys(_PROJECTS).join(',')}}`,
    (files, options, context): Promise<CreateNodesResultArray> => createNodesFromFiles((file) => _runtime.runPromise(_project(file, context.workspaceRoot)), files, options, context),
];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
