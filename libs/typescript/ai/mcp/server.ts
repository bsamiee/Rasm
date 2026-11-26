import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Boolean, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, FiberSet, Function, Layer, Match, Metric, Number, Option, Order, Predicate, RcMap, Record, Redacted, Ref, Result, Schema, SchemaGetter, type Scope, Stream, Struct, SubscriptionRef, TxRef, TxSubscriptionRef, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { type Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { Jwk, Jwt } from 'effect/jose';
import { Host, NetAddress } from 'effect/net';
import { Handles } from '#/handles';

// --- [TYPES] ---------------------------------------------------------------------------

interface HandleState {
    readonly state: Context.Context<never>;
    readonly dropped: number;
    readonly entries: readonly { readonly sequence: number; readonly text: Effect.Effect<string>; readonly owns: (state: Context.Context<never>) => boolean }[];
    readonly held: HandleState['entries'];
    readonly calls: number;
    readonly answering: number;
    readonly running: readonly { readonly call: number; readonly tool: string; readonly fiber: Fiber.Fiber<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>; readonly ends: Stream.Stream<Effect.Effect<string>> }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRESH: HandleState = { state: Context.empty(), dropped: 0, entries: [], held: [], calls: 0, answering: 0, running: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Resumed = Schema.Struct({ handle: Schema.String, state: Schema.OptionFromOptionalKey(Schema.String), aud: Schema.String });
const isUuidV7 = Schema.is(Schema.String.check(Schema.isUUID(7)));

// --- [SERVICES] ------------------------------------------------------------------------

const handleOf = (row: TxSubscriptionRef.TxSubscriptionRef<HandleState>) => ({
    get: <A>(key: Context.Reference<A>) => Effect.map(TxSubscriptionRef.get(row), ({ state }) => Context.get(state, key)),
    set: <A>(key: Context.Reference<A>, value: A) => TxSubscriptionRef.update(row, (slot) => ({ ...slot, state: Context.add(slot.state, key, value) })),
});
const Handle = Context.Reference<ReturnType<typeof handleOf>>('@rasm/mcp/server/Handle', { defaultValue: () => handleOf(Effect.runSync(TxSubscriptionRef.make(FRESH))) });

class Server extends Context.Service<
    Server,
    {
        readonly uri: (path: string) => string;
        readonly events: <A>(source: { readonly stream: Stream.Stream<A>; readonly render: (event: A) => Effect.Effect<string>; readonly owns?: (event: A, state: Context.Context<never>) => boolean }) => Effect.Effect<void, never, Scope.Scope>;
        readonly notifyListChanged: Effect.Effect<void, Error>;
        readonly bind: Effect.Effect<typeof Handles.Bound.Type>;
        readonly follow: (handle: string) => Effect.Effect<Stream.Stream<typeof Handles.Followed.Type, Cause.NoSuchElementError>, Cause.NoSuchElementError>;
        readonly ack: (handle: string, through: number) => Effect.Effect<void, Cause.NoSuchElementError>;
    }
>()('@rasm/mcp/server') {
    static readonly protocols = Option.getOrThrow(Option.liftPredicate(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), Array.isArrayNonEmpty));
    static readonly scheme = Schema.decodeEffect(
        Schema.String.pipe(
            Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
                decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
                encode: SchemaGetter.forbiddenEncoding,
            }),
        ),
    );
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
    static readonly Visibility = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visibility', { defaultValue: () => Effect.succeed(true) });
    static readonly Icons = Context.Reference<readonly McpSchema.Icon[]>('@rasm/mcp/server/Icons', { defaultValue: () => [] });
    static readonly EarlyReturn = Context.Reference<Stream.Stream<Effect.Effect<string>>>('@rasm/mcp/server/EarlyReturn', { defaultValue: () => Stream.never });
    static readonly ToolCall = Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly state: (line: string) => Effect.Effect<void>;
        readonly progress: (report: Omit<Parameters<McpServer.McpServer['Service']['notifications']['notifications/progress']>[0], 'progressToken' | '_meta'>) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
        readonly awaitRunning: Effect.Effect<void>;
    }>('@rasm/mcp/server/ToolCall', {
        defaultValue: () => ({ add: () => Effect.void, state: () => Effect.void, progress: () => Effect.void, input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })), awaitRunning: Effect.void }),
    });
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly progress =
        <A>(report: (value: A, index: number) => Parameters<(typeof Server.ToolCall)['Service']['progress']>[0]) =>
        <E, R>(self: Stream.Stream<A, E, R>) =>
            Stream.mapEffect(self, (value, index) => Server.ToolCall.use(({ progress }) => Effect.as(progress(report(value, index)), value)));
    static readonly registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Never; readonly failureMode: Tool.FailureMode }>,
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
    static readonly layer = Effect.fnUntraced(function* ({ scheme, retention, limit, budget }: { readonly scheme: Effect.Success<ReturnType<typeof Server.scheme>>; readonly retention: Duration.Duration; readonly limit: number; readonly budget: number }) {
        const [server, scope, live, sequence, random, crypto] = yield* Effect.all([McpServer.McpServer, Effect.scope, TxRef.make<Record.ReadonlyRecord<string, TxSubscriptionRef.TxSubscriptionRef<HandleState>>>({}), TxRef.make(0), SecureRandom.SecureRandom, Effect.context<Aead.Aead | SecureRandom.SecureRandom>()]);
        const [key, epoch] = yield* Effect.all([Effect.orDie(Effect.flatMap(Jwk.generateSecret('dir'), (secret) => Jwk.importSecret(secret, 'dir'))), random.uuidV7()]);
        const handles = yield* RcMap.make({
            lookup: (id: string) => Effect.tap(Effect.all({ id: Effect.succeed(id), row: TxSubscriptionRef.make(FRESH), fibers: FiberSet.make() }), ({ row }) => Effect.acquireRelease(TxRef.update(live, Record.set(id, row)), () => TxRef.update(live, Record.remove(id)))),
            idleTimeToLive: retention,
        });
        const kept = Duration.format(retention);
        const findHandle = (known: string) =>
            RcMap.getOption(handles, known).pipe(Effect.flatMap(Effect.fromOption(() => new Cause.NoSuchElementError(`Handle ${known} ${isUuidV7(known) && known < epoch ? 'predates a restart' : `is unknown or expired after ${kept} without a call`}, call again without handle to start one`))));
        const issueHandle = Effect.flatMap(random.uuidV7(), (id) => RcMap.get(handles, id));
        const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${kept} without a call` })) });
        const added = Record.map(Given.fields, (field) => Schema.toJsonSchemaDocument(field).schema);
        const open = (tool: string, sealed: string) =>
            Effect.provideContext(Effect.mapBoth(Jwt.decrypt(Resumed, sealed, key, { audience: tool }), { onFailure: () => McpSchema.InvalidParams.make({ message: 'requestState came from no input request of this server and tool, call again without it' }), onSuccess: ({ claims }) => claims }), crypto);
        const gate = (subject: Readonly<Record<string, string>>, { annotations, services = Context.empty() }: Pick<Parameters<McpServer.McpServer['Service']['addResource']>[0], 'annotations' | 'services'>) =>
            Effect.map(Layer.buildWithScope(McpServer.visibility(() => Server.orInternalError(Effect.tapCause(Context.get(annotations, Server.Visibility), (cause) => Effect.annotateLogs(Effect.logError('Visibility check failed', cause), subject)))).layer, scope), (gated) => Context.merge(services, gated));
        const listed = yield* Ref.make<Record.ReadonlyRecord<'tools' | 'resources' | 'prompts', Record.ReadonlyRecord<string, boolean>>>({ tools: {}, resources: {}, prompts: {} });
        const notifyListChanged = Effect.gen(function* () {
            const now = yield* Effect.all({
                tools: Effect.all(Record.fromIterableWith(server.tools, ({ tool, annotations }) => [tool.name, Context.get(annotations, Server.Visibility)])),
                resources: Effect.all({
                    ...Record.fromIterableWith(server.resources, ({ resource, annotations }) => [resource.uri, Context.get(annotations, Server.Visibility)]),
                    ...Record.fromIterableWith(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, Context.get(annotations, Server.Visibility)]),
                }),
                prompts: Effect.all(Record.fromIterableWith(server.prompts, ({ prompt, annotations }) => [prompt.name, Context.get(annotations, Server.Visibility)])),
            });
            const before = yield* Ref.getAndSet(listed, now);
            yield* Effect.forEach(Record.keys(now), (kind) => (Record.some(Record.intersection(now[kind], before[kind], Boolean.xor), Function.identity) ? server.notifications[`notifications/${kind}/list_changed`]({}) : Effect.void), { discard: true });
        });
        yield* Layer.build(metrics.layerGlobal);
        const gatedServer = McpServer.McpServer.of({
            ...server,
            addTool: (options) => {
                const ends = Context.get(options.annotations, Server.EarlyReturn);
                const tool = McpSchema.Tool.make({ ...Struct.omit(options.tool, ['outputSchema']), icons: withIcons(options.annotations, options.tool.icons), inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, ...added } } });
                const handle = Effect.fnUntraced(
                    function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                        const [{ handle: given }, request] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ message }) => McpSchema.InvalidParams.make({ message })), McpSchema.McpRequestContext]);
                        const resumed = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(request.requestState), (sealed) => open(options.tool.name, sealed)));
                        const { id, row, fibers } = yield* Option.match(Option.firstSomeOf([Option.map(resumed, Struct.get('handle')), Option.fromUndefinedOr(given)]), {
                            onNone: Function.constant(issueHandle),
                            onSome: findHandle,
                        });
                        yield* Effect.acquireRelease(TxSubscriptionRef.update(row, Struct.evolve({ answering: Number.increment })), () => TxSubscriptionRef.update(row, Struct.evolve({ answering: Number.decrement })));
                        const [blocks, stated, asked, pace, frozen] = yield* Effect.all([
                            Ref.make(Array.empty<McpSchema.ContentBlock>()),
                            Ref.make(Array.empty<McpSchema.TextContent>()),
                            Deferred.make<McpSchema.InputRequired>(),
                            SubscriptionRef.make({ last: -1, offset: 0 }),
                            Effect.flatMap(TxSubscriptionRef.get(row), TxSubscriptionRef.make),
                        ]);
                        const [bound, ending, token] = [handleOf(row), Stream.provideService(ends, Handle, handleOf(frozen)), Schema.decodeUnknownOption(McpSchema.ProgressToken)(request.requestMetadata?.progressToken)];
                        const notify = (report: Parameters<(typeof Server.ToolCall)['Service']['progress']>[0]) => Option.match(token, { onNone: () => Effect.void, onSome: (progressToken) => server.notifications['notifications/progress']({ progressToken, ...report }) });
                        const poll = SubscriptionRef.updateAndGet(pace, Struct.evolve({ last: Number.increment, offset: Number.increment })).pipe(Effect.flatMap(({ last }) => notify({ progress: last })));
                        const ended = Stream.runHead(Stream.merge(ending, Option.match(token, { onNone: () => Stream.empty, onSome: () => Stream.drain(Stream.mapEffect(Stream.debounce(SubscriptionRef.changes(pace), Duration.minutes(1)), () => poll)) })));
                        const answer = Effect.zipWith(options.handle(Record.remove(payload, 'handle')), Effect.all([Ref.get(stated), Ref.get(blocks)]), (outcome, [state, more]) =>
                            outcome instanceof McpSchema.InputRequired ? outcome : McpSchema.CallToolResult.make({ ...outcome, content: [...state, ...outcome.content, ...more] }),
                        ).pipe(
                            Effect.raceFirst(Deferred.await(asked)),
                            Effect.provideService(Server.ToolCall, {
                                add: (block) => Ref.update(blocks, Array.append(block)),
                                state: (line) => Ref.set(stated, [McpSchema.TextContent.make({ text: line })]),
                                progress: (report) => SubscriptionRef.modify(pace, ({ offset }) => [Struct.evolve(report, { progress: Number.sum(offset), total: UndefinedOr.map(Number.sum(offset)) }), { last: report.progress + offset, offset }]).pipe(Effect.flatMap(notify)),
                                input: (required) => Effect.andThen(Deferred.succeed(asked, required), Effect.never),
                                awaitRunning: Effect.map(TxSubscriptionRef.get(row), Struct.get('running')).pipe(Effect.flatMap(Effect.forEach((each) => Effect.raceFirst(Fiber.await(each.fiber), Stream.runHead(each.ends)), { concurrency: 'unbounded', discard: true }))),
                            }),
                            Effect.provideService(Handle, bound),
                            Effect.updateService(McpSchema.McpRequestContext, (context) => ({ ...context, requestState: Option.getOrUndefined(Option.flatMap(resumed, Struct.get('state'))) })),
                            Effect.scoped,
                        );
                        const fiber = yield* FiberSet.run(fibers)(Effect.flatMap(TxRef.get(sequence), (from) => Effect.ensuring(answer, TxSubscriptionRef.update(row, routeEvents(limit, from, [])))));
                        const [outcome, early] = yield* Fiber.join(fiber).pipe(
                            Effect.map((value) => [value, false] as const),
                            Effect.raceFirst(Effect.flatMap(ended, Option.match({ onNone: () => Effect.never, onSome: (text) => Effect.map(Effect.provideService(text, Handle, bound), (rendered) => [McpSchema.CallToolResult.make({ content: [McpSchema.TextContent.make({ text: rendered })] }), true] as const) }))),
                            Effect.interruptible,
                            Effect.onInterrupt(() => Fiber.interrupt(fiber)),
                        );
                        return yield* outcome instanceof McpSchema.InputRequired
                            ? Effect.map(
                                  Effect.provideContext(Effect.orDie(Jwt.encrypt(Resumed, { handle: id, state: Option.fromUndefinedOr(outcome.requestState), aud: options.tool.name }, key)), crypto),
                                  (sealed) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState: Redacted.value(sealed) }),
                              )
                            : composeResult(outcome, { row, limit, budget, stated: !early && Array.isReadonlyArrayNonEmpty(yield* Ref.get(stated)), issued: given === undefined ? Option.some(id) : Option.none(), early: early ? Option.some({ tool: options.tool.name, fiber, ends: ending }) : Option.none() });
                    },
                    Effect.catchTag('NoSuchElementError', ({ message }) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message })] }))),
                    Effect.uninterruptible,
                    Effect.scoped,
                );
                const reserved = Record.keys(added).filter((field) => Record.has(options.tool.inputSchema.properties ?? {}, field));
                return Array.match(reserved, {
                    onEmpty: () => Effect.flatMap(gate({ [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }, options), (services) => server.addTool({ ...options, tool, handle, services })),
                    onNonEmpty: (taken) => Effect.annotateLogs(Effect.logError(`Tool ${options.tool.name} takes ${taken.join(', ')}, which the server adds to every tool, and stays unregistered`), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }),
                });
            },
            addResource: (options) => Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.resource.uri }, options), (services) => server.addResource({ ...options, services, resource: McpSchema.Resource.make({ ...options.resource, icons: withIcons(options.annotations, options.resource.icons) }) })),
            addResourceTemplate: (options) =>
                Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.template.uriTemplate }, options), (services) =>
                    server.addResourceTemplate({ ...options, services, template: McpSchema.ResourceTemplate.make({ ...options.template, icons: withIcons(options.annotations, options.template.icons) }), completions: Record.map(options.completions, rankCompletions) }),
                ),
            addPrompt: (options) =>
                Effect.flatMap(gate({ [ATTR_GEN_AI_PROMPT_NAME]: options.prompt.name }, options), (services) =>
                    server.addPrompt({ ...options, services, prompt: McpSchema.Prompt.make({ ...options.prompt, icons: withIcons(options.annotations, options.prompt.icons) }), completions: Record.map(options.completions, rankCompletions) }),
                ),
        });
        const service = Server.of({
            uri: (path) => `${scheme}://${path}`,
            events: ({ stream, render, owns = Function.constTrue }) =>
                Stream.runForEach(stream, (event) =>
                    Effect.tx(
                        Effect.flatMap(Effect.all([TxRef.modify(sequence, (count) => [count + 1, count + 1]), TxRef.get(live)]), ([at, rows]) =>
                            Effect.forEach(Record.values(rows), TxSubscriptionRef.update(routeEvents(limit, at - 1, [{ sequence: at, text: render(event), owns: (state) => owns(event, state) }])), { discard: true }),
                        ),
                    ),
                ).pipe(Effect.forkScoped, Effect.asVoid),
            notifyListChanged,
            bind: Effect.scoped(Effect.map(issueHandle, ({ id }) => ({ handle: id, server: scheme }))),
            follow: (handle) =>
                Effect.as(
                    Effect.scoped(findHandle(handle)),
                    Stream.unwrap(
                        Effect.map(findHandle(handle), ({ row }) =>
                            TxSubscriptionRef.changesStream(row).pipe(
                                Stream.filter(({ answering }) => answering === 0),
                                Stream.mapAccum(Function.constant(0), (last, { entries }) => [Math.max(last, ...Array.map(entries, Struct.get('sequence'))), Array.filter(entries, isAfter(last))] as const),
                                Stream.mapEffect(formatLine),
                                Stream.provideService(Handle, handleOf(row)),
                            ),
                        ),
                    ),
                ),
            ack: (handle, through) => Effect.scoped(Effect.flatMap(findHandle(handle), ({ row }) => TxSubscriptionRef.update(row, (slot) => ({ ...slot, entries: Array.filter(slot.entries, isAfter(through)) })))),
        });
        return Context.make(McpServer.McpServer, gatedServer).pipe(Context.add(Server, service));
    }, Layer.effectContext);
    static readonly layerStdio = <E, R>(manifest: Parameters<typeof serverOptions>[0], settings: Parameters<typeof serverOptions>[1], app: Layer.Layer<never, E, R>) =>
        Layer.unwrap(Effect.map(serverOptions(manifest, settings, app), ({ options }) => McpServer.layerStdio(options))).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    static readonly layerHttp = <E, R>(manifest: Parameters<typeof serverOptions>[0], { address, hosts, ...settings }: Parameters<typeof serverOptions>[1] & { readonly address: Host.HostPort; readonly hosts: readonly string[] }, app: Layer.Layer<never, E, R>) => {
        const [names, bound] = [['localhost', ...Array.map(NetAddress.loopbackAddresses, NetAddress.formatUrlHost)], Host.formatHostPort(address)];
        const allowed = new Set([...hosts, ...names, bound]);
        const hostValidation = HttpRouter.middleware((next) => HttpServerRequest.HttpServerRequest.use((request) => (Option.exists(HttpServerRequest.toURL(request), ({ host, hostname }) => allowed.has(host) || allowed.has(hostname)) ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
            global: true,
        });
        const allowedOrigins = Array.cartesianWith(['http', 'https'], [...hosts, ...Array.map([...hosts, ...names], (name) => `${name}:${address.port}`), bound], (scheme, host) => `${scheme}://${host}`);
        const orNotFound = Effect.mapError((_: Cause.NoSuchElementError) => HttpApiError.NotFound.make());
        const transport = Layer.unwrap(
            Effect.map(serverOptions(manifest, settings, app), ({ options, served }) =>
                Layer.merge(
                    McpServer.layerHttp({ ...options, path: '/', allowSessionTermination: true, allowedOrigins }),
                    HttpApiBuilder.layer(Handles).pipe(
                        Layer.provide(
                            HttpApiBuilder.group(Handles, 'handles', (handlers) =>
                                handlers.handleAll({
                                    bind: () => served.bind,
                                    follow: ({ params }) => orNotFound(served.follow(params.handle)),
                                    ack: ({ params, query }) => orNotFound(served.ack(params.handle, query.through)),
                                }),
                            ),
                        ),
                    ),
                ),
            ),
        );
        return Layer.merge(transport.pipe(Layer.provideMerge(McpServer.McpServer.layer)), hostValidation);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const serverOptions = Effect.fnUntraced(function* <E, R>(
    manifest: { readonly name: Parameters<typeof Server.layer>[0]['scheme'] } & Pick<Parameters<typeof McpServer.layerHttp>[0], 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'>,
    { instructions, ...settings }: Omit<Parameters<typeof Server.layer>[0], 'scheme'> & { readonly instructions: readonly string[] },
    app: Layer.Layer<never, E, R>,
) {
    const server = yield* McpServer.McpServer;
    const built = yield* Layer.build(Layer.provideMerge(app, Server.layer({ scheme: manifest.name, ...settings })));
    const shown = yield* Effect.filter([...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts], ({ annotations }) => Context.get(annotations, Server.Visibility));
    return { options: { ...manifest, protocols: Server.protocols, instructions: Array.dedupe([...instructions, ...Array.flatMap(shown, ({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n') }, served: Context.get(built, Server) };
});
const withIcons = (annotations: Context.Context<never>, own: readonly McpSchema.Icon[] | undefined) => Array.match(Context.get(annotations, Server.Icons), { onEmpty: () => own, onNonEmpty: Function.identity });
const operationDuration = Metric.mapInput(
    Metric.histogram(METRIC_MCP_SERVER_OPERATION_DURATION, {
        description: 'Duration of MCP server operations',
        boundaries: [0.01, 0.02, 0.05, 0.1, 0.2, 0.5, 1, 2, 5, 10, 30, 60, 120, 300],
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
const metrics = McpServer.middleware((effect, { method, target }) =>
    UndefinedOr.match(operations[method], {
        onUndefined: () => effect,
        onDefined: Effect.fnUntraced(function* (attributing) {
            const [duration, exit] = yield* Effect.timed(Effect.exit(effect));
            yield* Metric.update(Metric.withAttributes(operationDuration, { [ATTR_MCP_METHOD_NAME]: method, ...attributing(target), ...failureAttributes(exit) }), duration);
            return yield* exit;
        }),
    }),
);
const rankCompletions =
    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
    (input: string, context: C) =>
        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
            const ordered = Unmatched.rank(input, values);
            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
        });
const split = ({ content, structuredContent }: McpSchema.CallToolResult) => {
    const [texts, blocks] = Array.partition(content, (block) => (block.type === 'text' ? Result.succeed(block.text) : Result.fail(block)));
    const shown = UndefinedOr.map(structuredContent, (json) => [JSON.stringify(json), Predicate.isString(json) ? json : JSON.stringify(json)] as const);
    return { blocks, texts: UndefinedOr.match(shown, { onUndefined: () => texts, onDefined: ([mirror, text]) => (Array.contains(texts, mirror) ? Array.map(texts, (each) => (each === mirror ? text : each)) : Array.append(texts, text)) }) };
};
const enqueue = (limit: number, owned: HandleState['entries']) => (slot: HandleState) => {
    const merged = Array.sortWith([...slot.entries, ...owned], (each) => each.sequence, Number.Order);
    const [gone, entries] = Array.splitAt(merged, merged.length - limit);
    return { ...slot, dropped: slot.dropped + gone.length, entries };
};
const formatLine = ({ sequence, text }: HandleState['entries'][number]) => Effect.map(text, (line) => ({ sequence, line: `! #${sequence} ${line}`.replaceAll('\n', '\n  ') }));
const isAfter = (sequence: number) => Predicate.mapInput(Number.isGreaterThan(sequence), (entry: HandleState['entries'][number]) => entry.sequence);
const routeEvents =
    (limit: number, from: number, raised: HandleState['held']) =>
    (slot: HandleState): HandleState => {
        const [owned, rest] = Array.partition([...slot.held, ...raised], (each) => (isAfter(from)(each) && each.owns(slot.state) ? Result.succeed(each) : Result.fail(each)));
        return enqueue(limit, owned)({ ...slot, held: Array.takeRight(rest, limit) });
    };
const composeResult = Effect.fnUntraced(function* (
    outcome: McpSchema.CallToolResult,
    { row, stated, issued, early, limit, budget }: { readonly row: TxSubscriptionRef.TxSubscriptionRef<HandleState>; readonly stated: boolean; readonly issued: Option.Option<string>; readonly early: Option.Option<Omit<HandleState['running'][number], 'call'>>; readonly limit: number; readonly budget: number },
) {
    const taken = yield* TxSubscriptionRef.modify(row, (slot) => {
        const [ended, pending] = Array.partition(slot.running, (each) => UndefinedOr.match(each.fiber.pollUnsafe(), { onUndefined: () => Result.fail(each), onDefined: (exit) => Result.succeed({ ...each, exit }) }));
        const started = Array.map(Option.toArray(early), (each) => ({ ...each, call: slot.calls + 1 }));
        return [
            { ...slot, ended, pending, started },
            { ...slot, dropped: 0, entries: [], calls: slot.calls + started.length, running: [...pending, ...started] },
        ];
    });
    const lines = yield* Effect.forEach(taken.entries, formatLine).pipe(Effect.provideService(Handle, handleOf(row)));
    const [, fitted] = Array.mapAccum(Array.zip(taken.entries, lines), budget, (left, [entry, { line }], index) => [left - line.length - 1, index === 0 || left > line.length ? Result.succeed(line) : Result.fail(entry)]);
    const [shown, later] = Array.separate(fitted);
    yield* TxSubscriptionRef.update(row, enqueue(limit, later));
    const finished = Array.map(taken.ended, ({ call, tool, exit }) => {
        const { failed, texts, blocks } = Exit.match(exit, {
            onSuccess: (result) => (result instanceof McpSchema.InputRequired ? { failed: true, texts: ['Asked for client input, call the tool again'], blocks: [] } : { failed: result.isError === true, ...split(result) }),
            onFailure: (cause) => ({ failed: true, texts: Array.map(Cause.prettyErrors(cause), Struct.get('message')), blocks: [] }),
        });
        return { blocks, line: [`finished: call ${call} ${tool}${blocks.length > 0 ? ` +${blocks.length} blocks` : ''}${failed ? ' failed:' : ''}`, ...texts].join(' ').replaceAll('\n', '\n  ') };
    });
    const { texts, blocks } = split(outcome);
    const [state, body] = Array.splitAt(texts, stated ? 1 : 0);
    const leading = [
        ...Option.toArray(Option.map(issued, (handle) => `handle ${handle} holds your state, pass it on each later call`)),
        ...(taken.dropped > 0 ? [`! ${taken.dropped} older events dropped`] : []),
        ...shown,
        ...(later.length > 0 ? [`! ${later.length} more events next call`] : []),
        ...Array.map(taken.started, ({ call }) => `early: call ${call} keeps running, its result arrives under finished`),
        ...(taken.pending.length > 0 ? [`pending: calls ${Array.map(taken.pending, Struct.get('call')).join(', ')}`] : []),
        ...Array.map(finished, Struct.get('line')),
    ];
    const text = [...state, ...leading, ...(Array.isReadonlyArrayNonEmpty(leading) && Array.isReadonlyArrayNonEmpty(body) ? [''] : []), ...body];
    return McpSchema.CallToolResult.make({ ...Struct.omit(outcome, ['structuredContent']), content: [...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] }), ...blocks, ...Array.flatMap(finished, Struct.get('blocks'))] });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
