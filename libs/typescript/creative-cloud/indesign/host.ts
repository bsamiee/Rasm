import { error, HostError, Requests } from '@rasm/automation/requests';
import { Typesetting } from '@rasm/automation/typesetting';
import { Effect, Layer, Schema } from 'effect';
import { Url } from 'effect/http';
import { Rpc, RpcMiddleware } from 'effect/rpc';
import { open } from '#/documents/document';
import { Model } from '#/dom/model';
import { preserve } from '#/dom/transactions';
import { adjust } from '#/requests/adjust';
import { Call } from '#/requests/call';
import { Change } from '#/requests/change';
import { executeScript } from '#/requests/execute-script';
import { FindChange } from '#/requests/find-change';
import { Font } from '#/requests/font';
import { Inspect } from '#/requests/inspect';
import { kerns } from '#/requests/kerns';
import { Invoke, Menu } from '#/requests/menu';
import { objectModel } from '#/requests/object-model';
import { Preflight } from '#/requests/preflight';
import { render } from '#/requests/render';
import { typeset } from '#/requests/typeset';

// --- [SERVICES] ------------------------------------------------------------------------

class ScriptPreferences extends RpcMiddleware.Service<ScriptPreferences>()('@rasm/indesign/host/ScriptPreferences', { error: HostError }) {}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = Requests.add(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error }),
    Rpc.make('change', { payload: Change, success: Change.Result, error }),
    Rpc.make('call', { payload: Call, success: Call.Result, error }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error }),
    Rpc.make('font', { payload: Font, success: Font.Result, error }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error }),
    Rpc.make('invoke', { payload: Invoke, success: Invoke.Result, error }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error }),
)
    .merge(Typesetting)
    .middleware(ScriptPreferences);
const scripted = Effect.flatMap(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }));
const host = (indesign: typeof import('adobe:indesign')) => {
    const model = Effect.runSync(Effect.cached(Model.make(indesign)));
    return {
        server: Layer.merge(
            Contract.toLayer({
                inspect: Inspect.handle,
                change: Change.handle,
                call: Call.handle,
                find_change: FindChange.handle,
                font: Font.handle,
                menu: Menu.handle,
                invoke: Invoke.handle,
                preflight: Preflight.handle,
                adjust,
                kerns,
                typeset,
                execute_script: executeScript,
                open,
                render,
                object_model: objectModel,
            }),
            Layer.effect(ScriptPreferences)(Effect.map(Model, (service) => (effect) => Effect.andThen(Effect.provideService(scripted, Model, service), effect))),
        ).pipe(Layer.provide(Layer.effect(Model, model))),
        local: <A, E, R>(effect: Effect.Effect<A, E, R>) => Effect.scoped(Effect.andThen(scripted, effect)).pipe(Effect.provideServiceEffect(Model, model)),
        application: Effect.tryPromise(() => indesign.app.fullName).pipe(
            Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ nativePath: Schema.String }))),
            Effect.flatMap(({ nativePath }) => Effect.fromResult(Url.fromString(encodeURI(`file://${nativePath}`)))),
        ),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, host };
