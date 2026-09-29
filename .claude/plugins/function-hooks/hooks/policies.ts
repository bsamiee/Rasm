import type { ToolCallInput } from 'claude-code';
import type { Command, Script, Span } from './command.ts';
import { fromUndefined, none, type Option, some } from './composition.ts';
import { basename, type Invocation, invocations, known, type Operands, operands, option, PROGRAMS } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];

interface GitRow {
    readonly reason: string;
    readonly any?: true;
    readonly words?: readonly string[];
    readonly prefixes?: readonly string[];
    readonly safe?: readonly string[];
    readonly refine?: Refinement;
}
interface Head {
    readonly key: Key;
    readonly args: readonly string[];
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
    readonly program: string;
    readonly flag: string;
}
interface Rewrite {
    readonly command: string;
    readonly note: string;
    readonly instruction: string;
}
interface Shape {
    readonly options: readonly string[];
    readonly operand: number;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const CLOUD = 'Library/CloudStorage';
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

// --- [GIT] -----------------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');

const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !_isFlag(word));
    const [target] = targets;
    return target === undefined || args.includes('--') || targets.some((named) => existing.includes(named)) ? [] : [`git reset ${target} moves HEAD and drops commits from the branch`];
};

const _restore: Refinement = (args, _existing, reason) => {
    const { options } = operands(['git', ...args]);
    return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [`git restore ${reason}`];
};

const _config: Refinement = (args, _existing, reason) => {
    const alias = args.find((word) => word.startsWith('alias.'));
    return alias !== undefined && args.slice(args.indexOf(alias) + 1).some((word) => !_isFlag(word)) ? [`git config ${alias} ${reason}`] : [];
};

const _checkout: Refinement = (args, existing) => {
    const [first, ...more] = args.filter((word) => word === '-' || !_isFlag(word));
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

type Key = keyof typeof _GIT;

const _isKey = (candidate: string): candidate is Key => Object.hasOwn(_GIT, candidate);

const _skip = (words: readonly string[], index: number): number => {
    const word = words[index];
    return word !== undefined && _isFlag(word) ? _skip(words, index + 1 + option('git', word).taken) : index;
};

const _heads = (commands: readonly Command[]): readonly { readonly aliased: boolean; readonly head: Option<Head> }[] =>
    commands
        .flatMap((command) => invocations(command.words))
        .filter(([program]) => program === 'git')
        .map((words) => {
            const index = _skip(words, 1);
            const [key, ...args] = words.slice(index);
            return { aliased: words.slice(1, index).some((word) => word.startsWith('alias.')), head: key !== undefined && _isKey(key) ? some({ key, args }) : none };
        });

const _refusals = ({ key, args }: Head, existing: readonly string[]): readonly string[] => {
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

const _git = (commands: readonly Command[], existing: readonly string[]): readonly string[] =>
    _heads(commands).flatMap(({ aliased, head }) => {
        if (aliased) {
            return ['inline git alias can hide a refused subcommand'];
        }
        return head.kind === 'some' ? _refusals(head.value, existing) : [];
    });

const gitPaths = (commands: readonly Command[]): readonly string[] =>
    _heads(commands).flatMap(({ head }) => (head.kind === 'some' && (head.value.key === 'reset' || head.value.key === 'checkout') ? head.value.args.filter((word) => !_isFlag(word)) : []));

// --- [STDIN] ---------------------------------------------------------------------------

const _sourced = (program: string, options: readonly string[]): boolean => options.some((name) => ['--help', '--version'].includes(name) || PROGRAMS[program]?.sources?.includes(name) === true);

const _reads = (invocation: Invocation): boolean => {
    const row = PROGRAMS[invocation[0]];
    const { inputs, options } = operands(invocation);
    return (
        row?.stdin !== undefined &&
        !_sourced(invocation[0], options) &&
        !options.some((name) => row.recursive?.includes(name) === true) &&
        (row.stdin === 'always' || inputs.length === 0 || inputs.includes('-'))
    );
};

const _stdin = (commands: readonly Command[]): readonly string[] =>
    commands.flatMap((command) =>
        !command.fed && invocations(command.words).some(_reads)
            ? [`${command.words.join(' ')} reads standard input and nothing feeds it, pass an operand, pipe, heredoc, herestring, or input redirect`]
            : [],
    );

// --- [WAIT] ----------------------------------------------------------------------------

const _waits = (command: Command): readonly string[] => {
    const waiters: Readonly<Record<string, (parsed: Operands, wraps: boolean) => boolean>> = {
        sleep: () => true,
        pwait: () => true,
        wait: ({ inputs }) => inputs.length > 0,
        caffeinate: ({ options }, wraps) => !wraps || options.includes('-w'),
        tail: ({ options }) => options.includes('--pid'),
        lsof: ({ options }) => options.some((name) => name === '-r' || name === '+r'),
    };
    const chain = invocations(command.words);
    const line = command.words.join(' ');
    return [
        ...(chain.some((invocation, index) => waiters[invocation[0]]?.(operands(invocation), index < chain.length - 1) === true) ? [`${line} waits`] : []),
        ...(command.polled ? [`${line} repeats until the loop condition changes`] : []),
    ];
};

const _wait = (commands: readonly Command[]): readonly string[] => {
    const reasons = commands.flatMap(_waits);
    return reasons.length === 0 ? [] : [...reasons, "act on the command's own exit, or run it with run_in_background and act on its completion notification"];
};

// --- [SCRIPT] --------------------------------------------------------------------------

const _writes = (command: Command): readonly string[] =>
    invocations(command.words)
        .slice(-1)
        .flatMap(([program, ...rest]) => (program === 'echo' || program === 'printf' || (program === 'cat' && rest.length === 0) ? command.writes.filter((path) => !path.startsWith('/dev/')) : []));

const _script = ({ commands, clocks }: Script): readonly string[] => {
    const timer = "hyperfine -N -r <runs> '<command>' times commands";
    return [
        ...commands.flatMap((command, index) => {
            const written = commands.slice(0, index).flatMap(_writes);
            return [
                ...[...command.reads, ...command.words].filter((path) => written.includes(path)).map((path) => `${path} written then read in one call`),
                ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop`) : []),
                ...(invocations(command.words).some(([program]) => program === 'time') ? [`${command.words.join(' ')} times by hand, ${timer}`] : []),
            ];
        }),
        ...clocks.map((clock) => `${clock} times by hand, ${timer}`),
    ];
};

// --- [WALK] ----------------------------------------------------------------------------

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
        .flatMap((command) => invocations(command.words))
        .flatMap(_starts)
        .map((word) => [word, word.replace(_HOME, () => home)] as const);

const _descends = (invocation: Invocation, { cloud, places }: Walk): boolean => {
    const folders = _starts(invocation).flatMap((word) => places.get(word) ?? []);
    const excluded = invocation.some((word) => word.includes(basename(CLOUD)));
    return folders.some((folder) => folder.startsWith(cloud)) || (!excluded && folders.some((folder) => cloud.startsWith(folder)));
};

const _walk = (commands: readonly Command[], walk: Walk): readonly string[] =>
    commands.flatMap((command) =>
        invocations(command.words).some((invocation) => _descends(invocation, walk))
            ? [`${command.words.join(' ')} descends into ~/${CLOUD}, dataless cloud placeholders there hang the walker on the file provider, start outside it or exclude ${basename(CLOUD)}`]
            : [],
    );

// --- [REWRITE] -------------------------------------------------------------------------

const _dashed = (program: string, word: string): boolean => word !== '-' && word !== '--' && word.startsWith('-') && !known(program, word);

const _shape = (program: string, args: readonly string[], index: number): Shape => {
    const word = args[index];
    if (word === undefined || word === '-' || word === '--' || !word.startsWith('-') || !known(program, word)) {
        return { options: [], operand: index };
    }
    const { names, taken } = option(program, word);
    const tail = _shape(program, args, index + 1 + taken);
    return { options: [...names, ...tail.options], operand: tail.operand };
};

const _sd = (command: Command): readonly Insertion[] => {
    const last = invocations(command.words).at(-1);
    if (command.nested || last === undefined || last[0] !== 'sd') {
        return [];
    }
    const [program, ...args] = last;
    const span = (index: number): Option<Span> => fromUndefined(command.spans[command.words.length - last.length + index]);
    const { options, operand } = _shape(program, args, 0);
    const rest = args.slice(operand);
    const across = span(0);
    const find = span(operand + 1);
    return [
        ...(across.kind === 'some' && !_sourced(program, options) && !options.some((name) => ['-A', '--across'].includes(name)) ? [{ at: across.value.end, text: ' -A', program, flag: '-A' }] : []),
        ...(find.kind === 'some' && !rest.includes('--') && rest.some((word) => _dashed(program, word)) ? [{ at: find.value.start, text: '-- ', program, flag: '--' }] : []),
    ];
};

const _spliced = (text: string, insertions: readonly Insertion[]): string => {
    const bytes = new TextEncoder().encode(text);
    const decoder = new TextDecoder();
    const done = insertions
        .toSorted((left, right) => left.at - right.at)
        .reduce<{ readonly at: number; readonly pieces: readonly string[] }>(
            (head, { at, text: inserted }) => ({ at, pieces: [...head.pieces, decoder.decode(bytes.subarray(head.at, at)), inserted] }),
            { at: 0, pieces: [] },
        );
    return [...done.pieces, decoder.decode(bytes.subarray(done.at))].join('');
};

const commandRewrite = (commands: readonly Command[], text: string): Option<Rewrite> => {
    const insertions = commands.flatMap(_sd);
    if (insertions.length === 0) {
        return none;
    }
    const added = [...Map.groupBy(insertions, ({ program }) => program)].map(([program, own]) => [program, [...new Set(own.map(({ flag }) => flag))].join(' and ')] as const);
    return some({
        command: _spliced(text, insertions),
        note: added.map(([program, flags]) => `${program} ran with ${flags} added`).join(', '),
        instruction: `write ${added.map(([, flags]) => flags).join(' and ')}`,
    });
};

// --- [DECISION] ------------------------------------------------------------------------

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
export { CLOUD, callRefusal, commandRefusal, commandRewrite, gitPaths, walkStarts };
