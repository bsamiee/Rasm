import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Data, Deferred, Duration, Effect, flow, Latch, Layer, Match, Option, Record, Schedule, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, type RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { type Route, rendered, targeted } from './server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Published<A, Chosen> = PublishedTool<A> | PublishedResource<Chosen>;

// --- [MODELS] --------------------------------------------------------------------------

class PublishedTool<A> extends Data.TaggedClass('Tool')<{ readonly annotations: Context.Context<never>; readonly success?: Schema.Codec<A, unknown> }> {}
class PublishedResource<Chosen> extends Data.TaggedClass('Resource')<{ readonly uri: string; readonly complete: Record.ReadonlyRecord<string, (chosen: Chosen) => Effect.Effect<readonly string[], Error>> }> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, I extends Readonly<Record<string, string | undefined>>, Chosen>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        peer: name,
        identity,
        serialization,
        middleware,
        serves,
        follow,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly peer: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
        readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>;
        readonly serves: (chosen: Chosen) => (peer: Peer) => boolean;
        readonly follow: (client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError>) => Effect.Effect<void, RpcClientError.RpcClientError>;
    },
) {
    class Linked extends Data.Class<{ readonly peer: Peer; readonly query: string; readonly client: RpcClient.RpcClient.Flat<Rpcs, RpcClientError.RpcClientError> }> {}
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, query, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), Effect.map(HttpServerRequest.ParsedSearchParams, flow(UrlParams.fromInput, UrlParams.toString)), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0) }).pipe(
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                    onDisconnect: Effect.andThen(
                        Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                        ended.open,
                    ),
                }),
                Effect.provideService(Socket.Socket, socket),
                Effect.provide(serialization),
            );
            const clients = yield* Layer.build(middleware).pipe(Effect.provideService(RpcClient.Protocol, protocol));
            const client = yield* RpcClient.make(group, { flatten: true }).pipe(Effect.provideContext(clients), Effect.provideService(RpcClient.Protocol, protocol));
            yield* Deferred.succeed(made, new Linked({ peer: found, query, client }));
            yield* Effect.forkScoped(follow(client));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === origin ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    return {
        peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))),
        call: (chosen: Chosen) =>
            SubscriptionRef.get(links).pipe(
                Effect.map(Array.filter(({ peer }) => serves(chosen)(peer))),
                Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new Cause.NoSuchElementError(`${name} is not linked`)),
                Effect.flatMap((serving) => Unmatched.single(name, Option.none())(serving, ({ query }) => [query])),
                Effect.map(Struct.get('client')),
            ),
    };
});
const publish = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & { readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields }; readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown> }, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    published: { readonly [Tag in Rpcs['_tag']]?: Published<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, Chosen> },
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
    const [server, services, register] = [yield* McpServer.McpServer, yield* Effect.context<R | Schema.Struct<Fields>['DecodingServices']>(), targeted(route)];
    const routed = Record.keys(route.fields);
    const bare = (schema: Schema.Top) => Schema.is(Schema.toEncoded(schema))({});
    const failed = Effect.catchCause((cause) => Effect.fail(McpSchema.InternalError.make({ message: rendered(cause) })));
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values: [...values] } })),
            failed,
            Effect.provideContext(services),
        );
    const given = Schema.toCodecStringTree(Schema.Struct(route.fields));
    const decoded = (rpc: Rpcs) => (values: { readonly route: unknown; readonly payload: unknown }) => Schema.decodeUnknownEffect(Schema.Struct({ route: given, payload: Schema.toCodecStringTree(rpc.payloadSchema) }))(values, { errors: 'all' });
    const called = (rpc: Rpcs) =>
        Effect.fnUntraced(function* ({ route: chosen, payload }: { readonly route: Schema.Struct.Type<Fields>; readonly payload: Rpc.Payload<Rpcs> }) {
            return yield* invoke(rpc)(payload, yield* route.resolve(chosen));
        }, failed);
    const read = (rpc: Rpcs, form: readonly string[]) => (uri: string, values: readonly string[]) => {
        const named = Record.fromEntries(Array.zip(form, values));
        return decoded(rpc)({ route: named, payload: named }).pipe(
            Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
            Effect.flatMap(called(rpc)),
            Effect.flatMap(flow(Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(rpc.successSchema))), failed)),
            Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri, mimeType: 'application/json', text })] })),
            Effect.provideContext(services),
        );
    };
    const defined = (rpc: Rpcs) => ({ name: rpc._tag, description: SchemaAST.resolveDescription(rpc.payloadSchema.ast), mimeType: 'application/json' });
    const registered = (rpc: Rpcs, uri: string, complete: Record.ReadonlyRecord<string, (chosen: Chosen) => Effect.Effect<readonly string[], Error>>) =>
        Array.match({
            onEmpty: () => server.addResource({ resource: McpSchema.Resource.make({ ...defined(rpc), uri }), annotations, handle: Effect.catchTag(read(rpc, [])(uri, []), 'InvalidParams', Effect.die) }),
            onNonEmpty: (params: Array.NonEmptyReadonlyArray<string>) =>
                server.addResourceTemplate({
                    template: McpSchema.ResourceTemplate.make({ ...defined(rpc), uriTemplate: `${uri}/${params.map((key) => `{${key}}`).join('/')}` }),
                    routerPath: `${uri.replace(':', '::')}/${params.map((_, index) => `:${index}`).join('/')}`,
                    annotations,
                    completions: {
                        ...Record.getSomes(Record.map(route.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
                        ...Record.map(
                            Record.filter(complete, (_, key) => params.includes(key)),
                            (select) => answered((_, context) => Schema.decodeEffect(given)(context?.arguments ?? {}, { errors: 'all' }).pipe(Effect.flatMap(route.resolve), Effect.flatMap(select))),
                        ),
                    },
                    handle: read(rpc, params),
                }),
        });
    const forms = (rpc: Rpcs) => {
        const keys = Record.keys(rpc.payloadSchema.fields);
        return [...(bare(rpc.payloadSchema) ? [...(bare(Schema.Struct(route.fields)) ? [[]] : []), ...(routed.length > 0 && keys.length > 0 ? [routed] : [])] : []), ...(routed.length + keys.length > 0 ? [[...routed, ...keys]] : [])];
    };
    return yield* Effect.forEach(
        group.requests.values(),
        (rpc: Rpcs) =>
            Option.match(Option.flatMap(Record.get<string, Published<Rpc.Success<Rpcs>, Chosen> | undefined>(published, rpc._tag), Option.fromUndefinedOr), {
                onNone: () => Effect.void,
                onSome: (row) =>
                    Match.valueTags(row, {
                        Tool: ({ annotations: own, success }) => register(Tool.make(rpc._tag, { description: defined(rpc).description, parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(annotations, own)), invoke(rpc)),
                        Resource: ({ uri, complete }) => Effect.forEach(forms(rpc), registered(rpc, uri, complete), { discard: true }),
                    }),
            }),
        { discard: true },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { listen, PublishedResource, PublishedTool, publish };
