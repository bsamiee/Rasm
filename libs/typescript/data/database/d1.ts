import { D1Client } from '@effect/sql-d1';
import { Effect, Layer } from 'effect';
import { type Dialect, Engine, Rows } from '#/engine';
import { Sqlite } from '#/sqlite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const d1Engine = (config: Omit<D1Client.D1ClientConfig, keyof Dialect['names']>, names: Dialect['names'] = Engine.names.snake) =>
    Layer.merge(
        Layer.effect(
            Engine,
            D1Client.D1Client.useSync((client) => ({ ...Sqlite, names, transactional: false, open: () => Effect.void, execute: client.batch })),
        ),
        Layer.effect(
            Rows,
            D1Client.D1Client.useSync((client) => ({ ...Sqlite, atomic: client.batch })),
        ),
    ).pipe(Layer.provideMerge(D1Client.layer({ ...config, ...names })));

// --- [EXPORTS] -------------------------------------------------------------------------

export { d1Engine };
