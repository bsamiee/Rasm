import { Array, Effect, Option, Predicate, Struct } from 'effect';
import type { Host } from '#/host';
import { all, commandOf, folderOf, holds, inserted, perSubcommand, type Rule, type Ruleset, redirected, refusal, refuse, respell, unresolved, when } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRANCHES = ['changes branches', 'stay on this branch'] as const;
const OVERWRITES = ['overwrites edits', 'use Edit'] as const;
const REMOTE = ['rewrites remote history', 'push plain refs'] as const;
const ALIASES = ['aliases hide subcommands', 'run them directly'] as const;
const HISTORY = /^[+:]/u;
const TAG = /^[+:]?refs\/tags\//u;
const ALIAS = /^alias\./u;
const REVISION_PATH = /^[^:]+:.+/u;
const BRANCH_WRITES = ['--delete', '-D', '-M', '--force', '--copy', '-C', '--move'];
const RESET_DISCARDS = ['--hard', '--merge', '--keep'];
const UNWINDS = ['--abort', '--quit'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const nested =
    (...names: readonly string[]): Predicate.Predicate<Invocation> =>
    ({ path, operands }) =>
        Option.exists(Option.firstSomeOf([Array.get(path, 1), Option.map(Array.head(operands), Struct.get('text'))]), (name) => names.includes(name));

const present = (host: Host['Service'], invocation: Invocation, word: string) =>
    Option.match(host.resolve(word, folderOf(host, invocation, '-C')), {
        onNone: () => Effect.succeed(false),
        onSome: host.exists,
    });

const inRepository =
    (...rules: readonly Rule[]): Rule =>
    (invocation, call) => {
        const folder = folderOf(call.host, invocation, '-C');
        return Option.exists(folder, (repository) => !call.host.inside(repository, call.host.root)) ? [] : Effect.map(all(...rules)(invocation, call), (verdicts) => (verdicts.length > 0 && Option.isNone(folder) ? unresolved(invocation) : verdicts));
    };

const checkoutPaths: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    const [first, ...more] = operands;
    if (first === undefined || invocation.has('--detach')) {
        return [];
    }
    const paths = invocation.tail.length > 0 || more.length > 0 || first.text === '.' || first.text.startsWith(':') || (first.text !== '-' && (yield* present(host, invocation, first.text)));
    const uncommitted = paths && (!operands.every(({ known }) => known) || (yield* host.output(['git', '-C', Option.getOrElse(folderOf(host, invocation, '-C'), () => host.root), 'status', '--porcelain', '--', ...operands.map(({ text }) => text)])).trim() !== '');
    return uncommitted ? refusal(`${commandOf(invocation)} ${first.text} ${OVERWRITES.join(', ')}`) : [];
});

const resetTarget: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const [target] = invocation.operands;
    const paths = yield* Effect.forEach(invocation.operands, ({ text }) => present(host, invocation, text));
    if (target === undefined || target.text === 'HEAD' || target.text === '@' || invocation.tail.length > 0 || paths.some(Boolean)) {
        return [];
    }
    return !target.known && invocation.command.origin === 'call' ? inserted(invocation, target.start, '-- ') : refusal(`${commandOf(invocation)} ${target.text} moves HEAD, commit forward`);
});

const showOverwrite: Rule = (invocation, { host }) =>
    Effect.map(
        Effect.filter(Array.getSomes(redirected(invocation, '>').map(({ target }) => target)), ({ text, known }) => {
            const file = Option.getOrUndefined(Option.filter(host.resolve(text, invocation.command.cwd), (candidate) => host.inside(candidate, host.root)));
            return file === undefined || !known ? Effect.succeed(file !== undefined) : Effect.map(host.output(['git', 'status', '--porcelain', '--', file]), (listed) => listed.trim() !== '');
        }),
        ([file]) => (file === undefined ? [] : refusal(`${commandOf(invocation)} > ${file.text} ${OVERWRITES.join(', ')}`)),
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const UNFORCED = respell({ drops: ['--force', '--force-with-lease', '--force-if-includes', '--discard-changes'], notify: true });

const GIT: Ruleset = {
    programs: {
        git: {
            stopsAtOperand: ['stash', 'grep', 'config'],
            rules: [
                inRepository(
                    when(holds(ALIAS, '-c'), refuse(...ALIASES, '-c')),
                    perSubcommand({
                        branch: [refuse(...BRANCHES, ...BRANCH_WRITES), when(({ operands, has }) => operands.length > 0 && !has('--list', '--all', '--remotes', '--verbose', '--show-current', '--contains', '--no-contains', '--merged', '--no-merged', '--points-at', '-u', ...BRANCH_WRITES), refuse(...BRANCHES))],
                        checkout: [UNFORCED, refuse(...OVERWRITES, '-B', '--patch', '--ours', '--theirs'), refuse(...BRANCHES, '-b', '--orphan', '--track'), checkoutPaths],
                        clean: [when(({ has }) => !has('--dry-run'), refuse('deletes untracked files', 'rm each by name'))],
                        config: [when(({ operands }) => operands.slice(0, -1).some(({ text }) => ALIAS.test(text)), refuse(...ALIASES))],
                        push: [when(Predicate.not(holds(TAG)), UNFORCED, refuse(...REMOTE, '--delete', '--mirror', '--prune'), when(holds(HISTORY), refuse(...REMOTE)))],
                        rebase: [when(({ has }) => !has(...UNWINDS), refuse('rewrites shared commits', 'merge or commit forward'))],
                        reflog: [when(nested('delete', 'drop', 'expire'), refuse('deletes entries', 'leave the reflog intact'))],
                        reset: [refuse(...OVERWRITES, ...RESET_DISCARDS), when(({ has }) => !has(...RESET_DISCARDS), resetTarget)],
                        restore: [when(({ has }) => !has('--staged') || has('--worktree'), refuse(...OVERWRITES))],
                        revert: [when(({ has }) => !has(...UNWINDS), refuse('reverses committed history', 'commit a forward fix'))],
                        show: [when(holds(REVISION_PATH), showOverwrite)],
                        stash: [when(Predicate.not(nested('list', 'show')), refuse('hides work', 'commit or leave it'))],
                        switch: [UNFORCED, refuse(...OVERWRITES, '-C'), refuse(...BRANCHES, '--create', '--orphan')],
                        worktree: [when(Predicate.not(nested('list')), refuse('creates a checkout', 'use this one'))],
                    }),
                ),
            ],
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { GIT };
