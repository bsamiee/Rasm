import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Column } from '@rasm/database/column';
import { Unmatched } from '@rasm/search/unmatched';
import {
    Array,
    Boolean,
    Cause,
    Context,
    DateTime,
    Deferred,
    Duration,
    Effect,
    type Exit,
    Fiber,
    FiberSet,
    Function,
    Layer,
    Match,
    Metric,
    Number,
    Option,
    Order,
    Predicate,
    RcMap,
    Record,
    Redacted,
    Ref,
    Result,
    Schema,
    SchemaGetter,
    type Scope,
    Stream,
    Struct,
    SubscriptionRef,
    Tuple,
    TxRef,
    TxSubscriptionRef,
    UndefinedOr,
} from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { type Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { Jwk, Jwt } from 'effect/jose';
import { Host, NetAddress } from 'effect/net';
import { Handles } from '#/handles';
import { callResult, type Entry, type HandleState, openStore } from '#/store';
import { ToolCall } from '#/toolkit';

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRESH: HandleState = { state: Context.empty(), pending: {}, touched: Option.none(), dropped: 0, held: [], entries: [], sent: [], calls: 0, answering: 0, running: [] };
const LISTING = new Intl.ListFormat('en');

// --- [MODELS] --------------------------------------------------------------------------

const Coded = Schema.Struct({ code: McpSchema.McpErrorBase.fields.code });
const Resumed = Schema.Struct({ handle: Schema.String, issued: Schema.Boolean, state: Schema.OptionFromOptionalKey(Schema.String), aud: Schema.String });
const isUuidV7 = Schema.is(Column.Uuid);

// --- [SERVICES] ------------------------------------------------------------------------

const handleOf = (row: TxSubscriptionRef.TxSubscriptionRef<HandleState>) => ({
    get: <A>(key: Context.Reference<A>) => Effect.map(TxSubscriptionRef.get(row), ({ state }) => Context.get(state, key)),
    set: <A>(key: Context.Reference<A>, value: A) => TxSubscriptionRef.update(row, (slot) => ({ ...slot, state: Context.add(slot.state, key, value) })),
});
const Handle = Context.Reference<ReturnType<typeof handleOf>>('@rasm/mcp/server/Handle', { defaultValue: () => TxSubscriptionRef.make(FRESH).pipe(Effect.runSync, handleOf) });

class Server extends Context.Service<
    Server,
    {
        readonly uri: (path: string) => string;
        readonly events: <A>(source: {
            readonly stream: Stream.Stream<A>;
            readonly render: (event: A) => Effect.Effect<string>;
            readonly owns?: (event: A, state: Context.Context<never>) => boolean;
            readonly leads?: (event: A, state: Context.Context<never>) => boolean;
            readonly delivers?: (event: A, state: Context.Context<never>) => Option.Option<typeof Handles.Kind.Type>;
            readonly standing?: (event: A) => Option.Option<typeof Handles.Standing.Type>;
        }) => Effect.Effect<void, never, Scope.Scope>;
        readonly states: <A>(key: Context.Reference<A>) => Stream.Stream<readonly A[]>;
        readonly persist: <A, RD, RE>(reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) => Effect.Effect<void, never, RD | RE>;
        readonly drain: Effect.Effect<void>;
        readonly notifyListChanged: Effect.Effect<void, Error>;
        readonly bind: Effect.Effect<typeof Handles.Binding.Type>;
        readonly follow: (handle: string) => Effect.Effect<Stream.Stream<typeof Handles.Event.Type>, Cause.NoSuchElementError>;
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
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly persist = <A, RD, RE>(reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) => Server.use((served) => served.persist(reference, schema));
    static readonly progress =
        <A>(report: (value: A, index: number) => Parameters<(typeof ToolCall)['Service']['progress']>[0]) =>
        <E, R>(self: Stream.Stream<A, E, R>) =>
            Stream.mapEffect(self, (value, index) => ToolCall.use(({ progress }) => Effect.as(progress(report(value, index)), value)));
    static readonly registerTool = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Never; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>,
        content?: (success: S['Type']) => McpSchema.CallToolResult['content'],
    ) {
        const context = yield* Effect.context<Exclude<R, McpSchema.McpRequestContext>>();
        const [parameters, success]: readonly [Schema.Codec<Effect.Effect<S['Type'], Error, McpSchema.McpRequestContext>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<S['Type'], unknown, S['DecodingServices'], S['EncodingServices']>] = [
            tool.parametersSchema.pipe(
                Schema.decodeTo(Schema.Any, {
                    decode: SchemaGetter.transform((given: P['Type']) => Effect.flatMap(Effect.context<McpSchema.McpRequestContext>(), (request) => Effect.provideContext(handle(given), Context.merge(context, request)))),
                    encode: SchemaGetter.forbiddenEncoding,
                }),
                Schema.catchDecoding((issue) => Effect.succeedSome(ToolCall.invalidParams(issue))),
            ),
            tool.successSchema,
        ];
        const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance(), dependencies: [McpSchema.McpRequestContext] }).annotateMerge(tool.annotations));
        yield* McpServer.registerToolkit(toolkit, { content: { [tool.name]: content } }).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
    });
    static readonly layer = Effect.fnUntraced(function* ({
        scheme,
        retention,
        limit,
        budget,
        state: folder,
        draining: signal,
    }: {
        readonly scheme: Effect.Success<ReturnType<typeof Server.scheme>>;
        readonly retention: Duration.Duration;
        readonly limit: number;
        readonly budget: number;
        readonly state?: string;
        readonly draining?: Deferred.Deferred<void>;
    }) {
        const [server, scope, live, random, crypto, draining, persisted] = yield* Effect.all([
            McpServer.McpServer,
            Effect.scope,
            TxSubscriptionRef.make<Record.ReadonlyRecord<string, TxSubscriptionRef.TxSubscriptionRef<HandleState>>>({}),
            SecureRandom.SecureRandom,
            Effect.context<Aead.Aead | SecureRandom.SecureRandom>(),
            UndefinedOr.match(signal, { onUndefined: () => Deferred.make<void>(), onDefined: Effect.succeed }),
            Ref.make(Record.empty<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>()),
        ]);
        const handles = yield* RcMap.make({
            lookup: (id: string) => Effect.tap(Effect.all({ id: Effect.succeed(id), row: TxSubscriptionRef.make(FRESH), fibers: FiberSet.make() }), ({ row }) => Effect.acquireRelease(TxSubscriptionRef.update(live, Record.set(id, row)), () => TxSubscriptionRef.update(live, Record.remove(id)))),
            idleTimeToLive: retention,
        });
        const { secret, sequence } = yield* openStore({ scheme, retention, folder, live, handles, persisted, render: (row) => Effect.provideService(Handle, handleOf(row)) });
        const key = yield* Effect.orDie(Jwk.importSecret(secret, 'dir'));
        const kept = Duration.format(retention);
        const findHandle = (known: string) => RcMap.getOption(handles, known).pipe(Effect.flatMap(Effect.fromOption), Effect.tap(touch));
        const issueHandle = random.uuidV7().pipe(
            Effect.flatMap((id) => RcMap.get(handles, id)),
            Effect.tap(touch),
        );
        const queued = ({ id, row }: Effect.Success<ReturnType<typeof findHandle>>) =>
            TxSubscriptionRef.changesStream(row).pipe(
                Stream.mapAccumEffect(Record.empty<string, number>, (waiting, slot) =>
                    Effect.gen(function* () {
                        const due = slot.answering === 0 && Array.isReadonlyArrayNonEmpty(send(slot)[0]) ? yield* TxSubscriptionRef.modify(row, send) : [];
                        return settle(waiting, yield* Effect.forEach(due, formatEvent), slot.entries);
                    }),
                ),
                Stream.provideService(Handle, handleOf(row)),
                Stream.ensuring(TxSubscriptionRef.update(row, (slot) => enqueue(limit, slot.sent)({ ...slot, sent: [] }))),
                Stream.interruptWhen(
                    live.pipe(
                        TxSubscriptionRef.changesStream,
                        Stream.filter((rows) => !Record.has(rows, id)),
                        Stream.runHead,
                        Effect.raceFirst(Deferred.await(draining)),
                    ),
                ),
            );
        const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String.annotate({ description: `Handle from an earlier result, holding your state and your place in the events; omit it to start one; it expires after ${kept} without a call` })) });
        const added = Record.map(Given.fields, (field) => Schema.toJsonSchemaDocument(field).schema);
        const open = (tool: string, sealed: string) =>
            Effect.provideContext(Effect.mapBoth(Jwt.decrypt(Resumed, sealed, key, { audience: tool }), { onFailure: () => McpSchema.InvalidParams.make({ message: 'requestState came from no input request of this server and tool, call again without it' }), onSuccess: ({ claims }) => claims }), crypto);
        const gate = (subject: Readonly<Record<string, string>>, { annotations, services = Context.empty() }: Pick<Parameters<McpServer.McpServer['Service']['addResource']>[0], 'annotations' | 'services'>) =>
            Effect.map(Layer.buildWithScope(McpServer.visibility(() => Server.orInternalError(Effect.tapCause(Context.get(annotations, Server.Visibility), (cause) => Effect.annotateLogs(Effect.logError('Visibility check failed', cause), subject)))).layer, scope), (gated) => Context.merge(services, gated));
        const listed = yield* Ref.make<Record.ReadonlyRecord<'tools' | 'resources' | 'prompts', Record.ReadonlyRecord<string, boolean>>>({ tools: {}, resources: {}, prompts: {} });
        const notifyListChanged = Effect.gen(function* () {
            const current = yield* Effect.all({
                tools: Effect.all(Record.fromIterableWith(server.tools, ({ tool, annotations }) => [tool.name, Context.get(annotations, Server.Visibility)])),
                resources: Effect.all({
                    ...Record.fromIterableWith(server.resources, ({ resource, annotations }) => [resource.uri, Context.get(annotations, Server.Visibility)]),
                    ...Record.fromIterableWith(server.resourceTemplates, ({ template, annotations }) => [template.uriTemplate, Context.get(annotations, Server.Visibility)]),
                }),
                prompts: Effect.all(Record.fromIterableWith(server.prompts, ({ prompt, annotations }) => [prompt.name, Context.get(annotations, Server.Visibility)])),
            });
            const before = yield* Ref.getAndSet(listed, current);
            yield* Effect.forEach(Record.keys(current), (kind) => (Record.some(Record.intersection(current[kind], before[kind], Boolean.xor), Function.identity) ? server.notifications[`notifications/${kind}/list_changed`]({}) : Effect.void), { discard: true });
        });
        yield* Layer.build(metrics.layerGlobal);
        const gatedServer = McpServer.McpServer.of({
            ...server,
            addTool: (options) => {
                const ends = Context.get(options.annotations, Server.EarlyReturn);
                const tool = McpSchema.Tool.make({ ...Struct.omit(options.tool, ['outputSchema']), icons: withIcons(options.annotations, options.tool.icons), inputSchema: { ...options.tool.inputSchema, properties: { ...options.tool.inputSchema.properties, ...added } } });
                const handle = Effect.fnUntraced(
                    function* (payload: Record.ReadonlyRecord<string, Schema.Json>) {
                        const [{ handle: given }, request] = yield* Effect.all([Effect.mapError(Schema.decodeEffect(Given)(payload), ({ issue }) => ToolCall.invalidParams(issue)), McpSchema.McpRequestContext]);
                        const resumed = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(request.requestState), (sealed) => open(options.tool.name, sealed)));
                        const wanted = Option.firstSomeOf([Option.map(resumed, Struct.get('handle')), Option.fromUndefinedOr(given)]);
                        const { id, row, fibers } = yield* Option.match(wanted, {
                            onNone: () => issueHandle,
                            onSome: (known) => Effect.mapError(findHandle(known), () => McpSchema.InvalidParams.make({ message: `Handle ${known} ${isUuidV7(known) ? `expired after ${kept} without a call` : 'is not one this server issued'}, call again without handle to start one` })),
                        });
                        const issued = Option.isNone(wanted) || Option.exists(resumed, Struct.get('issued')) ? Option.some(`handle ${id} holds your state, pass it on later calls`) : Option.none<string>();
                        yield* Effect.acquireRelease(TxSubscriptionRef.update(row, Struct.evolve({ answering: Number.increment })), () => TxSubscriptionRef.update(row, Struct.evolve({ answering: Number.decrement })));
                        const [blocks, stated, asked, pace, frozen] = yield* Effect.all([
                            Ref.make(Array.empty<McpSchema.ContentBlock>()),
                            Ref.make(Array.empty<McpSchema.TextContent>()),
                            Deferred.make<McpSchema.InputRequired>(),
                            SubscriptionRef.make({ last: -1, offset: 0 }),
                            Effect.flatMap(TxSubscriptionRef.get(row), TxSubscriptionRef.make),
                        ]);
                        const [bound, ending, token] = [handleOf(row), Stream.provideService(ends, Handle, handleOf(frozen)), Schema.decodeUnknownOption(McpSchema.ProgressToken)(request.requestMetadata?.progressToken)];
                        const notify = (report: Parameters<(typeof ToolCall)['Service']['progress']>[0]) => Option.match(token, { onNone: () => Effect.void, onSome: (progressToken) => server.notifications['notifications/progress']({ progressToken, ...report }) });
                        const poll = SubscriptionRef.updateAndGet(pace, Struct.evolve({ last: Number.increment, offset: Number.increment })).pipe(Effect.flatMap(({ last }) => notify({ progress: last })));
                        const ended = Stream.runHead(Stream.merge(ending, Option.match(token, { onNone: () => Stream.empty, onSome: () => pace.pipe(SubscriptionRef.changes, Stream.debounce(Duration.minutes(1)), Stream.mapEffect(Function.constant(poll)), Stream.drain) })));
                        const answer = Effect.zipWith(Effect.catchTag(options.handle(Record.remove(payload, 'handle')), 'InvalidParams', refused), Effect.all([Ref.get(stated), Ref.get(blocks)]), (outcome, [lines, more]) =>
                            outcome instanceof McpSchema.InputRequired ? outcome : McpSchema.CallToolResult.make({ ...outcome, content: [...lines, ...outcome.content, ...more] }),
                        ).pipe(
                            Effect.raceFirst(Deferred.await(asked)),
                            Effect.provideService(ToolCall, {
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
                        const fiber = yield* sequence.pipe(
                            TxRef.get,
                            Effect.flatMap((from) => Effect.ensuring(answer, TxSubscriptionRef.update(row, routeEvents(limit, from, [])))),
                            FiberSet.run(fibers),
                        );
                        const started = { tool: options.tool.name, fiber, ends: ending };
                        const continued = TxSubscriptionRef.update(row, (slot) => ({ ...slot, calls: slot.calls + 1, running: [...slot.running, { ...started, call: slot.calls + 1 }] }));
                        const [outcome, early] = yield* Fiber.join(fiber).pipe(
                            Effect.map((value) => [value, false] as const),
                            Effect.raceFirst(Effect.flatMap(ended, Option.match({ onNone: () => Effect.never, onSome: (text) => Effect.map(Effect.provideService(text, Handle, bound), (rendered) => [McpSchema.CallToolResult.make({ content: [McpSchema.TextContent.make({ text: rendered })] }), true] as const) }))),
                            Effect.interruptible,
                            Effect.onInterrupt(() => Effect.flatMap(Deferred.isDone(draining), Boolean.match({ onTrue: () => continued, onFalse: () => Fiber.interrupt(fiber) }))),
                        );
                        return yield* outcome instanceof McpSchema.InputRequired
                            ? Effect.map(
                                  Effect.provideContext(Effect.orDie(Jwt.encrypt(Resumed, { handle: id, issued: Option.isSome(issued), state: Option.fromUndefinedOr(outcome.requestState), aud: options.tool.name }, key)), crypto),
                                  (sealed) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState: Redacted.value(sealed) }),
                              )
                            : composeResult(outcome, { row, limit, budget, stated: !early && Array.isReadonlyArrayNonEmpty(yield* Ref.get(stated)), issued, early: early ? Option.some(started) : Option.none() });
                    },
                    Effect.catchTag('InvalidParams', refused),
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
            events: ({ stream, render, owns = Function.constTrue, leads = Function.constTrue, delivers = Option.none, standing = Option.none }) =>
                Stream.runForEach(stream, (event) =>
                    Effect.tx(
                        Effect.flatMap(Effect.all([TxRef.modify(sequence, (count) => [count + 1, count + 1]), TxSubscriptionRef.get(live)]), ([at, rows]) => {
                            const stated = standing(event);
                            return Effect.forEach(
                                Record.values(rows),
                                TxSubscriptionRef.update(
                                    routeEvents(limit, at - 1, [
                                        {
                                            sequence: at,
                                            text: render(event),
                                            owns: (held) => owns(event, held),
                                            leads: (held) => leads(event, held),
                                            standing: stated,
                                            delivers: (held) => Option.match(stated, { onNone: () => delivers(event, held), onSome: Option.liftPredicate(Handles.Standing.guards.Waiting) }),
                                        },
                                    ]),
                                ),
                                { discard: true },
                            );
                        }),
                    ),
                ).pipe(Effect.forkScoped, Effect.asVoid),
            states: (reference) =>
                TxSubscriptionRef.changesStream(live).pipe(
                    Stream.switchMap((rows) => Array.match(Record.values(rows), { onEmpty: () => Stream.make(Array.empty<HandleState>()), onNonEmpty: (each) => Stream.zipLatestAll(...Array.map(each, TxSubscriptionRef.changesStream)) })),
                    Stream.map(Array.map((slot) => Context.get(slot.state, reference))),
                ),
            persist: Effect.fnUntraced(function* <A, RD, RE>(reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) {
                const codec = Schema.toCodecJson(schema);
                const [context, rows] = yield* Effect.all([Effect.context<RD | RE>(), TxSubscriptionRef.get(live)]);
                yield* Ref.update(
                    persisted,
                    Record.set(reference.key, (held: Context.Context<never>) => held.pipe(Context.get(reference), Schema.encodeEffect(codec), Effect.provideContext(context), Effect.orDie)),
                );
                const values = yield* Effect.all(
                    Function.pipe(
                        yield* Effect.all(Record.map(rows, TxSubscriptionRef.get)),
                        Record.filterMap((slot) => Result.fromOption(Record.get(slot.pending, reference.key), Function.constVoid)),
                        Record.map((json) =>
                            Effect.matchEffect(Effect.provideContext(Schema.decodeEffect(codec)(json), context), {
                                onFailure: (error) => Effect.as(Effect.annotateLogs(Effect.logWarning('Stored handle state no longer decodes, the handle keeps its default', error), { reference: reference.key }), Option.none<A>()),
                                onSuccess: Effect.succeedSome,
                            }),
                        ),
                    ),
                );
                yield* Effect.all(
                    Record.collect(Record.intersection(rows, values, Tuple.make), (_, [row, value]) =>
                        TxSubscriptionRef.update(
                            row,
                            Struct.evolve({
                                pending: (held: HandleState['pending']) => Record.remove(held, reference.key),
                                state: (held: Context.Context<never>) => Option.match(value, { onNone: () => held, onSome: (decoded) => Context.add(held, reference, decoded) }),
                            }),
                        ),
                    ),
                    { discard: true },
                );
            }),
            drain: Effect.gen(function* () {
                yield* Deferred.succeed(draining, undefined);
                const slots = yield* Effect.all(Record.map(yield* TxSubscriptionRef.get(live), TxSubscriptionRef.get));
                yield* Fiber.interruptAll(Record.values(slots).flatMap(({ running }) => running.map(Struct.get('fiber'))));
            }),
            notifyListChanged,
            bind: Effect.scoped(Effect.map(issueHandle, ({ id }) => ({ handle: id, server: scheme }))),
            follow: (handle) => Effect.map(Effect.scoped(findHandle(handle)), queued),
            ack: (handle, through) => Effect.scoped(Effect.flatMap(findHandle(handle), ({ row }) => TxSubscriptionRef.update(row, acknowledge(through)))),
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
    const served = Context.get(built, Server);
    yield* Effect.addFinalizer(() => served.drain);
    const shown = yield* Effect.filter([...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts], ({ annotations }) => Context.get(annotations, Server.Visibility));
    return { options: { ...manifest, protocols: Server.protocols, instructions: Array.dedupe([...instructions, ...Array.flatMap(shown, ({ annotations }) => Context.get(annotations, Server.Instructions))]).join('\n') }, served };
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
    Match.when({ _tag: 'Failure' }, ({ cause }) => Option.match(Option.filter(Cause.findErrorOption(cause), Schema.is(Coded)), { onNone: () => ({ [ATTR_ERROR_TYPE]: ERROR_TYPE_VALUE_OTHER }), onSome: ({ code }) => ({ [ATTR_ERROR_TYPE]: `${code}` }) })),
    Match.orElse(() => ({})),
);
const operations: Readonly<Partial<Record<McpServer.MiddlewareMethod, (target: string) => Readonly<Record<string, string>>>>> = {
    'tools/call': (target) => ({ [ATTR_GEN_AI_TOOL_NAME]: target, [ATTR_GEN_AI_OPERATION_NAME]: GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL }),
    'resources/read': (target) => ({ [ATTR_MCP_RESOURCE_URI]: target }),
    'prompts/get': (target) => ({ [ATTR_GEN_AI_PROMPT_NAME]: target }),
    'completion/complete': () => ({}),
};
const metrics = McpServer.middleware((effect, { method, target }) =>
    UndefinedOr.match(operations[method], {
        onUndefined: () => effect,
        onDefined: Effect.fnUntraced(function* (attributing) {
            const [duration, exit] = yield* effect.pipe(Effect.exit, Effect.timed);
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
const refused = ({ message }: McpSchema.InvalidParams) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message })] }));
const touch = ({ row }: { readonly row: TxSubscriptionRef.TxSubscriptionRef<HandleState> }) => Effect.addFinalizer(() => Effect.flatMap(DateTime.now, (at) => TxSubscriptionRef.update(row, Struct.evolve({ touched: () => Option.some(at) }))));
const send = (slot: HandleState) => {
    const [due, kept] = Array.partition(slot.entries, (each) => Option.match(each.delivers(slot.state), { onNone: () => Result.fail(each), onSome: (pushed) => Result.succeed({ ...each, pushed }) }));
    return Tuple.make(due, { ...slot, entries: kept, sent: [...slot.sent, ...due] });
};
const enqueue = (limit: number, owned: readonly Entry[]) => (slot: HandleState) => {
    const merged = Array.sortWith([...slot.entries, ...owned], (each) => each.sequence, Number.Order);
    const [gone, entries] = Array.splitAt(merged, merged.length - limit);
    return { ...slot, dropped: slot.dropped + gone.length, entries };
};
const formatLine = (sequence: number, line: string) => `! #${sequence} ${line}`.replaceAll('\n', '\n  ');
const formatEvent = ({ pushed, ...entry }: ReturnType<typeof send>[0][number]): Effect.Effect<typeof Handles.Event.Type> =>
    Handles.Standing.guards.Waiting(pushed) ? Effect.map(entry.text, (line) => Handles.Event.cases.Waiting.make({ sequence: entry.sequence, line, key: pushed.key })) : Effect.map(entry.text, (line) => Handles.Event.cases.Line.make({ sequence: entry.sequence, line: formatLine(entry.sequence, line), kind: pushed }));
const settle = (waiting: Record.ReadonlyRecord<string, number>, sent: readonly (typeof Handles.Event.Type)[], entries: readonly Entry[]) => {
    const opened = Array.reduce(sent, waiting, (known, event) => (Handles.Event.guards.Waiting(event) ? Record.set(known, event.key, event.sequence) : known));
    const closing = Record.fromEntries(
        Array.flatMap(entries, ({ sequence, standing }) =>
            Option.filter(standing, Handles.Standing.guards.Settled).pipe(
                Option.map(({ key }) => [key, sequence] as const),
                Option.toArray,
            ),
        ),
    );
    const [settled, open] = Record.partition(opened, (at, key) => (Option.exists(Record.get(closing, key), Number.isGreaterThan(at)) ? Result.succeed(at) : Result.fail(at)));
    return [open, [...sent, ...Array.map(Record.keys(settled), (key) => Handles.Event.cases.Settled.make({ key }))]] as const;
};
const isAfter = (sequence: number) => Predicate.mapInput(Number.isGreaterThan(sequence), (entry: Entry) => entry.sequence);
const acknowledge =
    (through: number) =>
    (slot: HandleState): HandleState => ({ ...slot, sent: Array.filter(slot.sent, isAfter(through)), entries: Array.filter(slot.entries, (each) => isAfter(through)(each) || Option.isNone(each.delivers(slot.state))) });
const routeEvents =
    (limit: number, from: number, raised: readonly Entry[]) =>
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
        const [leading, waiting] = Array.partition(slot.entries, (each) => (each.leads(slot.state) ? Result.succeed(each) : Result.fail(each)));
        return [
            { ...slot, entries: leading, ended, pending, started },
            { ...slot, dropped: 0, entries: Array.filter(waiting, (each) => Option.isSome(each.delivers(slot.state))), calls: slot.calls + started.length, running: [...pending, ...started] },
        ];
    });
    const rendered = yield* Effect.forEach(taken.entries, Struct.get('text')).pipe(Effect.provideService(Handle, handleOf(row)));
    const runs = Array.match(Array.zip(taken.entries, rendered), { onEmpty: Array.empty, onNonEmpty: (pairs) => Array.groupWith(pairs, ([, left], [, right]) => left === right) });
    const [, fitted] = Array.mapAccum(runs, budget, (left, run, index) => {
        const [entry, text] = Array.headNonEmpty(run);
        const line = formatLine(entry.sequence, [text, ...(run.length > 1 ? [`${run.length} times`] : [])].join(', '));
        return [left - line.length - 1, index === 0 || left > line.length ? Result.succeed(line) : Result.fail(run.map(Tuple.get(0)))];
    });
    const [shown, deferred] = Array.separate(fitted);
    const later = deferred.flat();
    yield* TxSubscriptionRef.update(row, enqueue(limit, later));
    const finished = Array.map(taken.ended, ({ call, tool, exit }) => {
        const result = callResult(exit);
        const { texts, blocks } = split(result);
        return { blocks, line: [`finished: call ${call} ${tool}${blocks.length > 0 ? ` +${blocks.length} blocks` : ''}${result.isError === true ? ' failed:' : ''}`, ...texts].join(' ').replaceAll('\n', '\n  ') };
    });
    const { texts, blocks } = split(outcome);
    const [state, body] = Array.splitAt(texts, stated ? 1 : 0);
    const leading = [
        ...Option.toArray(issued),
        ...(taken.dropped > 0 ? [`! ${taken.dropped} older events dropped unread, more arrived than the handle keeps`] : []),
        ...shown,
        ...(later.length > 0 ? [`! ${later.length} more events lead a later result`] : []),
        ...Array.map(taken.started, ({ call, tool }) => `early: call ${call} ${tool} keeps running, its outcome leads a later result`),
        ...(taken.pending.length > 0 ? [`pending: ${LISTING.format(Array.map(taken.pending, ({ call, tool }) => `call ${call} ${tool}`))} ${taken.pending.length === 1 ? 'keeps' : 'keep'} running`] : []),
        ...Array.map(finished, Struct.get('line')),
    ];
    const text = [...state, ...leading, ...(Array.isReadonlyArrayNonEmpty(leading) && Array.isReadonlyArrayNonEmpty(body) ? [''] : []), ...body];
    return McpSchema.CallToolResult.make({ ...Struct.omit(outcome, ['structuredContent']), content: [...Array.match(text, { onEmpty: () => [], onNonEmpty: (rows) => [McpSchema.TextContent.make({ text: rows.join('\n') })] }), ...blocks, ...Array.flatMap(finished, Struct.get('blocks'))] });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
