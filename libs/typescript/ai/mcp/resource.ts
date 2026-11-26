import { Array, Cause, Context, Effect, HashMap, Option, Predicate, Record, Schema, SchemaAST, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type Descriptor = Omit<typeof McpServer.registerResource extends { (options: infer Options): unknown; (...template: never): unknown } ? Options : never, 'uri' | 'content'>;
type Params<Schemas extends readonly Schema.Constraint[]> = { readonly [K in Extract<keyof Schemas, `${number}`> as Schemas[K] extends McpSchema.Param<infer Name, Schema.Constraint> ? Name : `param${K}`]: Schemas[K]['Type'] };
interface Entry<A> {
    readonly read: Effect.Effect<A, Error>;
    readonly value: A;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeJsonText = Schema.encodeEffect(Schema.fromJsonString(Schema.Json));
const track = Effect.fnUntraced(function* <A, I, R>({ schema, changes, uri, mimeType, description, ...descriptor }: Descriptor & { readonly uri?: string; readonly schema: Schema.Codec<A, I>; readonly changes: Stream.Stream<unknown, never, R> }) {
    const [server, services, store] = yield* Effect.all([McpServer.McpServer, Effect.context<R>(), SubscriptionRef.make(HashMap.empty<string, Entry<A>>())]);
    const codec = Schema.toCodecJson(schema);
    const text = SchemaAST.isString(SchemaAST.toEncoded(codec.ast));
    const notify = (at: string) => server.notifications['notifications/resources/updated']({ uri: at });
    const provide = <X, E>(effect: Effect.Effect<X, E, R>) => Effect.updateContext(effect, (request: Context.Context<never>) => Context.merge(services, request));
    const record = <E extends Error>(at: string, read: Effect.Effect<A, E>) =>
        read.pipe(
            Effect.tap((value) => SubscriptionRef.update(store, (entries) => HashMap.set(entries, at, { read, value }))),
            Effect.tapError((error) => (Cause.isNoSuchElementError(error) ? SubscriptionRef.update(store, HashMap.remove(at)) : Effect.void)),
        );
    yield* changes.pipe(
        Stream.mapEffect(() => SubscriptionRef.get(store)),
        Stream.tap((entries) => (uri === undefined || HashMap.has(entries, uri) ? Effect.void : notify(uri))),
        Stream.flattenIterable,
        Stream.mapEffect(([at, { read }]) => record(at, read).pipe(Effect.catchNoSuchElement, Effect.ignoreCause({ log: 'Error' })), { concurrency: 'unbounded' }),
        Stream.runDrain,
        Effect.forkScoped,
    );
    yield* SubscriptionRef.changes(store).pipe(
        Stream.mapAccum(HashMap.empty<string, Entry<A>>, (last, next) => [next, Array.fromIterable(HashMap.keys(HashMap.filter(last, ({ value }, at) => !Option.contains(Option.map(HashMap.get(next, at), Struct.get('value')), value))))]),
        Stream.runForEach(notify),
        Effect.forkScoped,
    );
    return {
        descriptor: { ...descriptor, mimeType: text ? mimeType : 'application/json', description: description ?? SchemaAST.resolveDescription(schema.ast) },
        provide,
        read: <E extends Error>(at: string, content: Effect.Effect<A, E, R>) =>
            Effect.filterOrElse(
                record(at, provide(content)),
                (value): value is A & Uint8Array => Predicate.isUint8Array(value),
                (value) => Effect.orDie(Effect.filterOrElse(Schema.encodeEffect(codec)(value), (json): json is string => text && Predicate.isString(json), encodeJsonText)),
            ),
    };
});
const registerResource = Effect.fnUntraced(function* <A, I, E extends Error, R>({
    schema,
    content,
    changes,
    ...descriptor
}: Descriptor & {
    readonly schema: Schema.Codec<A, I>;
    readonly content: Effect.Effect<A, E, R>;
    readonly changes: Stream.Stream<unknown, never, R>;
}) {
    const uri = (yield* Server).uri(descriptor.name);
    const tracked = yield* track({ ...descriptor, uri, schema, changes });
    yield* McpServer.registerResource({ ...tracked.descriptor, uri, content: tracked.read(uri, content) });
});
const registerResourceTemplate = <const Schemas extends readonly Schema.Codec<unknown, unknown>[]>(path: TemplateStringsArray, ...schemas: Schemas) =>
    Effect.fnUntraced(function* <A, I, E extends Error, R>({
        schema,
        content,
        changes,
        completion,
        ...descriptor
    }: Descriptor & {
        readonly completion?: { readonly [K in keyof Params<Schemas>]?: (input: string, given: Partial<Params<Schemas>>) => Effect.Effect<Params<Schemas>[K][], Error, R> } | undefined;
        readonly schema: Schema.Codec<A, I>;
        readonly content: (uri: string, ...params: { readonly [K in keyof Schemas]: Schemas[K]['Type'] }) => Effect.Effect<A, E, R>;
        readonly changes: Stream.Stream<unknown, never, R>;
    }) {
        const [server, tracked] = yield* Effect.all([Server, track({ ...descriptor, schema, changes })]);
        const rooted = Array.map((part: string, index: number) => (index === 0 ? server.uri(part) : part));
        const none: Partial<Params<Schemas>> = {};
        const decoders = Array.map(schemas, (each, index) => [McpSchema.isParam(each) ? each.name : `param${index}`, Schema.decodeUnknownOption(Schema.toCodecStringTree(each))] as const);
        yield* McpServer.registerResource(
            Object.assign(rooted(path), { raw: rooted(path.raw) }),
            ...schemas,
        )({
            ...tracked.descriptor,
            completion: Record.map(
                { ...completion },
                (complete: (input: string, given: Partial<Params<Schemas>>) => Effect.Effect<Params<Schemas>[keyof Params<Schemas>][], Error, R>) => (input: string, context: (typeof McpSchema.Complete.payloadSchema.Type)['context']) =>
                    tracked.provide(complete(input, { ...none, ...Record.getSomes(Record.fromEntries(Array.map(decoders, ([name, decode]) => [name, decode(context?.arguments?.[name])]))) })),
            ),
            content: (at, ...params) => tracked.read(at, content(at, ...params)),
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerResource, registerResourceTemplate };
