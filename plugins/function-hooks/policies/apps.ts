import { constants } from 'node:os';
import { Array, Effect, Option, Predicate, Schema } from 'effect';
import { WATCHER } from '#/instances.ts';
import { type Call, type Fix, has, type Policy, quote, refusal, refuse, stages, when, whole, within } from '#/policies/fix.ts';
import type { Program } from '#/shell/programs.ts';
import type { Word } from '#/shell/script.ts';
import { Shell } from '#/shell/site.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const ADDRESSED = /^(?:tell|(?<verb>\w+)) (?:application|app) (?:id )?"(?<app>[^"]+)"(?: to (?<body>.+))?$/u;
const LAUNCHING = /^(?:activate|launch|run|reopen|open (?:POSIX file )?"(?<path>[^"]+)")$/u;
const SHELL_SCRIPT = /\bdo shell script\b/u;
const FOCUS = /\bactivate\b|\bset\s+frontmost\b|AXRaise/u;
const POSTING = /System Events|com\.apple\.systemevents/u;
const WEB_URL = /^https?:/iu;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const SIGNAL = /^-(?:\d+|(?:SIG)?[A-Z]{2,}\d*)$/u;
const PIDS = /^\d+/gmu;
const PLIST_TRUE = /^(?:1|YES|true)$/iu;
const ENDING = new Set(['TERM', 'KILL', `${constants.signals.SIGTERM}`, `${constants.signals.SIGKILL}`]);
const FINDERS = new Set(['ps', 'pgrep', 'pidof']);

// --- [MODELS] --------------------------------------------------------------------------

const BundleInfo = Schema.fromJsonString(Schema.Struct({ CFBundleExecutable: Schema.String, CFBundleIdentifier: Schema.String, LSBackgroundOnly: Schema.optionalKey(Schema.Unknown), LSUIElement: Schema.optionalKey(Schema.Unknown) }));

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [BUNDLES]

const bundleInfo = (bundle: string, { host }: Call) => Effect.map(host.output(['plutil', '-convert', 'json', '-o', '-', host.path.join(bundle, 'Contents/Info.plist')]), Schema.decodeOption(BundleInfo));

const backgrounded = ({ LSBackgroundOnly, LSUIElement }: typeof BundleInfo.Type): boolean => [LSBackgroundOnly, LSUIElement].some((declared) => PLIST_TRUE.test(`${declared}`));

const launchLine = (subjects: readonly string[], documents: readonly string[], environment: readonly Word[] = [], args: readonly Word[] = []): string =>
    [WATCHER, 'launch', ...subjects, ...(documents.length > 0 ? ['--document', ...documents] : []), ...(environment.length > 0 ? ['--environment', ...environment.map(({ raw }) => raw)] : []), ...(args.length > 0 ? ['--', ...args.map(({ raw }) => raw)] : [])].join(' ');

const appBundles = ({ text }: Word, cwd: Option.Option<string>, identifying: boolean, { host }: Call) =>
    text.includes('/')
        ? Effect.succeed(Option.toArray(host.place(text, cwd)))
        : Effect.map(host.output(['mdfind', identifying ? `kMDItemCFBundleIdentifier == "${text}"` : `kMDItemContentType == "com.apple.application-bundle" && kMDItemFSName == "${host.path.basename(text, '.app')}.app"c`]), (found) => found.split('\n').filter(Predicate.isTruthy));

const headlessPath = ([bundle, ...others]: readonly string[], [info]: readonly (typeof BundleInfo.Type)[], rest: readonly Word[], programs: ReadonlyMap<string, Program>): Option.Option<string> =>
    bundle !== undefined && others.length === 0 && info !== undefined && !new Set(rest.map(({ text }) => text)).isDisjointFrom(new Set(programs.get(info.CFBundleExecutable)?.headless)) ? Option.some(quote(`${bundle}/Contents/MacOS/${info.CFBundleExecutable}`)) : Option.none();

// --- [LAUNCHES]

const launched = (named: readonly string[], waits: readonly string[], allowed: readonly string[]): Fix =>
    Effect.fnUntraced(function* (site, call) {
        const { program, rest } = site;
        const loose = site.items.filter(({ names }) => names.length === 0).map(({ words: [word] }) => word);
        const [app] = [...site.supplied('--app', '--bundle'), ...loose.filter(({ text }) => text.endsWith('.app'))];
        const subjects = [...named, ...Option.toArray(Option.map(Option.fromNullishOr(app), ({ raw }) => raw))];
        const target = subjects.join(' ') || '<app>';
        const documents = loose.filter((word) => word !== app);
        const [url] = [...site.supplied('--url'), ...documents.filter(({ text }) => URL.canParse(text))];
        const [waited] = site.names.intersection(new Set(waits));
        const foreign = site.names.difference(new Set([...allowed, ...waits]));
        if (url !== undefined) {
            return refusal(WEB_URL.test(url.text) ? `${program} ${url.raw} opens a browser, use playwright-cli` : `${program} ${url.raw} opens its handler, use ${WATCHER} launch ${target}`);
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
        const bundles = app === undefined ? [] : yield* appBundles(app, site.command.cwd, site.supplied('--bundle').includes(app), call);
        const infos = Array.getSomes(yield* Effect.forEach(bundles, (bundle) => bundleInfo(bundle, call), { concurrency: 'unbounded' }));
        const environment = site.supplied('--env');
        if (infos.length > 0 && infos.every(backgrounded)) {
            return [];
        }
        return whole(
            site,
            Option.match(headlessPath(bundles, infos, rest, (yield* Shell).programs), {
                onNone: () =>
                    launchLine(
                        [...subjects, ...(site.has('--new') ? ['--new'] : [])],
                        documents.map(({ raw }) => raw),
                        environment,
                        rest,
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
            const launching = LAUNCHING.exec(body ?? line);
            return [stack, [{ app: app ?? stack.at(-1), launches: launching !== null, path: launching?.groups?.path }]];
        },
    )[1].flat();

const scripted: Fix = (site) => {
    const lines = [...site.values('-e'), ...Option.toArray(site.command.input)];
    const said = site.values('-l').includes('JavaScript') ? [] : statements(lines);
    const apps = [...new Set(said.map(({ app }) => app).filter(Predicate.isNotUndefined))].map(quote);
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
        return whole(site, launchLine(apps, documents.map(quote)), 'osascript');
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
                onSome: (place) => Effect.map(host.exists(place), (held) => (held ? { ...word, raw: quote(place) } : word)),
            },
        );

const binary: Fix = (site, call) => {
    const { bundle, executable } = BUNDLE.exec(Option.getOrElse(call.host.place(site.head.text, site.command.cwd), () => ''))?.groups ?? {};
    return bundle === undefined || site.has(...(site.row.headless ?? []))
        ? []
        : Effect.gen(function* () {
              const found = yield* bundleInfo(bundle, call);
              const args = yield* Effect.forEach(site.words.slice(1), absoluteArgument(call, site.command.cwd));
              return Option.toArray(Option.filter(found, (info) => info.CFBundleExecutable === executable && !backgrounded(info))).flatMap(() => whole(site, launchLine([quote(bundle)], [], site.assigned, args), site.program, site.assigned[0]));
          });
};

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
        Blender: { informational: ['-v', '--log-list-categories'], headless: ['-b', '--background', '-c', '--command'] },
    },
    every: [binary],
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { APPS };
