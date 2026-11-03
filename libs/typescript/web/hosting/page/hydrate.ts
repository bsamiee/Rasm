import { Cause, Effect, Schema } from 'effect';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from '#/page/shell';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydrate = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>) {
    const element = (id: string) => Effect.fromNullishOr(document.getElementById(id)).pipe(Effect.mapError(() => new Cause.NoSuchElementError(`page holds no #${id}`)));
    const [root, data] = yield* Effect.all([element(shell.root), element(shell.script.id)]);
    hydrateRoot(root, shell.render(yield* Schema.decodeEffect(shell.script.json)(data.textContent)));
    data.remove();
}, Effect.runSync);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydrate };
