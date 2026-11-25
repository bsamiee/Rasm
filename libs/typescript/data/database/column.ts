import { Array, Data, Equal, Filter, Function, flow, Match, Option, Predicate, pipe, Record, Schema, SchemaAST, SchemaGetter, SchemaRepresentation, Struct } from 'effect';
import type { RelationOptions } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = Schema.Struct.Fields & Readonly<Record<PropertyKey, Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>>>;
type Where<F extends Fields> = { readonly [K in keyof F & string]?: null | Array.NonEmptyReadonlyArray<Extract<F[K]['Encoded'], string | number>> | { readonly not: null | Array.NonEmptyReadonlyArray<Extract<F[K]['Encoded'], string | number>> } };
type Kind = 'text' | 'uuid' | 'json' | 'instant' | 'integer' | 'decimal' | 'real' | 'boolean' | 'blob';
type Projection = Data.TaggedEnum<{
    Path: { readonly from: string; readonly path: Array.NonEmptyReadonlyArray<string>; readonly jsonPath: string };
    Where: { readonly where: readonly { readonly column: string; readonly negated: boolean; readonly values: Option.Option<Array.NonEmptyReadonlyArray<string | number>> }[] };
}>;
type Collection = Data.TaggedEnum<{
    Scalar: { readonly kind: Kind };
    Array: { readonly element: Collection };
    Map: { readonly value: Collection };
    Records: { readonly members: Readonly<Record<string, Collection>> };
}>;
type Limit = number | bigint | Date;
type Range = Readonly<Partial<Record<'minimum' | 'maximum' | 'minCodePoints' | 'maxCodePoints', Limit | undefined>>> & Readonly<Partial<Record<'exclusiveMinimum' | 'exclusiveMaximum', Limit | true | undefined>>>;

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
    static readonly Uuid = Schema.String.check(Schema.isUUID(7), Schema.isLowercased());
    static readonly conditions = (where: Where<Fields>, rename: (name: string) => string) =>
        Record.collect(Record.filter(where, Predicate.isNotUndefined), (field, rule) => ({
            column: rename(field),
            ...Match.value(rule).pipe(
                Match.when({ not: null }, () => ({ negated: true, values: Option.none() })),
                Match.when({ not: Match.defined }, ({ not }) => ({ negated: true, values: Option.some(not) })),
                Match.orElse((values) => ({ negated: false, values: Option.fromNullOr(values) })),
            ),
        }));
    static readonly #members = (node: SchemaRepresentation.Representation) =>
        Match.value(node).pipe(
            Match.tag('Literal', ({ literal }) => [literal]),
            Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
            Match.option,
        );
    static readonly make = <F extends Fields>(field: keyof F & string, schema: F[keyof F & string], { defaults, projections, renamed, retype }: Pick<RelationOptions<F>, 'defaults' | 'projections' | 'renamed' | 'retype'>, retired: readonly string[], rename: (name: string) => string) => {
        const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] =>
            Match.value(node).pipe(
                Match.tag('Union', ({ types }) => types.flatMap(flat)),
                Match.orElse((leaf) => [leaf]),
            );
        const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: Function.constUndefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
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
                pipe(
                    nodes,
                    Array.flatMap((node) => (node._tag === 'Reference' ? [] : node.checks.flatMap(leaves))),
                    Array.flatMapNullishOr(Struct.get('representation')),
                    Array.flatMap((check) => (check.id === id ? Option.toArray(Schema.decodeOption(Schema.toCodecJson(payloadSchema))(check.payload)) : [])),
                );
        const textual = Predicate.isTagged(values[0], 'String') && decoded.some(Predicate.or(Predicate.isTagged('Number'), Predicate.isTagged('BigInt')));
        const [stored, typed] = [payloads(values), payloads(decoded)];
        const numbers = textual ? typed : stored;
        const has = flow(stored, Array.isReadonlyArrayNonEmpty);
        const side = (subject: 'value' | 'codePoints', op: '>' | '<', inclusive: Limit | undefined, exclusive: Limit | true | undefined) =>
            Array.fromNullishOr(exclusive === undefined || exclusive === true ? inclusive : exclusive).map((value) => Column.Bound.Compare({ op: exclusive === undefined ? (`${op}=` as const) : op, value, subject }));
        const ranges = (read: typeof stored, subject: 'value' | 'codePoints', revivers: readonly Pick<SchemaRepresentation.FilterReviver<Range>, 'id' | 'payloadSchema'>[]) =>
            revivers.flatMap(read).flatMap((range) => [...side(subject, '>', range.minimum ?? range.minCodePoints, range.exclusiveMinimum), ...side(subject, '<', range.maximum ?? range.maxCodePoints, range.exclusiveMaximum)]);
        const encodings = values.flatMap(
            Match.type<SchemaRepresentation.Representation>().pipe(
                Match.tag('Literal', ({ literal }) => [typeof literal]),
                Match.tag('Enum', ({ enums }) => enums.map(([, member]) => typeof member)),
                Match.tag('String', 'TemplateLiteral', () => ['string']),
                Match.tag('Number', () => ['number']),
                Match.tag('Declaration', 'Arrays', () => ['blob']),
                Match.orElse(({ _tag }): readonly string[] => [_tag]),
            ),
        );
        const boolean = decoded.some(Predicate.isTagged('Boolean'));
        const literals = boolean ? [] : Option.match(Option.all(values.map(Column.#members)), { onNone: () => [], onSome: Array.flatten });
        const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
        const integer = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isIntReviver)) || (numeric && literals.every(Number.isInteger));
        const finite = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isFiniteReviver));
        const number = Predicate.isTagged(values[0], 'Number') || (textual && decoded.some(Predicate.isTagged('Number')));
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
                Match.orElse(() => Option.none()),
            );
        return new Column({
            name,
            schema,
            kind,
            collection: kind === 'json' && Predicate.isTupleOf(decoded, 1) ? Option.filter(collected(Array.headNonEmpty(decoded)), Predicate.not(Column.Collection.$is('Scalar'))) : Option.none(),
            textual,
            ascii: has(SchemaRepresentation.isAsciiReviver),
            refusal: Match.value({ mixed: Array.dedupe(encodings).length > 1, unbounded: number && !(finite || integer) }).pipe(
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
            retype: retype?.includes(field) === true,
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Collection, Column, type Fields, type Kind, type Projection, type Where };
