import { runMain } from '@effect/platform-node/NodeRuntime';
import { SqliteClient } from '@effect/sql-sqlite-node';
import { Catalog } from '@rasm/database';
import { Sqlite } from '@rasm/database/sqlite';
import { serveNode } from '@rasm/hosting/node';
import { s3Storage } from '@rasm/object-storage/s3';
import { Layer } from 'effect';
import hosting from '#/hosting.config';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

runMain(
    serveNode(hosting, (env) =>
        app(
            Layer.mergeAll(
                Catalog.layer(
                    catalog,
                    Sqlite.layer((names) => SqliteClient.layer({ filename: env.DB, ...names })),
                ),
                s3Storage(env.BUCKET, { forcePathStyle: true }),
            ),
        ),
    ),
);
