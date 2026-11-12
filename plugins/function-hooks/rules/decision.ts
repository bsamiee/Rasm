import { Array, Data, Duration, Effect, Number, Option, Result, Schema } from 'effect';
import type { Span } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Verdict = Data.TaggedEnum<{
    Refuse: { readonly reason: string };
    Edit: Pick<Span, 'start' | 'end' | 'field'> & { readonly text: Option.Option<string>; readonly subject: Option.Option<string>; readonly timeout: Option.Option<number> };
    Background: Record<never, never>;
    Observe: Record<never, never>;
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Request = Schema.Struct({
    command: Schema.String,
    cwd: Schema.String,
    kind: Schema.Literals(['foreground', 'background', 'monitor', 'plain']),
    timeout: Schema.optionalKey(Schema.Finite),
    agent: Schema.optionalKey(Schema.String),
});

const Decision = Schema.TaggedUnion({
    Allow: { observes: Schema.Boolean },
    Deny: { reason: Schema.String },
    Rewrite: {
        command: Schema.String,
        observes: Schema.Boolean,
        background: Schema.Boolean,
        note: Schema.optionalKey(Schema.String),
        timeout: Schema.optionalKey(Schema.Finite),
    },
});

const Verdict = Data.taggedEnum<Verdict>();

// --- [OPERATIONS] ----------------------------------------------------------------------

const editsOf = (verdicts: readonly Verdict[]) => {
    const spans = verdicts.filter(Verdict.$is('Edit'));
    const groups = Map.groupBy(spans, (span) => span.field?.words ?? span);
    const whole = spans.flatMap((span) => {
        const group = groups.get(span.field?.words ?? span) ?? [];
        if (group.at(-1) !== span) {
            return [];
        }
        if (span.field === undefined) {
            return [span];
        }
        const { field: source, ...edit } = span;
        const replacements = new Map(Array.filterMap(group, ({ field, text }) => (field === undefined ? Result.failVoid : Result.succeed([field.index, text] as const))));
        return [{ ...edit, text: Option.some(Array.getSomes(source.words.map((raw, at) => replacements.get(at) ?? Option.some(raw))).join(' ')) }];
    });
    return whole.filter(({ start, end }, at) => !whole.some((wider, other) => wider.start <= start && end <= wider.end && end > wider.start && (wider.end - wider.start > end - start || (end > start && wider.end - wider.start === end - start && other > at))));
};

const decisionOf = Effect.fnUntraced(function* <E, R>(command: string, verdicts: readonly Verdict[], due: (notes: ReadonlyMap<string, string>) => Effect.Effect<readonly string[], E, R>) {
    const edits = editsOf(verdicts);
    const observes = verdicts.some(Verdict.$is('Observe'));
    const background = verdicts.some(Verdict.$is('Background'));
    const [refused] = verdicts.filter(Verdict.$is('Refuse'));
    if (refused !== undefined) {
        return Decision.cases.Deny.make({ reason: refused.reason });
    }
    if (edits.length === 0 && !background) {
        return Decision.cases.Allow.make({ observes });
    }
    const source = new TextEncoder().encode(command);
    const decoder = new TextDecoder();
    const between = (start: number, end?: number) => decoder.decode(source.subarray(start, end));
    const ordered = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const subjects = new Map(Array.getSomes(edits.map(({ subject, text }) => Option.map(subject, (word) => [word, Option.match(text, { onNone: () => `${word} removed`, onSome: (replacement) => `${word} became ${replacement}` })] as const))));
    const notes = [...(subjects.size === 0 ? [] : yield* due(subjects)), ...(background ? ['Call runs in the background, wait for its notice'] : [])];
    const timeouts = Array.getSomes(edits.map(({ timeout }) => timeout));
    return Decision.cases.Rewrite.make({
        command: [...ordered.flatMap(({ start, text }, at) => [between(ordered[at - 1]?.end ?? 0, start), ...Option.toArray(text)]), between(ordered.at(-1)?.end ?? 0)].join(''),
        observes,
        background,
        ...(notes.length > 0 && { note: notes.join('. ') }),
        ...(timeouts.length > 0 && { timeout: Math.min(Duration.toMillis('10 minutes'), Number.sumAll(timeouts)) }),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Decision, decisionOf, Request, Verdict };
