import { ranked } from '@rasm/search/ranked.ts';
import { Array, Cause, Context, Deferred, Effect, Exit, Formatter, flow, Layer, Metric, Option, Predicate, pipe, Record, Ref, type Schema } from 'effect';
import { McpProtocol, McpSchema, McpServer } from 'effect/ai';
import { Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpTraceContext } from 'effect/http';
import { NetAddress } from 'effect/net';
import { executions } from './executions.ts';

// --- [SERVICES] ------------------------------------------------------------------------

const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
const ContentBlocks = Context.Reference<(block: McpSchema.ContentBlock) => Effect.Effect<void>>('@rasm/mcp/server/ContentBlocks', { defaultValue: () => () => Effect.void });
const Respond = Context.Reference<(refusal: Error) => Effect.Effect<void>>('@rasm/mcp/server/Respond', { defaultValue: () => () => Effect.void });

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
const instrumented =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(McpSchema.McpRequestContext, ({ requestMetadata }) =>
            Effect.gen(function* () {
                const [duration, exit] = yield* Effect.timed(Effect.exit(self));
                const failure = Exit.match(exit, { onSuccess: (value) => (Predicate.hasProperty(value, 'isError') && value.isError === true ? { 'error.type': 'tool_error' } : {}), onFailure: (cause) => ({ 'error.type': Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error' }) });
                yield* Effect.annotateCurrentSpan(failure);
                yield* Metric.update(Metric.withAttributes(Metric.timer('mcp.server.operation.duration', { description: 'Duration of MCP server operations' }), { 'mcp.method.name': method, ...attributes, ...failure }), duration);
                return yield* exit;
            }).pipe(Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) })),
        );

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = (scheme: string) =>
    Layer.effect(McpServer.McpServer)(
        Effect.gen(function* () {
            const [server, { registration, record, link }] = yield* Effect.all([McpServer.McpServer, executions(scheme)]);
            const ranking =
                <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
                (input: string, context: C) =>
                    Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                        const ordered = ranked(input, values);
                        return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                    });
            const writes = ({ tool }: Parameters<typeof server.addTool>[0]) => tool.annotations?.readOnlyHint !== true;
            const refused = ({ message }: McpSchema.InternalError) => McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message }), link] });
            const served = (options: Parameters<typeof server.addTool>[0]) =>
                Effect.fnUntraced(
                    function* (payload: Schema.Json) {
                        const [blocks, early] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
                        const answer = Effect.gen(function* () {
                            const result = yield* options.handle(payload);
                            const added = yield* Ref.get(blocks);
                            return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                        }).pipe(
                            Effect.provideService(ContentBlocks, (block) => Ref.update(blocks, Array.append(block))),
                            Effect.provideService(Respond, (refusal) => Effect.asVoid(Deferred.complete(early, Effect.map(Effect.flip(orInternalError(Effect.fail(refusal))), refused)))),
                        );
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
const layerHttp = (manifest: { readonly name: string; readonly version: string; readonly description: string }, listening: { readonly address: NetAddress.InetAddress; readonly hosts: readonly string[] }) => {
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
            }),
        ),
    ).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    return Layer.merge(Layer.provide(layer(manifest.name), transport), hostValidation);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ContentBlocks, Instructions, layer, layerHttp, orInternalError, Respond };
