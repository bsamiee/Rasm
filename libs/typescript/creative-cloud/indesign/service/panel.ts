import indesign from 'adobe:indesign';
import { linked } from '@rasm/uxp/link';
import { Effect, Layer, Schema } from 'effect';
import { Rpc } from 'effect/rpc';
import { Model } from '../model.ts';
import { preserve, Transactions } from '../transactions.ts';
import { Hold, InDesign, PATH, WIRE } from './contract.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const hold = Layer.effect(Hold)(
    Effect.map(
        Model,
        ({ app, MeasurementUnits, UserInteractionLevels }) =>
            (effect) =>
                Effect.andThen(preserve(app.scriptPreferences, { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS }), effect),
    ),
);
const handlers = InDesign.toLayer({
    enter: () => Rpc.fork(Transactions.use(({ enter }) => enter)),
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
}).pipe(Layer.provideMerge(hold), Layer.provide(Layer.effect(Transactions, Transactions.make)));
const panel = linked(InDesign, handlers, {
    path: PATH,
    serialization: WIRE,
    application: Model.use(({ app }) =>
        Effect.flatMap(
            Effect.tryPromise(() => app.fullName),
            ({ url }) => Schema.decodeUnknownEffect(Schema.Union([Schema.URL, Schema.URLFromString]))(url),
        ),
    ),
}).pipe(Layer.provide(Layer.effect(Model, Model.make(indesign))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { panel };
