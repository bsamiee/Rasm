import { Array, Effect, Equivalence, Function, flow, Option, Predicate, pipe, Schema } from 'effect';
import { rank } from '#/rank';

// --- [CONSTANTS] -----------------------------------------------------------------------

const COLLATOR = new Intl.Collator(undefined, { usage: 'search', sensitivity: 'accent' });
const LISTING = new Intl.ListFormat('en');

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', {
    subject: Schema.String,
    query: Schema.OptionFromOptionalKey(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeedNone)),
    candidates: Schema.Array(Schema.String).pipe(Schema.withConstructorDefault(Effect.succeed([]))),
}) {
    static readonly named = Equivalence.make((left: string, right: string) => COLLATOR.compare(left, right) === 0);
    static readonly #shown = <A>(candidates: readonly A[], names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): readonly string[] => Array.dedupe(Array.map(candidates, flow(names, Array.headNonEmpty)));
    static readonly matching: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<readonly A[], Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<readonly A[], Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) => {
            const all = Array.fromIterable(candidates);
            const matched = Option.map(query, (wanted) => all.filter((candidate) => Array.containsWith(Unmatched.named)(names(candidate), wanted)));
            return Option.exists(matched, Array.isReadonlyArrayEmpty) ? Effect.fail(Unmatched.make({ subject, query, candidates: Unmatched.#shown(all, names) })) : Effect.succeed(Option.getOrElse(matched, () => all));
        },
    );
    static readonly single: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<A, Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) =>
            Unmatched.matching(candidates, subject, query, names).pipe(
                Effect.filterOrFail(Predicate.isTupleOf(1), (matched) => Unmatched.make({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Unmatched.#shown(matched, names) })),
                Effect.map(Array.headNonEmpty),
            ),
    );

    override get message(): string {
        const listed = 100;
        const refused = `No ${this.subject}${Option.match(this.query, { onNone: () => '', onSome: (query) => ` matching ${query}` })}`;
        const shown = pipe(
            this.query,
            Option.map((query) => rank(query, this.candidates)),
            Option.filter(Array.isReadonlyArrayNonEmpty),
            Option.getOrElse(() => this.candidates),
            Array.take(listed),
        );
        const rest = this.candidates.length - shown.length;
        return Array.isReadonlyArrayEmpty(this.candidates) ? refused : `${refused}, candidates: ${LISTING.format(rest === 0 ? shown : [...shown, `${rest} more`])}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
