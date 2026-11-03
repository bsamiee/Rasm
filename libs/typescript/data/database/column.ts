import { Array, Data, Equal, Filter, flow, Match, Option, Record, Schema, SchemaAST, SchemaGetter, SchemaRepresentation, String, Struct } from 'effect';
import type { RelationOptions } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>;
}
type Field<F> = keyof F & string;
type Constants<S extends Fields[string]> = Array.NonEmptyReadonlyArray<Extract<S['Encoded'], string | number>>;
type Where<F extends Fields> = { readonly [K in Field<F>]?: null | Constants<F[K]> | { readonly not: null | Constants<F[K]> } };
type Kind = 'text' | 'uuid' | 'json' | 'instant' | 'integer' | 'decimal' | 'real' | 'boolean' | 'blob';
type Projection = Data.TaggedEnum<{
    Path: { readonly from: string; readonly path: Array.NonEmptyReadonlyArray<string>; readonly jsonPath: string };
    Where: { readonly where: readonly { readonly column: string; readonly predicate: Exclude<Where<Fields>[string], undefined> }[] };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const uuidVersion = 7;
const byteMaximum = 255;

// --- [MODELS] --------------------------------------------------------------------------

const Default = Schema.Union([Schema.String, Schema.Finite]);
class Column extends Data.Class<{
    readonly field: string;
    readonly name: string;
    readonly schema: Fields[string];
    readonly kind: Kind;
    readonly textual: boolean;
    readonly ascii: boolean;
    readonly refusal: Option.Option<'unbounded' | 'mixed'>;
    readonly nullable: boolean;
    readonly literals: readonly SchemaAST.LiteralValue[];
    readonly bounds: readonly Data.TaggedEnum<{
        Compare: { readonly op: '>' | '>=' | '<' | '<='; readonly value: number | bigint | Date; readonly subject: 'value' | 'codePoints' };
        MultipleOf: { readonly divisor: number };
        Cased: { readonly case: 'lower' | 'upper' };
    }>[];
    readonly default: Option.Option<string | number>;
    readonly projection: Option.Option<Projection>;
    readonly renamedFrom: Option.Option<string>;
    readonly generation: number;
    readonly retype: boolean;
}> {
    static readonly transforms = { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel, transformJson: false };
    static readonly Projection = Data.taggedEnum<Projection>();
    static readonly Bound = Data.taggedEnum<Column['bounds'][number]>();
    static readonly Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: byteMaximum })))]).pipe(Schema.decodeTo(Schema.Uint8Array, { decode: SchemaGetter.transform((bytes) => new Uint8Array(bytes)), encode: SchemaGetter.passthroughSubtype() }));
    static readonly Uuid = Schema.String.check(Schema.isUUID(uuidVersion), Schema.isLowercased());
    static readonly conditions = (where: Where<Fields>) => Record.collect(where, (field, predicate) => (predicate === undefined ? [] : [{ column: String.camelToSnake(field), predicate }])).flat();
    static readonly make = <F extends Fields>(field: Field<F>, schema: F[Field<F>], { defaults, projections, renamed, retype }: Pick<RelationOptions<F>, 'defaults' | 'projections' | 'renamed' | 'retype'>, retired: readonly string[]) => {
        const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
        const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
        const [nulls, values] = walk(schema.ast);
        const [, decoded] = walk(SchemaAST.toType(schema.ast));
        const leaves = (check: SchemaRepresentation.Check): readonly SchemaRepresentation.Check[] => (check._tag === 'FilterGroup' ? check.checks.flatMap(leaves) : [check]);
        const payloads =
            (nodes: readonly SchemaRepresentation.Representation[]) =>
            <P>({ id, payloadSchema }: SchemaRepresentation.FilterReviver<P>) =>
                Array.flatMapNullishOr(
                    nodes.flatMap((node) => (node._tag === 'Reference' ? [] : node.checks.flatMap(leaves))),
                    Struct.get('representation'),
                ).flatMap((check) => (check.id === id ? Option.toArray(Schema.decodeOption(Schema.toCodecJson(payloadSchema))(check.payload)) : []));
        const textual = values[0]?._tag === 'String' && decoded.some(({ _tag }) => _tag === 'Number' || _tag === 'BigInt');
        const [stored, typed] = [payloads(values), payloads(decoded)];
        const numbers = textual ? typed : stored;
        const has = flow(stored, Array.isReadonlyArrayNonEmpty);
        const limits = <P, B>(read: typeof stored, reviver: SchemaRepresentation.FilterReviver<P>, ...bounds: ReadonlyArray<(payload: P) => B>) => read(reviver).flatMap((payload) => bounds.map((bound) => bound(payload)));
        const compare = (subject: 'value' | 'codePoints') => (op: '>' | '>=' | '<' | '<=', limit: number | bigint | Date) => Column.Bound.Compare({ op, value: limit, subject });
        const [value, points] = [compare('value'), compare('codePoints')];
        const lower =
            <V extends number | bigint | Date, B>(bound: (op: '>' | '>=', limit: V) => B) =>
            ({ minimum, exclusiveMinimum }: { readonly minimum: V; readonly exclusiveMinimum?: true | undefined }) =>
                bound(exclusiveMinimum === true ? '>' : '>=', minimum);
        const upper =
            <V extends number | bigint | Date, B>(bound: (op: '<' | '<=', limit: V) => B) =>
            ({ maximum, exclusiveMaximum }: { readonly maximum: V; readonly exclusiveMaximum?: true | undefined }) =>
                bound(exclusiveMaximum === true ? '<' : '<=', maximum);
        const storage = (node: SchemaRepresentation.Representation): readonly string[] =>
            Match.value(node).pipe(
                Match.tag('Literal', ({ literal: member }) => [typeof member]),
                Match.tag('Enum', ({ enums }) => enums.map(([, member]) => typeof member)),
                Match.tag('String', 'TemplateLiteral', () => ['string']),
                Match.tag('Number', () => ['number']),
                Match.orElse(({ _tag }) => [_tag]),
            );
        const boolean = decoded.some(({ _tag }) => _tag === 'Boolean');
        const literals =
            boolean || !values.every(({ _tag }) => _tag === 'Literal' || _tag === 'Enum')
                ? []
                : values.flatMap((node) =>
                      Match.value(node).pipe(
                          Match.tag('Literal', ({ literal: member }) => [member]),
                          Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
                          Match.orElse(() => []),
                      ),
                  );
        const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
        const integer = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isIntReviver)) || (numeric && literals.every(Number.isInteger));
        const finite = Array.isReadonlyArrayNonEmpty(numbers(SchemaRepresentation.isFiniteReviver));
        const number = values[0]?._tag === 'Number' || (textual && decoded.some(({ _tag }) => _tag === 'Number'));
        const name = String.camelToSnake(field);
        return new Column({
            field,
            name,
            schema,
            kind: Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, number, integer, uuid: has(SchemaRepresentation.isUUIDReviver) }).pipe(
                Match.withReturnType<Kind>(),
                Match.when({ node: { _tag: 'String', content: { mediaType: 'application/json' } } }, () => 'json'),
                Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'BigInt' } }, () => 'integer'),
                Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: Match.is(SchemaRepresentation.DateReviver.id, SchemaRepresentation.DateTimeUtcReviver.id) } } }, () => 'instant'),
                Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: SchemaRepresentation.BigDecimalReviver.id } } }, () => 'decimal'),
                Match.when({ node: { _tag: 'String' }, uuid: true }, () => 'uuid'),
                Match.when({ boolean: true }, () => 'boolean'),
                Match.whenOr({ number: true, integer: true }, { numeric: true, integer: true }, () => 'integer'),
                Match.whenOr({ number: true }, { numeric: true }, () => 'real'),
                Match.whenOr({ node: { _tag: 'Declaration' } }, { node: { _tag: 'Arrays' } }, () => 'blob'),
                Match.orElse(() => 'text'),
            ),
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
                limits(numbers, SchemaRepresentation.isGreaterThanReviver, ({ exclusiveMinimum }) => value('>', exclusiveMinimum)),
                limits(numbers, SchemaRepresentation.isGreaterThanOrEqualToReviver, lower(value)),
                limits(numbers, SchemaRepresentation.isLessThanReviver, ({ exclusiveMaximum }) => value('<', exclusiveMaximum)),
                limits(numbers, SchemaRepresentation.isLessThanOrEqualToReviver, upper(value)),
                limits(numbers, SchemaRepresentation.isBetweenReviver, lower(value), upper(value)),
                integer ? limits(numbers, SchemaRepresentation.isMultipleOfReviver, ({ divisor }) => Column.Bound.MultipleOf({ divisor })) : [],
                limits(stored, SchemaRepresentation.isMinCodePointsReviver, ({ minCodePoints }) => points('>=', minCodePoints)),
                limits(stored, SchemaRepresentation.isMaxCodePointsReviver, ({ maxCodePoints }) => points('<=', maxCodePoints)),
                limits(stored, SchemaRepresentation.isBetweenCodePointsReviver, lower(points), upper(points)),
                limits(typed, SchemaRepresentation.isGreaterThanBigIntReviver, ({ exclusiveMinimum }) => value('>', exclusiveMinimum)),
                limits(typed, SchemaRepresentation.isGreaterThanOrEqualToBigIntReviver, lower(value)),
                limits(typed, SchemaRepresentation.isLessThanBigIntReviver, ({ exclusiveMaximum }) => value('<', exclusiveMaximum)),
                limits(typed, SchemaRepresentation.isLessThanOrEqualToBigIntReviver, upper(value)),
                limits(typed, SchemaRepresentation.isBetweenBigIntReviver, lower(value), upper(value)),
                limits(typed, SchemaRepresentation.isGreaterThanDateReviver, ({ exclusiveMinimum }) => value('>', exclusiveMinimum)),
                limits(typed, SchemaRepresentation.isGreaterThanOrEqualToDateReviver, lower(value)),
                limits(typed, SchemaRepresentation.isLessThanDateReviver, ({ exclusiveMaximum }) => value('<', exclusiveMaximum)),
                limits(typed, SchemaRepresentation.isLessThanOrEqualToDateReviver, upper(value)),
                limits(typed, SchemaRepresentation.isBetweenDateReviver, lower(value), upper(value)),
                limits(stored, SchemaRepresentation.isLowercasedReviver, () => Column.Bound.Cased({ case: 'lower' })),
                limits(stored, SchemaRepresentation.isUppercasedReviver, () => Column.Bound.Cased({ case: 'upper' })),
            ].flat(),
            default: Schema.decodeUnknownOption(Default)(defaults?.[field]),
            projection: Option.map(Option.fromUndefinedOr(projections?.[field]), (projection: NonNullable<NonNullable<RelationOptions<Fields>['projections']>[string]>) =>
                Match.value(projection).pipe(
                    Match.when({ where: Match.defined }, ({ where }) => Column.Projection.Where({ where: Column.conditions(where) })),
                    Match.orElse(({ from, path }) => Column.Projection.Path({ from: String.camelToSnake(from), path, jsonPath: ['$', ...path.map((part) => JSON.stringify(part))].join('.') })),
                ),
            ),
            renamedFrom: Option.map(Option.fromUndefinedOr(renamed?.[field]), String.camelToSnake),
            generation: Array.countBy(retired, Equal.equals(name)),
            retype: Option.exists(Option.fromUndefinedOr(retype), Array.contains(field)),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Column, type Field, type Fields, type Kind, type Projection, type Where };
