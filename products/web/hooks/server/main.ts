import { layer } from '@effect/platform-node/NodeCrypto';
import { PgClient } from '@effect/sql-pg';
import { serve } from '@rasm/hosting/server';
import { Layer } from 'effect';
import { PersistedQueue } from 'effect/persistence';
import hosting from '#/hosting.config';
import { routes } from '#/server/senders';

// --- [COMPOSITION] ---------------------------------------------------------------------

export default serve(hosting, ({ POSTGRES_URL }) => routes.pipe(Layer.provide([layer, Layer.provide(Layer.merge(PersistedQueue.layer, PersistedQueue.layerCleanup()), PersistedQueue.layerStoreSql())]), Layer.provide(PgClient.layer({ url: POSTGRES_URL }))));
