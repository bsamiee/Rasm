import { Array, Duration, Effect, Function, Match, Option, Path, Result } from 'effect';
import { Host, NetAddress } from 'effect/net';
import { execute, type Invocation, rewrite, type Table, type Verdict } from '#/rules/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const inert = ['echo', 'printf', ':', 'true'];
const fixed = [':', 'true', 'false'];
const exits = ['break', 'exit', 'return'];
const checkers = ['sleep', 'wait', 'read', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'false', 'date', 'pgrep', 'ps', 'stat', 'ls', 'cat', 'tail', 'head', 'wc', 'curl', 'lsof', 'nc', 'wait4path', 'caffeinate', 'seq', 'continue', ...exits];
const pollers = ['sleep', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'date', 'read'];
const printers = ['cat', 'tail', 'head', 'echo', 'printf', 'rg', 'jq', 'ls'];
const following = ['-f', '-F', '--follow'];
const endless: ReadonlySet<string> = new Set(['/dev/zero', '/dev/urandom', '/dev/random']);
const zeroDuration = /^(?:0+(?:\.0*)?|\.0+)[smhd]?$/u;
const taskOutput = /\/tasks\/(?<id>[^/\s]+)\.output$/u;
const agentOutput = /\/tasks\/a[0-9a-f]{16}\.output$/u;
const records = [
    [agentOutput, "is a running agent's output that the agent reports when it ends", 'SendMessage asks it for status.'],
    [/\/\.(?:zsh|bash)_history$|\/\.zsh_sessions(?:\/|$)/u, 'is shell history', "atuin search --filter-mode global --search-mode fulltext '<words>' reads it."],
] as const;
const moves = {
    background: { run: 'background', note: 'This call runs in the background and notifies you when it ends, so never poll it.' },
    monitored: { run: 'monitored', note: 'This wait ran as a Monitor: each line it prints and its exit reach you as notices and its timeout bounds it, so keep working or end the turn.' },
} as const;
const waitAdvice = 'Background tasks and agents notify when they end, Monitor `until <check>; do sleep 5; done` waits on a condition with timeout_ms as its bound, and a turn with nothing to do ends until a notice arrives.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const sleepsZero = (invocation: Invocation) => invocation.runs('sleep') && Array.isReadonlyArrayNonEmpty(invocation.operands) && invocation.operands.every(({ word }) => word.test(zeroDuration));
const sleeps = (invocation: Invocation) => invocation.runs('sleep') && !sleepsZero(invocation);
const idles = (invocation: Invocation) => sleeps(invocation) || (invocation.runs('tail') && invocation.has(...following) && !invocation.has('--pid') && Array.isReadonlyArrayNonEmpty(invocation.reads) && invocation.reads.every(({ text }) => Option.contains(text, '/dev/null')));
const endsAlone = (invocation: Invocation) => sleeps(invocation) || invocation.runs('wait4path') || (invocation.runs('caffeinate') && invocation.has('-t', '-w')) || (invocation.runs('tail') && invocation.has('--pid'));
const pauses = (invocation: Invocation) => sleeps(invocation) || invocation.runs('wait') || (invocation.runs('read') && invocation.has('-t'));
const isReaped = (job: Invocation, invocations: readonly Invocation[]) => invocations.some((other) => other.start >= job.end && (other.runs('wait') || (other.runs('kill') && !other.has('-0'))));
const isDelimited = (read: Invocation) => read.has('-n', '-N') || read.values('-d').some((value) => value.spells(''));
const nameTask = (file: string) => taskOutput.exec(file)?.groups?.['id'] ?? file;
const adviseStream = (file: string) => `Monitor \`tail -n +1 -F ${file} | grep --line-buffered -E '<pattern>'\` streams the lines you wait for.`;
const listPaths = (invocation: Invocation) => [...Array.getSomes(invocation.reads.map(({ text }) => text)), ...Array.getSomes(invocation.words.map(({ text }) => Option.filter(text, (word) => word.startsWith('/'))))];
const readNumber = (invocation: Invocation, flag: string) =>
    Option.filter(
        Option.flatMap(Array.head(invocation.values(flag)), ({ text }) => text),
        (text) => Number.isFinite(Number(text)),
    );
const refuseRecords = (paths: readonly string[]) => records.flatMap(([pattern, fact, advice]) => paths.filter((path) => pattern.test(path)).map((path) => ({ deny: `${path} ${fact}.`, advice })));
const bracket = ({ node, text }: Invocation['words'][number]): readonly Verdict[] => Option.toArray(Option.all([node, Option.filter(text, (pattern) => /^\w/u.test(pattern))])).map(([word, pattern]) => ({ edit: (editor) => [editor.replace(word, [`[${pattern.slice(0, 1)}]${pattern.slice(1)}`])] }));
const deriveWatch = (view: Invocation) =>
    Option.map(
        Option.filter(
            Array.findFirst(view.operands, ({ word }) => Option.filter(word.text, (text) => /^\d+$/u.test(text))),
            () => view.runs('gh') && view.path[0] === 'run' && view.path[1] === 'view' && !view.has('--log', '--log-failed', '--job', '--web', '--attempt'),
        ),
        (id) => [id, '--exit-status', ...Array.getSomes(view.values('--repo', '-R').map(({ text }) => text)).flatMap((repository) => ['--repo', repository])],
    );
const groupLoops = (invocations: readonly Invocation[]) =>
    Array.dedupeWith(
        invocations.flatMap(({ repeated }) => Option.toArray(repeated)),
        (left, right) => left.source === right.source && left.start === right.start,
    ).map((loop) => {
        const inside = invocations.filter(({ source, start, end }) => source === loop.source && start >= loop.start && end <= loop.end);
        const tested = inside.filter(({ repeated }) => Option.exists(repeated, ({ start, condition }) => start === loop.start && condition));
        return { loop, inside, tested, body: inside.filter((invocation) => !tested.includes(invocation)), until: loop.source.slice(loop.start).startsWith('until') };
    });
const spins = ({ tested, until }: ReturnType<typeof groupLoops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && tested.every((invocation) => invocation.runs(...fixed) && invocation.runs('false') === until);
const probes = ({ tested }: ReturnType<typeof groupLoops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && !tested.every((invocation) => invocation.runs(...fixed));
const print = <E, R>(ran: Effect.Effect<{ readonly stdout: string }, E, R>) =>
    Effect.orElseSucceed(
        Effect.map(ran, ({ stdout }) => stdout),
        () => '',
    );
const findWriters = Effect.fnUntraced(function* (file: string, cwd: string) {
    const [open, processes] = yield* Effect.all([print(execute(cwd)(['lsof', '-F', 'pa', '--', file])), print(execute(cwd)(['ps', '-A', '-o', 'pid=,ppid=']))], { concurrency: 'unbounded' });
    const parents = new Map(processes.split('\n').flatMap((line) => Option.toArray(Option.map(Option.fromNullishOr(/^\s*(?<pid>\d+)\s+(?<parent>\d+)/u.exec(line)?.groups), ({ pid = '', parent = '' }) => [pid, parent] as const))));
    const holders = new Set(open.split(/^p/mu).flatMap((listing) => (/^a[uw]$/mu.test(listing) ? [listing.split('\n', 1).join('')] : [])));
    return [...holders].filter((pid) => parents.has(pid) && !holders.has(parents.get(pid) ?? ''));
});
const findRepeat = Effect.fnUntraced(function* (file: string, cwd: string) {
    const lines = (yield* print(execute(cwd)(['tail', '-n', '10', '--', file])))
        .split('\n')
        .filter((line) => line.trim() !== '')
        .map((line) => line.replaceAll(/\d+/gu, '#'));
    return Option.map(
        Array.findFirst(Map.groupBy(lines, Function.identity), ([, same]) => same.length >= 5),
        ([line]) => line,
    );
});

// --- [RULES] ---------------------------------------------------------------------------

const waits: Table = {
    'timer.bare': {
        action: 'Shell',
        decide: (_, { invocations }) => {
            const commands = invocations.filter(({ inner }) => inner.length === 0);
            return commands.some(idles) && commands.every((invocation) => idles(invocation) || invocation.runs(...inert)) ? [{ deny: 'A bare timer waits on nothing and wakes you with nothing new.', advice: waitAdvice }] : [];
        },
    },
    'timer.trailing': {
        action: 'Shell',
        decide: (_, { invocations }) => {
            const last = Array.findLast(invocations, ({ within }) => within.length === 0);
            return Option.exists(last, (invocation) => Option.isNone(invocation.repeated) && invocation.leaves.some(endsAlone)) ? [moves.background] : [];
        },
    },
    'sleep.instant': {
        program: 'sleep',
        decide: Effect.fnUntraced(function* (sleep, { script, shell }) {
            const stmt = Option.filter(sleep.node, () => sleepsZero(sleep) && sleep.source === script.source && sleep.within.length === 0 && Option.isNone(sleep.repeated) && Option.isNone(sleep.stage));
            const sole = Option.filter(stmt, (found) => script.syntax.stmts.length === 1 && script.syntax.stmts[0] === found);
            const noop = yield* Effect.transposeOption(Option.map(sole, () => shell.script(script.dialect)`:`));
            return Option.toArray(stmt).map((found) => ({ edit: (editor) => [Option.match(noop, { onNone: () => editor.remove(found), onSome: (command) => editor.command(found, command) })] }));
        }),
    },
    'call.idle': {
        action: 'Shell',
        decide: (_, { invocations }) =>
            Array.isReadonlyArrayNonEmpty(invocations) && invocations.every((invocation) => invocation.runs('echo', 'printf', 'true') && invocation.writes.length === 0 && Option.isSome(invocation.argv))
                ? [{ deny: 'Call prints literal text and runs nothing.', advice: 'Background tasks and agents notify when they end, so end the turn; a foreground subagent, whose background work ends with its final answer, keeps working instead.' }]
                : [],
    },
    'job.detached': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (script, { shell }) {
            const listed = yield* script.invocations;
            const detached = script.tree.links
                .keys()
                .filter((node) => node.type === 'Stmt' && node.background)
                .toArray();
            const trailing = Array.dropWhile(script.syntax.stmts, ({ background }) => !background);
            const last = Option.filter(Array.last(trailing), () => trailing.length === detached.length && trailing.every(({ background }) => background));
            const wait = yield* Effect.transposeOption(Option.map(last, () => shell.script(script.dialect)`wait`));
            return Option.toArray(Array.findFirst(listed, (job) => job.background && !isReaped(job, listed))).flatMap((job): readonly Verdict[] =>
                Option.match(Option.all([last, wait]), {
                    onNone: () => [{ deny: `${job.label} & outlives the call.`, advice: 'Set run_in_background on the call, or wait for the job in the same script.' }],
                    onSome: ([stmt, command]) => [{ edit: (editor) => [editor.follow(stmt, ';', command)] }, moves.background],
                }),
            );
        }),
    },
    'loop.endless': {
        action: 'Shell',
        decide: (_, { invocations }) =>
            groupLoops(invocations)
                .filter((loop) => spins(loop) && loop.body.some((invocation) => !invocation.runs(...fixed, ...inert) && !endsAlone(invocation)) && !loop.inside.some((invocation) => invocation.runs(...exits) || invocation.pipe(deriveWatch, Option.isSome)))
                .map(({ tested }) => ({ deny: `Loop on ${tested.map(({ label }) => label).join(' ')} never ends, as nothing changes its condition and no break or exit leaves it.`, advice: 'Wait on a real condition with Monitor `until <check>; do sleep <seconds>; done`.' })),
    },
    'loop.busy': {
        action: 'Shell',
        decide: (_, { invocations }) =>
            groupLoops(invocations)
                .filter((loop) => probes(loop) && loop.body.every((invocation) => invocation.runs(...inert)) && !loop.inside.some(pauses))
                .map(({ tested }) => ({ deny: `Loop on ${tested.map(({ label }) => label).join(' ')} checks again at once and spins a core.`, advice: 'Put sleep <seconds> in its body, or wait with Monitor `until <check>; do sleep <seconds>; done`.' })),
    },
    'loop.waiting': {
        action: 'Shell',
        decide: (_, { invocations }) => {
            const waiting = groupLoops(invocations).filter((loop) => (probes(loop) || loop.inside.some((invocation) => invocation.runs(...exits))) && loop.inside.some(pauses));
            const placed = invocations.filter(({ within }) => within.length === 0).every((invocation) => waiting.some(({ loop }) => (invocation.start >= loop.start && invocation.end <= loop.end) || (invocation.start >= loop.end && invocation.runs(...printers))));
            const checking = waiting.every((loop) => loop.inside.every((invocation) => invocation.runs(...checkers) || (invocation.runs('kill') && invocation.has('-0'))));
            return Array.isReadonlyArrayNonEmpty(waiting) ? [placed && checking ? moves.monitored : moves.background] : [];
        },
    },
    'watch.repaints': { program: 'watch', decide: (watch, { action }) => (action.run === 'monitored' ? [] : [{ deny: `${watch.label} repaints a screen.`, advice: 'Monitor `until <check>; do sleep <seconds>; done` waits on the condition and its exit notifies you.' }]) },
    'caffeinate.unbounded': { program: 'caffeinate', decide: (caffeinate) => (caffeinate.has('-t', '-w') || caffeinate.inner.length > 0 ? [] : [{ deny: 'caffeinate holds the call open until it is killed.', advice: 'Bound it with -t <seconds> or -w <pid>.' }]) },
    'caffeinate.wrapper': {
        program: 'caffeinate',
        decide: (caffeinate) => {
            const command = Option.flatMap(Array.head(caffeinate.inner), ({ words }) => Array.head(words));
            const last = Option.flatMap(command, (word) => Option.flatMap(Array.get(caffeinate.words, caffeinate.words.indexOf(word) - 1), ({ node }) => node));
            return Option.toArray(Option.all([caffeinate.head, last])).map((prefix) => ({ edit: (editor) => [editor.remove(prefix)] }));
        },
    },
    'launchctl.submit': { program: 'launchctl', path: ['submit'], deny: 'starts a detached job that outlives the session', advice: 'Use run_in_background and TaskStop.' },
    'gh.run.polled': {
        program: 'gh',
        path: ['run', 'view'],
        decide: Effect.fnUntraced(function* (view, { invocations, script, shell }) {
            const polls = invocations.every((other) => Option.isSome(other.repeated) && other.writes.every(({ text }) => Option.contains(text, '/dev/null')) && (other.runs(...pollers) || other.pipe(deriveWatch, Option.isSome)));
            const sole = Option.filter(Array.head(script.syntax.stmts), () => polls && script.syntax.stmts.length === 1);
            const watch = Option.filter(deriveWatch(view), () => Option.isSome(view.repeated));
            const command = yield* Effect.transposeOption(Option.map(Option.all([sole, watch]), ([, argv]) => shell.script(script.dialect)`gh run watch ${argv}`));
            return Option.toArray(watch).flatMap((argv): readonly Verdict[] =>
                Option.match(Option.all([sole, command]), {
                    onNone: () => [{ deny: 'gh run view in a loop polls a run.', advice: `Run gh run watch ${argv.join(' ')}, which waits and exits with the run's result.` }],
                    onSome: ([stmt, watched]) => [{ edit: (editor) => [editor.command(stmt, watched)] }, moves.background],
                }),
            );
        }),
    },
    'gh.run.slept': {
        program: 'gh',
        path: ['run', 'view'],
        decide: (view, { invocations, action }) => {
            const before = Array.findLast(invocations, (other) => other.end <= view.start && other.within.length === 0);
            const sleep = Option.flatMap(
                Option.filter(before, (other) => other.runs('sleep') && Option.isNone(other.repeated)),
                ({ node }) => node,
            );
            const plain = Option.isNone(view.repeated) && view.within.length === 0 && !view.has('--json') && action.run !== 'monitored';
            return Option.toArray(Option.filter(Option.all([sleep, deriveWatch(view), view.span]), () => plain)).flatMap(([stmt, argv, span]) => [{ edit: (editor) => [editor.remove(stmt), editor.replace(span, Array.make('gh', 'run', 'watch', ...argv))] }, moves.background]);
        },
    },
    'record.read': { action: 'Read', decide: ({ path }) => refuseRecords([path]) },
    'record.shell': { action: 'Shell', decide: (_, { invocations }) => refuseRecords(Array.dedupe(invocations.flatMap(listPaths))) },
    'task.output': {
        action: 'Shell',
        decide: Effect.fnUntraced(function* (_, { invocations, action, cwd }) {
            const files = Array.dedupe(invocations.flatMap(listPaths)).filter((file) => taskOutput.test(file) && !agentOutput.test(file));
            const watched = action.run === 'monitored' || invocations.some((invocation) => Option.isSome(invocation.repeated) || (invocation.runs('tail') && invocation.has(...following)));
            const polls = !watched && invocations.some(pauses);
            const live = yield* Effect.filter(watched || polls ? [] : files, (file) => Effect.map(findWriters(file, cwd), Array.isReadonlyArrayNonEmpty));
            const repeats = yield* Effect.forEach(live, (file) => findRepeat(file, cwd));
            return polls
                ? files.map((file) => ({ deny: `Call polls the output of task ${nameTask(file)}, which notifies when it ends.`, advice: `End the turn until its notice arrives, or run ${adviseStream(file)}` }))
                : Array.zipWith(live, repeats, (file, repeat) => ({
                      note: Option.match(repeat, {
                          onNone: () => `Task ${nameTask(file)} still runs and notifies when it ends; ${adviseStream(file)}`,
                          onSome: (line) => `Task ${nameTask(file)} repeats "${line}" and looks stuck: TaskStop ${nameTask(file)}, fix its cause, and rerun it bounded by a count or timeout.`,
                      }),
                      once: 'call' as const,
                  }));
        }),
    },
    'task.stuck': {
        action: 'Task',
        decide: Effect.fnUntraced(function* ({ id, output, resting }) {
            const folder = (yield* Path.Path).dirname(output);
            const live = Array.isReadonlyArrayNonEmpty(yield* findWriters(output, folder));
            const repeat = live && resting ? yield* findRepeat(output, folder) : Option.none();
            const stuck = Option.toArray(repeat).map((line) => ({
                note: `Background task ${id} repeats "${line}", so it looks stuck and its completion notice may never come (${output}): read its last lines, TaskStop ${id} if it hangs or loops, fix the cause, and rerun it bounded by a count or timeout.`,
                once: 'agent' as const,
            }));
            return live ? [{ again: Duration.minutes(5) }, ...stuck] : [];
        }),
    },
    'tail.follow': {
        program: 'tail',
        decide: Effect.fnUntraced(function* (tail, { script, shell, cwd }) {
            const files = Array.getSomes(tail.reads.map(({ text }) => text));
            const follows = tail.has(...following) && !tail.has('--pid') && !files.every((file) => file === '/dev/null') && !(tail.background && isReaped(tail, yield* script.invocations));
            const tasked = follows && Array.isReadonlyArrayNonEmpty(files) && files.every((file) => taskOutput.test(file));
            const writers = tasked ? (yield* Effect.forEach(files, (file) => findWriters(file, cwd))).flat() : [];
            const wait = yield* Effect.transposeOption(Option.map(Option.liftPredicate(writers, Array.isReadonlyArrayNonEmpty), (pids) => shell.script(script.dialect)`for pid in ${pids}; do while kill -0 "$pid" 2>/dev/null; do sleep 1; done; done; kill $!`));
            return Match.value({ follows, tasked, live: Array.isReadonlyArrayNonEmpty(writers) }).pipe(
                Match.when({ follows: false }, () => []),
                Match.when({ tasked: true, live: false }, () => [{ drop: following }]),
                Match.when({ tasked: true }, () => [...Option.toArray(Option.all([tail.node, wait])).map(([stmt, command]): Verdict => ({ edit: (editor) => [editor.follow(stmt, '&', command)] })), moves.monitored]),
                Match.orElse(() => [moves.monitored]),
            );
        }),
    },
    'read.timed': {
        program: 'read',
        decide: (read) => {
            const drains = Array.getSomes(read.reads.map(({ text }) => text)).filter((file) => endless.has(file)).length === read.reads.length && !isDelimited(read);
            const unfed = !read.has('-u') && (read.stdin._tag === 'Inherited' || (read.stdin._tag === 'File' && drains)) && Option.isNone(read.repeated);
            const seconds = Option.flatMap(Array.head(read.values('-t')), ({ text }) => text);
            return Option.toArray(Option.filter(seconds, () => unfed)).flatMap((given) => rewrite(read, ['sleep', given]));
        },
    },
    'read.endless': {
        program: 'read',
        decide: (read) => {
            const files = Array.getSomes(read.reads.map(({ text }) => text));
            return files.some((file) => endless.has(file)) && !isDelimited(read) && !read.has('-t') ? [{ deny: `read < ${files.join(' ')} never returns and spins a core.`, advice: 'Run sleep <seconds> to wait.' }] : [];
        },
    },
    'pgrep.self': { program: 'pgrep', decide: (pgrep) => (pgrep.has('-f', '--full') ? Array.take(pgrep.operands, 1).flatMap(({ word }) => bracket(word)) : []) },
    'ps.self': {
        program: 'grep',
        aliases: ['rg'],
        decide: (grep, { invocations }) => (invocations.some((other) => other.runs('ps') && other.feeds(grep)) && grep.flags.length === 0 && grep.operands.length === 1 ? grep.operands.flatMap(({ word }) => bracket(word)) : []),
    },
    'ffmpeg.timer': {
        program: 'ffmpeg',
        decide: (ffmpeg) => {
            const words = Option.getOrElse(ffmpeg.argv, Array.empty<string>);
            const paced = words.includes('-re') || words.includes('-readrate');
            const silent = words.includes('-i') && /^a?nullsrc\b/u.test(words[words.indexOf('-i') + 1] ?? '');
            const discarded = words.at(-1) === '/dev/null' || (words.includes('null') && words.at(-1) === '-');
            return Option.toArray(Option.filter(readNumber(ffmpeg, '-t'), () => paced && silent && discarded)).flatMap((seconds) => rewrite(ffmpeg, ['sleep', seconds]));
        },
    },
    'ping.timer': {
        program: 'ping',
        decide: (ping) => {
            const count = Option.match(readNumber(ping, '-c'), { onNone: () => Number.NaN, onSome: Number });
            const interval = Option.match(readNumber(ping, '-i'), { onNone: () => 1, onSome: Number });
            const seconds = (count - 1) * interval;
            const local =
                Option.exists(
                    Option.flatMap(ping.operand(), (host) => Result.getSuccess(Host.hostFromString(host))),
                    (host) => (NetAddress.isIpAddress(host) ? NetAddress.isLoopback(host) : host === 'localhost'),
                ) &&
                Option.isNone(ping.stage) &&
                !ping.within.some(({ _tag }) => _tag === 'Substitution');
            return local && seconds > 0 ? rewrite(ping, ['sleep', `${seconds}`]) : [];
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { moves, waits };
