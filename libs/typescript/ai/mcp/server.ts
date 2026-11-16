/* oxlint-disable typescript/no-deprecated -- the MCP and GenAI names moved to the GenAI semantic-conventions repository, which publishes no npm package, with values unchanged */
import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, type Exit, Fiber, Function, Layer, Match, Metric, Option, pipe, Record, Ref, Schema, SchemaGetter, type Scope, Stream, Struct, UndefinedOr } from 'effect';
import { type McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: Function.constUndefined })]), McpSchema.McpError, Schema.Defect())) }))),
);

// --- [SERVICES] ------------------------------------------------------------------------

class Server extends Context.Service<Server, { readonly uri: (path: string) => string }>()('@rasm/mcp/server') {
    static readonly Scheme = Schema.String.pipe(
        Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
            decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
    static readonly Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visible', { defaultValue: () => Effect.succeed(true) });
    static readonly ToolCall = Context.Reference<{ readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>; readonly respond: (refusal: Error) => Effect.Effect<void> }>('@rasm/mcp/server/ToolCall', { defaultValue: () => ({ add: () => Effect.void, respond: () => Effect.void }) });
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly progress = <A>(report: (value: A, index: number) => Omit<typeof McpSchema.ProgressNotification.payloadSchema.Type, 'progressToken' | '_meta'>) =>
        Effect.fnUntraced(function* <E, R>(self: Stream.Stream<A, E, R>) {
            const [server, request] = yield* Effect.all([McpServer.McpServer, Effect.serviceOption(McpSchema.McpRequestContext)]);
            const token = Option.flatMap(request, ({ requestMetadata }) => Schema.decodeUnknownOption(McpSchema.ProgressToken)(requestMetadata?.progressToken));
            return Option.match(token, { onNone: () => self, onSome: (progressToken) => Stream.mapEffect(self, (value, index) => Effect.as(server.notifications['notifications/progress']({ progressToken, ...report(value, index) }), value)) });
        }, Stream.unwrap);
    static readonly registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>,
    ) {
        const context = yield* Effect.context<R>();
        const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
            tool.parametersSchema.pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform((given: P['Type']) => Effect.provideContext(handle(given), context)), encode: SchemaGetter.forbiddenEncoding })),
            tool.successSchema,
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
        yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
    });
    static readonly layer = ({ scheme, executions: settings }: { readonly scheme: typeof Server.Scheme.Type; readonly executions: { readonly name: string; readonly description: string; readonly retention: Duration.Duration } }) =>
        Layer.effect(McpServer.McpServer)(
            Effect.gen(function* () {
                const [{ uri }, server, scope] = yield* Effect.all([Server, McpServer.McpServer, Effect.scope]);
                const history = yield* executions({ ...settings, uri: uri(settings.name) }, server, scope);
                yield* Layer.build(metered.layerGlobal);
                return McpServer.McpServer.of({
                    ...server,
                    addTool: addTool(server, history),
                    addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking) }),
                    addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking) }),
                });
            }),
        ).pipe(Layer.provideMerge(Layer.succeed(Server)({ uri: (path) => `${scheme}://${path}` })));
    static readonly layerStdio = <E, R>(manifest: Parameters<typeof served>[0], settings: Parameters<typeof served>[1], app: Layer.Layer<never, E, R>) =>
        Layer.unwrap(Effect.map(McpServer.McpServer, (server) => McpServer.layerStdio(served(manifest, settings, app, server)))).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    static readonly layerHttp = <E, R>(manifest: Parameters<typeof served>[0], { address, hosts, ...settings }: Parameters<typeof served>[1] & { readonly address: Host.HostPort; readonly hosts: readonly string[] }, app: Layer.Layer<never, E, R>) => {
        const [names, bound] = [['localhost', ...Array.map(NetAddress.loopbackAddresses, NetAddress.formatUrlHost)], Host.formatHostPort(address)];
        const allowed = new Set([...hosts, ...names, bound]);
        const hostValidation = HttpRouter.middleware((next) => Effect.flatMap(HttpServerRequest.HttpServerRequest, (request) => (Option.exists(HttpServerRequest.toURL(request), ({ host, hostname }) => allowed.has(host) || allowed.has(hostname)) ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
            global: true,
        });
        const transport = Layer.unwrap(
            Effect.map(McpServer.McpServer, (server) =>
                McpServer.layerHttp({
                    ...served(manifest, settings, app, server),
                    path: '/',
                    allowSessionTermination: true,
                    allowedOrigins: Array.flatMap(['http', 'https'], (scheme) => Array.map([...hosts, ...Array.map([...hosts, ...names], (name) => `${name}:${address.port}`), bound], (host) => `${scheme}://${host}`)),
                }),
            ),
        );
        return Layer.merge(transport.pipe(Layer.provideMerge(McpServer.McpServer.layer)), hostValidation);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = <E, R>(
    manifest: { readonly name: typeof Server.Scheme.Type; readonly version: string; readonly description: string },
    { executions, instructions, protocols }: { readonly executions: Parameters<typeof Server.layer>[0]['executions']; readonly instructions: readonly string[]; readonly protocols: Array.NonEmptyReadonlyArray<McpProtocol.ProtocolAdapter> },
    app: Layer.Layer<never, E, R>,
    server: McpServer.McpServer['Service'],
) => ({
    ...manifest,
    get instructions() {
        return Array.dedupe([manifest.description, ...instructions, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n');
    },
    protocols,
    registrations: Layer.provide(app, Server.layer({ scheme: manifest.name, executions })),
});
const operationDuration = Metric.mapInput(
    Metric.histogram(METRIC_MCP_SERVER_OPERATION_DURATION, {
        description: 'Duration of MCP server operations',
        boundaries: Array.map(['10 millis', '20 millis', '50 millis', '100 millis', '200 millis', '500 millis', '1 second', '2 seconds', '5 seconds', '10 seconds', '30 seconds', '1 minute', '2 minutes', '5 minutes'] as const, Duration.toSeconds),
    }),
    Duration.toSeconds,
);
const failureAttributes = Match.type<Exit.Exit<unknown, unknown>>().pipe(
    Match.when({ _tag: 'Success', value: Schema.is(Schema.Struct({ isError: Schema.Literal(true) })) }, () => ({ [ATTR_ERROR_TYPE]: 'tool_error' })),
    Match.when({ _tag: 'Failure', cause: Cause.hasInterruptsOnly }, () => ({ [ATTR_ERROR_TYPE]: 'cancelled' })),
    Match.when({ _tag: 'Failure' }, ({ cause }) => ({ [ATTR_ERROR_TYPE]: Option.match(Schema.decodeUnknownOption(Tagged)(Cause.squash(cause)), { onNone: () => ERROR_TYPE_VALUE_OTHER, onSome: (failure) => failure._tag }) })),
    Match.orElse(() => ({})),
);
const operations: Readonly<Partial<Record<McpServer.MiddlewareMethod, (target: string) => Readonly<Record<string, string>>>>> = {
    'tools/call': (target) => ({ [ATTR_GEN_AI_TOOL_NAME]: target, [ATTR_GEN_AI_OPERATION_NAME]: GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL }),
    'resources/read': (target) => ({ [ATTR_MCP_RESOURCE_URI]: target }),
    'prompts/get': (target) => ({ [ATTR_GEN_AI_PROMPT_NAME]: target }),
};
const metered = McpServer.middleware((effect, { method, target }) =>
    UndefinedOr.match(operations[method], {
        onUndefined: () => effect,
        onDefined: Effect.fnUntraced(function* (attributing) {
            const [duration, exit] = yield* Effect.timed(Effect.exit(effect));
            yield* Metric.update(Metric.withAttributes(operationDuration, { [ATTR_MCP_METHOD_NAME]: method, ...attributing(target), ...failureAttributes(exit) }), duration);
            return yield* exit;
        }),
    }),
);
const ranking =
    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
    (input: string, context: C) =>
        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
            const ordered = Unmatched.rank(input, values);
            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
        });
const executions = ({ retention, ...descriptor }: Parameters<typeof Server.layer>[0]['executions'] & { readonly uri: string }, server: McpServer.McpServer['Service'], scope: Scope.Scope) => {
    const entries = Ref.makeUnsafe(Array.empty<Effect.Effect<(typeof Executions.Type)[number]>>());
    const listed = { ...descriptor, mimeType: 'application/json' };
    const registered = Effect.cached(
        McpServer.registerResource({
            ...listed,
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(entries);
                const read = yield* Effect.all(rows);
                yield* Ref.update(entries, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return yield* Effect.orDie(Schema.encodeEffect(Executions)(read));
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
        const running = yield* Effect.forkIn(Deferred.complete(done, answer), scope);
        const [outcome, pending] = yield* pipe(
            Deferred.await(done),
            Effect.map((value) => [value, false] as const),
            Effect.raceFirst(Effect.map(Deferred.await(earlyResult), (value) => [value, true] as const)),
            Effect.interruptible,
            Effect.onInterrupt(() => Fiber.interrupt(running)),
        );
        const dropped = Deferred.await(done).pipe(Effect.exit, Effect.andThen(Effect.sleep(retention)), Effect.andThen(Ref.update(entries, Array.difference([entry]))));
        return yield* Effect.as(pending ? Effect.andThen(Ref.update(entries, Array.append(entry)), Effect.forkIn(dropped, scope)) : Effect.void, outcome);
    }, Effect.uninterruptible);
    return Effect.map(registered, (registration) => ({ registration, record, link: McpSchema.ResourceLink.make(listed) }));
};
const addTool =
    (server: McpServer.McpServer['Service'], { registration, record, link }: Effect.Success<ReturnType<typeof executions>>) =>
    (options: Parameters<McpServer.McpServer['Service']['addTool']>[0]) => {
        const writes = !Context.get(options.annotations, Tool.Readonly);
        const visible = Context.get(options.annotations, Server.Visible).pipe(
            Effect.tapCause((cause) => Effect.annotateLogs(Effect.logError('Tool visibility check failed', cause), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name })),
            Server.orInternalError,
        );
        const handle = Effect.fnUntraced(function* (payload: Schema.Json) {
            const [blocks, earlyResult] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
            const answer = Effect.zipWith(options.handle(payload), Ref.get(blocks), (result, added) => (result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make(Struct.evolve(result, { content: Array.appendAll(added) })))).pipe(
                Effect.provideService(Server.ToolCall, {
                    add: (block) => Ref.update(blocks, Array.append(block)),
                    respond: (refusal) => Effect.asVoid(Deferred.succeed(earlyResult, McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: refusal.message }), link] }))),
                }),
            );
            return yield* writes ? record(options.tool.name, payload, answer, earlyResult) : answer;
        });
        return Effect.andThen(
            writes ? registration : Effect.void,
            Effect.flatMap(Effect.scoped(Layer.build(McpServer.visibility(() => visible).layer)), (gate) => server.addTool({ ...options, handle, services: Context.merge(options.services ?? Context.empty(), gate) })),
        );
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Server };
