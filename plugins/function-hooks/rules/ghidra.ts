import { Array, Effect, Option, Predicate, Record } from 'effect';
import type { Host } from '#/host';
import { Verdict } from '#/rules/decision';
import { deleted, edited, inserted, perSubcommand, quote, type Rule, type Ruleset, refusal, silent, unassign, when } from '#/rules/rule';

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

// --- [OPERATIONS] ----------------------------------------------------------------------

const group = (pattern: RegExp, text: string, name: string) => Option.filter(Option.flatMap(Option.fromNullishOr(pattern.exec(text)?.groups), Record.get(name)), Predicate.isString);

const projectBases = (host: Host['Service'], folder: string, stem: Option.Option<string>) =>
    Option.match(group(PROJECT_PATH, folder, 'base'), {
        onSome: (named) => Effect.succeed([named]),
        onNone: () =>
            Effect.map(
                Effect.orElseSucceed(host.fs.readDirectory(folder), (): readonly string[] => []),
                (files) =>
                    Array.getSomes(files.map((file) => group(PROJECT_FILE, file, 'base')))
                        .filter((base) => Option.isNone(stem) || Option.contains(stem, base))
                        .map((base) => host.path.join(folder, base)),
            ),
    });

const lockDeletion = (...base: readonly string[]): Rule =>
    Effect.fnUntraced(function* (invocation, call) {
        const { host } = call;
        const { words, start, patterns } = deleted(invocation, call, ...base);
        const shared = yield* host.environment(PROJECT_VARIABLE);
        const locks = Array.getSomes(words.filter(({ known }) => known).map(({ text }) => host.resolve(text, start)))
            .flatMap((target) => (patterns.length === 0 ? [target] : patterns.map((pattern) => host.path.join(target, pattern))))
            .filter((target) => LOCK.test(host.path.basename(target)));
        const bases = yield* Effect.forEach(locks, (lock) => projectBases(host, host.path.dirname(lock), group(LOCK, host.path.basename(lock), 'stem')));
        return Array.zip(locks, bases)
            .flatMap(([lock, found]) => found.map((project) => [host.path.basename(lock), `${project}.lock~`] as const))
            .flatMap(([leaf, held]) => {
                const shown = Option.match(
                    Option.filter(shared, (folder) => host.inside(held, folder)),
                    { onNone: () => quote(held), onSome: (folder) => `"$${PROJECT_VARIABLE}/${host.path.relative(folder, held)}"` },
                );
                return refusal(`${invocation.program} ${leaf} deletes Ghidra lock, run lsof -t ${shown}`);
            });
    });

const projectCopy: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const sources = Array.getSomes(invocation.inputs.slice(0, -1).map(({ text }) => host.resolve(text, invocation.command.cwd))).filter((folder) => invocation.has('-R', '-r', '-a') || PROJECT_PATH.test(folder));
    const bases = yield* Effect.forEach(sources, (folder) => projectBases(host, folder, Option.none()));
    return bases.flat().flatMap((base) => refusal(`${invocation.program} copies Ghidra project ${host.path.basename(base)}, use its ${SERVE}`));
});

const heapAssigned: Rule = (_invocation, { analysis: { commands } }) => commands.flatMap(({ variables }) => [...variables.keys()]).flatMap((name) => (HEAP.test(name) ? refusal(`${name} overrides the ${HEADLESS} heap, remove it`) : []));

const projectLocation: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const shared = Option.flatMap(yield* host.environment(PROJECT_VARIABLE), (folder) => host.resolve(folder, Option.none()));
    const [location] = invocation.operands;
    if (location === undefined || PROJECT_REFERENCE.test(location.raw)) {
        return [];
    }
    return Option.exists(host.resolve(location.text, invocation.command.cwd), (folder) => Option.contains(shared, folder)) ? silent(edited(invocation, location, PROJECT_WORD, `${HEADLESS} ${location.raw}`)) : refusal(`${HEADLESS} project ${location.raw} is not $${PROJECT_VARIABLE}, pass ${PROJECT_WORD}`);
});

const logFiles: Rule = (invocation, { host }) =>
    Object.entries(LOGS).flatMap(([flag, file]) => {
        const log = quote(`${host.root}/.artifacts/logs/${file}`);
        const given = invocation.args.find(({ names }) => names.includes(flag));
        const value = given?.words[1];
        if (given === undefined) {
            return inserted(invocation, Array.lastNonEmpty(invocation.words).end, ` ${flag} ${log}`);
        }
        return value === undefined || value.raw === log ? [] : edited(invocation, value, log);
    });

const concurrentRuns: Rule = (invocation, { analysis: { invocations, jobs } }) => {
    const [, name] = invocation.operands;
    const socket = name?.known === true ? name.text : '<name>';
    return invocation.repeated || (jobs.length > 0 && invocations.filter(({ program }) => program === HEADLESS).length > 1) ? refusal(invocation.has('-import') ? `${HEADLESS} runs concurrently, chain each -import with &&` : `${HEADLESS} runs concurrently, send runs to nc -U "$${PROJECT_VARIABLE}/${socket}.sock"`) : [];
};

const longRun: Rule = (invocation, { request, host }) => (request.kind === 'foreground' && (invocation.has('-import') || invocation.values('-postScript').some((script) => host.path.basename(script) === SERVE)) ? [Verdict.Background()] : []);

// --- [COMPOSITION] ---------------------------------------------------------------------

const GHIDRA: Ruleset = {
    programs: {
        [HEADLESS]: {
            flags: ['-log=', '-scriptlog=', '-scriptPath=', '-propertiesPath=', '-processor=', '-cspec=', '-analysisTimeoutPerFile=', '-keystore=', '-max-cpu=', '-librarySearchPaths=', '-loader=', '-postScript=', '-preScript=', '-import='],
            rules: [unassign(HEAP), heapAssigned, projectLocation, when(({ operands }) => operands.length > 0, logFiles), concurrentRuns, longRun],
        },
        cp: { aliases: ['rsync'], rules: [projectCopy] },
        rm: { rules: [lockDeletion()] },
        find: { rules: [when(({ has }) => has('-delete'), lockDeletion())] },
        git: { rules: [perSubcommand({ rm: [when(({ has }) => !has('--cached'), lockDeletion('-C'))] })] },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { GHIDRA };
