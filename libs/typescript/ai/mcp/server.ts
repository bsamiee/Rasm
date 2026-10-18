import { createServer } from 'node:http';
import { stripTypeScriptTypes } from 'node:module';
import { NodeHttpServer, NodeRuntime, NodeServices } from '@effect/platform-node';
import {
    Array,
    Cause,
    Clock,
    Config,
    Context,
    Deferred,
    Duration,
    Effect,
    Equal,
    Exit,
    Filter,
    Formatter,
    Function,
    flow,
    Latch,
    Layer,
    Logger,
    Match,
    Metric,
    Option,
    Predicate,
    pipe,
    Queue,
    Record,
    Ref,
    References,
    Result,
    Runtime,
    Schedule,
    Schema,
    SchemaAST,
    SchemaGetter,
    SchemaIssue,
    Stream,
    Struct,
    SubscriptionRef,
} from 'effect';
import { McpProtocol, McpSchema, McpServer, Tool } from 'effect/ai';
import { Command, Flag } from 'effect/cli';
import { FetchHttpClient, Headers, HttpRouter, HttpServerRequest, HttpServerResponse, HttpTraceContext } from 'effect/http';
import { NetAddress } from 'effect/net';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { Rpc, RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { Socket } from 'effect/socket';
import { Activity, Blocks, Enter, Instructions, type Phase, REDIAL, Surface, Transpile, Unmatched, WIRE } from './link.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Source = Parameters<(typeof McpServer.McpServer.Service)['addTool']>[0];
type Erased = Rpc.AnyWithProps & { readonly payloadSchema: Schema.Codec<unknown, unknown> & { readonly fields: Readonly<Record<string, Schema.Codec<unknown, unknown>>> }; readonly successSchema: Schema.Codec<unknown, unknown>; readonly errorSchema: Schema.Codec<unknown, unknown> };
type Call = <R extends Erased>(rpc: R, payload: R['payloadSchema']['Type']) => Effect.Effect<R['successSchema']['Type'], LinkError>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const DURATION = Metric.timer('mcp.server.operation.duration', { description: 'Duration of MCP server operations' });

// --- [ERRORS] --------------------------------------------------------------------------

class LinkError extends Schema.TaggedError<LinkError, { readonly brand: unique symbol }>()('LinkError', {
    peer: Schema.String,
    reason: Schema.TaggedUnion({ Unlinked: {}, Dropped: { cause: RpcClientError.RpcClientError }, Stalled: { seconds: Schema.Finite }, Refused: { cause: Schema.Unknown } }),
}) {
    override get message(): string {
        return LinkError.fields.reason.match(this.reason, {
            Unlinked: () => `${this.peer} is not linked. Start it, then retry`,
            Dropped: ({ cause }) => `${this.peer} dropped the link mid-call, ${cause.message}. The call may have taken effect, check before repeating it`,
            Stalled: ({ seconds }) => `${this.peer} received the call ${seconds} s ago without starting it. An open menu, dialog, or drag holds it, or ${this.peer} hangs. The call stays queued, check before repeating it`,
            Refused: ({ cause }) => (Predicate.isError(cause) ? cause.message : Formatter.format(cause)),
        });
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

const Progress = Context.Reference<(message: string) => Effect.Effect<void>>('@rasm/mcp/server/Progress', { defaultValue: () => () => Effect.void });

// --- [OPERATIONS] ----------------------------------------------------------------------

const observed =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.gen(function* () {
            const [started, { requestMetadata }] = yield* Effect.all([Clock.currentTimeMillis, McpSchema.McpRequestContext]);
            return yield* self.pipe(
                Effect.onExit(
                    Effect.fnUntraced(function* (exit) {
                        const failure = Exit.match(exit, { onSuccess: (value) => (Predicate.hasProperty(value, 'isError') && value.isError === true ? Option.some('tool_error') : Option.none()), onFailure: (cause) => Option.some(Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error') });
                        const recorded = { 'mcp.method.name': method, ...attributes, ...Option.match(failure, { onNone: () => ({}), onSome: (kind) => ({ 'error.type': kind }) }) };
                        yield* Effect.annotateCurrentSpan(recorded);
                        yield* Metric.update(Metric.withAttributes(DURATION, recorded), Duration.millis((yield* Clock.currentTimeMillis) - started));
                    }),
                ),
                Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) }),
            );
        });
const merged = ([newest, ...older]: Array.NonEmptyReadonlyArray<Source>): Source => {
    const sources = [newest, ...older];
    const definitions = (schema: Schema.JsonObject) => (Schema.is(Schema.JsonObject)(schema.$defs) ? schema.$defs : {});
    const bare = ({ $defs: _, ...schema }: Schema.JsonObject) => schema;
    const either = (schemas: readonly Schema.JsonObject[]): Schema.JsonObject => Array.match(Array.dedupe(schemas), { onEmpty: () => ({}), onNonEmpty: ([only, ...rest]) => (rest.length === 0 ? only : { anyOf: [only, ...rest] }) });
    const inputs = sources.map(({ tool }) => tool.inputSchema);
    const hints = sources.map(({ tool }) => tool.annotations);
    return {
        tool: McpSchema.Tool.make({
            ...newest.tool,
            description: Option.getOrUndefined(Array.findFirst(sources.toReversed(), ({ tool }) => Option.fromUndefinedOr(tool.description))),
            inputSchema: {
                type: 'object',
                properties: Record.map(
                    Array.groupBy(
                        inputs.flatMap((schema) => Record.toEntries(schema.properties ?? {})),
                        ([key]) => key,
                    ),
                    (entries) => either(entries.map(([, schema]) => schema)),
                ),
                required: inputs.map((schema) => schema.required ?? []).reduce(Array.intersection),
                $defs: Object.assign({}, ...inputs.map(definitions)),
            },
            outputSchema: Option.getOrUndefined(Option.map(Option.all(sources.map(({ tool }) => Option.fromUndefinedOr(tool.outputSchema))), (schemas) => ({ ...either(schemas.map(bare)), $defs: Object.assign({}, ...schemas.map(definitions)) }))),
            annotations: McpSchema.ToolAnnotations.make({
                ...newest.tool.annotations,
                readOnlyHint: hints.every((each) => each?.readOnlyHint === true),
                destructiveHint: hints.some((each) => each?.readOnlyHint !== true && each?.destructiveHint !== false),
                idempotentHint: hints.every((each) => each?.idempotentHint === true),
                openWorldHint: hints.some((each) => each?.openWorldHint !== false),
            }),
        }),
        annotations: Context.add(
            sources.reduceRight((context, { annotations }) => Context.merge(context, annotations), Context.empty()),
            Instructions,
            sources.toReversed().flatMap(({ annotations }) => Context.get(annotations, Instructions)),
        ),
        handle: (payload) => older.reduce((attempt, source) => Effect.catch(attempt, () => source.handle(payload)), newest.handle(payload)),
    };
};
const decorated = Effect.fnUntraced(function* (scheme: string) {
    const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(McpSchema.CallToolResult) });
    const [server, scope, held, stacks] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<typeof Row.Type>>()), Ref.make(Record.empty<string, Array.NonEmptyReadonlyArray<Source>>())]);
    const uri = `${scheme}://executions`;
    const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
    const recording = yield* Effect.cached(
        McpServer.registerResource({
            uri,
            name: 'executions',
            description: 'Calls that change state, cancelled before their response, each with its arguments and, once finished, its result. Reads drop a finished call after listing it',
            annotations: Context.make(Instructions, [`- ${uri}: results of calls that change state, cancelled before their response.`]),
            mimeType: 'application/json',
            content: Effect.gen(function* () {
                const rows = yield* Ref.get(held);
                const read = yield* Effect.all(rows);
                yield* Ref.update(held, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                return yield* Effect.orDie(encode(read));
            }),
        }).pipe(Effect.provideService(McpServer.McpServer, server)),
    );
    const kept = Effect.fnUntraced(function* (tool: string, given: Schema.Json, answer: ReturnType<Source['handle']>) {
        const done = yield* Deferred.make<Effect.Success<typeof answer>, Effect.Error<typeof answer>>();
        const row = Effect.map(Deferred.poll(done), (polled) => ({ tool, arguments: given, result: Option.flatMap(polled, flow(Effect.runSyncExit, Exit.match({ onFailure: Option.none, onSuccess: Option.liftPredicate(Schema.is(McpSchema.CallToolResult)) }))) }));
        yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
        return yield* Effect.interruptible(Deferred.await(done)).pipe(Effect.onInterrupt(() => Effect.andThen(Ref.update(held, Array.append(row)), Effect.forkIn(Effect.andThen(Deferred.await(done), server.notifications['notifications/resources/updated']({ uri })), scope))));
    }, Effect.uninterruptible);
    const served = ({ tool, handle }: Source, destructive: boolean) =>
        Effect.fnUntraced(
            function* (payload: Schema.Json) {
                const [blocks, stage, { requestMetadata }] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), SubscriptionRef.make(Option.match(Schema.decodeUnknownOption(Activity)(payload), { onNone: () => tool.name, onSome: Struct.get('description') })), McpSchema.McpRequestContext]);
                yield* Stream.fromIterable(Option.toArray(Schema.decodeUnknownOption(McpSchema.ProgressToken)(requestMetadata?.progressToken))).pipe(
                    Stream.cross(
                        SubscriptionRef.changes(stage).pipe(
                            Stream.switchMap((message) => Stream.as(Stream.tick(Duration.minutes(1)), message)),
                            Stream.zipWithIndex,
                        ),
                    ),
                    Stream.runForEach(([progressToken, [message, progress]]) => server.notifications['notifications/progress']({ progressToken, progress, message })),
                    Effect.forkChild,
                );
                const answer = Effect.gen(function* () {
                    const result = yield* handle(payload);
                    const added = yield* Ref.get(blocks);
                    return Schema.is(McpSchema.CallToolResult)(result) ? McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] }) : result;
                }).pipe(
                    Effect.provideService(Blocks, ({ data, mimeType, uri: file }) =>
                        Ref.update(blocks, Array.append(Option.match(file, { onNone: () => McpSchema.ImageContent.make({ data, mimeType }), onSome: (at) => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri: at, blob: data, mimeType }) }) }))),
                    ),
                    Effect.provideService(Progress, (message) => SubscriptionRef.set(stage, message)),
                );
                return yield* destructive ? kept(tool.name, payload, answer) : answer;
            },
            observed('tools/call', tool.name, { 'gen_ai.tool.name': tool.name, 'gen_ai.operation.name': 'execute_tool' }),
        );
    const ranking =
        <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
        (input: string, context: C) =>
            Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                const ordered = Unmatched.ranked(input, values);
                return McpSchema.CompleteResult.make({ ...rest, completion: { values: [...ordered], total: values.length, hasMore: ordered.length < values.length } });
            });
    return McpServer.McpServer.of({
        ...server,
        addTool: Effect.fnUntraced(function* (options) {
            const source = yield* Ref.modify(stacks, (current) => {
                const stack = Array.prepend(Option.getOrElse(Record.get(current, options.tool.name), Array.empty), options);
                return [merged(stack), Record.set(current, options.tool.name, stack)];
            });
            const destructive = source.tool.annotations?.readOnlyHint !== true && source.tool.annotations?.destructiveHint !== false;
            yield* destructive ? recording : Effect.void;
            yield* server.addTool({ ...source, handle: served(source, destructive) });
        }),
        addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(observed('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
        addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(observed('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
        addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(observed('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
    });
});
const expose = Effect.fnUntraced(function* <A, S, R1, R2, R3>(
    { name, description, annotations, parameters, success }: { readonly name: string; readonly description: string | undefined; readonly annotations: Context.Context<never>; readonly parameters: Schema.Top; readonly success: Schema.Top },
    decode: (args: unknown) => Effect.Effect<A, McpSchema.InvalidParams, R1>,
    handle: (decoded: A) => Effect.Effect<S, Error, R2>,
    encode: (result: S) => Effect.Effect<Schema.Json, Schema.SchemaError, R3>,
) {
    const [server, context] = yield* Effect.all([McpServer.McpServer, Effect.context<R1 | R2 | R3>()]);
    const document = (schema: Schema.Top) => {
        const { schema: root, definitions } = Schema.toJsonSchemaDocument(schema);
        return Record.isEmptyRecord(definitions) ? root : { ...root, $defs: definitions };
    };
    const [inputSchema, outputSchema] = yield* Effect.orDie(Effect.all([parameters.pipe(document, Schema.decodeUnknownEffect(McpSchema.ToolJson)), success.pipe(document, Schema.decodeUnknownEffect(McpSchema.ToolOutputJson))]));
    const respond = Effect.fnUntraced(
        function* (args: unknown) {
            const result = yield* handle(yield* decode(args));
            const structuredContent = yield* Effect.orDie(encode(result));
            const text = yield* Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Schema.Json))(structuredContent));
            return McpSchema.CallToolResult.make({ structuredContent, content: [McpSchema.TextContent.make({ text })] });
        },
        Effect.catch((error) =>
            Schema.is(McpSchema.InvalidParams)(error) || (Schema.is(LinkError)(error) && error.reason._tag === 'Unlinked')
                ? Effect.fail(McpSchema.InvalidParams.make({ message: error.message }))
                : Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: error.message })] })),
        ),
        Effect.catchDefect((defect) => Effect.as(Effect.logError(defect), McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: Formatter.format(defect) })] }))),
        Effect.provideContext(context),
    );
    yield* server.addTool({
        tool: McpSchema.Tool.make({
            name,
            description,
            inputSchema,
            outputSchema,
            annotations: {
                title: Option.getOrUndefined(Context.getOption(annotations, Tool.Title)),
                readOnlyHint: Context.get(annotations, Tool.Readonly),
                destructiveHint: Context.get(annotations, Tool.Destructive),
                idempotentHint: Context.get(annotations, Tool.Idempotent),
                openWorldHint: Context.get(annotations, Tool.OpenWorld),
            },
        }),
        annotations,
        handle: respond,
    });
});
const register = <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) =>
    expose(
        { name: tool.name, description: Tool.getDescription(tool), annotations: tool.annotations, parameters: tool.parametersSchema, success: tool.successSchema },
        (args) => Effect.mapError(Schema.decodeUnknownEffect(Schema.toCodecJson(tool.parametersSchema))(args, { errors: 'all' }), ({ message }) => McpSchema.InvalidParams.make({ message })),
        handle,
        Schema.encodeUnknownEffect(Schema.toCodecJson(tool.successSchema)),
    );
const watched = <A, E, R>(peer: string, work: Effect.Effect<A, E, R>, enter: Effect.Effect<typeof Phase.Type, E, R>) => {
    const [stall, quiet] = ['30 seconds', '2 seconds'] as const;
    const stages = {
        Held: `An open menu, dialog, or drag in ${peer} holds the call before it starts`,
        Unanswered: `${peer} sent no answer for ${Duration.toSeconds(quiet)} s while a dialog the call opened, or long work, holds the call. The call finishes once released`,
        Entered: `${peer} runs the call`,
    };
    const probe = Effect.zip(Effect.timeoutOrElse(enter, { duration: quiet, orElse: () => Effect.succeed('Unanswered' as const) }), Clock.currentTimeMillis);
    return Effect.gen(function* () {
        const [missed, report] = yield* Effect.all([Queue.unbounded<void>(), Progress]);
        return yield* Stream.merge(Stream.fromSchedule(Schedule.min([Schedule.exponential(Duration.millis(2)), Schedule.spaced(Duration.seconds(1))])), Stream.fromQueue(missed)).pipe(
            Stream.mapEffect(() => probe, { concurrency: 'unbounded', unordered: true }),
            Stream.tap(([phase]) => (phase === 'Missed' ? Queue.offer(missed, undefined) : Effect.void)),
            Stream.mapAccum(
                () => ({ since: Option.none<number>(), last: Option.none<keyof typeof stages>() }),
                ({ since, last }, [phase, at]) => {
                    const holding = phase === 'Held' ? Option.orElseSome(since, () => at) : Option.none<number>();
                    const stage = ({ Held: Option.some('Held'), Unanswered: Option.some('Unanswered'), Entered: Option.as(last, 'Entered'), Idle: last, Missed: last } satisfies Record<typeof phase, Option.Option<keyof typeof stages>>)[phase];
                    return [
                        { since: holding, last: stage },
                        [
                            {
                                told: Option.map(
                                    Option.filter(stage, () => !Equal.equals(stage, last)),
                                    (each) => stages[each],
                                ),
                                stalled: Option.exists(holding, (start) => at - start >= Duration.toMillis(stall)),
                            },
                        ],
                    ] as const;
                },
            ),
            Stream.tap(({ told }) => Effect.transposeOption(Option.map(told, report))),
            Stream.takeUntil(Struct.get('stalled')),
            Stream.runDrain,
            Effect.andThen(Effect.fail(LinkError.make({ peer, reason: { _tag: 'Stalled', seconds: Duration.toSeconds(stall) } }))),
            Effect.raceFirst(work),
        );
    });
};
const listen = Effect.fn('listen')(function* <Rpcs extends Erased, Peer, I extends Readonly<Record<string, string | undefined>>, F extends Schema.Struct.Fields>(
    group: RpcGroup.RpcGroup<Rpcs>,
    {
        path,
        origin,
        peer: name,
        identity,
        route,
    }: {
        readonly path: HttpRouter.PathInput;
        readonly origin: string;
        readonly peer: string;
        readonly identity: Schema.Codec<Peer, I>;
        readonly route: { readonly fields: F; readonly select: (route: Schema.Struct.Type<F>) => Option.Option<(peer: Peer) => boolean> };
    },
) {
    const rpcs: readonly Erased[] = Array.fromIterable(group.requests.values());
    const mirror = (rpc: Erased) =>
        Rpc.make(rpc._tag, {
            payload: rpc.payloadSchema,
            success: rpc.successSchema,
            error: rpc.errorSchema.pipe(Schema.decodeTo(Schema.toType(LinkError), { decode: SchemaGetter.transform((cause) => LinkError.make({ peer: name, reason: { _tag: 'Refused', cause } })), encode: SchemaGetter.forbidden(() => 'refusals decode only') })),
        });
    interface Linked {
        readonly peer: Peer;
        readonly client: RpcClient.RpcClient.Flat<ReturnType<typeof mirror> | typeof Enter, RpcClientError.RpcClientError>;
    }
    const [router, links] = yield* Effect.all([HttpRouter.HttpRouter, SubscriptionRef.make(Array.empty<Linked>())]);
    const mirrors = RpcGroup.make(...rpcs.map(mirror), Enter);
    const linking = Effect.fnUntraced(
        function* (request: HttpServerRequest.HttpServerRequest) {
            const [found, socket, made, ended] = yield* Effect.all([HttpServerRequest.schemaSearchParams(identity), request.upgrade, Deferred.make<Linked>(), Latch.make()]);
            const client = yield* RpcClient.make(mirrors, { disableTracing: true, flatten: true }).pipe(
                Effect.provideServiceEffect(RpcClient.Protocol, RpcClient.makeProtocolSocket({ pingInterval: Duration.infinity, retryPolicy: Schedule.recurs(0).pipe(Schedule.tap(() => ended.open)) })),
                Effect.provideService(RpcClient.ConnectionHooks, {
                    onConnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.prepend(entry))),
                    onDisconnect: Effect.flatMap(Deferred.await(made), (entry) => SubscriptionRef.update(links, Array.difference([entry]))),
                }),
                Effect.provideService(Socket.Socket, socket),
                Effect.provide(WIRE),
            );
            yield* Deferred.succeed(made, { peer: found, client });
            return yield* Effect.as(ended.await, HttpServerResponse.empty());
        },
        Effect.orElseSucceed(() => HttpServerResponse.empty({ status: 400 })),
    );
    yield* router.add('GET', path, (request) => (request.headers.origin === origin ? linking(request) : Effect.succeed(HttpServerResponse.empty({ status: 403 }))));
    const unlinked = LinkError.make({ peer: name, reason: { _tag: 'Unlinked' } });
    const reach = (matches: (peer: Peer) => boolean) => SubscriptionRef.changes(links).pipe(Stream.filterMap(Filter.fromPredicateOption(Array.findFirst((entry: Linked) => matches(entry.peer)))), Stream.timeout(REDIAL), Stream.runHead, Effect.flatMap(Effect.fromOption(() => unlinked)));
    const send = ({ client }: Linked, tag: string, payload: unknown) =>
        (group.requests.has(Enter._tag) ? watched(name, client(tag, payload), client(Enter._tag, {})) : client(tag, payload)).pipe(Effect.catchTag('RpcClientError', (cause) => Effect.fail(LinkError.make({ peer: name, reason: { _tag: 'Dropped', cause } }))));
    const call =
        (entry: Linked): Call =>
        (rpc, payload) =>
            Effect.flatMap(send(entry, rpc._tag, payload), flow(Schema.decodeUnknownEffect(Schema.toType(rpc.successSchema)), Effect.orDie));
    const annotated = (annotations: Context.Context<never>) => Context.add(annotations, Instructions, [...Context.get(group.annotations, Instructions), ...Context.get(annotations, Instructions)]);
    const tool = <P extends Schema.Top & { readonly fields: Schema.Struct.Fields }, S extends Schema.Top, R>(
        definition: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
        handle: (payload: P['Type'], use: Call) => Effect.Effect<S['Type'], Error, R>,
    ) =>
        expose(
            { name: definition.name, description: Tool.getDescription(definition), annotations: annotated(definition.annotations), parameters: Schema.Struct({ ...definition.parametersSchema.fields, ...route.fields }), success: definition.successSchema },
            Effect.fnUntraced(function* (args: unknown) {
                const [payload, given] = yield* Effect.all([Schema.decodeUnknownEffect(Schema.toCodecJson(definition.parametersSchema))(args, { errors: 'all' }), Schema.decodeUnknownEffect(Schema.toCodecJson(Schema.Struct(route.fields)))(args, { errors: 'all' })], { mode: 'result' });
                return yield* Result.isSuccess(payload) && Result.isSuccess(given) ? Effect.succeed([payload.success, given.success] as const) : Effect.fail(McpSchema.InvalidParams.make({ message: [payload, given].flatMap((decoded) => (Result.isFailure(decoded) ? [decoded.failure.message] : [])).join('\n') }));
            }),
            Effect.fnUntraced(function* ([payload, given]: readonly [P['Type'], Schema.Struct.Type<F>]) {
                const entry = yield* Option.match(route.select(given), { onNone: () => Effect.fail(unlinked), onSome: reach });
                return yield* handle(payload, call(entry));
            }),
            Schema.encodeUnknownEffect(Schema.toCodecJson(definition.successSchema)),
        );
    const read = Effect.fnUntraced(function* (rpc: Erased, args: unknown) {
        const payload = yield* Schema.decodeUnknownEffect(Schema.toCodecJson(rpc.payloadSchema))(args, { errors: 'all' });
        return yield* send(yield* reach(Function.constTrue), rpc._tag, payload);
    });
    const encoded = (rpc: Erased) => flow(Schema.encodeUnknownEffect(Schema.fromJsonString(Schema.toCodecJson(rpc.successSchema))), Effect.orDie);
    const completion = (rpc: Erased) =>
        Record.map(
            rpc.payloadSchema.fields,
            (_, key) => (input: string, context: { readonly arguments?: Readonly<Record<string, string>> } | undefined) =>
                read(rpc, { ...context?.arguments, [key]: input }).pipe(
                    Effect.as([input]),
                    Effect.catchFilter(
                        Filter.fromPredicateOption((error: Schema.SchemaError | LinkError) => (Schema.is(LinkError)(error) && error.reason._tag === 'Refused' ? Option.liftPredicate(error.reason.cause, Schema.is(Unmatched)) : Option.none())),
                        ({ candidates }) => Effect.succeed([...candidates]),
                    ),
                ),
        );
    const publish = Effect.forEach(
        rpcs,
        (rpc) => {
            const description = SchemaAST.resolveDescription(rpc.payloadSchema.ast);
            const keys = Record.keys(rpc.payloadSchema.fields);
            return Match.value(Context.get(rpc.annotations, Surface)).pipe(
                Match.tagsExhaustive({
                    Tool: () => tool(Tool.make(rpc._tag, { description, parameters: rpc.payloadSchema, success: rpc.successSchema }).annotateMerge(rpc.annotations), (payload, use) => use(rpc, payload)),
                    Resource: ({ uri }) => {
                        const segments = [`${uri}/`, ...keys.slice(1).map(() => '/'), ''];
                        return Effect.all(
                            [
                                Record.every(rpc.payloadSchema.fields, (field) => SchemaAST.isOptional(SchemaAST.toEncoded(field.ast)))
                                    ? McpServer.registerResource({ uri, name: rpc._tag, description, mimeType: 'application/json', annotations: annotated(rpc.annotations), content: Effect.flatMap(read(rpc, {}), encoded(rpc)) })
                                    : Effect.void,
                                Array.isReadonlyArrayNonEmpty(keys)
                                    ? McpServer.registerResource(
                                          Object.assign([...segments], { raw: segments }),
                                          ...keys.map((key) => McpSchema.param(key, Schema.String)),
                                      )({
                                          name: rpc._tag,
                                          description,
                                          mimeType: 'application/json',
                                          annotations: annotated(rpc.annotations),
                                          completion: completion(rpc),
                                          content: (_uri, ...values) => Effect.flatMap(read(rpc, Record.fromEntries(Array.zip(keys, values))), encoded(rpc)),
                                      })
                                    : Effect.void,
                            ],
                            { discard: true },
                        );
                    },
                    Prompt: () =>
                        McpServer.registerPrompt({
                            name: rpc._tag,
                            description,
                            parameters: rpc.payloadSchema.fields,
                            completion: completion(rpc),
                            annotations: annotated(rpc.annotations),
                            content: (given) => Effect.flatMap(read(rpc, given), flow(Schema.decodeUnknownEffect(Schema.String), Effect.orDie)),
                        }),
                    None: () => Effect.void,
                }),
            );
        },
        { discard: true },
    );
    return { peers: Effect.map(SubscriptionRef.get(links), Array.map(Struct.get('peer'))), tool, publish };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const main = <E>(manifest: { readonly name: string; readonly version: string; readonly description: string }, layer: Layer.Layer<never, E, McpServer.McpServer | HttpRouter.HttpRouter | NodeServices.NodeServices>): void => {
    const name = manifest.name.slice(manifest.name.lastIndexOf('/') + 1);
    const settings = {
        listen: Flag.String('listen').pipe(Flag.withFallbackConfig(Config.String('MCP_LISTEN')), Flag.withDescription('Socket address the Streamable HTTP endpoint listens on, as host:port or [IPv6]:port')),
        hosts: Flag.String('allowed-host').pipe(Flag.atLeast(1), Flag.withFallbackConfig(Config.Array(Schema.String, 'MCP_ALLOWED_HOSTS')), Flag.withDefault([]), Flag.withDescription('Host authority answered beside loopback names and the bound address')),
        log: Flag.Literals('log', ['All', 'Fatal', 'Error', 'Warn', 'Info', 'Debug', 'Trace', 'None']).pipe(Flag.withFallbackConfig(Config.LogLevel('MCP_LOG')), Flag.withDefault('Info' as const), Flag.withDescription('Minimum log level')),
        otlp: Flag.Boolean('otlp').pipe(Flag.withFallbackConfig(Config.Boolean('MCP_OTLP')), Flag.withDefault(false), Flag.withDescription('Exports traces, metrics, and logs over OTLP/HTTP to the endpoints OTEL_EXPORTER_OTLP_* variables name')),
    };
    const [open, close] = ['async function body() {\n', '\n}'];
    Command.make(
        name,
        settings,
        Effect.fnUntraced(function* ({ listen: at, hosts, log, otlp }) {
            const address = yield* Effect.fromResult(NetAddress.inetAddressFromString(at));
            const allowed = new Set([...hosts, 'localhost', '127.0.0.1', '[::1]', ...(NetAddress.isUnspecified(address.address) ? [] : [NetAddress.formatUrlHost(address.address)])]);
            const admitted = HttpRouter.middleware((app) => Effect.flatMap(HttpServerRequest.HttpServerRequest, (request) => (allowed.has(request.headers.host ?? '') || allowed.has(URL.parse(`http://${request.headers.host}`)?.hostname ?? '') ? app : Effect.succeed(HttpServerResponse.empty({ status: 403 })))), {
                global: true,
            });
            const runtime = Layer.unwrap(
                Effect.map(McpServer.McpServer, (server) =>
                    McpServer.layerHttp({
                        ...manifest,
                        name,
                        get instructions() {
                            return Array.dedupe([manifest.description, ...[...server.tools, ...server.resources, ...server.resourceTemplates, ...server.prompts].flatMap(({ annotations }) => Context.get(annotations, Instructions))]).join('\n');
                        },
                        path: '/',
                        allowSessionTermination: true,
                        protocols: [McpProtocol.v2026_07_28, McpProtocol.v2025_11_25, McpProtocol.v2025_06_18, McpProtocol.v2025_03_26, McpProtocol.v2024_11_05],
                    }),
                ),
            ).pipe(Layer.provideMerge(McpServer.McpServer.layer));
            const logged = Logger.layer([Logger.consoleJson]);
            return yield* HttpRouter.serve(Layer.mergeAll(Layer.provide(layer, Layer.effect(McpServer.McpServer, decorated(name))).pipe(Layer.provide(runtime)), admitted), { disableLogger: true }).pipe(
                Layer.provide(NodeHttpServer.layerServer(createServer, { host: NetAddress.formatHost(address), port: address.port, disablePreemptiveShutdown: true })),
                Layer.launch,
                Effect.provide(otlp ? Otlp.layerFromConfig({ resource: { serviceName: name, serviceVersion: manifest.version } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(logged)) : logged),
                Effect.provideService(References.MinimumLogLevel, log),
                Effect.provideService(Transpile, (code) => Effect.try({ try: () => stripTypeScriptTypes(`${open}${code}${close}`, { mode: 'strip' }).slice(open.length, -close.length), catch: (cause) => new SchemaIssue.InvalidValue({ message: Formatter.format(cause) }, code) })),
            );
        }),
    ).pipe(Command.run({ version: manifest.version }), Effect.provide(NodeServices.layer), NodeRuntime.runMain({ teardown: (exit, onExit) => (Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause) ? onExit(0) : Runtime.defaultTeardown(exit, onExit)) }));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { listen, main, register };
