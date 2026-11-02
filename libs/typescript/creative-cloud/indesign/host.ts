import { error, HostError, Requests } from '@rasm/automation/requests';
import { Effect, Layer, Schema } from 'effect';
import { Url } from 'effect/http';
import { Rpc, RpcMiddleware } from 'effect/rpc';
import { open } from '#/documents/document';
import { Model } from '#/dom/model';
import { preserve } from '#/dom/transactions';
import { Call } from '#/requests/call';
import { Change } from '#/requests/change';
import { executeScript } from '#/requests/execute-script';
import { FindChange } from '#/requests/find-change';
import { Font } from '#/requests/font';
import { Inspect } from '#/requests/inspect';
import { Invoke, Menu } from '#/requests/menu';
import { objectModel } from '#/requests/object-model';
import { Preflight } from '#/requests/preflight';
import { render } from '#/requests/render';

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
).middleware(ScriptPreferences);
const host = (indesign: typeof import('adobe:indesign')) => ({
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
            execute_script: executeScript,
            open,
            render,
            object_model: objectModel,
        }),
        Layer.effect(ScriptPreferences)(
            Effect.map(
                Model,
                ({ app, MeasurementUnits, UserInteractionLevels }) =>
                    (effect) =>
                        Effect.andThen(preserve(app.scriptPreferences, { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }), effect),
            ),
        ),
    ).pipe(Layer.provide(Layer.effect(Model, Model.make(indesign)))),
    application: Effect.tryPromise(() => indesign.app.fullName).pipe(
        Effect.flatMap(Schema.decodeUnknownEffect(Schema.Struct({ nativePath: Schema.String }))),
        Effect.flatMap(({ nativePath }) => Effect.fromResult(Url.fromString(encodeURI(`file://${nativePath}`)))),
    ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, host };
