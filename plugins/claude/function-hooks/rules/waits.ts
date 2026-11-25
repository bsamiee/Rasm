import { Array, Effect, Match, Option } from 'effect';
import { walk } from 'sh-syntax';
import { command, type Editor, foreground, type Invocation, is, leaves, operand, ran, said, span, spells, staged, type Table, texts, type Written } from '#/rules/rule';

// --- [TYPES] ---------------------------------------------------------------------------

type Loop = Option.Option.Value<Invocation['repeated']>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const idle = ['echo', 'printf', ':', 'true'];
const constant = [':', 'true', 'false'];
const statuses = ['nx', 'uv', 'dotnet', 'xcodebuild', 'swift', 'npm', 'pnpm', 'cargo', 'curl', 'pytest', 'vitest'];
const loopback: ReadonlySet<string> = new Set(['127.0.0.1', 'localhost', '::1']);
const endless: ReadonlySet<string> = new Set(['/dev/zero', '/dev/urandom', '/dev/random']);
const output = /\/tasks\/(?<id>[^/\s]+)\.output$/u;
const agent = /\/tasks\/a[0-9a-f]{16}\.output$/u;
const records = [
    [agent, "is a running agent's output that the agent reports when it ends", 'SendMessage asks it for status.'],
    [/\/\.(?:zsh|bash)_history$|\/\.zsh_sessions(?:\/|$)/u, 'is shell history', "atuin search --filter-mode global --search-mode fulltext '<words>' reads it."],
] as const;
const waiting = 'Background tasks and agents notify when they end, Monitor `until <check>; do sleep 5; done` waits on a condition with timeout_ms as its bound, and a turn with nothing to do ends until a notice arrives.';

// --- [OPERATIONS] ----------------------------------------------------------------------

const timer = (invocation: Invocation) => is(invocation, 'sleep') || (is(invocation, 'tail') && invocation.has('-f', '-F', '--follow') && Array.isReadonlyArrayNonEmpty(invocation.reads) && invocation.reads.every(({ text }) => Option.contains(text, '/dev/null')));
const ends = (invocation: Invocation) => is(invocation, 'sleep', 'wait4path') || (is(invocation, 'caffeinate') && invocation.has('-t', '-w')) || (is(invocation, 'tail') && invocation.has('--pid'));
const pauses = (invocation: Invocation) => is(invocation, 'sleep', 'wait') || (is(invocation, 'read') && invocation.has('-t'));
const closes = (invocation: Invocation) => is(invocation, 'head') || (is(invocation, 'grep', 'rg') && invocation.has('-q', '--quiet', '-m', '--max-count', '-l', '--files-with-matches'));
const task = (file: string) => output.exec(file)?.groups?.['id'] ?? file;
const streamed = (file: string) => `Monitor \`tail -n +1 -F ${file} | grep --line-buffered -E '<pattern>'\` streams the lines you wait for.`;
const holds = Effect.fnUntraced(function* (file: string, cwd: string) {
    const { printed } = yield* Effect.orElseSucceed(ran(['lsof', '-t', '--', file], cwd), () => ({ printed: '', code: 1 }));
    return printed.trim() !== '';
});
const stuck = Effect.fnUntraced(function* (file: string, cwd: string) {
    const { printed } = yield* Effect.orElseSucceed(ran(['tail', '-n', '10', '--', file], cwd), () => ({ printed: '', code: 1 }));
    const lines = printed
        .split('\n')
        .filter((line) => line.trim() !== '')
        .map((line) => line.replaceAll(/\d+/gu, '#'));
    return lines.length >= 5 && new Set(lines).size === 1;
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
const spins = ({ tested, until }: ReturnType<typeof loops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && tested.every((invocation) => is(invocation, ...constant) && is(invocation, 'false') === until);
const probes = ({ tested }: ReturnType<typeof loops>[number]) => Array.isReadonlyArrayNonEmpty(tested) && !tested.every((invocation) => is(invocation, ...constant));
const marked = (written: Written) => Option.map(Option.all([written.node, Option.filter(said(written), (text) => /^\w/u.test(text))]), ([node, text]) => ({ edit: (editor: Editor) => [editor.replace(node, Array.of(`[${text.slice(0, 1)}]${text.slice(1)}`))] }));
const read = (invocation: Invocation) => [...invocation.reads.flatMap(({ text }) => Option.toArray(text)), ...invocation.words.flatMap((written) => Option.toArray(Option.filter(said(written), (text) => text.startsWith('/'))))];
const after = (invocation: Invocation, flag: string) => Option.filter(Option.flatMap(Array.head(invocation.values(flag)), said), (text) => Number.isFinite(Number(text)));
const slept = (invocation: Invocation, seconds: string) => Option.toArray(span(invocation)).map((whole) => ({ edit: (editor: Editor) => [editor.replace(whole, Array.make('sleep', seconds))] }));

const waits: Table = {
    'timer.bare': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const run = leaves(yield* script.invocations).filter(({ inner }) => inner.length === 0);
            return run.some(timer) && run.every((invocation) => timer(invocation) || is(invocation, ...idle)) ? [{ refuse: 'A bare timer waits on nothing and wakes you with nothing new.', instruct: waiting }] : [];
        }),
    },
    'timer.trailing': {
        script: true,
        judge: Effect.fnUntraced(function* (script, call) {
            const last = Array.last((yield* script.invocations).filter(({ within }) => within.length === 0));
            return foreground(call) && Option.exists(last, (invocation) => Option.isNone(invocation.repeated) && leaves([invocation]).some(ends)) ? [{ run: 'background' as const }] : [];
        }),
    },
    'call.idle': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const idled = Array.isReadonlyArrayNonEmpty(invocations) && invocations.every((invocation) => is(invocation, 'echo', 'printf', 'true') && invocation.writes.length === 0 && Option.isSome(texts(invocation.words)));
            return idled ? [{ refuse: 'Call prints literal text and runs nothing.', instruct: 'Background tasks and agents notify when they end, so end the turn; a foreground subagent, whose background work ends with its final answer, keeps working instead.' }] : [];
        }),
    },
    'job.detached': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const jobs = invocations.filter(({ background }) => background);
            const ending = Array.last(jobs).pipe(Option.map(({ end }) => end));
            const reaped = invocations.some((other) => Option.exists(ending, (end) => other.start >= end) && (is(other, 'wait') || (is(other, 'kill') && !other.has('-0'))));
            return Array.match(jobs, { onEmpty: () => [], onNonEmpty: ([job]) => (reaped ? [] : [{ refuse: `${command(job)} & outlives the call.`, instruct: 'Set run_in_background on the call, or wait for the job in the same script.' }]) });
        }),
    },
    'loop.endless': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const spinning = loops(leaves(yield* script.invocations)).filter((found) => spins(found) && found.body.some((invocation) => !is(invocation, ...constant, ...idle) && !ends(invocation)) && !found.inside.some((invocation) => is(invocation, 'break', 'exit', 'return')));
            return spinning.map(({ tested }) => ({ refuse: `Loop on ${tested.map(command).join(' ')} never ends, as nothing changes its condition and no break or exit leaves it.`, instruct: 'Wait on a real condition with Monitor `until <check>; do sleep <seconds>; done`.' }));
        }),
    },
    'loop.busy': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const busy = loops(leaves(yield* script.invocations)).filter((found) => probes(found) && found.body.every((invocation) => is(invocation, ...idle)) && !found.inside.some(pauses));
            return busy.map(({ tested }) => ({ refuse: `Loop on ${tested.map(command).join(' ')} checks again at once and spins a core.`, instruct: 'Put sleep <seconds> in its body, or wait with Monitor `until <check>; do sleep <seconds>; done`.' }));
        }),
    },
    'loop.waiting': {
        script: true,
        judge: Effect.fnUntraced(function* (script, call) {
            const invocations = yield* script.invocations;
            const waited = loops(leaves(invocations)).filter((found) => probes(found) && found.inside.some(pauses));
            const inside = (invocation: Invocation) => waited.some(({ loop }) => invocation.start >= loop.start && invocation.end <= loop.end);
            const trailing = (invocation: Invocation) => waited.some(({ loop }) => invocation.start >= loop.end) && is(invocation, 'cat', 'tail', 'head', 'echo', 'printf', 'rg', 'jq', 'ls');
            const placed = invocations.filter(({ within }) => within.length === 0).every((invocation) => inside(invocation) || trailing(invocation));
            return foreground(call) && Array.isReadonlyArrayNonEmpty(waited) && placed ? [{ run: 'background' as const }] : [];
        }),
    },
    'watch.repaints': { program: 'watch', refuse: 'repaints a screen', instruct: 'Monitor `until <check>; do sleep <seconds>; done` waits on the condition and its exit notifies you.' },
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
        judge: (invocation) => {
            const run = invocation.operands.flatMap(({ word }) => Option.toArray(Option.filter(said(word), (text) => /^\d+$/u.test(text))));
            const repos = invocation.values('--repo', '-R').flatMap((value) => Option.toArray(said(value)).map((repo) => ` --repo ${repo}`));
            return Option.isSome(invocation.repeated) && !invocation.has('--log', '--log-failed', '--job', '--web', '--attempt')
                ? [{ refuse: 'gh run view in a loop polls a run.', instruct: `Run gh run watch ${run.join(' ') || '<run>'} --exit-status${repos.join('')}, which waits and exits with the run's result.` }]
                : [];
        },
    },
    'gh.run.slept': {
        program: 'gh',
        path: ['run', 'view'],
        judge: Effect.fnUntraced(function* (invocation, call) {
            const before = (yield* call.script.invocations).filter((other) => other.end <= invocation.start && other.within.length === 0);
            const sleep = Option.filter(Array.last(before), (other) => is(other, 'sleep') && Option.isNone(other.repeated));
            const run = Array.findFirst(invocation.operands, ({ word }) => Option.filter(said(word), (text) => /^\d+$/u.test(text)));
            const repos = invocation.values('--repo', '-R').flatMap((value) => Option.toArray(said(value)).flatMap((repo) => ['--repo', repo]));
            const plain = Option.isNone(invocation.repeated) && invocation.within.length === 0 && !invocation.has('--log', '--log-failed', '--job', '--web', '--attempt', '--json') && call.action.tool === 'Bash';
            return Option.match(
                Option.filter(Option.all([Option.flatMap(sleep, ({ node }) => node), run, span(invocation)]), () => plain),
                {
                    onNone: () => [],
                    onSome: ([stmt, id, whole]) => [{ edit: (editor: Editor) => [editor.remove(stmt), editor.replace(whole, Array.make('gh', 'run', 'watch', id, '--exit-status', ...repos))] }, { run: 'background' as const }],
                },
            );
        }),
    },
    'record.read': { tool: 'Read', judge: (action) => records.filter(([pattern]) => pattern.test(action.file_path)).map(([, fact, route]) => ({ refuse: `${action.file_path} ${fact}.`, instruct: route })) },
    'record.shell': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const files = Array.dedupe(leaves(yield* script.invocations).flatMap(read));
            return records.flatMap(([pattern, fact, route]) => files.filter((file) => pattern.test(file)).map((file) => ({ refuse: `${file} ${fact}.`, instruct: route })));
        }),
    },
    'task.output': {
        script: true,
        judge: Effect.fnUntraced(function* (script, call) {
            const all = leaves(yield* script.invocations);
            const files = Array.dedupe(all.flatMap(read)).filter((file) => output.test(file) && !agent.test(file));
            const polled = all.some((invocation) => Option.isSome(invocation.repeated) || pauses(invocation));
            const live = polled ? [] : yield* Effect.filter(files, (file) => holds(file, call.cwd));
            const repeating = yield* Effect.forEach(live, (file) => stuck(file, call.cwd));
            const notes = Array.zipWith(live, repeating, (file, repeats) => ({
                note: repeats ? `Task ${task(file)} prints one line again and again and looks stuck: TaskStop ${task(file)}, fix its cause, and rerun it bounded by a count or timeout.` : `Task ${task(file)} still runs and notifies when it ends; ${streamed(file)}`,
                once: 'call' as const,
            }));
            return polled ? files.map((file) => ({ refuse: `Call polls the output of task ${task(file)}, which notifies when it ends.`, instruct: `End the turn until its notice arrives, or run ${streamed(file)}` })) : notes;
        }),
    },
    'tail.follow': {
        program: 'tail',
        judge: Effect.fnUntraced(function* (invocation, call) {
            const files = invocation.reads.flatMap(({ text }) => Option.toArray(text));
            const follows = invocation.has('-f', '-F', '--follow') && !invocation.has('--pid') && call.action.tool === 'Bash' && !files.every((file) => file === '/dev/null');
            const tasked = follows && Array.isReadonlyArrayNonEmpty(files) && files.every((file) => output.test(file));
            const holders = tasked ? yield* Effect.forEach(files, (file) => holds(file, call.cwd)) : [true];
            return Match.value({ following: follows, finished: !holders.some(Boolean) }).pipe(
                Match.when({ finished: true }, () => [{ drop: ['-f', '-F', '--follow'] }]),
                Match.when({ following: true }, () => [{ refuse: `${command(invocation)} -f never ends, so the call blocks until its timeout.`, instruct: 'Run it with the Monitor tool, or drop -f to print what the file holds now.' }]),
                Match.orElse(() => []),
            );
        }),
    },
    'read.timed': {
        program: 'read',
        judge: (invocation) => {
            const unfed = !invocation.has('-u') && invocation.reads.length === 0 && Option.isNone(invocation.repeated) && !Option.exists(invocation.stage, ({ index }) => index > 0);
            return Option.match(
                Option.filter(Option.flatMap(Array.head(invocation.values('-t')), said), () => unfed),
                { onNone: () => [], onSome: (seconds) => slept(invocation, seconds) },
            );
        },
    },
    'read.endless': {
        program: 'read',
        judge: (invocation) => {
            const sources = invocation.reads.flatMap(({ text }) => Option.toArray(text));
            const bounded = invocation.has('-n', '-N') || invocation.values('-d').some((value) => Option.contains(said(value), ''));
            return sources.some((file) => endless.has(file)) && !bounded ? [{ refuse: `read < ${sources.join(' ')} never returns and spins a core.`, instruct: 'Run sleep <seconds> to wait.' }] : [];
        },
    },
    'pgrep.self': { program: 'pgrep', judge: (invocation) => (invocation.has('-f', '--full') ? Option.toArray(Option.flatMap(Array.head(invocation.operands), ({ word }) => marked(word))) : []) },
    'ps.self': {
        program: 'grep',
        aliases: ['rg'],
        judge: Effect.fnUntraced(function* (grep, { script }) {
            const ps = Option.exists(staged(yield* script.invocations, grep, -1), (found) => is(found, 'ps'));
            return ps && grep.flags.length === 0 && grep.operands.length === 1 ? Option.toArray(Option.flatMap(Array.head(grep.operands), ({ word }) => marked(word))) : [];
        }),
    },
    'pipe.status': {
        script: true,
        judge: Effect.fnUntraced(function* (script) {
            const invocations = yield* script.invocations;
            const set = invocations.some((invocation) => is(invocation, 'set') && invocation.words.some((written) => spells(written, 'pipefail')));
            const hidden = invocations.filter((invocation) => Option.exists(invocation.stage, ({ index, count }) => index < count - 1) && leaves([invocation]).some((leaf) => is(leaf, ...statuses)) && !Option.exists(staged(invocations, invocation, 1), closes));
            return set ? [] : hidden.map((invocation) => ({ note: `${command(invocation)} exits into a pipe, so the call reports the last stage's status; set -o pipefail; before it keeps the failure.`, once: 'agent' as const }));
        }),
    },
    'zsh.special': {
        script: true,
        judge: (script) => {
            const names: string[] = [];
            walk(script.syntax, (node) => {
                names.push(...(node.type === 'Assign' && node.name !== null ? [node.name.value] : []), ...(node.type === 'WordIter' && node.name !== null ? [node.name.value] : []));
            });
            return Array.dedupe(names)
                .filter((name) => script.dialect.special.has(name))
                .map((name) => ({ refuse: `zsh ${name} is a special parameter, so assigning it changes the shell itself.`, instruct: 'Rename the variable.' }));
        },
    },
    'ffmpeg.timer': {
        program: 'ffmpeg',
        judge: (invocation) => {
            const words = Option.getOrElse(texts(invocation.words), (): readonly string[] => []);
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
            const local = Option.exists(operand(invocation), (host) => loopback.has(host)) && Option.isNone(invocation.stage) && !invocation.within.some(({ _tag }) => _tag === 'Substitution');
            return local && seconds > 0 ? slept(invocation, `${seconds}`) : [];
        },
    },
} satisfies Table;

// --- [EXPORTS] -------------------------------------------------------------------------

export { waits };
