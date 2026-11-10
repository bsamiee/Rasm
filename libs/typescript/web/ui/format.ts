import { Array, ByteSize, Data, DateTime, Duration, Option, Record, Struct } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const byteUnits = [
    ['byte', 'B'],
    ['kilobyte', 'kB'],
    ['megabyte', 'MB'],
    ['gigabyte', 'GB'],
    ['terabyte', 'TB'],
    ['petabyte', 'PB'],
] as const;
const relativeUnits = [
    ['week', Duration.toWeeks],
    ['day', Duration.toDays],
    ['hour', Duration.toHours],
    ['minute', Duration.toMinutes],
    ['second', Duration.toSeconds],
] as const;

// --- [MODELS] --------------------------------------------------------------------------

class Format extends Data.Class<{ readonly locales?: Intl.LocalesArgument; readonly byteDigits?: number }> {
    readonly #plurals = Record.map(Struct.Record(['cardinal', 'ordinal'], this.locales), (locales, type) => new Intl.PluralRules(locales, { type }));
    readonly #lists = Record.map(Struct.Record(['conjunction', 'disjunction', 'unit'], this.locales), (locales, type) => new Intl.ListFormat(locales, { type }));
    readonly #count = new Intl.NumberFormat(this.locales);
    readonly #numeral = new Intl.NumberFormat(this.locales, { useGrouping: false });
    readonly #bytes = Array.map(byteUnits, ([unit, symbol]) => ({ symbol, format: new Intl.NumberFormat(this.locales, { style: 'unit', unit, unitDisplay: 'narrow', maximumSignificantDigits: this.byteDigits }) }));
    readonly date = DateTime.formatIntl(new Intl.DateTimeFormat(this.locales));
    readonly #relative = new Intl.RelativeTimeFormat(this.locales);
    readonly #duration = new Intl.DurationFormat(this.locales);
    quantity(count: number, forms: { readonly [Category in Intl.LDMLPluralRule]?: (count: string) => string } & { readonly other: (count: string) => string }, type: Intl.PluralRuleType = 'cardinal') {
        return (forms[this.#plurals[type].select(count)] ?? forms.other)(this.#count.format(count));
    }
    numeral(value: number, total = value) {
        return this.#numeral.format(value).padStart(this.#numeral.format(total).length, this.#numeral.format(0));
    }
    bytes(size: ByteSize.ByteSize) {
        const rounded = ByteSize.bytes(Number(ByteSize.toNumberUnsafe(size).toPrecision(this.byteDigits)));
        const { symbol, format } = Array.findLast(this.#bytes, (unit) => ByteSize.toUnit(rounded, unit.symbol) >= 1).pipe(Option.getOrElse(() => Array.headNonEmpty(this.#bytes)));
        return format.format(ByteSize.toUnit(size, symbol));
    }
    list(values: Iterable<string>, type: Intl.ListFormatType = 'conjunction') {
        return this.#lists[type].format(values);
    }
    relative(offset: Duration.Duration) {
        const [unit, amount] = Array.findFirst(relativeUnits, ([, to]) => Math.abs(to(offset)) >= 1).pipe(Option.getOrElse(() => Array.lastNonEmpty<(typeof relativeUnits)[number]>(relativeUnits)));
        return this.#relative.format(Math.round(amount(offset)), unit);
    }
    duration(value: Duration.Duration) {
        return this.#duration.format(Struct.renameKeys(Duration.parts(value), { millis: 'milliseconds', nanos: 'nanoseconds' }));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Format };
