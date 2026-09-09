// The git guard table over parsed argvs with the filesystem facts passed in, the one refusal of destructive actions

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { INTERPRETER, parse, strip } from '../text/argv.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The reason the arguments are destructive, undefined when they can run
type Refinement = (args: readonly string[], existing: ReadonlySet<string>) => string | undefined;

// A subcommand's row: any refuses every argument list, flags and starts the arguments they name, safe first arguments pass, and the refinement decides the rest
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

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADVICE = 'destructive git actions are refused, leave the tree as it is';
const _KIB = 1024;
const _MAX_COMMAND_KIB = 128;
const _MAX_COMMAND = _MAX_COMMAND_KIB * _KIB;
const _TOO_LONG = `command exceeds the ${_MAX_COMMAND_KIB} KiB check limit, split it`;
const _ALIAS = 'inline git alias can hide a refused subcommand';
const _CTRL = /\p{Cc}+/gu;
const _CHECKOUT_CREATE: readonly string[] = ['-b', '--orphan', '-t', '--track', '--detach'];
const _GIT_VALUE_OPTS: readonly string[] = ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'];
const _REFINED: readonly string[] = ['reset', 'checkout'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');

// Clustered short flags holding the letter, as -SW holds both
const _short = (word: string, letter: string): boolean => word.startsWith('-') && !word.startsWith('--') && word.includes(letter);

// Resets move HEAD off the branch tip unless they name an existing path or an index-only unstage
const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !_isFlag(word));
    const [target] = targets;
    if (target === undefined || args.includes('--') || targets.some((named) => existing.has(named))) {
        return;
    }
    return `git reset ${target} moves HEAD and drops commits from the branch`;
};

// Restores touch the working tree unless they are index-only --staged
const _restore: Refinement = (args) => {
    const staged = args.some((word) => word === '-S' || word === '--staged' || _short(word, 'S'));
    const worktree = args.some((word) => word === '-W' || word === '--worktree' || _short(word, 'W'));
    return staged && !worktree ? undefined : 'git restore overwrites working-tree files';
};

// Checkouts overwrite working-tree files unless they only move a ref or create a branch
const _checkout: Refinement = (args, existing) => {
    const targets = args.filter((word) => word === '-' || !_isFlag(word));
    const first = targets[0] ?? '';
    if (!args.includes('--') && args.some((word) => _CHECKOUT_CREATE.includes(word))) {
        return;
    }
    if (args.includes('--') || targets.length > 1 || first === '.' || (targets.length > 0 && first.startsWith(':'))) {
        return 'git checkout with a pathspec overwrites working-tree files';
    }
    if (targets.length === 0 || (targets.length === 1 && first === '-')) {
        return;
    }
    return existing.has(first) ? `git checkout ${first} names an existing path and would overwrite it` : undefined;
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

// The index past the global options, consuming the operand of each value-taking one
const _skip = (argv: readonly string[], index: number): number => {
    const word = argv[index];
    return word !== undefined && _isFlag(word) ? _skip(argv, index + (_GIT_VALUE_OPTS.includes(word) ? 2 : 1)) : index;
};

// The two-word key first, then the one-word key
const _lookup = (words: readonly string[]): Head | undefined => {
    const pair = words.slice(0, 2).join(' ');
    const single = words[0] ?? '';
    if (_isKey(pair)) {
        return { key: pair, args: words.slice(2) };
    }
    return _isKey(single) ? { key: single, args: words.slice(1) } : undefined;
};

// The first flag or prefix hit among the arguments, '' for a row that refuses any argument list
const _hit = (row: GitRow, args: readonly string[]): string | undefined =>
    row.any === true ? '' : args.find((word) => row.flags?.includes(word) === true || row.starts?.some((start) => word.startsWith(start)) === true);

// Safe first arguments allow, a flag or prefix hit refuses with the row's why, and the refinement decides the rest
const _verdict = (head: Head, existing: ReadonlySet<string>): string | undefined => {
    const row = _rows[head.key];
    if (row.safe?.includes(head.args[0] ?? '') === true) {
        return undefined;
    }
    const hit = _hit(row, head.args);
    if (hit === undefined) {
        return row.refine?.(head.args, existing);
    }
    return head.key === INTERPRETER ? row.why : ['git', head.key, hit, row.why].filter((word) => word !== '').join(' ');
};

// The reason a git argv is destructive under the table, undefined when it can run
const _reason = (argv: readonly string[], existing: ReadonlySet<string>): string | undefined => {
    const index = _skip(argv, 1);
    if (argv.slice(1, index).some((word) => word.startsWith('alias.'))) {
        return _ALIAS;
    }
    const head = _lookup(argv.slice(index));
    return head === undefined ? undefined : _verdict(head, existing);
};

// Every git argv of a command, from an argv's first word or the word after a -- with basename git
const _heads = (command: string): readonly (readonly string[])[] =>
    parse(['git'])(command)
        .map((argv) => strip(argv).map((word) => word.text))
        .flatMap((words) =>
            words.flatMap((word, index) => ((index === 0 || words[index - 1] === '--') && basename(word) === 'git' ? [words.slice(index)] : [])),
        );

// The paths the reset and checkout refinements can test, for the hook body's $.fs.exists calls
const gitPaths = (command: string): readonly string[] =>
    _heads(command).flatMap((argv) => {
        const head = _lookup(argv.slice(_skip(argv, 1)));
        return head !== undefined && _REFINED.includes(head.key) ? head.args.filter((word) => !_isFlag(word)) : [];
    });

// argv.ts has no unparsed state, the length cap is the one fail-closed arm
const gitGuard =
    (existing: ReadonlySet<string>): (<E extends { readonly command: string }>(e: E) => Decision<E>) =>
    <E extends { readonly command: string }>(e: E): Decision<E> => {
        const reason = e.command.length > _MAX_COMMAND ? _TOO_LONG : _heads(e.command).flatMap((argv) => _reason(argv, existing) ?? [])[0];
        return reason === undefined ? rewrite(e) : deny(`${reason.replace(_CTRL, ' ')}, ${ADVICE}`);
    };

// --- [EXPORTS] -------------------------------------------------------------------------

export type { GitRow, Refinement };
export { ADVICE, gitGuard, gitPaths };
