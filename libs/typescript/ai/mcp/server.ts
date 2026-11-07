import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, type Exit, Function, flow, Layer, Match, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaGetter, type Scope, Stream, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { FindMyWay, Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpTraceContext } from 'effect/http';
import { NetAddress } from 'effect/net';

// --- [MODELS] --------------------------------------------------------------------------

const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) }))),
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
            Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform((given: P['Type']) => Effect.provideContext(handle(given), context)), encode: SchemaGetter.forbiddenEncoding })),
            Schema.toCodecJson(tool.successSchema),
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
        yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
    });
    static readonly layer = ({ scheme, retention }: { readonly scheme: typeof Server.Scheme.Type; readonly retention: Duration.Duration }) =>
        Layer.effect(McpServer.McpServer)(
            Effect.gen(function* () {
                const [{ uri }, server, scope] = yield* Effect.all([Server, McpServer.McpServer, Effect.scope]);
                const history = yield* executions(uri, retention, server, scope);
                const templates = FindMyWay.make<string>({ ignoreTrailingSlash: true, ignoreDuplicateSlashes: true, caseSensitive: true });
                yield* Layer.build(instrumented((target) => templates.find('GET', `/${target}`)?.handler ?? target).layerGlobal);
                const ranking =
                    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
                    (input: string, context: C) =>
                        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                            const ordered = Unmatched.rank(input, values);
                            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                        });
                return McpServer.McpServer.of({
                    ...server,
                    addTool: addTool(server, history),
                    addResourceTemplate: (options) =>
                        Effect.andThen(
                            Effect.sync(() => templates.on('GET', `/${options.routerPath}`, options.template.uriTemplate)),
                            server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking) }),
                        ),
                    addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking) }),
                });
            }),
        ).pipe(Layer.provideMerge(Layer.succeed(Server)({ uri: (path) => `${scheme}://${path}` })));
    static readonly layerHttp = <E, R>(
        manifest: { readonly name: typeof Server.Scheme.Type; readonly version: string; readonly description: string },
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
const failureAttributes = Match.type<Exit.Exit<unknown, unknown>>().pipe(
    Match.when({ _tag: 'Success', value: Schema.is(Schema.Struct({ isError: Schema.Literal(true) })) }, () => ({ 'error.type': 'tool_error' })),
    Match.when({ _tag: 'Failure', cause: Cause.hasInterruptsOnly }, () => ({ 'error.type': 'cancelled' })),
    Match.when({ _tag: 'Failure' }, () => ({ 'error.type': 'error' })),
    Match.orElse(() => ({})),
);
const operations: { readonly [Method in McpServer.MiddlewareMethod]?: (target: string) => Readonly<Record<string, string>> } = {
    'tools/call': (target) => ({ 'gen_ai.tool.name': target, 'gen_ai.operation.name': 'execute_tool' }),
    'resources/read': (target) => ({ 'mcp.resource.uri': target }),
    'prompts/get': (target) => ({ 'gen_ai.prompt.name': target }),
};
const instrumented = (subject: (target: string) => string) =>
    McpServer.middleware((effect, { method, target, requestContext: { requestMetadata } }) =>
        UndefinedOr.match(operations[method], {
            onUndefined: () => effect,
            onDefined: Effect.fn(`${method} ${subject(target)}`, { kind: 'server', parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) })(function* (attributing) {
                const [duration, exit] = yield* Effect.timed(Effect.exit(effect));
                const attributes = { 'mcp.method.name': method, ...attributing(target), ...failureAttributes(exit) };
                yield* Effect.annotateCurrentSpan(attributes);
                yield* Metric.update(Metric.withAttributes(operationDuration, attributes), duration);
                return yield* exit;
            }),
        }),
    );
const executions = (uri: Server['Service']['uri'], retention: Duration.Duration, server: McpServer.McpServer['Service'], scope: Scope.Scope) => {
    const entries = Ref.makeUnsafe(Array.empty<Effect.Effect<(typeof Executions.Type)[number]>>());
    const name = 'executions';
    const descriptor = { uri: uri(name), name, description: `Cancelled or refused state-changing calls that keep running, finished calls drop after one read or ${Duration.format(retention)}`, mimeType: 'application/json' };
    const registered = Effect.cached(
        McpServer.registerResource({
            ...descriptor,
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(entries);
                const read = yield* Effect.all(rows);
                yield* Ref.update(entries, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return { contents: [{ uri: descriptor.uri, mimeType: descriptor.mimeType, text: yield* Effect.orDie(Schema.encodeEffect(Executions)(read)) }] };
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
        const [outcome, pending] = yield* pipe(
            Deferred.await(done),
            Effect.map((value) => [value, false] as const),
            Effect.raceFirst(Effect.map(Deferred.await(earlyResult), (value) => [value, true] as const)),
            Effect.interruptible,
            Effect.onInterrupt(() => addEntry),
        );
        return yield* Effect.as(pending ? addEntry : Effect.void, outcome);
    }, Effect.uninterruptible);
    return Effect.map(registered, (registration) => ({ registration, record, link: McpSchema.ResourceLink.make(descriptor) }));
};
const addTool =
    (server: McpServer.McpServer['Service'], { registration, record, link }: Effect.Success<ReturnType<typeof executions>>) =>
    (options: Parameters<McpServer.McpServer['Service']['addTool']>[0]) => {
        const writes = !Context.get(options.annotations, Tool.Readonly);
        const visible = Context.get(options.annotations, Server.Visible).pipe(
            Effect.tapCause((cause) => Effect.logError(`${options.tool.name} visibility check failed`, cause)),
            Server.orInternalError,
        );
        const handle = Effect.fnUntraced(function* (payload: Schema.Json) {
            const [blocks, earlyResult] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
            const answer = Effect.zipWith(options.handle(payload), Ref.get(blocks), (result, added) => (result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] }))).pipe(
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
