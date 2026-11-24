import { d1Engine } from '@rasm/database/d1';
import { r2Storage } from '@rasm/object-storage/r2';
import { layer } from '@rasm/telemetry';
import { env, tracing } from 'cloudflare:workers';
import { Config, ConfigProvider, Context, DateTime, Duration, Effect, Exit, Formatter, Layer, Predicate, Record, Redacted, Scheduler, Schema, Scope, Struct, Tracer } from 'effect';
import { FetchHttpClient, HttpEffect, HttpRouter, HttpServer, type HttpServerRequest } from 'effect/http';
import { App, type Binding } from '#/app/app';
import { hosted, type Routes, type Served } from '#/server/hosted';

// --- [TYPES] ---------------------------------------------------------------------------

interface Kind<R, A = never, E = never> {
    readonly layer: Layer.Layer<A, E, R>;
    readonly handlers: (ready: () => Promise<Context.Context<R | A>>) => ExportedHandler<Cloudflare.Env>;
}

// --- [MODELS] --------------------------------------------------------------------------

const Held = {
    sqlite: Schema.declare((value): value is D1Database => Predicate.hasProperty(value, 'prepare')),
    bucket: Schema.declare((value): value is R2Bucket => Predicate.hasProperty(value, 'head')),
    postgres: Schema.declare((value): value is Hyperdrive => Predicate.hasProperty(value, 'connectionString')),
    assets: Schema.declare((value): value is Fetcher => Predicate.hasProperty(value, 'fetch')),
} as const;

class WorkersSpan extends Tracer.NativeSpan {
    readonly traced = tracing.startSpan(this.name);

    override get traceId() {
        return this.traced.spanContext().traceId;
    }
    override get spanId() {
        return this.traced.spanContext().spanId;
    }
    override attribute(key: string, value: unknown) {
        super.attribute(key, value);
        this.traced.setAttribute(key, Predicate.isString(value) || Predicate.isNumber(value) || Predicate.isBoolean(value) ? value : Formatter.format(value));
    }
    override end(endTime: bigint, exit: Exit.Exit<unknown, unknown>) {
        super.end(endTime, exit);
        this.traced.setStatus({ code: Exit.isSuccess(exit) ? 'ok' : 'error' }).end();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Execution extends Context.Service<Execution, ExecutionContext>()('@rasm/hosting/server/workerd/Execution') {
    static readonly Invocation = Context.Reference<Layer.Layer<never>>('@rasm/hosting/server/workerd/Execution/Invocation', { defaultValue: () => Layer.empty });
    static readonly isolate = Layer.merge(Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync')), ConfigProvider.layer(ConfigProvider.fromUnknown(Record.filter({ ...env }, (variable) => !Predicate.isObject(variable) || Object.getPrototypeOf(variable) === Object.prototype))));
    static readonly telemetry = (resource?: Parameters<typeof layer>[0]) => Layer.merge(Layer.succeed(Tracer.Tracer, Tracer.make({ span: (options) => new WorkersSpan(options) })), Layer.succeed(Execution.Invocation, layer(resource).pipe(Layer.provide(FetchHttpClient.layer), Layer.orDie)));
    static readonly run =
        <R>(isolate: Context.Context<R>, execution: ExecutionContext) =>
        <A, E>(effect: Effect.Effect<A, E, R | Execution | Scope.Scope>) =>
            Effect.runPromiseWith(Context.add(isolate, Execution, execution))(
                Effect.gen(function* () {
                    const scope = yield* Scope.make();
                    const invocation = yield* Layer.buildWithScope(yield* Execution.Invocation, scope);
                    const exit = yield* Effect.exit(Effect.provide(Scope.provide(scope)(effect), invocation));
                    execution.waitUntil(Effect.runPromiseWith(isolate)(Scope.close(scope, exit)));
                    return yield* exit;
                }),
            );
    static readonly waitUntil = Effect.fnUntraced(function* <A, E, R>(effect: Effect.Effect<A, E, R | Execution | Scope.Scope>) {
        const services = yield* Effect.context<R | Execution>();
        const execution = Context.get(services, Execution);
        execution.waitUntil(Execution.run(services, execution)(effect));
    });
}

class Fetched extends Context.Service<Fetched, (request: Request, invocation: Context.Context<Execution>) => Promise<Response>>()('@rasm/hosting/server/workerd/Fetched') {}

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = <K extends keyof typeof Held>(kind: K, variable: string) => Schema.decodeUnknownEffect(Held[kind])(Reflect.get(env, variable));

// --- [COMPOSITION] ---------------------------------------------------------------------

const Worker = Object.assign(
    <R, E, K, const A extends readonly unknown[]>(provided: Layer.Layer<R, E>, ...kinds: { readonly [I in keyof A]: Kind<R, A[I], K> }) => {
        const isolate = Layer.provideMerge(Layer.mergeAll(Layer.empty, ...kinds.map((kind) => kind.layer)), Layer.provideMerge(provided, Execution.isolate));
        const built = Scope.make().pipe(
            Effect.flatMap((scope) => Effect.onError(Layer.buildWithScope(isolate, scope), (cause) => Scope.close(scope, Exit.failCause(cause)))),
            Effect.cachedWithTTL((exit) => (Exit.isSuccess(exit) ? Duration.infinity : Duration.zero)),
            Effect.runSync,
        );
        const ready = () => Effect.runPromise(built);
        return kinds.reduce<ExportedHandler<Cloudflare.Env>>((handlers, kind) => Object.assign(handlers, kind.handlers(ready)), {});
    },
    {
        fetch: <A, E>(routes: Layer.Layer<A, E, HttpRouter.HttpRouter | HttpRouter.Request<'Requires', Execution> | HttpRouter.Request<'GlobalRequires', Execution> | HttpRouter.Request<'Error', unknown> | HttpRouter.Request<'GlobalError', unknown>>): Kind<never, Fetched, HttpRouter.Request.Without<E>> => ({
            layer: Layer.effect(Fetched, Effect.map(HttpRouter.toHttpEffect(routes), HttpEffect.toWebHandlerWith<never, HttpServerRequest.HttpServerRequest | Scope.Scope | Execution>(Context.empty()))),
            handlers: (ready) => ({
                fetch: (request, _, execution) => ready().then((isolate) => Execution.run(isolate, execution)(Effect.flatMap(Effect.context<Execution>(), (invocation) => Effect.promise(() => Context.get(isolate, Fetched)(request, invocation))))),
            }),
        }),
        scheduled: <E, R>(job: (controller: ScheduledController) => Effect.Effect<unknown, E, R | Execution | Scope.Scope>): Kind<R> => ({
            layer: Layer.empty,
            handlers: (ready) => ({
                scheduled: (controller, _, execution) =>
                    ready().then((isolate) => Execution.run(isolate, execution)(Effect.asVoid(Effect.withSpan(job(controller), `scheduled ${controller.cron}`, { attributes: { 'faas.trigger': 'timer', 'faas.cron': controller.cron, 'faas.time': DateTime.formatIso(DateTime.makeUnsafe(controller.scheduledTime)) } })))),
            }),
        }),
        queue: <S extends Schema.Top, E, R>(body: S, work: (body: S['Type'], delivery: Pick<Message, 'id' | 'attempts' | 'timestamp'>) => Effect.Effect<unknown, E, R | Execution | Scope.Scope>): Kind<R | S['DecodingServices']> => ({
            layer: Layer.empty,
            handlers: (ready) => ({
                queue: (batch, _, execution) => {
                    const delivered = Effect.forEach(
                        batch.messages,
                        (message) =>
                            Schema.decodeUnknownEffect(body)(message.body).pipe(
                                Effect.flatMap((decoded) => work(decoded, message)),
                                Effect.tapCause(Effect.logError),
                                Effect.matchCause({ onFailure: () => message.retry(), onSuccess: () => message.ack() }),
                                Effect.withSpan(`process ${batch.queue}`, { kind: 'consumer', attributes: { 'messaging.operation.type': 'process', 'messaging.destination.name': batch.queue, 'messaging.message.id': message.id, 'messaging.message.delivery_attempt': message.attempts } }),
                            ),
                        { concurrency: 'unbounded', discard: true },
                    );
                    return ready().then((isolate) => Execution.run(isolate, execution)(delivered));
                },
            }),
        }),
    },
);

const serve = <const E extends Record.ReadonlyRecord<string, Binding>, A, Err, R extends Served | HttpRouter.Request<'Requires', Execution>>(app: App<E>, routes: Routes<E, A, Err, R>) => {
    const routed = Effect.all([Config.Literals(Struct.keys(App.modes), App.bundle.mode), held('assets', App.bundle.fetcher)]).pipe(
        Effect.map(([mode, assets]) =>
            hosted(
                app,
                {
                    sqlite: (variable) => Effect.map(held('sqlite', variable), (db) => d1Engine({ db })),
                    bucket: (variable) => Effect.map(held('bucket', variable), (bucket) => r2Storage({ bucket, name: App.resource(app, mode, variable) })),
                    postgres: (variable) => Effect.map(held('postgres', variable), ({ connectionString }) => Redacted.make(connectionString)),
                },
                routes,
                assets,
            ),
        ),
        Layer.unwrap,
        Layer.provide(HttpServer.layerServices),
    );
    return Worker(Execution.telemetry({ serviceName: app.name }), Worker.fetch(routed));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Execution, type Kind, serve, Worker };
