import { none, type Option, rendered, some } from '../composition.ts';
import { type Command, offset, parse, type Scanner, type Script, type Span } from './command.ts';
import { basename, declared, firstOperand, type Invocation, informational, operands, placed, quoted, segments, waits, within } from './invocation.ts';
import { type Catalog, type Route, route, solutions } from './targets.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];
type GitCall = { readonly kind: 'aliased' } | { readonly kind: 'unplaced'; readonly key: string } | GitSubcommand;
type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow'; readonly rewrite: Option<Rewrite> };

interface Host {
    readonly scan: Scanner;
    readonly root: () => Promise<Option<string>>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly cwd: () => Promise<string>;
    readonly home: () => Promise<Option<string>>;
    readonly catalog: () => Promise<Option<Catalog>>;
    readonly read: (path: string) => Promise<Option<string>>;
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
    readonly cwd: string;
}
interface Located {
    readonly root: Option<string>;
    readonly home: Option<string>;
    readonly commands: readonly (readonly [command: Command, cwd: Option<string>])[];
}
interface Splice extends Span {
    readonly text: string;
}
interface Change {
    readonly splices: readonly Splice[];
    readonly notice: Option<string>;
}
interface Rewrite {
    readonly command: string;
    readonly notice: Option<string>;
}
interface Routed {
    readonly command: Command;
    readonly position: number;
    readonly routes: readonly Route[];
    readonly denials: readonly string[];
    readonly launchers: readonly Invocation[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates a second checkout. Work in the main checkout';
const _UNPLACED = 'runs after a directory change the hook cannot resolve. Run cd with a named directory first';
const _BREAK = /\n|(?<!\\)(?:\\\\)*\\n/u;
const _STACK = /^(?:-|[+-]\d+)$/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [PLACES]

const _located = (commands: readonly Command[], cwd: string, home: Option<string>): Located['commands'] =>
    commands.reduce<{ readonly cwd: Option<string>; readonly held: Located['commands'] }>(
        (state, command) => {
            const held: Located['commands'] = [...state.held, [command, state.cwd]];
            const moved = command.invocations.find(([program]) => declared(program).writes?.includes('PWD') === true);
            if (moved === undefined) {
                return { cwd: state.cwd, held };
            }
            const [target = declared(moved[0]).defaultOperand] = operands(moved).inputs;
            return { cwd: target === undefined || _STACK.test(target) ? none : placed(target, state.cwd, home), held };
        },
        { cwd: some(`/${segments(cwd).join('/')}`), held: [] },
    ).held;

// --- [DELETE]

const _rooted = async (host: Host, { commands, home }: Located, root: string): Promise<readonly string[]> => {
    const held = commands.flatMap(([{ source, words, spans }, cwd]) =>
        words.flatMap((word, at) => {
            const place = placed(word, cwd, home);
            return (place.kind === 'some' ? within(root, place.value) : !word.startsWith('-')) ? spans.slice(at, at + 1).map(({ start, end }) => ({ source, word, start, end, place })) : [];
        }),
    );
    const reasons = await Promise.all(
        held.map(async ({ source, word, start, end, place }) => {
            const bytes = new TextEncoder().encode(source);
            const decoder = new TextDecoder();
            const rescanned = await parse(host.scan, `${decoder.decode(bytes.subarray(0, start))}*${decoder.decode(bytes.subarray(end))}`);
            return rescanned.kind === 'fault' ? [`Command not parsed, ${rendered(rescanned.faults)}`] : rescanned.value.deletes.map(({ program }) => (place.kind === 'some' ? `${program} deletes ${word}, at or above repository root` : `${program} ${_UNPLACED}`));
        }),
    );
    return reasons.flat();
};

const _delete = async (host: Host, { deletes }: Script, project: string, located: Located): Promise<readonly string[]> => {
    if (deletes.length > 0) {
        return [...deletes.map(({ message }) => message), `Name each path literally, or run nx run ${project}:clean`];
    }
    const rooted = located.root.kind === 'none' ? [] : await _rooted(host, located, located.root.value);
    return rooted.length === 0 ? [] : [...rooted, `Name paths beneath repository root, or run nx run ${project}:clean`];
};

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
    return first !== undefined && first !== '-' && existing.includes(first) ? [`git checkout ${first} overwrites existing path ${first}. Edit the file`] : [];
};

const _GIT: ReadonlyMap<string, GitRow> = new Map(
    Object.entries({
        branch: { reason: 'deletes or force-moves a branch', words: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
        checkout: { reason: 'discards local changes', words: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], prefixes: ['--force'], refine: _checkout },
        clean: { reason: 'deletes untracked files. Remove named files with rm', any: true },
        config: { reason: 'defines an alias that can hide a refused subcommand', refine: _config },
        push: { reason: 'rewrites or deletes remote history', words: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
        rebase: { reason: 'rewrites commits other agents can hold', any: true },
        reflog: { reason: 'erases reflog entries', words: ['delete', 'drop', 'expire'] },
        reset: { reason: 'discards working-tree or index state', words: ['--hard', '--merge', '--keep'], refine: _reset },
        restore: { reason: 'overwrites working-tree files. Edit the files', refine: _restore },
        revert: { reason: 'reverses committed history', any: true },
        stash: { reason: 'hides uncommitted work other agents depend on. Commit to a branch', any: true, safe: ['list', 'show'] },
        switch: { reason: 'discards local changes', words: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
        worktree: { reason: WORKTREE, any: true, safe: ['list'] },
    } satisfies Record<string, GitRow>),
);

const _calls = ({ commands, home, root }: Located): readonly GitCall[] =>
    commands.flatMap(([command, cwd]) =>
        command.invocations
            .filter(([program]) => program === 'git')
            .flatMap((invocation): readonly GitCall[] => {
                const index = firstOperand(invocation, 1);
                const [key, ...args] = invocation.slice(index);
                const moved = operands([invocation[0], ...invocation.slice(1, index)]).values.reduce((folder, [name, value]) => (name === '-C' ? placed(value, folder, home) : folder), cwd);
                if (root.kind === 'none' || (moved.kind === 'some' && !within(moved.value, root.value))) {
                    return [];
                }
                if (invocation.slice(1, index).some((word) => word.startsWith('alias.'))) {
                    return [{ kind: 'aliased' }];
                }
                const row = key === undefined ? undefined : _GIT.get(key);
                return key === undefined || row === undefined ? [] : [moved.kind === 'some' ? { kind: 'subcommand', key, row, args, cwd: moved.value } : { kind: 'unplaced', key }];
            }),
    );

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

const _git = async (host: Host, located: Located): Promise<readonly string[]> =>
    (
        await Promise.all(
            _calls(located).map(async (call) => {
                if (call.kind === 'aliased') {
                    return ['Inline git alias can hide a refused subcommand'];
                }
                if (call.kind === 'unplaced') {
                    return [`git ${call.key} ${_UNPLACED}`];
                }
                const named = call.row.refine === undefined ? [] : call.args.filter((word) => !word.startsWith('-'));
                const existing = await Promise.all(
                    named.map(async (word) => {
                        const place = placed(word, some(call.cwd), located.home);
                        return place.kind === 'some' && (await host.exists(place.value)) ? [word] : [];
                    }),
                );
                return _refusals(call, existing.flat());
            }),
        )
    ).flat();

// --- [STDIN]

const _stdin = (commands: readonly Command[]): readonly string[] => commands.flatMap((command) => (!command.fed && command.invocations.some(waits) ? [`${command.words.join(' ')} waits on standard input nothing feeds. Pass a file or pipe input`] : []));

// --- [INLINE]

const _inline = (commands: readonly Command[]): readonly string[] =>
    commands.flatMap((command) =>
        command.invocations.flatMap((invocation) => {
            const [program] = invocation;
            const { code, shell } = declared(program);
            return code !== undefined && shell !== true && (operands(invocation).options.some((name) => code.includes(name)) || (command.fed && waits(invocation))) ? [`${program} runs an inline program. Edit files with Edit or sd, run installed tools by name`] : [];
        }),
    );

// --- [SEQUENCE]

const _sequence = ({ sequences }: Script): readonly string[] =>
    sequences.flatMap((steps) => {
        const loose = steps.filter(({ writes }, at) => !steps.slice(at + 1).some(({ reads }) => writes.some((name) => reads.includes(name))));
        return loose.length > 1 ? [`${loose.map(({ programs }) => programs.join(' | ')).join(', ')} pass no shell data between them. Send each as its own Bash call, batched when independent`] : [];
    });

// --- [REWRITE]

const _writers = (command: Command, project: string): readonly Change[] =>
    command.invocations.flatMap((invocation, index) => {
        const at = offset(command, index);
        const { inputs, options, positions } = operands(invocation);
        const start = command.spans[at];
        const scan = command.spans[at + 1];
        const update = (name: string): boolean => name === '-U' || name === '--update-all';
        if (!((invocation[0] === 'ast-grep' || invocation[0] === 'sg') && inputs[0] === 'scan' && options.some(update) && !options.includes('--stdin')) || start === undefined || scan === undefined) {
            return [];
        }
        const flags = positions
            .filter(({ names }) => names.some(update))
            .flatMap(({ at: position, names }) => {
                const before = command.spans[at + position - 1];
                const span = command.spans[at + position];
                const kept = names.filter((name) => !update(name));
                return span === undefined || before === undefined ? [] : [kept.length === 0 ? { start: before.end, end: span.end, text: '' } : { start: span.start, end: span.end, text: `-${kept.map((name) => name.slice(1)).join('')}` }];
            });
        return [{ splices: [{ start: start.start, end: scan.end, text: `nx run ${project}:rewrite --` }, ...flags], notice: some(`${invocation[0]} scan -U ran as nx run ${project}:rewrite`) }];
    });

const _hidden = (command: Command): readonly Change[] =>
    command.invocations.flatMap((invocation, index) => {
        const { positions } = operands(invocation);
        const named = command.spans[offset(command, index)];
        const run = positions.find(({ names }) => names.some((name) => declared(invocation[0]).execs?.includes(name) === true))?.at ?? invocation.length;
        const chosen = positions.some(({ at, names }) => at < run && names.some((name) => ['-H', '--hidden', '--no-hidden', '-u', '--unrestricted', '-h', '--help', '-V', '--version'].includes(name)));
        return invocation[0] !== 'fd' || named === undefined || chosen ? [] : [{ splices: [{ start: named.end, end: named.end, text: ' --hidden' }], notice: none }];
    });

const _refurb = (command: Command): readonly Change[] =>
    command.invocations.flatMap((invocation, index) => {
        const named = command.spans[offset(command, index)];
        return invocation[0] !== 'refurb' || named === undefined ? [] : [{ splices: [{ start: named.start, end: named.end, text: 'python -m tools.refurb' }], notice: some('refurb ran as python -m tools.refurb over the mypy cache') }];
    });

const _across = (command: Command): readonly Change[] =>
    command.invocations.slice(-1).flatMap((invocation) => {
        const [program] = invocation;
        const { options } = operands(invocation);
        const [first, second] = invocation.slice(firstOperand(invocation, 1));
        const find = first === '--' ? second : first;
        const named = command.spans[offset(command, command.invocations.length - 1)];
        const breaks = find !== undefined && (options.some((name) => name === '-F' || name === '--fixed-strings') ? find.includes('\n') : _BREAK.test(find));
        return program !== 'sd' || named === undefined || !breaks || informational(program, options) || options.some((name) => name === '-A' || name === '--across') ? [] : [{ splices: [{ start: named.end, end: named.end, text: ' -A' }], notice: none }];
    });

const _relaunched = (launcher: Invocation, invocation: Invocation): { readonly words: readonly string[]; readonly set: readonly string[] } => {
    const groups = (words: Invocation): readonly { readonly names: readonly string[]; readonly words: readonly string[]; readonly given: string; readonly end: number }[] =>
        operands(words).positions.map(({ at, names, taken }) => {
            const { options, values } = operands([words[0], ...words.slice(at, at + 1 + taken)]);
            return { names, words: words.slice(at, at + 1 + taken), given: JSON.stringify([options, values]), end: at + 1 + taken };
        });
    const fixed = groups(launcher);
    const called = groups(invocation);
    const kept = called.filter(({ names }) => !names.some((name) => fixed.some((held) => held.names.includes(name)))).flatMap(({ words }) => words);
    const end = Math.max(1, ...fixed.map((group) => group.end));
    return { words: [...launcher.slice(0, end), ...kept, ...launcher.slice(end)], set: fixed.filter(({ given }) => !called.some((held) => held.given === given)).flatMap(({ words }) => words) };
};

const _rewrite = (text: string, { commands }: Located, project: string, routed: readonly Routed[]): Option<Rewrite> => {
    const bytes = new TextEncoder().encode(text);
    const decoder = new TextDecoder();
    const replaced = (command: Command, from: number, to: number, inserted: string): readonly Splice[] => {
        const first = command.spans[offset(command, from)];
        const last = command.spans[offset(command, to) - 1];
        return first === undefined || last === undefined ? [] : [{ start: first.start, end: last.end, text: inserted }];
    };
    const routes = routed.flatMap(({ command, position, routes: held, launchers }): readonly Change[] => {
        const at = offset(command, position);
        const wrapped = command.invocations.slice(0, position);
        const absorbed = wrapped.findLastIndex(([program]) => !launchers.some(([owned]) => owned === program)) + 1;
        const relaunched = wrapped.flatMap((invocation, index) =>
            launchers.flatMap((launcher) => {
                const { words, set } = launcher[0] === invocation[0] ? _relaunched(launcher, invocation) : { words: invocation, set: [] };
                return set.length === 0 ? [] : [{ splices: replaced(command, index, index + 1, words.map(quoted).join(' ')), notice: some(`${invocation[0]} took ${set.join(' ')} from its Nx targets`) }];
            }),
        );
        return held.length === 0
            ? relaunched
            : held.map(({ form, forwarded, notice }) => {
                  const passed = forwarded.flatMap((index) => command.spans.slice(at + index, at + index + 1).map(({ start, end }) => decoder.decode(bytes.subarray(start, end))));
                  return { splices: replaced(command, absorbed, position + 1, [form, ...(passed.length === 0 ? [] : ['--', ...passed])].join(' ')), notice: some(notice) };
              });
    });
    const outside = ({ splices }: Change): boolean => splices.every(({ start, end }) => !routes.some(({ splices: claimed }) => claimed.some((span) => start >= span.start && end <= span.end)));
    const changes = [...routes, ...commands.flatMap(([command]) => (command.nested ? [] : [..._writers(command, project), ..._hidden(command), ..._refurb(command), ..._across(command)])).filter(outside)];
    const spliced = changes
        .flatMap(({ splices }) => splices)
        .toSorted((left, right) => left.start - right.start)
        .reduce<{ readonly at: number; readonly pieces: readonly string[] }>((head, { start, end, text: inserted }) => ({ at: end, pieces: [...head.pieces, decoder.decode(bytes.subarray(head.at, start)), inserted] }), { at: 0, pieces: [] });
    const notices = [...new Set(changes.flatMap(({ notice }) => (notice.kind === 'some' ? [notice.value] : [])))];
    return changes.length === 0 ? none : some({ command: [...spliced.pieces, decoder.decode(bytes.subarray(spliced.at))].join(''), notice: notices.length === 0 ? none : some(notices.join('. ')) });
};

// --- [DECISION]

const _routed = async (host: Host, owned: Catalog, { commands }: Located): Promise<readonly Routed[]> => {
    const calls = commands.flatMap(([command, cwd]) => (command.nested ? [] : command.invocations.map((invocation, position) => ({ command, invocation, cwd, position }))));
    const paths = [...new Set(calls.flatMap(({ invocation, cwd }) => (cwd.kind === 'some' ? solutions(invocation, cwd.value) : [])))];
    const read = await Promise.all(paths.map(async (path) => [path, await host.read(path)] as const));
    const files = new Map(read.flatMap(([path, held]) => (held.kind === 'some' ? [[path, held.value] as const] : [])));
    return calls.map(({ command, invocation, cwd, position }) => {
        const program = basename(invocation[0]);
        return { command, position, ...(cwd.kind === 'some' ? route(owned, invocation, cwd.value, files) : { routes: [], denials: owned.tools.has(program) ? [`${program} ${_UNPLACED}`] : [], launchers: [] }) };
    });
};

const commandDecision = async (host: Host, command: string, project: string): Promise<Decision> => {
    const parsed = await parse(host.scan, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `Command not parsed, ${rendered(parsed.faults)}` };
    }
    const [owned, root, cwd, home] = await Promise.all([host.catalog(), host.root(), host.cwd(), host.home()]);
    const located: Located = { root, home, commands: _located(parsed.value.commands, cwd, home) };
    const routed = owned.kind === 'some' ? await _routed(host, owned.value, located) : [];
    const reasons = [...(await _delete(host, parsed.value, project, located)), ...(await _git(host, located)), ..._stdin(parsed.value.commands), ..._inline(parsed.value.commands), ..._sequence(parsed.value), ...routed.flatMap(({ denials }) => denials)];
    return reasons.length === 0 ? { kind: 'allow', rewrite: _rewrite(command, located, project, routed) } : { kind: 'deny', reason: [...new Set(reasons)].join('. ') };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { Decision, Host };
export { commandDecision, WORKTREE };
