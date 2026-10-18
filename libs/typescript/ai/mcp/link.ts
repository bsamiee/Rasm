import { Array, Context, Data, Deferred, Duration, Effect, Filter, Function, flow, Latch, Match, Option, Queue, Record, Schedule, Schema, SchemaAST, SchemaGetter, SchemaIssue, Scope, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse, UrlParams } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Rpc, RpcClient, RpcClientError, RpcGroup, RpcSerialization, RpcServer } from 'effect/rpc';
import { Socket, SocketServer } from 'effect/socket';
import { Instructions, register, Unmatched } from './server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Link = 'linked' | 'closed' | 'unreachable';
type Fields = Readonly<Record<string, Schema.Codec<unknown, unknown>>>;
type Erased = Rpc.AnyWithProps & { readonly payloadSchema: Schema.Codec<unknown, unknown> & { readonly fields: Fields }; readonly successSchema: Schema.Codec<unknown, unknown>; readonly errorSchema: Schema.Codec<Error, unknown> };
type Call = <R extends Erased>(rpc: R, payload: R['payloadSchema']['Type']) => Effect.Effect<R['successSchema']['Type'], Error>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const WIRE = RpcSerialization.layerSchemaBinary({ fingerprintPayloads: true });
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

const Surface = Context.Reference<{ readonly _tag: 'Tool' } | { readonly _tag: 'Resource'; readonly uri: string } | { readonly _tag: 'Prompt' } | { readonly _tag: 'None' }>('@rasm/mcp/link/Surface', { defaultValue: () => ({ _tag: 'Tool' }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const serve = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, { url, identity, concurrency }: { readonly url: string; readonly identity: UrlParams.Input; readonly concurrency: number | 'unbounded' }) =>
    Stream.unwrap(
        Effect.gen(function* () {
            const [{ reader, writer }, states, linked] = yield* Effect.all([Socket.makeWebSocket(`${url}?${UrlParams.toString(UrlParams.fromInput(identity))}`), Queue.unbounded<Link>(), Effect.flatMap(Effect.scope, Scope.fork)]);
            const protocol = yield* RpcServer.makeProtocolSocketServer.pipe(
                Effect.provideService(SocketServer.SocketServer, {
                    address: NetAddress.inetAddressUnsafe(NetAddress.ipv4Unspecified, 0),
                    run: (handler) =>
                        handler(
                            Socket.make({
                                reader: reader.pipe(
                                    Effect.tapError(() => Queue.offer(states, 'unreachable')),
                                    Effect.tap(() => Effect.acquireRelease(Queue.offer(states, 'linked'), () => Queue.offer(states, 'closed'))),
                                ),
                                writer,
                            }),
                        ).pipe(Effect.ignoreCause, Effect.andThen(Effect.sleep(REDIAL)), Effect.forever),
                }),
                Effect.provide(WIRE),
                Scope.provide(linked),
            );
            yield* Effect.forkScoped(RpcServer.make(group, { concurrency, disableTracing: true }).pipe(Effect.provideService(RpcServer.Protocol, protocol)));
            return Stream.fromQueue(states);
        }),
    );
const listen = Effect.fn('listen')(function* <Rpcs extends Erased, Peer, I extends Readonly<Record<string, string | undefined>>, F extends Fields>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        peer: name,
        identity,
        route,
        watch,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly peer: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly route: { readonly fields: F; readonly select: (route: Schema.Struct.Type<F>) => Option.Option<(peer: Peer) => boolean> };
        readonly watch: <A, E>(sent: Effect.Effect<A, E>, call: Call) => Effect.Effect<A, E>;
    },
) {
    const rpcs: readonly Erased[] = Array.fromIterable(group.requests.values());
    const contract = rpcs.map((rpc) => Rpc.make(rpc._tag, { payload: rpc.payloadSchema, success: rpc.successSchema, error: rpc.errorSchema }));
    class Linked extends Data.Class<{ readonly peer: Peer; readonly client: RpcClient.RpcClient.Flat<(typeof contract)[number], RpcClientError.RpcClientError> }> {}
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const client = yield* RpcClient.make(RpcGroup.make(...contract), { disableTracing: true, flatten: true }).pipe(
                Effect.provideServiceEffect(RpcClient.Protocol, RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0).pipe(Schedule.tap(() => ended.open)) })),
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                    onDisconnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                }),
                Effect.provideService(Socket.Socket, socket),
                Effect.provide(WIRE),
            );
            yield* Deferred.succeed(made, new Linked({ peer: found, client }));
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === origin ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    const reach = (matches: (peer: Peer) => boolean) =>
        SubscriptionRef.changes(links).pipe(Stream.filterMap(Filter.fromPredicateOption(Array.findFirst((entry: Linked) => matches(entry.peer)))), Stream.timeout(REDIAL), Stream.runHead, Effect.flatMap(Effect.fromOption(() => LinkError.make({ peer: name, cause: Option.none() }))));
    const call =
        ({ client }: Linked): Call =>
        (rpc, payload) => {
            const sent: Call = (each, given) => Effect.flatMap(client(each._tag, given), flow(Schema.decodeUnknownEffect(Schema.toType(each.successSchema)), Effect.orDie));
            return watch(sent(rpc, payload), sent).pipe(Effect.catchIf(Schema.is(RpcClientError.RpcClientError), (cause) => Effect.fail(LinkError.make({ peer: name, cause: Option.some(cause) }))));
        };
    const annotated = (annotations: Context.Context<never>) => Context.add(annotations, Instructions, [...Context.get(group.annotations, Instructions), ...Context.get(annotations, Instructions)]);
    const tool = <P extends Schema.Codec<unknown, unknown> & { readonly fields: Fields }, S extends Schema.Codec<unknown, unknown>, R>(
        definition: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
        handle: (payload: P['Type'], use: Call) => Effect.Effect<S['Type'], Error, R>,
    ) => {
        const parts = Schema.Tuple([definition.parametersSchema, Schema.Struct(route.fields)]);
        return register(
            definition
                .setParameters(
                    Schema.toEncoded(Schema.Struct({ ...definition.parametersSchema.fields, ...route.fields })).pipe(
                        Schema.decodeTo(Schema.Tuple([Schema.toType(definition.parametersSchema), Schema.instanceOf(Linked)]), {
                            decode: SchemaGetter.transformEffect(
                                Effect.fnUntraced(function* (given) {
                                    const [payload, chosen] = yield* Effect.mapError(Schema.decodeEffect(Schema.toCodecJson(parts))([given, given], { errors: 'all' }), Struct.get('issue'));
                                    const linked = yield* Option.match(route.select(chosen), { onNone: () => Effect.fail(LinkError.make({ peer: name, cause: Option.none() })), onSome: reach }).pipe(Effect.mapError(({ message }) => new SchemaIssue.InvalidValue({ message }, given)));
                                    return [payload, linked] as const;
                                }),
                            ),
                            encode: SchemaGetter.forbidden(() => 'arguments decode only'),
                        }),
                    ),
                )
                .annotateMerge(annotated(definition.annotations)),
            ([payload, linked]) => handle(payload, call(linked)),
        );
    };
    const read = Effect.fnUntraced(function* (rpc: Erased, args: unknown) {
        const payload = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(rpc.payloadSchema))(args, { errors: 'all' });
        return yield* call(yield* reach(Function.constTrue))(rpc, payload);
    });
    const encoded = (rpc: Erased) => flow(Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(rpc.successSchema))), Effect.orDie);
    const completion = (rpc: Erased) =>
        Record.map(
            rpc.payloadSchema.fields,
            (_, key) => (input: string, context: { readonly arguments?: Readonly<Record<string, string>> } | undefined) =>
                read(rpc, { ...context?.arguments, [key]: input }).pipe(
                    Effect.as([input]),
                    Effect.catchIf(Schema.is(Unmatched), ({ candidates }) => Effect.succeed([...candidates])),
                ),
        );
    const publish = Effect.forEach(
        rpcs,
        (rpc) => {
            const [description, keys, annotations] = [SchemaAST.resolveDescription(rpc.payloadSchema.ast), Record.keys(rpc.payloadSchema.fields), annotated(rpc.annotations)];
            return Match.value(Context.get(rpc.annotations, Surface)).pipe(
                Match.tagsExhaustive({
                    Tool: () => tool(Tool.make(rpc._tag, { description, parameters: rpc.payloadSchema, success: rpc.successSchema }).annotateMerge(rpc.annotations), (payload, use) => use(rpc, payload)),
                    Resource: ({ uri }) => {
                        const segments = [`${uri}/`, ...keys.slice(1).map(() => '/'), ''];
                        return Effect.all(
                            [
                                Record.every(rpc.payloadSchema.fields, (field) => SchemaAST.isOptional(SchemaAST.toEncoded(field.ast))) ? McpServer.registerResource({ uri, name: rpc._tag, description, mimeType: 'application/json', annotations, content: Effect.flatMap(read(rpc, {}), encoded(rpc)) }) : Effect.void,
                                Array.isReadonlyArrayNonEmpty(keys)
                                    ? McpServer.registerResource(
                                          Object.assign([...segments], { raw: segments }),
                                          ...keys.map((key) => McpSchema.param(key, Schema.String)),
                                      )({ name: rpc._tag, description, mimeType: 'application/json', annotations, completion: completion(rpc), content: (_uri, ...values) => Effect.flatMap(read(rpc, Record.fromEntries(Array.zip(keys, values))), encoded(rpc)) })
                                    : Effect.void,
                            ],
                            { discard: true },
                        );
                    },
                    Prompt: () => McpServer.registerPrompt({ name: rpc._tag, description, parameters: rpc.payloadSchema.fields, completion: completion(rpc), annotations, content: (given) => Effect.flatMap(read(rpc, given), flow(Schema.decodeUnknownEffect(Schema.String), Effect.orDie)) }),
                    None: () => Effect.void,
                }),
            );
        },
        { discard: true },
    );
    return { peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))), tool, publish };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Link, listen, Surface, serve };
