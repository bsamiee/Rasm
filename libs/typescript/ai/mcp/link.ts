import { dial, listen } from '@rasm/rpc';
import type { Unmatched } from '@rasm/search';
import { Array, Context, Duration, Effect, Inspectable, Layer, Record, Ref, Schema, SchemaGetter, Stream, Struct } from 'effect';
import { McpSchema, McpServer, Tool, type Toolkit } from 'effect/ai';
import { Url } from 'effect/http';
import { type Rpc, type RpcClient, type RpcClientError, RpcGroup } from 'effect/rpc';
import { Server } from '#/server';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /(?<=\.app)(?:\/.*)?$|(?<=[^/])$/u;
const PATH = '/link';
const REDIAL = Duration.seconds(1);

// --- [MODELS] --------------------------------------------------------------------------

const Call = Schema.Struct({
    ...McpSchema.CallTool.payloadSchema.fields,
    context: Schema.Struct({ clientId: Schema.Int, protocolVersion: Schema.String, clientCapabilities: McpSchema.ClientCapabilities }).annotate({ description: 'Request context of the linked server call' }),
});

class Link extends Schema.Class<Link, { readonly brand: unique symbol }>('Link')({
    application: Schema.Union([
        Schema.URLFromString.pipe(Schema.decode({ decode: SchemaGetter.transform((url) => Url.setPathname(url, url.pathname.replace(BUNDLE, '/'))), encode: SchemaGetter.passthrough() })).annotate({ description: 'Host application bundle URL with trailing slash' }),
        Schema.String.annotate({ description: 'Host application name, from a host that reports no bundle path' }),
    ]),
    extension: Schema.String.annotate({ description: 'Extension identifier inside its host application' }),
    root: Schema.OptionFromOptionalKey(Schema.URLFromString).annotate({ description: 'Directory the host loaded the extension from' }),
    runtime: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'ECMAScript target of the source the extension evaluates for the linked server' })),
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
        dial(Link.Tools.merge(group), handlers, { url: new URL(PATH, url), redial: REDIAL, peer: Effect.flatMap(peer, (identity) => Effect.orDie(Schema.encodeEffect(Link)(Link.make({ ...identity, requests: [...group.requests.keys()] })))) });
    static readonly listen = <Rpcs extends Rpc.Any>(group: RpcGroup.RpcGroup<Rpcs>) => listen(Link.Tools.merge(group), { path: PATH, origin: 'file://', peer: Link, key: Link.key, middleware: Layer.empty });
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
                            Effect.provideContext(Context.add(services, McpSchema.McpRequestContext, McpSchema.McpRequestContext.of({ ...context, requestMetadata: _meta }))),
                            Effect.provideService(Server.ToolCall, { add: (block) => Ref.update(blocks, Array.append(block)), respond: () => Effect.void }),
                            Effect.filterOrFail(Schema.is(McpSchema.CallToolResult), () => McpSchema.InternalError.make({ message: `${name} asked for client input, which a link does not carry` })),
                        );
                        return McpSchema.CallToolResult.make(Struct.evolve(result, { content: Array.appendAll(yield* Ref.get(blocks)) }));
                    }),
                };
            }),
        );
    static readonly host = Effect.fnUntraced(function* (
        links: { readonly changes: Stream.Stream<readonly Link[]>; readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof Link.Tools>, RpcClientError.RpcClientError>, Unmatched> },
        { name }: { readonly name: (peer: Link) => Effect.Effect<string> },
    ) {
        const route = 'application';
        const offered = Effect.fnUntraced(function* (peer: Link) {
            const [client, named] = yield* Effect.all([links.call((each) => Link.key(each) === Link.key(peer), Link.key(peer)), name(peer)]);
            const { tools } = yield* client('tools/list', undefined);
            return Array.map(tools, (tool) => ({ tool, named, client }));
        });
        const [server, shown, routes] = yield* Effect.all([McpServer.McpServer, Ref.make<Record.ReadonlyRecord<string, McpSchema.Tool>>({}), Ref.make<Record.ReadonlyRecord<string, Array.NonEmptyReadonlyArray<Effect.Success<ReturnType<typeof offered>>[number]>>>({})]);
        const routed = (rows: Array.NonEmptyReadonlyArray<Effect.Success<ReturnType<typeof offered>>[number]>) => {
            const { tool } = Array.headNonEmpty(rows);
            return Array.isReadonlyArrayNonEmpty(Array.tailNonEmpty(rows))
                ? McpSchema.Tool.make({
                      ...tool,
                      inputSchema: {
                          ...tool.inputSchema,
                          properties: { ...tool.inputSchema.properties, [route]: { type: 'string', enum: Array.map(rows, Struct.get('named')), description: 'Linked application running the tool' } },
                          required: [...(tool.inputSchema.required ?? []), route],
                      },
                  })
                : tool;
        };
        const forwarding = (toolName: string) =>
            Effect.fnUntraced(function* (payload: Record.ReadonlyRecord<string, unknown>) {
                const [request, known] = yield* Effect.all([McpSchema.McpRequestContext, Ref.get(routes)]);
                const rows = yield* Effect.fromOption(Record.get(known, toolName), () => McpSchema.InternalError.make({ message: `${toolName} has no linked application` }));
                const { [route]: chosen, ...given } = payload;
                const row = yield* Effect.fromOption(
                    Array.findFirst(rows, ({ named }) => rows.length === 1 || named === chosen),
                    () => McpSchema.InvalidParams.make({ message: `${route} ${Inspectable.toStringUnknown(chosen)} is none of ${Array.map(rows, Struct.get('named')).join(', ')}` }),
                );
                return yield* row
                    .client('tools/call', { name: toolName, arguments: given, context: Struct.pick(request, ['clientId', 'protocolVersion', 'clientCapabilities']), _meta: request.requestMetadata })
                    .pipe(Effect.mapError((error) => (Schema.is(McpSchema.InvalidParams)(error) ? error : McpSchema.InternalError.make({ message: error.message }))));
            });
        const registered = Effect.fnUntraced(function* (peers: readonly Link[]) {
            const [served, failed] = yield* Effect.partition(peers, offered, { concurrency: 'unbounded' });
            yield* Effect.forEach(failed, (cause) => Effect.logError('Linked extension listed no tools', cause), { discard: true });
            const byName = Array.groupBy(Array.flatten(served), ({ tool }) => tool.name);
            yield* Ref.set(routes, byName);
            const known = yield* Ref.updateAndGet(shown, (previous) => ({ ...previous, ...Record.map(byName, routed) }));
            yield* Effect.forEach(
                Record.toEntries(known),
                ([toolName, tool]) =>
                    server.addTool({
                        tool,
                        annotations: Context.make(Tool.Readonly, tool.annotations?.readOnlyHint ?? false).pipe(Context.add(Tool.Destructive, tool.annotations?.destructiveHint ?? true), Context.add(Server.Visible, Effect.succeed(Record.has(byName, toolName)))),
                        handle: forwarding(toolName),
                    }),
                { discard: true },
            );
        });
        yield* Effect.forkScoped(Stream.runForEach(links.changes, registered));
    });
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Link };
