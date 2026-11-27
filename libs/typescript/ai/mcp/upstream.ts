import { parse } from 'node:path';
import { fileURLToPath } from 'node:url';
import manifest from '@rasm/mcp/package.json' with { type: 'json' };
import { Array, Cause, Context, Data, type Duration, Effect, FiberMap, Function, Match, Option, Pool, Predicate, pipe, Queue, Record, Ref, Result, Schedule, Schema, Stream, Struct, Tuple } from 'effect';
import { McpClient, McpClientError, McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import type { HttpClient } from 'effect/http';
import { Rpc, RpcClientError } from 'effect/rpc';
import { Link } from '#/link';
import { Line, Report } from '#/report';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface Filter {
    readonly allow?: readonly (string | RegExp)[];
    readonly deny?: readonly (string | RegExp)[];
}
type Route = (peers: Array.NonEmptyReadonlyArray<Link>, given: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<Link, McpSchema.InvalidParams>;
interface Primitive<A> {
    readonly items: (source: Upstream) => readonly A[];
    readonly key: (item: A) => string;
    readonly registered: () => readonly (readonly [string, Context.Context<never>])[];
    readonly add: (forward: <B, E, R>(at: string, given: Record.ReadonlyRecord<string, unknown>, use: (source: Upstream) => Effect.Effect<B, E, R>) => Effect.Effect<B, E | McpSchema.InternalError | McpSchema.InvalidParams, R>, item: A, annotations: Context.Context<never>) => Effect.Effect<void>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLIENT_INFO = Struct.pick(manifest, ['name', 'version', 'description']);
const PROTOCOLS = Array.filter(Server.protocols, (adapter) => adapter.protocolVersion !== McpProtocol.v2024_11_05.protocolVersion);

// --- [MODELS] --------------------------------------------------------------------------

class Listing extends Schema.Opaque<Listing>()(Schema.Struct({ tools: Schema.Array(McpSchema.Tool), resources: Schema.Array(McpSchema.Resource), templates: Schema.Array(McpSchema.ResourceTemplate), prompts: Schema.Array(McpSchema.Prompt) })) {}

class Upstream extends Data.Class<
    Listing & {
        readonly peer: Link;
        readonly annotations: Context.Context<never>;
        readonly events: Stream.Stream<typeof Link.Event.Type, Error>;
        readonly call: (name: string, given: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<McpSchema.CallToolResult, McpSchema.InternalError | McpSchema.InvalidParams, McpSchema.McpRequestContext>;
        readonly read: (uri: string) => Effect.Effect<McpSchema.ReadResourceResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly get: (name: string, given: Record.ReadonlyRecord<string, string>) => Effect.Effect<McpSchema.GetPromptResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly complete: (payload: Omit<typeof McpSchema.Complete.payloadSchema.Type, 'context'> & { readonly context: (typeof McpSchema.Complete.payloadSchema.Type)['context'] }) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError | McpSchema.InvalidParams>;
    }
> {
    static readonly serve = Effect.fnUntraced(function* <R>(sources: Array.NonEmptyReadonlyArray<Stream.Stream<readonly Upstream[], never, R>>, { route }: { readonly route: Route }) {
        const [server, { notifyListChanged, events }] = yield* Effect.all([McpServer.McpServer, Server]);
        const reconcilers = yield* Effect.all([
            register<McpSchema.Tool>(route, {
                items: Struct.get('tools'),
                key: Struct.get('name'),
                registered: () => Array.map(server.tools, ({ tool, annotations }) => [tool.name, annotations] as const),
                add: (forward, tool, annotations) =>
                    server.addTool({
                        tool,
                        annotations,
                        handle: (given: Record.ReadonlyRecord<string, unknown>) =>
                            forward(tool.name, given, (source) => source.call(tool.name, given)).pipe(Effect.catchTag('InternalError', ({ message }) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message })] })))),
                    }),
            }),
            register<McpSchema.Resource>(route, {
                items: Struct.get('resources'),
                key: Struct.get('uri'),
                registered: () => Array.map(server.resources, ({ resource, annotations }) => [resource.uri, annotations] as const),
                add: (forward, resource, annotations) => server.addResource({ resource, annotations, handle: Server.orInternalError(forward(resource.uri, { uri: resource.uri }, (source) => source.read(resource.uri))) }),
            }),
            register<McpSchema.ResourceTemplate>(route, {
                items: Struct.get('templates'),
                key: Struct.get('uriTemplate'),
                registered: () => Array.map(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, annotations] as const),
                add: (forward, template, annotations) => {
                    const { routerPath, names } = compileUriTemplate(template.uriTemplate);
                    return server.addResourceTemplate({
                        template,
                        annotations,
                        routerPath,
                        completions: Record.fromIterableWith(names, (name) => [name, (value, context) => Server.orInternalError(forward(template.uriTemplate, { uri: template.uriTemplate }, (source) => source.complete({ ref: { type: 'ref/resource', uri: template.uriTemplate }, argument: { name, value }, context })))]),
                        handle: (uri) => forward(template.uriTemplate, { uri }, (source) => source.read(uri)),
                    });
                },
            }),
            register<McpSchema.Prompt>(route, {
                items: Struct.get('prompts'),
                key: Struct.get('name'),
                registered: () => Array.map(server.prompts, ({ prompt, annotations }) => [prompt.name, annotations] as const),
                add: (forward, prompt, annotations) =>
                    server.addPrompt({
                        prompt,
                        annotations,
                        completions: Record.fromIterableWith(prompt.arguments ?? [], ({ name }) => [name, (value, context) => Server.orInternalError(forward(prompt.name, { name: prompt.name }, (source) => source.complete({ ref: { type: 'ref/prompt', name: prompt.name }, argument: { name, value }, context })))]),
                        handle: (given) => forward(prompt.name, given, (source) => source.get(prompt.name, given)),
                    }),
            }),
        ]);
        const [eventQueue, following, followed] = yield* Effect.all([Queue.unbounded<{ readonly peer: Link; readonly event: Result.Result<typeof Link.Event.Type, Error> }>(), FiberMap.make<string>(), Ref.make<Record.ReadonlyRecord<string, Upstream>>({})]);
        const follow = ({ peer, events: stream }: Upstream) =>
            stream.pipe(
                Stream.tapError((error) => Effect.logWarning(`${Link.key(peer)} stopped sending events`, error)),
                Stream.result,
                Stream.runForEach((event) => Queue.offer(eventQueue, { peer, event })),
            );
        const reconcile = Effect.fnUntraced(
            function* (current: readonly Upstream[]) {
                yield* Effect.forEach(reconcilers, (apply) => apply(current), { discard: true });
                yield* notifyListChanged;
                const now = Record.fromIterableWith(current, (source) => [Link.key(source.peer), source] as const);
                const before = yield* Ref.getAndSet(followed, now);
                yield* Effect.forEach(Record.keys(before), (peer) => (Record.has(now, peer) ? Effect.void : FiberMap.remove(following, peer)), { discard: true });
                yield* Effect.forEach(Record.toEntries(now), ([peer, source]) => (before[peer] === source ? Effect.void : FiberMap.run(following, peer, follow(source))), { discard: true });
            },
            Effect.catchCause((cause) => Effect.logError('Upstream registration failed', cause)),
        );
        yield* Stream.zipLatestAll(...Array.map(sources, Stream.prepend([Array.empty<Upstream>()]))).pipe(Stream.map(Array.flatten), Stream.runForEach(reconcile), Effect.forkScoped);
        yield* events({
            stream: Stream.fromQueue(eventQueue),
            route: ({ peer, event }) =>
                Option.some({
                    push: false,
                    text: Result.match(event, {
                        onSuccess: ({ name, target, detail }) => Report.text(new Report({ lines: [new Line({ head: [label(peer), name, ...Option.toArray(target)], rows: Option.match(detail, { onNone: Array.empty, onSome: (held) => Report.of(held).lines }) })] })),
                        onFailure: ({ message }) => `${label(peer)} stopped sending events: ${message}`,
                    }),
                }),
        });
    });
    static readonly fromLinks = (links: Pick<Effect.Success<ReturnType<typeof Link.listen<never>>>, 'changes' | 'call'>) => {
        const connect = Effect.fnUntraced(function* (peer: Link) {
            const client = yield* links.call((each) => each === peer, Link.key(peer));
            const { tools } = yield* client('tools/list', undefined);
            const unserved = () => McpSchema.InternalError.make({ message: `${label(peer)} serves tools alone` });
            return new Upstream({
                peer,
                tools,
                resources: [],
                templates: [],
                prompts: [],
                annotations: Context.empty(),
                events: client('events', undefined).pipe(Stream.catchIf(Schema.is(RpcClientError.RpcClientError), (error) => Stream.fromEffectDrain(Effect.logInfo(`${Link.key(peer)} sends no more events`, error)))),
                call: (name, given) =>
                    Effect.flatMap(McpSchema.McpRequestContext, (request) => client('tools/call', { name, arguments: given, context: Struct.pick(request, ['clientId', 'protocolVersion', 'clientCapabilities']), _meta: request.requestMetadata })).pipe(
                        Effect.mapError(refuse(`${label(peer)} is not linked, call again once ${named(peer)} runs`)),
                    ),
                read: unserved,
                get: unserved,
                complete: unserved,
            });
        });
        return links.changes.pipe(
            Stream.mapAccumEffect(
                (): ReadonlyMap<Link, Upstream> => new Map(),
                (held, peers) =>
                    pipe(
                        Array.filter(peers, (peer) => !held.has(peer)),
                        Effect.partition(connect, { concurrency: 'unbounded' }),
                        Effect.tap(([, refused]) => Effect.forEach(refused, (cause) => Effect.logError('Linked plugin listed no tools', cause), { discard: true })),
                        Effect.map(([joined]) => {
                            const current: ReadonlyMap<Link, Upstream> = new Map(Array.map([...peers.flatMap((peer) => Array.fromNullishOr(held.get(peer))), ...joined], (source) => [source.peer, source] as const));
                            return [current, [current.values().toArray()]] as const;
                        }),
                    ),
            ),
        );
    };
    static readonly fromHttp = <E extends Error, R>({
        peer,
        filters = {},
        prefix = '',
        endpoint,
        presence = Stream.succeed(true),
        poll,
        annotations = Context.empty(),
    }: {
        readonly peer: Link;
        readonly filters?: { readonly tools?: Filter; readonly resources?: Filter; readonly templates?: Filter; readonly prompts?: Filter };
        readonly prefix?: string;
        readonly endpoint: Effect.Effect<{ readonly url: URL; readonly headers: Readonly<Record<string, string>> }, E, R>;
        readonly presence?: Stream.Stream<boolean, never, R>;
        readonly poll: Duration.Duration;
        readonly annotations?: Context.Context<never>;
    }): Stream.Stream<readonly Upstream[], never, R | HttpClient.HttpClient | SecureRandom.SecureRandom> => {
        const expose = <A extends { readonly name: string }>(items: readonly A[], filter: Filter | undefined) => Record.fromEntries(Array.filterMap(items, (item) => (allows(item.name, filter) ? Result.succeed([`${prefix}${item.name}`, item] as const) : Result.failVoid)));
        const refused = refuse(`${named(peer)} MCP server is not answering, open ${named(peer)} if it is closed and call again`);
        const connect = Effect.flatMap(endpoint, (at) =>
            Effect.mapError(
                Effect.flip(Effect.forEach(PROTOCOLS, (protocol) => Effect.flip(Effect.all({ client: McpClient.make(CLIENT_INFO), transport: McpClient.Transport }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ ...at, protocol })))))),
                (failures) => new Cause.UnknownError(failures, `${Link.key(peer)} answered no MCP protocol: ${Array.map(failures, Struct.get('message')).join('; ')}`),
            ),
        );
        const connected = Effect.gen(function* () {
            const [connections, services] = yield* Effect.all([Pool.make({ acquire: connect, size: 1, concurrency: Number.POSITIVE_INFINITY }), Effect.context<SecureRandom.SecureRandom>()]);
            const run = <A>(use: (connection: Parameters<typeof list>[0]) => Effect.Effect<A, McpClientError.McpClientError, SecureRandom.SecureRandom>) =>
                Pool.get(connections).pipe(
                    Effect.flatMap((connection) => Effect.tapError(use(connection), ({ reason }) => (reason.pipe(disconnects, Option.isSome) ? Pool.invalidate(connections, connection) : Effect.void))),
                    Effect.scoped,
                    Effect.retry({ times: 1, while: (error) => McpClientError.isMcpClientError(error) && Option.contains(disconnects(error.reason), true) }),
                    Effect.updateContext((request: Context.Context<never>) => Context.merge(services, request)),
                );
            const toUpstream = (listing: Listing) => {
                const [tools, prompts] = [expose(listing.tools, filters.tools), expose(listing.prompts, filters.prompts)];
                const found = <A>(rows: Record.ReadonlyRecord<string, A>, name: string) => Effect.fromOption(Record.get(rows, name), () => McpSchema.InvalidParams.make({ message: `${named(peer)} MCP server lists no ${name}` }));
                return new Upstream({
                    peer,
                    annotations,
                    events: Stream.empty,
                    tools: Record.collect(tools, (name, tool) => McpSchema.Tool.make({ ...tool, name })),
                    resources: Array.filter(listing.resources, ({ uri }) => allows(uri, filters.resources)),
                    templates: Array.filter(listing.templates, ({ uriTemplate }) => allows(uriTemplate, filters.templates)),
                    prompts: Record.collect(prompts, (name, prompt) => McpSchema.Prompt.make({ ...prompt, name })),
                    call: (name, given) => Effect.flatMap(found(tools, name), (tool) => run(({ client }) => client.callTool({ tool, arguments: given })).pipe(Effect.mapError(refused))),
                    read: (uri) => run(({ client }) => client.readResource({ uri })).pipe(Effect.mapError(refused)),
                    get: (name, given) => Effect.flatMap(found(prompts, name), (prompt) => run(({ client }) => client.getPrompt({ prompt, arguments: given })).pipe(Effect.mapError(refused))),
                    complete: (payload) => run(({ client, transport }) => (client.capabilities.completions === undefined ? Effect.succeed(McpSchema.CompleteResult.empty) : request(transport, McpSchema.Complete, payload))).pipe(Effect.mapError(refused)),
                });
            };
            return Stream.fromEffectSchedule(run(list).pipe(Effect.sandbox, Effect.result), Schedule.spaced(poll)).pipe(
                Stream.changesWith(Result.makeEquivalence(Schema.toEquivalence(Listing), Function.constTrue)),
                Stream.flatMap(
                    Result.match({
                        onFailure: (cause) => Stream.fromEffectDrain(Effect.logWarning(`${Link.key(peer)} is not answering, its last listing stays offered`, cause)),
                        onSuccess: (listing) => Stream.fromEffect(Effect.as(Effect.logInfo(`${Link.key(peer)} is answering`), [toUpstream(listing)])),
                    }),
                ),
            );
        });
        return presence.pipe(
            Stream.changes,
            Stream.switchMap((present) => Stream.prepend(present ? Stream.unwrap(connected) : Stream.empty, [Array.empty<Upstream>()])),
        );
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const named = ({ application }: Link) => (Predicate.isString(application) ? application : parse(fileURLToPath(application)).name);
const label = (peer: Link) => `${peer.plugin} in ${named(peer)}`;
const list = ({ client, transport }: { readonly client: McpClient.McpClient; readonly transport: McpClient.Transport }) =>
    Effect.all({
        tools: client.listTools(),
        resources: client.listResources(),
        templates:
            client.capabilities.resources === undefined
                ? Effect.succeed([])
                : Stream.runCollect(Stream.paginate({}, (params: { readonly cursor?: string }) => Effect.map(request(transport, McpSchema.ListResourceTemplates, params), ({ resourceTemplates, nextCursor }) => [resourceTemplates, Option.map(Option.fromUndefinedOr(nextCursor), (cursor) => ({ cursor }))] as const))),
        prompts: client.listPrompts(),
    });
const allows = (key: string, { allow, deny = [] }: Filter = {}) => {
    const hits = (pattern: string | RegExp) => (Predicate.isString(pattern) ? pattern === key : pattern.test(key));
    return (allow?.some(hits) ?? true) && !deny.some(hits);
};
const disconnects = Match.type<McpClientError.McpClientErrorReason>().pipe(
    Match.tag('ClosedError', ({ sessionExpired }) => sessionExpired === true),
    Match.tag('TransportError', 'TimeoutError', Function.constFalse),
    Match.when({ _tag: 'HttpError', status: (status: number) => status === 401 || status === 403 }, Function.constTrue),
    Match.when({ _tag: 'HttpError', status: (status: number) => status === 404 || status >= 500 }, Function.constFalse),
    Match.option,
);
const refuse =
    (unanswered: string) =>
    (error: { readonly message: string }): McpSchema.InvalidParams | McpSchema.InternalError =>
        Match.value(error).pipe(
            Match.when(Schema.is(McpSchema.InvalidParams), (invalid) => invalid),
            Match.when(Schema.is(McpSchema.McpError), ({ message }) => McpSchema.InternalError.make({ message })),
            Match.when(McpClientError.isMcpClientError, ({ reason }) =>
                Match.value(reason).pipe(
                    Match.when({ code: McpSchema.INVALID_PARAMS_ERROR_CODE }, ({ description, message }) => McpSchema.InvalidParams.make({ message: description ?? message })),
                    Match.when(
                        (each) => each.pipe(disconnects, Option.isSome),
                        () => McpSchema.InternalError.make({ message: unanswered }),
                    ),
                    Match.orElse(({ message }) => McpSchema.InternalError.make({ message })),
                ),
            ),
            Match.orElse(() => McpSchema.InternalError.make({ message: unanswered })),
        );
const compileUriTemplate = (template: string) => {
    const [, parts] = Array.mapAccum(template.matchAll(/\{(?<operator>[#&+./;?]?)(?<names>[^}]+)\}|[^{]+/gu).toArray(), 0, (count, [text, operator = '', names]) =>
        names === undefined ? [count, { path: text.replaceAll(':', '::'), names: [] }] : [count + 1, { path: Option.getOrElse(Record.get<string, string>({ '+': '*', '#': '*', '?': '', '&': '' }, operator), () => `:${count}`), names: Array.map(names.split(','), (name) => name.replace(/\*$|:\d+$/u, '')) }],
    );
    return { routerPath: Array.map(parts, Struct.get('path')).join(''), names: Array.flatMap(parts, Struct.get('names')) };
};
const request = Effect.fnUntraced(function* <S extends Schema.Top>(transport: McpClient.Transport, rpc: { readonly _tag: string; readonly successSchema: S }, params: object) {
    const invalid = (cause: unknown) => McpClientError.McpClientError.make({ reason: McpClientError.ValidationError.make({ description: `Invalid MCP ${rpc._tag} payload or result`, cause }) });
    const declared = yield* Effect.fromOption(Option.filter(Option.fromUndefinedOr(transport.protocol.clientRpcs.requests.get(rpc._tag)), Rpc.isRpc), () =>
        McpClientError.McpClientError.make({ reason: McpClientError.UnsupportedError.make({ description: `MCP ${transport.protocol.protocolVersion} declares no ${rpc._tag}` }) }),
    );
    const [codecs, meta] = [transport.protocol.payloadCodecs(declared), { 'io.modelcontextprotocol/protocolVersion': transport.protocol.protocolVersion, 'io.modelcontextprotocol/clientInfo': CLIENT_INFO, 'io.modelcontextprotocol/clientCapabilities': {} }];
    const payload = yield* codecs.decode(Predicate.isTagged(transport.protocol.runtime, 'Stateless') ? { ...params, _meta: meta } : params).pipe(Effect.flatMap(codecs.encode), Effect.filterOrFail(Predicate.isReadonlyObject), Effect.mapError(invalid));
    const response = yield* transport.request({ jsonrpc: '2.0', id: yield* SecureRandom.SecureRandom.use((random) => random.uuidV7()), method: rpc._tag, params: payload });
    return yield* 'error' in response ? McpClientError.McpClientError.make({ reason: McpClientError.ProtocolError.make({ code: response.error.code, data: response.error.data, description: response.error.message }) }) : Effect.mapError(Schema.decodeUnknownEffect(rpc.successSchema)(response.result), invalid);
});
const register = Effect.fnUntraced(function* <A>(route: Route, { items, key, registered, add }: Primitive<A>) {
    const [shown, routes] = yield* Effect.all([Ref.make<Record.ReadonlyRecord<string, { readonly item: A; readonly annotations: Context.Context<never> }>>({}), Ref.make<Record.ReadonlyRecord<string, Array.NonEmptyReadonlyArray<Upstream>>>({})]);
    const forward = Effect.fnUntraced(function* <B, E, R>(at: string, given: Record.ReadonlyRecord<string, unknown>, use: (source: Upstream) => Effect.Effect<B, E, R>) {
        const rows = yield* Effect.fromOption(Record.get(yield* Ref.get(routes), at), () => McpSchema.InternalError.make({ message: `No upstream serves ${at} now, call again once its application runs` }));
        const peers = Array.map(rows, Struct.get('peer'));
        const chosen = yield* Array.isReadonlyArrayNonEmpty(Array.tailNonEmpty(peers)) ? route(peers, given) : Effect.succeed(Array.headNonEmpty(peers));
        const source = yield* Effect.fromOption(
            Array.findFirst(rows, ({ peer }) => Link.key(peer) === Link.key(chosen)),
            () => McpSchema.InvalidParams.make({ message: `${label(chosen)} serves no ${at}, ${Array.map(peers, label).join(', ')} do` }),
        );
        return yield* use(source);
    });
    return Effect.fnUntraced(function* (sources: readonly Upstream[]) {
        const previous = yield* Ref.get(shown);
        const owned = new Set(Array.filterMap(registered(), ([at, annotations]) => (previous[at]?.annotations === annotations ? Result.failVoid : Result.succeed(at))));
        const [byKey, shadowed] = pipe(
            Array.flatMap(sources, (source) => Array.cartesian([source], items(source))),
            Array.groupBy(([, item]) => key(item)),
            Record.partition((rows, at) => (owned.has(at) ? Result.fail(rows) : Result.succeed(rows))),
        );
        yield* Record.isEmptyRecord(shadowed) ? Effect.void : Effect.logError(`Upstream ${Record.keys(shadowed).join(', ')} share a key with the server's own registrations and stay unregistered`);
        const fresh = Record.filterMap(byKey, ([[source, item]], at) =>
            Option.contains(Option.map(Record.get(previous, at), Struct.get('item')), item) ? Result.failVoid : Result.succeed({ item, annotations: Context.merge(source.annotations, Context.make(Server.Visibility, Effect.map(Ref.get(routes), Record.has(at)))) }),
        );
        yield* Effect.all([Ref.set(routes, Record.map(byKey, Array.map(Tuple.get(0)))), Ref.update(shown, (kept) => ({ ...kept, ...fresh }))]);
        yield* Effect.forEach(Record.values(fresh), ({ item, annotations }) => add(forward, item, annotations), { discard: true });
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Upstream };
