import type { Shell } from '@rasm/shell';
import { Array, Boolean, Cause, Combiner, Duration, Effect, Equal, type FileSystem, Filter, Match, Option, Ordering, type Path, Predicate, Record, Reducer, Struct } from 'effect';
import type { ChildProcessSpawner } from 'effect/process';
import { Action, byRun, type scopes } from '#/decision/action';

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
interface Note {
    readonly text: string;
    readonly once: keyof typeof scopes;
}
interface Move extends Pick<typeof Action.cases.Shell.Type, 'run'> {
    readonly notes: readonly Note[];
}
interface Ensure {
    readonly ensure: Array.NonEmptyReadonlyArray<string>;
    readonly unless?: readonly string[];
}
type FlagEdit = Ensure | { readonly drop: readonly string[] } | { readonly rename: Readonly<Record<string, string>> };
interface Stated {
    readonly Shell: Denial | Edit | (Pick<Move, 'run'> & { readonly note: string });
    readonly Isolate: Denial | { readonly apart: false };
    readonly Task: Pick<typeof Action.cases.Task.Type, 'again'>;
}
type Verdict<K extends Kind> = { readonly note: string; readonly once: Note['once'] } | (K extends keyof Stated ? Stated[K] : Denial);
interface Call extends Pick<Move, 'run'> {
    readonly action: typeof Action.cases.Shell.Type;
    readonly script: Script;
    readonly invocations: readonly Invocation[];
    readonly cwd: string;
    readonly shell: Shell['Service'];
}
type ProgramRow = { readonly program: string; readonly aliases?: readonly string[]; readonly path?: readonly string[]; readonly action?: never } & (
    | (Denial & { readonly when?: readonly string[]; readonly unless?: readonly string[] })
    | FlagEdit
    | { readonly decide: (invocation: Invocation, call: Call) => Verdicts<Verdict<'Shell'> | FlagEdit> }
);
type ScriptRow = { readonly action: 'Shell'; readonly program?: never } & (Denial | { readonly decide: (script: Script, call: Call) => Verdicts<Verdict<'Shell'>> });
type ActionRow<K extends NonShell> = { readonly action: K; readonly program?: never } & ((Denial extends Verdict<K> ? Denial : never) | { decide(action: (typeof Action.cases)[K]['Type']): Verdicts<Verdict<K>> });
type Table = Readonly<Record<string, ProgramRow | ScriptRow | { [K in NonShell]: ActionRow<K> }[NonShell] | null>>;
interface Tally extends Pick<typeof Action.cases.Task.Type, 'again'>, Pick<typeof Action.cases.Isolate.Type, 'apart'> {
    readonly reasons: readonly string[];
    readonly notes: readonly Note[];
    readonly faults: readonly string[];
    readonly edits: readonly (Edit & { readonly rule: string })[];
    readonly move: Move;
    readonly drop: readonly string[];
    readonly rename: Readonly<Record<string, string>>;
    readonly ensure: readonly Ensure[];
}
interface Compiled {
    readonly kinds: readonly Kind[];
    readonly shell: readonly ((call: Call) => Effect.Effect<Tally, never, Requirements>)[];
    readonly others: readonly ((action: Action) => Effect.Effect<Tally, never, Requirements>)[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const tally = Struct.makeReducer<Tally>({
    reasons: Array.getReadonlyReducerConcat(),
    notes: Array.getReadonlyReducerConcat(),
    faults: Array.getReadonlyReducerConcat(),
    edits: Array.getReadonlyReducerConcat(),
    move: Reducer.make<Move>((self, that) => Ordering.match(byRun(self.run, that.run), { onLessThan: () => that, onEqual: () => ({ ...self, notes: [...self.notes, ...that.notes] }), onGreaterThan: () => self }), { run: 'foreground', notes: [] }),
    again: Reducer.make(Duration.min, Duration.infinity),
    apart: Boolean.ReducerAnd,
    drop: Array.getReadonlyReducerConcat(),
    rename: Record.makeReducerUnion(Combiner.last()),
    ensure: Array.getReadonlyReducerConcat(),
});
const compiled = Struct.makeReducer<Compiled>({ kinds: Array.getReadonlyReducerConcat(), shell: Array.getReadonlyReducerConcat(), others: Array.getReadonlyReducerConcat() });

// --- [OPERATIONS] ----------------------------------------------------------------------

const message = <E>(cause: Cause.Cause<E>) => Cause.prettyErrors(cause).map(Struct.get('message')).join('; ');
const hasSpelling = (names: readonly string[]) => (bound: Invocation['flags'][number]) => bound.flag.spellings.some((spelling) => names.includes(spelling));
const evaluate = (rule: string) => {
    const lift = Match.type<Verdict<Kind> | FlagEdit>().pipe(
        Match.when({ deny: Match.string }, ({ deny, advice }): Partial<Tally> => ({ reasons: [`${deny} ${advice}`] })),
        Match.when({ run: Match.string }, ({ run, note }): Partial<Tally> => ({ move: { run, notes: [{ text: `${rule}: ${note}`, once: 'call' }] } })),
        Match.when({ note: Match.string }, ({ note, once }): Partial<Tally> => ({ notes: [{ text: `${rule}: ${note}`, once }] })),
        Match.when({ edit: Predicate.isFunction }, ({ edit }): Partial<Tally> => ({ edits: [{ rule, edit }] })),
        Match.when({ ensure: Match.defined }, (ensure): Partial<Tally> => ({ ensure: [ensure] })),
        Match.orElse((stated): Partial<Tally> => stated),
    );
    return (verdicts: () => Verdicts<Verdict<Kind> | FlagEdit>): Effect.Effect<Tally, never, Requirements> =>
        Effect.suspend(() => {
            const given = verdicts();
            return Effect.isEffect(given) ? given : Effect.succeed(given);
        }).pipe(
            Effect.map((given) => tally.combineAll(given.map((verdict) => ({ ...tally.initialValue, ...lift(verdict) })))),
            Effect.catchCause((cause) => Effect.succeed({ ...tally.initialValue, faults: [`${rule}: failed and judged nothing: ${message(cause)}`] })),
        );
};
const flagEdits =
    (rule: string, invocation: Invocation, anchor: Option.Option<NonNullable<Parameters<Editor['after']>[0]>>) =>
    (said: Tally): Tally => {
        const placed = Array.filterMap(
            invocation.flags,
            Filter.fromPredicateOption((bound) => Option.map(bound.spelled.node, (word) => ({ bound, word }))),
        );
        const respelled = (Array.isReadonlyArrayNonEmpty(placed) ? Array.groupWith(placed, (left, right) => Equal.equals(left.word, right.word)) : []).flatMap((group) => {
            const { word, bound: first } = Array.headNonEmpty(group);
            const parts = Array.map(group, ({ bound }) => {
                const to = Array.findFirst(Record.toEntries(said.rename), ([from, name]) => (bound.flag.spellings.includes(from) && name !== bound.spelling ? Option.some(name) : Option.none()));
                const [name, value] = [
                    Option.getOrElse(to, () => bound.spelling),
                    Option.toArray(
                        Option.flatMap(
                            Array.findFirst(bound.values, ({ node }) => Option.isNone(node)),
                            Struct.get('text'),
                        ),
                    ),
                ];
                return Match.value({ dropped: hasSpelling(said.drop)(bound), short: !bound.spelling.startsWith('--') && !name.startsWith('--'), long: name.startsWith('--') }).pipe(
                    Match.when({ dropped: true }, () => ({ changed: true, name, letters: '', words: Array.empty<string>(), gone: Array.getSomes(bound.values.map(Struct.get('node'))) })),
                    Match.when({ short: true }, () => ({ changed: Option.isSome(to), name, letters: [name.slice(1), ...value].join(''), words: [], gone: [] })),
                    Match.when({ long: true }, () => ({ changed: Option.isSome(to), name, letters: '', words: [[name, ...value].join('=')], gone: [] })),
                    Match.orElse(() => ({ changed: Option.isSome(to), name, letters: '', words: [name, ...value], gone: [] })),
                );
            });
            const [letters, { name }] = [parts.map(Struct.get('letters')).join(''), Array.headNonEmpty(parts)];
            const target = Option.match(Array.last(parts.flatMap(Struct.get('gone'))), { onNone: () => word, onSome: (value) => [word, value] as const });
            const written = (editor: Editor) =>
                Array.match([...parts.flatMap(Struct.get('words')), ...(letters === '' ? [] : [`-${letters}`])], {
                    onEmpty: () => editor.remove(target),
                    onNonEmpty: (given) => editor.replace(target, given),
                });
            const prefixed = Option.map(
                Option.filter(Array.head(word.parts), (part) => part.type === 'Lit' && part.value === `${first.spelling}=`),
                (part) => (editor: Editor) => editor.replace(part, [name.startsWith('--') ? `${name}=` : name]),
            );
            return Match.value({ changed: parts.some(Struct.get('changed')), literal: Option.isSome(first.spelled.text), dropped: hasSpelling(said.drop)(first) }).pipe(
                Match.when({ changed: false }, () => []),
                Match.whenOr({ literal: true }, { dropped: true }, () => [written]),
                Match.orElse(() => Option.toArray(prefixed)),
            );
        });
        const ensured = said.ensure.filter(({ ensure, unless = [] }) => !invocation.has(Array.headNonEmpty(ensure), ...unless)).flatMap(({ ensure }) => Option.toArray(anchor).map((word) => (editor: Editor) => editor.after(word, ensure)));
        const edits = [...respelled, ...ensured];
        return { ...said, drop: [], rename: {}, ensure: [], edits: [...said.edits, ...(Array.isReadonlyArrayNonEmpty(edits) ? [{ rule, edit: (editor: Editor) => edits.map((edit) => edit(editor)) }] : [])] };
    };
const compileProgram = (rule: string, { program, aliases = [], path = [], ...row }: ProgramRow): Compiled['shell'][number] => {
    const [names, toTally] = [new Set([program, ...aliases]), evaluate(rule)];
    const select = (invocation: Invocation) => {
        const stated = [...invocation.subcommands, ...invocation.operands.map(({ word }) => word).filter((word) => !invocation.rest.includes(word))];
        const running = Match.value(invocation.program).pipe(
            Match.tag('External', 'Builtin', ({ name }) => names.has(name)),
            Match.orElse(() => false),
        );
        return Option.liftPredicate(stated, () => running && path.every((part, at) => Option.exists(Array.get(stated, at), (word) => word.spells(part))));
    };
    const decide = Match.value(row).pipe(
        Match.when({ decide: Predicate.isFunction }, (given) => given.decide),
        Match.when({ deny: Match.string }, ({ deny, advice, when = [], unless = [] }) => (invocation: Invocation) => {
            const flags = Array.dedupe(invocation.flags.filter(hasSpelling(when)).flatMap(({ spelled }) => Option.toArray(spelled.text)));
            return (Array.isReadonlyArrayEmpty(when) || invocation.has(...when)) && !invocation.has(...unless) ? [{ deny: `${[...Option.toArray(invocation.name), ...(invocation.path.length < path.length ? path : invocation.path), ...flags].join(' ')} ${deny}.`, advice }] : [];
        }),
        Match.orElse((flagEdit) => () => [flagEdit]),
    );
    return (call) =>
        Effect.map(
            Effect.forEach(
                Array.filterMap(
                    call.invocations,
                    Filter.fromPredicateOption((invocation) => Option.map(select(invocation), (stated) => ({ invocation, stated }))),
                ),
                ({ invocation, stated }) =>
                    Effect.map(
                        toTally(() => decide(invocation, call)),
                        flagEdits(
                            rule,
                            invocation,
                            Option.flatMap(
                                Option.orElse(Array.get(stated, path.length - 1), () => Array.head(invocation.words)),
                                ({ node }) => node,
                            ),
                        ),
                    ),
            ),
            tally.combineAll,
        );
};
const compileScript = (rule: string, row: ScriptRow): Compiled['shell'][number] => {
    const [decide, toTally] = ['decide' in row ? row.decide : () => [row], evaluate(rule)];
    return (call) => toTally(() => decide(call.script, call));
};
const compileAction = <K extends NonShell>(rule: string, row: ActionRow<K>): Compiled['others'][number] => {
    const [is, toTally] = [Action.guards[row.action], evaluate(rule)];
    const decide = 'decide' in row ? (action: (typeof Action.cases)[K]['Type']) => row.decide(action) : () => [row];
    return (action) => (is(action) ? toTally(() => decide(action)) : Effect.succeed(tally.initialValue));
};
const compile = (rule: string, row: NonNullable<Table[string]>): Compiled =>
    Match.value(row).pipe(
        Match.when({ program: Match.string }, (given) => ({ kinds: ['Shell' as const], shell: [compileProgram(rule, given)], others: [] })),
        Match.when({ action: 'Shell' }, (given) => ({ kinds: ['Shell' as const], shell: [compileScript(rule, given)], others: [] })),
        Match.orElse((given) => ({ kinds: [given.action], shell: [], others: [compileAction(rule, given)] })),
    );

// --- [EXPORTS] -------------------------------------------------------------------------

export { type Call, compile, type Compiled, compiled, message, type ProgramRow, type Requirements, type Table, type Tally, tally };
