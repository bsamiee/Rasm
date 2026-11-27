import { InterfaceMaps } from '@rasm/applications/interface-map';
import { Output } from '@rasm/mcp/cli';
import { type Handle, Server } from '@rasm/mcp/server';
import { Array, Context, Effect, Layer, Record } from 'effect';
import { Toolkit } from 'effect/ai';
import { Act } from '#/act';
import { Applications } from '#/applications';
import { Diagnostics, Logs } from '#/diagnostics';
import { ExecuteScript, Script } from '#/execute-script';
import { Source, Toolbar } from '#/handle/address';
import { Notices } from '#/handle/notices';
import { Target } from '#/handle/target';
import { Inspect } from '#/inspect';
import { Render, Side, View } from '#/view';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEAMS = { script: Script, render: Render, logs: Logs, source: Source, toolbar: Toolbar } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const bind = <P, A, E, R>(handle: (given: P) => Effect.Effect<A, E, R | Handle>) => Effect.map(Effect.context<R>(), (built) => (given: P) => Effect.updateContext(handle(given), (request: Context.Context<Handle>) => Context.merge(built, request)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Toolkit.make(Applications.tool, View.tool, Inspect.tool, Act.tool, ExecuteScript.tool, Diagnostics.tool);
const handlers = tools.toLayer(
    Effect.all({
        applications: bind(Applications.handle),
        view: bind(View.handle),
        inspect: bind(Inspect.handle),
        act: bind(Act.handle),
        execute_script: bind(ExecuteScript.handle),
        diagnostics: bind(Diagnostics.handle),
    }),
);
const ApplicationTools = Object.assign(tools, {
    settings: [Side] as const,
    layer: ({ descriptions = {}, ...seams }: { readonly descriptions?: Readonly<Partial<Record<keyof typeof tools.tools, string>>> } & { readonly [Name in keyof typeof SEAMS]?: Context.Service.Shape<(typeof SEAMS)[Name]> } = {}) =>
        Layer.merge(Server.toolkit(tools, { descriptions: { execute_script: ExecuteScript.clause, ...descriptions } }), Notices.layer).pipe(
            Layer.provide(handlers),
            Layer.provide(Target.layer),
            Layer.provide(Layer.succeedContext(Context.makeUnsafe(new Map(Record.toEntries(SEAMS).flatMap(([name, { key }]) => Array.fromNullishOr(seams[name]).map((seam) => [key, seam] as const)))))),
            Layer.provide(Layer.unwrap(Effect.map(Output, ({ state }) => InterfaceMaps.layer(state)))),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationTools };
