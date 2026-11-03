import { Effect, Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from '#/page/shell';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydrate = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>) {
    const [root, data] = yield* Effect.all([Effect.fromNullishOr(document.getElementById(shell.root)), Effect.fromNullishOr(document.getElementById(shell.script.id))]);
    hydrateRoot(root, shell.render(yield* Schema.decodeEffect(shell.script.json)(data.textContent)));
    data.remove();
}, Effect.runSync);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydrate };
