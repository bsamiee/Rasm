import type { Vitest } from '@effect/vitest';
import { Data, Effect, Exit } from 'effect';
import type * as FastCheck from 'effect/FastCheck';
import type * as Schema from 'effect/Schema';
import type * as TestServices from 'effect/TestServices';

// --- [TYPES] -----------------------------------------------------------------------------

declare namespace Law {
    type Arbs = { readonly [K in string]: Schema.Schema.Any | FastCheck.Arbitrary<unknown> };
    type ArgsOf<A extends Arbs> = {
        readonly [K in keyof A]: A[K] extends FastCheck.Arbitrary<infer T> ? T : Schema.Schema.Type<A[K]>;
    };
    type Api<R> = Vitest.Methods<R> | Vitest.MethodsNonLive<R>;
    interface Shape<S, A extends Arbs, E, R> {
        readonly name: string;
        readonly arbitraries: A;
        readonly predicate: (subject: S, args: ArgsOf<A>) => Effect.Effect<boolean, E, R | TestServices.TestServices>;
        readonly witness: { readonly label: string; readonly foil: S; readonly args: ArgsOf<A> };
    }
    interface Law<S, R = never> {
        readonly name: string;
        readonly register: (api: Api<R>, subject: S) => void;
    }
}

// --- [ERRORS] ----------------------------------------------------------------------------

class LawRefuted extends Data.TaggedError('LawRefuted')<{ readonly law: string; readonly args: unknown }> {}

class LawTautology extends Data.TaggedError('LawTautology')<{ readonly law: string; readonly witness: string }> {}

// --- [OPERATIONS] ------------------------------------------------------------------------

const _held = <E, R>(
    verdict: Effect.Effect<boolean, E, R>,
    evidence: { readonly law: string; readonly args: unknown },
): Effect.Effect<void, E | LawRefuted, R> => Effect.flatMap(verdict, (held) => (held ? Effect.void : Effect.fail(new LawRefuted(evidence))));

const Law = {
    // A witness the predicate survives is a tautology no mutant can violate; auditing it is part of every registration.
    audit: <S, A extends Law.Arbs, E, R>(shape: Law.Shape<S, A, E, R>): Effect.Effect<void, LawTautology, R | TestServices.TestServices> =>
        Effect.flatMap(Effect.exit(shape.predicate(shape.witness.foil, shape.witness.args)), (exit) =>
            Exit.match(exit, {
                onFailure: () => Effect.void,
                onSuccess: (held) => (held ? Effect.fail(new LawTautology({ law: shape.name, witness: shape.witness.label })) : Effect.void),
            }),
        ),
    // The witness is mandatory at construction: a law value cannot exist without its refuting foil.
    make: <S, const A extends Law.Arbs, E = never, R = never>(shape: Law.Shape<S, A, E, R>): Law.Law<S, R> => ({
        name: shape.name,
        register: (api, subject) => {
            api.effect.prop(shape.name, shape.arbitraries, (args) => _held(shape.predicate(subject, args), { law: shape.name, args }));
            api.effect(`${shape.name} refutes ${shape.witness.label}`, () => Law.audit(shape));
        },
    }),
    register: <S, R>(api: Law.Api<R>, subject: S, laws: ReadonlyArray<Law.Law<S, R>>): void => {
        for (const law of laws) {
            law.register(api, subject);
        }
    },
} as const;

// --- [EXPORTS] ---------------------------------------------------------------------------

export { Law, LawRefuted, LawTautology };
