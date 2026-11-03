import { Effect, FileSystem, Match, Option, Path, type PlatformError, Predicate, Record, Schema, SchemaGetter } from 'effect';
import { Expression } from '#/cases';
import { Enumerated, Enumerator } from '#/results';
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

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Literal(true).annotate({ description: 'Application-level case reading defaults, opening no document' });
const Open = Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' });
const Copy = Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy the engine opens' });
const Assignments = Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' }));
const Steps = Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' }));

// --- [OPERATIONS] ----------------------------------------------------------------------

const documents = <S extends Schema.Top>(item: S) => Schema.Array(item).annotate({ description: 'Case documents opened with a window in order, the first one active, none for an application case' });
const added = <F extends Schema.Struct.Fields>(kind: string, fields: F) =>
    Schema.optionalKey(
        Schema.Array(Schema.Struct({ as: Schema.NonEmptyString, to: Expression, kind: Schema.NonEmptyString.annotate({ description: `Collection member of the target, as ${kind}` }), ...fields, properties: Schema.optionalKey(JsonObject) })).annotate({
            description: 'Items added and named as made.<as>',
        }),
    );
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
    const declared = (tag: string) => Option.exists(Schema.decodeUnknownOption(Enumerator)(tag), ([, enumeration, , name]) => Option.exists(Record.get(enumerations, enumeration), (listed) => listed.includes(name)));
    return <T extends Schema.Json>() =>
        Schema.makeFilter(
            (value: T) =>
                tags(value)
                    .filter((tag) => tag.startsWith('enum:') && !declared(tag))
                    .map((tag) => `${tag} names no enumerator the host declares, enumerators read enum:<Enumeration>.<MEMBER>`)[0] ?? true,
        );
};
const member = (enumeration: string, names?: readonly string[]) => {
    const named: Schema.Codec<string> = names === undefined ? Schema.NonEmptyString : Schema.Literals(names);
    return named.pipe(Schema.decodeTo(Enumerated, { decode: SchemaGetter.transform((name) => ({ $: `enum:${enumeration}.${name}` as const })), encode: SchemaGetter.forbiddenEncoding })).annotate({ description: `${enumeration} member the host declares` });
};
const opens = (document: unknown): document is { readonly open: string } => Predicate.hasProperty(document, 'open') && Predicate.isString(document.open);
const copied = Effect.fn('copied')(function* <P extends { readonly documents: readonly unknown[] }>(decoded: P, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const copy = (open: string) => path.join(folder, path.basename(open));
    const relocated = yield* Effect.forEach(decoded.documents, (document): Effect.Effect<unknown, PlatformError.PlatformError> => (opens(document) ? Effect.as(fs.copyFile(path.join(product, document.open), copy(document.open)), { ...document, open: copy(document.open) }) : Effect.succeed(document)));
    return { precondition: { ...decoded, documents: relocated }, copies: decoded.documents.filter(opens).map(({ open }) => path.join(product, open)) };
});
const placings = <P extends Schema.Top, D extends Schema.Top & Schema.Decoder<unknown>, O extends Schema.Top & Schema.Decoder<unknown>, A extends Schema.Top & Schema.Decoder<unknown>>(Placed: P, sources: Sources<D, O, A>, made: Placings<P, D, O, A>) =>
    Schema.Union([placing(Placed, sources.document, made.document), placing(Placed, sources.open, made.open), placing(Placed, sources.application, made.application)]);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Assignments, added, Copy, copied, documents, enumerators, many, member, merged, Open, placing, placings, Steps, tags };
