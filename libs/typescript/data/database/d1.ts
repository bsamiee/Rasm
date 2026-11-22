import { D1Client } from '@effect/sql-d1';
import { Context, Effect, Layer } from 'effect';
import { type Dialect, Engine, Rows } from '#/engine.ts';
import { Sqlite } from '#/sqlite.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const d1Engine = (config: Omit<D1Client.D1ClientConfig, keyof Dialect['names']>, names: Dialect['names'] = Engine.names.snake) =>
    D1Client.D1Client.useSync((client) => Context.make(Engine, { ...Sqlite, names, plan: Sqlite.plan(['_cf_*']), transactional: false, open: () => Effect.void, execute: client.batch }).pipe(Context.add(Rows, { ...Sqlite, atomic: client.batch }))).pipe(
        Layer.effectContext,
        Layer.provideMerge(D1Client.layer({ ...config, ...names })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { d1Engine };
