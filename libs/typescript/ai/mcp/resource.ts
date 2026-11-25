import { Array, Cause, Context, Effect, Equal, flow, HashMap, HashSet, Option, Predicate, Record, Schema, SchemaAST, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import { Server } from '#/server';

// --- [TYPES] ---------------------------------------------------------------------------

type NamedParams<Schemas extends readonly Schema.Constraint[]> = { readonly [K in Extract<keyof Schemas, `${number}`> as Schemas[K] extends McpSchema.Param<infer Name, Schema.Constraint> ? Name : `param${K}`]: Schemas[K]['Type'] };
type Descriptor = Omit<typeof McpServer.registerResource extends { (options: infer Options): unknown; (...template: never): unknown } ? Options : never, 'uri' | 'content'>;
interface Entry<A> {
    readonly read: Effect.Effect<A, Error>;
    readonly value: A;
    readonly uris: HashSet.HashSet<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeJsonText = Schema.encodeEffect(Schema.fromJsonString(Schema.Json));
const make = Effect.fnUntraced(function* <A, I, R, D extends Partial<Record<'uri' | 'mimeType' | 'description', string | undefined>>>({ schema, changes, uri, mimeType, description, ...descriptor }: D & { readonly schema: Schema.Codec<A, I>; readonly changes: Stream.Stream<unknown, never, R> }) {
    const [server, store] = yield* Effect.all([McpServer.McpServer, SubscriptionRef.make(HashMap.empty<string, Entry<A>>())]);
    const codec = Schema.toCodecJson(schema);
    const media = SchemaAST.isString(SchemaAST.toEncoded(codec.ast)) ? mimeType : 'application/json';
    const notifyUpdated = (at: string) => server.notifications['notifications/resources/updated']({ uri: at });
    yield* changes.pipe(
        Stream.mapEffect(() => SubscriptionRef.get(store)),
        Stream.tap((entries) => (uri === undefined || HashMap.has(entries, uri) ? Effect.void : notifyUpdated(uri))),
        Stream.flattenIterable,
        Stream.mapEffect(
            Effect.fnUntraced(
                function* ([key, entry]: readonly [string, Entry<A>]) {
                    const kept = Option.filter(yield* Effect.catchNoSuchElement(entry.read), Equal.equals(entry.value));
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
        Stream.mapAccum(HashMap.empty<string, Entry<A>>, (last, next) => [next, Array.flatMap([...last], ([key, entry]) => (Option.contains(Option.map(HashMap.get(next, key), Struct.get('value')), entry.value) ? [] : [...entry.uris]))]),
        Stream.runForEach(notifyUpdated),
        Effect.forkScoped,
    );
    return {
        descriptor: { ...descriptor, mimeType: media, description: description ?? SchemaAST.resolveDescription(schema.ast) },
        read: (key: string, at: string, content: Effect.Effect<A, Error>) =>
            content.pipe(
                Effect.tap((value) => SubscriptionRef.update(store, HashMap.modifyAt(key, Option.match({ onNone: () => Option.some({ read: content, value, uris: HashSet.make(key, at) }), onSome: ({ uris }) => Option.some({ read: content, value, uris: HashSet.add(uris, at) }) })))),
                Effect.tapCauseIf(flow(Cause.findErrorOption, Option.exists(Cause.isNoSuchElementError)), () => SubscriptionRef.update(store, HashMap.remove(key))),
                Effect.catchNoSuchElement,
                Server.orInternalError,
                Effect.flatMap(Effect.fromOption(() => McpSchema.InvalidParams.make({ message: `Resource '${at}' not found` }))),
                Effect.flatMap((value) =>
                    value instanceof Uint8Array
                        ? Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [McpSchema.BlobResourceContents.make({ uri: at, mimeType: media, blob: value })] }))
                        : Effect.map(Effect.orDie(Effect.filterOrElse(Schema.encodeEffect(codec)(value), Predicate.isString, encodeJsonText)), (text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri: at, mimeType: media, text })] })),
                ),
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
    const [server, services] = yield* Effect.all([Server, Effect.context<R>()]);
    const [uri, read] = [server.uri(descriptor.name), Effect.updateContext(content, (request: Context.Context<never>) => Context.merge(services, request))];
    const store = yield* make({ ...descriptor, uri, schema, changes });
    yield* McpServer.registerResource({ ...store.descriptor, uri, content: store.read(uri, uri, read) });
});
const registerResourceTemplate = <const Schemas extends readonly Schema.Codec<unknown, unknown>[]>(path: TemplateStringsArray, ...schemas: Schemas) =>
    Effect.fnUntraced(function* <A, I, E extends Error, R>({
        schema,
        content,
        changes,
        completion,
        ...descriptor
    }: Descriptor & {
        readonly completion?: { readonly [K in keyof NamedParams<Schemas>]?: (input: string, given: Partial<NamedParams<Schemas>>) => Effect.Effect<NamedParams<Schemas>[K][], Error, R> } | undefined;
        readonly schema: Schema.Codec<A, I>;
        readonly content: (uri: string, ...params: { readonly [K in keyof Schemas]: Schemas[K]['Type'] }) => Effect.Effect<A, E, R>;
        readonly changes: Stream.Stream<unknown, never, R>;
    }) {
        const [server, services, store] = yield* Effect.all([Server, Effect.context<R>(), make({ ...descriptor, schema, changes })]);
        const rootSegments = Array.map((part: string, index: number) => (index === 0 ? server.uri(part) : part));
        const segments = Object.assign(rootSegments(path), { raw: rootSegments(path.raw) });
        const interleave = <B>(values: readonly B[]) => Array.flatMap(segments, (segment, index) => [segment, ...Option.toArray(Array.get(values, index))]);
        const canonical = Schema.encodeUnknownEffect(Schema.TemplateLiteralParser(interleave(Array.map(schemas, (each) => Schema.StringFromUriComponent.pipe(Schema.decodeTo(Schema.toCodecStringTree(each)))))));
        const mergeContext = (request: Context.Context<never>) => Context.merge(services, request);
        const none: Partial<NamedParams<Schemas>> = {};
        const decoders = schemas.map((each, index) => Schema.decodeUnknownEffect(Schema.toCodecStringTree(Schema.Struct({ [McpSchema.isParam(each) ? each.name : `param${index}`]: each }))));
        yield* McpServer.registerResource(
            segments,
            ...schemas,
        )({
            ...store.descriptor,
            completion: Record.map(
                { ...completion },
                (handle: (input: string, given: Partial<NamedParams<Schemas>>) => Effect.Effect<NamedParams<Schemas>[keyof NamedParams<Schemas>][], Error, R>) => (input: string, context: Parameters<McpServer.McpServer['Service']['completion']>[0]['context']) =>
                    Effect.forEach(decoders, (decoder) => Effect.option(decoder(context?.arguments ?? {}))).pipe(
                        Effect.flatMap((picked) => handle(input, { ...none, ...Object.fromEntries(Array.flatMap(Array.getSomes(picked), Object.entries)) })),
                        Effect.updateContext(mergeContext),
                    ),
            ),
            content: (at, ...params) => Effect.flatMap(Effect.orDie(canonical(interleave(params))), (key) => store.read(key, at, Effect.updateContext(content(at, ...params), mergeContext))),
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerResource, registerResourceTemplate };
