import indesign from 'adobe:indesign';
import { linked } from '@rasm/uxp/runtime';
import { Effect, Layer, Option } from 'effect';
import { InDesign, PATH } from './contract.ts';
import { held, Model, transacted } from './dom.ts';
import { execute } from './execute.ts';
import { dialect, find, prompt, queries } from './find.ts';
import { face, subset } from './fonts.ts';
import { described, menu, preflight, state } from './inspect.ts';
import { render } from './render.ts';

// --- [COMPOSITION] ---------------------------------------------------------------------

const served = linked(
    InDesign,
    { path: PATH, hold: Effect.suspend(() => held(indesign.app.scriptPreferences, { userInteractionLevel: indesign.UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: indesign.MeasurementUnits.POINTS })) },
    InDesign.omit('enter')
        .toLayer({
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
        })
        .pipe(Layer.provide(Model.layer)),
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { served };
