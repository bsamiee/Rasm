import { HostError } from '@rasm/automation/requests';
import { Link } from '@rasm/uxp/link';
import { Uxp } from '@rasm/uxp/runtime';
import { Effect, Layer, Schema, Scope, Semaphore } from 'effect';
import { InDesign } from '#/contract';
import { Model } from '#/dom/model';
import { requests } from '#/requests';

// --- [OPERATIONS] ----------------------------------------------------------------------

const preferences = Effect.flatMap(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => Uxp.preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));

// --- [COMPOSITION] ---------------------------------------------------------------------

const host = (indesign: typeof import('adobe:indesign')) => {
    const [model, permit] = [Effect.runSync(Model.make(indesign)), Semaphore.makeUnsafe(1)];
    const scripted = <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scopedWith((scope) => Effect.andThen(Scope.provide(preferences, scope), effect)).pipe(permit.withPermit, Effect.provideService(Model, model));
    const server = Layer.merge(InDesign.Contract.toLayer(requests), Layer.succeed(InDesign.ScriptPreferences)(scripted)).pipe(Layer.provide(Layer.succeed(Model)(model)));
    const application = Effect.tryPromise({ try: () => indesign.app.fullName, catch: HostError.refused('application fullName') }).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Uxp.Entry)),
        Effect.map(({ url }) => new URL(url)),
    );
    return { linked: Link.dial(InDesign.Contract, server, application), scripted };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { host };
