import type { Shell } from '@rasm/shell';
import { Array, Cause, Combiner, Duration, Effect, Equal, type FileSystem, Match, Option, Order, type Path, Predicate, Record, Reducer, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { Action, byRun, type Run, type scopes } from '#/decision/action';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Effect.Success<ReturnType<Shell['Service']['read']>>;
type Editor = Parameters<Parameters<Shell['Service']['edit']>[1]>[0];
type Invocation = Effect.Success<Script['invocations']>[number];
type Kind = Action['_tag'];
type NonShell = Exclude<Kind, 'Shell'>;
type Requirements = Shell | FileSystem.FileSystem | Path.Path | ChildProcessSpawner.ChildProcessSpawner;
type Verdicts<V> = readonly V[] | Effect.Effect<readonly V[], Cause.YieldableError, Requirements>;
interface Denial {
    readonly deny: string;
    readonly advice: string;
}
interface Edit {
    readonly edit: (editor: Editor, within: (nested: Script) => Editor) => readonly ReturnType<Editor['replace']>[];
}
type FlagEdit = { readonly drop: readonly string[] } | { readonly ensure: Array.NonEmptyReadonlyArray<string>; readonly unless?: readonly string[] } | { readonly rename: Readonly<Record<string, string>> };
type Verdict<K extends Kind> = { readonly note: string; readonly once: Note['once'] } | (K extends 'Task' ? { readonly again: Duration.Duration } : Denial) | (K extends 'Shell' ? Edit | { readonly run: Run; readonly note: string } : never);
interface Call {
    readonly action: ReturnType<typeof Action.Shell>;
    readonly script: Script;
    readonly cwd: string;
    readonly shell: Shell['Service'];
    readonly run: Run;
}
type ProgramRow = { readonly program: string; readonly aliases?: readonly string[]; readonly path?: readonly string[]; readonly action?: never } & (
    | (Denial & { readonly when?: readonly string[]; readonly unless?: readonly string[] })
    | FlagEdit
    | { readonly decide: (invocation: Invocation, call: Call) => Verdicts<Verdict<'Shell'> | FlagEdit> }
);
type ScriptRow = { readonly action: 'Shell'; readonly program?: never } & (Denial | { readonly decide: (script: Script, call: Call) => Verdicts<Verdict<'Shell'>> });
type ActionRow<K extends NonShell> = { readonly action: K; readonly program?: never } & (Extract<Verdict<K>, Denial> | { decide(action: Extract<Action, { readonly _tag: K }>): Verdicts<Verdict<K>> });
type Table = Readonly<Record<string, ProgramRow | ScriptRow | { [K in NonShell]: ActionRow<K> }[NonShell] | null>>;
interface Note {
    readonly text: string;
    readonly once: keyof typeof scopes;
}
interface Tally {
    readonly reasons: readonly string[];
    readonly notes: readonly Note[];
    readonly edits: readonly (Edit & { readonly rule: string })[];
    readonly move: { readonly run: Run; readonly notes: readonly Note[] };
    readonly again: Duration.Duration;
    readonly drop: readonly string[];
    readonly rename: Readonly<Record<string, string>>;
    readonly ensure: readonly Extract<FlagEdit, { readonly ensure: unknown }>[];
}
interface Round {
    readonly call: Call;
    readonly invocations: readonly Invocation[];
}
interface Compiled {
    readonly kinds: readonly Kind[];
    readonly shell: readonly ((round: Round) => Effect.Effect<Tally, never, Requirements>)[];
    readonly others: readonly ((action: Action) => Effect.Effect<Tally, never, Requirements>)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const shortFlag = /^-[^-]$/u;
const furthest = Combiner.max(Order.mapInput(byRun, ({ run }: Tally['move']) => run));
const tally = Struct.makeReducer<Tally>({
    reasons: Array.getReadonlyReducerConcat(),
    notes: Array.getReadonlyReducerConcat(),
    edits: Array.getReadonlyReducerConcat(),
    move: Reducer.make(furthest.combine, { run: 'foreground', notes: [] }),
    again: Reducer.make(Duration.min, Duration.infinity),
    drop: Array.getReadonlyReducerConcat(),
    rename: Record.makeReducerUnion(Combiner.last()),
    ensure: Array.getReadonlyReducerConcat(),
});
const compiledAll = Struct.makeReducer<Compiled>({ kinds: Array.getReadonlyReducerConcat(), shell: Array.getReadonlyReducerConcat(), others: Array.getReadonlyReducerConcat() });

// --- [OPERATIONS] ----------------------------------------------------------------------

const message = <E>(cause: Cause.Cause<E>) => Cause.prettyErrors(cause).map(Struct.get('message')).join('; ');
const hasSpelling = (names: readonly string[]) => (bound: Invocation['flags'][number]) => bound.flag.spellings.some((spelling) => names.includes(spelling));
const classify = (rule: string) => {
    const lift = Match.type<Verdict<Kind> | FlagEdit>().pipe(
        Match.when({ deny: Match.string }, ({ deny, advice }): Partial<Tally> => ({ reasons: [`${deny} ${advice}`] })),
        Match.when({ run: Match.string }, ({ run, note }): Partial<Tally> => ({ move: { run, notes: [{ text: `${rule}: ${note}`, once: 'call' }] } })),
        Match.when({ note: Match.string }, ({ note, once }): Partial<Tally> => ({ notes: [{ text: `${rule}: ${note}`, once }] })),
        Match.when({ edit: Predicate.isFunction }, ({ edit }): Partial<Tally> => ({ edits: [{ rule, edit }] })),
        Match.when({ ensure: Match.defined }, (ensure): Partial<Tally> => ({ ensure: [ensure] })),
        Match.orElse((stated): Partial<Tally> => stated),
    );
    const sum = (given: readonly (Verdict<Kind> | FlagEdit)[]) => tally.combineAll(given.map((verdict) => ({ ...tally.initialValue, ...lift(verdict) })));
    return (verdicts: () => Verdicts<Verdict<Kind> | FlagEdit>): Effect.Effect<Tally, never, Requirements> =>
        Effect.suspend(() => {
            const given = verdicts();
            return Effect.isEffect(given) ? given : Effect.succeed(given);
        }).pipe(
            Effect.map(sum),
            Effect.catchCause((cause) => Effect.succeed(sum([{ note: `failed and judged nothing: ${message(cause)}`, once: 'call' }]))),
        );
};
const flagEdits =
    (rule: string, invocation: Invocation, path: readonly string[]) =>
    (said: Tally): Tally => {
        const placed = invocation.flags.flatMap((bound) => Option.toArray(Option.map(Option.all([bound.spelled.node, bound.spelled.text]), ([word, text]) => ({ bound, word, text }))));
        const words = Array.isReadonlyArrayNonEmpty(placed) ? Array.groupWith(placed, (left, right) => Equal.equals(left.word, right.word)) : [];
        const written = words.flatMap((group) => {
            const { word, text } = Array.headNonEmpty(group);
            const parts = group.map(({ bound }) => ({
                bound,
                dropped: hasSpelling(said.drop)(bound),
                gone: Array.getSomes(bound.values.map(({ node }) => node)),
                value: Option.flatMap(
                    Array.findFirst(bound.values, ({ node }) => Option.isNone(node)),
                    ({ text: attached }) => attached,
                ),
                to: Array.findFirst(Record.toEntries(said.rename), ([from, name]) => (bound.flag.spellings.includes(from) && name !== bound.spelling ? Option.some(name) : Option.none())),
            }));
            const spelled = parts.map(({ bound, dropped, gone, value, to }) => {
                const [name, held] = [Option.getOrElse(to, () => bound.spelling), Option.toArray(value)];
                return Match.value({ dropped, letter: shortFlag.test(bound.spelling) && shortFlag.test(name), kept: Option.isNone(to), long: name.startsWith('--') }).pipe(
                    Match.when({ dropped: true }, () => ({ letters: '', words: Array.empty<string>(), gone })),
                    Match.when({ letter: true }, () => ({ letters: [name.slice(1), ...held].join(''), words: [], gone: [] })),
                    Match.when({ kept: true }, () => ({ letters: '', words: [text], gone: [] })),
                    Match.when({ long: true }, () => ({ letters: '', words: [[name, ...held].join('=')], gone: [] })),
                    Match.orElse(() => ({ letters: '', words: [name, ...held], gone: [] })),
                );
            });
            const cluster = spelled.map(({ letters }) => letters).join('');
            const target = Option.match(Array.last(spelled.flatMap(({ gone }) => gone)), { onNone: () => word, onSome: (value) => [word, value] as const });
            const respell = (editor: Editor) =>
                Array.match([...(cluster === '' ? [] : [`-${cluster}`]), ...spelled.flatMap(({ words: spilled }) => spilled)], {
                    onEmpty: () => editor.remove(target),
                    onNonEmpty: (given) => editor.replace(target, given),
                });
            return parts.some(({ dropped, to }) => dropped || Option.isSome(to)) ? [respell] : [];
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
const compileProgram = (rule: string, { program, aliases = [], path = [], ...row }: ProgramRow): Compiled['shell'][number] => {
    const [names, evaluate] = [new Set([program, ...aliases]), classify(rule)];
    const select = (invocation: Invocation) => {
        const said = Option.isSome(invocation.usage) ? [...invocation.path.map(Option.some), ...invocation.operands.map(({ word }) => word.text)] : Array.drop(invocation.words, 1).map(({ text }) => text);
        const running = Match.value(invocation.program).pipe(
            Match.tag('External', 'Builtin', ({ name }) => names.has(name)),
            Match.orElse(() => false),
        );
        return Option.liftPredicate(invocation.path.length < path.length ? path : invocation.path, () => running && path.every((part, at) => Option.contains(Option.flatten(Array.get(said, at)), part)));
    };
    const decide = Match.value(row).pipe(
        Match.when({ decide: Predicate.isFunction }, (given) => given.decide),
        Match.when({ deny: Match.string }, ({ deny, advice, when = [], unless = [] }) => (invocation: Invocation, _: Call, at: readonly string[]) => {
            const flags = Array.dedupe(invocation.flags.filter(hasSpelling(when)).flatMap(({ spelled }) => Option.toArray(spelled.text)));
            return (Array.isReadonlyArrayEmpty(when) || invocation.has(...when)) && !invocation.has(...unless) ? [{ deny: `${[...Option.toArray(invocation.name), ...at, ...flags].join(' ')} ${deny}.`, advice }] : [];
        }),
        Match.orElse((flagEdit) => () => [flagEdit]),
    );
    return ({ call, invocations }) =>
        Effect.map(
            Effect.forEach(
                invocations.flatMap((invocation) => Option.toArray(Option.map(select(invocation), (at) => ({ invocation, at })))),
                ({ invocation, at }) =>
                    Effect.map(
                        evaluate(() => decide(invocation, call, at)),
                        flagEdits(rule, invocation, at),
                    ),
            ),
            tally.combineAll,
        );
};
const compileScript = (rule: string, row: ScriptRow): Compiled['shell'][number] => {
    const [decide, evaluate] = ['decide' in row ? row.decide : () => [row], classify(rule)];
    return ({ call }) => evaluate(() => decide(call.script, call));
};
const compileAction = <K extends NonShell>(rule: string, row: ActionRow<K>): Compiled['others'][number] => {
    const [is, evaluate] = [Action.$is(row.action), classify(rule)];
    const decide = 'decide' in row ? (action: Extract<Action, { readonly _tag: K }>) => row.decide(action) : () => [row];
    return (action) => (is(action) ? evaluate(() => decide(action)) : Effect.succeed(tally.initialValue));
};
const compile = (rule: string, row: NonNullable<Table[string]>): Compiled =>
    Match.value(row).pipe(
        Match.when({ program: Match.string }, (given) => ({ kinds: ['Shell' as const], shell: [compileProgram(rule, given)], others: [] })),
        Match.when({ action: 'Shell' }, (given) => ({ kinds: ['Shell' as const], shell: [compileScript(rule, given)], others: [] })),
        Match.orElse((given) => ({ kinds: [given.action], shell: [], others: [compileAction(rule, given)] })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { compile, type Compiled, compiledAll, furthest, message, type Requirements, type Round, type Table, type Tally, tally };
