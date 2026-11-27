import { type Array, Data, Number, Order, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Action = typeof Action.Type;
type Scope = Data.TaggedEnum<{ Session: object; Agent: Pick<Action, 'agent'>; Call: object }>;
interface Told {
    readonly notes: readonly string[];
    readonly faults: readonly string[];
}
type Decision = Data.TaggedEnum<{
    Allow: Told;
    Deny: Told & { readonly reasons: Array.NonEmptyReadonlyArray<string> };
    Rewrite: Told & { readonly action: Action };
}>;

// --- [MODELS] --------------------------------------------------------------------------

const caller = { call: Schema.String, agent: Schema.Option(Schema.String) };
const Action = Schema.TaggedUnion({
    Shell: { ...caller, command: Schema.String, run: Schema.Literals(['foreground', 'background', 'monitored']) },
    Read: { ...caller, path: Schema.String },
    Write: { ...caller, path: Schema.String, content: Schema.String },
    Edit: { ...caller, path: Schema.String, old: Schema.String, new: Schema.String },
    Fetch: { ...caller, url: Schema.String },
    Mcp: { ...caller, server: Schema.String, tool: Schema.String, input: Schema.Record(Schema.String, Schema.Unknown) },
    Spawn: { ...caller, prompt: Schema.String, model: Schema.Option(Schema.String) },
    Task: { ...caller, id: Schema.String, output: Schema.String, resting: Schema.Boolean, again: Schema.Duration },
});
const Decision = Data.taggedEnum<Decision>();
const Scope = Data.taggedEnum<Scope>();
const byRun = Order.mapInput(Number.Order, (run: (typeof Action.cases.Shell.Type)['run']) => Action.cases.Shell.fields.run.literals.indexOf(run));
const scopes = {
    session: (): Scope => Scope.Session(),
    agent: ({ agent }: Pick<Action, 'agent'>): Scope => Scope.Agent({ agent }),
    call: (): Scope => Scope.Call(),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, byRun, Decision, Scope, scopes };
