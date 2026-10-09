import { none, type Option, rendered, some } from '../composition.ts';
import { type Command, offset, parse, type Scanner, type Script } from './command.ts';
import { basename, declared, firstOperand, type Invocation, known, type Operands, operands } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];
type GitCall = { readonly kind: 'aliased' } | GitSubcommand;
type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow'; readonly rewrite: Option<Rewrite> };

interface Host {
    readonly scan: Scanner;
    readonly repo: () => Promise<Option<string>>;
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
interface GitSubcommand {
    readonly kind: 'subcommand';
    readonly key: string;
    readonly row: GitRow;
    readonly args: readonly string[];
}
interface Splice {
    readonly start: number;
    readonly end: number;
    readonly text: string;
}
interface Build {
    readonly at: number;
    readonly subcommand: string;
}
interface Rewrite {
    readonly command: string;
    readonly notice: string;
    readonly context: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _CLOUD = 'Library/CloudStorage';
const _WORKTREE = 'creates a second checkout with its own metadata and sync cost. Work in the main checkout';
const _HOME = /^(?:~|\$HOME|\$\{HOME\})(?=\/|$)/u;
const _PRIMARY = /^(?:-.{2,}|\(|!)$/u;
const _BREAK = /\n|(?<!\\)(?:\\\\)*\\n/u;
const _TRAILING = /\/$/u;
const _BINLOG = /^(?:--?|\/)(?:bl|binarylogger)(?::|$)/iu;
const _NX_ENTRY = /(?:^|\/)nx\/bin\/nx(?:\.js)?$/u;
const _DOTNET: ReadonlyMap<string, string> = new Map(
    Object.entries({
        build: 'compiles outside the Nx task graph and its cache. Run nx run <project>:build, -- forwarding MSBuild switches and --skip-nx-cache forcing the run, then read the binary log under .artifacts/dotnet/binlog/ through the binlog MCP',
        test: 'runs tests outside the Nx task graph and its cache. Run nx run <project>:test',
        format: 'checks or rewrites files outside the Nx task graph. Run nx run rasm:lint:dotnet-format to check or nx run rasm:format to write',
    }),
);
const _MINI_CONFIGS: readonly (readonly [RegExp, string])[] = [
    [/^project\.json$/u, 'package.json'],
    [/^\.nxignore$/u, '.gitignore'],
    [/^(?:\.mise(?:\..+)?\.toml|mise\..+\.toml|\.miserc\.toml|\.rtx\.toml|\.tool-versions|\.nvmrc|\.(?:node|python)-version)$/u, 'mise.toml'],
    [/^tsconfig\.(?!base\.json$).+\.json$/u, 'tsconfig.json'],
    [/^(?:\.?ruff\.toml|\.?mypy\.ini|pytest\.ini|tox\.ini|setup\.cfg)$/u, 'pyproject.toml'],
    [/^biome\.jsonc$/u, 'biome.json'],
    [/^\.yamllint(?:\.yml)?$/u, '.yamllint.yaml'],
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
    const separated = args.includes('--');
    if (!separated && args.some((word) => ['-b', '--orphan', '-t', '--track', '--detach'].includes(word))) {
        return [];
    }
    if (separated || more.length > 0 || first === '.' || first?.startsWith(':') === true) {
        return ['git checkout with a pathspec overwrites working-tree files. Edit the files'];
    }
    return first !== undefined && first !== '-' && existing.includes(first) ? [`git checkout ${first} names an existing path it overwrites. Edit the file`] : [];
};

const _GIT: ReadonlyMap<string, GitRow> = new Map(
    Object.entries({
        branch: { reason: 'deletes or force-moves a branch', words: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
        checkout: { reason: 'discards local changes', words: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], prefixes: ['--force'], refine: _checkout },
        clean: { reason: 'deletes untracked files. Remove the named files with rm', any: true },
        config: { reason: 'defines a git alias that can hide a refused subcommand', refine: _config },
        push: { reason: 'rewrites or deletes remote history', words: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
        rebase: { reason: 'rewrites commits other agents can hold', any: true },
        reflog: { reason: 'erases reflog entries, the last recovery path', words: ['delete', 'drop', 'expire'] },
        reset: { reason: 'wipes working-tree or index state', words: ['--hard', '--merge', '--keep'], refine: _reset },
        restore: { reason: 'overwrites working-tree files. Edit the files', refine: _restore },
        revert: { reason: 'reverses committed history', any: true },
        stash: { reason: 'hides uncommitted work other agents depend on. Commit to a branch', any: true, safe: ['list', 'show'] },
        switch: { reason: 'discards local changes', words: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
        worktree: { reason: _WORKTREE, any: true, safe: ['list'] },
    } satisfies Record<string, GitRow>),
);

const _calls = (commands: readonly Command[]): readonly GitCall[] =>
    commands
        .flatMap((command) => command.invocations)
        .filter(([program]) => program === 'git')
        .flatMap((invocation): readonly GitCall[] => {
            const index = firstOperand(invocation, 1);
            const [key, ...args] = invocation.slice(index);
            if (invocation.slice(1, index).some((word) => word.startsWith('alias.'))) {
                return [{ kind: 'aliased' }];
            }
            const row = key === undefined ? undefined : _GIT.get(key);
            return key === undefined || row === undefined ? [] : [{ kind: 'subcommand', key, row, args }];
        });

const _refusals = ({ key, row, args }: GitSubcommand, existing: readonly string[]): readonly string[] => {
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

const _git = (calls: readonly GitCall[], existing: readonly string[]): readonly string[] => calls.flatMap((call) => (call.kind === 'aliased' ? ['inline git alias can hide a refused subcommand'] : _refusals(call, existing)));

// --- [STDIN]

const _informational = (program: string, options: readonly string[]): boolean => options.some((name) => name === '--help' || name === '--version' || declared(program).informational?.includes(name) === true);

const _reads = (invocation: Invocation): boolean => {
    const [program] = invocation;
    const { stdin, stdinless, recursive } = declared(program);
    const { inputs, options } = operands(invocation);
    const fed = _informational(program, options) || options.some((name) => stdinless?.includes(name) === true || recursive?.includes(name) === true);
    return stdin !== undefined && !fed && (stdin === 'always' || inputs.length === 0 || inputs.includes('-'));
};

const _stdin = (commands: readonly Command[]): readonly string[] => commands.flatMap((command) => (!command.fed && command.invocations.some(_reads) ? [`${command.words.join(' ')} reads standard input and nothing feeds it. Pass an operand, pipe, heredoc, herestring, or input redirect`] : []));

// --- [WAIT]

const _waits = (command: Command): readonly string[] => {
    const waiters: ReadonlyMap<string, (parsed: Operands, wraps: boolean) => boolean> = new Map(
        Object.entries({
            sleep: () => true,
            pwait: () => true,
            wait: ({ inputs }) => inputs.length > 0,
            caffeinate: ({ options }, wraps) => !wraps || options.includes('-w'),
            tail: ({ options }) => options.includes('--pid'),
            lsof: ({ options }) => options.some((name) => name === '-r' || name === '+r'),
        } satisfies Record<string, (parsed: Operands, wraps: boolean) => boolean>),
    );
    const chain = command.invocations;
    const line = command.words.join(' ');
    return [...(chain.some((invocation, index) => waiters.get(invocation[0])?.(operands(invocation), index < chain.length - 1) === true) ? [`${line} waits`] : []), ...(command.polled ? [`${line} repeats until the loop condition changes`] : [])];
};

const _wait = (commands: readonly Command[]): readonly string[] => {
    const reasons = commands.flatMap(_waits);
    return reasons.length === 0 ? [] : [...reasons, "Act on the command's own exit, or run it with run_in_background and act on its completion notification"];
};

// --- [SCRIPT]

const _writes = (command: Command): readonly string[] => command.invocations.slice(-1).flatMap(([program, ...rest]) => (program === 'echo' || program === 'printf' || (program === 'cat' && rest.length === 0) ? command.writes.filter((path) => !path.startsWith('/dev/')) : []));

const _script = ({ commands, clocks }: Script): readonly string[] => {
    const timer = "times by hand. Time it with hyperfine -N -r <runs> '<command>'";
    return [
        ...commands.flatMap((command, index) => {
            const written = commands.slice(0, index).flatMap(_writes);
            return [
                ...[...command.reads, ...command.words].filter((path) => written.includes(path)).map((path) => `${path} written then read in one call. Read it in a later call`),
                ...(command.looped ? _writes(command).map((path) => `${path} appended in a loop. Write it once after the loop`) : []),
                ...(command.invocations.some(([program]) => program === 'time') ? [`${command.words.join(' ')} ${timer}`] : []),
            ];
        }),
        ...clocks.map((clock) => `${clock} ${timer}`),
    ];
};

// --- [DOTNET]

const _subcommand = (invocation: Invocation): Option<string> => {
    const [program, subcommand] = invocation;
    return program !== 'dotnet' || subcommand === undefined || _informational(program, operands(invocation).options) ? none : some(subcommand);
};

const _dotnet = (commands: readonly Command[]): readonly string[] =>
    commands
        .flatMap((command) => command.invocations)
        .flatMap((invocation) => {
            const subcommand = _subcommand(invocation);
            const reason = subcommand.kind === 'some' ? _DOTNET.get(subcommand.value) : undefined;
            return subcommand.kind === 'none' || reason === undefined ? [] : [`dotnet ${subcommand.value} ${reason}`];
        });

// --- [WALK]

const _starts = (invocation: Invocation): readonly string[] => {
    const [program, ...args] = invocation;
    const { inputs, options, values } = operands(invocation);
    const here = (words: readonly string[]): readonly string[] => (words.length === 0 ? ['.'] : words);
    const given = (names: readonly string[]): readonly string[] => values.flatMap(([name, value]) => (names.includes(name) ? [value] : []));
    const recursive = (): readonly string[] => (options.some((name) => declared(program).recursive?.includes(name) === true) ? here(inputs) : []);
    const walkers: ReadonlyMap<string, () => readonly string[]> = new Map(
        Object.entries({
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
        } satisfies Record<string, () => readonly string[]>),
    );
    return walkers.get(program)?.() ?? [];
};

const _located = async (real: Host['real'], path: string): Promise<Option<string>> => {
    const cut = path.lastIndexOf('/');
    const own = await real(path);
    const found = own.kind === 'some' ? own : await real(cut < 0 ? '.' : path.slice(0, cut + 1)).then((folder) => (folder.kind === 'some' ? some(`${folder.value.replace(_TRAILING, '')}/${path.slice(cut + 1)}`) : none));
    return found.kind === 'some' ? some(`${found.value.replace(_TRAILING, '')}/`) : none;
};

const _descends = (invocation: Invocation, cloud: string, places: ReadonlyMap<string, string>): boolean => {
    const folders = _starts(invocation).flatMap((word) => places.get(word) ?? []);
    const excluded = invocation.some((word) => word.includes(basename(_CLOUD)));
    return folders.some((folder) => folder.startsWith(cloud)) || (!excluded && folders.some((folder) => cloud.startsWith(folder)));
};

const _walk = async (real: Host['real'], home: string, commands: readonly Command[]): Promise<readonly string[]> => {
    const starts = commands.flatMap((command) => command.invocations).flatMap(_starts);
    const [cloud, located] = await Promise.all([
        _located(real, `${home}/${_CLOUD}`),
        Promise.all(
            starts.map(
                async (word) =>
                    [
                        word,
                        await _located(
                            real,
                            word.replace(_HOME, () => home),
                        ),
                    ] as const,
            ),
        ),
    ]);
    const places = new Map(located.flatMap(([word, place]) => (place.kind === 'some' ? [[word, place.value] as const] : [])));
    return cloud.kind === 'none'
        ? []
        : commands.flatMap((command) => (command.invocations.some((invocation) => _descends(invocation, cloud.value, places)) ? [`${command.words.join(' ')} descends into ~/${_CLOUD}. Dataless cloud placeholders there hang walkers on the file provider. Start outside ~/${_CLOUD} or exclude ${basename(_CLOUD)}`] : []));
};

// --- [REWRITE]

const _quoted = (text: string): string => `'${text.replaceAll("'", "'\\''")}'`;

const _sourceWrites = (commands: readonly Command[]): readonly Splice[] => {
    const modes = ['-U', '--update-all', '-i', '--interactive'];
    const calls = commands.flatMap((command) => command.invocations.map((invocation, index) => ({ command, invocation, at: offset(command, index), parsed: operands(invocation) })));
    const writers = calls.filter(({ invocation, parsed }) => (invocation[0] === 'ast-grep' || invocation[0] === 'sg') && parsed.inputs[0] === 'scan' && parsed.options.some((option) => modes.includes(option)) && !parsed.options.includes('--stdin'));
    const prefixes = writers.flatMap(({ command, at, parsed }) => {
        const start = command.spans[at];
        const scan = command.spans[at + 1];
        const interactive = parsed.options.includes('-i') || parsed.options.includes('--interactive');
        return start === undefined || scan === undefined ? [] : [{ start: start.start, end: scan.end, text: `mise exec -- nx run rasm:rewrite${interactive ? ' --args=-i' : ''} --` }];
    });
    const positions = writers.flatMap(({ command, at, parsed }) => parsed.positions.map(({ at: position, names }) => ({ span: command.spans[at + position], names })));
    const flags = positions
        .filter(({ names }) => names.some((name) => modes.includes(name)))
        .flatMap(({ span, names }) => {
            const remaining = names.filter((name) => !modes.includes(name));
            return span === undefined ? [] : [{ start: span.start, end: span.end, text: remaining.length === 0 ? '' : `-${remaining.map((name) => name.slice(1)).join('')}` }];
        });
    return [...prefixes, ...flags];
};

const _sd = (command: Command): readonly (Splice & { readonly option: '-A' | '--' })[] =>
    command.nested
        ? []
        : command.invocations
              .slice(-1)
              .filter(([program]) => program === 'sd')
              .flatMap((invocation) => {
                  const [program] = invocation;
                  const start = offset(command, command.invocations.length - 1);
                  const { options } = operands(invocation);
                  const operand = firstOperand(invocation, 1);
                  const rest = invocation.slice(operand);
                  const pattern = rest[0] === '--' ? rest[1] : rest[0];
                  const across = command.spans[start];
                  const find = command.spans[start + operand];
                  const breaks = pattern !== undefined && (options.some((name) => name === '-F' || name === '--fixed-strings') ? pattern.includes('\n') : _BREAK.test(pattern));
                  const dashed = rest.some((word) => word !== '-' && word !== '--' && word.startsWith('-') && !known(program, word));
                  return [
                      ...(across !== undefined && breaks && !_informational(program, options) && !options.some((name) => name === '-A' || name === '--across') ? [{ start: across.end, end: across.end, text: ' -A', option: '-A' as const }] : []),
                      ...(find !== undefined && dashed && !rest.includes('--') ? [{ start: find.start, end: find.start, text: '-- ', option: '--' as const }] : []),
                  ];
              });

const _managed = (command: Command): boolean => command.invocations.some(([program]) => program === 'mise');
const _runsNx = (invocation: Invocation): boolean =>
    invocation[0] === 'nx' ||
    (invocation[0] === 'node' &&
        operands(invocation)
            .inputs.slice(0, 1)
            .some((entry) => _NX_ENTRY.test(entry)));

const _nx = (commands: readonly Command[]): readonly Splice[] => [
    ...new Map(
        commands.flatMap((command, position) => {
            const root = commands.slice(0, position + 1).findLast((other) => !other.nested);
            const index = command.invocations.findIndex(_runsNx);
            const prior = command.invocations[index - 1]?.[0];
            const launched = !command.nested && prior !== undefined && ['npx', 'npm', 'pnpm'].includes(prior);
            const from = command.nested ? root?.spans[0] : command.spans[offset(command, launched ? index - 1 : index)];
            const to = command.spans[offset(command, index)];
            return index < 0 || _managed(command) || root === undefined || _managed(root) || from === undefined || to === undefined ? [] : [[from.start, { start: from.start, end: launched ? to.start : from.start, text: 'mise exec -- ' }] as const];
        }),
    ).values(),
];

const _builds = (command: Command): readonly Build[] =>
    command.nested
        ? []
        : command.invocations.flatMap((invocation, index) => {
              const subcommand = _subcommand(invocation);
              const named = command.spans[offset(command, index) + 1];
              return subcommand.kind === 'none' || !['publish', 'pack', 'msbuild'].includes(subcommand.value) || invocation.slice(2).some((word) => _BINLOG.test(word)) || named === undefined ? [] : [{ at: named.end, subcommand: subcommand.value }];
          });

const _rewrite = (commands: readonly Command[], text: string, root: Option<string>, id: string): Option<Rewrite> => {
    const actions = { '-A': 'Pass -A on a find holding a line break', '--': 'Pass -- before a find opening with -' } as const;
    const sd = commands.flatMap(_sd);
    const added = [...new Set(sd.map(({ option }) => option))];
    const unmanaged = _nx(commands);
    const writers = _sourceWrites(commands);
    const builds = root.kind === 'none' ? [] : commands.flatMap(_builds).map(({ at, subcommand }, index) => ({ at, path: `${root.value}/.artifacts/dotnet/binlog/${subcommand}-${id}-${index + 1}.binlog` }));
    const notes: readonly (readonly [notice: string, ...actions: string[]])[] = [
        ...(sd.length === 0 ? [] : [[`sd ran with ${added.join(' and ')} added`, ...added.map((option) => actions[option])] as const]),
        ...(unmanaged.length === 0 ? [] : [['nx ran under mise exec', 'Call nx as mise exec -- nx'] as const]),
        ...(builds.length === 0 ? [] : [['dotnet ran with -bl added', `Diagnose a failed build from ${builds.map(({ path }) => path).join(' and ')} with the dotnet-msbuild-diagnostics skill`] as const]),
        ...(writers.length === 0 ? [] : [['ast-grep writes run through rasm:rewrite', 'Nx owns rewrite execution and source exclusion'] as const]),
    ];
    const bytes = new TextEncoder().encode(text);
    const decoder = new TextDecoder();
    const spliced = [...sd, ...unmanaged, ...writers, ...builds.map(({ at, path }) => ({ start: at, end: at, text: ` -bl:${_quoted(path)}` }))]
        .toSorted((left, right) => left.start - right.start)
        .reduce<{ readonly at: number; readonly pieces: readonly string[] }>((head, { start, end, text: inserted }) => ({ at: end, pieces: [...head.pieces, decoder.decode(bytes.subarray(head.at, start)), inserted] }), { at: 0, pieces: [] });
    return notes.length === 0
        ? none
        : some({
              command: [...spliced.pieces, decoder.decode(bytes.subarray(spliced.at))].join(''),
              notice: notes.map(([notice]) => notice).join(' · '),
              context: notes.flat().join('. '),
          });
};

// --- [DECISION]

const _refusal = async (host: Host, tool: 'Bash' | 'Monitor', script: Script, walkPolicy: boolean): Promise<Option<string>> => {
    const { commands } = script;
    const calls = _calls(commands);
    const named = calls.flatMap((call) => (call.kind === 'subcommand' && (call.key === 'reset' || call.key === 'checkout') ? call.args.filter((word) => !word.startsWith('-')) : []));
    const [existing, home] = await Promise.all([Promise.all(named.map(async (path) => ((await host.exists(path)) ? [path] : []))), walkPolicy && tool === 'Bash' ? host.home() : none]);
    const walked = home.kind === 'some' ? await _walk(host.real, home.value, commands) : [];
    const reasons = [_git(calls, existing.flat()), _stdin(commands), _wait(commands), ...(tool === 'Bash' ? [_script(script), _dotnet(commands)] : []), walked].find((found) => found.length > 0);
    return reasons === undefined ? none : some([...new Set(reasons)].join('. '));
};

const _allowed = async (host: Host, tool: 'Bash' | 'Monitor', commands: readonly Command[], text: string, id: string): Promise<Decision> => {
    const root = tool === 'Bash' && commands.some((command) => _builds(command).length > 0) ? await host.repo() : none;
    return { kind: 'allow', rewrite: _rewrite(commands, text, root, id) };
};

const commandDecision = async (host: Host, tool: 'Bash' | 'Monitor', command: string, id: string, walkPolicy: boolean): Promise<Decision> => {
    const parsed = await parse(host.scan, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `command not parsed, ${rendered(parsed.faults)}` };
    }
    const refusal = await _refusal(host, tool, parsed.value, walkPolicy);
    return refusal.kind === 'some' ? { kind: 'deny', reason: refusal.value } : _allowed(host, tool, parsed.value.commands, command, id);
};

const pathRefusal = (paths: readonly string[]): Option<string> => {
    const found = paths.map(basename).flatMap((name) => _MINI_CONFIGS.flatMap(([pattern, owner]) => (pattern.test(name) ? [`${name} is a mini config. Edit ${owner}`] : [])));
    return found.length === 0 ? none : some(found.join('. '));
};

const worktreeRefusal = (tool: 'EnterWorktree' | 'Agent'): string => `${tool === 'Agent' ? 'Agent with isolation worktree' : tool} ${_WORKTREE}`;

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Host, Rewrite };
export { commandDecision, pathRefusal, worktreeRefusal };
