import { HostSession } from '@rasm/atelier-protocol';
import { Peer } from '@rasm/atelier-protocol/peer';
import { adjust, call, change, execute, findChange, font, inspect, invoke, kerns, menu, Model, objectModel, open, preflight, rasterize, runs, session, typeset } from '@rasm/indesign/uxp';
import { HostError } from '@rasm/object-model';
import { Uxp } from '@rasm/uxp/plugin';
import { Effect, Layer, Schema, Scope, Semaphore } from 'effect';
import { InDesign } from '#/contract';

// --- [COMPOSITION] ---------------------------------------------------------------------

const link = Effect.gen(function* () {
    const [model, permit] = yield* Effect.all([Model.make, Semaphore.make(1)]);
    const scripted = <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scopedWith((scope) => Effect.andThen(Scope.provide(session, scope), effect)).pipe(Semaphore.withPermit(permit), Effect.provideService(Model, model));
    const server = Layer.merge(InDesign.toLayer({ inspect, change, call, find_change: findChange, font, menu, invoke, preflight, adjust, kerns, runs, typeset, execute_script: execute, open, rasterize, object_model: objectModel }), Layer.succeed(HostSession)(scripted)).pipe(Layer.provide(Layer.succeed(Model)(model)));
    const application = Effect.tryPromise({ try: () => model.app.fullName, catch: HostError.refused('application fullName') }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Entry)),
        Effect.map(({ url }) => new URL(url)),
    );
    return { linked: Peer.dial(InDesign, server, application), scripted };
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { link };
