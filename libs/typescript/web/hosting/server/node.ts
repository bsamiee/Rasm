import { createServer } from 'node:http';
import { dirname, join } from 'node:path';
import process from 'node:process';
import { layer as fileSystem } from '@effect/platform-node/NodeFileSystem';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import { layerNodeFromConfig } from '@rasm/telemetry/node';
import { Config, ConfigProvider, Effect, Layer, type Record } from 'effect';
import { HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { App, type Binding } from '#/app/app';
import { hosted, type Routes, type Runtime, type Served } from '#/server/hosted';

// --- [OPERATIONS] ----------------------------------------------------------------------

const runtime: Runtime = {
    sqlite: (variable) => Effect.map(Effect.all([Config.String(variable), Effect.promise(() => import('@rasm/database/sqlite-node'))]), ([filename, { sqliteNodeEngine }]) => Layer.provide(sqliteNodeEngine({ filename }), fileSystem)),
    bucket: (variable) => Effect.map(Effect.all([Config.String(variable), Effect.promise(() => import('@rasm/object-storage/s3'))]), ([bucket, { s3Storage }]) => s3Storage(bucket)),
    postgres: Config.Redacted,
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const serve = <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R extends Served>(app: App<E>, routes: Routes<E, A, Err, R>) =>
    HttpRouter.serve(
        Effect.fromNullishOr(process.argv[1]).pipe(
            Effect.flatMap((script) => HttpStaticServer.make({ root: join(dirname(script), App.bundle.assets) })),
            Effect.map((files) => Layer.mergeAll(hosted(app, runtime, routes, { fetch: HttpEffect.toWebHandler(files) }), HttpRouter.add('GET', App.bundle.readiness, HttpServerResponse.empty().pipe(Effect.succeed, HttpMiddleware.withLoggerDisabled)), HttpRouter.add('GET', '/*', files))),
            Layer.unwrap,
        ),
    ).pipe(
        Layer.provide([HttpMiddleware.layerTracerDisabledForUrls([App.bundle.readiness]), layerConfig(createServer, { port: Config.Port(App.bundle.portVariable) })]),
        Layer.provide(layerNodeFromConfig({ resource: { serviceName: app.name }, target: 'stderr' }).pipe(Layer.provide(ConfigProvider.layer(ConfigProvider.fromEnv().pipe(ConfigProvider.nested(app.name), ConfigProvider.constantCase, ConfigProvider.orElse(ConfigProvider.fromEnv())))), Layer.provide(fileSystem))),
        Layer.launch,
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { serve };
