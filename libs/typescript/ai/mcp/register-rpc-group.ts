import { Array, Context, Effect, Function, flow, Option, Predicate, Record, Result, Schema, Stream } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { Url, UrlParams } from 'effect/http';
import type { Rpc, RpcGroup } from 'effect/rpc';
import { Resource } from '#/resource';
import { Route } from '#/route';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface ToolRow<A> {
    readonly description: string;
    readonly annotations?: Context.Context<never> | undefined;
    readonly success?: Schema.Codec<A, unknown> | undefined;
}
type ResourceRow<A, Field extends string> = Pick<Parameters<ReturnType<Resource<A>['depend']>>[0], 'report'> & {
    readonly description: string;
    readonly completion: { readonly [Key in Field]?: (success: A) => readonly string[] };
};
interface RpcSchemas<Rpcs extends Rpc.Any> {
    readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields };
    readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown>;
}
interface RpcRoute<Chosen, Fields extends Schema.Struct.Fields, R> extends Route<Chosen, Fields, R> {
    readonly serves: (chosen: Chosen) => boolean;
    readonly completion: { readonly [Key in keyof Fields]?: (input: string, given: Partial<Schema.Struct.Type<Fields>>) => Effect.Effect<readonly string[], Error, R> };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const registerResource = Effect.fnUntraced(function* <Current extends Rpc.Any & RpcSchemas<Current>, Chosen, Fields extends Schema.Struct.Fields, R>(
    rpc: Current,
    { description, completion, report }: ResourceRow<Rpc.Success<Current>, string>,
    {
        routing,
        invoke,
        annotations,
    }: {
        readonly routing: RpcRoute<Chosen, Fields, R>;
        readonly invoke: (payload: Rpc.Payload<Current>, chosen: Chosen) => Effect.Effect<Rpc.Success<Current>, Error, R>;
        readonly annotations: Context.Context<never>;
    },
) {
    const [server, registry, services] = yield* Effect.all([Server, McpServer.McpServer, Effect.context<R | Schema.Struct<Fields>['DecodingServices'] | Schema.Struct<Fields>['EncodingServices']>()]);
    const uri = server.uri(rpc._tag);
    const [base, store] = yield* Effect.all([Effect.fromResult(Url.fromString(uri)), Resource({ name: rpc._tag, description, schema: rpc.successSchema, changes: Stream.empty })]);
    const fields = { ...routing.fields, ...rpc.payloadSchema.fields };
    const query = Schema.Struct({ route: Schema.RecordFromUrlParams.pipe(Schema.decodeTo(Schema.toCodecStringTree(Schema.Struct(routing.fields)))), payload: Schema.RecordFromUrlParams.pipe(Schema.decodeTo(Schema.toCodecStringTree(rpc.payloadSchema))) });
    const decoded = (values: UrlParams.UrlParams) => Schema.decodeEffect(query)({ route: values, payload: values }, { errors: 'all' });
    const invoked = (parsed: typeof query.Type) => Effect.flatMap(routing.resolve(parsed.route), (chosen) => invoke(parsed.payload, chosen)).pipe(Effect.provideContext(services));
    const located = Effect.fnUntraced(function* (parsed: typeof query.Type) {
        const { route: routed, payload } = yield* Effect.orDie(Schema.encodeEffect(query)(parsed));
        return { key: Url.setUrlParams(base, UrlParams.appendAll(routed, payload)).href, content: invoked(parsed) };
    });
    const readUri = (at: string) =>
        Effect.flatMap(Schema.decodeEffect(Schema.URLFromString)(at), flow(Url.urlParams, decoded)).pipe(
            Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
            Effect.flatMap(located),
            Effect.flatMap(({ key, content }) => store.read(key, at, content)),
            Effect.provideContext(services),
        );
    const answered = <RF>(found: Effect.Effect<readonly string[], Error, RF>) =>
        found.pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values } })),
            Server.orInternalError,
            Effect.provideContext(services),
        );
    const completing = Route.complete(routing.fields);
    const completions = {
        ...Record.map(Record.filter(routing.completion, Predicate.isNotUndefined), (handle) => flow(completing(handle), answered)),
        ...Record.map(Record.filter(completion, Predicate.isNotUndefined), (select) => (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => decoded(UrlParams.fromInput(context?.arguments ?? {})).pipe(Effect.flatMap(located), Effect.flatMap(store.cached), Effect.map(select), answered)),
    };
    yield* Schema.is(Schema.toEncoded(Schema.Struct(fields)))({}) ? registry.addResource({ resource: McpSchema.Resource.make({ ...store.descriptor, uri }), annotations, handle: Effect.catchTag(readUri(uri), 'InvalidParams', Effect.die) }) : Effect.void;
    yield* Array.match(Record.keys(fields), {
        onEmpty: () => Effect.void,
        onNonEmpty: (keys) => registry.addResourceTemplate({ template: McpSchema.ResourceTemplate.make({ ...store.descriptor, uriTemplate: `${uri}{?${keys.join(',')}}` }), routerPath: uri.replace(':', '::'), annotations, completions, handle: readUri }),
    });
    return (chosen: Chosen, routed: Schema.Struct.Type<Fields>) => (routing.serves(chosen) ? [store.depend(Schema.encodeEffect(query.fields.route)(routed).pipe(Effect.orDie, Effect.flatMap(decoded), Effect.flatMap(located), Effect.provideContext(services)))({ report })] : []);
});
const registerRpcGroup = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & RpcSchemas<Rpcs>, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs & { readonly payloadSchema: { readonly fields: { readonly [Key in keyof Fields]?: never } } }>,
    {
        tools,
        resources,
        route,
        invoke,
    }: {
        readonly tools: { readonly [Tag in Rpcs['_tag']]?: ToolRow<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>> };
        readonly resources: { readonly [Tag in Rpcs['_tag']]?: ResourceRow<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, keyof Rpc.ExtractTag<Rpcs, Tag>['payloadSchema']['fields'] & string> };
        readonly route: <Tag extends Rpcs['_tag']>(tag: Tag) => RpcRoute<Chosen, Fields, R>;
        readonly invoke: (rpc: Rpcs) => (payload: Rpc.Payload<Rpcs>, chosen: Chosen) => Effect.Effect<Rpc.Success<Rpcs>, Error, R>;
    },
) {
    const listedRpcs = <Row>(rows: Readonly<Record<string, Row | undefined>>) => Array.filterMap([...group.requests.values()], (rpc) => Result.fromOption(Option.product(Option.some(rpc), Option.fromUndefinedOr(rows[rpc._tag])), Function.constVoid));
    const dependencyLookups = yield* Effect.forEach(listedRpcs<ResourceRow<Rpc.Success<Rpcs>, string>>(resources), ([rpc, row]) => registerResource(rpc, row, { routing: route(rpc._tag), invoke: invoke(rpc), annotations: group.annotations }));
    const routeDependencies = (chosen: Chosen, routed: Schema.Struct.Type<Fields>) => Array.flatMap(dependencyLookups, (dependency) => dependency(chosen, routed));
    yield* Effect.forEach(
        listedRpcs<ToolRow<Rpc.Success<Rpcs>>>(tools),
        ([rpc, { description, annotations = Context.empty(), success }]) => {
            const routing = route(rpc._tag);
            return Route({
                ...routing,
                depends: (chosen, routed) => Effect.map(routing.depends(chosen, routed), Array.appendAll(routeDependencies(chosen, routed))),
            })(Tool.make(rpc._tag, { description, parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(group.annotations, annotations)), invoke(rpc));
        },
        { discard: true },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerRpcGroup };
