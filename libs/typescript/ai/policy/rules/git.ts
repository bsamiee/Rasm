import { Option } from 'effect';
import type { Table } from '#/decision/rule';

// --- [CONSTANTS] -----------------------------------------------------------------------

const restoreAdvice = 'Read the committed text with `git diff -- <path>` or `git show HEAD:./<path>` and put back only your own lines with your file edit tool.';
const discardsEdits = 'discards uncommitted edits other sessions in this checkout may hold';
const strandsCommits = 'drops commits other sessions in this checkout may still need';
const forceFlags = ['-f', '--force', '--discard-changes'];
const tags = /^[+:]?refs\/tags\//u;

// --- [OPERATIONS] ----------------------------------------------------------------------

const git: Table = {
    'git.stash': {
        program: 'git',
        path: ['stash'],
        decide: (invocation) => (['list', 'show'].includes(invocation.path[1] ?? '') ? [] : [{ deny: `git ${invocation.path.join(' ')} hides work other sessions in this checkout may hold.`, advice: 'Leave the work in the tree and commit your own files by path.' }]),
    },
    'git.revert': { program: 'git', path: ['revert'], deny: 'undoes committed work other sessions in this checkout build on', advice: restoreAdvice },
    'git.restore': { program: 'git', path: ['restore'], decide: (invocation) => (invocation.has('--staged') && !invocation.has('--worktree') ? [] : [{ deny: `git restore ${discardsEdits}.`, advice: restoreAdvice }]) },
    'git.checkout.paths': {
        program: 'git',
        path: ['checkout'],
        decide: (invocation) => (invocation.rest.length > 0 || invocation.writes.length > 0 ? [{ deny: `git checkout of paths ${discardsEdits}.`, advice: restoreAdvice }] : []),
    },
    'git.checkout.force': { program: 'git', path: ['checkout'], when: forceFlags, deny: discardsEdits, advice: restoreAdvice },
    'git.checkout.reset': { program: 'git', path: ['checkout'], when: ['-B'], deny: `resets an existing branch and ${strandsCommits}`, advice: 'Create a branch under a new name with `git switch -c <name>`.' },
    'git.switch.force': { program: 'git', path: ['switch'], when: forceFlags, deny: discardsEdits, advice: restoreAdvice },
    'git.switch.reset': { program: 'git', path: ['switch'], when: ['-C', '--force-create'], deny: `resets an existing branch and ${strandsCommits}`, advice: 'Create a branch under a new name with `git switch -c <name>`.' },
    'git.reset': { program: 'git', path: ['reset'], when: ['--hard', '--merge', '--keep'], deny: discardsEdits, advice: restoreAdvice },
    'git.clean': { program: 'git', path: ['clean'], unless: ['-n', '--dry-run'], deny: 'deletes untracked files other sessions in this checkout may hold', advice: 'Remove the files you created by name.' },
    'git.rm.force': { program: 'git', path: ['rm'], when: ['-f', '--force'], unless: ['--cached'], deny: discardsEdits, advice: 'Delete only files you created, by name, and leave edited files to their sessions.' },
    'git.checkout-index': { program: 'git', path: ['checkout-index'], when: ['-f', '--force', '-a', '--all'], deny: discardsEdits, advice: restoreAdvice },
    'git.update-index': { program: 'git', path: ['update-index'], when: ['--assume-unchanged', '--skip-worktree'], deny: 'hides edits from git status for every session in this checkout', advice: 'Leave the index flags alone.' },
    'git.branch.delete': {
        program: 'git',
        path: ['branch'],
        decide: (invocation) => (invocation.has('-D') || (invocation.has('-d', '--delete') && invocation.has('-f', '--force')) ? [{ deny: `git branch -D ${strandsCommits}.`, advice: 'Delete it with `git branch -d`, which refuses a branch whose commits are not merged.' }] : []),
    },
    'git.branch.overwrite': { program: 'git', path: ['branch'], when: ['-M', '-C'], deny: `overwrites an existing branch and ${strandsCommits}`, advice: 'Rename or copy to a name no branch holds, without the forcing flag.' },
    'git.push': {
        program: 'git',
        path: ['push'],
        decide: (invocation) => {
            const refs = invocation.operands.slice(1).flatMap(({ word }) => Option.toArray(word.text));
            const tagged = refs.length > 0 && refs.every((ref) => tags.test(ref));
            const forced = invocation.has('-f', '--force', '--force-with-lease', '--force-if-includes') || refs.some((ref) => ref.startsWith('+'));
            const deleted = invocation.has('-d', '--delete', '--mirror', '--prune') || refs.some((ref) => ref.startsWith(':'));
            return tagged
                ? []
                : [
                      ...(forced ? [{ deny: 'git push with force overwrites commits on the remote that other sessions may have pushed.', advice: 'Pull, integrate their commits, and push without force.' }] : []),
                      ...(deleted ? [{ deny: 'git push that deletes remote refs drops commits other sessions may still need.', advice: 'Leave remote branches in place.' }] : []),
                  ];
        },
    },
    'git.tag.delete': { program: 'git', path: ['tag'], when: ['-d', '--delete'], deny: 'deletes a tag that marks work other sessions and releases point at', advice: 'Leave tags in place.' },
    'git.update-ref.delete': { program: 'git', path: ['update-ref'], when: ['-d'], deny: `deletes a ref and ${strandsCommits}`, advice: 'Leave the ref in place.' },
    'git.reflog.delete': {
        program: 'git',
        path: ['reflog'],
        decide: (invocation) => (['delete', 'drop', 'expire'].includes(invocation.path[1] ?? Option.getOrElse(invocation.operand(), () => '')) ? [{ deny: 'git reflog delete or expire erases the record that recovers lost commits.', advice: 'Leave the reflog intact.' }] : []),
    },
    'git.filter-branch': { program: 'git', path: ['filter-branch'], deny: 'rewrites every commit and drops the history other sessions build on', advice: 'Commit the change forward.' },
    'git.filter-repo': { program: 'git', path: ['filter-repo'], deny: 'rewrites every commit and drops the history other sessions build on', advice: 'Commit the change forward.' },
    'git.worktree.remove': { program: 'git', path: ['worktree', 'remove'], when: ['-f', '--force'], deny: 'deletes a worktree with its uncommitted edits', advice: 'Commit or move its edits, then run `git worktree remove` without --force.' },
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { git };
