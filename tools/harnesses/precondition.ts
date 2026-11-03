import { Effect, FileSystem, Match, Option, Path, type PlatformError, Predicate, Record, Schema, SchemaGetter } from 'effect';
import { Enumerated } from '#/results';
import { JsonObject, Tagged } from '#/values';

// --- [TYPES] ---------------------------------------------------------------------------

interface Sources<D extends Schema.Top, O extends Schema.Top, A extends Schema.Top> {
    readonly document: D;
    readonly open: O;
    readonly application: A;
}
interface Placings<P extends Schema.Top, D extends Schema.Top, O extends Schema.Top, A extends Schema.Top> {
    readonly document: (held: D['Type']) => P['Type'];
    readonly open: (held: O['Type']) => P['Type'];
    readonly application: (held: A['Type']) => P['Type'];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const OPENING: readonly string[] = ['application', 'document', 'open', 'template'];
const ENUMERATOR = /^enum:(?<enumeration>[^.]+)\.(?<name>.+)$/u;

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Literal(true).annotate({ description: 'Application-level case reading defaults, opening no document' });

// --- [OPERATIONS] ----------------------------------------------------------------------

const deep = (left: typeof JsonObject.Type, right: typeof JsonObject.Type): typeof JsonObject.Type => Record.union(left, right, (inner, outer) => (Schema.is(JsonObject)(inner) && Schema.is(JsonObject)(outer) ? deep(inner, outer) : outer));
const merged = (batch: typeof JsonObject.Type, row: typeof JsonObject.Type): typeof JsonObject.Type => {
    const replaced = OPENING.some((key) => key in row);
    return deep(
        Record.filter(batch, (_, key) => !(replaced && OPENING.includes(key))),
        row,
    );
};
const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.NonEmptyArray(item), item.pipe(Schema.decodeTo(Schema.NonEmptyArray(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const placing = <P extends Schema.Top, S extends Schema.Top & Schema.Decoder<unknown>>(Placed: P, row: S, decode: (held: S['Type']) => P['Type']) => row.pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
const tags = (value: Schema.Json): readonly string[] =>
    Match.value(value).pipe(
        Match.when(Schema.is(Schema.Array(Schema.Json)), (items) => items.flatMap(tags)),
        Match.when(Schema.is(Tagged), ({ $ }) => [$]),
        Match.when(Schema.is(JsonObject), (held) => Object.values(held).flatMap(tags)),
        Match.orElse(() => []),
    );
const enumerators = (enumerations: Record.ReadonlyRecord<string, readonly string[]>) => {
    const declared = (tag: string) => Option.exists(Option.fromNullishOr(ENUMERATOR.exec(tag)?.groups), ({ enumeration = '', name = '' }) => Option.exists(Record.get(enumerations, enumeration), (listed) => listed.includes(name)));
    return <T extends Schema.Json>() =>
        Schema.makeFilter(
            (value: T) =>
                tags(value)
                    .filter((tag) => tag.startsWith('enum:') && !declared(tag))
                    .map((tag) => `${tag} names no enumerator the host declares, enumerators read enum:<Enumeration>.<MEMBER>`)[0] ?? true,
        );
};
const member = (enumeration: string, names: readonly string[]) =>
    Schema.Literals(names)
        .pipe(Schema.decodeTo(Enumerated, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding }))
        .annotate({ description: `${enumeration} member the host declares` });
const opens = (document: unknown): document is { readonly open: string } => Predicate.hasProperty(document, 'open') && Predicate.isString(document.open);
const copied = Effect.fn('copied')(function* <P extends { readonly documents: readonly unknown[] }>(decoded: P, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const copy = (open: string) => path.join(folder, path.basename(open));
    const documents = yield* Effect.forEach(decoded.documents, (document): Effect.Effect<unknown, PlatformError.PlatformError> => (opens(document) ? Effect.as(fs.copyFile(path.join(product, document.open), copy(document.open)), { ...document, open: copy(document.open) }) : Effect.succeed(document)));
    return { precondition: { ...decoded, documents }, copies: decoded.documents.filter(opens).map(({ open }) => path.join(product, open)) };
});
const placings = <P extends Schema.Top, D extends Schema.Top & Schema.Decoder<unknown>, O extends Schema.Top & Schema.Decoder<unknown>, A extends Schema.Top & Schema.Decoder<unknown>>(Placed: P, sources: Sources<D, O, A>, made: Placings<P, D, O, A>) =>
    Schema.Union([placing(Placed, sources.document, made.document), placing(Placed, sources.open, made.open), placing(Placed, sources.application, made.application)]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, copied, enumerators, many, member, merged, placing, placings, tags };
