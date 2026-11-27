import { InterfaceMap } from '@rasm/applications/map';
import { Output } from '@rasm/mcp/cli';
import { type Handle, Server } from '@rasm/mcp/server';
import { Array, Context, Effect, Layer, Record } from 'effect';
import { Toolkit } from 'effect/ai';
import { Act } from '#/act';
import { Applications } from '#/applications';
import { Diagnostics } from '#/diagnostics';
import { ExecuteScript } from '#/execute-script';
import { Address } from '#/handle/address';
import { Notice } from '#/handle/notice';
import { Target } from '#/handle/target';
import { Inspect } from '#/inspect';
import { View } from '#/view';

// --- [CONSTANTS] -----------------------------------------------------------------------

const SEAMS = { script: ExecuteScript.Script, render: View.Render, logs: Diagnostics.Logs, source: Address.Linked, toolbar: Address.Toolbar } as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const bind = <P, A, E, R>(run: (given: P) => Effect.Effect<A, E, R | Handle>) => Effect.map(Effect.context<R>(), (built) => (given: P) => Effect.updateContext(run(given), (request: Context.Context<Handle>) => Context.merge(built, request)));

// --- [COMPOSITION] ---------------------------------------------------------------------

const tools = Toolkit.make(Applications.tool, View.tool, Inspect.tool, Act.tool, ExecuteScript.tool, Diagnostics.tool);
const runs = tools.toLayer(
    Effect.all({
        applications: bind(Applications.run),
        view: bind(View.run),
        inspect: bind(Inspect.run),
        act: bind(Act.run),
        execute_script: bind(ExecuteScript.run),
        diagnostics: bind(Diagnostics.run),
    }),
);
const ApplicationTools = Object.assign(tools, {
    settings: [View.Side] as const,
    layer: ({ descriptions = {}, ...seams }: { readonly descriptions?: Readonly<Partial<Record<Exclude<keyof typeof tools.tools, typeof ExecuteScript.tool.name>, string>>> } & { readonly [Name in keyof typeof SEAMS]?: Context.Service.Shape<(typeof SEAMS)[Name]> } = {}) =>
        Layer.merge(Server.toolkit(tools, { descriptions: { ...descriptions, execute_script: (seams.script ?? ExecuteScript.Script.defaultValue()).description } }), Notice.layer).pipe(
            Layer.provide(runs),
            Layer.provide(Target.layer),
            Layer.provide(Layer.succeedContext(Context.makeUnsafe(new Map(Record.toEntries(SEAMS).flatMap(([name, { key }]) => Array.fromNullishOr(seams[name]).map((seam) => [key, seam] as const)))))),
            Layer.provide(Layer.unwrap(Effect.map(Output, ({ state }) => InterfaceMap.layer(state)))),
        ),
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { ApplicationTools };
