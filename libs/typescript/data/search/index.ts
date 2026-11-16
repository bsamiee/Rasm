import UFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, Equivalence, Function, flow, Option, Predicate, pipe, Record, Schema, SchemaGetter, SchemaIssue } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECIMAL_DIGITS = 10;
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;
const COLLATOR = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const LISTING = new Intl.ListFormat('en');
const FUZZY = new UFuzzy({
    // oxlint-disable-next-line typescript/no-unsafe-enum-assignment -- ufuzzy types IntraMode as an ambient const enum, absent at runtime and unreachable under isolatedModules
    intraMode: 1,
    unicode: true,
    interSplit: String.raw`[^\p{L}\p{Nd}']+`,
    interBound: String.raw`[^\p{L}\p{Nd}]`,
    intraSplit: String.raw`\p{Ll}\p{Lu}`,
    intraBound: String.raw`\p{L}\p{Nd}|\p{Nd}\p{L}|\p{Ll}\p{Lu}`,
    intraChars: String.raw`[\p{L}\p{Nd}']`,
    intraContr: String.raw`'\p{L}{1,2}\b`,
});

// --- [MODELS] --------------------------------------------------------------------------

class Digits extends Schema.Class<Digits, { readonly brand: unique symbol }>('Digits')({ system: Schema.String, table: Schema.Array(Schema.String.check(Schema.isPattern(DIGIT))).check(Schema.isBetweenLength(DECIMAL_DIGITS, DECIMAL_DIGITS)) }) {
    static readonly #bySystem = pipe(
        Intl.supportedValuesOf('numberingSystem'),
        Record.fromIterableWith((system) => [system, Array.makeBy(DECIMAL_DIGITS, new Intl.NumberFormat(undefined, { numberingSystem: system }).format)]),
        Record.filterMap((table, system) => Schema.decodeResult(Digits)({ system, table })),
    );
    static readonly #values = Record.fromEntries(Object.values(Digits.#bySystem).flatMap(({ table }) => table.map((digit, value) => [digit, value] as const)));
    static readonly #replace = (text: string, written: (value: number) => string) => text.replaceAll(DECIMAL, (digit) => Option.match(Record.get(Digits.#values, digit), { onNone: () => digit, onSome: written }));
    static readonly fold = (text: string): string => Digits.#replace(text, String);
    static readonly of = (system: string): Option.Option<Digits> => Record.get(Digits.#bySystem, system);
    static readonly System = Schema.Literals(Record.keys(Digits.#bySystem))
        .annotate({ description: 'Decimal numbering system id' })
        .pipe(
            Schema.decodeTo(Schema.instanceOf(Digits), {
                decode: SchemaGetter.transformEffect((system: string, options) => Effect.fromOption(Digits.of(system), () => new SchemaIssue.InvalidValue({ message: `No numbering system ${system}` }, system, options))),
                encode: SchemaGetter.transform((digits: Digits) => digits.system),
            }),
        );
    static readonly Numeral = Schema.String.check(Schema.isPattern(/^\s*[+-]?\p{Nd}+\s*$/u)).pipe(Schema.decodeTo(Schema.NumberFromString.check(Schema.isInt()), { decode: SchemaGetter.transform(Digits.fold), encode: SchemaGetter.passthrough() }));

    format(text: string): string {
        return Digits.#replace(text, (value) => Array.getUnsafe(this.table, value));
    }
}

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', {
    subject: Schema.String,
    query: Schema.OptionFromOptionalKey(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    candidates: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly named = Equivalence.make((left: string, right: string) => COLLATOR.compare(left, right) === 0);
    static readonly rank = (query: string, candidates: readonly string[]): readonly string[] => {
        const [matches, info, order] = FUZZY.search(candidates.map(Digits.fold), Digits.fold(query), 0, Number.POSITIVE_INFINITY);
        return matches === null ? candidates : (info === null ? matches : order.map((position) => Array.getUnsafe(info.idx, position))).map((index) => Array.getUnsafe(candidates, index));
    };
    static readonly #candidateNames = <A>(candidates: readonly A[], names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): readonly string[] => Array.dedupe(Array.map(candidates, flow(names, Array.headNonEmpty)));
    static readonly matching: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<readonly A[], Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<readonly A[], Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) => {
            const all = Array.fromIterable(candidates);
            return Option.match(query, {
                onNone: () => Effect.succeed(all),
                onSome: (wanted) => Effect.succeed(all.filter((candidate) => Array.containsWith(Unmatched.named)(names(candidate), wanted))).pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => Unmatched.make({ subject, query, candidates: Unmatched.#candidateNames(all, names) }))),
            });
        },
    );
    static readonly single: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<A, Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) =>
            Unmatched.matching(candidates, subject, query, names).pipe(
                Effect.filterOrFail(Predicate.isTupleOf(1), (matched) => Unmatched.make({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Unmatched.#candidateNames(matched, names) })),
                Effect.map(Array.headNonEmpty),
            ),
    );

    override get message() {
        const limit = 100;
        const refused = ['No', this.subject, ...Option.toArray(Option.map(this.query, (query) => `matching ${query}`))].join(' ');
        const displayed = pipe(
            this.query,
            Option.map((query) => Unmatched.rank(query, this.candidates)),
            Option.filter(Array.isReadonlyArrayNonEmpty),
            Option.getOrElse(() => this.candidates),
            Array.take(limit),
        );
        const rest = this.candidates.length - displayed.length;
        const candidates = rest === 0 ? displayed : [...displayed, `${rest} more`];
        return Array.isReadonlyArrayEmpty(this.candidates) ? refused : `${refused}, candidates: ${LISTING.format(candidates)}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Digits, Unmatched };
