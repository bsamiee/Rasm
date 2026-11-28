import { assert, it } from '@effect/vitest';
import { Outcome } from '@rasm/policy/action';
import { Policy } from '@rasm/policy/policy';
import type { Table } from '@rasm/policy/rule';
import { environment, folder, printed, services, shell } from '@rasm/testing/policy';
import { Array, Cause, Deferred, Duration, Effect, Exit, Fiber, MutableRef, Option } from 'effect';

// --- [CONSTANTS] -----------------------------------------------------------------------

const files = { lines: 'pattern\nPATTERN\nother\n', needle: 'PATTERN\n' };
const caller = { call: 'call', agent: Option.none<string>() };
const task = { ...caller, _tag: 'Task', id: 'b1', output: '/tmp/b1.output', again: Duration.infinity } as const;
const respelled = [
    { name: 'a short flag renamed long inside a cluster keeps the value of the flag beside it', row: { program: 'grep', rename: { '-i': '--ignore-case' } }, asked: 'grep -ie PATTERN lines' },
    { name: 'a renamed flag keeps a value no reading of the script knows', row: { program: 'grep', rename: { '--regexp': '-e' } }, asked: 'grep --regexp="$(cat needle)" lines' },
] as const;
const written = [
    { name: 'a drop takes a flag whose value no reading of the script knows', row: { program: 'grep', drop: ['--exclude-dir'] }, asked: 'grep --exclude-dir="$(cat skip)" -r x .', ran: 'grep -r x .' },
    { name: 'an ensure lands after the subcommand, past a flag value that spells it', row: { program: 'git', path: ['log'], ensure: ['--oneline'] }, asked: 'git -C log log -p', ran: 'git -C log log --oneline -p' },
    { name: 'an ensure lands once on a statement each pass of a loop reaches', row: { program: 'grep', ensure: ['-i'] }, asked: 'for f in a b; do grep x "$f"; done', ran: 'for f in a b; do grep -i x "$f"; done' },
] as const;
const paths: Table = {
    patch: { program: 'zzpm', path: ['patch'], deny: 'copies a package into the tree', values: { instead: 'set the package option' } },
    sub: { program: 'sort', path: ['sub'], deny: 'names a file', values: { instead: 'sort another' } },
};
const rewrites: Table = {
    once: { program: 'tr', decide: ({ operands }) => Array.take(Array.getSomes(operands.filter(({ word }) => word.spells('a')).map(({ word }) => word.node)), 1).map((word) => ({ edit: (editor) => [editor.replace(word, ['b'])] })) },
    ping: { program: 'ping', decide: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['pong'])] })) },
    pong: { program: 'pong', decide: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['ping'])] })) },
    behind: {
        program: 'pong',
        decide: () => [
            { run: 'background', subject: 'pong', note: 'runs behind the turn' },
            { subject: 'pong', note: 'saw pong', once: 'call' },
        ],
    },
};
const notes: Table = {
    tail: {
        program: 'tail',
        decide: () => [
            { subject: 'tail', note: 'each agent', once: 'agent' },
            { subject: 'tail', note: 'the session', once: 'session' },
            { subject: 'tail', note: 'same', once: 'agent' },
            { subject: 'tail', note: 'same', once: 'session' },
        ],
    },
    first: { program: 'sleep', decide: () => [{ run: 'background', subject: 'sleep', note: 'first' }] },
    second: { program: 'sleep', decide: () => [{ run: 'background', subject: 'sleep', note: 'second' }] },
};
const stated: Table = { 'git.stash': { program: 'git', path: ['stash'], deny: 'hides work' }, 'git.revert': { program: 'git', path: ['revert'], deny: 'undoes work' } };
const replaced: Table = { 'git.stash': null, 'git.revert': { program: 'git', path: ['revert'], deny: 'is replaced', values: { instead: 'read the new row' } } };
const kinds: Table = {
    look: { action: 'Task', decide: ({ resting }) => [{ again: resting ? Duration.minutes(5) : Duration.zero }] },
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const held = (entered: Deferred.Deferred<void>, later: MutableRef.MutableRef<boolean>): Table => ({
    held: { program: 'true', decide: () => Effect.as(Effect.andThen(Deferred.succeed(entered, undefined), Effect.never), []) },
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
const heard = (policy: Policy, agent: Option.Option<string>) => Effect.map(policy.decide(shell('tail f', agent), environment()), (decision) => decision.notes.map(({ reason }) => reason));

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
            assert.deepStrictEqual(moved.notes, [Outcome.of(['first'], { subject: 'sleep', reason: 'first' }), Outcome.of(['second'], { subject: 'sleep', reason: 'second' })]);
        }),
    );
    suite.effect('a later table replaces a row by name and removes one with null', () =>
        Effect.gen(function* () {
            const policy = new Policy(stated, replaced);
            const refused = yield* policy.decide(shell('git revert HEAD'), environment());
            assert.deepStrictEqual([yield* ran(new Policy(stated), 'git stash'), yield* ran(policy, 'git stash')], [['Deny'], ['Allow']]);
            assert.deepStrictEqual(refused, { _tag: 'Deny', reasons: [Outcome.of(['git.revert'], { subject: 'git revert', reason: 'is replaced', values: { instead: 'read the new row' } })], notes: [], faults: [] });
        }),
    );
    suite.effect('a decision interrupted inside a rule ends interrupted and runs no later rule', () =>
        Effect.gen(function* () {
            const [entered, later] = [yield* Deferred.make<void>(), MutableRef.make(false)];
            const deciding = yield* Effect.forkChild(new Policy(held(entered, later)).decide(shell('true'), environment()));
            yield* Deferred.await(entered).pipe(Effect.timeoutOrElse({ duration: '1 minute', orElse: () => Effect.fail(new Cause.UnknownError(entered, 'the decision never reached rule held')) }));
            yield* Fiber.interrupt(deciding);
            assert.deepStrictEqual([Exit.hasInterrupts(yield* Fiber.await(deciding)), MutableRef.get(later)], [true, false]);
        }),
    );
    suite.effect('a row over another kind of act restates it', () =>
        Effect.gen(function* () {
            const policy = new Policy(kinds);
            const [watched, ended] = [yield* policy.decide({ ...task, resting: true }, environment()), yield* policy.decide({ ...task, resting: false }, environment())];
            assert.deepStrictEqual(watched, { _tag: 'Rewrite', action: { ...task, resting: true, again: Duration.minutes(5) }, rules: ['look'], notes: [], faults: [] });
            assert.deepStrictEqual([ended._tag, policy.appliesTo('Task'), policy.appliesTo('Read')], ['Allow', true, false]);
        }),
    );
});
