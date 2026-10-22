import { NodeServices } from '@effect/platform-node';
import { PgliteClient } from '@effect/sql-pglite';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { describe, inject, it, layer } from '@effect/vitest';
import { pg_trgm } from '@electric-sql/pglite/contrib/pg_trgm';
import { unaccent } from '@electric-sql/pglite/contrib/unaccent';
import type { transforms } from '@rasm/database/lexical.ts';
import { parsers } from '@rasm/database/pg.ts';
import { Effect, FileSystem, Layer, Path } from 'effect';
import { cases } from './cases.ts';

// --- [TYPES] ---------------------------------------------------------------------------

declare module 'vitest' {
    interface ProvidedContext {
        readonly artifacts: string;
    }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const plans = { directory: [inject('artifacts'), 'sql', 'postgres'], separator: /[^a-z0-9]+/gu };

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
            const folder = path.join(...plans.directory);
            yield* fs.remove(folder, { recursive: true, force: true });
            yield* fs.makeDirectory(folder, { recursive: true });
        }),
    ).pipe(Layer.provideMerge(NodeServices.layer)),
)('pglite', (methods) =>
    cases(
        methods,
        (options: typeof transforms) => PgliteClient.layer({ dataDir: 'memory://', extensions: { pg_trgm, unaccent }, ...options, parsers }),
        Effect.fnUntraced(function* (title, { preamble, statements, concurrent }) {
            const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
            const text = ['BEGIN', ...preamble, ...statements, 'COMMIT', ...concurrent].map((statement) => `${statement};\n`).join('');
            yield* fs.writeFileString(path.join(...plans.directory, `${title.toLowerCase().replaceAll(plans.separator, '-')}.sql`), text, { flag: 'a' });
        }, Effect.orDie),
    ),
);
