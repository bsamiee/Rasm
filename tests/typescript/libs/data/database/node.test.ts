import { NodeServices } from '@effect/platform-node';
import { PgliteClient } from '@effect/sql-pglite';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { describe, it, layer } from '@effect/vitest';
import { pg_trgm } from '@electric-sql/pglite/contrib/pg_trgm';
import { Effect, FileSystem, Function, Layer, Path } from 'effect';
import { cases } from './cases.ts';
import type { names } from './catalog.ts';
import { parsers } from './pg.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = { directory: ['..', '..', '..', '..', '.artifacts', 'sql', 'postgres'], separator: /[^a-z0-9]+/gu };

// --- [COMPOSITION] ---------------------------------------------------------------------

describe('sqlite', () =>
    cases(
        it,
        (options) => SqliteClient.layer({ filename: ':memory:', ...options }),
        Function.identity,
        () => Effect.void,
    ));
layer(
    Layer.effectDiscard(
        Effect.gen(function* () {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const folder = path.resolve(import.meta.dirname, ...plans.directory);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
        }),
    ).pipe(Layer.provide(NodeServices.layer)),
)('pglite', (methods) =>
    cases(
        methods,
        (options: typeof names) => PgliteClient.layer({ dataDir: 'memory://', extensions: { pg_trgm }, ...options, parsers }),
        Function.identity,
        Effect.fnUntraced(
            function* (title, { preamble, statements, concurrent }) {
                const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const text = ['BEGIN', ...preamble, ...statements, 'COMMIT', ...concurrent].map((statement) => `${statement};\n`).join('');
                yield* fs.writeFileString(path.resolve(import.meta.dirname, ...plans.directory, `${title.toLowerCase().replaceAll(plans.separator, '-')}.sql`), text, { flag: 'a' });
            },
            Effect.provide(NodeServices.layer),
            Effect.orDie,
        ),
    ),
);
