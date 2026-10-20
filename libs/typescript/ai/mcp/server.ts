import { ranked, Unmatched } from '@rasm/search';
import { Array, Cause, Context, Deferred, Duration, Effect, Exit, Filter, Formatter, Function, Layer, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaGetter, Stream, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Headers, HttpTraceContext } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

interface Route<Chosen, Fields extends Schema.Struct.Fields, R> {
    readonly fields: Fields;
    readonly resolve: (given: Schema.Struct.Type<Fields>) => Effect.Effect<Chosen, Error, R>;
    readonly trailer: (chosen: Chosen, value: unknown) => Effect.Effect<Option.Option<McpSchema.ContentBlock>, Error, R>;
    readonly annotations: Context.Context<never>;
    readonly completion: { readonly [Key in keyof Fields]?: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R> };
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const READING = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.Destructive, false));

// --- [MODELS] --------------------------------------------------------------------------

const Content = <S extends Schema.Top>(schema: S, description: string, block: (value: S['Type']) => readonly [McpSchema.ContentBlock, string]) =>
    schema.annotate({
        toCodecJson: () =>
            Schema.link<S['Type']>()(Schema.String.annotate({ description }), {
                decode: SchemaGetter.forbidden(() => 'Content blocks hold the value'),
                encode: SchemaGetter.transformEffect((value: S['Type']) => {
                    const [added, shown] = block(value);
                    return Effect.flatMap(Blocks, (add) => Effect.as(add(added), shown));
                }),
            }),
    });
const Image = (mimeType: string) => Content(Schema.Uint8Array, 'Media type of the image content block beside the result', (data) => [McpSchema.ImageContent.make({ data, mimeType }), mimeType]);
const Attachment = Content(Schema.Struct({ uri: Schema.String, mimeType: Schema.OptionFromOptionalKey(Schema.String), data: Schema.Uint8Array }), 'URI of the file in the content block beside the result', ({ uri, mimeType, data }) => [
    Option.match(
        Option.filter(mimeType, (type) => type.startsWith('image/')),
        {
            onSome: (image) => McpSchema.ImageContent.make({ data, mimeType: image }),
            onNone: () => McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri, blob: data, mimeType: Option.getOrUndefined(mimeType) }) }),
        },
    ),
    uri,
]);

// --- [SERVICES] ------------------------------------------------------------------------

const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
const Progress = Context.Reference<(message: string) => Effect.Effect<void>>('@rasm/mcp/server/Progress', { defaultValue: () => () => Effect.void });
const Blocks = Context.Reference<(block: McpSchema.ContentBlock) => Effect.Effect<void>>('@rasm/mcp/server/Blocks', { defaultValue: () => () => Effect.void });

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = (cause: Cause.Cause<unknown>) => {
    const suggestions = 100;
    return Cause.prettyErrors(Cause.map(cause, (error) => (Schema.is(Unmatched)(error) ? Unmatched.make({ subject: error.subject, query: error.query, candidates: Option.match(error.query, { onNone: () => error.candidates, onSome: (wanted) => ranked(wanted, error.candidates) }).slice(0, suggestions) }) : error)))
        .map((error) => Formatter.format(error))
        .join('\n');
};
const observed =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.flatMap(McpSchema.McpRequestContext, ({ requestMetadata }) =>
            Effect.gen(function* () {
                const [duration, exit] = yield* Effect.timed(Effect.exit(self));
                const recorded = {
                    'mcp.method.name': method,
                    ...attributes,
                    ...Exit.match(exit, { onSuccess: (value) => (Predicate.hasProperty(value, 'isError') && value.isError === true ? { 'error.type': 'tool_error' } : {}), onFailure: (cause) => ({ 'error.type': Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error' }) }),
                };
                yield* Effect.annotateCurrentSpan(recorded);
                yield* Metric.update(Metric.withAttributes(Metric.timer('mcp.server.operation.duration', { description: 'Duration of MCP server operations' }), recorded), duration);
                return yield* exit;
            }).pipe(Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) })),
        );
const register = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) {
    const context = yield* Effect.context<R>();
    const called = (given: P['Type']) =>
        Effect.provide(handle(given), context).pipe(
            Effect.catchCauseFilter(
                Filter.fromPredicate((cause) => !Cause.hasInterruptsOnly(cause)),
                (cause) => Effect.fail(new Cause.UnknownError(cause, rendered(cause))),
            ),
        );
    const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown, P['DecodingServices'], P['EncodingServices']>, Schema.Codec<unknown, unknown, S['DecodingServices'], S['EncodingServices']>] = [
        Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform(called), encode: SchemaGetter.forbidden(() => 'calls decode only') })),
        Schema.toCodecJson(tool.successSchema),
    ];
    const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.ErrorInstance() }).annotateMerge(tool.annotations));
    yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
});
const targeted =
    <Chosen, Fields extends Schema.Struct.Fields, RR>(route: Route<Chosen, Fields, RR>) =>
    <P extends Schema.Top & { readonly fields: Schema.Struct.Fields }, S extends Schema.Top, R>(
        tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
        handle: (parameters: P['Type'], chosen: Chosen) => Effect.Effect<S['Type'], Error, R>,
    ) => {
        const parts = Schema.toCodecJson(Schema.Tuple([tool.parametersSchema, Schema.Struct(route.fields)]));
        return register(
            tool.setParameters(Schema.toEncoded(Schema.Struct({ ...tool.parametersSchema.fields, ...route.fields }))).annotateMerge(route.annotations),
            Effect.fnUntraced(function* (given) {
                const [parameters, routed] = yield* Schema.decodeEffect(parts)([given, given], { errors: 'all' });
                const chosen = yield* route.resolve(routed);
                const value = yield* handle(parameters, chosen);
                const [block, add] = yield* Effect.all([route.trailer(chosen, value), Blocks]);
                yield* Option.match(block, { onNone: () => Effect.void, onSome: add });
                return value;
            }),
        );
    };

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = (scheme: string) =>
    Layer.effect(McpServer.McpServer)(
        Effect.gen(function* () {
            const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) });
            const [server, scope, held] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<typeof Row.Type>>())]);
            const uri = `${scheme}://executions`;
            const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
            const executions = yield* Effect.cached(
                McpServer.registerResource({
                    uri,
                    name: 'executions',
                    description: 'Calls that change state, cancelled before their response, each with its arguments and completed outcome, including failures and requested input. A call left unanswered can have run, so read its outcome here before repeating it. Reads drop a completed call after listing it',
                    mimeType: 'application/json',
                    content: Effect.gen(function* () {
                        const rows = yield* Ref.get(held);
                        const read = yield* Effect.all(rows);
                        yield* Ref.update(held, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                        return { contents: [{ uri, mimeType: 'application/json', text: yield* Effect.orDie(encode(read)) }] };
                    }),
                }).pipe(Effect.provideService(McpServer.McpServer, server)),
            );
            const kept = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>) {
                const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
                const row = Deferred.poll(done).pipe(
                    Effect.map(Option.map(Effect.exit)),
                    Effect.flatMap(Effect.transposeOption),
                    Effect.map((result) => ({ tool, arguments: given, result })),
                );
                yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
                return yield* Effect.interruptible(Deferred.await(done)).pipe(Effect.onInterrupt(() => Effect.andThen(Ref.update(held, Array.append(row)), Effect.forkIn(Effect.andThen(Effect.exit(Deferred.await(done)), server.notifications['notifications/resources/updated']({ uri })), scope))));
            }, Effect.uninterruptible);
            const ranking =
                <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
                (input: string, context: C) =>
                    Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                        const ordered = ranked(input, values);
                        return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                    });
            const destructive = ({ tool }: Parameters<typeof server.addTool>[0]) => tool.annotations?.readOnlyHint !== true && tool.annotations?.destructiveHint !== false;
            const served = (options: Parameters<typeof server.addTool>[0]) =>
                Effect.fnUntraced(
                    function* (payload: Schema.Json) {
                        const [blocks, stage, { requestMetadata }] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), SubscriptionRef.make(options.tool.name), McpSchema.McpRequestContext]);
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
                            const result = yield* options.handle(payload);
                            const added = yield* Ref.get(blocks);
                            return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                        }).pipe(
                            Effect.provideService(Blocks, (block) => Ref.update(blocks, Array.append(block))),
                            Effect.provideService(Progress, (message) => SubscriptionRef.set(stage, message)),
                        );
                        return yield* destructive(options) ? kept(options.tool.name, payload, answer) : answer;
                    },
                    observed('tools/call', options.tool.name, { 'gen_ai.tool.name': options.tool.name, 'gen_ai.operation.name': 'execute_tool' }),
                );
            return McpServer.McpServer.of({
                ...server,
                addTool: (options) => Effect.andThen(destructive(options) ? executions : Effect.void, server.addTool({ ...options, handle: served(options) })),
                addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(observed('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
                addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(observed('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
                addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(observed('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
            });
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attachment, Image, Instructions, layer, Progress, READING, type Route, register, rendered, targeted };
