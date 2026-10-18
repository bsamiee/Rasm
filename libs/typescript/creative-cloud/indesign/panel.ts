import indesign from 'adobe:indesign';
import { linked } from '@rasm/uxp/runtime';
import { Effect, Layer, Option, Record } from 'effect';
import { Rpc, RpcMiddleware } from 'effect/rpc';
import { Enter, InDesign, PATH } from './contract.ts';
import { Handoff, Model, transacted } from './dom.ts';
import { execute } from './execute.ts';
import { dialect, find, prompt, queries } from './find.ts';
import { face, subset } from './fonts.ts';
import { described, menu, preflight, state } from './inspect.ts';
import { render, Scratch } from './render.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/panel/Hold') {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = linked(
    InDesign.omit(Enter._tag).middleware(Hold).add(Enter),
    PATH,
    Layer.mergeAll(
        InDesign.toLayer({
            enter: () => Rpc.fork(Effect.flatMap(Handoff, ({ enter }) => enter)),
            inspect: ({ target, fields }) => Option.match(target, { onNone: state, onSome: (named) => described(named, fields) }),
            execute,
            render: ({ target }) => transacted('Rendering', false, render(target)),
            find_change: (payload) => {
                const listed = 200;
                return find(payload, listed);
            },
            font: face,
            subset,
            menu,
            preflight,
            queries,
            grep_reference: () => dialect,
            grep: prompt,
        }),
        Layer.succeed(Hold)((effect) => {
            const { app, MeasurementUnits, UserInteractionLevels } = indesign;
            const values = { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
            return Effect.andThen(
                Effect.acquireRelease(
                    Effect.sync(() => {
                        const saved = Record.map(values, (_, key) => app.scriptPreferences[key]);
                        Object.assign(app.scriptPreferences, values);
                        return saved;
                    }),
                    (saved) => Effect.sync(() => Object.assign(app.scriptPreferences, saved)),
                ),
                effect,
            );
        }),
    ).pipe(Layer.provide([Model.layer, Handoff.layer, Scratch.layer])),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { served };
