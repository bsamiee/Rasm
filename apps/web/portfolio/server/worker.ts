import { env } from 'cloudflare:workers';
import { Catalog } from '@rasm/database';
import { d1Engine } from '@rasm/database/d1';
import { Access } from '@rasm/hosting';
import { Sites } from '@rasm/hosting/openai-sites';
import { Template } from '@rasm/hosting/template';
import { r2Storage } from '@rasm/object-storage/r2';
import { Worker } from '@rasm/workers';
import { layerTelemetry } from '@rasm/workers/execution';
import { Config, Effect, Layer, Option } from 'effect';
import { HttpServer } from 'effect/http';
import { app } from '#/server/app';
import { catalog } from '#/server/catalog';

// --- [COMPOSITION] ---------------------------------------------------------------------

const services = Layer.mergeAll(Catalog.layer(catalog, d1Engine({ db: env.DB })), r2Storage({ bucket: env.BUCKET, uploadPrefix: env.UPLOAD_PREFIX }), Template.layer(env.ASSETS)).pipe(Layer.provideMerge(layerTelemetry()));
const access = Access.config.pipe(Config.option, Effect.map(Option.match({ onNone: () => Layer.empty, onSome: (source) => source.layer(Sites.identity) })), Layer.unwrap);

export default Worker(services, Worker.fetch(Layer.merge(app(env.OWNER_EMAIL, services), access).pipe(Layer.provide(HttpServer.layerServices))));
