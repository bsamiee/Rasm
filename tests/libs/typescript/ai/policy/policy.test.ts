import { assert, it } from '@effect/vitest';
import { Policy } from '@rasm/policy';
import { Array, Duration, Effect, Match, Option } from 'effect';
import { judging, shell, where } from '#/fixtures';

// --- [CONSTANTS] -----------------------------------------------------------------------

const policy = new Policy({
    'stand.refuse': { program: 'rm', refuse: 'deletes files', instruct: 'Move them aside.' },
    'stand.patch': { program: 'zzpm', path: ['patch'], refuse: 'copies a package into the tree', instruct: 'Set the package option.' },
    'stand.rename': { program: 'grep', rename: { '-i': '--ignore-case', '-h': '-I', '--include': '-g' } },
    'stand.follow': { program: 'grep', judge: (invocation) => (invocation.words.some((word) => word.spells('--ignore-case')) && !invocation.has('-n') ? Option.toArray(invocation.head).map((word) => ({ edit: (editor) => [editor.after(word, ['-n'])] })) : []) },
    'stand.left': { program: 'cat', judge: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['bat'])] })) },
    'stand.right': { program: 'cat', judge: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['dog'])] })) },
    'stand.swap': { program: 'more', judge: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['less'])] })) },
    'stand.less': { program: 'less', refuse: 'pages its output', instruct: 'Print the file.' },
    'stand.ping': { program: 'ping', judge: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['pong'])] })) },
    'stand.pong': { program: 'pong', judge: ({ head }) => Option.toArray(head).map((word) => ({ edit: (editor) => [editor.replace(word, ['ping'])] })) },
    'stand.grow': { program: 'yes', judge: ({ words }) => Option.toArray(Option.flatMap(Array.last(words), ({ node }) => node)).map((word) => ({ edit: (editor) => [editor.after(word, ['y'])] })) },
    'stand.throws': {
        program: 'echo',
        judge: () => {
            throw new Error('boom');
        },
    },
    'stand.sleep': { program: 'sleep', judge: () => [{ wait: 'notified', note: 'runs in the background' }] },
    'stand.tail': {
        program: 'tail',
        judge: () => [
            { wait: 'streamed', note: 'runs as a watcher' },
            { note: 'heard once by each agent', once: 'agent' },
            { note: 'heard once in the session', once: 'session' },
        ],
    },
    'stand.then': { action: 'Shell', judge: (_, call) => (call.wait === 'notified' ? [{ note: 'sees the call run in the background', once: 'call' }] : []) },
    'respell.sed': { program: 'sed', drop: ['-E'] },
    'respell.sort': { program: 'sort', drop: ['-k'] },
    'respell.ls': { program: 'ls', rename: { '-a': '--all' } },
    'respell.log': { program: 'git', path: ['log'], ensure: ['--oneline'], unless: ['--format'] },
    'read.hosts': { action: 'Read', judge: ({ path }) => (path === '/etc/hosts' ? [{ refuse: `${path} is off limits.`, instruct: 'Read another file.' }] : []) },
    'work.apart': { action: 'Isolate', refuse: 'A second checkout repeats every build.', instruct: 'Work in place.' },
    'task.look': { action: 'Task', judge: ({ id, resting }) => (resting ? [{ again: Duration.minutes(5) }, { note: `task ${id} repeats a line`, once: 'agent' }] : []) },
});
const rows = [
    { name: 'a refusal beats a rewrite in the same script', asked: shell('grep -i x f; rm a'), ran: [], said: ['rm deletes files. Move them aside.'] },
    { name: 'a rewrite is judged again and a later rule edits what it wrote', asked: shell('grep -i x f'), ran: ['grep -n --ignore-case x f', 'held'], said: [] },
    { name: 'edits that overlap apply in table order', asked: shell('cat f'), ran: ['bat f', 'held'], said: [] },
    { name: 'a refusal after a rewrite names the rewrite first', asked: shell('more f'), ran: [], said: ['stand.swap rewrote the call to less f, which is refused:', 'less pages its output. Print the file.'] },
    { name: 'rules that undo each other run the call as written and are named', asked: shell('ping h'), ran: ['ping h', 'held'], said: ['stand.ping, stand.pong: rewrites undo each other'] },
    { name: 'a rule that never settles runs the call as written and is named', asked: shell('yes'), ran: ['yes', 'held'], said: ['stand.grow: rewrites never settle'] },
    { name: 'a throwing rule lets the call run with a note', asked: shell('echo hi'), ran: ['echo hi', 'held'], said: ['stand.throws: failed and judged nothing: boom'] },
    { name: 'a script no shell reads runs with a note', asked: shell('if then'), ran: ['if then', 'held'], said: ['policy did not judge this call and let it run:'] },
    { name: 'no rule speaks and the call passes', asked: shell('true'), ran: ['true', 'held'], said: [] },
    { name: 'a refusal names the subcommand of a program with no usage', asked: shell('zzpm patch left-pad'), ran: [], said: ['zzpm patch copies a package into the tree. Set the package option.'] },
    { name: 'a call moves to the wait a rule asks, told by that rule, and the next round sees it', asked: shell('sleep 5'), ran: ['sleep 5', 'notified'], said: ['stand.then: sees the call run in the background', 'stand.sleep: runs in the background'] },
    { name: 'the furthest wait wins and tells its own note alone', asked: shell('sleep 5; tail -f x', 'held', Option.some('furthest')), ran: ['sleep 5; tail -f x', 'streamed'], said: ['stand.tail: heard once by each agent', 'stand.tail: heard once in the session', 'stand.tail: runs as a watcher'] },
    { name: 'a call asked under the wait a rule states is not moved', asked: shell('sleep 5', 'notified'), ran: ['sleep 5', 'notified'], said: ['stand.then: sees the call run in the background'] },
    { name: 'a drop takes one letter out of a cluster', asked: shell('sed -nE p f'), ran: ['sed -n p f', 'held'], said: [] },
    { name: 'a drop takes the separate value word with its flag', asked: shell('sort -k 2 -n f'), ran: ['sort -n f', 'held'], said: [] },
    { name: 'a drop leaves a value spelled like the flag', asked: shell('sed -e -E f'), ran: ['sed -e -E f', 'held'], said: [] },
    { name: 'a rename to a short flag swaps the letter in a cluster', asked: shell('grep -ch x f'), ran: ['grep -cI x f', 'held'], said: [] },
    { name: 'a rename matches any spelling of the flag', asked: shell('grep --no-filename x f'), ran: ['grep -I x f', 'held'], said: [] },
    { name: 'a rename to a short flag moves an attached value to its own word', asked: shell("grep --include='*.ts' x ."), ran: ["grep -g '*.ts' x .", 'held'], said: [] },
    { name: 'a rename to a long flag spills out of a cluster', asked: shell('ls -la d'), ran: ['ls -l --all d', 'held'], said: [] },
    { name: 'an ensure lands after the subcommand', asked: shell('git log -p'), ran: ['git log --oneline -p', 'held'], said: [] },
    { name: 'an ensure yields to a flag it names as unless', asked: shell('git log --format=%H'), ran: ['git log --format=%H', 'held'], said: [] },
] as const;

// --- [OPERATIONS] ----------------------------------------------------------------------

const judged = (action: Parameters<Policy['judge']>[0]) =>
    Effect.map(policy.judge(action, where), (judgment) =>
        Match.valueTags(judgment, {
            Refuse: ({ reasons, notes }) => ({ ran: Array.empty<string>(), said: [...reasons, ...notes] }),
            Rewrite: ({ action: restated, notes }) => ({ ran: restated._tag === 'Shell' ? [restated.command, restated.wait] : [restated._tag], said: notes }),
            Pass: ({ notes }) => ({ ran: action._tag === 'Shell' ? [action.command, action.wait] : [action._tag], said: notes }),
        }),
    );

const heard = (agent: Option.Option<string>) => Effect.map(judged(shell('tail -f x', 'streamed', agent)), ({ said }) => said.map((line) => line.replace('stand.tail: heard once ', '')));
const begun = (lines: readonly string[], prefixes: readonly string[]) => lines.map((line) => prefixes.find((prefix) => line.startsWith(prefix)) ?? line);

// --- [COMPOSITION] ---------------------------------------------------------------------

it.layer(judging, { excludeTestServices: true })('policy', (suite) => {
    suite.effect.each(rows)('$name', ({ asked, ran, said }) =>
        Effect.gen(function* () {
            const found = yield* judged(asked);
            assert.deepStrictEqual(found.ran, [...ran]);
            assert.deepStrictEqual(begun(found.said, said), [...said]);
        }),
    );
    suite.effect('a note is told once to the memory its lifetime names, until that loop forgets', () =>
        Effect.gen(function* () {
            yield* policy.forget(Option.none());
            const [main, again, other] = [yield* heard(Option.none()), yield* heard(Option.none()), yield* heard(Option.some('other'))];
            yield* policy.forget(Option.none());
            const [forgotten, kept] = [yield* heard(Option.none()), yield* heard(Option.some('other'))];
            const [first, second, each] = [yield* policy.unjudged(Option.none(), 'why', 'agent'), yield* policy.unjudged(Option.none(), 'why', 'agent'), yield* policy.unjudged(Option.none(), 'why', 'call')];
            assert.deepStrictEqual([main, again, other, forgotten, kept], [['by each agent', 'in the session'], [], ['by each agent'], ['by each agent', 'in the session'], []]);
            assert.deepStrictEqual([first.notes.length, second.notes.length, each.notes.length], [1, 0, 1]);
        }),
    );
    suite.effect('actions beside shell calls are judged by the rows that name their kind', () =>
        Effect.gen(function* () {
            const task = { _tag: 'Task', call: 'call', agent: Option.none<string>(), id: 'b1', output: '/tmp/b1.output', again: Duration.infinity } as const;
            const found = yield* Effect.forEach(
                [
                    { _tag: 'Read', call: 'call', agent: Option.none<string>(), path: '/etc/hosts' },
                    { _tag: 'Read', call: 'call', agent: Option.none<string>(), path: '/etc/shells' },
                    { _tag: 'Isolate', call: 'call', agent: Option.none<string>(), from: '/repository' },
                    { ...task, resting: false },
                ] as const,
                judged,
            );
            const watched = yield* policy.judge({ ...task, resting: true }, where);
            assert.deepStrictEqual(found, [
                { ran: [], said: ['/etc/hosts is off limits. Read another file.'] },
                { ran: ['Read'], said: [] },
                { ran: [], said: ['A second checkout repeats every build. Work in place.'] },
                { ran: ['Task'], said: [] },
            ]);
            assert.deepStrictEqual(watched, { _tag: 'Rewrite', action: { ...task, resting: true, again: Duration.minutes(5) }, notes: ['task.look: task b1 repeats a line'] });
            assert.deepStrictEqual([policy.hears('Shell'), policy.hears('Read'), policy.hears('Write'), policy.hears('Task')], [true, true, false, true]);
        }),
    );
});
