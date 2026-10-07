import { type Command, parse, type Scanner, type Script } from './command.ts';
import { bind, decoded, fault, map, none, type Option, ok, type Result, rendered, some } from './composition.ts';
import { basename, type Invocation, known, type Operands, operands, option, PROGRAMS } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];
type OptionPredicate = (given: (...names: readonly string[]) => boolean, rest: readonly string[]) => boolean;
type Key = keyof typeof _GIT;
type GitCall = { readonly kind: 'aliased' } | { readonly kind: 'subcommand'; readonly key: Key; readonly args: readonly string[] };
type Decision = { readonly kind: 'deny'; readonly reason: string } | ({ readonly kind: 'rewrite' } & Rewrite) | { readonly kind: 'pass' };

interface Host {
    readonly scan: Scanner;
    readonly read: (path: string) => Promise<Result<string>>;
    readonly repo: () => Promise<Option<string>>;
    readonly make: (folders: readonly string[]) => Promise<Result<unknown>>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly real: (path: string) => Promise<Option<string>>;
    readonly home: () => Promise<Option<string>>;
}
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
    readonly context: string;
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
interface Configured {
    readonly project: Option<string>;
    readonly key: RegExp;
    readonly configuration: TargetConfiguration;
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
const WORKTREE = 'creates a second checkout with its own metadata and sync cost';
const _HOME = /^(?:~|\$HOME|\$\{HOME\})(?=\/|$)/u;
const _PRIMARY = /^(?:-.{2,}|\(|!)$/u;
const _BREAK = /\n|(?<!\\)(?:\\\\)*\\n/u;
const _DESCRIPTOR = /^\d+$/u;
const _TRAILING = /\/$/u;
const _SECOND_FILES: readonly RegExp[] = [
    /^(?:project\.json|\.nxignore)$/u,
    /^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u,
    /^tsconfig\.(?!base\.json$).+\.json$/u,
    /^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u,
    /^biome\.jsonc$/u,
    /^\.yamllint(?:\.yml)?$/u,
];

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
    worktree: { reason: WORKTREE, any: true, safe: ['list'] },
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

const _gitPaths = (commands: readonly Command[]): readonly string[] => _calls(commands).flatMap((call) => (call.kind === 'subcommand' && (call.key === 'reset' || call.key === 'checkout') ? call.args.filter((word) => !word.startsWith('-')) : []));

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

const _located = async (real: Host['real'], path: string): Promise<Option<string>> => {
    const cut = path.lastIndexOf('/');
    const own = await real(path);
    const found = own.kind === 'some' ? own : await real(cut < 0 ? '.' : path.slice(0, cut + 1)).then((folder) => (folder.kind === 'some' ? some(`${folder.value.replace(_TRAILING, '')}/${path.slice(cut + 1)}`) : none));
    return found.kind === 'some' ? some(`${found.value.replace(_TRAILING, '')}/`) : none;
};

const _places = async ({ real, home }: Host, commands: readonly Command[]): Promise<Option<Walk>> => {
    const root = await home();
    if (root.kind === 'none') {
        return none;
    }
    const starts = commands
        .flatMap((command) => command.invocations)
        .flatMap(_starts)
        .map((word) => [word, word.replace(_HOME, () => root.value)] as const);
    const [cloud, places] = await Promise.all([_located(real, `${root.value}/${CLOUD}`), Promise.all(starts.map(([word, path]) => _located(real, path).then((place) => (place.kind === 'some' ? [[word, place.value] as const] : []))))]);
    return cloud.kind === 'some' ? some({ cloud: cloud.value, places: new Map(places.flat()) }) : none;
};

const _descends = (invocation: Invocation, { cloud, places }: Walk): boolean => {
    const folders = _starts(invocation).flatMap((word) => places.get(word) ?? []);
    const excluded = invocation.some((word) => word.includes(basename(CLOUD)));
    return folders.some((folder) => folder.startsWith(cloud)) || (!excluded && folders.some((folder) => cloud.startsWith(folder)));
};

const _walk = (commands: readonly Command[], walk: Walk): readonly string[] =>
    commands.flatMap((command) => (command.invocations.some((invocation) => _descends(invocation, walk)) ? [`${command.words.join(' ')} descends into ~/${CLOUD}, dataless cloud placeholders there hang the walker on the file provider, start outside it or exclude ${basename(CLOUD)}`] : []));

// --- [QUEUE]

const queues = (commands: readonly Command[]): readonly string[] => {
    const always: OptionPredicate = () => true;
    const unfrozen: OptionPredicate = (given) => !given('--frozen');
    const writes: OptionPredicate = (given) => given('-U', '--update-all', '-i', '--interactive') && !given('--stdin');
    const families: readonly (readonly [string, readonly (readonly [readonly string[], OptionPredicate])[]])[] = [
        [
            '.cache/uv-resolver.lock',
            [
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
            ],
        ],
        [
            '.cache/ast-grep-rewrite.lock',
            [
                [['ast-grep', 'scan'], writes],
                [['sg', 'scan'], writes],
            ],
        ],
    ];
    const invocations = commands.flatMap((command) => command.invocations);
    const calls = invocations.map((invocation) => {
        const { inputs, options } = operands(invocation);
        return { words: [invocation[0], ...inputs], given: (...names: readonly string[]): boolean => options.some((name) => names.includes(name)) };
    });
    const resolved = families.flatMap(([lock, resolvers]) => resolvers.map(([path, resolves]) => ({ lock, path, resolves }))).filter(({ path, resolves }) => calls.some(({ words, given }) => !given('-h', '--help') && path.every((word, index) => words[index] === word) && resolves(given, words.slice(path.length))));
    return [...new Set(resolved.map(({ lock }) => lock))].filter((lock) => !invocations.some(([program, ...args]) => program === 'lockf' && args.slice(-1).some((file) => _DESCRIPTOR.test(file) || file.endsWith(lock))));
};

const _nxSpecifiers = (commands: readonly Command[]): readonly (readonly string[])[] =>
    commands
        .flatMap((command) => command.invocations)
        .filter(([program]) => program === 'nx')
        .flatMap(([, ...words]) => words.flatMap((word) => word.slice(word.indexOf('=') + 1).split(',')))
        .map((item) => item.split(':'));

const _nxReviver = (key: string, value: unknown): unknown => {
    const expanded = (entry: string): object => (key === 'commands' ? { command: entry } : { target: entry.slice(entry.startsWith('^') ? 1 : 0), dependencies: entry.startsWith('^') });
    return Array.isArray(value) && (key === 'commands' || key === 'dependsOn') ? value.map((entry: unknown) => (typeof entry === 'string' ? expanded(entry) : entry)) : value;
};

const _object = (value: unknown): value is Readonly<Record<string, unknown>> => typeof value === 'object' && value !== null;

const _decodedManifest = (subject: string, value: unknown): Result<Manifest> => {
    const declared = (candidate: unknown): candidate is Manifest => _object(candidate) && typeof candidate.name === 'string' && _object(candidate.nx) && _object(candidate.nx.targets);
    return declared(value) ? ok(value) : fault({ kind: 'invalid', subject, cause: 'name or nx.targets missing' });
};

const _decodedNxJson = (subject: string, value: unknown): Result<NxJsonConfiguration> => {
    const declared = (candidate: unknown): candidate is NxJsonConfiguration => _object(candidate) && _object(candidate.targetDefaults);
    return declared(value) ? ok(value) : fault({ kind: 'invalid', subject, cause: 'targetDefaults missing' });
};

const _key = (key: string): RegExp =>
    new RegExp(
        `^${key
            .replaceAll(/[$()+.[\]\\^|]/gu, '\\$&')
            .replaceAll('*', '.*')
            .replaceAll('?', '.')
            .replaceAll(/\{(?<names>[^}]*)\}/gu, (_brace, names: string) => `(?:${names.replaceAll(',', '|')})`)}$`,
        'u',
    );

const _owned = (owner: Option<string>, project: Option<string>): boolean => owner.kind === 'none' || project.kind === 'none' || owner.value === project.value;

const _targetCommands = (specifiers: readonly (readonly string[])[], manifest: Manifest, nxJson: NxJsonConfiguration): readonly string[] => {
    const configured: readonly Configured[] = [
        ...Object.entries(manifest.nx.targets).map(([target, configuration]) => ({ project: some(manifest.name), key: _key(target), configuration })),
        ...Object.entries(nxJson.targetDefaults).flatMap(([target, configurations]) => [configurations].flat().map((configuration) => ({ project: none, key: _key(target), configuration }))),
    ];
    const opened = (segments: readonly string[], key: RegExp): number => segments.findLastIndex((_segment, index) => key.test(segments.slice(0, index + 1).join(':'))) + 1;
    const specified = specifiers.flatMap((segments) => {
        const readings: readonly (readonly [Option<string>, readonly string[]])[] = [[none, segments], ...segments.slice(0, 1).map((owner) => [some(owner), segments.slice(1)] as const)];
        const scored = readings.flatMap(([owner, rest]) =>
            configured.flatMap((entry) => {
                const depth = _owned(owner, entry.project) ? opened(rest, entry.key) : 0;
                return depth === 0 ? [] : [{ entry, depth: segments.length - rest.length + depth }];
            }),
        );
        const deepest = Math.max(0, ...scored.map(({ depth }) => depth));
        return scored.flatMap(({ entry, depth }) => (depth === deepest ? [entry] : []));
    });
    const upstream = ({ project, configuration }: Configured): readonly Target[] => (configuration.dependsOn ?? []).flatMap(({ target, projects, dependencies }) => (projects === undefined ? [dependencies === true ? none : project] : [projects].flat().map(some)).map((owner) => ({ project: owner, target })));
    const reached = (pending: readonly Target[], seen: ReadonlySet<Configured>): ReadonlySet<Configured> => {
        const found = configured.filter((entry) => !seen.has(entry) && pending.some(({ project, target }) => entry.key.test(target) && _owned(project, entry.project)));
        return found.length === 0 ? seen : reached(found.flatMap(upstream), new Set([...seen, ...found]));
    };
    return [...reached(specified.flatMap(upstream), new Set(specified))].flatMap(({ configuration: { command, options } }) => [command, options?.command, ...(options?.commands ?? []).map((entry) => entry.command)].filter((text) => text !== undefined));
};

const _locks = async ({ scan, read, repo, make }: Host, commands: readonly Command[]): Promise<Result<readonly string[]>> => {
    const named = _nxSpecifiers(commands);
    const root = named.length > 0 || queues(commands).length > 0 ? await repo() : none;
    if (root.kind === 'none') {
        return ok([]);
    }
    const decode = <T>(path: string, declared: (subject: string, value: unknown) => Result<T>): Promise<Result<T>> => read(`${root.value}/${path}`).then((text) => bind(decoded<unknown>(path, text, _nxReviver), (value) => declared(path, value)));
    const lines = named.length === 0 ? ok<readonly string[]>([]) : await Promise.all([decode('package.json', _decodedManifest), decode('nx.json', _decodedNxJson)]).then(([manifest, nxJson]) => bind(manifest, (targets) => map(nxJson, (defaults) => _targetCommands(named, targets, defaults))));
    const reached = await bind(lines, async (text) => (text.length === 0 ? ok([]) : map(await parse(scan, text.join('\n')), (script) => script.commands)));
    const queued = map(reached, (targeted) => queues([...commands, ...targeted]).map((lock) => `${root.value}/${lock}`));
    return bind(queued, async (paths) => (paths.length === 0 ? queued : map(await make(paths.map((lock) => lock.slice(0, lock.lastIndexOf('/')))), () => paths)));
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

const _rewrite = (commands: readonly Command[], text: string, held: readonly string[]): Option<Rewrite> => {
    const insertions = commands.flatMap(_sd);
    if (insertions.length === 0 && held.length === 0) {
        return none;
    }
    const flags = [...new Set(insertions.map(({ flag }) => flag))].join(' and ');
    const note = [...(insertions.length === 0 ? [] : [`sd ran with ${flags} added`]), ...(held.length === 0 ? [] : [`command queued under lockf on ${held.join(' and ')}`])].join(', ');
    return some({
        command: held.reduce((body, lock) => `{ lockf 9 && {\n${body}\n} 9>&-; } 9>>'${lock.replaceAll("'", "'\\''")}'`, _spliced(text, insertions)),
        note,
        context: insertions.length === 0 ? note : `${note}, write ${flags}`,
    });
};

// --- [DECISION]

const _refusal = (tool: 'Bash' | 'Monitor', script: Script, facts: Facts): Option<string> => {
    const { commands } = script;
    const walked = facts.walk.kind === 'some' ? _walk(commands, facts.walk.value) : [];
    const reasons = [_git(commands, facts.existing), _stdin(commands), _wait(commands), ...(tool === 'Bash' ? [_script(script)] : []), walked].find((found) => found.length > 0);
    return reasons === undefined ? none : some([...new Set(reasons)].join(', '));
};

const _facts = async (host: Host, commands: readonly Command[], walking: boolean): Promise<Facts> => {
    const [existing, walk] = await Promise.all([Promise.all(_gitPaths(commands).map((path) => host.exists(path).then((found) => (found ? [path] : [])))), walking ? _places(host, commands) : none]);
    return { existing: existing.flat(), walk };
};

const commandDecision = async (host: Host, tool: 'Bash' | 'Monitor', command: string, walking: boolean): Promise<Decision> => {
    const parsed = await parse(host.scan, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `command not parsed, ${rendered(parsed.fault)}` };
    }
    const { commands } = parsed.value;
    const refusal = _refusal(tool, parsed.value, await _facts(host, commands, walking && tool === 'Bash'));
    if (refusal.kind === 'some') {
        return { kind: 'deny', reason: refusal.value };
    }
    const queue = tool === 'Bash' ? await _locks(host, commands) : ok<readonly string[]>([]);
    if (queue.kind === 'fault') {
        return { kind: 'deny', reason: `command not queued, ${rendered(queue.fault)}` };
    }
    const rewrite = _rewrite(commands, command, queue.value);
    return rewrite.kind === 'some' ? { kind: 'rewrite', ...rewrite.value } : { kind: 'pass' };
};

const pathRefusal = (paths: readonly string[]): Option<string> => {
    const names = paths.map(basename).filter((name) => _SECOND_FILES.some((pattern) => pattern.test(name)));
    return names.length === 0 ? none : some(names.map((name) => `${name} is a second file beside its owner`).join(', '));
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Host, Rewrite };
export { commandDecision, pathRefusal, WORKTREE };
