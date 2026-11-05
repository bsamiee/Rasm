import { Array, Effect, Option, Predicate } from 'effect';
import type { Host } from '#/host';
import { removed } from '#/policies/files';
import { all, type Call, type Fix, folderOf, has, holds, inserted, type Policy, perSubcommand, redirected, refusal, refuse, respell, subject, unresolved, when, whole } from '#/policies/fix';
import type { Word } from '#/shell/script';
import type { Site } from '#/shell/site';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRANCHES = ['changes branches', 'stay on this branch'] as const;
const OVERWRITES = ['overwrites edits', 'use Edit'] as const;
const REMOTE = ['rewrites remote history', 'push plain refs'] as const;
const ALIASES = ['aliases hide subcommands', 'run them directly'] as const;
const HISTORY = /^[+:]/u;
const ALIAS_KEY = 'alias.';
const ALIAS = new RegExp(`^${RegExp.escape(ALIAS_KEY)}`, 'u');
const REVISION_PATH = /^[^:]+:.+/u;
const BRANCH_WRITES = ['--delete', '-D', '-M', '--force', '--copy', '-C', '--move'];
const RESET_DISCARDS = ['--hard', '--merge', '--keep'];
const CURRENT = ['HEAD', '@'];
const UNWINDS = ['--abort', '--quit'];
const LISTING = ['--list', '--all', '--remotes', '--verbose', '--show-current', '--contains', '--no-contains', '--merged', '--no-merged', '--points-at', '-u'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const subcommand =
    (...names: readonly string[]): Predicate.Predicate<Site> =>
    ({ path, operands }) =>
        names.includes(path[1] ?? operands[0]?.text ?? '');

const present = (host: typeof Host.Service, site: Site, word: string) =>
    Option.match(host.place(word, folderOf(host, site, '-C')), {
        onNone: () => Effect.succeed(false),
        onSome: host.exists,
    });

const writesAlias: Predicate.Predicate<Site> = ({ operands }) => {
    const at = operands.findIndex(({ text }) => text.startsWith(ALIAS_KEY));
    return at >= 0 && at < operands.length - 1;
};

const inRepository =
    (...fixes: readonly Fix[]): Fix =>
    (site, call) => {
        const folder = folderOf(call.host, site, '-C');
        return Option.exists(folder, (place) => !call.host.inside(place, call.host.root))
            ? []
            : Effect.map(all(...fixes)(site, call), (verdicts) =>
                  verdicts.length === 0
                      ? []
                      : Option.match(folder, {
                            onNone: () => unresolved(site),
                            onSome: () => verdicts,
                        }),
              );
    };

const checkoutPaths: Fix = Effect.fnUntraced(function* (site, { host }) {
    const [first, ...more] = site.operands;
    if (first === undefined || site.has('--detach')) {
        return [];
    }
    const paths = site.tail.length > 0 || more.length > 0 || first.text === '.' || first.text.startsWith(':') || (first.text !== '-' && (yield* present(host, site, first.text)));
    return paths ? refusal(`${subject(site)} ${first.text} ${OVERWRITES.join(', ')}`) : [];
});

const resetTarget: Fix = Effect.fnUntraced(function* (site, { host }) {
    const [target] = site.operands;
    const paths = yield* Effect.forEach(site.operands, ({ text }) => present(host, site, text));
    if (target === undefined || CURRENT.includes(target.text) || site.tail.length > 0 || paths.some(Boolean)) {
        return [];
    }
    return !target.known && site.command.origin === 'call' ? inserted(site, target.start, '-- ') : refusal(`${subject(site)} ${target.text} moves HEAD, commit forward`);
});

const overwrites = ({ text, known }: Word, { command: { cwd } }: Site, { host }: Call) => {
    const place = Option.getOrUndefined(Option.filter(host.place(text, cwd), (held) => host.inside(held, host.root)));
    if (place === undefined) {
        return Effect.succeed(false);
    }
    return known ? Effect.zipWith(host.exists(place), host.run(['git', 'check-ignore', '-q', '--', place]), (held, { code }) => held && code !== 0) : Effect.succeed(true);
};

const overwrittenByShow: Fix = (site, call) =>
    Effect.map(
        Effect.filter(Array.getSomes(redirected(site, '>').map(({ target }) => target)), (word) => overwrites(word, site, call)),
        ([file]) => (file === undefined ? [] : refusal(`${subject(site)} > ${file.text} ${OVERWRITES.join(', ')}`)),
    );

const untrackedMove: Fix = Effect.fnUntraced(function* (site, { host }) {
    const [source] = site.operands;
    if (source === undefined || !source.known || site.names.difference(new Set(['-C'])).size > 0) {
        return [];
    }
    const { code } = yield* host.run(['git', '-C', Option.getOrElse(folderOf(host, site, '-C'), () => host.root), 'ls-files', '--error-unmatch', '--', source.text]);
    return code === 0 ? [] : whole(site, ['mv', ...site.operands.map(({ raw }) => raw)].join(' '), 'git mv');
});

// --- [TABLES] --------------------------------------------------------------------------

const UNFORCED = respell({ '--force': '', '--force-with-lease': '', '--force-if-includes': '', '--discard-changes': '' }, { notify: true });

const GIT: Policy = {
    programs: {
        git: {
            stops: ['stash', 'grep', 'config'],
            fixes: [
                inRepository(
                    when(holds(ALIAS, '-c'), refuse(...ALIASES, '-c')),
                    perSubcommand({
                        branch: [
                            refuse(...BRANCHES, ...BRANCH_WRITES),
                            when(
                                Predicate.and(({ operands }) => operands.length > 0, Predicate.not(has(...LISTING, ...BRANCH_WRITES))),
                                refuse(...BRANCHES),
                            ),
                        ],
                        checkout: [UNFORCED, refuse(...OVERWRITES, '-B', '--patch', '--ours', '--theirs'), refuse(...BRANCHES, '-b', '--orphan', '--track'), checkoutPaths],
                        clean: [when(Predicate.not(has('--dry-run')), refuse('deletes untracked files', 'rm each by name'))],
                        config: [when(writesAlias, refuse(...ALIASES))],
                        push: [UNFORCED, refuse(...REMOTE, '--delete', '--mirror', '--prune'), when(holds(HISTORY), refuse(...REMOTE))],
                        mv: [untrackedMove],
                        rebase: [when(Predicate.not(has(...UNWINDS)), refuse('rewrites shared commits', 'merge or commit forward'))],
                        reflog: [when(subcommand('delete', 'drop', 'expire'), refuse('deletes entries', 'leave the reflog intact'))],
                        reset: [refuse(...OVERWRITES, ...RESET_DISCARDS), when(Predicate.not(has(...RESET_DISCARDS)), resetTarget)],
                        restore: [when(({ has: holding }) => !holding('--staged') || holding('--worktree'), refuse(...OVERWRITES))],
                        revert: [when(Predicate.not(has(...UNWINDS)), refuse('reverses committed history', 'commit a forward fix'))],
                        rm: [when(Predicate.not(has('--cached')), removed)],
                        show: [when(holds(REVISION_PATH), overwrittenByShow)],
                        stash: [when(Predicate.not(subcommand('list', 'show')), refuse('hides work', 'commit or leave it'))],
                        switch: [UNFORCED, refuse(...OVERWRITES, '-C'), refuse(...BRANCHES, '--create', '--orphan')],
                        worktree: [when(Predicate.not(subcommand('list')), refuse('creates a checkout', 'use this one'))],
                    }),
                ),
            ],
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { GIT };
