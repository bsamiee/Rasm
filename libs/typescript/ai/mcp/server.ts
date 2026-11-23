import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Unmatched } from '@rasm/search';
import { Array, type Brand, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, Function, Layer, Match, Metric, Number, Option, Order, pipe, RcMap, Record, Ref, Result, Schema, SchemaGetter, type Scope, Stream, Struct, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { Host, NetAddress } from 'effect/net';

// --- [TYPES] ---------------------------------------------------------------------------

interface Line {
    readonly sequence: number;
    readonly text: string;
    readonly shown: Effect.Effect<boolean>;
}
interface Running {
    readonly call: number;
    readonly tool: string;
    readonly done: Deferred.Deferred<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>;
    readonly ends: Stream.Stream<string>;
}
interface Holder {
    readonly state: Ref.Ref<Context.Context<never>>;
    readonly cursor: Ref.Ref<number>;
    readonly calls: Ref.Ref<number>;
    readonly running: Ref.Ref<readonly Running[]>;
}
type Progressed = Omit<Parameters<McpServer.McpServer['Service']['notifications']['notifications/progress']>[0], 'progressToken' | '_meta'>;
type Gated = Pick<Parameters<McpServer.McpServer['Service']['addResource']>[0], 'annotations' | 'services'>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROTOCOLS = Option.getOrThrow(Array.match(Array.sort(Object.values(McpProtocol), Order.flip(Order.mapInput(Order.String, (adapter: McpProtocol.ProtocolAdapter) => adapter.protocolVersion))), { onEmpty: Option.none, onNonEmpty: Option.some }));
const EXECUTIONS = { name: 'executions', description: 'Calls of a handle that returned early: the numbers still running, and each finished one with its result, listed once', mimeType: 'application/json' } as const;
const PROGRESS = Duration.minutes(1);
const SEALING = Aead.xchacha20Poly1305;

// --- [MODELS] --------------------------------------------------------------------------

const Tagged = Schema.Struct({ _tag: Schema.String });
class Finished extends Schema.Opaque<Finished, unknown>()(
    Schema.Struct({
    call: Schema.Int.annotate({ description: 'Number of the call in the early result that returned it' }),
    tool: Schema.String,
    result: Schema.optionalKey(Schema.Json),
    error: Schema.optionalKey(Schema.String),
    }),
) {}
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
    static readonly Scheme = Schema.String.pipe(
        Schema.decodeTo(Schema.String.check(Schema.isPattern(/^[A-Za-z][\d+.A-Za-z-]*$/u)).pipe(Schema.brand('Scheme')), {
            decode: SchemaGetter.transform((name: string) => name.slice(name.lastIndexOf('/') + 1)),
            encode: SchemaGetter.forbiddenEncoding,
        }),
    );
    static readonly Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
    static readonly Visible = Context.Reference<Effect.Effect<boolean, Error>>('@rasm/mcp/server/Visible', { defaultValue: () => Effect.succeed(true) });
    static readonly Icons = Context.Reference<readonly McpSchema.Icon[]>('@rasm/mcp/server/Icons', { defaultValue: () => [] });
    static readonly Ends = Context.Reference<Stream.Stream<string>>('@rasm/mcp/server/Ends', { defaultValue: () => Stream.never });
    static readonly ToolCall = Context.Reference<{
        readonly add: (block: McpSchema.ContentBlock) => Effect.Effect<void>;
        readonly progress: (report: Progressed) => Effect.Effect<void>;
        readonly input: (required: McpSchema.InputRequired) => Effect.Effect<never, McpSchema.InternalError>;
        readonly settle: Effect.Effect<void>;
    }>('@rasm/mcp/server/ToolCall', {
        defaultValue: () => ({ add: () => Effect.void, progress: () => Effect.void, input: () => Effect.fail(McpSchema.InternalError.make({ message: 'Client input needs a tool call that carries it to the client' })), settle: Effect.void }),
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
    static readonly layer = Effect.fnUntraced(function* ({ scheme, retention, limit }: { readonly scheme: Brand.Branded<string, 'Scheme'>; readonly retention: Duration.Duration; readonly limit: number }) {
        const [server, scope, journal, aead, random] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make<readonly Line[]>([]), Aead.Aead, SecureRandom.SecureRandom]);
        const [key, trailer] = yield* Effect.orDie(Effect.all([aead.generateKey(SEALING), Schema.decodeUnknownEffect(McpSchema.ToolJson)(Schema.toJsonSchemaDocument(Trailer).schema)]));
        const handles = yield* RcMap.make({
            lookup: (_: string): Effect.Effect<Holder> => Effect.all({ state: Ref.make(Context.empty()), cursor: Effect.flatMap(Ref.get(journal), (lines) => Ref.make(Option.match(Array.last(lines), { onNone: () => 0, onSome: Struct.get('sequence') }))), calls: Ref.make(0), running: Ref.make<readonly Running[]>([]) }),
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
        const gate = (subject: Readonly<Record<string, string>>, { annotations, services = Context.empty() }: Gated) =>
            Effect.map(
                Layer.buildWithScope(
                    McpServer.visibility(() =>
                        Context.get(annotations, Server.Visible).pipe(
                            Effect.tapCause((cause) => Effect.annotateLogs(Effect.logError('Visibility check failed', cause), subject)),
                            Server.orInternalError,
                        ),
                    ).layer,
                    scope,
                ),
                (gated) => Context.merge(services, gated),
            );
        const calls = { server, scope, gate, retention, handles, journal, trailer, early, seal, open, mint: random.uuidV7() };
        const listed = yield* Ref.make<Record.ReadonlyRecord<'tools' | 'resources' | 'prompts', Record.ReadonlyRecord<string, boolean>>>({ tools: {}, resources: {}, prompts: {} });
        const visible = (entries: readonly (readonly [string, Context.Context<never>])[]) =>
            Effect.map(
                Effect.forEach(entries, ([at, annotations]) => Effect.map(Context.get(annotations, Server.Visible), (shown) => [at, shown] as const)),
                Record.fromEntries,
            );
        const relist = Effect.gen(function* () {
            const now = yield* Effect.all({
                tools: visible(Array.map(server.tools, ({ tool, annotations }) => [tool.name, annotations] as const)),
                resources: visible([...Array.map(server.resources, ({ resource, annotations }) => [resource.uri, annotations] as const), ...Array.map(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, annotations] as const)]),
                prompts: visible(Array.map(server.prompts, ({ prompt, annotations }) => [prompt.name, annotations] as const)),
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
                const [found, missing] = [yield* RcMap.getOption(handles, id), Function.constant(new Cause.NoSuchElementError(`Handle ${id} is unknown or expired`))];
                const { running } = yield* Effect.fromOption(found, missing);
                return yield* Effect.orDie(Effect.flatMap(delivered(running), Schema.encodeEffect(Executions)));
            }, Effect.scoped),
        });
        yield* Layer.build(metered.layerGlobal);
        return Context.make(
            McpServer.McpServer,
            McpServer.McpServer.of({
                ...server,
                addTool: addTool(calls),
                addResource: (options) => Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.resource.uri }, options), (services) => server.addResource({ ...options, services, resource: McpSchema.Resource.make({ ...options.resource, icons: iconed(options.annotations, options.resource.icons) }) })),
                addResourceTemplate: (options) =>
                    Effect.flatMap(gate({ [ATTR_MCP_RESOURCE_URI]: options.template.uriTemplate }, options), (services) =>
                        server.addResourceTemplate({ ...options, services, template: McpSchema.ResourceTemplate.make({ ...options.template, icons: iconed(options.annotations, options.template.icons) }), completions: Record.map(options.completions, ranking) }),
                    ),
                addPrompt: (options) =>
                    Effect.flatMap(gate({ [ATTR_GEN_AI_PROMPT_NAME]: options.prompt.name }, options), (services) =>
                        server.addPrompt({ ...options, services, prompt: McpSchema.Prompt.make({ ...options.prompt, icons: iconed(options.annotations, options.prompt.icons) }), completions: Record.map(options.completions, ranking) }),
                    ),
            }),
        ).pipe(Context.add(Server, Server.of({ uri, events: events({ journal, limit }), relist })));
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
const since = <L extends Line>(at: number, lines: readonly L[]) => [Option.match(Array.last(lines), { onNone: () => at, onSome: ({ sequence }) => Math.max(at, sequence) }), lines.filter(({ sequence }) => sequence > at)] as const;
const ranking =
    <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
    (input: string, context: C) =>
        Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
            const ordered = Unmatched.rank(input, values);
            return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
        });

const events = ({ journal, limit }: Pick<Parameters<typeof addTool>[0], 'journal'> & { readonly limit: number }) =>
    Effect.fnUntraced(function* <A>({ text, shown = Function.constant(Effect.succeed(true)), stream }: { readonly text: (event: A) => string; readonly shown?: (event: A) => Effect.Effect<boolean>; readonly stream: Stream.Stream<A> }) {
        yield* stream.pipe(
            Stream.runForEach((value) => Ref.update(journal, (kept) => Array.takeRight(Array.append(kept, { text: text(value), shown: shown(value), sequence: Option.match(Array.last(kept), { onNone: () => 1, onSome: ({ sequence }) => sequence + 1 }) }), limit))),
            Effect.forkScoped,
        );
    });
const texts = ({ content }: McpSchema.CallToolResult) => content.flatMap((block) => (block.type === 'text' ? [block.text] : [])).join('\n');
const finished = ({ call, tool }: Running, exit: Exit.Exit<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>): Finished =>
    Exit.match(exit, {
        onSuccess: (outcome) =>
            outcome instanceof McpSchema.InputRequired
                ? { call, tool, error: 'Asked for client input, call the tool again' }
                : { call, tool, ...(outcome.isError === true ? { error: texts(outcome) } : Option.getOrUndefined(Option.map(Option.flatMap(Option.fromUndefinedOr(outcome.structuredContent), Schema.decodeUnknownOption(Schema.Json)), (result) => ({ result })))) },
        onFailure: (cause) => ({ call, tool, error: Array.map(Cause.prettyErrors(cause), Struct.get('message')).join('\n') }),
    });
const delivered = Effect.fnUntraced(function* (running: Ref.Ref<readonly Running[]>) {
    const read = yield* Effect.forEach(yield* Ref.get(running), (each) =>
        Effect.map(
            Effect.flatMap(Deferred.poll(each.done), (polled) => Effect.transposeOption(Option.map(polled, Effect.exit))),
            Option.match({ onNone: (): Result.Result<Finished, number> => Result.fail(each.call), onSome: (exit) => Result.succeed(finished(each, exit)) }),
        ),
    );
    const [ended, pending] = Array.separate(read);
    yield* Ref.update(
        running,
        Array.filter(({ call }) => !ended.some((entry) => entry.call === call)),
    );
    return { ...Array.match(pending, { onEmpty: () => ({}), onNonEmpty: (calls) => ({ pending: calls }) }), ...Array.match(ended, { onEmpty: () => ({}), onNonEmpty: (entries) => ({ finished: entries }) }) };
});
const addTool =
    ({
        server,
        scope,
        gate,
        retention,
        handles,
        journal,
        trailer,
        early,
        seal,
        open,
        mint,
    }: {
        readonly server: McpServer.McpServer['Service'];
        readonly scope: Scope.Scope;
        readonly gate: (subject: Readonly<Record<string, string>>, options: Gated) => Effect.Effect<Context.Context<never>>;
        readonly retention: Duration.Duration;
        readonly handles: RcMap.RcMap<string, Holder>;
        readonly journal: Ref.Ref<readonly Line[]>;
        readonly trailer: McpSchema.ToolJson;
        readonly early: (id: string, text: string) => McpSchema.CallToolResult;
        readonly seal: (tool: string, resumed: Resumed) => Effect.Effect<string>;
        readonly open: (tool: string, state: string) => Effect.Effect<Resumed, McpSchema.InvalidParams>;
        readonly mint: Effect.Effect<string>;
    }) =>
    (options: Parameters<McpServer.McpServer['Service']['addTool']>[0]) => {
        const ends = Context.get(options.annotations, Server.Ends);
        const Named = Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${Duration.format(retention)} without a call` });
        const { $defs, ...output } = options.tool.outputSchema ?? {};
        const tool = McpSchema.Tool.make({
            ...options.tool,
            icons: iconed(options.annotations, options.tool.icons),
            inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, handle: Schema.toJsonSchemaDocument(Named).schema } },
            outputSchema: { ...trailer, properties: { result: output, ...trailer.properties }, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr($defs), (definitions) => ({ $defs: definitions }))) },
        });
        const called = Effect.fnUntraced(function* (payload: Record.ReadonlyRecord<string, Schema.Json>, { id, minted, state, row: { state: kept, cursor, calls, running: unfinished } }: { readonly id: string; readonly minted: boolean; readonly state: Option.Option<string>; readonly row: Holder }) {
            const bound = held(kept);
            const [request, blocks, done, asked, pace, frozen] = yield* Effect.all([
                McpSchema.McpRequestContext,
                Ref.make(Array.empty<McpSchema.ContentBlock>()),
                Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, McpSchema.InternalError | McpSchema.InvalidParams>(),
                Deferred.make<McpSchema.InputRequired>(),
                Ref.make({ last: -1, offset: 0, quiet: true }),
                Effect.map(Effect.flatMap(Ref.get(kept), Ref.make), held),
            ]);
            const ending = Stream.provideService(ends, Handle, frozen);
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
                settle: Effect.flatMap(
                    Ref.get(unfinished),
                    Effect.forEach((each) => Effect.raceFirst(Effect.exit(Deferred.await(each.done)), Stream.runHead(each.ends)), { concurrency: 'unbounded', discard: true }),
                ),
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
            const [outcome, reason] = yield* pipe(
                Deferred.await(done),
                Effect.map((value) => [value, Option.none<string>()] as const),
                Effect.raceFirst(Effect.flatMap(Stream.runHead(ending), Option.match({ onNone: () => Effect.never, onSome: (text) => Effect.succeed([early(id, text), Option.some(text)] as const) }))),
                Effect.raceFirst(Option.match(token, { onNone: () => Effect.never, onSome: () => Effect.forever(Effect.andThen(Effect.sleep(PROGRESS), beat)) })),
                Effect.interruptible,
                Effect.onInterrupt(() => Fiber.interrupt(running)),
            );
            const returned = yield* Effect.transposeOption(Option.map(reason, (text) => Effect.map(Ref.updateAndGet(calls, Number.increment), (number) => ({ call: number, reason: text }))));
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
            const [from, fresh] = yield* Ref.modify(cursor, (at) => {
                const [next, unseen] = since(at, lines);
                return [[at, unseen] as const, next];
            });
            const dropped = Option.filter(
                Option.map(Array.head(fresh), ({ sequence }) => sequence - from - 1),
                (count) => count > 0,
            );
            const shown = yield* Effect.provideService(Effect.filter(fresh, Struct.get('shown')), Handle, bound);
            const text = [...(minted ? [`Handle ${id} holds your state and your place in the events, pass it as handle on each call`] : []), ...Option.toArray(Option.map(dropped, (count) => `${count} older events dropped`)), ...Array.map(shown, ({ sequence, text: line }) => `#${sequence} ${line}`)];
            const mirrored = (content: readonly McpSchema.ContentBlock[]) => [...content, ...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] })];
            const trailed = (structured: unknown) =>
                Effect.map(delivered(unfinished), (delivery) => ({
                    ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(structured), (value) => ({ result: value }))),
                    handle: id,
                    ...Option.getOrUndefined(Option.map(dropped, (count) => ({ dropped: count }))),
                    events: Array.map(shown, Struct.pick(['sequence', 'text'])),
                    ...Option.getOrUndefined(Option.map(returned, (returning) => ({ early: returning }))),
                    ...delivery,
                }));
            return yield* outcome instanceof McpSchema.InputRequired
                ? Effect.map(seal(options.tool.name, { handle: id, state: outcome.requestState }), (requestState) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState }))
                : Effect.map(outcome.isError === true ? Effect.succeed({}) : Effect.map(trailed(outcome.structuredContent), (structuredContent) => ({ structuredContent })), (structured) => McpSchema.CallToolResult.make({ ...outcome, content: mirrored(outcome.content), ...structured }));
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
        return Array.match(
            Array.filter(Record.keys(Given.fields), (key) => Record.has(options.tool.inputSchema.properties ?? {}, key)),
            {
                onEmpty: () => Effect.flatMap(gate({ [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }, options), (services) => server.addTool({ ...options, tool, handle, services })),
                onNonEmpty: (taken) => Effect.annotateLogs(Effect.logError(`Tool ${options.tool.name} takes ${taken.join(', ')}, which the server adds to every tool, and stays unregistered`), { [ATTR_GEN_AI_TOOL_NAME]: options.tool.name }),
            },
        );
    };
// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
