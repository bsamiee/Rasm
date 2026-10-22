import { ranked } from '@rasm/search';
import { Array, Cause, Context, Data, Deferred, Effect, Equal, Exit, Formatter, Function, flow, HashMap, Layer, Match, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaAST, SchemaGetter, Stream, Struct } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Headers, HttpTraceContext, UrlParams } from 'effect/http';
import type { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

interface Acts<A> {
    readonly refuse?: Predicate.Predicate<A> | undefined;
    readonly report?: Predicate.Predicate<A> | undefined;
}
interface Dependency {
    readonly refused: Effect.Effect<Option.Option<readonly [string, Schema.Json]>, Error>;
    readonly reported: Effect.Effect<Option.Option<readonly [string, Schema.Json]>, Error>;
}
interface Route<Chosen, Fields extends Schema.Struct.Fields, R> {
    readonly fields: Fields;
    readonly resolve: (given: Schema.Struct.Type<Fields>) => Effect.Effect<Chosen, Error, R>;
    readonly depends: (chosen: Chosen, given: Schema.Struct.Type<Fields>) => Effect.Effect<readonly Dependency[], Error, R>;
    readonly annotations: Context.Context<never>;
    readonly completion: { readonly [Key in keyof Fields]?: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R> };
}
type Params<Schemas extends readonly Schema.Constraint[]> = { readonly [K in keyof Schemas]: Schemas[K]['Type'] };
type Published<A, Key extends string> = PublishedTool<A> | PublishedResource<A, Key>;

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
class PublishedTool<A> extends Data.TaggedClass('Tool')<{ readonly annotations: Context.Context<never>; readonly success?: Schema.Codec<A, unknown> | undefined }> {}
class PublishedResource<A, Key extends string> extends Data.TaggedClass('Resource')<{ readonly uri: string; readonly complete: { readonly [Field in Key]?: (success: A) => readonly string[] } } & Acts<A>> {}

// --- [SERVICES] ------------------------------------------------------------------------

const Instructions = Context.Reference<readonly string[]>('@rasm/mcp/server/Instructions', { defaultValue: () => [] });
const Blocks = Context.Reference<(block: McpSchema.ContentBlock) => Effect.Effect<void>>('@rasm/mcp/server/Blocks', { defaultValue: () => () => Effect.void });

// --- [OPERATIONS] ----------------------------------------------------------------------

const rendered = Effect.catchCause((cause: Cause.Cause<unknown>) =>
    Cause.hasInterruptsOnly(cause)
        ? Effect.interrupt
        : Effect.fail(
              McpSchema.InternalError.make({
                  message: Cause.prettyErrors(cause)
                      .map((error) => Formatter.format(error))
                      .join('\n'),
              }),
          ),
);
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
const held = Effect.fnUntraced(function* <A, I, R>(schema: Schema.Codec<A, I>, changes: Stream.Stream<unknown, never, R>) {
    const [server, store] = yield* Effect.all([McpServer.McpServer, Ref.make(HashMap.empty<string, readonly [Effect.Effect<A, Error>, A]>())]);
    const [encoded, written] = [Schema.encodeEffect(Schema.toCodecJson(schema)), Schema.encodeEffect(Schema.fromJsonString(Schema.Json))];
    const observe = (uri: string, read: Effect.Effect<A, Error>) => {
        const record = (value: Option.Option<A>) =>
            Ref.modify(store, (entries) => [Option.exists(HashMap.get(entries, uri), ([, last]) => !Equal.equals(Option.some(last), value)), Option.match(value, { onNone: () => HashMap.remove(entries, uri), onSome: (now) => HashMap.set(entries, uri, [read, now] as const) })]).pipe(
                Effect.flatMap((changed) => (changed ? server.notifications['notifications/resources/updated']({ uri }) : Effect.void)),
            );
        return read.pipe(
            Effect.tap(flow(Option.some, record)),
            Effect.tapError((error) => (Cause.isNoSuchElementError(error) ? record(Option.none()) : Effect.void)),
        );
    };
    yield* changes.pipe(
        Stream.mapEffect(() => Ref.get(store)),
        Stream.flattenIterable,
        Stream.mapEffect(([uri, [read]]) => observe(uri, read).pipe(Effect.catchNoSuchElement, Effect.catchCause(Effect.logError)), { concurrency: 'unbounded' }),
        Stream.runDrain,
        Effect.forkScoped,
    );
    return {
        read: (uri: string, content: Effect.Effect<A, Error>) =>
            rendered(
                Effect.flatMap(
                    observe(uri, content),
                    flow(
                        encoded,
                        Effect.flatMap((json) => (Predicate.isString(json) ? Effect.succeed(json) : written(json))),
                    ),
                ),
            ),
        depend:
            (uri: Effect.Effect<string, Error>, read: (uri: string) => Effect.Effect<A, Error>) =>
            ({ refuse, report }: Acts<A>): Dependency => {
                const when = (act: Predicate.Predicate<A> | undefined) =>
                    Option.match(Option.fromUndefinedOr(act), {
                        onNone: () => Effect.succeedNone,
                        onSome: (holds) =>
                            Effect.gen(function* () {
                                const at = yield* uri;
                                const value = yield* Effect.catchNoSuchElement(observe(at, read(at)));
                                const json = yield* Effect.transposeOption(Option.map(Option.filter(value, holds), encoded));
                                return Option.map(json, (shown) => [at, shown] as const);
                            }),
                    });
                return { refused: when(refuse), reported: when(report) };
            },
    };
});
const register = Effect.fnUntraced(function* <P extends Schema.Top, S extends Schema.Top, R>(tool: Tool.Tool<string, { readonly parameters: P; readonly success: S; readonly failure: Schema.Top; readonly failureMode: Tool.FailureMode }>, handle: (parameters: P['Type']) => Effect.Effect<S['Type'], Error, R>) {
    const context = yield* Effect.context<R>();
    const called = (given: P['Type']) => rendered(Effect.provide(handle(given), context));
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
        const annotations = Context.merge(route.annotations, tool.annotations);
        const fields = { ...tool.parametersSchema.fields, ...route.fields };
        const text = Schema.encodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json)));
        return register(
            tool.setParameters(Record.isEmptyRecord(fields) ? Tool.EmptyParams : Schema.toEncoded(Schema.Struct(fields))).annotateMerge(annotations),
            Effect.fnUntraced(function* (given) {
                const [parameters, routed] = yield* Schema.decodeEffect(parts)([given, given], { errors: 'all' });
                const chosen = yield* route.resolve(routed);
                const dependencies = yield* route.depends(chosen, routed);
                const stated = (act: (dependency: Dependency) => Dependency['refused']) => Effect.forEach(dependencies, act, { concurrency: 'unbounded' }).pipe(Effect.flatMap(flow(Array.getSomes, Option.liftPredicate(Array.isReadonlyArrayNonEmpty), Option.map(flow(Record.fromEntries, text)), Effect.transposeOption)));
                yield* Effect.flatMap(Context.get(annotations, Tool.Readonly) ? Effect.succeedNone : stated(Struct.get('refused')), Option.match({ onNone: () => Effect.void, onSome: (refusal) => Effect.fail(new Cause.IllegalArgumentError(refusal)) }));
                return yield* Effect.onExitIf(
                    handle(parameters, chosen),
                    (exit) => !(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause)),
                    () =>
                        Effect.flatMap(
                            Effect.all([
                                stated(Struct.get('reported')).pipe(
                                    Effect.map(Option.map((reported) => McpSchema.TextContent.make({ text: reported }))),
                                    rendered,
                                    Effect.catch((error) => Effect.succeedSome(McpSchema.TextContent.make({ text: error.message }))),
                                ),
                                Blocks,
                            ]),
                            ([report, add]) => Option.match(report, { onNone: () => Effect.void, onSome: add }),
                        ),
                );
            }),
        );
    };
const resource = Effect.fnUntraced(function* <A, I, E extends Error, R>(options: {
    readonly uri: string;
    readonly name: string;
    readonly description?: string | undefined;
    readonly mimeType?: string | undefined;
    readonly annotations?: Context.Context<never> | undefined;
    readonly schema: Schema.Codec<A, I>;
    readonly content: Effect.Effect<A, E, R>;
    readonly changes: Stream.Stream<unknown, never, R>;
}) {
    const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), held(options.schema, options.changes)]);
    const content = Effect.provideContext(options.content, services);
    yield* McpServer.registerResource({ ...options, content: read(options.uri, content) });
    return depend(Effect.succeed(options.uri), Function.constant(content));
});
const resourceTemplate =
    <const Schemas extends readonly Schema.Codec<unknown, unknown, never, never>[]>(segments: TemplateStringsArray, ...schemas: Schemas) =>
    <A, I, E extends Error, R, const Completions extends Partial<McpServer.ResourceCompletions<Schemas>> = Record<never, never>>(options: {
        readonly name: string;
        readonly description?: string | undefined;
        readonly mimeType?: string | undefined;
        readonly annotations?: Context.Context<never> | undefined;
        readonly completion?: McpServer.ValidateCompletions<Completions, keyof McpServer.ResourceCompletions<Schemas>> | undefined;
        readonly schema: Schema.Codec<A, I>;
        readonly content: (uri: string, ...params: Params<Schemas>) => Effect.Effect<A, E, R>;
        readonly changes: Stream.Stream<unknown, never, R>;
    }) =>
        Effect.gen(function* () {
            const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), held(options.schema, options.changes)]);
            yield* McpServer.registerResource<Schemas>(segments, ...schemas)<McpSchema.InternalError, never, Completions>({ ...options, content: (uri, ...params) => read(uri, Effect.provideContext(options.content(uri, ...params), services)) });
            return (...params: Params<Schemas>) =>
                depend(
                    Effect.map(
                        Effect.forEach(Array.zip(schemas, params), ([schema, param]) => Schema.encodeUnknownEffect(Schema.String.pipe(Schema.decodeTo(Schema.toCodecStringTree(schema))))(param)),
                        (values) => Array.zipWith(segments, [...values, ''], (segment, value) => `${segment}${encodeURIComponent(value)}`).join(''),
                    ),
                    (uri) => Effect.provideContext(options.content(uri, ...params), services),
                );
        });
const publish = Effect.fnUntraced(function* <Rpcs extends Rpc.Any & { readonly payloadSchema: Schema.Codec<Rpc.Payload<Rpcs>, unknown> & { readonly fields: Schema.Struct.Fields }; readonly successSchema: Schema.Codec<Rpc.Success<Rpcs>, unknown> }, Chosen, Fields extends Schema.Struct.Fields, R>(
    group: RpcGroup.RpcGroup<Rpcs>,
    published: { readonly [Tag in Rpcs['_tag']]?: Published<Rpc.Success<Rpc.ExtractTag<Rpcs, Tag>>, keyof Rpc.ExtractTag<Rpcs, Tag>['payloadSchema']['fields'] & string> },
    {
        route,
        invoke,
        annotations,
    }: {
        readonly route: Route<Chosen, Fields, R>;
        readonly invoke: (rpc: Rpcs) => (payload: Rpc.Payload<Rpcs>, chosen: Chosen) => Effect.Effect<Rpc.Success<Rpcs>, Error, R>;
        readonly annotations: Context.Context<never>;
    },
) {
    const [server, services] = yield* Effect.all([McpServer.McpServer, Effect.context<R | Schema.Struct<Fields>['DecodingServices'] | Schema.Struct<Fields>['EncodingServices']>()]);
    const given = Schema.toCodecStringTree(Schema.Struct(route.fields));
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values: [...values] } })),
            rendered,
            Effect.provideContext(services),
        );
    const decoded = (rpc: Rpcs) => (values: unknown) => Schema.decodeUnknownEffect(Schema.Struct({ route: given, payload: Schema.toCodecStringTree(rpc.payloadSchema) }))({ route: values, payload: values }, { errors: 'all' });
    const called =
        (rpc: Rpcs) =>
        ({ route: chosen, payload }: { readonly route: Schema.Struct.Type<Fields>; readonly payload: Rpc.Payload<Rpcs> }) =>
            Effect.flatMap(route.resolve(chosen), (found) => invoke(rpc)(payload, found));
    const defined = (rpc: Rpcs) => ({ name: rpc._tag, description: SchemaAST.resolveDescription(rpc.payloadSchema.ast), mimeType: 'application/json' });
    const completions = (rpc: Rpcs, complete: PublishedResource<Rpc.Success<Rpcs>, string>['complete']) => ({
        ...Record.getSomes(Record.map(route.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
        ...Record.getSomes(
            Record.map(
                complete,
                flow(
                    Option.fromUndefinedOr,
                    Option.map((select) => answered((_, context) => decoded(rpc)(context?.arguments ?? {}).pipe(Effect.flatMap(called(rpc)), Effect.map(select)))),
                ),
            ),
        ),
    });
    const located = (uri: string, named: Readonly<Record<string, string>>) => (Record.isEmptyRecord(named) ? uri : `${uri}?${UrlParams.toString(UrlParams.fromInput(named))}`);
    const resourced = Effect.fnUntraced(function* (rpc: Rpcs, { uri, complete, refuse, report }: PublishedResource<Rpc.Success<Rpcs>, string>) {
        const { read, depend } = yield* held(rpc.successSchema, Stream.empty);
        const fields = { ...route.fields, ...rpc.payloadSchema.fields };
        const parsed = (at: string) => Effect.flatMap(Schema.decodeEffect(Schema.URLFromString)(at), ({ searchParams }) => decoded(rpc)(UrlParams.toReadonlyRecord(UrlParams.fromInput(searchParams))));
        const answer = (at: string) =>
            parsed(at).pipe(
                Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
                Effect.flatMap((call) => read(at, Effect.provideContext(called(rpc)(call), services))),
                Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri: at, mimeType: 'application/json', text })] })),
                Effect.provideContext(services),
            );
        yield* Schema.is(Schema.toEncoded(Schema.Struct(fields)))({}) ? server.addResource({ resource: McpSchema.Resource.make({ ...defined(rpc), uri }), annotations, handle: Effect.catchTag(answer(uri), 'InvalidParams', Effect.die) }) : Effect.void;
        yield* Array.match(Record.keys(fields), {
            onEmpty: () => Effect.void,
            onNonEmpty: (keys) => server.addResourceTemplate({ template: McpSchema.ResourceTemplate.make({ ...defined(rpc), uriTemplate: `${uri}{?${keys.join(',')}}` }), routerPath: uri.replace(':', '::'), annotations, completions: completions(rpc, complete), handle: answer }),
        });
        return (routed: Schema.Struct.Type<Fields>) =>
            depend(
                Effect.map(Schema.encodeEffect(Schema.Record(Schema.String, Schema.String).pipe(Schema.decodeTo(given)))(routed).pipe(Effect.provideContext(services)), (named) => located(uri, named)),
                (at) => Effect.flatMap(parsed(at), called(rpc)).pipe(Effect.provideContext(services)),
            )({ refuse, report });
    });
    const rows = Array.flatMap([...group.requests.values()], (rpc: Rpcs) => Option.toArray(Option.map(Option.flatMap(Record.get<string, Published<Rpc.Success<Rpcs>, string> | undefined>(published, rpc._tag), Option.fromUndefinedOr), (row) => [rpc, row] as const)));
    const linked = Array.getSomes(yield* Effect.forEach(rows, ([rpc, row]) => Match.valueTags(row, { Tool: () => Effect.succeedNone, Resource: (resourceRow) => Effect.asSome(resourced(rpc, resourceRow)) })));
    const depends = (routed: Schema.Struct.Type<Fields>) => Array.map(linked, (dependency) => dependency(routed));
    yield* Effect.forEach(
        rows,
        ([rpc, row]) =>
            Match.valueTags(row, {
                Tool: ({ annotations: own, success }) =>
                    targeted({ ...route, depends: (chosen, routed) => Effect.map(route.depends(chosen, routed), Array.appendAll(depends(routed))) })(
                        Tool.make(rpc._tag, { description: defined(rpc).description, parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(annotations, own)),
                        invoke(rpc),
                    ),
                Resource: () => Effect.void,
            }),
        { discard: true },
    );
    return depends;
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const layer = (scheme: string) =>
    Layer.effect(McpServer.McpServer)(
        Effect.gen(function* () {
            const Row = Schema.Struct({ tool: Schema.String, arguments: Schema.Json, result: Schema.OptionFromOptionalKey(Schema.Exit(Schema.Union([McpSchema.CallToolResult, Schema.instanceOf(McpSchema.InputRequired, { toCodecJson: () => undefined })]), McpSchema.McpError, Schema.Defect())) });
            const [server, scope, kept] = yield* Effect.all([McpServer.McpServer, Effect.scope, Ref.make(Array.empty<Effect.Effect<typeof Row.Type>>())]);
            const uri = `${scheme}://executions`;
            const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
            const executions = yield* Effect.cached(
                McpServer.registerResource({
                    uri,
                    name: 'executions',
                    description: 'Calls that change state, cancelled before their response, each with its arguments and completed outcome, including failures and requested input. A call left unanswered can have run, so read its outcome here before repeating it. Reads drop a completed call after listing it',
                    mimeType: 'application/json',
                    content: Effect.gen(function* () {
                        const rows = yield* Ref.get(kept);
                        const read = yield* Effect.all(rows);
                        yield* Ref.update(kept, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                        return { contents: [{ uri, mimeType: 'application/json', text: yield* Effect.orDie(encode(read)) }] };
                    }),
                }).pipe(Effect.provideService(McpServer.McpServer, server)),
            );
            const keep = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>) {
                const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
                const row = Deferred.poll(done).pipe(
                    Effect.map(Option.map(Effect.exit)),
                    Effect.flatMap(Effect.transposeOption),
                    Effect.map((result) => ({ tool, arguments: given, result })),
                );
                yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
                return yield* Effect.interruptible(Deferred.await(done)).pipe(Effect.onInterrupt(() => Ref.update(kept, Array.append(row))));
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
                        const blocks = yield* Ref.make(Array.empty<McpSchema.ContentBlock>());
                        const answer = Effect.gen(function* () {
                            const result = yield* options.handle(payload);
                            const added = yield* Ref.get(blocks);
                            return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                        }).pipe(Effect.provideService(Blocks, (block) => Ref.update(blocks, Array.append(block))));
                        return yield* destructive(options) ? keep(options.tool.name, payload, answer) : answer;
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

export { Attachment, Content, Image, Instructions, layer, PublishedResource, PublishedTool, publish, READING, register, resource, resourceTemplate, targeted };
