import { Array, Cause, Context, Effect, Equal, flow, HashMap, HashSet, Option, Predicate, Record, Schema, SchemaAST, Stream, Struct, SubscriptionRef } from 'effect';
import { McpSchema, McpServer } from 'effect/ai';
import { Server } from '#/server.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Params<Schemas extends readonly Schema.Constraint[]> = { readonly [K in keyof Schemas]: Schemas[K]['Type'] };
type Named<Schemas extends readonly Schema.Constraint[]> = { readonly [K in Extract<keyof Schemas, `${number}`> as Schemas[K] extends McpSchema.Param<infer Name, Schema.Constraint> ? Name : `param${K}`]: Schemas[K]['Type'] };
type Descriptor = Omit<typeof McpServer.registerResource extends { (options: infer Options): unknown; (...template: never): unknown } ? Options : never, 'uri' | 'content'>;
interface Entry<A> {
    readonly read: Effect.Effect<A, Error>;
    readonly value: A;
    readonly uris: HashSet.HashSet<string>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const encodeJsonText = Schema.encodeEffect(Schema.fromJsonString(Schema.Json));
const updatedUris = <A>(last: HashMap.HashMap<string, Entry<A>>, current: HashMap.HashMap<string, Entry<A>>) => Array.flatMap([...last], ([key, entry]) => (Option.contains(Option.map(HashMap.get(current, key), Struct.get('value')), entry.value) ? [] : [...entry.uris]));
const complete = <Fields extends Schema.Struct.Fields, Given = Schema.Struct.Type<Fields>>(fields: Fields) => {
    const none: Partial<Given> = {};
    const decoders = Record.collect(fields, (key) => Schema.decodeUnknownEffect(Schema.toCodecStringTree(Schema.Struct(Struct.pick(fields, [key])))));
    const decode = (resolved: Readonly<Record<string, string>>): Effect.Effect<Partial<Given>, never, Schema.Struct<Fields>['DecodingServices']> =>
        Effect.map(
            Effect.forEach(decoders, (decoder) => Effect.option(decoder(resolved))),
            (picked) => ({ ...none, ...Object.fromEntries(Array.flatMap(Array.getSomes(picked), Object.entries)) }),
        );
    return <Values, R>(completing: (input: string, given: Partial<Given>) => Effect.Effect<Values, Error, R>) =>
        (input: string, context: typeof McpSchema.Complete.payloadSchema.Type.context) =>
            Effect.flatMap(decode(context?.arguments ?? {}), (given) => completing(input, given));
};
const make = Effect.fnUntraced(function* <A, I, R, D extends Partial<Record<'uri' | 'mimeType' | 'description', string | undefined>>>({ schema, changes, uri, mimeType, description, ...descriptor }: D & { readonly schema: Schema.Codec<A, I>; readonly changes: Stream.Stream<unknown, never, R> }) {
    const [server, store] = yield* Effect.all([McpServer.McpServer, SubscriptionRef.make(HashMap.empty<string, Entry<A>>())]);
    const codec = Schema.toCodecJson(schema);
    const encoded = Schema.encodeEffect(codec);
    const media = SchemaAST.isString(SchemaAST.toEncoded(codec.ast)) ? mimeType : 'application/json';
    const observe = (key: string, read: Effect.Effect<A, Error>, at: string) =>
        read.pipe(
            Effect.tap((value) => SubscriptionRef.update(store, HashMap.modifyAt(key, Option.match({ onNone: () => Option.some({ read, value, uris: HashSet.make(key, at) }), onSome: ({ uris }) => Option.some({ read, value, uris: HashSet.add(uris, at) }) })))),
            Effect.tapCauseIf(flow(Cause.findErrorOption, Option.exists(Cause.isNoSuchElementError)), () => SubscriptionRef.update(store, HashMap.remove(key))),
        );
    const reply = (at: string, value: A) =>
        value instanceof Uint8Array
            ? Effect.succeed(McpSchema.ReadResourceResult.make({ contents: [McpSchema.BlobResourceContents.make({ uri: at, mimeType: media, blob: value })] }))
            : encoded(value).pipe(
                  Effect.filterOrElse(Predicate.isString, encodeJsonText),
                  Effect.orDie,
                  Effect.map((text) => McpSchema.ReadResourceResult.make({ contents: [McpSchema.TextResourceContents.make({ uri: at, mimeType: media, text })] })),
              );
    const updated = (at: string) => server.notifications['notifications/resources/updated']({ uri: at });
    yield* changes.pipe(
        Stream.mapEffect(() => SubscriptionRef.get(store)),
        Stream.tap((entries) => (uri === undefined || HashMap.has(entries, uri) ? Effect.void : updated(uri))),
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
        Stream.mapAccum(HashMap.empty<string, Entry<A>>, (last, next) => [next, updatedUris(last, next)]),
        Stream.runForEach(updated),
        Effect.forkScoped,
    );
    return {
        descriptor: { ...descriptor, mimeType: media, description: description ?? SchemaAST.resolveDescription(schema.ast) },
        read: (key: string, at: string, content: Effect.Effect<A, Error>) =>
            observe(key, content, at).pipe(Effect.catchNoSuchElement, Server.orInternalError, Effect.flatMap(Option.match({ onNone: () => Effect.fail(McpSchema.InvalidParams.make({ message: `Resource '${at}' not found` })), onSome: (value) => reply(at, value) }))),
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
const registerTemplate = <const Schemas extends readonly Schema.Codec<unknown, unknown>[]>(path: TemplateStringsArray, ...schemas: Schemas) =>
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
        const merged = (request: Context.Context<never>) => Context.merge(services, request);
        const reading = (params: Params<Schemas>, at: string) => Effect.updateContext(content(at, ...params), merged);
        const completing = complete<Record<string, Schemas[number]>, Named<Schemas>>(Record.fromEntries(Array.map(schemas, (each, index) => [McpSchema.isParam(each) ? each.name : `param${index}`, each] as const)));
        yield* McpServer.registerResource(
            segments,
            ...schemas,
        )({
            ...store.descriptor,
            completion: Record.map({ ...completion }, (handle) => flow(completing<Named<Schemas>[keyof Named<Schemas>][], R>(handle), Effect.updateContext(merged))),
            content: (at, ...params) => Effect.flatMap(keyed(params), (key) => store.read(key, at, reading(params, at))),
        });
    });

// --- [EXPORTS] -------------------------------------------------------------------------

export { registerResource, registerTemplate };
