import { dial, listen } from '@rasm/rpc';
import { Array, Context, Duration, Effect, type Layer, Record, Ref, Schema, type Stream, Struct } from 'effect';
import { McpSchema, McpServer, type Tool, type Toolkit } from 'effect/ai';
import { Rpc, RpcGroup } from 'effect/rpc';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/link';

// --- [MODELS] --------------------------------------------------------------------------

class Link extends Schema.Class<Link, { readonly brand: unique symbol }>('Link')({
    application: Schema.Union([Schema.URLFromString.annotate({ description: 'Host application bundle URL with trailing slash' }), Schema.String.annotate({ description: 'Host application name, from a host that reports no bundle path' })]),
    plugin: Schema.String.annotate({ description: 'Plugin identifier inside its host application' }),
    requests: Schema.fromJsonString(Schema.toCodecJson(Schema.ReadonlySet(Schema.String)))
        .annotate({ description: 'Requests the plugin serves beside tool calls' })
        .pipe(Schema.withConstructorDefault(Effect.succeed(new Set<string>()))),
}) {
    static readonly Event = Schema.Struct({
        name: Schema.String.annotate({ description: 'Host event name as the host API states it' }),
        target: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Specifier of the object the event concerns' })),
        detail: Schema.OptionFromOptionalKey(Schema.Json),
    });
    static readonly Rpcs = RpcGroup.make(
        McpSchema.ListTools,
        McpSchema.CallTool.setPayload(Schema.Struct({ ...McpSchema.CallTool.payloadSchema.fields, context: Schema.Struct({ clientId: Schema.Int, protocolVersion: Schema.String, clientCapabilities: McpSchema.ClientCapabilities }).annotate({ description: 'Request context of the linked server call' }) })),
        Rpc.make('events', { success: Link.Event, stream: true }),
    );
    static readonly key = ({ application, plugin }: Link) => `${plugin} in ${String(application)}`;
    static readonly serves =
        <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>) =>
        ({ requests }: Link) =>
            requests.isSupersetOf(group.requests);
    static readonly runsIn =
        ({ url, name }: { readonly url: URL; readonly name: string }) =>
        ({ application }: Link) =>
            typeof application === 'string' ? application === name : application.href === url.href;
    static readonly dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, url: URL, peer: Effect.Effect<Pick<Link, 'application' | 'plugin'>, E, R>) => {
        const requests = new Set(Array.filter(group.requests.keys(), (tag) => !Link.Rpcs.requests.has(tag)));
        return dial(Link.Rpcs.merge(group), handlers, { url: new URL(PATH, url), redial: Duration.seconds(1), peer: Effect.flatMap(peer, (identity) => Effect.orDie(Schema.encodeEffect(Link)(Link.make({ ...identity, requests })))) });
    };
    static readonly listen = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, origin: string) => listen(Link.Rpcs.merge(group), { path: PATH, origin, peer: Link, key: Link.key });
    static readonly serve = <Served extends Record.ReadonlyRecord<string, Tool.Any>, R>(toolkit: Toolkit.Toolkit<Served>, events: Stream.Stream<typeof Link.Event.Type, never, R>) =>
        Link.Rpcs.toLayer(
            Effect.gen(function* () {
                const [server, registered] = yield* Effect.all([McpServer.McpServer.make, Ref.make<Record.ReadonlyRecord<string, Parameters<McpServer.McpServer['Service']['addTool']>[0]>>({})]);
                yield* McpServer.registerToolkit(toolkit).pipe(Effect.provideService(McpServer.McpServer, McpServer.McpServer.of({ ...server, addTool: (options) => Ref.update(registered, Record.set(options.tool.name, options)) })));
                const rows = yield* Ref.get(registered);
                return {
                    'tools/list': () => Effect.succeed(McpSchema.ListToolsResult.make({ tools: Record.collect(rows, (_, { tool }) => tool) })),
                    'tools/call': Effect.fnUntraced(function* ({ name, arguments: given, context, _meta }) {
                        const [{ handle, services = Context.empty() }, blocks] = yield* Effect.all([Effect.fromOption(Record.get(rows, name), () => McpSchema.InvalidParams.make({ message: `Tool '${name}' not found among ${Record.keys(rows).join(', ')}` })), Ref.make(Array.empty<McpSchema.ContentBlock>())]);
                        const result = yield* handle(given).pipe(
                            Effect.provideService(McpSchema.McpRequestContext, McpSchema.McpRequestContext.of({ ...context, requestMetadata: _meta })),
                            Effect.updateContext((request: Context.Context<never>) => Context.merge(services, request)),
                            Effect.provideService(Server.ToolCall, { ...Server.ToolCall.defaultValue(), add: (block) => Ref.update(blocks, Array.append(block)) }),
                            Effect.filterOrFail(Schema.is(McpSchema.CallToolResult), () => McpSchema.InternalError.make({ message: `${name} asked for client input, which a link does not carry` })),
                        );
                        return McpSchema.CallToolResult.make(Struct.evolve(result, { content: Array.appendAll(yield* Ref.get(blocks)) }));
                    }),
                    events: () => Rpc.fork(events),
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link };
