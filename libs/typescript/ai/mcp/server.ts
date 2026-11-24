import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, Function, Layer, Match, Metric, Number, Option, Order, RcMap, Record, Ref, Schema, SchemaGetter, type Scope, Stream, Struct, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Running {
    readonly call: number;
    readonly tool: string;
    readonly done: Deferred.Deferred<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>;
    readonly ends: Stream.Stream<string>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOLS = Option.getOrThrow(Array.match(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), { onEmpty: Option.none, onNonEmpty: Option.some }));
const EXECUTIONS = { name: 'executions', description: 'Calls of a handle that returned early: the numbers still running, and each finished one with its result, listed once', mimeType: 'application/json' } as const;
const PROGRESS = Duration.minutes(1);
const SEALING = Aead.xchacha20Poly1305;

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
const Finished = Schema.Struct({
    call: Schema.Int.annotate({ description: 'Number of the call in the early result that returned it' }),
    tool: Schema.String,
    result: Schema.optionalKey(Schema.Json),
    error: Schema.optionalKey(Schema.String),
});
const Delivered = Schema.Struct({
    pending: Schema.optionalKey(Schema.Array(Schema.Int).annotate({ description: 'Calls of this handle still running' })),
    finished: Schema.optionalKey(Schema.Array(Finished).annotate({ description: 'Calls that returned early and finished since, each listed once' })),
});
const Executions = Schema.fromJsonString(Delivered);
const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String) });
const Trailer = Schema.Struct({
    handle: Schema.String.annotate({ description: 'Handle to pass on each later call' }),
    dropped: Schema.optionalKey(Schema.Int.annotate({ description: 'Events before the first listed that dropped unread' })),
    events: Schema.Array(Schema.Struct({ sequence: Schema.Int, text: Schema.String })).annotate({ description: 'Events since the last call with this handle' }),
    early: Schema.optionalKey(Schema.Struct({ call: Schema.Int, reason: Schema.String }).annotate({ description: 'Why this call returned before its result: it keeps running, and finished of a later result holds its result under this call number' })),
    ...Delivered.fields,
});
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
        readonly events: <A>(source: { readonly text: (event: A) => string; readonly shown?: (event: A) => Effect.Effect<boolean>; readonly stream: Stream.Stream<A> }) => Effect.Effect<void, never, Scope.Scope>;
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
    static readonly Ends = Context.Reference<Stream.Stream<string>>('@rasm/mcp/server/Ends', { defaultValue: () => Stream.never });
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
        const [server, scope, journal, aead, random] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make<readonly { readonly sequence: number; readonly text: string; readonly shown: Effect.Effect<boolean> }[]>([]), Aead.Aead, SecureRandom.SecureRandom]);
        const [key, trailer] = yield* Effect.orDie(Effect.all([aead.generateKey(SEALING), Schema.decodeUnknownEffect(McpSchema.ToolJson)(Schema.toJsonSchemaDocument(Trailer).schema)]));
        const handles = yield* RcMap.make({
            lookup: (_: string) => Effect.all({ state: Ref.make(Context.empty()), cursor: Effect.flatMap(Ref.get(journal), (lines) => Ref.make(Option.match(Array.last(lines), { onNone: () => 0, onSome: Struct.get('sequence') }))), calls: Ref.make(0), running: Ref.make<readonly Running[]>([]) }),
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
        const early = (id: string, text: string) => McpSchema.CallToolResult.make({ content: [McpSchema.ResourceLink.make({ ...EXECUTIONS, uri: uri(`${EXECUTIONS.name}/${id}`), description: `${text}, the call keeps running and its result arrives here and in a later result` })] });
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
        const root = uri(`${EXECUTIONS.name}/`);
        yield* McpServer.registerResource(
            Object.assign([root, ''], { raw: [root, ''] }),
            McpSchema.param('handle', Schema.String),
        )({
            ...EXECUTIONS,
            content: Effect.fnUntraced(function* (_: string, id: string) {
                const { running } = yield* Effect.fromOption(yield* RcMap.getOption(handles, id), () => new Cause.NoSuchElementError(`Handle ${id} is unknown or expired`));
                return yield* Effect.orDie(Effect.flatMap(delivered(running), Schema.encodeEffect(Executions)));
            }, Effect.scoped),
        });
        yield* Layer.build(metered.layerGlobal);
        const guarded = McpServer.McpServer.of({
            ...server,
            addTool: (options) => {
                const ends = Context.get(options.annotations, Server.Ends);
                const Named = Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${Duration.format(retention)} without a call` });
                const { $defs, ...output } = options.tool.outputSchema ?? {};
                const tool = McpSchema.Tool.make({
                    ...options.tool,
                    icons: iconed(options.annotations, options.tool.icons),
                    inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, handle: Schema.toJsonSchemaDocument(Named).schema } },
                    outputSchema: { ...trailer, properties: { result: output, ...trailer.properties }, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr($defs), (definitions) => ({ $defs: definitions }))) },
                });
                const handle = Effect.fnUntraced(
                    function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                        const [{ handle: given }, request] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ message }) => McpSchema.InvalidParams.make({ message })), McpSchema.McpRequestContext]);
                        const resumed = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(request.requestState), (sealed) => open(options.tool.name, sealed)));
                        const named = Option.orElse(Option.map(resumed, Struct.get('handle')), () => Option.fromUndefinedOr(given));
                        const id = yield* Option.match(named, { onNone: () => random.uuidV7(), onSome: Effect.succeed });
                        const row = yield* Option.match(named, { onNone: () => Effect.asSome(RcMap.get(handles, id)), onSome: () => RcMap.getOption(handles, id) });
                        const { state: kept, cursor, calls, running: unfinished } = yield* Effect.fromOption(row, () => new Cause.NoSuchElementError(`Handle ${id} is unknown or expired after ${Duration.format(retention)} without a call, call again without handle to start one`));
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
                        const [outcome, reason] = yield* Deferred.await(done).pipe(
                            Effect.map((value) => [value, Option.none<string>()] as const),
                            Effect.raceFirst(Effect.flatMap(Stream.runHead(ending), Option.match({ onNone: () => Effect.never, onSome: (text) => Effect.succeed([early(id, text), Option.some(text)] as const) }))),
                            Effect.raceFirst(Option.match(token, { onNone: () => Effect.never, onSome: () => Effect.forever(Effect.andThen(Effect.sleep(PROGRESS), beat)) })),
                            Effect.interruptible,
                            Effect.onInterrupt(() => Fiber.interrupt(running)),
                        );
                        const returned = Option.all({ call: yield* Effect.transposeOption(Option.as(reason, Ref.updateAndGet(calls, Number.increment))), reason });
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
                        yield* Option.match(returned, { onNone: () => Effect.void, onSome: (returning) => Effect.andThen(Ref.update(unfinished, Array.append({ call: returning.call, tool: options.tool.name, done, ends: ending })), Effect.forkIn(forgotten, scope)) });
                        const lines = yield* Ref.get(journal);
                        const from = yield* Ref.getAndUpdate(cursor, (at) => Option.match(Array.last(lines), { onNone: () => at, onSome: ({ sequence }) => Math.max(at, sequence) }));
                        const fresh = lines.filter(({ sequence }) => sequence > from);
                        const dropped = Option.flatMap(Array.head(fresh), ({ sequence }) => Option.liftPredicate(sequence - from - 1, Number.isGreaterThan(0)));
                        const shown = yield* Effect.provideService(Effect.filter(fresh, Struct.get('shown')), Handle, bound);
                        const text = [
                            ...(Option.isNone(named) ? [`Handle ${id} holds your state and your place in the events, pass it as handle on each call`] : []),
                            ...Option.toArray(Option.map(dropped, (count) => `${count} older events dropped`)),
                            ...Array.map(shown, ({ sequence, text: line }) => `#${sequence} ${line}`),
                        ];
                        const mirrored = (content: readonly McpSchema.ContentBlock[]) => [...content, ...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] })];
                        const trailed = (structured: unknown) =>
                            Effect.map(delivered(unfinished), (delivery) => ({
                                ...Option.getOrUndefined(Option.bindTo(Option.fromUndefinedOr(structured), 'result')),
                                handle: id,
                                ...Option.getOrUndefined(Option.bindTo(dropped, 'dropped')),
                                events: Array.map(shown, Struct.pick(['sequence', 'text'])),
                                ...Option.getOrUndefined(Option.bindTo(returned, 'early')),
                                ...delivery,
                            }));
                        return yield* outcome instanceof McpSchema.InputRequired
                            ? Effect.map(seal(options.tool.name, { handle: id, state: outcome.requestState }), (requestState) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState }))
                            : Effect.map(outcome.isError === true ? Effect.succeed({}) : Effect.map(trailed(outcome.structuredContent), (structuredContent) => ({ structuredContent })), (structured) => McpSchema.CallToolResult.make({ ...outcome, content: mirrored(outcome.content), ...structured }));
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
            events: ({ text, shown = Function.constant(Effect.succeed(true)), stream }) =>
                stream.pipe(
                    Stream.runForEach((value) => Ref.update(journal, (kept) => Array.takeRight(Array.append(kept, { text: text(value), shown: shown(value), sequence: Option.match(Array.last(kept), { onNone: () => 1, onSome: ({ sequence }) => sequence + 1 }) }), limit))),
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
const texts = ({ content }: McpSchema.CallToolResult) => content.flatMap((block) => (block.type === 'text' ? [block.text] : [])).join('\n');
const finished = ({ call, tool }: Running, exit: Exit.Exit<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>) =>
    Exit.match(exit, {
        onSuccess: (outcome) =>
            outcome instanceof McpSchema.InputRequired ? { call, tool, error: 'Asked for client input, call the tool again' } : { call, tool, ...(outcome.isError === true ? { error: texts(outcome) } : Option.getOrUndefined(Option.bindTo(Schema.decodeUnknownOption(Schema.Json)(outcome.structuredContent), 'result'))) },
        onFailure: (cause) => ({ call, tool, error: Array.map(Cause.prettyErrors(cause), Struct.get('message')).join('\n') }),
    });
const delivered = Effect.fnUntraced(function* (running: Ref.Ref<readonly Running[]>) {
    const [ended, pending] = yield* Effect.partition(yield* Ref.get(running), (each) => Effect.map(Effect.flatMap(Deferred.poll(each.done), Option.match({ onNone: () => Effect.fail(each.call), onSome: (done) => Effect.exit(done) })), (exit) => finished(each, exit)));
    yield* Ref.update(
        running,
        Array.filter(({ call }) => !ended.some((entry) => entry.call === call)),
    );
    return { ...Array.match(pending, { onEmpty: () => ({}), onNonEmpty: (calls) => ({ pending: calls }) }), ...Array.match(ended, { onEmpty: () => ({}), onNonEmpty: (entries) => ({ finished: entries }) }) };
});
// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
