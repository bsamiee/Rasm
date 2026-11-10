import { env, tracing } from 'cloudflare:workers';
import { errorReporter, formatLogRecord, layerOtlp, layerWithoutDefaultLogger } from '@rasm/telemetry';
import { ConfigProvider, Context, Effect, ErrorReporter, Exit, Layer, Logger, Predicate, Record, Scheduler, Scope, Tracer } from 'effect';
import { FetchHttpClient } from 'effect/http';

// --- [TYPES] ---------------------------------------------------------------------------

interface Kind<R, A = never, E = never> {
    readonly layer: Layer.Layer<A, E, R>;
    readonly handlers: (ready: () => Promise<Context.Context<R | A>>) => ExportedHandler<Cloudflare.Env>;
}

// --- [MODELS] --------------------------------------------------------------------------

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
        this.traced.setAttribute(key, Predicate.isString(value) || Predicate.isNumber(value) || Predicate.isBoolean(value) ? value : `${value}`);
    }
    override end(endTime: bigint, exit: Exit.Exit<unknown, unknown>) {
        super.end(endTime, exit);
        this.traced.setStatus({ code: Exit.isSuccess(exit) ? 'ok' : 'error' }).end();
    }
}

// --- [SERVICES] ------------------------------------------------------------------------

class Execution extends Context.Service<Execution, ExecutionContext>()('@rasm/workers/runtime/execution') {
    static readonly Invocation = Context.Reference<Layer.Layer<never>>('@rasm/workers/runtime/execution/Invocation', { defaultValue: () => Layer.empty });
    static readonly isolate = Layer.merge(Layer.succeed(Scheduler.Scheduler, new Scheduler.MixedScheduler('sync')), ConfigProvider.layer(ConfigProvider.fromUnknown(Record.filter({ ...env }, (variable) => !Predicate.isObject(variable) || Object.getPrototypeOf(variable) === Object.prototype))));
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

// --- [COMPOSITION] ---------------------------------------------------------------------

const layerTelemetry = (resource?: Parameters<typeof layerOtlp>[0]) =>
    Layer.mergeAll(
        Logger.layer([Logger.withLeveledConsole(formatLogRecord())], { mergeWithExisting: true }).pipe(Layer.provide(layerWithoutDefaultLogger)),
        ErrorReporter.layer([errorReporter], { mergeWithExisting: true }),
        Layer.succeed(Tracer.Tracer, Tracer.make({ span: (options) => new WorkersSpan(options) })),
        Layer.succeed(Execution.Invocation, layerOtlp(resource).pipe(Layer.provide(FetchHttpClient.layer), Layer.orDie)),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { Execution, type Kind, layerTelemetry };
