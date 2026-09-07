// The git guard table over parsed leaves with the filesystem facts passed in, the frozen policy verdicts are its spec

// --- [IMPORTS] -------------------------------------------------------------------------

import { type Decision, deny, rewrite } from '../composition/decision.ts';
import { flatMap, fromBoolean, fromNullable, fromPredicate, liftPredicate, map, none, type Option, some, toArray } from '../composition/option.ts';
import { INTERPRETER, leaves, strip } from '../text/argv.ts';
import { basename } from '../text/path.ts';

// --- [TYPES] ---------------------------------------------------------------------------

// The none case allows, the some case holds the reason
type Refinement = (args: readonly string[], existing: ReadonlySet<string>) => Option<string>;

interface GitRow {
    readonly why: string;
    readonly flags: readonly string[];
    readonly starts: readonly string[];
    readonly safe: readonly string[];
    readonly refine: Refinement;
}

type PathspecClass = 'create' | 'spec' | 'pure' | 'named' | 'fresh';

// --- [CONSTANTS] -----------------------------------------------------------------------

const ADVICE = 'Blocked by git-guard: destructive git actions are disabled. Keep all work as-is.';
const _KIB = 1024;
const _MAX_COMMAND_KIB = 128;
const _MAX_COMMAND = _MAX_COMMAND_KIB * _KIB;
const _TOO_LONG = 'the command is too long to lex within the hook deadline, it cannot be checked safely';
const _ALIAS = 'an inline git alias (-c alias.*) can hide a blocked subcommand';
const _PATHSPEC_SPEC = 'git checkout with a pathspec overwrites working-tree files';
const _CTRL = /\p{Cc}+/gu;
const _ANY_ARG: readonly string[] = [''];
const _CHECKOUT_CREATE: readonly string[] = ['-b', '--orphan', '-t', '--track', '--detach'];
const _GIT_VALUE_OPTS: readonly string[] = ['-C', '-c', '--git-dir', '--work-tree', '--namespace', '--config-env', '--exec-path'];
const _REFINED: readonly string[] = ['reset', 'checkout'];

// --- [REFINEMENTS] ---------------------------------------------------------------------

const _isFlag = (word: string): boolean => word.startsWith('-');

// Clustered short flags holding the letter, as -SW holds both
const _short = (word: string, letter: string): boolean => word.startsWith('-') && !word.startsWith('--') && word.includes(letter);

const _allow: Refinement = none;

// Resets move HEAD off the branch tip unless they name an existing path or an index-only unstage
const _reset: Refinement = (args, existing) => {
    const targets = args.filter((word) => !_isFlag(word));
    return liftPredicate<string>(() => !(args.includes('--') || targets.length === 0 || targets.some((target) => existing.has(target))))(
        `git reset ${targets[0] ?? ''} moves HEAD and drops commits from the branch`,
    );
};

// Restores touch the working tree unless they are index-only --staged
const _restore: Refinement = (args) => {
    const staged = args.some((word) => word === '-S' || word === '--staged' || _short(word, 'S'));
    const worktree = args.some((word) => word === '-W' || word === '--worktree' || _short(word, 'W'));
    return liftPredicate<string>(() => !(staged && !worktree))('git restore overwrites working-tree files');
};

const _PATHSPEC: Readonly<Record<PathspecClass, (target: string) => Option<string>>> = {
    create: none,
    spec: (): Option<string> => some(_PATHSPEC_SPEC),
    pure: none,
    named: (target): Option<string> => some(`git checkout ${target} names an existing path and would overwrite it`),
    fresh: none,
};

// Checkouts overwrite working-tree files unless they only move a ref or create a branch
const _pathspec: Refinement = (args, existing) => {
    const targets = args.filter((word) => word === '-' || !_isFlag(word));
    const first = targets[0] ?? '';
    const create = !args.includes('--') && args.some((word) => _CHECKOUT_CREATE.includes(word));
    const spec = args.includes('--') || targets.length > 1 || first === '.' || (targets.length > 0 && first.startsWith(':'));
    const pure = targets.length === 0 || (targets.length === 1 && first === '-');
    return _PATHSPEC[(create && 'create') || (spec && 'spec') || (pure && 'pure') || (existing.has(first) && 'named') || 'fresh'](first);
};

// --- [POLICY] --------------------------------------------------------------------------

const GIT = {
    [INTERPRETER]: {
        why: 'the command runs git or a script through a shell or an interpreter, run git directly and a script by its path',
        flags: [],
        starts: _ANY_ARG,
        safe: [],
        refine: _allow,
    },
    branch: { why: 'deletes or force-moves a branch', flags: ['-d', '-D', '-M', '--delete'], starts: ['--force'], safe: [], refine: _allow },
    checkout: {
        why: 'discards local changes',
        flags: ['-f', '-B', '-p', '--patch', '--ours', '--theirs'],
        starts: ['--force'],
        safe: [],
        refine: _pathspec,
    },
    clean: { why: 'deletes untracked files', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    config: { why: 'defines a git alias that can hide a blocked subcommand', flags: [], starts: ['alias.'], safe: [], refine: _allow },
    push: {
        why: 'rewrites or deletes remote history',
        flags: ['-f', '-d', '--delete', '--mirror', '--prune'],
        starts: ['--force', '+', ':'],
        safe: [],
        refine: _allow,
    },
    rebase: { why: 'rewrites commits other agents can already hold', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    'reflog delete': { why: 'erases reflog entries, the last recovery path', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    'reflog drop': { why: 'erases reflog entries, the last recovery path', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    'reflog expire': { why: 'erases reflog entries, the last recovery path', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    reset: { why: 'wipes working-tree or index state', flags: ['--hard', '--merge', '--keep'], starts: [], safe: [], refine: _reset },
    restore: { why: 'discards working-tree state', flags: [], starts: [], safe: [], refine: _restore },
    revert: { why: 'reverses committed history', flags: [], starts: _ANY_ARG, safe: [], refine: _allow },
    stash: { why: 'hides uncommitted work other agents depend on', flags: [], starts: _ANY_ARG, safe: ['list', 'show'], refine: _allow },
    switch: { why: 'discards local changes', flags: ['-f', '-C', '--discard-changes'], starts: ['--force'], safe: [], refine: _allow },
} as const satisfies Readonly<Record<string, GitRow>>;

type Key = keyof typeof GIT;

interface Head {
    readonly key: Key;
    readonly args: readonly string[];
}

const _rows: Readonly<Record<Key, GitRow>> = GIT;

// --- [OPERATIONS] ----------------------------------------------------------------------

const _isKey = fromPredicate((candidate: string): candidate is Key => Object.hasOwn(GIT, candidate));

// The index past the global options, consuming the operand of each value-taking one
const _skip = (argv: readonly string[], index: number): number =>
    fromNullable(argv[index]).match<number>({
        some: (word) =>
            fromBoolean(_isFlag(word)).match<number>({
                some: () => _skip(argv, index + ((_GIT_VALUE_OPTS.includes(word) && 2) || 1)),
                none: () => index,
            }),
        none: () => index,
    });

// The two-word key first, then the one-word key
const _lookup = (words: readonly string[]): Option<Head> =>
    _isKey(words.slice(0, 2).join(' ')).match<Option<Head>>({
        some: (key) => some({ key, args: words.slice(2) }),
        none: () => map((key: Key): Head => ({ key, args: words.slice(1) }))(_isKey(words.slice(0, 1).join(' '))),
    });

// The first flag or prefix hit, the empty candidate first and a starts of '' then hits any argument list
const _hit = (row: GitRow, args: readonly string[]): Option<string> =>
    fromNullable(['', ...args].find((word) => row.flags.includes(word) || row.starts.some((start) => word.startsWith(start))));

// Safe first arguments allow, a flag or prefix hit refuses with the row's why, and the refinement decides the rest
const _verdict =
    (existing: ReadonlySet<string>) =>
    (head: Head): Option<string> => {
        const row = _rows[head.key];
        const safe = fromNullable(head.args[0]).match<boolean>({ some: (first) => row.safe.includes(first), none: () => false });
        const named = (hit: string): string => ['git', head.key, hit, row.why].filter((word) => word !== '').join(' ');
        const why = map((hit: string) => fromBoolean(head.key === INTERPRETER).match<string>({ some: () => row.why, none: () => named(hit) }))(
            _hit(row, head.args),
        );
        const refined = why.match<Option<string>>({ some, none: () => row.refine(head.args, existing) });
        return flatMap(() => refined)(fromBoolean(!safe));
    };

// The reason a git argv is destructive under the table, none when it can run
const _reason =
    (existing: ReadonlySet<string>) =>
    (argv: readonly string[]): Option<string> => {
        const index = _skip(argv, 1);
        return fromBoolean(argv.slice(1, index).some((word) => word.startsWith('alias.'))).match<Option<string>>({
            some: () => some(_ALIAS),
            none: () => flatMap(_verdict(existing))(_lookup(argv.slice(index))),
        });
    };

// Every git argv of a command, from a leaf's first word or the word after a -- with basename git
const _heads = (command: string): readonly (readonly string[])[] =>
    leaves(['git'])(command)
        .map(strip)
        .flatMap((leaf) => {
            const words = leaf.map((word) => word.text);
            return words.flatMap((word, index) =>
                toArray(
                    liftPredicate<readonly string[]>(() => (index === 0 || words[index - 1] === '--') && basename(word) === 'git')(
                        words.slice(index),
                    ),
                ),
            );
        });

// argv.ts has no unparseable state, the length cap is the one fail-closed arm
const _refusal =
    (existing: ReadonlySet<string>) =>
    (command: string): Option<string> =>
        fromBoolean(command.length > _MAX_COMMAND).match<Option<string>>({
            some: () => some(_TOO_LONG),
            none: () => fromNullable(_heads(command).flatMap((argv) => toArray(_reason(existing)(argv)))[0]),
        });

// The paths the reset and checkout refinements can test, for the hook body's $.fs.exists calls
const gitPaths = (command: string): readonly string[] =>
    _heads(command).flatMap((argv) => {
        const head = flatMap(liftPredicate<Head>((candidate) => _REFINED.includes(candidate.key)))(_lookup(argv.slice(_skip(argv, 1))));
        return toArray(head).flatMap((refined) => refined.args.filter((word) => !_isFlag(word)));
    });

const gitGuard =
    (existing: ReadonlySet<string>): (<E extends { readonly command: string }>(e: E) => Decision<E, unknown, string>) =>
    <E extends { readonly command: string }>(e: E): Decision<E, unknown, string> =>
        _refusal(existing)(e.command).match<Decision<E, unknown, string>>({
            some: (reason) => deny(`git-guard: ${reason.replace(_CTRL, ' ')}. ${ADVICE}`),
            none: () => rewrite(e, []),
        });

// --- [EXPORTS] -------------------------------------------------------------------------

export type { GitRow, Refinement };
export { ADVICE, GIT, gitGuard, gitPaths };
