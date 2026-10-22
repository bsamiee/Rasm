import { Effect, Schema } from 'effect';
import type { ReactNode } from 'react';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from './server.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydratePage = Effect.fnUntraced(function* <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, render: (value: S['Type']) => ReactNode) {
    const [root, data] = yield* Effect.all([Effect.fromNullishOr(document.querySelector(`#${CSS.escape(shell.root)}`)), Effect.fromNullishOr(document.querySelector(`#${CSS.escape(shell.data)}`))]);
    hydrateRoot(root, render(yield* Schema.decodeUnknownEffect(Schema.fromJsonString(shell.schema))(data.textContent)));
    data.remove();
}, Effect.runSync);

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydratePage };
