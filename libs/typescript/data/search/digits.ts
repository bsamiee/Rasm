import { Array, type Option, pipe, Record, Schema, SchemaGetter } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCII = '0123456789';
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Schema.Class<Digits, { readonly brand: unique symbol }>('Digits')({ system: Schema.String, table: Schema.Array(Schema.String.check(Schema.isPattern(DIGIT))).check(Schema.isBetweenLength(ASCII.length, ASCII.length)) }) {
    static readonly #bySystem = pipe(
        Intl.supportedValuesOf('numberingSystem'),
        Record.fromIterableWith((system) => [system, Array.makeBy(ASCII.length, new Intl.NumberFormat(undefined, { numberingSystem: system }).format)]),
        Record.filterMap((table, system) => Schema.decodeResult(Digits)({ system, table })),
    );
    static readonly #preceding = (point: number): number => (DIGIT.test(String.fromCodePoint(point - 1)) ? 1 + Digits.#preceding(point - 1) : 0);
    static readonly #replace = (text: string, written: (value: number) => string): string => text.replace(DECIMAL, (digit) => written(Digits.#preceding(digit.codePointAt(0) ?? 0) % ASCII.length));
    static readonly fold = (text: string): string => Digits.#replace(text, String);
    static readonly of = (system: string): Option.Option<Digits> => Record.get(Digits.#bySystem, system);
    static readonly systems: readonly string[] = Record.keys(Digits.#bySystem);
    static readonly System = Schema.Literals(Digits.systems).annotate({ description: 'Decimal numbering system id' });
    static readonly Numeral = Schema.String.check(Schema.isPattern(/^\s*[+-]?\p{Nd}+\s*$/u)).pipe(Schema.decodeTo(Schema.NumberFromString.check(Schema.isInt()), { decode: SchemaGetter.transform(Digits.fold), encode: SchemaGetter.passthrough() }));

    format(text: string): string {
        return Digits.#replace(text, (value) => Array.getUnsafe(this.table, value));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits };
