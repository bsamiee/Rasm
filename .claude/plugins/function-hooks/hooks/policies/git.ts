// --- [IMPORTS] -------------------------------------------------------------------------

import type { ToolCallInput } from 'claude-code';
import { type Decision, deny, pass } from '../composition/decision.ts';
import { fromNullable, none, type Option, some } from '../composition/option.ts';
import { type Command, strip } from '../text/command.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: readonly string[], existing: readonly string[]) => readonly string[];

interface GitRow {
    readonly why: string;
    readonly any?: true;
    readonly flags?: readonly string[];
    readonly starts?: readonly string[];
    readonly safe?: readonly string[];
    readonly refine?: Refinement;
}

interface Head {
    readonly key: Key;
    readonly args: readonly string[];
}

type WorktreeEvent = Extract<ToolCallInput, { readonly tool: 'Agent' | 'EnterWorktree' }>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ADVICE = 'leave the tree and its history as they are';
const _ALIAS = 'inline git alias can hide a refused subcommand';
const _DIRECT = 'git show HEAD:<path> reads one committed file, git archive <rev> <path> | tar -x -C <dir> extracts a committed tree';
const _WORKTREE = `creates a second checkout with its own metadata and sync cost, ${_DIRECT}`;
const _CHECKOUT_CREATE: readonly string[] = ['-b', '--orphan', '-t', '--track', '--detach'];
const _GIT_VALUE_OPTS: readonly string[] = ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'];
const _REFINED: readonly string[] = ['reset', 'checkout'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');

const _short = (word: string, letter: string): boolean => _isFlag(word) && !word.startsWith('--') && word.includes(letter);

const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !_isFlag(word));
    const [target] = targets;
    return target === undefined || args.includes('--') || targets.some((named) => existing.includes(named))
        ? []
        : [`git reset ${target} moves HEAD and drops commits from the branch`];
};

const _restore: Refinement = (args) => {
    const staged = args.some((word) => word === '-S' || word === '--staged' || _short(word, 'S'));
    const worktree = args.some((word) => word === '-W' || word === '--worktree' || _short(word, 'W'));
    return staged && !worktree ? [] : ['git restore overwrites working-tree files'];
};

const _config: Refinement = (args) => {
    const alias = args.find((word) => word.startsWith('alias.'));
    return alias !== undefined && args.slice(args.indexOf(alias) + 1).some((word) => !_isFlag(word))
        ? [`git config ${alias} defines a git alias that can hide a refused subcommand`]
        : [];
};

const _checkout: Refinement = (args, existing) => {
    const [first, ...more] = args.filter((word) => word === '-' || !_isFlag(word));
    if (!args.includes('--') && args.some((word) => _CHECKOUT_CREATE.includes(word))) {
        return [];
    }
    if (args.includes('--') || more.length > 0 || first === '.' || first?.startsWith(':') === true) {
        return ['git checkout with a pathspec overwrites working-tree files'];
    }
    return first !== undefined && first !== '-' && existing.includes(first)
        ? [`git checkout ${first} names an existing path and would overwrite it`]
        : [];
};

// --- [POLICY] --------------------------------------------------------------------------

const GIT = {
    branch: { why: 'deletes or force-moves a branch', flags: ['-d', '-D', '-M', '--delete'], starts: ['--force'] },
    checkout: { why: 'discards local changes', flags: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], starts: ['--force'], refine: _checkout },
    clean: { why: 'deletes untracked files', any: true },
    config: { why: 'defines a git alias that can hide a refused subcommand', refine: _config },
    push: { why: 'rewrites or deletes remote history', flags: ['-f', '-d', '--delete', '--mirror', '--prune'], starts: ['--force', '+', ':'] },
    rebase: { why: 'rewrites commits other agents can already hold', any: true },
    'reflog delete': { why: 'erases reflog entries, the last recovery path', any: true },
    'reflog drop': { why: 'erases reflog entries, the last recovery path', any: true },
    'reflog expire': { why: 'erases reflog entries, the last recovery path', any: true },
    reset: { why: 'wipes working-tree or index state', flags: ['--hard', '--merge', '--keep'], refine: _reset },
    restore: { why: 'discards working-tree state', refine: _restore },
    revert: { why: 'reverses committed history', any: true },
    stash: { why: `hides uncommitted work other agents depend on, ${_DIRECT}`, any: true, safe: ['list', 'show'] },
    switch: { why: 'discards local changes', flags: ['-f', '-C', '--discard-changes'], starts: ['--force'] },
    worktree: { why: _WORKTREE, any: true, safe: ['list'] },
} as const satisfies Readonly<Record<string, GitRow>>;

type Key = keyof typeof GIT;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isKey = (candidate: string): candidate is Key => Object.hasOwn(GIT, candidate);

const _skip = (words: readonly string[], index: number): number => {
    const word = words[index];
    return word !== undefined && _isFlag(word) ? _skip(words, index + (_GIT_VALUE_OPTS.includes(word) ? 2 : 1)) : index;
};

const _head = (words: readonly string[]): Option<Head> => {
    const key = [words.slice(0, 2).join(' '), ...words.slice(0, 1)].find(_isKey);
    return key === undefined ? none : some({ key, args: words.slice(key.split(' ').length) });
};

const _hit = (row: GitRow, args: readonly string[]): Option<string> =>
    fromNullable(args.find((word) => row.flags?.includes(word) === true || row.starts?.some((start) => word.startsWith(start)) === true));

const _refusals = (head: Head, existing: readonly string[]): readonly string[] => {
    const row: GitRow = GIT[head.key];
    const [first] = head.args;
    if (first !== undefined && row.safe?.includes(first) === true) {
        return [];
    }
    if (row.any === true) {
        return [`git ${head.key} ${row.why}`];
    }
    const hit = _hit(row, head.args);
    return hit.kind === 'some' ? [`git ${head.key} ${hit.value} ${row.why}`] : (row.refine?.(head.args, existing) ?? []);
};

const _reason = (words: readonly string[], existing: readonly string[]): readonly string[] => {
    const index = _skip(words, 1);
    if (words.slice(1, index).some((word) => word.startsWith('alias.'))) {
        return [_ALIAS];
    }
    const head = _head(words.slice(index));
    return head.kind === 'some' ? _refusals(head.value, existing) : [];
};

const _gits = (commands: readonly Command[]): readonly (readonly string[])[] =>
    commands
        .map((command) => strip(command.words))
        .flatMap((words) =>
            words.flatMap((word, index) => ((index === 0 || words[index - 1] === '--') && basename(word) === 'git' ? [words.slice(index)] : [])),
        );

const gitPaths = (commands: readonly Command[]): readonly string[] =>
    _gits(commands).flatMap((words) => {
        const head = _head(words.slice(_skip(words, 1)));
        return head.kind === 'some' && _REFINED.includes(head.value.key) ? head.value.args.filter((word) => !_isFlag(word)) : [];
    });

const gitPolicy =
    (commands: readonly Command[], existing: readonly string[]): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        const distinct: ReadonlySet<string> = new Set(_gits(commands).flatMap((words) => _reason(words, existing)));
        return distinct.size === 0 ? pass(e) : deny(`${[...distinct].join(', ')}, ${_ADVICE}`);
    };

const _isolated = (e: WorktreeEvent): boolean => e.tool === 'EnterWorktree' || e.isolation === 'worktree';

const _label = (e: WorktreeEvent): string => (e.tool === 'EnterWorktree' ? e.tool : `${e.tool} isolation worktree`);

const worktreePolicy = (e: WorktreeEvent): Decision<WorktreeEvent> => (_isolated(e) ? deny(`${_label(e)} ${_WORKTREE}, ${_ADVICE}`) : pass(e));

// --- [EXPORTS] -------------------------------------------------------------------------

export type { WorktreeEvent };
export { gitPaths, gitPolicy, worktreePolicy };
