import { type Fault, none, type Option, some } from '../composition.ts';
import { type Command, parse, type Scanner, type Script, type Span, scanner } from './command.ts';
import { declared, firstOperand, type Invocation, informational, operands, placed, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Located = readonly (readonly [command: Command, cwd: Option<string>])[];
type Decision = { readonly kind: 'deny'; readonly reason: string } | { readonly kind: 'allow' } | { readonly kind: 'rewrite'; readonly command: string; readonly notice: Option<string> };

interface System {
    readonly run: (argv: Invocation, init: { readonly cwd: string; readonly stdin: string }) => Promise<{ readonly exitCode: number; readonly stdout: string; readonly stderr: string }>;
    readonly exists: (path: string) => Promise<boolean>;
    readonly root: Option<string>;
    readonly cwd: string;
    readonly home: Option<string>;
}
interface Call {
    readonly system: System;
    readonly scan: Scanner;
    readonly script: Script;
    readonly located: Located;
    readonly project: string;
}
interface GitRow {
    readonly reason: string;
    readonly allows?: readonly string[];
    readonly words?: readonly string[];
    readonly prefixes?: readonly string[];
    readonly refine?: (args: readonly string[], existing: readonly string[], reason: string) => readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates a second checkout. Work in the main checkout';
const _UNPLACED = 'runs after a directory change the hook cannot resolve. Run cd with a named directory first';
const _STACK = /^(?:-|[+-]\d+)$/u;
const _BREAK = /\n|(?<!\\)(?:\\\\)*\\n/u;
const _GIT: ReadonlyMap<string, GitRow> = new Map(
    Object.entries({
        branch: { reason: 'deletes or force-moves a branch', words: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
        checkout: {
            reason: 'discards local changes',
            words: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'],
            prefixes: ['--force'],
            refine: (args, existing) => {
                const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
                const separated = args.includes('--');
                if (!separated && args.some((word) => ['-b', '--orphan', '-t', '--track', '--detach'].includes(word))) {
                    return [];
                }
                if (separated || more.length > 0 || first === '.' || first?.startsWith(':') === true) {
                    return ['git checkout with a pathspec overwrites working-tree files. Edit the files'];
                }
                return first !== undefined && first !== '-' && existing.includes(first) ? [`git checkout ${first} overwrites existing path ${first}. Edit the file`] : [];
            },
        },
        clean: { reason: 'deletes untracked files. Remove named files with rm', allows: [] },
        config: {
            reason: 'defines an alias that can hide a refused subcommand',
            refine: (args, _existing, reason) => {
                const alias = args.find((word) => word.startsWith('alias.'));
                return alias !== undefined && args.slice(args.indexOf(alias) + 1).some((word) => !word.startsWith('-')) ? [`git config ${alias} ${reason}`] : [];
            },
        },
        push: { reason: 'rewrites or deletes remote history', words: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
        rebase: { reason: 'rewrites commits other agents can hold', allows: [] },
        reflog: { reason: 'erases reflog entries', words: ['delete', 'drop', 'expire'] },
        reset: {
            reason: 'discards working-tree or index state',
            words: ['--hard', '--merge', '--keep'],
            refine: (args, existing) => {
                const targets = args.filter((word) => !word.startsWith('-'));
                const [target] = targets;
                return target === undefined || args.includes('--') || targets.some((named) => existing.includes(named)) ? [] : [`git reset ${target} moves HEAD and drops commits from the branch`];
            },
        },
        restore: {
            reason: 'overwrites working-tree files. Edit the files',
            refine: (args, _existing, reason) => {
                const { options } = operands(['git', ...args]);
                return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [`git restore ${reason}`];
            },
        },
        revert: { reason: 'reverses committed history', allows: [] },
        stash: { reason: 'hides uncommitted work other agents depend on. Commit to a branch', allows: ['list', 'show'] },
        switch: { reason: 'discards local changes', words: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
        worktree: { reason: WORKTREE, allows: ['list'] },
    } satisfies Record<string, GitRow>),
);

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [TEXT]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const _unparsed = (faults: readonly Fault[]): string =>
    `Command not parsed. ${faults
        .map((value) => {
            switch (value.kind) {
                case 'exited':
                    return [`${value.subject} exited ${value.code}`, value.output].filter((part) => part !== '').join(', ');
                case 'unstarted':
                    return `${value.subject} did not run, ${String(value.cause)}`;
                default:
                    return `${value.subject} printed no JSON, ${String(value.cause)}`;
            }
        })
        .join('. ')}`;

const _spliced = (source: string, splices: readonly (Span & { readonly text: string })[]): string => {
    const bytes = new TextEncoder().encode(source);
    const decoder = new TextDecoder();
    const sorted = splices.toSorted((left, right) => left.start - right.start);
    return [...sorted.map(({ start, text }, index) => `${decoder.decode(bytes.subarray(sorted[index - 1]?.end, start))}${text}`), decoder.decode(bytes.subarray(sorted.at(-1)?.end))].join('');
};

// --- [DELETE]

const _delete = async ({ scan, system: { root, home }, script: { deletes }, located, project }: Call): Promise<readonly string[]> => {
    if (deletes.length > 0) {
        return [...deletes.map(({ message }) => message), `Name each path literally, or run nx run ${project}:clean`];
    }
    if (root.kind === 'none') {
        return [];
    }
    const held = located.flatMap(([{ source, words }, cwd]) =>
        words.flatMap(({ text, span }) => {
            const place = placed(text, cwd, home);
            return (place.kind === 'some' ? within(root.value, place.value) : !text.startsWith('-')) ? [{ source, text, span, place }] : [];
        }),
    );
    const rooted = (
        await Promise.all(
            held.map(async ({ source, text, span, place }) => {
                const rescanned = await parse(scan, _spliced(source, [{ ...span, text: '*' }]), { fed: false, nested: false });
                return rescanned.kind === 'fault' ? [_unparsed(rescanned.faults)] : rescanned.value.deletes.map(({ program }) => (place.kind === 'some' ? `${program} deletes ${text}, at or above repository root` : `${program} ${_UNPLACED}`));
            }),
        )
    ).flat();
    return rooted.length === 0 ? [] : [...rooted, `Name paths beneath repository root, or run nx run ${project}:clean`];
};

// --- [GIT]

const _git = async ({ system: { exists, root, home }, located }: Call): Promise<readonly string[]> => {
    if (root.kind === 'none') {
        return [];
    }
    const calls = located
        .flatMap(([command, cwd]) => command.invocations.filter(([program]) => program === 'git').map((invocation) => ({ invocation, cwd })))
        .map(({ invocation, cwd }) => {
            const index = firstOperand(invocation, 1);
            const { values } = operands(['git', ...invocation.slice(1, index)]);
            return { values, words: invocation.slice(index), moved: values.reduce((folder, [name, value]) => (name === '-C' ? placed(value, folder, home) : folder), cwd) };
        })
        .filter(({ moved }) => moved.kind === 'none' || within(moved.value, root.value));
    const refused = await Promise.all(
        calls.map(async ({ values, words: [key, ...args], moved }) => {
            const row = key === undefined ? undefined : _GIT.get(key);
            if (values.some(([, value]) => value.startsWith('alias.'))) {
                return ['Inline git alias can hide a refused subcommand'];
            }
            if (row === undefined || row.allows?.some((word) => word === args[0]) === true) {
                return [];
            }
            if (moved.kind === 'none') {
                return [`git ${key} ${_UNPLACED}`];
            }
            const hit = args.find((word) => row.words?.includes(word) === true || row.prefixes?.some((prefix) => word.startsWith(prefix)) === true);
            if (row.allows !== undefined || hit !== undefined) {
                return [['git', key, hit, row.reason].filter((part) => part !== undefined).join(' ')];
            }
            if (row.refine === undefined) {
                return [];
            }
            const existing = await Promise.all(
                args
                    .filter((word) => !word.startsWith('-'))
                    .map(async (word) => {
                        const place = placed(word, moved, home);
                        return place.kind === 'some' && (await exists(place.value)) ? [word] : [];
                    }),
            );
            return row.refine(args, existing.flat(), row.reason);
        }),
    );
    return refused.flat();
};

// --- [STDIN]

const _stdin = ({ script: { commands } }: Call): readonly string[] => commands.flatMap((command) => (!command.fed && command.invocations.some(waits) ? [`${command.words.map(({ text }) => text).join(' ')} waits on standard input nothing feeds. Pass a file or pipe input`] : []));

// --- [INLINE]

const _inline = ({ script: { commands } }: Call): readonly string[] =>
    commands.flatMap((command) =>
        command.invocations.flatMap((invocation) => {
            const [program] = invocation;
            const { code } = declared(program);
            return code !== undefined && (operands(invocation).options.some((name) => code.includes(name)) || (command.fed && waits(invocation))) ? [`${program} runs an inline program. Edit files with Edit or sd, run installed tools by name`] : [];
        }),
    );

// --- [SEQUENCE]

const _sequence = ({ script: { sequences } }: Call): readonly string[] =>
    sequences.flatMap((steps) => {
        const loose = steps.filter(
            ({ writes }, at) =>
                !steps
                    .slice(at + 1)
                    .flatMap(({ reads }) => reads)
                    .some((name) => writes.includes(name)),
        );
        return loose.length > 1 ? [`${loose.map(({ programs }) => programs.join(' | ')).join(', ')} pass no shell data between them. Send each as its own Bash call, batched when independent`] : [];
    });

// --- [REWRITE]

const _hidden = (invocation: Invocation, { end }: Span) => {
    const { options } = operands(invocation);
    return invocation[0] !== 'fd' || informational('fd', options) || options.some((name) => ['-H', '--hidden', '--no-hidden', '-u', '--unrestricted'].includes(name)) ? [] : [{ start: end, end, text: ' --hidden', notice: none }];
};

const _refurb = (invocation: Invocation, span: Span) => {
    const text = 'python -m tools.refurb';
    return invocation[0] === 'refurb' ? [{ ...span, text, notice: some(`refurb ran as ${text} over the mypy cache`) }] : [];
};

const _across = (invocation: Invocation, { end }: Span) => {
    const [program] = invocation;
    const { options } = operands(invocation);
    const [first, second] = invocation.slice(firstOperand(invocation, 1));
    const find = first === '--' ? second : first;
    const breaks = find !== undefined && (options.some((name) => name === '-F' || name === '--fixed-strings') ? find.includes('\n') : _BREAK.test(find));
    return program !== 'sd' || !breaks || informational(program, options) || options.some((name) => name === '-A' || name === '--across') ? [] : [{ start: end, end, text: ' -A', notice: none }];
};

// --- [DECISION]

const commandDecision = async (system: System, command: string, project: string): Promise<Decision> => {
    const scan = await scanner(system);
    const parsed = await parse(scan, command, { fed: false, nested: false });
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: _unparsed(parsed.faults) };
    }
    const script = parsed.value;
    const locate = ([head, ...rest]: readonly Command[], cwd: Option<string>): Located => {
        if (head === undefined) {
            return [];
        }
        const moved = head.invocations.find(([program]) => declared(program).writes?.includes('PWD') === true);
        if (moved === undefined) {
            return [[head, cwd], ...locate(rest, cwd)];
        }
        const [target = declared(moved[0]).defaultOperand] = operands(moved).inputs;
        return [[head, cwd], ...locate(rest, target === undefined || _STACK.test(target) ? none : placed(target, cwd, system.home))];
    };
    const call = { system, scan, script, located: locate(script.commands, placed(system.cwd, none, none)), project };
    const reasons = new Set((await Promise.all([_delete, _git, _stdin, _inline, _sequence].map((policy) => policy(call)))).flat());
    if (reasons.size > 0) {
        return { kind: 'deny', reason: [...reasons].join('. ') };
    }
    const changes = script.commands.flatMap(({ nested, words, invocations }) =>
        invocations.slice(-1).flatMap((last) => {
            const program = words.at(-last.length);
            return nested || program === undefined ? [] : [_hidden, _refurb, _across].flatMap((rewrite) => rewrite(last, program.span));
        }),
    );
    if (changes.length === 0) {
        return { kind: 'allow' };
    }
    const notices = [...new Set(changes.flatMap(({ notice }) => (notice.kind === 'some' ? [notice.value] : [])))];
    return { kind: 'rewrite', command: _spliced(command, changes), notice: notices.length === 0 ? none : some(notices.join('. ')) };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { System };
export { commandDecision, unrun, WORKTREE };
