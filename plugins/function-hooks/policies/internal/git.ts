import { firstOperand, isInside, operands, resolvePath } from './invocation.ts';
import { type Call, invocationSites, refuse, UNRESOLVED_FOLDER, type Verdict } from './verdict.ts';

// --- [CONSTANTS] -----------------------------------------------------------------------

const WORKTREE = 'creates a second checkout, work in the main checkout';
const BRANCH = 'creates a branch, stay on the current branch';
const ALIAS = 'defines an alias hiding refused subcommands, run the subcommands directly';
const OVERWRITES = 'overwrites edits, use the Edit tool';

// --- [OPERATIONS] ----------------------------------------------------------------------

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

// --- [EXPORTS] -------------------------------------------------------------------------

export { git, WORKTREE };
