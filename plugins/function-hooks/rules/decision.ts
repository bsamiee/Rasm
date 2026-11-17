import { Array, Data, Effect, Option, Result, Schema } from 'effect';
import type { Span } from '#/shell/script';

// --- [TYPES] ---------------------------------------------------------------------------

type Verdict = Data.TaggedEnum<{
    Refuse: { readonly reason: string };
    Edit: Pick<Span, 'start' | 'end' | 'field'> & { readonly text: Option.Option<string>; readonly subject: Option.Option<string> };
    Background: { readonly note?: string; readonly tasks?: readonly string[] };
}>;

// --- [MODELS] --------------------------------------------------------------------------

const Request = Schema.Struct({
    command: Schema.String,
    cwd: Schema.String,
    kind: Schema.Literals(['foreground', 'background', 'monitor', 'plain']),
    shell: Schema.optionalKey(Schema.Literals(['bash', 'zsh'])),
    agent: Schema.optionalKey(Schema.String),
    session: Schema.optionalKey(Schema.String),
    call: Schema.optionalKey(Schema.String),
    owned: Schema.optionalKey(Schema.Record(Schema.String, Schema.String)),
});

const Decision = Schema.TaggedUnion({
    Allow: {},
    Deny: { reason: Schema.String },
    Rewrite: {
        command: Schema.String,
        background: Schema.Boolean,
        note: Schema.optionalKey(Schema.String),
        tasks: Schema.optionalKey(Schema.Array(Schema.String)),
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
    return whole.filter(({ start, end }, at) => !whole.some((inner, other) => start <= inner.start && inner.end <= end && inner.end > start && (end - start > inner.end - inner.start || (inner.end > inner.start && end - start === inner.end - inner.start && other > at))));
};

const decisionOf = Effect.fnUntraced(function* <E, R>(command: string, verdicts: readonly Verdict[], due: (notes: ReadonlyMap<string, string>) => Effect.Effect<readonly string[], E, R>) {
    const edits = editsOf(verdicts);
    const backgrounds = verdicts.filter(Verdict.$is('Background'));
    const background = backgrounds.length > 0;
    const tasks = Array.dedupe(backgrounds.flatMap(({ tasks: watched = [] }) => watched));
    const refused = Array.dedupe(verdicts.filter(Verdict.$is('Refuse')).map(({ reason }) => reason));
    if (refused.length > 0) {
        return Decision.cases.Deny.make({ reason: refused.join('; ') });
    }
    if (edits.length === 0 && !background) {
        return Decision.cases.Allow.make({});
    }
    const source = new TextEncoder().encode(command);
    const decoder = new TextDecoder();
    const between = (start: number, end?: number) => decoder.decode(source.subarray(start, end));
    const ordered = edits.toSorted((left, right) => left.start - right.start || left.end - right.end);
    const subjects = new Map(Array.getSomes(edits.map(({ subject, text }) => Option.map(subject, (word) => [word, Option.match(text, { onNone: () => `${word} removed`, onSome: (replacement) => `${word} became ${replacement}` })] as const))));
    const notes = [...(subjects.size === 0 ? [] : yield* due(subjects)), ...Array.dedupe(backgrounds.map(({ note = 'Call runs in the background and notifies on completion, never poll it' }) => note))];
    return Decision.cases.Rewrite.make({
        command: [...ordered.flatMap(({ start, text }, at) => [between(ordered[at - 1]?.end ?? 0, start), ...Option.toArray(text)]), between(ordered.at(-1)?.end ?? 0)].join(''),
        background,
        ...(notes.length > 0 && { note: notes.join('. ') }),
        ...(tasks.length > 0 && { tasks }),
    });
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Decision, decisionOf, Request, Verdict };
