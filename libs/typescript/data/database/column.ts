import { Array, Equal, Filter, Function, flow, Match, Option, Schema, SchemaAST, SchemaRepresentation, SchemaTransformation, Struct } from 'effect';
import { transforms } from '#/lexical';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>;
}
type Field<F> = keyof F & string;
type Where<F> = { readonly [K in Field<F>]?: null | Array.NonEmptyReadonlyArray<string | number> | { readonly not: null | Array.NonEmptyReadonlyArray<string | number> } };
interface ColumnOptions<F extends Fields> {
    readonly defaults?: { readonly [K in keyof F]?: Exclude<F[K]['Encoded'], Uint8Array | readonly number[]> };
    readonly projections?: { readonly [K in Field<F>]?: { readonly from: Field<F>; readonly path: Array.NonEmptyReadonlyArray<string> } | { readonly where: Where<F> } };
    readonly renamed?: { readonly [K in Field<F>]?: string };
    readonly retype?: readonly Field<F>[];
}
type Column = ReturnType<typeof column>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const uuidVersion = 7;
const dataTypes = {
    sqlite: { text: 'TEXT', uuid: 'TEXT', json: 'TEXT', instant: 'TEXT', integer: 'INTEGER', int64: 'INTEGER', decimal: 'TEXT', real: 'REAL', number: 'REAL', boolean: 'INTEGER', blob: 'BLOB' },
    pg: { text: 'text', uuid: 'uuid', json: 'jsonb', instant: 'timestamptz(3)', integer: 'bigint', int64: 'bigint', decimal: 'numeric', real: 'double precision', number: 'double precision', boolean: 'boolean', blob: 'bytea' },
} as const;

// --- [MODELS] --------------------------------------------------------------------------

const Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int)]).pipe(Schema.decodeTo(Schema.Uint8Array, SchemaTransformation.transform<Uint8Array, Uint8Array | readonly number[]>({ decode: (bytes) => new Uint8Array(bytes), encode: Function.identity })));
const Uuid = Schema.String.check(Schema.isUUID(uuidVersion), Schema.isLowercased());

// --- [OPERATIONS] ----------------------------------------------------------------------

const conditions = <F extends Fields>(where: Where<F>) => Object.entries(where).flatMap(([field, predicate]) => (predicate === undefined ? [] : [{ column: transforms.transformQueryNames(field), predicate }]));
const column = <F extends Fields>(field: Field<F>, schema: F[Field<F>], { defaults, projections, renamed, retype }: ColumnOptions<F>, retired: readonly string[]) => {
    const flat = (node: SchemaRepresentation.Representation): readonly SchemaRepresentation.Representation[] => (node._tag === 'Union' ? node.types.flatMap(flat) : [node]);
    const walk = (ast: SchemaAST.AST) => Array.partition(flat(SchemaRepresentation.toRepresentation(ast, { referencePolicy: () => undefined }).representation), Filter.tagged<SchemaRepresentation.Representation>()('Null'));
    const [nulls, values] = walk(schema.ast);
    const [, decoded] = walk(SchemaAST.toType(schema.ast));
    const leaves = (check: SchemaRepresentation.Check): readonly SchemaRepresentation.Check[] => (check._tag === 'FilterGroup' ? check.checks.flatMap(leaves) : [check]);
    const payloads =
        (nodes: readonly SchemaRepresentation.Representation[]) =>
        <P>({ id, payloadSchema }: SchemaRepresentation.FilterReviver<P>) =>
            Array.flatMapNullishOr(
                nodes.flatMap((node) => ('checks' in node ? node.checks.flatMap(leaves) : [])),
                Struct.get('representation'),
            ).flatMap((check) => (check.id === id ? Option.toArray(Schema.decodeOption(Schema.toCodecJson(payloadSchema))(check.payload)) : []));
    const [stored, typed] = [payloads(values), payloads(decoded)];
    const has = flow(stored, Array.isReadonlyArrayNonEmpty);
    const limits = <P>(read: typeof stored, reviver: SchemaRepresentation.FilterReviver<P>, length: boolean, ...bounds: ReadonlyArray<(payload: P) => readonly [string, number | bigint]>) => read(reviver).flatMap((payload) => bounds.map((bound) => bound(payload)).map(([op, value]) => ({ op, value, length })));
    const lower = ({ minimum, exclusiveMinimum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type | typeof SchemaRepresentation.isBetweenBigIntReviver.payloadSchema.Type, 'minimum' | 'exclusiveMinimum'>) => [exclusiveMinimum === true ? '>' : '>=', minimum] as const;
    const upper = ({ maximum, exclusiveMaximum }: Pick<typeof SchemaRepresentation.isBetweenReviver.payloadSchema.Type | typeof SchemaRepresentation.isBetweenBigIntReviver.payloadSchema.Type, 'maximum' | 'exclusiveMaximum'>) => [exclusiveMaximum === true ? '<' : '<=', maximum] as const;
    const boolean = decoded.some(({ _tag }) => _tag === 'Boolean');
    const literals = values.flatMap((node) =>
        Match.value(node).pipe(
            Match.tag('Literal', ({ literal: member }) => (boolean ? [] : [member])),
            Match.tag('Enum', ({ enums }) => enums.map(([, member]) => member)),
            Match.orElse(() => []),
        ),
    );
    const numeric = Array.isReadonlyArrayNonEmpty(literals) && literals.every((member) => typeof member === 'number');
    const integer = has(SchemaRepresentation.isIntReviver);
    const name = transforms.transformQueryNames(field);
    return {
        field,
        name,
        schema,
        kind: Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, integer: integer || (numeric && literals.every(Number.isInteger)), finite: has(SchemaRepresentation.isFiniteReviver), uuid: has(SchemaRepresentation.isUUIDReviver) }).pipe(
            Match.withReturnType<keyof typeof dataTypes.sqlite>(),
            Match.when({ node: { _tag: 'String', annotations: { contentMediaType: 'application/json' } } }, () => 'json'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'BigInt' } }, () => 'int64'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: Match.is(SchemaRepresentation.DateReviver.id, SchemaRepresentation.DateTimeUtcReviver.id) } } }, () => 'instant'),
            Match.when({ node: { _tag: 'String' }, decoded: { _tag: 'Declaration', representation: { id: SchemaRepresentation.BigDecimalReviver.id } } }, () => 'decimal'),
            Match.when({ node: { _tag: 'String' }, uuid: true }, () => 'uuid'),
            Match.when({ boolean: true }, () => 'boolean'),
            Match.whenOr({ node: { _tag: 'Number' }, integer: true }, { numeric: true, integer: true }, () => 'integer'),
            Match.whenOr({ node: { _tag: 'Number' }, finite: true }, { numeric: true }, () => 'real'),
            Match.when({ node: { _tag: 'Number' } }, () => 'number'),
            Match.whenOr({ node: { _tag: 'Declaration' } }, { node: { _tag: 'Arrays' } }, () => 'blob'),
            Match.orElse(() => 'text'),
        ),
        nullable: Array.isReadonlyArrayNonEmpty(nulls),
        literals,
        bounds: [
            limits(stored, SchemaRepresentation.isGreaterThanReviver, false, ({ exclusiveMinimum }) => ['>', exclusiveMinimum]),
            limits(stored, SchemaRepresentation.isGreaterThanOrEqualToReviver, false, lower),
            limits(stored, SchemaRepresentation.isLessThanReviver, false, ({ exclusiveMaximum }) => ['<', exclusiveMaximum]),
            limits(stored, SchemaRepresentation.isLessThanOrEqualToReviver, false, upper),
            limits(stored, SchemaRepresentation.isBetweenReviver, false, lower, upper),
            integer ? limits(stored, SchemaRepresentation.isMultipleOfReviver, false, ({ divisor }) => ['%', divisor]) : [],
            limits(stored, SchemaRepresentation.isMinCodePointsReviver, true, ({ minCodePoints }) => ['>=', minCodePoints]),
            limits(stored, SchemaRepresentation.isMaxCodePointsReviver, true, ({ maxCodePoints }) => ['<=', maxCodePoints]),
            limits(stored, SchemaRepresentation.isBetweenCodePointsReviver, true, lower, upper),
            limits(typed, SchemaRepresentation.isGreaterThanBigIntReviver, false, ({ exclusiveMinimum }) => ['>', exclusiveMinimum]),
            limits(typed, SchemaRepresentation.isGreaterThanOrEqualToBigIntReviver, false, lower),
            limits(typed, SchemaRepresentation.isLessThanBigIntReviver, false, ({ exclusiveMaximum }) => ['<', exclusiveMaximum]),
            limits(typed, SchemaRepresentation.isLessThanOrEqualToBigIntReviver, false, upper),
            limits(typed, SchemaRepresentation.isBetweenBigIntReviver, false, lower, upper),
        ].flat(),
        fallback: Option.fromNullishOr(defaults?.[field]),
        projection: Option.map(Option.fromUndefinedOr(projections?.[field]), (projection) => ('where' in projection ? { _tag: 'Where' as const, where: conditions(projection.where) } : { _tag: 'Path' as const, from: transforms.transformQueryNames(projection.from), path: projection.path })),
        earlier: Option.map(Option.fromUndefinedOr(renamed?.[field]), transforms.transformQueryNames),
        incarnation: Array.countBy(retired, Equal.equals(name)),
        retype: Option.exists(Option.fromUndefinedOr(retype), Array.contains(field)),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Bytes, type Column, type ColumnOptions, column, conditions, dataTypes, type Field, type Fields, Uuid, type Where };
