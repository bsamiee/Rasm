import { ranked, Unmatched } from '@rasm/search';
import { Array, Cause, Clock, Context, Data, Deferred, Duration, Effect, Exit, Function, Layer, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaGetter, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Headers, HttpTraceContext } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

type Source = Parameters<(typeof McpServer.McpServer.Service)['addTool']>[0];

// --- [CONSTANTS] -----------------------------------------------------------------------

const READING = Context.make(Tool.Readonly, true).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.Destructive, false), Context.add(Tool.OpenWorld, false));
const DURATION = Metric.timer('mcp.server.operation.duration', { description: 'Duration of MCP server operations' });
const SUGGESTIONS = 100;

// --- [MODELS] --------------------------------------------------------------------------

const Activity = Schema.Struct({ description: Schema.String.annotate({ description: 'Present-progressive sentence shown while the call runs' }) });
const Image = (mimeType: string) =>
    Schema.Uint8Array.annotate({
        toCodecJson: () =>
            Schema.link<Uint8Array>()(Schema.String.annotate({ description: 'Media type of the image content block the result carries' }), {
                decode: SchemaGetter.forbidden(() => 'images travel as content blocks'),
                encode: SchemaGetter.transformEffect((data: Uint8Array) => Effect.flatMap(Blocks, (add) => Effect.as(add(McpSchema.ImageContent.make({ data, mimeType })), mimeType))),
            }),
    });
class Attachment extends Data.Class<{ readonly uri: string; readonly mimeType: string; readonly data: Uint8Array }> {
    static readonly schema = Schema.instanceOf(Attachment, {
        toCodecJson: () =>
            Schema.link<Attachment>()(Schema.String.annotate({ description: 'URI of the file the content block beside the result carries' }), {
                decode: SchemaGetter.forbidden(() => 'files travel as content blocks'),
                encode: SchemaGetter.transformEffect(({ uri, mimeType, data }: Attachment) =>
                    Effect.flatMap(Blocks, (add) => Effect.as(add(mimeType.startsWith('image/') ? McpSchema.ImageContent.make({ data, mimeType }) : McpSchema.EmbeddedResource.make({ resource: McpSchema.BlobResourceContents.make({ uri, blob: data, mimeType }) })), uri)),
                ),
            }),
    });
}

// --- [SERVICES] ------------------------------------------------------------------------

const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
const Progress = Context.Reference<(message: string) => Effect.Effect<void>>('@rasm/mcp/server/Progress', { defaultValue: () => () => Effect.void });
const Blocks = Context.Reference<(block: McpSchema.ContentBlock) => Effect.Effect<void>>('@rasm/mcp/server/Blocks', { defaultValue: () => () => Effect.void });

// --- [OPERATIONS] ----------------------------------------------------------------------

const observed =
    (method: string, name: string, attributes: Readonly<Record<string, string>>) =>
    <A, E, R>(self: Effect.Effect<A, E, R>) =>
        Effect.gen(function* () {
            const [started, { requestMetadata }] = yield* Effect.all([Clock.currentTimeMillis, McpSchema.McpRequestContext]);
            return yield* self.pipe(
                Effect.onExit(
                    Effect.fnUntraced(function* (exit) {
                        const recorded = {
                            'mcp.method.name': method,
                            ...attributes,
                            ...Exit.match(exit, { onSuccess: (value) => (Predicate.hasProperty(value, 'isError') && value.isError === true ? { 'error.type': 'tool_error' } : {}), onFailure: (cause) => ({ 'error.type': Cause.hasInterruptsOnly(cause) ? 'cancelled' : 'error' }) }),
                        };
                        yield* Effect.annotateCurrentSpan(recorded);
                        yield* Metric.update(Metric.withAttributes(DURATION, recorded), Duration.millis((yield* Clock.currentTimeMillis) - started));
                    }),
                ),
                Effect.withSpan(`${method} ${name}`, { kind: 'server', attributes: { 'mcp.method.name': method, ...attributes }, parent: pipe(Record.filter({ ...requestMetadata }, Predicate.isString), Headers.fromInput, HttpTraceContext.fromHeaders, Option.getOrUndefined) }),
            );
        });
const merged = (sources: Array.NonEmptyReadonlyArray<Source>): Source => {
    const [newest] = sources;
    const union = ([only, ...rest]: Array.NonEmptyReadonlyArray<Schema.JsonObject>, kind: 'input' | 'output'): Schema.JsonObject => (rest.length === 0 ? only : { anyOf: [only, ...rest].map((schema, index) => ({ $id: `urn:mcp:${encodeURIComponent(newest.tool.name)}:${kind}:${index}`, ...schema })) });
    const hints = sources.map(({ tool }) => tool.annotations);
    return {
        tool: McpSchema.Tool.make({
            ...newest.tool,
            description: Option.getOrUndefined(Array.findFirst(sources.toReversed(), ({ tool }) => Option.fromUndefinedOr(tool.description))),
            inputSchema: {
                ...union(
                    Array.map(sources, ({ tool }) => tool.inputSchema),
                    'input',
                ),
                type: 'object',
            },
            outputSchema: pipe(
                sources,
                Array.map(({ tool }) => Option.fromUndefinedOr(tool.outputSchema)),
                Option.all,
                Option.map((schemas) => union(schemas, 'output')),
                Option.getOrUndefined,
            ),
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
        handle: (payload) => {
            const answer = (rejected: readonly McpSchema.InvalidParams[], [source, ...rest]: Array.NonEmptyReadonlyArray<Source>): ReturnType<Source['handle']> =>
                Effect.catchTag(source.handle(payload), 'InvalidParams', (rejection) =>
                    Array.match(rest, {
                        onEmpty: () =>
                            Array.match(rejected, {
                                onEmpty: () => Effect.fail(rejection),
                                onNonEmpty: (earlier) => Effect.succeed(McpSchema.CallToolResult.make({ isError: true, content: [...earlier, rejection].map(({ message }) => McpSchema.TextContent.make({ text: message })) })),
                            }),
                        onNonEmpty: (next) => answer([...rejected, rejection], next),
                    }),
                );
            return answer([], sources);
        },
    };
};
const register = Effect.fnUntraced(function* <P extends Schema.Codec<unknown, unknown>, S extends Schema.Codec<unknown, unknown>, R>(
    tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>,
    handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>,
) {
    const context = yield* Effect.context<R>();
    const called = (given: P['Type']) => Effect.provide(handle(given), context).pipe(Effect.catchIf(Schema.is(Unmatched), ({ subject, query, candidates }) => Effect.fail(Unmatched.make({ subject, query, candidates: ranked(query, candidates).slice(0, SUGGESTIONS) }))));
    const [parameters, success]: readonly [Schema.Codec<Effect.Effect<unknown, Error>, unknown>, Schema.Codec<unknown, unknown>] = [
        Schema.toCodecJson(tool.parametersSchema).pipe(Schema.decodeTo(Schema.Any, { decode: SchemaGetter.transform(called), encode: SchemaGetter.forbidden(() => 'calls decode only') })),
        Schema.toCodecJson(tool.successSchema),
    ];
    const toolkit = Toolkit.make(Tool.make(tool.name, { description: Tool.getDescription(tool), parameters, success, failure: Schema.instanceOf(Error) }).annotateMerge(tool.annotations));
    yield* McpServer.registerToolkit(toolkit).pipe(Effect.provide(toolkit.toLayer({ [tool.name]: Function.identity })));
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = (scheme: string) =>
    Layer.effect(McpServer.McpServer)(
        Effect.gen(function* () {
            const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) });
            const [server, scope, held, stacks] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<typeof Row.Type>>()), Ref.make(Record.empty<string, Array.NonEmptyReadonlyArray<Source>>())]);
            const uri = `${scheme}://executions`;
            const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
            const executions = yield* Effect.cached(
                McpServer.registerResource({
                    uri,
                    name: 'executions',
                    description: 'Calls that change state, cancelled before their response, each with its arguments and completed outcome, including failures and requested input. Reads drop a completed call after listing it',
                    mimeType: 'application/json',
                    content: Effect.gen(function* () {
                        const rows = yield* Ref.get(held);
                        const read = yield* Effect.all(rows);
                        yield* Ref.update(held, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                        return { contents: [{ uri, mimeType: 'application/json', text: yield* Effect.orDie(encode(read)) }] };
                    }),
                }).pipe(Effect.provideService(McpServer.McpServer, server)),
            );
            const kept = Effect.fnUntraced(function* (tool: string, given: Schema.Json, answer: ReturnType<Source['handle']>) {
                const done = yield* Deferred.make<Effect.Success<typeof answer>, Effect.Error<typeof answer>>();
                const row = Effect.map(Deferred.poll(done), (result) => ({ tool, arguments: given, result: Option.map(result, Effect.runSyncExit) }));
                yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
                return yield* Effect.interruptible(Deferred.await(done)).pipe(Effect.onInterrupt(() => Effect.andThen(Ref.update(held, Array.append(row)), Effect.forkIn(Effect.andThen(Effect.exit(Deferred.await(done)), server.notifications['notifications/resources/updated']({ uri })), scope))));
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
                            return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                        }).pipe(
                            Effect.provideService(Blocks, (block) => Ref.update(blocks, Array.append(block))),
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
                        const ordered = ranked(input, values);
                        return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered.slice(0, SUGGESTIONS), total: ordered.length, hasMore: ordered.length > SUGGESTIONS } });
                    });
            return McpServer.McpServer.of({
                ...server,
                addTool: Effect.fnUntraced(function* (options) {
                    const source = yield* Ref.modify(stacks, (current) => {
                        const stack = Array.prepend(Option.getOrElse(Record.get(current, options.tool.name), Array.empty), options);
                        return [merged(stack), Record.set(current, options.tool.name, stack)];
                    });
                    const destructive = source.tool.annotations?.readOnlyHint !== true && source.tool.annotations?.destructiveHint !== false;
                    yield* destructive ? executions : Effect.void;
                    yield* server.addTool({ ...source, handle: served(source, destructive) });
                }),
                addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(observed('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
                addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(observed('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
                addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(observed('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
            });
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Activity, Attachment, Blocks, Image, Instructions, layer, Progress, READING, register };
