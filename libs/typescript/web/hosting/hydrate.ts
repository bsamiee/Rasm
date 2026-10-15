import { Effect, Schema } from 'effect';
import type { ReactNode } from 'react';
import { hydrateRoot } from 'react-dom/client';
import type { Shell } from './render.ts';

// --- [OPERATIONS] ----------------------------------------------------------------------

const hydratePage = <S extends Schema.Codec<unknown, unknown>>(shell: Shell<S>, render: (value: S['Type']) => ReactNode): void => {
    const root = document.querySelector(`#${CSS.escape(shell.root)}`);
    const data = document.querySelector(`#${CSS.escape(shell.data)}`);
    if (root !== null) {
        hydrateRoot(root, render(Effect.runSync(Schema.decodeUnknownEffect(Schema.fromJsonString(shell.schema))(data?.textContent))));
        data?.remove();
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { hydratePage };
