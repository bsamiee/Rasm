import { Effect, Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from '#/page/render';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydratePage = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>) {
    const [root, data] = yield* Effect.all([Effect.fromNullishOr(document.getElementById(shell.root)), Effect.fromNullishOr(document.getElementById(shell.data))]);
    hydrateRoot(root, shell.render(yield* Schema.decodeEffect(Schema.fromJsonString(shell.schema))(data.textContent)));
    data.remove();
}, Effect.runSync);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydratePage };
