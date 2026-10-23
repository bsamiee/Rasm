import { Unmatched } from '@rasm/search';
import { Cache, Cause, Effect, Option, Predicate, Record } from 'effect';
import { resolveObject } from '#/dom/location';
import { Model } from '#/dom/model';
import { leaf, Reference, Specifier } from '#/dom/reference';
import { Mutation } from '#/dom/transactions';

// --- [MODELS] --------------------------------------------------------------------------

class Remove extends Mutation.extend<Remove>('Remove')({ target: Specifier }, { description: 'Removes object in one undo step' }) {
    override get label(): string {
        return `Remove ${leaf(this.target)}`;
    }

    run() {
        const { target } = this;
        return Effect.flatMap(resolveObject(target), (found) =>
            this.commit(
                Effect.gen(function* () {
                    const { methods } = yield* Model.use(({ rows }) => Cache.get(rows, found.constructorName));
                    const remove = yield* Effect.fromOption(Option.liftPredicate(Reflect.get(found, 'remove'), Predicate.isFunction), () => Unmatched.make({ subject: `method of ${found.constructorName}`, query: Option.some('remove'), candidates: Record.keys(methods) }));
                    const removed = Reference.of(found);
                    yield* Effect.try({ try: () => Reflect.apply(remove, found, []), catch: (cause) => new Cause.UnknownError(cause, target) });
                    return Reference.make({ ...removed, valid: found.isValid });
                }),
            ),
        );
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Remove };
