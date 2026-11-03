import { Context, Effect, Function, Schema, SchemaGetter } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/ai';

// --- [SERVICES] ------------------------------------------------------------------------

const Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/tool/Visible', { defaultValue: () => Effect.succeed(true) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerTool = Object.assign(
    Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) {
        const context = yield* Effect.context<R>();
        const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
            Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform((given: P['Type']) => Effect.provideContext(handle(given), context)), encode: SchemaGetter.forbiddenEncoding })),
            Schema.toCodecJson(tool.successSchema),
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
        const visible = Context.get(tool.annotations, Visible);
        yield* McpServer.registerToolkit(toolkit).pipe(
            Effect.provide([
                toolkit.toLayer({ [tool.name]: Function.identity }),
                McpServer.visibility(() =>
                    visible.pipe(
                        Effect.tapCause((cause) => Effect.logError(`${tool.name} hidden after its visibility check failed`, cause)),
                        Effect.orElseSucceed(() => false),
                    ),
                ).layer,
            ]),
        );
    }),
    { Visible },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerTool };
