import { ranked } from '@rasm/search';
import { Array, Cause, Context, Data, Deferred, Effect, Equal, Exit, Formatter, Function, flow, HashMap, Layer, Match, Metric, Option, Predicate, pipe, Record, Ref, Schema, SchemaAST, SchemaGetter, Sink, Stream, Struct, SubscriptionRef, Tuple } from 'effect';
import { McpSchema, McpServer, Tool, Toolkit } from 'effect/ai';
import { Headers, HttpTraceContext, UrlParams } from 'effect/http';
import type { Rpc, RpcGroup } from 'effect/rpc';

// --- [TYPES] ---------------------------------------------------------------------------

interface Acts<A> {
    readonly refuse?: Predicate.Predicate<A> | undefined;
    readonly report?: Predicate.Predicate<A> | undefined;
}
interface Dependency {
    readonly refusal: Stream.Stream<Option.Option<readonly [string, Schema.Json]>, Error>;
    readonly report: Effect.Effect<Option.Option<readonly [string, Schema.Json]>, Error>;
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
                    return Blocks.use((add) => Effect.as(add(added), shown));
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
const Respond = Context.Reference<(refusal: Error) => Effect.Effect<void>>('@rasm/mcp/server/Respond', { defaultValue: () => () => Effect.void });

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
const held = Effect.fnUntraced(function* <A, I, R>(schema: Schema.Codec<A, I>, mimeType: string | undefined, changes: Stream.Stream<unknown, never, R>) {
    const [server, store] = yield* Effect.all([McpServer.McpServer, SubscriptionRef.make(HashMap.empty<string, readonly [Effect.Effect<A, Error>, A]>())]);
    const [encoded, written] = [Schema.encodeEffect(Schema.toCodecJson(schema)), Schema.encodeEffect(Schema.fromJsonString(Schema.Json))];
    const observe = (uri: string, read: Effect.Effect<A, Error>) =>
        read.pipe(
            Effect.tap((value) => SubscriptionRef.update(store, HashMap.set(uri, [read, value] as const))),
            Effect.tapError((error) => (Cause.isNoSuchElementError(error) ? SubscriptionRef.update(store, HashMap.remove(uri)) : Effect.void)),
        );
    const shown = (at: string, holds: Predicate.Predicate<A>) =>
        flow(
            Option.filter(holds),
            Option.map((value: A) => Effect.map(encoded(value), (json) => [at, json] as const)),
            Effect.transposeOption,
        );
    yield* changes.pipe(
        Stream.mapEffect(() => SubscriptionRef.get(store)),
        Stream.flattenIterable,
        Stream.mapEffect(([uri, [read]]) => observe(uri, read).pipe(Effect.catchNoSuchElement, Effect.catchCause(Effect.logError)), { concurrency: 'unbounded' }),
        Stream.runDrain,
        Effect.forkScoped,
    );
    yield* Effect.forkScoped(
        SubscriptionRef.changes(store).pipe(
            Stream.map(HashMap.map(Tuple.get(1))),
            Stream.zipWithPrevious,
            Stream.map(([previous, current]) => Option.match(previous, { onNone: Array.empty<string>, onSome: (last) => Array.filter(HashMap.keys(last), (uri) => !Equal.equals(HashMap.get(last, uri), HashMap.get(current, uri))) })),
            Stream.flattenIterable,
            Stream.runForEach((uri) => server.notifications['notifications/resources/updated']({ uri })),
        ),
    );
    return {
        read: (uri: string, content: Effect.Effect<A, Error>) =>
            rendered(
                observe(uri, content).pipe(
                    Effect.flatMap(encoded),
                    Effect.filterOrElse(Predicate.isString, written),
                    Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri, mimeType, text })] })),
                ),
            ),
        depend:
            (uri: Effect.Effect<string, Error>, read: (uri: string) => Effect.Effect<A, Error>) =>
            ({ refuse, report }: Acts<A>): Dependency => {
                const current = (at: string) => Effect.catchNoSuchElement(observe(at, read(at)));
                return {
                    refusal: Option.match(Option.fromUndefinedOr(refuse), {
                        onNone: () => Stream.empty,
                        onSome: (holds) => Stream.unwrap(Effect.flatMap(uri, (at) => Effect.as(current(at), Stream.mapEffect(SubscriptionRef.changes(store), flow(HashMap.get(at), Option.map(Tuple.get(1)), shown(at, holds)))))),
                    }),
                    report: Option.match(Option.fromUndefinedOr(report), { onNone: () => Effect.succeedNone, onSome: (holds) => Effect.flatMap(uri, (at) => Effect.flatMap(current(at), shown(at, holds))) }),
                };
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
        const stated = (entries: readonly (readonly [string, Schema.Json])[]) => Effect.orDie(Schema.encodeEffect(Schema.fromJsonString(Schema.Record(Schema.String, Schema.Json)))(Record.fromEntries(entries)));
        const refused = flow(
            stated,
            Effect.map((shown) => new Cause.IllegalArgumentError(shown)),
        );
        const reported =
            (dependencies: readonly Dependency[]) =>
            <A, E, R2>(self: Effect.Effect<A, E, R2>) =>
                Effect.onExitIf(
                    self,
                    (exit) => !(Exit.isFailure(exit) && Cause.hasInterruptsOnly(exit.cause)),
                    () =>
                        Effect.gen(function* () {
                            const [[holding, failed], add] = yield* Effect.all([Effect.partition(dependencies, (dependency) => rendered(dependency.report), { concurrency: 'unbounded' }), Blocks]);
                            const shown = yield* pipe(holding, Array.getSomes, Option.liftPredicate(Array.isReadonlyArrayNonEmpty), Option.map(stated), Effect.transposeOption);
                            yield* Effect.forEach([...Option.toArray(shown), ...Array.map(failed, Struct.get('message'))], (text) => add(McpSchema.TextContent.make({ text })), { discard: true });
                        }),
                );
        const guarded =
            (dependencies: readonly Dependency[]) =>
            <A, E, R2>(run: Effect.Effect<A, E, R2>) =>
                Effect.scoped(
                    Effect.gen(function* () {
                        const peeled = yield* Effect.forEach(dependencies, (dependency) => Stream.peel(dependency.refusal, Sink.head()));
                        yield* Array.match(Array.getSomes(Array.map(peeled, ([first]) => Option.flatten(first))), { onEmpty: () => Effect.void, onNonEmpty: flow(refused, Effect.flatMap(Effect.fail)) });
                        yield* Stream.mergeAll(Array.map(peeled, Tuple.get(1)), { concurrency: 'unbounded' }).pipe(
                            Stream.filter(Option.isSome),
                            Stream.runHead,
                            Effect.flatMap(Option.match({ onNone: () => Effect.void, onSome: ({ value }) => Effect.flatMap(Effect.all([refused([value]), Respond]), ([refusal, respond]) => respond(refusal)) })),
                            Effect.catchCause(Effect.logError),
                            Effect.forkScoped,
                        );
                        return yield* reported(dependencies)(run);
                    }),
                );
        return register(
            tool.setParameters(Record.isEmptyRecord(fields) ? Tool.EmptyParams : Schema.toEncoded(Schema.Struct(fields))).annotateMerge(annotations),
            Effect.fnUntraced(function* (given) {
                const [parameters, routed] = yield* Schema.decodeEffect(parts)([given, given], { errors: 'all' });
                const chosen = yield* route.resolve(routed);
                const dependencies = yield* route.depends(chosen, routed);
                const run = handle(parameters, chosen);
                return yield* Context.get(annotations, Tool.Readonly) ? reported(dependencies)(run) : guarded(dependencies)(run);
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
    const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), held(options.schema, options.mimeType, options.changes)]);
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
            const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), held(options.schema, options.mimeType, options.changes)]);
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
    const flat = Schema.Record(Schema.String, Schema.String);
    const answered = (completed: (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.Effect<readonly string[], Error, R | Schema.Struct<Fields>['DecodingServices']>) => (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
        completed(input, context).pipe(
            Effect.map((values) => McpSchema.CompleteResult.make({ completion: { values: [...values] } })),
            rendered,
            Effect.provideContext(services),
        );
    const given = flat.pipe(Schema.decodeTo(Schema.toCodecStringTree(Schema.Struct(route.fields))));
    const resourced = Effect.fnUntraced(function* (rpc: Rpcs, { uri, complete, ...acts }: PublishedResource<Rpc.Success<Rpcs>, string>) {
        const defined = { name: rpc._tag, mimeType: 'application/json', ...Option.match(Option.fromUndefinedOr(SchemaAST.resolveDescription(rpc.payloadSchema.ast)), { onNone: () => ({}), onSome: (description) => ({ description }) }) };
        const { read, depend } = yield* held(rpc.successSchema, defined.mimeType, Stream.empty);
        const fields = { ...route.fields, ...rpc.payloadSchema.fields };
        const call = Schema.Struct({ route: given, payload: flat.pipe(Schema.decodeTo(Schema.toCodecStringTree(rpc.payloadSchema))) });
        const decoded = (values: unknown) => Schema.decodeUnknownEffect(call)({ route: values, payload: values }, { errors: 'all' });
        const called = ({ route: chosen, payload }: typeof call.Type) => Effect.flatMap(route.resolve(chosen), (found) => invoke(rpc)(payload, found)).pipe(Effect.provideContext(services));
        const answer = (at: string) =>
            Effect.flatMap(Schema.decodeEffect(Schema.URLFromString)(at), ({ searchParams }) => decoded(UrlParams.toReadonlyRecord(UrlParams.fromInput(searchParams)))).pipe(
                Effect.mapError(({ message }) => McpSchema.InvalidParams.make({ message })),
                Effect.flatMap((parsed) => read(at, called(parsed))),
                Effect.provideContext(services),
            );
        const resolved = (_: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) => Effect.flatMap(decoded(context?.arguments ?? {}), called);
        const completions = {
            ...Record.getSomes(Record.map(route.completion, flow(Option.fromUndefinedOr, Option.map(answered)))),
            ...Record.getSomes(
                Record.map(
                    complete,
                    flow(
                        Option.fromUndefinedOr,
                        Option.map((select) => answered(flow(resolved, Effect.map(select)))),
                    ),
                ),
            ),
        };
        yield* Schema.is(Schema.toEncoded(Schema.Struct(fields)))({}) ? server.addResource({ resource: McpSchema.Resource.make({ ...defined, uri }), annotations, handle: Effect.catchTag(answer(uri), 'InvalidParams', Effect.die) }) : Effect.void;
        yield* Array.match(Record.keys(fields), {
            onEmpty: () => Effect.void,
            onNonEmpty: (keys) => server.addResourceTemplate({ template: McpSchema.ResourceTemplate.make({ ...defined, uriTemplate: `${uri}{?${keys.join(',')}}` }), routerPath: uri.replace(':', '::'), annotations, completions, handle: answer }),
        });
        return (routed: Schema.Struct.Type<Fields>) => {
            const named = Effect.provideContext(Schema.encodeEffect(given)(routed), services);
            return depend(
                Effect.map(named, (values) => (Record.isEmptyRecord(values) ? uri : `${uri}?${UrlParams.toString(UrlParams.fromInput(values))}`)),
                () => named.pipe(Effect.flatMap(decoded), Effect.flatMap(called), Effect.provideContext(services)),
            )(acts);
        };
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
                        Tool.make(rpc._tag, { description: SchemaAST.resolveDescription(rpc.payloadSchema.ast), parameters: rpc.payloadSchema, success: success ?? rpc.successSchema }).annotateMerge(Context.merge(annotations, own)),
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
            const listed = {
                uri: `${scheme}://executions`,
                name: 'executions',
                description:
                    'Calls that change state answered before their outcome, cancelled or refused while running, each with its arguments and completed outcome, including failures and requested input. A cancelled call, or a refusal linking here, can have run, so read its outcome here before repeating it. Reads drop a completed call after listing it',
                mimeType: 'application/json',
            };
            const encode = Schema.encodeEffect(Schema.fromJsonString(Schema.toCodecJson(Schema.Array(Row))));
            const executions = yield* Effect.cached(
                McpServer.registerResource({
                    ...listed,
                    content: Effect.gen(function* () {
                        const rows = yield* Ref.get(kept);
                        const read = yield* Effect.all(rows);
                        yield* Ref.update(kept, Array.difference(Array.getSomes(Array.zipWith(rows, read, (row, { result }) => Option.as(result, row)))));
                        return { contents: [{ uri: listed.uri, mimeType: listed.mimeType, text: yield* Effect.orDie(encode(read)) }] };
                    }),
                }).pipe(Effect.provideService(McpServer.McpServer, server)),
            );
            const keep = Effect.fnUntraced(function* <E extends McpSchema.McpError, R>(tool: string, given: Schema.Json, answer: Effect.Effect<McpSchema.CallToolResult | McpSchema.InputRequired, E, R>, early: Deferred.Deferred<McpSchema.CallToolResult>) {
                const done = yield* Deferred.make<McpSchema.CallToolResult | McpSchema.InputRequired, E>();
                const recorded = Ref.update(
                    kept,
                    Array.append(
                        Deferred.poll(done).pipe(
                            Effect.map(Option.map(Effect.exit)),
                            Effect.flatMap(Effect.transposeOption),
                            Effect.map((result) => ({ tool, arguments: given, result })),
                        ),
                    ),
                );
                yield* Effect.forkIn(Deferred.complete(done, answer), scope, { uninterruptible: true });
                const [outcome, pending] = yield* Effect.raceFirst(
                    Effect.map(Deferred.await(done), (value) => [value, false] as const),
                    Effect.map(Deferred.await(early), (value) => [value, true] as const),
                ).pipe(
                    Effect.interruptible,
                    Effect.onInterrupt(() => recorded),
                );
                return yield* Effect.as(pending ? recorded : Effect.void, outcome);
            }, Effect.uninterruptible);
            const ranking =
                <C, R>(complete: (input: string, context: C) => Effect.Effect<McpSchema.CompleteResult, McpSchema.InternalError, R>) =>
                (input: string, context: C) =>
                    Effect.map(complete(input, context), ({ completion: { values }, ...rest }) => {
                        const ordered = ranked(input, values);
                        return McpSchema.CompleteResult.make({ ...rest, completion: { values: ordered, total: ordered.length } });
                    });
            const writes = ({ tool }: Parameters<typeof server.addTool>[0]) => tool.annotations?.readOnlyHint !== true;
            const refused = ({ message }: McpSchema.InternalError) => McpSchema.CallToolResult.make({ isError: true, content: [McpSchema.TextContent.make({ text: message }), McpSchema.ResourceLink.make(listed)] });
            const served = (options: Parameters<typeof server.addTool>[0]) =>
                Effect.fnUntraced(
                    function* (payload: Schema.Json) {
                        const [blocks, early] = yield* Effect.all([Ref.make(Array.empty<McpSchema.ContentBlock>()), Deferred.make<McpSchema.CallToolResult>()]);
                        const answer = Effect.gen(function* () {
                            const result = yield* options.handle(payload);
                            const added = yield* Ref.get(blocks);
                            return result instanceof McpSchema.InputRequired ? result : McpSchema.CallToolResult.make({ ...result, content: [...result.content, ...added] });
                        }).pipe(
                            Effect.provideService(Blocks, (block) => Ref.update(blocks, Array.append(block))),
                            Effect.provideService(Respond, (refusal) => Effect.asVoid(Deferred.complete(early, Effect.map(Effect.flip(rendered(Effect.fail(refusal))), refused)))),
                        );
                        return yield* writes(options) ? keep(options.tool.name, payload, answer, early) : answer;
                    },
                    observed('tools/call', options.tool.name, { 'gen_ai.tool.name': options.tool.name, 'gen_ai.operation.name': 'execute_tool' }),
                );
            return McpServer.McpServer.of({
                ...server,
                addTool: (options) => Effect.andThen(writes(options) ? executions : Effect.void, server.addTool({ ...options, handle: served(options) })),
                addResource: (options) => server.addResource({ ...options, handle: options.handle.pipe(observed('resources/read', options.resource.uri, { 'mcp.resource.uri': options.resource.uri })) }),
                addResourceTemplate: (options) => server.addResourceTemplate({ ...options, completions: Record.map(options.completions, ranking), handle: (at, params) => options.handle(at, params).pipe(observed('resources/read', options.template.uriTemplate, { 'mcp.resource.uri': at })) }),
                addPrompt: (options) => server.addPrompt({ ...options, completions: Record.map(options.completions, ranking), handle: (params) => options.handle(params).pipe(observed('prompts/get', options.prompt.name, { 'gen_ai.prompt.name': options.prompt.name })) }),
            });
        }),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Attachment, Content, Image, Instructions, layer, PublishedResource, PublishedTool, publish, READING, register, resource, resourceTemplate, targeted };
