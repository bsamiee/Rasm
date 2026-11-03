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
    static readonly matching: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<readonly A[], Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<readonly A[], Unmatched>;
    } = Function.dual(4, <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) => {
        const all = Array.fromIterable(candidates);
        return Option.match(query, {
            onNone: () => Effect.succeed(all),
            onSome: (wanted) =>
                Effect.filterOrFail(
                    Effect.succeed(all.filter((candidate) => Array.containsWith(Unmatched.named)(names(candidate), wanted))),
                    Array.isReadonlyArrayNonEmpty,
                    () => Unmatched.make({ subject, query, candidates: Array.dedupe(Array.map(all, flow(names, Array.headNonEmpty))) }),
                ),
        });
    });
    static readonly single: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>): Effect.Effect<A, Unmatched>;
    } = Function.dual(4, <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => Array.NonEmptyReadonlyArray<string>) =>
        Effect.map(
            Effect.filterOrFail(Unmatched.matching(candidates, subject, query, names), Predicate.isTupleOf(1), (matched) => Unmatched.make({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Array.map(matched, flow(names, Array.headNonEmpty)) })),
            ([only]) => only,
        ),
    );

    override get message(): string {
        const listedCandidates = 100;
        const refused = `No ${this.subject}${Option.match(this.query, { onNone: () => '', onSome: (query) => ` matching ${query}` })}`;
        const ordered = pipe(
            this.query,
            Option.map((query) => rank(query, this.candidates)),
            Option.filter(Array.isReadonlyArrayNonEmpty),
            Option.getOrElse(() => this.candidates),
        );
        const shown = Array.take(ordered, listedCandidates);
        const rest = this.candidates.length - shown.length;
        return Array.isReadonlyArrayEmpty(this.candidates) ? refused : `${refused}, candidates: ${LISTING.format([...shown, ...(rest === 0 ? [] : [`${rest} more`])])}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
