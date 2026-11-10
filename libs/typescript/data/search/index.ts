import uFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, Equivalence, Function, flow, Option, Predicate, pipe, Schema } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const DECIMAL_DIGITS = 10;
const DECIMAL = /\p{Nd}/gu;
const DIGIT = /^\p{Nd}$/u;
const COLLATOR = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const LISTING = new Intl.ListFormat('en');
const FUZZY = new uFuzzy({
    intraMode: 1,
    unicode: true,
    interSplit: String.raw`[^\p{L}\p{Nd}']+`,
    interBound: String.raw`[^\p{L}\p{Nd}]`,
    intraSplit: String.raw`\p{Ll}\p{Lu}`,
    intraBound: String.raw`\p{L}\p{Nd}|\p{Nd}\p{L}|\p{Ll}\p{Lu}`,
    intraChars: String.raw`[\p{L}\p{Nd}']`,
    intraContr: String.raw`'\p{L}{1,2}\b`,
});

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', {
    subject: Schema.String,
    query: Schema.OptionFromOptionalKey(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    candidates: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly named = Equivalence.make((left: string, right: string) => COLLATOR.compare(left, right) === 0);
    static readonly #preceding = (point: number): number => (DIGIT.test(String.fromCodePoint(point - 1)) ? 1 + Unmatched.#preceding(point - 1) : 0);
    static readonly #folded = (text: string) => text.replace(DECIMAL, (digit) => String(Unmatched.#preceding(digit.codePointAt(0) ?? 0) % DECIMAL_DIGITS));
    static readonly rank = (query: string, candidates: readonly string[]): readonly string[] => {
        const [matches, info, order] = FUZZY.search(candidates.map(Unmatched.#folded), Unmatched.#folded(query), 0, Number.POSITIVE_INFINITY);
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

export { Unmatched };
