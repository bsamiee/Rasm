import type { Engine, Rows } from '@rasm/database';
import type { ObjectStorage } from '@rasm/object-storage';
import { Config, Effect, Layer, Record, Schema, Struct } from 'effect';
import { HttpRouter, type HttpServer } from 'effect/http';
import type { SqlClient, SqlError } from 'effect/sql';
import { App, type Binding } from '#/app/app';
import { Enrollment, Identity } from '#/app/identity';
import { Template } from '#/rendering/template';

// --- [TYPES] ---------------------------------------------------------------------------

interface Typed extends Struct.Lambda {
    (binding: Binding): (typeof Bound)[Binding['_tag']];
    readonly '~lambda.in': Binding;
    readonly '~lambda.out': (typeof Bound)[this['~lambda.in']['_tag']];
}
type Routes<E extends Record.ReadonlyRecord<string, Binding>, A, Err, R> = (bound: Schema.Struct<{ readonly [K in keyof E]: Struct.Apply<Typed, E[K]> }>['Type']) => Layer.Layer<A, Err, R>;
type Runtime = { readonly [K in 'sqlite' | 'bucket' | 'postgres']: (variable: string) => Effect.Effect<(typeof Bound)[K]['Type'], Config.ConfigError | Schema.SchemaError> };
type Served =
    | Template
    | Enrollment
    | Identity
    | Layer.Success<typeof HttpServer.layerServices>
    | HttpRouter.HttpRouter
    | HttpRouter.Request<'Requires', Template>
    | HttpRouter.Request<'Requires', Enrollment>
    | HttpRouter.Request<'Requires', Identity>
    | HttpRouter.Request<'Error', unknown>
    | HttpRouter.Request<'GlobalError', unknown>;

// --- [MODELS] --------------------------------------------------------------------------

const Bound = {
    sqlite: Schema.declare((value): value is Layer.Layer<Engine | Rows | SqlClient.SqlClient, SqlError.SqlError | Config.ConfigError | Schema.SchemaError> => Layer.isLayer(value)),
    postgres: Schema.Redacted(Schema.String),
    bucket: Schema.declare((value): value is Layer.Layer<ObjectStorage, Config.ConfigError | Schema.SchemaError> => Layer.isLayer(value)),
    secret: Schema.Redacted(Schema.String),
    text: Schema.String,
    json: Schema.Json,
} as const;

// --- [COMPOSITION] ---------------------------------------------------------------------

const hosted = <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R>(app: App<E>, runtime: Runtime, routes: Routes<E, A, Err, R>, assets: Parameters<typeof Template.layer>[0]) => {
    const services = Layer.mergeAll(Template.layer(assets), Enrollment.layer(app.allowed), Identity.layer);
    const resolved = Record.map(app.env, (binding: Binding, variable) =>
        App.Binding.$match(binding, {
            sqlite: () => runtime.sqlite(variable),
            bucket: () => runtime.bucket(variable),
            postgres: () => runtime.postgres(variable),
            secret: () => Config.Redacted(variable),
            text: ({ value }) => Effect.succeed(value),
            json: ({ value }) => Effect.succeed(value),
        }),
    );
    const typed = Schema.Struct(
        Struct.map(
            app.env,
            Struct.lambda<Typed>((binding) => Bound[binding._tag]),
        ),
    );
    return Effect.all(resolved).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(typed)),
        Effect.map((bound) => routes(bound).pipe(HttpRouter.provideRequest(services), Layer.provide(services), Layer.merge(Identity.routes))),
        Layer.unwrap,
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { hosted, type Routes, type Runtime, type Served };
