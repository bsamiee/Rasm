import { Array, Cause, Context, Effect, Exit, Function, flow, Option, pipe, Record, Schema, SchemaGetter, Sink, Stream, Struct, Tuple } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import type { Dependency } from './resources.ts';
import { ContentBlocks, orInternalError, Respond } from './server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Route<Chosen, Fields extends Schema.Struct.Fields, R> {
    readonly fields: Fields;
    readonly resolve: (given: Schema.Struct.Type<Fields>) => Effect.Effect<Chosen, Error, R>;
    readonly depends: (chosen: Chosen, given: Schema.Struct.Type<Fields>) => Effect.Effect<readonly Dependency[], Error, R>;
    readonly annotations: Context.Context<never>;
    readonly completion: { readonly [Key in keyof Fields]?: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READ_ONLY = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.Destructive, false));

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) {
    const context = yield* Effect.context<R>();
    const called = (given: P['Type']) => orInternalError(Effect.provide(handle(given), context));
    const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
        Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform(called), encode: SchemaGetter.forbidden(() => 'calls decode only') })),
        Schema.toCodecJson(tool.successSchema),
    ];
    const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
    yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
});
const registerRoutedTool =
    <Chosen, Fields extends Schema.Struct.Fields, RR>(route: Route<Chosen, Fields, RR>) =>
    <P extends Schema.Top & { readonly fields: Schema.Struct.Fields }, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type'], chosen: Chosen) => Effect.Effect<S['Type'], Error, R>,
    ) => {
        const parts = Schema.toCodecJson(Schema.Tuple([tool.parametersSchema, Schema.Struct(route.fields)]));
        const annotations = Context.merge(route.annotations, tool.annotations);
        const fields = { ...tool.parametersSchema.fields, ...route.fields };
        const stated = (entries: readonly (readonly [string, Schema.Json])[]) => Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json)))(Record.fromEntries(entries)));
        const refused = flow(
            stated,
            Effect.map((shown) => new Cause.IllegalArgumentError(shown)),
        );
        const reported =
            (dependencies: readonly Dependency[]) =>
            <A, E, R2>(self: Effect.Effect<A, E, R2>) =>
                Effect.onExitIf(
                    self,
                    (exit) => !(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause)),
                    () =>
                        Effect.gen(function* () {
                            const [[holding, failed], add] = yield* Effect.all([Effect.partition(dependencies, (dependency) => orInternalError(dependency.report), { concurrency: 'unbounded' }), ContentBlocks]);
                            const shown = yield* pipe(holding, Array.getSomes, Option.liftPredicate(Array.isReadonlyArrayNonEmpty), Option.map(stated), Effect.transposeOption);
                            yield* Effect.forEach([...Option.toArray(shown), ...Array.map(failed, Struct.get('message'))], (text) => add(McpSchema.TextContent.make({ text })), { discard: true });
                        }),
                );
        const guarded =
            (dependencies: readonly Dependency[]) =>
            <A, E, R2>(run: Effect.Effect<A, E, R2>) =>
                Effect.scoped(
                    Effect.gen(function* () {
                        const peeled = yield* Effect.forEach(dependencies, (dependency) => Stream.peel(dependency.refusal, Sink.head()));
                        yield* Array.match(Array.getSomes(Array.map(peeled, ([first]) => Option.flatten(first))), { onEmpty: () => Effect.void, onNonEmpty: flow(refused, Effect.flatMap(Effect.fail)) });
                        yield* Stream.mergeAll(Array.map(peeled, Tuple.get(1)), { concurrency: 'unbounded' }).pipe(
                            Stream.filter(Option.isSome),
                            Stream.runHead,
                            Effect.flatMap(Option.match({ onNone: () => Effect.void, onSome: ({ value }) => Effect.flatMap(Effect.all([refused([value]), Respond]), ([refusal, respond]) => respond(refusal)) })),
                            Effect.catchCause(Effect.logError),
                            Effect.forkScoped,
                        );
                        return yield* reported(dependencies)(run);
                    }),
                );
        return registerTool(
            tool.setParameters(Record.isEmptyRecord(fields) ? Tool.EmptyParams : Schema.toEncoded(Schema.Struct(fields))).annotateMerge(annotations),
            Effect.fnUntraced(function* (given) {
                const [parameters, routed] = yield* Schema.decodeEffect(parts)([given, given], { errors: 'all' });
                const chosen = yield* route.resolve(routed);
                const dependencies = yield* route.depends(chosen, routed);
                const run = handle(parameters, chosen);
                return yield* Context.get(annotations, Tool.Readonly) ? reported(dependencies)(run) : guarded(dependencies)(run);
            }),
        );
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { READ_ONLY, type Route, registerRoutedTool, registerTool };
