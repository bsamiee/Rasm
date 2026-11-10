import type { CreateNodes, CreateNodesContext, CreateNodesResultArray } from '@nx/devkit';
import { aggregated, identity, inferred, reported } from '@rasm/nx-devkit/inference';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEMPLATE = 'buf.gen.yaml';
const TECHNOLOGIES = ['buf'];

// --- [MODELS] --------------------------------------------------------------------------

const Template = Schema.Struct({
    inputs: Schema.Array(Schema.Struct({ directory: Schema.optionalKey(Schema.String) })),
    plugins: Schema.Array(Schema.Struct({ out: Schema.String })),
}).annotate({ description: 'buf.gen.yaml rows the plugin reads: each local input folder and each output folder' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const nodes = Effect.fnUntraced(function* (files: readonly string[], { workspaceRoot }: CreateNodesContext) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const plugin = yield* identity(import.meta.url, workspaceRoot);
    const [read, failures] = yield* Effect.partition(
        files,
        Effect.fnUntraced(
            function* (file) {
                const root = path.dirname(file);
                const text = yield* fs.readFileString(path.join(workspaceRoot, file));
                const { inputs, plugins } = yield* Schema.decodeUnknownEffect(Template)(yield* Effect.try(() => Yaml.parse(text)));
                const sources = Array.getSomes(inputs.map(({ directory }) => Option.fromUndefinedOr(directory))).map((directory) => path.join('{workspaceRoot}', root, directory, '**/*.proto'));
                return [
                    file,
                    {
                        projects: {
                            [root]: {
                                targets: aggregated(
                                    {
                                        build: {
                                            buf: {
                                                executor: 'nx:run-commands',
                                                options: { command: 'buf generate', cwd: root },
                                                cache: true,
                                                inputs: [`{projectRoot}/${TEMPLATE}`, ...sources, { runtime: 'buf --version' }, { externalDependencies: [] }],
                                                outputs: plugins.map(({ out }) => path.join('{workspaceRoot}', root, out)),
                                                metadata: { technologies: TECHNOLOGIES, description: `Generates code from the Protobuf inputs of ${TEMPLATE} with buf generate`, help: { command: 'buf generate --help', example: { args: ['--debug'] } } },
                                            },
                                        },
                                    },
                                    TECHNOLOGIES,
                                    { executor: plugin.cleaner, tool: 'buf' },
                                ),
                            },
                        },
                    },
                ] satisfies CreateNodesResultArray[number];
            },
            (effect, file) => Effect.mapError(effect, (cause) => [file, ToolError.of(file, 'does not read as a buf generation template')(cause)] as const),
        ),
        { concurrency: 'unbounded' },
    );
    return yield* reported(read, failures);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const createNodes: CreateNodes = [`**/${TEMPLATE}`, (files, _options, context) => inferred(nodes(files, context))];

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
