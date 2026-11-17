import { Array, Effect, Option, Predicate, Struct } from 'effect';
import { RedirOperator } from 'sh-syntax/sync';
import type { Host } from '#/host';
import { all, commandOf, edited, folderOf, holds, inserted, perSubcommand, quote, type Rule, type Ruleset, redirected, refusal, refuse, relocated, respell, unresolved, when, whole } from '#/rules/rule';
import type { Invocation } from '#/shell/invocation';

// --- [CONSTANTS] -----------------------------------------------------------------------

const BRANCHES = ['changes branches', 'stay on this branch'] as const;
const OVERWRITES = ['overwrites edits', 'use Edit'] as const;
const REMOTE = ['rewrites remote history', 'push plain refs'] as const;
const ALIASES = ['aliases hide subcommands', 'run them directly'] as const;
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

const dirty = (host: Host['Service'], invocation: Invocation, paths: readonly string[]) =>
    Effect.orElseSucceed(
        Effect.map(host.output(['git', '-C', Option.getOrElse(folderOf(host, invocation, '-C'), () => host.root), 'status', '--porcelain', '--', ...paths]), (listed) => listed.trim() !== ''),
        () => true,
    );

const checkoutPaths: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    const [first, ...more] = operands;
    if (first === undefined || invocation.has('--detach')) {
        return [];
    }
    const paths = invocation.tail.length > 0 || more.length > 0 || first.text === '.' || first.text.startsWith(':') || (first.text !== '-' && (yield* present(host, invocation, first.text)));
    const uncommitted =
        paths &&
        (!operands.every(({ known }) => known) ||
            (yield* dirty(
                host,
                invocation,
                operands.map(({ text }) => text),
            )));
    return uncommitted ? refusal(`${commandOf(invocation)} ${first.text} ${OVERWRITES.join(', ')}`) : [];
});

const restoredPaths: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    const uncommitted =
        !operands.every(({ known }) => known) ||
        (yield* dirty(
            host,
            invocation,
            operands.map(({ text }) => text),
        ));
    return uncommitted ? refusal(`${commandOf(invocation)} ${OVERWRITES.join(', ')}`) : [];
});

const appliedPatch: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { operands } = invocation;
    if (operands.length === 0 || !operands.every(({ known }) => known)) {
        return refusal(`${commandOf(invocation)} reads a patch no check can list before it runs, pass the patch file`);
    }
    const listed = yield* Effect.orElseSucceed(host.output(['git', '-C', Option.getOrElse(folderOf(host, invocation, '-C'), () => host.root), 'apply', '--numstat', ...operands.map(({ text }) => text)]), () => '');
    const paths = listed.split('\n').flatMap((line) => Option.toArray(Array.get(line.split('\t'), 2)));
    return paths.length === 0 || (yield* dirty(host, invocation, paths)) ? refusal(`${commandOf(invocation)} ${OVERWRITES.join(', ')}`) : [];
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
        Effect.filter(Array.getSomes(redirected(invocation, RedirOperator.RdrOut).map(({ target }) => target)), ({ text, known }) => {
            const file = Option.getOrUndefined(Option.filter(host.resolve(text, invocation.command.cwd), (candidate) => host.inside(candidate, host.root)));
            return file === undefined || !known ? Effect.succeed(file !== undefined) : dirty(host, invocation, [file]);
        }),
        ([file]) => (file === undefined ? [] : refusal(`${commandOf(invocation)} > ${file.text} ${OVERWRITES.join(', ')}`)),
    );

const exportedTree: Rule = Effect.fnUntraced(function* (invocation, { host }) {
    const { path, operands } = invocation;
    const [folder, revision] = path[1] === undefined ? operands.slice(1) : operands;
    if (!nested('add')(invocation) || folder === undefined) {
        return [];
    }
    if (invocation.has('-b', '-B', '--orphan')) {
        return refusal(`${commandOf(invocation)} ${BRANCHES.join(', ')}`);
    }
    const temporary = Option.getOrElse(yield* host.environment('TMPDIR'), () => '/tmp');
    const target = Option.getOrUndefined(Option.filter(host.resolve(relocated(folder.text, temporary), folderOf(host, invocation, '-C')), () => folder.known));
    const found = target !== undefined && (yield* host.exists(target));
    const occupied = target === undefined || (found && (yield* Effect.match(host.fs.readDirectory(target), { onFailure: () => true, onSuccess: Array.isReadonlyArrayNonEmpty })));
    if (occupied) {
        return refusal(`${commandOf(invocation)} ${folder.raw} ${folder.known ? 'holds files the export overwrites, pass a new folder' : 'expands later, pass a literal path'}`);
    }
    const git = ['git', ...invocation.supplied('-C').flatMap(({ raw }) => ['-C', raw])].join(' ');
    const exported = relocated(folder.raw);
    return whole(invocation, `mkdir -p ${exported} && ${git} archive ${revision?.raw ?? 'HEAD'} | tar -x -C ${exported}`, 'git worktree add');
});

const forcedRefs: Rule = (invocation) =>
    invocation.operands.flatMap((word) => {
        if (word.text.startsWith(':')) {
            return refusal(`${commandOf(invocation)} ${word.text} ${REMOTE.join(', ')}`);
        }
        return word.text.startsWith('+') && word.known ? edited(invocation, word, quote(word.text.slice(1)), `git push ${word.text}`) : [];
    });

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
                        clean: [when(({ has }) => !has('--dry-run'), refuse("deletes other sessions' untracked files", 'rm the paths you created by name'))],
                        config: [when(({ operands }) => operands.slice(0, -1).some(({ text }) => text.startsWith('alias.')), refuse(...ALIASES))],
                        push: [when(Predicate.not(holds(TAG)), UNFORCED, refuse(...REMOTE, '--delete', '--mirror', '--prune'), forcedRefs)],
                        rebase: [when(({ has }) => !has(...UNWINDS), refuse('rewrites shared commits', 'merge or commit forward'))],
                        reflog: [when(nested('delete', 'drop', 'expire'), refuse('deletes entries', 'leave the reflog intact'))],
                        reset: [refuse(...OVERWRITES, ...RESET_DISCARDS), when(({ has }) => !has(...RESET_DISCARDS), resetTarget)],
                        apply: [when(({ has }) => !has('--cached') && (has('--apply') || !has('--check', '--stat', '--numstat', '--summary')), appliedPatch)],
                        restore: [when(({ has }) => !has('--staged') || has('--worktree'), restoredPaths)],
                        revert: [when(({ has }) => !has(...UNWINDS), refuse('reverses committed history', 'commit a forward fix'))],
                        show: [when(holds(REVISION_PATH), showOverwrite)],
                        stash: [when(Predicate.not(nested('list', 'show')), refuse('hides work', 'leave the work in the tree'))],
                        switch: [UNFORCED, refuse(...OVERWRITES, '-C'), refuse(...BRANCHES, '--create', '--orphan')],
                    }),
                ),
                perSubcommand({ worktree: [exportedTree] }),
            ],
        },
        patch: {
            flags: ['-d=', '--directory='],
            rules: [(invocation, { host }) => (Option.exists(folderOf(host, invocation, '-d', '--directory'), (folder) => host.inside(folder, host.root)) ? refusal('patch applies a diff outside git, run git apply') : [])],
        },
    },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { GIT };
