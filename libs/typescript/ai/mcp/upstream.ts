import manifest from '@rasm/mcp/package.json' with { type: 'json' };
import { Array, Cause, Context, Data, type Duration, Effect, FiberMap, Function, flow, Match, Option, Order, Pool, Predicate, Queue, Record, Ref, Result, Schedule, Schema, Stream, Struct, Tuple } from 'effect';
import { McpClient, McpClientError, McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import type { HttpClient } from 'effect/http';
import { Rpc } from 'effect/rpc';
import { Link } from '#/link';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type Pattern = string | RegExp;
type Noticed = typeof Link.Notice.Type & { readonly peer: string };
interface Filter {
    readonly allow?: readonly Pattern[];
    readonly deny?: readonly Pattern[];
}
type Route = (peers: Array.NonEmptyReadonlyArray<Link>, given: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<Link, McpSchema.InvalidParams>;
interface Kind<A> {
    readonly items: (source: Upstream) => readonly A[];
    readonly key: (item: A) => string;
    readonly same: (self: A, that: A) => boolean;
    readonly registered: () => readonly (readonly [string, Context.Context<never>])[];
    readonly add: (routing: {
        readonly forward: <B, E, R>(at: string, given: Record.ReadonlyRecord<string, unknown>, use: (source: Upstream) => Effect.Effect<B, E, R>) => Effect.Effect<B, E | McpSchema.InternalError | McpSchema.InvalidParams, R>;
    }) => (item: A, annotations: Context.Context<never>) => Effect.Effect<void>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const IDENTITY = { name: manifest.name, version: manifest.version } as const;
const PROTOCOLS = Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))).filter((adapter) => adapter.protocolVersion !== McpProtocol.v2024_11_05.protocolVersion);

// --- [MODELS] --------------------------------------------------------------------------

const Listing = Schema.Struct({ tools: Schema.Array(McpSchema.Tool), resources: Schema.Array(McpSchema.Resource), templates: Schema.Array(McpSchema.ResourceTemplate), prompts: Schema.Array(McpSchema.Prompt) });

class Upstream extends Data.Class<
    typeof Listing.Type & {
        readonly peer: Link;
        readonly annotations: Context.Context<never>;
        readonly notices: Stream.Stream<typeof Link.Notice.Type>;
        readonly call: (name: string, given: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<McpSchema.CallToolResult, McpSchema.InternalError | McpSchema.InvalidParams, McpSchema.McpRequestContext>;
        readonly read: (uri: string) => Effect.Effect<McpSchema.ReadResourceResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly get: (name: string, given: Record.ReadonlyRecord<string, string>) => Effect.Effect<McpSchema.GetPromptResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly complete: (payload: Pick<typeof McpSchema.Complete.payloadSchema.Type, 'ref' | 'argument'> & { readonly context: typeof McpSchema.Complete.payloadSchema.Type.context | undefined }) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError>;
    }
> {
    static readonly serve = Effect.fnUntraced(function* <R>(sources: Array.NonEmptyReadonlyArray<Stream.Stream<readonly Upstream[], never, R>>, { route }: { readonly route: Route }) {
        const [server, { relist, events }, services] = yield* Effect.all([McpServer.McpServer, Server, Effect.context<R>()]);
        const kinds = yield* Effect.all([
            kind<McpSchema.Tool>(route, {
                items: Struct.get('tools'),
                key: Struct.get('name'),
                same: Schema.toEquivalence(McpSchema.Tool),
                registered: () => Array.map(server.tools, ({ tool, annotations }) => [tool.name, annotations] as const),
                add:
                    ({ forward }) =>
                    (tool, annotations) =>
                        server.addTool({ tool, annotations, handle: (given: Record.ReadonlyRecord<string, unknown>) => forward(tool.name, given, (source) => source.call(tool.name, given)) }),
            }),
            kind<McpSchema.Resource>(route, {
                items: Struct.get('resources'),
                key: Struct.get('uri'),
                same: Schema.toEquivalence(McpSchema.Resource),
                registered: () => Array.map(server.resources, ({ resource, annotations }) => [resource.uri, annotations] as const),
                add:
                    ({ forward }) =>
                    (resource, annotations) =>
                        server.addResource({ resource, annotations, handle: Server.orInternalError(forward(resource.uri, { uri: resource.uri }, (source) => source.read(resource.uri))) }),
            }),
            kind<McpSchema.ResourceTemplate>(route, {
                items: Struct.get('templates'),
                key: Struct.get('uriTemplate'),
                same: Schema.toEquivalence(McpSchema.ResourceTemplate),
                registered: () => Array.map(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, annotations] as const),
                add:
                    ({ forward }) =>
                    (template, annotations) => {
                        const { routerPath, names } = compiled(template.uriTemplate);
                        return server.addResourceTemplate({
                            template,
                            annotations,
                            routerPath,
                            completions: Record.fromIterableWith(names, (name) => [
                                name,
                                (value, context) => Server.orInternalError(forward(template.uriTemplate, { uri: template.uriTemplate }, (source) => source.complete({ ref: { type: 'ref/resource', uri: template.uriTemplate }, argument: { name, value }, context }))),
                            ]),
                            handle: (uri) => forward(template.uriTemplate, { uri }, (source) => source.read(uri)),
                        });
                    },
            }),
            kind<McpSchema.Prompt>(route, {
                items: Struct.get('prompts'),
                key: Struct.get('name'),
                same: Schema.toEquivalence(McpSchema.Prompt),
                registered: () => Array.map(server.prompts, ({ prompt, annotations }) => [prompt.name, annotations] as const),
                add:
                    ({ forward }) =>
                    (prompt, annotations) =>
                        server.addPrompt({
                            prompt,
                            annotations,
                            completions: Record.fromIterableWith(prompt.arguments ?? [], ({ name }) => [name, (value, context) => Server.orInternalError(forward(prompt.name, { name: prompt.name }, (source) => source.complete({ ref: { type: 'ref/prompt', name: prompt.name }, argument: { name, value }, context })))]),
                            handle: (given) => forward(prompt.name, given, (source) => source.get(prompt.name, given)),
                        }),
            }),
        ]);
        const [noticed, following] = yield* Effect.all([Queue.unbounded<Noticed>(), FiberMap.make<string>()]);
        const follow = (current: readonly Upstream[]) => {
            const peers = Record.fromIterableWith(current, ({ peer, notices }) => [Link.key(peer), notices] as const);
            return Effect.andThen(
                Effect.forEach(Array.fromIterable(following), ([peer]) => (Record.has(peers, peer) ? Effect.void : FiberMap.remove(following, peer)), { discard: true }),
                Effect.forEach(
                    Record.toEntries(peers),
                    ([peer, notices]) =>
                        FiberMap.run(
                            following,
                            peer,
                            Stream.runForEach(notices, (notice) => Queue.offer(noticed, { peer, ...notice })),
                            { onlyIfMissing: true },
                        ),
                    { discard: true },
                ),
            );
        };
        const registered = (current: readonly Upstream[]) =>
            Effect.andThen(
                Effect.forEach(kinds, (reconcile) => reconcile(current), { discard: true }),
                relist,
            );
        yield* Stream.zipLatestAll(...Array.map(sources, (source) => Stream.concat(Stream.succeed(Array.empty<Upstream>()), source))).pipe(
            Stream.map(Array.flatten),
            Stream.runForEach((current) => Effect.catchCause(Effect.andThen(registered(current), follow(current)), (cause) => Effect.logError('Upstream registration failed', cause))),
            Effect.provideContext(services),
            Effect.forkScoped,
        );
        yield* events({ text: ({ peer, name, target, detail }) => [peer, name, ...Option.toArray(target), ...Option.toArray(Option.map(detail, (json) => JSON.stringify(json)))].join(' '), stream: Stream.fromQueue(noticed) });
    });
    static readonly linked = (links: Pick<Effect.Success<ReturnType<typeof Link.listen<never>>>, 'changes' | 'call'>) => {
        const offered = Effect.fnUntraced(function* (peer: Link) {
            const client = yield* links.call((each) => Link.key(each) === Link.key(peer), Link.key(peer));
            const { tools } = yield* client('tools/list', undefined);
            const unserved = () => Effect.fail(McpSchema.InternalError.make({ message: `${Link.key(peer)} serves tools alone` }));
            return new Upstream({
                peer,
                tools,
                resources: [],
                templates: [],
                prompts: [],
                annotations: Context.empty(),
                notices: Stream.catchCause(client('notices', undefined), (cause) => Stream.drain(Stream.fromEffect(Effect.logInfo(`${Link.key(peer)} sends no more notices`, cause)))),
                call: (name, given) => Effect.flatMap(McpSchema.McpRequestContext, (request) => Effect.mapError(client('tools/call', { name, arguments: given, context: Struct.pick(request, ['clientId', 'protocolVersion', 'clientCapabilities']), _meta: request.requestMetadata }), failed)),
                read: unserved,
                get: unserved,
                complete: unserved,
            });
        });
        return Stream.mapEffect(links.changes, (peers) =>
            Effect.flatMap(Effect.partition(peers, offered, { concurrency: 'unbounded' }), ([served, refused]) =>
                Effect.as(
                    Effect.forEach(refused, (cause) => Effect.logError('Linked extension listed no tools', cause), { discard: true }),
                    served,
                ),
            ),
        );
    };
    static readonly mcp = <E extends Error, R>({
        peer,
        enabled,
        filters = {},
        prefix,
        endpoint,
        presence = Stream.succeed(true),
        poll,
        annotations = Context.empty(),
    }: {
        readonly peer: Link;
        readonly enabled: boolean;
        readonly filters?: { readonly tools?: Filter; readonly resources?: Filter; readonly templates?: Filter; readonly prompts?: Filter };
        readonly prefix?: string;
        readonly endpoint: Effect.Effect<{ readonly url: URL; readonly headers: Readonly<Record<string, string>> }, E, R>;
        readonly presence?: Stream.Stream<boolean>;
        readonly poll: Duration.Duration;
        readonly annotations?: Context.Context<never>;
    }): Stream.Stream<readonly Upstream[], never, R | HttpClient.HttpClient | SecureRandom.SecureRandom> => {
        const named = (name: string) => (prefix === undefined ? name : `${prefix}${name}`);
        const opened = (at: Effect.Success<typeof endpoint>, protocol: McpProtocol.ProtocolAdapter<McpClient.ProtocolVersion>) => Effect.all({ client: McpClient.make(IDENTITY), transport: McpClient.Transport }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ ...at, protocol })));
        const connect = Effect.flatMap(endpoint, (at) => Effect.mapError(Effect.flip(Effect.forEach(PROTOCOLS, (protocol) => Effect.flip(opened(at, protocol)))), (failures) => new Cause.UnknownError(failures, `${Link.key(peer)} answered no MCP protocol: ${Array.map(failures, Struct.get('message')).join('; ')}`)));
        const connected = Effect.gen(function* () {
            const [connections, services] = yield* Effect.all([Pool.make({ acquire: connect, size: 1, concurrency: Number.POSITIVE_INFINITY }), Effect.context<SecureRandom.SecureRandom>()]);
            const attempt = Effect.fnUntraced(function* <A, E2>(use: (connection: Parameters<typeof listed>[0]) => Effect.Effect<A, E2, SecureRandom.SecureRandom>) {
                const connection = yield* Pool.get(connections);
                return yield* Effect.tapError(use(connection), (error) => (Option.isSome(lost(error)) ? Pool.invalidate(connections, connection) : Effect.void));
            }, Effect.scoped);
            const run = <A, E2>(use: (connection: Parameters<typeof listed>[0]) => Effect.Effect<A, E2, SecureRandom.SecureRandom>) =>
                Effect.catchIf(attempt(use), flow(lost, Option.contains(true)), Function.constant(attempt(use))).pipe(Effect.updateContext((request: Context.Context<never>) => Context.merge(services, request)));
            const served = (listing: typeof Listing.Type) => {
                const [tools, prompts] = [
                    Record.fromIterableBy(
                        Array.filter(listing.tools, ({ name }) => admits(name, filters.tools)),
                        (tool) => named(tool.name),
                    ),
                    Record.fromIterableBy(
                        Array.filter(listing.prompts, ({ name }) => admits(name, filters.prompts)),
                        (prompt) => named(prompt.name),
                    ),
                ];
                const found = <A>(rows: Record.ReadonlyRecord<string, A>, name: string) => Effect.fromOption(Record.get(rows, name), () => McpSchema.InvalidParams.make({ message: `${Link.key(peer)} lists no ${name}` }));
                return new Upstream({
                    peer,
                    annotations,
                    notices: Stream.empty,
                    tools: Record.collect(tools, (name, tool) => McpSchema.Tool.make({ ...tool, name })),
                    resources: Array.filter(listing.resources, ({ uri }) => admits(uri, filters.resources)),
                    templates: Array.filter(listing.templates, ({ uriTemplate }) => admits(uriTemplate, filters.templates)),
                    prompts: Record.collect(prompts, (name, prompt) => McpSchema.Prompt.make({ ...prompt, name })),
                    call: (name, given) => Effect.flatMap(found(tools, name), (tool) => run(({ client }) => client.callTool({ tool, arguments: given })).pipe(Effect.mapError(failed))),
                    read: (uri) => run(({ client }) => client.readResource({ uri })).pipe(Effect.mapError(failed)),
                    get: (name, given) => Effect.flatMap(found(prompts, name), (prompt) => run(({ client }) => client.getPrompt({ prompt, arguments: given })).pipe(Effect.mapError(failed))),
                    complete: (payload) => Server.orInternalError(run(({ client, transport }) => (client.capabilities.completions === undefined ? Effect.succeed(McpSchema.CompleteResult.empty) : ask(transport, McpSchema.Complete, payload)))),
                });
            };
            return Stream.fromEffectSchedule(Effect.result(Effect.sandbox(run(listed))), Schedule.spaced(poll)).pipe(
                Stream.changesWith(Result.makeEquivalence(Schema.toEquivalence(Listing), Function.constTrue)),
                Stream.mapEffect(
                    Result.match({
                        onFailure: (cause) => Effect.as(Effect.logInfo(`${Link.key(peer)} is unreachable`, cause), []),
                        onSuccess: (listing) => Effect.as(Effect.logInfo(`${Link.key(peer)} is reachable`), [served(listing)]),
                    }),
                ),
            );
        });
        return enabled
            ? presence.pipe(
                  Stream.changes,
                  Stream.switchMap((present) => Stream.concat(Stream.succeed(Array.empty<Upstream>()), present ? Stream.unwrap(connected) : Stream.empty)),
              )
            : Stream.succeed([]);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const listed = ({ client, transport }: { readonly client: McpClient.McpClient; readonly transport: McpClient.Transport }) =>
    Effect.all({
        tools: client.listTools(),
        resources: client.listResources(),
        templates:
            client.capabilities.resources === undefined
                ? Effect.succeed([])
                : Stream.runCollect(Stream.paginate({}, (params: { readonly cursor?: string }) => Effect.map(ask(transport, McpSchema.ListResourceTemplates, params), ({ resourceTemplates, nextCursor }) => [resourceTemplates, Option.map(Option.fromUndefinedOr(nextCursor), (cursor) => ({ cursor }))] as const))),
        prompts: client.listPrompts(),
    });
const admits = (key: string, { allow, deny = [] }: Filter = {}) => {
    const hits = (pattern: Pattern) => (Predicate.isString(pattern) ? pattern === key : pattern.test(key));
    return (allow?.some(hits) ?? true) && !deny.some(hits);
};
const lost = (error: unknown) =>
    Option.flatMap(Option.liftPredicate(error, McpClientError.isMcpClientError), ({ reason }) =>
        Match.value(reason).pipe(
            Match.when({ _tag: 'ClosedError' }, ({ sessionExpired }) => sessionExpired === true),
            Match.when({ _tag: 'TransportError' }, Function.constFalse),
            Match.when({ _tag: 'HttpError', status: (status: number) => status === 401 || status === 403 }, Function.constTrue),
            Match.option,
        ),
    );
const failed = Match.type<{ readonly message: string }>().pipe(
    Match.when(Schema.is(McpSchema.InvalidParams), (invalid) => invalid),
    Match.when(
        (error) => McpClientError.isMcpClientError(error) && Predicate.isTagged(error.reason, 'ProtocolError') && error.reason.code === McpSchema.INVALID_PARAMS_ERROR_CODE,
        ({ message }) => McpSchema.InvalidParams.make({ message }),
    ),
    Match.orElse(({ message }) => McpSchema.InternalError.make({ message })),
);
const compiled = (template: string) => {
    const [, parts] = Array.mapAccum(template.matchAll(/\{(?<operator>[#&+./;?]?)(?<names>[^}]+)\}|[^{]+/gu).toArray(), 0, (count, [text, operator = '', names]) =>
        names === undefined ? [count, { path: text.replaceAll(':', '::'), names: [] }] : [count + 1, { path: Option.getOrElse(Record.get<string, string>({ '+': '*', '#': '*', '?': '', '&': '' }, operator), () => `:${count}`), names: Array.map(names.split(','), (name) => name.replace(/\*$|:\d+$/u, '')) }],
    );
    return { routerPath: Array.map(parts, Struct.get('path')).join(''), names: Array.flatMap(parts, Struct.get('names')) };
};
const ask = Effect.fnUntraced(function* <S extends Schema.Top>(transport: McpClient.Transport, rpc: { readonly _tag: string; readonly successSchema: S }, params: object) {
    const invalid = (cause: unknown) => McpClientError.McpClientError.make({ reason: McpClientError.ValidationError.make({ description: `MCP ${rpc._tag} payload or result failed to decode`, cause }) });
    const declared = yield* Effect.fromOption(Option.filter(Option.fromUndefinedOr(transport.protocol.clientRpcs.requests.get(rpc._tag)), Rpc.isRpc), () =>
        McpClientError.McpClientError.make({ reason: McpClientError.UnsupportedError.make({ description: `MCP ${transport.protocol.protocolVersion} declares no ${rpc._tag}` }) }),
    );
    const [codecs, meta] = [transport.protocol.payloadCodecs(declared), { 'io.modelcontextprotocol/protocolVersion': transport.protocol.protocolVersion, 'io.modelcontextprotocol/clientInfo': IDENTITY, 'io.modelcontextprotocol/clientCapabilities': {} }];
    const payload = yield* codecs.decode(Predicate.isTagged(transport.protocol.runtime, 'Stateless') ? { ...params, _meta: meta } : params).pipe(Effect.flatMap(codecs.encode), Effect.filterOrFail(Predicate.isReadonlyObject), Effect.mapError(invalid));
    const { result } = yield* transport.request({ jsonrpc: '2.0', id: yield* SecureRandom.SecureRandom.use((random) => random.uuidV7()), method: rpc._tag, params: payload }).pipe(
        Effect.filterOrFail(
            (response) => response.error === undefined,
            ({ error }) => McpClientError.McpClientError.make({ reason: McpClientError.ProtocolError.make({ code: error?.code, data: error?.data, description: error?.message }) }),
        ),
    );
    return yield* Effect.mapError(Schema.decodeUnknownEffect(rpc.successSchema)(result), invalid);
});
const kind = Effect.fnUntraced(function* <A>(route: Route, { items, key, same, registered, add }: Kind<A>) {
    const [shown, routes] = yield* Effect.all([Ref.make<Record.ReadonlyRecord<string, { readonly item: A; readonly annotations: Context.Context<never> }>>({}), Ref.make<Record.ReadonlyRecord<string, Array.NonEmptyReadonlyArray<Upstream>>>({})]);
    const register = add({
        forward: Effect.fnUntraced(function* <B, E, R>(at: string, given: Record.ReadonlyRecord<string, unknown>, use: (source: Upstream) => Effect.Effect<B, E, R>) {
            const rows = yield* Effect.fromOption(Record.get(yield* Ref.get(routes), at), () => McpSchema.InternalError.make({ message: `${at} has no reachable upstream` }));
            const peers = Array.map(rows, Struct.get('peer'));
            const chosen = yield* Array.isReadonlyArrayNonEmpty(Array.tailNonEmpty(peers)) ? route(peers, given) : Effect.succeed(Array.headNonEmpty(peers));
            const source = yield* Effect.fromOption(
                Array.findFirst(rows, ({ peer }) => Link.key(peer) === Link.key(chosen)),
                () => McpSchema.InvalidParams.make({ message: `${Link.key(chosen)} serves no ${at}, ${Array.map(peers, Link.key).join(', ')} do` }),
            );
            return yield* use(source);
        }),
    });
    return Effect.fnUntraced(function* (sources: readonly Upstream[]) {
        const previous = yield* Ref.get(shown);
        const owned = new Set(Array.filterMap(registered(), ([at, annotations]) => (previous[at]?.annotations === annotations ? Result.failVoid : Result.succeed(at))));
        const [byKey, shadowed] = Record.partition(
            Array.groupBy(
                Array.flatMap(sources, (source) => Array.cartesian([source], items(source))),
                ([, item]) => key(item),
            ),
            (rows, at) => (owned.has(at) ? Result.fail(rows) : Result.succeed(rows)),
        );
        yield* Record.isEmptyRecord(shadowed) ? Effect.void : Effect.logError(`Upstream ${Record.keys(shadowed).join(', ')} share a key with the server's own registrations and stay unregistered`);
        const fresh = Record.filterMap(byKey, ([[source, item]], at) =>
            Option.containsWith(same)(Option.map(Record.get(previous, at), Struct.get('item')), item) ? Result.failVoid : Result.succeed({ item, annotations: Context.merge(source.annotations, Context.make(Server.Visible, Effect.map(Ref.get(routes), Record.has(at)))) }),
        );
        yield* Effect.all([Ref.set(routes, Record.map(byKey, Array.map(Tuple.get(0)))), Ref.update(shown, (kept) => ({ ...kept, ...fresh }))]);
        yield* Effect.forEach(Object.values(fresh), ({ item, annotations }) => register(item, annotations), { discard: true });
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Upstream };
