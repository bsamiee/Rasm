import { Effect, FileSystem, Match, Option, Path, type PlatformError, Record, Schema, SchemaGetter } from 'effect';
import { Expression } from '#/cases/schema';
import { JsonObject, Tagged } from '#/cases/values';

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

// --- [MODELS] --------------------------------------------------------------------------

const Application = Schema.Literal(true).annotate({ description: 'Application-level case reading defaults, opening no document' });
const Open = Schema.NonEmptyString.annotate({ description: 'Document path under the product folder, its copy opened with a window' });
const Copy = Schema.NonEmptyString.annotate({ description: 'Absolute path of the case copy the engine opens' });
const Assignments = Schema.optionalKey(Schema.Array(Schema.Tuple([Expression, Schema.Json])).annotate({ description: 'Member path and value written in order, [*] spreading over a collection' }));
const Steps = Schema.optionalKey(Schema.Array(Schema.NonEmptyString).annotate({ description: 'Statements run in order after set and add' }));
const Parsed = Schema.TemplateLiteralParser(['enum:', Schema.String, '.', Schema.String]);
const Jsons = Schema.Array(Schema.Json);
const Opened = Schema.Struct({ open: Schema.String }).annotate({ description: 'Case document a path opens' });
const Enumerated = Object.assign(Schema.Struct({ $: Schema.TemplateLiteral(Parsed.parts) }).annotate({ description: 'Enumerator the surveyed enumerations declare' }), { parts: Parsed });

// --- [OPERATIONS] ----------------------------------------------------------------------

const documents = <S extends Schema.Top>(item: S) => Schema.Array(item).annotate({ description: 'Case documents opened with a window in order, the first one active, none for an application case' });
const added = <F extends Schema.Struct.Fields>(kind: string, fields: F) => Schema.Struct({ as: Schema.NonEmptyString, to: Expression, kind: Schema.NonEmptyString.annotate({ description: `Collection member of the target, as ${kind}` }), ...fields, properties: Schema.optionalKey(JsonObject) });
const additions = <S extends Schema.Top>(row: S) => Schema.optionalKey(Schema.Array(row).annotate({ description: 'Items added and named as made.<as>' }));
const deep = (left: typeof JsonObject.Type, right: typeof JsonObject.Type): typeof JsonObject.Type => Record.union(left, right, (inner, outer) => (Schema.is(JsonObject)(inner) && Schema.is(JsonObject)(outer) ? deep(inner, outer) : outer));
const merged = (batch: typeof JsonObject.Type, row: typeof JsonObject.Type): typeof JsonObject.Type => {
    const opening: readonly string[] = ['application', 'document', 'open', 'template'];
    const replaced = opening.some((key) => key in row);
    return deep(
        Record.filter(batch, (_, key) => !(replaced && opening.includes(key))),
        row,
    );
};
const many = <S extends Schema.Top>(item: S) => Schema.Union([Schema.NonEmptyArray(item), item.pipe(Schema.decodeTo(Schema.NonEmptyArray(Schema.toType(item)), { decode: SchemaGetter.transform((one) => [one] as const), encode: SchemaGetter.forbiddenEncoding }))]);
const tags = (value: Schema.Json): readonly string[] =>
    Match.value(value).pipe(
        Match.when(Schema.is(Jsons), (items) => items.flatMap(tags)),
        Match.when(Schema.is(Tagged), ({ $ }) => [$]),
        Match.when(Schema.is(JsonObject), (held) => Object.values(held).flatMap(tags)),
        Match.orElse(() => []),
    );
const enumerators = (enumerations: Record.ReadonlyRecord<string, readonly string[]>) => {
    const declared = (tag: string) => Option.exists(Schema.decodeUnknownOption(Enumerated.parts)(tag), ([, enumeration, , name]) => Option.exists(Record.get(enumerations, enumeration), (listed) => listed.includes(name)));
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
const copied = Effect.fn('copied')(function* <P extends { readonly documents: readonly unknown[] }>(decoded: P, folder: string, product: string) {
    const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
    const copy = (open: string) => path.join(folder, path.basename(open));
    const relocated = yield* Effect.forEach(decoded.documents, (document): Effect.Effect<unknown, PlatformError.PlatformError> => (Schema.is(Opened)(document) ? Effect.as(fs.copyFile(path.join(product, document.open), copy(document.open)), { ...document, open: copy(document.open) }) : Effect.succeed(document)));
    return { precondition: { ...decoded, documents: relocated }, copies: decoded.documents.filter(Schema.is(Opened)).map(({ open }) => path.join(product, open)) };
});
const placings = <P extends Schema.Top, D extends Schema.Top & Schema.Decoder<unknown>, O extends Schema.Top & Schema.Decoder<unknown>, A extends Schema.Top & Schema.Decoder<unknown>>(Placed: P, sources: Sources<D, O, A>, made: Placings<P, D, O, A>) => {
    const placing = <S extends Schema.Top & Schema.Decoder<unknown>>(row: S, decode: (held: S['Type']) => P['Type']) => row.pipe(Schema.decodeTo(Schema.toType(Placed), { decode: SchemaGetter.transform(decode), encode: SchemaGetter.forbiddenEncoding }));
    return Schema.Union([placing(sources.document, made.document), placing(sources.open, made.open), placing(sources.application, made.application)]);
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Application, Assignments, added, additions, Copy, copied, documents, Enumerated, enumerators, many, member, merged, Open, placings, Steps };
