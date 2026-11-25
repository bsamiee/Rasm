import { type Array, Data, type Duration, Number, Option, Order } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Run = (typeof runs)[number];
interface Caller {
    readonly call: string;
    readonly agent: Option.Option<string>;
}
type Action = Data.TaggedEnum<{
    Shell: Caller & { readonly command: string; readonly run: Run };
    Read: Caller & { readonly path: string };
    Write: Caller & { readonly path: string; readonly content: string };
    Edit: Caller & { readonly path: string; readonly old: string; readonly new: string };
    Fetch: Caller & { readonly url: string };
    Mcp: Caller & { readonly server: string; readonly tool: string; readonly input: Readonly<Record<string, unknown>> };
    Spawn: Caller & { readonly prompt: string; readonly model: Option.Option<string> };
    Isolate: Caller & { readonly from: string };
    Task: Caller & { readonly id: string; readonly output: string; readonly resting: boolean; readonly again: Duration.Duration };
}>;
type Decision = Data.TaggedEnum<{
    Allow: { readonly notes: readonly string[] };
    Deny: { readonly reasons: Array.NonEmptyReadonlyArray<string>; readonly notes: readonly string[] };
    Rewrite: { readonly action: Action; readonly notes: readonly string[] };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const runs = ['foreground', 'background', 'monitored'] as const;
const byRun = Order.mapInput(Number.Order, (run: Run) => runs.indexOf(run));
const scopes = {
    session: () => Option.some(Option.none<string>()),
    agent: ({ agent }: Pick<Action, 'agent'>) => Option.some(agent),
    call: () => Option.none<Action['agent']>(),
};
const Action = Data.taggedEnum<Action>();
const Decision = Data.taggedEnum<Decision>();

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, byRun, Decision, scopes, type Run };
