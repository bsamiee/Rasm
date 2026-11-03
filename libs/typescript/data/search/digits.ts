import { Array, Data, Option, Record, Schema, SchemaGetter } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Fields = { readonly system: string; readonly table: readonly string[] };

// --- [CONSTANTS] -----------------------------------------------------------------------

const ASCII = '0123456789';
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Data.Class<Fields> {
    static readonly #bySystem = Record.getSomes(
        Record.fromIterableWith(Intl.supportedValuesOf('numberingSystem'), (system) => [
            system,
            Option.map(
                Option.liftPredicate(Array.makeBy(ASCII.length, new Intl.NumberFormat(undefined, { numberingSystem: system }).format), (table) => table.every((digit) => DIGIT.test(digit))),
                (table) => new Digits({ system, table }),
            ),
        ]),
    );
    static readonly #values = Record.fromEntries(Array.flatMap(Record.values(Digits.#bySystem), ({ table }) => Array.map(table, (digit, value) => [digit, value] as const)));
    static readonly #replace = (text: string, written: (value: number) => string): string => text.replace(DECIMAL, (digit) => Option.match(Record.get(Digits.#values, digit), { onNone: () => digit, onSome: written }));
    static readonly fold = (text: string): string => Digits.#replace(text, String);
    static readonly of = (system: string): Option.Option<Digits> => Record.get(Digits.#bySystem, system);
    static readonly systems: readonly string[] = Record.keys(Digits.#bySystem);
    static readonly System = Schema.Literals(Digits.systems).annotate({ description: 'Decimal numbering system id' });
    static readonly Numeral = Schema.String.check(Schema.isPattern(/^\s*[+-]?\p{Nd}+\s*$/u)).pipe(Schema.decodeTo(Schema.NumberFromString.check(Schema.isInt()), { decode: SchemaGetter.transform(Digits.fold), encode: SchemaGetter.passthrough() }));

    private constructor(fields: Fields) {
        super(fields);
    }

    format(text: string): string {
        return Digits.#replace(text, (value) => Array.getUnsafe(this.table, value));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits };
