import { Array, ByteSize, Data, Option } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const byteUnits = [
    ['byte', ByteSize.bytes],
    ['kilobyte', ByteSize.kilobytes],
    ['megabyte', ByteSize.megabytes],
    ['gigabyte', ByteSize.gigabytes],
    ['terabyte', ByteSize.terabytes],
    ['petabyte', ByteSize.petabytes],
] as const;
const byteDigits = 3;

// --- [MODELS] --------------------------------------------------------------------------

class Format extends Data.Class<{ readonly locale: Intl.UnicodeBCP47LocaleIdentifier }> {
    readonly #plural = new Intl.PluralRules(this.locale);
    readonly #count = new Intl.NumberFormat(this.locale);
    readonly #numeral = new Intl.NumberFormat(this.locale, { minimumIntegerDigits: 2, useGrouping: false });
    readonly #bytes = Array.map(byteUnits, ([unit, size]) => ({ scale: ByteSize.toNumberUnsafe(size(1)), format: new Intl.NumberFormat(this.locale, { style: 'unit', unit, unitDisplay: 'narrow', maximumSignificantDigits: byteDigits }) }));
    readonly #lists = { conjunction: new Intl.ListFormat(this.locale, { type: 'conjunction' }), disjunction: new Intl.ListFormat(this.locale, { type: 'disjunction' }), unit: new Intl.ListFormat(this.locale, { type: 'unit' }) } satisfies Record<Intl.ListFormatType, Intl.ListFormat>;
    quantity(count: number, forms: { readonly [Category in Intl.LDMLPluralRule]?: string } & { readonly other: string }) {
        return `${this.#count.format(count)} ${forms[this.#plural.select(count)] ?? forms.other}`;
    }
    numeral(value: number) {
        return this.#numeral.format(value);
    }
    bytes(size: number) {
        const rounded = Number(size.toPrecision(byteDigits));
        const { scale, format } = Option.getOrElse(
            Array.findLast(this.#bytes, (unit) => unit.scale <= rounded),
            () => Array.headNonEmpty(this.#bytes),
        );
        return format.format(size / scale);
    }
    list(values: Iterable<string>, type: Intl.ListFormatType = 'conjunction') {
        return this.#lists[type].format(values);
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Format };
