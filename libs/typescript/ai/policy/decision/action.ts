import type { Call } from '@rasm/report';
import { type Array, Data, Number, Option, Order, Schema } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Action = typeof Action.Type;
type Scope = Data.TaggedEnum<{ Session: object; Agent: Pick<Action, 'agent'>; Call: object }>;
interface Told {
    readonly notes: readonly Outcome[];
    readonly faults: readonly string[];
}
type Decision = Data.TaggedEnum<{
    Allow: Told;
    Deny: Told & { readonly reasons: Array.NonEmptyReadonlyArray<Outcome> };
    Rewrite: Told & { readonly action: Action; readonly rules: readonly string[] };
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
    Output: { ...caller, tool: Schema.String, text: Schema.String, readOnly: Schema.Boolean },
});
class Outcome extends Data.Class<{
    readonly rules: Array.NonEmptyReadonlyArray<string>;
    readonly subject: string;
    readonly reason: string;
    readonly values: Readonly<Record<string, string>>;
    readonly next: Option.Option<Call>;
}> {
    static readonly of = (rules: Array.NonEmptyReadonlyArray<string>, { subject, reason, values = {}, next }: Pick<Outcome, 'subject' | 'reason'> & { readonly values?: Outcome['values']; readonly next?: Call }) => new Outcome({ rules, subject, reason, values, next: Option.fromUndefinedOr(next) });
}
const Decision = Data.taggedEnum<Decision>();
const Scope = Data.taggedEnum<Scope>();
const byRun = Order.mapInput(Number.Order, (run: (typeof Action.cases.Shell.Type)['run']) => Action.cases.Shell.fields.run.literals.indexOf(run));
const scopes = {
    session: (): Scope => Scope.Session(),
    agent: ({ agent }: Pick<Action, 'agent'>): Scope => Scope.Agent({ agent }),
    call: (): Scope => Scope.Call(),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Action, byRun, Decision, Outcome, Scope, scopes };
