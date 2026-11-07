import { Array, Duration, Effect, Option, type Predicate } from 'effect';
import { Verdict } from '#/decision.ts';
import { appOf, WATCHER } from '#/instances.ts';
import { absent, all, BACKGROUNDED, type Call, dropped, edited, type Fix, has, inserted, type Policy, quote, refusal, silent, spoken, stages, unwrap, type Verdicts, when, within } from '#/policies/fix.ts';
import type { Word } from '#/shell/script.ts';
import type { Site } from '#/shell/site.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LETTER = /^\w/u;
const DIGITS = /^\d+$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output(?![^\s'"])/gu;
const BLANK_LINE = /^\s*(?:#.*)?$/u;
const SPACES = /\s+/u;
const LIMIT = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smhd])?$/u;
const LIMIT_UNITS: Readonly<Record<string, (amount: number) => Duration.Duration>> = { s: Duration.seconds, m: Duration.minutes, h: Duration.hours, d: Duration.days };
const DRIVING = new Set([WATCHER, 'open', 'xed']);
const QUICK = new Set(['timeout', 'cd']);
const REAPING = new Set(['wait', 'kill']);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [WATCHING]

const watchAction = ({ request }: Call, names: readonly string[]): string => {
    const shown = names.length === 0 ? ['<app>'] : names.map(quote);
    return request.kind === 'plain' ? `run ${WATCHER} read ${shown.join(' ')}` : `run ${WATCHER} watch ${shown.join(' ')} in Monitor`;
};

const polling = (names: readonly string[], call: Call): Verdicts => refusal(`Loop polls ${names.join(' ') || WATCHER}, ${watchAction(call, names)}`);

const watching: Fix = (site, call) => {
    const { command, operands, path } = site;
    const names = path[0] === 'perform' || path[0] === 'set' ? [] : operands.map(({ text }) => text);
    const later = Array.dedupe(stages(site, call).later.map(({ command: stage }) => stage));
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
    if (site.repeated && (path[0] === 'read' || path[0] === 'watch') && operands.every(({ known }) => known)) {
        return polling(names, call);
    }
    if (path[0] !== 'watch') {
        return [];
    }
    if (call.request.kind !== 'monitor') {
        return refusal(`${WATCHER} watch never exits, ${watchAction(call, names)}`);
    }
    return command.pipes
        ? Option.match(closing, {
              onNone: () => refusal(`Filter drops ${WATCHER} lines, run it unfiltered`),
              onSome: ({ end }) => edited(site, { start: command.end, end }, '', 'Filter'),
          })
        : [];
};

// --- [TIMERS]

const taskIds = (text: string): readonly string[] => [...new Set(Array.fromIterable(text.matchAll(TASK_OUTPUTS)).map(({ groups }) => `${groups?.id}`))];

const taskNotice = (ids: readonly string[]): Verdicts => refusal(`Task ${ids.join(' ')} notifies on completion, continue until its notice`);

const timed: Fix = (site, call) => {
    const { command, operands, last } = site;
    const { kind, command: source } = call.request;
    const alone = command.origin === 'call' && site.outer.length === 0 && !command.looping && !command.fed && !command.pipes;
    const driving = call.analysis.sites.some(({ program, command: other }) => DRIVING.has(program) && !other.looping);
    const tasks = taskIds(source);
    const instant = site.program === 'sleep' && operands.every(({ text }) => Number(text) === 0);
    if (site.program === 'sleep' && alone && (instant || (driving && (kind === 'foreground' || kind === 'plain')))) {
        return dropped(site, spoken(site.words));
    }
    if (instant) {
        return [];
    }
    if (tasks.length > 0 && last) {
        return taskNotice(tasks);
    }
    return kind === 'foreground' && last ? BACKGROUNDED : [];
};

const endless: Predicate.Predicate<Site> = ({ on }) => on('endless') && !on('timer');

const leakingRead: Fix = (site, call) => {
    const [amount] = site.values('-t');
    return amount !== undefined && site.command.fed && within(site, call).some(({ command }) => command.substituted) ? silent(edited(site, site.command, `sleep ${amount}`, `read -t ${amount}`)) : [];
};

const selfMatch = (site: Site, word: Word | undefined): Verdicts => (word === undefined || !word.known || !LETTER.test(word.text) ? [] : edited(site, word, quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`)));

const polledApp: Fix = (site, call) => {
    const pids = [...site.operands.map(({ text }) => text), ...site.values('-p')].flatMap((value) => value.split(',')).filter((pid) => DIGITS.test(pid));
    return !site.repeated || pids.length === 0
        ? []
        : Effect.map(Effect.forEach(pids, appOf, { concurrency: 'unbounded' }), (found) => {
              const bundles = Array.getSomes(found)
                  .filter(({ foreground }) => foreground)
                  .map(({ bundle }) => bundle);
              return bundles.length === 0 ? [] : polling(bundles, call);
          });
};

const psFilter: Fix = (site, call) => {
    const fromPs = stages(site, call).earlier.some(({ program }) => program === 'ps');
    return fromPs && site.names.size === 0 && site.command.fed && site.outer.length === 0 ? selfMatch(site, site.leading[0]) : [];
};

const onlyTimed = ({ analysis: { sites } }: Call): boolean => sites.filter(({ command: { origin, stage }, outer }) => origin === 'call' && outer.length === 0 && !Option.exists(stage, ({ at }) => at > 0)).every(({ program }) => QUICK.has(program));

const limited: Fix = Effect.fnUntraced(function* (site: Site, call: Call) {
    const { value, unit = 's' } = LIMIT.exec(site.leading[0]?.text ?? '')?.groups ?? {};
    const limit = Duration.toMillis(LIMIT_UNITS[unit]?.(Number(value)) ?? Duration.zero);
    const fallback = Option.map(yield* call.host.environment('BASH_DEFAULT_TIMEOUT_MS'), Number);
    const applies = call.request.kind === 'foreground' && call.request.timeout === undefined && !site.repeated && (onlyTimed(call) || Option.exists(fallback, (held) => limit > held));
    return yield* all(unwrap(applies && limit > 0 ? limit : undefined))(site, call);
});

// --- [TASKS]

const writerOf = Effect.fnUntraced(function* (file: string, { host }: Call) {
    const holders = (yield* host.output(['lsof', '-t', file])).trim().split(SPACES);
    const parents = yield* host.parents;
    return Option.fromNullishOr(holders.find((pid) => parents.has(pid) && !holders.includes(`${parents.get(pid)}`)));
});

const closedLater = (site: Site, call: Call): boolean => stages(site, call).later.some(({ on }) => on('closes'));

const tailedTask: Fix = (site, call) => {
    const files = site.inputs.map(({ text }) => text).filter((file) => taskIds(file).length > 0);
    if (call.request.kind !== 'monitor' || files.length === 0 || closedLater(site, call)) {
        return [];
    }
    return Effect.map(
        Effect.forEach(files, (file) => writerOf(file, call), { concurrency: 'unbounded' }),
        (found): Verdicts => {
            const roots = Array.getSomes(found);
            return roots.length < files.length ? taskNotice(taskIds(files.join(' '))) : [...inserted(site, site.head.start, '{ '), ...inserted(site, Array.lastNonEmpty(site.words).end, ` & ${roots.map((pid) => `caffeinate -w ${pid}`).join('; ')}; kill $!; }`)];
        },
    );
};

// --- [SCRIPTS]

const jobbed = ({ analysis, request }: Call): Verdicts => {
    const [job, ...more] = analysis.jobs;
    if (job === undefined || analysis.sites.some(({ program }) => REAPING.has(program))) {
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
    return [Verdict.Edit({ start: job.start, end: job.end, text: '', notice: Option.none(), timeout: Option.none() }), ...(request.kind === 'foreground' ? BACKGROUNDED : [])];
};

const masked = ({ analysis: { sites } }: Call): Verdicts => {
    const own = sites.filter(({ command: { origin } }) => origin === 'call');
    const set = own.some(({ program, values }) => program === 'set' && values('-o').includes('pipefail'));
    const hidden = own.find(({ row, last, command: { pipes } }) => pipes && last && (row.reports === true || row.routes !== undefined));
    const closing = own.some(({ on, command: { fed } }) => fed && on('closes'));
    return hidden !== undefined && !set && !closing ? inserted(hidden, 0, 'set -o pipefail; ') : [];
};

// --- [TABLES] --------------------------------------------------------------------------

const WAITS: Policy = {
    programs: {
        sleep: { timer: true },
        wait4path: { aliases: ['watch'], endless: true },
        timeout: { leading: 1, wraps: [[]], fixes: [absent(limited)] },
        caffeinate: { wraps: [[]], timer: ['-t'], endless: true, fixes: [unwrap()] },
        read: { reads: 'always', readsUnless: ['-u'], timer: ['-t'], fixes: [leakingRead] },
        pgrep: { fixes: [when(has('--full'), (site) => selfMatch(site, site.operands[0]))] },
        grep: { fixes: [psFilter] },
        kill: { fixes: [polledApp] },
        ps: { fixes: [polledApp] },
        tail: { flags: ['-F', '-b='], reads: 'default', endless: ['-f', '-F'], fixes: [when(endless, tailedTask)] },
        [WATCHER]: { dump: true, fixes: [watching] },
    },
    every: [
        (site, call) => (call.request.kind === 'foreground' && site.command.busy ? refusal('Loop spins in the foreground, use Monitor or run_in_background') : []),
        when(({ on }) => on('timer'), timed),
        when(endless, (site, call) => (call.request.kind === 'foreground' && site.last && !site.outer.some(({ program }) => program === 'timeout') && !closedLater(site, call) ? refusal(`${site.program} never exits, run it in Monitor`) : [])),
    ],
    calls: [jobbed, masked],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { WAITS };
