import indesign from 'adobe:indesign';
import { Effect, Layer, Schema } from 'effect';
import { RpcServer } from 'effect/rpc';
import { Model } from '../model.ts';
import { Mutation, preserve } from '../transactions.ts';
import { Hold, InDesign } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hold = Layer.effect(Hold)(
    Effect.map(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => (effect, { payload }) => {
        const held = { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
        return Effect.andThen(preserve(app.scriptPreferences, Schema.is(Mutation)(payload) ? held : { ...held, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }), effect);
    }),
);
const handlers = InDesign.toLayer({
    inspect: (payload) => payload.run(),
    change: (payload) => payload.run(),
    create: (payload) => payload.run(),
    remove: (payload) => payload.run(),
    execute_script: (payload) => payload.run(),
    find_change: (payload) => payload.run(),
    font: (payload) => payload.run(),
    kind: (payload) => payload.run(),
    render: (payload) => payload.run(),
    menu: (payload) => payload.run(),
    invoke: (payload) => payload.run(),
    preflight: (payload) => payload.run(),
    queries: (payload) => payload.run(),
});
const server = RpcServer.layer(InDesign).pipe(Layer.provide([handlers, hold]), Layer.provide(Layer.effect(Model, Model.make(indesign))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { server };
