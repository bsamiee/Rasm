import { layer } from '@effect/platform-node/NodeCrypto';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { PgClient } from '@effect/sql-pg';
import { serveNode } from '@rasm/hosting/node';
import { Layer } from 'effect';
import { PersistedQueue } from 'effect/persistence';
import hosting from '#/hosting.config';
import { routes } from '#/senders';

// --- [COMPOSITION] ---------------------------------------------------------------------

runMain(serveNode(hosting, (env) => routes.pipe(Layer.provide([layer, Layer.provide(Layer.merge(PersistedQueue.layer, PersistedQueue.layerCleanup()), PersistedQueue.layerStoreSql())]), Layer.provide(PgClient.layer({ url: env.POSTGRES_URL })))));
