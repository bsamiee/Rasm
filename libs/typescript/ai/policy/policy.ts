import { Shell } from '@rasm/shell';
import { Array, Cause, ConfigProvider, Data, Effect, Exit, Function, Match, Option, Order, Predicate, Record, Schema } from 'effect';
import { Action, type Kind } from '#/judgment/action';
import { type Call, type Dialect, type Edit, type Editor, type Invocation, type Needs, type Note, plain, type Rule, type Run, type Script, type Settled, settled, type Table, Told, type Tool, type Used, type Verdicts } from '#/judgment/rule';
import { git } from '#/rules/git';

// --- [TYPES] ---------------------------------------------------------------------------

type Judgment = Data.TaggedEnum<{
    Pass: { readonly notes: readonly Told[] };
    Refuse: { readonly reasons: Array.NonEmptyReadonlyArray<string>; readonly notes: readonly Told[] };
    Rewrite: { readonly command: string; readonly run: Run; readonly notes: readonly Told[] };
}>;
interface Owned {
    readonly rule: string;
    readonly verdict: Settled;
}
type Ruled<A> = A & { readonly rule: string };
interface Tally {
    readonly reasons: readonly string[];
    readonly notes: readonly Ruled<Note>[];
    readonly edits: readonly Ruled<Extract<Settled, { readonly edit: unknown }>>[];
    readonly runs: readonly Run[];
}
interface Round {
    readonly script: Script;
    readonly run: Run;
    readonly notes: readonly Ruled<Note>[];
    readonly seen: readonly string[];
    readonly editors: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Judgment = Data.taggedEnum<Judgment>();
const shellish = Schema.is(Action.Shell);
const ranked = Order.mapInput(Order.Number, (run: Run) => ['foreground', 'background', 'monitor'].indexOf(run));

// --- [OPERATIONS] ----------------------------------------------------------------------

const ran = <V>(rule: string, verdicts: () => Verdicts<V>, settle: (given: readonly V[]) => readonly Settled[]): Effect.Effect<readonly Owned[], never, Needs> =>
    Effect.suspend(() => {
        const given = verdicts();
        return Effect.isEffect(given) ? given : Effect.succeed(given);
    }).pipe(
        Effect.map((given) => settle(given).map((verdict): Owned => ({ rule, verdict }))),
        Effect.catchCause((cause) => Effect.succeed<readonly Owned[]>([{ rule, verdict: { note: `failed and judged nothing: ${Cause.pretty(cause)}`, once: 'call' } }])),
    );
const tallied = (owned: readonly Owned[]): Tally =>
    owned.reduce<Tally>(
        (held, { rule, verdict }) =>
            Match.value(verdict).pipe(
                Match.when({ refuse: Match.string }, ({ refuse, instruct }) => ({ ...held, reasons: Array.dedupe([...held.reasons, `${refuse} ${instruct}`]) })),
                Match.when({ note: Match.string }, (note) => ({ ...held, notes: [...held.notes, { ...note, rule }] })),
                Match.when({ edit: Match.defined }, (edit) => ({ ...held, edits: [...held.edits, { ...edit, rule }] })),
                Match.orElse(({ run }) => ({ ...held, runs: [...held.runs, run] })),
            ),
        { reasons: [], notes: [], edits: [], runs: [] },
    );
const said = ({ program, aliases = [], path = [] }: Extract<Rule, { readonly program: string }>, invocation: Invocation): Option.Option<readonly string[]> => {
    const written = (Option.isSome(invocation.usage) ? [...invocation.path.map((part) => Option.some(part)), ...invocation.operands.map(({ word }) => word.text)] : Array.drop(invocation.words, 1).map(({ text }) => text)).map(Option.getOrElse(() => ''));
    const named = Match.value(invocation.program).pipe(
        Match.tag('External', 'Builtin', ({ name }) => [program, ...aliases].includes(name)),
        Match.orElse(Function.constFalse),
    );
    return Option.liftPredicate(invocation.path.length < path.length ? path : invocation.path, () => named && path.every((part, index) => written[index] === part));
};
const judged = (name: string, rule: Rule, found: readonly Invocation[], call: Call): Effect.Effect<readonly Owned[], never, Needs> =>
    Match.value(rule).pipe(
        Match.when({ script: true }, (row) => ran(name, () => row.judge(call.script, call), Function.identity)),
        Match.when({ program: Match.string }, (row) =>
            Effect.map(
                Effect.forEach(
                    found.flatMap((invocation) => Option.toArray(Option.map(said(row, invocation), (path) => ({ invocation, path })))),
                    ({ invocation, path }) =>
                        'judge' in row
                            ? ran(
                                  name,
                                  () => row.judge(invocation, call),
                                  (given) => settled(given, invocation, path),
                              )
                            : ran(name, () => plain(row, invocation, path), Function.identity),
                ),
                Array.flatten,
            ),
        ),
        Match.orElse(() => Effect.succeed([])),
    );
const used = <K extends Tool>(name: string, rule: Used<K>, action: Action): Effect.Effect<readonly Owned[], never, Needs> => (Schema.is(Action[rule.tool])(action) ? ran(name, () => ('judge' in rule ? rule.judge(action) : [{ refuse: rule.refuse, instruct: rule.instruct }]), Function.identity) : Effect.succeed([]));
const failure = <E>(cause: Cause.Cause<E>) =>
    Option.getOrElse(
        Option.flatMap(Cause.findErrorOption(cause), (error) => Option.filter(Option.some(Predicate.hasProperty(error, 'message') ? error.message : undefined), Predicate.isString)),
        () => Cause.pretty(cause),
    );
const overlapping = (cause: Cause.Cause<Effect.Error<ReturnType<Shell['Service']['edit']>>>) => Option.exists(Cause.findErrorOption(cause), ({ reason }) => reason._tag === 'Edit' && reason.kind === 'Overlap');

// --- [MODELS] --------------------------------------------------------------------------

class Policy {
    readonly #rules: readonly (readonly [string, Rule])[];
    readonly #heard: readonly ((input: unknown) => boolean)[];

    constructor(...tables: Table[]) {
        const rules = Record.filter(
            tables.reduce((merged, table) => Record.union(merged, table, (_, later) => later), git),
            Predicate.isNotNull,
        );
        this.#rules = Record.toEntries(rules);
        this.#heard = Array.dedupe(Record.collect(rules, (_, rule): Kind => rule.tool ?? 'Shell')).map((kind) => Schema.is(Schema.toEncoded(Action[kind])));
    }

    hears(input: unknown): boolean {
        return this.#heard.some((heard) => heard(input));
    }

    judge<E, R>(input: unknown, dialect: Effect.Effect<Dialect, E, R>, cwd: string): Effect.Effect<Judgment, never, Needs | R> {
        const rules = this.#rules;
        const converged = Effect.fnUntraced(function* (action: Action, first: Script, round: Round): Effect.fn.Return<readonly [Round, readonly string[]], Cause.YieldableError, Needs> {
            const shell = yield* Shell;
            const found = (yield* round.script.invocations).flatMap(({ leaves }) => leaves);
            const tally = tallied((yield* Effect.forEach(rules, ([name, rule]) => judged(name, rule, found, { action, script: round.script, cwd, shell, run: round.run }))).flat());
            const reached = { ...round, run: Array.max([round.run, ...tally.runs], ranked), notes: [...round.notes, ...tally.notes] };
            const applied = yield* Effect.reduce(
                Array.isReadonlyArrayNonEmpty(tally.reasons) ? [] : tally.edits,
                () => ({ edit: (_editor: Editor, _within: (nested: Script) => Editor): readonly Edit[] => [], rules: Array.empty<string>(), script: Option.none<Script>(), failed: Array.empty<Ruled<Note>>() }),
                (held, verdict) => {
                    const edit = (editor: Editor, within: (nested: Script) => Editor) => [...held.edit(editor, within), ...verdict.edit(editor, within)];
                    return Effect.map(
                        Effect.exit(shell.edit(round.script, edit)),
                        Exit.match({
                            onSuccess: (edited) => ({ ...held, edit, rules: [...held.rules, verdict.rule], script: Option.some(edited) }),
                            onFailure: (cause) => (Array.isReadonlyArrayNonEmpty(held.rules) && overlapping(cause) ? held : { ...held, failed: [...held.failed, { rule: verdict.rule, note: `its rewrite could not apply, so the call ran without it: ${failure(cause)}`, once: 'call' as const }] }),
                        }),
                    );
                },
            );
            const editors = Array.dedupe([...round.editors, ...applied.rules]);
            const next = { ...reached, notes: [...reached.notes, ...applied.failed] };
            return yield* Option.match(applied.script, {
                onNone: () =>
                    next.run !== round.run && !Array.isReadonlyArrayNonEmpty(tally.reasons)
                        ? converged(action, first, next)
                        : Effect.succeed([next, Array.isReadonlyArrayNonEmpty(tally.reasons) && Array.isReadonlyArrayNonEmpty(round.editors) ? [`${round.editors.join(', ')} rewrote the call to ${round.script.source}, which is refused:`, ...tally.reasons] : tally.reasons] as const),
                onSome: (edited) =>
                    Match.value({ cycled: round.seen.includes(edited.source), endless: round.seen.length >= editors.length + 2 }).pipe(
                        Match.when({ cycled: false, endless: false }, () => converged(action, first, { ...next, script: edited, seen: [...round.seen, edited.source], editors })),
                        Match.orElse(({ cycled }) =>
                            Effect.succeed([
                                {
                                    ...next,
                                    script: first,
                                    notes: [
                                        ...next.notes,
                                        {
                                            rule: editors.join(', '),
                                            note: cycled ? `rewrites undo each other (${[...round.seen, edited.source].join(' then ')}), so the call ran as written.` : `rewrites never settle (${round.seen.length} rounds, the last ${edited.source}), so the call ran as written.`,
                                            once: 'call' as const,
                                        },
                                    ],
                                },
                                [],
                            ] as const),
                        ),
                    ),
            });
        });
        const shelled = Effect.fnUntraced(function* (action: typeof Action.Shell.Type, called: Run) {
            const [shell, found] = yield* Effect.all([Shell, dialect]);
            const script = yield* shell.read(action.command, { dialect: found, cwd });
            const [ended, reasons] = yield* Effect.provideService(converged(action, script, { script, run: called, notes: [], seen: [script.source], editors: [] }), ConfigProvider.ConfigProvider, ConfigProvider.fromEnvRecord({ ...found.environment }));
            return { notes: ended.notes, reasons, command: Option.liftPredicate(ended.script.source, (source) => source !== action.command || ended.run !== called), run: ended.run === called ? ('foreground' as const) : ended.run };
        });
        return Effect.gen(function* () {
            const action = yield* Schema.decodeUnknownEffect(Action)(input);
            const called: Run = Match.value(action).pipe(
                Match.when({ tool: 'Monitor' }, () => 'monitor' as const),
                Match.when({ run_in_background: true }, () => 'background' as const),
                Match.orElse(() => 'foreground' as const),
            );
            const { notes, reasons, command, run } = shellish(action) ? yield* shelled(action, called) : { ...tallied((yield* Effect.forEach(rules, ([name, rule]) => (rule.tool === undefined ? Effect.succeed([]) : used(name, rule, action)))).flat()), command: Option.none(), run: 'foreground' as const };
            const told = Array.dedupe(notes.map(({ rule, note, once }) => new Told({ text: `${rule}: ${note}`, once, by: { session: () => '', agent: () => Option.getOrElse(action.agentId, () => ''), call: () => action.tool_use_id }[once]() })));
            return Array.isReadonlyArrayNonEmpty(reasons)
                ? Judgment.Refuse({ reasons, notes: told })
                : Option.match(command, {
                      onNone: () => Judgment.Pass({ notes: told }),
                      onSome: (written) => Judgment.Rewrite({ command: written, run, notes: told }),
                  });
        }).pipe(Effect.catchCause((cause) => Effect.succeed(Judgment.Pass({ notes: [new Told({ text: `policy could not judge this call and let it run: ${failure(cause)}`, once: 'call', by: '' })] }))));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Policy };
