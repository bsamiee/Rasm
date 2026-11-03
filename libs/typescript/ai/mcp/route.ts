import { Array, Cause, Context, Effect, Exit, flow, Option, pipe, Record, Schema, Sink, Stream, Struct, Tuple } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { Resource } from '#/resource';
import { Server } from '#/server';
import { registerTool } from '#/tool';

// --- [TYPES] ---------------------------------------------------------------------------

interface Route<Chosen, Fields extends Schema.Struct.Fields, R> {
    readonly fields: Fields;
    readonly resolve: (given: Schema.Struct.Type<Fields>) => Effect.Effect<Chosen, Error, R>;
    readonly depends: (chosen: Chosen, given: Schema.Struct.Type<Fields>) => Effect.Effect<readonly ReturnType<ReturnType<Resource<unknown>['depend']>>[], Error, R>;
    readonly annotations: Context.Context<never>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeEntries = flow(Record.fromEntries<readonly [string, Schema.Json]>, Schema.encodeEffect(Schema.fromJsonString(Schema.JsonObject)), Effect.orDie);
const refusalError = flow(
    encodeEntries,
    Effect.map((reportText) => new Cause.IllegalArgumentError(reportText)),
);
const complete = <Fields extends Schema.Struct.Fields, Given = Schema.Struct.Type<Fields>>(fields: Fields) => {
    const decoders = Record.map(fields, (_, key) => Schema.decodeUnknownEffect(Schema.toCodecStringTree(Schema.Struct(Struct.pick(fields, [key])))));
    return <Values, R>(completing: (input: string, given: Partial<Given>) => Effect.Effect<Values, Error, R>) =>
        (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
            Effect.all(Record.map(decoders, (decode) => Effect.option(decode(context?.arguments ?? {})))).pipe(
                Effect.map(flow(Record.values, Array.getSomes, (picked): Partial<Given> => Object.assign({}, ...picked))),
                Effect.flatMap((given) => completing(input, given)),
            );
};
const optional = <Chosen, Fields extends Schema.Struct.Fields, R>(route: Route<Chosen, Fields, R>): Route<Option.Option<Chosen>, Fields, R> => ({
    fields: route.fields,
    resolve: (given) => (Object.values(given).every((value) => Option.isOption(value) && Option.isNone(value)) ? Effect.succeedNone : Effect.asSome(route.resolve(given))),
    depends: (chosen, given) => Option.match(chosen, { onNone: () => Effect.succeed([]), onSome: (picked) => route.depends(picked, given) }),
    annotations: Context.omit(registerTool.Visible)(route.annotations),
});
const register =
    <Chosen, Fields extends Schema.Struct.Fields, RR>(route: Route<Chosen, Fields, RR>) =>
    <P extends Schema.Top & { readonly fields: Schema.Struct.Fields & { readonly [Key in keyof Fields]?: never } }, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type'], chosen: Chosen) => Effect.Effect<S['Type'], Error, R>,
    ) => {
        const parts = Schema.toCodecJson(Schema.Tuple([tool.parametersSchema, Schema.Struct(route.fields)]));
        const annotations = Context.merge(route.annotations, tool.annotations);
        const fields = { ...tool.parametersSchema.fields, ...route.fields };
        return registerTool(
            tool.setParameters(Record.isEmptyRecord(fields) ? Tool.EmptyParams : Schema.toEncoded(Schema.Struct(fields))).annotateMerge(annotations),
            Effect.fnUntraced(function* (given) {
                const [parameters, routed] = yield* Schema.decodeEffect(parts)([given, given], { errors: 'all' });
                const chosen = yield* route.resolve(routed);
                const dependencies = yield* route.depends(chosen, routed);
                const peeled = yield* Effect.forEach(Context.get(annotations, Tool.Readonly) ? [] : dependencies, (dependency) => Stream.peel(dependency.refusal, Sink.head()), { concurrency: 'unbounded' });
                yield* Array.match(Array.getSomes(Array.map(peeled, ([first]) => Option.flatten(first))), { onEmpty: () => Effect.void, onNonEmpty: flow(refusalError, Effect.flatMap(Effect.fail)) });
                yield* Stream.mergeAll(Array.map(peeled, Tuple.get(1)), { concurrency: 'unbounded' }).pipe(
                    Stream.filter(Option.isSome),
                    Stream.take(1),
                    Stream.mapEffect(({ value }) => refusalError([value])),
                    Stream.catch(Stream.succeed),
                    Stream.runForEach((refusal) => Server.ToolCall.use(({ respond }) => respond(refusal))),
                    Effect.forkScoped,
                );
                return yield* Effect.onExitIf(
                    handle(parameters, chosen),
                    (exit) => !(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause)),
                    Effect.fnUntraced(function* () {
                        const [[reports, failed], { add }] = yield* Effect.all([Effect.partition(dependencies, (dependency) => Server.orInternalError(dependency.report), { concurrency: 'unbounded' }), Server.ToolCall]);
                        const reportText = yield* pipe(reports, Array.getSomes, Option.liftPredicate(Array.isReadonlyArrayNonEmpty), Option.map(encodeEntries), Effect.transposeOption);
                        yield* Effect.forEach([...Option.toArray(reportText), ...Array.map(failed, Struct.get('message'))], (text) => add(McpSchema.TextContent.make({ text })), { discard: true });
                    }),
                );
            }, Effect.scoped),
        );
    };
const Route = { complete, optional, register };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Route };
