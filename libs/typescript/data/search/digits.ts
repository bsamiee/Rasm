import { Array, Option, Record, Schema, SchemaGetter } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCII = '0123456789';
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Schema.Class<Digits, { readonly brand: unique symbol }>('Digits')({
    system: Schema.String.annotate({ description: 'Intl numbering system id' }),
    table: Schema.Array(Schema.String.check(Schema.isPattern(DIGIT)))
        .check(Schema.isBetweenLength(ASCII.length, ASCII.length))
        .annotate({ description: 'Decimal digit for each value 0 to 9' }),
}) {
    static readonly #bySystem = Record.getSomes(Record.fromIterableWith(Intl.supportedValuesOf('numberingSystem'), (system) => [system, Schema.decodeOption(Digits)({ system, table: Array.makeBy(ASCII.length, new Intl.NumberFormat(undefined, { numberingSystem: system }).format) })]));
    static readonly #value = (digit: string): number => Array.unfold((digit.codePointAt(0) ?? 0) - 1, (previous) => Option.map(Option.fromNullOr(DIGIT.exec(String.fromCodePoint(previous))), () => [previous, previous - 1] as const)).length % ASCII.length;
    static readonly #replace = (text: string, written: (value: number) => string): string => text.replace(DECIMAL, (digit) => written(Digits.#value(digit)));
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
