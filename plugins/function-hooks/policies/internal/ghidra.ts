import { Maybe } from '../../monads.ts';
import type { System } from '../command-decision.ts';
import type { Command, Word } from './command.ts';
import { basename, informational, isInside, operands, resolvePath } from './invocation.ts';
import { background, type Call, edit, editOrRefuse, expanded, invocationSites, invocationsOf, quote, refuse, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ANALYZE_HEADLESS = 'analyzeHeadless';
const PROJECT_DIR = 'GHIDRA_PROJECT_DIR';
const SERVE = 'Serve.java';
const HEAP_VARIABLE = /^(?<name>GHIDRA_(?:HEADLESS_)?MAXMEM)(?:=|$)/u;
const PROJECT_REFERENCE = /^"?\$\{?GHIDRA_PROJECT_DIR\}?"?$/u;
const PROJECT_FILE = /\.(?:gpr|rep)$/u;
const LOCK = /^(?<stem>[^/]*?)\.?lock[*~]*$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PROJECTS]

const projectDir = async ({ environment, home }: System): Promise<Maybe<string>> => Maybe.fromNullable((await environment(PROJECT_DIR)).values.flatMap((value) => resolvePath(value, Maybe.none, home).values)[0]);

const serveCommand = (name: string): string => `printf '%s\\n' <program> <script> <argument>... | paste -s - | nc -U "$${PROJECT_DIR}/${name}.sock"`;

const projectBases = async ({ run, plugin }: System, path: string, stem: string): Promise<readonly string[]> => {
    const parts = path.split('/');
    const [base] = parts.flatMap((part, at) => (PROJECT_FILE.test(part) ? [[...parts.slice(0, at), part.replace(PROJECT_FILE, '')].join('/')] : []));
    if (base !== undefined) {
        return [base];
    }
    const { stdout } = await run(['find', path, '-maxdepth', '1', '-name', `${stem}.gpr`], { cwd: plugin, stdin: '' });
    return [...new Set(stdout.split('\n').flatMap((file) => (PROJECT_FILE.test(file) ? [file.replace(PROJECT_FILE, '')] : [])))];
};

const lockDeletes = async (system: System, program: string, path: string): Promise<readonly Verdict[]> => {
    const cut = path.lastIndexOf('/');
    const leaf = path.slice(cut + 1);
    const stem = LOCK.exec(leaf)?.groups?.stem;
    const bases = stem === undefined ? [] : await projectBases(system, path.slice(0, cut), stem || '*');
    if (bases.length === 0) {
        return [];
    }
    const [shared] = (await projectDir(system)).values;
    const locks = bases.map((base) => `${base}.lock~`).map((lock) => (shared !== undefined && isInside(lock, shared) ? `"$${PROJECT_DIR}${lock.slice(shared.length)}"` : quote(lock)));
    return [refuse(`${program} ${leaf} deletes a Ghidra lock that blocks only while held, run lsof -t ${locks.join(' ')}`)];
};

const projectCopies = async ({ system, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const sources = invocationSites(commands)
        .filter(({ invocation: [program] }) => program === 'cp' || program === 'rsync')
        .flatMap(({ command, invocation }) =>
            operands(invocation)
                .inputs.slice(0, -1)
                .map((source) => ({ program: invocation[0], command, source })),
        )
        .flatMap(({ program, command, source }) => resolvePath(expanded(command.words, source), command.cwd, system.home).values.map((place) => ({ program, place })));
    const found = await Promise.all(sources.map(async ({ program, place }) => ({ program, bases: await projectBases(system, place, '*') })));
    return found.flatMap(({ program, bases }) => bases.map((base) => refuse(`${program} copies Ghidra project ${basename(base)}, send requests to its ${SERVE} with ${serveCommand(basename(base))}`)));
};

// --- [HEADLESS]

const heapOverride = (name: string): string => `${name} overrides the ${ANALYZE_HEADLESS} heap, remove it`;

const heapAssignment = (command: Command, word: Word): readonly Verdict[] => {
    const [, name] = HEAP_VARIABLE.exec(word.text) ?? [];
    const next = command.words.find(({ span }) => span.start > word.span.start) ?? word;
    return name === undefined ? [] : [editOrRefuse(command, edit(word.span.start, next.span.start, '', Maybe.some(`${name} removed, ${ANALYZE_HEADLESS} sets the heap`)), heapOverride(name))];
};

const logFlags = (root: string, words: readonly Word[]): readonly Verdict[] =>
    Object.entries({ '-log': 'ghidra.log', '-scriptlog': 'ghidra-script.log' }).flatMap(([flag, file]) => {
        const log = quote(`${root}/.artifacts/logs/${file}`);
        const at = words.findIndex(({ text }) => text === flag);
        const [value, tail] = [words[at + 1], words.at(-1)];
        const notice = Maybe.some(`${ANALYZE_HEADLESS} logs to .artifacts/logs`);
        if (at < 0) {
            return tail === undefined ? [] : [edit(tail.span.end, tail.span.end, ` ${flag} ${log}`, notice)];
        }
        return value === undefined || value.raw === log ? [] : [edit(value.span.start, value.span.end, log, notice)];
    });

const headlessRuns = ({ home, root }: System, folder: Maybe<string>, command: Command): readonly Verdict[] =>
    command.invocations.flatMap((invocation, at) => {
        const words = invocation[0] === ANALYZE_HEADLESS ? (command.parts[at]?.words.slice(1) ?? []) : [];
        const [location] = words;
        const logs = command.origin === 'call' && location !== undefined ? root.values.flatMap((top) => logFlags(top, words)) : [];
        if (location === undefined || PROJECT_REFERENCE.test(location.raw)) {
            return logs;
        }
        const shared = `"$${PROJECT_DIR}"`;
        const derived = !new Set(folder.values).isDisjointFrom(new Set(resolvePath(location.text, command.cwd, home).values));
        return [
            derived
                ? editOrRefuse(command, edit(location.span.start, location.span.end, shared, Maybe.some(`${ANALYZE_HEADLESS} ${location.raw} became ${shared}`)), `${ANALYZE_HEADLESS} project ${location.raw} is ${shared}, pass ${shared}`)
                : refuse(`${ANALYZE_HEADLESS} project ${location.raw} is not $${PROJECT_DIR}, pass ${shared}`),
            ...logs,
        ];
    });

const ghidra = async ({ request, system, script: { commands, assignments } }: Call): Promise<readonly Verdict[]> => {
    const runs = commands.filter(({ invocations: chain }) => chain.some((invocation) => invocation[0] === ANALYZE_HEADLESS && !informational(ANALYZE_HEADLESS, operands(invocation).options)));
    const prefixed = commands.flatMap((command) => command.parts.flatMap(({ assigned }) => assigned).map((word) => ({ command, word })));
    const lasting = invocationsOf(runs, ANALYZE_HEADLESS).some((invocation) => operands(invocation).options.includes('-import') || invocation.slice(1).some((word, at) => invocation[at] === '-postScript' && basename(word) === SERVE));
    const folder = runs.length === 0 ? Maybe.none : await projectDir(system);
    return [
        ...prefixed.flatMap(({ command, word }) => heapAssignment(command, word)),
        ...assignments.flatMap(({ name }) => (HEAP_VARIABLE.test(name) ? [refuse(heapOverride(name))] : [])),
        ...runs.flatMap((run) => headlessRuns(system, folder, run)),
        ...(request.kind === 'foreground' && lasting ? [background(ANALYZE_HEADLESS)] : []),
    ];
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { ANALYZE_HEADLESS, ghidra, lockDeletes, projectCopies, serveCommand };
