import { Shell } from '@rasm/shell';
import { Array, Cause, ConfigProvider, Duration, Effect, Exit, HashSet, Match, Option, Path, Predicate, Record, Ref, Stream } from 'effect';
import { Action, Decision, scopes } from '#/decision/action';
import { compile, type Compiled, compiledAll, furthest, message, type Requirements, type Round, type Table, type Tally, tally } from '#/decision/rule';
import { git } from '#/rules/git';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Round['call']['script'];
type Environment = Required<Pick<Parameters<Shell['Service']['read']>[1], 'cwd' | 'dialect'>>;
interface Progress extends Pick<Tally, 'move' | 'notes'> {
    readonly trail: Array.NonEmptyReadonlyArray<{ readonly script: Script; readonly by: readonly string[] }>;
}
interface Outcome extends Pick<Tally, 'move' | 'notes' | 'reasons'> {
    readonly script: Script;
}
interface Edits {
    readonly edits: readonly Tally['edits'][number]['edit'][];
    readonly rules: readonly string[];
    readonly script: Option.Option<Script>;
    readonly failed: Tally['notes'];
}
interface Result extends Pick<Tally, 'notes' | 'reasons'> {
    readonly instead: Option.Option<Action>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const overlaps = (cause: Cause.Cause<Effect.Error<ReturnType<Shell['Service']['edit']>>>) => Option.exists(Cause.findErrorOption(cause), ({ reason }) => reason._tag === 'Edit' && reason.kind === 'Overlap');
const decideShell = Effect.fnUntraced(function* <E, R>(action: Round['call']['action'], rounds: Compiled['shell'], environment: Effect.Effect<Environment, E, R>) {
    const [shell, { dialect, cwd }] = yield* Effect.all([Shell, environment]);
    const written = yield* shell.read(action.command, { dialect, cwd });
    const apply =
        (script: Script) =>
        (held: Edits, { rule, edit }: Tally['edits'][number]) => {
            const edits = [...held.edits, edit];
            return Effect.map(
                Effect.exit(shell.edit(script, (editor, within) => edits.flatMap((each) => each(editor, within)))),
                Exit.match({
                    onSuccess: (edited): Edits => ({ ...held, edits, rules: [...held.rules, rule], script: Option.some(edited) }),
                    onFailure: (cause): Edits => (Array.isReadonlyArrayNonEmpty(held.rules) && overlaps(cause) ? held : { ...held, failed: [...held.failed, { text: `${rule}: its rewrite could not apply, so the call ran without it: ${message(cause)}`, once: 'call' }] }),
                }),
            );
        };
    const round = Effect.fnUntraced(function* ({ trail, move: reached, notes: told }: Progress): Effect.fn.Return<readonly [readonly Outcome[], Option.Option<Progress>], Effect.Error<Script['invocations']>, Requirements> {
        const { script } = Array.lastNonEmpty(trail);
        const invocations = (yield* script.invocations).flatMap(({ leaves }) => leaves);
        const sum = tally.combineAll(yield* Effect.forEach(rounds, (decide) => decide({ call: { action, script, cwd, shell, run: reached.run }, invocations })));
        const edited = yield* Effect.reduce(Array.isReadonlyArrayNonEmpty(sum.reasons) ? [] : sum.edits, (): Edits => ({ edits: [], rules: [], script: Option.none(), failed: [] }), apply(script));
        const before = Array.dedupe(trail.flatMap(({ by }) => by));
        const editors = Array.dedupe([...before, ...edited.rules]);
        const [move, notes] = [furthest.combine(sum.move, reached), [...told, ...sum.notes, ...edited.failed]];
        const rewritten = Option.filter(edited.script, ({ source }) => source !== script.source);
        const sources = [...trail.map((seen) => seen.script.source), ...Option.toArray(rewritten).map(({ source }) => source)];
        const end = (to: Script, reasons: readonly string[], unsettled: readonly string[]) => [[{ move, script: to, reasons, notes: [...notes, ...unsettled.map((how) => ({ text: `${editors.join(', ')}: rewrites ${how}, so the call ran as written.`, once: 'call' as const }))] }], Option.none()] as const;
        return Match.value({ denied: Array.isReadonlyArrayNonEmpty(sum.reasons), rewritten: Option.getOrUndefined(rewritten), cycled: new Set(sources).size < sources.length, endless: trail.length >= editors.length + 2, moved: move.run !== reached.run }).pipe(
            Match.when({ denied: true }, () => end(script, Array.isReadonlyArrayNonEmpty(before) ? [`${before.join(', ')} rewrote the call to ${script.source}, which is refused:`, ...sum.reasons] : sum.reasons, [])),
            Match.when({ rewritten: Match.defined, cycled: true }, () => end(Array.headNonEmpty(trail).script, [], [`undo each other (${sources.join(' then ')})`])),
            Match.when({ rewritten: Match.defined, endless: true }, () => end(Array.headNonEmpty(trail).script, [], [`never settle (${trail.length} rounds, the last ${sources.at(-1)})`])),
            Match.when({ rewritten: Match.defined }, ({ rewritten: next }) => [[], Option.some({ move, notes, trail: Array.append(trail, { script: next, by: edited.rules }) })] as const),
            Match.when({ moved: true }, () => [[], Option.some({ move, notes, trail })] as const),
            Match.orElse(() => end(script, [], [])),
        );
    });
    const { script, move, notes, reasons } = yield* Stream.paginate({ trail: [{ script: written, by: [] }], move: { run: action.run, notes: [] }, notes: [] }, round).pipe(
        Stream.runHead,
        Effect.flatMap(Effect.fromOption),
        Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnvRecord({ ...dialect.environment })),
        Effect.updateService(Path.Path, (path) => Path.make({ ...path, resolve: (...segments) => path.resolve(cwd, ...segments) })),
    );
    const restated = Action.Shell({ ...action, command: script.source, run: move.run });
    return { reasons, notes: [...notes, ...(Array.isReadonlyArrayNonEmpty(reasons) ? [] : move.notes)], instead: Option.liftPredicate(restated, () => restated.command !== action.command || restated.run !== action.run) } satisfies Result;
});

// --- [MODELS] --------------------------------------------------------------------------

class Policy {
    readonly #compiled: Compiled;
    readonly #told = Ref.makeUnsafe(HashSet.empty<{ readonly text: string; readonly scope: ReturnType<(typeof scopes)[keyof typeof scopes]> }>());

    constructor(...tables: Table[]) {
        const rules = Record.filter(
            tables.reduce((merged, table) => Record.union(merged, table, (_, later) => later), git),
            Predicate.isNotNull,
        );
        this.#compiled = compiledAll.combineAll(Record.collect(rules, compile));
    }

    appliesTo(kind: Action['_tag']): boolean {
        return this.#compiled.kinds.includes(kind);
    }

    forget(agent: Action['agent']): Effect.Effect<void> {
        return Ref.update(
            this.#told,
            HashSet.filter(({ scope }) => !Option.contains(scope, agent)),
        );
    }

    undecided(agent: Action['agent'], why: string, once: keyof typeof scopes): Effect.Effect<Decision> {
        return Effect.map(this.#fresh(agent, [{ text: `policy did not judge this call and let it run: ${why}`, once }]), (notes) => Decision.Allow({ notes }));
    }

    decide<E, R>(action: Action, environment: Effect.Effect<Environment, E, R>): Effect.Effect<Decision, never, Requirements | R> {
        const { shell, others } = this.#compiled;
        const decideOther = (other: Exclude<Action, { readonly _tag: 'Shell' }>) =>
            Effect.map(
                Effect.forEach(others, (decide) => decide(other)),
                tally.combineAll,
            );
        const decided = Match.value(action).pipe(
            Match.tag('Shell', (call) => decideShell(call, shell, environment)),
            Match.tag('Task', (task) => Effect.map(decideOther(task), ({ reasons, notes, again }): Result => ({ reasons, notes, instead: Option.liftPredicate(Action.Task({ ...task, again }), () => Duration.isFinite(again)) }))),
            Match.orElse((other) => Effect.map(decideOther(other), ({ reasons, notes }): Result => ({ reasons, notes, instead: Option.none() }))),
        );
        return decided.pipe(
            Effect.flatMap(({ reasons, notes, instead }) =>
                Effect.map(this.#fresh(action.agent, notes), (fresh) =>
                    Array.match(Array.dedupe(reasons), {
                        onNonEmpty: (given) => Decision.Deny({ reasons: given, notes: fresh }),
                        onEmpty: () => Option.match(instead, { onNone: () => Decision.Allow({ notes: fresh }), onSome: (restated) => Decision.Rewrite({ action: restated, notes: fresh }) }),
                    }),
                ),
            ),
            Effect.catchCause((cause) => this.undecided(action.agent, message(cause), 'call')),
        );
    }

    #fresh(agent: Action['agent'], notes: Tally['notes']): Effect.Effect<readonly string[]> {
        return Ref.modify(this.#told, (heard) => {
            const fresh = Array.dedupe(notes.map(({ text, once }) => ({ text, scope: scopes[once]({ agent }) }))).filter((note) => !HashSet.has(heard, note));
            return [fresh.map(({ text }) => text), HashSet.union(heard, HashSet.fromIterable(fresh.filter(({ scope }) => Option.isSome(scope))))] as const;
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Policy };
