import { ranked } from '@rasm/search/ranked';
import { Array, Cause, Context, Deferred, Effect, Exit, Formatter, flow, Layer, Metric, Option, Predicate, pipe, Record, Ref, Schema } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool } from 'effect/ai';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpTraceContext } from 'effect/http';
import { NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) }))),
);

// --- [SERVICES] ------------------------------------------------------------------------

const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
const Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visible', { defaultValue: () => Effect.succeed(true) });
class Scheme extends Context.Service<Scheme, { readonly uri: (path: string) => string }>()('@rasm/mcp/server/Scheme') {}
const ToolCall = Context.Reference<{ readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>; readonly respond: (refusal: Error) => Effect.Effect<void> }>('@rasm/mcp/server/ToolCall', { defaultValue: () => ({ add: () => Effect.void, respond: () => Effect.void }) });

// --- [OPERATIONS] ----------------------------------------------------------------------

const orInternalError = Effect.catchCause((cause: Cause.Cause<unknown>) =>
    Cause.hasInterruptsOnly(cause)
        ? Effect.interrupt
        : Effect.fail(
              McpSchema.InternalError.make({
                  message: Cause.prettyErrors(cause)
                      .map((error) => Formatter.format(error))
                      .join('\n'),
              }),
          ),
);
const failure = Exit.match({ onSuccess: (value: unknown) => (Schema.is(McpSchema.CallToolResult)(value) && value.isError === true ? { 'error.type': 'tool_error' } : {}), onFailure: (cause: Cause.Cause<unknown>) => ({ 'error.type': Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error' }) });
const instrumented =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(McpSchema.McpRequestContext, ({ requestMetadata }) =>
            Effect.gen(function* () {
                const [duration, exit] = yield* Effect.timed(Effect.exit(self));
                const failed = failure(exit);
                yield* Effect.annotateCurrentSpan(failed);
                yield* Metric.update(Metric.withAttributes(Metric.timer('mcp.server.operation.duration', { description: 'Duration of MCP server operations' }), { 'mcp.method.name': method, ...attributes, ...failed }), duration);
                return yield* exit;
            }).pipe(Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) })),
        );
const executions = Effect.gen(function* () {
    const [server, scheme, scope, kept] = yield* Effect.all([McpServer.McpServer, Scheme, Effect.scope, Ref.make(Array.empty<Effect.Effect<(typeof Executions.Type)[number]>>())]);
    const listed = { uri: scheme.uri('executions'), name: 'executions', description: 'Cancelled or refused state-changing calls that keep running. Read outcome before repeating a call, finished calls drop after one read', mimeType: 'application/json' };
    const registration = yield* Effect.cached(
        McpServer.registerResource({
            ...listed,
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(kept);
                const read = yield* Effect.all(rows);
                yield* Ref.update(kept, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return { contents: [{ uri: listed.uri, mimeType: listed.mimeType, text: yield* Effect.orDie(Schema.encodeEffect(Executions)(read)) }] };
            }),
        }).pipe(Effect.provideService(McpServer.McpServer, server)),
    );
    const record = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>, early: Deferred.Deferred<McpSchema.CallToolResult>) {
        const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
        const recorded = Ref.update(
            kept,
            Array.append(
                Deferred.poll(done).pipe(
                    Effect.map(Option.map(Effect.exit)),
                    Effect.flatMap(Effect.transposeOption),
                    Effect.map((result) => ({ tool, arguments: given, result })),
                ),
            ),
        );
        yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
        const [outcome, pending] = yield* Effect.raceFirst(
            Effect.map(Deferred.await(done), (value) => [value, false] as const),
            Effect.map(Deferred.await(early), (value) => [value, true] as const),
        ).pipe(
            Effect.interruptible,
            Effect.onInterrupt(() => recorded),
        );
        return yield* Effect.as(pending ? recorded : Effect.void, outcome);
    }, Effect.uninterruptible);
    return { registration, record, link: McpSchema.ResourceLink.make(listed) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = Layer.effect(McpServer.McpServer)(
    Effect.gen(function* () {
        const [server, { registration, record, link }] = yield* Effect.all([McpServer.McpServer, executions]);
        const ranking =
            <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
            (input: string, context: C) =>
                Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                    const ordered = ranked(input, values);
                    return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                });
        const writes = ({ annotations }: Parameters<typeof server.addTool>[0]) => !Context.get(annotations, Tool.Readonly);
        const refused = ({ message }: McpSchema.InternalError) => McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message }), link] });
        const served = (options: Parameters<typeof server.addTool>[0]) =>
            Effect.fnUntraced(
                function* (payload: Schema.Json) {
                    const [blocks, early] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
                    const answer = Effect.gen(function* () {
                        const result = yield* options.handle(payload);
                        const added = yield* Ref.get(blocks);
                        return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                    }).pipe(Effect.provideService(ToolCall, { add: (block) => Ref.update(blocks, Array.append(block)), respond: (refusal) => Effect.asVoid(Deferred.complete(early, Effect.map(Effect.flip(orInternalError(Effect.fail(refusal))), refused))) }));
                    return yield* writes(options) ? record(options.tool.name, payload, answer, early) : answer;
                },
                instrumented('tools/call', options.tool.name, { 'gen_ai.tool.name': options.tool.name, 'gen_ai.operation.name': 'execute_tool' }),
            );
        return McpServer.McpServer.of({
            ...server,
            addTool: (options) => Effect.andThen(writes(options) ? registration : Effect.void, server.addTool({ ...options, handle: served(options) })),
            addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(instrumented('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
            addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(instrumented('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
            addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(instrumented('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
        });
    }),
);
const layerHttp = <E, R>(manifest: { readonly name: string; readonly version: string; readonly description: string }, listening: { readonly address: NetAddress.InetAddress; readonly hosts: readonly string[] }, app: Layer.Layer<never, E, R>) => {
    const allowed = new Set([...listening.hosts, 'localhost', ...[...NetAddress.loopbackAddresses, ...(NetAddress.isUnspecified(listening.address.address) ? [] : [listening.address.address])].map(NetAddress.formatUrlHost)]);
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
                    return Array.dedupe([manifest.description, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Instructions))]).join('\n');
                },
                path: '/',
                allowSessionTermination: true,
                protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
                registrations: Layer.provide(app, Layer.provideMerge(layer, Layer.succeed(Scheme)({ uri: (path) => `${manifest.name}://${path}` }))),
            }),
        ),
    ).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    return Layer.merge(transport, hostValidation);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Instructions, layer, layerHttp, orInternalError, Scheme, ToolCall, Visible };
