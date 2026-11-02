import { Unmatched } from '@rasm/search/unmatched';
import type { Manifest } from '@rasm/uxp/manifest';
import { Effect, Layer, Option, Schema } from 'effect';
import { Rpc, RpcGroup, RpcMiddleware, RpcServer } from 'effect/rpc';
import { Model } from '#/dom/model';
import { Reference } from '#/dom/specifier';
import { Mutation, preserve } from '#/dom/transactions';
import { InDesignError } from '#/error';
import { Change, Create, Remove } from '#/requests/change';
import { ExecuteScript } from '#/requests/execute-script';
import { FindChange } from '#/requests/find-change';
import { Font } from '#/requests/font';
import { Inspect } from '#/requests/inspect';
import { Kind } from '#/requests/kind';
import { Invoke, Menu } from '#/requests/menu';
import { Preflight } from '#/requests/preflight';
import { Render } from '#/requests/render';

// --- [CONSTANTS] -----------------------------------------------------------------------

const manifest = {
    host: { app: 'ID', minVersion: '18.5.0' },
    requiredPermissions: { localFileSystem: Option.some('fullAccess') },
} as const satisfies Pick<Parameters<typeof Manifest.make>[0], 'host' | 'requiredPermissions'>;

// --- [ERRORS] --------------------------------------------------------------------------

const error = Schema.Union([Unmatched, InDesignError, Schema.ErrorInstance()]);

// --- [SERVICES] ------------------------------------------------------------------------

class ScriptPreferences extends RpcMiddleware.Service<ScriptPreferences>()('@rasm/indesign/contract/ScriptPreferences', { error: InDesignError }) {
    static readonly layer = Layer.effect(ScriptPreferences)(
        Effect.map(Model, ({ app, MeasurementUnits, UserInteractionLevels }) => (effect, { payload }) => {
            const held = { enableRedraw: false, measurementUnit: MeasurementUnits.POINTS };
            return Effect.andThen(preserve(app.scriptPreferences, Schema.is(Mutation)(payload) ? held : { ...held, userInteractionLevel: UserInteractionLevels.NEVER_INTERACT }), effect);
        }),
    );
}

// --- [COMPOSITION] ---------------------------------------------------------------------

const Contract = RpcGroup.make(
    Rpc.make('inspect', { payload: Inspect, success: Inspect.Result, error }),
    Rpc.make('change', { payload: Change, success: Change.Result, error }),
    Rpc.make('create', { payload: Create, success: Reference, error }),
    Rpc.make('remove', { payload: Remove, success: Reference, error }),
    Rpc.make('execute_script', { payload: ExecuteScript, success: ExecuteScript.Result, error }),
    Rpc.make('find_change', { payload: FindChange, success: FindChange.Result, error }),
    Rpc.make('font', { payload: Font, success: Font.Result, error }),
    Rpc.make('kind', { payload: Kind, success: Kind.Result, error }),
    Rpc.make('render', { payload: Render, success: Render.Result, error }),
    Rpc.make('menu', { payload: Menu, success: Menu.Result, error }),
    Rpc.make('invoke', { payload: Invoke, success: Menu.Action, error }),
    Rpc.make('preflight', { payload: Preflight, success: Preflight.Result, error }),
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
const server = (indesign: typeof import('adobe:indesign')) => RpcServer.layer(Contract).pipe(Layer.provide(handlers), Layer.provide(Layer.effect(Model, Model.make(indesign))));

// --- [EXPORTS] -------------------------------------------------------------------------

export { Contract, manifest, server };
