import { Maybe } from '../../monads.ts';
import type { CommandRequest, System } from '../command-decision.ts';
import { type Command, runJson, type Word } from './command.ts';
import { argumentGroups, type Invocation, informational, operands, programRow, resolvePath } from './invocation.ts';
import { type Call, invocationsOf, nestedIn, pipelineNeighbors, quote, type Route, refuse, type Verdict } from './verdict.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Statement = { readonly kind: 'launch' | 'other' } | { readonly kind: 'open'; readonly path: string } | { readonly kind: 'shell'; readonly inner: string };

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCHER = 'watch-apps';
const ARGUMENT = '--arguments';
const DOCUMENT = '--document';
const BUNDLE_ID = /bundleID="(?<bundle>[^"]+)"/u;
const PID = /^\d+$/u;
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const TELL = /^tell (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const APP_COMMAND = /^(?<body>\w+) (?:application|app) (?:id )?"(?<app>[^"]+)"$/u;
const OPEN_FILE = /^open (?:POSIX file )?"(?<path>[^"]+)"$/u;
const SHELL_SCRIPT = /^do shell script "(?<inner>(?:[^"\\]|\\.)*)"$/u;
const LAUNCH_STATEMENT = /^(?:activate|launch|run|reopen)$/u;
const FOCUS = /\bactivate\b|\bset\s+frontmost\b|AXRaise/u;
const POSTING = /System Events|com\.apple\.systemevents/u;
const WEB_URL = /^https?:/iu;
const BACKGROUND_ONLY = /\btype="BackgroundOnly"/u;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const PLIST_TRUE = /^(?:1|YES|true)$/iu;
const ENDING_SIGNALS: ReadonlySet<string> = new Set(['TERM', '15', 'KILL', '9']);

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

const killSignal = ([, ...args]: Invocation): { readonly ending: boolean; readonly pids: readonly string[] } => {
    const [first] = args;
    const valued = first === '-s' || first === '-n';
    const cut = valued ? 2 : Number(first !== undefined && first !== '--' && first.startsWith('-'));
    const [signal = 'TERM'] = args.slice(Math.max(cut - 1, 0), cut).map((word) => word.toUpperCase().replace(SIGNAL_PREFIX, ''));
    return { ending: ENDING_SIGNALS.has(signal), pids: args.slice(cut).filter((word) => word !== '--') };
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

const processKills = ({ script: { commands } }: Call): readonly Verdict[] => [
    ...[...new Set(invocationsOf(commands, 'killall', 'pkill').map(([program]) => program))].map((program) => refuse(`${program} ends every match, run kill <pid> on one pid pgrep lists`)),
    ...commands.flatMap((command, index) => {
        const ending = command.invocations.some((invocation) => invocation[0] === 'kill' && killSignal(invocation).ending);
        const sources = [...(command.hasInput ? pipelineNeighbors(commands, index, -1) : []), ...nestedIn(commands, command)];
        const [finder] = invocationsOf(sources, 'ps', 'pgrep', 'pidof').map(([program]) => program);
        return ending && finder !== undefined ? [refuse(`${finder} feeds kill every match, run kill <pid> on one pid ${finder} lists`)] : [];
    }),
];

// --- [ROUTES]

const bundleInfo = async (system: System, bundle: string): Promise<Maybe<{ readonly executable: string; readonly identifier: string; readonly background: boolean }>> => {
    const info = await runJson<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string; readonly LSBackgroundOnly?: unknown; readonly LSUIElement?: unknown }>(system, ['plutil', '-convert', 'json', '-o', '-', `${bundle}/Contents/Info.plist`], system.plugin);
    const [{ CFBundleExecutable, CFBundleIdentifier, LSBackgroundOnly, LSUIElement } = {}] = info.values;
    return CFBundleExecutable !== undefined && CFBundleIdentifier !== undefined ? Maybe.some({ executable: CFBundleExecutable, identifier: CFBundleIdentifier, background: [LSBackgroundOnly, LSUIElement].some((declared) => PLIST_TRUE.test(String(declared))) }) : Maybe.none;
};

const appBundles = async (system: System, cwd: Maybe<string>, app: Word, identifying: boolean): Promise<readonly string[]> => {
    const file = app.text.endsWith('.app') ? app.text : `${app.text}.app`;
    const query = identifying ? `kMDItemCFBundleIdentifier == "${app.text}"` : `kMDItemContentType == "com.apple.application-bundle" && kMDItemFSName == "${file}"c`;
    return app.text.includes('/') ? resolvePath(app.text, cwd, system.home).values : (await system.run(['mdfind', query], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((line) => line !== '');
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
    const bundles = app === undefined ? [] : await appBundles(system, cwd, app, valued('-b').includes(app));
    const infos = (await Promise.all(bundles.map((bundle) => bundleInfo(system, bundle)))).flatMap(({ values }) => values);
    if (infos.length > 0 && infos.every(({ background }) => background)) {
        return Maybe.none;
    }
    const [bundle] = bundles;
    const [run] = infos;
    const direct = bundles.length === 1 && run !== undefined && programRow(run.executable).headless?.some((flag) => passed.some(({ text }) => text === flag)) === true ? quote(`${bundle}/Contents/MacOS/${run.executable}`) : undefined;
    const environment = valued('--env');
    const launch =
        direct === undefined
            ? watchApps('launch', ...subjects, ...(switches.some((name) => name === '-n' || name === '--new') ? ['--new'] : []), ...documents.map(({ raw }) => `${DOCUMENT} ${raw}`), ...environment.map(({ raw }) => `--environment=${raw}`), ...passed.map(({ raw }) => `${ARGUMENT}=${raw}`))
            : [...environment.map(({ raw }) => raw), direct, ...passed.map(({ raw }) => raw)].join(' ');
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
    return { kind: LAUNCH_STATEMENT.test(body) ? 'launch' : 'other' };
};

const appleScriptStatements = (lines: readonly string[]): readonly { readonly app: Maybe<string>; readonly statement: Statement }[] =>
    lines
        .flatMap((value) => value.split('\n'))
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
    const lines = values.flatMap(([name, value]) => (name === '-e' ? [value] : []));
    const said = values.some(([name, value]) => name === '-l' && value === 'JavaScript') ? [] : appleScriptStatements(lines);
    const aimed = said.flatMap(({ app, statement: spoke }) => app.values.map((name) => ({ app: name, statement: spoke })));
    const apps = [...new Set(aimed.map(({ app }) => app))].map(quote);
    const [shell] = said.flatMap(({ statement: spoke }) => (spoke.kind === 'shell' ? [spoke.inner] : []));
    const documents = aimed.flatMap(({ statement: spoke }) => (spoke.kind === 'open' ? [`${DOCUMENT} ${quote(spoke.path)}`] : []));
    const launching = said.length > 0 && aimed.length === said.length && aimed.every(({ statement: { kind } }) => kind === 'launch' || kind === 'open') && (documents.length === 0 || apps.length === 1);
    const [refused] = (
        [
            [inputs.length > 0 || lines.length === 0, `osascript ${inputs.join(' ') || 'stdin'} hides its statements, pass them as -e lines`],
            [lines.some((line) => POSTING.test(line)), `System Events posts to the frontmost app, use ${watchApps('perform')}`],
            [shell !== undefined, `osascript runs a shell command, run ${shell ?? ''} directly`],
            [!launching && lines.some((line) => FOCUS.test(line)), 'osascript takes focus, capture and perform work without it'],
        ] as const
    ).flatMap(([held, text]) => (held ? [text] : []));
    if (refused !== undefined) {
        return Maybe.some(refuse(refused));
    }
    return launching ? Maybe.some({ kind: 'rewrite', program: 'osascript', start: head, text: watchApps('launch', ...apps, ...documents) }) : Maybe.none;
};

const bundleRoute = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Maybe<Route>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || groups?.bundle === undefined || informational(program, operands(invocation).options) || programRow(program).headless?.some((name) => invocation.includes(name)) === true) {
        return Maybe.none;
    }
    const [info] = (await bundleInfo(system, groups.bundle)).values;
    return info !== undefined && info.executable === groups.executable && !info.background ? Maybe.some({ kind: 'rewrite', program, start: assigned[0] ?? head, text: watchApps('launch', info.identifier, ...args.map(({ raw }) => `${ARGUMENT}=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)) }) : Maybe.none;
};

const appRoute = async (system: System, request: CommandRequest, command: Command): Promise<Maybe<Route>> => {
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
        case 'qlmanage':
            return last.includes('-p') ? Maybe.some(refuse('qlmanage -p opens a preview window, use qlmanage -t')) : Maybe.none;
        default:
            return await bundleRoute(system, last, part);
    }
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { appRoute, bundleId, namedPids, PID, processKills, runningApps, WATCHER, watchAction, watchedApps };
