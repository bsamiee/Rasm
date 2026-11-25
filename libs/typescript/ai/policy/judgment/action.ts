import { type Array, Data, type Duration, Number, Option, Order } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Wait = (typeof waits)[number];
interface Asked {
    readonly call: string;
    readonly agent: Option.Option<string>;
}
type Action = Data.TaggedEnum<{
    Shell: Asked & { readonly command: string; readonly wait: Wait };
    Read: Asked & { readonly path: string };
    Write: Asked & { readonly path: string; readonly content: string };
    Edit: Asked & { readonly path: string; readonly old: string; readonly new: string };
    Fetch: Asked & { readonly url: string };
    Mcp: Asked & { readonly server: string; readonly tool: string; readonly input: Readonly<Record<string, unknown>> };
    Spawn: Asked & { readonly prompt: string; readonly model: Option.Option<string> };
    Isolate: Asked & { readonly from: string };
    Task: Asked & { readonly id: string; readonly output: string; readonly resting: boolean; readonly again: Duration.Duration };
}>;
type Judgment = Data.TaggedEnum<{
    Pass: { readonly notes: readonly string[] };
    Refuse: { readonly reasons: Array.NonEmptyReadonlyArray<string>; readonly notes: readonly string[] };
    Rewrite: { readonly action: Action; readonly notes: readonly string[] };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const waits = ['held', 'notified', 'streamed'] as const;
const byWait = Order.mapInput(Number.Order, (wait: Wait) => waits.indexOf(wait));
const lifetimes = {
    session: () => Option.some(Option.none<string>()),
    agent: ({ agent }: Pick<Action, 'agent'>) => Option.some(agent),
    call: () => Option.none<Action['agent']>(),
};
const Action = Data.taggedEnum<Action>();
const Judgment = Data.taggedEnum<Judgment>();

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, byWait, Judgment, lifetimes, type Wait };
