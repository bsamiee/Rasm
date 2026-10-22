import indesign from 'adobe:indesign';
import { InDesign } from '@rasm/indesign/contract';
import { Model } from '@rasm/indesign/model';
import { Mutation, preserve } from '@rasm/indesign/transactions';
import { script } from '@rasm/uxp/entry';
import { Effect, Layer, Schema } from 'effect';
import { RpcMiddleware, RpcServer } from 'effect/rpc';

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/galley/main/Hold', { error: Schema.ErrorInstance() }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Served = InDesign.middleware(Hold);
const hold = Layer.effect(Hold)(
    Effect.map(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => (effect, { payload }) => {
        const held = { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
        return Effect.andThen(preserve(app.scriptPreferences, Schema.is(Mutation)(payload) ? held : { ...held, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }), effect);
    }),
);
const handlers = Served.toLayer({
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
});

script(RpcServer.layer(Served).pipe(Layer.provide([handlers, hold]), Layer.provide(Layer.effect(Model, Model.make(indesign)))));
