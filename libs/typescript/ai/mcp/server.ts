import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, Function, Layer, Match, Metric, Number, Option, Order, Predicate, RcMap, Record, Ref, Result, Schema, SchemaGetter, type Scope, Stream, Struct, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Running {
    readonly call: number;
    readonly tool: string;
    readonly done: Deferred.Deferred<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>;
    readonly ends: Stream.Stream<Effect.Effect<string>>;
}
interface Owed {
    readonly dropped: number;
    readonly entries: readonly { readonly sequence: number; readonly text: Effect.Effect<string> }[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOLS = Option.getOrThrow(Array.match(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), { onEmpty: Option.none, onNonEmpty: Option.some }));
const PROGRESS = Duration.minutes(1);
const SEALING = Aead.xchacha20Poly1305;
const QUIET: Owed = { dropped: 0, entries: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String) });
class Resumed extends Schema.Opaque<Resumed, unknown>()(Schema.Struct({ handle: Schema.String, state: Schema.optional(Schema.String) })) {
    static readonly json = Schema.fromJsonString(this);
}

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
        readonly events: <A>(source: { readonly stream: Stream.Stream<A>; readonly render: (event: A) => Effect.Effect<string>; readonly owns?: (event: A) => Effect.Effect<boolean> }) => Effect.Effect<void, never, Scope.Scope>;
        readonly relist: Effect.Effect<void, Error>;
    }
>()('@rasm/mcp/server') {
    static readonly scheme = Schema.decodeEffect(
        Schema.String.pipe(
            Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
                decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
                encode: SchemaGetter.forbiddenEncoding,
            }),
        ),
    );
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
    static readonly Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visible', { defaultValue: () => Effect.succeed(true) });
    static readonly Icons = Context.Reference<readonly McpSchema.Icon[]>('@rasm/mcp/server/Icons', { defaultValue: () => [] });
    static readonly Ends = Context.Reference<Stream.Stream<Effect.Effect<string>>>('@rasm/mcp/server/Ends', { defaultValue: () => Stream.never });
    static readonly ToolCall = Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly progress: (report: Omit<Parameters<McpServer.McpServer['Service']['notifications']['notifications/progress']>[0], 'progressToken' | '_meta'>) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
        readonly settle: Effect.Effect<void>;
    }>('@rasm/mcp/server/ToolCall', {
        defaultValue: () => ({ add: () => Effect.void, progress: () => Effect.void, input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })), settle: Effect.void }),
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
    static readonly layer = Effect.fnUntraced(function* ({ scheme, retention, limit }: { readonly scheme: Effect.Success<ReturnType<typeof Server.scheme>>; readonly retention: Duration.Duration; readonly limit: number }) {
        const [server, scope, live, sequence, aead, random] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make<Record.ReadonlyRecord<string, { readonly state: Ref.Ref<Context.Context<never>>; readonly owed: Ref.Ref<Owed> }>>({}), Ref.make(0), Aead.Aead, SecureRandom.SecureRandom]);
        const key = yield* Effect.orDie(aead.generateKey(SEALING));
        const handles = yield* RcMap.make({
            lookup: (id: string) => Effect.tap(Effect.all({ state: Ref.make(Context.empty()), owed: Ref.make(QUIET), calls: Ref.make(0), running: Ref.make<readonly Running[]>([]) }), (row) => Effect.acquireRelease(Ref.update(live, Record.set(id, row)), () => Ref.update(live, Record.remove(id)))),
            idleTimeToLive: retention,
        });
        const uri = (path: string) => `${scheme}://${path}`;
        const seal = Effect.fnUntraced(function* (tool: string, resumed: Resumed) {
            const [nonce, plain] = yield* Effect.all([random.randomBytes(SEALING.nonceLength), Effect.orDie(Schema.encodeEffect(Resumed.json)(resumed))]);
            const sealed = yield* Effect.orDie(aead.encrypt(key, new TextEncoder().encode(plain), { nonce, aad: new TextEncoder().encode(tool) }));
            return yield* Effect.orDie(Schema.encodeEffect(Schema.Uint8ArrayFromBase64Url)(new Uint8Array([...nonce, ...sealed])));
        });
        const open = Effect.fnUntraced(
            function* (tool: string, state: string) {
                const bytes = yield* Schema.decodeEffect(Schema.Uint8ArrayFromBase64Url)(state);
                const plain = yield* aead.decrypt(key, bytes.subarray(SEALING.nonceLength), { nonce: bytes.subarray(0, SEALING.nonceLength), aad: new TextEncoder().encode(tool) });
                return yield* Effect.orDie(Schema.decodeEffect(Resumed.json)(new TextDecoder().decode(plain)));
            },
            Effect.mapError(() => McpSchema.InvalidParams.make({ message: 'requestState came from no input request of this server and tool, call again without it' })),
        );
        const gate = (subject: Readonly<Record<string, string>>, { annotations, services = Context.empty() }: Pick<Parameters<McpServer.McpServer['Service']['addResource']>[0], 'annotations' | 'services'>) =>
            Effect.map(Layer.buildWithScope(McpServer.visibility(() => Server.orInternalError(Effect.tapCause(Context.get(annotations, Server.Visible), (cause) => Effect.annotateLogs(Effect.logError('Visibility check failed', cause), subject)))).layer, scope), (gated) => Context.merge(services, gated));
        const listed = yield* Ref.make<Record.ReadonlyRecord<'tools' | 'resources' | 'prompts', Record.ReadonlyRecord<string, boolean>>>({ tools: {}, resources: {}, prompts: {} });
        const relist = Effect.gen(function* () {
            const now = yield* Effect.all({
                tools: Effect.all(Record.fromIterableWith(server.tools, ({ tool, annotations }) => [tool.name, Context.get(annotations, Server.Visible)])),
                resources: Effect.all({
                    ...Record.fromIterableWith(server.resources, ({ resource, annotations }) => [resource.uri, Context.get(annotations, Server.Visible)]),
                    ...Record.fromIterableWith(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, Context.get(annotations, Server.Visible)]),
                }),
                prompts: Effect.all(Record.fromIterableWith(server.prompts, ({ prompt, annotations }) => [prompt.name, Context.get(annotations, Server.Visible)])),
            });
            const before = yield* Ref.getAndSet(listed, now);
            yield* Effect.forEach(Record.keys(now), (kind) => (Record.some(now[kind], flipped(before[kind])) ? server.notifications[`notifications/${kind}/list_changed`]({}) : Effect.void), { discard: true });
        });
        yield* Layer.build(metered.layerGlobal);
        const guarded = McpServer.McpServer.of({
            ...server,
            addTool: (options) => {
                const ends = Context.get(options.annotations, Server.Ends);
                const Named = Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${Duration.format(retention)} without a call` });
                const tool = McpSchema.Tool.make({
                    ...Struct.omit(options.tool, ['outputSchema']),
                    icons: iconed(options.annotations, options.tool.icons),
                    inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, handle: Schema.toJsonSchemaDocument(Named).schema } },
                });
                const handle = Effect.fnUntraced(
                    function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                        const [{ handle: given }, request] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ message }) => McpSchema.InvalidParams.make({ message })), McpSchema.McpRequestContext]);
                        const resumed = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(request.requestState), (sealed) => open(options.tool.name, sealed)));
                        const named = Option.orElse(Option.map(resumed, Struct.get('handle')), () => Option.fromUndefinedOr(given));
                        const id = yield* Option.match(named, { onNone: () => random.uuidV7(), onSome: Effect.succeed });
                        const row = yield* Option.match(named, { onNone: () => Effect.asSome(RcMap.get(handles, id)), onSome: () => RcMap.getOption(handles, id) });
                        const { state: kept, owed, calls, running: unfinished } = yield* Effect.fromOption(row, () => new Cause.NoSuchElementError(`Handle ${id} is unknown or expired after ${Duration.format(retention)} without a call, call again without handle to start one`));
                        const bound = held(kept);
                        const [blocks, done, asked, pace, frozen] = yield* Effect.all([
                            Ref.make(Array.empty<McpSchema.ContentBlock>()),
                            Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>(),
                            Deferred.make<McpSchema.InputRequired>(),
                            Ref.make({ last: -1, offset: 0, quiet: true }),
                            Effect.map(Effect.flatMap(Ref.get(kept), Ref.make), held),
                        ]);
                        const ending = Stream.provideService(ends, Handle, frozen);
                        const token = Schema.decodeUnknownOption(McpSchema.ProgressToken)(request.requestMetadata?.progressToken);
                        const notify = (report: Parameters<(typeof Server.ToolCall)['Service']['progress']>[0]) => Option.match(token, { onNone: () => Effect.void, onSome: (progressToken) => server.notifications['notifications/progress']({ progressToken, ...report }) });
                        const beat = Ref.modify(pace, ({ last, offset, quiet }) => (quiet ? [Option.some(last + 1), { last: last + 1, offset: offset + 1, quiet }] : [Option.none(), { last, offset, quiet: true }])).pipe(
                            Effect.flatMap(Option.match({ onNone: () => Effect.void, onSome: (progress) => notify({ progress }) })),
                        );
                        const answer = Effect.zipWith(options.handle(Record.remove(payload, 'handle')), Ref.get(blocks), (outcome, added) => (outcome instanceof McpSchema.InputRequired ? outcome : McpSchema.CallToolResult.make({ ...outcome, content: [...outcome.content, ...added] }))).pipe(
                            Effect.raceFirst(Deferred.await(asked)),
                            Effect.provideService(Server.ToolCall, {
                                add: (block) => Ref.update(blocks, Array.append(block)),
                                progress: (report) =>
                                    Ref.modify(pace, ({ offset }) => [
                                        { ...report, progress: report.progress + offset, total: UndefinedOr.map(report.total, Number.sum(offset)) },
                                        { last: report.progress + offset, offset, quiet: false },
                                    ]).pipe(Effect.flatMap(notify)),
                                input: (required) => Effect.andThen(Deferred.succeed(asked, required), Effect.never),
                                settle: Ref.get(unfinished).pipe(Effect.flatMap(Effect.forEach((each) => Effect.raceFirst(Effect.exit(Deferred.await(each.done)), Stream.runHead(each.ends)), { concurrency: 'unbounded', discard: true }))),
                            }),
                            Effect.provideService(Handle, bound),
                            Effect.updateService(McpSchema.McpRequestContext, (context) => ({ ...context, requestState: Option.getOrUndefined(resumed)?.state })),
                            Effect.scoped,
                        );
                        const running = yield* Effect.forkIn(Deferred.complete(done, answer), scope);
                        const [outcome, early] = yield* Deferred.await(done).pipe(
                            Effect.map((value) => [value, false] as const),
                            Effect.raceFirst(
                                Effect.flatMap(Stream.runHead(ending), Option.match({ onNone: () => Effect.never, onSome: (text) => Effect.map(Effect.provideService(text, Handle, bound), (rendered) => [McpSchema.CallToolResult.make({ content: [McpSchema.TextContent.make({ text: rendered })] }), true] as const) })),
                            ),
                            Effect.raceFirst(Option.match(token, { onNone: () => Effect.never, onSome: () => Effect.forever(Effect.andThen(Effect.sleep(PROGRESS), beat)) })),
                            Effect.interruptible,
                            Effect.onInterrupt(() => Fiber.interrupt(running)),
                        );
                        const returned = yield* early ? Effect.asSome(Ref.updateAndGet(calls, Number.increment)) : Effect.succeedNone;
                        const forgotten = RcMap.get(handles, id).pipe(
                            Effect.andThen(Effect.exit(Deferred.await(done))),
                            Effect.andThen(Effect.sleep(retention)),
                            Effect.andThen(
                                Ref.update(
                                    unfinished,
                                    Array.filter((each) => each.done !== done),
                                ),
                            ),
                            Effect.scoped,
                        );
                        return yield* outcome instanceof McpSchema.InputRequired
                            ? Effect.map(seal(options.tool.name, { handle: id, state: outcome.requestState }), (requestState) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState }))
                            : Effect.tap(delivered(outcome, { owed, running: unfinished, bound, minted: Option.match(named, { onNone: () => Option.some(id), onSome: Option.none }), early: returned }), () =>
                                  Option.match(returned, { onNone: () => Effect.void, onSome: (call) => Effect.andThen(Ref.update(unfinished, Array.append({ call, tool: options.tool.name, done, ends: ending })), Effect.forkIn(forgotten, scope)) }),
                              );
                    },
                    Effect.catchTag('NoSuchElementError', ({ message }) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message })] }))),
                    Effect.uninterruptible,
                    Effect.scoped,
                );
                const reserved = Record.keys(Given.fields).filter((field) => Record.has(options.tool.inputSchema.properties ?? {}, field));
                return Array.match(reserved, {
                    onEmpty: () => Effect.flatMap(gate({ [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }, options), (services) => server.addTool({ ...options, tool, handle, services })),
                    onNonEmpty: (taken) => Effect.annotateLogs(Effect.logError(`Tool ${options.tool.name} takes ${taken.join(', ')}, which the server adds to every tool, and stays unregistered`), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }),
                });
            },
            addResource: (options) => Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.resource.uri }, options), (services) => server.addResource({ ...options, services, resource: McpSchema.Resource.make({ ...options.resource, icons: iconed(options.annotations, options.resource.icons) }) })),
            addResourceTemplate: (options) =>
                Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.template.uriTemplate }, options), (services) =>
                    server.addResourceTemplate({ ...options, services, template: McpSchema.ResourceTemplate.make({ ...options.template, icons: iconed(options.annotations, options.template.icons) }), completions: Record.map(options.completions, ranking) }),
                ),
            addPrompt: (options) =>
                Effect.flatMap(gate({ [ATTR_GEN_AI_PROMPT_NAME]: options.prompt.name }, options), (services) =>
                    server.addPrompt({ ...options, services, prompt: McpSchema.Prompt.make({ ...options.prompt, icons: iconed(options.annotations, options.prompt.icons) }), completions: Record.map(options.completions, ranking) }),
                ),
        });
        const service = Server.of({
            uri,
            events: ({ stream, render, owns = Function.constant(Effect.succeed(true)) }) =>
                stream.pipe(
                    Stream.runForEach(
                        Effect.fnUntraced(function* (event) {
                            const [at, rows] = yield* Effect.all([Ref.updateAndGet(sequence, Number.increment), Ref.get(live)]);
                            const owners = yield* Effect.filter(Record.values(rows), ({ state }) => Effect.provideService(owns(event), Handle, held(state)));
                            const entry = { sequence: at, text: render(event) };
                            yield* Effect.forEach(
                                Array.map(owners, Struct.get('owed')),
                                Ref.update(({ dropped, entries }) => ({ dropped: dropped + Math.max(0, entries.length + 1 - limit), entries: Array.takeRight(Array.append(entries, entry), limit) })),
                                { discard: true },
                            );
                        }),
                    ),
                    Effect.forkScoped,
                    Effect.asVoid,
                ),
            relist,
        });
        return Context.make(McpServer.McpServer, guarded).pipe(Context.add(Server, service));
    }, Layer.effectContext);
    static readonly layerStdio = <E, R>(manifest: Parameters<typeof served>[0], settings: Parameters<typeof served>[1], app: Layer.Layer<never, E, R>) => Layer.unwrap(Effect.map(served(manifest, settings, app), (options) => McpServer.layerStdio(options))).pipe(Layer.provideMerge(McpServer.McpServer.layer));
    static readonly layerHttp = <E, R>(manifest: Parameters<typeof served>[0], { address, hosts, ...settings }: Parameters<typeof served>[1] & { readonly address: Host.HostPort; readonly hosts: readonly string[] }, app: Layer.Layer<never, E, R>) => {
        const [names, bound] = [['localhost', ...Array.map(NetAddress.loopbackAddresses, NetAddress.formatUrlHost)], Host.formatHostPort(address)];
        const allowed = new Set([...hosts, ...names, bound]);
        const hostValidation = HttpRouter.middleware((next) => HttpServerRequest.HttpServerRequest.use((request) => (Option.exists(HttpServerRequest.toURL(request), ({ host, hostname }) => allowed.has(host) || allowed.has(hostname)) ? next : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
            global: true,
        });
        const allowedOrigins = Array.cartesianWith(['http', 'https'], [...hosts, ...Array.map([...hosts, ...names], (name) => `${name}:${address.port}`), bound], (scheme, host) => `${scheme}://${host}`);
        const transport = Layer.unwrap(Effect.map(served(manifest, settings, app), (options) => McpServer.layerHttp({ ...options, path: '/', allowSessionTermination: true, allowedOrigins })));
        return Layer.merge(transport.pipe(Layer.provideMerge(McpServer.McpServer.layer)), hostValidation);
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const served = Effect.fnUntraced(function* <E, R>(
    manifest: { readonly name: Parameters<typeof Server.layer>[0]['scheme'] } & Pick<Parameters<typeof McpServer.layerHttp>[0], 'version' | 'description' | 'websiteUrl' | 'icons' | 'extensions' | 'cacheHints'>,
    { instructions, ...settings }: Omit<Parameters<typeof Server.layer>[0], 'scheme'> & { readonly instructions: readonly string[] },
    app: Layer.Layer<never, E, R>,
) {
    const server = yield* McpServer.McpServer;
    yield* Layer.build(Layer.provide(app, Server.layer({ scheme: manifest.name, ...settings })));
    const shown = yield* Effect.filter([...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts], ({ annotations }) => Context.get(annotations, Server.Visible));
    return { ...manifest, protocols: PROTOCOLS, instructions: Array.dedupe([...instructions, ...Array.flatMap(shown, ({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n') };
});
const flipped = (before: Record.ReadonlyRecord<string, boolean>) => (shown: boolean, at: string) => before[at] === !shown;
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
const ranking =
    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
    (input: string, context: C) =>
        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
            const ordered = Unmatched.rank(input, values);
            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
        });
const split = ({ content, structuredContent }: McpSchema.CallToolResult) => {
    const [texts, others] = Array.partition(content, (block) => (block.type === 'text' ? Result.succeed(block.text) : Result.fail(block)));
    const body = Predicate.isString(structuredContent) ? [structuredContent] : Array.match(texts, { onEmpty: () => Option.toArray(Option.map(Option.fromUndefinedOr(structuredContent), (json) => JSON.stringify(json))), onNonEmpty: Function.identity });
    return [body, others] as const;
};
const delivered = Effect.fnUntraced(function* (
    outcome: McpSchema.CallToolResult,
    { owed, running, bound, minted, early }: { readonly owed: Ref.Ref<Owed>; readonly running: Ref.Ref<readonly Running[]>; readonly bound: (typeof Handle)['Service']; readonly minted: Option.Option<string>; readonly early: Option.Option<number> },
) {
    const [{ dropped, entries }, rows] = yield* Effect.all([Ref.getAndSet(owed, QUIET), Ref.get(running)]);
    const [ended, pending] = yield* Effect.partition(rows, (each) => Effect.flatMap(Deferred.poll(each.done), Option.match({ onNone: () => Effect.fail(each.call), onSome: (done) => Effect.map(Effect.exit(done), (exit) => [each, exit] as const) })));
    yield* Ref.update(
        running,
        Array.filter(({ done }) => !ended.some(([each]) => each.done === done)),
    );
    const shown = yield* Effect.provideService(
        Effect.forEach(entries, ({ sequence, text }) => Effect.map(text, (line) => `! #${sequence} ${line}`)),
        Handle,
        bound,
    );
    const finished = Array.map(ended, ([{ call, tool }, exit]) => {
        const [failed, lines, blocks] = Exit.match(exit, {
            onSuccess: (result) => (result instanceof McpSchema.InputRequired ? ([true, ['Asked for client input, call the tool again'], []] as const) : ([result.isError === true, ...split(result)] as const)),
            onFailure: (cause) => [true, Array.map(Cause.prettyErrors(cause), Struct.get('message')), []] as const,
        });
        return [[`finished: call ${call} ${tool}${failed ? ' failed:' : ''}`, ...lines].join(' '), blocks] as const;
    });
    const [body, others] = split(outcome);
    const text = [
        ...Option.toArray(Option.map(minted, (id) => `handle ${id}`)),
        ...(dropped > 0 ? [`! ${dropped} older events dropped`] : []),
        ...shown,
        ...Option.toArray(Option.map(early, (call) => `early: call ${call} keeps running, its result arrives under finished`)),
        ...Array.match(pending, { onEmpty: () => [], onNonEmpty: (calls) => [`pending: calls ${calls.join(', ')}`] }),
        ...Array.map(finished, ([line]) => line),
        ...body,
    ];
    return McpSchema.CallToolResult.make({ ...Struct.omit(outcome, ['structuredContent']), content: [McpSchema.TextContent.make({ text: text.join('\n') }), ...others, ...Array.flatMap(finished, ([, blocks]) => blocks)] });
});
// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
