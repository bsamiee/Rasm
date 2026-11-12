import { Array, Effect, flow, Option, Predicate, Record, String } from 'effect';
import type { Host } from '#/host';
import { BACKGROUNDED, edited, type Fix, inserted, type Policy, quote, refusal, silent, unassign, when } from '#/policies/fix';
import type { Site } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const HEADLESS = 'analyzeHeadless';
const PROJECT_VARIABLE = 'GHIDRA_PROJECT_DIR';
const PROJECT_WORD = `"$${PROJECT_VARIABLE}"`;
const SERVE = 'Serve.java';
const HEAP = /^GHIDRA_(?:HEADLESS_)?MAXMEM(?:=|$)/u;
const PROJECT_REFERENCE = new RegExp(`^"?\\$\\{?${PROJECT_VARIABLE}\\}?"?$`, 'u');
const PROJECT_PATH = /^(?<base>.*?)\.(?:gpr|rep)(?:\/|$)/u;
const PROJECT_FILE = /^(?<base>.*)\.gpr$/u;
const LOCK = /^(?:(?<stem>[^/]+?)\.?)?lock[*~]*$/u;
const LOGS = { '-log': 'ghidra.log', '-scriptlog': 'ghidra-script.log' };
const RECURSIVE = ['-R', '-r', '-a'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const projectBases = Effect.fnUntraced(function* (host: typeof Host.Service, folder: string, stem: Option.Option<string>) {
    const listed = Effect.orElseSucceed(host.fs.readDirectory(folder), (): readonly string[] => []);
    const projects = Effect.map(listed, (files) =>
        Array.getSomes(
            files.map(
                flow(
                    String.match(PROJECT_FILE),
                    Option.flatMapNullishOr(({ groups }) => groups),
                    Option.flatMap(Record.get('base')),
                ),
            ),
        ),
    );
    return yield* Option.match(
        String.match(PROJECT_PATH)(folder).pipe(
            Option.flatMapNullishOr(({ groups }) => groups),
            Option.flatMap(Record.get('base')),
        ),
        {
            onSome: (named) => Effect.succeed([named]),
            onNone: () => Effect.map(projects, (bases) => bases.filter((base) => Option.match(stem, { onNone: () => true, onSome: (wanted) => base === wanted })).map((base) => host.path.join(folder, base))),
        },
    );
});

const lockedBases = (host: typeof Host.Service, target: string) => {
    const leaf = host.path.basename(target);
    return LOCK.test(leaf)
        ? Effect.map(
              projectBases(
                  host,
                  host.path.dirname(target),
                  String.match(LOCK)(leaf).pipe(
                      Option.flatMapNullishOr(({ groups }) => groups),
                      Option.flatMap(Record.get('stem')),
                      Option.filter(Predicate.isString),
                  ),
              ),
              (bases) => bases.map((base) => ({ leaf, base })),
          )
        : Effect.succeed([]);
};

const lockDeletes = Effect.fnUntraced(function* (site: Site, host: typeof Host.Service, targets: readonly string[]) {
    const shared = yield* host.environment(PROJECT_VARIABLE);
    const spelled = (lock: string): string =>
        Option.match(
            Option.filter(shared, (folder) => host.inside(lock, folder)),
            { onNone: () => quote(lock), onSome: (folder) => `"$${PROJECT_VARIABLE}/${host.path.relative(folder, lock)}"` },
        );
    const found = yield* Effect.forEach(targets, (target) => lockedBases(host, target));
    return found.flat().flatMap(({ leaf, base }) => refusal(`${site.program} ${leaf} deletes Ghidra lock, run lsof -t ${spelled([base, 'lock~'].join('.'))}`));
});

const copied: Fix = Effect.fnUntraced(function* (site, { host }) {
    const sources = Array.getSomes(site.inputs.slice(0, -1).map(({ text }) => host.place(text, site.command.cwd))).filter((place) => site.has(...RECURSIVE) || PROJECT_PATH.test(place));
    const bases = yield* Effect.forEach(sources, (place) => projectBases(host, place, Option.none()));
    return bases.flat().flatMap((base) => refusal(`${site.program} copies Ghidra project ${host.path.basename(base)}, use its ${SERVE}`));
});

const heapAssigned: Fix = (_site, { analysis: { commands } }) => commands.flatMap(({ bound }) => [...bound.keys()]).flatMap((name) => (HEAP.test(name) ? refusal(`${name} overrides the ${HEADLESS} heap, remove it`) : []));

const projectLocation: Fix = Effect.fnUntraced(function* (site, { host }) {
    const shared = Option.flatMap(yield* host.environment(PROJECT_VARIABLE), (folder) => host.place(folder, Option.none()));
    const [location] = site.operands;
    if (location === undefined || PROJECT_REFERENCE.test(location.raw)) {
        return [];
    }
    return Option.exists(host.place(location.text, site.command.cwd), (place) => Option.contains(shared, place)) ? silent(edited(site, location, PROJECT_WORD, `${HEADLESS} ${location.raw}`)) : refusal(`${HEADLESS} project ${location.raw} is not $${PROJECT_VARIABLE}, pass ${PROJECT_WORD}`);
});

const logged: Fix = (site, { host }) =>
    Object.entries(LOGS).flatMap(([flag, file]) => {
        const log = quote(`${host.root}/.artifacts/logs/${file}`);
        const item = site.items.find(({ names }) => names.includes(flag));
        const value = item?.words[1];
        if (item === undefined) {
            return inserted(site, Array.lastNonEmpty(site.words).end, ` ${flag} ${log}`);
        }
        return value === undefined || value.raw === log ? [] : edited(site, value, log);
    });

const concurrent: Fix = (site, { analysis: { sites, jobs } }) => {
    const [, name] = site.operands;
    const socket = name?.known === true ? name.text : '<name>';
    return site.repeated || (jobs.length > 0 && sites.filter(({ program }) => program === HEADLESS).length > 1) ? refusal(site.has('-import') ? `${HEADLESS} runs concurrently, chain each -import with &&` : `${HEADLESS} runs concurrently, send runs to nc -U "$${PROJECT_VARIABLE}/${socket}.sock"`) : [];
};

const longRunning: Fix = (site, { request, host }) => (request.kind === 'foreground' && (site.has('-import') || site.values('-postScript').some((script) => host.path.basename(script) === SERVE)) ? BACKGROUNDED : []);

// --- [TABLES] --------------------------------------------------------------------------

const GHIDRA: Policy = {
    programs: {
        [HEADLESS]: {
            flags: ['-log=', '-scriptlog=', '-scriptPath=', '-propertiesPath=', '-processor=', '-cspec=', '-analysisTimeoutPerFile=', '-keystore=', '-max-cpu=', '-librarySearchPaths=', '-loader=', '-postScript=', '-preScript=', '-import='],
            fixes: [unassign(HEAP), heapAssigned, projectLocation, when(({ operands }) => operands.length > 0, logged), concurrent, longRunning],
        },
        cp: { aliases: ['rsync'], fixes: [copied] },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { GHIDRA, lockDeletes };
