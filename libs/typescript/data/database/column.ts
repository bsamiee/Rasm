import { Array, Data, Equal, Filter, flow, Match, Option, Record, Schema, SchemaAST, SchemaGetter, SchemaRepresentation, String, Struct } from 'effect';
import type { TableOptions } from '#/relation';

// --- [TYPES] ---------------------------------------------------------------------------

interface Fields extends Schema.Struct.Fields {
    readonly [field: PropertyKey]: Schema.ConstraintCodec<unknown, string | number | Uint8Array | readonly number[] | null>;
}
type Field<F> = keyof F & string;
type Where<F> = { readonly [K in Field<F>]?: null | Array.NonEmptyReadonlyArray<string | number> | { readonly not: null | Array.NonEmptyReadonlyArray<string | number> } };
type Kind = 'text' | 'uuid' | 'json' | 'instant' | 'integer' | 'int64' | 'decimal' | 'real' | 'number' | 'boolean' | 'blob';
type Projection = Data.TaggedEnum<{
    Path: { readonly from: string; readonly path: Array.NonEmptyReadonlyArray<string> };
    Where: { readonly where: readonly { readonly column: string; readonly predicate: Exclude<Where<Fields>[string], undefined> }[] };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const uuidVersion = 7;
const byteMaximum = 255;

// --- [MODELS] --------------------------------------------------------------------------

const Fallback = Schema.Union([Schema.String, Schema.Finite]);
class Column extends Data.Class<{
    readonly field: string;
    readonly name: string;
    readonly schema: Fields[string];
    readonly kind: Kind;
    readonly nullable: boolean;
    readonly literals: readonly SchemaAST.LiteralValue[];
    readonly bounds: readonly { readonly op: string; readonly value: number | bigint; readonly length: boolean }[];
    readonly fallback: Option.Option<string | number>;
    readonly projection: Option.Option<Projection>;
    readonly earlier: Option.Option<string>;
    readonly incarnation: number;
    readonly retype: boolean;
}> {
    static readonly transforms = { transformQueryNames: String.camelToSnake, transformResultNames: String.snakeToCamel, transformJson: false };
    static readonly Projection = Data.taggedEnum<Projection>();
    static readonly Bytes = Schema.Union([Schema.Uint8Array, Schema.Array(Schema.Int.check(Schema.isBetween({ minimum: 0, maximum: byteMaximum })))]).pipe(Schema.decodeTo(Schema.Uint8Array, { decode: SchemaGetter.transform((bytes) => new Uint8Array(bytes)), encode: SchemaGetter.passthroughSubtype() }));
    static readonly Uuid = Schema.String.check(Schema.isUUID(uuidVersion), Schema.isLowercased());
    static readonly conditions = (where: Where<Fields>) => Array.getSomes(Record.collect(where, (field, predicate) => Option.map(Option.fromUndefinedOr(predicate), (rule) => ({ column: String.camelToSnake(field), predicate: rule }))));
    static readonly make = <F extends Fields>(field: Field<F>, schema: F[Field<F>], { defaults, projections, renamed, retype }: Pick<TableOptions<F>, 'defaults' | 'projections' | 'renamed' | 'retype'>, retired: readonly string[]) => {
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
        const name = String.camelToSnake(field);
        return new Column({
            field,
            name,
            schema,
            kind: Match.value({ node: values[0], decoded: decoded[0], boolean, numeric, integer: integer || (numeric && literals.every(Number.isInteger)), finite: has(SchemaRepresentation.isFiniteReviver), uuid: has(SchemaRepresentation.isUUIDReviver) }).pipe(
                Match.withReturnType<Kind>(),
                Match.when({ node: { _tag: 'String', content: { mediaType: 'application/json' } } }, () => 'json'),
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
            fallback: Schema.decodeUnknownOption(Fallback)(defaults?.[field]),
            projection: Option.map(Option.fromUndefinedOr(projections?.[field]), (projection: NonNullable<NonNullable<TableOptions<Fields>['projections']>[string]>) =>
                Match.value(projection).pipe(
                    Match.when({ where: Match.defined }, ({ where }) => Column.Projection.Where({ where: Column.conditions(where) })),
                    Match.orElse(({ from, path }) => Column.Projection.Path({ from: String.camelToSnake(from), path })),
                ),
            ),
            earlier: Option.map(Option.fromUndefinedOr(renamed?.[field]), String.camelToSnake),
            incarnation: Array.countBy(retired, Equal.equals(name)),
            retype: Option.exists(Option.fromUndefinedOr(retype), Array.contains(field)),
        });
    };
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Column, type Field, type Fields, type Kind, type Projection, type Where };
