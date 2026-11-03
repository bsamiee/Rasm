import { Context, Effect, Function, Option, Schema, SchemaGetter } from 'effect';
import { McpServer, Tool, Toolkit } from 'effect/ai';
import { Server } from '#/server';

// --- [SERVICES] ------------------------------------------------------------------------

class Visible extends Context.Service<Visible, Effect.Effect<boolean, Error>>()('@rasm/mcp/tool/Visible') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerTool = Object.assign(
    Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) {
        const [server, context] = yield* Effect.all([Server, Effect.context<R>()]);
        const called = (given: P['Type']) => Effect.provideContext(handle(given), context);
        const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
            Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform(called), encode: SchemaGetter.forbiddenEncoding })),
            Schema.toCodecJson(tool.successSchema),
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
        const registration = McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
        yield* server.register(
            Option.match(Context.getOption(tool.annotations, Visible), {
                onNone: () => registration,
                onSome: (visible) =>
                    Effect.provide(
                        registration,
                        McpServer.visibility(() =>
                            visible.pipe(
                                Effect.tapCause((cause) => Effect.logError(`${tool.name} hidden after its visibility check failed`, cause)),
                                Effect.orElseSucceed(() => false),
                            ),
                        ).layer,
                    ),
            }),
        );
    }),
    { Visible },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerTool };
