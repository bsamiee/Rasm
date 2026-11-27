import manifest from '@rasm/mcp/package.json' with { type: 'json' };
import { Array, Cause, Context, Data, type Duration, Effect, FiberMap, Function, Match, Option, Pool, Predicate, pipe, Queue, Record, Ref, Result, Schedule, Schema, Stream, Struct, Tuple } from 'effect';
import { McpClient, McpClientError, McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import type { HttpClient } from 'effect/http';
import { Rpc } from 'effect/rpc';
import { Link } from '#/link';
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
        readonly events: Stream.Stream<typeof Link.Event.Type>;
        readonly call: (name: string, given: Record.ReadonlyRecord<string, unknown>) => Effect.Effect<McpSchema.CallToolResult, McpSchema.InternalError | McpSchema.InvalidParams, McpSchema.McpRequestContext>;
        readonly read: (uri: string) => Effect.Effect<McpSchema.ReadResourceResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly get: (name: string, given: Record.ReadonlyRecord<string, string>) => Effect.Effect<McpSchema.GetPromptResult, McpSchema.InternalError | McpSchema.InvalidParams>;
        readonly complete: (payload: Omit<typeof McpSchema.Complete.payloadSchema.Type, 'context'> & { readonly context: (typeof McpSchema.Complete.payloadSchema.Type)['context'] }) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError>;
    }
> {
    static readonly serve = Effect.fnUntraced(function* <R>(sources: Array.NonEmptyReadonlyArray<Stream.Stream<readonly Upstream[], never, R>>, { route }: { readonly route: Route }) {
        const [server, { notifyListChanged, events }] = yield* Effect.all([McpServer.McpServer, Server]);
        const reconcilers = yield* Effect.all([
            register<McpSchema.Tool>(route, {
                items: Struct.get('tools'),
                key: Struct.get('name'),
                registered: () => Array.map(server.tools, ({ tool, annotations }) => [tool.name, annotations] as const),
                add: (forward, tool, annotations) => server.addTool({ tool, annotations, handle: (given: Record.ReadonlyRecord<string, unknown>) => forward(tool.name, given, (source) => source.call(tool.name, given)) }),
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
        const [eventQueue, following] = yield* Effect.all([Queue.unbounded<Stream.Success<Upstream['events']> & { readonly peer: string }>(), FiberMap.make<string>()]);
        const reconcile = (current: readonly Upstream[]) => {
            const peers = Record.fromIterableWith(current, ({ peer, events: stream }) => [Link.key(peer), stream] as const);
            return Effect.forEach(reconcilers, (apply) => apply(current), { discard: true }).pipe(
                Effect.andThen(notifyListChanged),
                Effect.andThen(Effect.forEach(Array.fromIterable(following), ([peer]) => (Record.has(peers, peer) ? Effect.void : FiberMap.remove(following, peer)), { discard: true })),
                Effect.andThen(Effect.forEach(Record.toEntries(peers), ([peer, stream]) => Stream.runForEach(stream, (event) => Queue.offer(eventQueue, { peer, ...event })).pipe(FiberMap.run(following, peer, { onlyIfMissing: true })), { discard: true })),
                Effect.catchCause((cause) => Effect.logError('Upstream registration failed', cause)),
            );
        };
        yield* Stream.zipLatestAll(...Array.map(sources, Stream.prepend([Array.empty<Upstream>()]))).pipe(Stream.map(Array.flatten), Stream.runForEach(reconcile), Effect.forkScoped);
        yield* events({ stream: Stream.fromQueue(eventQueue), render: ({ peer, name, target, detail }) => Effect.succeed([peer, name, ...Option.toArray(target), ...Option.toArray(Option.map(detail, (json) => JSON.stringify(json)))].join(' ')) });
    });
    static readonly fromLinks = (links: Pick<Effect.Success<ReturnType<typeof Link.listen<never>>>, 'changes' | 'call'>) => {
        const connect = Effect.fnUntraced(function* (peer: Link) {
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
                events: Stream.catchCause(client('events', undefined), (cause) => Stream.fromEffectDrain(Effect.logInfo(`${Link.key(peer)} sends no more events`, cause))),
                call: (name, given) => Effect.flatMap(McpSchema.McpRequestContext, (request) => Effect.mapError(client('tools/call', { name, arguments: given, context: Struct.pick(request, ['clientId', 'protocolVersion', 'clientCapabilities']), _meta: request.requestMetadata }), toMcpError)),
                read: unserved,
                get: unserved,
                complete: unserved,
            });
        });
        return Stream.mapEffect(links.changes, (peers) =>
            Effect.partition(peers, connect, { concurrency: 'unbounded' }).pipe(
                Effect.tap(([, refused]) => Effect.forEach(refused, (cause) => Effect.logError('Linked plugin listed no tools', cause), { discard: true })),
                Effect.map(([upstreams]) => upstreams),
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
        const connect = Effect.flatMap(endpoint, (at) =>
            Effect.mapError(
                Effect.flip(Effect.forEach(PROTOCOLS, (protocol) => Effect.flip(Effect.all({ client: McpClient.make(CLIENT_INFO), transport: McpClient.Transport }).pipe(Effect.provideServiceEffect(McpClient.Transport, McpClient.http({ ...at, protocol })))))),
                (failures) => new Cause.UnknownError(failures, `${Link.key(peer)} answered no MCP protocol: ${Array.map(failures, Struct.get('message')).join('; ')}`),
            ),
        );
        const connected = Effect.gen(function* () {
            const [connections, random] = yield* Effect.all([Pool.make({ acquire: connect, size: 1, concurrency: Number.POSITIVE_INFINITY }), SecureRandom.SecureRandom]);
            const run = <A>(use: (connection: Parameters<typeof list>[0]) => Effect.Effect<A, McpClientError.McpClientError, SecureRandom.SecureRandom>) =>
                Pool.get(connections).pipe(
                    Effect.flatMap((connection) => Effect.tapError(use(connection), ({ reason }) => (reason.pipe(disconnects, Option.isSome) ? Pool.invalidate(connections, connection) : Effect.void))),
                    Effect.scoped,
                    Effect.retry({ times: 1, while: (error) => McpClientError.isMcpClientError(error) && Option.contains(disconnects(error.reason), true) }),
                    Effect.provideService(SecureRandom.SecureRandom, random),
                );
            const toUpstream = (listing: Listing) => {
                const [tools, prompts] = [expose(listing.tools, filters.tools), expose(listing.prompts, filters.prompts)];
                const found = <A>(rows: Record.ReadonlyRecord<string, A>, name: string) => Effect.fromOption(Record.get(rows, name), () => McpSchema.InvalidParams.make({ message: `${Link.key(peer)} lists no ${name}` }));
                return new Upstream({
                    peer,
                    annotations,
                    events: Stream.empty,
                    tools: Record.collect(tools, (name, tool) => McpSchema.Tool.make({ ...tool, name })),
                    resources: Array.filter(listing.resources, ({ uri }) => allows(uri, filters.resources)),
                    templates: Array.filter(listing.templates, ({ uriTemplate }) => allows(uriTemplate, filters.templates)),
                    prompts: Record.collect(prompts, (name, prompt) => McpSchema.Prompt.make({ ...prompt, name })),
                    call: (name, given) => Effect.flatMap(found(tools, name), (tool) => run(({ client }) => client.callTool({ tool, arguments: given })).pipe(Effect.mapError(toMcpError))),
                    read: (uri) => run(({ client }) => client.readResource({ uri })).pipe(Effect.mapError(toMcpError)),
                    get: (name, given) => Effect.flatMap(found(prompts, name), (prompt) => run(({ client }) => client.getPrompt({ prompt, arguments: given })).pipe(Effect.mapError(toMcpError))),
                    complete: (payload) => Server.orInternalError(run(({ client, transport }) => (client.capabilities.completions === undefined ? Effect.succeed(McpSchema.CompleteResult.empty) : request(transport, McpSchema.Complete, payload)))),
                });
            };
            return Stream.fromEffectSchedule(run(list).pipe(Effect.sandbox, Effect.result), Schedule.spaced(poll)).pipe(
                Stream.changesWith(Result.makeEquivalence(Schema.toEquivalence(Listing), Function.constTrue)),
                Stream.mapEffect(
                    Result.match({
                        onFailure: (cause) => Effect.as(Effect.logInfo(`${Link.key(peer)} is unreachable`, cause), []),
                        onSuccess: (listing) => Effect.as(Effect.logInfo(`${Link.key(peer)} is reachable`), [toUpstream(listing)]),
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
    Match.tag('TransportError', Function.constFalse),
    Match.when({ _tag: 'HttpError', status: (status: number) => status === 401 || status === 403 }, Function.constTrue),
    Match.option,
);
const toMcpError = Match.type<{ readonly message: string }>().pipe(
    Match.when(Schema.is(McpSchema.InvalidParams), (invalid) => invalid),
    Match.when(
        (error) => McpClientError.isMcpClientError(error) && 'code' in error.reason && error.reason.code === McpSchema.INVALID_PARAMS_ERROR_CODE,
        ({ message }) => McpSchema.InvalidParams.make({ message }),
    ),
    Match.orElse(({ message }) => McpSchema.InternalError.make({ message })),
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
        const rows = yield* Effect.fromOption(Record.get(yield* Ref.get(routes), at), () => McpSchema.InternalError.make({ message: `${at} has no reachable upstream` }));
        const peers = Array.map(rows, Struct.get('peer'));
        const chosen = yield* Array.isReadonlyArrayNonEmpty(Array.tailNonEmpty(peers)) ? route(peers, given) : Effect.succeed(Array.headNonEmpty(peers));
        const source = yield* Effect.fromOption(
            Array.findFirst(rows, ({ peer }) => Link.key(peer) === Link.key(chosen)),
            () => McpSchema.InvalidParams.make({ message: `${Link.key(chosen)} serves no ${at}, ${Array.map(peers, Link.key).join(', ')} do` }),
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
