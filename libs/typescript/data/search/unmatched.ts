import UFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, Equivalence, Function, flow, Option, Predicate, pipe, Schema } from 'effect';
import { Digits } from '#/digits';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COLLATOR = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const LISTING = new Intl.ListFormat('en');
const FUZZY = new UFuzzy({
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
    query: Schema.OptionFromOptionalKey(Schema.String),
    candidates: Schema.Array(Schema.String),
    message: Schema.String,
}) {
    static readonly named = Equivalence.make((left: string, right: string) => COLLATOR.compare(left, right) === 0);
    static readonly rank = (query: string, candidates: readonly string[]): readonly string[] => {
        const [matches, info, order] = FUZZY.search(candidates.map(Digits.fold), Digits.fold(query), 0, Number.POSITIVE_INFINITY);
        return matches === null ? candidates : (info === null ? matches : order.map((position) => Array.getUnsafe(info.idx, position))).map((index) => Array.getUnsafe(candidates, index));
    };
    static readonly of = ({ subject, query = Option.none(), candidates = [] }: { readonly subject: string; readonly query?: Option.Option<string>; readonly candidates?: readonly string[] }) => {
        const limit = 100;
        const refused = ['No', subject, ...Option.toArray(Option.map(query, (wanted) => `matching ${wanted}`))].join(' ');
        const displayed = pipe(
            query,
            Option.map((wanted) => Unmatched.rank(wanted, candidates)),
            Option.filter(Array.isReadonlyArrayNonEmpty),
            Option.getOrElse(() => candidates),
            Array.take(limit),
        );
        const rest = candidates.length - displayed.length;
        const listed = rest === 0 ? displayed : [...displayed, `${rest} more`];
        return Unmatched.make({ subject, query, candidates, message: Array.isReadonlyArrayEmpty(candidates) ? refused : `${refused}, candidates: ${LISTING.format(listed)}` });
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
                onSome: (wanted) => Effect.succeed(all.filter((candidate) => Array.containsWith(Unmatched.named)(names(candidate), wanted))).pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => Unmatched.of({ subject, query, candidates: Unmatched.#candidateNames(all, names) }))),
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
                Effect.filterOrFail(Predicate.isTupleOf(1), (matched) => Unmatched.of({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Unmatched.#candidateNames(matched, names) })),
                Effect.map(Array.headNonEmpty),
            ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
