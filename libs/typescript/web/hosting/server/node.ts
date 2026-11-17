import { createServer } from 'node:http';
import { dirname, join } from 'node:path';
import process from 'node:process';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Config, Effect, Layer, type Record } from 'effect';
import { HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { App, type Binding } from '#/app/app';
import { hosted, type Routes, type Runtime, type Served } from '#/server/hosted';

// --- [OPERATIONS] ----------------------------------------------------------------------

const runtime: Runtime = {
    sqlite: (variable) => Effect.map(Effect.all([Config.String(variable), Effect.promise(() => import('@effect/sql-sqlite-node')), Effect.promise(() => import('@rasm/database/sqlite'))]), ([filename, { SqliteClient }, { Sqlite }]) => Sqlite.layer((names) => SqliteClient.layer({ filename, ...names }))),
    bucket: (variable) =>
        Effect.zipWith(
            Config.String(variable),
            Effect.promise(() => import('@rasm/object-storage/s3')),
            (bucket, { s3Storage }) => s3Storage(bucket),
        ),
    postgres: Config.Redacted,
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R extends Served>(app: App<E>, routes: Routes<E, A, Err, R>) =>
    HttpRouter.serve(
        Layer.unwrap(
            Effect.map(
                Effect.flatMap(Effect.fromNullishOr(process.argv[1]), (script) => HttpStaticServer.make({ root: join(dirname(script), App.bundle.assets) })),
                (files) => Layer.mergeAll(hosted(app, runtime, routes, { fetch: HttpEffect.toWebHandler(files) }), HttpRouter.add('GET', App.bundle.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))), HttpRouter.add('GET', '/*', files)),
            ),
        ),
    ).pipe(Layer.provide([HttpMiddleware.layerTracerDisabledForUrls([App.bundle.readiness]), layerConfig(createServer, { port: Config.Port(App.bundle.portVariable) })]), Layer.provide(layerNodeFromConfig({ resource: { serviceName: app.name }, target: 'stderr' })), Layer.launch);

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
