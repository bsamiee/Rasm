import indesign from 'adobe:indesign';
import { Array, Effect, Layer, Option, Record, Struct } from 'effect';
import { Rpc, RpcMiddleware } from 'effect/rpc';
import { described, menu, preflight, state } from '../document/inspect.ts';
import { Handoff, Model, preserve, read } from '../document/objects.ts';
import { render } from '../document/render.ts';
import { InDesignError } from '../models.ts';
import { execute } from '../scripting.ts';
import { face } from '../text/fonts.ts';
import { find, queries, savedQueries } from '../text/search.ts';
import { Complete, Enter, InDesign } from './protocol.ts';

// --- [SERVICES] ------------------------------------------------------------------------

class Hold extends RpcMiddleware.Service<Hold>()('@rasm/indesign/mcp/panel/Hold', { error: InDesignError }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const group = InDesign.omit(Enter._tag).middleware(Hold).add(Enter);
const identity = Effect.tryPromise({ try: () => indesign.app.fullName, catch: InDesignError.refused(Option.none()) }).pipe(Effect.map((entry) => ({ application: String(entry.url) })));
const handlers = Layer.mergeAll(
    InDesign.toLayer({
        enter: () => Rpc.fork(Effect.flatMap(Handoff, ({ enter }) => enter)),
        complete: ({ operation, parameter }) => {
            const { app } = indesign;
            const candidates: Record.ReadonlyRecord<string, Record.ReadonlyRecord<string, Effect.Effect<readonly string[], InDesignError>>> = {
                menu: { area: read(Complete.successSchema, () => Array.dedupe(app.menuActions.everyItem().area)) },
                preflight: { profile: read(Complete.successSchema, () => app.preflightProfiles.everyItem().name) },
                queries: { name: Effect.map(savedQueries, Array.map(Struct.get('name'))) },
            };
            return Option.getOrElse(Option.flatMap(Record.get(candidates, operation), Record.get(parameter)), () => Effect.succeed([]));
        },
        inspect: ({ target, fields }) => Option.match(target, { onNone: state, onSome: (named) => described(named, fields) }),
        execute,
        render: ({ target }) => render(target),
        find_change: find,
        font: face,
        menu,
        preflight,
        queries,
    }),
    Layer.succeed(Hold)((effect) => {
        const { app, MeasurementUnits, UserInteractionLevels } = indesign;
        const values = { userInteractionLevel: UserInteractionLevels.NEVER_INTERACT, enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
        return Effect.andThen(preserve(app.scriptPreferences, values), effect);
    }),
).pipe(Layer.provide([Model.layer, Handoff.layer]));

// --- [EXPORTS] -------------------------------------------------------------------------

export { group, handlers, identity };
