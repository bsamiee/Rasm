import { NodeServices } from '@effect/platform-node';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { describe, inject, it, layer } from '@effect/vitest';
import { pg_trgm } from '@electric-sql/pglite/contrib/pg_trgm';
import { unaccent } from '@electric-sql/pglite/contrib/unaccent';
import { pglite } from '@rasm/database/pg';
import { Effect, FileSystem, Layer, Path } from 'effect';
import { cases } from '#/tests/typescript/libs/data/database/cases';

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = [inject('artifacts'), 'sql', 'postgres'] as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('sqlite', () =>
    cases(
        it,
        (options) => SqliteClient.layer({ filename: ':memory:', ...options }),
        () => Effect.void,
    ));
layer(
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.join(...plans);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
        }),
    ).pipe(Layer.provideMerge(NodeServices.layer)),
)('pglite', (methods) =>
    cases(
        methods,
        (options) => pglite({ dataDir: 'memory://', extensions: { pg_trgm, unaccent }, ...options }),
        Effect.fnUntraced(function* (title, { preamble, statements, concurrent }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const text = ['BEGIN', ...preamble, ...statements, 'COMMIT', ...concurrent].map((statement) => `${statement};\n`).join('');
            yield* fs.writeFileString(path.join(...plans, `${title.toLowerCase().replaceAll(/[^a-z0-9]+/gu, '-')}.sql`), text, { flag: 'a' });
        }),
    ),
);
