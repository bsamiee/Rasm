import { Unmatched } from '@rasm/search';
import { Effect, Layer, Schema } from 'effect';
import { Rpc, RpcGroup, RpcMiddleware } from 'effect/rpc';
import { Model } from '#/dom/model';
import { Reference } from '#/dom/reference';
import { Mutation, preserve } from '#/dom/transactions';
import { Change } from '#/requests/change';
import { Create } from '#/requests/create';
import { ExecuteScript } from '#/requests/execute-script';
import { FindChange } from '#/requests/find-change';
import { Font } from '#/requests/font';
import { Inspect } from '#/requests/inspect';
import { Invoke } from '#/requests/invoke';
import { Kind } from '#/requests/kind';
import { Menu } from '#/requests/menu';
import { Preflight } from '#/requests/preflight';
import { Remove } from '#/requests/remove';
import { Render } from '#/requests/render';

// --- [SERVICES] ------------------------------------------------------------------------

class ScriptPreferences extends RpcMiddleware.Service<ScriptPreferences>()('@rasm/indesign/contract/ScriptPreferences', { error: Schema.ErrorInstance() }) {
    static readonly layer = Layer.effect(ScriptPreferences)(
        Effect.map(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => (effect, { payload }) => {
            const held = { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
            return Effect.andThen(preserve(app.scriptPreferences, Schema.is(Mutation)(payload) ? held : { ...held, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }), effect);
        }),
    );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error: Unmatched.Refusal }),
    Rpc.make('change', { payload: Change, success: Change.Result, error: Unmatched.Refusal }),
    Rpc.make('create', { payload: Create, success: Reference, error: Unmatched.Refusal }),
    Rpc.make('remove', { payload: Remove, success: Reference, error: Unmatched.Refusal }),
    Rpc.make('execute_script', { payload: ExecuteScript, success: ExecuteScript.Result, error: Unmatched.Refusal }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error: Unmatched.Refusal }),
    Rpc.make('font', { payload: Font, success: Font.Result, error: Unmatched.Refusal }),
    Rpc.make('kind', { payload: Kind, success: Kind.Result, error: Unmatched.Refusal }),
    Rpc.make('render', { payload: Render, success: Render.Result, error: Unmatched.Refusal }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error: Unmatched.Refusal }),
    Rpc.make('invoke', { payload: Invoke, success: Menu.Action, error: Unmatched.Refusal }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error: Unmatched.Refusal }),
).middleware(ScriptPreferences);
const handlers = Layer.merge(
    Contract.toLayer({
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
    }),
    ScriptPreferences.layer,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, handlers, ScriptPreferences };
