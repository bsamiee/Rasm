import { Array, Duration, Effect, Function, Match, Option, Path, Struct } from 'effect';
import { type Stmt, walk } from 'sh-syntax';
import { type Editor, feeds, type Invocation, ran, type Table, type Written } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Loop = Option.Option.Value<Invocation['repeated']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const idle = ['echo', 'printf', ':', 'true'];
const constant = [':', 'true', 'false'];
const leaving = ['break', 'exit', 'return'];
const checks = ['sleep', 'wait', 'read', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'false', 'date', 'pgrep', 'ps', 'stat', 'ls', 'cat', 'tail', 'head', 'wc', 'curl', 'lsof', 'nc', 'wait4path', 'caffeinate', 'seq', 'continue', ...leaving];
const polls = ['sleep', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'date', 'read'];
const statuses = ['nx', 'uv', 'dotnet', 'xcodebuild', 'swift', 'npm', 'pnpm', 'cargo', 'curl', 'pytest', 'vitest'];
const loopback: ReadonlySet<string> = new Set(['127.0.0.1', 'localhost', '::1']);
const endless: ReadonlySet<string> = new Set(['/dev/zero', '/dev/urandom', '/dev/random']);
const instant = /^(?:0+(?:\.0*)?|\.0+)[smhd]?$/u;
const output = /\/tasks\/(?<id>[^/\s]+)\.output$/u;
const agent = /\/tasks\/a[0-9a-f]{16}\.output$/u;
const records = [
    [agent, "is a running agent's output that the agent reports when it ends", 'SendMessage asks it for status.'],
    [/\/\.(?:zsh|bash)_history$|\/\.zsh_sessions(?:\/|$)/u, 'is shell history', "atuin search --filter-mode global --search-mode fulltext '<words>' reads it."],
] as const;
const unattended = { wait: 'notified', note: 'This call runs in the background and notifies you when it ends, so never poll it.' } as const;
const monitored = { wait: 'streamed', note: 'This wait ran as a Monitor: each line it prints and its exit reach you as notices and its timeout bounds it, so keep working or end the turn.' } as const;
const waiting = 'Background tasks and agents notify when they end, Monitor `until <check>; do sleep 5; done` waits on a condition with timeout_ms as its bound, and a turn with nothing to do ends until a notice arrives.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const settles = (invocation: Invocation) => invocation.runs('sleep') && Array.isReadonlyArrayNonEmpty(invocation.operands) && invocation.operands.every(({ word }) => word.test(instant));
const sleeps = (invocation: Invocation) => invocation.runs('sleep') && !settles(invocation);
const timer = (invocation: Invocation) => sleeps(invocation) || (invocation.runs('tail') && invocation.has('-f', '-F', '--follow') && !invocation.has('--pid') && Array.isReadonlyArrayNonEmpty(invocation.reads) && invocation.reads.every(({ text }) => Option.contains(text, '/dev/null')));
const ends = (invocation: Invocation) => sleeps(invocation) || invocation.runs('wait4path') || (invocation.runs('caffeinate') && invocation.has('-t', '-w')) || (invocation.runs('tail') && invocation.has('--pid'));
const pauses = (invocation: Invocation) => sleeps(invocation) || invocation.runs('wait') || (invocation.runs('read') && invocation.has('-t'));
const closes = (invocation: Invocation) => invocation.runs('head') || (invocation.runs('grep', 'rg') && invocation.has('-q', '--quiet', '-m', '--max-count', '-l', '--files-with-matches'));
const reaped = (invocations: readonly Invocation[], job: Invocation) => invocations.some((other) => other.start >= job.end && (other.runs('wait') || (other.runs('kill') && !other.has('-0'))));
const task = (file: string) => output.exec(file)?.groups?.['id'] ?? file;
const silent = () => ({ printed: '', code: 1 });
const streamed = (file: string) => `Monitor \`tail -n +1 -F ${file} | grep --line-buffered -E '<pattern>'\` streams the lines you wait for.`;
const writers = Effect.fnUntraced(function* (file: string, cwd: string) {
    const [held, table] = yield* Effect.all([Effect.orElseSucceed(ran(['lsof', '-F', 'pa', '--', file], cwd), silent), Effect.orElseSucceed(ran(['ps', '-A', '-o', 'pid=,ppid='], cwd), silent)], { concurrency: 'unbounded' });
    const parents = new Map(table.printed.split('\n').flatMap((line) => Option.toArray(Option.map(Option.fromNullishOr(/^\s*(?<pid>\d+)\s+(?<parent>\d+)/u.exec(line)?.groups), ({ pid = '', parent = '' }) => [pid, parent] as const))));
    const holders = new Set(held.printed.split(/^p/mu).flatMap((listing) => (/^a[uw]$/mu.test(listing) ? [listing.split('\n', 1).join('')] : [])));
    return [...holders].filter((pid) => parents.has(pid) && !holders.has(parents.get(pid) ?? ''));
});
const stuck = Effect.fnUntraced(function* (file: string, cwd: string) {
    const { printed } = yield* Effect.orElseSucceed(ran(['tail', '-n', '10', '--', file], cwd), silent);
    const lines = printed
        .split('\n')
        .filter((line) => line.trim() !== '')
        .map((line) => line.replaceAll(/\d+/gu, '#'));
    return Option.map(
        Option.fromUndefinedOr(
            Map.groupBy(lines, Function.identity)
                .entries()
                .find(([, same]) => same.length >= 5),
        ),
        ([line]) => line,
    );
});
const loops = (invocations: readonly Invocation[]) =>
    Array.dedupeWith(
        invocations.flatMap(({ repeated }) => Option.toArray(repeated)),
        (left, right) => left.source === right.source && left.start === right.start,
    ).map((loop: Loop) => {
        const inside = invocations.filter(({ source, start, end }) => source === loop.source && start >= loop.start && end <= loop.end);
        const tested = inside.filter(({ repeated }) => Option.exists(repeated, ({ start, condition }) => start === loop.start && condition));
        return { loop, inside, tested, body: inside.filter((invocation) => !tested.includes(invocation)), until: loop.source.slice(loop.start).startsWith('until') };
    });
const spins = ({ tested, until }: ReturnType<typeof loops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && tested.every((invocation) => invocation.runs(...constant) && invocation.runs('false') === until);
const probes = ({ tested }: ReturnType<typeof loops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && !tested.every((invocation) => invocation.runs(...constant));
const marked = (written: Written) => Option.map(Option.all([written.node, Option.filter(written.text, (text) => /^\w/u.test(text))]), ([node, text]) => ({ edit: (editor: Editor) => [editor.replace(node, Array.of(`[${text.slice(0, 1)}]${text.slice(1)}`))] }));
const read = (invocation: Invocation) => [...invocation.reads.flatMap(({ text }) => Option.toArray(text)), ...invocation.words.flatMap((written) => Option.toArray(Option.filter(written.text, (text) => text.startsWith('/'))))];
const after = (invocation: Invocation, flag: string) => Option.filter(Option.flatMap(Array.head(invocation.values(flag)), Struct.get('text')), (text) => Number.isFinite(Number(text)));
const slept = (invocation: Invocation, seconds: string) => Option.toArray(invocation.span).map((whole) => ({ edit: (editor: Editor) => [editor.replace(whole, Array.make('sleep', seconds))] }));
const watching = (invocation: Invocation) =>
    Option.map(
        Option.filter(
            Array.findFirst(invocation.operands, ({ word }) => Option.filter(word.text, (text) => /^\d+$/u.test(text))),
            () => invocation.runs('gh') && invocation.path[0] === 'run' && invocation.path[1] === 'view' && !invocation.has('--log', '--log-failed', '--job', '--web', '--attempt'),
        ),
        (run) => [run, '--exit-status', ...invocation.values('--repo', '-R').flatMap((value) => Option.toArray(value.text).flatMap((repo) => ['--repo', repo]))],
    );

const waits: Table = {
    'timer.bare': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const run = (yield* script.invocations).flatMap(Struct.get('leaves')).filter(({ inner }) => inner.length === 0);
            return run.some(timer) && run.every((invocation) => timer(invocation) || invocation.runs(...idle)) ? [{ refuse: 'A bare timer waits on nothing and wakes you with nothing new.', instruct: waiting }] : [];
        }),
    },
    'timer.trailing': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const last = Array.last((yield* script.invocations).filter(({ within }) => within.length === 0));
            return Option.exists(last, (invocation) => Option.isNone(invocation.repeated) && invocation.leaves.some(ends)) ? [unattended] : [];
        }),
    },
    'sleep.instant': {
        program: 'sleep',
        judge: Effect.fnUntraced(function* (invocation, call) {
            const stmt = Option.filter(invocation.node, () => settles(invocation) && invocation.source === call.script.source && invocation.within.length === 0 && Option.isNone(invocation.repeated) && Option.isNone(invocation.stage));
            const sole = yield* Effect.transposeOption(
                Option.map(
                    Option.filter(stmt, (found) => call.script.syntax.stmts.length === 1 && call.script.syntax.stmts[0] === found),
                    () => call.shell.script(call.script.dialect)`:`,
                ),
            );
            return Option.toArray(stmt).map((found) => ({ edit: (editor: Editor) => [Option.match(sole, { onNone: () => editor.remove(found), onSome: (held) => editor.command(found, held) })] }));
        }),
    },
    'call.idle': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const idled = Array.isReadonlyArrayNonEmpty(invocations) && invocations.every((invocation) => invocation.runs('echo', 'printf', 'true') && invocation.writes.length === 0 && Option.isSome(Option.all(invocation.words.map(Struct.get('text')))));
            return idled ? [{ refuse: 'Call prints literal text and runs nothing.', instruct: 'Background tasks and agents notify when they end, so end the turn; a foreground subagent, whose background work ends with its final answer, keeps working instead.' }] : [];
        }),
    },
    'job.detached': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script, call) {
            const invocations = yield* script.invocations;
            const jobs = invocations.filter(({ background }) => background);
            const detached: Stmt[] = [];
            walk(script.syntax, (node) => {
                detached.push(...(node.type === 'Stmt' && node.background ? [node] : []));
            });
            const trailing = Array.dropWhile(script.syntax.stmts, ({ background }) => !background);
            const tail = Option.filter(Array.last(trailing), () => trailing.length === detached.length && trailing.every(({ background }) => background));
            const reap = yield* Effect.transposeOption(Option.map(tail, () => call.shell.script(script.dialect)`wait`));
            return Array.match(
                jobs.filter((job) => !reaped(invocations, job)),
                {
                    onEmpty: () => [],
                    onNonEmpty: ([job]) =>
                        Option.match(Option.all([tail, reap]), {
                            onNone: () => [{ refuse: `${job.label} & outlives the call.`, instruct: 'Set run_in_background on the call, or wait for the job in the same script.' }],
                            onSome: ([stmt, wait]) => [{ edit: (editor: Editor) => [editor.follow(stmt, ';', wait)] }, unattended],
                        }),
                },
            );
        }),
    },
    'loop.endless': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const spinning = loops((yield* script.invocations).flatMap(Struct.get('leaves'))).filter(
                (found) => spins(found) && found.body.some((invocation) => !invocation.runs(...constant, ...idle) && !ends(invocation)) && !found.inside.some((invocation) => invocation.runs(...leaving) || Option.isSome(watching(invocation))),
            );
            return spinning.map(({ tested }) => ({ refuse: `Loop on ${tested.map(Struct.get('label')).join(' ')} never ends, as nothing changes its condition and no break or exit leaves it.`, instruct: 'Wait on a real condition with Monitor `until <check>; do sleep <seconds>; done`.' }));
        }),
    },
    'loop.busy': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const busy = loops((yield* script.invocations).flatMap(Struct.get('leaves'))).filter((found) => probes(found) && found.body.every((invocation) => invocation.runs(...idle)) && !found.inside.some(pauses));
            return busy.map(({ tested }) => ({ refuse: `Loop on ${tested.map(Struct.get('label')).join(' ')} checks again at once and spins a core.`, instruct: 'Put sleep <seconds> in its body, or wait with Monitor `until <check>; do sleep <seconds>; done`.' }));
        }),
    },
    'loop.waiting': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const waited = loops(invocations.flatMap(Struct.get('leaves'))).filter((found) => (probes(found) || found.inside.some((invocation) => invocation.runs(...leaving))) && found.inside.some(pauses));
            const inside = (invocation: Invocation) => waited.some(({ loop }) => invocation.start >= loop.start && invocation.end <= loop.end);
            const trailing = (invocation: Invocation) => waited.some(({ loop }) => invocation.start >= loop.end) && invocation.runs('cat', 'tail', 'head', 'echo', 'printf', 'rg', 'jq', 'ls');
            const placed = invocations.filter(({ within }) => within.length === 0).every((invocation) => inside(invocation) || trailing(invocation));
            const checking = waited.every((found) => found.inside.every((invocation) => invocation.runs(...checks) || (invocation.runs('kill') && invocation.has('-0'))));
            return Array.isReadonlyArrayNonEmpty(waited) ? [placed && checking ? monitored : unattended] : [];
        }),
    },
    'watch.repaints': { program: 'watch', judge: (invocation, call) => (call.action.wait === 'streamed' ? [] : [{ refuse: `${invocation.label} repaints a screen.`, instruct: 'Monitor `until <check>; do sleep <seconds>; done` waits on the condition and its exit notifies you.' }]) },
    'caffeinate.unbounded': { program: 'caffeinate', judge: (invocation) => (invocation.has('-t', '-w') || invocation.inner.length > 0 ? [] : [{ refuse: 'caffeinate holds the call open until it is killed.', instruct: 'Bound it with -t <seconds> or -w <pid>.' }]) },
    'caffeinate.wrapper': {
        program: 'caffeinate',
        judge: (invocation) => {
            const first = Option.flatMap(Array.head(invocation.inner), ({ words }) => Array.head(words));
            const last = Option.flatMap(first, (word) => Option.flatMap(Array.get(invocation.words, invocation.words.indexOf(word) - 1), ({ node }) => node));
            return Option.toArray(Option.all([Option.flatMap(Array.head(invocation.words), ({ node }) => node), last])).map((words) => ({ edit: (editor: Editor) => [editor.remove(words)] }));
        },
    },
    'launchctl.submit': { program: 'launchctl', path: ['submit'], refuse: 'starts a detached job that outlives the session', instruct: 'Use run_in_background and TaskStop.' },
    'gh.run.polled': {
        program: 'gh',
        path: ['run', 'view'],
        judge: Effect.fnUntraced(function* (invocation, call) {
            const watch = Option.filter(watching(invocation), () => Option.isSome(invocation.repeated));
            const polled = (yield* call.script.invocations).flatMap(Struct.get('leaves')).every((other) => Option.isSome(other.repeated) && other.writes.every(({ text }) => Option.contains(text, '/dev/null')) && (other.runs(...polls) || Option.isSome(watching(other))));
            const sole = Option.filter(Array.head(call.script.syntax.stmts), () => polled && call.script.syntax.stmts.length === 1);
            const written = yield* Effect.transposeOption(Option.map(Option.all([sole, watch]), ([, argv]) => call.shell.script(call.script.dialect)`gh run watch ${argv}`));
            return Option.match(watch, {
                onNone: () => [],
                onSome: (argv) =>
                    Option.match(Option.all([sole, written]), {
                        onNone: () => [{ refuse: 'gh run view in a loop polls a run.', instruct: `Run gh run watch ${argv.join(' ')}, which waits and exits with the run's result.` }],
                        onSome: ([stmt, watched]) => [{ edit: (editor: Editor) => [editor.command(stmt, watched)] }, unattended],
                    }),
            });
        }),
    },
    'gh.run.slept': {
        program: 'gh',
        path: ['run', 'view'],
        judge: Effect.fnUntraced(function* (invocation, call) {
            const before = (yield* call.script.invocations).filter((other) => other.end <= invocation.start && other.within.length === 0);
            const sleep = Option.filter(Array.last(before), (other) => other.runs('sleep') && Option.isNone(other.repeated));
            const plain = Option.isNone(invocation.repeated) && invocation.within.length === 0 && !invocation.has('--json') && call.action.wait !== 'streamed';
            return Option.match(
                Option.filter(Option.all([Option.flatMap(sleep, ({ node }) => node), watching(invocation), invocation.span]), () => plain),
                {
                    onNone: () => [],
                    onSome: ([stmt, argv, whole]) => [{ edit: (editor: Editor) => [editor.remove(stmt), editor.replace(whole, Array.make('gh', 'run', 'watch', ...argv))] }, unattended],
                },
            );
        }),
    },
    'record.read': { action: 'Read', judge: ({ path }) => records.filter(([pattern]) => pattern.test(path)).map(([, fact, route]) => ({ refuse: `${path} ${fact}.`, instruct: route })) },
    'record.shell': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script) {
            const files = Array.dedupe((yield* script.invocations).flatMap(Struct.get('leaves')).flatMap(read));
            return records.flatMap(([pattern, fact, route]) => files.filter((file) => pattern.test(file)).map((file) => ({ refuse: `${file} ${fact}.`, instruct: route })));
        }),
    },
    'task.output': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script, call) {
            const all = (yield* script.invocations).flatMap(Struct.get('leaves'));
            const files = Array.dedupe(all.flatMap(read)).filter((file) => output.test(file) && !agent.test(file));
            const watched = call.action.wait === 'streamed' || all.some((invocation) => Option.isSome(invocation.repeated) || (invocation.runs('tail') && invocation.has('-f', '-F', '--follow')));
            const polled = !watched && all.some(pauses);
            const live = watched || polled ? [] : yield* Effect.filter(files, (file) => Effect.map(writers(file, call.cwd), Array.isReadonlyArrayNonEmpty));
            const repeating = yield* Effect.forEach(live, (file) => stuck(file, call.cwd));
            const notes = Array.zipWith(live, repeating, (file, repeats) => ({
                note: Option.match(repeats, {
                    onNone: () => `Task ${task(file)} still runs and notifies when it ends; ${streamed(file)}`,
                    onSome: (line) => `Task ${task(file)} repeats "${line}" and looks stuck: TaskStop ${task(file)}, fix its cause, and rerun it bounded by a count or timeout.`,
                }),
                once: 'call' as const,
            }));
            return polled ? files.map((file) => ({ refuse: `Call polls the output of task ${task(file)}, which notifies when it ends.`, instruct: `End the turn until its notice arrives, or run ${streamed(file)}` })) : notes;
        }),
    },
    'task.stuck': {
        action: 'Task',
        judge: Effect.fnUntraced(function* ({ id, output: file, resting }) {
            const folder = (yield* Path.Path).dirname(file);
            const live = Array.isReadonlyArrayNonEmpty(yield* writers(file, folder));
            const repeats = live && resting ? yield* stuck(file, folder) : Option.none();
            const told = Option.toArray(repeats).map((line) => ({
                note: `Background task ${id} repeats "${line}", so it looks stuck and its completion notice may never come (${file}): read its last lines, TaskStop ${id} if it hangs or loops, fix the cause, and rerun it bounded by a count or timeout.`,
                once: 'agent' as const,
            }));
            return live ? [{ again: Duration.minutes(5) }, ...told] : [];
        }),
    },
    'tail.follow': {
        program: 'tail',
        judge: Effect.fnUntraced(function* (invocation, call) {
            const files = invocation.reads.flatMap(({ text }) => Option.toArray(text));
            const follows = invocation.has('-f', '-F', '--follow') && !invocation.has('--pid') && !files.every((file) => file === '/dev/null') && !(invocation.background && reaped(yield* call.script.invocations, invocation));
            const tasked = follows && Array.isReadonlyArrayNonEmpty(files) && files.every((file) => output.test(file));
            const held = tasked ? (yield* Effect.forEach(files, (file) => writers(file, call.cwd))).flat() : [];
            const waiter = yield* Effect.transposeOption(Option.liftPredicate(held, Array.isReadonlyArrayNonEmpty).pipe(Option.map((pids) => call.shell.script(call.script.dialect)`for pid in ${pids}; do while kill -0 "$pid" 2>/dev/null; do sleep 1; done; done; kill $!`)));
            return Match.value({ follows, tasked, live: Array.isReadonlyArrayNonEmpty(held) }).pipe(
                Match.when({ follows: false }, () => []),
                Match.when({ tasked: true, live: false }, () => [{ drop: ['-f', '-F', '--follow'] }]),
                Match.when({ tasked: true }, () => [...Option.toArray(Option.all([invocation.node, waiter])).map(([stmt, wait]) => ({ edit: (editor: Editor) => [editor.follow(stmt, '&', wait)] })), monitored]),
                Match.orElse(() => [monitored]),
            );
        }),
    },
    'read.timed': {
        program: 'read',
        judge: (invocation) => {
            const sources = invocation.reads.flatMap(({ text }) => Option.toArray(text));
            const bounded = invocation.has('-n', '-N') || invocation.values('-d').some((value) => Option.contains(value.text, ''));
            const filed = invocation.stdin._tag === 'File' && !(sources.length === invocation.reads.length && sources.every((file) => endless.has(file)) && !bounded) && !(Array.isReadonlyArrayNonEmpty(invocation.inner) && invocation.inner.every((inner) => ends(inner) || inner.runs(...idle)));
            const unfed = !invocation.has('-u') && (invocation.stdin._tag === 'Inherited' || (invocation.stdin._tag === 'File' && !filed)) && Option.isNone(invocation.repeated);
            return Option.match(
                Option.filter(Option.flatMap(Array.head(invocation.values('-t')), Struct.get('text')), () => unfed),
                { onNone: () => [], onSome: (seconds) => slept(invocation, seconds) },
            );
        },
    },
    'read.endless': {
        program: 'read',
        judge: (invocation) => {
            const sources = invocation.reads.flatMap(({ text }) => Option.toArray(text));
            const bounded = invocation.has('-n', '-N', '-t') || invocation.values('-d').some((value) => Option.contains(value.text, ''));
            return sources.some((file) => endless.has(file)) && !bounded ? [{ refuse: `read < ${sources.join(' ')} never returns and spins a core.`, instruct: 'Run sleep <seconds> to wait.' }] : [];
        },
    },
    'pgrep.self': { program: 'pgrep', judge: (invocation) => (invocation.has('-f', '--full') ? Option.toArray(Option.flatMap(Array.head(invocation.operands), ({ word }) => marked(word))) : []) },
    'ps.self': {
        program: 'grep',
        aliases: ['rg'],
        judge: Effect.fnUntraced(function* (grep, { script }) {
            const ps = (yield* script.invocations).flatMap(Struct.get('leaves')).some((found) => found.runs('ps') && feeds(found, grep));
            return ps && grep.flags.length === 0 && grep.operands.length === 1 ? Option.toArray(Option.flatMap(Array.head(grep.operands), ({ word }) => marked(word))) : [];
        }),
    },
    'pipe.status': {
        action: 'Shell',
        judge: Effect.fnUntraced(function* (script, call) {
            const invocations = yield* script.invocations;
            const set = script.dialect.options.pipefail || invocations.some((invocation) => invocation.runs('set') && invocation.words.some((written) => Option.contains(written.text, 'pipefail')));
            const hidden = invocations.some(
                (invocation) => invocation.source === script.source && Option.exists(invocation.stage, ({ index, count }) => index < count - 1) && invocation.leaves.some((leaf) => leaf.runs(...statuses)) && !invocations.flatMap(Struct.get('leaves')).some((next) => feeds(invocation, next) && closes(next)),
            );
            const first = Option.filter(Array.head(script.syntax.stmts), () => hidden && !set);
            const pipefail = yield* Effect.transposeOption(Option.map(first, () => call.shell.script(script.dialect)`set -o pipefail`));
            return Option.toArray(Option.all([first, pipefail])).map(([stmt, prefix]) => ({ edit: (editor: Editor) => [editor.precede(prefix, ';', stmt)] }));
        }),
    },
    'zsh.special': {
        action: 'Shell',
        judge: (script) => {
            const names: string[] = [];
            walk(script.syntax, (node) => {
                names.push(...(node.type === 'Assign' && node.name !== null ? [node.name.value] : []), ...(node.type === 'WordIter' && node.name !== null ? [node.name.value] : []));
            });
            return Array.dedupe(names).flatMap((name) =>
                Option.toArray(Option.filter(Option.fromUndefinedOr(script.dialect.parameters.get(name)), (kind) => /^\p{Ll}/u.test(name) && kind.has('special') && ['tied', 'readonly', 'association'].some((word) => kind.has(word)))).map((kind) => ({
                    refuse: `zsh parameter ${name} is ${[...kind].join('-')}, so assigning it changes the shell itself.`,
                    instruct: 'Rename the variable.',
                })),
            );
        },
    },
    'ffmpeg.timer': {
        program: 'ffmpeg',
        judge: (invocation) => {
            const words = Option.getOrElse(Option.all(invocation.words.map(Struct.get('text'))), (): readonly string[] => []);
            const paced = words.includes('-re') || words.includes('-readrate');
            const silenced = /^a?nullsrc\b/u.test(Option.getOrUndefined(Array.get(words, words.indexOf('-i') + 1)) ?? '') && words.includes('-i');
            const discarded = words.at(-1) === '/dev/null' || (words.includes('null') && words.at(-1) === '-');
            return Option.match(
                Option.filter(after(invocation, '-t'), () => paced && silenced && discarded),
                { onNone: () => [], onSome: (seconds) => slept(invocation, seconds) },
            );
        },
    },
    'ping.timer': {
        program: 'ping',
        judge: (invocation) => {
            const seconds = (Number(Option.getOrUndefined(after(invocation, '-c')) ?? 'NaN') - 1) * Number(Option.getOrUndefined(after(invocation, '-i')) ?? '1');
            const local = Option.exists(invocation.operand(), (host) => loopback.has(host)) && Option.isNone(invocation.stage) && !invocation.within.some(({ _tag }) => _tag === 'Substitution');
            return local && seconds > 0 ? slept(invocation, `${seconds}`) : [];
        },
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { unattended, waits };
