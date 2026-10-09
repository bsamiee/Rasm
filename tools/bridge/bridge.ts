// biome-ignore lint/correctness/noNodejsModules: NodeHttpServer.layerServer consumes the native Node HTTP server factory.
import { createServer } from 'node:http';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import { Array, Context, Effect, flow, Layer, Logger, type LogLevel, Match, Option, Predicate, Queue, Runtime, Schema, Stream, Struct } from 'effect';
import { McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { Argument, Command, Flag } from 'effect/cli';
import { HttpRouter } from 'effect/http';
import { ChildProcess } from 'effect/process';
import { RpcClient, RpcClientError, type RpcMessage, RpcSerialization } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

type Client = RpcClient.FromGroup<typeof McpSchema.ClientRpcs, RpcClientError.RpcClientError>;
type Registry = McpServer.McpServer['Service'];
type Failure = McpSchema.McpError | RpcClientError.RpcClientError;

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRIDGE = { name: 'bridge', version: '0' };

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
const log = (payload: unknown): Effect.Effect<void, Schema.SchemaError> =>
    Effect.flatMap(Schema.decodeUnknownEffect(McpSchema.LoggingMessageNotification.payloadSchema)(payload), ({ data, level, logger }) =>
        Effect.logWithLevel(({ alert: 'Fatal', critical: 'Fatal', debug: 'Debug', emergency: 'Fatal', error: 'Error', info: 'Info', notice: 'Info', warning: 'Warn' } satisfies Record<McpSchema.LoggingLevel, LogLevel.Severity>)[level])(...Array.fromNullishOr(logger), data),
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
const tools = (client: Client, registry: Registry, hidden: readonly string[]): Effect.Effect<void, Failure> =>
    Effect.flatMap(pages(client['tools/list'], Struct.get('tools')), (list) =>
        Effect.forEach(
            Array.filter(list, (tool) => !hidden.includes(tool.name)),
            (tool) => registry.addTool({ annotations: Context.empty(), handle: (input: Record<string, unknown>) => relay(client['tools/call']({ arguments: input, name: tool.name })), tool }),
            { discard: true },
        ),
    );
const resources = (client: Client, registry: Registry): Effect.Effect<void, Failure> =>
    Effect.all(
        [
            Effect.flatMap(pages(client['resources/list'], Struct.get('resources')), (list) => Effect.forEach(list, (resource) => registry.addResource({ annotations: Context.empty(), handle: Effect.mapError(client['resources/read']({ uri: resource.uri }), internal), resource }), { discard: true })),
            Effect.flatMap(pages(client['resources/templates/list'], Struct.get('resourceTemplates')), (list) =>
                Effect.forEach(list, (template) => registry.addResourceTemplate({ annotations: Context.empty(), completions: {}, handle: (uri) => relay(client['resources/read']({ uri })), routerPath: '*', template }), { discard: true }),
            ),
        ],
        { concurrency: 'unbounded', discard: true },
    );
const prompts = (client: Client, registry: Registry): Effect.Effect<void, Failure> =>
    Effect.flatMap(pages(client['prompts/list'], Struct.get('prompts')), (list) => Effect.forEach(list, (prompt) => registry.addPrompt({ annotations: Context.empty(), completions: {}, handle: (params) => relay(client['prompts/get']({ arguments: params, name: prompt.name })), prompt }), { discard: true }));
const LISTINGS = [
    { capability: 'tools', changed: McpSchema.ToolListChangedNotification._tag, register: tools },
    { capability: 'resources', changed: McpSchema.ResourceListChangedNotification._tag, register: resources },
    { capability: 'prompts', changed: McpSchema.PromptListChangedNotification._tag, register: prompts },
] as const;
const register = (client: Client, registry: Registry, hidden: readonly string[], listings: readonly (typeof LISTINGS)[number][]): Effect.Effect<void, Failure> => Effect.forEach(listings, (listing) => listing.register(client, registry, hidden), { concurrency: 'unbounded', discard: true });

// --- [LIFECYCLE]
const initialize = (client: Client): Effect.Effect<McpSchema.InitializeResult, Failure> => client.initialize({ capabilities: {}, clientInfo: BRIDGE, protocolVersion: McpProtocol.v2025_11_25.protocolVersion }).pipe(Effect.tap(() => client['notifications/initialized'](undefined, { discard: true })));

// --- [COMPOSITION] ---------------------------------------------------------------------

const OPTIONS = {
    port: Flag.Int('port').pipe(Flag.withSchema(Port), Flag.withDescription('Loopback port the Streamable HTTP endpoint listens on')),
    hide: Flag.String('hide').pipe(Flag.atLeast(0), Flag.withDescription('Server tool left out of the endpoint, repeated per tool')),
    command: Argument.String('command').pipe(Argument.withDescription('Stdio MCP server to bridge, written after --')),
    args: Argument.String('argument').pipe(Argument.variadic()),
};
const bridge = Effect.fn('bridge')(function* ({ args, command, hide, port }: Command.Command.Config.Infer<typeof OPTIONS>) {
    const child = yield* spawn(command, args);
    const serving = Effect.gen(function* () {
        const info = yield* initialize(child.client);
        const services = yield* Layer.build(
            HttpRouter.serve(
                Layer.effectDiscard(
                    Effect.flatMap(McpServer.McpServer, (server) =>
                        register(
                            child.client,
                            server,
                            hide,
                            Array.filter(LISTINGS, ({ capability }) => Predicate.isNotUndefined(info.capabilities[capability])),
                        ),
                    ),
                ).pipe(Layer.provideMerge(McpServer.layerHttp({ ...info.serverInfo, instructions: info.instructions, path: '/', protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_06_18] }))),
                { disableLogger: true },
            ).pipe(Layer.provide(NodeHttpServer.layerServer(createServer, { host: '127.0.0.1', port }))),
        );
        const registry = Context.get(services, McpServer.McpServer);
        return yield* Stream.runForEach(
            Stream.fromQueue(child.notifications),
            (notification): Effect.Effect<void, Failure | Schema.SchemaError> =>
                notification.tag === McpSchema.LoggingMessageNotification._tag
                    ? log(notification.payload)
                    : register(
                          child.client,
                          registry,
                          hide,
                          Array.filter(LISTINGS, ({ changed }) => changed === notification.tag),
                      ),
        );
    });
    return yield* Effect.raceFirst(
        serving,
        Effect.flatMap(child.exit, (code) => Effect.fail(new ChildExited({ [Runtime.errorExitCode]: code }))),
    );
}, Effect.scoped);
Command.make(BRIDGE.name, OPTIONS, bridge).pipe(Command.run({ version: BRIDGE.version }), Effect.provide(NodeServices.layer), Effect.provideService(Logger.LogToStderr, true), NodeRuntime.runMain);
