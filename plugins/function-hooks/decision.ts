import { Array, Cause, Data, Duration, Effect, Number, type Option, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verdict = Data.TaggedEnum<{
    Refuse: { readonly reason: string };
    Edit: { readonly start: number; readonly end: number; readonly text: string; readonly notice: Option.Option<string>; readonly timeout: Option.Option<number> };
    Background: { readonly notice: string };
}>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LEADING_WORDS = /^(?:\w+: )?(?<words>(?:\S+ +){0,6}\S+)/u;

// --- [MODELS] --------------------------------------------------------------------------

const Request = Schema.Struct({
    command: Schema.String,
    cwd: Schema.String,
    kind: Schema.Literals(['foreground', 'background', 'monitor', 'plain']),
    timeout: Schema.optionalKey(Schema.Finite),
});
type Request = typeof Request.Type;

const Instance = Schema.Struct({ pid: Schema.Int, bundle: Schema.String.check(Schema.isPattern(/^[\w.-]+$/u)) });
type Instance = typeof Instance.Type;

const Change = Schema.Struct({ created: Schema.Array(Instance), reused: Schema.Array(Instance), exited: Schema.Array(Schema.Int) });
type Change = typeof Change.Type;

const Decision = Schema.TaggedUnion({
    Allow: { observes: Schema.Boolean },
    Deny: { reason: Schema.String },
    Rewrite: {
        command: Schema.String,
        observes: Schema.Boolean,
        background: Schema.Boolean,
        notice: Schema.optionalKey(Schema.String),
        timeout: Schema.optionalKey(Schema.Finite),
    },
});
type Decision = typeof Decision.Type;

const Routes = { decide: [Request, Decision], observe: [Schema.String, Change], quit: [Schema.Array(Instance), Change], watched: [Schema.String, Schema.Array(Instance.mapFields(Struct.assign({ notes: Schema.Array(Schema.String), ended: Schema.Boolean })))] } as const;
type Routes = { readonly [Route in keyof typeof Routes]: { readonly [At in 0 | 1]: (typeof Routes)[Route][At]['Type'] } };

const Verdict = Data.taggedEnum<Verdict>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const unjudged = (cause: Cause.Cause<unknown>): Effect.Effect<Decision> => Effect.succeed(Decision.cases.Deny.make({ reason: `Not judged, ${LEADING_WORDS.exec(Cause.pretty(cause))?.groups?.words}` }));

const applied = (source: Uint8Array, edits: readonly Extract<Verdict, { readonly _tag: 'Edit' }>[]): string => {
    const decoder = new TextDecoder();
    const ordered = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const between = (start: number, end?: number): string => decoder.decode(source.subarray(start, end));
    return [...ordered.map(({ start, text }, index) => `${between(ordered[index - 1]?.end ?? 0, start)}${text}`), between(ordered.at(-1)?.end ?? 0)].join('');
};

const decisionOf = (command: string, verdicts: readonly Verdict[], observes: boolean): Decision => {
    const [refused] = verdicts.filter(Verdict.$is('Refuse'));
    const spans = verdicts.filter(Verdict.$is('Edit'));
    const edits = spans.filter(({ start, end }, at) => !spans.some((wider, other) => wider.start <= start && end <= wider.end && end > wider.start && (wider.end - wider.start > end - start || (end > start && wider.end - wider.start === end - start && other > at))));
    const backgrounds = verdicts.filter(Verdict.$is('Background')).map(({ notice }) => notice);
    const notices = Array.dedupe([...Array.getSomes(edits.map(({ notice }) => notice)), ...backgrounds]);
    const timeouts = Array.getSomes(edits.map(({ timeout }) => timeout));
    if (refused !== undefined) {
        return Decision.cases.Deny.make({ reason: refused.reason });
    }
    return edits.length + backgrounds.length === 0
        ? Decision.cases.Allow.make({ observes })
        : Decision.cases.Rewrite.make({
              command: applied(new TextEncoder().encode(command), edits),
              observes,
              background: backgrounds.length > 0,
              ...(notices.length > 0 && { notice: notices.join('. ') }),
              ...(timeouts.length > 0 && { timeout: Math.min(Duration.toMillis('10 minutes'), Number.sumAll(timeouts)) }),
          });
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Change, Decision, decisionOf, Instance, type Request, Routes, unjudged, Verdict };
