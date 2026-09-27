import type { ToolCallInput } from 'claude-code';
import type { Command } from '../command.ts';
import { type Decision, deny, none, type Option, pass, refuse, some } from '../composition.ts';
import { type Invocation, invocations } from '../invocation.ts';
import { basename } from '../path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[]) => readonly string[];
type WorktreeEvent = Extract<ToolCallInput, { readonly tool: 'Agent' | 'EnterWorktree' }>;

interface GitRow {
    readonly reason: string;
    readonly any?: true;
    readonly flags?: readonly string[];
    readonly prefixes?: readonly string[];
    readonly safe?: readonly string[];
    readonly refine?: Refinement;
}
interface Head {
    readonly key: Key;
    readonly args: readonly string[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _WORKTREE = 'creates a second checkout with its own metadata and sync cost';

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');
const _short = (word: string, letter: string): boolean => _isFlag(word) && !word.startsWith('--') && word.includes(letter);

const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !_isFlag(word));
    const [target] = targets;
    return target === undefined || args.includes('--') || targets.some((named) => existing.includes(named)) ? [] : [`git reset ${target} moves HEAD and drops commits from the branch`];
};

const _restore: Refinement = (args) => {
    const staged = args.some((word) => word === '-S' || word === '--staged' || _short(word, 'S'));
    const worktree = args.some((word) => word === '-W' || word === '--worktree' || _short(word, 'W'));
    return staged && !worktree ? [] : ['git restore overwrites working-tree files'];
};

const _config: Refinement = (args) => {
    const alias = args.find((word) => word.startsWith('alias.'));
    return alias !== undefined && args.slice(args.indexOf(alias) + 1).some((word) => !_isFlag(word)) ? [`git config ${alias} defines a git alias that can hide a refused subcommand`] : [];
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

// --- [POLICY] --------------------------------------------------------------------------

const GIT = {
    branch: { reason: 'deletes or force-moves a branch', flags: ['-d', '-D', '-M', '--delete'], prefixes: ['--force'] },
    checkout: { reason: 'discards local changes', flags: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], prefixes: ['--force'], refine: _checkout },
    clean: { reason: 'deletes untracked files', any: true },
    config: { reason: 'defines a git alias that can hide a refused subcommand', refine: _config },
    push: { reason: 'rewrites or deletes remote history', flags: ['-f', '-d', '--delete', '--mirror', '--prune'], prefixes: ['--force', '+', ':'] },
    rebase: { reason: 'rewrites commits other agents can hold', any: true },
    'reflog delete': { reason: 'erases reflog entries, the last recovery path', any: true },
    'reflog drop': { reason: 'erases reflog entries, the last recovery path', any: true },
    'reflog expire': { reason: 'erases reflog entries, the last recovery path', any: true },
    reset: { reason: 'wipes working-tree or index state', flags: ['--hard', '--merge', '--keep'], refine: _reset },
    restore: { reason: 'discards working-tree state', refine: _restore },
    revert: { reason: 'reverses committed history', any: true },
    stash: { reason: 'hides uncommitted work other agents depend on', any: true, safe: ['list', 'show'] },
    switch: { reason: 'discards local changes', flags: ['-f', '-C', '--discard-changes'], prefixes: ['--force'] },
    worktree: { reason: _WORKTREE, any: true, safe: ['list'] },
} as const satisfies Readonly<Record<string, GitRow>>;

type Key = keyof typeof GIT;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isKey = (candidate: string): candidate is Key => Object.hasOwn(GIT, candidate);

const _skip = (words: readonly string[], index: number): number => {
    const word = words[index];
    return word !== undefined && _isFlag(word) ? _skip(words, index + (['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'].includes(word) ? 2 : 1)) : index;
};

const _head = (words: readonly string[]): Option<Head> => {
    const key = [words.slice(0, 2).join(' '), ...words.slice(0, 1)].find(_isKey);
    return key === undefined ? none : some({ key, args: words.slice(key.split(' ').length) });
};

const _refusals = (head: Head, existing: readonly string[]): readonly string[] => {
    const row: GitRow = GIT[head.key];
    const [first] = head.args;
    if (first !== undefined && row.safe?.includes(first) === true) {
        return [];
    }
    if (row.any === true) {
        return [`git ${head.key} ${row.reason}`];
    }
    const hit = head.args.find((word) => row.flags?.includes(word) === true || row.prefixes?.some((prefix) => word.startsWith(prefix)) === true);
    return hit === undefined ? (row.refine?.(head.args, existing) ?? []) : [`git ${head.key} ${hit} ${row.reason}`];
};

const _reason = (words: readonly string[], existing: readonly string[]): readonly string[] => {
    const index = _skip(words, 1);
    if (words.slice(1, index).some((word) => word.startsWith('alias.'))) {
        return ['inline git alias can hide a refused subcommand'];
    }
    const head = _head(words.slice(index));
    return head.kind === 'some' ? _refusals(head.value, existing) : [];
};

const _gits = (commands: readonly Command[]): readonly Invocation[] => commands.flatMap((command) => invocations(command.words).filter(([head]) => basename(head) === 'git'));

const gitPaths = (commands: readonly Command[]): readonly string[] =>
    _gits(commands).flatMap((words) => {
        const head = _head(words.slice(_skip(words, 1)));
        return head.kind === 'some' && ['reset', 'checkout'].includes(head.value.key) ? head.value.args.filter((word) => !_isFlag(word)) : [];
    });

const gitPolicy = (commands: readonly Command[], existing: readonly string[]): (<E>(e: E) => Decision<E>) => refuse(_gits(commands).flatMap((words) => _reason(words, existing)));

const worktreePolicy = (e: WorktreeEvent): Decision<WorktreeEvent> =>
    e.tool === 'EnterWorktree' || e.isolation === 'worktree' ? deny(`${e.tool === 'EnterWorktree' ? e.tool : `${e.tool} isolation worktree`} ${_WORKTREE}`) : pass(e);

// --- [EXPORTS] -------------------------------------------------------------------------

export type { WorktreeEvent };
export { gitPaths, gitPolicy, worktreePolicy };
