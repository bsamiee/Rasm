import { dirname } from 'node:path';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Effect, FileSystem, Layer } from 'effect';
import { SqlError } from 'effect/sql';
import { type Dialect, Engine, Rows } from '#/engine';
import { Sqlite } from '#/sqlite';

// --- [COMPOSITION] ---------------------------------------------------------------------

const sqliteNodeEngine = (config: Omit<SqliteClient.SqliteClientConfig, keyof Dialect['names']>, names: Dialect['names'] = Engine.names.snake) =>
    Layer.mergeAll(Layer.succeed(Engine, { ...Sqlite, names, plan: Sqlite.plan([]) }), Layer.succeed(Rows, Sqlite), SqliteClient.layer({ ...config, ...names })).pipe(
        Layer.provide(
            FileSystem.FileSystem.use((files) => files.makeDirectory(dirname(config.filename), { recursive: true })).pipe(
                Effect.mapError((cause) => SqlError.SqlError.make({ reason: SqlError.ConnectionError.make({ cause, message: 'Failed to open database', operation: 'connect' }) })),
                Effect.when(Effect.succeed(config.filename !== ':memory:' && config.readonly !== true)),
                Layer.effectDiscard,
            ),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { sqliteNodeEngine };
