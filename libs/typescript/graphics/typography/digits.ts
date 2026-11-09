import { Array, Effect, Option, pipe, Record, Schema, SchemaGetter, SchemaIssue } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECIMAL_DIGITS = 10;
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Schema.Class<Digits, { readonly brand: unique symbol }>('Digits')({ system: Schema.String, table: Schema.Array(Schema.String.check(Schema.isPattern(DIGIT))).check(Schema.isBetweenLength(DECIMAL_DIGITS, DECIMAL_DIGITS)) }) {
    static readonly #bySystem = pipe(
        Intl.supportedValuesOf('numberingSystem'),
        Record.fromIterableWith((system) => [system, Array.makeBy(DECIMAL_DIGITS, new Intl.NumberFormat(undefined, { numberingSystem: system }).format)]),
        Record.filterMap((table, system) => Schema.decodeResult(Digits)({ system, table })),
    );
    static readonly #values = Record.fromEntries(Record.values(Digits.#bySystem).flatMap(({ table }) => table.map((digit, value) => [digit, value] as const)));
    static readonly #replace = (text: string, written: (value: number) => string): string => text.replace(DECIMAL, (digit) => Option.match(Record.get(Digits.#values, digit), { onNone: () => digit, onSome: written }));
    static readonly of = (system: string): Option.Option<Digits> => Record.get(Digits.#bySystem, system);
    static readonly System = Schema.Literals(Record.keys(Digits.#bySystem))
        .annotate({ description: 'Decimal numbering system id' })
        .pipe(
            Schema.decodeTo(Schema.instanceOf(Digits), {
                decode: SchemaGetter.transformEffect((system: string, options) => Effect.fromOption(Digits.of(system), () => new SchemaIssue.InvalidValue({ message: `No numbering system ${system}` }, system, options))),
                encode: SchemaGetter.transform((digits: Digits) => digits.system),
            }),
        );
    static readonly Numeral = Schema.String.check(Schema.isPattern(/^\s*[+-]?\p{Nd}+\s*$/u)).pipe(Schema.decodeTo(Schema.NumberFromString.check(Schema.isInt()), { decode: SchemaGetter.transform((text: string) => Digits.#replace(text, String)), encode: SchemaGetter.passthrough() }));

    format(text: string): string {
        return Digits.#replace(text, (value) => Array.getUnsafe(this.table, value));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits };
