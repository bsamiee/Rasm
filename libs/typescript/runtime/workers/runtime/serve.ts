import { App, type Binding } from '@rasm/hosting';
import { Access, Enrollment } from '@rasm/hosting/identity';
import { Template } from '@rasm/hosting/template';
import { env } from 'cloudflare:workers';
import { Config, Effect, Layer, Option, Predicate, type Record, Redacted, Schema, SchemaGetter, SchemaTransformation, Struct } from 'effect';
import { HttpRouter, HttpServer } from 'effect/http';
import { layerTelemetry } from '#/runtime/execution';
import { Worker } from '#/runtime/worker';

// --- [TYPES] ---------------------------------------------------------------------------

interface Read extends Struct.Lambda {
    (binding: Binding): ReturnType<(typeof read)[Binding['_tag']]>;
    readonly '~lambda.out': this['~lambda.in'] extends { readonly _tag: infer T extends Binding['_tag'] } ? ReturnType<(typeof read)[T]> : never;
}

// --- [MODELS] --------------------------------------------------------------------------

const D1 = Schema.declare((value): value is D1Database => Predicate.isObject(value));
const R2 = Schema.declare((value): value is R2Bucket => Predicate.isObject(value));
const Assets = Schema.declare((value): value is Fetcher => Predicate.isObject(value));
const Pooled = Schema.declare((value): value is Hyperdrive => Predicate.isObject(value));
const Uploads = Schema.Struct({ bucket: R2, uploadPrefix: Schema.String });
const read = {
    sqlite: () => D1,
    postgres: () => Pooled.pipe(Schema.decodeTo(Schema.Redacted(Schema.String), { decode: SchemaGetter.transform(({ connectionString }) => Redacted.make(connectionString)), encode: SchemaGetter.forbiddenEncoding })),
    bucket: ({ uploads }: { readonly uploads: string }) => R2.pipe(Schema.decodeTo(Uploads, SchemaTransformation.transform({ decode: (bucket) => ({ bucket, uploadPrefix: uploads }), encode: ({ bucket }) => bucket }))),
    secret: () => Schema.String.pipe(Schema.decodeTo(Schema.Redacted(Schema.String), SchemaTransformation.transform({ decode: Redacted.make, encode: Redacted.value }))),
    text: () => Schema.String,
    json: () => Schema.Json,
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const serveWorker = <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R extends Layer.Services<Parameters<typeof Worker.fetch>[0]> | Layer.Success<typeof HttpServer.layerServices> | Enrollment | Template | HttpRouter.Request<'Requires', Enrollment> | HttpRouter.Request<'Requires', Template>>(
    app: App<E>,
    routes: (env: Schema.Struct<{ readonly [K in keyof E]: Struct.Apply<Read, E[K]> }>['Type']) => Layer.Layer<A, Err, R>,
) =>
    Worker(
        layerTelemetry(),
        Worker.fetch(
            Effect.gen(function* () {
                const [[decoded, assets], access] = yield* Effect.all([Effect.all([Schema.decodeUnknownEffect(Schema.Struct(Struct.map(app.env, Struct.lambda<Read>(App.Binding.$match(read)))))(env), Schema.decodeUnknownEffect(Assets)(Reflect.get(env, App.bundle.fetcher))]), Config.option(Access.config)]);
                const provided = Layer.merge(Template.layer(assets), Enrollment.layer(app));
                return Layer.mergeAll(routes(decoded).pipe(HttpRouter.provideRequest(provided)), Option.match(access, { onNone: () => Layer.empty, onSome: (source) => source.layer(app.identity) })).pipe(Layer.provide([provided, HttpServer.layerServices]));
            }).pipe(Layer.unwrap),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { serveWorker };
