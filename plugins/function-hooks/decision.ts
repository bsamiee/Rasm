import { Array, Data, Duration, Number, Option, Schema, Struct } from 'effect';

// --- [TYPES] ---------------------------------------------------------------------------

type Verdict = Data.TaggedEnum<{
    Refuse: { readonly reason: string };
    Edit: { readonly start: number; readonly end: number; readonly text: Option.Option<string>; readonly subject: Option.Option<string>; readonly timeout: Option.Option<number> };
    Background: { readonly notice: string };
}>;
type Edit = Extract<Verdict, { readonly _tag: 'Edit' }>;

// --- [MODELS] --------------------------------------------------------------------------

const Request = Schema.Struct({
    command: Schema.String,
    cwd: Schema.String,
    kind: Schema.Literals(['foreground', 'background', 'monitor', 'plain']),
    timeout: Schema.optionalKey(Schema.Finite),
    agent: Schema.optionalKey(Schema.String),
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

const applied = (source: Uint8Array, edits: readonly Edit[]): string => {
    const decoder = new TextDecoder();
    const ordered = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const between = (start: number, end?: number): string => decoder.decode(source.subarray(start, end));
    return [
        ...ordered.map(({ start, text }, index) => {
            const kept = between(ordered[index - 1]?.end ?? 0, start);
            return Option.match(text, { onNone: () => kept, onSome: (replacement) => `${kept}${replacement}` });
        }),
        between(ordered.at(-1)?.end ?? 0),
    ].join('');
};

const editsOf = (verdicts: readonly Verdict[]): readonly Edit[] => {
    const spans = verdicts.filter(Verdict.$is('Edit'));
    return spans.filter(({ start, end }, at) => !spans.some((wider, other) => wider.start <= start && end <= wider.end && end > wider.start && (wider.end - wider.start > end - start || (end > start && wider.end - wider.start === end - start && other > at))));
};

const notesOf = (edits: readonly Edit[]): ReadonlyMap<string, string> => {
    const noted = Array.getSomes(edits.map(({ subject, text }) => Option.map(subject, (word) => [word, text] as const)));
    return new Map(noted.map(([word, text]) => [word, Option.match(text, { onNone: () => `${word} removed`, onSome: (replacement) => `${word} became ${replacement}` })]));
};

const decisionOf = (command: string, verdicts: readonly Verdict[], edits: readonly Edit[], notes: readonly string[], observes: boolean): Decision => {
    const [refused] = verdicts.filter(Verdict.$is('Refuse'));
    const backgrounds = verdicts.filter(Verdict.$is('Background')).map(({ notice }) => notice);
    const notices = Array.dedupe([...notes, ...backgrounds]);
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

export { type Change, Decision, decisionOf, editsOf, Instance, notesOf, type Request, Routes, Verdict };
