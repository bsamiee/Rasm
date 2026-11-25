import { Shell } from '@rasm/shell';
import { Array, Cause, Data, Effect, Function, HashSet, Match, Option, Predicate, Record, Schema } from 'effect';
import { Action, type Kind } from '#/judgment/action';
import { type Call, type Dialect, type Invocation, type Needs, type Note, plain, type Rule, type Script, type Settled, settled, type Table, texted, Told, type Tool, type Used, type Verdicts } from '#/judgment/rule';
import { git } from '#/rules/git';

// --- [TYPES] ---------------------------------------------------------------------------

type Judgment = Data.TaggedEnum<{
    Pass: { readonly notes: readonly Told[] };
    Refuse: { readonly reasons: Array.NonEmptyReadonlyArray<string>; readonly notes: readonly Told[] };
    Rewrite: { readonly command: string; readonly background: boolean; readonly notes: readonly Told[] };
}>;
interface Owned {
    readonly rule: string;
    readonly verdict: Settled;
}
interface Tally {
    readonly reasons: readonly string[];
    readonly notes: readonly (Note & { readonly rule: string })[];
    readonly edits: readonly (Extract<Settled, { readonly edit: unknown }> & { readonly rule: string })[];
    readonly background: boolean;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const Judgment = Data.taggedEnum<Judgment>();
const shellish = Schema.is(Action.Shell);

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
                Match.orElse(() => ({ ...held, background: true })),
            ),
        { reasons: [], notes: [], edits: [], background: false },
    );
const nested = (invocation: Invocation): readonly Invocation[] => [invocation, ...invocation.inner.flatMap(nested)];
const said = ({ program, aliases = [], path = [] }: Extract<Rule, { readonly program: string }>, invocation: Invocation): Option.Option<readonly string[]> => {
    const written = (Option.isSome(invocation.usage) ? [...invocation.path.map((part) => Option.some(part)), ...invocation.operands.map(({ word }) => texted(word))] : Array.drop(invocation.words, 1).map(texted)).map(Option.getOrElse(() => ''));
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
        const converged = Effect.fnUntraced(function* (script: Script, action: Action, seen: HashSet.HashSet<string>, earlier: Pick<Tally, 'notes' | 'background'>): Effect.fn.Return<readonly [Script, Tally], Cause.YieldableError, Needs> {
            const shell = yield* Shell;
            const found = (yield* script.invocations).flatMap(nested);
            const tally = tallied((yield* Effect.forEach(rules, ([name, rule]) => judged(name, rule, found, { action, script, cwd }))).flat());
            const reached = { ...tally, notes: [...earlier.notes, ...tally.notes], background: earlier.background || tally.background };
            const named = Array.dedupe(tally.edits.map(({ rule }) => rule)).join(', ');
            return yield* Array.isReadonlyArrayNonEmpty(tally.reasons) || !Array.isReadonlyArrayNonEmpty(tally.edits)
                ? Effect.succeed([script, reached] as const)
                : shell
                      .edit(script, (editor, within) => tally.edits.flatMap(({ edit }) => edit(editor, within)))
                      .pipe(
                          Effect.matchCauseEffect({
                              onFailure: (cause) => Effect.succeed([script, { ...reached, notes: [...reached.notes, { rule: named, note: `edits overlap or fail, so none applied: ${Cause.pretty(cause)}`, once: 'call' as const }] }] as const),
                              onSuccess: (edited) => (HashSet.has(seen, edited.source) ? Effect.succeed([edited, reached] as const) : converged(edited, action, HashSet.add(seen, edited.source), reached)),
                          }),
                      );
        });
        return Effect.gen(function* () {
            const action = yield* Schema.decodeUnknownEffect(Action)(input);
            const { tally, command, background } = shellish(action)
                ? yield* Effect.flatMap(Effect.all([Shell, dialect]), ([shell, found]) => shell.read(action.command, { dialect: found, cwd })).pipe(
                      Effect.flatMap((script) => converged(script, action, HashSet.make(script.source), { notes: [], background: false })),
                      Effect.map(([ended, reached]) => {
                          const detached = reached.background && action.run_in_background !== true;
                          return { tally: reached, command: Option.liftPredicate(ended.source, (source) => source !== action.command || detached), background: detached };
                      }),
                  )
                : { tally: tallied((yield* Effect.forEach(rules, ([name, rule]) => (rule.tool === undefined ? Effect.succeed([]) : used(name, rule, action)))).flat()), command: Option.none(), background: false };
            const notes = Array.dedupe(tally.notes.map(({ rule, note, once }) => new Told({ text: `${rule}: ${note}`, once, by: { session: () => '', agent: () => Option.getOrElse(action.agentId, () => ''), call: () => action.tool_use_id }[once]() })));
            return Array.isReadonlyArrayNonEmpty(tally.reasons)
                ? Judgment.Refuse({ reasons: tally.reasons, notes })
                : Option.match(command, {
                      onNone: () => Judgment.Pass({ notes }),
                      onSome: (written) => Judgment.Rewrite({ command: written, background, notes }),
                  });
        }).pipe(Effect.catchCause((cause) => Effect.succeed(Judgment.Pass({ notes: [new Told({ text: `policy could not judge this call and let it run: ${Cause.pretty(cause)}`, once: 'call', by: '' })] }))));
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Policy };
