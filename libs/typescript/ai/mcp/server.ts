import { ATTR_ERROR_TYPE, ATTR_GEN_AI_OPERATION_NAME, ATTR_GEN_AI_PROMPT_NAME, ATTR_GEN_AI_TOOL_NAME, ATTR_MCP_METHOD_NAME, ATTR_MCP_RESOURCE_URI, ERROR_TYPE_VALUE_OTHER, GEN_AI_OPERATION_NAME_VALUE_EXECUTE_TOOL, METRIC_MCP_SERVER_OPERATION_DURATION } from '@opentelemetry/semantic-conventions/incubating';
import { Column } from '@rasm/database/column';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Boolean, Cause, Context, Deferred, Duration, Effect, Exit, Fiber, FiberSet, Function, Layer, Match, Metric, Number, Option, Order, RcMap, Record, Redacted, Ref, Result, Schema, SchemaGetter, type Scope, Stream, Struct, SubscriptionRef, Tuple, TxRef, TxSubscriptionRef, UndefinedOr } from 'effect';
import { McpProtocol, McpSchema, McpServer, type Tool, type Toolkit } from 'effect/ai';
import { type Aead, SecureRandom } from 'effect/crypto';
import { HttpRouter, HttpServerRequest, HttpServerResponse } from 'effect/http';
import { HttpApiBuilder, HttpApiError } from 'effect/http-api';
import { Jwk, Jwt } from 'effect/jose';
import { Host, NetAddress } from 'effect/net';
import { Handles, Notice } from '#/handles';
import { callResult, type HandleState, openStore } from '#/store';
import { registerToolkit, ToolCall } from '#/toolkit';

// --- [TYPES] ---------------------------------------------------------------------------

interface Settled {
    readonly exit: Option.Option<Parameters<typeof callResult>[0]>;
    readonly detached: Option.Option<HandleState['running'][number]>;
}
type Routed = Option.Option<{ readonly text?: string; readonly push: boolean; readonly key?: string; readonly row?: (typeof Handles.Rows.Type)['items'][number]; readonly clears?: string; readonly state?: Context.Context<never> }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRESH: HandleState = { state: Context.empty(), pending: {}, dropped: 0, held: [], notices: [], handed: [], rows: { revision: 0, items: [] }, blocks: [], calls: 0, polls: 0, answering: 0, running: [] };

// --- [MODELS] --------------------------------------------------------------------------

const Coded = Schema.Struct({ code: McpSchema.McpErrorBase.fields.code });
const Resumed = Schema.Struct({ handle: Schema.String, issued: Schema.Boolean, state: Schema.OptionFromOptionalKey(Schema.String), aud: Schema.String });
const Given = Schema.Struct({ handle: Schema.optionalKey(Schema.String.annotate({ description: 'Handle a result gave, omit to start one' })) });
const isUuidV7 = Schema.is(Column.Uuid);
const sameRows = Schema.toEquivalence(Handles.Rows.fields.items);

// --- [SERVICES] ------------------------------------------------------------------------

const Row = Context.Reference<Option.Option<TxSubscriptionRef.TxSubscriptionRef<HandleState>>>('@rasm/mcp/server/Row', { defaultValue: Option.none });
const callRow = Row.pipe(Effect.flatMap(Effect.fromOption), Effect.orDie);
const callState = {
    get: <A>(key: Context.Reference<A>) => Effect.map(Effect.flatMap(callRow, TxSubscriptionRef.get), ({ state }) => Context.get(state, key)),
    update: <A>(key: Context.Reference<A>, change: (current: A) => A) => Effect.flatMap(callRow, (slot) => TxSubscriptionRef.update(slot, (before) => ({ ...before, state: Context.add(before.state, key, change(Context.get(before.state, key))) }))),
    set: <A>(key: Context.Reference<A>, value: A) => callState.update(key, () => value),
};
class Handle extends Context.Service<Handle, typeof callState>()('@rasm/mcp/server/Handle') {
    static readonly current = Effect.map(
        Row,
        Option.map(() => callState),
    );
}

class Server extends Context.Service<
    Server,
    {
        readonly uri: (path: string) => string;
        readonly events: <A>(source: {
            readonly stream: Stream.Stream<A>;
            readonly route: (event: A, state: Context.Context<never>) => Routed;
            readonly admit?: (call: number, state: Context.Context<never>) => Routed;
            readonly press?: (key: string) => Effect.Effect<void, Error>;
        }) => Effect.Effect<void, never, Scope.Scope>;
        readonly states: <A>(key: Context.Reference<A>) => Stream.Stream<readonly A[]>;
        readonly persist: <A, RD, RE>(key: string, reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) => Effect.Effect<void, never, RD | RE>;
        readonly drain: Effect.Effect<void>;
        readonly notifyListChanged: Effect.Effect<void, Error>;
        readonly bind: Effect.Effect<typeof Handles.Binding.Type>;
        readonly poll: (handle: string, query: { readonly after: number; readonly rows: number; readonly idle: boolean; readonly wait: number }) => Effect.Effect<typeof Handles.Polled.Type, Cause.NoSuchElementError>;
        readonly press: (handle: string, key: string) => Effect.Effect<void, Cause.NoSuchElementError | Cause.UnknownError>;
        readonly release: (handle: string) => Effect.Effect<void, Cause.NoSuchElementError>;
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
    static readonly orInternalError = Effect.mapError(({ message }: Error) => McpSchema.InternalError.make({ message }));
    static readonly persist = <A, RD, RE>(key: string, reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) => Server.use((served) => served.persist(key, reference, schema));
    static readonly progress =
        <A>(report: (value: A, index: number) => Parameters<(typeof ToolCall)['Service']['progress']>[0]) =>
        <E, R>(self: Stream.Stream<A, E, R>) =>
            Stream.mapEffect(self, (value, index) => ToolCall.use(({ progress }) => Effect.as(progress(report(value, index)), value)));
    static readonly toolkit = <Tools extends Record.ReadonlyRecord<string, Tool.Any & { readonly parametersSchema: Schema.Decoder<unknown> }>>(toolkit: Toolkit.Toolkit<Tools>, options?: Parameters<typeof registerToolkit<Tools>>[1]) =>
        Layer.provide(Layer.effectDiscard(Effect.andThen(Server, registerToolkit(toolkit, options))), handled.layer);
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
        const [server, scope, live, random, crypto, draining, persisted, pressers, admitters] = yield* Effect.all([
            McpServer.McpServer,
            Effect.scope,
            TxSubscriptionRef.make<Record.ReadonlyRecord<string, TxSubscriptionRef.TxSubscriptionRef<HandleState>>>({}),
            SecureRandom.SecureRandom,
            Effect.context<Aead.Aead | SecureRandom.SecureRandom>(),
            UndefinedOr.match(signal, { onUndefined: () => Deferred.make<void>(), onDefined: Effect.succeed }),
            Ref.make(Record.empty<string, (state: Context.Context<never>) => Effect.Effect<Schema.Json>>()),
            Ref.make(Array.empty<(key: string) => Effect.Effect<void, Error>>()),
            Ref.make(Array.empty<(call: number, state: Context.Context<never>) => Routed>()),
        ]);
        const handles = yield* RcMap.make({
            lookup: (id: string) => Effect.tap(Effect.all({ id: Effect.succeed(id), row: TxSubscriptionRef.make(FRESH), fibers: FiberSet.make() }), ({ row }) => Effect.acquireRelease(TxSubscriptionRef.update(live, Record.set(id, row)), () => TxSubscriptionRef.update(live, Record.remove(id)))),
            idleTimeToLive: retention,
        });
        const { secret, sequence, cut } = yield* openStore({ scheme, folder, live, handles, persisted });
        const numbered = TxRef.modify(sequence, (count) => [count + 1, count + 1]);
        yield* Effect.forEach(cut, ({ row, ...call }) => Effect.tx(Effect.flatMap(numbered, (at) => TxSubscriptionRef.update(row, finish(limit, at, call, Exit.interrupt())))), { discard: true });
        const key = yield* Effect.orDie(Jwk.importSecret(secret, 'dir'));
        const kept = Duration.format(retention);
        const findHandle = (known: string) => Effect.flatMap(RcMap.getOption(handles, known), Effect.fromOption);
        const issueHandle = Effect.flatMap(random.uuidV7(), (id) => RcMap.get(handles, id));
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
                        const left = TxSubscriptionRef.update(row, (slot) => ({ ...settle(slot), answering: slot.answering - 1 }));
                        const [at, admits] = yield* Effect.all([numbered, Ref.get(admitters)]);
                        const call = yield* Effect.acquireRelease(TxSubscriptionRef.modify(row, admitCall(limit, at, admits)), () => left);
                        const [blocks, asked, pace, early, settled] = yield* Effect.all([
                            Ref.make(Array.empty<McpSchema.ContentBlock>()),
                            Deferred.make<McpSchema.InputRequired>(),
                            SubscriptionRef.make({ last: -1, offset: 0 }),
                            SubscriptionRef.make(Array.empty<Stream.Stream<Effect.Effect<string>>>()),
                            TxRef.make({ exit: Option.none<Parameters<typeof callResult>[0]>(), detached: Option.none<HandleState['running'][number]>() }),
                        ]);
                        const token = Schema.decodeUnknownOption(McpSchema.ProgressToken)(request.requestMetadata?.progressToken);
                        const ends = SubscriptionRef.changes(early).pipe(Stream.switchMap((streams) => Stream.mergeAll(streams, { concurrency: 'unbounded' })));
                        const notify = (report: Parameters<(typeof ToolCall)['Service']['progress']>[0]) => Option.match(token, { onNone: () => Effect.void, onSome: (progressToken) => server.notifications['notifications/progress']({ progressToken, ...report }) });
                        const poll = SubscriptionRef.updateAndGet(pace, Struct.evolve({ last: Number.increment, offset: Number.increment })).pipe(Effect.flatMap(({ last }) => notify({ progress: last })));
                        const ended = Stream.runHead(Stream.merge(ends, Option.match(token, { onNone: () => Stream.empty, onSome: () => pace.pipe(SubscriptionRef.changes, Stream.debounce(Duration.minutes(1)), Stream.mapEffect(Function.constant(poll)), Stream.drain) })));
                        const answer = Effect.zipWith(Effect.catchTag(options.handle(Record.remove(payload, 'handle')), 'InvalidParams', refused), Ref.get(blocks), (outcome, more) =>
                            outcome instanceof McpSchema.InputRequired ? outcome : McpSchema.CallToolResult.make({ ...outcome, content: [...outcome.content, ...more] }),
                        ).pipe(
                            Effect.raceFirst(Deferred.await(asked)),
                            Effect.provideService(ToolCall, {
                                add: (block) => Ref.update(blocks, Array.append(block)),
                                progress: (report) => SubscriptionRef.modify(pace, ({ offset }) => [Struct.evolve(report, { progress: Number.sum(offset), total: UndefinedOr.map(Number.sum(offset)) }), { last: report.progress + offset, offset }]).pipe(Effect.flatMap(notify)),
                                input: (required) => Effect.andThen(Deferred.succeed(asked, required), Effect.never),
                                returnEarly: (stream) => SubscriptionRef.update(early, Array.append(stream)),
                                awaitRunning: Effect.map(TxSubscriptionRef.get(row), Struct.get('running')).pipe(Effect.flatMap(Effect.forEach((each) => (each.call < call ? Effect.raceFirst(Fiber.await(each.fiber), Stream.runHead(each.ends)) : Effect.void), { concurrency: 'unbounded', discard: true }))),
                                call,
                            }),
                            Effect.provideService(Row, Option.some(row)),
                            Effect.updateService(McpSchema.McpRequestContext, (context) => ({ ...context, requestState: Option.getOrUndefined(Option.flatMap(resumed, Struct.get('state'))) })),
                            Effect.scoped,
                        );
                        const fiber = yield* answer.pipe(
                            Effect.onExit((exit) => settleCall(row, limit, numbered, settled, { exit: Option.some(exit) })),
                            FiberSet.run(fibers),
                        );
                        const started = { call, tool: options.tool.name, fiber, ends };
                        const detach = settleCall(row, limit, numbered, settled, { detached: Option.some(started) });
                        yield* Effect.addFinalizer((exit) => (Exit.hasInterrupts(exit) ? Effect.andThen(detach, fiber.pipe(Fiber.interrupt, Effect.when(Deferred.isDone(draining)), Effect.asVoid)) : Effect.void));
                        const returned = yield* Fiber.await(fiber).pipe(Effect.as(Option.none<string>()), Effect.raceFirst(Effect.asSome(Effect.flatMap(ended, Option.match({ onNone: () => Effect.never, onSome: Function.identity })))), Effect.interruptible);
                        yield* Effect.when(detach, Effect.succeed(Option.isSome(returned)));
                        const outcome = yield* Option.match(returned, {
                            onNone: () => Fiber.join(fiber),
                            onSome: (text) => Effect.succeed(McpSchema.CallToolResult.make({ content: [McpSchema.TextContent.make({ text })] })),
                        });
                        return yield* outcome instanceof McpSchema.InputRequired
                            ? Effect.map(
                                  Effect.provideContext(Effect.orDie(Jwt.encrypt(Resumed, { handle: id, issued: Option.isSome(issued), state: Option.fromUndefinedOr(outcome.requestState), aud: options.tool.name }, key)), crypto),
                                  (sealed) => new McpSchema.InputRequired({ inputRequests: outcome.inputRequests, requestState: Redacted.value(sealed) }),
                              )
                            : composeResult(outcome, { row, budget, issued, started });
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
            events: ({ stream, route, admit, press }) =>
                Effect.andThen(Ref.update(admitters, Array.appendAll(Array.fromNullishOr(admit))), Ref.update(pressers, Array.appendAll(Array.fromNullishOr(press)))).pipe(
                    Effect.andThen(
                        Stream.runForEach(stream, (event) =>
                            Effect.tx(
                                Effect.flatMap(Effect.all([numbered, TxSubscriptionRef.get(live)]), ([at, rows]) => {
                                    const routed = (slot: HandleState) => place(limit, at, route(event, slot.state), false)(slot);
                                    const raised = (slot: HandleState): HandleState => (slot.answering > 0 ? { ...slot, held: [...slot.held, routed] } : routed(slot));
                                    return Effect.forEach(Record.values(rows), TxSubscriptionRef.update(raised), { discard: true });
                                }),
                            ),
                        ),
                    ),
                    Effect.forkScoped,
                    Effect.asVoid,
                ),
            states: (reference) =>
                TxSubscriptionRef.changesStream(live).pipe(
                    Stream.switchMap((rows) => Array.match(Record.values(rows), { onEmpty: () => Stream.make(Array.empty<HandleState>()), onNonEmpty: (each) => Stream.zipLatestAll(...Array.map(each, TxSubscriptionRef.changesStream)) })),
                    Stream.map(Array.map((slot) => Context.get(slot.state, reference))),
                ),
            persist: Effect.fnUntraced(function* <A, RD, RE>(stored: string, reference: Context.Reference<A>, schema: Schema.Codec<A, unknown, RD, RE>) {
                const codec = Schema.toCodecJson(schema);
                const [context, rows] = yield* Effect.all([Effect.context<RD | RE>(), TxSubscriptionRef.get(live)]);
                yield* Ref.update(
                    persisted,
                    Record.set(stored, (held: Context.Context<never>) => held.pipe(Context.get(reference), Schema.encodeEffect(codec), Effect.provideContext(context), Effect.orDie)),
                );
                const values = yield* Effect.all(
                    Function.pipe(
                        yield* Effect.all(Record.map(rows, TxSubscriptionRef.get)),
                        Record.filterMap((slot) => Result.fromOption(Record.get(slot.pending, stored), Function.constVoid)),
                        Record.map((json) =>
                            Effect.matchEffect(Effect.provideContext(Schema.decodeEffect(codec)(json), context), {
                                onFailure: (error) => Effect.as(Effect.annotateLogs(Effect.logWarning('Stored handle state no longer decodes, the handle keeps its default', error), { key: stored }), Option.none<A>()),
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
                                pending: (held: HandleState['pending']) => Record.remove(held, stored),
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
            poll: Effect.fnUntraced(function* (handle: string, { after, wait, ...query }: Parameters<Server['Service']['poll']>[1]) {
                const { id, row } = yield* findHandle(handle);
                const turn = yield* TxSubscriptionRef.modify(row, (slot) => [slot.polls + 1, claim(after)(slot)]);
                const taken = Effect.tx(
                    Effect.gen(function* () {
                        const [known, slot] = yield* Effect.all([TxSubscriptionRef.get(live), TxSubscriptionRef.get(row)]);
                        yield* Effect.when(new Cause.NoSuchElementError(`Handle ${id} was released`), Effect.succeed(!Record.has(known, id)));
                        const [answer, next] = yield* Option.match(hand(turn, query)(slot), { onNone: () => Effect.txRetry, onSome: Effect.succeed });
                        yield* TxSubscriptionRef.set(row, next);
                        return answer;
                    }),
                );
                const answered = yield* taken.pipe(Effect.timeoutOption(Duration.seconds(wait)), Effect.raceFirst(Effect.as(Deferred.await(draining), Option.none())));
                return yield* Option.match(answered, { onNone: () => Effect.map(TxSubscriptionRef.get(row), (slot) => ({ notices: [], rows: slot.rows })), onSome: Effect.succeed });
            }, Effect.scoped),
            press: Effect.fnUntraced(function* (handle: string, pressed: string) {
                const { row } = yield* findHandle(handle);
                const [slot, sources] = yield* Effect.all([TxSubscriptionRef.get(row), Ref.get(pressers)]);
                yield* Effect.when(new Cause.NoSuchElementError(`Handle ${handle} holds no row ${pressed}`), Effect.succeed(!slot.rows.items.some((item) => item.key === pressed)));
                yield* Effect.mapError(Effect.flip(Effect.forEach(sources, (run) => Effect.flip(run(pressed)))), (misses) => new Cause.UnknownError(misses, `Row ${pressed} answered no press`));
            }, Effect.scoped),
            release: (handle) => Effect.scoped(findHandle(handle)).pipe(Effect.andThen(TxSubscriptionRef.update(live, Record.remove(handle))), Effect.andThen(RcMap.invalidate(handles, handle))),
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
        const transport = Layer.unwrap(
            Effect.map(serverOptions(manifest, settings, app), ({ options, served }) =>
                Layer.merge(
                    McpServer.layerHttp({ ...options, path: '/', allowSessionTermination: true, allowedOrigins }),
                    HttpApiBuilder.layer(Handles).pipe(
                        Layer.provide(
                            HttpApiBuilder.group(Handles, 'handles', (handlers) =>
                                handlers.handleAll({
                                    bind: () => served.bind,
                                    poll: ({ params, query }) => Effect.mapError(served.poll(params.handle, query), () => HttpApiError.NotFound.make()),
                                    press: ({ params }) =>
                                        served.press(params.handle, params.key).pipe(
                                            Effect.tapErrorTag('UnknownError', (error) => Effect.logWarning('Row press failed', error)),
                                            Effect.mapError((error) => (Cause.isUnknownError(error) ? HttpApiError.ServiceUnavailable.make() : HttpApiError.NotFound.make())),
                                        ),
                                    release: ({ params }) => Effect.mapError(served.release(params.handle), () => HttpApiError.NotFound.make()),
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
const handled = McpServer.middleware<{ provides: Handle }>()((effect) => Effect.provideService(effect, Handle, callState));
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
const split = ({ content }: McpSchema.CallToolResult) => Array.partition(content, (block) => (block.type === 'text' ? Result.succeed(block.text) : Result.fail(block)));
const refused = ({ message }: McpSchema.InvalidParams) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message })] }));
const place =
    (limit: number, sequence: number, routed: Routed, woken: boolean) =>
    (slot: HandleState): HandleState =>
        Option.match(routed, {
            onNone: () => slot,
            onSome: ({ text, push, key, row, clears, state = slot.state }) => {
                const kept = Array.filter(slot.rows.items, (item) => item.key !== clears);
                const items = UndefinedOr.match(row, { onUndefined: () => kept, onDefined: (shown) => (kept.some((item) => item.key === shown.key) ? Array.map(kept, (item) => (item.key === shown.key ? shown : item)) : [...kept, shown]) });
                const rows = sameRows(items, slot.rows.items) ? slot.rows : { revision: slot.rows.revision + 1, items };
                const merged = UndefinedOr.match(text, {
                    onUndefined: () => slot.notices,
                    onDefined: (line) => {
                        const unread = Array.filter(slot.notices, (each) => !Option.exists(each.key, (held) => held === key));
                        const raised = Notice.make({ sequence, text: line, push, wake: push && (woken || Array.isReadonlyArrayNonEmpty(slot.running)), key: Option.fromUndefinedOr(key) });
                        return Array.sortWith([...unread, raised], Struct.get('sequence'), Number.Order);
                    },
                });
                const gone = Number.max(merged.length - limit, 0);
                return state === slot.state && rows === slot.rows && merged === slot.notices ? slot : { ...slot, state, rows, dropped: slot.dropped + gone, notices: Array.drop(merged, gone) };
            },
        });
const admitCall =
    (limit: number, sequence: number, admits: readonly ((call: number, state: Context.Context<never>) => Routed)[]) =>
    (slot: HandleState): [number, HandleState] => [slot.calls + 1, admits.reduce<HandleState>((next, admit) => place(limit, sequence, admit(next.calls, next.state), false)(next), { ...slot, calls: slot.calls + 1, answering: slot.answering + 1 })];
const claim =
    (after: number) =>
    (slot: HandleState): HandleState =>
        Array.match(slot.handed, {
            onEmpty: () => ({ ...slot, polls: slot.polls + 1 }),
            onNonEmpty: (handed) => ({ ...slot, polls: slot.polls + 1, handed: [], notices: Array.lastNonEmpty(handed).sequence === after ? slot.notices : Array.sortWith([...handed, ...slot.notices], Struct.get('sequence'), Number.Order) }),
        });
const hand =
    (turn: number, { rows, idle }: Pick<Parameters<Server['Service']['poll']>[1], 'rows' | 'idle'>) =>
    (slot: HandleState): Option.Option<readonly [typeof Handles.Polled.Type, HandleState]> => {
        const notices = slot.answering > 0 || slot.polls !== turn ? [] : Array.filter(slot.notices, (each) => (idle ? each.wake : each.push));
        const next: HandleState = Array.isReadonlyArrayEmpty(notices) ? slot : { ...slot, handed: notices, notices: Array.difference(slot.notices, notices) };
        return next === slot && slot.polls === turn && slot.rows.revision <= rows ? Option.none() : Option.some([{ notices, rows: slot.rows }, next]);
    };
const settle = (slot: HandleState): HandleState => (slot.answering > 1 ? slot : slot.held.reduce<HandleState>((next, routed) => routed(next), { ...slot, held: [] }));
const finish =
    (limit: number, sequence: number, { call, tool }: Pick<HandleState['running'][number], 'call' | 'tool'>, exit: Parameters<typeof callResult>[0]) =>
    (slot: HandleState): HandleState => {
        const result = callResult(exit);
        const [texts, blocks] = split(result);
        const text = [`finished: call ${call} ${tool}${blocks.length > 0 ? ` +${blocks.length} blocks` : ''}${result.isError === true ? ' failed:' : ''}`, ...texts].join(' ');
        return place(limit, sequence, Option.some({ text, push: true }), true)({ ...slot, running: Array.filter(slot.running, (each) => each.call !== call), blocks: [...slot.blocks, ...blocks] });
    };
const settleCall = (row: TxSubscriptionRef.TxSubscriptionRef<HandleState>, limit: number, numbered: Effect.Effect<number>, settled: TxRef.TxRef<Settled>, change: Partial<Settled>) =>
    Effect.tx(
        Effect.gen(function* () {
            const before = yield* TxRef.get(settled);
            const after = { ...before, ...change };
            yield* TxRef.set(settled, after);
            yield* Option.match(
                Option.filter(Option.all({ exit: after.exit, own: after.detached }), () => Option.isNone(before.exit) || Option.isNone(before.detached)),
                {
                    onNone: () =>
                        TxSubscriptionRef.update(row, (slot) =>
                            Option.match(
                                Option.filter(after.detached, (own) => Option.isNone(after.exit) && !slot.running.includes(own)),
                                { onNone: () => slot, onSome: (own) => ({ ...slot, running: [...slot.running, own] }) },
                            ),
                        ),
                    onSome: ({ exit, own }) => Effect.flatMap(numbered, (sequence) => TxSubscriptionRef.update(row, finish(limit, sequence, own, exit))),
                },
            );
        }),
    );
const composeResult = Effect.fnUntraced(function* (outcome: McpSchema.CallToolResult, { row, issued, started, budget }: { readonly row: TxSubscriptionRef.TxSubscriptionRef<HandleState>; readonly issued: Option.Option<string>; readonly started: HandleState['running'][number]; readonly budget: number }) {
    const taken = yield* TxSubscriptionRef.modify(row, (held) => {
        const slot = settle(held);
        const runs = Array.match(slot.notices, { onEmpty: Array.empty, onNonEmpty: (all) => Array.groupWith(all, (left, right) => left.text === right.text) });
        const [, fitted] = Array.mapAccum(runs, budget, (left, run, index) => {
            const line = [Array.headNonEmpty(run).text, ...(run.length > 1 ? [`${run.length} times`] : [])].join(', ').replaceAll('\n', '\n  ');
            return [left - line.length - 1, index === 0 || left > line.length ? Result.succeed(line) : Result.fail(run)];
        });
        const [shown, deferred] = Array.separate(fitted);
        const later = deferred.flat();
        return [
            { dropped: slot.dropped, shown, later: later.length, blocks: slot.blocks, notices: Array.difference(slot.notices, later), running: slot.running },
            { ...slot, dropped: 0, notices: later, blocks: [] },
        ];
    });
    yield* Effect.addFinalizer((exit) =>
        Exit.hasInterrupts(exit) ? TxSubscriptionRef.update(row, (slot) => ({ ...slot, dropped: slot.dropped + taken.dropped, notices: Array.sortWith([...taken.notices, ...slot.notices], Struct.get('sequence'), Number.Order), blocks: [...taken.blocks, ...slot.blocks] })) : Effect.void,
    );
    const [texts, blocks] = split(outcome);
    const leading = [
        ...Option.toArray(issued),
        ...(taken.dropped > 0 ? [`dropped: ${taken.dropped} notices`] : []),
        ...taken.shown,
        ...(taken.later > 0 ? [`more: ${taken.later} notices`] : []),
        ...(taken.running.includes(started) ? [`early: call ${started.call} ${started.tool} keeps running`] : []),
        ...Array.match(
            Array.filter(taken.running, (each) => each !== started),
            { onEmpty: Array.empty, onNonEmpty: (calls) => [`pending: ${Array.map(calls, (each) => `call ${each.call} ${each.tool}`).join(', ')}`] },
        ),
    ];
    const text = [...leading, ...(Array.isReadonlyArrayNonEmpty(leading) && Array.isReadonlyArrayNonEmpty(texts) ? [''] : []), ...texts];
    return McpSchema.CallToolResult.make({ ...Struct.omit(outcome, ['structuredContent']), content: [...Array.match(text, { onEmpty: () => [], onNonEmpty: (lines) => [McpSchema.TextContent.make({ text: lines.join('\n') })] }), ...blocks, ...taken.blocks] });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Handle, Server };
