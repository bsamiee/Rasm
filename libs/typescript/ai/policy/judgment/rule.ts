import type { Shell } from '@rasm/shell';
import { Array, Cause, Combiner, Duration, Effect, Equal, type FileSystem, Match, Option, Order, type Path, Predicate, Record, Reducer, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { Action, byWait, type lifetimes, type Wait } from '#/judgment/action';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Editor = Parameters<Parameters<Shell['Service']['edit']>[1]>[0];
type Invocation = Effect.Success<Script['invocations']>[number];
type Kind = Action['_tag'];
type Other = Exclude<Kind, 'Shell'>;
type Needs = Shell | FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner;
type Verdicts<V> = readonly V[] | Effect.Effect<readonly V[], Cause.YieldableError, Needs>;
interface Refusal {
    readonly refuse: string;
    readonly instruct: string;
}
interface Edited {
    readonly edit: (editor: Editor, within: (nested: Script) => Editor) => readonly ReturnType<Editor['replace']>[];
}
type Respelling = { readonly drop: readonly string[] } | { readonly ensure: Array.NonEmptyReadonlyArray<string>; readonly unless?: readonly string[] } | { readonly rename: Readonly<Record<string, string>> };
type Verdict<K extends Kind> = { readonly note: string; readonly once: Told['once'] } | (K extends 'Task' ? { readonly again: Duration.Duration } : Refusal) | (K extends 'Shell' ? Edited | { readonly wait: Wait; readonly note: string } : never);
interface Call {
    readonly action: ReturnType<typeof Action.Shell>;
    readonly script: Script;
    readonly cwd: string;
    readonly shell: Shell['Service'];
    readonly wait: Wait;
}
type Programmed = { readonly program: string; readonly aliases?: readonly string[]; readonly path?: readonly string[]; readonly action?: never } & (
    | (Refusal & { readonly when?: readonly string[]; readonly unless?: readonly string[] })
    | Respelling
    | { readonly judge: (invocation: Invocation, call: Call) => Verdicts<Verdict<'Shell'> | Respelling> }
);
type Scripted = { readonly action: 'Shell'; readonly program?: never } & (Refusal | { readonly judge: (script: Script, call: Call) => Verdicts<Verdict<'Shell'>> });
type Acted<K extends Other> = { readonly action: K; readonly program?: never } & (Extract<Verdict<K>, Refusal> | { judge(action: Extract<Action, { readonly _tag: K }>): Verdicts<Verdict<K>> });
type Table = Readonly<Record<string, Programmed | Scripted | { [K in Other]: Acted<K> }[Other] | null>>;
interface Told {
    readonly text: string;
    readonly once: keyof typeof lifetimes;
}
interface Tally {
    readonly reasons: readonly string[];
    readonly notes: readonly Told[];
    readonly edits: readonly (Edited & { readonly rule: string })[];
    readonly move: { readonly wait: Wait; readonly told: readonly Told[] };
    readonly again: Duration.Duration;
    readonly drop: readonly string[];
    readonly rename: Readonly<Record<string, string>>;
    readonly ensure: readonly Extract<Respelling, { readonly ensure: unknown }>[];
}
interface Round {
    readonly call: Call;
    readonly found: readonly Invocation[];
}
interface Judges {
    readonly heard: readonly Kind[];
    readonly rounds: readonly ((round: Round) => Effect.Effect<Tally, never, Needs>)[];
    readonly actions: readonly ((action: Action) => Effect.Effect<Tally, never, Needs>)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const lettered = /^-[^-]$/u;
const moved = Combiner.max(Order.mapInput(byWait, ({ wait }: Tally['move']) => wait));
const tallied = Struct.makeReducer<Tally>({
    reasons: Array.getReadonlyReducerConcat(),
    notes: Array.getReadonlyReducerConcat(),
    edits: Array.getReadonlyReducerConcat(),
    move: Reducer.make(moved.combine, { wait: 'held', told: [] }),
    again: Reducer.make(Duration.min, Duration.infinity),
    drop: Array.getReadonlyReducerConcat(),
    rename: Record.makeReducerUnion(Combiner.last()),
    ensure: Array.getReadonlyReducerConcat(),
});
const judges = Struct.makeReducer<Judges>({ heard: Array.getReadonlyReducerConcat(), rounds: Array.getReadonlyReducerConcat(), actions: Array.getReadonlyReducerConcat() });

// --- [OPERATIONS] ----------------------------------------------------------------------

const failure = <E>(cause: Cause.Cause<E>) =>
    Cause.prettyErrors(cause)
        .map(({ message }) => message)
        .join('; ');
const named = (names: readonly string[]) => (bound: Invocation['flags'][number]) => bound.flag.spellings.some((spelling) => names.includes(spelling));
const judging = (rule: string) => {
    const lifted = Match.type<Verdict<Kind> | Respelling>().pipe(
        Match.when({ refuse: Match.string }, ({ refuse, instruct }): Partial<Tally> => ({ reasons: [`${refuse} ${instruct}`] })),
        Match.when({ wait: Match.string }, ({ wait, note }): Partial<Tally> => ({ move: { wait, told: [{ text: `${rule}: ${note}`, once: 'call' }] } })),
        Match.when({ note: Match.string }, ({ note, once }): Partial<Tally> => ({ notes: [{ text: `${rule}: ${note}`, once }] })),
        Match.when({ edit: Predicate.isFunction }, ({ edit }): Partial<Tally> => ({ edits: [{ rule, edit }] })),
        Match.when({ ensure: Match.defined }, (ensure): Partial<Tally> => ({ ensure: [ensure] })),
        Match.orElse((stated): Partial<Tally> => stated),
    );
    const summed = (given: readonly (Verdict<Kind> | Respelling)[]) => tallied.combineAll(given.map((verdict) => ({ ...tallied.initialValue, ...lifted(verdict) })));
    return (verdicts: () => Verdicts<Verdict<Kind> | Respelling>): Effect.Effect<Tally, never, Needs> =>
        Effect.suspend(() => {
            const given = verdicts();
            return Effect.isEffect(given) ? given : Effect.succeed(given);
        }).pipe(
            Effect.map(summed),
            Effect.catchCause((cause) => Effect.succeed(summed([{ note: `failed and judged nothing: ${failure(cause)}`, once: 'call' }]))),
        );
};
const respelled =
    (rule: string, invocation: Invocation, path: readonly string[]) =>
    (said: Tally): Tally => {
        const placed = invocation.flags.flatMap((bound) => Option.toArray(Option.map(Option.all([bound.spelled.node, bound.spelled.text]), ([word, text]) => ({ bound, word, text }))));
        const words = Array.isReadonlyArrayNonEmpty(placed) ? Array.groupWith(placed, (left, right) => Equal.equals(left.word, right.word)) : [];
        const written = words.flatMap((group) => {
            const { word, text } = Array.headNonEmpty(group);
            const parts = group.map(({ bound }) => ({
                bound,
                dropped: named(said.drop)(bound),
                gone: Array.getSomes(bound.values.map(({ node }) => node)),
                value: Option.flatMap(
                    Array.findFirst(bound.values, ({ node }) => Option.isNone(node)),
                    ({ text: attached }) => attached,
                ),
                to: Array.findFirst(Record.toEntries(said.rename), ([from, name]) => (bound.flag.spellings.includes(from) && name !== bound.spelling ? Option.some(name) : Option.none())),
            }));
            const spelled = parts.map(({ bound, dropped, gone, value, to }) => {
                const [name, held] = [Option.getOrElse(to, () => bound.spelling), Option.toArray(value)];
                return Match.value({ dropped, letter: lettered.test(bound.spelling) && lettered.test(name), kept: Option.isNone(to), long: name.startsWith('--') }).pipe(
                    Match.when({ dropped: true }, () => ({ letters: '', words: Array.empty<string>(), gone })),
                    Match.when({ letter: true }, () => ({ letters: [name.slice(1), ...held].join(''), words: [], gone: [] })),
                    Match.when({ kept: true }, () => ({ letters: '', words: [text], gone: [] })),
                    Match.when({ long: true }, () => ({ letters: '', words: [[name, ...held].join('=')], gone: [] })),
                    Match.orElse(() => ({ letters: '', words: [name, ...held], gone: [] })),
                );
            });
            const cluster = spelled.map(({ letters }) => letters).join('');
            const target = Option.match(Array.last(spelled.flatMap(({ gone }) => gone)), { onNone: () => word, onSome: (value) => [word, value] as const });
            const worded = (editor: Editor) =>
                Array.match([...(cluster === '' ? [] : [`-${cluster}`]), ...spelled.flatMap(({ words: spilled }) => spilled)], {
                    onEmpty: () => editor.remove(target),
                    onNonEmpty: (given) => editor.replace(target, given),
                });
            return parts.some(({ dropped, to }) => dropped || Option.isSome(to)) ? [worded] : [];
        });
        const anchor = Option.flatMap(
            Option.orElse(
                Option.flatMap(Array.last(path), (last) => Array.findFirst(Array.drop(invocation.words, 1), (word) => word.spells(last))),
                () => Array.head(invocation.words),
            ),
            ({ node }) => node,
        );
        const ensured = said.ensure.filter(({ ensure, unless = [] }) => !invocation.has(Array.headNonEmpty(ensure), ...unless)).flatMap(({ ensure }) => Option.toArray(anchor).map((word) => (editor: Editor) => editor.after(word, ensure)));
        const edits = [...written, ...ensured];
        return { ...said, drop: [], rename: {}, ensure: [], edits: [...said.edits, ...(Array.isReadonlyArrayNonEmpty(edits) ? [{ rule, edit: (editor: Editor) => edits.map((edit) => edit(editor)) }] : [])] };
    };
const programmed = (rule: string, { program, aliases = [], path = [], ...row }: Programmed): Judges['rounds'][number] => {
    const [names, summed] = [new Set([program, ...aliases]), judging(rule)];
    const selected = (invocation: Invocation) => {
        const said = Option.isSome(invocation.usage) ? [...invocation.path.map(Option.some), ...invocation.operands.map(({ word }) => word.text)] : Array.drop(invocation.words, 1).map(({ text }) => text);
        const running = Match.value(invocation.program).pipe(
            Match.tag('External', 'Builtin', ({ name }) => names.has(name)),
            Match.orElse(() => false),
        );
        return Option.liftPredicate(invocation.path.length < path.length ? path : invocation.path, () => running && path.every((part, at) => Option.contains(Option.flatten(Array.get(said, at)), part)));
    };
    const judge = Match.value(row).pipe(
        Match.when({ judge: Predicate.isFunction }, (given) => given.judge),
        Match.when({ refuse: Match.string }, ({ refuse, instruct, when = [], unless = [] }) => (invocation: Invocation, _: Call, at: readonly string[]) => {
            const flags = Array.dedupe(invocation.flags.filter(named(when)).flatMap(({ spelled }) => Option.toArray(spelled.text)));
            return (Array.isReadonlyArrayEmpty(when) || invocation.has(...when)) && !invocation.has(...unless) ? [{ refuse: `${[...Option.toArray(invocation.name), ...at, ...flags].join(' ')} ${refuse}.`, instruct }] : [];
        }),
        Match.orElse((respelling) => () => [respelling]),
    );
    return ({ call, found }) =>
        Effect.map(
            Effect.forEach(
                found.flatMap((invocation) => Option.toArray(Option.map(selected(invocation), (at) => ({ invocation, at })))),
                ({ invocation, at }) =>
                    Effect.map(
                        summed(() => judge(invocation, call, at)),
                        respelled(rule, invocation, at),
                    ),
            ),
            tallied.combineAll,
        );
};
const scripted = (rule: string, row: Scripted): Judges['rounds'][number] => {
    const [judge, summed] = ['judge' in row ? row.judge : () => [row], judging(rule)];
    return ({ call }) => summed(() => judge(call.script, call));
};
const acted = <K extends Other>(rule: string, row: Acted<K>): Judges['actions'][number] => {
    const [is, summed] = [Action.$is(row.action), judging(rule)];
    const judge = 'judge' in row ? (action: Extract<Action, { readonly _tag: K }>) => row.judge(action) : () => [row];
    return (action) => (is(action) ? summed(() => judge(action)) : Effect.succeed(tallied.initialValue));
};
const compiled = (rule: string, row: NonNullable<Table[string]>): Judges =>
    Match.value(row).pipe(
        Match.when({ program: Match.string }, (given) => ({ heard: ['Shell' as const], rounds: [programmed(rule, given)], actions: [] })),
        Match.when({ action: 'Shell' }, (given) => ({ heard: ['Shell' as const], rounds: [scripted(rule, given)], actions: [] })),
        Match.orElse((given) => ({ heard: [given.action], rounds: [], actions: [acted(rule, given)] })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { compiled, failure, type Judges, judges, moved, type Needs, type Round, type Table, type Tally, tallied };
