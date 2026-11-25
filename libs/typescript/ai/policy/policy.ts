import { Shell } from '@rasm/shell';
import { Array, ConfigProvider, Duration, Effect, Equal, Exit, HashSet, Match, Number, Option, Predicate, Record, Ref, Stream } from 'effect';
import { Action, byRun, Decision, Scope, scopes } from '#/decision/action';
import { compile, type Compiled, compiled, message, type Requirements, type Round, type Table, type Tally, tally } from '#/decision/rule';
import { git } from '#/rules/git';

// --- [TYPES] ---------------------------------------------------------------------------

type Script = Round['call']['script'];
type Environment = Required<Pick<Parameters<Shell['Service']['read']>[1], 'cwd' | 'dialect'>>;
type Outcome = Pick<Tally, 'faults' | 'move' | 'notes' | 'reasons'> & { readonly script: Script };
interface Progress extends Pick<Tally, 'faults' | 'move' | 'notes'> {
    readonly trail: Array.NonEmptyReadonlyArray<{ readonly script: Script; readonly by: readonly string[] }>;
    readonly written: Option.Option<Pick<Tally, 'move' | 'notes'>>;
}
interface Edits extends Pick<Tally, 'faults'> {
    readonly edits: readonly Tally['edits'][number]['edit'][];
    readonly rules: readonly string[];
    readonly script: Option.Option<Script>;
}
interface Result extends Pick<Tally, 'faults' | 'notes' | 'reasons'> {
    readonly instead: Option.Option<Action>;
}

// --- [SERVICES] ------------------------------------------------------------------------

class Policy {
    readonly #compiled: Compiled;
    readonly #told = Ref.makeUnsafe(HashSet.empty<{ readonly text: string; readonly scope: Scope }>());

    constructor(...tables: Table[]) {
        const rules = Record.filter(
            tables.reduce((merged, table) => Record.union(merged, table, (_, later) => later), git),
            Predicate.isNotNull,
        );
        this.#compiled = compiled.combineAll(Record.collect(rules, compile));
    }

    appliesTo(kind: Action['_tag']): boolean {
        return this.#compiled.kinds.includes(kind);
    }

    forget(agent: Action['agent']): Effect.Effect<void> {
        return Ref.update(
            this.#told,
            HashSet.filter(({ scope }) => !Equal.equals(scope, Scope.Agent({ agent }))),
        );
    }

    clear(): Effect.Effect<void> {
        return Ref.set(this.#told, HashSet.empty());
    }

    undecided(agent: Action['agent'], why: string, once: keyof typeof scopes): Effect.Effect<Decision> {
        return Effect.map(this.#fresh(agent, [{ text: `policy did not judge this call and let it run: ${why}`, once }]), (faults) => Decision.Allow({ notes: [], faults }));
    }

    decide<E, R>(action: Action, environment: Effect.Effect<Environment, E, R>): Effect.Effect<Decision, never, Requirements | R> {
        const { shell, others } = this.#compiled;
        const decideOther = (other: Action) =>
            Effect.map(
                Effect.forEach(others, (decide) => decide(other)),
                tally.combineAll,
            );
        const decided = Action.match(action, {
            Shell: (call) => decideShell(call, shell, environment),
            Task: (task) => Effect.map(decideOther(task), ({ again, ...sum }): Result => ({ ...sum, instead: Option.liftPredicate(Action.cases.Task.make({ ...task, again }), () => Duration.isFinite(again) && Duration.isPositive(again)) })),
            Isolate: (isolate) => Effect.map(decideOther(isolate), ({ apart, ...sum }): Result => ({ ...sum, instead: Option.liftPredicate(Action.cases.Isolate.make({ ...isolate, apart }), () => apart !== isolate.apart) })),
            Read: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
            Write: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
            Edit: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
            Fetch: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
            Mcp: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
            Spawn: (other) => Effect.map(decideOther(other), (sum): Result => ({ ...sum, instead: Option.none() })),
        });
        return decided.pipe(
            Effect.flatMap(({ reasons, notes, faults, instead }) =>
                Effect.map(Effect.all({ notes: this.#fresh(action.agent, notes), faults: this.#fresh(action.agent, faults) }), (told) =>
                    Array.match(Array.dedupe(reasons), {
                        onNonEmpty: (given) => Decision.Deny({ ...told, reasons: given }),
                        onEmpty: () => Option.match(instead, { onNone: () => Decision.Allow(told), onSome: (restated) => Decision.Rewrite({ ...told, action: restated }) }),
                    }),
                ),
            ),
            Effect.catchCause((cause) => this.undecided(action.agent, message(cause), 'call')),
        );
    }

    #fresh(agent: Action['agent'], notes: Tally['notes']): Effect.Effect<readonly string[]> {
        return Ref.modify(this.#told, (heard) => {
            const fresh = notes.map(({ text, once }) => ({ text, scope: scopes[once]({ agent }) })).filter((note) => !HashSet.has(heard, note));
            return [Array.dedupe(fresh.map(({ text }) => text)), HashSet.union(heard, HashSet.fromIterable(fresh.filter(({ scope }) => !Scope.$is('Call')(scope))))] as const;
        });
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------

const decideShell = Effect.fnUntraced(function* <E, R>(action: Round['call']['action'], rounds: Compiled['shell'], environment: Effect.Effect<Environment, E, R>) {
    const [shell, { dialect, cwd }] = yield* Effect.all([Shell, environment]);
    const written = yield* shell.read(action.command, { dialect, cwd });
    const [asked, words] = [{ run: action.run, notes: [] }, Number.sumAll((yield* written.invocations).flatMap(({ leaves }) => leaves).map((invocation) => invocation.words.length))];
    const attempt = (script: Script, edits: Edits['edits']) => Effect.exit(shell.edit(script, (editor, within) => edits.flatMap((each) => each(editor, within))));
    const apply = (script: Script) =>
        Effect.fnUntraced(function* (held: Edits, { rule, edit }: Tally['edits'][number]) {
            const joined = yield* attempt(script, [...held.edits, edit]);
            const alone = Exit.isFailure(joined) && Array.isReadonlyArrayNonEmpty(held.rules) ? yield* attempt(script, [edit]) : joined;
            return Match.value({ joined, alone, idle: Option.exists(held.script, ({ source }) => source === script.source) }).pipe(
                Match.when({ joined: Exit.isSuccess }, ({ joined: { value } }): Edits => ({ ...held, edits: [...held.edits, edit], rules: [...held.rules, rule], script: Option.some(value) })),
                Match.when({ alone: Exit.isSuccess, idle: true }, ({ alone: { value } }): Edits => ({ ...held, edits: [edit], rules: [rule], script: Option.some(value) })),
                Match.when({ alone: Exit.isFailure }, ({ alone: { cause } }): Edits => ({ ...held, faults: [...held.faults, { text: `${rule}: its rewrite could not apply, so the call ran without it: ${message(cause)}`, once: 'call' }] })),
                Match.orElse(() => held),
            );
        });
    const round = Effect.fnUntraced(function* ({ trail, move: reached, notes: told, faults: failed, written: first }: Progress): Effect.fn.Return<readonly [readonly Outcome[], Option.Option<Progress>], Effect.Error<Script['invocations']>, Requirements> {
        const { script } = Array.lastNonEmpty(trail);
        const invocations = (yield* script.invocations).flatMap(({ leaves }) => leaves);
        const sum = tally.combineAll(yield* Effect.forEach(rounds, (decide) => decide({ call: { action, script, cwd, shell, run: reached.run }, invocations })));
        const edited = yield* Effect.reduce(Array.isReadonlyArrayNonEmpty(sum.reasons) ? [] : sum.edits, (): Edits => ({ edits: [], rules: [], script: Option.none(), faults: [] }), apply(script));
        const before = Array.dedupe(trail.flatMap(({ by }) => by));
        const editors = Array.dedupe([...before, ...edited.rules]);
        const held = { move: byRun(sum.move.run, reached.run) === 1 ? sum.move : reached, notes: [...told, ...sum.notes], faults: [...failed, ...sum.faults, ...edited.faults] };
        const kept = Option.getOrElse(first, () => ({ move: held.move, notes: held.notes }));
        const rewritten = Option.filter(edited.script, ({ source }) => source !== script.source);
        const sources = [...trail.map((seen) => seen.script.source), ...Option.toArray(rewritten).map(({ source }) => source)];
        const revert = (how: string) => [[{ ...kept, script: Array.headNonEmpty(trail).script, reasons: [], faults: [...held.faults, { text: `${editors.join(', ')}: rewrites ${how}, so the call ran as written.`, once: 'call' as const }] }], Option.none()] as const;
        return Match.value({ denied: Array.isReadonlyArrayNonEmpty(sum.reasons), rewritten: Option.getOrUndefined(rewritten), cycled: new Set(sources).size < sources.length, endless: trail.length >= editors.length * Math.max(1, words) + 2, moved: held.move.run !== reached.run }).pipe(
            Match.when({ denied: true }, () => [[{ ...held, notes: sum.notes, script, reasons: Array.isReadonlyArrayNonEmpty(before) ? [`${before.join(', ')} rewrote the call to ${script.source}, which is refused:`, ...sum.reasons] : sum.reasons }], Option.none()] as const),
            Match.when({ rewritten: Match.defined, cycled: true }, () => revert(`undo each other (${sources.join(' then ')})`)),
            Match.when({ rewritten: Match.defined, endless: true }, () => revert(`never settle (${trail.length} rounds, the last ${sources.at(-1)})`)),
            Match.when({ rewritten: Match.defined }, ({ rewritten: next }) => [[], Option.some({ ...held, written: Option.some(kept), trail: Array.append(trail, { script: next, by: edited.rules }) })] as const),
            Match.when({ moved: true }, () => [[], Option.some({ ...held, written: Option.some(kept), trail })] as const),
            Match.orElse(() => [[{ ...held, script, reasons: [] }], Option.none()] as const),
        );
    });
    const { script, move, notes, faults, reasons } = yield* Stream.paginate({ trail: [{ script: written, by: [] }], move: asked, notes: [], faults: [], written: Option.none() }, round).pipe(
        Stream.runFold(
            (): Outcome => ({ script: written, move: asked, notes: [], faults: [], reasons: [] }),
            (_, outcome) => outcome,
        ),
        Effect.provideService(ConfigProvider.ConfigProvider, ConfigProvider.fromEnvRecord({ ...dialect.environment })),
    );
    const restated = Action.cases.Shell.make({ ...action, command: script.source, run: move.run });
    return { reasons, faults, notes: [...notes, ...(Array.isReadonlyArrayNonEmpty(reasons) ? [] : move.notes)], instead: Option.liftPredicate(restated, () => restated.command !== action.command || restated.run !== action.run) } satisfies Result;
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Policy };
