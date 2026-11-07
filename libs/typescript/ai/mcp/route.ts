import { Array, Cause, Context, Effect, Exit, flow, Option, pipe, Record, Schema, Sink, Stream, Struct, Tuple } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import type { Resource } from '#/resource.ts';
import { Server } from '#/server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Route<Chosen, Fields extends Schema.Struct.Fields, R> {
    readonly fields: Fields;
    readonly resolve: (given: Schema.Struct.Type<Fields>) => Effect.Effect<Chosen, Error, R>;
    readonly depends: (chosen: Chosen, given: Schema.Struct.Type<Fields>) => Effect.Effect<readonly ReturnType<ReturnType<Resource<unknown>['depend']>>[], Error, R>;
    readonly annotations: Context.Context<never>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeEntries = flow(Record.fromEntries<readonly [string, Schema.Json]>, Schema.encodeEffect(Schema.fromJsonString(Schema.JsonObject)), Effect.orDie);
const refusalError = (entries: readonly (readonly [string, Schema.Json])[]) => Effect.map(encodeEntries(entries), (text) => new Cause.IllegalArgumentError(text));
const optional = <Chosen, Fields extends Schema.Struct.Fields, R>(route: Route<Chosen, Fields, R>): Route<Option.Option<Chosen>, Fields, R> => ({
    fields: route.fields,
    resolve: (given) => (Object.values(given).every((value) => Option.isOption(value) && Option.isNone(value)) ? Effect.succeedNone : Effect.asSome(route.resolve(given))),
    depends: (chosen, given) => Option.match(chosen, { onNone: () => Effect.succeed([]), onSome: (picked) => route.depends(picked, given) }),
    annotations: Context.omit(Server.Visible)(route.annotations),
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
        const report = Effect.fnUntraced(function* (dependencies: Effect.Success<ReturnType<typeof route.depends>>) {
            const [[reports, failed], { add }] = yield* Effect.all([Effect.partition(dependencies, Struct.get('report'), { concurrency: 'unbounded' }), Server.ToolCall]);
            const encoded = yield* pipe(reports, Array.getSomes, Option.liftPredicate(Array.isReadonlyArrayNonEmpty), Option.map(encodeEntries), Effect.transposeOption);
            yield* Effect.forEach([...Option.toArray(encoded), ...Array.map(failed, Struct.get('message'))], (text) => add(McpSchema.TextContent.make({ text })), { discard: true });
        });
        return Server.registerTool(
            tool.setParameters(Record.isEmptyRecord(fields) ? Tool.EmptyParams : Schema.toEncoded(Schema.Struct(fields))).annotateMerge(annotations),
            Effect.fnUntraced(function* (given) {
                const [[parameters, routed], { respond }] = yield* Effect.all([Schema.decodeEffect(parts)([given, given], { errors: 'all' }), Server.ToolCall]);
                const chosen = yield* route.resolve(routed);
                const dependencies = yield* route.depends(chosen, routed);
                const peeled = yield* Effect.forEach(Context.get(annotations, Tool.Readonly) ? [] : dependencies, (dependency) => Stream.peel(dependency.refusal, Sink.head()), { concurrency: 'unbounded' });
                yield* Array.match(Array.getSomes(Array.map(peeled, ([first]) => Option.flatten(first))), { onEmpty: () => Effect.void, onNonEmpty: flow(refusalError, Effect.flatMap(Effect.fail)) });
                yield* pipe(
                    Array.map(peeled, Tuple.get(1)),
                    Stream.mergeAll({ concurrency: 'unbounded' }),
                    Stream.filter(Option.isSome),
                    Stream.take(1),
                    Stream.mapEffect(({ value }) => refusalError([value])),
                    Stream.ignore({ log: 'Error' }),
                    Stream.runForEach(respond),
                    Effect.forkScoped,
                );
                return yield* Effect.onExitIf(
                    handle(parameters, chosen),
                    (exit) => !(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause)),
                    () => report(dependencies),
                );
            }, Effect.scoped),
        );
    };
const complete = <Fields extends Schema.Struct.Fields, Given = Schema.Struct.Type<Fields>>(fields: Fields) => {
    const none: Partial<Given> = {};
    const decoders = Record.collect(fields, (key) => Schema.decodeUnknownEffect(Schema.toCodecStringTree(Schema.Struct(Struct.pick(fields, [key])))));
    const decode = (resolved: Readonly<Record<string, string>>): Effect.Effect<Partial<Given>, never, Schema.Struct<Fields>['DecodingServices']> =>
        Effect.map(
            Effect.forEach(decoders, (decoder) => Effect.option(decoder(resolved))),
            (picked) => ({ ...none, ...Object.fromEntries(Array.flatMap(Array.getSomes(picked), Object.entries)) }),
        );
    return <Values, R>(completing: (input: string, given: Partial<Given>) => Effect.Effect<Values, Error, R>) =>
        (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
            Effect.flatMap(decode(context?.arguments ?? {}), (given) => completing(input, given));
};
const Route = Object.assign(register, { complete, optional });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Route };
