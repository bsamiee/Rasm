import { devNull } from 'node:os';
import process from 'node:process';
import { Array, Clock, Duration, Effect, Filter, Option, Record } from 'effect';
import { RedirOperator } from 'sh-syntax/sync';
import { Host } from '#/host';
import { Verdict } from '#/rules/decision';
import { type Call, edited, erased, inserted, once, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, respell, spelled, stages, when, whole, within } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';
import type { Word } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTER = /^\w/u;
const DIGITS = /^\d+$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output(?![^\s'"])/gu;
const PROCESS_FIELD = /^p/mu;
const WRITE_ACCESS = /^a[uw]$/mu;
const BLANK_LINE = /^\s*(?:#.*)?$/u;
const LOWERCASE = /^\p{Ll}/u;
const SPECIAL_KIND = /\b(?:tied|readonly|association)\b.*\bspecial\b/u;
const ENDLESS: ReadonlySet<string> = new Set(['/dev/zero', '/dev/urandom', '/dev/random']);
const IDLE: ReadonlySet<string> = new Set(['echo', 'printf', ':', 'true']);
const CAPTURE = /^(?:\$\((?!\()|`)/u;
const RUN_POLLS: ReadonlySet<string> = new Set(['sleep', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'date', 'read']);
const PROBES: ReadonlySet<string> = new Set([...RUN_POLLS, 'false', 'pgrep', 'ps', 'stat', 'ls', 'cat', 'tail', 'head', 'wc', 'curl', 'lsof', 'nc', 'wait4path', 'caffeinate', 'seq', 'break', 'continue', 'exit']);
const UNTESTED: ReadonlySet<string> = new Set(['true', ':', 'false', 'read', 'sleep']);
const CONSTANT: ReadonlySet<string> = new Set(['true', ':', 'false']);
const LEAVES: ReadonlySet<string> = new Set(['break', 'exit']);
const TRAILING_READS: ReadonlySet<string> = new Set(['cat', 'tail', 'head', 'echo', 'printf', 'rg', 'jq', 'ls']);
const WAITERS: ReadonlySet<string> = new Set(['sleep', 'timeout', 'caffeinate', 'wait4path', 'watch']);
const AGENT_TASK = /^a[0-9a-f]{16}$/u;
const PATH_WORD = /^\S+$/u;
const TRANSCRIPT = /\/\.claude\/projects(?:\/[^/]+)?\/?$|\/\.claude\/projects\/.+\.jsonl$|\/\.codex\/(?:archived_)?sessions(?:\/|$)/u;
const SHELL_HISTORY = /\/\.(?:zsh|bash)_history$|\/\.zsh_sessions(?:\/|$)/u;
const LOOPBACK: ReadonlySet<string> = new Set(['127.0.0.1', 'localhost', '::1']);
const SILENT_SOURCE = /^a?nullsrc\b/u;
const STUCK_LINES = 10;
const STUCK_REPEATS = 5;
const QUIET = Duration.minutes(10);
const EXIT_WAITERS: Partial<Record<NodeJS.Platform, (pid: string) => string>> = {
    darwin: (pid) => `caffeinate -w ${pid}`,
    linux: (pid) => `tail --pid=${pid} -f /dev/null`,
};

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [RUNS]

const viewedRun = (invocation: Invocation) => {
    const [run] = invocation.operands;
    const viewed = invocation.path[0] === 'run' && invocation.path[1] === 'view' && !invocation.has('--log', '--log-failed', '--job', '--web', '--attempt');
    return Option.map(
        Option.filter(Option.fromNullishOr(run), ({ text }) => viewed && DIGITS.test(text)),
        ({ text }) => ({ run: text, follow: ['gh run watch', text, '--exit-status', ...invocation.supplied('--repo').flatMap(({ raw }) => ['--repo', raw])].join(' ') }),
    );
};

const polledRun: Rule = (invocation, { analysis, request }) =>
    Option.match(
        Option.filter(viewedRun(invocation), () => invocation.repeated),
        {
            onNone: () => [],
            onSome: ({ run, follow }) => {
                const own = analysis.invocations.filter((other) => other.command.origin === 'call');
                const polls = own.every((other) => other.repeated && (RUN_POLLS.has(other.program) || Option.exists(viewedRun(other), (viewed) => viewed.run === run)));
                const background = request.kind === 'foreground' ? [Verdict.Background()] : [];
                return invocation.command.origin === 'call' && polls ? [Verdict.Edit({ start: 0, end: invocation.command.source.length, text: Option.some(follow), subject: Option.some('gh run view loop') }), ...background] : refusal(`Loop polls gh run view, run ${follow}`);
            },
        },
    );

const watchedRun: Rule = (invocation, { analysis, request }) => {
    const { command } = invocation;
    const waited = analysis.invocations.findLast((other) => other.command.source === command.source && other.command.end <= command.start);
    const read = command.origin === 'call' && invocation.outer.length === 0 && !(command.pipes || command.fed || command.captured) && command.redirects.every(({ op }) => op === RedirOperator.DplOut);
    const slept = waited?.program === 'sleep' && waited.outer.length === 0 && !waited.command.looping;
    return Option.match(
        Option.filter(viewedRun(invocation), () => !invocation.repeated && read && slept && (request.kind === 'foreground' || request.kind === 'background')),
        {
            onNone: () => [],
            onSome: ({ follow }) => [...(waited === undefined ? [] : edited(waited, waited.command, ':')), ...whole(invocation, follow, 'sleep before gh run view'), ...(request.kind === 'foreground' ? [Verdict.Background()] : [])],
        },
    );
};

// --- [TIMERS]

const taskIds = (text: string): readonly string[] => Array.dedupe(Array.getSomes(Array.fromIterable(text.matchAll(TASK_OUTPUTS)).map(({ groups }) => Option.flatMap(Option.fromNullishOr(groups), Record.get('id')))));

const closesInput = ({ row: { closesInput: closes }, has }: Invocation): boolean => closes === true || (closes !== undefined && has(...closes));

const timer = (invocation: Invocation, { request }: Call): readonly Verdict[] => (request.kind === 'foreground' && invocation.last ? [Verdict.Background()] : []);

const bareTimer = (call: Call, waiter: string): readonly Verdict[] | undefined =>
    call.analysis.invocations.every(({ program, command: other }) => other.origin !== 'call' || program === waiter || program === 'timeout' || IDLE.has(program))
        ? refusal(
              `A bare timer waits on nothing and wakes you with nothing new: ${call.request.kind === 'plain' ? 'a wait on a spawned agent returns with its result' : 'background tasks and agents notify when they end, Monitor `until <check>; do sleep 5; done` waits on a condition with timeout_ms as its bound'}, and a turn with nothing to do ends until a notice arrives`,
          )
        : undefined;

const sleep: Rule = (invocation, call) => {
    const { command, operands } = invocation;
    const alone = command.origin === 'call' && invocation.outer.length === 0 && !command.looping && !command.fed && !command.pipes;
    if (operands.every(({ text }) => Number(text) === 0)) {
        return alone ? edited(invocation, command, ':', spelled(invocation.words)) : [];
    }
    return bareTimer(call, 'sleep') ?? timer(invocation, call);
};

const nullTail: Rule = (_invocation, call) => bareTimer(call, 'tail') ?? [];

const streamed: Rule = (invocation, call) => {
    const { kind } = call.request;
    if (kind === 'monitor' || !invocation.last || (invocation.command.job && reaped(call))) {
        return [];
    }
    if (kind !== 'plain') {
        return [Verdict.Monitor()];
    }
    return stages(invocation, call).later.some(closesInput) ? [] : refusal(`${invocation.program} holds the call open, run it in the background`);
};

const exitsAlone: Rule = (invocation, call) => (invocation.command.job && reaped(call) ? [] : timer(invocation, call));

const repainted: Rule = (invocation, { request }) => (request.kind === 'monitor' ? [] : refusal(`${invocation.program} repaints a screen, Monitor \`until <check>; do sleep <seconds>; done\` waits on the condition and its exit notifies you`));

const silentSleep: Rule = (invocation) => {
    const texts = invocation.words.map(({ text }) => text);
    const after = (flag: string) =>
        Option.flatMap(
            Array.findFirstIndex(texts, (text) => text === flag),
            (at) => Array.get(texts, at + 1),
        );
    const format = Option.flatMap(
        Array.findLastIndex(texts, (text) => text === '-f'),
        (at) => Array.get(texts, at + 1),
    );
    const target = Array.last(texts);
    const discarded = Option.contains(format, 'null') || Option.contains(target, devNull) || (Option.contains(target, '-') && !invocation.command.pipes);
    const silent = Option.exists(after('-i'), (source) => SILENT_SOURCE.test(source)) && discarded;
    return Option.match(
        Option.filter(after('-t'), (seconds) => silent && Number.isFinite(Number(seconds))),
        { onNone: () => [], onSome: (seconds) => whole(invocation, `sleep ${seconds}`, spelled(invocation.words)) },
    );
};

const loopbackPing: Rule = (invocation) => {
    const texts = invocation.words.map(({ text }) => text);
    const after = (flag: string, fallback: number) =>
        Option.match(
            Option.flatMap(
                Array.findFirstIndex(texts, (text) => text === flag),
                (at) => Array.get(texts, at + 1),
            ),
            { onNone: () => fallback, onSome: Number },
        );
    const seconds = after('-c', Number.NaN) * after('-i', 1);
    return invocation.operands.some(({ text }) => LOOPBACK.has(text)) && Number.isFinite(seconds) ? whole(invocation, `sleep ${seconds}`, spelled(invocation.words)) : [];
};

const unwrap = (invocation: Invocation) => Option.match(Array.head(invocation.rest), { onNone: () => [], onSome: ({ start }) => erased(invocation, { start: invocation.head.start, end: start }, spelled(invocation.words)) });

const stalledRead: Rule = (invocation, call) => {
    const { command } = invocation;
    if (invocation.has('-u')) {
        return [];
    }
    const [amount] = invocation.values('-t');
    const sources = Array.getSomes(redirected(invocation, RedirOperator.RdrIn).flatMap(({ descriptor, target }) => (Option.getOrElse(descriptor, () => '0') === '0' ? [target] : []))).map(({ text }) => text);
    const endless = sources.find((source) => ENDLESS.has(source));
    const unwaited = !command.fed || (sources.length > 0 && sources.every((source) => source === '/dev/null' || ENDLESS.has(source))) || within(invocation, call).some(({ command: inner }) => inner.substituted);
    if (amount !== undefined) {
        return unwaited ? [...edited(invocation, command, `sleep ${amount}`, `read -t ${amount}`), ...timer(invocation, call)] : [];
    }
    return endless === undefined || invocation.has('-n', '-N') ? [] : refusal(`read < ${endless} never returns and spins a core, run sleep <seconds>`);
};

const selfMatch = (invocation: Invocation, word: Word | undefined) => (word === undefined || !word.known || !LETTER.test(word.text) ? [] : edited(invocation, word, quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`)));

const psFilter: Rule = (invocation, call) => {
    const fromPs = stages(invocation, call).earlier.some(({ program }) => program === 'ps');
    return fromPs && invocation.names.size === 0 && invocation.command.fed && invocation.outer.length === 0 ? selfMatch(invocation, invocation.leading[0]) : [];
};

// --- [TASK_OUTPUT]

const writers = (host: Host['Service'], files: readonly string[]) =>
    Effect.map(Effect.all([host.parents, Effect.forEach(files, (file) => host.output(['lsof', '-F', 'pa', file]), { concurrency: 'unbounded' })]), ([parents, listings]) =>
        listings.map((listing) => {
            const holders = listing
                .split(PROCESS_FIELD)
                .filter((held) => WRITE_ACCESS.test(held))
                .map((held) => held.split('\n', 1).join(''));
            return holders.filter((pid) => parents.has(pid) && !holders.includes(`${parents.get(pid)}`)).slice(0, 1);
        }),
    );

const tailedTask: Rule = (invocation, call) => {
    const files = invocation.inputs.map(({ text }) => text).filter((file) => taskIds(file).length > 0);
    if (call.request.kind !== 'monitor' || files.length === 0 || (invocation.command.job && reaped(call)) || stages(invocation, call).later.some(closesInput)) {
        return [];
    }
    return writers(call.host, files).pipe(
        Effect.map((held) =>
            Option.match(Option.fromNullishOr(EXIT_WAITERS[process.platform]), {
                onNone: () => [],
                onSome: (waiter) => (held.flat().length === 0 ? respell({ drops: ['-f', '-F', '--follow'] })(invocation) : [...inserted(invocation, invocation.head.start, '{ '), ...inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` & ${held.flat().map(waiter).join('; ')}; kill $!; }`)]),
            }),
        ),
        Effect.orElseSucceed(() => []),
    );
};

const stalled = Effect.fnUntraced(function* (host: Host['Service'], file: string) {
    const [tail, info, now] = yield* Effect.all([Effect.orElseSucceed(host.output(['tail', '-n', `${STUCK_LINES}`, file]), () => ''), Effect.option(host.fs.stat(file)), Clock.currentTimeMillis]);
    const lines = tail
        .split('\n')
        .filter((line) => line.trim() !== '')
        .map((line) => line.replaceAll(/\d+/gu, '#'));
    const [line, count] = Record.reduce(
        Array.groupBy(lines, (held) => held),
        ['', 0] as readonly [string, number],
        (best, same, held) => (same.length > best[1] ? [held, same.length] : best),
    );
    const quiet = Option.getOrElse(
        Option.flatMap(info, ({ mtime }) => Option.map(mtime, (date) => Duration.millis(now - date.getTime()))),
        () => Duration.zero,
    );
    return Option.firstSomeOf([Option.liftPredicate({ key: `repeats ${line}`, text: `repeats "${line}"` }, () => count >= STUCK_REPEATS), Option.liftPredicate({ key: 'quiet', text: `wrote nothing for ${Duration.format(quiet)}` }, () => Duration.isGreaterThan(quiet, QUIET))]);
});

const watched = Effect.fnUntraced(function* (files: readonly string[]) {
    const host = yield* Host;
    const held = yield* Effect.orElseSucceed(writers(host, files), () => files.map((): readonly string[] => []));
    const stuck = yield* Effect.forEach(Array.zip(files, held), ([file, holders]) => (Array.isReadonlyArrayEmpty(holders) ? Effect.succeedNone : stalled(host, file)));
    return files.map((file, at) => ({ file, live: Array.isReadonlyArrayNonEmpty(held[at] ?? []), ...Option.getOrUndefined(Option.map(stuck[at] ?? Option.none(), (found) => ({ stuck: found }))) }));
});

const RECORDS = [
    { held: (file: string) => taskIds(file).some((id) => AGENT_TASK.test(id)), route: "is a running agent's output that the agent reports when it ends, SendMessage asks it for status" },
    { held: (file: string) => TRANSCRIPT.test(file), route: 'is a session transcript, atuin threads show <session id> --style ndjson reads it' },
    { held: (file: string) => SHELL_HISTORY.test(file), route: "is shell history, atuin search --filter-mode global --search-mode fulltext '<words>' reads it" },
] as const;

const polledTask = Effect.fnUntraced(function* (call: Call) {
    const { analysis, host, request } = call;
    const own = analysis.invocations.filter(({ command }) => command.origin === 'call');
    const files = Array.dedupe(
        own.flatMap((invocation) =>
            Array.getSomes([...invocation.words, ...Array.getSomes(invocation.command.redirects.map(({ target }) => target))].filter(({ text }) => PATH_WORD.test(text) && (taskIds(text).length > 0 || TRANSCRIPT.test(text) || SHELL_HISTORY.test(text))).map(({ text }) => host.resolve(text, invocation.command.cwd))),
        ),
    );
    const routes = RECORDS.flatMap(({ held, route }) => Array.match(files.filter(held), { onEmpty: () => [], onNonEmpty: (read) => [`${read.join(' ')} ${route}`] }));
    const ids = taskIds(files.join(' '));
    const waits = own.some(({ program, repeated, has }) => repeated || WAITERS.has(program) || (program === 'read' && has('-t')));
    if (routes.length > 0) {
        return refusal(routes.join('; '));
    }
    if (ids.length === 0 || waits || request.kind === 'monitor') {
        return [];
    }
    const live = yield* Effect.match(writers(host, files), { onFailure: () => [], onSuccess: (held) => files.filter((_file, at) => Array.isReadonlyArrayNonEmpty(held[at] ?? [])) });
    const judged = request.call === undefined || (yield* once(request, `call ${request.call}`));
    const again = yield* Effect.filter(live, (file) => (judged ? Effect.map(once(request, `task ${file}`), (fresh) => !fresh) : Effect.succeed(false)));
    return yield* Effect.forEach(again, (file) =>
        Effect.map(stalled(host, file), (evidence) => {
            const id = taskIds(file).join(' ');
            return Verdict.Note({
                text: Option.match(evidence, {
                    onNone: () => `Task ${id} still runs and notifies when it ends, and Monitor \`tail -n +1 -F ${quote(file)} | grep --line-buffered -E '<pattern>'\` streams the lines you wait for`,
                    onSome: ({ text }) => `Task ${id} ${text} and looks stuck, TaskStop ${id}, fix its cause, and rerun it bounded by a count or timeout`,
                }),
            });
        }),
    );
});

// --- [JOBS]

const reaped = ({ analysis }: Call): boolean => analysis.invocations.some(({ program }) => program === 'wait' || program === 'kill');

const backgroundJob = (call: Call): readonly Verdict[] => {
    const { analysis, request } = call;
    const [job, ...more] = analysis.jobs;
    if (job === undefined || reaped(call)) {
        return [];
    }
    const trailing =
        more.length === 0 &&
        job.origin === 'call' &&
        new TextDecoder()
            .decode(new TextEncoder().encode(request.command).subarray(job.end))
            .split('\n')
            .every((line) => BLANK_LINE.test(line));
    if (!trailing) {
        return refusal(`& job outlives the call, ${request.kind === 'plain' ? 'run it as its own call' : 'set run_in_background'}`);
    }
    return [Verdict.Edit({ start: job.start, end: job.end, text: Option.none(), subject: Option.none() }), ...(request.kind === 'foreground' ? [Verdict.Background()] : [])];
};

const pipedStatus = (call: Call): readonly Verdict[] => {
    const own = call.analysis.invocations.filter(({ command: { origin } }) => origin === 'call');
    const set = own.some(({ program, values }) => program === 'set' && values('-o').includes('pipefail'));
    const hidden = own.find(({ row, last, command: { pipes } }) => pipes && last && (row.pipefail === true || row.nx !== undefined));
    const closing = own.some((invocation) => invocation.command.fed && closesInput(invocation));
    return hidden !== undefined && !set && !closing ? inserted(hidden, 0, 'set -o pipefail; ') : [];
};

const waitLoop = ({ analysis, request }: Call): readonly Verdict[] => {
    const own = analysis.invocations.filter(({ command }) => command.origin === 'call' && !command.substituted);
    const [looping, outside] = Array.partition(
        own,
        Filter.fromPredicateOption(({ program, has, command }: Invocation) => Option.map(command.loop, (loop) => ({ program, loop, probe: PROBES.has(program) || (program === 'kill' && has('-0')), sleep: program === 'sleep' || (program === 'read' && has('-t')) }))),
    );
    const end = Math.max(...looping.map(({ loop }) => loop.end));
    const tested = looping.filter(({ loop }) => loop.condition);
    const waits = (tested.length > 0 && tested.every(({ probe, program }) => probe && !UNTESTED.has(program))) || looping.some(({ program }) => LEAVES.has(program));
    const probes = looping.every(({ probe }) => probe);
    const sleeps = looping.some(({ sleep: waited }) => waited);
    const placed = outside.every(({ program, command }) => command.start >= end && TRAILING_READS.has(program));
    return (request.kind === 'foreground' || request.kind === 'background') && looping.length > 0 && waits && probes && sleeps && placed ? [Verdict.Monitor()] : [];
};

const endlessLoop = ({ analysis }: Call): readonly Verdict[] => {
    const loops = Map.groupBy(
        analysis.invocations.filter(({ command }) => command.origin === 'call' && Option.isSome(command.loop)),
        ({ command }) => Option.match(command.loop, { onNone: () => -1, onSome: ({ start }) => start }),
    );
    return [...loops.values()].flatMap((looped) => {
        const tested = looped.filter(({ command }) => Option.exists(command.loop, ({ condition }) => condition));
        const runs = looped.some(({ program }) => !CONSTANT.has(program) && !IDLE.has(program) && !WAITERS.has(program));
        return runs && tested.length > 0 && tested.every(({ program }) => CONSTANT.has(program)) && !looped.some(({ program }) => LEAVES.has(program) || program === 'return')
            ? refusal(`Loop on ${tested.map(({ program }) => program).join(' ')} never ends, as nothing changes its condition and no break or exit leaves it, wait on a real condition with Monitor \`until <check>; do sleep <seconds>; done\``)
            : [];
    });
};

const idleCall = ({ analysis: { invocations } }: Call): readonly Verdict[] =>
    invocations.length > 0 &&
    invocations.every(({ program, words, command }) => (IDLE.has(program) || (program === 'date' && command.captured)) && command.redirects.length === 0 && words.every(({ parts }) => parts.every(({ known, glob, parameter, raw }) => parameter === undefined && !glob && (known || CAPTURE.test(raw)))))
        ? refusal('Call prints literal text and runs nothing: background tasks and agents notify when they end, so end the turn, and a foreground subagent, whose background work ends with its final answer, keeps working instead')
        : [];

// --- [PARAMETERS]

const zshParameter = ({ analysis: { zshAssigned }, host }: Call) => {
    const names = Array.dedupe(zshAssigned.filter((name) => LOWERCASE.test(name)));
    return names.length === 0
        ? []
        : Effect.orElseSucceed(
              Effect.map(host.output(['zsh', '-fc', 'for name; print -r -- "$name $parameters[$name]"', 'zsh', ...names]), (lines) =>
                  lines
                      .split('\n')
                      .map((line) => line.split(' '))
                      .flatMap(([name, kind = '']) => (SPECIAL_KIND.test(kind) ? refusal(`zsh ${name} is ${kind}, rename the variable`) : [])),
              ),
              () => [],
          );
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const WAITS: Ruleset = {
    programs: {
        sleep: { rules: [sleep] },
        wait4path: { rules: [exitsAlone] },
        watch: { rules: [repainted] },
        timeout: { leading: 1, wraps: [[]] },
        caffeinate: { wraps: [[]], rules: [unwrap, when(({ has }) => has('-t', '-w'), exitsAlone), when(({ has, rest }) => !has('-t', '-w') && rest.length === 0, refuse('holds the call open until it is killed', 'bound it with -t <seconds> or -w <pid>'))] },
        read: { leading: Number.POSITIVE_INFINITY, rules: [stalledRead] },
        pgrep: { rules: [(invocation) => (invocation.has('--full') ? selfMatch(invocation, invocation.operands[0]) : [])] },
        gh: { rules: [polledRun, watchedRun] },
        grep: { closesInput: ['--quiet', '--silent', '--max-count', '--files-with-matches'], rules: [psFilter] },
        rg: { closesInput: ['--quiet', '--max-count', '--files-with-matches'] },
        head: { closesInput: true },
        tail: {
            flags: ['-F', '-b='],
            reads: true,
            rules: [when(({ has, inputs }) => has('-f', '-F') && inputs.some(({ text }) => text !== devNull), tailedTask, streamed), when(({ has, inputs }) => has('-f', '-F') && inputs.length > 0 && inputs.every(({ text }) => text === devNull), nullTail)],
        },
        ffmpeg: { rules: [silentSleep] },
        ping: { rules: [loopbackPing] },
        launchctl: { rules: [perSubcommand({ submit: [() => refusal('launchctl submit detached jobs outlive the session; use run_in_background and TaskStop, or a mise [daemons] row')] })] },
    },
    every: [(invocation) => (invocation.command.busy ? refusal('Loop body never waits and spins a core, put sleep <seconds> in it') : [])],
    calls: [backgroundJob, pipedStatus, zshParameter, idleCall, polledTask, endlessLoop, waitLoop],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { WAITS, watched };
