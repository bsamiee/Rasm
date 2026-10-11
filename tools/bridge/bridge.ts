// biome-ignore-all lint/correctness/noNodejsModules: NodeHttpServer.layerServer consumes the native Node HTTP server factory, and Node owns glob matching of hidden tool names.
import { createServer } from 'node:http';
import { matchesGlob } from 'node:path';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Context, Effect, flow, Layer, Logger, Match, MutableRef, Option, Predicate, Queue, Result, Runtime, Schema, Stream, Struct } from 'effect';
import { McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { Argument, Command, Flag } from 'effect/cli';
import { HttpRouter } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { RpcClient, RpcClientError, type RpcMessage, RpcSerialization } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Client = RpcClient.FromGroup<typeof McpSchema.ClientRpcs, RpcClientError.RpcClientError>;
type Registry = McpServer.McpServer['Service'];
type Failure = McpSchema.McpError | RpcClientError.RpcClientError;
type Live = MutableRef.MutableRef<ReadonlySet<string>>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRIDGE = { name: 'bridge', version: '0' };
const REVISION = McpProtocol.v2025_11_25;

// --- [MODELS] --------------------------------------------------------------------------

const Port = Schema.Number.check(Schema.isBetween({ minimum: 1, maximum: 65_535 }));

// --- [ERRORS] --------------------------------------------------------------------------

class ChildExited extends Schema.TaggedError<ChildExited>()('ChildExited', { [Runtime.errorExitCode]: Schema.Int }) {}

// --- [SERVICES] ------------------------------------------------------------------------

const spawn = Effect.fn('spawn')(function* (command: string, args: readonly string[]) {
    const serialization = RpcSerialization.ndJsonRpc();
    const parser = serialization.makeUnsafe();
    const outbound = yield* Queue.make<RpcMessage.FromClientEncoded | RpcMessage.FromServerEncoded>();
    const notifications = yield* Queue.make<RpcMessage.RequestEncoded>();
    const handle = yield* ChildProcess.make(command, args, { stderr: 'inherit', stdin: Stream.fromQueue(outbound).pipe(Stream.map(parser.encode), Stream.filter(Predicate.isString), Stream.encodeText) });
    const protocol = yield* RpcClient.Protocol.make(
        Effect.fn('protocol')(function* (write, clientIds) {
            yield* handle.stdout.pipe(
                Stream.map((chunk) => parser.decode(chunk) as readonly RpcMessage.FromServerEncoded[]),
                Stream.flattenIterable,
                Stream.runForEach((message) => (message._tag === 'Request' ? Effect.asVoid(message.isNotification === true ? Queue.offer(notifications, message) : Queue.offer(outbound, reply(message))) : Effect.forEach(clientIds, (clientId) => write(clientId, message), { discard: true }))),
                Effect.forkScoped,
            );
            return {
                codecFor: serialization.codecFor,
                send: (_clientId, request) => Option.match(outgoing(request), { onNone: () => Effect.void, onSome: (message) => Effect.asVoid(Queue.offer(outbound, message)) }),
                supportsAck: false,
                supportsTransferables: false,
            };
        }),
    );
    const client = yield* RpcClient.make(McpSchema.ClientRpcs, { disableTracing: true }).pipe(Effect.provideService(RpcClient.Protocol, protocol));
    return { client, exit: handle.exitCode, notifications };
});

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TRANSPORT]
const outgoing = (request: RpcMessage.FromClientEncoded): Option.Option<RpcMessage.FromClientEncoded> =>
    Match.valueTags(request, {
        Request: (message) => Option.some<RpcMessage.FromClientEncoded>(McpSchema.ClientNotificationRpcs.requests.has(message.tag) ? { ...message, isNotification: true } : message),
        Interrupt: ({ requestId }) => Option.some<RpcMessage.FromClientEncoded>({ _tag: 'Request', headers: [], id: requestId, isNotification: true, payload: { requestId }, tag: McpSchema.CancelledNotification._tag }),
        Ack: Option.none,
        Ping: Option.none,
        Eof: Option.none,
    });
const reply = (request: RpcMessage.RequestEncoded): RpcMessage.ResponseExitEncoded => ({
    _tag: 'Exit',
    exit: request.tag === McpSchema.Ping._tag ? { _tag: 'Success', value: {} } : { _tag: 'Failure', cause: [{ _tag: 'Fail', error: Schema.encodeSync(McpSchema.MethodNotFound)(new McpSchema.MethodNotFound({ message: `${request.tag} is not supported by the bridge` })) }] },
    requestId: request.id,
});
const relayed =
    (registry: Registry) =>
    ({ tag, payload }: RpcMessage.RequestEncoded): Result.Result<{ readonly tag: string; readonly sent: Effect.Effect<void, Schema.SchemaError> }, string> =>
        Match.value(tag).pipe(
            Match.when(McpSchema.LoggingMessageNotification._tag, () => Effect.flatMap(Schema.decodeUnknownEffect(McpSchema.LoggingMessageNotification.payloadSchema)(payload), registry.notifications['notifications/message'])),
            Match.when(McpSchema.ResourceUpdatedNotification._tag, () => Effect.flatMap(Schema.decodeUnknownEffect(McpSchema.ResourceUpdatedNotification.payloadSchema)(payload), registry.notifications['notifications/resources/updated'])),
            Match.when(McpSchema.ToolListChangedNotification._tag, () => registry.notifications['notifications/tools/list_changed']({})),
            Match.when(McpSchema.ResourceListChangedNotification._tag, () => registry.notifications['notifications/resources/list_changed']({})),
            Match.when(McpSchema.PromptListChangedNotification._tag, () => registry.notifications['notifications/prompts/list_changed']({})),
            Match.option,
            Option.match({ onNone: () => Result.fail(tag), onSome: (sent) => Result.succeed({ tag, sent }) }),
        );

// --- [REGISTRATION]
const pages = <A, R extends { readonly nextCursor?: string | undefined }, E>(list: (request: { readonly cursor?: string }) => Effect.Effect<R, E>, items: (result: R) => readonly A[]): Effect.Effect<A[], E> =>
    Stream.runCollect(Stream.paginate({}, (request: { readonly cursor?: string }) => Effect.map(list(request), (result) => [items(result), Option.map(Option.fromNullishOr(result.nextCursor), (cursor) => ({ cursor }))] as const)));
const internal = (error: Failure): McpSchema.InternalError => new McpSchema.InternalError({ message: error.message });
const relay: <A>(effect: Effect.Effect<A, Failure>) => Effect.Effect<A, McpSchema.InternalError | McpSchema.InvalidParams> = Effect.catchIf(
    (error) => !(error instanceof RpcClientError.RpcClientError) && error.code === McpSchema.INVALID_PARAMS_ERROR_CODE,
    (error) => Effect.fail(new McpSchema.InvalidParams({ message: error.message })),
    flow(internal, Effect.fail),
);
const listed = (live: Live, key: string): Context.Context<never> => Context.make(McpSchema.EnabledWhen, () => MutableRef.get(live).has(key));
const published = (live: Live, keys: readonly string[], added: Effect.Effect<void>): Effect.Effect<void> =>
    Effect.andThen(
        Effect.sync(() => MutableRef.set(live, new Set(keys))),
        added,
    );
const tools = (client: Client, registry: Registry, hidden: Predicate.Predicate<string>, live: Live): Effect.Effect<void, Failure> =>
    Effect.flatMap(pages(client['tools/list'], Struct.get('tools')), (list) => {
        const shown = Array.filter(list, (tool) => !hidden(tool.name));
        return published(
            live,
            Array.map(shown, Struct.get('name')),
            Effect.forEach(shown, (tool) => registry.addTool({ annotations: listed(live, tool.name), handle: (input: Record<string, unknown>) => relay(client['tools/call']({ arguments: input, name: tool.name })), tool }), { discard: true }),
        );
    });
const resources = (client: Client, registry: Registry, _hidden: Predicate.Predicate<string>, live: Live): Effect.Effect<void, Failure> =>
    Effect.flatMap(Effect.all([pages(client['resources/list'], Struct.get('resources')), pages(client['resources/templates/list'], Struct.get('resourceTemplates'))], { concurrency: 'unbounded' }), ([concrete, templates]) =>
        published(
            live,
            [...Array.map(concrete, Struct.get('uri')), ...Array.map(templates, Struct.get('uriTemplate'))],
            Effect.all(
                [
                    Effect.forEach(concrete, (resource) => registry.addResource({ annotations: listed(live, resource.uri), handle: Effect.mapError(client['resources/read']({ uri: resource.uri }), internal), resource }), { discard: true }),
                    Effect.forEach(templates, (template) => registry.addResourceTemplate({ annotations: listed(live, template.uriTemplate), completions: {}, handle: (uri) => relay(client['resources/read']({ uri })), routerPath: '*', template }), { discard: true }),
                ],
                { concurrency: 'unbounded', discard: true },
            ),
        ),
    );
const prompts = (client: Client, registry: Registry, _hidden: Predicate.Predicate<string>, live: Live): Effect.Effect<void, Failure> =>
    Effect.flatMap(pages(client['prompts/list'], Struct.get('prompts')), (list) =>
        published(
            live,
            Array.map(list, Struct.get('name')),
            Effect.forEach(list, (prompt) => registry.addPrompt({ annotations: listed(live, prompt.name), completions: {}, handle: (params) => relay(client['prompts/get']({ arguments: params, name: prompt.name })), prompt }), { discard: true }),
        ),
    );
const LISTINGS = [
    { capability: 'tools', changed: McpSchema.ToolListChangedNotification._tag, register: tools },
    { capability: 'resources', changed: McpSchema.ResourceListChangedNotification._tag, register: resources },
    { capability: 'prompts', changed: McpSchema.PromptListChangedNotification._tag, register: prompts },
] as const;
const register = (client: Client, registry: Registry, hidden: Predicate.Predicate<string>, live: Readonly<Record<(typeof LISTINGS)[number]['capability'], Live>>, listings: readonly (typeof LISTINGS)[number][]): Effect.Effect<void, Failure> =>
    Effect.forEach(listings, (listing) => listing.register(client, registry, hidden, live[listing.capability]), { concurrency: 'unbounded', discard: true });

// --- [LIFECYCLE]
const initialize = (client: Client): Effect.Effect<McpSchema.InitializeResult, Failure> => client.initialize({ capabilities: {}, clientInfo: BRIDGE, protocolVersion: REVISION.protocolVersion }).pipe(Effect.tap(() => client['notifications/initialized'](undefined, { discard: true })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const OPTIONS = {
    port: Flag.Int('port').pipe(Flag.withSchema(Port), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')),
    hide: Flag.String('hide').pipe(Flag.atLeast(0), Flag.withDescription('Glob of server tool names left out of the endpoint, repeated per glob')),
    command: Argument.String('command').pipe(Argument.withDescription('Stdio MCP server to bridge, written after --')),
    args: Argument.String('argument').pipe(Argument.variadic()),
};
const bridge = Effect.fn('bridge')(function* ({ args, command, hide, port }: Command.Command.Config.Infer<typeof OPTIONS>) {
    const child = yield* spawn(command, args);
    const hidden = (name: string): boolean => hide.some((glob) => matchesGlob(name, glob));
    const live = { tools: MutableRef.make<ReadonlySet<string>>(new Set()), resources: MutableRef.make<ReadonlySet<string>>(new Set()), prompts: MutableRef.make<ReadonlySet<string>>(new Set()) };
    const serving = Effect.gen(function* () {
        const info = yield* initialize(child.client);
        const services = yield* Layer.build(
            HttpRouter.serve(
                Layer.effectDiscard(
                    Effect.flatMap(McpServer.McpServer, (server) =>
                        register(
                            child.client,
                            server,
                            hidden,
                            live,
                            Array.filter(LISTINGS, ({ capability }) => Predicate.isNotUndefined(info.capabilities[capability])),
                        ),
                    ),
                ).pipe(Layer.provideMerge(McpServer.layerHttp({ ...info.serverInfo, instructions: info.instructions, path: '/', protocols: [McpProtocol.v2026_07_28, REVISION, McpProtocol.v2025_06_18] }))),
                { disableLogger: true },
            ).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port }))),
        );
        const registry = Context.get(services, McpServer.McpServer);
        return yield* Stream.fromQueue(child.notifications).pipe(
            Stream.filterMap(relayed(registry)),
            Stream.runForEach(({ tag, sent }) =>
                Effect.andThen(
                    register(
                        child.client,
                        registry,
                        hidden,
                        live,
                        Array.filter(LISTINGS, ({ changed }) => changed === tag),
                    ),
                    sent,
                ),
            ),
        );
    });
    return yield* Effect.raceFirst(
        serving,
        Effect.flatMap(child.exit, (code) => Effect.fail(new ChildExited({ [Runtime.errorExitCode]: code }))),
    );
}, Effect.scoped);
Command.make(BRIDGE.name, OPTIONS, bridge).pipe(Command.run({ version: BRIDGE.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
