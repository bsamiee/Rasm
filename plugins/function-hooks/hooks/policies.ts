import type { ToolCallInput } from 'claude-code';
import type { Command, Script } from './command.ts';
import { fault, none, type Option, ok, type Result, some } from './composition.ts';
import { basename, type Invocation, known, type Operands, operands, option, PROGRAMS } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];
type OptionPredicate = (given: (...names: readonly string[]) => boolean, rest: readonly string[]) => boolean;
type Key = keyof typeof _GIT;
type GitCall = { readonly kind: 'aliased' } | { readonly kind: 'subcommand'; readonly key: Key; readonly args: readonly string[] };

interface GitRow {
    readonly reason: string;
    readonly any?: true;
    readonly words?: readonly string[];
    readonly prefixes?: readonly string[];
    readonly safe?: readonly string[];
    readonly refine?: Refinement;
}
interface Walk {
    readonly cloud: string;
    readonly places: ReadonlyMap<string, string>;
}
interface Facts {
    readonly existing: readonly string[];
    readonly walk: Option<Walk>;
}
interface Insertion {
    readonly at: number;
    readonly text: string;
    readonly flag: string;
}
interface Rewrite {
    readonly command: string;
    readonly note: string;
    readonly instruction: Option<string>;
}
interface Target {
    readonly project: Option<string>;
    readonly target: string;
}
interface TargetDependencyConfig {
    readonly target: string;
    readonly projects?: string | readonly string[];
    readonly dependencies?: boolean;
}
interface TargetConfiguration {
    readonly command?: string;
    readonly options?: { readonly command?: string; readonly commands?: readonly { readonly command: string }[] };
    readonly dependsOn?: readonly TargetDependencyConfig[];
}
interface Manifest {
    readonly name: string;
    readonly nx: { readonly targets: Readonly<Record<string, TargetConfiguration>> };
}
interface NxJsonConfiguration {
    readonly targetDefaults: Readonly<Record<string, TargetConfiguration | readonly TargetConfiguration[]>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLOUD = 'Library/CloudStorage';
const LOCK = '.cache/uv-resolver.lock';
const _WORKTREE = 'creates a second checkout with its own metadata and sync cost';
const _SECOND_FILES: readonly RegExp[] = [
    /^(?:project\.json|\.nxignore)$/u,
    /^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u,
    /^tsconfig\.(?!base\.json$).+\.json$/u,
    /^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u,
    /^biome\.jsonc$/u,
    /^\.yamllint(?:\.yml)?$/u,
];
const _HOME = /^(?:~|\$HOME|\$\{HOME\})(?=\/|$)/u;
const _PRIMARY = /^(?:-.{2,}|\(|!)$/u;
const _BREAK = /\n|(?<!\\)(?:\\\\)*\\n/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [GIT]

const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !word.startsWith('-'));
    const [target] = targets;
    return target === undefined || args.includes('--') || targets.some((named) => existing.includes(named)) ? [] : [`git reset ${target} moves HEAD and drops commits from the branch`];
};

const _restore: Refinement = (args, _existing, reason) => {
    const { options } = operands(['git', ...args]);
    return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [`git restore ${reason}`];
};

const _config: Refinement = (args, _existing, reason) => {
    const alias = args.find((word) => word.startsWith('alias.'));
    return alias !== undefined && args.slice(args.indexOf(alias) + 1).some((word) => !word.startsWith('-')) ? [`git config ${alias} ${reason}`] : [];
};

const _checkout: Refinement = (args, existing) => {
    const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
    if (!args.includes('--') && args.some((word) => ['-b', '--orphan', '-t', '--track', '--detach'].includes(word))) {
        return [];
    }
    if (args.includes('--') || more.length > 0 || first === '.' || first?.startsWith(':') === true) {
        return ['git checkout with a pathspec overwrites working-tree files'];
    }
    return first !== undefined && first !== '-' && existing.includes(first) ? [`git checkout ${first} names an existing path it overwrites`] : [];
};

const _GIT = {
    branch: { reason: 'deletes or force-moves a branch', words: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
    checkout: { reason: 'discards local changes', words: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], prefixes: ['--force'], refine: _checkout },
    clean: { reason: 'deletes untracked files', any: true },
    config: { reason: 'defines a git alias that can hide a refused subcommand', refine: _config },
    push: { reason: 'rewrites or deletes remote history', words: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
    rebase: { reason: 'rewrites commits other agents can hold', any: true },
    reflog: { reason: 'erases reflog entries, the last recovery path', words: ['delete', 'drop', 'expire'] },
    reset: { reason: 'wipes working-tree or index state', words: ['--hard', '--merge', '--keep'], refine: _reset },
    restore: { reason: 'overwrites working-tree files', refine: _restore },
    revert: { reason: 'reverses committed history', any: true },
    stash: { reason: 'hides uncommitted work other agents depend on', any: true, safe: ['list', 'show'] },
    switch: { reason: 'discards local changes', words: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
    worktree: { reason: _WORKTREE, any: true, safe: ['list'] },
} as const satisfies Readonly<Record<string, GitRow>>;

const _isKey = (candidate: string): candidate is Key => Object.hasOwn(_GIT, candidate);

const _skip = (words: readonly string[], index: number): number => {
    const word = words[index];
    return word?.startsWith('-') === true ? _skip(words, index + 1 + option('git', word).taken) : index;
};

const _calls = (commands: readonly Command[]): readonly GitCall[] =>
    commands
        .flatMap((command) => command.invocations)
        .filter(([program]) => program === 'git')
        .flatMap((words): readonly GitCall[] => {
            const index = _skip(words, 1);
            const [key, ...args] = words.slice(index);
            if (words.slice(1, index).some((word) => word.startsWith('alias.'))) {
                return [{ kind: 'aliased' }];
            }
            return key !== undefined && _isKey(key) ? [{ kind: 'subcommand', key, args }] : [];
        });

const _refusals = (key: Key, args: readonly string[], existing: readonly string[]): readonly string[] => {
    const row: GitRow = _GIT[key];
    const [first] = args;
    if (first !== undefined && row.safe?.includes(first) === true) {
        return [];
    }
    if (row.any === true) {
        return [`git ${key} ${row.reason}`];
    }
    const hit = args.find((word) => row.words?.includes(word) === true || row.prefixes?.some((prefix) => word.startsWith(prefix)) === true);
    return hit === undefined ? (row.refine?.(args, existing, row.reason) ?? []) : [`git ${key} ${hit} ${row.reason}`];
};

const _git = (commands: readonly Command[], existing: readonly string[]): readonly string[] => _calls(commands).flatMap((call) => (call.kind === 'aliased' ? ['inline git alias can hide a refused subcommand'] : _refusals(call.key, call.args, existing)));

const gitPaths = (commands: readonly Command[]): readonly string[] => _calls(commands).flatMap((call) => (call.kind === 'subcommand' && (call.key === 'reset' || call.key === 'checkout') ? call.args.filter((word) => !word.startsWith('-')) : []));

// --- [STDIN]

const _sourced = (program: string, options: readonly string[]): boolean => options.some((name) => ['--help', '--version'].includes(name) || PROGRAMS[program]?.sources?.includes(name) === true);

const _reads = (invocation: Invocation): boolean => {
    const row = PROGRAMS[invocation[0]];
    const { inputs, options } = operands(invocation);
    return row?.stdin !== undefined && !_sourced(invocation[0], options) && !options.some((name) => row.recursive?.includes(name) === true) && (row.stdin === 'always' || inputs.length === 0 || inputs.includes('-'));
};

const _stdin = (commands: readonly Command[]): readonly string[] => commands.flatMap((command) => (!command.fed && command.invocations.some(_reads) ? [`${command.words.join(' ')} reads standard input and nothing feeds it, pass an operand, pipe, heredoc, herestring, or input redirect`] : []));

// --- [WAIT]

const _waits = (command: Command): readonly string[] => {
    const waiters: Readonly<Record<string, (parsed: Operands, wraps: boolean) => boolean>> = {
        sleep: () => true,
        pwait: () => true,
        wait: ({ inputs }) => inputs.length > 0,
        caffeinate: ({ options }, wraps) => !wraps || options.includes('-w'),
        tail: ({ options }) => options.includes('--pid'),
        lsof: ({ options }) => options.some((name) => name === '-r' || name === '+r'),
    };
    const chain = command.invocations;
    const line = command.words.join(' ');
    return [...(chain.some((invocation, index) => waiters[invocation[0]]?.(operands(invocation), index < chain.length - 1) === true) ? [`${line} waits`] : []), ...(command.polled ? [`${line} repeats until the loop condition changes`] : [])];
};

const _wait = (commands: readonly Command[]): readonly string[] => {
    const reasons = commands.flatMap(_waits);
    return reasons.length === 0 ? [] : [...reasons, "act on the command's own exit, or run it with run_in_background and act on its completion notification"];
};

// --- [SCRIPT]

const _writes = (command: Command): readonly string[] => command.invocations.slice(-1).flatMap(([program, ...rest]) => (program === 'echo' || program === 'printf' || (program === 'cat' && rest.length === 0) ? command.writes.filter((path) => !path.startsWith('/dev/')) : []));

const _script = ({ commands, clocks }: Script): readonly string[] => {
    const timer = "hyperfine -N -r <runs> '<command>' times commands";
    return [
        ...commands.flatMap((command, index) => {
            const written = commands.slice(0, index).flatMap(_writes);
            return [
                ...[...command.reads, ...command.words].filter((path) => written.includes(path)).map((path) => `${path} written then read in one call`),
                ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop`) : []),
                ...(command.invocations.some(([program]) => program === 'time') ? [`${command.words.join(' ')} times by hand, ${timer}`] : []),
            ];
        }),
        ...clocks.map((clock) => `${clock} times by hand, ${timer}`),
    ];
};

// --- [WALK]

const _starts = (invocation: Invocation): readonly string[] => {
    const [program, ...args] = invocation;
    const { inputs, options, values } = operands(invocation);
    const here = (words: readonly string[]): readonly string[] => (words.length === 0 ? ['.'] : words);
    const given = (names: readonly string[]): readonly string[] => values.flatMap(([name, value]) => (names.includes(name) ? [value] : []));
    const recursive = (): readonly string[] => (options.some((name) => PROGRAMS[program]?.recursive?.includes(name) === true) ? here(inputs) : []);
    const walkers: Readonly<Record<string, () => readonly string[]>> = {
        fd: () => here([...inputs, ...given(['-C', '--base-directory', '--search-path'])]),
        find: () => {
            const primary = args.findIndex((word) => _PRIMARY.test(word));
            return here(operands([program, ...args.slice(0, primary < 0 ? args.length : primary)]).inputs);
        },
        rg: () => here(inputs),
        grep: () => (given(['-d', '--directories']).includes('recurse') ? here(inputs) : recursive()),
        du: () => here(inputs),
        tree: () => here(inputs),
        ls: recursive,
        lsof: () => given(['+D']),
    };
    return walkers[program]?.() ?? [];
};

const walkStarts = (commands: readonly Command[], home: string): readonly (readonly [string, string])[] =>
    commands
        .flatMap((command) => command.invocations)
        .flatMap(_starts)
        .map((word) => [word, word.replace(_HOME, () => home)] as const);

const _descends = (invocation: Invocation, { cloud, places }: Walk): boolean => {
    const folders = _starts(invocation).flatMap((word) => places.get(word) ?? []);
    const excluded = invocation.some((word) => word.includes(basename(CLOUD)));
    return folders.some((folder) => folder.startsWith(cloud)) || (!excluded && folders.some((folder) => cloud.startsWith(folder)));
};

const _walk = (commands: readonly Command[], walk: Walk): readonly string[] =>
    commands.flatMap((command) => (command.invocations.some((invocation) => _descends(invocation, walk)) ? [`${command.words.join(' ')} descends into ~/${CLOUD}, dataless cloud placeholders there hang the walker on the file provider, start outside it or exclude ${basename(CLOUD)}`] : []));

// --- [QUEUE]

const queues = (commands: readonly Command[], reached: readonly Command[]): boolean => {
    const always: OptionPredicate = () => true;
    const unfrozen: OptionPredicate = (given) => !given('--frozen');
    const resolvers: readonly (readonly [readonly string[], OptionPredicate])[] = [
        [['uv', 'lock'], always],
        [['uv', 'sync'], unfrozen],
        [['uv', 'run'], (given): boolean => given('-w', '--with', '--with-editable', '--with-requirements') || !given('--frozen', '--no-sync', '--no-project')],
        [['uv', 'add'], unfrozen],
        [['uv', 'remove'], unfrozen],
        [['uv', 'version'], (given, rest): boolean => !given('--frozen', '--dry-run') && (given('--bump') || rest.length > 0)],
        [['uv', 'export'], unfrozen],
        [['uv', 'tree'], unfrozen],
        [['uv', 'check'], unfrozen],
        [['uv', 'audit'], unfrozen],
        [['uv', 'venv'], (given): boolean => given('--seed')],
        [['uv', 'build'], always],
        [['uv', 'tool', 'run'], always],
        [['uv', 'tool', 'install'], always],
        [['uv', 'tool', 'upgrade'], always],
        [['uv', 'pip', 'install'], always],
        [['uv', 'pip', 'sync'], always],
        [['uv', 'pip', 'compile'], always],
        [['uvx'], always],
    ];
    const held = commands.some((command) => command.invocations.some(([program, ...args]) => program === 'lockf' && args.some((word) => word.endsWith(LOCK))));
    return (
        !held &&
        [...commands, ...reached]
            .flatMap((command) => command.invocations)
            .some((invocation) => {
                const { inputs, options } = operands(invocation);
                const words = [invocation[0], ...inputs];
                const given = (...names: readonly string[]): boolean => options.some((name) => names.includes(name));
                return !given('-h', '--help') && resolvers.some(([path, resolves]) => path.every((word, index) => words[index] === word) && resolves(given, words.slice(path.length)));
            })
    );
};

const nxTargets = (commands: readonly Command[]): readonly Target[] =>
    commands
        .flatMap((command) => command.invocations)
        .filter(([program]) => program === 'nx')
        .flatMap(([, ...words]) => words.flatMap((word) => word.slice(word.indexOf('=') + 1).split(',')))
        .map((item): Target => {
            const [, target] = item.split(':');
            return target === undefined ? { project: none, target: item } : { project: some(item.slice(0, item.indexOf(':'))), target };
        });

const nxReviver = (key: string, value: unknown): unknown => {
    const expanded = (entry: string): object => (key === 'commands' ? { command: entry } : { target: entry.slice(entry.startsWith('^') ? 1 : 0), dependencies: entry.startsWith('^') });
    return Array.isArray(value) && (key === 'commands' || key === 'dependsOn') ? value.map((entry: unknown) => (typeof entry === 'string' ? expanded(entry) : entry)) : value;
};

const _object = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null;

const decodedManifest = (subject: string, value: unknown): Result<Manifest> => {
    const declared = (candidate: unknown): candidate is Manifest => _object(candidate) && typeof candidate['name'] === 'string' && _object(candidate['nx']) && _object(candidate['nx']['targets']);
    return declared(value) ? ok(value) : fault({ kind: 'invalid', subject, cause: 'name or nx.targets missing' });
};

const decodedNxJson = (subject: string, value: unknown): Result<NxJsonConfiguration> => {
    const declared = (candidate: unknown): candidate is NxJsonConfiguration => _object(candidate) && _object(candidate['targetDefaults']);
    return declared(value) ? ok(value) : fault({ kind: 'invalid', subject, cause: 'targetDefaults missing' });
};

const targetCommands = (named: readonly Target[], manifest: Manifest, nxJson: NxJsonConfiguration): readonly string[] => {
    const configured: readonly (Target & { readonly configuration: TargetConfiguration })[] = [
        ...Object.entries(manifest.nx.targets).map(([target, configuration]) => ({ project: some(manifest.name), target, configuration })),
        ...Object.entries(nxJson.targetDefaults).flatMap(([target, configurations]) => [configurations].flat().map((configuration) => ({ project: none, target, configuration }))),
    ];
    const upstream = ({ project, configuration }: (typeof configured)[number]): readonly Target[] =>
        (configuration.dependsOn ?? []).flatMap(({ target, projects, dependencies }) => (projects === undefined ? [dependencies === true ? none : project] : [projects].flat().map(some)).map((owner) => ({ project: owner, target })));
    const reached = (pending: readonly Target[], seen: ReadonlySet<(typeof configured)[number]>): ReadonlySet<(typeof configured)[number]> => {
        const found = configured.filter((entry) => !seen.has(entry) && pending.some(({ project, target }) => target === entry.target && (project.kind === 'none' || entry.project.kind === 'none' || project.value === entry.project.value)));
        return found.length === 0 ? seen : reached(found.flatMap(upstream), new Set([...seen, ...found]));
    };
    return [...reached(named, new Set())].flatMap(({ configuration: { command, options } }) => [command, options?.command, ...(options?.commands ?? []).map((entry) => entry.command)].filter((text) => text !== undefined));
};

// --- [REWRITE]

const _operand = (program: string, args: readonly string[], index: number): number => {
    const word = args[index];
    return word === undefined || word === '-' || word === '--' || !word.startsWith('-') || !known(program, word) ? index : _operand(program, args, index + 1 + option(program, word).taken);
};

const _sd = (command: Command): readonly Insertion[] => {
    const last = command.invocations.at(-1);
    if (command.nested || last === undefined || last[0] !== 'sd') {
        return [];
    }
    const [program, ...args] = last;
    const offset = command.words.length - last.length;
    const { options } = operands(last);
    const operand = _operand(program, args, 0);
    const rest = args.slice(operand);
    const pattern = rest[0] === '--' ? rest[1] : rest[0];
    const fixed = options.some((name) => ['-F', '--fixed-strings'].includes(name));
    const across = command.spans[offset];
    const find = command.spans[offset + operand + 1];
    const breaks = pattern !== undefined && (fixed ? pattern.includes('\n') : _BREAK.test(pattern));
    const dashed = rest.some((word) => word !== '-' && word !== '--' && word.startsWith('-') && !known(program, word));
    return [...(across !== undefined && breaks && !_sourced(program, options) && !options.some((name) => ['-A', '--across'].includes(name)) ? [{ at: across.end, text: ' -A', flag: '-A' }] : []), ...(find !== undefined && !rest.includes('--') && dashed ? [{ at: find.start, text: '-- ', flag: '--' }] : [])];
};

const _spliced = (text: string, insertions: readonly Insertion[]): string => {
    const bytes = new TextEncoder().encode(text);
    const decoder = new TextDecoder();
    const done = insertions.toSorted((left, right) => left.at - right.at).reduce<{ readonly at: number; readonly pieces: readonly string[] }>((head, { at, text: inserted }) => ({ at, pieces: [...head.pieces, decoder.decode(bytes.subarray(head.at, at)), inserted] }), { at: 0, pieces: [] });
    return [...done.pieces, decoder.decode(bytes.subarray(done.at))].join('');
};

const commandRewrite = (commands: readonly Command[], text: string, lock: Option<string>): Option<Rewrite> => {
    const insertions = commands.flatMap(_sd);
    if (insertions.length === 0 && lock.kind === 'none') {
        return none;
    }
    const flags = [...new Set(insertions.map(({ flag }) => flag))].join(' and ');
    const spliced = _spliced(text, insertions);
    return some({
        command: lock.kind === 'some' ? `{ lockf 9 && {\n${spliced}\n} 9>&-; } 9>>'${lock.value.replaceAll("'", "'\\''")}'` : spliced,
        note: [...(insertions.length === 0 ? [] : [`sd ran with ${flags} added`]), ...(lock.kind === 'some' ? [`command queued under lockf on ${lock.value}`] : [])].join(', '),
        instruction: insertions.length === 0 ? none : some(`write ${flags}`),
    });
};

// --- [DECISION]

const callRefusal = (e: ToolCallInput): Option<string> => {
    if (e.tool === 'Write') {
        const name = basename(e.file_path);
        return _SECOND_FILES.some((pattern) => pattern.test(name)) ? some(`${name} is a second file beside its owner`) : none;
    }
    if (e.tool === 'EnterWorktree') {
        return some(`EnterWorktree ${_WORKTREE}`);
    }
    return e.tool === 'Agent' && e.isolation === 'worktree' ? some(`Agent isolation worktree ${_WORKTREE}`) : none;
};

const commandRefusal = (tool: 'Bash' | 'Monitor', script: Script, facts: Facts): Option<string> => {
    const { commands } = script;
    const walked = facts.walk.kind === 'some' ? _walk(commands, facts.walk.value) : [];
    const reasons = [_git(commands, facts.existing), _stdin(commands), _wait(commands), ...(tool === 'Bash' ? [_script(script)] : []), walked].find((found) => found.length > 0);
    return reasons === undefined ? none : some([...new Set(reasons)].join(', '));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Facts, Rewrite, Walk };
export { CLOUD, callRefusal, commandRefusal, commandRewrite, decodedManifest, decodedNxJson, gitPaths, LOCK, nxReviver, nxTargets, queues, targetCommands, walkStarts };
