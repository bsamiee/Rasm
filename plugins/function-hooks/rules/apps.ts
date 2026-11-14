import { constants } from 'node:os';
import { basename } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { Installation } from '@rasm/applications';
import { Array, Effect, Option, Predicate, Record, String } from 'effect';
import { Verdict } from '#/rules/decision';
import { type Call, ensure, inserted, perSubcommand, quote, type Rule, type Ruleset, refusal, refuse, stages, when, whole, within } from '#/rules/rule';
import { WATCH_APPS } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/[^/]+$/u;
const ADDRESSED = /^(?:tell|(?<command>\w+)) (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const LAUNCHING = /^(?:activate|launch|run|reopen|open (?:POSIX file )?"(?<path>[^"]+)")$/u;
const SHELL_SCRIPT = /\bdo shell script\b/u;
const FOCUS = /\bactivate\b|\bset\s+frontmost\b|AXRaise/u;
const SYSTEM_EVENTS = /System Events|com\.apple\.systemevents/u;
const WEB_URL = /^https?:/iu;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const SIGNAL = /^-(?:\d+|(?:SIG)?[A-Z]{2,}\d*)$/u;
const PIDS = /^\d+/gmu;
const ENDING = new Set(['TERM', 'KILL', `${constants.signals.SIGTERM}`, `${constants.signals.SIGKILL}`]);
const OBSERVED: readonly Verdict[] = [Verdict.Observe()];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LAUNCHES]

const switchesOf = (installations: readonly Installation[], given: readonly string[], { programs }: Call): readonly string[] => {
    const passed = new Set(given);
    return Array.dedupe(
        Array.getSomes(installations.map(({ executable }) => Option.flatMapNullishOr(executable, ({ href }) => programs.get(basename(fileURLToPath(href))))))
            .filter(({ headless = [] }) => passed.isDisjointFrom(new Set(headless)))
            .flatMap(({ switches = [] }) => switches)
            .filter((flag) => !passed.has(flag)),
    );
};

const launchLine = (subjects: readonly string[], documents: readonly string[], environment: readonly string[], args: readonly string[]): string =>
    [WATCH_APPS, 'launch', ...subjects, ...(documents.length > 0 ? ['--document', ...documents] : []), ...(environment.length > 0 ? ['--environment', ...environment] : []), ...(args.length > 0 ? ['--arguments', ...args] : [])].join(' ');

const opens = (named: readonly string[], waits: readonly string[], allowed: readonly string[]): Rule =>
    Effect.fnUntraced(function* (invocation, call) {
        const { program, rest, operands } = invocation;
        const [app] = [...invocation.supplied('--app', '--bundle'), ...operands.filter(({ text }) => text.endsWith('.app') && !URL.canParse(text))];
        const bundle = Option.flatMap(Option.fromNullishOr(app), (word) => Option.filter(call.host.resolve(word.text, invocation.command.cwd), () => word.known && (word.text.includes('/') || operands.includes(word))));
        const subjects = [...named, ...Option.toArray(Option.orElse(Option.map(bundle, quote), () => Option.map(Option.fromNullishOr(app), ({ raw }) => raw)))];
        const target = subjects.join(' ') || '<app>';
        const documents = [...operands.filter((word) => word !== app), ...invocation.supplied('--url')];
        const url = documents.find(({ text }) => WEB_URL.test(text));
        const [waited] = invocation.names.intersection(new Set(waits));
        const foreign = invocation.names.difference(new Set([...allowed, ...waits]));
        if (url !== undefined) {
            const playwright = Option.exists(invocation.command.cwd, (folder) => call.host.inside(folder, call.host.root)) ? 'playwright' : `mise -C ${quote(call.host.root)} exec -- playwright`;
            return refusal(`${program} ${url.raw} opens a browser, use ${playwright} cli open ${url.raw}`);
        }
        if (subjects.length + documents.length === 0) {
            return [];
        }
        if (waited !== undefined) {
            return refusal(`${program} ${waited} waits, run ${WATCH_APPS} watch ${target} in Monitor`);
        }
        if (foreign.size > 0) {
            return refusal(`${program} ${[...foreign].join(' ')} opens ${target}, use ${WATCH_APPS} launch ${target}`);
        }
        const found = app === undefined ? [] : yield* call.host.installations([Option.getOrElse(bundle, () => app.text)]);
        const environment = invocation.supplied('--env').map(({ raw }) => raw);
        const passed = rest.map(({ text }) => text);
        const [first, ...more] = found;
        const headless = Option.fromUndefinedOr(first).pipe(
            Option.flatMap(({ executable }) => executable),
            Option.map(({ href }) => fileURLToPath(href)),
            Option.filter((executable) => more.length === 0 && !new Set(passed).isDisjointFrom(new Set(call.programs.get(basename(executable))?.headless))),
        );
        if (found.length > 0 && !found.some(({ activationPolicy }) => activationPolicy === 'regular')) {
            return [];
        }
        return Option.match(headless, {
            onNone: () => [
                ...whole(
                    invocation,
                    launchLine(
                        [...subjects, ...(invocation.has('--new') ? ['--new'] : [])],
                        documents.map(({ raw }) => raw),
                        environment,
                        [...rest.map(({ raw }) => raw), ...switchesOf(found, passed, call)],
                    ),
                    program,
                ),
                ...OBSERVED,
            ],
            onSome: (executable) => whole(invocation, [...environment, quote(executable), ...rest.map(({ raw }) => raw)].join(' '), program),
        });
    });

const appleScript: Rule = (invocation, call) => {
    const lines = [...invocation.values('-e'), ...Option.toArray(invocation.command.input)];
    const [, parsed] = Array.mapAccum(
        (invocation.values('-l').includes('JavaScript') ? [] : lines)
            .flatMap((value) => value.split('\n'))
            .map((line) => line.trim())
            .filter(Predicate.isTruthy),
        [],
        (tells: readonly string[], line): readonly [readonly string[], readonly { readonly app: string | undefined; readonly launches: boolean; readonly path: string | undefined }[]] => {
            const { app, command, body = command } = ADDRESSED.exec(line)?.groups ?? {};
            if (line === 'end tell') {
                return [tells.slice(0, -1), []];
            }
            if (app !== undefined && body === undefined) {
                return [[...tells, app], []];
            }
            const launching = Option.fromNullishOr(LAUNCHING.exec(body ?? line));
            return [
                tells,
                [
                    {
                        app: app ?? tells.at(-1),
                        launches: Option.isSome(launching),
                        path: Option.getOrUndefined(
                            Option.flatMap(
                                Option.flatMapNullishOr(launching, ({ groups }) => groups),
                                Record.get('path'),
                            ),
                        ),
                    },
                ],
            ];
        },
    );
    const statements = parsed.flat();
    const named = [...new Set(statements.map(({ app }) => app).filter(Predicate.isNotUndefined))];
    const quoted = named.map((name) => quote(name));
    const documents = statements
        .map(({ path }) => path)
        .filter(Predicate.isNotUndefined)
        .map((path) => quote(path));
    const script = invocation.has('-e') ? undefined : invocation.operands.find(({ text }) => text !== '-');
    if (script !== undefined || lines.length === 0) {
        return refusal(`osascript ${script?.text ?? 'stdin'} hides its statements, pass -e lines`);
    }
    if (lines.some((line) => SYSTEM_EVENTS.test(line))) {
        return refusal(`System Events posts to the frontmost app, use ${WATCH_APPS} perform or mcp__cua-driver__press_key`);
    }
    if (lines.some((line) => SHELL_SCRIPT.test(line))) {
        return refusal('osascript runs a shell command, run it directly');
    }
    if (statements.length > 0 && statements.every(({ app, launches }) => app !== undefined && launches) && (documents.length === 0 || named.length === 1)) {
        return Effect.map(call.host.installations(named), (found) => [...whole(invocation, launchLine(quoted, documents, [], switchesOf(found, [], call)), 'osascript'), ...OBSERVED]);
    }
    return lines.some((line) => FOCUS.test(line)) ? refusal(`osascript takes focus, use ${WATCH_APPS} capture or perform`) : [];
};

const bundleExecutable: Rule = (invocation, call) => {
    const { host } = call;
    const { cwd } = invocation.command;
    const executable = Option.filter(host.resolve(invocation.head.text, cwd), () => !invocation.has(...(invocation.row.headless ?? [])));
    return Option.match(
        Option.all({
            head: executable,
            bundle: Option.flatMap(executable, String.match(BUNDLE)).pipe(
                Option.flatMapNullishOr(({ groups }) => groups),
                Option.flatMap(Record.get('bundle')),
            ),
        }),
        {
            onNone: () => [],
            onSome: Effect.fnUntraced(function* ({ head, bundle }) {
                const found = yield* host.installations([bundle]);
                const args = yield* Effect.forEach(invocation.words.slice(1), ({ raw, text, known }) => {
                    const file = Option.getOrUndefined(Option.filter(host.resolve(text, cwd), () => known && !text.startsWith('-') && !host.path.isAbsolute(text)));
                    return file === undefined ? Effect.succeed({ raw, text }) : Effect.map(host.exists(file), (exists) => ({ raw: exists ? quote(file) : raw, text }));
                });
                const runs = found.some(({ activationPolicy, executable: main }) => activationPolicy === 'regular' && Option.exists(main, ({ href }) => fileURLToPath(href) === head));
                const switches = switchesOf(
                    found,
                    args.map(({ text }) => text),
                    call,
                );
                return runs
                    ? [
                          ...whole(
                              invocation,
                              launchLine(
                                  [quote(bundle)],
                                  [],
                                  invocation.assigned.map(({ raw }) => raw),
                                  [...args.map(({ raw }) => raw), ...switches],
                              ),
                              invocation.program,
                              invocation.assigned[0],
                          ),
                          ...OBSERVED,
                      ]
                    : [];
            }),
        },
    );
};

const launchSwitches: Rule = Effect.fnUntraced(function* (invocation, call) {
    const given = [...invocation.values('--arguments'), ...invocation.rest.map(({ text }) => text)];
    const switches = invocation.path[0] === 'launch' ? switchesOf(yield* call.host.installations(invocation.operands.map(({ text }) => text)), given, call) : [];
    return switches.length === 0 ? [] : inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` ${[...(invocation.has('--arguments') ? [] : ['--arguments']), ...switches].join(' ')}`);
});

// --- [BROWSERS]

const browserSession: Rule = (invocation, { request: { agent } }) => {
    const [cli] = invocation.operands;
    const stated = invocation.has('-s', '--session') || [...invocation.outer, invocation].some(({ assigned }) => assigned.some(({ text }) => text.startsWith('PLAYWRIGHT_CLI_SESSION=')));
    return agent === undefined || cli?.text !== 'cli' || stated ? [] : inserted(invocation, cli.end, ` -s=${quote(agent)}`);
};

// --- [KILLS]

const killsFound: Rule = (invocation, call) => {
    const { command, values, words, names } = invocation;
    const signal = (values('-s', '-n')[0] ?? (names.size > 0 ? words[1]?.text : undefined) ?? 'TERM').toUpperCase().replace(SIGNAL_PREFIX, '');
    const finder = [...(command.fed ? stages(invocation, call).earlier : []), ...within(invocation, call)].find(({ program }) => ['ps', 'pgrep', 'pidof'].includes(program));
    return ENDING.has(signal) && finder !== undefined ? refusal(`${finder.program} feeds kill every match, kill one pid`) : [];
};

const killsByName: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { program, words } = invocation;
    const [signal] = words.slice(1, 2).filter(({ text }) => SIGNAL.test(text));
    const rest = words.slice(signal === undefined ? 1 : 2);
    const flags = rest.map(({ text }) => text).filter((text) => text.startsWith('-'));
    const queries = program === 'pkill' ? [rest.map(({ text }) => text)] : invocation.operands.map(({ text }) => [...(flags.includes('-m') ? [] : ['-x']), text]);
    const answers = rest.every(({ known }) => known) && (program === 'pkill' || flags.every((flag) => flag === '-m' || flag === '-v')) ? yield* Effect.forEach(queries, (query) => host.output(['pgrep', ...query]), { concurrency: 'unbounded' }) : [];
    const pids = answers.join('\n').match(PIDS) ?? [];
    const parents = yield* host.parents;
    const descends = (pid: string | undefined): boolean => pid === host.parent || (pid !== undefined && Number(pid) > 1 && descends(parents.get(pid)));
    return pids.length > 0 && pids.every((pid) => descends(parents.get(pid))) ? whole(invocation, ['kill', ...Option.toArray(Option.map(Option.fromNullishOr(signal), ({ raw }) => raw)), ...pids].join(' '), program) : refusal(`${program} ends every match, kill one pid`);
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const APPS: Ruleset = {
    programs: {
        open: { restAfter: ['--args'], rules: [opens([], ['--wait', '--wait-apps', '--stdin', '--stdout', '--stderr'], ['--app', '--bundle', '--new', '--hide', '--background', '-j', '--fresh', '--env', '--url', '--args'])] },
        xed: { flags: ['-l, --line='], informational: ['-v'], rules: [opens(['com.apple.dt.Xcode'], ['-w', '--wait'], ['-x', '--launch', '-b', '--background'])] },
        osascript: { rules: [appleScript] },
        qlmanage: { rules: [refuse('opens a preview window', 'use qlmanage -t', '-p')] },
        killall: { flags: ['-s', '-c=', '-t='], informational: ['-s', '-l'], rules: [when(({ has }) => !has('-0'), killsByName)] },
        pkill: { rules: [when(({ has }) => !has('-0'), killsByName)] },
        kill: { flags: ['-n='], rules: [killsFound] },
        Blender: {
            aliases: ['blender'],
            flags: ['-P, --python=', '--python-expr='],
            informational: ['-v', '--log-list-categories'],
            headless: ['-b', '--background', '-c', '--command'],
            switches: ['--no-window-focus'],
            rules: [when(({ has }) => has('--python', '--python-expr'), ensure(['--python-use-system-env']))],
        },
        Rhinoceros: { switches: ['-nosplash'] },
        'cua-driver': { rules: [perSubcommand(Record.fromIterableWith(['call', 'serve'], (subcommand) => [subcommand, [refuse('bypasses the session MCP server', 'call an mcp__cua-driver__ tool')]]))] },
        playwright: { flags: ['-s, --session='], rules: [browserSession] },
        [WATCH_APPS]: { restAfter: ['--arguments'], rules: [() => OBSERVED, launchSwitches] },
    },
    every: [bundleExecutable],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS };
