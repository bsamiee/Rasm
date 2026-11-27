import { Array, ByteSize, Data, DateTime, Duration, Effect, Function, Match, Number, Option, Order, Path, Predicate, Record, Result, Schema, SchemaGetter, String } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const INDENT = '  ';
const BUDGET = { page: 6000, pinned: 2 } as const;
const AFTER = 'after';
const AMBIGUOUS = /^\s|\s$|, |: |["\n]/u;
const MEASURE = new Intl.NumberFormat('en', { maximumFractionDigits: 2, maximumSignificantDigits: 3, roundingPriority: 'morePrecision', useGrouping: false, signDisplay: 'negative' });
const PERCENT = new Intl.NumberFormat('en', { style: 'percent' });
const PLURAL = new Intl.PluralRules('en');
const LIST = new Intl.ListFormat('en', { type: 'unit' });

// --- [MODELS] --------------------------------------------------------------------------

class Call extends Data.Class<{ readonly tool: string; readonly given: Readonly<Record<string, Schema.Json>> }> {}
class Line extends Data.Class<{
    readonly head: readonly string[];
    readonly value?: string;
    readonly facts?: readonly string[];
    readonly call?: Call;
    readonly rows?: readonly Line[];
    readonly pinned?: boolean;
    readonly cut?: number;
}> {}
class Report extends Data.Class<{ readonly lines: readonly Line[]; readonly next?: Call; readonly budget?: number }> {
    static readonly page: number = BUDGET.page;
    static readonly of = (value: unknown): Report => new Report({ lines: stated(value, Option.none()) });
    static readonly text = (report: Report): string => [...fit(report.lines, report.budget ?? BUDGET.page), ...Array.fromNullishOr(report.next).map((call) => written(new Line({ head: ['next'], value: Format.call(call) })))].join('\n');
    static readonly schema: Schema.Codec<Report, string> = Schema.String.pipe(Schema.decodeTo(Schema.instanceOf(Report), { decode: SchemaGetter.forbidden(() => 'report text decodes to no report'), encode: SchemaGetter.transform(Report.text) }));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const spans = Function.memoize((intl: typeof Intl) => ({ whole: new intl.DurationFormat('en', { style: 'narrow' }), tenths: new intl.DurationFormat('en', { style: 'narrow', milliseconds: 'numeric', fractionalDigits: 1 }) }));
const argument = (value: unknown): string =>
    Match.value(value).pipe(
        Match.when(Predicate.isObjectOrArray, (held) => (Array.isArray(held) ? `[${held.map(argument).join(',')}]` : `{${Record.collect(held, (key, each) => `${key}:${argument(each)}`).join(', ')}}`)),
        Match.orElse((scalar) => JSON.stringify(scalar)),
    );
const Format = {
    count: (amount: number, noun?: Readonly<Partial<Record<Intl.LDMLPluralRule, string>>> & { readonly other: string }) => [MEASURE.format(amount), ...Array.fromNullishOr(noun).map((forms) => forms[PLURAL.select(amount)] ?? forms.other)].join(' '),
    duration: (elapsed: Duration.Input) => {
        const whole = Duration.millis(Math.round(Duration.toMillis(elapsed)));
        const { days, hours, minutes, seconds, millis } = Duration.parts(whole);
        const { whole: units, tenths } = spans(Intl);
        return Match.value(whole).pipe(
            Match.when(
                () => Duration.toMillis(elapsed) < 1,
                () => `<${units.format({ milliseconds: 1 })}`,
            ),
            Match.when(Duration.isLessThan(Duration.seconds(1)), () => units.format({ milliseconds: millis })),
            Match.when(Duration.isLessThan(Duration.minutes(1)), () => tenths.format({ seconds, milliseconds: millis })),
            Match.orElse(() => units.format({ days, hours, minutes, seconds })),
        );
    },
    bytes: ByteSize.format,
    percent: PERCENT.format,
    instant: DateTime.formatIso,
    path: (url: string | URL) => Path.Path.use((path) => path.fromFileUrl(new URL(url))).pipe(Effect.provide(Path.layer), Effect.runSync),
    quoted: (text: string) => (AMBIGUOUS.test(text) ? `"${text}"` : text),
    list: (items: Iterable<string>) => LIST.format(items),
    call: ({ tool, given }: Call) => `${tool} ${argument(given)}`,
} as const;
const written = ({ head, value, facts = [], call, rows }: Line) => {
    const heading = [head.join(' '), ...Array.fromNullishOr(value)].filter(String.isNonEmpty).join(': ');
    const line = [heading, ...facts].filter(String.isNonEmpty).join(', ');
    return rows === undefined && call !== undefined ? `${line} - ${Format.call(call)}` : line;
};
const stated = (value: unknown, label: Option.Option<string>): readonly Line[] => {
    const head = Option.toArray(label);
    const under = (rows: readonly Line[]) => (Array.isReadonlyArrayEmpty(rows) ? [] : Option.match(label, { onNone: () => rows, onSome: (name) => [new Line({ head: [`${name}:`], rows })] }));
    return Match.value(value).pipe(
        Match.when(true, () => [new Line({ head })]),
        Match.when(Match.number, (amount) => (amount === 0 ? [] : [new Line({ head, value: Format.count(amount) })])),
        Match.when(Match.string, (text) => {
            const lines = text.split('\n').filter(String.isNonEmpty);
            return Predicate.isTupleOf(lines, 1) ? [new Line({ head, value: lines[0] })] : under(lines.map((line) => new Line({ head: [line] })));
        }),
        Match.when(Predicate.isObjectOrArray, (held) => {
            const lines = Array.isArray(held) ? held.flatMap((item) => (Predicate.isObject(item) ? row(item) : stated(item, Option.none()))) : Record.collect(held, (key, each) => stated(each, Option.some(String.noCase()(key)))).flat();
            const values = lines.flatMap(({ value: text }) => Array.fromNullishOr(text));
            return Array.isArray(held) && lines.every(({ head: parts, rows }) => Array.isReadonlyArrayEmpty(parts) && rows === undefined) ? Array.match(values, { onEmpty: Array.empty, onNonEmpty: (texts) => [new Line({ head, value: Format.list(texts.map(Format.quoted)) })] }) : under(lines);
        }),
        Match.orElse((): readonly Line[] => []),
    );
};
const row = (item: unknown): readonly Line[] => {
    const [flat, nested] = Array.partition(stated(item, Option.none()), (line) => (line.rows === undefined ? Result.succeed(line) : Result.fail(line)));
    const named = flat.map(({ head, value }) => [...head, ...Array.fromNullishOr(value).map(Format.quoted)]);
    return Array.match(named, {
        onEmpty: () => nested,
        onNonEmpty: (parts) => [new Line({ head: Array.headNonEmpty(parts), facts: Array.tailNonEmpty(parts).map((part) => part.join(' ')), ...(Array.isReadonlyArrayEmpty(nested) ? {} : { rows: nested }) })],
    });
};
const orders = (rows: readonly Line[]): readonly number[] => rows.flatMap((each) => [...Array.fromNullishOr(each.cut), ...orders(each.rows ?? [])]);
const layout = (rows: readonly Line[], level: number) => {
    const groups = Array.chop(rows, (rest) => {
        const [loose, tail] = Array.span(rest, ({ head }) => Array.isReadonlyArrayEmpty(head));
        return [[...loose, ...Array.take(tail, 1)], Array.drop(tail, 1)] as const;
    });
    const pinned =
        Option.getOrElse(
            Array.findLastIndex(rows, (each) => each.pinned === true),
            () => -1,
        ) + 1;
    const cut = Option.getOrElse(
        Array.findFirstIndex(rows, (each) => each.cut !== undefined && each.cut < level),
        () => rows.length,
    );
    return { groups, pinned, wanted: Math.max(cut, pinned) };
};
const print =
    (rows: readonly Line[], depth: number, level: number, budget: number, call: Option.Option<Call>) =>
    (used: number): { readonly lines: readonly string[]; readonly used: number } => {
        const indent = INDENT.repeat(depth);
        const { groups, pinned, wanted } = layout(rows, level);
        const [state, chunks] = Array.mapAccum(groups, { start: 0, used, cut: Option.none<number>() }, (spent, group) => {
            const drawn = group.map((each) => ({ each, own: [written(each)].filter(String.isNonEmpty).map((text) => `${indent}${text.replaceAll('\n', `\n${indent}${INDENT}`)}`) }));
            const cost = Number.sumAll(drawn.flatMap(({ own }) => own.map((text) => text.length + 1)));
            const fits = Option.isNone(spent.cut) && spent.start + group.length <= wanted && (spent.start === 0 || spent.used + cost <= budget || (spent.start < pinned && spent.used + cost <= budget * BUDGET.pinned));
            const [total, lines] = Array.mapAccum(fits ? drawn : [], spent.used, (sum, { each, own }) => {
                const inner = print(each.rows ?? [], depth + own.length, level, budget, Option.fromUndefinedOr(each.call))(sum + Number.sumAll(own.map((text) => text.length + 1)));
                return [inner.used, [...own, ...inner.lines]] as const;
            });
            return [{ start: spent.start + group.length, used: total, cut: fits ? spent.cut : Option.orElse(spent.cut, () => Option.some(spent.start)) }, lines.flat()] as const;
        });
        const more = Option.map(state.cut, (at) => {
            const cursor = Option.flatMap(Array.get(rows, at - 1), ({ head }) => Array.head(head));
            const next = Option.map(call, ({ tool, given }) => new Call({ tool, given: Option.match(cursor, { onNone: () => given, onSome: (after) => Record.set(given, AFTER, after) }) }));
            return `${indent}${written(new Line({ head: ['more'], value: Format.count(rows.length - at), ...Option.getOrUndefined(Option.map(next, (held) => ({ call: held }))) }))}`;
        });
        return { lines: [...chunks.flat(), ...Option.toArray(more)], used: state.used + Option.match(more, { onNone: () => 0, onSome: (line) => line.length + 1 }) };
    };
const fit = (lines: readonly Line[], budget: number) => {
    const levels = Array.prepend(Array.sort(Array.dedupe(orders(lines)), Order.Number).map(Number.increment), 0);
    return Option.getOrElse(
        Array.findFirst(levels, (level) => Option.liftPredicate(print(lines, 0, level, globalThis.Number.POSITIVE_INFINITY, Option.none())(0), ({ used }) => used <= budget)),
        () => print(lines, 0, Array.lastNonEmpty(levels), budget, Option.none())(0),
    ).lines;
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Call, Format, Line, Report };
