import { type Context, Effect, Exit, Layer, MutableRef, Option, Scope } from 'effect';
import { Execution, type Kind } from '#/runtime/execution';
import { fetch } from '#/runtime/fetch';
import { queue } from '#/runtime/queue';
import { scheduled } from '#/runtime/scheduled';

// --- [COMPOSITION] ---------------------------------------------------------------------

const Worker = Object.assign(
    <R, E, K, const A extends readonly unknown[]>(layer: Layer.Layer<R, E>, ...kinds: { readonly [I in keyof A]: Kind<R, A[I], K> }) => {
        const isolate = Layer.provideMerge(Layer.mergeAll(Layer.empty, ...kinds.map((kind) => kind.layer)), Layer.provideMerge(layer, Execution.isolate));
        const shared = MutableRef.make(Option.none<Context.Context<Layer.Success<typeof isolate>>>());
        const built = Effect.gen(function* () {
            const scope = yield* Scope.make();
            const context = yield* Effect.onError(Layer.buildWithScope(isolate, scope), (cause) => Scope.close(scope, Exit.failCause(cause)));
            const kept = Option.getOrElse(
                MutableRef.updateAndGet(
                    shared,
                    Option.orElseSome(() => context),
                ),
                () => context,
            );
            yield* kept === context ? Effect.void : Scope.close(scope, Exit.void);
            return kept;
        });
        const ready = () => Effect.runPromise(Option.match(MutableRef.get(shared), { onSome: Effect.succeed, onNone: () => built }));
        return kinds.reduce<ExportedHandler<Cloudflare.Env>>((handlers, kind) => Object.assign(handlers, kind.handlers(ready)), {});
    },
    { fetch, scheduled, queue },
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Worker };
