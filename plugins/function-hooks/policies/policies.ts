import { none, type Option, some } from '../composition.ts';
import { parse, type Script } from './command.ts';
import { declared, firstOperand, type Invocation, informational, invocations, operands, placed, waits, within } from './invocation.ts';

// --- [TYPES] ---------------------------------------------------------------------------

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
    readonly script: Script;
    readonly project: string;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates a second checkout. Work in the main checkout';
const UNPLACED = 'runs after a directory change the hook cannot resolve. Run cd with a named directory first';
const ALIAS = 'defines an alias that can hide a refused subcommand';
const OVERWRITES = 'overwrites working-tree files. Edit the files';
const BREAK = /(?<!\\)(?:\\\\)*\\n/u;

// --- [OPERATIONS] ----------------------------------------------------------------------

// --- [DELETE]

const deletes = ({ system: { root, home }, script: { commands }, project }: Call): readonly string[] => {
    const removing = (invocation: Invocation): readonly Invocation[] => {
        if (invocation[0] !== 'git') {
            return ['rm', 'rimraf', 'trash', 'unlink', 'shred'].includes(invocation[0]) ? [invocation] : [];
        }
        const [key, ...args] = invocation.slice(firstOperand(invocation, 1));
        return key === 'rm' && !args.includes('--cached') ? [['rm', ...args]] : [];
    };
    const runtime = commands.flatMap(({ words, invocations: chain }) =>
        chain.flatMap((invocation, at) => {
            const [program] = invocation;
            const { options, inner } = operands(invocation);
            const [loose] = at === chain.length - 1 ? words.slice(words.length - invocation.length + 1).filter(({ unbounded }) => unbounded) : [];
            const sources = [
                ...invocations(inner)
                    .flatMap(removing)
                    .map(([name]) => [name, program] as const),
                ...(options.includes('-delete') ? [[program, '-delete'] as const] : []),
                ...(declared(program).stdin === 'always' ? chain.slice(at + 1).flatMap(removing) : []).map(([name]) => [name, program] as const),
                ...(loose === undefined ? [] : removing(invocation).map(([name]) => [name, loose.text] as const)),
            ];
            return sources.map(([name, source]) => `${name} deletes paths known at run time (${source})`);
        }),
    );
    const rooted =
        root.kind === 'none'
            ? []
            : commands.flatMap(({ invocations: chain, cwd }) =>
                  chain
                      .slice(-1)
                      .flatMap(removing)
                      .flatMap((target) =>
                          operands(target).inputs.flatMap((path) => {
                              const place = placed(path, cwd, home);
                              if (place.kind === 'none') {
                                  return [`${target[0]} ${UNPLACED}`];
                              }
                              return within(root.value, place.value) ? [`${target[0]} deletes ${path}, at or above repository root`] : [];
                          }),
                      ),
              );
    if (runtime.length > 0) {
        return [...runtime, `Name each path literally, or run nx run ${project}:clean`];
    }
    return rooted.length > 0 ? [...rooted, `Name paths beneath repository root, or run nx run ${project}:clean`] : [];
};

// --- [GIT]

const flagged = (args: readonly string[], reason: string, words: readonly string[], prefixes: readonly string[]): readonly string[] =>
    args
        .filter((word) => words.includes(word) || prefixes.some((prefix) => word.startsWith(prefix)))
        .slice(0, 1)
        .map((word) => `${word} ${reason}`);

const GIT = new Map(
    Object.entries<(args: readonly string[], present: (word: string) => boolean) => readonly string[]>({
        branch: (args) => flagged(args, 'deletes or force-moves a branch', ['-d', '-D', '-M', '--delete'], ['--force']),
        checkout: (args, present) => {
            const forced = flagged(args, 'discards local changes', ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], ['--force']);
            const [first, ...more] = args.filter((word) => word === '-' || !word.startsWith('-'));
            const separated = args.includes('--');
            const branching = !separated && args.some((word) => ['-b', '--orphan', '-t', '--track', '--detach'].includes(word));
            const paths = separated || more.length > 0 || first === '.' || first?.startsWith(':') === true || (first !== undefined && first !== '-' && present(first));
            return forced.length > 0 || branching || !paths ? forced : [`${args.join(' ')} ${OVERWRITES}`];
        },
        clean: () => ['deletes untracked files. Remove named files with rm'],
        config: (args) => args.flatMap((word, at) => (word.startsWith('alias.') && args.slice(at + 1).some((value) => !value.startsWith('-')) ? [`${word} ${ALIAS}`] : [])).slice(0, 1),
        push: (args) => flagged(args, 'rewrites or deletes remote history', ['-f', '-d', '--delete', '--mirror', '--prune'], ['--force', '+', ':']),
        rebase: () => ['rewrites commits other agents can hold'],
        reflog: (args) => flagged(args, 'erases reflog entries', ['delete', 'drop', 'expire'], []),
        reset: (args, present) => {
            const forced = flagged(args, 'discards working-tree or index state', ['--hard', '--merge', '--keep'], []);
            const targets = args.filter((word) => !word.startsWith('-'));
            const [target] = targets;
            return forced.length > 0 || target === undefined || args.includes('--') || targets.some(present) ? forced : [`${target} moves HEAD and drops commits from the branch`];
        },
        restore: (args) => {
            const { options } = operands(['git', ...args]);
            return options.some((name) => name === '-S' || name === '--staged') && !options.some((name) => name === '-W' || name === '--worktree') ? [] : [OVERWRITES];
        },
        revert: () => ['reverses committed history'],
        stash: ([first]) => (first === 'list' || first === 'show' ? [] : ['hides uncommitted work other agents depend on. Commit to a branch']),
        switch: (args) => flagged(args, 'discards local changes', ['-f', '-C', '--discard-changes'], ['--force']),
        worktree: ([first]) => (first === 'list' ? [] : [WORKTREE]),
    }),
);

const git = async ({ system: { exists, root, home }, script: { commands } }: Call): Promise<readonly string[]> => {
    if (root.kind === 'none') {
        return [];
    }
    const calls = commands.flatMap(({ invocations: chain, cwd }) =>
        chain
            .filter(([program]) => program === 'git')
            .flatMap((invocation) => {
                const index = firstOperand(invocation, 1);
                const { values } = operands(['git', ...invocation.slice(1, index)]);
                const moved = values.reduce((folder, [name, value]) => (name === '-C' ? placed(value, folder, home) : folder), cwd);
                const [key, ...args] = invocation.slice(index);
                return key === undefined || (moved.kind === 'some' && !within(moved.value, root.value)) ? [] : [{ values, key, args, moved }];
            }),
    );
    const probed = calls.flatMap(({ args, moved }) => args.filter((word) => !word.startsWith('-')).map((word) => placed(word, moved, home))).flatMap((place) => (place.kind === 'some' ? [place.value] : []));
    const found = new Set((await Promise.all(probed.map(async (path) => ((await exists(path)) ? [path] : [])))).flat());
    return calls.flatMap(({ values, key, args, moved }) => {
        const aliased = values.flatMap(([name, value]) => (value.startsWith('alias.') ? [`git ${name} ${value} ${ALIAS}`] : []));
        const rule = GIT.get(key);
        const judged =
            rule === undefined
                ? []
                : [
                      ...rule(args, (word) => {
                          const place = placed(word, moved, home);
                          return place.kind === 'some' && found.has(place.value);
                      }),
                      ...(moved.kind === 'none' ? rule(args, () => true) : []),
                  ];
        if (aliased.length > 0) {
            return aliased;
        }
        if (moved.kind === 'none') {
            return judged.length > 0 ? [`git ${key} ${UNPLACED}`] : [];
        }
        return judged.map((tail) => `git ${key} ${tail}`);
    });
};

// --- [STDIN]

const stdin = ({ script: { commands } }: Call): readonly string[] => commands.flatMap(({ fed, words, invocations: chain }) => (!fed && chain.some(waits) ? [`${words.map(({ text }) => text).join(' ')} waits on standard input nothing feeds. Pass a file or pipe input`] : []));

// --- [INLINE]

const inline = ({ script: { commands } }: Call): readonly string[] =>
    commands.flatMap(({ fed, invocations: chain }) =>
        chain.flatMap((invocation) => {
            const [program] = invocation;
            const { code } = declared(program);
            return code !== undefined && (operands(invocation).options.some((name) => code.includes(name)) || (fed && waits(invocation))) ? [`${program} runs an inline program. Edit files with Edit or sd, run installed tools by name`] : [];
        }),
    );

// --- [SEQUENCE]

const sequence = ({ script: { sequences } }: Call): readonly string[] =>
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

// --- [DECISION]

const unrun = (detail: string): string => `function-hooks policy did not run. ${detail}`;

const commandDecision = async (system: System, command: string, project: string): Promise<Decision> => {
    const parsed = await parse(system, command);
    if (parsed.kind === 'fault') {
        return { kind: 'deny', reason: `Command not parsed. ${parsed.faults.map((value) => (value.kind === 'exited' ? [`${value.program} exited ${value.code}`, value.output].filter((part) => part !== '').join(', ') : `${value.program} failed, ${String(value.cause)}`)).join('. ')}` };
    }
    const call = { system, script: parsed.value, project };
    const reasons = new Set((await Promise.all([deletes, git, stdin, inline, sequence].map((policy) => policy(call)))).flat());
    if (reasons.size > 0) {
        return { kind: 'deny', reason: [...reasons].join('. ') };
    }
    const changes = parsed.value.commands
        .filter(({ nested }) => !nested)
        .flatMap(({ words, invocations: chain }) => chain.slice(-1).flatMap((last) => words.slice(words.length - last.length, words.length - last.length + 1).map(({ span }) => ({ last, span }))))
        .flatMap(({ last, span: { start, end } }) => {
            const [program] = last;
            const { options, leading } = operands(last);
            const given = (names: readonly string[]): boolean => informational(program, options) || names.some((name) => options.includes(name));
            switch (program) {
                case 'fd':
                    return given(['-H', '--hidden', '--no-hidden', '-u', '--unrestricted']) ? [] : [{ start: end, end, text: ' --hidden', notice: none }];
                case 'sd': {
                    const [find] = leading;
                    const breaks = find !== undefined && (find.includes('\n') || (!given(['-F', '--fixed-strings']) && BREAK.test(find)));
                    return breaks && !given(['-A', '--across']) ? [{ start: end, end, text: ' -A', notice: none }] : [];
                }
                case 'refurb': {
                    const text = 'python -m tools.refurb';
                    return [{ start, end, text, notice: some(`refurb ran as ${text} over the mypy cache`) }];
                }
                default:
                    return [];
            }
        });
    if (changes.length === 0) {
        return { kind: 'allow' };
    }
    const bytes = new TextEncoder().encode(command);
    const decoder = new TextDecoder();
    const sorted = changes.toSorted((left, right) => left.start - right.start);
    const notices = [...new Set(changes.flatMap(({ notice }) => (notice.kind === 'some' ? [notice.value] : [])))];
    return {
        kind: 'rewrite',
        command: [...sorted.map(({ start, text }, index) => `${decoder.decode(bytes.subarray(sorted[index - 1]?.end, start))}${text}`), decoder.decode(bytes.subarray(sorted.at(-1)?.end))].join(''),
        notice: notices.length === 0 ? none : some(notices.join('. ')),
    };
};

// --- [EXPORTS] -------------------------------------------------------------------------

export type { System };
export { commandDecision, unrun, WORKTREE };
