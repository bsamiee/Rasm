import { Option } from '../monads.ts';
import { bundleId } from './apps.ts';
import { applyEdits, type Command, parse, runJson, type Script, type Word } from './command.ts';
import { basename, enabled, firstOperand, type Invocation, informational, invocations, isInside, isOption, operands, option, programRow, readsStdin, resolvePath, unquote } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

interface Refusal {
    readonly kind: 'refuse';
    readonly reason: string;
}
type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string>; readonly timeout: Option<number>; readonly background: boolean };
type Request = { readonly kind: 'foreground'; readonly timeout: Option<number> } | { readonly kind: 'background' | 'monitor' | 'plain' };
type Verdict = Refusal | { readonly kind: 'edit'; readonly start: number; readonly end: number; readonly text: string; readonly notice: Option<string>; readonly timeout: Option<number> } | { readonly kind: 'background'; readonly notice: string };
type Route = Refusal | { readonly kind: 'rewrite'; readonly program: string; readonly start: Word; readonly text: string };
type Statement = { readonly kind: (typeof STATEMENTS)[number][0] | 'drive' } | { readonly kind: 'open'; readonly path: string } | { readonly kind: 'shell'; readonly inner: string };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly read: (path: string) => Promise<string>;
    readonly environment: (name: string) => Promise<Option<string>>;
    readonly scanner: string;
    readonly plugin: string;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}
interface Call {
    readonly system: System;
    readonly source: Uint8Array;
    readonly script: Script;
    readonly project: string;
    readonly request: Request;
    readonly apps: ReadonlyMap<string, string>;
    readonly writers: ReadonlyMap<string, string>;
    readonly routes: ReadonlyMap<Command, Route>;
}
interface Target {
    readonly options?: { readonly command?: string; readonly commands?: readonly (string | { readonly command: string })[] };
    readonly dependsOn?: readonly (string | { readonly target: string })[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates a second checkout, work in the main checkout';
const BRANCH = 'creates a branch, stay on the current branch';
const UNRESOLVED_FOLDER = 'runs in an unresolved folder, cd to a literal path';
const ALIAS = 'defines an alias hiding refused subcommands, run the subcommands directly';
const OVERWRITES = 'overwrites edits, use the Edit tool';
const ANALYZE_HEADLESS = 'analyzeHeadless';
const PROJECT_DIR = 'GHIDRA_PROJECT_DIR';
const SERVE = 'Serve.java';
const MAX_TIMEOUT = 600_000;
const ESCAPED_NEWLINE = /(?<!\\)(?:\\\\)*\\n/u;
const EXPANSION = /[$`]/u;
const GLOB = /[*?[{]/u;
const NAME_CHARACTER = /[\p{L}\p{N}]/u;
const SHELL_SAFE = /^[\w@%+=:,./-]+$/u;
const LETTER = /^\w/u;
const HEAP_VARIABLE = /^(?<name>GHIDRA_(?:HEADLESS_)?MAXMEM)(?:=|$)/u;
const PROJECT_REFERENCE = /^"?\$\{?GHIDRA_PROJECT_DIR\}?"?$/u;
const PROJECT_FILE = /\.(?:gpr|rep)$/u;
const LOCK = /^(?<stem>[^/]*?)\.?lock[*~]*$/u;
const DURATION = /^(?<value>\d+(?:\.\d+)?)(?<unit>[smh]?)$/u;
const BUNDLE = /^(?<bundle>\/.+?\.app)\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const EXECUTABLE = /\/Contents\/MacOS\/(?<executable>[^/]+)$/u;
const SUBSTITUTION = /\$\(|`/u;
const TASK_OUTPUT = /\/tasks\/(?<id>[^/]+)\.output$/u;
const TASK_OUTPUTS = /\/tasks\/(?<id>[^/\s'"]+)\.output/gu;
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
const BUNDLE_ID = /^bundleid=(?<id>.+)$/iu;
const WEB_URL = /^https?:/iu;
const PID = /^\d+$/u;
const BACKGROUND_ONLY = /\btype="BackgroundOnly"/u;
const SIGNAL_PREFIX = /^-?(?:SIG)?/u;
const SPACES = /\s+/u;
const WATCHER = 'watch-apps';
const ARGUMENT = '--arguments';
const DOCUMENT = '--document';
const CHECK_TARGET = /^(?:lint|format|typecheck):(?<tool>[^:]+)/u;
const CACHED_TARGET = /^(?:check|typecheck(?::.+)?)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [VERDICTS]

const refuse = (reason: string): Refusal => ({ kind: 'refuse', reason });

const edit = (start: number, end: number, text: string, notice: Option<string> = Option.none, timeout: Option<number> = Option.none): Verdict => ({ kind: 'edit', start, end, text, notice, timeout });

const background = (cause: string): Verdict => ({ kind: 'background', notice: `Call runs in the background for ${cause}, wait for its completion notice` });

const editOrRefuse = (command: Command, change: Verdict, reason: string): Verdict => (command.origin === 'call' ? change : refuse(reason));

// --- [WORDS]

const quote = (word: string): string => (SHELL_SAFE.test(word) ? word : `'${word.replaceAll("'", String.raw`'\''`)}'`);

const watchApps = (...words: readonly string[]): string => [WATCHER, ...words].join(' ');

const expanded = (words: readonly Word[], text: string): string => {
    const [spoken] = words.filter((word) => word.expands && word.text === text);
    return spoken === undefined || !EXPANSION.test(text) ? text : unquote(spoken.literal);
};

const argumentGroups = (program: string, [head, ...rest]: readonly Word[]): readonly { readonly words: readonly Word[]; readonly names: readonly string[] }[] => {
    if (head === undefined || head.text === '--') {
        return [];
    }
    const { names, taken } = isOption(programRow(program), head.text) ? option(programRow(program), head.text) : { names: [], taken: 0 };
    return [{ words: [head, ...rest.slice(0, taken)], names }, ...argumentGroups(program, rest.slice(taken))];
};

// --- [COMMANDS]

const invocationsOf = (commands: readonly Command[], ...programs: readonly string[]): readonly Invocation[] => commands.flatMap(({ invocations: chain }) => chain.filter(([program]) => programs.includes(program)));

const invocationSites = (commands: readonly Command[]): readonly { readonly command: Command; readonly index: number; readonly invocation: Invocation; readonly at: number }[] => commands.flatMap((command, index) => command.invocations.map((invocation, at) => ({ command, index, invocation, at })));

const nestedIn = (commands: readonly Command[], command: Command): readonly Command[] => commands.filter((other) => other !== command && other.extent.start >= command.extent.start && other.extent.end <= command.extent.end);

const pipelineNeighbors = (commands: readonly Command[], index: number, step: 1 | -1): readonly Command[] => {
    const next = commands[index + step];
    return (step > 0 ? commands[index] : next)?.pipesOutput === true && next !== undefined ? [next, ...pipelineNeighbors(commands, index + step, step)] : [];
};

// --- [DELETE]

const deletedPath =
    (system: System, command: Command, name: string, start: Option<string>, patterns: readonly string[], covered: (word: string) => string) =>
    async (word: string): Promise<readonly Verdict[]> => {
        const { root, home } = system;
        const tail = command.parts.at(-1)?.words ?? [];
        const spoken = tail.filter(({ text, expands }) => expands && text === word);
        const path = expanded(tail, word);
        if (spoken.length > 0 && EXPANSION.test(path)) {
            return [refuse(`${name} ${word} expands at run time, pass a literal path`)];
        }
        const cut = spoken.length > 0 ? path.search(GLOB) : -1;
        const [place] = resolvePath(cut < 0 ? path : path.slice(0, path.lastIndexOf('/', cut) + 1), start, home).values;
        if (place === undefined) {
            return [refuse(`${name} ${UNRESOLVED_FOLDER}`)];
        }
        if (!patterns.some((pattern) => NAME_CHARACTER.test(pattern)) && root.values.some((top) => isInside(top, place))) {
            return [refuse(covered(path))];
        }
        const edits = path === word ? [] : spoken.map(({ raw, span, literal }) => editOrRefuse(command, edit(span.start, span.end, literal, Option.some(`${name} ${raw} became ${literal}`)), `${name} ${raw} expands at run time, pass ${literal}`));
        const [target = place] = resolvePath(path, start, home).values;
        const targets = patterns.length === 0 ? [target] : patterns.map((pattern) => `${target}/${pattern}`);
        return [...edits, ...(await Promise.all(targets.map((candidate) => lockDeletes(system, name, candidate)))).flat()];
    };

const deletes = async ({ system, script: { commands }, project }: Call): Promise<readonly Verdict[]> => {
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return ['rm', 'rimraf', 'trash', 'unlink', 'shred'].includes(invocation[0]) ? [invocation] : [];
        }
        const [key, ...args] = invocation.slice(firstOperand(invocation));
        return key === 'rm' && !args.includes('--cached') ? [['rm', ...args]] : [];
    };
    const judged = commands.flatMap((command): readonly (readonly Verdict[] | Promise<readonly Verdict[]>)[] => {
        const { invocations: chain, cwd } = command;
        const last = chain.at(-1);
        if (last === undefined) {
            return [];
        }
        const [program, ...args] = last;
        const { options, inner, roots, base, leading, values } = operands(last);
        const [remover] = removing(last);
        const [feeder] = chain.slice(0, -1).filter(([wrapper]) => programRow(wrapper).stdin === 'always');
        const name = program === 'git' ? 'git rm' : program;
        if (remover !== undefined) {
            return feeder === undefined ? [...new Set(operands(remover).inputs)].map(deletedPath(system, command, name, cwd, [], (word) => `${name} ${word} covers the repository root, run nx run ${project}:clean`)) : [[refuse(`${name} reads paths from ${feeder[0]}, use fd -X ${name}`)]];
        }
        if (!(options.includes('-delete') || invocations(inner).some((launched) => removing(launched).length > 0))) {
            return [];
        }
        const tests = ['-name', '-iname', '-path', '-ipath', '-wholename', '-iwholename', '-regex', '-iregex'];
        const patterns = program === 'find' || program === 'gfind' ? args.flatMap((word, at) => (tests.includes(word) ? args.slice(at + 1, at + 2) : [])) : [...leading, ...values.flatMap(([flag, value]) => (flag === '-e' || flag === '--extension' ? [value] : []))];
        const folder = base.reduce((place, word) => resolvePath(word, place, system.home), cwd);
        return [...new Set(roots)].map(deletedPath(system, command, program, folder, patterns, (word) => `${program} deletes everything under ${word}, add a -name pattern`));
    });
    return (await Promise.all(judged)).flat();
};

// --- [GIT]

const flagReasons = (args: readonly string[], reason: string, words: readonly string[], prefixes: readonly string[]): readonly string[] => args.filter((word) => words.includes(word) || prefixes.some((prefix) => word.startsWith(prefix))).map((word) => `${word} ${reason}`);

const GIT = new Map(
    Object.entries<(args: readonly string[], present: (word: string) => boolean) => readonly string[]>({
        branch: (args) => {
            const listing = args.some((word) => ['-l', '--list', '-a', '--all', '-r', '--remotes', '-v', '-vv', '--verbose', '--show-current', '--contains', '--no-contains', '--merged', '--no-merged', '--points-at'].includes(word));
            return [...flagReasons(args, 'deletes or force-moves a branch', ['-d', '-D', '-M', '--delete'], ['--force']), ...flagReasons(args, BRANCH, ['-c', '-C', '--copy', '-m', '--move'], []), ...(!listing && args.some((word) => !word.startsWith('-')) ? [BRANCH] : [])];
        },
        checkout: (args, present) => {
            const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
            const paths = args.includes('--') || more.length > 0 || first === '.' || first?.startsWith(':') === true || (first !== undefined && first !== '-' && present(first));
            return [...flagReasons(args, 'discards local changes', ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], ['--force']), ...flagReasons(args, BRANCH, ['-b', '--orphan', '-t', '--track'], []), ...(args.includes('--detach') || !paths ? [] : [`${first ?? '--'} ${OVERWRITES}`])];
        },
        clean: () => ['deletes untracked files, rm each file by name'],
        config: (args) => args.flatMap((word, at) => (word.startsWith('alias.') && args.slice(at + 1).some((value) => !value.startsWith('-')) ? [`${word} ${ALIAS}`] : [])),
        push: (args) => flagReasons(args, 'rewrites or deletes remote history', ['-f', '-d', '--delete', '--mirror', '--prune'], ['--force', '+', ':']),
        rebase: () => ['rewrites commits other sessions hold'],
        reflog: (args) => flagReasons(args, 'deletes reflog entries', ['delete', 'drop', 'expire'], []),
        reset: (args, present) => {
            const forced = flagReasons(args, 'discards working-tree or index state', ['--hard', '--merge', '--keep'], []);
            const targets = args.filter((word) => !word.startsWith('-'));
            const [target] = targets;
            return forced.length > 0 || target === undefined || args.includes('--') || targets.some(present) ? forced : [`${target} moves HEAD and drops branch commits`];
        },
        restore: (args) => {
            const { options } = operands(['git', ...args]);
            return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [OVERWRITES];
        },
        revert: () => ['reverses committed history'],
        stash: ([first]) => (first === 'list' || first === 'show' ? [] : ['hides uncommitted work other sessions need']),
        switch: (args) => [...flagReasons(args, 'discards local changes', ['-f', '-C', '--discard-changes'], ['--force']), ...flagReasons(args, BRANCH, ['-c', '--create', '--orphan'], [])],
        worktree: ([first]) => (first === 'list' ? [] : [WORKTREE]),
    }),
);

const git = async ({ system: { exists, root, home }, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const [top] = root.values;
    if (top === undefined) {
        return [];
    }
    const calls = invocationSites(commands)
        .filter(({ invocation: [program] }) => program === 'git')
        .flatMap(({ command, invocation }) => {
            const index = firstOperand(invocation);
            const { values } = operands(['git', ...invocation.slice(1, index)]);
            const moved = values.reduce((folder, [name, value]) => (name === '-C' ? resolvePath(value, folder, home) : folder), command.cwd);
            const [key, ...args] = invocation.slice(index);
            return key === undefined || moved.values.some((folder) => !isInside(folder, top)) ? [] : [{ values, key, args, moved }];
        });
    const probed = calls.flatMap(({ args, moved }) => args.filter((word) => !word.startsWith('-')).flatMap((word) => resolvePath(word, moved, home).values));
    const found = new Set((await Promise.all(probed.map(async (path) => ((await exists(path)) ? [path] : [])))).flat());
    return calls.flatMap(({ values, key, args, moved }) => {
        const aliased = values.flatMap(([name, value]) => (value.startsWith('alias.') ? [refuse(`git ${name} ${value} ${ALIAS}`)] : []));
        const present = (word: string): boolean => resolvePath(word, moved, home).values.some((place) => found.has(place));
        const [tail] = GIT.get(key)?.(args, present) ?? [];
        const [anywhere] = GIT.get(key)?.(args, () => true) ?? [];
        if (aliased.length > 0) {
            return aliased;
        }
        return moved.match(
            () => (tail === undefined ? [] : [refuse(`git ${key} ${tail}`)]),
            () => (tail === undefined && anywhere === undefined ? [] : [refuse(`git ${key} ${UNRESOLVED_FOLDER}`)]),
        );
    });
};

// --- [SEARCH_ROOT]

const searchRoots = ({ system: { root, home }, script: { commands } }: Call): readonly Verdict[] =>
    commands.flatMap(({ invocations: chain, cwd }) => {
        const last = chain.at(-1);
        if (last === undefined || chain.slice(0, -1).some(([name]) => programRow(name).stdin === 'always')) {
            return [];
        }
        const { roots, base } = operands(last);
        const folder = base.reduce((place, word) => resolvePath(word, place, home), cwd);
        const wide = (place: string): boolean => place.lastIndexOf('/') === 0 || home.values.some((user) => isInside(user, place)) || root.values.some((top) => top !== place && isInside(top, place));
        return roots.flatMap((word) => resolvePath(word, folder, home).values.filter(wide)).map((place) => refuse(`${last[0]} root ${place} is too wide, pass a package folder`));
    });

// --- [REPLACE]

const noOpReplacements = ({ script: { commands } }: Call): readonly Verdict[] =>
    invocationSites(commands).flatMap(({ invocation }) => {
        const [program] = invocation;
        const { previews } = programRow(program);
        const { leading, inputs, options } = operands(invocation);
        const [find, replacement] = leading;
        if (previews === undefined || find === undefined) {
            return [];
        }
        const preview = `preview with ${program} ${previews[0]}`;
        if (find === '') {
            return [refuse(`${program} empty pattern matches everywhere, ${preview}`)];
        }
        const [line] = find.split('\n');
        return find === replacement && inputs.length > 0 && !options.some((name) => previews.includes(name)) ? [refuse(`${program} replaces ${quote(line ?? find)} with itself, ${preview}`)] : [];
    });

// --- [INPUT]

const inputSources = ({ system: { home }, script: { commands } }: Call): readonly Verdict[] => {
    const written = commands.map((writer) => writer.heredocFiles.flatMap((path) => resolvePath(path, writer.cwd, home).values));
    return invocationSites(commands).flatMap(({ command: { hasInput, cwd }, index, invocation }) => {
        const heredocFiles = new Set(written.slice(0, index).flat());
        const ran = (word: string): boolean => resolvePath(word, cwd, home).values.some((path) => heredocFiles.has(path));
        const [program] = invocation;
        const { code, shell } = programRow(program);
        const { options, inputs } = operands(invocation);
        const coded = code !== undefined && (options.some((name) => code.includes(name)) || (hasInput && readsStdin(invocation)));
        return [...(coded || ((code ?? shell) !== undefined && inputs.slice(0, 1).some(ran)) ? [refuse(`${program} runs inline code, use Edit or the installed tool`)] : []), ...(!hasInput && readsStdin(invocation) ? [refuse(`${program} waits on stdin with no input, pass a file or pipe`)] : [])];
    });
};

// --- [WAIT]

const selfMatches = ({ script: { commands } }: Call): readonly Verdict[] =>
    invocationSites(commands).flatMap(({ command, index, invocation, at }) => {
        const [program] = invocation;
        const { options } = operands(invocation);
        const listing = commands[index - 1];
        const grepped = program === 'grep' && command.hasInput && at === 0 && listing?.pipesOutput === true && listing.invocations.at(-1)?.[0] === 'ps' && options.length === 0;
        const word = ((program === 'pgrep' && options.includes('-f')) || grepped) && command.origin !== 'file' ? argumentGroups(program, command.parts[at]?.words.slice(1) ?? []).find(({ names }) => names.length === 0)?.words[0] : undefined;
        const text = word === undefined || word.expands || !LETTER.test(word.text) ? undefined : quote(`[${word.text.slice(0, 1)}]${word.text.slice(1)}`);
        return word === undefined || text === undefined ? [] : [editOrRefuse(command, edit(word.span.start, word.span.end, text), `${program} ${word.raw} matches its own shell, use ${text}`)];
    });

const removedSleeps = ({ request, routes, script: { commands } }: Call): readonly { readonly command: Command; readonly verdict: Verdict }[] => {
    const own = commands.filter(({ origin }) => origin === 'call').toSorted((left, right) => left.extent.start - right.extent.start);
    const driving = own.some((command) => !command.inLoop && (routes.get(command)?.kind === 'rewrite' || command.invocations.some(([program]) => program === WATCHER)));
    const answered = request.kind === 'foreground' || request.kind === 'plain';
    return own.flatMap((command, at) => {
        const [only, ...more] = command.invocations;
        const [next, last] = [own[at + 1], own[at - 1]];
        if (!(driving && answered && only?.[0] === 'sleep' && more.length === 0) || command.inLoop || command.hasInput || command.pipesOutput) {
            return [];
        }
        const [start, end] = next === undefined ? [last?.extent.end ?? command.extent.start, command.extent.end] : [command.extent.start, next.extent.start];
        return [{ command, verdict: edit(start, end, '', Option.some(`sleep removed, ${WATCHER} prints current states`)) }];
    });
};

const taskNotice = (id: string): string => `Task ${id} notifies on completion, continue until its notice arrives`;

const pollingLoops = ({ request, apps, source, script: { commands } }: Call): readonly string[] => {
    const loopCommands = commands.filter(({ inLoop }) => inLoop);
    const sleeping = loopCommands.some(({ invocations: chain }) => chain.some(([program]) => enabled(programRow(program).timer, [])));
    const waited = sleeping ? [...new TextDecoder().decode(source).matchAll(TASK_OUTPUTS)].flatMap((match) => Option.fromNullable(match.groups?.id).values) : [];
    const names = invocationSites(loopCommands).flatMap(({ invocation }) =>
        invocation[0] === WATCHER
            ? [watchedApps(invocation)]
            : namedPids(invocation)
                  .flatMap((pid) => Option.fromNullable(apps.get(pid)).values)
                  .map((bundle) => [bundle]),
    );
    return [...[...new Set(waited)].map(taskNotice), ...names.map((name) => `Loop polls ${name.join(' ') || WATCHER}, ${watchAction(request, name)}`)];
};

const tailedTasks = ({ invocations: chain }: Command): readonly string[] => chain.flatMap((invocation) => (invocation[0] === 'tail' && enabled(programRow('tail').endless, operands(invocation).options) ? operands(invocation).inputs.filter((file) => TASK_OUTPUT.test(file)) : []));

const taskWriters = async (system: System, request: Request, commands: readonly Command[]): Promise<ReadonlyMap<string, string>> => {
    const files = request.kind === 'monitor' ? [...new Set(commands.flatMap(tailedTasks))] : [];
    const probed = await Promise.all(
        files.map(async (file) => {
            const holders = (await system.run(['lsof', '-t', file], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((pid) => PID.test(pid));
            const table = holders.length === 0 ? '' : (await system.run(['ps', '-o', 'pid=,ppid=', '-p', holders.join(',')], { cwd: system.plugin, stdin: '' })).stdout;
            const [root] = table.split('\n').flatMap((row) => {
                const [pid, parent] = row.trim().split(SPACES);
                return pid !== undefined && PID.test(pid) && !holders.includes(parent ?? '') ? [pid] : [];
            });
            return root === undefined ? [] : [[file, root] as const];
        }),
    );
    return new Map(probed.flat());
};

const taskTails = ({ request, writers, script: { commands } }: Call): readonly Verdict[] =>
    request.kind === 'monitor'
        ? commands.flatMap((command, index) => {
              const files = tailedTasks(command);
              const words = command.parts[command.invocations.findIndex(([program]) => program === 'tail')]?.words ?? [];
              const [first, last] = [words[0], words.at(-1)];
              const ended = invocationSites(pipelineNeighbors(commands, index, 1)).some(({ invocation }) => enabled(programRow(invocation[0]).closesInput, operands(invocation).options));
              const roots = files.flatMap((file) => Option.fromNullable(writers.get(file)).values);
              const ids = files.flatMap((file) => Option.fromNullable(TASK_OUTPUT.exec(file)?.groups?.id).values).join(' ');
              if (files.length === 0 || first === undefined || last === undefined || ended) {
                  return [];
              }
              return roots.length < files.length ? [refuse(taskNotice(ids))] : [edit(first.span.start, first.span.start, '{ '), edit(last.span.end, last.span.end, ` & ${roots.map((pid) => `caffeinate -w ${pid}`).join('; ')}; kill $!; }`, Option.some(`tail ends with task ${ids}`))];
          })
        : [];

const foregroundWaits = (call: Call): readonly Verdict[] => {
    const { request, script } = call;
    const { commands } = script;
    const dropped = removedSleeps(call);
    const polling = pollingLoops(call).length > 0;
    const watched = invocationsOf(commands, WATCHER);
    const waitReason = watched.length === 0 ? 'Command waits in the foreground, use Monitor or run_in_background' : `Command waits on ${WATCHER} in the foreground, ${watchAction(request, watched.flatMap(watchedApps))}`;
    const kept = commands.filter((command) => !(dropped.some((drop) => drop.command === command) || (polling && command.inLoop)));
    return [
        ...dropped.map(({ verdict }) => verdict),
        ...invocationSites(kept).flatMap(({ command, invocation, at }) => {
            const [program] = invocation;
            const { timer, endless } = programRow(program);
            const { options, values } = operands(invocation);
            const holding = at === command.invocations.length - 1 && (enabled(timer, options) || enabled(endless, options));
            const [first] = command.parts[at]?.words ?? [];
            const next = command.parts[at + 1]?.words[0];
            const [amount] = values.flatMap(([name, value]) => (name === '-t' ? [value] : []));
            const leaking = program === 'read' && enabled(timer, options) && command.hasInput && nestedIn(commands, command).some(({ inSubstitution }) => inSubstitution);
            return [
                ...(request.kind === 'foreground' && (holding || command.inBusyLoop) ? [refuse(waitReason)] : []),
                ...(program === 'caffeinate' && first !== undefined && next !== undefined ? [editOrRefuse(command, edit(first.span.start, next.span.start, '', Option.some(`caffeinate removed, ${next.text} runs directly`)), `caffeinate wraps ${next.text}, run ${next.text} directly`)] : []),
                ...(leaking && amount !== undefined ? [editOrRefuse(command, edit(command.extent.start, command.extent.end, `sleep ${amount}`, Option.some(`read -t ${amount} became sleep ${amount}`)), `read -t ${amount} leaks its input process, use sleep ${amount}`)] : []),
            ];
        }),
    ];
};

const backgroundJobs = ({ request, source, script: { commands, jobs } }: Call): readonly Verdict[] => {
    const [job, ...more] = jobs;
    if (job === undefined || invocationsOf(commands, 'wait').length > 0) {
        return [];
    }
    const trailing = more.length === 0 && job.origin === 'call' && new TextDecoder().decode(source.subarray(job.end)).trim() === '';
    if (!trailing) {
        return [refuse(`& job outlives the call, ${request.kind === 'plain' ? 'run it as its own call' : 'set run_in_background'}`)];
    }
    return [edit(job.start, job.end, ''), ...(request.kind === 'foreground' ? [background('&')] : [])];
};

// --- [GHIDRA]

const heapOverride = (name: string): string => `${name} overrides the ${ANALYZE_HEADLESS} heap, remove it`;

const heapAssignment = (command: Command, word: Word): readonly Verdict[] => {
    const [, name] = HEAP_VARIABLE.exec(word.text) ?? [];
    const next = command.words.find(({ span }) => span.start > word.span.start) ?? word;
    return name === undefined ? [] : [editOrRefuse(command, edit(word.span.start, next.span.start, '', Option.some(`${name} removed, ${ANALYZE_HEADLESS} sets the heap`)), heapOverride(name))];
};

const projectDir = async ({ environment, home }: System): Promise<Option<string>> => Option.fromNullable((await environment(PROJECT_DIR)).values.flatMap((value) => resolvePath(value, Option.none, home).values)[0]);

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

const headlessRuns = ({ home, root }: System, folder: Option<string>, command: Command): readonly Verdict[] =>
    command.invocations.flatMap((invocation, at) => {
        const words = invocation[0] === ANALYZE_HEADLESS ? (command.parts[at]?.words.slice(1) ?? []) : [];
        const [location] = words;
        const logs = command.origin === 'call' && location !== undefined ? root.values.flatMap((top) => logFlags(top, words)) : [];
        if (location === undefined || PROJECT_REFERENCE.test(location.raw)) {
            return logs;
        }
        const shared = `"$${PROJECT_DIR}"`;
        const derived = resolvePath(location.text, command.cwd, home).values.some((place) => folder.toArray().includes(place));
        return [
            derived
                ? editOrRefuse(command, edit(location.span.start, location.span.end, shared, Option.some(`${ANALYZE_HEADLESS} ${location.raw} became ${shared}`)), `${ANALYZE_HEADLESS} project ${location.raw} is ${shared}, pass ${shared}`)
                : refuse(`${ANALYZE_HEADLESS} project ${location.raw} is not $${PROJECT_DIR}, pass ${shared}`),
            ...logs,
        ];
    });

const ghidra = async ({ request, system, script: { commands, assignments } }: Call): Promise<readonly Verdict[]> => {
    const runs = commands.filter(({ invocations: chain }) => chain.some((invocation) => invocation[0] === ANALYZE_HEADLESS && !informational(ANALYZE_HEADLESS, operands(invocation).options)));
    const prefixed = commands.flatMap((command) => command.parts.flatMap(({ assigned }) => assigned).map((word) => ({ command, word })));
    const lasting = invocationsOf(runs, ANALYZE_HEADLESS).some((invocation) => operands(invocation).options.includes('-import') || invocation.slice(1).some((word, at) => invocation[at] === '-postScript' && basename(word) === SERVE));
    const folder = runs.length === 0 ? Option.none : await projectDir(system);
    return [
        ...prefixed.flatMap(({ command, word }) => heapAssignment(command, word)),
        ...assignments.flatMap(({ name }) => (HEAP_VARIABLE.test(name) ? [refuse(heapOverride(name))] : [])),
        ...runs.flatMap((run) => headlessRuns(system, folder, run)),
        ...(request.kind === 'foreground' && lasting ? [background(ANALYZE_HEADLESS)] : []),
    ];
};

const logFlags = (root: string, words: readonly Word[]): readonly Verdict[] =>
    Object.entries({ '-log': 'ghidra.log', '-scriptlog': 'ghidra-script.log' }).flatMap(([flag, file]) => {
        const log = quote(`${root}/.artifacts/logs/${file}`);
        const at = words.findIndex(({ text }) => text === flag);
        const [value, tail] = [words[at + 1], words.at(-1)];
        const notice = Option.some(`${ANALYZE_HEADLESS} logs to .artifacts/logs`);
        if (at < 0) {
            return tail === undefined ? [] : [edit(tail.span.end, tail.span.end, ` ${flag} ${log}`, notice)];
        }
        return value === undefined || value.raw === log ? [] : [edit(value.span.start, value.span.end, log, notice)];
    });

// --- [PIPELINE]

const pipefail = ({ script: { commands } }: Call): readonly Verdict[] => {
    const own = commands.filter(({ origin }) => origin === 'call');
    const set = invocationsOf(own, 'set').some((invocation) => operands(invocation).values.some(([, value]) => value === 'pipefail'));
    const masked = own.some(({ pipesOutput, invocations: chain }) => pipesOutput && chain.slice(-1).some(([program]) => programRow(program).exitStatus === true));
    const closing = own.some(({ hasInput, invocations: chain }) => hasInput && chain.some((invocation) => enabled(programRow(invocation[0]).closesInput, operands(invocation).options)));
    return masked && !set && !closing ? [edit(0, 0, 'set -o pipefail; ')] : [];
};

// --- [RERUN]

const loopReruns = ({ script: { commands, jobs } }: Call): readonly Verdict[] => {
    const runs = commands.filter(({ invocations: chain }) => chain.some(([program]) => program === ANALYZE_HEADLESS));
    const fanning = runs.filter(({ inLoop }) => inLoop || (runs.length > 1 && jobs.length > 0));
    const looping = commands.filter(({ inLoop }) => inLoop);
    return [
        ...(invocationsOf(looping, 'nx').some((invocation) => operands(invocation).inputs[0] !== 'show') ? [refuse('Loop reruns nx tasks, use one nx run-many')] : []),
        ...invocationsOf(fanning, ANALYZE_HEADLESS).map((invocation) => {
            const { options, inputs } = operands(invocation);
            const [, name] = inputs;
            const project = name === undefined || EXPANSION.test(name) ? '<name>' : name;
            return refuse(`${ANALYZE_HEADLESS} fans out per input, ${options.includes('-import') ? 'chain one -import per project with &&' : `send each run with ${serveCommand(project)}`}`);
        }),
    ];
};

// --- [APPS]

const watchAction = ({ kind }: Request, names: readonly string[]): string => {
    const shown = names.length === 0 ? ['<app>'] : names.map(quote);
    return kind === 'plain' ? `run ${watchApps('read', ...shown)}` : `run ${watchApps('watch', ...shown)} in Monitor`;
};

const watchedApps = (invocation: Invocation): readonly string[] => {
    const [subcommand, ...names] = operands(invocation).inputs;
    return subcommand === 'perform' || subcommand === 'set' ? [] : names;
};

const killSignal = ([, ...args]: Invocation): { readonly force: Option<readonly string[]>; readonly pids: readonly string[] } => {
    const [first] = args;
    const valued = first === '-s' || first === '-n';
    const cut = valued ? 2 : Number(first !== undefined && first !== '--' && first.startsWith('-'));
    const quits: Readonly<Record<string, readonly string[]>> = { TERM: [], 15: [], KILL: ['--force'], 9: ['--force'] };
    const [signal = 'TERM'] = args.slice(Math.max(cut - 1, 0), cut).map((word) => word.toUpperCase().replace(SIGNAL_PREFIX, ''));
    return { force: Option.fromNullable(quits[signal]), pids: args.slice(cut).filter((word) => word !== '--') };
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

const killRoute = (apps: ReadonlyMap<string, string>, head: Word, invocation: Invocation): Option<Route> => {
    const { force, pids } = killSignal(invocation);
    const [flags] = force.values;
    if (flags === undefined || pids.length === 0 || !new Set(pids).isSubsetOf(apps)) {
        return Option.none;
    }
    const quits = pids.map((pid) => watchApps('quit', '--pid', pid, ...flags));
    return Option.some({ kind: 'rewrite', program: 'kill', start: head, text: quits.length === 1 ? quits.join('') : `{ ${quits.join('; ')}; }` });
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

const appPolls = (call: Call): readonly Verdict[] => {
    const { request, script } = call;
    const { commands } = script;
    const streaming = invocationsOf(commands, WATCHER).filter((invocation) => operands(invocation).inputs[0] === 'watch' && !informational(WATCHER, operands(invocation).options));
    return [...pollingLoops(call).map(refuse), ...(request.kind === 'monitor' ? [] : streaming.map((invocation) => refuse(`${WATCHER} watch never exits, ${watchAction(request, watchedApps(invocation))}`)))];
};

const bundleInfo = async (system: System, bundle: string): Promise<Option<{ readonly executable: string; readonly identifier: string }>> => {
    const info = await runJson<{ readonly CFBundleExecutable?: string; readonly CFBundleIdentifier?: string }>(system, ['plutil', '-convert', 'json', '-o', '-', `${bundle}/Contents/Info.plist`], system.plugin);
    const [{ CFBundleExecutable, CFBundleIdentifier } = {}] = info.toArray();
    return CFBundleExecutable !== undefined && CFBundleIdentifier !== undefined ? Option.some({ executable: CFBundleExecutable, identifier: CFBundleIdentifier }) : Option.none;
};

const headlessExecutable = async (system: System, cwd: Option<string>, app: Word, identifying: boolean, passed: readonly Word[]): Promise<Option<string>> => {
    const file = app.text.endsWith('.app') ? app.text : `${app.text}.app`;
    const query = identifying ? `kMDItemCFBundleIdentifier == "${app.text}"` : `kMDItemContentType == "com.apple.application-bundle" && kMDItemFSName == "${file}"c`;
    const [bundle, ...more] = app.text.includes('/') ? resolvePath(app.text, cwd, system.home).values : (await system.run(['mdfind', query], { cwd: system.plugin, stdin: '' })).stdout.split('\n').filter((line) => line !== '');
    const [run] = bundle === undefined || more.length > 0 ? [] : (await bundleInfo(system, bundle)).values;
    return bundle !== undefined && run !== undefined && programRow(run.executable).headless?.some((flag) => passed.some(({ text }) => text === flag)) === true ? Option.some(quote(`${bundle}/Contents/MacOS/${run.executable}`)) : Option.none;
};

const openRoute = async (system: System, request: Request, { cwd }: Command, program: 'open' | 'xed', head: Word, args: readonly Word[]): Promise<Option<Route>> => {
    const launchers = {
        open: { app: [], waits: ['-W', '--wait-apps', '--stdin', '--stdout', '--stderr'], kept: ['-a', '-b', '-n', '--new', '-g', '--background', '-j', '--hide', '-F', '--fresh', '--env', '-u', '--url'] },
        xed: { app: ['com.apple.dt.Xcode'], waits: ['-w', '--wait'], kept: ['-x', '--launch', '-b', '--background'] },
    };
    const { app: fixed, waits: blocking, kept } = launchers[program];
    const cut = args.findIndex(({ text }) => text === '--args');
    const listed = argumentGroups(program, cut < 0 ? args : args.slice(0, cut));
    const passed = cut < 0 ? [] : args.slice(cut + 1);
    const valued = (...flags: readonly string[]): readonly Word[] => listed.flatMap(({ names, words: [, value] }) => (value !== undefined && names.some((name) => flags.includes(name)) ? [value] : []));
    const loose = listed.flatMap(({ names, words: [word] }) => (names.length === 0 && word !== undefined ? [word] : []));
    const switches = listed.flatMap(({ names }) => names);
    const [app] = [...valued('-a', '-b'), ...loose.filter(({ text }) => text.endsWith('.app'))];
    const subjects = [...fixed, ...Option.fromNullable(app?.raw).values];
    const target = subjects.join(' ') || '<app>';
    const documents = loose.filter((word) => word !== app);
    const [url] = [...valued('-u', '--url'), ...documents.filter(({ text }) => URL.canParse(text))];
    const [waited] = switches.filter((name) => blocking.includes(name));
    const foreign = switches.filter((name) => !(kept.includes(name) || blocking.includes(name)));
    if (informational(program, switches) || subjects.length + documents.length === 0) {
        return Option.none;
    }
    if (url !== undefined) {
        return Option.some(refuse(WEB_URL.test(url.text) ? `${program} ${url.raw} opens a browser, use playwright-cli` : `${program} ${url.raw} launches its handler, use ${watchApps('launch', target)}`));
    }
    if (waited !== undefined) {
        return Option.some(refuse(`${program} ${waited} waits on ${target}, ${watchAction(request, subjects)}`));
    }
    if (foreign.length > 0) {
        return Option.some(refuse(`${program} ${foreign.join(' ')} launches ${target}, use ${watchApps('launch', target)}`));
    }
    const direct = app !== undefined && passed.length > 0 ? await headlessExecutable(system, cwd, app, valued('-b').includes(app), passed) : Option.none;
    const environment = valued('--env');
    const launch = direct.match(
        (path) => [...environment.map(({ raw }) => raw), path, ...passed.map(({ raw }) => raw)].join(' '),
        () => watchApps('launch', ...subjects, ...(switches.some((name) => name === '-n' || name === '--new') ? ['--new'] : []), ...documents.map(({ raw }) => `${DOCUMENT} ${raw}`), ...environment.map(({ raw }) => `--environment=${raw}`), ...passed.map(({ raw }) => `${ARGUMENT}=${raw}`)),
    );
    return Option.some({ kind: 'rewrite', program, start: head, text: launch });
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

const appleScriptStatements = (values: readonly (readonly [string, string])[]): readonly { readonly app: Option<string>; readonly statement: Statement }[] =>
    values
        .flatMap(([name, value]) => (name === '-e' ? value.split('\n') : []))
        .map((line) => line.trim())
        .filter((line) => line !== '')
        .reduce<{ readonly stack: readonly string[]; readonly said: readonly { readonly app: Option<string>; readonly statement: Statement }[] }>(
            ({ stack, said }, line) => {
                const groups = TELL.exec(line)?.groups ?? APP_COMMAND.exec(line)?.groups;
                if (line === 'end tell') {
                    return { stack: stack.slice(0, -1), said };
                }
                return groups?.app !== undefined && groups.body === undefined ? { stack: [...stack, groups.app], said } : { stack, said: [...said, { app: Option.fromNullable(groups?.app ?? stack.at(-1)), statement: statement(groups?.body ?? line) }] };
            },
            { stack: [], said: [] },
        ).said;

const osascriptRoute = (head: Word, invocation: Invocation): Option<Route> => {
    const { values, inputs } = operands(invocation);
    const events = 'System Events';
    if (values.some(([name, value]) => name === '-l' && value === 'JavaScript')) {
        return values.some(([name, value]) => name === '-e' && JXA_APP.test(value)) ? Option.some(refuse(`JavaScript osascript drives apps, run ${WATCHER} <subcommand> <app>`)) : Option.none;
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
        return Option.some(refuse(refused));
    }
    return aimed.length === 0 || verb === undefined ? Option.none : Option.some({ kind: 'rewrite', program: 'osascript', start: head, text: watchApps(verb, ...apps, ...documents) });
};

const lsappinfoRoute = (head: Word, invocation: Invocation): Option<Route> => {
    const [subcommand, ...rest] = operands(invocation).inputs;
    const [id] = rest.flatMap((word) => Option.fromNullable(BUNDLE_ID.exec(word)?.groups?.id).values);
    if (subcommand !== 'launch') {
        return Option.none;
    }
    return Option.some(id === undefined ? refuse(`lsappinfo launch bypasses ${WATCHER}, use ${watchApps('launch', '<app>')}`) : { kind: 'rewrite', program: 'lsappinfo', start: head, text: watchApps('launch', quote(id)) });
};

const launchctlRoute = (invocation: Invocation): Option<Route> => {
    const at = invocation.indexOf('--');
    const [program, ...args] = at < 0 ? [] : invocation.slice(at + 1);
    const executed = program === undefined ? undefined : EXECUTABLE.exec(program)?.groups?.executable;
    return invocation[1] === 'submit' && executed !== undefined && programRow(executed).headless?.some((flag) => args.includes(flag)) !== true ? Option.some(refuse(`launchctl submit keeps ${executed} alive, use ${watchApps('launch', quote(executed))}`)) : Option.none;
};

const bundleRoute = async (system: System, invocation: Invocation, { assigned, words: [head, ...args] }: Command['parts'][number]): Promise<Option<Route>> => {
    const [program] = invocation;
    const groups = head === undefined ? undefined : BUNDLE.exec(head.text)?.groups;
    if (head === undefined || informational(program, operands(invocation).options) || programRow(program).headless?.some((name) => invocation.includes(name)) === true) {
        return Option.none;
    }
    if (groups?.bundle === undefined) {
        return SUBSTITUTION.test(head.raw) && EXECUTABLE.test(head.text) ? Option.some(refuse(`${program} path expands at run time, use ${watchApps('launch', quote(program))}`)) : Option.none;
    }
    const [info] = (await bundleInfo(system, groups.bundle)).values;
    return info !== undefined && info.executable === groups.executable ? Option.some({ kind: 'rewrite', program, start: assigned[0] ?? head, text: watchApps('launch', info.identifier, ...args.map(({ raw }) => `${ARGUMENT}=${raw}`), ...assigned.map(({ raw }) => `--environment=${raw}`)) }) : Option.none;
};

const appRoute = async (system: System, request: Request, apps: ReadonlyMap<string, string>, command: Command): Promise<Option<Route>> => {
    const last = command.invocations.at(-1);
    const part = command.parts.at(-1);
    const [head, ...args] = part?.words ?? [];
    if (last === undefined || part === undefined || head === undefined) {
        return Option.none;
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
            return Option.some(refuse(`screencapture captures the whole display, use ${watchApps('capture', '<app>')}`));
        case 'qlmanage':
            return last.includes('-p') ? Option.some(refuse('qlmanage -p opens a preview window, use qlmanage -t')) : Option.none;
        default:
            return await bundleRoute(system, last, part);
    }
};

// --- [REWRITE]

const timeoutCommands = async ({ request, system, script: { commands } }: Call): Promise<readonly Verdict[]> => {
    const second = 1000;
    const applied = request.kind === 'foreground' && request.timeout.values.length === 0;
    const limited = commands.flatMap((command) =>
        command.invocations.flatMap((invocation, at) => {
            const [from] = command.parts[at]?.words ?? [];
            const [to] = [...(command.parts[at + 1]?.assigned ?? []), ...(command.parts[at + 1]?.words ?? [])];
            const [amount] = operands(invocation).leading;
            const groups = invocation[0] === 'timeout' ? DURATION.exec(amount ?? '')?.groups : undefined;
            const limit = Number(groups?.value) * second * 60 ** 'smh'.indexOf(String(groups?.unit));
            return from === undefined || to === undefined || amount === undefined || !(limit > 0) ? [] : [{ command, from, to, amount, limit }];
        }),
    );
    const installed =
        limited.length > 0 &&
        (await system.run(['timeout', '--version'], { cwd: system.plugin, stdin: '' }).then(
            ({ exitCode }) => exitCode === 0,
            () => false,
        ));
    return installed
        ? []
        : limited.map(({ command, from, to, amount, limit }) => {
              const capped = limit > MAX_TIMEOUT ? `capped at ${MAX_TIMEOUT / second}s` : `set to ${amount}`;
              return editOrRefuse(command, edit(from.span.start, to.span.start, '', Option.some(`timeout is not installed, call timeout ${applied ? capped : 'stays'}`), applied ? Option.some(Math.min(limit, MAX_TIMEOUT)) : Option.none), 'timeout is not installed, remove it');
          });
};

const optionRespellings = (program: string, words: readonly Word[], table: Readonly<Record<string, string>>, reason = ''): readonly Verdict[] =>
    argumentGroups(program, words).flatMap(({ words: [head, ...taken], names }) => {
        const kept = names.map((name) => table[name] ?? name).filter((name) => name !== '');
        const [first] = kept;
        if (head === undefined || kept.join(' ') === names.join(' ')) {
            return [];
        }
        if (first === undefined) {
            return [edit(head.span.start - 1, (taken.at(-1) ?? head).span.end, '', Option.some(`${program} ${head.text} removed${reason}`))];
        }
        const { joined } = option(programRow(program), head.text);
        const text = first.startsWith('--') ? [first, ...joined].join('=') : `-${kept.map((name) => name.slice(1)).join('')}${joined.join('')}`;
        return [edit(head.span.start, head.span.end, text, Option.some(`${program} ${head.text} became ${text}`))];
    });

const sdFixes = (invocation: Invocation, [head, ...rest]: readonly Word[]): readonly Verdict[] => {
    const { options, leading } = operands(invocation);
    const [find] = leading;
    const [word] = argumentGroups('sd', rest).flatMap(({ names, words: [first] }) => (names.length === 0 && first?.text.startsWith('-') === true ? [first] : []));
    const breaks = find !== undefined && (find.includes('\n') || (!options.some((name) => name === '-F' || name === '--fixed-strings') && ESCAPED_NEWLINE.test(find)));
    return [
        ...(head === undefined || !breaks || options.some((name) => name === '-A' || name === '--across') ? [] : [edit(head.span.end, head.span.end, ' -A')]),
        ...(word === undefined || rest.some(({ text }) => text === '--') ? [] : [edit(word.span.start, word.span.start, '-- ', Option.some(`sd -- added before ${word.raw}`))]),
    ];
};

const nxCacheFlags = (invocation: Invocation, words: readonly Word[]): readonly Verdict[] => {
    const { inputs, values } = operands(invocation);
    const listed = values.flatMap(([name, value]) => (['-t', '--targets', '--target'].includes(name) ? value.split(',') : []));
    const targets = inputs[0] === 'run' ? inputs.slice(1, 2).flatMap((word) => (word.includes(':') ? [word.slice(word.indexOf(':') + 1)] : [])) : [...listed, ...inputs.slice(0, listed.length === 0 ? 1 : 0)];
    return targets.length > 0 && targets.every((target) => CACHED_TARGET.test(target)) ? optionRespellings('nx', words, Object.fromEntries(['--skip-nx-cache', '--skipNxCache', '--disable-nx-cache', '--disableNxCache'].map((name) => [name, ''])), `, ${targets.join(' ')} inputs hash every source`) : [];
};

const programRewrites = (invocation: Invocation, words: readonly Word[]): readonly Verdict[] => {
    const [program] = invocation;
    const [head, ...rest] = words;
    const { options } = operands(invocation);
    const stated = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
    if (head === undefined) {
        return [];
    }
    switch (program) {
        case 'fd':
        case 'rg':
            return stated(['--hidden', '--no-hidden', '-u', '--unrestricted', ...(program === 'fd' ? ['-H'] : ['-.', '--no-config'])]) ? [] : [edit(head.span.end, head.span.end, ' --hidden')];
        case 'sd':
            return informational(program, options) ? [] : sdFixes(invocation, words);
        case 'tree':
            return optionRespellings(program, rest, { '-a': '', '--all': '', '--noreport': '', '--dirsfirst': '--group-directories-first' });
        case 'refurb': {
            const text = 'python -m tools.refurb';
            return [edit(head.span.start, head.span.end, text, Option.some(`refurb became ${text} to read the mypy cache`))];
        }
        case 'nx':
            return nxCacheFlags(invocation, rest);
        default:
            return [];
    }
};

// --- [NX_TARGET]

const optionKeys = (invocation: Invocation, known: (name: string) => boolean = () => true): ReadonlySet<string> => {
    const { options, values } = operands(invocation);
    return new Set([...options.filter(known), ...values.flatMap(([name, value]) => (known(name) ? [`${name}=${value}`] : []))]);
};

const groupTarget = (graph: Readonly<Record<string, Target>>, target: string): string => {
    const parent = target.split(':').slice(0, 2).join(':');
    const named = (graph[parent]?.dependsOn ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.target));
    return named.some((name) => name === target || (name.endsWith('*') && target.startsWith(name.slice(0, -1)))) ? parent : target;
};

const rootRelative = (word: Word, cwd: string, root: string, home: Option<string>): readonly string[] => {
    const [place] = resolvePath(word.text, Option.some(cwd), home).values;
    return word.expands || EXPANSION.test(word.raw) || place === undefined || !isInside(place, root) ? [] : [quote(place.slice(root.length + 1) || '.')];
};

const ownerProject = async ({ system, project }: Call, selectors: readonly string[], invocation: Invocation, cwd: string, root: string): Promise<Option<{ readonly name: string; readonly targets: Readonly<Record<string, Target>> }>> => {
    const shown = async (named: string) => {
        const graph = await runJson<{ readonly targets: Readonly<Record<string, Target>> }>(system, ['mise', 'exec', '--', 'nx', 'show', 'project', named, '--json'], root);
        return Option.fromNullable(graph.toArray()[0]).map(({ targets }) => ({ name: named, targets }));
    };
    const [value, ...more] = operands(invocation).values.flatMap(([flag, argument]) => (selectors.includes(flag) ? [argument] : []));
    const [place] = resolvePath(value ?? '.', Option.some(cwd), system.home).values.filter((path) => isInside(path, root));
    const located = (path: string): Option<string> => {
        const leaf = basename(path);
        const folder = leaf === 'tsconfig.json' ? path.slice(0, path.lastIndexOf('/')) : path;
        return folder === path && leaf.lastIndexOf('.') > 0 ? Option.none : Option.some(folder.slice(root.length + 1) || '.');
    };
    if (selectors.length === 0) {
        return shown(project);
    }
    if (more.length > 0) {
        return Option.none;
    }
    const [pattern] = (place !== undefined && (await system.exists(place)) ? located(place) : Option.fromNullable(value)).values;
    if (pattern === undefined) {
        return Option.none;
    }
    const [found, guessed] = await Promise.all([runJson<readonly string[]>(system, ['mise', 'exec', '--', 'nx', 'show', 'projects', '--json', '-p', pattern], root), shown(pattern)]);
    const [name, ...others] = found.toArray().flat();
    if (name === undefined || others.length > 0) {
        return Option.none;
    }
    return guessed.values.some((candidate) => candidate.name === name) ? guessed : shown(name);
};

const checkerRuns = async ({ system }: Call, graph: Readonly<Record<string, Target>>, program: string, subcommand: string | undefined) => {
    const listed = Object.entries(graph).flatMap(([target, { options }]) => {
        const tool = CHECK_TARGET.exec(target)?.groups?.tool;
        const texts = [options?.command, ...(options?.commands ?? []).map((entry) => (typeof entry === 'string' ? entry : entry.command))];
        return tool === program || tool === subcommand ? texts.flatMap((text) => (text === undefined ? [] : [{ target, text }])) : [];
    });
    const parsed = await Promise.all(listed.map(async ({ target, text }) => ({ target, script: await parse(system, text) })));
    return parsed
        .flatMap(({ target, script }) =>
            script
                .toArray()
                .flatMap(({ commands }) => invocationsOf(commands, program))
                .map((ran) => ({ target, ran })),
        )
        .flatMap(({ target, ran }) => (operands(ran).inputs[0] === subcommand ? [{ target, keys: optionKeys(ran), options: operands(ran).options }] : []));
};

const forwardedArguments = (program: string, words: readonly Word[], dropped: ReadonlySet<string>, subcommand: boolean) => {
    const listed = argumentGroups(program, words);
    const first = listed.findIndex(({ names }) => names.length === 0);
    return listed.filter(({ names }, at) => (names.length === 0 ? !subcommand || at !== first : !new Set(names).isSubsetOf(dropped)));
};

const checkerTarget = async (call: Call, { cwd, hasInput }: Command, invocation: Invocation, words: readonly Word[]): Promise<Option<{ readonly run: string; readonly passed: readonly string[] }>> => {
    const [program] = invocation;
    const { selectors } = programRow(program);
    const { home } = call.system;
    const [root] = call.system.root.values;
    const [folder] = cwd.values;
    const { options, inputs } = operands(invocation);
    if (selectors === undefined || root === undefined || folder === undefined || !isInside(folder, root) || hasInput || informational(program, options) || words.some(({ text }) => text === '--')) {
        return Option.none;
    }
    const [project] = (await ownerProject(call, selectors, invocation, folder, root)).values;
    if (project === undefined) {
        return Option.none;
    }
    const { name, targets } = project;
    const [subcommand] = inputs;
    const found = await checkerRuns(call, targets, program, subcommand);
    const known = new Set(found.flatMap((ran) => ran.options));
    const wanted = optionKeys(invocation, (flag) => known.has(flag));
    const fitting = found.filter(({ keys }) => wanted.isSubsetOf(keys));
    const fewest = Math.min(...fitting.map(({ keys }) => keys.size));
    const [target, ...others] = [...new Set(fitting.filter(({ keys }) => keys.size === fewest).map((ran) => groupTarget(targets, ran.target)))];
    const aggregate = !found.some((ran) => ran.target === target);
    const kept = forwardedArguments(program, words.slice(1), new Set([...known, ...selectors]), subcommand !== undefined);
    const switched = kept.some(({ names }) => names.length > 0);
    const moved = selectors.length === 0 && folder !== root;
    const passed = kept.flatMap((item) => item.words).map((word) => (moved ? rootRelative(word, folder, root, home) : [word.raw]));
    if (target === undefined || others.length > 0 || (aggregate && (wanted.size > 0 || switched)) || (moved && switched) || passed.some((argument) => argument.length === 0)) {
        return Option.none;
    }
    return Option.some({ run: `${name}:${target}`, passed: [...passed.flat(), ...(moved && passed.length === 0 ? [quote(folder.slice(root.length + 1))] : [])] });
};

// --- [DECISION]

const policyFailure = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const commandRewrites = async (call: Call, command: Command): Promise<readonly Verdict[]> => {
    const last = command.invocations.at(-1);
    const words = command.parts.at(-1)?.words ?? [];
    const [head] = words;
    const tail = words.at(-1);
    const route = call.routes.get(command);
    if (last === undefined || head === undefined || tail === undefined) {
        return [];
    }
    if (route?.kind === 'refuse') {
        return [route];
    }
    if (command.origin !== 'call') {
        return route === undefined ? [] : [refuse(`Nested ${route.program} cannot be rewritten, run ${route.text}`)];
    }
    if (route !== undefined) {
        return [edit(route.start.span.start, tail.span.end, route.text, Option.some(`${route.program} became ${route.text}`))];
    }
    const [found] = (await checkerTarget(call, command, last, words)).values;
    if (found === undefined) {
        return programRewrites(last, words);
    }
    const { run, passed } = found;
    return [edit(head.span.start, tail.span.end, [`nx run ${run}`, ...(passed.length > 0 ? ['--', ...passed] : [])].join(' '), Option.some(`${last[0]} became nx run ${run}`))];
};

const scriptDecision = async (system: System, source: Uint8Array, script: Script, project: string, request: Request): Promise<Decision> => {
    const { commands } = script;
    const [apps, writers] = await Promise.all([runningApps(system, commands), taskWriters(system, request, commands)]);
    const routes = new Map((await Promise.all(commands.map(async (each) => (await appRoute(system, request, apps, each)).values.map((route) => [each, route] as const)))).flat());
    const call: Call = { system, source, script, project, request, apps, writers, routes };
    const verdicts = (
        await Promise.all([...[deletes, git, searchRoots, noOpReplacements, inputSources, selfMatches, foregroundWaits, taskTails, backgroundJobs, timeoutCommands, ghidra, projectCopies, pipefail, loopReruns, processKills, appPolls].map((policy) => policy(call)), ...commands.map((each) => commandRewrites(call, each))])
    ).flat();
    const reasons = [...new Set(verdicts.flatMap((verdict) => (verdict.kind === 'refuse' ? [verdict.reason] : [])))];
    const edits = verdicts.flatMap((verdict) => (verdict.kind === 'edit' ? [verdict] : []));
    const backgrounds = verdicts.flatMap((verdict) => (verdict.kind === 'background' ? [verdict.notice] : []));
    const notices = [...new Set([...edits.flatMap(({ notice }) => notice.values), ...backgrounds])];
    const timeouts = edits.flatMap(({ timeout }) => timeout.values);
    const total = timeouts.reduce((sum, value) => sum + value, 0);
    if (reasons.length > 0) {
        return { kind: 'deny', reason: reasons.join('. ') };
    }
    if (edits.length + backgrounds.length === 0) {
        return { kind: 'allow' };
    }
    return {
        kind: 'rewrite',
        command: applyEdits(source, 0, edits),
        notice: notices.length === 0 ? Option.none : Option.some(notices.join('. ')),
        timeout: timeouts.length === 0 ? Option.none : Option.some(Math.min(MAX_TIMEOUT, total)),
        background: backgrounds.length > 0,
    };
};

const commandDecision = async (system: System, command: string, project: string, request: Request): Promise<Decision> => {
    const source = new TextEncoder().encode(command);
    return (await parse(system, command)).match<Decision | Promise<Decision>>(
        (script) => scriptDecision(system, source, script, project, request),
        (faults) => ({ kind: 'deny', reason: `Command not parsed. ${faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, ...(value.output === '' ? [] : [value.output])].join('. ') : `${value.program} failed. ${String(value.cause)}`)).join('. ')}` }),
    );
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { commandDecision, policyFailure, type Request, type System, WATCHER, WORKTREE };
