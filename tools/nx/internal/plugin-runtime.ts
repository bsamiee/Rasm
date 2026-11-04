import { layerPosix } from '@effect/platform-node/NodePath';
import { layer } from '@effect/platform-node/NodeServices';
import { Cause, Effect, Layer, ManagedRuntime, type Scope } from 'effect';

// --- [COMPOSITION] ---------------------------------------------------------------------

const pluginRuntime = ManagedRuntime.make(Layer.merge(layer, layerPosix));
const runPlugin = <A, E>(effect: Effect.Effect<A, E, Scope.Scope | ManagedRuntime.ManagedRuntime.Services<typeof pluginRuntime>>) =>
    pluginRuntime.runPromise(
        effect.pipe(
            Effect.scoped,
            Effect.catchCause((cause) => Effect.die(new Error(Cause.pretty(cause)))),
        ),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { pluginRuntime, runPlugin };
