// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { type Argv, type Command, INTERPRETER, strip } from '../text/argv.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

type Refinement = (args: Argv, existing: readonly string[]) => readonly string[];

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
    readonly args: Argv;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const _ADVICE = 'destructive git actions are refused, leave the tree as it is';
const _ALIAS = 'inline git alias can hide a refused subcommand';
const _CTRL = /\p{Cc}+/gu;
const _CHECKOUT_CREATE: readonly string[] = ['-b', '--orphan', '-t', '--track', '--detach'];
const _GIT_VALUE_OPTS: readonly string[] = ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'];
const _REFINED: readonly string[] = ['reset', 'checkout'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');

const _short = (word: string, letter: string): boolean => word.startsWith('-') && !word.startsWith('--') && word.includes(letter);

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

const _checkout: Refinement = (args, existing) => {
    const targets = args.filter((word) => word === '-' || !_isFlag(word));
    const first = targets[0] ?? '';
    if (!args.includes('--') && args.some((word) => _CHECKOUT_CREATE.includes(word))) {
        return [];
    }
    if (args.includes('--') || targets.length > 1 || first === '.' || (targets.length > 0 && first.startsWith(':'))) {
        return ['git checkout with a pathspec overwrites working-tree files'];
    }
    return targets.length === 1 && first !== '-' && existing.includes(first)
        ? [`git checkout ${first} names an existing path and would overwrite it`]
        : [];
};

// --- [POLICY] --------------------------------------------------------------------------

const GIT = {
    [INTERPRETER]: {
        why: 'run git directly and a script by its path, not through a shell or an interpreter',
        any: true,
    },
    branch: { why: 'deletes or force-moves a branch', flags: ['-d', '-D', '-M', '--delete'], starts: ['--force'] },
    checkout: { why: 'discards local changes', flags: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'], starts: ['--force'], refine: _checkout },
    clean: { why: 'deletes untracked files', any: true },
    config: { why: 'defines a git alias that can hide a refused subcommand', starts: ['alias.'] },
    push: { why: 'rewrites or deletes remote history', flags: ['-f', '-d', '--delete', '--mirror', '--prune'], starts: ['--force', '+', ':'] },
    rebase: { why: 'rewrites commits other agents can already hold', any: true },
    'reflog delete': { why: 'erases reflog entries, the last recovery path', any: true },
    'reflog drop': { why: 'erases reflog entries, the last recovery path', any: true },
    'reflog expire': { why: 'erases reflog entries, the last recovery path', any: true },
    reset: { why: 'wipes working-tree or index state', flags: ['--hard', '--merge', '--keep'], refine: _reset },
    restore: { why: 'discards working-tree state', refine: _restore },
    revert: { why: 'reverses committed history', any: true },
    stash: { why: 'hides uncommitted work other agents depend on', any: true, safe: ['list', 'show'] },
    switch: { why: 'discards local changes', flags: ['-f', '-C', '--discard-changes'], starts: ['--force'] },
} as const satisfies Readonly<Record<string, GitRow>>;

type Key = keyof typeof GIT;

const _rows: Readonly<Record<Key, GitRow>> = GIT;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isKey = (candidate: string): candidate is Key => Object.hasOwn(GIT, candidate);

const _skip = (argv: Argv, index: number): number => {
    const word = argv[index];
    return word !== undefined && _isFlag(word) ? _skip(argv, index + (_GIT_VALUE_OPTS.includes(word) ? 2 : 1)) : index;
};

const _heads = (words: Argv): readonly Head[] =>
    [words.slice(0, 2).join(' '), words[0] ?? '']
        .filter(_isKey)
        .slice(0, 1)
        .map((key): Head => ({ key, args: words.slice(key.split(' ').length) }));

const _hits = (row: GitRow, args: Argv): readonly string[] =>
    args.filter((word) => row.flags?.includes(word) === true || row.starts?.some((start) => word.startsWith(start)) === true).slice(0, 1);

const _verdict = (head: Head, existing: readonly string[]): readonly string[] => {
    const row = _rows[head.key];
    if (row.safe?.includes(head.args[0] ?? '') === true) {
        return [];
    }
    if (row.any === true) {
        return [head.key === INTERPRETER ? row.why : `git ${head.key} ${row.why}`];
    }
    const hits = _hits(row, head.args);
    return hits.length > 0 ? hits.map((hit) => `git ${head.key} ${hit} ${row.why}`) : (row.refine?.(head.args, existing) ?? []);
};

const _reason = (argv: Argv, existing: readonly string[]): readonly string[] => {
    const index = _skip(argv, 1);
    return argv.slice(1, index).some((word) => word.startsWith('alias.'))
        ? [_ALIAS]
        : _heads(argv.slice(index)).flatMap((head) => _verdict(head, existing));
};

const _gits = (commands: readonly Command[]): readonly Argv[] =>
    commands
        .map((command) => strip(command.words))
        .flatMap((words) =>
            words.flatMap((word, index) => ((index === 0 || words[index - 1] === '--') && basename(word) === 'git' ? [words.slice(index)] : [])),
        );

const gitPaths = (commands: readonly Command[]): readonly string[] =>
    _gits(commands).flatMap((argv) =>
        _heads(argv.slice(_skip(argv, 1))).flatMap((head) => (_REFINED.includes(head.key) ? head.args.filter((word) => !_isFlag(word)) : [])),
    );

const gitGuard =
    (commands: readonly Command[], existing: readonly string[]): (<E>(e: E) => Decision<E>) =>
    <E>(e: E): Decision<E> => {
        const reasons = _gits(commands)
            .flatMap((argv) => _reason(argv, existing))
            .map((reason) => reason.replace(_CTRL, ' '));
        const distinct = reasons.filter((reason, index) => reasons.indexOf(reason) === index);
        return distinct.length === 0 ? rewrite(e) : deny(`${distinct.join(', ')}, ${_ADVICE}`);
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export { gitGuard, gitPaths };
