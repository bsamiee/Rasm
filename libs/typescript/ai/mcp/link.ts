import { Unmatched } from '@rasm/search';
import { Array, Context, Data, Deferred, Duration, Effect, Filter, Function, flow, Latch, type Layer, Option, Record, Schedule, Schema, SchemaAST, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { type Rpc, RpcClient, RpcClientError, RpcGroup, type RpcSerialization } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Blocks, Instructions, register } from './server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Readonly<Record<string, Schema.Codec<unknown, unknown>>>;
type Call = <Tag extends string, A, I, B, J, E extends Error, K>(rpc: Rpc.Rpc<Tag, Schema.Codec<A, I>, Schema.Codec<B, J>, Schema.Codec<E, K>>, payload: A) => Effect.Effect<B, E | LinkError>;
type Publish = <Tag extends string, A, I, B, J, E extends Error, K>(rpc: Rpc.Rpc<Tag, Schema.Codec<A, I> & { readonly fields: Fields }, Schema.Codec<B, J>, Schema.Codec<E, K>>, uri: Option.Option<string>, annotations: Context.Context<never>) => Effect.Effect<void, never, McpServer.McpServer>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const REDIAL = Duration.seconds(2);

// --- [ERRORS] --------------------------------------------------------------------------

class LinkError extends Schema.TaggedError<LinkError, { readonly brand: unique symbol }>()('LinkError', { peer: Schema.String, cause: Schema.OptionFromOptionalKey(RpcClientError.RpcClientError) }) {
    override get message(): string {
        return Option.match(this.cause, {
            onNone: () => `${this.peer} is not linked, start it and retry`,
            onSome: ({ message }) => `${this.peer} dropped the link mid-call (${message}), the call may have taken effect, check before repeating it`,
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const Publication = Context.Reference<(publish: Publish, annotations: Context.Context<never>) => Effect.Effect<void, never, McpServer.McpServer>>('@rasm/mcp/link/Publication', { defaultValue: () => () => Effect.void });
const Completion = Context.Reference<(rpc: Rpc.Any, parameter: string, call: Call) => Effect.Effect<readonly Schema.Json[], Error>>('@rasm/mcp/link/Completion', { defaultValue: () => () => Effect.succeed([]) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const routed = <P extends Schema.Codec<unknown, unknown> & { readonly fields: Fields }, Route extends Schema.Codec<unknown, unknown> & { readonly fields: Fields }>(parameters: P, route: Route) => {
    const parts = Schema.Tuple([parameters, route]);
    return Schema.toEncoded(Schema.Struct({ ...parameters.fields, ...route.fields })).pipe(
        Schema.decodeTo(Schema.Tuple([Schema.toType(parameters), Schema.toType(route)]), {
            decode: SchemaGetter.transformEffect((given) => Effect.mapError(Schema.decodeUnknownEffect(Schema.toCodecJson(parts))([given, given], { errors: 'all' }), Struct.get('issue'))),
            encode: SchemaGetter.forbidden(() => 'arguments decode only'),
        }),
    );
};
const tool = <Tag extends string, A, I, B, J, E extends Error, K>(rpc: Rpc.Rpc<Tag, Schema.Codec<A, I> & { readonly fields: Fields }, Schema.Codec<B, J>, Schema.Codec<E, K>>) => rpc.annotate(Publication, (publish, annotations) => publish(rpc, Option.none(), annotations));
const resource =
    (uri: string) =>
    <Tag extends string, A, I, B, J, E extends Error, K>(rpc: Rpc.Rpc<Tag, Schema.Codec<A, I> & { readonly fields: Fields }, Schema.Codec<B, J>, Schema.Codec<E, K>>) =>
        rpc.annotate(Publication, (publish, annotations) => publish(rpc, Option.some(uri), annotations));
const listen = Effect.fn('listen')(function* <Rpcs extends Rpc.Any, Peer, I extends Readonly<Record<string, string | undefined>>, Route, Encoded>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        peer: name,
        identity,
        serialization,
        route,
        watch,
        trailer,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly peer: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly serialization: Layer.Layer<RpcSerialization.RpcSerialization>;
        readonly route: {
            readonly schema: Schema.Codec<Route, Encoded> & { readonly fields: Fields };
            readonly select: (route: Route, peers: readonly Peer[]) => Option.Option<(peer: Peer) => boolean>;
            readonly completion: Readonly<Record<string, (peers: readonly Peer[], input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => readonly Schema.Json[]>>;
        };
        readonly watch: <A, E>(sent: Effect.Effect<A, E>, call: Call) => Effect.Effect<A, E>;
        readonly trailer: (route: Route, peer: Peer) => Effect.Effect<McpSchema.ContentBlock, Error>;
    },
) {
    class Linked extends Data.Class<{ readonly peer: Peer; readonly protocol: typeof RpcClient.Protocol.Service }> {}
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const protocol = yield* RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0).pipe(Schedule.tap(() => ended.open)) }).pipe(
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                    onDisconnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                }),
                Effect.provideService(Socket.Socket, socket),
                Effect.provide(serialization),
            );
            yield* Deferred.succeed(made, new Linked({ peer: found, protocol }));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === origin ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    const reach = (given: Route) =>
        SubscriptionRef.changes(links).pipe(
            Stream.map((rows) => Option.match(route.select(given, rows.map(Struct.get('peer'))), { onNone: Array.empty<Linked>, onSome: (matches) => rows.filter((entry) => matches(entry.peer)) })),
            Stream.filterMap(Filter.fromPredicate(Array.isArrayNonEmpty)),
            Stream.mapEffect(
                Effect.fnUntraced(function* ([only, ...others]) {
                    return yield* others.length === 0
                        ? Effect.succeed(only)
                        : Effect.fail(
                              Unmatched.make({
                                  subject: `single ${name} peer`,
                                  query: '',
                                  candidates: yield* Effect.forEach([only, ...others], ({ peer }) => Schema.encodeEffect(identity)(peer).pipe(Effect.orDie, Effect.map(flow(UrlParams.fromInput, UrlParams.toString)))),
                              }),
                          );
                }),
            ),
            Stream.timeout(REDIAL),
            Stream.runHead,
            Effect.flatMap(Effect.fromOption(() => LinkError.make({ peer: name, cause: Option.none() }))),
        );
    const call =
        ({ protocol }: Linked): Call =>
        (rpc, payload) => {
            const sent: Call = (each, given) =>
                Effect.scoped(Effect.flatMap(RpcClient.make(RpcGroup.make(each), { disableTracing: true, flatten: true }), (client) => client(each._tag, given))).pipe(
                    Effect.provideService(RpcClient.Protocol, protocol),
                    Effect.catchIf(Schema.is(RpcClientError.RpcClientError), (cause) => Effect.fail(LinkError.make({ peer: name, cause: Option.some(cause) }))),
                );
            return watch(sent(rpc, payload), sent);
        };
    const annotated = (annotations: Context.Context<never>) => Context.add(annotations, Instructions, [...Context.get(group.annotations, Instructions), ...Context.get(annotations, Instructions)]);
    const registerTool = <P extends Schema.Codec<unknown, unknown> & { readonly fields: Fields }, S extends Schema.Codec<unknown, unknown>, R>(
        definition: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
        handle: (payload: P['Type'], use: Call) => Effect.Effect<S['Type'], Error, R>,
    ) => {
        const parameters = routed(definition.parametersSchema, route.schema);
        return register(
            definition
                .setParameters(
                    parameters.pipe(
                        Schema.decodeTo(Schema.toType(parameters), {
                            decode: SchemaGetter.checkEffect(([, chosen]) => Effect.map(SubscriptionRef.get(links), (rows) => Option.isSome(route.select(chosen, rows.map(Struct.get('peer')))) || `No applicable ${name} peer`)),
                            encode: SchemaGetter.passthrough(),
                        }),
                    ),
                )
                .annotateMerge(annotated(definition.annotations)),
            Effect.fnUntraced(function* ([payload, chosen]) {
                const linked = yield* reach(chosen);
                const value = yield* handle(payload, call(linked));
                const [block, add] = yield* Effect.all([trailer(chosen, linked.peer), Blocks]);
                yield* add(block);
                return value;
            }),
        );
    };
    const completion = (rpc: Rpc.Any, fields: readonly string[]) => ({
        ...Record.map(route.completion, (complete) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.map(SubscriptionRef.get(links), (rows) => Array.fromIterable(complete(rows.map(Struct.get('peer')), input, context)))),
        ...Record.fromIterableWith(fields, (key) => [
            key,
            (_input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
                Schema.decodeEffect(Schema.toCodecStringTree(route.schema))(context?.arguments ?? {}).pipe(
                    Effect.flatMap(reach),
                    Effect.flatMap((linked) => Context.get(group.annotations, Completion)(rpc, key, call(linked))),
                    Effect.map(Array.fromIterable),
                ),
        ]),
    });
    const routes = Record.toEntries(route.schema.fields).map(([key, field]) => McpSchema.param(key, Schema.toEncoded(Schema.toCodecJson(field))));
    const publish: Publish = (rpc, location, inherited) => {
        const description = SchemaAST.resolveDescription(rpc.payloadSchema.ast);
        const definition = { name: rpc._tag, ...(description === undefined ? {} : { description }), mimeType: 'application/json', annotations: annotated(inherited) };
        const keys = Record.keys(rpc.payloadSchema.fields);
        const parameters = [...routes, ...Record.toEntries(rpc.payloadSchema.fields).map(([key, field]) => McpSchema.param(key, Schema.toEncoded(Schema.toCodecJson(field))))];
        const read = Effect.fnUntraced(function* (uri: string, args: unknown) {
            const [payload, chosen] = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(Schema.Tuple([rpc.payloadSchema, route.schema])))([args, args], { errors: 'all' });
            const text = yield* call(yield* reach(chosen))(rpc, payload).pipe(Effect.flatMap(flow(Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(rpc.successSchema))), Effect.orDie)));
            return { contents: [{ uri, mimeType: definition.mimeType, text }] };
        });
        return Option.match(location, {
            onNone: () => registerTool(Tool.make(definition.name, { description, parameters: rpc.payloadSchema, success: rpc.successSchema }).annotateMerge(inherited), (payload, use) => use(rpc, payload)),
            onSome: (uri) => {
                const { optional, unrouted } = Record.map(
                    { optional: rpc.payloadSchema.fields, unrouted: route.schema.fields },
                    Record.every((field) => SchemaAST.isOptional(SchemaAST.toEncoded(field.ast))),
                );
                return Effect.forEach(
                    [...(optional ? [...(unrouted ? [[]] : []), ...(routes.length > 0 && keys.length > 0 ? [routes] : [])] : []), ...(parameters.length > 0 ? [parameters] : [])],
                    (params) => {
                        const fields = params.map(Struct.get('name'));
                        const segments = [`${uri}/`, ...fields.slice(1).map(Function.constant('/')), ''];
                        return params.length === 0
                            ? McpServer.registerResource({ ...definition, uri, content: read(uri, {}) })
                            : McpServer.registerResource(
                                  Object.assign([...segments], { raw: segments }),
                                  ...params,
                              )({
                                  ...definition,
                                  completion: completion(
                                      rpc,
                                      fields.filter((key) => keys.includes(key)),
                                  ),
                                  content: (at, ...values) => read(at, Record.fromEntries(Array.zip(fields, values))),
                              });
                    },
                    { discard: true },
                );
            },
        });
    };
    return { peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))), tool: registerTool, publish: Effect.forEach(group.requests.values(), (rpc) => Context.get(rpc.annotations, Publication)(publish, rpc.annotations), { discard: true }) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Completion, listen, resource, routed, tool };
