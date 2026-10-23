import { hash } from 'node:crypto';
import { Array, Effect, Number, Option, Order, Record, Schema, SchemaGetter } from 'effect';
import { Variation } from 'harfbuzzjs';
import { Settings, Tag } from '#/tags';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEADING_ZERO = /^(?<sign>-?)0\./u;

// --- [MODELS] --------------------------------------------------------------------------

const InstanceRecords = Schema.Array(
    Schema.Struct({
        fvar: Schema.Struct({ instance: Schema.Array(Schema.Struct({ nameID: Schema.Int, coord: Schema.Array(Schema.Finite), postscriptNameID: Schema.OptionFromOptionalKey(Schema.Int) })) }).pipe(Schema.withDecodingDefaultKey(Effect.succeed({ instance: [] }))),
    }),
);
const Instance = Schema.Struct({
    variations: Settings(Variation, 'HarfBuzz tag=value over selected instance, clamped to axis range, absent axes ignored'),
    size: Schema.OptionFromOptionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Point size for opsz axis, overrides opsz variation' })),
});
const Descriptor = Schema.TemplateLiteralParser([Schema.String.check(Schema.isStringFinite()).pipe(Schema.decodeTo(Schema.Finite, { decode: SchemaGetter.Number(), encode: SchemaGetter.transform((value: number) => `${value}`.replace(LEADING_ZERO, '$<sign>.')) })), Tag]).pipe(
    Schema.decodeTo(Schema.Tuple([Schema.String, Schema.Finite]), { decode: SchemaGetter.transform(([value, tag]) => [tag, value] as const), encode: SchemaGetter.transform(([tag, value]) => [value, tag] as const) }),
);
const GeneratedName = Schema.TemplateLiteralParser([Schema.String.check(Schema.isPattern(/^[^_]+$/u)), '_', Schema.String.pipe(Schema.decodeTo(Schema.Array(Descriptor), { decode: SchemaGetter.split({ separator: '_' }), encode: SchemaGetter.transform(Array.join('_')) }))]);

// --- [OPERATIONS] ----------------------------------------------------------------------

const coordinate = (value: number) => {
    const fractionBits = 16;
    const fixed = (given: number) => Math.round(given * 2 ** fractionBits);
    const target = fixed(value);
    return Array.min(
        Array.map(Array.range(0, fractionBits), (places) => Number.round(value, places)),
        Order.mapInput(Order.Boolean, (rounded) => fixed(rounded) !== target),
    );
};
const instanceName = (prefix: string, coordinates: Record.ReadonlyRecord<string, number>) => {
    const nameLength = 127;
    return Option.map(Schema.encodeOption(GeneratedName)([prefix, '_', Record.toEntries(coordinates)]), (name) => (name.length > nameLength ? `${prefix}-${hash('sha1', name)}...` : name));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { coordinate, GeneratedName, Instance, InstanceRecords, instanceName };
