import { Array, Context, Data, Effect, flow, Match, Option, Record, Schema, SchemaAST, Stream } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { UrlParams } from 'effect/http';
import type { Rpc, RpcGroup } from 'effect/rpc';
import { type DependencyOptions, resourceValues } from '#/resources';
import { orInternalError } from '#/server';
import { type Route, registerRoutedTool } from '#/tools';

// --- [MODELS] --------------------------------------------------------------------------

type Registration<A, Key> = Data.TaggedEnum<{
    Tool: { readonly annotations: Context.Context<never>; readonly success?: Schema.Codec<A, unknown> | undefined };
    Resource: { readonly uri: string; readonly complete: { readonly [Field in Extract<Key, string>]?: (success: A) => readonly string[] } } & DependencyOptions<A>;
}>;
interface RegistrationDefinition extends Data.TaggedEnum.WithGenerics<2> {
    readonly taggedEnum: Registration<this['A'], this['B']>;
}
const Registration = Data.taggedEnum<RegistrationDefinition>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerRpcGroup = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & { readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields }; readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown> }, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    published: { readonly [Tag in Rpcs['_tag']]?: Registration<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, keyof Rpc.ExtractTag<Rpcs, Tag>['payloadSchema']['fields'] & string> },
    {
        route,
        invoke,
        annotations,
    }: {
        readonly route: Route<Chosen, Fields, R>;
        readonly invoke: (rpc: Rpcs) => (payload: Rpc.Payload<Rpcs>, chosen: Chosen) => Effect.Effect<Rpc.Success<Rpcs>, Error, R>;
        readonly annotations: Context.Context<never>;
    },
) {
    const [server, services] = yield* Effect.all([McpServer.McpServer, Effect.context<R | Schema.Struct<Fields>['DecodingServices'] | Schema.Struct<Fields>['EncodingServices']>()]);
    const flat = Schema.Record(Schema.String, Schema.String);
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values: [...values] } })),
            orInternalError,
            Effect.provideContext(services),
        );
    const given = flat.pipe(Schema.decodeTo(Schema.toCodecStringTree(Schema.Struct(route.fields))));
    const resourced = Effect.fnUntraced(function* (rpc: Rpcs, { uri, complete, ...acts }: Data.TaggedEnum.Value<Registration<Rpc.Success<Rpcs>, string>, 'Resource'>) {
        const defined = { name: rpc._tag, mimeType: 'application/json', ...Option.match(Option.fromUndefinedOr(SchemaAST.resolveDescription(rpc.payloadSchema.ast)), { onNone: () => ({}), onSome: (description) => ({ description }) }) };
        const { read, depend } = yield* resourceValues(rpc.successSchema, defined.mimeType, Stream.empty);
        const fields = { ...route.fields, ...rpc.payloadSchema.fields };
        const call = Schema.Struct({ route: given, payload: flat.pipe(Schema.decodeTo(Schema.toCodecStringTree(rpc.payloadSchema))) });
        const decoded = (values: unknown) => Schema.decodeUnknownEffect(call)({ route: values, payload: values }, { errors: 'all' });
        const called = ({ route: chosen, payload }: typeof call.Type) => Effect.flatMap(route.resolve(chosen), (found) => invoke(rpc)(payload, found)).pipe(Effect.provideContext(services));
        const answer = (at: string) =>
            Effect.flatMap(Schema.decodeEffect(Schema.URLFromString)(at), ({ searchParams }) => decoded(UrlParams.toReadonlyRecord(UrlParams.fromInput(searchParams)))).pipe(
                Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
                Effect.flatMap((parsed) => read(at, called(parsed))),
                Effect.provideContext(services),
            );
        const resolved = (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.flatMap(decoded(context?.arguments ?? {}), called);
        const completions = {
            ...Record.getSomes(Record.map(route.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
            ...Record.getSomes(
                Record.map(
                    complete,
                    flow(
                        Option.fromUndefinedOr,
                        Option.map((select) => answered(flow(resolved, Effect.map(select)))),
                    ),
                ),
            ),
        };
        yield* Schema.is(Schema.toEncoded(Schema.Struct(fields)))({}) ? server.addResource({ resource: McpSchema.Resource.make({ ...defined, uri }), annotations, handle: Effect.catchTag(answer(uri), 'InvalidParams', Effect.die) }) : Effect.void;
        yield* Array.match(Record.keys(fields), {
            onEmpty: () => Effect.void,
            onNonEmpty: (keys) => server.addResourceTemplate({ template: McpSchema.ResourceTemplate.make({ ...defined, uriTemplate: `${uri}{?${keys.join(',')}}` }), routerPath: uri.replace(':', '::'), annotations, completions, handle: answer }),
        });
        return (routed: Schema.Struct.Type<Fields>) => {
            const named = Effect.provideContext(Schema.encodeEffect(given)(routed), services);
            return depend(
                Effect.map(named, (values) => (Record.isEmptyRecord(values) ? uri : `${uri}?${UrlParams.toString(UrlParams.fromInput(values))}`)),
                () => named.pipe(Effect.flatMap(decoded), Effect.flatMap(called), Effect.provideContext(services)),
            )(acts);
        };
    });
    const rows = Array.flatMap([...group.requests.values()], (rpc: Rpcs) => Option.toArray(Option.map(Option.flatMap(Record.get<string, Registration<Rpc.Success<Rpcs>, string> | undefined>(published, rpc._tag), Option.fromUndefinedOr), (row) => [rpc, row] as const)));
    const linked = Array.getSomes(yield* Effect.forEach(rows, ([rpc, row]) => Match.valueTags(row, { Tool: () => Effect.succeedNone, Resource: (resourceRow) => Effect.asSome(resourced(rpc, resourceRow)) })));
    const depends = (routed: Schema.Struct.Type<Fields>) => Array.map(linked, (dependency) => dependency(routed));
    yield* Effect.forEach(
        rows,
        ([rpc, row]) =>
            Match.valueTags(row, {
                Tool: ({ annotations: own, success }) =>
                    registerRoutedTool({ ...route, depends: (chosen, routed) => Effect.map(route.depends(chosen, routed), Array.appendAll(depends(routed))) })(
                        Tool.make(rpc._tag, { description: SchemaAST.resolveDescription(rpc.payloadSchema.ast), parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(annotations, own)),
                        invoke(rpc),
                    ),
                Resource: () => Effect.void,
            }),
        { discard: true },
    );
    return depends;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Registration, registerRpcGroup };
