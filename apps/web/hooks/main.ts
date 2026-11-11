import { createServer } from 'node:http';
import { layer } from '@effect/platform-node/NodeCrypto';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { runMain } from '@effect/platform-node/NodeRuntime';
import { PgClient } from '@effect/sql-pg';
import { App } from '@rasm/hosting/app';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Config, Effect, Layer } from 'effect';
import { HttpMiddleware, HttpRouter, HttpServerResponse } from 'effect/http';
import { PersistedQueue } from 'effect/persistence';
import { routes } from '#/senders';

// --- [COMPOSITION] ---------------------------------------------------------------------

HttpRouter.serve(Layer.merge(routes, HttpRouter.add('GET', App.bundle.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))))).pipe(
    Layer.provide(layerConfig(createServer, { port: Config.Port(App.bundle.portVariable) })),
    Layer.provide([layer, Layer.provide(Layer.merge(PersistedQueue.layer, PersistedQueue.layerCleanup()), PersistedQueue.layerStoreSql()), HttpMiddleware.layerTracerDisabledForUrls([App.bundle.readiness])]),
    Layer.provide(PgClient.layerConfig({ url: Config.Redacted('POSTGRES_URL') })),
    Layer.provide(layerNodeFromConfig({ resource: { serviceName: 'hooks' }, target: 'stderr' })),
    Layer.launch,
    runMain,
);
