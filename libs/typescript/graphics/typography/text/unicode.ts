import { Context, Effect, Layer } from 'effect';

// --- [SERVICES] ------------------------------------------------------------------------

class Unicode extends Context.Service<Unicode, typeof import('icu')>()('@rasm/typography/text/unicode') {
    static readonly layer = Layer.effect(
        Unicode,
        Effect.promise(() => import('icu')),
    );
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Unicode };
