import { Array, Effect, Option, Record } from 'effect';
import { RateLimiter } from 'effect/persistence';
import { RedirOperator } from 'sh-syntax/sync';
import { Verdict } from '#/rules/decision';
import { type Call, edited, erased, inserted, quote, type Rule, type Ruleset, redirected, refusal, spelled, stages, when, whole, within } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';
import { WATCH_APPS } from '#/shell/programs';
import type { Word } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTER = /^\w/u;
const DIGITS = /^\d+$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output(?![^\s'"])/gu;
const BLANK_LINE = /^\s*(?:#.*)?$/u;
const LOWERCASE = /^\p{Ll}/u;
const SPECIAL_KIND = /\b(?:tied|readonly|association)\b.*\bspecial\b/u;
const ENDLESS: ReadonlySet<string> = new Set(['/dev/zero', '/dev/urandom', '/dev/random']);
const IDLE: ReadonlySet<string> = new Set(['echo', 'printf', ':', 'true']);
const CAPTURE = /^(?:\$\(|`)/u;
const RUN_POLLS: ReadonlySet<string> = new Set(['sleep', 'jq', 'grep', 'rg', 'echo', 'printf', 'test', '[', 'true', ':', 'date', 'read']);
const WAITERS: ReadonlySet<string> = new Set(['sleep', 'timeout', 'caffeinate', 'wait4path', 'watch']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WATCH_APPS]

const watchAction = ({ request }: Call, names: readonly string[]): string => {
    const shown = names.length === 0 ? ['<app>'] : names.map((name) => quote(name));
    return request.kind === 'plain' ? `run ${WATCH_APPS} read ${shown.join(' ')}` : `run ${WATCH_APPS} watch ${shown.join(' ')} in Monitor`;
};

const polling = (names: readonly string[], call: Call) => refusal(`Loop polls ${names.join(' ') || WATCH_APPS}, ${watchAction(call, names)}`);

const watch: Rule = (invocation, call) => {
    const { command, operands, path } = invocation;
    const names = path[0] === 'perform' || path[0] === 'set' ? [] : operands.map(({ text }) => text);
    const later = Array.dedupe(stages(invocation, call).later.map(({ command: stage }) => stage));
    const closing = Array.last(later).pipe(
        Option.filter(
            ({ pipes, stage }) =>
                !pipes &&
                Option.exists(
                    Option.zipWith(stage, command.stage, (end, own) => end.at - own.at),
                    (gap) => gap === later.length,
                ),
        ),
    );
    if (invocation.repeated && (path[0] === 'read' || path[0] === 'watch') && operands.every(({ known }) => known)) {
        return polling(names, call);
    }
    if (path[0] !== 'watch') {
        return [];
    }
    if (command.job && reaped(call)) {
        return [];
    }
    if (call.request.kind !== 'monitor') {
        return refusal(`${WATCH_APPS} watch never exits, ${watchAction(call, names)}`);
    }
    const fitted = invocation.has('--line-length') ? [] : inserted(invocation, Array.lastNonEmpty(invocation.words).end, ' --line-length 500');
    return command.pipes
        ? Option.match(closing, {
              onNone: () => refusal(`Filter drops ${WATCH_APPS} lines, run it unfiltered`),
              onSome: ({ end }) => [...fitted, ...erased(invocation, { start: command.end, end }, 'Filter')],
          })
        : fitted;
};

const polledApp: Rule = (invocation, call) => {
    const pids = [...invocation.operands.map(({ text }) => text), ...invocation.values('-p')].flatMap((value) => value.split(',')).filter((pid) => DIGITS.test(pid));
    return !invocation.repeated || pids.length === 0
        ? []
        : Effect.orElseSucceed(
              Effect.map(call.host.installations(pids), (running) => {
                  const bundles = running.filter(({ activationPolicy }) => activationPolicy !== 'prohibited').map(({ identifier }) => identifier);
                  return bundles.length === 0 ? [] : polling(bundles, call);
              }),
              () => [],
          );
};

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

const taskNotice = (ids: readonly string[]) => refusal(`Task ${ids.join(' ')} notifies on completion, never poll its output`);

const closesInput = ({ row: { closesInput: closes }, has }: Invocation): boolean => closes === true || (closes !== undefined && has(...closes));

const timer = (invocation: Invocation, { request }: Call): readonly Verdict[] => (request.kind === 'foreground' && invocation.last ? [Verdict.Background()] : []);

const sleep: Rule = (invocation, call) => {
    const { command, operands } = invocation;
    const { kind } = call.request;
    const alone = command.origin === 'call' && invocation.outer.length === 0 && !command.looping && !command.fed && !command.pipes;
    const driving = call.analysis.invocations.some(({ program, command: other }) => [WATCH_APPS, 'open', 'xed'].includes(program) && !other.looping && !other.job);
    const instant = operands.every(({ text }) => Number(text) === 0);
    if (alone && (instant || (driving && (kind === 'foreground' || kind === 'plain')))) {
        return edited(invocation, command, ':', spelled(invocation.words));
    }
    return instant ? [] : timer(invocation, call);
};

const neverExits: Rule = (invocation, call) => {
    if (call.request.kind !== 'foreground' || !invocation.last || (invocation.command.job && reaped(call)) || stages(invocation, call).later.some(closesInput)) {
        return [];
    }
    return invocation.outer.some(({ program }) => program === 'timeout') ? timer(invocation, call) : refusal(`${invocation.program} never exits, run it in Monitor`);
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

const tailedTask: Rule = (invocation, call) => {
    const { host, request } = call;
    const files = invocation.inputs.map(({ text }) => text).filter((file) => taskIds(file).length > 0);
    if (request.kind !== 'monitor' || files.length === 0 || stages(invocation, call).later.some(closesInput)) {
        return [];
    }
    const notice = taskNotice(taskIds(files.join(' ')));
    return Effect.all([host.parents, Effect.forEach(files, (file) => host.output(['lsof', '-t', file]), { concurrency: 'unbounded' })]).pipe(
        Effect.map(([parents, held]) => {
            const writers = held.flatMap((pids) => {
                const holders = pids.trim().split('\n');
                return holders.filter((pid) => parents.has(pid) && !holders.includes(`${parents.get(pid)}`)).slice(0, 1);
            });
            const awake = writers.map((pid) => `caffeinate -w ${pid}`).join('; ');
            return writers.length < files.length ? notice : [...inserted(invocation, invocation.head.start, '{ '), ...inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` & ${awake}; kill $!; }`)];
        }),
        Effect.orElseSucceed(() => notice),
    );
};

const polledTask = Effect.fnUntraced(function* ({ analysis, host, request }: Call) {
    const own = analysis.invocations.filter(({ command }) => command.origin === 'call');
    const readers = own.flatMap((invocation) => {
        const held = [...invocation.words, ...Array.getSomes(invocation.command.redirects.map(({ target }) => target))].filter(({ text }) => taskIds(text).length > 0);
        return held.length === 0 ? [] : [{ invocation, files: Array.getSomes(held.map(({ text }) => host.resolve(text, invocation.command.cwd))) }];
    });
    const ids = taskIds(readers.flatMap(({ files }) => files).join(' '));
    const tailed = request.kind === 'monitor' && readers.every(({ invocation }) => invocation.program === 'tail' && invocation.has('-f', '-F'));
    if (ids.length === 0 || tailed) {
        return [];
    }
    const held = yield* Effect.forEach(
        Array.dedupe(readers.flatMap(({ files }) => files)),
        (file) =>
            Effect.match(host.output(['lsof', '-t', file]), {
                onFailure: () => true,
                onSuccess: (pids) => pids.trim() !== '',
            }),
        { concurrency: 'unbounded' },
    );
    const waits = own.some(({ program, repeated, has }) => repeated || WAITERS.has(program) || (program === 'read' && has('-t')));
    if (!held.some(Boolean)) {
        return [];
    }
    const limiter = yield* RateLimiter.RateLimiter;
    const reader = `${request.session ?? host.parent} ${request.agent ?? 'main'}`;
    const first = (key: string) => Effect.isSuccess(limiter.consume({ key, limit: 1, window: '10 minutes', algorithm: 'fixed-window', onExceeded: 'fail' }));
    const judged = request.call === undefined || (yield* first(`${request.call} tasks`));
    const fresh = yield* Effect.forEach(ids, (id) => (judged ? first(`${reader} task ${id}`) : Effect.succeed(true)));
    return waits || !fresh.every(Boolean) ? taskNotice(ids) : [];
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

const idleCall = ({ analysis: { invocations } }: Call): readonly Verdict[] =>
    invocations.length > 0 &&
    invocations.every(({ program, words, command }) => (IDLE.has(program) || (program === 'date' && command.captured)) && command.redirects.length === 0 && words.every(({ parts }) => parts.every(({ known, glob, parameter, raw }) => parameter === undefined && !glob && (known || CAPTURE.test(raw)))))
        ? refusal('Call prints literal text and runs nothing, end the turn until a notice arrives')
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
        wait4path: { aliases: ['watch'], rules: [neverExits] },
        timeout: { leading: 1, wraps: [[]] },
        caffeinate: { wraps: [[]], rules: [unwrap, when(({ has }) => has('-t'), timer), when(({ has }) => !has('-t'), neverExits)] },
        read: { leading: Number.POSITIVE_INFINITY, rules: [stalledRead] },
        pgrep: { rules: [(invocation) => (invocation.has('--full') ? selfMatch(invocation, invocation.operands[0]) : [])] },
        gh: { rules: [polledRun, watchedRun] },
        grep: { closesInput: ['--quiet', '--silent', '--max-count', '--files-with-matches'], rules: [psFilter] },
        rg: { closesInput: ['--quiet', '--max-count', '--files-with-matches'] },
        head: { closesInput: true },
        kill: { rules: [polledApp] },
        ps: { rules: [polledApp] },
        tail: { flags: ['-F', '-b='], reads: true, rules: [when(({ has }) => has('-f', '-F'), tailedTask, neverExits)] },
        [WATCH_APPS]: { dumpsHelp: true, rules: [watch] },
    },
    every: [(invocation) => (invocation.command.busy ? refusal('Loop body never waits and spins a core, put sleep <seconds> in it') : [])],
    calls: [backgroundJob, pipedStatus, zshParameter, idleCall, polledTask],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { WAITS };
