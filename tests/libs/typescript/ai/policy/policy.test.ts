import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy/policy';
import { Array, Duration, Effect, MutableRef, Option } from 'effect';
import { environment, folder, printed, services, shell } from '#/fixtures';

// --- [TYPES] ---------------------------------------------------------------------------

type Table = ConstructorParameters<typeof Policy>[number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = { lines: 'pattern\nPATTERN\nother\n', needle: 'PATTERN\n' };
const caller = { call: 'call', agent: Option.none<string>() };
const isolate = { ...caller, _tag: 'Isolate', from: '/repository', apart: true } as const;
const task = { ...caller, _tag: 'Task', id: 'b1', output: '/tmp/b1.output', again: Duration.infinity } as const;
const respelled = [
    { name: 'a short flag renamed long inside a cluster keeps the value of the flag beside it', row: { program: 'grep', rename: { '-i': '--ignore-case' } }, asked: 'grep -ie PATTERN lines' },
    { name: 'a renamed flag keeps a value no reading of the script knows', row: { program: 'grep', rename: { '--regexp': '-e' } }, asked: 'grep --regexp="$(cat needle)" lines' },
] as const;
const written = [
    { name: 'a drop takes a flag whose value no reading of the script knows', row: { program: 'grep', drop: ['--exclude-dir'] }, asked: 'grep --exclude-dir="$(cat skip)" -r x .', ran: 'grep -r x .' },
    { name: 'an ensure lands after the subcommand, past a flag value that spells it', row: { program: 'git', path: ['log'], ensure: ['--oneline'] }, asked: 'git -C log log -p', ran: 'git -C log log --oneline -p' },
] as const;
const paths: Table = {
    patch: { program: 'zzpm', path: ['patch'], deny: 'copies a package into the tree', advice: 'Set the package option.' },
    sub: { program: 'sort', path: ['sub'], deny: 'names a file', advice: 'Sort another.' },
};
const rewrites: Table = {
    once: { program: 'tr', decide: ({ operands }) => Array.take(Array.getSomes(operands.filter(({ word }) => word.spells('a')).map(({ word }) => word.node)), 1).map((word) => ({ edit: (editor) => [editor.replace(word, ['b'])] })) },
    ping: { program: 'ping', decide: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['pong'])] })) },
    pong: { program: 'pong', decide: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['ping'])] })) },
    behind: {
        program: 'pong',
        decide: () => [
            { run: 'background', note: 'runs behind the turn' },
            { note: 'saw pong', once: 'call' },
        ],
    },
};
const notes: Table = {
    tail: {
        program: 'tail',
        decide: () => [
            { note: 'each agent', once: 'agent' },
            { note: 'the session', once: 'session' },
            { note: 'same', once: 'agent' },
            { note: 'same', once: 'session' },
        ],
    },
    first: { program: 'sleep', decide: () => [{ run: 'background', note: 'first' }] },
    second: { program: 'sleep', decide: () => [{ run: 'background', note: 'second' }] },
};
const replaced: Table = { 'git.stash': null, 'git.revert': { program: 'git', path: ['revert'], deny: 'is replaced', advice: 'Read the new row.' } };
const kinds: Table = {
    apart: { action: 'Isolate', decide: ({ worker }) => Option.match(worker, { onSome: () => [{ apart: false as const }], onNone: () => [{ deny: 'A second checkout repeats every build.', advice: 'Work in place.' }] }) },
    look: { action: 'Task', decide: ({ resting }) => [{ again: resting ? Duration.minutes(5) : Duration.zero }] },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = (later: MutableRef.MutableRef<boolean>): Table => ({
    held: { program: 'true', decide: () => Effect.as(Effect.never, []) },
    later: {
        program: 'true',
        decide: () =>
            Effect.as(
                Effect.sync(() => MutableRef.set(later, true)),
                [],
            ),
    },
});
const ran = (policy: Policy, command: string, cwd?: string) => Effect.map(policy.decide(shell(command), environment(cwd)), (decision) => (decision._tag === 'Rewrite' && decision.action._tag === 'Shell' ? [decision.action.command, decision.action.run] : [decision._tag]));
const heard = (policy: Policy, agent: Option.Option<string>) => Effect.map(policy.decide(shell('tail f', agent), environment()), (decision) => decision.notes.map((note) => note.replace('tail: ', '')));

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(services, { excludeTestServices: true })('policy', (suite) => {
    suite.effect.each(respelled)('$name', ({ row, asked }) =>
        Effect.gen(function* () {
            const cwd = yield* folder(files);
            const [command = asked] = yield* ran(new Policy({ respell: row }), asked, cwd);
            assert.notStrictEqual(command, asked);
            assert.deepStrictEqual(yield* printed(command, cwd), yield* printed(asked, cwd));
        }).pipe(Effect.scoped),
    );
    suite.effect.each(written)('$name', ({ row, asked, ran: expected }) => Effect.map(ran(new Policy({ respell: row }), asked), (found) => assert.deepStrictEqual(found, [expected, 'foreground'])));
    suite.effect('a path names subcommands and operands before the end of options', () =>
        Effect.gen(function* () {
            const policy = new Policy(paths);
            assert.deepStrictEqual([yield* ran(policy, 'zzpm -v patch left-pad'), yield* ran(policy, 'sort -- sub')], [['Deny'], ['Allow']]);
        }),
    );
    suite.effect('a rewrite settles in as many passes as its rule needs, and one that cannot settle runs the call as written', () =>
        Effect.gen(function* () {
            const policy = new Policy(rewrites);
            const reverted = yield* policy.decide(shell('ping h'), environment());
            assert.deepStrictEqual(yield* ran(policy, 'tr a a a'), ['tr b b b', 'foreground']);
            assert.deepStrictEqual([reverted._tag, reverted.notes, reverted.faults.length], ['Allow', [], 1]);
            assert.match(reverted.faults.join('\n'), /^ping, pong: rewrites undo each other/u);
        }),
    );
    suite.effect('a note is told once per scope, an agent forgets its own, and a cleared policy forgets all', () =>
        Effect.gen(function* () {
            const [policy, main, other] = [new Policy(notes), Option.none<string>(), Option.some('other')];
            const told = [yield* heard(policy, main), yield* heard(policy, main), yield* heard(policy, other)];
            yield* policy.forget(main);
            const forgotten = [yield* heard(policy, main), yield* heard(policy, other)];
            yield* policy.clear();
            const cleared = yield* heard(policy, other);
            const moved = yield* policy.decide(shell('sleep 5'), environment());
            assert.deepStrictEqual(told, [['each agent', 'the session', 'same'], [], ['each agent', 'same']]);
            assert.deepStrictEqual(forgotten, [['each agent', 'same'], []]);
            assert.deepStrictEqual(cleared, ['each agent', 'the session', 'same']);
            assert.deepStrictEqual(moved.notes, ['first: first', 'second: second']);
        }),
    );
    suite.effect('a later table replaces a row by name and removes one with null', () =>
        Effect.gen(function* () {
            const policy = new Policy(replaced);
            const refused = yield* policy.decide(shell('git revert HEAD'), environment());
            assert.deepStrictEqual([yield* ran(new Policy(), 'git stash'), yield* ran(policy, 'git stash')], [['Deny'], ['Allow']]);
            assert.deepStrictEqual(refused, { _tag: 'Deny', reasons: ['git revert is replaced. Read the new row.'], notes: [], faults: [] });
        }),
    );
    suite.effect('a decision cut off inside a rule answers nothing and runs no later rule', () =>
        Effect.gen(function* () {
            const later = MutableRef.make(false);
            const answered = yield* new Policy(held(later)).decide(shell('true'), environment()).pipe(Effect.timeoutOrElse({ duration: '50 millis', orElse: () => Effect.succeed('cut' as const) }));
            yield* Effect.sleep('50 millis');
            assert.deepStrictEqual([answered, MutableRef.get(later)], ['cut', false]);
        }),
    );
    suite.effect('a row over another kind of act restates it', () =>
        Effect.gen(function* () {
            const policy = new Policy(kinds);
            const [staying, refused] = [yield* policy.decide({ ...isolate, worker: Option.some('a1') }, environment()), yield* policy.decide({ ...isolate, worker: Option.none() }, environment())];
            const [watched, ended] = [yield* policy.decide({ ...task, resting: true }, environment()), yield* policy.decide({ ...task, resting: false }, environment())];
            assert.deepStrictEqual(staying, { _tag: 'Rewrite', action: { ...isolate, worker: Option.some('a1'), apart: false }, notes: [], faults: [] });
            assert.deepStrictEqual(refused, { _tag: 'Deny', reasons: ['A second checkout repeats every build. Work in place.'], notes: [], faults: [] });
            assert.deepStrictEqual(watched, { _tag: 'Rewrite', action: { ...task, resting: true, again: Duration.minutes(5) }, notes: [], faults: [] });
            assert.deepStrictEqual([ended._tag, policy.appliesTo('Isolate'), policy.appliesTo('Task'), policy.appliesTo('Read')], ['Allow', true, true, false]);
        }),
    );
});
