import { Effect, Function, Schema, SchemaGetter } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/ai';

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(
    tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>,
    handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>,
) {
    const context = yield* Effect.context<R>();
    const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
        Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform((given: P['Type']) => Effect.provideContext(handle(given), context)), encode: SchemaGetter.forbiddenEncoding })),
        Schema.toCodecJson(tool.successSchema),
    ];
    const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
    yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerTool };
