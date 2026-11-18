import { dial, listen } from '@rasm/rpc';
import { Array, Context, Duration, Effect, Layer, Record, Ref, Schema, Struct } from 'effect';
import { McpSchema, McpServer, type Tool, type Toolkit } from 'effect/ai';
import { type Rpc, RpcGroup } from 'effect/rpc';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const PATH = '/link';
const REDIAL = Duration.seconds(1);

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.Struct({
    ...McpSchema.CallTool.payloadSchema.fields,
    context: Schema.Struct({ clientId: Schema.Int, protocolVersion: Schema.String, clientCapabilities: McpSchema.ClientCapabilities }).annotate({ description: 'Request context of the linked server call' }),
});

class Link extends Schema.Class<Link, { readonly brand: unique symbol }>('Link')({
    application: Schema.Union([Schema.URLFromString.annotate({ description: 'Host application bundle URL with trailing slash' }), Schema.String.annotate({ description: 'Host application name, from a host that reports no bundle path' })]),
    extension: Schema.String.annotate({ description: 'Extension identifier inside its host application' }),
    root: Schema.OptionFromOptionalKey(Schema.URLFromString).annotate({ description: 'Directory the host loaded the extension from' }),
    requests: Schema.fromJsonString(Schema.Array(Schema.String))
        .annotate({ description: 'Requests the extension serves beside tool calls' })
        .pipe(Schema.withDecodingDefaultKey(Effect.succeed('[]')), Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly Tools = RpcGroup.make(McpSchema.ListTools, McpSchema.CallTool.setPayload(Call));
    static readonly key = ({ application, extension }: Link) => `${extension} in ${String(application)}`;
    static readonly serves = (tags: Iterable<string>) => {
        const wanted = [...tags];
        return ({ requests }: Link) => wanted.every((tag) => requests.includes(tag));
    };
    static readonly hosted =
        ({ url, name }: { readonly url: URL; readonly name: string }) =>
        ({ application }: Link) =>
            typeof application === 'string' ? application === name : application.href === url.href;
    static readonly dial = <Rpcs extends Rpc.Any, H, EH, RH, E, R>(group: RpcGroup.RpcGroup<Rpcs>, handlers: Layer.Layer<H, EH, RH>, url: URL, peer: Effect.Effect<Link, E, R>) =>
        dial(Link.Tools.merge(group), handlers, { url: new URL(PATH, url), redial: REDIAL, peer: Effect.flatMap(peer, (identity) => Effect.orDie(Schema.encodeEffect(Link)(Link.make({ ...identity, requests: group.requests.keys().toArray() })))) });
    static readonly listen = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>, origin: string) => listen(Link.Tools.merge(group), { path: PATH, origin, peer: Link, key: Link.key, middleware: Layer.empty });
    static readonly serve = <Served extends Record.ReadonlyRecord<string, Tool.Any>>(toolkit: Toolkit.Toolkit<Served>) =>
        Link.Tools.toLayer(
            Effect.gen(function* () {
                const [made, registered] = yield* Effect.all([McpServer.McpServer.make, Ref.make<Record.ReadonlyRecord<string, Parameters<McpServer.McpServer['Service']['addTool']>[0]>>({})]);
                yield* McpServer.registerToolkit(toolkit).pipe(Effect.provideService(McpServer.McpServer, McpServer.McpServer.of({ ...made, addTool: (options) => Ref.update(registered, Record.set(options.tool.name, options)) })));
                const rows = yield* Ref.get(registered);
                return {
                    'tools/list': () => Effect.succeed(McpSchema.ListToolsResult.make({ tools: Record.collect(rows, (_, { tool }) => tool) })),
                    'tools/call': Effect.fnUntraced(function* ({ name, arguments: given, context, _meta }: typeof Call.Type) {
                        const [{ handle, services = Context.empty() }, blocks] = yield* Effect.all([Effect.fromOption(Record.get(rows, name), () => McpSchema.InvalidParams.make({ message: `Tool '${name}' not found among ${Record.keys(rows).join(', ')}` })), Ref.make(Array.empty<McpSchema.ContentBlock>())]);
                        const result = yield* handle(given).pipe(
                            Effect.provideService(McpSchema.McpRequestContext, McpSchema.McpRequestContext.of({ ...context, requestMetadata: _meta })),
                            Effect.updateContext((request: Context.Context<never>) => Context.merge(services, request)),
                            Effect.provideService(Server.ToolCall, { ...Server.ToolCall.defaultValue(), add: (block) => Ref.update(blocks, Array.append(block)) }),
                            Effect.filterOrFail(Schema.is(McpSchema.CallToolResult), () => McpSchema.InternalError.make({ message: `${name} asked for client input, which a link does not carry` })),
                        );
                        return McpSchema.CallToolResult.make(Struct.evolve(result, { content: Array.appendAll(yield* Ref.get(blocks)) }));
                    }),
                };
            }),
        );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link };
