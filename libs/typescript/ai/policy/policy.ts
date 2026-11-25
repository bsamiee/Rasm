import { Shell } from '@rasm/shell';
import { Array, Cause, ConfigProvider, Duration, Effect, Exit, HashSet, Match, Option, Path, Predicate, Record, Ref, Stream } from 'effect';
import { Action, Judgment, lifetimes } from '#/judgment/action';
import { compiled, failure, type Judges, judges, moved, type Needs, type Round, type Table, type Tally, tallied } from '#/judgment/rule';
import { git } from '#/rules/git';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Round['call']['script'];
type At = Required<Pick<Parameters<Shell['Service']['read']>[1], 'cwd' | 'dialect'>>;
interface Held extends Pick<Tally, 'move' | 'notes'> {
    readonly trail: Array.NonEmptyReadonlyArray<{ readonly script: Script; readonly by: readonly string[] }>;
}
interface Ended extends Pick<Tally, 'move' | 'notes' | 'reasons'> {
    readonly script: Script;
}
interface Applied {
    readonly edits: readonly Tally['edits'][number]['edit'][];
    readonly rules: readonly string[];
    readonly script: Option.Option<Script>;
    readonly failed: Tally['notes'];
}
interface Ruling extends Pick<Tally, 'notes' | 'reasons'> {
    readonly instead: Option.Option<Action>;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const overlapping = (cause: Cause.Cause<Effect.Error<ReturnType<Shell['Service']['edit']>>>) => Option.exists(Cause.findErrorOption(cause), ({ reason }) => reason._tag === 'Edit' && reason.kind === 'Overlap');
const shelled = Effect.fnUntraced(function* <E, R>(action: Round['call']['action'], rounds: Judges['rounds'], at: Effect.Effect<At, E, R>) {
    const [shell, { dialect, cwd }] = yield* Effect.all([Shell, at]);
    const written = yield* shell.read(action.command, { dialect, cwd });
    const applied =
        (script: Script) =>
        (held: Applied, { rule, edit }: Tally['edits'][number]) => {
            const edits = [...held.edits, edit];
            return Effect.map(
                Effect.exit(shell.edit(script, (editor, within) => edits.flatMap((each) => each(editor, within)))),
                Exit.match({
                    onSuccess: (edited): Applied => ({ ...held, edits, rules: [...held.rules, rule], script: Option.some(edited) }),
                    onFailure: (cause): Applied => (Array.isReadonlyArrayNonEmpty(held.rules) && overlapping(cause) ? held : { ...held, failed: [...held.failed, { text: `${rule}: its rewrite could not apply, so the call ran without it: ${failure(cause)}`, once: 'call' }] }),
                }),
            );
        };
    const judged = Effect.fnUntraced(function* ({ trail, move: reached, notes: told }: Held): Effect.fn.Return<readonly [readonly Ended[], Option.Option<Held>], Effect.Error<Script['invocations']>, Needs> {
        const { script } = Array.lastNonEmpty(trail);
        const found = (yield* script.invocations).flatMap(({ leaves }) => leaves);
        const said = tallied.combineAll(yield* Effect.forEach(rounds, (judge) => judge({ call: { action, script, cwd, shell, wait: reached.wait }, found })));
        const edited = yield* Effect.reduce(Array.isReadonlyArrayNonEmpty(said.reasons) ? [] : said.edits, (): Applied => ({ edits: [], rules: [], script: Option.none(), failed: [] }), applied(script));
        const before = Array.dedupe(trail.flatMap(({ by }) => by));
        const editors = Array.dedupe([...before, ...edited.rules]);
        const [move, notes] = [moved.combine(said.move, reached), [...told, ...said.notes, ...edited.failed]];
        const rewritten = Option.filter(edited.script, ({ source }) => source !== script.source);
        const sources = [...trail.map((seen) => seen.script.source), ...Option.toArray(rewritten).map(({ source }) => source)];
        const ended = (to: Script, reasons: readonly string[], unsettled: readonly string[]) => [[{ move, script: to, reasons, notes: [...notes, ...unsettled.map((how) => ({ text: `${editors.join(', ')}: rewrites ${how}, so the call ran as written.`, once: 'call' as const }))] }], Option.none()] as const;
        return Match.value({ refused: Array.isReadonlyArrayNonEmpty(said.reasons), rewritten: Option.getOrUndefined(rewritten), cycled: new Set(sources).size < sources.length, endless: trail.length >= editors.length + 2, moved: move.wait !== reached.wait }).pipe(
            Match.when({ refused: true }, () => ended(script, Array.isReadonlyArrayNonEmpty(before) ? [`${before.join(', ')} rewrote the call to ${script.source}, which is refused:`, ...said.reasons] : said.reasons, [])),
            Match.when({ rewritten: Match.defined, cycled: true }, () => ended(Array.headNonEmpty(trail).script, [], [`undo each other (${sources.join(' then ')})`])),
            Match.when({ rewritten: Match.defined, endless: true }, () => ended(Array.headNonEmpty(trail).script, [], [`never settle (${trail.length} rounds, the last ${sources.at(-1)})`])),
            Match.when({ rewritten: Match.defined }, ({ rewritten: next }) => [[], Option.some({ move, notes, trail: Array.append(trail, { script: next, by: edited.rules }) })] as const),
            Match.when({ moved: true }, () => [[], Option.some({ move, notes, trail })] as const),
            Match.orElse(() => ended(script, [], [])),
        );
    });
    const { script, move, notes, reasons } = yield* Stream.paginate({ trail: [{ script: written, by: [] }], move: { wait: action.wait, told: [] }, notes: [] }, judged).pipe(
        Stream.runHead,
        Effect.flatMap(Effect.fromOption),
        Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnvRecord({ ...dialect.environment })),
        Effect.updateService(Path.Path, (path) => Path.make({ ...path, resolve: (...segments) => path.resolve(cwd, ...segments) })),
    );
    const restated = Action.Shell({ ...action, command: script.source, wait: move.wait });
    return { reasons, notes: [...notes, ...(Array.isReadonlyArrayNonEmpty(reasons) ? [] : move.told)], instead: Option.liftPredicate(restated, () => restated.command !== action.command || restated.wait !== action.wait) } satisfies Ruling;
});

// --- [MODELS] --------------------------------------------------------------------------

class Policy {
    readonly #judges: Judges;
    readonly #told = Ref.makeUnsafe(HashSet.empty<{ readonly text: string; readonly held: ReturnType<(typeof lifetimes)[keyof typeof lifetimes]> }>());

    constructor(...tables: Table[]) {
        const rules = Record.filter(
            tables.reduce((merged, table) => Record.union(merged, table, (_, later) => later), git),
            Predicate.isNotNull,
        );
        this.#judges = judges.combineAll(Record.collect(rules, compiled));
    }

    hears(kind: Action['_tag']): boolean {
        return this.#judges.heard.includes(kind);
    }

    forget(agent: Action['agent']): Effect.Effect<void> {
        return Ref.update(
            this.#told,
            HashSet.filter(({ held }) => !Option.contains(held, agent)),
        );
    }

    unjudged(agent: Action['agent'], why: string, once: keyof typeof lifetimes): Effect.Effect<Judgment> {
        return Effect.map(this.#fresh(agent, [{ text: `policy did not judge this call and let it run: ${why}`, once }]), (notes) => Judgment.Pass({ notes }));
    }

    judge<E, R>(action: Action, at: Effect.Effect<At, E, R>): Effect.Effect<Judgment, never, Needs | R> {
        const { rounds, actions } = this.#judges;
        const said = (other: Exclude<Action, { readonly _tag: 'Shell' }>) =>
            Effect.map(
                Effect.forEach(actions, (judge) => judge(other)),
                tallied.combineAll,
            );
        const ruled = Match.value(action).pipe(
            Match.tag('Shell', (shell) => shelled(shell, rounds, at)),
            Match.tag('Task', (task) => Effect.map(said(task), ({ reasons, notes, again }): Ruling => ({ reasons, notes, instead: Option.liftPredicate(Action.Task({ ...task, again }), () => Duration.isFinite(again)) }))),
            Match.orElse((other) => Effect.map(said(other), ({ reasons, notes }): Ruling => ({ reasons, notes, instead: Option.none() }))),
        );
        return ruled.pipe(
            Effect.flatMap(({ reasons, notes, instead }) =>
                Effect.map(this.#fresh(action.agent, notes), (fresh) =>
                    Array.match(Array.dedupe(reasons), {
                        onNonEmpty: (given) => Judgment.Refuse({ reasons: given, notes: fresh }),
                        onEmpty: () => Option.match(instead, { onNone: () => Judgment.Pass({ notes: fresh }), onSome: (restated) => Judgment.Rewrite({ action: restated, notes: fresh }) }),
                    }),
                ),
            ),
            Effect.catchCause((cause) => this.unjudged(action.agent, failure(cause), 'call')),
        );
    }

    #fresh(agent: Action['agent'], notes: Tally['notes']): Effect.Effect<readonly string[]> {
        return Ref.modify(this.#told, (heard) => {
            const fresh = Array.dedupe(notes.map(({ text, once }) => ({ text, held: lifetimes[once]({ agent }) }))).filter((note) => !HashSet.has(heard, note));
            return [fresh.map(({ text }) => text), HashSet.union(heard, HashSet.fromIterable(fresh.filter(({ held }) => Option.isSome(held))))] as const;
        });
    }
}

// --- [EXPORTS] -------------------------------------------------------------------------

export { Policy };
