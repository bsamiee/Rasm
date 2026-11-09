import { Array, Cause, Effect, Equal, Function, flow, HashMap, HashSet, Number, Option, Predicate, Record, Ref, Schema, SchemaAST, Stream, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import { Route } from '#/route';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type Params<Schemas extends readonly Schema.Constraint[]> = { readonly [K in keyof Schemas]: Schemas[K]['Type'] };
type Named<Schemas extends readonly Schema.Constraint[]> = { readonly [K in Extract<keyof Schemas, `${number}`> as Schemas[K] extends McpSchema.Param<infer Name, Schema.Constraint> ? Name : `param${K}`]: Schemas[K]['Type'] };
type Resource<A> = Effect.Success<ReturnType<typeof make<A, unknown, never, Record<string, never>>>>;
type Descriptor = Omit<typeof McpServer.registerResource extends { (options: infer Options): unknown; (...template: never): unknown } ? Options : never, 'uri' | 'content'>;
interface Entry<A> {
    readonly read: Effect.Effect<A, Error>;
    readonly value: A;
    readonly uris: HashSet.HashSet<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeJsonText = Schema.encodeEffect(Schema.fromJsonString(Schema.Json));
const updatedUris = <A>(last: HashMap.HashMap<string, Entry<A>>, current: HashMap.HashMap<string, Entry<A>>) => Array.flatMap([...last], ([key, entry]) => (Option.contains(Option.map(HashMap.get(current, key), Struct.get('value')), entry.value) ? [] : [...entry.uris]));
const make = Effect.fnUntraced(function* <A, I, R, D extends { readonly mimeType?: string | undefined; readonly description?: string | undefined }>({ schema, changes, mimeType, description, ...descriptor }: D & { readonly schema: Schema.Codec<A, I>; readonly changes: Stream.Stream<unknown, never, R> }) {
    const [server, store, watched] = yield* Effect.all([McpServer.McpServer, SubscriptionRef.make(HashMap.empty<string, Entry<A>>()), Ref.make(HashMap.empty<string, number>())]);
    const codec = Schema.toCodecJson(schema);
    const encoded = Schema.encodeEffect(codec);
    const media = SchemaAST.isString(SchemaAST.toEncoded(codec.ast)) ? mimeType : 'application/json';
    const observe = (key: string, read: Effect.Effect<A, Error>, at: string = key) =>
        read.pipe(
            Effect.tap((value) => SubscriptionRef.update(store, HashMap.modifyAt(key, Option.match({ onNone: () => Option.some({ read, value, uris: HashSet.make(key, at) }), onSome: ({ uris }) => Option.some({ read, value, uris: HashSet.add(uris, at) }) })))),
            Effect.tapCauseIf(flow(Cause.findErrorOption, Option.exists(Cause.isNoSuchElementError)), () => SubscriptionRef.update(store, HashMap.remove(key))),
        );
    const cached = ({ key, content }: { readonly key: string; readonly content: Effect.Effect<A, Error> }) => Effect.flatMap(SubscriptionRef.get(store), flow(HashMap.get(key), Option.match({ onNone: () => observe(key, content), onSome: ({ value }) => Effect.succeed(value) })));
    const matchingEntry = (at: string, holds: Predicate.Predicate<A>) =>
        flow(
            Option.filter(holds),
            Option.map(
                flow(
                    encoded,
                    Effect.map((json) => [at, json] as const),
                ),
            ),
            Effect.transposeOption,
        );
    const watch = (at: string) => Effect.acquireRelease(Ref.update(watched, HashMap.modifyAt(at, flow(Option.map(Number.increment), Option.orElseSome(Function.constant(1))))), () => Ref.update(watched, HashMap.modifyAt(at, flow(Option.map(Number.decrement), Option.filter(Number.isGreaterThan(0))))));
    const reply = (at: string, value: A) =>
        value instanceof Uint8Array
            ? Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [McpSchema.BlobResourceContents.make({ uri: at, mimeType: media, blob: value })] }))
            : encoded(value).pipe(
                  Effect.filterOrElse(Predicate.isString, encodeJsonText),
                  Effect.orDie,
                  Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri: at, mimeType: media, text })] })),
              );
    yield* changes.pipe(
        Stream.mapEffect(() => SubscriptionRef.get(store)),
        Stream.flattenIterable,
        Stream.mapEffect(
            Effect.fnUntraced(
                function* ([key, entry]: readonly [string, Entry<A>]) {
                    const [value, watching] = yield* Effect.all([Effect.catchNoSuchElement(entry.read), Ref.get(watched)]);
                    const kept = HashMap.has(watching, key) ? value : Option.filter(value, Equal.equals(entry.value));
                    yield* SubscriptionRef.update(store, Option.match(kept, { onNone: () => HashMap.remove(key), onSome: (next) => HashMap.modify(key, Struct.assign({ value: next })) }));
                },
                Effect.ignoreCause({ log: 'Error' }),
            ),
            { concurrency: 'unbounded' },
        ),
        Stream.runDrain,
        Effect.forkScoped,
    );
    yield* SubscriptionRef.changes(store).pipe(
        Stream.mapAccum(HashMap.empty<string, Entry<A>>, (last, current) => [current, updatedUris(last, current)]),
        Stream.runForEach((uri) => server.notifications['notifications/resources/updated']({ uri })),
        Effect.forkScoped,
    );
    return {
        descriptor: { ...descriptor, mimeType: media, description: description ?? SchemaAST.resolveDescription(schema.ast) },
        read: (key: string, at: string, content: Effect.Effect<A, Error>) =>
            observe(key, content, at).pipe(Effect.catchNoSuchElement, Server.orInternalError, Effect.flatMap(Option.match({ onNone: () => Effect.fail(McpSchema.InvalidParams.make({ message: `Resource '${at}' not found` })), onSome: (value) => reply(at, value) }))),
        cached,
        depend:
            (source: Effect.Effect<Parameters<typeof cached>[0], Error>) =>
            ({ refuse, report }: { readonly refuse?: Predicate.Predicate<A> | undefined; readonly report?: Predicate.Predicate<A> | undefined }) => {
                const current = ({ key, content }: Effect.Success<typeof source>) => Effect.catchNoSuchElement(observe(key, content));
                return {
                    refusal: UndefinedOr.match(refuse, {
                        onUndefined: () => Stream.empty,
                        onDefined: Effect.fnUntraced(function* (holds: Predicate.Predicate<A>) {
                            const located = yield* source;
                            yield* Effect.andThen(watch(located.key), current(located));
                            return SubscriptionRef.changes(store).pipe(Stream.map(flow(HashMap.get(located.key), Option.map(Struct.get('value')))), Stream.changes, Stream.mapEffect(matchingEntry(located.key, holds)));
                        }, Stream.unwrap),
                    }),
                    report: UndefinedOr.match(report, {
                        onUndefined: () => Effect.succeedNone,
                        onDefined: Effect.fnUntraced(function* (holds: Predicate.Predicate<A>) {
                            const located = yield* source;
                            return yield* matchingEntry(located.key, holds)(yield* current(located));
                        }),
                    }),
                };
            },
    };
});
const register = Effect.fnUntraced(function* <A, I, E extends Error, R>({
    schema,
    content,
    changes,
    ...descriptor
}: Descriptor & {
    readonly schema: Schema.Codec<A, I>;
    readonly content: Effect.Effect<A, E, R>;
    readonly changes: Stream.Stream<unknown, never, R>;
}) {
    const [server, services, store] = yield* Effect.all([Server, Effect.context<R>(), make({ ...descriptor, schema, changes })]);
    const [uri, read] = [server.uri(descriptor.name), Effect.provideContext(content, services)];
    yield* McpServer.registerResource({ ...store.descriptor, uri, content: store.read(uri, uri, read) });
    return store.depend(Effect.succeed({ key: uri, content: read }));
});
const template = <const Schemas extends readonly Schema.Codec<unknown, unknown>[]>(path: TemplateStringsArray, ...schemas: Schemas) =>
    Effect.fnUntraced(function* <A, I, E extends Error, R>({
        schema,
        content,
        changes,
        completion,
        ...descriptor
    }: Descriptor & {
        readonly completion?: { readonly [K in keyof Named<Schemas>]?: (input: string, given: Partial<Named<Schemas>>) => Effect.Effect<Named<Schemas>[K][], Error, R> } | undefined;
        readonly schema: Schema.Codec<A, I>;
        readonly content: (uri: string, ...params: Params<Schemas>) => Effect.Effect<A, E, R>;
        readonly changes: Stream.Stream<unknown, never, R>;
    }) {
        const [server, services, store] = yield* Effect.all([Server, Effect.context<R>(), make({ ...descriptor, schema, changes })]);
        const rooted = Array.map((part: string, index: number) => (index === 0 ? server.uri(part) : part));
        const segments = Object.assign(rooted(path), { raw: rooted(path.raw) });
        const interleaved = <B>(values: readonly B[]) => Array.flatMap(segments, (segment, index) => [segment, ...Option.toArray(Array.get(values, index))]);
        const canonical = Schema.encodeUnknownEffect(Schema.TemplateLiteralParser(interleaved(Array.map(schemas, (each) => Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.toCodecStringTree(each)))))));
        const keyed = (params: Params<Schemas>) => Effect.orDie(canonical(interleaved(params)));
        const reading = (params: Params<Schemas>, at: string) => Effect.provideContext(content(at, ...params), services);
        const completing = Route.complete<Record<string, Schemas[number]>, Named<Schemas>>(Record.fromEntries(Array.map(schemas, (each, index) => [McpSchema.isParam(each) ? each.name : `param${index}`, each] as const)));
        yield* McpServer.registerResource(
            segments,
            ...schemas,
        )({
            ...store.descriptor,
            completion: Record.map({ ...completion }, (handle) => flow(completing(handle), Effect.provideContext(services))),
            content: (at, ...params) => Effect.flatMap(keyed(params), (key) => store.read(key, at, reading(params, at))),
        });
        return (...params: Params<Schemas>) => store.depend(Effect.map(keyed(params), (key) => ({ key, content: reading(params, key) })));
    });
const Resource = Object.assign(make, { register, template });

// --- [EXPORTS] -------------------------------------------------------------------------

export { Resource };
