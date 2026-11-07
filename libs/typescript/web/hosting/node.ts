import { createServer } from 'node:http';
import { join } from 'node:path';
import { DeleteObjectCommand, S3Client, type S3ClientConfig } from '@aws-sdk/client-s3';
import { layerConfig } from '@effect/platform-node/NodeHttpServer';
import type { AssetsBinding, ConfigContext, JsonBinding } from 'cf/config';
import { Config, ConfigProvider, Effect, Layer, Logger, Record, Result, Schema, Struct } from 'effect';
import { FetchHttpClient, HttpEffect, HttpMiddleware, HttpRouter, HttpServerResponse, HttpStaticServer } from 'effect/http';
import { Otlp, OtlpSerialization } from 'effect/observability';
import { s3Store, s3UploadStore } from 'partial-content/s3';
import { hostingMode } from '#/cloudflare';
import { HostingError } from '#/error';
import { nodeBundle } from '#/node-bundle';
import { ObjectStorage } from '#/object-storage';
import { Template } from '#/template';

// --- [TYPES] ---------------------------------------------------------------------------

interface Field extends Struct.Lambda {
    (binding: { readonly type: string }): typeof Json | typeof Schema.String;
    readonly '~lambda.out': this['~lambda.in'] extends JsonBinding ? typeof Json : typeof Schema.String;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const SERVED = { mode: hostingMode.server, isPreview: false };

// --- [MODELS] --------------------------------------------------------------------------

const Json = Schema.fromJsonString(Schema.Unknown);

// --- [OPERATIONS] ----------------------------------------------------------------------

const s3Storage = Effect.fnUntraced(function* (bucket: string, options: S3ClientConfig) {
    const client = yield* Effect.acquireRelease(
        Effect.sync(() => new S3Client(options)),
        (opened) => Effect.sync(() => opened.destroy()),
    );
    const uploads = s3UploadStore({ client, bucket });
    return {
        objects: s3Store({ client, bucket, reservedPrefix: uploads.uploadPrefix }),
        uploads,
        delete: (key: string) => Effect.tryPromise({ try: (signal) => client.send(new DeleteObjectCommand({ Bucket: bucket, Key: key }), { abortSignal: signal }), catch: HostingError.storage('delete', key) }).pipe(Effect.asVoid),
    };
}, Layer.effect(ObjectStorage));

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveNode = Effect.fnUntraced(
    function* <B extends Record.ReadonlyRecord<string, { readonly type: string }>, A, E, R>(config: { readonly worker: (context: ConfigContext) => { readonly name: string; readonly env: B } }, app: (env: Schema.Struct<{ [K in keyof B]: Struct.Apply<Field, B[K]> }>['Type']) => Layer.Layer<A, E, R>) {
        const declared = config.worker(SERVED).env;
        const root = join(import.meta.dirname, nodeBundle.layout.assets);
        const [env, files] = yield* Effect.all([
            Config.schema(
                Schema.Struct(
                    Struct.map(
                        declared,
                        Struct.lambda<Field>(({ type }) => (type === ('json' satisfies JsonBinding['type']) ? Json : Schema.String)),
                    ),
                ),
            ).pipe(Effect.provide(ConfigProvider.layerAdd(ConfigProvider.fromUnknown(Record.filterMap(declared, ({ type }) => (type === ('assets' satisfies AssetsBinding['type']) ? Result.succeed(root) : Result.failVoid))), { asPrimary: true }))),
            HttpStaticServer.make({ root }),
        ]);
        return HttpRouter.serve(
            Layer.mergeAll(
                app(env),
                HttpRouter.add('GET', nodeBundle.layout.readiness, HttpMiddleware.withLoggerDisabled(Effect.succeed(HttpServerResponse.empty()))),
                HttpRouter.add('GET', '/*', files),
                HttpRouter.middleware(
                    Effect.map((response: HttpServerResponse.HttpServerResponse) => (response.body._tag === 'Raw' && response.body.body instanceof Response ? HttpServerResponse.fromWeb(HttpServerResponse.toWeb(response)) : response)),
                    { global: true },
                ),
            ),
        ).pipe(Layer.provide([Template.layer({ fetch: HttpEffect.toWebHandler(files) }), HttpMiddleware.layerTracerDisabledForUrls([nodeBundle.layout.readiness])]));
    },
    Layer.unwrap,
    (layer, config) =>
        layer.pipe(
            Layer.provide([
                layerConfig(createServer, { port: Config.Port(nodeBundle.layout.portVariable) }),
                Otlp.layerFromConfig({ resource: { serviceName: config.worker(SERVED).name } }).pipe(Layer.provide([FetchHttpClient.layer, OtlpSerialization.layerProtobuf]), Layer.provideMerge(Logger.layer([Logger.consoleJson, Logger.tracerLogger]))),
            ]),
        ),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { s3Storage, serveNode };
