import { constants } from 'node:os';
import { basename } from 'node:path';
import { fileURLToPath } from 'node:url';
import { Installation } from '@rasm/applications';
import { Array, Effect, Option, Predicate, Record, Schema, String } from 'effect';
import { Host } from '#/host';
import { type Call, ensure, type Fix, has, inserted, type Policy, quote, refusal, refuse, stages, when, whole, within } from '#/policies/fix';
import type { Program } from '#/shell/programs';
import type { Word } from '#/shell/script';
import { Shell } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WATCHER = 'watch-apps';
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/[^/]+$/u;
const ADDRESSED = /^(?:tell|(?<verb>\w+)) (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const LAUNCHING = /^(?:activate|launch|run|reopen|open (?:POSIX file )?"(?<path>[^"]+)")$/u;
const SHELL_SCRIPT = /\bdo shell script\b/u;
const FOCUS = /\bactivate\b|\bset\s+frontmost\b|AXRaise/u;
const POSTING = /System Events|com\.apple\.systemevents/u;
const WEB_URL = /^https?:/iu;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const SIGNAL = /^-(?:\d+|(?:SIG)?[A-Z]{2,}\d*)$/u;
const PIDS = /^\d+/gmu;
const ENDING = new Set(['TERM', 'KILL', `${constants.signals.SIGTERM}`, `${constants.signals.SIGKILL}`]);
const FINDERS = new Set(['ps', 'pgrep', 'pidof']);

// --- [MODELS] --------------------------------------------------------------------------

const Found = Schema.fromJsonString(Schema.toCodecJson(Schema.Record(Schema.String, Schema.Array(Installation))));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [BUNDLES]

const installed = (names: readonly (number | string)[]): Effect.Effect<Record<string, readonly Installation[]>, never, Host> =>
    Array.isReadonlyArrayNonEmpty(names)
        ? Effect.map(
              Host.use((host) => host.output([WATCHER, 'find', ...Array.map(names, (name) => `${name}`)])),
              (text) => Record.fromEntries(Array.getSomes(text.split('\n').map((line) => Schema.decodeOption(Found)(line))).flatMap(Record.toEntries)),
          )
        : Effect.succeed({});

const foreground = ({ activationPolicy }: Installation) => activationPolicy === 'regular';

const switchesOf = (installations: readonly Installation[], given: readonly string[], programs: ReadonlyMap<string, Program>): readonly string[] => {
    const passed = new Set(given);
    return Array.dedupe(
        Array.getSomes(installations.map(({ executable }) => Option.flatMapNullishOr(executable, ({ href }) => programs.get(basename(fileURLToPath(href))))))
            .filter(({ headless = [] }) => passed.isDisjointFrom(new Set(headless)))
            .flatMap(({ launching = [] }) => launching)
            .filter((flag) => !passed.has(flag)),
    );
};

const launchLine = (subjects: readonly string[], documents: readonly string[], environment: readonly Word[] = [], args: readonly Word[] = [], switches: readonly string[] = []): string =>
    [WATCHER, 'launch', ...subjects, ...(documents.length > 0 ? ['--document', ...documents] : []), ...(environment.length > 0 ? ['--environment', ...environment.map(({ raw }) => raw)] : []), ...(args.length + switches.length > 0 ? ['--arguments', ...args.map(({ raw }) => raw), ...switches] : [])].join(' ');

const headlessPath = ([installation, ...others]: readonly Installation[], rest: readonly Word[], programs: ReadonlyMap<string, Program>): Option.Option<string> =>
    Option.fromUndefinedOr(installation).pipe(
        Option.flatMap(({ executable }) => executable),
        Option.map(({ href }) => fileURLToPath(href)),
        Option.filter((path) => others.length === 0 && !new Set(rest.map(({ text }) => text)).isDisjointFrom(new Set(programs.get(basename(path))?.headless))),
        Option.map(quote),
    );

// --- [LAUNCHES]

const launched = (named: readonly string[], waits: readonly string[], allowed: readonly string[]): Fix =>
    Effect.fnUntraced(function* (site, call) {
        const { program, rest } = site;
        const loose = site.operands;
        const [app] = [...site.supplied('--app', '--bundle'), ...loose.filter(({ text }) => text.endsWith('.app') && !URL.canParse(text))];
        const place = Option.flatMap(Option.fromNullishOr(app), (word) => Option.filter(call.host.place(word.text, site.command.cwd), () => word.known && (word.text.includes('/') || loose.includes(word))));
        const subjects = [...named, ...Option.toArray(Option.orElse(Option.map(place, quote), () => Option.map(Option.fromNullishOr(app), ({ raw }) => raw)))];
        const target = subjects.join(' ') || '<app>';
        const documents = [...loose.filter((word) => word !== app), ...site.supplied('--url')];
        const url = documents.find(({ text }) => WEB_URL.test(text));
        const [waited] = site.names.intersection(new Set(waits));
        const foreign = site.names.difference(new Set([...allowed, ...waits]));
        if (url !== undefined) {
            return refusal(`${program} ${url.raw} opens a browser, use playwright cli open ${url.raw}`);
        }
        if (subjects.length + documents.length === 0) {
            return [];
        }
        if (waited !== undefined) {
            return refusal(`${program} ${waited} waits, run ${WATCHER} watch ${target} in Monitor`);
        }
        if (foreign.size > 0) {
            return refusal(`${program} ${[...foreign].join(' ')} opens ${target}, use ${WATCHER} launch ${target}`);
        }
        const found = app === undefined ? [] : Object.values(yield* installed([Option.getOrElse(place, () => app.text)])).flat();
        const environment = site.supplied('--env');
        const { programs } = yield* Shell;
        if (found.length > 0 && !found.some(foreground)) {
            return [];
        }
        return whole(
            site,
            Option.match(headlessPath(found, rest, programs), {
                onNone: () =>
                    launchLine(
                        [...subjects, ...(site.has('--new') ? ['--new'] : [])],
                        documents.map(({ raw }) => raw),
                        environment,
                        rest,
                        switchesOf(
                            found,
                            rest.map(({ text }) => text),
                            programs,
                        ),
                    ),
                onSome: (path) => [...environment.map(({ raw }) => raw), path, ...rest.map(({ raw }) => raw)].join(' '),
            }),
            program,
        );
    });

// --- [OSASCRIPT]

const statements = (lines: readonly string[]) =>
    Array.mapAccum(
        lines
            .flatMap((value) => value.split('\n'))
            .map((line) => line.trim())
            .filter(Predicate.isTruthy),
        [],
        (stack: readonly string[], line): readonly [readonly string[], readonly { readonly app: string | undefined; readonly launches: boolean; readonly path: string | undefined }[]] => {
            const { app, verb, body = verb } = ADDRESSED.exec(line)?.groups ?? {};
            if (line === 'end tell') {
                return [stack.slice(0, -1), []];
            }
            if (app !== undefined && body === undefined) {
                return [[...stack, app], []];
            }
            const launching = body ?? line;
            return [
                stack,
                [
                    {
                        app: app ?? stack.at(-1),
                        launches: LAUNCHING.test(launching),
                        path: Option.getOrUndefined(
                            String.match(LAUNCHING)(launching).pipe(
                                Option.flatMapNullishOr(({ groups }) => groups),
                                Option.flatMap(Record.get('path')),
                                Option.filter(Predicate.isString),
                            ),
                        ),
                    },
                ],
            ];
        },
    )[1].flat();

const scripted: Fix = (site) => {
    const lines = [...site.values('-e'), ...Option.toArray(site.command.input)];
    const said = site.values('-l').includes('JavaScript') ? [] : statements(lines);
    const named = [...new Set(said.map(({ app }) => app).filter(Predicate.isNotUndefined))];
    const apps = named.map(quote);
    const documents = said.map(({ path }) => path).filter(Predicate.isNotUndefined);
    const script = site.has('-e') ? undefined : site.operands.find(({ text }) => text !== '-');
    if (script !== undefined || lines.length === 0) {
        return refusal(`osascript ${script?.text ?? 'stdin'} hides its statements, pass -e lines`);
    }
    if (lines.some((line) => POSTING.test(line))) {
        return refusal(`System Events posts to the frontmost app, use ${WATCHER} perform`);
    }
    if (lines.some((line) => SHELL_SCRIPT.test(line))) {
        return refusal('osascript runs a shell command, run it directly');
    }
    if (said.length > 0 && said.every(({ app, launches }) => app !== undefined && launches) && (documents.length === 0 || apps.length === 1)) {
        return Effect.gen(function* () {
            const switches = switchesOf(Object.values(yield* installed(named)).flat(), [], (yield* Shell).programs);
            return whole(site, launchLine(apps, documents.map(quote), [], [], switches), 'osascript');
        });
    }
    return lines.some((line) => FOCUS.test(line)) ? refusal(`osascript takes focus, use ${WATCHER} capture or perform`) : [];
};

// --- [BINARIES]

const absoluteArgument =
    ({ host }: Call, cwd: Option.Option<string>) =>
    (word: Word): Effect.Effect<Word> =>
        Option.match(
            Option.filter(host.place(word.text, cwd), () => word.known && !word.text.startsWith('-') && !host.path.isAbsolute(word.text)),
            {
                onNone: () => Effect.succeed(word),
                onSome: (place) => Effect.map(host.exists(place), (exists) => (exists ? { ...word, raw: quote(place) } : word)),
            },
        );

const binary: Fix = (site, call) => {
    const executable = Option.filter(call.host.place(site.head.text, site.command.cwd), () => !site.has(...(site.row.headless ?? [])));
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
                const installations = Object.values(yield* installed([bundle])).flat();
                const args = yield* Effect.forEach(site.words.slice(1), absoluteArgument(call, site.command.cwd));
                const runs = installations.some((installation) => foreground(installation) && Option.exists(installation.executable, ({ href }) => fileURLToPath(href) === head));
                const switches = switchesOf(
                    installations,
                    args.map(({ text }) => text),
                    (yield* Shell).programs,
                );
                return runs ? whole(site, launchLine([quote(bundle)], [], site.assigned, args, switches), site.program, site.assigned[0]) : [];
            }),
        },
    );
};

// --- [SWITCHES]

const switched: Fix = Effect.fnUntraced(function* (site) {
    const given = [...site.values('--arguments'), ...site.rest.map(({ text }) => text)];
    const switches = site.path[0] === 'launch' ? switchesOf(Object.values(yield* installed(site.operands.map(({ text }) => text))).flat(), given, (yield* Shell).programs) : [];
    return switches.length === 0 ? [] : inserted(site, Array.lastNonEmpty(site.words).end, ` ${[...(site.has('--arguments') ? [] : ['--arguments']), ...switches].join(' ')}`);
});

// --- [KILLS]

const fedKill: Fix = (site, call) => {
    const { command, values, words, names } = site;
    const signal = (values('-s', '-n')[0] ?? (names.size > 0 ? words[1]?.text : undefined) ?? 'TERM').toUpperCase().replace(SIGNAL_PREFIX, '');
    const finder = [...(command.fed ? stages(site, call).earlier : []), ...within(site, call)].find(({ program }) => FINDERS.has(program));
    return ENDING.has(signal) && finder !== undefined ? refusal(`${finder.program} feeds kill every match, kill one pid`) : [];
};

const matched: Fix = Effect.fnUntraced(function* (site, { host }) {
    const [signal] = site.words.slice(1, 2).filter(({ text }) => SIGNAL.test(text));
    const rest = site.words.slice(signal === undefined ? 1 : 2);
    const flags = rest.map(({ text }) => text).filter((text) => text.startsWith('-'));
    const queries = site.program === 'pkill' ? [rest.map(({ text }) => text)] : site.operands.map(({ text }) => [...(flags.includes('-m') ? [] : ['-x']), text]);
    const answers = rest.every(({ known }) => known) && (site.program === 'pkill' || flags.every((flag) => flag === '-m' || flag === '-v')) ? yield* Effect.forEach(queries, (query) => host.output(['pgrep', ...query]), { concurrency: 'unbounded' }) : [];
    const pids = answers.join('\n').match(PIDS) ?? [];
    const parents = yield* host.parents;
    const descends = (pid: string | undefined): boolean => pid === host.parent || (pid !== undefined && Number(pid) > 1 && descends(parents.get(pid)));
    return pids.length > 0 && pids.every((pid) => descends(parents.get(pid))) ? whole(site, ['kill', ...Option.toArray(Option.map(Option.fromNullishOr(signal), ({ raw }) => raw)), ...pids].join(' '), site.program) : refusal(`${site.program} ends every match, kill one pid`);
});

// --- [TABLES] --------------------------------------------------------------------------

const ENDS_MATCHES = when(Predicate.not(has('-0')), matched);

const APPS: Policy = {
    programs: {
        open: { ends: ['--args'], fixes: [launched([], ['--wait', '--wait-apps', '--stdin', '--stdout', '--stderr'], ['--app', '--bundle', '--new', '--hide', '--background', '-j', '--fresh', '--env', '--url', '--args'])] },
        xed: { flags: ['-l, --line='], informational: ['-v'], fixes: [launched(['com.apple.dt.Xcode'], ['-w', '--wait'], ['-x', '--launch', '-b', '--background'])] },
        osascript: { fixes: [scripted] },
        qlmanage: { fixes: [refuse('opens a preview window', 'use qlmanage -t', '-p')] },
        killall: { flags: ['-s', '-c=', '-t='], informational: ['-s', '-l'], fixes: [ENDS_MATCHES] },
        pkill: { fixes: [ENDS_MATCHES] },
        kill: { flags: ['-n='], fixes: [fedKill] },
        Blender: {
            aliases: ['blender'],
            flags: ['-P, --python=', '--python-expr='],
            informational: ['-v', '--log-list-categories'],
            headless: ['-b', '--background', '-c', '--command'],
            launching: ['--no-window-focus'],
            fixes: [when(has('--python', '--python-expr'), ensure('--python-use-system-env'))],
        },
        Rhinoceros: { launching: ['-nosplash'] },
        [WATCHER]: { ends: ['--arguments'], fixes: [switched] },
    },
    every: [binary],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS, installed, WATCHER };
