import { Array, Duration, Effect, Option, Record, String } from 'effect';
import { Verdict } from '#/rules/decision';
import { absent, type Call, edited, erased, inserted, quote, type Rule, type Ruleset, refusal, silent, spelled, stages, when, within } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';
import { WATCH_APPS } from '#/shell/programs';
import type { Word } from '#/shell/script';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTER = /^\w/u;
const DIGITS = /^\d+$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output(?![^\s'"])/gu;
const BLANK_LINE = /^\s*(?:#.*)?$/u;
const LIMIT = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smhd])?$/u;
const LIMIT_UNITS: Readonly<Record<string, (amount: number) => Duration.Duration>> = { s: Duration.seconds, m: Duration.minutes, h: Duration.hours, d: Duration.days };

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
        : Effect.map(call.host.installations(pids), (running) => {
              const bundles = running.filter(({ activationPolicy }) => activationPolicy !== 'prohibited').map(({ identifier }) => identifier);
              return bundles.length === 0 ? [] : polling(bundles, call);
          });
};

// --- [TIMERS]

const taskIds = (text: string): readonly string[] => Array.dedupe(Array.getSomes(Array.fromIterable(text.matchAll(TASK_OUTPUTS)).map(({ groups }) => Option.flatMap(Option.fromNullishOr(groups), Record.get('id')))));

const taskNotice = (ids: readonly string[]) => refusal(`Task ${ids.join(' ')} notifies on completion, continue until its notice`);

const closesInput = ({ row: { closesInput: closes }, has }: Invocation): boolean => closes === true || (closes !== undefined && has(...closes));

const timer: Rule = (invocation, { request }) => {
    const tasks = taskIds(request.command);
    if (tasks.length > 0 && invocation.last) {
        return taskNotice(tasks);
    }
    return request.kind === 'foreground' && invocation.last ? [Verdict.Background()] : [];
};

const sleep: Rule = (invocation, call) => {
    const { command, operands } = invocation;
    const { kind } = call.request;
    const alone = command.origin === 'call' && invocation.outer.length === 0 && !command.looping && !command.fed && !command.pipes;
    const driving = call.analysis.invocations.some(({ program, command: other }) => [WATCH_APPS, 'open', 'xed'].includes(program) && !other.looping);
    const instant = operands.every(({ text }) => Number(text) === 0);
    if (alone && (instant || (driving && (kind === 'foreground' || kind === 'plain')))) {
        return edited(invocation, command, ':', spelled(invocation.words));
    }
    return instant ? [] : timer(invocation, call);
};

const neverExits: Rule = (invocation, call) => (call.request.kind === 'foreground' && invocation.last && !invocation.outer.some(({ program }) => program === 'timeout') && !stages(invocation, call).later.some(closesInput) ? refusal(`${invocation.program} never exits, run it in Monitor`) : []);

const unwrap = (timeout?: number) => (invocation: Invocation) => Option.match(Array.head(invocation.rest), { onNone: () => [], onSome: ({ start }) => erased(invocation, { start: invocation.head.start, end: start }, spelled(invocation.words), timeout) });

const timeoutLimit: Rule = Effect.fnUntraced(function* (invocation, call) {
    const { analysis, request, host } = call;
    const { value, unit = 's' } = Option.getOrElse(
        Option.flatMapNullishOr(
            Option.flatMap(Array.head(invocation.leading), ({ text }) => String.match(LIMIT)(text)),
            ({ groups }) => groups,
        ),
        Record.empty<string, string>,
    );
    const limit = Duration.toMillis(LIMIT_UNITS[unit]?.(Number(value)) ?? Duration.zero);
    const fallback = Option.map(yield* host.environment('BASH_DEFAULT_TIMEOUT_MS'), Number);
    const onlyQuick = analysis.invocations.filter(({ command: { origin, stage }, outer }) => origin === 'call' && outer.length === 0 && !Option.exists(stage, ({ at }) => at > 0)).every(({ program }) => program === 'timeout' || program === 'cd');
    const applies = request.kind === 'foreground' && request.timeout === undefined && !invocation.repeated && (onlyQuick || Option.exists(fallback, (milliseconds) => limit > milliseconds));
    return unwrap(applies && limit > 0 ? limit : undefined)(invocation);
});

const leakingRead: Rule = (invocation, call) => {
    const [amount] = invocation.values('-t');
    return amount !== undefined && invocation.command.fed && within(invocation, call).some(({ command }) => command.substituted) ? silent(edited(invocation, invocation.command, `sleep ${amount}`, `read -t ${amount}`)) : [];
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
    return Effect.map(Effect.all([host.parents, Effect.forEach(files, (file) => host.output(['lsof', '-t', file]), { concurrency: 'unbounded' })]), ([parents, held]) => {
        const writers = held.flatMap((pids) => {
            const holders = pids.trim().split('\n');
            return holders.filter((pid) => parents.has(pid) && !holders.includes(`${parents.get(pid)}`)).slice(0, 1);
        });
        const awake = writers.map((pid) => `caffeinate -w ${pid}`).join('; ');
        return writers.length < files.length ? taskNotice(taskIds(files.join(' '))) : [...inserted(invocation, invocation.head.start, '{ '), ...inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` & ${awake}; kill $!; }`)];
    });
};

// --- [JOBS]

const backgroundJob = ({ analysis, request }: Call): readonly Verdict[] => {
    const [job, ...more] = analysis.jobs;
    if (job === undefined || analysis.invocations.some(({ program }) => program === 'wait' || program === 'kill')) {
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
    return [Verdict.Edit({ start: job.start, end: job.end, text: Option.none(), subject: Option.none(), timeout: Option.none() }), ...(request.kind === 'foreground' ? [Verdict.Background()] : [])];
};

const pipedStatus = (call: Call): readonly Verdict[] => {
    const own = call.analysis.invocations.filter(({ command: { origin } }) => origin === 'call');
    const set = own.some(({ program, values }) => program === 'set' && values('-o').includes('pipefail'));
    const hidden = own.find(({ row, last, command: { pipes } }) => pipes && last && (row.pipefail === true || row.nx !== undefined));
    const closing = own.some((invocation) => invocation.command.fed && closesInput(invocation));
    return hidden !== undefined && !set && !closing ? inserted(hidden, 0, 'set -o pipefail; ') : [];
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const WAITS: Ruleset = {
    programs: {
        sleep: { rules: [sleep] },
        wait4path: { aliases: ['watch'], rules: [neverExits] },
        timeout: { leading: 1, wraps: [[]], rules: [absent(timeoutLimit)] },
        caffeinate: { wraps: [[]], rules: [unwrap(), when(({ has }) => has('-t'), timer), when(({ has }) => !has('-t'), neverExits)] },
        read: { leading: Number.POSITIVE_INFINITY, reads: true, readsUnless: ['-u'], rules: [leakingRead, when(({ has }) => has('-t'), timer)] },
        pgrep: { rules: [(invocation) => (invocation.has('--full') ? selfMatch(invocation, invocation.operands[0]) : [])] },
        grep: { closesInput: ['--quiet', '--silent', '--max-count', '--files-with-matches'], rules: [psFilter] },
        rg: { closesInput: ['--quiet', '--max-count', '--files-with-matches'] },
        head: { closesInput: true },
        kill: { rules: [polledApp] },
        ps: { rules: [polledApp] },
        tail: { flags: ['-F', '-b='], reads: true, rules: [when(({ has }) => has('-f', '-F'), tailedTask, neverExits)] },
        [WATCH_APPS]: { dumpsHelp: true, rules: [watch] },
    },
    every: [(invocation, call) => (call.request.kind === 'foreground' && invocation.command.busy ? refusal('Loop spins in the foreground, use Monitor or run_in_background') : [])],
    calls: [backgroundJob, pipedStatus],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { WAITS };
