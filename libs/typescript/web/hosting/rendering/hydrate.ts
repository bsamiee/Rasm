import { Effect, Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from '#/rendering/shell';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydrate = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>) {
    const { textContent } = yield* Effect.fromNullishOr(document.getElementById(shell.data.id));
    hydrateRoot(document, shell.page(yield* Schema.decodeEffect(shell.data.json)(textContent), textContent, { lang: document.documentElement.lang }));
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydrate };
