import UFuzzy from '@leeoniya/ufuzzy';
import { Array, Effect, Equivalence, ErrorReporter, Function, flow, type LogLevel, Option, Predicate, Schema, SchemaGetter, Struct } from 'effect';
import { Digits } from '#/digits';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COLLATOR = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'base', ignorePunctuation: true });
const LISTING = new Intl.ListFormat('en');
const LIMITS = { listed: 100, nearest: 8 } as const;
const FUZZY = new UFuzzy({
    intraMode: 1,
    unicode: true,
    interSplit: String.raw`[^\p{L}\p{Nd}']+`,
    interBound: String.raw`[^\p{L}\p{Nd}]`,
    intraSplit: String.raw`\p{Ll}\p{Lu}`,
    intraBound: String.raw`\p{L}\p{Nd}|\p{Nd}\p{L}|\p{Ll}\p{Lu}`,
    intraChars: String.raw`[\p{L}\p{Nd}']`,
});

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', {
    subject: Schema.String,
    query: Schema.OptionFromOptionalKey(Schema.String),
    candidates: Schema.Array(Schema.String),
    message: Schema.String,
}) {
    static readonly #listing = (items: readonly string[], limit: number) => LISTING.format(items.length > limit ? [...Array.take(items, limit), `${items.length - limit} more`] : items);
    static readonly #folded = (text: string) => Digits.fold(text.normalize('NFKD').replaceAll(/\p{M}/gu, ''));
    static readonly named = Equivalence.make((left: string, right: string) => COLLATOR.compare(left, right) === 0);
    static readonly rank: {
        (query: string, candidates: readonly string[]): readonly string[];
        <A>(query: string, candidates: readonly A[], name: (candidate: A) => string): readonly A[];
    } = <A>(query: string, candidates: readonly A[], name: (candidate: A) => string = String) => {
        const [matches, info, order] = FUZZY.search(Array.map(candidates, flow(name, Unmatched.#folded)), Unmatched.#folded(query), 0, Number.POSITIVE_INFINITY);
        return matches === null ? candidates : (info === null ? matches : order.map((position) => Array.getUnsafe(info.idx, position))).map((index) => Array.getUnsafe(candidates, index));
    };
    static readonly of = ({ subject, query = Option.none(), candidates = [] }: { readonly subject: string; readonly query?: Option.Option<string>; readonly candidates?: readonly string[] }) => {
        const refused = Option.match(query, { onNone: () => `No ${subject}`, onSome: (wanted) => `No ${subject} matching ${wanted}` });
        const listed = Option.match(
            Option.map(query, (wanted) => Unmatched.rank(wanted, candidates)),
            {
                onNone: () => [`candidates: ${Unmatched.#listing(candidates, LIMITS.listed)}`],
                onSome: Array.match({
                    onNonEmpty: (ranked) => [`nearest: ${Unmatched.#listing(ranked, LIMITS.nearest)}`],
                    onEmpty: () => (candidates.length > LIMITS.nearest ? [] : [`candidates: ${Unmatched.#listing(candidates, LIMITS.nearest)}`]),
                }),
            },
        );
        const message = Array.isReadonlyArrayEmpty(candidates) ? refused : [refused, ...listed].join(', ');
        return Unmatched.make({ subject, query, candidates, message });
    };
    static readonly stated = Schema.Struct(Struct.omit(Unmatched.fields, ['message']))
        .pipe(Schema.decodeTo(Schema.toType(Unmatched), { decode: SchemaGetter.transform(Unmatched.of), encode: SchemaGetter.passthroughSubtype() }))
        .annotate({ description: 'Failed lookup as a host with no Effect states it, decoding to the error with its message derived' });
    override readonly [ErrorReporter.severity]: LogLevel.Severity = 'Info';
    static readonly #candidateNames = <A>(candidates: readonly A[], names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): readonly string[] => Array.dedupe(Array.map(candidates, flow(names, Array.headNonEmpty)));
    static readonly matching: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<readonly A[], Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<readonly A[], Unmatched>;
    } = Function.dual(4, <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) => {
        const all = Array.fromIterable(candidates);
        return Option.match(query, {
            onNone: () => Effect.succeed(all),
            onSome: (wanted) => Effect.succeed(all.filter((candidate) => Array.containsWith(Unmatched.named)(names(candidate), wanted))).pipe(Effect.filterOrFail(Array.isReadonlyArrayNonEmpty, () => Unmatched.of({ subject, query, candidates: Unmatched.#candidateNames(all, names) }))),
        });
    });
    static readonly single: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<A, Unmatched>;
    } = Function.dual(4, <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) =>
        Unmatched.matching(candidates, subject, query, names).pipe(
            Effect.filterOrFail(Predicate.isTupleOf(1), (matched) => Unmatched.of({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Unmatched.#candidateNames(matched, names) })),
            Effect.map(Array.headNonEmpty),
        ),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
