import { rank } from '@rasm/search/rank';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, flow, Layer, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaGetter } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool } from 'effect/ai';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpTraceContext } from 'effect/http';
import { NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) }))),
);
const Scheme = Schema.String.pipe(
    Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
        decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
        encode: SchemaGetter.forbiddenEncoding,
    }),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Server extends Context.Service<Server, { readonly uri: (path: string) => string }>()('@rasm/mcp/server') {
    static readonly Scheme = Scheme;
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/Server/Instructions', { defaultValue: () => [] });
    static readonly ToolCall = Context.Reference<{ readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>; readonly respond: (refusal: Error) => Effect.Effect<void> }>('@rasm/mcp/Server/ToolCall', { defaultValue: () => ({ add: () => Effect.void, respond: () => Effect.void }) });
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly layer = ({ scheme, retention }: { readonly scheme: typeof Scheme.Type; readonly retention: Duration.Duration }) =>
        Layer.effect(McpServer.McpServer)(
            Effect.gen(function* () {
                const { uri } = yield* Server;
                const [server, { registration, record, link }] = yield* Effect.all([McpServer.McpServer, executions(uri('executions'), retention)]);
                const ranking =
                    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
                    (input: string, context: C) =>
                        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                            const ordered = rank(input, values);
                            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                        });
                const writes = ({ annotations }: Parameters<typeof server.addTool>[0]) => !Context.get(annotations, Tool.Readonly);
                const handleCall = (options: Parameters<typeof server.addTool>[0]) =>
                    Effect.fnUntraced(
                        function* (payload: Schema.Json) {
                            const [blocks, earlyResult] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
                            const answer = Effect.zipWith(options.handle(payload), Ref.get(blocks), (result, added) => (result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] }))).pipe(
                                Effect.provideService(Server.ToolCall, {
                                    add: (block) => Ref.update(blocks, Array.append(block)),
                                    respond: (refusal) => Effect.asVoid(Deferred.succeed(earlyResult, McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: refusal.message }), link] }))),
                                }),
                            );
                            return yield* writes(options) ? record(options.tool.name, payload, answer, earlyResult) : answer;
                        },
                        instrumented('tools/call', options.tool.name, { 'gen_ai.tool.name': options.tool.name, 'gen_ai.operation.name': 'execute_tool' }),
                    );
                return McpServer.McpServer.of({
                    ...server,
                    addTool: (options) => Effect.andThen(writes(options) ? registration : Effect.void, server.addTool({ ...options, handle: handleCall(options) })),
                    addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(instrumented('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
                    addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(instrumented('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
                    addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(instrumented('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
                });
            }),
        ).pipe(Layer.provideMerge(Layer.succeed(Server)({ uri: (path) => `${scheme}://${path}` })));
    static readonly layerHttp = <E, R>(
        manifest: { readonly name: typeof Scheme.Type; readonly version: string; readonly description: string },
        options: { readonly address: NetAddress.InetAddress; readonly hosts: readonly string[]; readonly retention: Duration.Duration; readonly instructions: readonly string[] },
        app: Layer.Layer<never, E, R>,
    ) => {
        const allowed = new Set([...options.hosts, 'localhost', ...[...NetAddress.loopbackAddresses, ...(NetAddress.isUnspecified(options.address.address) ? [] : [options.address.address])].map(NetAddress.formatUrlHost)]);
        const hostValidation = HttpRouter.middleware(
            (next) =>
                Effect.flatMap(
                    HttpServerRequest.HttpServerRequest,
                    flow(
                        HttpServerRequest.toURL,
                        Option.exists(({ host, hostname }) => allowed.has(host) || allowed.has(hostname)),
                        (accepted) => (accepted ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 }))),
                    ),
                ),
            { global: true },
        );
        const transport = Layer.unwrap(
            Effect.map(McpServer.McpServer, (server) =>
                McpServer.layerHttp({
                    ...manifest,
                    get instructions() {
                        return Array.dedupe([manifest.description, ...options.instructions, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n');
                    },
                    path: '/',
                    allowSessionTermination: true,
                    protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
                    registrations: Layer.provide(app, Server.layer({ scheme: manifest.name, retention: options.retention })),
                }),
            ),
        ).pipe(Layer.provideMerge(McpServer.McpServer.layer));
        return Layer.merge(transport, hostValidation);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const operationDuration = Metric.mapInput(
    Metric.histogram('mcp.server.operation.duration', {
        description: 'Duration of MCP server operations',
        boundaries: Array.map(['10 millis', '20 millis', '50 millis', '100 millis', '200 millis', '500 millis', '1 second', '2 seconds', '5 seconds', '10 seconds', '30 seconds', '1 minute', '2 minutes', '5 minutes'] as const, Duration.toSeconds),
    }),
    Duration.toSeconds,
);
const failureAttributes = Exit.match({ onSuccess: (value: unknown) => (Schema.is(McpSchema.CallToolResult)(value) && value.isError === true ? { 'error.type': 'tool_error' } : {}), onFailure: (cause: Cause.Cause<unknown>) => ({ 'error.type': Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error' }) });
const instrumented =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(McpSchema.McpRequestContext, ({ requestMetadata }) =>
            Effect.gen(function* () {
                const [duration, exit] = yield* Effect.timed(Effect.exit(self));
                const outcomeAttributes = failureAttributes(exit);
                yield* Effect.annotateCurrentSpan(outcomeAttributes);
                yield* Metric.update(Metric.withAttributes(operationDuration, { 'mcp.method.name': method, ...attributes, ...outcomeAttributes }), duration);
                return yield* exit;
            }).pipe(Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) })),
        );
const executions = Effect.fnUntraced(function* (uri: string, retention: Duration.Duration) {
    const [server, scope, entries] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<(typeof Executions.Type)[number]>>())]);
    const descriptor = { uri, name: 'executions', description: `Cancelled or refused state-changing calls that keep running, finished calls drop after one read or ${Duration.format(retention)}`, mimeType: 'application/json' };
    const registration = yield* Effect.cached(
        McpServer.registerResource({
            ...descriptor,
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(entries);
                const read = yield* Effect.all(rows);
                yield* Ref.update(entries, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return { contents: [{ uri, mimeType: descriptor.mimeType, text: yield* Effect.orDie(Schema.encodeEffect(Executions)(read)) }] };
            }),
        }).pipe(Effect.provideService(McpServer.McpServer, server)),
    );
    const record = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>, earlyResult: Deferred.Deferred<McpSchema.CallToolResult>) {
        const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
        const entry: Effect.Effect<(typeof Executions.Type)[number]> = Deferred.poll(done).pipe(
            Effect.map(Option.map(Effect.exit)),
            Effect.flatMap(Effect.transposeOption),
            Effect.map((result) => ({ tool, arguments: given, result })),
        );
        const addEntry = Effect.gen(function* () {
            yield* Ref.update(entries, Array.append(entry));
            yield* Effect.forkIn(Deferred.await(done).pipe(Effect.exit, Effect.andThen(Effect.sleep(retention)), Effect.andThen(Ref.update(entries, Array.difference([entry])))), scope);
        });
        yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
        const [outcome, pending] = yield* Effect.raceFirst(
            Effect.map(Deferred.await(done), (value) => [value, false] as const),
            Effect.map(Deferred.await(earlyResult), (value) => [value, true] as const),
        ).pipe(
            Effect.interruptible,
            Effect.onInterrupt(() => addEntry),
        );
        return yield* Effect.as(pending ? addEntry : Effect.void, outcome);
    }, Effect.uninterruptible);
    return { registration, record, link: McpSchema.ResourceLink.make(descriptor) };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Server };
