import { Call } from '@rasm/report';
import { Effect, FileSystem, Option, Path, Record } from 'effect';
import type { ProgramRow, Table } from '#/decision/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const RESTORE = { instead: 'read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool' };
const DISCARDS_EDITS = 'discards uncommitted edits other sessions in this checkout may hold';
const STRANDS_COMMITS = 'drops commits other sessions in this checkout may still need';
const FORCE_FLAGS = ['-f', '--force', '--discard-changes'];
const HELP_FLAGS = ['-h', '--help'];
const DRY_RUN_FLAGS = ['-n', '--dry-run'];
const PATH_FLAGS = ['--pathspec-from-file', '--ours', '--theirs', '-p', '--patch'];

// --- [OPERATIONS] ----------------------------------------------------------------------

const passHelp = (row: ProgramRow): ProgramRow => {
    if ('decide' in row) {
        return { ...row, decide: (invocation, call) => (invocation.has(...HELP_FLAGS) ? [] : row.decide(invocation, call)) };
    }
    return 'deny' in row ? { ...row, unless: [...(row.unless ?? []), ...HELP_FLAGS] } : row;
};

// --- [RULES] ---------------------------------------------------------------------------

const git: Table = Record.map<string, ProgramRow, ProgramRow>(
    {
        'git.stash': {
            program: 'git',
            path: ['stash'],
            decide: (invocation) =>
                ['list', 'show'].includes(invocation.path[1] ?? '')
                    ? []
                    : [{ subject: `git ${invocation.path.join(' ')}`, deny: 'hides work other sessions in this checkout may hold', values: { instead: 'leave the work in the tree and commit all of it whole when the phase ends' }, next: new Call({ tool: 'Bash', given: { command: 'git add -A && git commit' } }) }],
        },
        'git.revert': { program: 'git', path: ['revert'], unless: ['--abort', '--continue', '--quit', '--skip'], deny: 'undoes committed work other sessions in this checkout build on', values: RESTORE },
        'git.restore': { program: 'git', path: ['restore'], decide: (invocation) => (invocation.has('--staged') && !invocation.has('--worktree') ? [] : [{ subject: 'git restore', deny: DISCARDS_EDITS, values: RESTORE }]) },
        'git.checkout.paths': {
            program: 'git',
            path: ['checkout'],
            decide: Effect.fnUntraced(function* (invocation, { cwd }) {
                const [fs, path] = yield* Effect.all([FileSystem.FileSystem, Path.Path]);
                const [named, ...paths] = invocation.operands.filter(({ word }) => !invocation.rest.includes(word));
                const folder = path.resolve(cwd, ...invocation.values('-C').map(({ text }) => Option.getOrElse(text, () => '')));
                const existing = yield* Effect.orElseSucceed(
                    Option.match(
                        Option.flatMap(Option.fromUndefinedOr(named), ({ word }) => word.text),
                        { onNone: () => Effect.succeed(false), onSome: (operand) => fs.exists(path.resolve(folder, operand)) },
                    ),
                    () => false,
                );
                return invocation.rest.length > 0 || invocation.writes.length > 0 || paths.length > 0 || existing || invocation.has(...PATH_FLAGS) ? [{ subject: 'git checkout of paths', deny: DISCARDS_EDITS, values: RESTORE }] : [];
            }),
        },
        'git.checkout.force': { program: 'git', path: ['checkout'], when: FORCE_FLAGS, deny: DISCARDS_EDITS, values: RESTORE },
        'git.checkout.reset': { program: 'git', path: ['checkout'], when: ['-B'], deny: `resets an existing branch and ${STRANDS_COMMITS}`, values: { instead: 'create a branch under a new name with `git switch -c <name>`' } },
        'git.switch.force': { program: 'git', path: ['switch'], when: FORCE_FLAGS, deny: DISCARDS_EDITS, values: RESTORE },
        'git.switch.reset': { program: 'git', path: ['switch'], when: ['-C', '--force-create'], deny: `resets an existing branch and ${STRANDS_COMMITS}`, values: { instead: 'create a branch under a new name with `git switch -c <name>`' } },
        'git.reset': { program: 'git', path: ['reset'], when: ['--hard', '--merge', '--keep'], deny: DISCARDS_EDITS, values: RESTORE },
        'git.clean': { program: 'git', path: ['clean'], unless: DRY_RUN_FLAGS, deny: 'deletes untracked files other sessions in this checkout may hold', values: { instead: 'remove the files you created by name' } },
        'git.rm.force': { program: 'git', path: ['rm'], when: ['-f', '--force'], unless: ['--cached', ...DRY_RUN_FLAGS], deny: DISCARDS_EDITS, values: { instead: 'delete only files you created, by name, and leave edited files to their sessions' } },
        'git.checkout-index': { program: 'git', path: ['checkout-index'], when: ['-f', '--force', '-a', '--all'], deny: DISCARDS_EDITS, values: RESTORE },
        'git.update-index': { program: 'git', path: ['update-index'], when: ['--assume-unchanged', '--skip-worktree'], deny: 'hides edits from git status for every session in this checkout', values: { instead: 'leave the index flags alone' } },
        'git.worktree.detached': { program: 'git', path: ['worktree', 'add'], when: ['-d', '--detach'], deny: 'creates a worktree no branch holds', values: { instead: 'name its branch, as a branch takes one worktree that every session, agent, and workflow shares' } },
        'git.worktree.second': { program: 'git', path: ['worktree', 'add'], when: ['-f', '--force'], deny: 'gives a branch a second worktree', values: { instead: 'work in the worktree `git worktree list` shows for that branch' } },
        'git.worktree.remove': { program: 'git', path: ['worktree', 'remove'], when: ['-f', '--force'], deny: 'deletes a worktree with its uncommitted edits', values: { instead: 'commit or move its edits, then run `git worktree remove` without --force' } },
        'git.branch.delete': {
            program: 'git',
            path: ['branch'],
            decide: (invocation) => (invocation.has('-D') || (invocation.has('-d', '--delete') && invocation.has('-f', '--force')) ? [{ subject: 'git branch -D', deny: STRANDS_COMMITS, values: { instead: 'delete it with `git branch -d`, which refuses a branch whose commits are not merged' } }] : []),
        },
        'git.branch.overwrite': { program: 'git', path: ['branch'], when: ['-M', '-C', '-f', '--force'], unless: ['-D', '-d', '--delete'], deny: `overwrites an existing branch and ${STRANDS_COMMITS}`, values: { instead: 'rename, copy, or create under a name no branch holds, without the forcing flag' } },
        'git.push': {
            program: 'git',
            path: ['push'],
            decide: (invocation) => {
                const refs = invocation.operands.map(({ word }) => word.field.prefix);
                const forced = invocation.has('-f', '--force', '--force-with-lease', '--force-if-includes') || refs.some((ref) => ref.startsWith('+'));
                const deleted = invocation.has('-d', '--delete', '--mirror', '--prune') || refs.some((ref) => ref.startsWith(':'));
                return invocation.has(...DRY_RUN_FLAGS)
                    ? []
                    : [
                          ...(forced ? [{ subject: 'git push with force', deny: 'overwrites commits and tags on the remote that other sessions may have pushed', values: { instead: 'pull, integrate their commits, and push without force' } }] : []),
                          ...(deleted ? [{ subject: 'git push deleting remote refs', deny: 'drops commits and tags other sessions may still need', values: { instead: 'leave remote branches and tags in place' } }] : []),
                      ];
            },
        },
        'git.tag.delete': { program: 'git', path: ['tag'], when: ['-d', '--delete'], deny: 'deletes a tag that marks work other sessions and releases point at', values: { instead: 'leave tags in place' } },
        'git.tag.move': { program: 'git', path: ['tag'], when: ['-f', '--force'], unless: ['-d', '--delete'], deny: 'moves a tag that marks work other sessions and releases point at', values: { instead: 'tag under a name no tag holds' } },
        'git.update-ref.delete': { program: 'git', path: ['update-ref'], when: ['-d'], deny: `deletes a ref and ${STRANDS_COMMITS}`, values: { instead: 'leave the ref in place' } },
        'git.update-ref.stdin': { program: 'git', path: ['update-ref'], when: ['--stdin'], deny: `takes ref deletions and moves from standard input, where no rule reads them, and ${STRANDS_COMMITS}`, values: { instead: 'name the one ref and its new value as arguments' } },
        'git.reflog.delete': {
            program: 'git',
            path: ['reflog'],
            decide: (invocation) => (['delete', 'drop', 'expire'].includes(invocation.path[1] ?? Option.getOrElse(invocation.operand(), () => '')) ? [{ subject: 'git reflog delete or expire', deny: 'erases the record that recovers lost commits', values: { instead: 'leave the reflog intact' } }] : []),
        },
        'git.filter-branch': { program: 'git', path: ['filter-branch'], deny: 'rewrites every commit and drops the history other sessions build on', values: { instead: 'commit the change forward' } },
        'git.filter-repo': { program: 'git', path: ['filter-repo'], deny: 'rewrites every commit and drops the history other sessions build on', values: { instead: 'commit the change forward' } },
    },
    passHelp,
);

// --- [EXPORTS] -------------------------------------------------------------------------

export { git };
