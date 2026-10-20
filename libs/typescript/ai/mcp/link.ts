import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Data, Deferred, Duration, Effect, flow, Latch, Layer, Match, Option, Record, Schedule, Schema, SchemaAST, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { type Rpc, RpcClient, type RpcClientError, type RpcGroup, type RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { type Route, rendered, targeted } from './server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Published<A, Key extends string> = PublishedTool<A> | PublishedResource<A, Key>;

// --- [MODELS] --------------------------------------------------------------------------

class PublishedTool<A> extends Data.TaggedClass('Tool')<{ readonly annotations: Context.Context<never>; readonly success?: Schema.Codec<A, unknown> }> {}
class PublishedResource<A, Key extends string> extends Data.TaggedClass('Resource')<{ readonly uri: string; readonly complete: { readonly [Field in Key]?: (success: A) => readonly string[] } }> {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, I extends Readonly<Record<string, string | undefined>>, Chosen>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        identity,
        serialization,
        middleware,
        serves,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
        readonly middleware: Layer.Layer<Rpc.MiddlewareClient<Rpcs>, never, RpcClient.Protocol>;
        readonly serves: (chosen: Chosen) => (peer: Peer) => boolean;
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
            const clients = yield* Layer.build(Layer.provideMerge(middleware, Layer.succeed(RpcClient.Protocol, protocol)));
            const client = yield* RpcClient.make(group, { flatten: true }).pipe(Effect.provideContext(clients));
            yield* Deferred.succeed(made, new Linked({ peer: found, query, client }));
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
                Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => new Cause.NoSuchElementError('No linked extension serves the application')),
                Effect.flatMap((serving) => Unmatched.single('linked extension serving the application', Option.none())(serving, ({ query }) => [query])),
                Effect.map(Struct.get('client')),
            ),
    };
});
const publish = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & { readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields }; readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown> }, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    published: { readonly [Tag in Rpcs['_tag']]?: Published<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, keyof Rpc.ExtractTag<Rpcs, Tag>['payloadSchema']['fields'] & string> },
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
    const [server, services] = yield* Effect.all([McpServer.McpServer, Effect.context<R | Schema.Struct<Fields>['DecodingServices']>()]);
    const routed = Record.keys(route.fields);
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values: [...values] } })),
            rendered,
            Effect.provideContext(services),
        );
    const given = Schema.toCodecStringTree(Schema.Struct(route.fields));
    const decoded = (rpc: Rpcs) => (values: { readonly route: unknown; readonly payload: unknown }) => Schema.decodeUnknownEffect(Schema.Struct({ route: given, payload: Schema.toCodecStringTree(rpc.payloadSchema) }))(values, { errors: 'all' });
    const called =
        (rpc: Rpcs) =>
        ({ route: chosen, payload }: { readonly route: Schema.Struct.Type<Fields>; readonly payload: Rpc.Payload<Rpcs> }) =>
            rendered(Effect.flatMap(route.resolve(chosen), (found) => invoke(rpc)(payload, found)));
    const read = (rpc: Rpcs, form: readonly string[]) => (uri: string, values: readonly string[]) => {
        const named = Record.fromEntries(Array.zip(form, values));
        return decoded(rpc)({ route: named, payload: named }).pipe(
            Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
            Effect.flatMap(called(rpc)),
            Effect.flatMap(flow(Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(rpc.successSchema))), rendered)),
            Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri, mimeType: 'application/json', text })] })),
            Effect.provideContext(services),
        );
    };
    const defined = (rpc: Rpcs) => ({ name: rpc._tag, description: SchemaAST.resolveDescription(rpc.payloadSchema.ast), mimeType: 'application/json' });
    const registered = (rpc: Rpcs, uri: string, complete: PublishedResource<Rpc.Success<Rpcs>, string>['complete']) =>
        Array.match({
            onEmpty: () => server.addResource({ resource: McpSchema.Resource.make({ ...defined(rpc), uri }), annotations, handle: Effect.catchTag(read(rpc, [])(uri, []), 'InvalidParams', Effect.die) }),
            onNonEmpty: (params: Array.NonEmptyReadonlyArray<string>) =>
                server.addResourceTemplate({
                    template: McpSchema.ResourceTemplate.make({ ...defined(rpc), uriTemplate: `${uri}/${params.map((key) => `{${key}}`).join('/')}` }),
                    routerPath: `${uri.replace(':', '::')}/${params.map((_, index) => `:${index}`).join('/')}`,
                    annotations,
                    completions: Record.filter(
                        {
                            ...Record.getSomes(Record.map(route.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
                            ...Record.getSomes(
                                Record.map(
                                    complete,
                                    flow(
                                        Option.fromUndefinedOr,
                                        Option.map((select) => answered((_, context) => decoded(rpc)({ route: context?.arguments ?? {}, payload: {} }).pipe(Effect.flatMap(called(rpc)), Effect.map(select)))),
                                    ),
                                ),
                            ),
                        },
                        (_, key) => params.includes(key),
                    ),
                    handle: read(rpc, params),
                }),
        });
    const forms = (rpc: Rpcs) => Array.dedupe(Array.flatMap([[], Record.keys(rpc.payloadSchema.fields)], (own) => [own, [...routed, ...own]])).filter((form) => Schema.is(Schema.toEncoded(Schema.Struct(Struct.omit({ ...route.fields, ...rpc.payloadSchema.fields }, form))))({}));
    return yield* Effect.forEach(
        group.requests.values(),
        (rpc: Rpcs) =>
            Option.match(Option.flatMap(Record.get<string, Published<Rpc.Success<Rpcs>, string> | undefined>(published, rpc._tag), Option.fromUndefinedOr), {
                onNone: () => Effect.void,
                onSome: (row) =>
                    Match.valueTags(row, {
                        Tool: ({ annotations: own, success }) => targeted(route)(Tool.make(rpc._tag, { description: defined(rpc).description, parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(annotations, own)), invoke(rpc)),
                        Resource: ({ uri, complete }) => Effect.forEach(forms(rpc), registered(rpc, uri, complete), { discard: true }),
                    }),
            }),
        { discard: true },
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { listen, PublishedResource, PublishedTool, publish };
