import { Array, Effect, Function, flow, Option, Predicate, pipe, Schema } from 'effect';
import { rank } from '#/rank';

// --- [ERRORS] --------------------------------------------------------------------------

class Unmatched extends Schema.TaggedError<Unmatched, { readonly brand: unique symbol }>()('Unmatched', { subject: Schema.String, query: Schema.OptionFromOptionalKey(Schema.String), candidates: Schema.Array(Schema.String) }) {
    static readonly matching: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]): (candidates: Iterable<A>) => Effect.Effect<readonly A[], Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]): Effect.Effect<readonly A[], Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]) => {
            const all = Array.fromIterable(candidates);
            return Option.match(query, {
                onNone: () => Effect.succeed(all),
                onSome: (wanted) => {
                    const { compare } = new Intl.Collator('en', { usage: 'search', sensitivity: 'accent' });
                    return Effect.fromOption(
                        Option.liftPredicate(
                            all.filter((candidate) => names(candidate).some((name) => compare(name, wanted) === 0)),
                            Array.isReadonlyArrayNonEmpty,
                        ),
                        () => Unmatched.make({ subject, query, candidates: Array.dedupe(Array.flatMap(all, flow(names, Array.take(1)))) }),
                    );
                },
            });
        },
    );
    static readonly single: {
        <A>(subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]): (candidates: Iterable<A>) => Effect.Effect<A, Unmatched>;
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]): Effect.Effect<A, Unmatched>;
    } = Function.dual(
        (args) => !Predicate.isString(args[0]),
        <A>(candidates: Iterable<A>, subject: string, query: Option.Option<string>, names: (candidate: A) => readonly string[]) =>
            Effect.map(
                Effect.filterOrFail(Unmatched.matching(candidates, subject, query, names), Predicate.isTupleOf(1), (matched) => Unmatched.make({ subject: Array.isReadonlyArrayEmpty(matched) ? subject : `single ${subject}`, query, candidates: Array.flatMap(matched, flow(names, Array.take(1))) })),
                ([only]) => only,
            ),
    );

    override get message(): string {
        const limit = 100;
        const ordered = pipe(
            this.query,
            Option.map((query) => rank(query, this.candidates)),
            Option.filter(Array.isReadonlyArrayNonEmpty),
            Option.getOrElse(() => this.candidates),
        );
        const shown = Array.take(ordered, limit);
        const rest = this.candidates.length - shown.length;
        const listed = new Intl.ListFormat('en').format([...shown, ...(rest === 0 ? [] : [`${rest} more`])]);
        return `No ${this.subject}${Option.match(this.query, { onNone: () => '', onSome: (query) => ` matching ${query}` })}${listed === '' ? '' : `, candidates: ${listed}`}`;
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unmatched };
