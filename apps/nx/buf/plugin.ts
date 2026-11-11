import { inferred, type Request, type Unit } from '@rasm/nx-devkit/plugin';
import { ToolError } from '@rasm/nx-devkit/process';
import { Array, Effect, FileSystem, Option, Path, Schema } from 'effect';
import { Yaml } from 'effect/encoding';

// --- [CONSTANTS] -----------------------------------------------------------------------

const TEMPLATE = 'buf.gen.yaml';

// --- [MODELS] --------------------------------------------------------------------------

const Template = Schema.Struct({
    inputs: Schema.Array(Schema.Struct({ directory: Schema.optionalKey(Schema.String) })),
    plugins: Schema.Array(Schema.Struct({ out: Schema.String })),
}).annotate({ description: 'buf.gen.yaml rows the plugin reads: each local input folder and each output folder' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const decoded = Effect.fnUntraced(function* (workspaceRoot: string, file: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    return yield* fs.readFileString(path.join(workspaceRoot, file)).pipe(Effect.map(Yaml.parse), Effect.flatMap(Schema.decodeUnknownEffect(Template)), Effect.mapBoth({ onFailure: ToolError.of(file, 'does not read as a buf generation template'), onSuccess: (report) => ({ report }) }), Effect.result);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const { createNodes } = inferred({
    module: import.meta.url,
    files: [TEMPLATE],
    options: Schema.Struct({}),
    units: ({ files }: Request<unknown>) => Effect.succeed(files.map((file) => ({ key: file, file }))),
    read: (missing: Array.NonEmptyReadonlyArray<Unit>, { context }: Request<unknown>) => Effect.forEach(missing, ({ file }) => decoded(context.workspaceRoot, file), { concurrency: 'unbounded' }),
    projects: ({ inputs, plugins }: typeof Template.Type, { file }, { path }) => {
        const root = path.dirname(file);
        return [
            {
                file,
                root,
                technologies: ['buf'],
                clean: 'buf',
                members: {
                    build: {
                        buf: {
                            command: 'buf generate',
                            cache: true,
                            inputs: [`{projectRoot}/${TEMPLATE}`, ...Array.getSomes(inputs.map(({ directory }) => Option.fromUndefinedOr(directory))).map((directory) => path.join('{workspaceRoot}', root, directory, '**/*.proto')), { runtime: 'buf --version' }, { externalDependencies: [] }],
                            outputs: plugins.map(({ out }) => path.join('{workspaceRoot}', root, out)),
                            metadata: { description: `Generates code from the Protobuf inputs of ${TEMPLATE} with buf generate`, help: { command: 'buf generate --help', example: { args: ['--debug'] } } },
                        },
                    },
                },
            },
        ];
    },
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { createNodes };
