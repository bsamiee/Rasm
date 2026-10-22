import { Array, Cause, type Context, Effect, Equal, Function, flow, HashMap, Option, Predicate, Schema, Stream, SubscriptionRef, Tuple } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import { orInternalError } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

interface DependencyOptions<A> {
    readonly refuse?: Predicate.Predicate<A> | undefined;
    readonly report?: Predicate.Predicate<A> | undefined;
}
interface Dependency {
    readonly refusal: Stream.Stream<Option.Option<readonly [string, Schema.Json]>, Error>;
    readonly report: Effect.Effect<Option.Option<readonly [string, Schema.Json]>, Error>;
}
type Params<Schemas extends readonly Schema.Constraint[]> = { readonly [K in keyof Schemas]: Schemas[K]['Type'] };

// --- [OPERATIONS] ----------------------------------------------------------------------

const resourceValues = Effect.fnUntraced(function* <A, I, R>(schema: Schema.Codec<A, I>, mimeType: string | undefined, changes: Stream.Stream<unknown, never, R>) {
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
            orInternalError(
                observe(uri, content).pipe(
                    Effect.flatMap(encoded),
                    Effect.filterOrElse(Predicate.isString, written),
                    Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri, mimeType, text })] })),
                ),
            ),
        depend:
            (uri: Effect.Effect<string, Error>, read: (uri: string) => Effect.Effect<A, Error>) =>
            ({ refuse, report }: DependencyOptions<A>): Dependency => {
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
const registerResource = Effect.fnUntraced(function* <A, I, E extends Error, R>(options: {
    readonly uri: string;
    readonly name: string;
    readonly description?: string | undefined;
    readonly mimeType?: string | undefined;
    readonly annotations?: Context.Context<never> | undefined;
    readonly schema: Schema.Codec<A, I>;
    readonly content: Effect.Effect<A, E, R>;
    readonly changes: Stream.Stream<unknown, never, R>;
}) {
    const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), resourceValues(options.schema, options.mimeType, options.changes)]);
    const content = Effect.provideContext(options.content, services);
    yield* McpServer.registerResource({ ...options, content: read(options.uri, content) });
    return depend(Effect.succeed(options.uri), Function.constant(content));
});
const registerResourceTemplate =
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
            const [services, { read, depend }] = yield* Effect.all([Effect.context<R>(), resourceValues(options.schema, options.mimeType, options.changes)]);
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

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Dependency, type DependencyOptions, registerResource, registerResourceTemplate, resourceValues };
