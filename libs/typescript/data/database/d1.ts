import { D1Client } from '@effect/sql-d1';
import { Effect, Layer } from 'effect';
import { Column } from '#/column';
import { Engine } from '#/engine';
import { Sqlite } from '#/sqlite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const D1 = {
    layer: (config: Omit<D1Client.D1ClientConfig, keyof typeof Column.transforms>) =>
        Layer.effect(
            Engine,
            D1Client.D1Client.useSync((client) => ({ ...Sqlite, transactional: false, open: () => Effect.void, atomic: client.batch })),
        ).pipe(Layer.provideMerge(D1Client.layer({ ...config, ...Column.transforms }))),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { D1 };
