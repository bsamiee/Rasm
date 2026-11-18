import { constants } from 'node:os';
import { basename } from 'node:path';
import { fileURLToPath } from 'node:url';
import type { Installation } from '@rasm/applications';
import { Array, Effect, Option, Predicate, Record, String } from 'effect';
import { Verdict } from '#/rules/decision';
import { type Call, edited, ensure, inserted, quote, respell, type Rule, type Ruleset, refusal, refuse, settled, spelled, stages, when, whole, within } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';
import { WATCH_APP } from '#/shell/programs';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/[^/]+$/u;
const ADDRESSED = /^(?:tell|(?<command>\w+)) (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const LAUNCHING = /^(?:activate|launch|run|reopen|open (?:POSIX file )?"(?<path>[^"]+)")$/u;
const SHELL_RUN = /\bdo shell script\b/u;
const SHELL_SCRIPT = /^do shell script "(?<body>(?:[^"\\]|\\.)*)"$/u;
const APPLESCRIPT_ESCAPE = /\\(?<escaped>["\\])/gu;
const FOCUS = /\bactivate\b|\bset\s+frontmost\b|AXRaise/u;
const SYSTEM_EVENTS = /System Events|com\.apple\.systemevents/u;
const WEB_URL = /^https?:/iu;
const BROWSER_NAME = /^[a-z][\w-]*$/u;
const VERBS = {
    'install-browser': undefined,
    list: undefined,
    'close-all': "closes every agent's sessions, use playwright cli -s=<session> close",
    'kill-all': "kills every agent's browsers, use playwright cli -s=<session> close",
    install: 'writes a .playwright workspace in the working folder, use playwright cli install-browser <browser>',
} as const;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const SIGNAL = /^-(?:\d+|(?:SIG)?[A-Z]{2,}\d*)$/u;
const PIDS = /^\d+/gmu;
const ENDING = new Set(['TERM', 'KILL', `${constants.signals.SIGTERM}`, `${constants.signals.SIGKILL}`]);
const OBSERVED: readonly Verdict[] = [Verdict.Observe()];

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [LAUNCHES]

const installed = (call: Call, names: readonly string[]) => Effect.orElseSucceed(call.host.installations(names), (): readonly Installation[] => []);

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
    [WATCH_APP, 'launch', ...subjects, ...(documents.length > 0 ? ['--document', ...documents] : []), ...(environment.length > 0 ? ['--environment', ...environment] : []), ...(args.length > 0 ? ['--arguments', ...args] : [])].join(' ');

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
            const session = call.request.agent === undefined ? [] : [`-s=${quote(call.request.agent)}`];
            const opened = ['playwright', 'cli', ...session, 'open', url.raw].join(' ');
            return documents.length === 1 && waited === undefined ? whole(invocation, opened, `${program} ${url.raw}`) : refusal(`${program} ${url.raw} opens a browser, use ${opened}`);
        }
        if (subjects.length + documents.length === 0) {
            return [];
        }
        if (waited !== undefined) {
            return refusal(`${program} ${waited} waits, use ${WATCH_APP} launch --wait ${target}`);
        }
        if (foreign.size > 0) {
            return refusal(`${program} ${[...foreign].join(' ')} opens ${target}, use ${WATCH_APP} launch ${target}`);
        }
        const found = app === undefined ? [] : yield* installed(call, [Option.getOrElse(bundle, () => app.text)]);
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
    const script = invocation.has('-e') ? undefined : invocation.operands.find(({ text }) => text !== '-');
    const file = Option.flatMap(Option.fromNullishOr(script), ({ text, known }) => Option.filter(call.host.resolve(text, invocation.command.cwd), () => known));
    if (script === undefined) {
        return statementsOf(invocation, call, [...invocation.values('-e'), ...Option.toArray(invocation.command.input)]);
    }
    return Option.match(file, {
        onNone: () => refusal(`osascript ${script.raw} expands later, pass a literal script path`),
        onSome: (path) => Effect.flatMap(Effect.option(call.host.output(['osadecompile', path])), Option.match({ onNone: () => Effect.succeed(refusal(`osascript ${script.raw} does not decompile, pass -e lines`)), onSome: (text) => settled(statementsOf(invocation, call, [text])) })),
    });
};

const statementsOf = (invocation: Invocation, call: Call, lines: readonly string[]): ReturnType<Rule> => {
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
    const shell = statements.length === 1 ? Option.fromNullishOr(SHELL_SCRIPT.exec(lines.join('\n').trim())?.groups?.['body']) : Option.none();
    if (lines.length === 0) {
        return refusal('osascript stdin hides its statements, pass -e lines');
    }
    if (lines.some((line) => SYSTEM_EVENTS.test(line))) {
        return refusal(`System Events posts to the frontmost app, use ${WATCH_APP} perform`);
    }
    if (Option.isSome(shell) && invocation.command.origin === 'call' && invocation.outer.length === 0) {
        return whole(invocation, shell.value.replaceAll(APPLESCRIPT_ESCAPE, '$<escaped>'), 'osascript do shell script');
    }
    if (lines.some((line) => SHELL_RUN.test(line))) {
        return refusal('osascript runs a shell command, run it directly');
    }
    if (statements.length > 0 && statements.every(({ app, launches }) => app !== undefined && launches) && (documents.length === 0 || named.length === 1)) {
        return Effect.map(installed(call, named), (found) => [...whole(invocation, launchLine(quoted, documents, [], switchesOf(found, [], call)), 'osascript'), ...OBSERVED]);
    }
    return lines.some((line) => FOCUS.test(line)) ? refusal(`osascript takes focus, use ${WATCH_APP} capture or perform`) : [];
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
                const found = yield* installed(call, [bundle]);
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
    const switches =
        invocation.path[0] === 'launch'
            ? switchesOf(
                  yield* installed(
                      call,
                      invocation.operands.map(({ text }) => text),
                  ),
                  given,
                  call,
              )
            : [];
    return switches.length === 0 ? [] : inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` ${[...(invocation.has('--arguments') ? [] : ['--arguments']), ...switches].join(' ')}`);
});

// --- [BROWSERS]

const browserCli: Rule = (invocation, { request: { agent } }) => {
    const [cli, verb] = invocation.operands;
    const named = verb?.text ?? '';
    const stated = agent === undefined || cli === undefined || invocation.has('-s', '--session') || [...invocation.outer, invocation].some(({ assigned }) => assigned.some(({ text }) => text.startsWith('PLAYWRIGHT_CLI_SESSION=')));
    const unlisted = [...(stated ? [] : inserted(invocation, cli.end, ` -s=${quote(agent)}`)), ...(named === 'attach' ? respell({ renames: { '--cdp': '--extension' }, only: BROWSER_NAME, notify: true })(invocation) : [])];
    return cli?.text === 'cli' ? Option.match(Record.get(named)(VERBS), { onNone: () => unlisted, onSome: (reason) => (reason === undefined ? [] : refusal(`playwright cli ${named} ${reason}`)) }) : [];
};

// --- [KILLS]

const ownPids = Effect.fnUntraced(
    function* (host: Call['host'], queries: readonly (readonly string[])[]) {
        const answers = yield* Effect.forEach(queries, (query) => host.output(['pgrep', ...query]), { concurrency: 'unbounded' });
        const pids: readonly string[] = answers.join('\n').match(PIDS) ?? [];
        const parents = yield* host.parents;
        const descends = (pid: string | undefined): boolean => pid === host.parent || (pid !== undefined && Number(pid) > 1 && descends(parents.get(pid)));
        return Option.liftPredicate(pids, () => pids.length > 0 && pids.every((pid) => descends(parents.get(pid))));
    },
    (effect) => Effect.orElseSucceed(effect, () => Option.none<readonly string[]>()),
);

const killsFound: Rule = (invocation, call) => {
    const { command, values, words, names } = invocation;
    const signal = (values('-s', '-n')[0] ?? (names.size > 0 ? words[1]?.text : undefined) ?? 'TERM').toUpperCase().replace(SIGNAL_PREFIX, '');
    const piped = command.fed ? stages(invocation, call).earlier : [];
    const finder = [...piped, ...within(invocation, call)].find(({ program }) => ['ps', 'pgrep', 'pidof'].includes(program));
    if (!ENDING.has(signal) || finder === undefined) {
        return [];
    }
    const refused = refusal(`${finder.program} feeds kill every match, kill one pid`);
    const query = finder.words.slice(1);
    if (finder.program !== 'pgrep' || !query.every(({ known }) => known) || command.origin !== 'call') {
        return refused;
    }
    const kill = ['kill', ...invocation.args.filter(({ names: flags }) => flags.length > 0).map(({ words: held }) => spelled(held))].join(' ');
    return Effect.map(
        ownPids(call.host, [query.map(({ text }) => text)]),
        Option.match({
            onNone: () => refused,
            onSome: (pids) => edited(invocation, { start: piped.includes(finder) ? finder.command.start : invocation.head.start, end: Array.lastNonEmpty(invocation.words).end }, [kill, ...pids].join(' '), `${finder.program} | kill`),
        }),
    );
};

const killsByName: Rule = (invocation, { host }) => {
    const { program, words } = invocation;
    const signal = words.slice(1, 2).find(({ text }) => SIGNAL.test(text));
    const rest = words.slice(signal === undefined ? 1 : 2);
    const flags = rest.map(({ text }) => text).filter((text) => text.startsWith('-'));
    const queries = program === 'pkill' ? [rest.map(({ text }) => text)] : invocation.operands.map(({ text }) => [...(flags.includes('-m') ? [] : ['-x']), text]);
    const refused = refusal(`${program} ends every match, kill one pid`);
    return rest.every(({ known }) => known) && (program === 'pkill' || flags.every((flag) => flag === '-m' || flag === '-v'))
        ? Effect.map(ownPids(host, queries), Option.match({ onNone: () => refused, onSome: (pids) => whole(invocation, ['kill', ...Option.toArray(Option.map(Option.fromNullishOr(signal), ({ raw }) => raw)), ...pids].join(' '), program) }))
        : refused;
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const APPS: Ruleset = {
    programs: {
        open: { restAfter: ['--args'], rules: [opens([], ['--wait', '--wait-apps', '--stdin', '--stdout', '--stderr'], ['--app', '--bundle', '--new', '--hide', '--background', '-j', '--fresh', '--env', '--url', '--args'])] },
        xed: { flags: ['-l, --line='], informational: ['-v'], rules: [opens(['com.apple.dt.Xcode'], ['-w', '--wait'], ['-x', '--launch', '-b', '--background'])] },
        osascript: { rules: [appleScript] },
        qlmanage: { rules: [refuse('opens a preview window', 'use qlmanage -t', '-p')] },
        screencapture: { rules: [respell({ drops: ['--interactive', '--screen', '--window', '-W', '--style', '-U', '--ui', '-P', '-M', '--bundleid', '--defaults', '--display-errors'], notify: true }), refuse("replaces the owner's clipboard", 'pass a file', '-c')] },
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
        playwright: { flags: ['-s, --session=', '--cdp=', '--extension='], rules: [browserCli] },
        [WATCH_APP]: { restAfter: ['--arguments'], rules: [() => OBSERVED, launchSwitches] },
    },
    every: [bundleExecutable],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS };
