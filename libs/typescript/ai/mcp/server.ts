/* oxlint-disable typescript/no-deprecated -- the MCP and GenAI names moved to the GenAI semantic-conventions repository, which publishes no npm package, with values unchanged */
import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, type Exit, Fiber, Function, flow, Layer, Match, Metric, Option, Order, pipe, RcMap, Record, Ref, Schema, SchemaGetter, type Scope, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Line {
    readonly sequence: number;
    readonly event: unknown;
    readonly text: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOLS = Option.getOrThrow(Array.match(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), { onEmpty: Option.none, onNonEmpty: Option.some }));
const EXECUTIONS = { name: 'executions', description: 'Calls an event ended early, each with its result once it finishes', mimeType: 'application/json' } as const;

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: Function.constUndefined })]), McpSchema.McpError, Schema.Defect())) }))),
);
const Named = Schema.String.annotate({ description: 'Handle a result named, holding your state and your place in the events, absent on a call that starts one' });
const Given = Schema.Struct({ handle: Schema.optionalKey(Named) });

// --- [SERVICES] ------------------------------------------------------------------------

const held = (id: string, state: Ref.Ref<Context.Context<never>>) => ({
    id,
    get: <A>(key: Context.Reference<A>) => Effect.map(Ref.get(state), (context) => Context.get(context, key)),
    set: <A>(key: Context.Reference<A>, value: A) => Ref.update(state, Context.add(key, value)),
});
const Handle = Context.Reference<ReturnType<typeof held>>('@rasm/mcp/server/Handle', { defaultValue: () => held('local', Ref.makeUnsafe(Context.empty())) });
const Session = Context.Reference<Option.Option<string>>('@rasm/mcp/server/Session', { defaultValue: Option.none });

class Server extends Context.Service<
    Server,
    {
        readonly uri: (path: string) => string;
        readonly events: <A, I>(source: { readonly schema: Schema.Codec<A, I>; readonly text: (event: A) => string; readonly stream: Stream.Stream<A> }) => Effect.Effect<{ readonly ends: (holds: (event: A) => Effect.Effect<boolean>) => Context.Context<never> }, never, Scope.Scope>;
    }
>()('@rasm/mcp/server') {
    static readonly Scheme = Schema.String.pipe(
        Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
            decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
    static readonly Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visible', { defaultValue: () => Effect.succeed(true) });
    static readonly Ends = Context.Reference<(after: number) => Stream.Stream<Line>>('@rasm/mcp/server/Ends', { defaultValue: () => () => Stream.never });
    static readonly ToolCall = Context.Reference<{ readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void> }>('@rasm/mcp/server/ToolCall', { defaultValue: () => ({ add: () => Effect.void }) });
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
    static readonly layer = Effect.fnUntraced(function* ({ scheme, retention, limit }: { readonly scheme: typeof Server.Scheme.Type; readonly retention: Duration.Duration; readonly limit: number }) {
        const [server, scope, journal] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make<Effect.Effect<readonly Line[]>>(Effect.succeed([]))]);
        const latest = Effect.map(Effect.flatten(Ref.get(journal)), (lines) => Option.match(Array.last(lines), { onNone: () => 0, onSome: Struct.get('sequence') }));
        const handles = yield* RcMap.make({
            lookup: (id: string) => Effect.map(Effect.all([Ref.make(Context.empty()), Effect.flatMap(latest, Ref.make), Ref.make<readonly Effect.Effect<(typeof Executions.Type)[number]>[]>([])]), ([state, cursor, executions]) => ({ handle: held(id, state), cursor, executions })),
            idleTimeToLive: retention,
        });
        const uri = (path: string) => `${scheme}://${path}`;
        const calls = { server, scope, retention, handles, journal, latest, link: (id: string) => McpSchema.ResourceLink.make({ ...EXECUTIONS, uri: uri(`${EXECUTIONS.name}/${id}`) }) };
        yield* Layer.build(Layer.mergeAll(metered.layerGlobal, caller.layerGlobal));
        return Context.make(
            McpServer.McpServer,
            McpServer.McpServer.of({
                ...server,
                addTool: addTool(calls),
                addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking) }),
                addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking) }),
            }),
        ).pipe(Context.add(Server, Server.of({ uri, events: events({ ...calls, uri, limit }) })));
    }, Layer.effectContext);
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
    { instructions, ...settings }: Omit<Parameters<typeof Server.layer>[0], 'scheme'> & { readonly instructions: readonly string[] },
    app: Layer.Layer<never, E, R>,
    server: McpServer.McpServer['Service'],
) => ({
    ...manifest,
    get instructions() {
        return Array.dedupe([manifest.description, ...instructions, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n');
    },
    protocols: PROTOCOLS,
    registrations: Layer.provide(app, Server.layer({ scheme: manifest.name, ...settings })),
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
const caller = McpServer.middleware((effect, { sessionId, requestContext }) =>
    Effect.flatMap(Effect.map(Effect.serviceOption(HttpServerRequest.HttpServerRequest), Option.isNone), (local) =>
        Effect.provideService(
            effect,
            Session,
            Option.orElse(Option.fromUndefinedOr(sessionId), () => Option.liftPredicate(`${requestContext.clientId}`, Function.constant(local))),
        ),
    ),
);
const since = <L extends Line>(at: number, lines: readonly L[]) => [Option.match(Array.last(lines), { onNone: () => at, onSome: ({ sequence }) => Math.max(at, sequence) }), lines.filter(({ sequence }) => sequence > at)] as const;
const ranking =
    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
    (input: string, context: C) =>
        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
            const ordered = Unmatched.rank(input, values);
            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
        });
const events = ({ server, handles, journal, limit, uri }: Pick<Parameters<typeof addTool>[0], 'server' | 'handles' | 'journal'> & { readonly limit: number; readonly uri: (path: string) => string }) =>
    Effect.fnUntraced(function* <A, I>({ schema, text, stream }: { readonly schema: Schema.Codec<A, I>; readonly text: (event: A) => string; readonly stream: Stream.Stream<A> }) {
        const lines = yield* SubscriptionRef.make<readonly (Line & { readonly value: A })[]>([]);
        const encoded = Schema.encodeEffect(Schema.toCodecJson(schema));
        const root = uri(`${EXECUTIONS.name}/`);
        yield* Ref.set(journal, SubscriptionRef.get(lines));
        yield* McpServer.registerResource(
            Object.assign([root, ''], { raw: [root, ''] }),
            McpSchema.param('handle', Schema.String),
        )({
            ...EXECUTIONS,
            content: Effect.fnUntraced(function* (_: string, id: string) {
                const { executions } = yield* RcMap.get(handles, id);
                const rows = yield* Ref.get(executions);
                const read = yield* Effect.all(rows);
                yield* Ref.update(executions, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return yield* Effect.orDie(Schema.encodeEffect(Executions)(read));
            }, Effect.scoped),
        }).pipe(Effect.provideService(McpServer.McpServer, server));
        yield* stream.pipe(
            Stream.mapEffect((value) => Effect.map(Effect.orDie(encoded(value)), (event) => ({ value, event, text: text(value) }))),
            Stream.runForEach((entry) => SubscriptionRef.update(lines, (kept) => Array.takeRight(Array.append(kept, { ...entry, sequence: Option.match(Array.last(kept), { onNone: () => 1, onSome: ({ sequence }) => sequence + 1 }) }), limit))),
            Effect.forkScoped,
        );
        return { ends: (holds: (event: A) => Effect.Effect<boolean>) => Context.make(Server.Ends, (after: number) => SubscriptionRef.changes(lines).pipe(Stream.mapAccum(Function.constant(after), since), Stream.filterEffect(flow(Struct.get('value'), holds)))) };
    });
const addTool =
    ({
        server,
        scope,
        retention,
        handles,
        journal,
        latest,
        link,
    }: {
        readonly server: McpServer.McpServer['Service'];
        readonly scope: Scope.Scope;
        readonly retention: Duration.Duration;
        readonly handles: RcMap.RcMap<string, { readonly handle: ReturnType<typeof held>; readonly cursor: Ref.Ref<number>; readonly executions: Ref.Ref<readonly Effect.Effect<(typeof Executions.Type)[number]>[]> }>;
        readonly journal: Ref.Ref<Effect.Effect<readonly Line[]>>;
        readonly latest: Effect.Effect<number>;
        readonly link: (id: string) => McpSchema.ResourceLink;
    }) =>
    (options: Parameters<McpServer.McpServer['Service']['addTool']>[0]) => {
        const ends = Context.get(options.annotations, Server.Ends);
        const visible = Context.get(options.annotations, Server.Visible).pipe(
            Effect.tapCause((cause) => Effect.annotateLogs(Effect.logError('Tool visibility check failed', cause), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name })),
            Server.orInternalError,
        );
        const tool = McpSchema.Tool.make({ ...options.tool, inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, handle: Schema.toJsonSchemaDocument(Named).schema } } });
        const handle = Effect.fnUntraced(
            function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                const [{ handle: given }, session, start] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ message }) => McpSchema.InvalidParams.make({ message })), Session, latest]);
                const named = Option.orElse(Option.fromUndefinedOr(given), () => session);
                const id = yield* Option.match(named, { onNone: () => Effect.sync(() => SecureRandom.uuidV4Unsafe()), onSome: Effect.succeed });
                const [{ handle: bound, cursor, executions }, blocks, done] = yield* Effect.all([RcMap.get(handles, id), Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>()]);
                const answer = Effect.zipWith(options.handle(Record.remove(payload, 'handle')), Ref.get(blocks), (result, added) => (result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] }))).pipe(
                    Effect.provideService(Server.ToolCall, { add: (block) => Ref.update(blocks, Array.append(block)) }),
                    Effect.provideService(Handle, bound),
                );
                const running = yield* Effect.forkIn(Deferred.complete(done, answer), scope);
                const [outcome, ended] = yield* pipe(
                    Deferred.await(done),
                    Effect.map((value) => [value, false] as const),
                    Effect.raceFirst(
                        Stream.runHead(ends(start)).pipe(
                            Effect.provideService(Handle, bound),
                            Effect.flatMap(
                                Option.match({ onNone: () => Effect.never, onSome: ({ text }) => Effect.succeed([McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: `${text}, the call keeps running and its result goes to ${link(id).uri}` }), link(id)] }), true] as const) }),
                            ),
                        ),
                    ),
                    Effect.interruptible,
                    Effect.onInterrupt(() => Fiber.interrupt(running)),
                );
                const entry: Effect.Effect<(typeof Executions.Type)[number]> = Deferred.poll(done).pipe(
                    Effect.map(Option.map(Effect.exit)),
                    Effect.flatMap(Effect.transposeOption),
                    Effect.map((result) => ({ tool: options.tool.name, arguments: payload, result })),
                );
                const kept = Effect.scoped(RcMap.get(handles, id).pipe(Effect.andThen(Effect.exit(Deferred.await(done))), Effect.andThen(Effect.sleep(retention)), Effect.andThen(Ref.update(executions, Array.difference([entry])))));
                yield* ended ? Effect.andThen(Ref.update(executions, Array.append(entry)), Effect.forkIn(kept, scope)) : Effect.void;
                const lines = yield* Effect.flatten(Ref.get(journal));
                const [from, fresh] = yield* Ref.modify(cursor, (at) => {
                    const [next, unseen] = since(at, lines);
                    return [[at, unseen] as const, next];
                });
                const dropped = Option.match(Array.head(fresh), { onNone: () => 0, onSome: ({ sequence }) => sequence - from - 1 });
                const text = [
                    ...Option.match(named, { onNone: () => [`Handle ${id} holds your state and your place in the events, pass it as handle on each call`], onSome: () => [] }),
                    ...(dropped > 0 ? [`${dropped} older events dropped`] : []),
                    ...Array.map(fresh, ({ sequence, text: line }) => `#${sequence} ${line}`),
                ];
                return outcome instanceof McpSchema.InputRequired
                    ? outcome
                    : McpSchema.CallToolResult.make({
                          ...outcome,
                          content: [...outcome.content, ...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] })],
                          _meta: { ...outcome._meta, handle: id, events: Array.map(fresh, ({ sequence, event }) => ({ sequence, event })) },
                      });
            },
            Effect.uninterruptible,
            Effect.scoped,
        );
        return Effect.flatMap(Layer.buildWithScope(McpServer.visibility(() => visible).layer, scope), (gate) => server.addTool({ ...options, tool, handle, services: Context.merge(options.services ?? Context.empty(), gate) }));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
