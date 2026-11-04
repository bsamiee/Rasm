import { Maybe } from '../../monads.ts';
import type { CommandRequest, System } from '../command-decision.ts';
import { type Command, runJson, type Word } from './command.ts';
import { argumentGroups, type Invocation, informational, operands, programRow, resolvePath } from './invocation.ts';
import { type Call, invocationsOf, nestedIn, pipelineNeighbors, quote, type Route, refuse, type Verdict } from './verdict.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Statement = { readonly kind: (typeof STATEMENTS)[number][0] | 'drive' } | { readonly kind: 'open'; readonly path: string } | { readonly kind: 'shell'; readonly inner: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCHER = 'watch-apps';
const ARGUMENT = '--arguments';
const DOCUMENT = '--document';
const BUNDLE_ID = /bundleID="(?<bundle>[^"]+)"/u;
const BUNDLE_ID_OPTION = /^bundleid=(?<id>.+)$/iu;
const PID = /^\d+$/u;
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const EXECUTABLE = /\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const APP_COMMAND = /^(?<body>\w+) (?:application|app) (?:id )?"(?<app>[^"]+)"$/u;
const OPEN_FILE = /^open (?:POSIX file )?"(?<path>[^"]+)"$/u;
const SHELL_SCRIPT = /^do shell script "(?<inner>(?:[^"\\]|\\.)*)"$/u;
const STATEMENTS = [
    ['launch', /^(?:activate|launch|run|reopen)$/u],
    ['quit', /^quit(?: saving (?:no|yes|ask))?$/u],
    ['hide', /^hide$/u],
    ['script', /^do (?:javascript|script)\b/iu],
] as const;
const JXA_APP = /\b(?:Application|NSRunningApplication|NSWorkspace)\b/u;
const WEB_URL = /^https?:/iu;
const BACKGROUND_ONLY = /\btype="BackgroundOnly"/u;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DECODE]

const bundleId = (output: string): Maybe<string> => Maybe.fromNullable(BUNDLE_ID.exec(output)?.groups?.bundle);

// --- [WATCH]

const watchApps = (...words: readonly string[]): string => [WATCHER, ...words].join(' ');

const watchAction = ({ kind }: CommandRequest, names: readonly string[]): string => {
    const shown = names.length === 0 ? ['<app>'] : names.map(quote);
    return kind === 'plain' ? `run ${watchApps('read', ...shown)}` : `run ${watchApps('watch', ...shown)} in Monitor`;
};

const watchedApps = (invocation: Invocation): readonly string[] => {
    const [subcommand, ...names] = operands(invocation).inputs;
    return subcommand === 'perform' || subcommand === 'set' ? [] : names;
};

// --- [KILL]

const killSignal = ([, ...args]: Invocation): { readonly force: Maybe<readonly string[]>; readonly pids: readonly string[] } => {
    const [first] = args;
    const valued = first === '-s' || first === '-n';
    const cut = valued ? 2 : Number(first !== undefined && first !== '--' && first.startsWith('-'));
    const quits: Readonly<Record<string, readonly string[]>> = { TERM: [], 15: [], KILL: ['--force'], 9: ['--force'] };
    const [signal = 'TERM'] = args.slice(Math.max(cut - 1, 0), cut).map((word) => word.toUpperCase().replace(SIGNAL_PREFIX, ''));
    return { force: Maybe.fromNullable(quits[signal]), pids: args.slice(cut).filter((word) => word !== '--') };
};

const namedPids = (invocation: Invocation): readonly string[] => {
    const [program] = invocation;
    const pids = program === 'kill' ? killSignal(invocation).pids : operands(invocation).values.flatMap(([name, value]) => (program === 'ps' && name === '-p' ? value.split(',') : []));
    return pids.filter((pid) => PID.test(pid));
};

const runningApps = async (system: System, commands: readonly Command[]): Promise<ReadonlyMap<string, string>> => {
    const pids = [...new Set(commands.flatMap(({ invocations: chain }) => chain.flatMap(namedPids)))];
    const probed = await Promise.all(
        pids.map(async (pid) => {
            const { stdout } = await system.run(['lsappinfo', 'info', '-only', 'bundleID,ApplicationType', pid], { cwd: system.plugin, stdin: '' });
            return BACKGROUND_ONLY.test(stdout) ? [] : bundleId(stdout).values.map((bundle) => [pid, bundle] as const);
        }),
    );
    return new Map(probed.flat());
};

const killRoute = (apps: ReadonlyMap<string, string>, head: Word, invocation: Invocation): Maybe<Route> => {
    const { force, pids } = killSignal(invocation);
    const [flags] = force.values;
    if (flags === undefined || pids.length === 0 || !new Set(pids).isSubsetOf(apps)) {
        return Maybe.none;
    }
    const quits = pids.map((pid) => watchApps('quit', '--pid', pid, ...flags));
    return Maybe.some({ kind: 'rewrite', program: 'kill', start: head, text: quits.length === 1 ? quits.join('') : `{ ${quits.join('; ')}; }` });
};

const processKills = ({ script: { commands } }: Call): readonly Verdict[] => [
    ...[...new Set(invocationsOf(commands, 'killall', 'pkill').map(([program]) => program))].map((program) => refuse(`${program} ends every match, run kill <pid> on one pid pgrep lists`)),
    ...commands.flatMap((command, index) => {
        const quitting = command.invocations.some((invocation) => invocation[0] === 'kill' && killSignal(invocation).force.values.length > 0);
        const sources = [...(command.hasInput ? pipelineNeighbors(commands, index, -1) : []), ...nestedIn(commands, command)];
        const [finder] = invocationsOf(sources, 'ps', 'pgrep', 'pidof').map(([program]) => program);
        const watched = invocationsOf(sources, WATCHER);
        return quitting ? [...(finder === undefined ? [] : [refuse(`${finder} feeds kill every match, run kill <pid> on one pid ${finder} lists`)]), ...(watched.length === 0 ? [] : [refuse(`kill takes pids from ${WATCHER}, use ${watchApps('quit', ...watched.flatMap(watchedApps).map(quote))}`)])] : [];
    }),
];

// --- [ROUTES]

const bundleInfo = async (system: System, bundle: string): Promise<Maybe<{ readonly executable: string; readonly identifier: string }>> => {
    const info = await runJson<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string }>(system, ['plutil', '-convert', 'json', '-o', '-', `${bundle}/Contents/Info.plist`], system.plugin);
    const [{ CFBundleExecutable, CFBundleIdentifier } = {}] = info.values;
    return CFBundleExecutable !== undefined && CFBundleIdentifier !== undefined ? Maybe.some({ executable: CFBundleExecutable, identifier: CFBundleIdentifier }) : Maybe.none;
};

const headlessExecutable = async (system: System, cwd: Maybe<string>, app: Word, identifying: boolean, passed: readonly Word[]): Promise<Maybe<string>> => {
    const file = app.text.endsWith('.app') ? app.text : `${app.text}.app`;
    const query = identifying ? `kMDItemCFBundleIdentifier == "${app.text}"` : `kMDItemContentType == "com.apple.application-bundle" && kMDItemFSName == "${file}"c`;
    const [bundle, ...more] = app.text.includes('/') ? resolvePath(app.text, cwd, system.home).values : (await system.run(['mdfind', query], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((line) => line !== '');
    const [run] = bundle === undefined || more.length > 0 ? [] : (await bundleInfo(system, bundle)).values;
    return bundle !== undefined && run !== undefined && programRow(run.executable).headless?.some((flag) => passed.some(({ text }) => text === flag)) === true ? Maybe.some(quote(`${bundle}/Contents/MacOS/${run.executable}`)) : Maybe.none;
};

const openRoute = async (system: System, request: CommandRequest, { cwd }: Command, program: 'open' | 'xed', head: Word, args: readonly Word[]): Promise<Maybe<Route>> => {
    const launchers = {
        open: { app: [], waits: ['-W', '--wait-apps', '--stdin', '--stdout', '--stderr'], kept: ['-a', '-b', '-n', '--new', '-g', '--background', '-j', '--hide', '-F', '--fresh', '--env', '-u', '--url'] },
        xed: { app: ['com.apple.dt.Xcode'], waits: ['-w', '--wait'], kept: ['-x', '--launch', '-b', '--background'] },
    };
    const { app: fixed, waits: blocking, kept } = launchers[program];
    const cut = args.findIndex(({ text }) => text === '--args');
    const listed = argumentGroups(program, (cut < 0 ? args : args.slice(0, cut)).values()).toArray();
    const passed = cut < 0 ? [] : args.slice(cut + 1);
    const valued = (...flags: readonly string[]): readonly Word[] => listed.flatMap(({ names, words: [, value] }) => (value !== undefined && names.some((name) => flags.includes(name)) ? [value] : []));
    const loose = listed.flatMap(({ names, words: [word] }) => (names.length === 0 && word !== undefined ? [word] : []));
    const switches = listed.flatMap(({ names }) => names);
    const [app] = [...valued('-a', '-b'), ...loose.filter(({ text }) => text.endsWith('.app'))];
    const subjects = [...fixed, ...Maybe.fromNullable(app?.raw).values];
    const target = subjects.join(' ') || '<app>';
    const documents = loose.filter((word) => word !== app);
    const [url] = [...valued('-u', '--url'), ...documents.filter(({ text }) => URL.canParse(text))];
    const [waited] = switches.filter((name) => blocking.includes(name));
    const foreign = switches.filter((name) => !(kept.includes(name) || blocking.includes(name)));
    if (informational(program, switches) || subjects.length + documents.length === 0) {
        return Maybe.none;
    }
    if (url !== undefined) {
        return Maybe.some(refuse(WEB_URL.test(url.text) ? `${program} ${url.raw} opens a browser, use playwright-cli` : `${program} ${url.raw} launches its handler, use ${watchApps('launch', target)}`));
    }
    if (waited !== undefined) {
        return Maybe.some(refuse(`${program} ${waited} waits on ${target}, ${watchAction(request, subjects)}`));
    }
    if (foreign.length > 0) {
        return Maybe.some(refuse(`${program} ${foreign.join(' ')} launches ${target}, use ${watchApps('launch', target)}`));
    }
    const direct = app !== undefined && passed.length > 0 ? await headlessExecutable(system, cwd, app, valued('-b').includes(app), passed) : Maybe.none;
    const environment = valued('--env');
    const launch = direct.match(
        (path) => [...environment.map(({ raw }) => raw), path, ...passed.map(({ raw }) => raw)].join(' '),
        () => watchApps('launch', ...subjects, ...(switches.some((name) => name === '-n' || name === '--new') ? ['--new'] : []), ...documents.map(({ raw }) => `${DOCUMENT} ${raw}`), ...environment.map(({ raw }) => `--environment=${raw}`), ...passed.map(({ raw }) => `${ARGUMENT}=${raw}`)),
    );
    return Maybe.some({ kind: 'rewrite', program, start: head, text: launch });
};

const statement = (body: string): Statement => {
    const path = OPEN_FILE.exec(body)?.groups?.path;
    const inner = SHELL_SCRIPT.exec(body)?.groups?.inner;
    if (path !== undefined) {
        return { kind: 'open', path };
    }
    if (inner !== undefined) {
        return { kind: 'shell', inner: inner.replaceAll(/\\(?<char>.)/gu, '$<char>') };
    }
    return { kind: STATEMENTS.find(([, pattern]) => pattern.test(body))?.[0] ?? 'drive' };
};

const appleScriptStatements = (values: readonly (readonly [string, string])[]): readonly { readonly app: Maybe<string>; readonly statement: Statement }[] =>
    values
        .flatMap(([name, value]) => (name === '-e' ? value.split('\n') : []))
        .map((line) => line.trim())
        .filter((line) => line !== '')
        .reduce<{ readonly stack: readonly string[]; readonly said: readonly { readonly app: Maybe<string>; readonly statement: Statement }[] }>(
            ({ stack, said }, line) => {
                const groups = TELL.exec(line)?.groups ?? APP_COMMAND.exec(line)?.groups;
                if (line === 'end tell') {
                    return { stack: stack.slice(0, -1), said };
                }
                return groups?.app !== undefined && groups.body === undefined ? { stack: [...stack, groups.app], said } : { stack, said: [...said, { app: Maybe.fromNullable(groups?.app ?? stack.at(-1)), statement: statement(groups?.body ?? line) }] };
            },
            { stack: [], said: [] },
        ).said;

const osascriptRoute = (head: Word, invocation: Invocation): Maybe<Route> => {
    const { values, inputs } = operands(invocation);
    const events = 'System Events';
    if (values.some(([name, value]) => name === '-l' && value === 'JavaScript')) {
        return values.some(([name, value]) => name === '-e' && JXA_APP.test(value)) ? Maybe.some(refuse(`JavaScript osascript drives apps, run ${WATCHER} <subcommand> <app>`)) : Maybe.none;
    }
    const said = appleScriptStatements(values);
    const aimed = said.flatMap(({ app, statement: spoke }) => app.values.map((name) => ({ app: name, statement: spoke })));
    const apps = [...new Set(aimed.map(({ app }) => app))].map(quote);
    const [shell] = said.flatMap(({ statement: spoke }) => (spoke.kind === 'shell' ? [spoke.inner] : []));
    const documents = aimed.flatMap(({ statement: spoke }) => (spoke.kind === 'open' ? [`${DOCUMENT} ${quote(spoke.path)}`] : []));
    const verbs = new Set(aimed.map(({ statement: { kind } }) => (kind === 'open' ? 'launch' : kind)));
    const [verb] = verbs.size === 1 ? [...verbs].filter((kind) => kind === 'launch' || kind === 'quit') : [];
    const [refused] = (
        [
            [inputs.length > 0, `osascript ${inputs.join(' ')} hides its statements, pass them as -e lines`],
            [aimed.some(({ app }) => app === events), `${events} posts to the frontmost app, use ${watchApps('perform')}`],
            [shell !== undefined, `osascript runs a shell command, run ${shell ?? ''} directly`],
            [aimed.some(({ statement: spoke }) => spoke.kind === 'script'), `osascript scripts ${apps.join(' ')}, use Atelier execute_script`],
            [aimed.some(({ statement: spoke }) => spoke.kind === 'hide'), `${apps.join(' ')} instances launch hidden, run ${watchApps('launch', ...apps)}`],
            [aimed.length > 0 && (verb === undefined || (documents.length > 0 && apps.length > 1)), `osascript drives ${apps.join(' ')}, use ${watchApps('perform')}`],
        ] as const
    ).flatMap(([held, text]) => (held ? [text] : []));
    if (refused !== undefined) {
        return Maybe.some(refuse(refused));
    }
    return aimed.length === 0 || verb === undefined ? Maybe.none : Maybe.some({ kind: 'rewrite', program: 'osascript', start: head, text: watchApps(verb, ...apps, ...documents) });
};

const lsappinfoRoute = (head: Word, invocation: Invocation): Maybe<Route> => {
    const [subcommand, ...rest] = operands(invocation).inputs;
    const [id] = rest.flatMap((word) => Maybe.fromNullable(BUNDLE_ID_OPTION.exec(word)?.groups?.id).values);
    if (subcommand !== 'launch') {
        return Maybe.none;
    }
    return Maybe.some(id === undefined ? refuse(`lsappinfo launch bypasses ${WATCHER}, use ${watchApps('launch', '<app>')}`) : { kind: 'rewrite', program: 'lsappinfo', start: head, text: watchApps('launch', quote(id)) });
};

const launchctlRoute = (invocation: Invocation): Maybe<Route> => {
    const at = invocation.indexOf('--');
    const [program, ...args] = at < 0 ? [] : invocation.slice(at + 1);
    const executed = program === undefined ? undefined : EXECUTABLE.exec(program)?.groups?.executable;
    return invocation[1] === 'submit' && executed !== undefined && programRow(executed).headless?.some((flag) => args.includes(flag)) !== true ? Maybe.some(refuse(`launchctl submit keeps ${executed} alive, use ${watchApps('launch', quote(executed))}`)) : Maybe.none;
};

const bundleRoute = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Maybe<Route>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || informational(program, operands(invocation).options) || programRow(program).headless?.some((name) => invocation.includes(name)) === true) {
        return Maybe.none;
    }
    if (groups?.bundle === undefined) {
        return head.expands && EXECUTABLE.test(head.text) ? Maybe.some(refuse(`${program} path expands at run time, use ${watchApps('launch', quote(program))}`)) : Maybe.none;
    }
    const [info] = (await bundleInfo(system, groups.bundle)).values;
    return info !== undefined && info.executable === groups.executable ? Maybe.some({ kind: 'rewrite', program, start: assigned[0] ?? head, text: watchApps('launch', info.identifier, ...args.map(({ raw }) => `${ARGUMENT}=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)) }) : Maybe.none;
};

const appRoute = async (system: System, request: CommandRequest, apps: ReadonlyMap<string, string>, command: Command): Promise<Maybe<Route>> => {
    const last = command.invocations.at(-1);
    const part = command.parts.at(-1);
    const [head, ...args] = part?.words ?? [];
    if (last === undefined || part === undefined || head === undefined) {
        return Maybe.none;
    }
    switch (last[0]) {
        case 'open':
        case 'xed':
            return await openRoute(system, request, command, last[0], head, args);
        case 'osascript':
            return osascriptRoute(head, last);
        case 'kill':
            return killRoute(apps, head, last);
        case 'lsappinfo':
            return lsappinfoRoute(head, last);
        case 'launchctl':
            return launchctlRoute(last);
        case 'screencapture':
            return Maybe.some(refuse(`screencapture captures the whole display, use ${watchApps('capture', '<app>')}`));
        case 'qlmanage':
            return last.includes('-p') ? Maybe.some(refuse('qlmanage -p opens a preview window, use qlmanage -t')) : Maybe.none;
        default:
            return await bundleRoute(system, last, part);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { appRoute, bundleId, namedPids, PID, processKills, runningApps, WATCHER, watchAction, watchedApps };
