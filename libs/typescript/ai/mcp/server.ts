/* oxlint-disable typescript/no-deprecated -- the MCP and GenAI names moved to the GenAI semantic-conventions repository, which publishes no npm package, with values unchanged */
import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, type Exit, Fiber, Function, flow, Layer, Match, Metric, Option, Order, pipe, RcMap, Record, Ref, Schema, SchemaGetter, type Scope, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Line {
    readonly sequence: number;
    readonly event: unknown;
    readonly text: string;
}
interface Holder {
    readonly handle: ReturnType<typeof held>;
    readonly cursor: Ref.Ref<number>;
    readonly executions: Ref.Ref<readonly Effect.Effect<(typeof Executions.Type)[number]>[]>;
}
type Progressed = Omit<typeof McpSchema.ProgressNotification.payloadSchema.Type, 'progressToken' | '_meta'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOLS = Option.getOrThrow(Array.match(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), { onEmpty: Option.none, onNonEmpty: Option.some }));
const EXECUTIONS = { name: 'executions', description: 'Calls an event ended early, each with its result once it finishes', mimeType: 'application/json' } as const;
const PROGRESS = Duration.minutes(1);
const SEALING = Aead.xchacha20Poly1305;

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Executions = Schema.fromJsonString(
    Schema.toCodecJson(Schema.Array(Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: Function.constUndefined })]), McpSchema.McpError, Schema.Defect())) }))),
);
const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String) });
const Trailer = Schema.Struct({
    handle: Schema.String.annotate({ description: 'Handle to pass on each later call' }),
    dropped: Schema.optionalKey(Schema.Int.annotate({ description: 'Events before the first listed that dropped unread' })),
    events: Schema.Array(Schema.Struct({ sequence: Schema.Int, text: Schema.String, event: Schema.Json })).annotate({ description: 'Events since the last call with this handle' }),
});
const Resumed = Schema.fromJsonString(Schema.Struct({ handle: Schema.String, state: Schema.optional(Schema.String) }));

// --- [SERVICES] ------------------------------------------------------------------------

const held = (state: Ref.Ref<Context.Context<never>>) => ({
    get: <A>(key: Context.Reference<A>) => Effect.map(Ref.get(state), (context) => Context.get(context, key)),
    set: <A>(key: Context.Reference<A>, value: A) => Ref.update(state, Context.add(key, value)),
});
const Handle = Context.Reference<ReturnType<typeof held>>('@rasm/mcp/server/Handle', { defaultValue: () => held(Ref.makeUnsafe(Context.empty())) });

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
    static readonly Icons = Context.Reference<readonly McpSchema.Icon[]>('@rasm/mcp/server/Icons', { defaultValue: () => [] });
    static readonly Ends = Context.Reference<(after: number) => Stream.Stream<Line>>('@rasm/mcp/server/Ends', { defaultValue: () => () => Stream.never });
    static readonly ToolCall = Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly progress: (report: Progressed) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
    }>('@rasm/mcp/server/ToolCall', {
        defaultValue: () => ({ add: () => Effect.void, progress: () => Effect.void, input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })) }),
    });
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly progress =
        <A>(report: (value: A, index: number) => Progressed) =>
        <E, R>(self: Stream.Stream<A, E, R>) =>
            Stream.mapEffect(self, (value, index) =>
                Effect.as(
                    Server.ToolCall.use(({ progress }) => progress(report(value, index))),
                    value,
                ),
            );
    static readonly registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: typeof Schema.Never; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>,
    ) {
        const context = yield* Effect.context<Exclude<R, McpSchema.McpRequestContext>>();
        const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error, McpSchema.McpRequestContext>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
            tool.parametersSchema.pipe(
                Schema.decodeTo(Schema.Any, {
                    decode: SchemaGetter.transform((given: P['Type']) => Effect.flatMap(Effect.context<McpSchema.McpRequestContext>(), (request) => Effect.provideContext(handle(given), Context.merge(context, request)))),
                    encode: SchemaGetter.forbiddenEncoding,
                }),
            ),
            tool.successSchema,
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance(), dependencies: [McpSchema.McpRequestContext] }).annotateMerge(tool.annotations));
        yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
    });
    static readonly layer = Effect.fnUntraced(function* ({ scheme, retention, limit }: { readonly scheme: typeof Server.Scheme.Type; readonly retention: Duration.Duration; readonly limit: number }) {
        const [server, scope, journal, aead, random] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make<Effect.Effect<readonly Line[]>>(Effect.succeed([])), Aead.Aead, SecureRandom.SecureRandom]);
        const [key, trailer] = yield* Effect.orDie(Effect.all([aead.generateKey(SEALING), Schema.decodeUnknownEffect(McpSchema.ToolJson)(Schema.toJsonSchemaDocument(Trailer).schema)]));
        const latest = Effect.map(Effect.flatten(Ref.get(journal)), (lines) => Option.match(Array.last(lines), { onNone: () => 0, onSome: Struct.get('sequence') }));
        const handles = yield* RcMap.make({
            lookup: (_: string): Effect.Effect<Holder> => Effect.map(Effect.all([Ref.make(Context.empty()), Effect.flatMap(latest, Ref.make), Ref.make<readonly Effect.Effect<(typeof Executions.Type)[number]>[]>([])]), ([state, cursor, executions]) => ({ handle: held(state), cursor, executions })),
            idleTimeToLive: retention,
        });
        const uri = (path: string) => `${scheme}://${path}`;
        const seal = Effect.fnUntraced(function* (tool: string, resumed: typeof Resumed.Type) {
            const [nonce, plain] = yield* Effect.all([random.randomBytes(SEALING.nonceLength), Effect.orDie(Schema.encodeEffect(Resumed)(resumed))]);
            const sealed = yield* Effect.orDie(aead.encrypt(key, new TextEncoder().encode(plain), { nonce, aad: new TextEncoder().encode(tool) }));
            return yield* Effect.orDie(Schema.encodeEffect(Schema.Uint8ArrayFromBase64Url)(new Uint8Array([...nonce, ...sealed])));
        });
        const open = Effect.fnUntraced(
            function* (tool: string, state: string) {
                const bytes = yield* Schema.decodeEffect(Schema.Uint8ArrayFromBase64Url)(state);
                const plain = yield* aead.decrypt(key, bytes.subarray(SEALING.nonceLength), { nonce: bytes.subarray(0, SEALING.nonceLength), aad: new TextEncoder().encode(tool) });
                return yield* Effect.orDie(Schema.decodeEffect(Resumed)(new TextDecoder().decode(plain)));
            },
            Effect.mapError(() => McpSchema.InvalidParams.make({ message: 'requestState came from no input request of this server and tool, call again without it' })),
        );
        const early = (id: string, text: string) => McpSchema.CallToolResult.make({ content: [McpSchema.ResourceLink.make({ ...EXECUTIONS, uri: uri(`${EXECUTIONS.name}/${id}`), description: `${text}, the call keeps running and its result goes here` })] });
        const calls = { server, scope, retention, handles, journal, latest, trailer, early, seal, open, mint: random.uuidV4() };
        yield* Layer.build(metered.layerGlobal);
        return Context.make(
            McpServer.McpServer,
            McpServer.McpServer.of({
                ...server,
                addTool: addTool(calls),
                addResource: (options) => server.addResource({ ...options, resource: McpSchema.Resource.make({ ...options.resource, icons: iconed(options.annotations, options.resource.icons) }) }),
                addResourceTemplate: (options) => server.addResourceTemplate({ ...options, template: McpSchema.ResourceTemplate.make({ ...options.template, icons: iconed(options.annotations, options.template.icons) }), completions: Record.map(options.completions, ranking) }),
                addPrompt: (options) => server.addPrompt({ ...options, prompt: McpSchema.Prompt.make({ ...options.prompt, icons: iconed(options.annotations, options.prompt.icons) }), completions: Record.map(options.completions, ranking) }),
            }),
        ).pipe(Context.add(Server, Server.of({ uri, events: events({ ...calls, uri, limit }) })));
    }, Layer.effectContext);
    static readonly layerStdio = <E, R>(manifest: Parameters<typeof served>[0], settings: Parameters<typeof served>[1], app: Layer.Layer<never, E, R>) => Layer.unwrap(Effect.map(served(manifest, settings, app), (options) => McpServer.layerStdio(options))).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    static readonly layerHttp = <E, R>(manifest: Parameters<typeof served>[0], { address, hosts, ...settings }: Parameters<typeof served>[1] & { readonly address: Host.HostPort; readonly hosts: readonly string[] }, app: Layer.Layer<never, E, R>) => {
        const [names, bound] = [['localhost', ...Array.map(NetAddress.loopbackAddresses, NetAddress.formatUrlHost)], Host.formatHostPort(address)];
        const allowed = new Set([...hosts, ...names, bound]);
        const hostValidation = HttpRouter.middleware((next) => Effect.flatMap(HttpServerRequest.HttpServerRequest, (request) => (Option.exists(HttpServerRequest.toURL(request), ({ host, hostname }) => allowed.has(host) || allowed.has(hostname)) ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
            global: true,
        });
        const transport = Layer.unwrap(
            Effect.map(served(manifest, settings, app), (options) =>
                McpServer.layerHttp({
                    ...options,
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

const served = Effect.fnUntraced(function* <E, R>(
    manifest: { readonly name: typeof Server.Scheme.Type } & Pick<Parameters<typeof McpServer.layerHttp>[0], 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'>,
    { instructions, ...settings }: Omit<Parameters<typeof Server.layer>[0], 'scheme'> & { readonly instructions: readonly string[] },
    app: Layer.Layer<never, E, R>,
) {
    const server = yield* McpServer.McpServer;
    yield* Layer.build(Layer.provide(app, Server.layer({ scheme: manifest.name, ...settings })));
    const shown = yield* Effect.filter([...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts], ({ annotations }) => Context.get(annotations, Server.Visible));
    return { ...manifest, protocols: PROTOCOLS, instructions: Array.dedupe([...instructions, ...Array.flatMap(shown, ({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n') };
});
const iconed = (annotations: Context.Context<never>, own: readonly McpSchema.Icon[] | undefined) => Array.match(Context.get(annotations, Server.Icons), { onEmpty: () => own, onNonEmpty: Function.identity });
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
                const [found, missing] = [yield* RcMap.getOption(handles, id), Function.constant(new Cause.NoSuchElementError(`Handle ${id} is unknown or expired`))];
                const { executions } = yield* Effect.fromOption(found, missing);
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
        trailer,
        early,
        seal,
        open,
        mint,
    }: {
        readonly server: McpServer.McpServer['Service'];
        readonly scope: Scope.Scope;
        readonly retention: Duration.Duration;
        readonly handles: RcMap.RcMap<string, Holder>;
        readonly journal: Ref.Ref<Effect.Effect<readonly Line[]>>;
        readonly latest: Effect.Effect<number>;
        readonly trailer: McpSchema.ToolJson;
        readonly early: (id: string, text: string) => McpSchema.CallToolResult;
        readonly seal: (tool: string, resumed: typeof Resumed.Type) => Effect.Effect<string>;
        readonly open: (tool: string, state: string) => Effect.Effect<typeof Resumed.Type, McpSchema.InvalidParams>;
        readonly mint: Effect.Effect<string>;
    }) =>
    (options: Parameters<McpServer.McpServer['Service']['addTool']>[0]) => {
        const ends = Context.get(options.annotations, Server.Ends);
        const visible = Context.get(options.annotations, Server.Visible).pipe(
            Effect.tapCause((cause) => Effect.annotateLogs(Effect.logError('Tool visibility check failed', cause), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name })),
            Server.orInternalError,
        );
        const Named = Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${Duration.format(retention)} without a call` });
        const { $defs, ...output } = options.tool.outputSchema ?? {};
        const tool = McpSchema.Tool.make({
            ...options.tool,
            icons: iconed(options.annotations, options.tool.icons),
            inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, handle: Schema.toJsonSchemaDocument(Named).schema } },
            outputSchema: { ...trailer, properties: { result: output, ...trailer.properties }, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr($defs), (definitions) => ({ $defs: definitions }))) },
        });
        const called = Effect.fnUntraced(function* (payload: Record.ReadonlyRecord<string, Schema.Json>, { id, minted, state, row: { handle: bound, cursor, executions } }: { readonly id: string; readonly minted: boolean; readonly state: Option.Option<string>; readonly row: Holder }) {
            const [request, start, blocks, done, asked, pace] = yield* Effect.all([
                McpSchema.McpRequestContext,
                latest,
                Ref.make(Array.empty<McpSchema.ContentBlock>()),
                Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>(),
                Deferred.make<McpSchema.InputRequired>(),
                Ref.make({ last: -1, offset: 0, quiet: true }),
            ]);
            const token = Schema.decodeUnknownOption(McpSchema.ProgressToken)(request.requestMetadata?.progressToken);
            const notify = (report: Progressed) => Option.match(token, { onNone: () => Effect.void, onSome: (progressToken) => server.notifications['notifications/progress']({ progressToken, ...report }) });
            const beat = Effect.flatMap(
                Ref.modify(pace, ({ last, offset, quiet }) => (quiet ? [Option.some(last + 1), { last: last + 1, offset: offset + 1, quiet }] : [Option.none(), { last, offset, quiet: true }])),
                Option.match({ onNone: () => Effect.void, onSome: (progress) => notify({ progress }) }),
            );
            const call = {
                add: (block: McpSchema.ContentBlock) => Ref.update(blocks, Array.append(block)),
                progress: (report: Progressed) =>
                    Effect.flatMap(
                        Ref.modify(pace, ({ offset }) => [
                            { ...report, progress: report.progress + offset, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(report.total), (total) => ({ total: total + offset }))) },
                            { last: report.progress + offset, offset, quiet: false },
                        ]),
                        notify,
                    ),
                input: (required: McpSchema.InputRequired) => Effect.andThen(Deferred.succeed(asked, required), Effect.never),
            };
            const answer = Effect.raceFirst(
                Effect.zipWith(options.handle(Record.remove(payload, 'handle')), Ref.get(blocks), (outcome, added) => (outcome instanceof McpSchema.InputRequired ? outcome : McpSchema.CallToolResult.make({ ...outcome, content: [...outcome.content, ...added] }))),
                Deferred.await(asked),
            ).pipe(
                Effect.provideService(Server.ToolCall, call),
                Effect.provideService(Handle, bound),
                Effect.updateService(McpSchema.McpRequestContext, (context) => ({ ...context, requestState: Option.getOrUndefined(state) })),
                Effect.scoped,
            );
            const running = yield* Effect.forkIn(Deferred.complete(done, answer), scope);
            const [outcome, ended] = yield* pipe(
                Deferred.await(done),
                Effect.map((value) => [value, false] as const),
                Effect.raceFirst(Stream.runHead(ends(start)).pipe(Effect.provideService(Handle, bound), Effect.flatMap(Option.match({ onNone: () => Effect.never, onSome: ({ text }) => Effect.succeed([early(id, text), true] as const) })))),
                Effect.raceFirst(Option.match(token, { onNone: () => Effect.never, onSome: () => Effect.forever(Effect.andThen(Effect.sleep(PROGRESS), beat)) })),
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
            const dropped = Option.filter(
                Option.map(Array.head(fresh), ({ sequence }) => sequence - from - 1),
                (count) => count > 0,
            );
            const text = [...(minted ? [`Handle ${id} holds your state and your place in the events, pass it as handle on each call`] : []), ...Option.toArray(Option.map(dropped, (count) => `${count} older events dropped`)), ...Array.map(fresh, ({ sequence, text: line }) => `#${sequence} ${line}`)];
            const mirrored = (content: readonly McpSchema.ContentBlock[]) => [...content, ...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] })];
            return yield* outcome instanceof McpSchema.InputRequired
                ? Effect.map(seal(options.tool.name, { handle: id, state: outcome.requestState }), (requestState) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState }))
                : Effect.succeed(
                      McpSchema.CallToolResult.make({
                          ...outcome,
                          content: mirrored(outcome.content),
                          ...(outcome.isError === true
                              ? {}
                              : {
                                    structuredContent: {
                                        ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(outcome.structuredContent), (value) => ({ result: value }))),
                                        handle: id,
                                        ...Option.getOrUndefined(Option.map(dropped, (count) => ({ dropped: count }))),
                                        events: Array.map(fresh, Struct.pick(['sequence', 'text', 'event'])),
                                    },
                                }),
                      }),
                  );
        });
        const handle = Effect.fnUntraced(
            function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                const [{ handle: given }, request] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ message }) => McpSchema.InvalidParams.make({ message })), McpSchema.McpRequestContext]);
                const resumed = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(request.requestState), (sealed) => open(options.tool.name, sealed)));
                const named = Option.orElse(Option.map(resumed, Struct.get('handle')), () => Option.fromUndefinedOr(given));
                const id = yield* Option.match(named, { onNone: () => mint, onSome: Effect.succeed });
                const row = yield* Option.match(named, { onNone: () => Effect.asSome(RcMap.get(handles, id)), onSome: () => RcMap.getOption(handles, id) });
                return yield* Option.match(row, {
                    onNone: () => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: `Handle ${id} is unknown or expired after ${Duration.format(retention)} without a call, call again without handle to start one` })] })),
                    onSome: (found) => called(payload, { id, minted: Option.isNone(named), state: Option.flatMap(resumed, ({ state }) => Option.fromUndefinedOr(state)), row: found }),
                });
            },
            Effect.uninterruptible,
            Effect.scoped,
        );
        return Effect.flatMap(Layer.buildWithScope(McpServer.visibility(() => visible).layer, scope), (gate) => server.addTool({ ...options, tool, handle, services: Context.merge(options.services ?? Context.empty(), gate) }));
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
