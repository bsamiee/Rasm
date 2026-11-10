import { Array, Data, Equal, Filter, flow, Match, Option, Predicate, Record, Schema, SchemaAST, SchemaGetter, SchemaRepresentation, Struct } from 'effect';
import type { RelationOptions } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>;
}
type FieldName<F> = keyof F & string;
type Constants<S extends Fields[string]> = Array.NonEmptyReadonlyArray<Extract<S['Encoded'], string | number>>;
type Where<F extends Fields> = { readonly [K in FieldName<F>]?: null | Constants<F[K]> | { readonly not: null | Constants<F[K]> } };
type Kind = 'text' | 'uuid' | 'json' | 'instant' | 'integer' | 'decimal' | 'real' | 'boolean' | 'blob';
type Projection = Data.TaggedEnum<{
    Path: { readonly from: string; readonly path: Array.NonEmptyReadonlyArray<string>; readonly jsonPath: string };
    Where: { readonly where: readonly { readonly column: string; readonly negated: boolean; readonly values: Option.Option<Constants<Fields[string]>> }[] };
}>;
type Collection = Data.TaggedEnum<{
    Scalar: { readonly kind: Kind };
    Array: { readonly element: Collection };
    Map: { readonly value: Collection };
    Records: { readonly members: { readonly [name: string]: Collection } };
}>;
type Limit = number | bigint | Date;
type Range = { readonly [K in 'minimum' | 'maximum' | 'minCodePoints' | 'maxCodePoints']?: Limit | undefined } & { readonly [K in 'exclusiveMinimum' | 'exclusiveMaximum']?: Limit | true | undefined };

// --- [CONSTANTS] -----------------------------------------------------------------------

const uuidVersion = 7;

// --- [MODELS] --------------------------------------------------------------------------

const Default = Schema.Union([Schema.String, Schema.Finite]);
class Column extends Data.Class<{
    readonly name: string;
    readonly schema: Fields[string];
    readonly kind: Kind;
    readonly collection: Option.Option<Collection>;
    readonly textual: boolean;
    readonly ascii: boolean;
    readonly refusal: Option.Option<'unbounded' | 'mixed'>;
    readonly nullable: boolean;
    readonly literals: readonly SchemaAST.LiteralValue[];
    readonly bounds: readonly Data.TaggedEnum<{
        Compare: { readonly op: '>' | '>=' | '<' | '<='; readonly value: Limit; readonly subject: 'value' | 'codePoints' };
        MultipleOf: { readonly divisor: number };
        Cased: { readonly case: 'lower' | 'upper' };
    }>[];
    readonly default: Option.Option<string | number>;
    readonly projection: Option.Option<Projection>;
    readonly renamedFrom: Option.Option<string>;
    readonly generation: number;
    readonly retype: boolean;
}> {
    static readonly Projection = Data.taggedEnum<Projection>();
    static readonly Collection = Data.taggedEnum<Collection>();
    static readonly Bound = Data.taggedEnum<Column['bounds'][number]>();
    static readonly Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: 0xff })))]).pipe(Schema.decodeTo(Schema.Uint8Array, { decode: SchemaGetter.transform((bytes) => new Uint8Array(bytes)), encode: SchemaGetter.passthroughSubtype() }));
    static readonly Uuid = Schema.String.check(Schema.isUUID(uuidVersion), Schema.isLowercased());
    static readonly conditions = (where: Where<Fields>, rename: (name: string) => string) =>
        Record.collect(Record.filter(where, Predicate.isNotUndefined), (field, rule) => ({
            column: rename(field),
            ...Match.value(rule).pipe(
                Match.when({ not: null }, () => ({ negated: true, values: Option.none() })),
                Match.when({ not: Match.defined }, ({ not }) => ({ negated: true, values: Option.some(not) })),
                Match.orElse((values) => ({ negated: false, values: Option.fromNullOr(values) })),
            ),
        }));
    static readonly make = <F extends Fields>(field: FieldName<F>, schema: F[FieldName<F>], { defaults, projections, renamed, retype }: Pick<RelationOptions<F>, 'defaults' | 'projections' | 'renamed' | 'retype'>, retired: readonly string[], rename: (name: string) => string) => {
        const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] =>
            Match.value(node).pipe(
                Match.tag('Union', ({ types }) => types.flatMap(flat)),
                Match.orElse((leaf) => [leaf]),
            );
        const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
        const [nulls, values] = walk(schema.ast);
        const [, decoded] = walk(SchemaAST.toType(schema.ast));
        const leaves = (check: SchemaRepresentation.Check): readonly SchemaRepresentation.Check[] =>
            Match.value(check).pipe(
                Match.tag('FilterGroup', ({ checks }) => checks.flatMap(leaves)),
                Match.orElse((leaf) => [leaf]),
            );
        const payloads =
            (nodes: readonly SchemaRepresentation.Representation[]) =>
            <P>({ id, payloadSchema }: Pick<SchemaRepresentation.FilterReviver<P>, 'id' | 'payloadSchema'>) =>
                Array.flatMapNullishOr(
                    nodes.flatMap(
                        Match.type<SchemaRepresentation.Representation>().pipe(
                            Match.tag('Reference', () => []),
                            Match.orElse(({ checks }) => checks.flatMap(leaves)),
                        ),
                    ),
                    Struct.get('representation'),
                ).flatMap((check) => (check.id === id ? Option.toArray(Schema.decodeOption(Schema.toCodecJson(payloadSchema))(check.payload)) : []));
        const textual = Option.exists(Array.head(values), Predicate.isTagged('String')) && decoded.some(Predicate.or(Predicate.isTagged('Number'), Predicate.isTagged('BigInt')));
        const [stored, typed] = [payloads(values), payloads(decoded)];
        const numbers = textual ? typed : stored;
        const has = flow(stored, Array.isReadonlyArrayNonEmpty);
        const side = (subject: 'value' | 'codePoints', op: '>' | '<', inclusive: Limit | undefined, exclusive: Limit | true | undefined) =>
            Array.fromNullishOr(exclusive === undefined || exclusive === true ? inclusive : exclusive).map((value) => Column.Bound.Compare({ op: exclusive === undefined ? (`${op}=` as const) : op, value, subject }));
        const ranges = (read: typeof stored, subject: 'value' | 'codePoints', revivers: readonly Pick<SchemaRepresentation.FilterReviver<Range>, 'id' | 'payloadSchema'>[]) =>
            revivers.flatMap(read).flatMap((range) => [...side(subject, '>', range.minimum ?? range.minCodePoints, range.exclusiveMinimum), ...side(subject, '<', range.maximum ?? range.maxCodePoints, range.exclusiveMaximum)]);
        const members = (node: SchemaRepresentation.Representation) =>
            Match.value(node).pipe(
                Match.tag('Literal', ({ literal }) => [literal]),
                Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
                Match.option,
            );
        const storage = (node: SchemaRepresentation.Representation): readonly string[] =>
            Option.match(members(node), {
                onSome: Array.map((member) => typeof member),
                onNone: () =>
                    Match.value(node._tag).pipe(
                        Match.when(Match.is('String', 'TemplateLiteral'), () => ['string']),
                        Match.when('Number', () => ['number']),
                        Match.when(Match.is('Declaration', 'Arrays'), () => ['blob']),
                        Match.orElse((tag) => [tag]),
                    ),
            });
        const boolean = decoded.some(Predicate.isTagged('Boolean'));
        const literals = boolean ? [] : Option.match(Option.all(values.map(members)), { onNone: () => [], onSome: Array.flatten });
        const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
        const integer = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isIntReviver)) || (numeric && literals.every(Number.isInteger));
        const finite = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isFiniteReviver));
        const number = Option.exists(Array.head(values), Predicate.isTagged('Number')) || (textual && decoded.some(Predicate.isTagged('Number')));
        const name = rename(field);
        const instant = { _tag: 'Declaration', representation: { id: Match.is(SchemaRepresentation.DateReviver.id, SchemaRepresentation.DateTimeUtcReviver.id) } } as const;
        const kind = Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, number, integer, uuid: has(SchemaRepresentation.isUUIDReviver) }).pipe(
            Match.withReturnType<Kind>(),
            Match.when({ node: { _tag: 'String', content: { mediaType: 'application/json' } } }, () => 'json'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'BigInt' } }, () => 'integer'),
            Match.when({ node: { _tag: 'String' }, decoded: instant }, () => 'instant'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: SchemaRepresentation.BigDecimalReviver.id } } }, () => 'decimal'),
            Match.when({ node: { _tag: 'String' }, uuid: true }, () => 'uuid'),
            Match.when({ boolean: true }, () => 'boolean'),
            Match.whenOr({ number: true, integer: true }, { numeric: true, integer: true }, () => 'integer'),
            Match.whenOr({ number: true }, { numeric: true }, () => 'real'),
            Match.whenOr({ node: { _tag: 'Declaration' } }, { node: { _tag: 'Arrays' } }, () => 'blob'),
            Match.orElse(() => 'text'),
        );
        const scalar = (held: Kind) => Option.some(Column.Collection.Scalar({ kind: held }));
        const collected = (node: SchemaRepresentation.Representation): Option.Option<Collection> =>
            Match.value(node).pipe(
                Match.withReturnType<Option.Option<Collection>>(),
                Match.tag('String', () => scalar('text')),
                Match.tag('Boolean', () => scalar('boolean')),
                Match.tag('BigInt', () => scalar('integer')),
                Match.tag('Number', (leaf) => scalar(Array.isReadonlyArrayNonEmpty(payloads([leaf])(SchemaRepresentation.isIntReviver)) ? 'integer' : 'real')),
                Match.when(instant, () => scalar('instant')),
                Match.when({ _tag: 'Arrays', elements: [], rest: [{ _tag: 'Objects', indexSignatures: [] }] }, ({ rest: [{ propertySignatures }] }) =>
                    Option.map(Option.all(Record.fromIterableWith(propertySignatures, (member) => [member.name.toString(), collected(member.type)])), (named) => Column.Collection.Records({ members: named })),
                ),
                Match.when({ _tag: 'Arrays', elements: [], rest: [Match.any] }, ({ rest: [item] }) => Option.map(collected(item), (element) => Column.Collection.Array({ element }))),
                Match.when({ _tag: 'Objects', propertySignatures: [], indexSignatures: [{ parameter: { _tag: 'String' } }] }, ({ indexSignatures: [{ type }] }) => Option.map(collected(type), (value) => Column.Collection.Map({ value }))),
                Match.option,
                Option.flatten,
            );
        return new Column({
            name,
            schema,
            kind,
            collection: kind === 'json' && decoded.length === 1 ? Option.filter(Option.flatMap(Array.head(decoded), collected), Predicate.not(Column.Collection.$is('Scalar'))) : Option.none(),
            textual,
            ascii: has(SchemaRepresentation.isAsciiReviver),
            refusal: Match.value({ mixed: Array.dedupe(values.flatMap(storage)).length > 1, unbounded: number && !(finite || integer) }).pipe(
                Match.when({ mixed: true }, () => 'mixed' as const),
                Match.when({ unbounded: true }, () => 'unbounded' as const),
                Match.option,
            ),
            nullable: Array.isReadonlyArrayNonEmpty(nulls),
            literals,
            bounds: [
                ...ranges(numbers, 'value', [SchemaRepresentation.isGreaterThanReviver, SchemaRepresentation.isGreaterThanOrEqualToReviver, SchemaRepresentation.isLessThanReviver, SchemaRepresentation.isLessThanOrEqualToReviver, SchemaRepresentation.isBetweenReviver]),
                ...(integer ? numbers(SchemaRepresentation.isMultipleOfReviver).map(Column.Bound.MultipleOf) : []),
                ...ranges(stored, 'codePoints', [SchemaRepresentation.isMinCodePointsReviver, SchemaRepresentation.isMaxCodePointsReviver, SchemaRepresentation.isBetweenCodePointsReviver]),
                ...ranges(typed, 'value', [SchemaRepresentation.isGreaterThanBigIntReviver, SchemaRepresentation.isGreaterThanOrEqualToBigIntReviver, SchemaRepresentation.isLessThanBigIntReviver, SchemaRepresentation.isLessThanOrEqualToBigIntReviver, SchemaRepresentation.isBetweenBigIntReviver]),
                ...ranges(typed, 'value', [SchemaRepresentation.isGreaterThanDateReviver, SchemaRepresentation.isGreaterThanOrEqualToDateReviver, SchemaRepresentation.isLessThanDateReviver, SchemaRepresentation.isLessThanOrEqualToDateReviver, SchemaRepresentation.isBetweenDateReviver]),
                ...stored(SchemaRepresentation.isLowercasedReviver).map(() => Column.Bound.Cased({ case: 'lower' })),
                ...stored(SchemaRepresentation.isUppercasedReviver).map(() => Column.Bound.Cased({ case: 'upper' })),
            ],
            default: Schema.decodeUnknownOption(Default)(defaults?.[field]),
            projection: Option.map(
                Option.fromUndefinedOr(projections?.[field]),
                Match.type<NonNullable<NonNullable<RelationOptions<Fields>['projections']>[string]>>().pipe(
                    Match.when({ where: Match.defined }, ({ where }) => Column.Projection.Where({ where: Column.conditions(where, rename) })),
                    Match.orElse(({ from, path }) => Column.Projection.Path({ from: rename(from), path, jsonPath: ['$', ...path.map((part) => JSON.stringify(part))].join('.') })),
                ),
            ),
            renamedFrom: Option.map(Option.fromUndefinedOr(renamed?.[field]), rename),
            generation: Array.countBy(retired, Equal.equals(name)),
            retype: Option.exists(Option.fromUndefinedOr(retype), Array.contains(field)),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Collection, Column, type FieldName, type Fields, type Kind, type Projection, type Where };
